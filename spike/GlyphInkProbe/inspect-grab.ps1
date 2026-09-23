# Read the existing grab and report where the red border lines actually land.
# The counter only printed the red bounding box (232,254 size 928x426 in a 1160x680 grab), whose right and bottom
# edges sit exactly on the window edge - that pattern says clipping, but which of the four edges were painted is
# what tells a clipped box from a mis-centred one, and only the picture can say.
param([string] $Grab = 'spike/GlyphInkProbe/grab.png')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$path = (Resolve-Path $Grab).Path
$img = [System.Drawing.Bitmap]::new($path)
Write-Host "grab $path size $($img.Width)x$($img.Height)"
$rowCounts = @{}
$colCounts = @{}
for ($y = 0; $y -lt $img.Height; $y++) {
    for ($x = 0; $x -lt $img.Width; $x++) {
        $c = $img.GetPixel($x, $y)
        if ($c.R -gt 180 -and $c.G -lt 90 -and $c.B -lt 90) {
            if (-not $rowCounts.ContainsKey($y)) { $rowCounts[$y] = 0 }
            if (-not $colCounts.ContainsKey($x)) { $colCounts[$x] = 0 }
            $rowCounts[$y] = $rowCounts[$y] + 1
            $colCounts[$x] = $colCounts[$x] + 1
        }
    }
}
$img.Dispose()
$rows = $rowCounts.Keys | Sort-Object
$cols = $colCounts.Keys | Sort-Object
Write-Host "red rows span $($rows[0])..$($rows[-1]) count=$($rows.Count)"
Write-Host "red cols span $($cols[0])..$($cols[-1]) count=$($cols.Count)"
# A painted horizontal edge is a row that spans nearly the whole box width; a clipped one is a short stub.
$long = $rows | Where-Object { $rowCounts[$_] -gt 200 }
Write-Host ("rows with >200 red px: " + (($long | ForEach-Object { "$_=$($rowCounts[$_])" }) -join ' '))
$tall = $cols | Where-Object { $colCounts[$_] -gt 200 }
Write-Host ("cols with >200 red px: " + (($tall | ForEach-Object { "$_=$($colCounts[$_])" }) -join ' '))
Write-Host ("top row reds: " + (($rows | Select-Object -First 4 | ForEach-Object { "$_=$($rowCounts[$_])" }) -join ' '))
Write-Host ("bottom row reds: " + (($rows | Select-Object -Last 4 | ForEach-Object { "$_=$($rowCounts[$_])" }) -join ' '))
Write-Host ("left col reds: " + (($cols | Select-Object -First 4 | ForEach-Object { "$_=$($colCounts[$_])" }) -join ' '))
Write-Host ("right col reds: " + (($cols | Select-Object -Last 4 | ForEach-Object { "$_=$($colCounts[$_])" }) -join ' '))
