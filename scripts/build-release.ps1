[CmdletBinding()]
param(
    [ValidatePattern('^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$')]
    [string]$Version = '1.0.0'
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $projectRoot 'src\PassiveWubi\PassiveWubi.csproj'
$distRoot = Join-Path $projectRoot 'dist'
$publishDir = Join-Path $distRoot "PassiveWubi-v$Version-win-x64"
$zipPath = Join-Path $distRoot "PassiveWubi-v$Version-win-x64.zip"
$hashPath = "$zipPath.sha256"

$resolvedRoot = [IO.Path]::GetFullPath($projectRoot).TrimEnd('\')
$resolvedDist = [IO.Path]::GetFullPath($distRoot).TrimEnd('\')
if (-not $resolvedDist.StartsWith("$resolvedRoot\dist", [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to write outside the project dist directory: $resolvedDist"
}

New-Item -ItemType Directory -Force -Path $distRoot | Out-Null

if (Test-Path -LiteralPath $publishDir) {
    Remove-Item -LiteralPath $publishDir -Recurse -Force
}
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}
if (Test-Path -LiteralPath $hashPath) {
    Remove-Item -LiteralPath $hashPath -Force
}

dotnet publish $projectFile `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $publishDir `
    -p:Version=$Version `
    -p:AssemblyVersion="$Version.0" `
    -p:FileVersion="$Version.0" `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination $publishDir
Copy-Item -LiteralPath (Join-Path $projectRoot 'CHANGELOG.md') -Destination $publishDir
Copy-Item -LiteralPath (Join-Path $projectRoot 'LICENSE') -Destination $publishDir

Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath -CompressionLevel Optimal

$hash = Get-FileHash -LiteralPath $zipPath -Algorithm SHA256
"$($hash.Hash.ToLowerInvariant())  $([IO.Path]::GetFileName($zipPath))" |
    Set-Content -LiteralPath $hashPath -Encoding ascii

Write-Host ''
Write-Host "Release package: $zipPath" -ForegroundColor Green
Write-Host "SHA-256: $($hash.Hash)" -ForegroundColor Green
