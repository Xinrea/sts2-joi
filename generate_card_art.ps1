param(
    [Parameter(Mandatory=$true)]
    [string]$Prompt,

    [Parameter(Mandatory=$true)]
    [string]$OutputName,

    [string]$ReferenceImage = "Joi.png",

    # Normal: 与多数角色卡一致，API 请求 4:3，导出 512x384（模组常用小图）。
    # Ancient: 与原版 Ancient 立绘一致，API 请求竖版 3:4，导出 606x852。
    [ValidateSet('Normal', 'Ancient')]
    [string]$PortraitKind = 'Normal'
)

$apiKey = $env:OPENROUTER_API_KEY
if (-not $apiKey) {
    Write-Error "OPENROUTER_API_KEY not set"
    exit 1
}

# Read and encode reference image
$imageBytes = [IO.File]::ReadAllBytes($ReferenceImage)
$base64Image = [Convert]::ToBase64String($imageBytes)

$apiAspectRatio = if ($PortraitKind -eq 'Ancient') { "3:4" } else { "4:3" }

$body = @{
    model = "openai/gpt-5.4-image-2"
    modalities = @("image", "text")
    image_config = @{
        aspect_ratio = $apiAspectRatio
    }
    messages = @(
        @{
            role = "user"
            content = @(
                @{
                    type = "image_url"
                    image_url = @{
                        url = "data:image/png;base64,$base64Image"
                    }
                },
                @{
                    type = "text"
                    text = $Prompt
                }
            )
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "Generating image (PortraitKind=$PortraitKind, API aspect_ratio=$apiAspectRatio)..."

$response = Invoke-RestMethod -Uri "https://openrouter.ai/api/v1/chat/completions" `
    -Method Post `
    -Headers @{
        "Authorization" = "Bearer $apiKey"
        "Content-Type" = "application/json"
    } `
    -Body $body

$images = $response.choices[0].message.images
if (-not $images -or $images.Count -lt 1) {
    Write-Error "No images in API response (check modalities and model output)."
    exit 1
}

$imageUrl = $images[0].image_url.url
if ($imageUrl -match '^data:image/[^;]+;base64,(.+)$') {
    $base64Data = $Matches[1]
} else {
    $base64Data = $imageUrl.Split(',')[1]
}
$bytes = [Convert]::FromBase64String($base64Data)

Add-Type -AssemblyName System.Drawing
$ms = New-Object System.IO.MemoryStream(,$bytes)
$image = [System.Drawing.Image]::FromStream($ms)

# Export size: Normal = 小图 4:3；Ancient = 原版 Ancient 竖版立绘 606x852
if ($PortraitKind -eq 'Ancient') {
    $targetWidth = 606
    $targetHeight = 852
} else {
    $targetWidth = 512
    $targetHeight = 384
}

$sourceAspect = $image.Width / $image.Height
$targetAspect = $targetWidth / $targetHeight

if ($sourceAspect -gt $targetAspect) {
    # Image is wider than target frame: crop width from center
    $cropHeight = $image.Height
    $cropWidth = [int]($cropHeight * $targetAspect)
    $cropX = [int](($image.Width - $cropWidth) / 2)
    $cropY = 0
} else {
    # Image is taller / narrower: crop height
    $cropWidth = $image.Width
    $cropHeight = [int]($cropWidth / $targetAspect)
    $cropX = 0
    if ($PortraitKind -eq 'Ancient') {
        $cropY = [int](($image.Height - $cropHeight) / 2)
    } else {
        $cropY = 0
    }
}

$cropped = New-Object System.Drawing.Bitmap($targetWidth, $targetHeight)
$graphics = [System.Drawing.Graphics]::FromImage($cropped)
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic

$srcRect = New-Object System.Drawing.Rectangle($cropX, $cropY, $cropWidth, $cropHeight)
$destRect = New-Object System.Drawing.Rectangle(0, 0, $targetWidth, $targetHeight)
$graphics.DrawImage($image, $destRect, $srcRect, [System.Drawing.GraphicsUnit]::Pixel)

$outputPath = "Joi/images/card_portraits/$OutputName.png"
$cropped.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)

$graphics.Dispose()
$cropped.Dispose()
$image.Dispose()

$ms.Dispose()

Write-Host "✓ Image saved to $outputPath (${targetWidth}x${targetHeight}, PortraitKind=$PortraitKind)"
