param(
    [Parameter(Mandatory=$true)]
    [string]$Type,
    [string]$ReferenceImage = "Joi.png"
)

$apiKey = $env:OPENROUTER_API_KEY
if (-not $apiKey) {
    Write-Error "OPENROUTER_API_KEY not set"
    exit 1
}

$imageBytes = [IO.File]::ReadAllBytes($ReferenceImage)
$base64Image = [Convert]::ToBase64String($imageBytes)

$configs = @{
    "map_marker" = @{
        Prompt = "Generate a circular character avatar icon with SOLID BRIGHT GREEN (#00FF00) background. Character's face centered, simple and recognizable at small size."
        Width = 128; Height = 128
        Path = "Joi/images/charui/map_marker_char_name.png"
        RemoveGreen = $true
    }
    "icon" = @{
        Prompt = "Generate a square character portrait icon, head and shoulders, clean style for UI."
        Width = 128; Height = 128
        Path = "Joi/images/charui/character_icon_char_name.png"
        RemoveGreen = $false
    }
    "select" = @{
        Prompt = "Generate a VERTICAL character portrait (taller than wide). Full body or 3/4 body shot, dynamic pose. Vibrant colors, detailed. Portrait orientation suitable for character selection card."
        Width = 132; Height = 195
        Path = "Joi/images/charui/char_select_char_name.png"
        RemoveGreen = $false
    }
    "select_locked" = @{
        Prompt = "Generate a VERTICAL silhouette portrait (taller than wide). Dark shadowy figure with glowing outline, mysterious locked appearance. Same vertical portrait orientation."
        Width = 132; Height = 195
        Path = "Joi/images/charui/char_select_char_name_locked.png"
        RemoveGreen = $false
    }
}

$config = $configs[$Type]
if (-not $config) {
    Write-Error "Invalid type. Use: map_marker, icon, select, select_locked"
    exit 1
}

$body = @{
    model = "google/gemini-3-pro-image-preview"
    messages = @(
        @{
            role = "user"
            content = @(
                @{ type = "image_url"; image_url = @{ url = "data:image/png;base64,$base64Image" } },
                @{ type = "text"; text = $config.Prompt }
            )
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "Generating $Type ($($config.Width)x$($config.Height))..."

$response = Invoke-RestMethod -Uri "https://openrouter.ai/api/v1/chat/completions" `
    -Method Post `
    -Headers @{ "Authorization" = "Bearer $apiKey"; "Content-Type" = "application/json" } `
    -Body $body

$imageUrl = $response.choices[0].message.images[0].image_url.url
$base64Data = $imageUrl.Split(',')[1]
$bytes = [Convert]::FromBase64String($base64Data)

$tempFile = [IO.Path]::GetTempFileName() + ".jpg"
[IO.File]::WriteAllBytes($tempFile, $bytes)

Add-Type -AssemblyName System.Drawing
$image = [System.Drawing.Image]::FromFile($tempFile)

# Create result with exact dimensions
$result = New-Object System.Drawing.Bitmap($config.Width, $config.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($result)
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic

# Scale to fit while maintaining aspect ratio
$srcAspect = $image.Width / $image.Height
$dstAspect = $config.Width / $config.Height

if ($srcAspect -gt $dstAspect) {
    # Source is wider, fit to height
    $newHeight = $config.Height
    $newWidth = [int]($newHeight * $srcAspect)
    $x = [int](($config.Width - $newWidth) / 2)
    $y = 0
} else {
    # Source is taller, fit to width
    $newWidth = $config.Width
    $newHeight = [int]($newWidth / $srcAspect)
    $x = 0
    $y = [int](($config.Height - $newHeight) / 2)
}

$destRect = New-Object System.Drawing.Rectangle($x, $y, $newWidth, $newHeight)
$srcRect = New-Object System.Drawing.Rectangle(0, 0, $image.Width, $image.Height)
$graphics.DrawImage($image, $destRect, $srcRect, [System.Drawing.GraphicsUnit]::Pixel)
$graphics.Dispose()

# Remove green background if needed
if ($config.RemoveGreen) {
    for ($px = 0; $px -lt $result.Width; $px++) {
        for ($py = 0; $py -lt $result.Height; $py++) {
            $pixel = $result.GetPixel($px, $py)
            $r = [int]$pixel.R
            $g = [int]$pixel.G
            $b = [int]$pixel.B

            if ($g -gt 100 -and $g -gt ($r + 40) -and $g -gt ($b + 40)) {
                $greenness = [Math]::Min(1.0, [Math]::Max(0, ($g - [Math]::Max($r, $b)) / 128.0))
                $newAlpha = [int]([Math]::Max(0, 255 * (1 - $greenness)))
                $newPixel = [System.Drawing.Color]::FromArgb($newAlpha, $r, $g, $b)
                $result.SetPixel($px, $py, $newPixel)
            }
        }
    }
}

$result.Save($config.Path, [System.Drawing.Imaging.ImageFormat]::Png)
$result.Dispose()
$image.Dispose()
Remove-Item $tempFile

Write-Host "Done! $($config.Path)"
