<#
.SYNOPSIS
  Shoot the running Gallery before and after a live Light->Dark theme flip, so "the pane icons keep their old ink"
  becomes a number instead of an impression.

.DESCRIPTION
  An in-process RenderTargetBitmap cannot see this defect: it re-runs the render pass, so a glyph whose ink is
  resolved at draw time picks up the new theme even when the on-screen compositor still shows the old one. Only a
  grab of the monitor answers what the user sees. This launches the Gallery with the temporary Program.cs flip hook
  (ASTRA_START_THEME=light, ASTRA_FLIP_MS=6000), grabs the whole virtual screen twice, records the window rect and
  DPI, and closes the process it started.
#>
[CmdletBinding()]
param(
    [int] $FlipMs = 6000,
    [string] $StartTheme = 'light',
    [string] $FlipTo = 'dark',
    [int] $BeforeMs = 2500,
    [int] $AfterMs = 9500,
    [string] $OutDir = 'spike/NavIconRecolor'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
if (-not ('NavRecolor.Native' -as [type])) {
    Add-Type -ReferencedAssemblies 'System.Drawing.dll', 'System.Windows.Forms.dll' -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

public static class NavRecolor {
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

    public struct Rect { public int Left, Top, Right, Bottom; }

    // SetForegroundWindow is refused across processes by the foreground lock; a topmost slot is not.
    public static void Raise(IntPtr hwnd) {
        ShowWindow(hwnd, 5);
        SetWindowPos(hwnd, new IntPtr(-1), 0, 0, 0, 0, 0x0043);
        SetWindowPos(hwnd, new IntPtr(-2), 0, 0, 0, 0, 0x0043);
        BringWindowToTop(hwnd);
        SetForegroundWindow(hwnd);
    }

    public static void Dump(IntPtr hwnd) {
        Rect r;
        GetWindowRect(hwnd, out r);
        Console.WriteLine("RECT {0} {1} {2} {3} DPI {4} VISIBLE {5}"
            .Replace("{0}", r.Left.ToString()).Replace("{1}", r.Top.ToString())
            .Replace("{2}", r.Right.ToString()).Replace("{3}", r.Bottom.ToString())
            .Replace("{4}", GetDpiForWindow(hwnd).ToString())
            .Replace("{5}", IsWindowVisible(hwnd).ToString()));
    }

    public static void Crop(string path, string outPath, int left, int top, int width, int height) {
        using (var bmp = new System.Drawing.Bitmap(path)) {
            using (var crop = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
                using (var g = System.Drawing.Graphics.FromImage(crop)) {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.DrawImage((System.Drawing.Image)bmp,
                        new System.Drawing.Rectangle(0, 0, width, height),
                        new System.Drawing.Rectangle(left, top, width, height),
                        System.Drawing.GraphicsUnit.Pixel);
                }
                crop.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        Console.WriteLine("crop " + outPath);
    }
}
'@
}

$root = (Resolve-Path "$PSScriptRoot/../..").Path
Set-Location $root
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$exe = Join-Path $root 'samples/FluentJalium.Gallery/bin/Debug/net10.0-windows/FluentJalium.Gallery.exe'
if (-not (Test-Path $exe)) { throw "missing $exe - build the Gallery first" }

$env:ASTRA_START_THEME = $StartTheme
$env:ASTRA_FLIP_TO = $FlipTo
$env:ASTRA_FLIP_MS = "$FlipMs"
$proc = Start-Process -FilePath $exe -PassThru

function Grab([string] $tag, [string] $out, [int] $pid_, [int] $paneWidth, [int] $paneHeight) {
    # MainWindowHandle is cached on the Process object, so it has to be re-fetched until the runtime sets the caption.
    $hwnd = [IntPtr]::Zero
    for ($try = 0; $try -lt 40; $try++) {
        $p = Get-Process -Id $pid_ -ErrorAction SilentlyContinue
        if ($null -eq $p) { throw "gallery pid=$pid_ gone before $tag" }
        $p.Refresh()
        $hwnd = $p.MainWindowHandle
        if ($hwnd -ne [IntPtr]::Zero) { break }
        Start-Sleep -Milliseconds 250
    }
    if ($hwnd -eq [IntPtr]::Zero) { throw "no Gallery window handle for $tag" }
    # Whatever else is on the monitor sits on top of a window that was activated from a background process.
    [NavRecolor]::Raise($hwnd)
    Start-Sleep -Milliseconds 700
    $png = Join-Path $root "$OutDir/$tag.png"
    & (Join-Path $root 'spike/VisualQA/grab-screen.ps1') -Out $png
    $rect = New-Object NavRecolor+Rect
    [void][NavRecolor]::GetWindowRect($hwnd, [ref]$rect)
    Write-Host ("{0} pid={1} hwnd={2} rect={3},{4},{5},{6} dpi={7}" -f $tag, $pid_, $hwnd, $rect.Left, $rect.Top, $rect.Right, $rect.Bottom, [NavRecolor]::GetDpiForWindow($hwnd))
    [NavRecolor]::Crop($png, (Join-Path $root "$OutDir/$tag-pane.png"), $rect.Left, $rect.Top, $paneWidth, $paneHeight)
    "$($rect.Left) $($rect.Top) $($rect.Right) $($rect.Bottom)" | Out-File -Encoding ascii (Join-Path $root "$OutDir/$tag-rect.txt")
}

Start-Sleep -Milliseconds 1800
Grab 'before' '' $proc.Id 460 560
Start-Sleep -Milliseconds ($AfterMs - $BeforeMs)
Grab 'after' '' $proc.Id 460 560

Stop-Process -Id $proc.Id -ErrorAction SilentlyContinue
Write-Host "closed pid=$($proc.Id)"
