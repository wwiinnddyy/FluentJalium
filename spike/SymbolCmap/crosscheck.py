"""Cross-check the Symbol member set the sweep parsed from source against the shipped 26.10.9 package.

    python crosscheck.py

`docs/astra/ROADMAP.md`'s cmap entry reads the enum out of the sibling source tree, which may be newer than the
runtime this repository pins. The NuGet package ships an XML documentation file that lists every enum field as
`F:Jalium.UI.Controls.Symbol.<Name>`, so the two member *sets* can be compared without building anything.
Values are only read from source: this proves the names agree, not that a codepoint moved.
"""

import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sweep import SYMBOL_SOURCE, members  # noqa: E402

PACKAGE = os.path.expanduser(r"~\.nuget\packages\jalium.ui.controls\26.10.9\lib\net10.0\Jalium.UI.Controls.xml")
FIELD = re.compile(r'name="F:Jalium\.UI\.Controls\.Symbol\.([A-Za-z][A-Za-z0-9_]*)"')


def main():
    source = {name for name, _ in members(SYMBOL_SOURCE)}
    if not os.path.exists(PACKAGE):
        print(f"missing shipped documentation: {PACKAGE}")
        return 1

    with open(PACKAGE, encoding="utf-8-sig") as handle:
        shipped = set(FIELD.findall(handle.read()))
    print(f"source members: {len(source)}, shipped 26.10.9 fields: {len(shipped)}")
    only_source = sorted(source - shipped)
    only_shipped = sorted(shipped - source)
    print(f"only in source: {len(only_source)} -> {', '.join(only_source) or 'none'}")
    print(f"only in shipped package: {len(only_shipped)} -> {', '.join(only_shipped) or 'none'}")
    print("name sets are identical" if not only_source and not only_shipped else "NAME SETS DIFFER")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
