<#
.SYNOPSIS
  Grab the whole virtual screen as one PNG, in physical pixels.

.DESCRIPTION
  PrintWindow asks a window to redraw itself into a DC, which answers "what does this control's own surface
  look like" but not "what does the user see": a popup that the runtime puts in its own top-level window is
  composited by the OS, and enumerating visible top-level windows cannot say which of the handles is the
  popup (a probe with one 760x900 window still showed a second visible 260x80 window in every mode,
  including the closed one). This script has no opinion about windows - it copies the screen, so the
  geometry it produces is the geometry on the monitor.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $Out
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
if (-not ('VisualQaScreen' -as [type])) {
    Add-Type -ReferencedAssemblies 'System.Drawing.dll', 'System.Windows.Forms.dll' -TypeDefinition @'
using System;
using System.Drawing;
using System.Runtime.InteropServices;

public static class VisualQaScreen {
    [DllImport("user32.dll")] private static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();

    public static int Dpi;

    public static void BecomeDpiAware() { SetProcessDpiAwarenessContext(new IntPtr(-4)); }

    public static void Save(string path) {
        var bounds = System.Windows.Forms.SystemInformation.VirtualScreen;
        using (var bitmap = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
            using (var graphics = Graphics.FromImage(bitmap)) {
                graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }
            bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }
        Dpi = (int)GetDpiForWindow(GetForegroundWindow());
    }
}
'@
}

[VisualQaScreen]::BecomeDpiAware()
[VisualQaScreen]::Save($Out)
Write-Host ("wrote {0} dpi={1}" -f $Out, [VisualQaScreen]::Dpi)
