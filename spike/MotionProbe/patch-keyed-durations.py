"""Point every template transition duration at its published upstream key.

Each of the three literals our styles carried has an upstream row with the same number: 0.083 is
ControlFasterAnimationDuration, 0.167 is ControlFastAnimationDuration (Common_themeresources_any.xaml:606,604) and the
single 0.200 sits on PART_PaneRoot's Width, which is SplitViewPaneAnimationOpenDuration
(SplitView_themeresources.xaml:10). The counts are asserted rather than reported, because a replacement that silently
missed a site would leave a duration that ReduceMotion cannot reach - the whole point of the change.
"""

import io
import os

ROOT = r"C:\git\Jalium\FluentJalium\src\FluentJalium\Styles"
MAP = [
    ('TransitionDuration="0:0:0.083"', 'TransitionDuration="{ThemeResource ControlFasterAnimationDuration}"', 16),
    ('TransitionDuration="0:0:0.167"', 'TransitionDuration="{ThemeResource ControlFastAnimationDuration}"', 4),
    ('TransitionDuration="0:0:0.200"', 'TransitionDuration="{ThemeResource SplitViewPaneAnimationOpenDuration}"', 1),
]

totals = {old: 0 for old, _, _ in MAP}
for name in sorted(os.listdir(ROOT)):
    if not name.endswith(".jalxaml"):
        continue
    path = os.path.join(ROOT, name)
    with io.open(path, encoding="utf-8-sig", newline="") as handle:
        text = handle.read()
    changed = []
    for old, new, _ in MAP:
        hits = text.count(old)
        if hits:
            text = text.replace(old, new)
            totals[old] += hits
            changed.append(f"{name}:{hits}")
    if changed:
        with io.open(path, "w", encoding="utf-8", newline="") as handle:
            handle.write(text)
        print(" ".join(changed))

print("totals:", {old.split('=')[-1]: count for old, count in totals.items()})
for old, _, expected in MAP:
    assert totals[old] == expected, f"{old} replaced {totals[old]} times, expected {expected}"

remaining = 0
for name in sorted(os.listdir(ROOT)):
    if name.endswith(".jalxaml"):
        with io.open(os.path.join(ROOT, name), encoding="utf-8-sig") as handle:
            remaining += handle.read().count('TransitionDuration="0:')
print("literal durations left:", remaining)
assert remaining == 0
