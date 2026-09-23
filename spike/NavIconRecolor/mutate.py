"""Cut one leg of the icon-ink hand-off so the fact that depends on it has to go red.

Usage:
    python spike/NavIconRecolor/mutate.py apply    <nobind|noguard|noappbarink|notoggleink>
    python spike/NavIconRecolor/mutate.py revert   <nobind|noguard|noappbarink|notoggleink>
    python spike/NavIconRecolor/mutate.py hash

Each mutant is (file, how many lines match, marker locating those lines, how the line changes). Lines are matched
by substring and rewritten in place, so no needle has to carry exact indentation, and the count is asserted
before anything is written. apply prints the diff it made - that diff, not a marker, is the witness grepped out
of the leg log.

revert restores the byte snapshot taken at the first apply: the fix is uncommitted while this runs, so
`git checkout HEAD --` would throw the fix away together with the mutant.
"""

import difflib
import hashlib
import pathlib
import sys

ROOT = pathlib.Path("C:/git/Jalium/FluentJalium")
SNAPSHOT_DIR = pathlib.Path("C:/tmp/navicon-snapshots")
HASHFILE = pathlib.Path("C:/tmp/navicon-consumed.dll.sha1")
NL = chr(10)

ICONINK = ROOT / "src/FluentJalium/Controls/IconInk.cs"
APPBAR = ROOT / "src/FluentJalium/Styles/AppBar.jalxaml"
MENUS = ROOT / "src/FluentJalium/Styles/Menus.jalxaml"
TABVIEW = ROOT / "src/FluentJalium/Styles/TabView.jalxaml"
NAVIGATION = ROOT / "src/FluentJalium/Styles/Navigation.jalxaml"

# The attached hand-off and the markup hand-off, exactly as they appear on their own line.
ATTACHED = 'fluent:IconInk.Source="{Binding RelativeSource={RelativeSource TemplatedParent}}"'
SELF_ATTACHED = 'controls:IconInk.Source="{Binding RelativeSource={RelativeSource Self}}"'
TOGGLE = 'Foreground="{Binding Foreground, RelativeSource={RelativeSource AncestorType=Button}}"'


def strip_attr(line: str) -> str:
    for attr in (ATTACHED, SELF_ATTACHED, TOGGLE):
        if attr in line:
            return line.replace(attr, "")
    raise SystemExit("strip_attr: no known attribute on " + line.strip())


def drop_statement(line: str) -> str:
    indent = line[: len(line) - len(line.lstrip())]
    return indent + "{ } // MUTANT:nobind - the carrier's ink is never handed to the icon"


def widen_guard(line: str) -> str:
    return line[:-1] + " || true)"


MUTANTS = {
    # The code route: Apply runs but binds nothing, so a live theme switch has no property to invalidate.
    "nobind": (ICONINK, 1, "BindingOperations.SetBinding(icon, IconElement.ForegroundProperty", drop_statement),
    # The guard goes: an icon that arrived with its own ink gets clobbered by the carrier's.
    "noguard": (ICONINK, 1, "if (ownInk is null || ReferenceEquals(ownInk", widen_guard),
    # The framework-host route, twice over: strip the attached source out of the app bar template.
    "noappbarink": (APPBAR, 2, ATTACHED, strip_attr),
    # ... the three menu presenters, ...
    "nomenusink": (MENUS, 3, ATTACHED, strip_attr),
    # ... and the tab item's icon host.
    "notabink": (TABVIEW, 1, SELF_ATTACHED, strip_attr),
    # The pane toggle glyph's own markup hand-off.
    "notoggleink": (NAVIGATION, 1, TOGGLE, strip_attr),
}


def consumed_dll() -> pathlib.Path:
    return ROOT / "tests/FluentJalium.Tests/bin/Debug/net10.0-windows/FluentJalium.dll"


def digest() -> str:
    path = consumed_dll()
    return hashlib.sha1(path.read_bytes()).hexdigest() if path.exists() else "missing"


def lines_of(path: pathlib.Path) -> list[str]:
    return path.read_text(encoding="utf-8").split(NL)


def main() -> int:
    action = sys.argv[1]

    if action == "hash":
        # The consumed copy is what the test host loads, so its digest witnesses that a rebuilt mutant landed
        # rather than a stale binary from the last leg running again.
        now = digest()
        previous = HASHFILE.read_text().strip() if HASHFILE.exists() else "none"
        print(f"consumed-dll {now} previous {previous} changed={now != previous}")
        HASHFILE.write_text(now, encoding="utf-8")
        return 0

    name = sys.argv[2]
    file, times, marker, transform = MUTANTS[name]
    snap = SNAPSHOT_DIR / (file.name + ".snap")

    # Revert has to run before the marker is read: a mutated file no longer contains it, and a revert that
    # quietly gives up would leave the mutant in the tree.
    if action == "revert":
        file.write_text(snap.read_text(encoding="utf-8"), encoding="utf-8", newline="")
        if lines_of(file) != lines_of(snap):
            print("REVERT FAILED - tree still mutated")
            return 1
        print(f"reverted {name}: {file.name} is byte-identical to its snapshot")
        return 0

    before = lines_of(file)
    hits = [index for index, line in enumerate(before) if marker in line]

    if len(hits) != times:
        print(f"SETUP FAILED {name}: marker matched {len(hits)} lines, expected {times}")
        return 1

    after = list(before)
    for index in hits:
        after[index] = transform(after[index])

    if action == "apply":
        SNAPSHOT_DIR.mkdir(parents=True, exist_ok=True)
        if not snap.exists():
            snap.write_text(NL.join(before), encoding="utf-8")
        file.write_text(NL.join(after), encoding="utf-8", newline="")
        if lines_of(file) != after:
            print(f"POSTCONDITION FAILED {name}: file does not hold the mutation")
            return 1
        print(f"applied {name} on {file.name} ({times} line(s), snapshot {snap.name})")
        print(NL.join(difflib.unified_diff(before, after, "before", "after", lineterm="", n=0)))
        return 0

    print(f"unknown action {action}")
    return 2


if __name__ == "__main__":
    sys.exit(main())
