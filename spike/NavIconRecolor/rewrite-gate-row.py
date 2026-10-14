"""Rewrite the 串行闸口 row of the 1.0 终态快照 table with the gate-95b reading.

The row is one very long line, so it is replaced wholesale by prefix rather than retyped as an old_string match:
a hand-copied prefix of the previous sentence would silently keep whatever follows it.
"""

import pathlib
import sys

DOC = pathlib.Path("C:/git/Jalium/FluentJalium/docs/astra/ROADMAP.md")

PREFIX = "| 串行闸口 |"

ROW = (
    "| 串行闸口 | **最新一次（`spike/PagePixelsPlateDiag/gate-93.log`，提交 `feb45af` 的树）：整条管道全绿** —— "
    "步骤依次是 restore、build（Debug，`0 个警告 / 0 个错误`）、套件 `失败: 0，通过: 1594，总计: 1594`（7 m 48 s）、"
    "页级像素 `PASS 13 pages x 3 variants, 0 offender(s)`（这一跑带着 #93 刚补的底板自证重编译过工具）、"
    "调色板漂移三档 `checked=True`、`keys.md is current: 1312 canonical lines.`、"
    "`known-gaps.md is current: 990 canonical lines.`，末尾打印 `All Astra gates passed.`，"
    "管道自己交出 `gate-exit=0`。上一跑全绿是 `09ec267` 的树（`spike/NavIconRecolor/gate-95b.log`，套件 8 m 26 s），"
    "再往前是 `gate-92.log` 的 1589 条。"
    "套件总数只能引管道打印的那一行：`[Theory]` 的 case 也要计进去，拿类别计数做加法一定会算错。"
    "**形状别丢**：这之前五次顺序跑各红在一步——页闸红 9 条（`gate-88.log`）/ 套件红 Rating 1 条（`gate-88b.log`）/ "
    "套件红 TeachingTip 1 条（`gate-88c.log`、`gate-94b.log`）/ 页闸红 `materials HighContrast` 2 条（`gate-91.log`）/ "
    "套件红本批自己的 `VisualTreeHelper` 一条（`gate-95.log`），被点名的测点单跑都绿。连着两次全绿**不解释其中任何一条**："
    "`gate-91` 那两条脏底板的复现条件仍未量（形状已收窄到『本页的槽当时还画在宿主上』，见 `## #93 一手`），"
    "页闸槽宽仍是整跑一个值、跑与跑之间不同（这跑每腿 788，`pixels-report-2.log` 那跑 39 腿全 1000），#47/#90 原样挂着"
    "（日志按批留在 `spike/SystemColorProbe/gate-*.log`、`spike/ForegroundArrival/gate-*.log` 与 "
    "`spike/NavIconRecolor/gate-9*.log`、`spike/PagePixelsPlateDiag/gate-93.log`；三档调色板与两份清单每次改动后各自 "
    "`-Check` 当场对过） |"
)


def main() -> int:
    text = DOC.read_text(encoding="utf-8")
    lines = text.split("\n")
    hits = [i for i, line in enumerate(lines) if line.startswith(PREFIX)]
    if len(hits) != 1:
        print(f"SETUP FAILED: expected exactly one {PREFIX!r} row, found {len(hits)} at {hits}")
        return 1
    index = hits[0]
    old = lines[index]
    if "gate-95b.log" not in old:
        print("SETUP FAILED: the row this script replaces is not the gate-95b one - re-read it before overwriting")
        return 1
    lines[index] = ROW

    after = "\n".join(lines)
    if after.count(ROW) != 1:
        print("POSTCONDITION FAILED: the new row is not present exactly once")
        return 1
    DOC.write_text(after, encoding="utf-8", newline="\n")
    print(f"row {index + 1} rewritten: {len(old)} -> {len(ROW)} chars")
    return 0


if __name__ == "__main__":
    sys.exit(main())
