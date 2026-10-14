"""Re-route the dead state cells the sweep found (spike/ForegroundSweep/census.py, gate-before.txt).

Two shapes, both silent:
  - `TargetName="<a ContentPresenter>" Property="Foreground"` - this runtime's ContentPresenter has no Foreground
    member, so the cell has nothing to write. The cell keeps its trigger and its position and loses only its
    TargetName, which points it at the control or container the template belongs to; Foreground is inherited, so the
    text element the presenter generates takes it. That is the route ComboBox's own rows already use in this repo
    (Styles/Selection.jalxaml writes ComboBoxForeground* targetlessly inside the same template trigger list) and the
    one the TreeView slice measured first.
  - `TargetName="CheckRoot" / "RadioRoot" Property="BorderBrush"` - the root those cells name is a Grid, and this
    runtime's Grid declares no BorderBrush either. Same treatment: the cell lands on the control, whose BorderBrush
    the template already binds through to the root. Nothing paints in Light or Dark either way (upstream resolves
    every one of those rows to SubtleFillColorTransparentBrush), so the reroute keeps the row consumed by a property
    that exists without inventing a surface upstream does not have.

MenuFlyoutItem's six IconContent cells are deleted outright instead of re-pointed: that style already writes
MenuFlyoutItemForegroundPointerOver / ...Disabled on the item itself, so the icon is coloured by inheritance today
and the cell was a duplicate of a live one.
"""
import re
from pathlib import Path

STYLES = Path("src/FluentJalium/Styles")

# part name -> property whose cell loses its TargetName and lands on the templated parent
REROUTE = {
    ("CheckLabel", "Foreground"),
    ("RadioLabel", "Foreground"),
    ("ContentPresenter", "Foreground"),
    ("PART_SelectionPresenter", "Foreground"),
    ("CheckRoot", "BorderBrush"),
    ("RadioRoot", "BorderBrush"),
}
# part name -> property whose cell is a duplicate of a live one and goes away
DELETE = {("IconContent", "Foreground")}

CELL = re.compile(r'<Setter TargetName="(?P<part>[A-Za-z_]+)" Property="(?P<property>Foreground|BorderBrush)" Value="(?P<row>[^"]+)"\s*/>')

report = []
for path in sorted(STYLES.glob("*.jalxaml")):
    text = path.read_text(encoding="utf-8")
    original = text
    counters = {"rerouted": 0, "deleted": 0}

    def rewrite(match):
        key = (match.group("part"), match.group("property"))
        if key in REROUTE:
            counters["rerouted"] += 1
            return f'<Setter Property="{match.group("property")}" Value="{match.group("row")}" />'
        if key in DELETE:
            counters["deleted"] += 1
            return ""
        return match.group(0)

    text = CELL.sub(rewrite, text)
    if text != original:
        path.write_text(text, encoding="utf-8", newline="\n")
        report.append(f"{path.name}: rerouted={counters['rerouted']} deleted={counters['deleted']}")

print("\n".join(report) or "nothing changed")
