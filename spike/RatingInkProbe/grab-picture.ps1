# Grab the rating probe's own window once it says it is laid out, so the cell numbers in probe-<run>.log can be
# looked at as pixels. DPI-aware for the reason spike/GlyphInkProbe/README.md records: an unaware grabber gets a
# virtualised rect and physical pixels, which reads as a crop of the window.
param(
    [string] $Exe = 'spike/RatingInkProbe/bin/Debug/net10.0-windows/RatingInkProbe.exe',
    [string] $Out = 'spike/RatingInkProbe/rating-after.png'
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
$root = (Resolve-Path "$PSScriptRoot/../..").Path
Set-Location $root

if (-not ('RatingGrabNative' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class RatingGrabNative {
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
    public struct Rect { public int Left, Top, Right, Bottom; }
    public static void Raise(IntPtr hwnd) {
        ShowWindow(hwnd, 5);
        SetWindowPos(hwnd, new IntPtr(-1), 0, 0, 0, 0, 0x0043);
        SetWindowPos(hwnd, new IntPtr(-2), 0, 0, 0, 0, 0x0043);
    }
}
'@
}
[void][RatingGrabNative]::SetProcessDPIAware()

$exePath = (Resolve-Path $Exe).Path
$bin = Split-Path $exePath
$ready = Join-Path $bin 'rating-ready.flag'
$flag = Join-Path $bin 'rating-stop.flag'
if (Test-Path $ready) { Remove-Item $ready -Force }
if (Test-Path $flag) { Remove-Item $flag -Force }

$proc = Start-Process -FilePath $exePath -PassThru
for ($try = 0; $try -lt 600; $try++) {
    if ((Test-Path $ready) -and $proc.MainWindowHandle -ne [IntPtr]::Zero) { break }
    if ($proc.HasExited) { throw "probe exited before it laid out (ready=$(Test-Path $ready))" }
    Start-Sleep -Milliseconds 100
}
if (-not (Test-Path $ready)) { $proc | Stop-Process -Force; throw "probe never wrote $ready" }

$hwnd = [IntPtr]::Zero
for ($try = 0; $try -lt 40; $try++) {
    $proc.Refresh()
    $hwnd = $proc.MainWindowHandle
    if ($hwnd -ne [IntPtr]::Zero) { break }
    Start-Sleep -Milliseconds 250
}
if ($hwnd -eq [IntPtr]::Zero) { $proc | Stop-Process -Force; throw 'no probe window handle' }
[RatingGrabNative]::Raise($hwnd)
Start-Sleep -Milliseconds 900

$rect = New-Object RatingGrabNative+Rect
[void][RatingGrabNative]::GetWindowRect($hwnd, [ref]$rect)
$bounds = New-Object System.Drawing.Rectangle($rect.Left, $rect.Top, ($rect.Right - $rect.Left), ($rect.Bottom - $rect.Top))
$bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
$bitmap.Save((Join-Path $root $Out), [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose(); $bitmap.Dispose()
Write-Host "grabbed pid=$($proc.Id) rect=$($bounds.X),$($bounds.Y),$($bounds.Width)x$($bounds.Height) -> $Out"

New-Item -ItemType File -Force -Path $flag | Out-Null
if (-not $proc.WaitForExit(30000)) { $proc | Stop-Process -Force; Write-Host 'probe killed' } else { Write-Host "probe closed pid=$($proc.Id)" }
