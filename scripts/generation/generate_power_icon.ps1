param(
    [Parameter(Mandatory=$true)]
    [string]$Prompt,

    [Parameter(Mandatory=$true)]
    [string]$OutputName,

    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "powers"
)

$apiKey = $env:OPENROUTER_API_KEY
if (-not $apiKey) {
    Write-Error "OPENROUTER_API_KEY not set"
    exit 1
}

$fullPrompt = @"
Generate a simple flat icon for a game buff/power. Requirements:
- MUST have a solid bright green (#00FF00) background, fill the entire background with this exact green color
- Simple, symbolic, minimalist design like mobile game UI icons
- Use soft glowing colors (white, light blue, purple, gold, pink)
- Clean geometric shapes, no realistic details
- No text, no border, no shadow
- The icon symbol should be centered and simple enough to read at 64x64 pixels
- Style reference: flat vector game icons like Slay the Spire power icons

Icon subject: $Prompt
"@

$body = @{
    model = "google/gemini-3-pro-image-preview"
    messages = @(
        @{
            role = "user"
            content = @(
                @{
                    type = "text"
                    text = $fullPrompt
                }
            )
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "Generating power icon for $OutputName..."

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

# Save as temporary file
$tempFile = [IO.Path]::GetTempFileName() + ".jpg"
[IO.File]::WriteAllBytes($tempFile, $bytes)

Add-Type -AssemblyName System.Drawing
$image = [System.Drawing.Image]::FromFile($tempFile)

# Crop to square from center
$minDim = [Math]::Min($image.Width, $image.Height)
$cropX = [int](($image.Width - $minDim) / 2)
$cropY = [int](($image.Height - $minDim) / 2)
$srcRect = New-Object System.Drawing.Rectangle($cropX, $cropY, $minDim, $minDim)

# Create 128x128 intermediate
$intermediate = New-Object System.Drawing.Bitmap(128, 128, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$gInter = [System.Drawing.Graphics]::FromImage($intermediate)
$gInter.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$destRect = New-Object System.Drawing.Rectangle(0, 0, 128, 128)
$gInter.DrawImage($image, $destRect, $srcRect, [System.Drawing.GraphicsUnit]::Pixel)
$gInter.Dispose()

# Save big icon (128x128)
$bigPath = "Joi/images/$OutputDir/big/$OutputName.png"
$bigDir = Split-Path $bigPath -Parent
if (-not (Test-Path $bigDir)) {
    New-Item -ItemType Directory -Path $bigDir -Force | Out-Null
}
$intermediate.Save($bigPath, [System.Drawing.Imaging.ImageFormat]::Png)

# Generate small icon (64x64) from the processed intermediate
$small = New-Object System.Drawing.Bitmap(64, 64, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$gSmall = [System.Drawing.Graphics]::FromImage($small)
$gSmall.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$smallDest = New-Object System.Drawing.Rectangle(0, 0, 64, 64)
$smallSrc = New-Object System.Drawing.Rectangle(0, 0, 128, 128)
$gSmall.DrawImage($intermediate, $smallDest, $smallSrc, [System.Drawing.GraphicsUnit]::Pixel)
$gSmall.Dispose()

$smallPath = "Joi/images/$OutputDir/$OutputName.png"
$smallDir = Split-Path $smallPath -Parent
if (-not (Test-Path $smallDir)) {
    New-Item -ItemType Directory -Path $smallDir -Force | Out-Null
}
$small.Save($smallPath, [System.Drawing.Imaging.ImageFormat]::Png)

$small.Dispose()
$intermediate.Dispose()
$image.Dispose()
Remove-Item $tempFile

Write-Host "Done! Small: $smallPath (64x64), Big: $bigPath (128x128)"
