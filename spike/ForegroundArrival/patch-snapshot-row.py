"""Replace the terminal-snapshot serial-gate row with the readings gate-91 actually printed."""

import io
import pathlib

DOC = pathlib.Path("docs/astra/ROADMAP.md")
text = DOC.read_text(encoding="utf-8")
lines = text.splitlines(keepends=True)
hits = [i for i, l in enumerate(lines) if l.startswith("| 串行闸口 | 已打印的部分")]
assert len(hits) == 1, f"expected exactly one row, saw {len(hits)}"

new = (
    "| 串行闸口 | 已打印的部分：build `0 警告 0 错误`；套件这一档打印过 `1585 / 1585，0 失败 0 跳过`"
    "（`gate-88.log`），#91 加 2 条之后第一次顺序跑过整档：`已通过! 失败: 0，通过: 1587，总计: 1587`"
    "（`gate-91.log`）；**但同一跑的页级像素仍红**——`FAIL 13 pages x 3 variants, 2 offender(s)`，"
    "两条都在 `materials HighContrast`（`the slot was not actually emptied (337249 lit pixels left)` 与 "
    "`printed nothing its empty slot does not already print (41 colours against 93)`），闸口就此停住，"
    "调色板与两份清单没跑到。四跑合起来的形状：红在页闸 9 条 / 套件 Rating 1 条 / 套件 TeachingTip 1 条 / "
    "页闸 2 条，被点名的测点单跑都绿（`pixels-solo-1.log`、`solo-teachingtip.log` 74/74），"
    "**至今没有一次全绿的顺序读数**，机制仍未量（#47/#90） | "
    "`tools/Test-AstraGates.ps1 -Configuration Debug`；测点数随批变，日志按批留在 "
    "`spike/SystemColorProbe/gate-*.log` 与 `spike/ForegroundArrival/gate-*.log`（最新四跑 `gate-88.log`、"
    "`gate-88b.log`、`gate-88c.log`、`gate-91.log`；单跑对照 `pixels-solo-1.log`、`solo-teachingtip.log`）；"
    "三档调色板与两份清单本批各自 `-Check` 当场对过：`checked=True` ×3、`keys.md is current: 1312 canonical lines.`、"
    "`known-gaps.md is current: 910 canonical lines.` |\n"
)
lines[hits[0]] = new
DOC.write_text("".join(lines), encoding="utf-8", newline="")
print("row replaced at line", hits[0] + 1)
