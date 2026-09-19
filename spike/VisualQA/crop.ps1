param([string]$In, [int]$X, [int]$Y, [int]$W, [int]$H, [string]$Out)
Add-Type -AssemblyName System.Drawing
$bmp = [System.Drawing.Bitmap]::FromFile($In)
Write-Host "src $($bmp.Width) x $($bmp.Height)"
$rect = New-Object System.Drawing.Rectangle $X,$Y,$W,$H
$crop = New-Object System.Drawing.Bitmap $W,$H
$g = [System.Drawing.Graphics]::FromImage($crop)
$g.InterpolationMode = 'NearestNeighbor'
$g.DrawImage([System.Drawing.Image]$bmp, (New-Object System.Drawing.Rectangle 0,0,$W,$H), $rect, [System.Drawing.GraphicsUnit]::Pixel)
$g.Dispose(); $bmp.Dispose()
# scale 2x for readability
$big = New-Object System.Drawing.Bitmap ($W*2),($H*2)
$g2 = [System.Drawing.Graphics]::FromImage($big)
$g2.InterpolationMode = 'NearestNeighbor'
$g2.DrawImage([System.Drawing.Image]$crop, 0, 0, $W*2, $H*2)
$g2.Dispose(); $crop.Dispose()
$big.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$big.Dispose()
Write-Host "wrote $Out"
