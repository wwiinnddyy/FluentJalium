"""Drop the whitespace-only lines the ring-stripping script left behind.

Only lines matching ^[ \\t]+$ are touched; git HEAD carries none of them in these files,
so every hit is a leftover from move-rings-to-focusvisual.py.
"""

import pathlib

FILES = [
    "src/FluentJalium/Styles/Inputs.jalxaml",
    "src/FluentJalium/Styles/Navigation.jalxaml",
    "src/FluentJalium/Styles/Selection.jalxaml",
    "src/FluentJalium/Styles/Surfaces.jalxaml",
    "src/FluentJalium/Styles/Common.jalxaml",
]

root = pathlib.Path(__file__).resolve().parents[2]
for rel in FILES:
    path = root / rel
    lines = path.read_text(encoding="utf-8").split("\n")
    kept = [l for l in lines if not (l.strip() == "" and l != "")]
    removed = len(lines) - len(kept)
    if removed:
        path.write_text("\n".join(kept), encoding="utf-8", newline="")
    print(f"{rel}: removed {removed}")
