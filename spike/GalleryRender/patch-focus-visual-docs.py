"""Point the six audits that describe the old template ring at the new focus-visual route.

Each edit is an exact, single-occurrence swap. The upstream raw dumps under adaptation/ are reference
material, not our claims, and stay untouched.
"""

import pathlib
import sys

root = pathlib.Path(__file__).resolve().parents[2]
audits = root / "docs" / "astra" / "audits"

EDITS = [
    ("button.md",
     "| 焦点框 | 模板里的 `FocusOutline` Border + `Trigger IsKeyboardFocused=True`（我们自绘，因为系统焦点框没有对应属性） | `FocusStrokeColorOuterBrush`/`Inner` |",
     "| 焦点框 | 2026-09-22 改：样式里 `FocusVisualStyle={ThemeResource FocusVisualRingStyle}`，环本体在 `Styles/FocusVisuals.jalxaml`。原来是模板里的 `FocusOutline` Border + `Trigger IsKeyboardFocused=True`——那一格鼠标点也满足，所以点击出环（`audits/focus-visual.md`） | `FocusStrokeColorOuterBrush`/`Inner`（两枚令牌未变） |"),

    ("button.md",
     "5. 系统焦点框缺失，我们自绘的 `FocusOutline` 与上游 `FocusVisualMargin=-3` 的框不逐位一致。",
     "5. 系统焦点框的属性缺失，我们把自绘的环交给框架自己的焦点视觉（`FocusVisualStyle`，门是 `FocusVisualManager.ShowFocusCues`），偏移进环的模板，因为本运行时不公开 `FocusVisualMargin`。与上游 `FocusVisualMargin=-3` 的框仍不逐位一致。（2026-09-22 改，`audits/focus-visual.md`）"),

    ("checkbox-radiobutton.md",
     "| 焦点框 | `Trigger IsKeyboardFocused=True` → `CheckFocus.Opacity` | `FocusStrokeColorOuterBrush` |",
     "| 焦点框 | 2026-09-22 改：样式里 `FocusVisualStyle={ThemeResource FocusVisualCheckStyle}`（偏移 `-2,1` 搬进环的模板），出不出由框架的 `ShowFocusCues` 决定。原来是 `Trigger IsKeyboardFocused=True` → `CheckFocus.Opacity`，鼠标点同样抬（`audits/focus-visual.md`） | `FocusStrokeColorOuterBrush` |"),

    ("expander.md",
     "| 焦点 | 控件级 `IsKeyboardFocused` 驱动 `FocusOutline`（上游靠头按钮自身） |",
     "| 焦点 | 控件级 `FocusVisualStyle={ThemeResource FocusVisualRingStyle}`，门是框架的 `ShowFocusCues`（上游靠头按钮自身）。2026-09-22 改，原来是控件级 `IsKeyboardFocused` 驱动 `FocusOutline`，鼠标点同样抬（`audits/focus-visual.md`） |"),

    ("slider.md",
     "所以滑块的视觉半边本批**没有**像素证据，只有上面的排布读数；留给任务 #13 的滚动/直挂通路。`SliderFocus` 的落点仍与上游的\n`FocusVisualMargin=\"-14,-6,-14,-6\"` 不同形（我们是模板内一圈描边）。",
     "所以滑块的视觉半边本批**没有**像素证据，只有上面的排布读数；留给任务 #13 的滚动/直挂通路。`SliderFocus` 部件已经不在了（2026-09-22 起环挂在 `FocusVisualStyle` 上，竖排由 `Orientation` 那一格换成 `FocusVisualSliderVerticalStyle`，见 `audits/focus-visual.md`），它的落点仍与上游的\n`FocusVisualMargin=\"-14,-6,-14,-6\"` 不同形（我们是环自己模板里的一圈描边）。"),

    ("splitbutton.md",
     "| `UseSystemFocusVisuals=True` + `FocusVisualMargin=-1` | 根上的自绘 `FocusOutline`（`Grid.ColumnSpan=3`） | 本运行时无系统焦点框。焦点落在控件上而非半区（两个半区 `IsTabStop=False`），所以环属于根；判据只能取根的那一个，因为按名字找到的第一个 `FocusOutline` 属于主半区（测试里用 `RootRing`） |",
     "| `UseSystemFocusVisuals=True` + `FocusVisualMargin=-1` | 根上的 `FocusVisualStyle={ThemeResource FocusVisualRingStyle}`（环本体在 `Styles/FocusVisuals.jalxaml`） | 本运行时不公开系统焦点框的属性，但公开它的门：`FocusVisualManager.ShowFocusCues`。焦点落在控件上而非半区（两个半区 `IsTabStop=False`），所以环属于根。2026-09-22 改：以前那是根上的自绘 `FocusOutline`（`Grid.ColumnSpan=3`），按名字找到的第一个属于主半区、测试得用 `RootRing()` 绕开，部件与助手都已删（`audits/focus-visual.md`） |"),

    ("togglebutton.md",
     "  外加自绘 `FocusOutline`），不复制上游的单 `ContentPresenter` 根。",
     "  外加 2026-09-22 之前的自绘 `FocusOutline`，该环现已搬到 `FocusVisualStyle` 上），不复制上游的单 `ContentPresenter` 根。"),

    ("togglebutton.md",
     "8. 自绘 `FocusOutline` ≠ 上游系统焦点框 + `FocusVisualMargin=-3`。",
     "8. 自绘环 ≠ 上游系统焦点框 + `FocusVisualMargin=-3`：2026-09-22 起环挂在 `FocusVisualStyle` 上、由框架 `ShowFocusCues` 门决定出不出（`audits/focus-visual.md`），形状与两枚令牌未变。"),
]

failures = []
for name, old, new in EDITS:
    path = audits / name
    text = path.read_text(encoding="utf-8")
    hits = text.count(old)
    if hits != 1:
        failures.append(f"{name}: {hits} occurrences of {old[:40]!r}")
        continue
    path.write_text(text.replace(old, new), encoding="utf-8", newline="")
    print(f"{name}: patched {old[:36]!r}...")

if failures:
    print("ABORTED, nothing further written:", file=sys.stderr)
    for line in failures:
        print("  " + line, file=sys.stderr)
    sys.exit(1)
