<#
.SYNOPSIS
  Throwaway visual-QA capture: shows the Gallery one page at a time and writes a PNG of each window.

.DESCRIPTION
  Not a gate. docs/astra/adaptation/06 records why a screenshot is not evidence in this runtime, so the
  serial gate script deliberately captures no image; this exists only so a human and the agent can look at
  the same pixels and talk about ghosting, spacing and rhythm. PrintWindow with PW_RENDERFULLCONTENT is what
  reaches a GPU-composited Jalium window; the DPI context is set per-process and the window's own DPI is
  printed so a measurement taken from the image can be converted back to DIPs.
#>
[CmdletBinding()]
param(
    [string[]] $Page = @('buttons'),
    [int] $SettleMilliseconds = 3000,
    [string] $Out = ""
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$executable = Join-Path $root 'samples/FluentJalium.Gallery/bin/Debug/net10.0-windows/FluentJalium.Gallery.exe'
if (-not (Test-Path $executable)) { Write-Error "No built Gallery at $executable." }

Add-Type -AssemblyName System.Drawing
if (-not ('VisualQa.Native' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using System.Drawing;

public static class VisualQa {
    [DllImport("user32.dll")] public static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr extra);
    private delegate bool EnumProc(IntPtr hwnd, IntPtr extra);

    public struct Rect { public int Left, Top, Right, Bottom; }

    public static void SetDpiAware() { SetProcessDpiAwarenessContext(new IntPtr(-4)); }

    /// <summary>
    /// The largest visible top-level window of a process. Process.MainWindowHandle is not usable here: it
    /// latched onto a 48x32 window the Gallery creates before its real one, and returned zero outright on
    /// a second run, so the pick has to be made from the window list itself.
    /// </summary>
    public static IntPtr LargestWindow(uint processId) {
        IntPtr best = IntPtr.Zero;
        long bestArea = 0;
        EnumWindows((hwnd, extra) => {
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            if (pid != processId || !IsWindowVisible(hwnd)) { return true; }
            Rect rect;
            if (!GetWindowRect(hwnd, out rect)) { return true; }
            long area = (long)(rect.Right - rect.Left) * (rect.Bottom - rect.Top);
            if (area > bestArea) { bestArea = area; best = hwnd; }
            return true;
        }, IntPtr.Zero);
        return best;
    }

    public static Bitmap Grab(IntPtr hwnd) {
        Rect rect;
        if (!GetWindowRect(hwnd, out rect)) { throw new InvalidOperationException("GetWindowRect failed"); }
        int width = rect.Right - rect.Left, height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0) { throw new InvalidOperationException("empty window rect"); }
        var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap)) {
            IntPtr hdc = graphics.GetHdc();
            // PW_CLIENTONLY(1) | PW_RENDERFULLCONTENT(2)
            bool ok = PrintWindow(hwnd, hdc, 3);
            graphics.ReleaseHdc(hdc);
            if (!ok) { throw new InvalidOperationException("PrintWindow refused"); }
        }
        return bitmap;
    }
}
'@ -ReferencedAssemblies System.Drawing
}

[VisualQa]::SetDpiAware()
if (-not $Out) { $Out = Join-Path $PSScriptRoot "out" }
New-Item -ItemType Directory -Force -Path $Out | Out-Null

foreach ($page in ($Page | ForEach-Object { $_ -split ',' } | Where-Object { $_ })) {
    Write-Host "==> $page"
    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $executable
    $startInfo.Arguments = "--page $page"
    $startInfo.UseShellExecute = $true
    $process = [System.Diagnostics.Process]::Start($startInfo)
    try {
        if (-not $process.WaitForInputIdle(30000)) { Write-Warning "$page never went idle"; continue }
        Start-Sleep -Milliseconds $SettleMilliseconds
        $process.Refresh()
        $hwnd = [VisualQa]::LargestWindow([uint32]$process.Id)
        if ($hwnd -eq [IntPtr]::Zero) { Write-Warning "$page published no visible window"; continue }
        Write-Host "   dpi=$([VisualQa]::GetDpiForWindow($hwnd))"
        $bitmap = [VisualQa]::Grab($hwnd)
        try {
            $path = Join-Path $Out "$page.png"
            $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
            Write-Host "   saved $($bitmap.Width)x$($bitmap.Height) -> $path"
        } finally { $bitmap.Dispose() }
    } finally {
        $process.Refresh()
        if ($process.MainWindowHandle -ne [IntPtr]::Zero) { [void]$process.CloseMainWindow() }
        if (-not $process.WaitForExit(15000)) { Stop-Process -Id $process.Id -Force }
        $process.Dispose()
    }
}
