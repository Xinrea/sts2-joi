[CmdletBinding()]
param(
    [Parameter(Position=0)][string]$GameDir = ""
)

$ErrorActionPreference = "Continue"
Set-StrictMode -Version Latest

function Write-Step($Step, $Total, $Message) {
    Write-Host ""
    Write-Host "[$Step/$Total] $Message" -ForegroundColor Cyan
}

function Write-OK($Message) {
    Write-Host "  $Message" -ForegroundColor Green
}

function Write-Err($Message) {
    Write-Host "  ERROR: $Message" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor White
Write-Host " Joi Mod - Tools Setup" -ForegroundColor White
Write-Host "========================================" -ForegroundColor White
Write-Host ""

$ProjectDir = $PSScriptRoot | Split-Path
$ToolsDir = Join-Path $ProjectDir "tools"
$IlspyDir = Join-Path $ToolsDir "ilspy"

# Create tools directory
if (-not (Test-Path $ToolsDir)) {
    New-Item -ItemType Directory -Path $ToolsDir | Out-Null
}

Write-Step 1 2 "Setting up ILSpy..."
if (-not (Test-Path $IlspyDir)) {
    New-Item -ItemType Directory -Path $IlspyDir | Out-Null
    Write-Host "  Downloading ILSpy..."
    $ilspyUrl = "https://github.com/icsharpcode/ILSpy/releases/download/v8.2/ILSpy_binaries_8.2.0.7535.zip"
    $ilspyZip = Join-Path $ToolsDir "ilspy.zip"
    Invoke-WebRequest -Uri $ilspyUrl -OutFile $ilspyZip
    Expand-Archive -Path $ilspyZip -DestinationPath $IlspyDir -Force
    Remove-Item $ilspyZip
    Write-OK "ILSpy installed"
} else {
    Write-OK "ILSpy already installed"
}

Write-Step 2 2 "Setup complete!"
Write-Host ""
