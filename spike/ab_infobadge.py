import io
import os
import sys

# The InfoBadge batch's A/B record: `mutate` breaks exactly the three things AstraInfoBadgeTests claims to read,
# `revert` puts them back. Each anchor must occur once - a second match would make the mutation meaningless.
root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
path = os.path.join(root, "src", "FluentJalium", "Styles", "InfoBadge.jalxaml")
mutate = sys.argv[1] == "mutate"

pairs = [
    # (a) the foreground binding the value test reads
    ('<TextBlock Name="ValueTextBlock" Foreground="{TemplateBinding Foreground}" FontSize="11"',
     '<TextBlock Name="ValueTextBlock" FontSize="11"'),
    # (b) the margin setter the trigger test reads
    ('<Setter TargetName="ValueTextBlock" Property="Margin" Value="{ThemeResource ValueInfoBadgeTextMargin}" />',
     '<!-- mutated -->'),
    # (c) the Critical severity's only difference from the default style: point it at another severity's brush
    ('<Setter Property="Background" Value="{ThemeResource SystemFillColorCriticalBrush}" />',
     '<Setter Property="Background" Value="{ThemeResource AccentFillColorDefaultBrush}" />'),
]

t = io.open(path, encoding="utf-8", newline="").read()
for old, new in pairs:
    if mutate:
        assert t.count(old) == 1, "anchor missing: " + old[:60]
        t = t.replace(old, new)
    else:
        assert t.count(new) == 1, "mutation marker missing: " + new[:60]
        t = t.replace(new, old)
io.open(path, "w", encoding="utf-8", newline="").write(t)
print(("mutated" if mutate else "reverted"), path)
