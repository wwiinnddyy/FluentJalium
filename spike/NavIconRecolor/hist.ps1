<#
.SYNOPSIS
  Histogram the colours inside a rectangle of a PNG, so a crop answers "how much of this ink is near-black vs
  near-white" instead of "looks fine".

.DESCRIPTION
  Used by shoot.ps1's grabs. The pane's icon column is a strip that contains nothing but the pane background and the
  glyphs, so a theme flip must move the count of dark glyph pixels and light glyph pixels in opposite directions. If
  one of those counts does not move, the ink in that strip is stale.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $Path,
    [Parameter(Mandatory = $true)][int] $Left,
    [Parameter(Mandatory = $true)][int] $Top,
    [Parameter(Mandatory = $true)][int] $Right,
    [Parameter(Mandatory = $true)][int] $Bottom,
    [int] $Top_N = 10
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
if (-not ('NavRecolorHist' -as [type])) {
    Add-Type -ReferencedAssemblies 'System.Drawing.dll' -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

public static class NavRecolorHist {
    public static void Run(string path, int left, int top, int right, int bottom, int n) {
        using (var bmp = new Bitmap(path)) {
            var counts = new Dictionary<uint, int>();
            for (int y = top; y < bottom; y++) {
                for (int x = left; x < right; x++) {
                    var c = bmp.GetPixel(x, y);
                    uint key = ((uint)c.A << 24) | ((uint)c.R << 16) | ((uint)c.G << 8) | c.B;
                    int old; counts.TryGetValue(key, out old); counts[key] = old + 1;
                }
            }
            var list = new List<KeyValuePair<uint, int>>(counts);
            list.Sort((a, b) => b.Value.CompareTo(a.Value));
            int shown = Math.Min(n, list.Count);
            for (int i = 0; i < shown; i++) {
                uint k = list[i].Key;
                Console.WriteLine("#{0:X2}{1:X2}{2:X2}{3:X2} {4}",
                    (k >> 24) & 0xff, (k >> 16) & 0xff, (k >> 8) & 0xff, k & 0xff, list[i].Value);
            }
            Console.WriteLine("area {0}x{1} distinct {2}", right - left, bottom - top, counts.Count);
        }
    }
}
'@
}

[NavRecolorHist]::Run($Path, $Left, $Top, $Right, $Bottom, $Top_N)
