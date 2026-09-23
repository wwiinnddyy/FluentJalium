# Sample the two A/B grabs so the pictures can be quoted as numbers: how much ink is there, and what colour is it.
# Blue-ish = accent fill reached the pixel; gray-ish = only the outline layer.
param([string[]] $Files = @('rating-before.png','rating-after.png'))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = $PSScriptRoot
foreach ($f in $Files) {
    $path = Join-Path $root $f
    if (-not (Test-Path $path)) { Write-Host "missing $f"; continue }
    $bmp = [System.Drawing.Bitmap]::FromFile($path)
    # Only the star band: the grab is a window rect and the desktop shows through at the far edges, which would
    # pollute the counts.
    $blue = 0; $gray = 0
    $tally = @{}
    for ($y = 100; $y -lt [Math]::Min(440, $bmp.Height); $y++) {
        for ($x = 0; $x -lt [Math]::Min(320, $bmp.Width); $x++) {
            $c = $bmp.GetPixel($x, $y)
            $r = [int]$c.R; $g = [int]$c.G; $b = [int]$c.B
            if ($r -gt 240 -and $g -gt 240 -and $b -gt 240) { continue }
            $key = '{0:X2}{1:X2}{2:X2}' -f ([int]($r/16)*16), ([int]($g/16)*16), ([int]($b/16)*16)
            $tally[$key] = [int]$tally[$key] + 1
            if ($b -gt 60 -and $b -gt ($r + 30)) { $blue++ }
            elseif (([Math]::Abs($r-$g) -lt 24) -and ([Math]::Abs($g-$b) -lt 24)) { $gray++ }
        }
    }
    $bmp.Dispose()
    $top = $tally.GetEnumerator() | Sort-Object -Property Value -Descending | Select-Object -First 6
    $topText = ($top | ForEach-Object { "#$($_.Key):$($_.Value)" }) -join ' '
    Write-Host ("{0}: blue={1} gray={2} total={3} | top {4}" -f $f, $blue, $gray, ($blue+$gray), $topText)
}
