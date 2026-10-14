"""Crop a captured Gallery page to its content region and split it into readable bands.

The window is captured at full physical resolution (dpi=168 on a 175% machine), so a whole 1925x1435 frame
read by eye shows nothing useful - a 32 DIP row is 56 px there. This cuts the nav pane and the title bar off
and slices what is left into bands scaled back to about 1 DIP per pixel, which is the only regime where a
spacing error is visible without measuring. It is a looking tool, not a judgment: any number still comes from
a pixel scan or a property read-back.
"""
import os
import sys

from PIL import Image

SCALE = 1.0 / 1.75  # physical px -> DIP
NAV_PANE_DIP = 240
TITLE_BAR_DIP = 48
BAND_DIP = 520


def crop(path, out_dir, first_band_only=False):
    image = Image.open(path)
    width, height = image.size
    left = int(NAV_PANE_DIP * 1.75)
    top = int(TITLE_BAR_DIP * 1.75)
    content = image.crop((left, top, width, height))
    bands = max(1, -(-content.height // int(BAND_DIP * 1.75)))
    names = []
    for index in range(bands):
        if first_band_only and index:
            break
        start = index * int(BAND_DIP * 1.75)
        band = content.crop((0, start, content.width, min(content.height, start + int(BAND_DIP * 1.75))))
        band = band.resize((int(band.width * SCALE), int(band.height * SCALE)), Image.LANCZOS)
        name = os.path.join(out_dir, f"{os.path.splitext(os.path.basename(path))[0]}-{index + 1}.png")
        band.save(name)
        names.append(name)
    return names


def main():
    out_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "out", "bands")
    os.makedirs(out_dir, exist_ok=True)
    first = len(sys.argv) > 3 and sys.argv[3] == "--first"
    for path in sys.argv[1:3]:
        for name in crop(path, out_dir, first):
            print(name)


if __name__ == "__main__":
    main()
