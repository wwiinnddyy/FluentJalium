"""Replace the one line the compiler refused to parse, by line index rather than by exact text.

An exact-text match fails here in two independent tools even though a byte dump shows the line and the search string as
the same characters, so this asserts on a substring and rewrites by index. The assertion still runs before anything is
written, and the printed replacement makes the change reviewable in the log.
"""

import io

path = r"C:\git\Jalium\FluentJalium\spike\MotionProbe\Program.cs"

with io.open(path, encoding="utf-8-sig", newline="") as handle:
    text = handle.read()

ending = "\r\n" if "\r\n" in text else "\n"
lines = text.split(ending)
indexes = [index for index, line in enumerate(lines) if "MainWindow named=" in line]
print("matched lines:", indexes)
assert len(indexes) == 1, "expected exactly one line naming MainWindow"
print("was:", lines[indexes[0]].strip())

replacement = [
    '        var main = _application.MainWindow is null ? "none" : _application.MainWindow.GetType().Name;',
    "        var merged = _application.Resources.MergedDictionaries.Count;",
    '        Note("application", "MainWindow named=" + main + ", merged dictionaries=" + merged);',
]
lines[indexes[0]:indexes[0] + 1] = replacement

with io.open(path, "w", encoding="utf-8", newline="") as handle:
    handle.write(ending.join(lines))

print("patched, line count", len(lines))
