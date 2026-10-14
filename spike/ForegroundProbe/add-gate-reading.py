"""Adds the serial gate reading to the foreground-audit ROADMAP section.

Byte-level again on purpose: the file is LF in the working tree, and the anchor is unique text written by
``write-docs.py`` in this same batch, so a mismatch is loud rather than a silent near-append.
"""

import io
import sys

PATH = "docs/astra/ROADMAP.md"

ANCHOR = "不声称：探针没有 subject 行、因此**未测到达与否**的 7 个 owner"

READING = """**闸口读数（串行 `tools/Test-AstraGates.ps1`，管道退出 0）**：整套 **1450/1450 通过、0 失败、0 跳过**（6 m 16 s），
比上段 1436 多 14 条＝本段新类；`0 个警告 / 0 个错误`（3.57 s，含 Gallery 重编）；调色板三档 `checked=True`
（Light/Dark 各 83 源色→101 刷，HighContrast 101 键映射，另有 3 个上游键因 palette 无对应条目而按记录扣住）；
`keys.md is current: 1298 canonical lines.`；末行 `All Astra gates passed.`
本段翻主题的那 14 条跑在既有主题套件之间没有把任何一条邻居断言带红（整套 0 失败），所以"审计批会污染主题态"这条
担忧在本次读数下不成立。

"""


def main() -> int:
    data = io.open(PATH, "rb").read()
    if data.count(ANCHOR.encode("utf-8")) != 1:
        print("FAIL anchor count", data.count(ANCHOR.encode("utf-8")))
        return 1
    if "1450/1450" in data.decode("utf-8"):
        print("already written")
        return 0
    io.open(PATH, "wb").write(data.replace(ANCHOR.encode("utf-8"), (READING + ANCHOR).encode("utf-8"), 1))
    print("gate reading inserted")
    return 0


if __name__ == "__main__":
    sys.exit(main())
