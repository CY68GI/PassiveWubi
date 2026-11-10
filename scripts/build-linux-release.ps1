[CmdletBinding()]
param(
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version = '1.1.0'
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $projectRoot 'src\PassiveWubi.Linux\PassiveWubi.Linux.csproj'
$distRoot = Join-Path $projectRoot 'dist'
$folderName = "PassiveWubi-Linux-x64-v$Version"
$publishDir = Join-Path $distRoot $folderName
$archivePath = Join-Path $distRoot "$folderName.tar.gz"
$hashPath = "$archivePath.sha256"

$resolvedRoot = [IO.Path]::GetFullPath($projectRoot).TrimEnd('\')
$resolvedDist = [IO.Path]::GetFullPath($distRoot).TrimEnd('\')
if (-not $resolvedDist.StartsWith("$resolvedRoot\dist", [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to write outside the project dist directory: $resolvedDist"
}

New-Item -ItemType Directory -Force -Path $distRoot | Out-Null
if (Test-Path -LiteralPath $publishDir) {
    Remove-Item -LiteralPath $publishDir -Recurse -Force
}
if (Test-Path -LiteralPath $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
}
if (Test-Path -LiteralPath $hashPath) {
    Remove-Item -LiteralPath $hashPath -Force
}

dotnet publish $projectFile `
    --configuration Release `
    --runtime linux-x64 `
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
Copy-Item -LiteralPath (Join-Path $projectRoot 'ABOUT.md') -Destination $publishDir
Copy-Item -LiteralPath (Join-Path $projectRoot 'CHANGELOG.md') -Destination $publishDir
Copy-Item -LiteralPath (Join-Path $projectRoot 'LICENSE') -Destination $publishDir
Copy-Item -LiteralPath (Join-Path $projectRoot 'packaging\linux\install-ubuntu.sh') -Destination $publishDir
Copy-Item -LiteralPath (Join-Path $projectRoot 'src\PassiveWubi\Assets\PassiveWubi.svg') -Destination $publishDir

$tar = Get-Command tar -ErrorAction Stop
& $tar.Source -czf $archivePath -C $distRoot $folderName
if ($LASTEXITCODE -ne 0) {
    throw "tar failed with exit code $LASTEXITCODE."
}

$hash = Get-FileHash -LiteralPath $archivePath -Algorithm SHA256
"$($hash.Hash.ToLowerInvariant())  $([IO.Path]::GetFileName($archivePath))" |
    Set-Content -LiteralPath $hashPath -Encoding ascii

Write-Host ''
Write-Host "Linux release package: $archivePath" -ForegroundColor Green
Write-Host "SHA-256: $($hash.Hash)" -ForegroundColor Green
