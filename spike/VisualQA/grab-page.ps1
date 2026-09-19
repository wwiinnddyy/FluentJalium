<#
.SYNOPSIS
  Start one of this repo's own windowed executables, PrintWindow it once it has drawn, and close it again.

.DESCRIPTION
  capture-pages.ps1 gates its grab on WaitForInputIdle, and when that times out it skips the page and writes
  no file at all - which reads like "the page never settles" but says nothing about the pixels. This helper
  drops that gate: start, wait, then take the picture repeatedly until a frame actually contains something,
  and always close the window it opened. It knows nothing about colours, so pair it with count-colors.ps1.
  Not a gate: it exists to make a visual claim measurable twice.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $Exe,
    [string] $Arguments = '',
    [Parameter(Mandatory = $true)][string] $Out,
    [int] $SettleMilliseconds = 8000,
    [int] $Attempts = 8
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
if (-not ('VisualQaGrab' -as [type])) {
    Add-Type -ReferencedAssemblies 'System.Drawing.dll' -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class VisualQaGrab {
    [DllImport("user32.dll")] private static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr extra);
    private delegate bool EnumProc(IntPtr hwnd, IntPtr extra);

    public struct Rect { public int Left, Top, Right, Bottom; }
    public static string Info = "";

    public static void BecomeDpiAware() { SetProcessDpiAwarenessContext(new IntPtr(-4)); }

    public static List<IntPtr> WindowsOf(int processId, int minWidth) {
        var found = new List<IntPtr>();
        EnumWindows((hwnd, extra) => {
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            if ((int)pid != processId || !IsWindowVisible(hwnd)) return true;
            Rect rect;
            if (!GetWindowRect(hwnd, out rect)) return true;
            if (rect.Right - rect.Left < minWidth || rect.Bottom - rect.Top < minWidth) return true;
            found.Add(hwnd);
            return true;
        }, IntPtr.Zero);
        return found;
    }

    // Returns the picture the way capture-pages.ps1 does: saving it in PowerShell is the half that is
    // already known to work here, so the grab only grabs.
    public static Bitmap Grab(IntPtr hwnd) {
        Rect rect;
        if (!GetWindowRect(hwnd, out rect)) throw new InvalidOperationException("GetWindowRect refused");
        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0) throw new InvalidOperationException("empty window rect");
        Info = "dpi=" + GetDpiForWindow(hwnd) + " " + width + "x" + height + " at " + rect.Left + "," + rect.Top;
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap)) {
            var hdc = graphics.GetHdc();
            // 3 == PW_CLIENTONLY | PW_RENDERFULLCONTENT: the only flag pair that reaches the surface.
            var ok = PrintWindow(hwnd, hdc, 3);
            graphics.ReleaseHdc(hdc);
            if (!ok) throw new InvalidOperationException("PrintWindow refused");
        }
        return bitmap;
    }

    public static int NonBlack(Bitmap bitmap) {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var buffer = new byte[Math.Abs(data.Stride) * height];
        Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
        bitmap.UnlockBits(data);
        var painted = 0;
        for (var i = 0; i + 3 < buffer.Length; i += 4) {
            if (buffer[i] != 0 || buffer[i + 1] != 0 || buffer[i + 2] != 0) painted++;
        }
        return painted;
    }
}
'@
}

[VisualQaGrab]::BecomeDpiAware()
$directory = Split-Path -Parent $Exe
$process = Start-Process -FilePath $Exe -ArgumentList $Arguments -WorkingDirectory $directory -PassThru
Write-Host "pid=$($process.Id) args=$Arguments"
try {
    Start-Sleep -Milliseconds $SettleMilliseconds
    for ($attempt = 0; $attempt -lt $Attempts; $attempt++) {
        $windows = [VisualQaGrab]::WindowsOf($process.Id, 64)
        if ($windows.Count -eq 0) { Start-Sleep -Milliseconds 500; continue }
        $bitmap = [VisualQaGrab]::Grab($windows[0])
        try {
            $painted = [VisualQaGrab]::NonBlack($bitmap)
            $bitmap.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
            Write-Host "attempt$attempt $([VisualQaGrab]::Info) painted=$painted -> $Out"
        }
        finally { $bitmap.Dispose() }
        if ($painted -gt 1000) { break }
        Start-Sleep -Milliseconds 500
    }
}
finally {
    $process.Refresh()
    if ($process.MainWindowHandle -ne [IntPtr]::Zero) { [void]$process.CloseMainWindow() }
    if (-not $process.WaitForExit(15000)) { Stop-Process -Id $process.Id -Force }
    $process.Dispose()
    Write-Host 'window closed'
}
