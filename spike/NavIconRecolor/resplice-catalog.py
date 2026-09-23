"""Re-point the three #95 gap strings in Catalog.json at the route that shipped.

The first splice described the walk-the-visual-tree version (`fluent:IconInk.Source`, "deleting that one
attribute"). That version never shipped - the serial gate rejected it - so the catalog was naming a mechanism
the shipped binary does not contain. This replaces those three strings in place: same array, same item, no new
gap, no reformatting (the file is byte-compared with the copy the Gallery reads at runtime).
"""

import json
import pathlib
import sys

CATALOG = pathlib.Path("C:/git/Jalium/FluentJalium/samples/FluentJalium.Gallery/Catalog.json")

REPLACEMENTS = [
    (
        "the icon under the content presenter is handed the button's ink through fluent:IconInk.Source: a live theme "
        "switch used to leave the glyph painted with the previous theme's ink (the frozen-ink census measured 519 "
        "pixels on this page), and deleting that one attribute turns "
        "AstraIconFamilyTests.A_template_that_hosts_an_icon_hands_it_the_carrier_ink red (#95). Hover, pressed and "
        "disabled glyph ink are still unread - the fact only covers Light<->Dark.",

        "the icon under the content presenter is handed the button's ink by a pair of ordinary bindings, "
        "fluent:IconInk.Carrier (whose ink) and fluent:IconInk.Icon (which icon): a live theme switch used to leave "
        "the glyph painted with the previous theme's ink, which the frozen-ink census measured as 519 pixels on this "
        "page and as 0 once the pair was in. Deleting the Carrier lines turns "
        "AstraIconFamilyTests.A_template_that_hosts_an_icon_hands_it_the_carrier_ink red, and letting the hand-off "
        "fire only on its first value turns "
        "AstraIconFamilyTests.An_icon_swapped_onto_the_host_after_it_was_built_is_handed_the_ink_too red - the "
        "second witness is only possible because the bindings stay live (#95). Hover, pressed and disabled glyph ink "
        "are still unread - these facts only cover Light<->Dark.",
    ),
    (
        "the same IconInk hand-off is wired on this template's IconContent, but the census never saw a menu icon at "
        "all: a closed flyout has no pixels to compare. So this host rests on the app-bar fact and the shared code "
        "route, not on a reading or a fact of its own (#95).",

        "the same IconInk.Carrier / IconInk.Icon pair is wired on this template's IconContent, but the census never "
        "saw a menu icon at all: a closed flyout has no pixels to compare. So this host rests on the app-bar fact "
        "and the shared code route, not on a reading or a fact of its own (#95).",
    ),
    (
        "the IconHost carries the same IconInk hand-off, so the icon follows the TabViewItemIconForeground row this "
        "template already writes; the census measured no frozen ink on this host either, which makes the wiring a "
        "mechanism resting on the app-bar fact rather than a reading of its own (#95).",

        "the IconHost carries the same IconInk.Carrier / IconInk.Icon pair (its carrier is the host itself), so the "
        "icon follows the TabViewItemIconForeground row this template already writes; the census measured no frozen "
        "ink on this host either, which makes the wiring a mechanism resting on the app-bar fact rather than a "
        "reading of its own (#95).",
    ),
]


def main() -> int:
    raw = CATALOG.read_bytes()
    had_bom = raw.startswith(b"\xef\xbb\xbf")
    text = raw.decode("utf-8-sig")
    if "\r\n" in text:
        print("SETUP FAILED: the catalog is not LF in the working tree, refusing to rewrite its line endings")
        return 1

    lines_before = len(text.splitlines())
    for old, new in REPLACEMENTS:
        o, n = json.dumps(old), json.dumps(new)
        hits = text.count(o)
        if hits != 1:
            print(f"SETUP FAILED: expected the old string exactly once, found {hits}: {old[:60]}...")
            return 1
        text = text.replace(o, n)

    payload = json.loads(text)
    for _, new in REPLACEMENTS:
        if not any(new in str(node) for node in payload.values()):
            print(f"POSTCONDITION FAILED: new string not in the parsed document: {new[:60]}...")
            return 1
    for old, _ in REPLACEMENTS:
        if old in text:
            print(f"POSTCONDITION FAILED: old string still present: {old[:60]}...")
            return 1

    CATALOG.write_bytes((b"\xef\xbb\xbf" if had_bom else b"") + text.encode("utf-8"))
    print(
        f"re-pointed {len(REPLACEMENTS)} gap strings; lines {lines_before} -> {len(text.splitlines())}, "
        f"bom={had_bom}"
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
