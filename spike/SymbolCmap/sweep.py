"""Verify every Jalium `Symbol` member's codepoint against the installed Segoe icon fonts.

    python sweep.py

The claim this measures is the one `docs/astra/README.md` and the ROADMAP's no-claims list refuse to
make: "all 764 Symbol members are usable". The enum's member *values* are the codepoints
(`Symbol.cs`: `GlobalNavButton = 0xE700`), so the question is answerable per member without rendering
anything: does the font's cmap contain the codepoint, and what glyph does it name.

Reads the enum from the sibling framework source, which may be newer than the pinned 26.10.9 runtime -
so the member count is cross-checked against the shipped assembly separately (spike/ControlCensus), and
this file prints the source path it read. Reports, per font: hits, misses, duplicate codepoints (two
names on one glyph), and members outside the Private Use Areas.
"""

import re
import os
from fontTools.ttLib import TTFont

SYMBOL_SOURCE = r"C:\git\Jalium\Jalium.UI\src\managed\Jalium.UI.Controls\Symbol.cs"
FONTS = {
    "Segoe Fluent Icons": r"C:\Windows\Fonts\SegoeIcons.ttf",
    "Segoe MDL2 Assets": r"C:\Windows\Fonts\segmdl2.ttf",
}
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "out", "symbol-cmap.txt")

MEMBER = re.compile(r"^\s*([A-Za-z][A-Za-z0-9_]*)\s*=\s*(0x[0-9A-Fa-f]+)\s*,\s*$")


def members(path):
    out = []
    with open(path, encoding="utf-8-sig") as handle:
        for line in handle:
            match = MEMBER.match(line.rstrip("\n"))
            if match:
                out.append((match.group(1), int(match.group(2), 16)))
    return out


def main():
    rows = members(SYMBOL_SOURCE)
    lines = [f"source: {SYMBOL_SOURCE}", f"members parsed: {len(rows)}"]

    values = [value for _, value in rows]
    duplicates = sorted({value for value in values if values.count(value) > 1})
    lines.append(f"distinct codepoints: {len(set(values))}, duplicated codepoints: {len(duplicates)}")
    for value in duplicates:
        names = [name for name, member in rows if member == value]
        lines.append(f"  shared U+{value:04X}: {', '.join(names)}")

    outside = [(name, value) for name, value in rows if not (0xE000 <= value <= 0xF8FF)]
    lines.append(f"outside the PUA: {len(outside)}")
    for name, value in outside:
        lines.append(f"  {name} = U+{value:04X}")

    for label, path in FONTS.items():
        if not os.path.exists(path):
            lines.append(f"--- {label}: {path} MISSING")
            continue
        font = TTFont(path, lazy=True)
        cmap = font.getBestCmap()
        lines.append(f"--- {label}  (upem={font['head'].unitsPerEm}, {len(cmap)} mapped)")
        misses = [(name, value) for name, value in rows if value not in cmap]
        lines.append(f"    hits {len(rows) - len(misses)} / {len(rows)}, misses {len(misses)}")
        for name, value in misses:
            lines.append(f"  U+{value:04X}  MISSING  {name}")

    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, "w", encoding="utf-8") as handle:
        handle.write("\n".join(lines) + "\n")
    print("\n".join(lines[:14]))
    print(f"... wrote {OUT}")


if __name__ == "__main__":
    main()
