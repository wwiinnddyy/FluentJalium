<#
.SYNOPSIS
  Print the colour runs along one horizontal line of a captured frame, so "the popup is smaller than the box"
  becomes four numbers instead of an impression.

.EXAMPLE
  scan-line.ps1 -In frame.png -Y 255 -From 150 -To 1100
#>
param(
    [Parameter(Mandatory = $true)][string] $In,
    [Parameter(Mandatory = $true)][int] $Y,
    [int] $From = 0,
    [int] $To = -1
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$bmp = [System.Drawing.Bitmap]::FromFile($In)
if ($To -lt 0) { $To = $bmp.Width - 1 }
$previous = ''
$start = $From
$runs = New-Object System.Collections.Generic.List[string]
for ($x = $From; $x -le $To; $x++) {
    $pixel = $bmp.GetPixel($x, $Y)
    $hex = '#{0:X2}{1:X2}{2:X2}' -f $pixel.R, $pixel.G, $pixel.B
    if ($hex -ne $previous) {
        if ($previous -ne '') { $runs.Add(("{0}..{1} {2}" -f $start, ($x - 1), $previous)) }
        $previous = $hex
        $start = $x
    }
}
$runs.Add(("{0}..{1} {2}" -f $start, $To, $previous))
Write-Host ("y={0} ({1}x{2} frame)" -f $Y, $bmp.Width, $bmp.Height)
# Runs shorter than 3 px are antialiasing noise on glyph edges; keep them out of the report.
$runs | Where-Object { $_ } | ForEach-Object {
    $parts = ($_ -split ' ')
    $bounds = ($parts[0] -split '\.\.')
    if (([int]$bounds[1] - [int]$bounds[0]) -ge 3) { Write-Host ("  " + $_) }
}
$bmp.Dispose()
