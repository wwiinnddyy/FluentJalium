"""Compare two AstraPagePixels --report logs field by field.

The shell one-liners kept tripping over the '|' separators inside the report lines themselves, so the
parsing lives here: one regex per line, then a field-wise diff. Reads only, writes nothing.
"""

import re
import sys

LINE = re.compile(
    r"^(?P<page>[a-z-]+) (?P<variant>\w+): "
    r"stable=(?P<whole>\w+)/(?P<slot>\w+) "
    r"painted=(?P<painted>\d+) "
    r"light-base=(?P<lb>\d+) dark-base=(?P<db>\d+) hc-base=(?P<hb>\d+) "
    r"placeholder=(?P<ph>\d+) green=(?P<gr>\d+) \| "
    r"slot (?P<colours>\d+) colours (?P<slotpx>\d+)px over (?P<emptycolours>\d+) colours (?P<emptypx>\d+)px "
    r"\(slot (?P<w>\d+)x(?P<h>\d+)\)"
    r"(?: rings (?P<found>\d+) found/(?P<stopped>\d+) stopped)? "
    r"\| top (?P<top>.*)$"
)

FIELDS = ("whole", "slot", "painted", "lb", "db", "hb", "ph", "gr",
          "colours", "slotpx", "emptycolours", "emptypx", "w", "h", "top")


def load(path):
    rows = {}
    with open(path, encoding="utf-8") as handle:
        for line in handle:
            match = LINE.match(line.strip())
            if match:
                rows[(match.group("page"), match.group("variant"))] = match.groupdict()
    return rows


def main(base_path, still_path):
    base = load(base_path)
    still = load(still_path)
    print(f"parsed legs: base={len(base)} still={len(still)}")
    if set(base) != set(still):
        print(f"LEG SETS DIFFER: only-base={sorted(set(base) - set(still))} "
              f"only-still={sorted(set(still) - set(base))}")

    changed = 0
    for key in sorted(base):
        if key not in still:
            continue
        diffs = [(f, base[key][f], still[key][f]) for f in FIELDS if base[key][f] != still[key][f]]
        if diffs:
            changed += 1
            print(f"{key[0]} {key[1]}: rings {still[key]['found']} found/{still[key]['stopped']} stopped")
            for field, old, new in diffs:
                print(f"    {field}: {old} -> {new}")
    print(f"legs with any field difference: {changed}/{len(base)}")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
