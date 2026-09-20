"""Report cmap hits and ink boxes for Segoe symbol codepoints.

    python check.py EA3B EDDB EDDC

For each font that has the codepoint: the glyph name, the advance width and the glyf
bounding box as a fraction of em. The fraction is what turns a FontSize into the dot
diameter a pip has to match.
"""

import sys
from fontTools.ttLib import TTFont

FONTS = {
    "Segoe Fluent Icons": r"C:\Windows\Fonts\SegoeIcons.ttf",
    "Segoe MDL2 Assets": r"C:\Windows\Fonts\segmdl2.ttf",
}


def report(path, codepoints):
    font = TTFont(path, lazy=True)
    cmap = font.getBestCmap()
    upem = font["head"].unitsPerEm
    hmtx = font["hmtx"]
    glyf = font["glyf"] if "glyf" in font else None
    print(f"--- {path}  (upem={upem}, {len(cmap)} mapped) ---")
    for cp in codepoints:
        if cp not in cmap:
            print(f"  U+{cp:04X}  MISSING")
            continue
        name = cmap[cp]
        advance, _ = hmtx[name]
        line = f"  U+{cp:04X}  {name:<34} advance={advance/upem:.3f}em"
        if glyf is not None:
            glyph = glyf[name]
            if glyph.numberOfContours == 0:
                line += "  bbox=(empty)"
            else:
                w = (glyph.xMax - glyph.xMin) / upem
                h = (glyph.yMax - glyph.yMin) / upem
                line += (
                    f"  bbox={w:.3f}x{h:.3f}em"
                    f"  x={glyph.xMin/upem:+.3f}..{glyph.xMax/upem:+.3f}"
                    f" y={glyph.yMin/upem:+.3f}..{glyph.yMax/upem:+.3f}"
                    f" contours={glyph.numberOfContours}"
                )
        print(line)
    font.close()


def main():
    codepoints = [int(arg, 16) for arg in sys.argv[1:]]
    if not codepoints:
        raise SystemExit(__doc__)
    for path in FONTS.values():
        report(path, codepoints)


if __name__ == "__main__":
    main()
