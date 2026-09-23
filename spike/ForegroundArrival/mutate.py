"""Toggle one style row's presence so a fact's teeth can be measured by A/B.

Usage: mutate.py <remove|restore> <title|subtitle|combo>

Each target names the exact line text as it appears in the tree; `remove` rewrites the element without its
Foreground row, `restore` puts the row back. Both are exact-string operations and both fail loudly if the
needle is not found exactly once, so a drifted file cannot silently produce a half-mutated build.
"""

import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]

TITLE_OLD = '                      <TextBlock Name="TitleTextBlock" Text="{TemplateBinding Title}" Foreground="{ThemeResource TeachingTipTitleForegroundBrush}" FontWeight="SemiBold" TextWrapping="Wrap" />\n'
TITLE_NEW = '                      <TextBlock Name="TitleTextBlock" Text="{TemplateBinding Title}" FontWeight="SemiBold" TextWrapping="Wrap" />\n'
SUBTITLE_OLD = '                      <TextBlock Name="SubtitleTextBlock" Text="{TemplateBinding Subtitle}" Foreground="{ThemeResource TeachingTipSubtitleForegroundBrush}" TextWrapping="Wrap" />\n'
SUBTITLE_NEW = '                      <TextBlock Name="SubtitleTextBlock" Text="{TemplateBinding Subtitle}" TextWrapping="Wrap" />\n'
# The scroller keeps its own line; only the attribute comes and goes.
COMBO_OLD = '<ScrollViewer Name="PART_ScrollViewer" Foreground="{ThemeResource ComboBoxDropDownForeground}"'
COMBO_NEW = '<ScrollViewer Name="PART_ScrollViewer"'

TARGETS = {
    "title": (ROOT / "src/FluentJalium/Styles/TeachingTip.jalxaml", TITLE_OLD, TITLE_NEW),
    "subtitle": (ROOT / "src/FluentJalium/Styles/TeachingTip.jalxaml", SUBTITLE_OLD, SUBTITLE_NEW),
    "combo": (ROOT / "src/FluentJalium/Styles/Selection.jalxaml", COMBO_OLD, COMBO_NEW),
    # Not a removal: point the row at a colour no other writer could be supplying, to tell "this edit is not in
    # the build" apart from "another writer wins over this row".
    "titleswap": (
        ROOT / "src/FluentJalium/Styles/TeachingTip.jalxaml",
        TITLE_OLD,
        TITLE_OLD.replace("TeachingTipTitleForegroundBrush", "TextFillColorDisabledBrush"),
    ),
    "subtitleswap": (
        ROOT / "src/FluentJalium/Styles/TeachingTip.jalxaml",
        SUBTITLE_OLD,
        SUBTITLE_OLD.replace("TeachingTipSubtitleForegroundBrush", "TextFillColorDisabledBrush"),
    ),
    "comboswap": (
        ROOT / "src/FluentJalium/Styles/Selection.jalxaml",
        COMBO_OLD,
        COMBO_OLD.replace("ComboBoxDropDownForeground", "TextFillColorDisabledBrush"),
    ),
}


def main() -> int:
    action, name = sys.argv[1], sys.argv[2]
    path, with_row, without_row = TARGETS[name]
    text = path.read_text(encoding="utf-8")
    if action == "remove":
        src, dst, want = with_row, without_row, 1
    elif action == "restore":
        src, dst, want = without_row, with_row, 1
    else:
        raise SystemExit(f"unknown action {action}")
    found = text.count(src)
    if found != want:
        print(f"FAIL {name} {action}: needle found {found} time(s), wanted {want}")
        return 1
    path.write_text(text.replace(src, dst), encoding="utf-8", newline="")
    print(f"ok {name} {action} ({'row present' if action == 'restore' else 'row mutated'})")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
