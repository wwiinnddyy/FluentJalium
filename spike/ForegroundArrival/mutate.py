"""Toggle one or two style rows so a fact's teeth can be measured by A/B.

Usage: mutate.py <apply|revert> <name>

`apply` saves every file a mutant touches byte-for-byte next to this script and rewrites one exact string in
each; `revert` copies the saved bytes back and refuses to call it a day unless git reports those files clean
again. Reverting by searching for the mutated text was the earlier design and it broke on the mutants whose
mutation is a deletion: the string to look for is then empty, so it matches everywhere, the restore bails out,
and the mutation stands in the tree for every later mutant to measure against.

A mutant may be two edits at once. The composite below is one: taking a cell away can only show that something
else supplies the same brush, so the second edit moves the name that other writer is suspected of reading.
"""

import shutil
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
BAK = Path(__file__).resolve().parent / "bak"

TIP = ROOT / "src/FluentJalium/Styles/TeachingTip.jalxaml"
SELECTION = ROOT / "src/FluentJalium/Styles/Selection.jalxaml"
COMMON = ROOT / "src/FluentJalium/Styles/Common.jalxaml"
RETINTS = ROOT / "src/FluentJalium/ThemeResources/FrameworkRetints.jalxaml"

TITLE_OLD = '                      <TextBlock Name="TitleTextBlock" Text="{TemplateBinding Title}" Foreground="{ThemeResource TeachingTipTitleForegroundBrush}" FontWeight="SemiBold" TextWrapping="Wrap" />\n'
TITLE_NEW = '                      <TextBlock Name="TitleTextBlock" Text="{TemplateBinding Title}" FontWeight="SemiBold" TextWrapping="Wrap" />\n'
SUBTITLE_OLD = '                      <TextBlock Name="SubtitleTextBlock" Text="{TemplateBinding Subtitle}" Foreground="{ThemeResource TeachingTipSubtitleForegroundBrush}" TextWrapping="Wrap" />\n'
SUBTITLE_NEW = '                      <TextBlock Name="SubtitleTextBlock" Text="{TemplateBinding Subtitle}" TextWrapping="Wrap" />\n'
# The scroller keeps its own line; only the attribute comes and goes.
COMBO_OLD = '<ScrollViewer Name="PART_ScrollViewer" Foreground="{ThemeResource ComboBoxDropDownForeground}"'
COMBO_NEW = '<ScrollViewer Name="PART_ScrollViewer"'

SUBTLE_CELL = '<Setter Property="Foreground" Value="{ThemeResource SubtleButtonForegroundDisabled}" />'
REPEAT_CELL = '<Setter Property="Foreground" Value="{ThemeResource RepeatButtonForegroundDisabled}" />'
CHECKBOX_CELL = '<Setter Property="Foreground" Value="{ThemeResource CheckBoxForegroundUncheckedDisabled}" />'
CHECKED_CELL = '<Setter Property="Foreground" Value="{ThemeResource CheckBoxForegroundChecked}" />'
# Pointing the cell at a brush neither our resting row nor the framework's disabled ink could supply. Nothing
# else in the tree writes white onto a disabled non-accent button, so a red fact means this cell is the writer
# and a green one means something else is.
POINTED = '<Setter Property="Foreground" Value="{ThemeResource AccentButtonForeground}" />'

TEXT_DISABLED = '<StaticResource x:Key="TextDisabled" ResourceKey="TextFillColorDisabledBrush" />'
TEXT_DISABLED_MOVED = '<StaticResource x:Key="TextDisabled" ResourceKey="TextFillColorSecondaryBrush" />'

TABVIEW = ROOT / "src/FluentJalium/Styles/TabView.jalxaml"
TAB_HEADER_SELECTED_CELL = '<Setter Property="Foreground" Value="{ThemeResource TabViewItemHeaderForegroundSelected}" />'
TAB_ICON_SELECTED_CELL = '<Setter TargetName="IconHost" Property="Foreground" Value="{ThemeResource TabViewItemIconForegroundSelected}" />'
TAB_POINTED = '<Setter Property="Foreground" Value="{ThemeResource AccentButtonForeground}" />'
TAB_ICON_POINTED = '<Setter TargetName="IconHost" Property="Foreground" Value="{ThemeResource AccentButtonForeground}" />'

