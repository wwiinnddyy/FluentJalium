"""Which installed icon font has exactly the holes SymbolIcon shows?

    python spike/GlyphInkProbe/match-cmap.py

The pixel grid (shoot-count.ps1 -Variant symbol) leaves 120 of the 764 enum values with no ink at all, while the same
codepoints handed to FontIcon paint under either of the two families the runtime names. So the painting face is not
one of those two at those numbers - and the only scale-free way to find out what it is, is to compare the blank set
against the cmap of every candidate font on the machine. A font is a candidate only if every blank cell is absent
from it and every painted cell is present: one contradiction in either direction rules it out, which is why this
prints the residuals instead of a best match.

Reads the font files DirectWrite actually resolves by name, so the sweep describes the installed faces rather than a
copy of them somewhere else.
"""

import csv
import glob
import os
import sys

from fontTools.ttLib import TTFont

HERE = os.path.dirname(os.path.abspath(__file__))
FONTS_DIR = r"C:\Windows\Fonts"
KEYWORDS = ("segoe", "symbol", "icon", "mdl", "fluent", "open-dyslexic", "codicon")


def load_symbols():
    path = os.path.join(HERE, "glyph-ink-symbol.csv")
    if not os.path.exists(path):
        sys.exit(f"missing {path} - run shoot-count.ps1 -Variant symbol first")
    rows = []
    with open(path, newline="", encoding="utf-8") as handle:
        for row in csv.DictReader(handle):
            if row["kind"] != "symbol":
                continue
            rows.append((row["name"], int(row["codepoint"][2:], 16), int(row["light"])))
    return rows


def cmap_of(path):
    faces = []
    for font_number in range(4):
        try:
            font = TTFont(path, fontNumber=font_number, lazy=True)
        except Exception:
            break
        try:
            # segoeui.ttf is a collection whose later faces expose no cmap through the best-table picker; a face
            # without one contributes nothing rather than killing the sweep.
            table = font.getBestCmap()
            if table:
                faces.append(set(table.keys()))
        finally:
            font.close()
    if not faces:
        return set()
    return faces[0].union(*faces[1:])


def main():
    symbols = load_symbols()
    blank = {(name, code) for name, code, light in symbols if light == 0}
    painted = {(name, code) for name, code, light in symbols if light > 0}
    print(f"symbol cells={len(symbols)} blank={len(blank)} painted={len(painted)}")

    candidates = sorted(
        path for path in glob.glob(os.path.join(FONTS_DIR, "*.*"))
        if path.lower().endswith((".ttf", ".otf", ".ttc"))
        and any(word in os.path.basename(path).lower() for word in KEYWORDS)
    )
    print(f"candidate font files: {len(candidates)}")
    for path in candidates:
        table = cmap_of(path)
        if not table:
            print(f"  {os.path.basename(path)}: <no readable cmap>")
            continue
        explained = {key for key in blank if key[1] not in table}
        unexplained = {key for key in blank if key[1] in table}
        contradiction = {key for key in painted if key[1] not in table}
        print(f"  {os.path.basename(path)}: covers {len(table):5d} codepoints | "
              f"blanks explained by absence {len(explained):3d}/{len(blank)} | "
              f"blanks it HAS (unexplained) {len(unexplained):3d} | "
              f"painted cells it LACKS (contradiction) {len(contradiction):3d}")
        if unexplained and len(unexplained) <= 12:
            print("      unexplained: " + " ".join(f"{n}/{c:#06X}" for n, c in sorted(unexplained)))
        if contradiction and len(contradiction) <= 12:
            print("      contradiction: " + " ".join(f"{n}/{c:#06X}" for n, c in sorted(contradiction)))


if __name__ == "__main__":
    main()
