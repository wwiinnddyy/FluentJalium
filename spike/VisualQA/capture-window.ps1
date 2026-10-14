<#
.SYNOPSIS
  Throwaway visual-QA helper: PrintWindow the largest visible top-level window of a process into a PNG.

.DESCRIPTION
  Companion to capture-pages.ps1 for a process this repo owns but the Gallery script cannot launch -
  a spike that holds a shown window for a few seconds. A control's own RenderTargetBitmap crop cannot show
  what a template's TextBlock paints (docs/astra/adaptation/06), so the only way to ask "who drew this text
  twice" is to capture the composited window from outside. Not a gate: a screenshot is not evidence, it is
  what makes a defect worth measuring.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $Name,
    [Parameter(Mandatory = $true)][string] $Out,
    [int] $TimeoutSeconds = 30
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
if (-not ('CaptureWindow.Native' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class CaptureWindow {
    [DllImport("user32.dll")] public static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr extra);
    private delegate bool EnumProc(IntPtr hwnd, IntPtr extra);

    public struct Rect { public int Left, Top, Right, Bottom; }

    // A spike's own window: the largest visible top-level of that process, the same rule capture-pages.ps1 uses.
    public static List<IntPtr> WindowsOf(int processId) {
        var found = new List<IntPtr>();
        EnumWindows((hwnd, extra) => {
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            if (pid == (uint)processId && IsWindowVisible(hwnd)) found.Add(hwnd);
            return true;
        }, IntPtr.Zero);
        return found;
    }
}
'@
}

# -4 == PER_PROCESS_AWARE_V2: without it every coordinate below is virtualised by the 175% scale.
[void][CaptureWindow]::SetProcessDpiAwarenessContext([IntPtr](-4))

# Git Bash hands out MSYS pids, so a pid passed in from there is meaningless to Win32: resolve by name here.
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
$windows = @()
$processId = 0
while ($windows.Count -eq 0 -and (Get-Date) -lt $deadline) {
    $candidate = Get-Process -Name $Name -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($candidate) {
        $processId = $candidate.Id
        $windows = @([CaptureWindow]::WindowsOf($processId))
    }

    if ($windows.Count -eq 0) { Start-Sleep -Milliseconds 250 }
}

if ($windows.Count -eq 0) { Write-Error "No visible window for process $Name within $TimeoutSeconds s." }

foreach ($hwnd in $windows) {
    $bounds = New-Object 'CaptureWindow+Rect'
    [void][CaptureWindow]::GetWindowRect($hwnd, [ref]$bounds)
    $dpi = [CaptureWindow]::GetDpiForWindow($hwnd)
    $width = $bounds.Right - $bounds.Left
    $height = $bounds.Bottom - $bounds.Top
    # A popup window is a second top-level of the same process: every one of them gets its own file.
    $target = if ($windows.Count -eq 1) { $Out } else { "$Out.$($bounds.Left)x$($bounds.Top).png" }
    Write-Host "hwnd=$hwnd dpi=$dpi ${width}x${height} -> $target"

    $bitmap = New-Object System.Drawing.Bitmap $width, $height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $hdc = $graphics.GetHdc()
    # 2 == PW_RENDERFULLCONTENT: the only flag that reaches a GPU-composited Jalium surface.
    [void][CaptureWindow]::PrintWindow($hwnd, $hdc, 2)
    $graphics.ReleaseHdc($hdc)
    $graphics.Dispose()
    $bitmap.Save($target, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}
