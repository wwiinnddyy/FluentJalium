"""Regenerate s2-symbol-cmap-raw.txt from the two codepoint lists the in-proc probe wrote.

    python spike/SymbolCmap/cmap-surface.py

The previous version of this file swept the enum parsed out of the sibling framework source, which AGENTS.md
warns may be newer than the pinned 26.10.9 runtime. Both lists it now reads come from the shipped assembly
(shipped-symbol.tsv) and from upstream's own declaration at the pinned commit (upstream-symbol.tsv), written by
spike/SymbolCmap/Program.cs, so the counts here no longer depend on a checked-out source tree.

Three questions per codepoint list, per installed icon font: does the font map it, does the whole list land inside
the Private Use Areas, and - for upstream's legacy numbers specifically - whether the number the enum holds is even
drawable here. That last one is what makes upstream's conversion table (icon.cpp:461) load-bearing rather than
incidental.
"""

import os
from fontTools.ttLib import TTFont

HERE = os.path.dirname(os.path.abspath(__file__))
OUT_DIR = os.path.join(HERE, "out")
TARGET = os.path.normpath(os.path.join(HERE, "..", "..", "docs", "astra", "adaptation", "s2-symbol-cmap-raw.txt"))

FONTS = {
    "Segoe Fluent Icons": r"C:\Windows\Fonts\SegoeIcons.ttf",
    "Segoe MDL2 Assets": r"C:\Windows\Fonts\segmdl2.ttf",
}

PUA = [(0xE000, 0xF8FF), (0xF0000, 0xFFFFD), (0x100000, 0x10FFFD)]


def read(path):
    rows = []
    with open(path, encoding="utf-8") as handle:
        for line in handle:
            parts = line.rstrip("\n").split("\t")
            if len(parts) == 3:
                rows.append((parts[0], int(parts[2])))
    return rows


def in_pua(value):
    return any(low <= value <= high for low, high in PUA)


def cover(lines, label, rows, fonts):
    values = [value for _, value in rows]
    shared = sorted({value for value in values if values.count(value) > 1})
    lines.append(f"--- {label}: {len(rows)} members, {len(set(values))} distinct codepoints, "
                 f"{len(shared)} carried by two or more names")
    outside = [(name, value) for name, value in rows if not in_pua(value)]
    lines.append(f"    outside the PUA: {len(outside)}"
                 + ("" if not outside else " -> " + ", ".join(f"{n}=U+{v:04X}" for n, v in outside[:8])))
    for font_label, path in fonts.items():
        if not os.path.exists(path):
            lines.append(f"    {font_label}: {path} MISSING")
            continue
        cmap = TTFont(path, lazy=True).getBestCmap()
        misses = [(name, value) for name, value in rows if value not in cmap]
        lines.append(f"    {font_label}: hits {len(rows) - len(misses)} / {len(rows)}, misses {len(misses)}")
        for name, value in misses[:40]:
            lines.append(f"      U+{value:04X}  MISSING  {name}")
        if len(misses) > 40:
            lines.append(f"      ... and {len(misses) - 40} more")


def main():
    shipped = read(os.path.join(OUT_DIR, "shipped-symbol.tsv"))
    upstream = read(os.path.join(OUT_DIR, "upstream-symbol.tsv"))
    painted = read(os.path.join(OUT_DIR, "upstream-painted.tsv"))
    painted_by_name = dict(painted)

    lines = [
        "input: spike/SymbolCmap/out/{shipped,upstream,upstream-painted}.tsv, written by spike/SymbolCmap",
        "       (in-proc reflection over the 26.10.9 assembly the tests resolve + the pinned upstream declaration)",
        "replaces: the sweep this file used to run, which parsed Symbol.cs out of the sibling source tree",
        "",
    ]
    cover(lines, "shipped Jalium.UI.Controls.Symbol", shipped, FONTS)
    cover(lines, "upstream Microsoft.UI.Xaml.Controls.Symbol (the number the IDL declares)", upstream, FONTS)
    cover(lines, "upstream Symbol after ConvertSymbolValueToGlyph (the number it paints)",
          [(name, painted_by_name[name]) for name, _ in upstream], FONTS)

    lines.append("")
    lines.append("--- what the three lists together say")
    here = {value for _, value in shipped}
    legacy_missing = sum(1 for name, value in upstream if value not in here)
    painted_missing = sum(1 for name, value in painted if value not in here)
    lines.append(f"    upstream's declared numbers absent from the shipped enum: {legacy_missing} of {len(upstream)}")
    lines.append(f"    upstream's painted numbers absent from the shipped enum: {painted_missing} of {len(painted)}")
    lines.append("    the second line is the one that matters: the shipped enum holds post-conversion numbers, so")
    lines.append("    parity is measured against what upstream paints, not against what its IDL declares.")

    with open(TARGET, "w", encoding="utf-8", newline="\n") as handle:
        handle.write("\n".join(lines) + "\n")
    print("\n".join(lines))
    print(f"... wrote {TARGET}")


if __name__ == "__main__":
    main()
