<#
.SYNOPSIS
  Compare the two grabs of one live theme flip and report the pixels whose ink did NOT move: dark in both, or
  light in both. Text and icons that re-tint drop out of both counts, so what is left is frozen ink.

.DESCRIPTION
  shoot.ps1 grabs the same window before and after a live flip. A host whose glyph ink comes from a value the
  framework resolves at draw time keeps the first theme's pixels, and an in-process capture cannot see that
  (#94, spike/NavIconRecolor/ab-pixels.md). Hunting the frozen element by eye needs its coordinates; this needs
  none - it walks a rectangle of the window and counts pixels that are near-black in both grabs and pixels that
  are near-white in both, then writes a mask of the dark-in-both set so the offender can be located on the crop.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $Before,
    [Parameter(Mandatory = $true)][string] $After,
    [int] $Left = 0,
    [int] $Top = 0,
    [int] $Right = 2254,
    [int] $Bottom = 1481,
    [int] $Dark = 60,
    [int] $Light = 230,
    [string] $Mask,
    [string] $Crop
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
if (-not ('NavRecolorFrozen' -as [type])) {
    Add-Type -ReferencedAssemblies 'System.Drawing.dll' -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;

public static class NavRecolorFrozen {
    public static void Run(string before, string after, int left, int top, int right, int bottom, int dark, int light, string mask, string crop) {
        using (var a = new Bitmap(before))
        using (var b = new Bitmap(after)) {
            int darkBoth = 0, lightBoth = 0, total = 0;
            // Where the frozen pixels are: a 32x32 bucket grid, so a count comes with a location and the
            // offender can be found without knowing its coordinates in advance.
            var buckets = new SortedDictionary<int, int>();
            Bitmap outMask = null;
            Graphics g = null;
            if (mask != null && mask.Length > 0) {
                outMask = new Bitmap(right - left, bottom - top, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                g = Graphics.FromImage(outMask);
                g.Clear(Color.Black);
            }
            for (int y = top; y < bottom; y++) {
                for (int x = left; x < right; x++) {
                    Color ca = a.GetPixel(x, y), cb = b.GetPixel(x, y);
                    int maxA = Math.Max(ca.R, Math.Max(ca.G, ca.B));
                    int maxB = Math.Max(cb.R, Math.Max(cb.G, cb.B));
                    int minA = Math.Min(ca.R, Math.Min(ca.G, ca.B));
                    int minB = Math.Min(cb.R, Math.Min(cb.G, cb.B));
                    total++;
                    bool frozenDark = maxA < dark && maxB < dark;
                    if (frozenDark) {
                        darkBoth++;
                        int key = ((x - left) / 60) * 100000 + ((y - top) / 60);
                        int had; buckets.TryGetValue(key, out had); buckets[key] = had + 1;
                        if (g != null) outMask.SetPixel(x - left, y - top, Color.White);
                    } else if (minA > light && minB > light) {
                        lightBoth++;
                    }
                }
            }
            Console.WriteLine("area {0}x{1} dark-in-both {2} light-in-both {3}", right - left, bottom - top, darkBoth, lightBoth);
            var ranked = new List<KeyValuePair<int, int>>(buckets);
            ranked.Sort((a2, b2) => b2.Value.CompareTo(a2.Value));
            int shown = Math.Min(8, ranked.Count);
            for (int i = 0; i < shown; i++) {
                int k = ranked[i].Key;
                int bx = k / 100000, by = k % 100000;
                Console.WriteLine("  at x={0}..{1} y={2}..{3} px={4}",
                    bx * 60, bx * 60 + 59, by * 60, by * 60 + 59, ranked[i].Value);
            }
            if (outMask != null) { g.Dispose(); outMask.Save(mask, System.Drawing.Imaging.ImageFormat.Png); Console.WriteLine("mask " + mask); }
            // A reading is only actionable if the worst bucket can be looked at. Grab a 240x240 patch of both
            // frames around it: a big number that turns out to be some other window over ours says so here.
            if (crop != null && crop.Length > 0 && ranked.Count > 0) {
                int k0 = ranked[0].Key;
                int cx = left + (k0 / 100000) * 60 + 30, cy = top + (k0 % 100000) * 60 + 30;
                int x0 = Math.Max(left, cx - 120), y0 = Math.Max(top, cy - 120);
                int x1 = Math.Min(right, cx + 120), y1 = Math.Min(bottom, cy + 120);
                var rect = new Rectangle(x0, y0, x1 - x0, y1 - y0);
                using (var pa = a.Clone(rect, a.PixelFormat)) pa.Save(crop + ".before.png", System.Drawing.Imaging.ImageFormat.Png);
                using (var pb = b.Clone(rect, b.PixelFormat)) pb.Save(crop + ".after.png", System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine("crop " + crop + " at " + rect.X + "," + rect.Y + "," + rect.Width + "x" + rect.Height);
            }
        }
    }
}
'@
}

[NavRecolorFrozen]::Run($Before, $After, $Left, $Top, $Right, $Bottom, $Dark, $Light, $Mask, $Crop)
