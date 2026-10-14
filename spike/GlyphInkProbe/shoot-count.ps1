<#
.SYNOPSIS
  Grab the glyph-ink probe once it says its grid is on screen, then count ink cell by cell.

.DESCRIPTION
  #96 asks whether a Symbol glyph puts ink on the screen - the claim "字形有没有画出来测不出来"
  (docs/astra/audits/icon-family.md 6.1) is only true of the in-process capture paths, which re-run the render pass.
  This does the whole loop: start the probe, wait for the cell map it writes after its own layout pass (not a fixed
  sleep), raise and grab the window, stop the probe, then locate the red box it drew, derive the DIP-to-pixel scale
  from that box, and count dark pixels inside every cell. Two controls ride along: a text cell that must carry ink
  and a blank cell that must not - if the blank one has ink the alignment is wrong, and if the text one has none the
  instrument is blind, and neither would be a finding about fonts.
#>
[CmdletBinding()]
param(
    [ValidateSet('symbol', 'fluent', 'mdl2', 'markup')]
    [string] $Variant = 'symbol',
    [string] $OutDir = 'spike/GlyphInkProbe',
    [int] $ReadyBudgetSeconds = 60
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

$root = (Resolve-Path "$PSScriptRoot/../..").Path
Set-Location $root
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$bin = Join-Path $root 'spike/GlyphInkProbe/bin/Debug/net10.0-windows'
$exe = Join-Path $bin 'GlyphInkProbe.exe'
if (-not (Test-Path $exe)) { throw "missing $exe - build spike/GlyphInkProbe first" }
$map = Join-Path $bin "glyph-cells-$Variant.tsv"
$flag = Join-Path $bin 'glyph-stop.flag'
if (Test-Path $map) { Remove-Item $map -Force }

$probeLog = Join-Path $OutDir "probe-$Variant.log"
$proc = Start-Process -FilePath $exe -ArgumentList @('--variant', $Variant) -PassThru -RedirectStandardOutput $probeLog
# The probe writes the map after its first real layout, so the map arriving IS the "grid is on screen" signal.
$ready = $false
for ($try = 0; $try -lt ($ReadyBudgetSeconds * 10); $try++) {
    if ((Test-Path $map) -and ((Get-Item $map).Length -gt 100)) { $ready = $true; break }
    if ($proc.HasExited) { break }
    Start-Sleep -Milliseconds 100
}
if (-not $ready) {
    $proc | Stop-Process -Force -ErrorAction SilentlyContinue
    throw "the probe never wrote $map within $ReadyBudgetSeconds s (exited=$($proc.HasExited)) - no reading exists"
}

if (-not ('GlyphInkNative' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

public static class GlyphInkNative {
    // Without this the grabber lives in a virtualized 96-DPI coordinate space: GetWindowRect returns the size the DPI-
    // unaware probe asked for (its DIP numbers) while CopyFromScreen reads physical pixels, so the "window" grabbed is
    // a top-left crop of the real one and the box always looks clipped. Three runs read that as a window-sizing bug.
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    public struct Rect { public int Left, Top, Right, Bottom; }
    public static void Raise(IntPtr hwnd) {
        ShowWindow(hwnd, 5);
        SetWindowPos(hwnd, new IntPtr(-1), 0, 0, 0, 0, 0x0043);
        SetWindowPos(hwnd, new IntPtr(-2), 0, 0, 0, 0, 0x0043);
    }
}
'@
}
[void][GlyphInkNative]::SetProcessDPIAware()

$hwnd = [IntPtr]::Zero
for ($try = 0; $try -lt 40; $try++) {
    $proc.Refresh()
    $hwnd = $proc.MainWindowHandle
    if ($hwnd -ne [IntPtr]::Zero) { break }
    Start-Sleep -Milliseconds 250
}
if ($hwnd -eq [IntPtr]::Zero) { $proc | Stop-Process -Force; throw 'no probe window handle' }
[GlyphInkNative]::Raise($hwnd)
Start-Sleep -Milliseconds 900
$rect = New-Object GlyphInkNative+Rect
[void][GlyphInkNative]::GetWindowRect($hwnd, [ref]$rect)
$grab = Join-Path $OutDir "grab-$Variant.png"
$bounds = New-Object System.Drawing.Rectangle($rect.Left, $rect.Top, ($rect.Right - $rect.Left), ($rect.Bottom - $rect.Top))
$bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
$bitmap.Save($grab, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose(); $bitmap.Dispose()
Write-Host "grabbed pid=$($proc.Id) hwnd=$hwnd rect=$($bounds.X),$($bounds.Y),$($bounds.Width)x$($bounds.Height) -> $grab"

New-Item -ItemType File -Force -Path $flag | Out-Null
if (-not $proc.WaitForExit(30000)) { $proc | Stop-Process -Force; Write-Host 'probe killed after the hold budget' } else { Write-Host "probe closed pid=$($proc.Id)" }
# The probe's own account of the layout pass, printed before the counter argues about the box it measured.
Write-Host '--- probe geometry ---'
if (Test-Path $probeLog) { Get-Content $probeLog | Where-Object { $_ -match 'geom|witness|markup|assembly=|glyph-cells' } | ForEach-Object { Write-Host $_ } }

if (-not ('GlyphInkCounter' -as [type])) {
    Add-Type -ReferencedAssemblies 'System.Drawing.dll' -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

public static class GlyphInkCounter {
    private static double boxDipHeight(int cellDip, int cells, int columns) {
        return Math.Ceiling(cells / (double)columns) * cellDip;
    }

    public static void Run(string grabPath, string mapPath, int cellDip, int columns, string csv) {
        var cells = new List<int[]>();      // col,row per index
        var kinds = new List<string>();
        var names = new List<string>();
        var codepoints = new List<string>();
        var boxDip = "";
        foreach (var line in File.ReadAllLines(mapPath)) {
            if (line.StartsWith("#")) { boxDip = line.Substring(2); continue; }
            if (line.StartsWith("index")) continue;
            if (line.Trim().Length == 0) continue;
            var parts = line.Split('\t');
            if (parts.Length < 6) continue;
            cells.Add(new[] { int.Parse(parts[4]), int.Parse(parts[5]) });
            kinds.Add(parts[1]); names.Add(parts[2]); codepoints.Add(parts[3]);
        }
        // The scale the window itself reported, read out of the same header line as the geometry: an independent
        // witness of whether the painted box is whole. A clipped box measures narrower than this, and the
        // width/height ratio guard alone cannot tell a clipped box from a non-square scale.
        double reportedScale = 0;
        foreach (var token in boxDip.Split(' ')) {
            if (token.StartsWith("px-per-dip="))
                double.TryParse(token.Substring(11), NumberStyles.Float, CultureInfo.InvariantCulture, out reportedScale);
        }
        using (var img = new Bitmap(grabPath)) {
            // A raw min/max over red pixels is not the box: one stray red pixel anywhere in the grab (another window's
            // chrome coming into the shot) moved the bounds from 1680x840 to 1807x984 while the red count stayed the
            // same. The four edges are what is long, so the edges define the box.
            var rowCount = new int[img.Height];
            var colCount = new int[img.Width];
            var reds = 0;
            for (int y = 0; y < img.Height; y++) {
                for (int x = 0; x < img.Width; x++) {
                    var c = img.GetPixel(x, y);
                    if (c.R > 180 && c.G < 90 && c.B < 90) { reds++; rowCount[y]++; colCount[x]++; }
                }
            }
            int left = int.MaxValue, top = int.MaxValue, right = -1, bottom = -1;
            for (int x = 0; x < img.Width; x++) if (colCount[x] > 50) { if (x < left) left = x; right = x; }
            for (int y = 0; y < img.Height; y++) if (rowCount[y] > 50) { if (y < top) top = y; bottom = y; }
            if (reds < 100 || right < 0 || bottom < 0) { Console.WriteLine("SETUP FAILED: no red box found in the grab (red pixels=" + reds + ") - the instrument has no reading"); return; }
            double boxW = right - left + 1, boxH = bottom - top + 1;
            // The distance from the red rectangle to each edge of the grab: a side with 0 gap is a side the window cut
            // off, which is what the ratio guard below is for, and printing the gaps makes the two runs distinguishable
            // instead of leaving "clipped or covered" as an inference from two numbers.
            Console.WriteLine("box " + boxDip + " at " + left + "," + top + " size " + boxW + "x" + boxH +
                " red pixels=" + reds + " gap-to-edge l=" + left + " t=" + top +
                " r=" + (img.Width - 1 - right) + " b=" + (img.Height - 1 - bottom));
            // Parse the grid geometry out of the probe's own header instead of assuming it.
            int dipCell = cellDip, dipCols = columns;
            foreach (var token in boxDip.Split(' ')) {
                if (token.StartsWith("cell-dip=")) dipCell = int.Parse(token.Substring(9));
                if (token.StartsWith("columns=")) dipCols = int.Parse(token.Substring(8));
            }
            double boxDipWidth = dipCell * (double)dipCols;
            double scale = boxW / boxDipWidth;
            Console.WriteLine(string.Format("scale from the box: {0:0.####} px per DIP (box width {1} px for {2:0} DIP); window reported {3:0.####}",
                scale, boxW, boxDipWidth, reportedScale));
            if (reportedScale > 0 && Math.Abs(scale - reportedScale) / reportedScale > 0.03) {
                Console.WriteLine(string.Format("BOX NOT WHOLE: the painted box is {0:0.###} px per DIP wide while the window says {1:0.###} - the right edge is cut off, so no cell reading exists",
                    scale, reportedScale));
                return;
            }
            if (Math.Abs(boxH / boxDipHeight(dipCell, cells.Count, dipCols) - scale) / scale > 0.03) {
                Console.WriteLine(string.Format("BOX RATIO OFF: width says {0:0.####} px/DIP, height says {1:0.####} - the red box is clipped or something is on top of it", scale, boxH / boxDipHeight(dipCell, cells.Count, dipCols)));
                return;
            }

            var ink = new List<int>();
            var light = new List<int>();
            var sig = new List<string>();
            double inset = 3 * scale;
            for (int i = 0; i < cells.Count; i++) {
                int x0 = (int)Math.Round(left + cells[i][0] * dipCell * scale + inset);
                int y0 = (int)Math.Round(top + cells[i][1] * dipCell * scale + inset);
                int x1 = (int)Math.Round(left + (cells[i][0] + 1) * dipCell * scale - inset);
                int y1 = (int)Math.Round(top + (cells[i][1] + 1) * dipCell * scale - inset);
                int count = 0, faint = 0;
                var blocks = new int[64];
                for (int y = Math.Max(0, y0); y < Math.Min(img.Height, y1); y++) {
                    for (int x = Math.Max(0, x0); x < Math.Min(img.Width, x1); x++) {
                        var c = img.GetPixel(x, y);
                        bool dark = c.R < 128 && c.G < 128 && c.B < 128;
                        // The second threshold is what separates "this font has no glyph" from "the glyph is a hairline
                        // that anti-aliasing lifted above the dark floor" - the two read identically at one threshold,
                        // and only one of them is a fact about fonts.
                        bool lit = c.R < 215 || c.G < 215 || c.B < 215;
                        if (dark) count++;
                        if (lit) {
                            faint++;
                            int bx = Math.Min(7, (x - x0) * 8 / Math.Max(1, x1 - x0));
                            int by = Math.Min(7, (y - y0) * 8 / Math.Max(1, y1 - y0));
                            blocks[by * 8 + bx]++;
                        }
                    }
                }
                ulong bits = 0;
                for (int b = 0; b < 64; b++) if (blocks[b] >= 2) bits |= 1UL << b;
                ink.Add(count);
                light.Add(faint);
                sig.Add(bits.ToString("x16"));
            }

            int blankMax = 0, textInk = 0;
            for (int i = 0; i < kinds.Count; i++) {
                if (kinds[i] == "blank") blankMax = Math.Max(blankMax, ink[i]);
                if (kinds[i] == "text") textInk = ink[i];
            }
            int floor = Math.Max(3, blankMax * 4);
            Console.WriteLine("controls: text=" + textInk + " px, blank(max)=" + blankMax + " px, floor=" + floor);
            if (textInk < floor) { Console.WriteLine("INSTRUMENT BLIND: the text control carries no ink, so every zero below is this script's problem, not the fonts'"); return; }
            if (blankMax > floor / 4) { Console.WriteLine("ALIGNMENT WRONG: the blank control carries ink (" + blankMax + "), the cell rects are not on the grid"); return; }

            var empty = new List<string>();
            int symbolCells = 0, inked = 0, bare = 0;
            var sorted = new List<int>(ink);
            using (var w = new StreamWriter(csv, false)) {
                w.WriteLine("index,kind,name,codepoint,ink,light,sig");
                for (int i = 0; i < kinds.Count; i++) {
                    w.WriteLine(i + "," + kinds[i] + "," + names[i] + "," + codepoints[i] + "," + ink[i] + "," + light[i] + "," + sig[i]);
                    if (kinds[i] != "symbol") continue;
                    symbolCells++;
                    if (light[i] == 0) bare++;
                    if (ink[i] >= floor) inked++; else empty.Add(names[i] + "/" + codepoints[i] + "=" + ink[i] + "/" + light[i]);
                }
            }
            sorted.Sort();
            Console.WriteLine(string.Format("symbols={0} inked={1} below-floor={2} with-no-ink-at-all={3} median ink={4} p10={5} max={6}",
                symbolCells, inked, empty.Count, bare, sorted[sorted.Count / 2], sorted[Math.Max(0, sorted.Count / 10)], sorted[sorted.Count - 1]));
            Console.WriteLine("csv " + csv);
            int shown = Math.Min(40, empty.Count);
            Console.WriteLine("below floor: " + string.Join(" ", empty.GetRange(0, shown)) + (empty.Count > shown ? " ..." : ""));
        }
    }
}
'@
}

$counterCellDip = 24
$counterColumns = 40
[GlyphInkCounter]::Run((Join-Path $OutDir "grab-$Variant.png"), $map, $counterCellDip, $counterColumns, (Join-Path $OutDir "glyph-ink-$Variant.csv"))
