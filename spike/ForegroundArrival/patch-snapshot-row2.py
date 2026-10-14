"""Rewrite the terminal-snapshot serial-gate row to the reading gate-92 printed (superseding the pre-green claim)."""
import pathlib

DOC = pathlib.Path("docs/astra/ROADMAP.md")
lines = DOC.read_text(encoding="utf-8").splitlines(keepends=True)
hits = [i for i, l in enumerate(lines) if l.startswith("| 串行闸口 |")]
assert len(hits) == 1, len(hits)

new = (
    "| 串行闸口 | **最新一次（`gate-92.log`，提交 `3cee913` 的树）：全绿** —— 套件 `已通过! 失败: 0，通过: 1589，"
    "总计: 1589，7 m 35 s` → 页级像素 `PASS 13 pages x 3 variants, 0 offender(s)` → 三档 `checked=True` → "
    "`keys.md is current: 1312 canonical lines.` → `known-gaps.md is current: 927 canonical lines.` → "
    "`All Astra gates passed.`，管道自己那一步的退出码 0。这一行的前四跑都不是全绿，形状留着别丢："
    "页闸红 9 条（`gate-88.log`）/ 套件红 Rating 1 条（`gate-88b.log`）/ 套件红 TeachingTip 1 条（`gate-88c.log`）/ "
    "页闸红 materials-HighContrast 2 条（`gate-91.log`），被点名的测点单跑都绿（`pixels-solo-1.log`、"
    "`solo-teachingtip.log` 74/74）——**一次全绿没有解释其中任何一条**：页闸槽宽在跑与跑之间 980/788 跳的机制仍未量，"
    "#47/#90 原样挂着（详见本节末"#92/#93 闸口补记"） | "
    "`tools/Test-AstraGates.ps1 -Configuration Debug`；日志按批留在 `spike/SystemColorProbe/gate-*.log` 与 "
    "`spike/ForegroundArrival/gate-*.log`（最新 `gate-91.log`、`gate-92.log`；单跑对照 `pixels-solo-1.log`、"
    "`solo-teachingtip.log`）；三档调色板与两份清单每次改动后各自 `-Check` 当场对过 |\n"
)
lines[hits[0]] = new
DOC.write_text("".join(lines), encoding="utf-8", newline="")
print("row", hits[0] + 1, "rewritten")
