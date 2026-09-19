<#
.SYNOPSIS
  PrintWindow every visible top-level window of a running process, one file each.

.DESCRIPTION
  capture-window.ps1 keeps the largest window, which is the right default for a page but wrong for a popup:
  a flyout's own window can be taller than the tiny host that anchors it while still not being the widest, and
  the question this answers ("did shrinking the separator row clip the rule the control draws?") is only
  visible in the popup. So this writes one PNG per window, named with its size, and gates each grab on the
  frame actually having been drawn (docs/astra/adaptation/00 S0-r correction: PrintWindow can return a blank
  frame from a window that is rendering).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $Name,
    [Parameter(Mandatory = $true)][string] $Out,
    [int] $Attempts = 8
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
if (-not ('VisualQaGrabMany' -as [type])) {
    Add-Type -ReferencedAssemblies 'System.Drawing.dll' -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class VisualQaGrabMany {
    [DllImport("user32.dll")] private static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr extra);
    private delegate bool EnumProc(IntPtr hwnd, IntPtr extra);

    public struct Rect { public int Left, Top, Right, Bottom; }
    public static int Dpi;

    public sealed class Found { public IntPtr Handle; public int Width; public int Height; }

    public static void BecomeDpiAware() { SetProcessDpiAwarenessContext(new IntPtr(-4)); }

    public static List<Found> WindowsOf(int processId) {
        var found = new List<Found>();
        EnumWindows((hwnd, extra) => {
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            if ((int)pid != processId || !IsWindowVisible(hwnd)) return true;
            Rect rect;
            if (!GetWindowRect(hwnd, out rect)) return true;
            if (rect.Right - rect.Left < 16 || rect.Bottom - rect.Top < 16) return true;
            found.Add(new Found { Handle = hwnd, Width = rect.Right - rect.Left, Height = rect.Bottom - rect.Top });
            return true;
        }, IntPtr.Zero);
        return found;
    }

    public static Bitmap Grab(IntPtr hwnd) {
        Rect rect;
        if (!GetWindowRect(hwnd, out rect)) throw new InvalidOperationException("GetWindowRect refused");
        Dpi = (int)GetDpiForWindow(hwnd);
        var bitmap = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap)) {
            var hdc = graphics.GetHdc();
            if (!PrintWindow(hwnd, hdc, 3)) throw new InvalidOperationException("PrintWindow refused");
            graphics.ReleaseHdc(hdc);
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

[VisualQaGrabMany]::BecomeDpiAware()
$process = Get-Process -Name $Name -ErrorAction Stop | Select-Object -First 1
for ($attempt = 0; $attempt -lt $Attempts; $attempt++) {
    $windows = [VisualQaGrabMany]::WindowsOf($process.Id)
    $written = 0
    foreach ($window in $windows) {
        $bitmap = [VisualQaGrabMany]::Grab($window.Handle)
        try {
            $painted = [VisualQaGrabMany]::NonBlack($bitmap)
            if ($painted -lt 100) { continue }
            $target = "{0}-{1}x{2}-{3}.png" -f $Out, $window.Width, $window.Height, [VisualQaGrabMany]::Dpi
            $bitmap.Save($target, [System.Drawing.Imaging.ImageFormat]::Png)
            Write-Host "wrote $target painted=$painted"
            $written++
        }
        finally { $bitmap.Dispose() }
    }
    if ($written -ge $windows.Count -and $written -gt 0) { break }
    Start-Sleep -Milliseconds 600
}
Write-Host "windows seen: $(([VisualQaGrabMany]::WindowsOf($process.Id)).Count)"
