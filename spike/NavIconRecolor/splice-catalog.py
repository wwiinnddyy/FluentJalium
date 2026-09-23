"""Splice the #95 icon-ink readings into three Catalog gap arrays without reformatting the file.

The catalog is byte-compared against the copy the Gallery ships, so this edits text: it finds the entry by its
"markup" line, then either closes a one-line gaps array or the multi-line one, and appends one string.
"""

import json
import pathlib
import sys

CATALOG = pathlib.Path("C:/git/Jalium/FluentJalium/samples/FluentJalium.Gallery/Catalog.json")

ADDITIONS = {
    "AppBarButton": (
        "the icon under the content presenter is handed the button's ink through fluent:IconInk.Source: a live theme "
        "switch used to leave the glyph painted with the previous theme's ink (the frozen-ink census measured 519 "
        "pixels on this page), and deleting that one attribute turns "
        "AstraIconFamilyTests.A_template_that_hosts_an_icon_hands_it_the_carrier_ink red (#95). Hover, pressed and "
        "disabled glyph ink are still unread - the fact only covers Light<->Dark."
    ),
    "MenuFlyoutItem": (
        "the same IconInk hand-off is wired on this template's IconContent, but the census never saw a menu icon at "
        "all: a closed flyout has no pixels to compare. So this host rests on the app-bar fact and the shared code "
        "route, not on a reading or a fact of its own (#95)."
    ),
    "FluentJalium.Controls.FluentTabViewItem": (
        "the IconHost carries the same IconInk hand-off, so the icon follows the TabViewItemIconForeground row this "
        "template already writes; the census measured no frozen ink on this host either, which makes the wiring a "
        "mechanism resting on the app-bar fact rather than a reading of its own (#95)."
    ),
}


def splice(lines: list[str], markup: str, gap: str) -> list[str]:
    try:
        start = next(i for i, line in enumerate(lines) if f'"markup": "{markup}"' in line)
    except StopIteration:
        raise SystemExit(f"SETUP FAILED: no entry for {markup}")
    end = next(i for i, line in enumerate(lines[start:], start) if '"markup":' in line and i > start)

    for index in range(start, end):
        line = lines[index]
        if not line.lstrip().startswith('"gaps":'):
            continue
        closing = line.rstrip().find("]")
        if line.rstrip().endswith("]"):  # one-line array: insert before the bracket
            head = line[:closing].rstrip()
            if head.endswith(","):
                head = head[:-1]
            return lines[:index] + [head + ", " + json.dumps(gap) + " ]"] + lines[index + 1:]
        # multi-line: the array closes on its own bracket line; the element before it needs a comma
        close = next(i for i in range(index, end) if lines[i].strip() == "]")
        previous = lines[close - 1].rstrip()
        if not previous.endswith(","):
            lines[close - 1] = previous + ","
        indent = " " * 8
        return lines[:close] + [indent + json.dumps(gap)] + lines[close:]
    raise SystemExit(f"SETUP FAILED: entry {markup} has no gaps array")


def main() -> int:
    raw = CATALOG.read_bytes()
    had_bom = raw.startswith(b"\xef\xbb\xbf")
    before_text = raw.decode("utf-8-sig")
    if "\r\n" in before_text:
        print("SETUP FAILED: the catalog is not LF in the working tree, refusing to rewrite its line endings")
        return 1
    lines = before_text.split("\n")
    for markup, gap in ADDITIONS.items():
        lines = splice(lines, markup, gap)

    after_text = "\n".join(lines)
    payload = json.loads(after_text)  # must still parse
    for markup, gap in ADDITIONS.items():
        if not any(gap in str(node) for node in payload.values()):
            print(f"POSTCONDITION FAILED: {markup} gap not in the parsed document")
            return 1

    CATALOG.write_bytes((b"\xef\xbb\xbf" if had_bom else b"") + after_text.encode("utf-8"))
    print(f"spliced {len(ADDITIONS)} gap strings; lines {len(before_text.splitlines())} -> {len(after_text.splitlines())}, bom={had_bom}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
