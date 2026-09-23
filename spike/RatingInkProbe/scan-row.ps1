# Measure the stars straight off the pixels: a horizontal scan line through the middle of the first row, so the
# run widths are the drawn star widths. The tree numbers say what the layout handed the glyph; this says what
# actually reached the screen.
param([string[]] $Files, [int] $Y = 145, [switch] $BlueOnly)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = $PSScriptRoot
foreach ($f in $Files) {
    $path = Join-Path $root $f
    $bmp = [System.Drawing.Bitmap]::FromFile($path)
    $runs = New-Object System.Collections.Generic.List[string]
    $start = -1; $prev = -1
    for ($x = 0; $x -lt [Math]::Min(340, $bmp.Width); $x++) {
        $c = $bmp.GetPixel($x, $Y)
        if ($BlueOnly) {
            # The accent fill layer on its own: the outline layer sits behind it and would widen every run.
            $ink = ([int]$c.B -gt 60) -and ([int]$c.B -gt ([int]$c.R + 30))
        }
        else {
            $ink = -not (($c.R -gt 240) -and ($c.G -gt 240) -and ($c.B -gt 240))
        }
        if ($ink) {
            if ($start -lt 0) { $start = $x }
            $prev = $x
        }
        elseif ($start -ge 0) {
            $runs.Add(("$($start)..$prev w=$($prev-$start+1)"))
            $start = -1
        }
    }
    if ($start -ge 0) { $runs.Add(("$($start)..$prev w=$($prev-$start+1)")) }
    $bmp.Dispose()
    Write-Host ("{0} y={1}: {2}" -f $f, $Y, ($runs -join ' '))
}
