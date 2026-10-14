<#
.SYNOPSIS
  Print the OUTER edge of a rounded surface as a per-row inset table, located automatically from the
  background it sits on.

.DESCRIPTION
  corner-stair.ps1 answers "where does the fill start" with a strict colour match, which excludes the
  anti-aliased boundary and reads the arc from inside. This answers the other question - where the card
  actually begins - by counting every pixel that is NOT the background colour, so the stroke and the fringe
  stay in. That is the edge a CornerRadius moves, which makes it the number to compare between two frames of
  the same card at two radii.

  The subject is located, not guessed: the first row in the box holding at least -MinCount non-background
  pixels is the card's top, and the body edges are read -BodyRows deep (below any arc). -XFrom/-XTo/-YFrom/-YTo
  bound the search, so pass a window around the card rather than the whole frame.

  Still not a ruler for the radius value - see docs/astra/adaptation/s1a-menu-surface-raw.txt row E for what
  the two frames do and do not prove.

.EXAMPLE
  corner-edge.ps1 -In spike/FlyoutSurfaceProbe/out/context.png -Background 1C1C1E -XFrom 300 -XTo 600 -YFrom 300 -YTo 520
#>
param(
    [Parameter(Mandatory = $true)][string] $In,
    [string] $Background = '1C1C1E',
    [int] $XFrom = 0,
    [int] $XTo = -1,
    [int] $YFrom = 0,
    [int] $YTo = -1,
    [int] $MinCount = 100,
    [int] $BodyRows = 40,
    [int] $Rows = 24
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$bg = [System.Drawing.Color]::FromArgb(
    [Convert]::ToInt32($Background.Substring(0, 2), 16),
    [Convert]::ToInt32($Background.Substring(2, 2), 16),
    [Convert]::ToInt32($Background.Substring(4, 2), 16)).ToArgb()

$bmp = [System.Drawing.Bitmap]::FromFile($In)
if ($XTo -lt 0) { $XTo = $bmp.Width - 1 }
if ($YTo -lt 0) { $YTo = $bmp.Height - 1 }

function Test-Surface([int] $x, [int] $y) { $bmp.GetPixel($x, $y).ToArgb() -ne $bg }

$top = -1
for ($y = $YFrom; $y -le $YTo; $y++) {
    $count = 0
    for ($x = $XFrom; $x -le $XTo; $x++) {
        if (Test-Surface $x $y) { $count++ }
    }
    if ($count -ge $MinCount) { $top = $y; break }
}

if ($top -lt 0) {
    "no row with $MinCount+ non-background pixels of #$Background between y=$YFrom..$YTo"
    $bmp.Dispose()
    return
}

$left = -1
$right = -1
for ($x = $XFrom; $x -le $XTo; $x++) {
    if (Test-Surface $x ($top + $BodyRows)) { $left = $x; break }
}
for ($x = $XTo; $x -ge $XFrom; $x--) {
    if (Test-Surface $x ($top + $BodyRows)) { $right = $x; break }
}

"$In  card top=$top left=$left right=$right width=$($right - $left + 1) (background #$Background)"
$prev = -1
for ($i = 0; $i -lt $Rows; $i++) {
    $y = $top + $i
    $first = -1
    for ($x = [Math]::Max(0, $left - 40); $x -le ($left + 80); $x++) {
        if (Test-Surface $x $y) { $first = $x; break }
    }
    if ($first -lt 0) { break }
    if ($first -eq $prev) { "  dy=$i ." } else { "  dy=$i inset=$($first - $left)"; $prev = $first }
}
$bmp.Dispose()
