[CmdletBinding()]
param(
    [string]$OutputPath = ''
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$projectRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $projectRoot 'src\PassiveWubi\Assets\PassiveWubi.ico'
}
$outputDirectory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

$size = 256
$bitmap = New-Object System.Drawing.Bitmap($size, $size)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$graphics.Clear([System.Drawing.Color]::Transparent)

$background = [System.Drawing.ColorTranslator]::FromHtml('#E9654F')
$dark = [System.Drawing.ColorTranslator]::FromHtml('#172033')
$white = [System.Drawing.Color]::White

$backgroundPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$radius = 54
$diameter = $radius * 2
$backgroundPath.AddArc(8, 8, $diameter, $diameter, 180, 90)
$backgroundPath.AddArc($size - 8 - $diameter, 8, $diameter, $diameter, 270, 90)
$backgroundPath.AddArc($size - 8 - $diameter, $size - 8 - $diameter, $diameter, $diameter, 0, 90)
$backgroundPath.AddArc(8, $size - 8 - $diameter, $diameter, $diameter, 90, 90)
$backgroundPath.CloseFigure()

$backgroundBrush = New-Object System.Drawing.SolidBrush($background)
$graphics.FillPath($backgroundBrush, $backgroundPath)

$darkBrush = New-Object System.Drawing.SolidBrush($dark)
$graphics.FillRectangle($darkBrush, 0, 204, $size, 52)

$keyFont = New-Object System.Drawing.Font('Microsoft YaHei UI', 112, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
$keyBrush = New-Object System.Drawing.SolidBrush($white)
$format = New-Object System.Drawing.StringFormat
$format.Alignment = [System.Drawing.StringAlignment]::Center
$format.LineAlignment = [System.Drawing.StringAlignment]::Center
$keyGlyph = [string][char]0x4E94
$graphics.DrawString($keyGlyph, $keyFont, $keyBrush, (New-Object System.Drawing.RectangleF(0, 0, $size, 220)), $format)

$linePen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(170, 255, 255, 255), 7)
$linePen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$linePen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$graphics.DrawLine($linePen, 58, 238, 94, 238)
$graphics.DrawLine($linePen, 108, 238, 148, 238)
$graphics.DrawLine($linePen, 162, 238, 198, 238)

$stream = New-Object System.IO.MemoryStream
$bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
$pngBytes = $stream.ToArray()

$iconStream = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter($iconStream)
$writer.Write([UInt16]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]1)
$writer.Write([Byte]0)
$writer.Write([Byte]0)
$writer.Write([Byte]0)
$writer.Write([Byte]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]32)
$writer.Write([UInt32]$pngBytes.Length)
$writer.Write([UInt32]22)
$writer.Write($pngBytes)
$writer.Flush()

[IO.File]::WriteAllBytes($OutputPath, $iconStream.ToArray())

$writer.Dispose()
$iconStream.Dispose()
$stream.Dispose()
$linePen.Dispose()
$format.Dispose()
$keyBrush.Dispose()
$keyFont.Dispose()
$darkBrush.Dispose()
$backgroundBrush.Dispose()
$backgroundPath.Dispose()
$graphics.Dispose()
$bitmap.Dispose()

Write-Host "Icon generated: $OutputPath"
