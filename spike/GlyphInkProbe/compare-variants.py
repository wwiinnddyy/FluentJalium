"""Compare the three glyph grids cell by cell and say which font SymbolIcon paints with.

    python spike/GlyphInkProbe/compare-variants.py

spike/GlyphInkProbe/shoot-count.ps1 wrote one CSV per variant over the same 766 cells at the same rects, so row i of
one file is the same square of the screen as row i of the next. Each row carries an 8x8 signature of where the ink
landed, which is a shape, not a size: two cells matching there is evidence about the glyph, and a match on ink counts
alone would only say both squares are busy.

The question is which of the two named families the runtime's own SymbolIcon agrees with. Three readings answer it:
how many of its shapes are identical to each variant, how many of the cells it leaves blank the named fonts fill, and
whether its non-blank shapes disagree with both (which would mean a third font, not a choice between these two).
"""

import csv
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
VARIANTS = ("symbol", "fluent", "mdl2", "markup")
# markup is the fluent row redelivered through the markup reader: same element, same family, same size, only the
# hand-off differs. If the two agree cell for cell, a template can carry the family; if they do not, the read-back
# that says the property survived parsing was never evidence about ink.
PAIRS = (("symbol", "fluent"), ("symbol", "mdl2"), ("fluent", "mdl2"), ("markup", "fluent"), ("markup", "mdl2"))


def load(variant):
    path = os.path.join(HERE, f"glyph-ink-{variant}.csv")
    if not os.path.exists(path):
        sys.exit(f"missing {path} - run shoot-count.ps1 -Variant {variant} first")
    rows = {}
    with open(path, newline="", encoding="utf-8") as handle:
        for row in csv.DictReader(handle):
            rows[int(row["index"])] = row
    return rows


def bits(hex_sig):
    value = int(hex_sig, 16)
    return [ (value >> i) & 1 for i in range(64) ]


def hamming(a, b):
    return sum(1 for x, y in zip(bits(a), bits(b)) if x != y)


def agree(grids, a, b, pool):
    total = len(pool)
    if not total:
        return f"  {a} vs {b}: no cell in the pool"
    same = sum(1 for i in pool if grids[a][i]["sig"] == grids[b][i]["sig"])
    near = sum(1 for i in pool if hamming(grids[a][i]["sig"], grids[b][i]["sig"]) <= 2)
    return (f"  {a} vs {b}: {total} cells -> identical {same} ({100.0 * same / total:.1f}%), "
            f"within 2 bits {near} ({100.0 * near / total:.1f}%)")


def main():
    grids = {name: load(name) for name in VARIANTS}
    symbol = grids["symbol"]
    cells = sorted(symbol)
    symbols = [i for i in cells if symbol[i]["kind"] == "symbol"]

    print(f"cells={len(cells)} symbol cells={len(symbols)}")
    for name in VARIANTS:
        inked = sum(1 for i in symbols if int(grids[name][i]["ink"]) > 0)
        blank = [i for i in symbols if int(grids[name][i]["light"]) == 0]
        print(f"  {name}: painted {inked} / {len(symbols)}, fully blank {len(blank)}")

    for other in ("fluent", "mdl2"):
        both = [i for i in symbols if int(symbol[i]["light"]) > 0 and int(grids[other][i]["light"]) > 0]
        print(agree(grids, "symbol", other, both) + " | painted under both")

    # Two readings of the same comparison, because an 8x8 signature moves when the same glyph is drawn bigger: the
    # first pool is every cell both renderers painted, the second keeps only those whose ink counts already agree
    # within 10% and so leaves the shape to disagree on its own. fluent-vs-mdl2 is the control - two named,
    # different fonts through one element - and whatever rate it lands on is what "not the same font" looks like
    # to this instrument, which is the number a symbol-vs-X rate has to be read against.
    for a, b in PAIRS:
        matched = [i for i in symbols
                   if int(grids[a][i]["light"]) > 0 and int(grids[b][i]["light"]) > 0
                   and abs(int(grids[a][i]["light"]) - int(grids[b][i]["light"])) <= 0.1 * int(grids[b][i]["light"])]
        print(agree(grids, a, b, matched) + " | size-matched")

    symbol_blank = [i for i in symbols if int(symbol[i]["light"]) == 0]
    for other in ("fluent", "mdl2"):
        filled = sum(1 for i in symbol_blank if int(grids[other][i]["light"]) > 0)
        print(f"  of the {len(symbol_blank)} cells SymbolIcon leaves blank, {other} paints {filled}")

    print(f"  first 12 of the cells SymbolIcon leaves blank, with what each named font put there (ink/light px):")
    for i in symbol_blank[:12]:
        print(f"    {symbol[i]['name']}/{symbol[i]['codepoint']} "
              + " ".join(f"{other}={grids[other][i]['ink']}/{grids[other][i]['light']}" for other in ("fluent", "mdl2")))

    # And the reverse direction: a cell that some font leaves blank while symbol paints, because that pair of readings
    # cannot both be true of one font and so names a mapping difference rather than a coverage difference.
    for other in ("fluent", "mdl2"):
        only_symbol = [i for i in symbols if int(symbol[i]["light"]) > 0 and int(grids[other][i]["light"]) == 0]
        print(f"  painted by symbol but blank under {other}: {len(only_symbol)} "
              + " ".join(f"{symbol[i]['name']}/{symbol[i]['codepoint']}" for i in only_symbol[:12]))


if __name__ == "__main__":
    main()
