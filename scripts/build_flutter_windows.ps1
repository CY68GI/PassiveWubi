[CmdletBinding()]
param(
    [string]$Version = '2.0.1'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$dist = Join-Path $root 'dist'
$folder = Join-Path $dist "PassiveWubi-Flutter-v$Version-win-x64"
$zip = "$folder.zip"

Push-Location $root
try {
    flutter pub get
    flutter build windows --release

    if (Test-Path -LiteralPath $folder) {
        Remove-Item -LiteralPath $folder -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path $folder | Out-Null
    Copy-Item -Path (Join-Path $root 'build\windows\x64\runner\Release\*') -Destination $folder -Recurse -Force
    Copy-Item -LiteralPath (Join-Path $root 'README.md') -Destination $folder
    Copy-Item -LiteralPath (Join-Path $root 'ABOUT.md') -Destination $folder
    Copy-Item -LiteralPath (Join-Path $root 'CHANGELOG.md') -Destination $folder
    Copy-Item -LiteralPath (Join-Path $root 'LICENSE') -Destination $folder
    if (Test-Path -LiteralPath $zip) {
        Remove-Item -LiteralPath $zip -Force
    }
    Compress-Archive -Path (Join-Path $folder '*') -DestinationPath $zip -CompressionLevel Optimal
    $hash = Get-FileHash -LiteralPath $zip -Algorithm SHA256
    "$($hash.Hash.ToLowerInvariant())  $([IO.Path]::GetFileName($zip))" |
        Set-Content -LiteralPath "$zip.sha256" -Encoding ascii
    Write-Host "Built $zip"
}
finally {
    Pop-Location
}
