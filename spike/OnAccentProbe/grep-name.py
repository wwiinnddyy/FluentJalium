"""Throwaway: is a name present in a shipped assembly's string heaps, in either encoding?

The .NET #US heap stores ldstr operands as UTF-16LE, so an ordinary ASCII grep over a DLL reports zero hits for a
name the IL plainly loads. This script searches both encodings and reports the offset count per encoding so a census
that says "the shipped 26.10.9 never mentions this name" can be told apart from "the grep was wrong" - which is the
difference between a null reading and a broken instrument (see the rule this repo already applies to uniform readings).
"""

import sys
from pathlib import Path


def hits(blob: bytes, needle: str) -> tuple[int, int]:
    return blob.count(needle.encode("ascii")), blob.count(needle.encode("utf-16-le"))


def main() -> int:
    target = sys.argv[1]
    names = sys.argv[2:]
    assembly = Path(target).read_bytes()
    print(f"# {target}  {len(assembly)} bytes")
    for name in names:
        ascii_count, utf16_count = hits(assembly, name)
        print(f"{name:28} ascii={ascii_count:4}  utf16={utf16_count:4}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
