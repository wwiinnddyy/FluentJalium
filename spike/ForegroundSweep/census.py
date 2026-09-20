"""Classify every `Setter TargetName="X" Property="Foreground"` cell in the shipped styles by what part X actually is.

The TreeView probe measured that `ContentPresenter` has no `Foreground` member on this runtime, which makes such a
cell silently inert (the dictionary loads, no error, the colour never changes). A cell whose target is a TextBlock or
a control does have the property, so the sweep has to be per target type, not per attribute pattern.
"""
import re
import sys
from collections import defaultdict
from pathlib import Path

STYLES = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("src/FluentJalium/Styles")

PART = re.compile(r'<(?P<type>[A-Za-z][\w.]*)\s+[^>]*?(?<![A-Za-z])Name="(?P<name>[\w-]+)"')
CELL = re.compile(r'TargetName="(?P<name>[\w-]+)"\s+Property="Foreground"')

for path in sorted(STYLES.glob("*.jalxaml")):
    text = path.read_text(encoding="utf-8")
    parts = defaultdict(set)
    for m in PART.finditer(text):
        parts[m.group("name")].add(m.group("type"))
    cells = list(CELL.finditer(text))
    if not cells:
        continue
    inert, ok, unknown = [], [], []
    for m in cells:
        line = text[: m.start()].count("\n") + 1
        names = parts.get(m.group("name"), set())
        entry = (line, m.group("name"), "|".join(sorted(names)) or "?")
        if names and names <= {"ContentPresenter"}:
            inert.append(entry)
        elif not names:
            unknown.append(entry)
        else:
            ok.append(entry)
    print(f"{path.name}: cells={len(cells)} inert-on-ContentPresenter={len(inert)} "
          f"target-has-foreground={len(ok)} name-not-in-this-file={len(unknown)}")
    for label, rows in (("  INERT", inert), ("  ok", ok), ("  unknown", unknown)):
        for line, name, kind in rows:
            print(f"{label} L{line} {name} -> {kind}")
