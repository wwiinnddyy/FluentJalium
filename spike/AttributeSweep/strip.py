"""Strip the attributes the census proved have no receiver (spike/AttributeSweep/attrsweep-1.txt).

Only three shapes are touched, and only on the element types that lack the member:
  <ContentPresenter ...>  - Foreground (inheritance carries it: adaptation/00 S1-f) and TextWrapping
                            (this runtime's generated text defaults to Wrap: Styles/Navigation.jalxaml measures it)
  <Grid .../>          - BorderBrush / BorderThickness / CornerRadius / Padding
  <StackPanel .../>        - Padding
Every removal is reported with its line so the diff can be read against the census.
"""
import re
import sys
from pathlib import Path

STYLES = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("src/FluentJalium/Styles")

DEAD = {
    "ContentPresenter": ["Foreground=\"{TemplateBinding Foreground}\"", "TextWrapping=\"Wrap\""],
    "Grid": ["BorderBrush=\"{TemplateBinding BorderBrush}\"", "BorderThickness=\"{TemplateBinding BorderThickness}\"",
             "CornerRadius=\"{TemplateBinding CornerRadius}\"", "CornerRadius=\"{ThemeResource OverlayCornerRadius}\""],
    "StackPanel": ["Padding=\"{ThemeResource InfoBarPanelVerticalOrientationPadding}\""],
}

total = 0
for path in sorted(STYLES.glob("*.jalxaml")):
    text = path.read_text(encoding="utf-8")
    removals = []

    def clean(match):
        global total
        tag = match.group(0)
        name = match.group("type")
        for dead in DEAD[name]:
            for candidate in (f" {dead}", f"{dead} "):
                if candidate in tag:
                    tag = tag.replace(candidate, "", 1)
                    removals.append((name, dead))
                    total += 1
                    break
        return tag

    pattern = re.compile(rf"<(?P<type>{'|'.join(DEAD)})\b[^>]*?/?>")
    new = pattern.sub(clean, text)
    if removals:
        path.write_text(new, encoding="utf-8", newline="")
        print(f"{path.name}: removed {len(removals)}")
        for name, dead in removals:
            print(f"   <{name}> {dead}")

print(f"total removals: {total}")
