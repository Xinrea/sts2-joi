param(
    [string]$InputFile = "Joi/images/card_portraits/collect_rice.png",
    [int]$Size = 512
)

Add-Type -AssemblyName System.Drawing

$img = [System.Drawing.Image]::FromFile($InputFile)
$newImg = New-Object System.Drawing.Bitmap($Size, $Size)
$graphics = [System.Drawing.Graphics]::FromImage($newImg)
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.DrawImage($img, 0, 0, $Size, $Size)

$tempFile = [IO.Path]::GetTempFileName() + ".png"
$newImg.Save($tempFile, [System.Drawing.Imaging.ImageFormat]::Png)

$graphics.Dispose()
$newImg.Dispose()
$img.Dispose()

Move-Item -Path $tempFile -Destination $InputFile -Force

Write-Host "Resized to ${Size}x${Size}"
