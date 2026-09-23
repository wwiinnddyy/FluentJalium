"""Which state cells write a foreground, and which of those rows already have an arrival reading?

The ledger's #56 remainder is "the 237 Foreground setters only prove their key resolves". That denominator is
wrong for this job: most of those 237 rows are resting rows on a template part, not state cells. This script
separates them so the next batch has an honest worklist:

  state cells   - Foreground setters that live inside <Trigger>/<ConditionGroup> in Styles/*.jalxaml
  resting rows  - Foreground setters on the template's own element tree

and then greps tests/ for each key the state cells name, reporting whether any fact reads the value back. A key
with no reader is a row whose arrival is unproven; a key with a reader is already covered and must not be
re-measured.
"""

import re
import sys
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
STYLES = ROOT / "src/FluentJalium/Styles"
TESTS = ROOT / "tests/FluentJalium.Tests"

# A style block: the owner is the TargetType of the <Style> the setter sits under - dotted and
# namespace-prefixed names included, since `controls:AppBarButton` is a real TargetType here.
STYLE_OPEN = re.compile(r'<Style\b[^>]*TargetType="?([\w:.]+)"?')
SETTER = re.compile(r'<Setter\s+((?:TargetName="(\w+)"\s+)?Property="Foreground"[^>]*Value="\{ThemeResource\s+(\w+)\}")')
TRIGGER_OPEN = re.compile(r'<(Trigger|MultiTrigger|ConditionGroup)(?![\w.])')
TRIGGER_CLOSE = re.compile(r'</(Trigger|MultiTrigger|ConditionGroup)(?![\w.])')
TRIGGER_COND = re.compile(r'<Trigger\s+Property="(\w+)"[^>]*Value="([^"]*)"')
# The multi-condition cells live in `<MultiTrigger><MultiTrigger.Conditions><Condition .../>...`, which the first
# version of this script did not see at all: `<Trigger` never matches `<MultiTrigger`, and `ConditionGroup` never
# matches `Condition`, so every checked+hover row was silently absent from the denominator rather than misplaced.
CONDITION = re.compile(r'<Condition\s+Property="(\w+)"[^>]*Value="([^"]*)"')


POINTER = {"IsMouseOver", "IsPressed", "IsMouseCaptureWithin"}


def is_pointer_free(condition: str) -> bool:
    """A multi-condition cell needs a pointer as much as a single one does, so every part counts."""
    return all(part.split("=")[0] not in POINTER for part in condition.split("+"))


def classify(path: Path):
    """Yield (owner, target, condition, key, line) for every state-cell Foreground setter in one file."""
    owner = "?"
    depth = 0
    condition = None
    for lineno, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
        m = STYLE_OPEN.search(line)
        if m:
            owner = m.group(1)
        if TRIGGER_OPEN.search(line):
            depth += 1
        # Both shapes arrive on one line in this markup, so a joined condition is enough to tell a caller which
        # state a cell belongs to.
        conditions = TRIGGER_COND.findall(line) + CONDITION.findall(line)
        if conditions:
            condition = "+".join(f"{prop}={value}" for prop, value in conditions)
        for match in SETTER.finditer(line):
            if depth == 0:
                continue
            target = match.group(2) or "(self)"
            yield owner, target, condition or "?", match.group(3), lineno
        if TRIGGER_CLOSE.search(line):
            depth = max(0, depth - 1)
            if depth == 0:
                condition = None


def readers(key: str):
    hits = []
    for path in TESTS.glob("*.cs"):
        text = path.read_text(encoding="utf-8")
        if f'"{key}"' in text:
            hits.append(path.name)
    return hits


def main() -> int:
    cells = defaultdict(list)
    for path in sorted(STYLES.glob("*.jalxaml")):
        for owner, target, condition, key, lineno in classify(path):
            cells[(owner, target, condition)].append((key, path.name, lineno))

    covered = 0
    for (owner, target, condition), rows in sorted(cells.items()):
        for key, name, lineno in rows:
            r = readers(key)
            mark = "read" if r else "NONE"
            if r:
                covered += 1
            pointer_free = is_pointer_free(condition)
            print(f"{mark:4} {'ptr' if not pointer_free else '  '} {owner:26} "
                  f"{target:22} {condition:34} {key:46} {name}:{lineno} readers={','.join(r) or '-'}")
    total = sum(len(v) for v in cells.values())
    free = sum(
        1
        for (owner, target, condition), rows in cells.items()
        for _ in rows
        if is_pointer_free(condition)
    )
    print(f"\nstate-cell Foreground rows: {total}; pointer-free: {free}; "
          f"with a reader somewhere in tests: {covered}; without: {total - covered}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