TARGETS = {
    # A row taken away. Where the row's value equals what some other writer already supplies, this is silent -
    # measured for the three #87 rows and the two disabled cells below, which is why each has a `*point` twin.
    "title": [(TIP, TITLE_OLD, TITLE_NEW)],
    "subtitle": [(TIP, SUBTITLE_OLD, SUBTITLE_NEW)],
    "combo": [(SELECTION, COMBO_OLD, COMBO_NEW)],
    "subtlecell": [(COMMON, SUBTLE_CELL, "")],
    "repeatcell": [(COMMON, REPEAT_CELL, "")],
    "checkboxcell": [(SELECTION, CHECKBOX_CELL, "")],
    # The same rows re-pointed instead of removed.
    "titleswap": [(TIP, TITLE_OLD, TITLE_OLD.replace("TeachingTipTitleForegroundBrush", "TextFillColorDisabledBrush"))],
    "subtitleswap": [(TIP, SUBTITLE_OLD, SUBTITLE_OLD.replace("TeachingTipSubtitleForegroundBrush", "TextFillColorDisabledBrush"))],
    "comboswap": [(SELECTION, COMBO_OLD, COMBO_OLD.replace("ComboBoxDropDownForeground", "TextFillColorDisabledBrush"))],
    "subtlepoint": [(COMMON, SUBTLE_CELL, POINTED)],
    "repeatpoint": [(COMMON, REPEAT_CELL, POINTED)],
    "checkboxpoint": [(SELECTION, CHECKBOX_CELL, POINTED)],
    # The same cell-shape on a state the framework does not write locally: a check box that is merely checked.
    # Its row forwards the same brush the control carries by default, so deletion says nothing either way and the
    # re-point is the only half with teeth - which is what makes this pair the ruler for "does a template-trigger
    # cell reach the control's own property at all".
    "checkedcell": [(SELECTION, CHECKED_CELL, "")],
    "checkedpoint": [(SELECTION, CHECKED_CELL, '<Setter Property="Foreground" Value="{ThemeResource TextFillColorDisabledBrush}" />')],
    # Which writer stands behind a deleted disabled cell? That cell's brush is the same instance the name
    # `TextDisabled` forwards, and #12 measured the framework taking its disabled label ink from that name. If
    # this is the same mechanism, the disabled button then reads secondary ink (#9E000000) with no cell in the
    # way; if the ink is being supplied some other way it stays #5C000000 and the question stays open.
    "subtlecellmask": [(COMMON, SUBTLE_CELL, ""), (RETINTS, TEXT_DISABLED, TEXT_DISABLED_MOVED)],
    # Same question on the other owner: the check box's disabled cell lives in a template trigger rather than a
    # style trigger, so which writer stands behind it is measured separately rather than carried over.
    "checkboxcellmask": [(SELECTION, CHECKBOX_CELL, ""), (RETINTS, TEXT_DISABLED, TEXT_DISABLED_MOVED)],
    # The two selected-tab cells of `Styles/TabView.jalxaml:203`, one written on the control and one on the
    # `IconHost` part. Unlike every row above, this control's resting ink is a *different* brush
    # (`TabViewItemHeaderForeground` = secondary, the selected row = primary), so deletion alone can be decisive -
    # and if the part's deletion is still green, the part is inheriting and that row has no witness at all.
    "tabselectedcell": [(TABVIEW, TAB_HEADER_SELECTED_CELL, "")],
    "tabselectedpoint": [(TABVIEW, TAB_HEADER_SELECTED_CELL, TAB_POINTED)],
    "tabiconcell": [(TABVIEW, TAB_ICON_SELECTED_CELL, "")],
    "tabiconpoint": [(TABVIEW, TAB_ICON_SELECTED_CELL, TAB_ICON_POINTED)],
}


def paths(name: str):
    seen = []
    for path, _, _ in TARGETS[name]:
        if path not in seen:
            seen.append(path)
    return seen


def bak_path(name: str, target: Path) -> Path:
    BAK.mkdir(exist_ok=True)
    return BAK / f"{target.name}.{name}.bak"


def apply(name: str) -> int:
    texts = {}
    for path, old, new in TARGETS[name]:
        text = texts.get(path) or path.read_text(encoding="utf-8")
        found = text.count(old)
        if found != 1:
            print(f"FAIL apply {name}: needle in {path.name} found {found} time(s), wanted 1")
            return 1
        texts[path] = text.replace(old, new)
    for path in texts:
        shutil.copyfile(path, bak_path(name, path))
    for path, text in texts.items():
        path.write_text(text, encoding="utf-8", newline="")
    print(f"ok apply {name} ({len(texts)} file(s) mutated)")
    return 0


def revert(name: str) -> int:
    if not paths(name):
        print(f"FAIL revert {name}: unknown target")
        return 1
    for path in paths(name):
        saved = bak_path(name, path)
        if not saved.exists():
            print(f"FAIL revert {name}: no saved copy at {saved.name}")
            return 1
        shutil.copyfile(saved, path)
        saved.unlink()
        clean = subprocess.run(
            ["git", "diff", "--quiet", "--", str(path.relative_to(ROOT))], cwd=ROOT
        )
        if clean.returncode != 0:
            print(f"FAIL revert {name}: git still reports {path.name} changed")
            return 1
    print(f"ok revert {name} ({', '.join(p.name for p in paths(name))} clean against HEAD)")
    return 0


def main() -> int:
    action, name = sys.argv[1], sys.argv[2]
    if name not in TARGETS:
        raise SystemExit(f"unknown target {name}")
    if action == "apply":
        return apply(name)
    if action == "revert":
        return revert(name)
    raise SystemExit(f"unknown action {action}")


if __name__ == "__main__":
    raise SystemExit(main())
