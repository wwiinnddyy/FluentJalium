<#
.SYNOPSIS
  Read a rounded corner out of a captured frame as a table of per-row insets, so "the radius looks wrong"
  becomes a number.

.DESCRIPTION
  A corner arc of radius r leaves the top row of the surface short of its left edge by r, and the inset falls
  towards zero over r rows. This walks rows from the first line where the surface colour appears and prints
  that inset per row, plus the row where the inset reaches zero - which is r, in device pixels. Divide by the
  capture scale (dpi 168 on this machine means 1 DIP = 1.75 px) to compare with a CornerRadius value.
  A strict colour match is used on purpose: antialiased boundary pixels are excluded, which biases the inset
  down by about one pixel and never invents an arc that is not there.

.EXAMPLE
  corner-stair.ps1 -In frame.png -Color 2C2C2C -XFrom 340 -XTo 560 -YFrom 300 -YTo 520 -Rows 20
#>
param(
    [Parameter(Mandatory = $true)][string] $In,
    [Parameter(Mandatory = $true)][string] $Color,
    [int] $XFrom = 0,
    [int] $XTo = -1,
    [int] $YFrom = 0,
    [int] $YTo = -1,
    [int] $Rows = 16,
    [int] $MinCount = 5
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$argb = [System.Drawing.Color]::FromArgb(
    [Convert]::ToInt32($Color.Substring(0, 2), 16),
    [Convert]::ToInt32($Color.Substring(2, 2), 16),
    [Convert]::ToInt32($Color.Substring(4, 2), 16)).ToArgb()

$bmp = [System.Drawing.Bitmap]::FromFile($In)
if ($XTo -lt 0) { $XTo = $bmp.Width - 1 }
if ($YTo -lt 0) { $YTo = $bmp.Height - 1 }
"$In $($bmp.Width)x$($bmp.Height) looking for #$Color"

$top = -1
$left = -1
$right = -1
for ($y = $YFrom; $y -le $YTo; $y++) {
    $count = 0
    for ($x = $XFrom; $x -le $XTo; $x++) {
        if ($bmp.GetPixel($x, $y).ToArgb() -eq $argb) { $count++ }
    }
    if ($count -ge $MinCount) {
        $top = $y
        break
    }
}

if ($top -lt 0) {
    "no row with $MinCount+ pixels of #$Color between y=$YFrom..$YTo"
    $bmp.Dispose()
    return
}

for ($x = $XFrom; $x -le $XTo; $x++) {
    if ($bmp.GetPixel($x, ($top + $Rows)).ToArgb() -eq $argb) { $left = $x; break }
}

for ($x = $XTo; $x -ge $XFrom; $x--) {
    if ($bmp.GetPixel($x, ($top + $Rows)).ToArgb() -eq $argb) { $right = $x; break }
}

"first row=$top  body left=$left right=$right body width=$($right - $left + 1)"
$row = @()
for ($i = 0; $i -lt $Rows; $i++) {
    $y = $top + $i
    $first = -1
    for ($x = $XFrom; $x -le $XTo; $x++) {
        if ($bmp.GetPixel($x, $y).ToArgb() -eq $argb) { $first = $x; break }
    }
    $row += "  dy=$i x=$first inset=$(if ($first -ge 0 -and $left -ge 0) { $first - $left } else { '-' })"
}
$row -join [Environment]::NewLine
$bmp.Dispose()
