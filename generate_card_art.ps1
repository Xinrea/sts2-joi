param(
    [Parameter(Mandatory=$true)]
    [string]$Prompt,

    [Parameter(Mandatory=$true)]
    [string]$OutputName,

    [string]$ReferenceImage = "Joi.png"
)

$apiKey = $env:OPENROUTER_API_KEY
if (-not $apiKey) {
    Write-Error "OPENROUTER_API_KEY not set"
    exit 1
}

# Read and encode reference image
$imageBytes = [IO.File]::ReadAllBytes($ReferenceImage)
$base64Image = [Convert]::ToBase64String($imageBytes)

$body = @{
    model = "google/gemini-3-pro-image-preview"
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

Write-Host "Generating image..."

$response = Invoke-RestMethod -Uri "https://openrouter.ai/api/v1/chat/completions" `
    -Method Post `
    -Headers @{
        "Authorization" = "Bearer $apiKey"
        "Content-Type" = "application/json"
    } `
    -Body $body

$imageUrl = $response.choices[0].message.images[0].image_url.url
$base64Data = $imageUrl.Split(',')[1]
$bytes = [Convert]::FromBase64String($base64Data)

# Save as temporary JPEG first
$tempJpeg = [IO.Path]::GetTempFileName() + ".jpg"
[IO.File]::WriteAllBytes($tempJpeg, $bytes)

# Convert JPEG to PNG and crop to 4:3 aspect ratio (512x384)
Add-Type -AssemblyName System.Drawing
$image = [System.Drawing.Image]::FromFile($tempJpeg)

# Calculate crop dimensions (top crop to 4:3)
$targetWidth = 512
$targetHeight = 384
$sourceAspect = $image.Width / $image.Height
$targetAspect = $targetWidth / $targetHeight

if ($sourceAspect -gt $targetAspect) {
    # Image is wider, crop width from center
    $cropHeight = $image.Height
    $cropWidth = [int]($cropHeight * $targetAspect)
    $cropX = [int](($image.Width - $cropWidth) / 2)
    $cropY = 0
} else {
    # Image is taller, crop from top
    $cropWidth = $image.Width
    $cropHeight = [int]($cropWidth / $targetAspect)
    $cropX = 0
    $cropY = 0  # Crop from top instead of center
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

# Clean up temp file
Remove-Item $tempJpeg

Write-Host "✓ Image saved to $outputPath (512x384, 4:3 aspect ratio)"
