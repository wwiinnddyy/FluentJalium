"""Move every Astra focus ring out of its control template and onto FrameworkElement.FocusVisualStyle.

Reads the five style dictionaries, finds each named ring Border, deletes the ring part together with the
state cells that only existed to toggle it, and adds one FocusVisualStyle setter to the style that owned the
ring. Everything it does is asserted: an unexpected hit count aborts the run rather than half-editing markup.
"""

import io
import re
import sys

ROOT = r"C:\git\Jalium\FluentJalium\src\FluentJalium\Styles"

# ring part name -> the FocusVisuals.jalxaml key that replaces it
KEYS = {
    "FocusOutline": "FocusVisualRingStyle",
    "SwitchFocus": "FocusVisualRingStyle",
    "CheckFocus": "FocusVisualCheckStyle",
    "RadioFocus": "FocusVisualCheckStyle",
    "SliderFocus": "FocusVisualSliderStyle",
}

# The vertical Slider template reuses the part name; decided per line range below.
VERTICAL_RANGE = (270, 335)

RING_OPEN = re.compile(r'^\s*<Border Name="(?P<name>FocusOutline|SwitchFocus|CheckFocus|RadioFocus|SliderFocus)"')
SETTER_RING = re.compile(r'<Setter TargetName="(?:FocusOutline|SwitchFocus|CheckFocus|RadioFocus|SliderFocus)" Property="Opacity" Value="[^"]*" />')
TRIGGER_SOLO = re.compile(r'^\s*<Trigger Property="IsKeyboardFocused" Value="True">\s*'
                          r'<Setter TargetName="[^"]+" Property="Opacity" Value="[^"]+" />\s*</Trigger>\s*$')
TRIGGER_MULTI_OPEN = re.compile(r'^\s*<Trigger Property="IsKeyboardFocused" Value="True">\s*$')


def ring_block(lines, start):
    """The ring Border plus its nested inner Border: from the open tag to its own </Border>.

    The inner Border is self-closed, so it contributes no closing tag and must not be counted as one.
    """
    depth = 0
    i = start
    while True:
        opens = len(re.findall(r"<Border\b", lines[i]))
        # A "/>" closes the most recently opened element, whether that element started on this line (the
        # nested inner Border) or on an earlier one (the rings that are self-closed with no child).
        self_closed = lines[i].count("/>")
        depth += opens - self_closed - lines[i].count("</Border>")
        if depth <= 0 and (self_closed or "</Border>" in lines[i]):
            return i
        i += 1
        if i - start > 40:
            sys.exit(f"ring block does not close near line {start + 1}: {lines[start]!r}")


def owner_style(lines, index):
    for i in range(index, -1, -1):
        m = re.match(r'\s*<Style\b(?P<attrs>.*)', lines[i])
        if m:
            key = re.search(r'x:Key="([^"]+)"', m.group("attrs"))
            target = re.search(r'TargetType="([^"]+)"', m.group("attrs"))
            return i, key.group(1) if key else f"(implicit {target.group(1) if target else '?'})"
    return None, None


report = []
for filename in ["Common.jalxaml", "Inputs.jalxaml", "Navigation.jalxaml", "Selection.jalxaml", "Surfaces.jalxaml"]:
    path = ROOT + "\\" + filename
    lines = io.open(path, encoding="utf-8").read().split("\n")
    out = []
    setters = []
    i = 0
    while i < len(lines):
        line = lines[i]

        match = RING_OPEN.match(line)
        if match:
            end = ring_block(lines, i)
            at, style = owner_style(lines, i)
            assert at is not None, f"{filename}:{i + 1} ring has no owning <Style>"
            name = match.group("name")
            key = KEYS[name]
            if name == "SliderFocus" and i + 1 in range(VERTICAL_RANGE[0], VERTICAL_RANGE[1]):
                key = "FocusVisualSliderVerticalStyle"
            setters.append((lines[at], style, key, i + 1))
            report.append(f"{filename}:{i + 1}-{end + 1} drops part {name} -> {key} (owner {style})")
            i = end + 1
            continue

        if TRIGGER_SOLO.match(line):
            report.append(f"{filename}:{i + 1} drops a ring-only cell")
            i += 1
            continue

        if TRIGGER_MULTI_OPEN.match(line):
            # a multi-line cell: drop it only when every setter inside targets the ring
            j = i + 1
            body = []
            while j < len(lines) and "</Trigger>" not in lines[j]:
                body.append(lines[j])
                j += 1
            if body and all(SETTER_RING.search(b) and "TargetName" in b for b in body):
                report.append(f"{filename}:{i + 1}-{j + 1} drops a multi-line ring cell")
                i = j + 1
                continue

        # mixed cells (the check/radio IsEnabled row writes foreground too): remove only the ring setter
        if "TargetName=" in line and SETTER_RING.search(line):
            stripped = SETTER_RING.sub("", line)
            empty = re.match(r"^\s*<Trigger [^>]*>\s*</Trigger>\s*$", stripped)
            if empty:
                report.append(f"{filename}:{i + 1} drops a cell left with no setter")
                i += 1
                continue
            report.append(f"{filename}:{i + 1} strips the ring setter out of a mixed cell")
            out.append(stripped)
            i += 1
            continue

        out.append(line)
        i += 1

    # insert one FocusVisualStyle setter per owning style, right after its opening tag
    wanted = {}
    for owner_line, style, key, _ in setters:
        wanted.setdefault((owner_line, style), set()).add(key)
    final = []
    placed = set()
    for line in out:
        final.append(line)
        for (owner_line, style), keys in wanted.items():
            if line == owner_line and (owner_line, style) not in placed:
                placed.add((owner_line, style))
                indent = re.match(r"\s*", line).group(0)
                for key in sorted(keys):
                    final.append(f'{indent}  <Setter Property="FocusVisualStyle" Value="{{ThemeResource {key}}}" />')
    missing = [style for (o, style) in wanted if (o, style) not in placed]
    if missing:
        sys.exit(f"{filename}: no owner line matched for {missing}")
    io.open(path, "w", encoding="utf-8", newline="").write("\n".join(final))
    print(f"{filename}: {len(setters)} ring site(s) rewritten")

print("\n".join(report))
