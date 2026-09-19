<#
.SYNOPSIS
  Count exact colours in a PNG so "the fill is missing" becomes a number instead of an opinion.

.DESCRIPTION
  Companion to capture-pages.ps1. A crop is only evidence once something is asserted about it: the spacing
  batch's claim that a checked app-bar cell lost its accent rested on a hand-read count, and a hand read
  cannot tell a missing fill from a blank grab. This prints the count per colour plus the window's own DPI,
  so the expected area in DIPs can be converted and compared.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $Path,
    [Parameter(Mandatory = $true)][string[]] $Color
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
if (-not ('ColourCount' -as [type])) {
    Add-Type -ReferencedAssemblies 'System.Drawing.dll' -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class ColourCount {
    public static string Info = "";

    public static Dictionary<string, int> Of(string path, string[] hexes) {
        var counts = new Dictionary<string, int>();
        var wanted = new Dictionary<string, uint>();
        foreach (var hex in hexes) {
            counts[hex] = 0;
            wanted[hex] = (uint)(Convert.ToInt32(hex.Substring(1, 2), 16) << 16 |
                                 Convert.ToInt32(hex.Substring(3, 2), 16) << 8 |
                                 Convert.ToInt32(hex.Substring(5, 2), 16));
        }
        using (var bitmap = new Bitmap(path)) {
            var width = bitmap.Width;
            var height = bitmap.Height;
            Info = width + "x" + height;
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

# Accept the list any way the caller can write it: an array, or one comma- or space-separated string,
# because a native string[] parameter binds oddly once it has crossed a shell.
$hexes = @($Color -join ',' -split '[,; ]+' | Where-Object { $_ } | ForEach-Object {
    if ($_.StartsWith('#')) { $_ } else { "#$_" }
} | Where-Object { $_ -match '^#[0-9A-Fa-f]{6}$' })
if ($hexes.Count -eq 0) { Write-Error 'no colour parsed' }
$counts = [ColourCount]::Of($Path, $hexes)
Write-Host "$Path [$([ColourCount]::Info)]"
foreach ($hex in $hexes) { Write-Host ("  {0,-9} {1}" -f $hex, $counts[$hex]) }
