"""Compare every upstream Thickness measure row against the Astra row that carries the same key.

The spacing batch fixed three controls one at a time. This is the sweep version: read the keys out of the
WinUI reference theme dictionaries, read the keys out of our ThemeResources, and print the pairs that exist
on both sides with different values, plus the upstream rows we simply do not have. Reference files are
read-only input; nothing here writes outside spike/SpacingSweep.
"""
import os
import re
import sys
import xml.etree.ElementTree as ET

UPSTREAM = r"C:\git\Jalium\microsoft-ui-xaml\controls\dev"
OURS = r"C:\git\Jalium\FluentJalium\src\FluentJalium\ThemeResources"
X = "{http://schemas.microsoft.com/winfx/2006/xaml}"

THICK = re.compile(r"^\s*(-?[\d.]+)\s*,\s*(-?[\d.]+)\s*,\s*(-?[\d.]+)\s*,\s*(-?[\d.]+)\s*$")
SCALAR = re.compile(r"^-?[\d.]+$")


def walk(root):
    for dirpath, dirnames, filenames in os.walk(root):
        dirnames[:] = [d for d in dirnames if "perf2026" not in d]
        for name in filenames:
            if "perf2026" in name:
                continue
            if root == UPSTREAM:
                # controls/dev also holds the sample pages, and those declare their own Thickness rows
                # (BorderThicknessPage.xaml has CheckBoxCheckedStrokeThickness, CommonStylesPage.xaml has
                # CommandBarMoreButtonMargin). Only the theme dictionaries are upstream's spec.
                if not name.endswith("_themeresources.xaml") and not name.endswith(
                        ("_themeresources_any.xaml", "_themeresources_20260420.xaml")):
                    continue
            if name.endswith((".xaml", ".jalxaml")):
                yield os.path.join(dirpath, name)


def rows(path):
    """Return {key: (kind, value, file)} for Thickness / x:Double / x:String resource rows."""
    text = open(path, encoding="utf-8-sig").read()
    try:
        # ET refuses a duplicate x:Key inside one dictionary; the upstream files are full of theme
        # scoping blocks, so parse line by line instead of as a document.
        pass
    except Exception:
        pass
    found = {}
    pattern = re.compile(
        r"<(Thickness|x:Double|sys:Double|x:String|FontFamily|CornerRadius)\s+x:Key=\"([^\"]+)\"[^>]*>(.*?)</\1>",
        re.S,
    )
    for kind, key, value in pattern.findall(text):
        value = " ".join(value.split())
        found.setdefault(key, (kind, value, os.path.basename(path)))
    return found


def norm(kind, value):
    if kind == "Thickness":
        m = THICK.match(value)
        if m:
            return tuple(float(g) for g in m.groups())
        if SCALAR.match(value):
            v = float(value)
            return (v, v, v, v)
    return value


def main():
    upstream = {}
    for path in walk(UPSTREAM):
        for key, row in rows(path).items():
            if row[0] != "Thickness":
                continue
            upstream.setdefault(key, []).append((norm(*row[:2]), row[2]))
    ours = {}
    for path in walk(OURS):
        for key, row in rows(path).items():
            if row[0] != "Thickness":
                continue
            ours[key] = (norm(*row[:2]), row[2])

    diff, missing, shared = [], [], []
    # A whole theme dictionary that has not been transcribed yet (TeachingTip, ContentDialog, the list
    # families) is not a spacing defect, so an unpublished row only counts when its own file already has a
    # published sibling - the families actually in play.
    in_play = {file for key in ours for _, file in upstream.get(key, [])}
    for key, variants in sorted(upstream.items()):
        values = sorted({v for v, _ in variants}, key=lambda t: str(t))
        source = variants[0][1]
        if key in ours:
            ours_value = ours[key][0]
            if all(ours_value != v for v in values):
                diff.append((key, values, ours_value, ours[key][1]))
            else:
                shared.append(key)
            continue
        if source in in_play:
            missing.append((key, values, source))

    print(f"upstream Thickness rows: {len(upstream)}   ours: {len(ours)}   matching value: {len(shared)}")
    print(f"\n== value differs ({len(diff)}) ==")
    for key, values, ours_value, source in diff:
        print(f"{key}\tours={ours_value}\tupstream={values}\t{source}")
    print(f"\n== upstream rows we do not publish ({len(missing)}) ==")
    for key, values, source in missing:
        print(f"{key}\t{values}\t{source}")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
