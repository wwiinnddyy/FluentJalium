"""Mutate the pane-icon ink hand-off so the two facts can be shown to depend on it.

Usage: python spike/NavIconRecolor/mutate.py {apply|revert} {nobind|noguard}

revert restores the byte snapshot taken at the first apply, because the fix is uncommitted while this runs -
`git checkout HEAD --` would throw the fix away with the mutant.
"""

import hashlib
import pathlib
import sys

ROOT = pathlib.Path("C:/git/Jalium/FluentJalium")
TARGET = ROOT / "src/FluentJalium/Controls/Navigation/FluentNavigationItem.cs"
SNAPSHOT = pathlib.Path("C:/tmp/navicon-fix.cs")
HASHFILE = pathlib.Path("C:/tmp/navicon-consumed.dll.sha1")

BIND = (
    "            BindingOperations.SetBinding(icon, IconElement.ForegroundProperty, "
    "new Binding(nameof(Foreground)) { Source = item });"
)
GUARD = "        if (ownInk is null || ReferenceEquals(ownInk, DependencyProperty.UnsetValue))"

MUTANTS = {
    # Take the hand-off away: the icon is never given the item's ink.
    # The empty block keeps the guard's `if` well-formed; the hand-off itself is what goes away.
    "nobind": (BIND, "            { } // MUTANT:nobind - the item's ink is never handed to the icon"),
    # Keep the hand-off but drop the "the icon carries its own ink" test: an app-set icon foreground gets clobbered.
    "noguard": (GUARD, GUARD[:-1] + " || true) // MUTANT:noguard"),
}


def consumed_dll() -> pathlib.Path:
    return ROOT / "tests/FluentJalium.Tests/bin/Debug/net10.0-windows/FluentJalium.dll"


def digest() -> str:
    path = consumed_dll()
    return hashlib.sha1(path.read_bytes()).hexdigest() if path.exists() else "missing"


def main() -> int:
    action = sys.argv[1]
    if action == "hash":
        # The consumed copy is what the test run loads, so its digest is the witness that a rebuilt mutant landed.
        now = digest()
        previous = HASHFILE.read_text().strip() if HASHFILE.exists() else "none"
        print(f"consumed-dll {now} previous {previous} changed={now != previous}")
        HASHFILE.write_text(now, encoding="utf-8")
        return 0

    name = sys.argv[2]
    old, new = MUTANTS[name]
    text = TARGET.read_text(encoding="utf-8")

    if action == "apply":
        if text.count(old) != 1:
            print(f"SETUP FAILED {name}: needle found {text.count(old)} times")
            return 1
        if not SNAPSHOT.exists():
            SNAPSHOT.write_text(text, encoding="utf-8")
        TARGET.write_text(text.replace(old, new), encoding="utf-8", newline="")
        if TARGET.read_text(encoding="utf-8").count(new) != 1:
            print(f"POSTCONDITION FAILED {name}: mutant not in the file")
            return 1
        print(f"applied {name} (marker present, snapshot kept)")
        return 0

    if action == "revert":
        TARGET.write_text(SNAPSHOT.read_text(encoding="utf-8"), encoding="utf-8", newline="")
        if TARGET.read_text(encoding="utf-8") != SNAPSHOT.read_text(encoding="utf-8"):
            print("REVERT FAILED - tree still mutated")
            return 1
        print(f"reverted {name} (file equals snapshot)")
        return 0

    print(f"unknown action {action}")
    return 2


if __name__ == "__main__":
    sys.exit(main())
