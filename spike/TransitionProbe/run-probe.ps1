<#
.SYNOPSIS
  Run the transition-commit matrix probe, PrintWindow it three times, and report which cells reached the frame.

.DESCRIPTION
  Companion to spike/TransitionProbe. The probe writes an EXPECT line per cell with the one colour only that
  cell paints, then marks its capture phases against a clock started at Loaded; this script waits on those
  marks, so every capture is aligned to the probe's own render loop rather than to a guessed sleep. A cell
  that never lands reads 0 - the number the spacing batch had to derive by hand for one app-bar cell.
  The grab is PrintWindow with PW_CLIENTONLY|PW_RENDERFULLCONTENT, the same call capture-pages.ps1 uses, and
  like that script it refuses windows whose rect cannot be read: a Jalium top-level that has not been placed
  yet answers 0x0 and would yield a blank bitmap, which is how the first run of this script lost its frames.
  Not a gate: a screenshot is not evidence, it is what makes a claim measurable in the first place.
#>
[CmdletBinding()]
param(
    [string] $Configuration = 'Debug',
    [int] $TimeoutSeconds = 120
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$outDir = Join-Path $PSScriptRoot 'out'
if (-not (Test-Path $outDir)) { [void](New-Item -ItemType Directory -Path $outDir) }

if (-not ('TransitionProbeGrab' -as [type])) {
    Add-Type -ReferencedAssemblies 'System.Drawing.dll' -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class TransitionProbeGrab {
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

    // PER_PROCESS_AWARE_V2: without it every coordinate is virtualised by the 175% scale on this machine.
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

    public static Dictionary<string, int> Grab(IntPtr hwnd, string path, string[] hexes) {
        Rect rect;
        if (!GetWindowRect(hwnd, out rect)) throw new InvalidOperationException("GetWindowRect refused");
        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;
        Info = "hwnd=" + hwnd + " dpi=" + GetDpiForWindow(hwnd) + " " + width + "x" + height + " at " + rect.Left + "," + rect.Top;

        var counts = new Dictionary<string, int>();
        foreach (var hex in hexes) counts[hex] = 0;
        var wanted = new Dictionary<string, uint>();
        foreach (var hex in hexes) {
            wanted[hex] = (uint)(Convert.ToInt32(hex.Substring(1, 2), 16) << 16 |
                                 Convert.ToInt32(hex.Substring(3, 2), 16) << 8 |
                                 Convert.ToInt32(hex.Substring(5, 2), 16));
        }

        using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb)) {
            using (var graphics = Graphics.FromImage(bitmap)) {
                var hdc = graphics.GetHdc();
                // 3 == PW_CLIENTONLY | PW_RENDERFULLCONTENT: the only flag pair that reaches a
                // GPU-composited Jalium surface at all.
                if (!PrintWindow(hwnd, hdc, 3)) throw new InvalidOperationException("PrintWindow refused");
                graphics.ReleaseHdc(hdc);
            }
            bitmap.Save(path, ImageFormat.Png);
            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var buffer = new byte[Math.Abs(data.Stride) * height];
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
            bitmap.UnlockBits(data);
            for (var i = 0; i + 3 < buffer.Length; i += 4) {
                uint key = (uint)(buffer[i + 2] << 16 | buffer[i + 1] << 8 | buffer[i]);
                foreach (var pair in wanted) {
                    if (pair.Value == key) counts[pair.Key]++;
                }
            }
        }
        return counts;
    }
}
'@
}

[TransitionProbeGrab]::BecomeDpiAware()

$bin = Join-Path $root "spike\TransitionProbe\bin\$Configuration\net10.0-windows"
Write-Host "build spike/TransitionProbe ($Configuration)"
dotnet build (Join-Path $root 'spike\TransitionProbe\TransitionProbe.csproj') --configuration $Configuration --nologo -v q | Out-Host
if ($LASTEXITCODE -ne 0) { throw 'probe build failed' }

$exe = Join-Path $bin 'TransitionProbe.exe'
$logPath = Join-Path $bin 'transition-probe.txt'
$markPath = Join-Path $bin 'transition-probe.marks'
foreach ($stale in @($logPath, $markPath)) { if (Test-Path $stale) { Remove-Item $stale } }
Get-ChildItem $outDir -Filter 'transition-capture*.png' -ErrorAction SilentlyContinue | Remove-Item

$process = Start-Process -FilePath $exe -PassThru
Write-Host "probe pid=$($process.Id)"

function Wait-ForMark([string]$label, [int]$seconds) {
    $deadline = (Get-Date).AddSeconds($seconds)
    while ((Get-Date) -lt $deadline) {
        if ($process.HasExited) { return $false }
        if ((Test-Path $markPath) -and (Select-String -Path $markPath -Pattern "`t$label$" -Quiet)) { return $true }
        Start-Sleep -Milliseconds 50
    }
    return $false
}

# The colours are only known once the probe has written its EXPECT lines, and ready is marked after them.
if (-not (Wait-ForMark 'ready' $TimeoutSeconds)) {
    if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    throw 'probe never got to the capture phase'
}

$expect = [ordered]@{}
foreach ($line in Get-Content $logPath) {
    if ($line -like 'EXPECT*') {
        $parts = $line -split "`t"
        $expect[$parts[1]] = $parts[2]
    }
}
$hexes = @($expect.Values)
Write-Host "cells=$($expect.Count)"

$perCapture = [ordered]@{}
foreach ($phase in @('capture1', 'capture2', 'capture3')) {
    if (-not (Wait-ForMark $phase 30)) {
        Write-Warning "no $phase mark (process exited: $($process.HasExited))"
        continue
    }
    $png = Join-Path $outDir "transition-$phase.png"
    # A PrintWindow grab of a GPU surface can come back blank, and a blank grab is indistinguishable from
    # every cell missing, so keep taking the picture until something is on it. Each attempt re-reads the
    # window list: a handle whose rect was mid-move refuses the very next GetWindowRect call.
    $counts = $null
    for ($attempt = 0; $attempt -lt 8; $attempt++) {
        try {
            $windows = [TransitionProbeGrab]::WindowsOf($process.Id, 64)
            if ($windows.Count -eq 0) { Start-Sleep -Milliseconds 200; continue }
            if ($windows.Count -gt 1) { Write-Warning "$($windows.Count) placeable windows; the first was read" }
            $try = [TransitionProbeGrab]::Grab($windows[0], $png, $hexes)
            $sum = ($try.Values | Measure-Object -Sum).Sum
            Write-Host "$phase attempt$attempt -> $([TransitionProbeGrab]::Info) cells-at-once=$sum"
            if ($sum -gt 0) { $counts = $try; break }
        } catch {
            Write-Host "$phase attempt$attempt threw $($_.Exception.Message)"
            Start-Sleep -Milliseconds 200
        }
    }
    if ($null -eq $counts) { Write-Warning "$phase never produced a grab with any cell in it" } else { $perCapture[$phase] = $counts }
}

if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
    Write-Warning 'probe still running; killing'
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
}

Write-Host "`n=== probe log ==="
Get-Content $logPath | Out-Host

Write-Host "`n=== transition cells x capture (0 = the composited frame never got the value) ==="
$rows = foreach ($cell in $expect.Keys) {
    $hex = $expect[$cell]
    $row = [ordered]@{ cell = $cell; hex = $hex }
    foreach ($phase in $perCapture.Keys) { $row[$phase] = $perCapture[$phase][$hex] }
    [pscustomobject]$row
}
$rows | Format-Table -AutoSize | Out-Host
