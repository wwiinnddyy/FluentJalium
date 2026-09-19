"""Measure a captured Gallery page in DIPs instead of judging it by eye.

A 32 DIP control is 56 pixels in a dpi=168 capture, and a 6 DIP inset is 10 - invisible in a downscaled
picture, obvious as a number. This reports bounding boxes of an exact colour and the vertical extent of a
region, both converted back to DIPs, so a spacing claim becomes a reading rather than an impression.
"""
import sys

from PIL import Image

SCALE = 1.75


def load(path):
    image = Image.open(path).convert("RGB")
    return image, image.load(), image.size


def parsecolour(text):
    text = text.lstrip("#")
    return tuple(int(text[i:i + 2], 16) for i in (0, 2, 4))


def bbox(px, size, colour, tolerance=0, x0=0, y0=0, x1=None, y1=None):
    width, height = size
    x1 = x1 or width
    y1 = y1 or height
    minx, miny, maxx, maxy, count = x1, y1, -1, -1, 0
    for y in range(y0, y1):
        for x in range(x0, x1):
            r, g, b = px[x, y]
            if abs(r - colour[0]) <= tolerance and abs(g - colour[1]) <= tolerance and abs(b - colour[2]) <= tolerance:
                count += 1
                minx = min(minx, x)
                maxx = max(maxx, x)
                miny = min(miny, y)
                maxy = max(maxy, y)
    if count == 0:
        return None
    return {
        "px": count,
        "x": (minx, maxx),
        "y": (miny, maxy),
        "dip": (round(minx / SCALE, 1), round(miny / SCALE, 1),
                round((maxx - minx + 1) / SCALE, 1), round((maxy - miny + 1) / SCALE, 1)),
    }


def column(px, y, x0, x1, step=1):
    """Colour run table along one scan row - where the fills start and stop."""
    runs = []
    previous = None
    for x in range(x0, x1, step):
        value = px[x, y]
        if value != previous:
            runs.append((round(x / SCALE, 1), value))
            previous = value
    return runs


def row(px, x, y0, y1, step=1):
    runs = []
    previous = None
    for y in range(y0, y1, step):
        value = px[x, y]
        if value != previous:
            runs.append((round(y / SCALE, 1), value))
            previous = value
    return runs


def main():
    path = sys.argv[1]
    image, px, size = load(path)
    print(f"{path} {size}")
    mode = sys.argv[2]
    if mode == "bbox":
        for colour in sys.argv[3:]:
            parsed = parsecolour(colour)
            found = bbox(px, size, parsed)
            print(f"bbox {colour} -> {found}")
    elif mode == "row":
        x, y0, y1 = (int(v) for v in sys.argv[3:6])
        for entry in row(px, x, y0, y1):
            print(f"  y={entry[0]:>7} #{entry[1][0]:02X}{entry[1][1]:02X}{entry[1][2]:02X}")
    elif mode == "column":
        y, x0, x1 = (int(v) for v in sys.argv[3:6])
        for entry in column(px, y, x0, x1):
            print(f"  x={entry[0]:>7} #{entry[1][0]:02X}{entry[1][1]:02X}{entry[1][2]:02X}")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
