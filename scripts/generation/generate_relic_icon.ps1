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

Write-Host "Generating relic icon..."

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

Add-Type -AssemblyName System.Drawing
$image = [System.Drawing.Image]::FromFile($tempJpeg)

# Crop to square (center crop)
$size = [Math]::Min($image.Width, $image.Height)
$cropX = [int](($image.Width - $size) / 2)
$cropY = [int](($image.Height - $size) / 2)
$srcRect = New-Object System.Drawing.Rectangle($cropX, $cropY, $size, $size)

# Generate 64x64 icon
$small = New-Object System.Drawing.Bitmap(64, 64)
$g = [System.Drawing.Graphics]::FromImage($small)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.DrawImage($image, (New-Object System.Drawing.Rectangle(0, 0, 64, 64)), $srcRect, [System.Drawing.GraphicsUnit]::Pixel)
$smallPath = "Joi/images/relics/$OutputName.png"
$small.Save($smallPath, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$small.Dispose()
Write-Host "  Saved $smallPath (64x64)"

# Generate 128x128 big icon
$big = New-Object System.Drawing.Bitmap(128, 128)
$g2 = [System.Drawing.Graphics]::FromImage($big)
$g2.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g2.DrawImage($image, (New-Object System.Drawing.Rectangle(0, 0, 128, 128)), $srcRect, [System.Drawing.GraphicsUnit]::Pixel)
$bigPath = "Joi/images/relics/big/$OutputName.png"
$big.Save($bigPath, [System.Drawing.Imaging.ImageFormat]::Png)
$g2.Dispose()
$big.Dispose()
Write-Host "  Saved $bigPath (128x128)"

$image.Dispose()
Remove-Item $tempJpeg

Write-Host "Done! Relic icon generated."
