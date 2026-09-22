# ToggleButton 审计（阶段 3 尾批）

上游依据：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`

- `controls/dev/CommonStyles/ToggleButton_themeresources.xaml` blob `868646a15b5a5062046b1edcc003f44288793b52`
  （37 行：Light/Default 分支 36 条别名 + `ToggleButtonBorderThemeThickness`）
- `controls/dev/CommonStyles/ToggleButton_themeresources_perf2026.xaml` blob `6fb99a6aaf28a8067ec1dbd078d38b507b479030`
  （同一批键，状态写成 `VisualState.Setters` 而非故事板）
- `dxaml/xcp/dxaml/themes/generic.xaml` blob `def26a6152487e366589660cd220509db6d4cf09`，
  `<Style TargetType="ToggleButton">` 在 **6171–6385 行**：一个 `ContentPresenter` 根 +
  一个 `VisualStateGroup` 的 **12 个状态**。

运行时：NuGet Jalium.UI **26.10.9**（`Jalium.UI.Controls.Primitives.ToggleButton`）。

一句话结论：**这条批次的价值不在样式，而在把两个"看不见的洞"变成断言**——
混合态格子此前只有键名证据（像素上根本不可能有证据），勾选/悬停三格此前少写一个属性
（于是三条上游行无人读），两者现在分别由探针用例与键消费闸口钉住。

## 0. 先量后写：这一批量到的运行时事实

**S1 · `{x:Null}` 触发条件在*样式*格子里也命中，并且可逆。**
Button 批留下的原话是"触发器要匹配 null 需要 `{x:Null}`，本运行时没有这项证据"
（`audits/button.md` Known Gap 8）。CheckBox 批证的是**模板**格子；样式格子走的是另一条
解析路径，所以不能拿它当证据。这次的探针（`A_null_checked_value_fires_the_indeterminate_cell`）
用一份只带 null 条件的临时样式，把混合态底色指到 `AccentFillColorDefaultBrush`——
控件上没有任何别的东西会读这条刷，于是"面变强调色"只有格子命中这一种解释；
再把 `IsChecked` 设回 `false`，底色回到休息刷，证明它不是一次性误命中。

**S2 · 混合态在像素上*天然*不可区分，这个问题本身要撤掉。**
上游把 `ToggleButtonBackgroundIndeterminate`（以及 `…PointerOver`/`…Pressed`/`…Disabled`）
映射到与休息位**同名**的调色板刷：`ControlFillColorDefault`/`Secondary`/`Tertiary`/`Disabled`，
前景与描边同理。也就是说混合态和未勾选态在上游就是同一个颜色，
`An_indeterminate_toggle_button_paints_the_resting_fill_not_the_accent` 那条像素断言
无论格子命不命中都会通过——它是**代理信号**，不是证据。
所以 button.md 里"混合条的像素归因仍未测"这句话要更正：不存在可测的像素差，
可测的是"格子生效"（S1 探针）与"实例接上"（S3 读回）。

**S3 · 三态读回的都是调色板实例，且后写的格子胜出。**
挂载后读 `toggle.Background/Foreground/BorderBrush`：休息 = `ToggleButtonBackground` 三件套，
`IsChecked=true` = `…Checked` 三件套，再 `IsEnabled=false` = `…CheckedDisabled` 三件套。
最后一步是有意义的：勾选与禁用两条格子同时成立，谁能落地取决于 reader 应用格子的次序，
实测**标记里后面的赢**——所以 §3 表里的顺序是语义，不是排版。
（读回前一律 `PixelHarness.Settle`：共享模板面带 `TransitionProperty`，
过渡中的 `Background` 是一个新插入的插值实例而不是调色板实例，见 `adaptation/12`。）

**S4 · 本地值压过样式格子，与上游相反。**
`Background` 设成本地刷 + `IsChecked=true` → 读回仍是本地实例。
上游用 VisualState 故事板驱动这三条属性，故事板的赋值压过本地值；
本运行时的样式格子在依赖属性优先级里排在本地值之下。
后果是可写的行为差异：应用给一个 `ToggleButton` 设了本地 `Background`，
在 WinUI 上勾选后变色，在我们这里**永远不变色**。已按实测固定为
`A_local_fill_keeps_the_surface_over_the_checked_cell`（这条账单从 TextBox 批的
"框架本地值压过样式"延伸到了我们自己的格子一侧）。

**S5 · 交互通路：框架的自动化模式就是真实点击通路。**
`Jalium.UI.Automation.Peers.ToggleButtonAutomationPeer`（public）+
`Jalium.UI.Automation.Provider.IToggleProvider.Toggle()` 驱动 `OnClick → OnToggle`，
不需要合成指针事件。实测：两态 `false→true→false`，三态 `false→true→null→false`，
`Checked/Unchecked/Indeterminate` 各按次触发；禁用时抛
`InvalidOperationException("Cannot toggle a disabled control.")`——与 WinUI 该模式
在禁用元素上抛 `ElementNotEnabledException` 是同一侧契约（不是静默忽略，这点必须记下来，
否则应用会以为换个写法就能吞掉）。

**S6 · `new ToggleButton()` 的休息位是 `false`，不是 `null`。**
单看颜色看不出差别（S2），所以这条只能靠断言固定：哪天框架改成 null 起步，
屏上每个两态开关都成了混合态而没人会注意到。

**S7 · 上游的 `RepeatButtonBorderThemeThickness` 是一条死行。**
整棵参考树里 `{ThemeResource RepeatButtonBorderThemeThickness}` 只出现在两个
资源加载测试夹具（`genericincomplete.xaml`、`genericred.xaml`），
`generic.xaml` 的 RepeatButton 样式读的是 `ButtonBorderThemeThickness`。
我们此前把这条行抄了进来，等于公开一个"覆盖后什么都不会变"的名字，
故删除并在文件头写清理由（与 AutoSuggestBox 那 6 条无上游消费点的行同一个处置）。
对照：`ToggleButtonBorderThemeThickness`（7 处引用）与
`HyperlinkButtonBorderThemeThickness`（同）都是真消费点，已接上。

**S8 · 三条 hover 前景行此前无人读。**
上游 `PointerOver`/`CheckedPointerOver`/`IndeterminatePointerOver` 每个状态都写
Background + BorderBrush + **Foreground**，我们的格子只写了前两个，
于是 `ToggleButtonForeground{,Checked,Indeterminate}PointerOver` 三行成了死行。
把 Button 全族四份字典纳入键消费闸口后立刻暴露，一并补齐
（同批还有 `Button`/`AccentButton`/`SubtleButton`/`RepeatButton` 的
`…ForegroundPointerOver` 四行同因）。

## 1. 资源行：上游 37 行，抄 37 行，消费 37 行

| | 行数 | 说明 |
|---|---|---|
| 逐字照抄 | 29 | 别名直接指向上游点名的调色板刷 |
| 值偏差 | 7 | 同 button.md 两类老原因：`…ForegroundCheckedDisabled` 的 Color→Brush（本运行时没有可用的 Color→Foreground 强制转换），以及 elevation 渐变描边改实底（`{ThemeResource}` 写进渐变停靠点会在解析期冻结，撑不住就地改色模型）；上游 HighContrast 分支自己也是实底 |
| 度量行 | 1 | `ToggleButtonBorderThemeThickness`，`Thickness=1`，本批接上消费点 |

闸口侧的变化：`Transcribed_control_rows_are_read_by_a_template` 从 8 份字典扩到 **12 份**，
新增 `Button`/`ToggleButton`/`RepeatButton`/`HyperlinkButton`。
`TitleBar.jalxaml` 与 `FlyoutPresenter.jalxaml` 仍然**故意不收**：
它们的读者是框架自己的窗口外壳与弹层样式，不是我们Assembly里的模板，
覆盖有效但没有消费点可查（证据在 `audits/window-shell.md`、`audits/flyout-presenter.md`）。

## 2. 结构处置：模板复用，格子放在样式里

- **模板**沿用 `ButtonLayoutStyle` 的共享模板（`Grid` → `Surface` Border → `ContentPresenter`，
  外加 2026-09-22 之前的自绘 `FocusOutline`，该环现已搬到 `FocusVisualStyle` 上），不复制上游的单 `ContentPresenter` 根。
  这是形状差异而非颜色差异，记在 §5 第 2 条。
- **状态格子放在样式而不是模板**：上游 12 态在模板的 VSM 里。放在样式侧的代价是 S4
  （与上游的优先序相反），收益是自带模板的应用仍然能拿到状态格子上游键名。
  本库其余控件（CheckBox/RadioButton/ComboBox/Slider）的格子在模板里，
  因为它们要动的是部件（勾选面、圆点、弹层）；ToggleButton 只动控件自身的三条属性，
  两种写法在结果上等价，优先序上不等价。
- **字面量**：`FontSize=14`、`MinHeight=32`、`MinWidth=0`（后两条是我们自加的，
  上游样式没有，沿用 button.md Known Gap 6 的记账）。`Padding` 与上游同源
  （`{StaticResource ButtonPadding}`）。

## 3. WinUI 视觉状态 → 本格子（12 态全表）

上游 12 态全在**一个** `CommonStates` 组里，除 `Normal` 外每态都写同一组三属性；
`Normal` 只有 `PointerUpThemeAnimation`。

| 上游状态 | 我们的条件 | Background | Foreground | BorderBrush | 主题动画 |
|---|---|---|---|---|---|
| Normal | （休息 setter） | `ToggleButtonBackground` | `ToggleButtonForeground` | `ToggleButtonBorderBrush` | PointerUp ✗ |
| PointerOver | `IsMouseOver=True` | `…PointerOver` | `…PointerOver` ← 本批补 | `…PointerOver` | PointerUp ✗ |
| Pressed | `IsPressed=True` | `…Pressed` | `…Pressed` | `…Pressed` | PointerDown ✗ |
| Disabled | `IsEnabled=False` | `…Disabled` | `…Disabled` | `…Disabled` | — |
| Checked | `IsChecked=True` | `…Checked` | `…Checked` | `…Checked` | PointerUp ✗ |
| CheckedPointerOver | `IsChecked=True` + `IsMouseOver=True` | `…CheckedPointerOver` | `…CheckedPointerOver` ← 本批补 | `…CheckedPointerOver` | PointerUp ✗ |
| CheckedPressed | `IsChecked=True` + `IsPressed=True` | `…CheckedPressed` | `…CheckedPressed` | `…CheckedPressed` | PointerDown ✗ |
| CheckedDisabled | `IsChecked=True` + `IsEnabled=False` | `…CheckedDisabled` | `…CheckedDisabled` | `…CheckedDisabled` | — |
| Indeterminate | `IsChecked={x:Null}` | `…Indeterminate` | `…Indeterminate` | `…Indeterminate` | PointerUp ✗ |
| IndeterminatePointerOver | `IsChecked={x:Null}` + `IsMouseOver=True` | `…IndeterminatePointerOver` | `…IndeterminatePointerOver` ← 本批补 | `…IndeterminatePointerOver` | PointerUp ✗ |
| IndeterminatePressed | `IsChecked={x:Null}` + `IsPressed=True` | `…IndeterminatePressed` | `…IndeterminatePressed` | `…IndeterminatePressed` | PointerDown ✗ |
| IndeterminateDisabled | `IsChecked={x:Null}` + `IsEnabled=False` | `…IndeterminateDisabled` | `…IndeterminateDisabled` | `…IndeterminateDisabled` | — |

11 格 × 3 属性 = 33 个消费点，全部由 `The_toggle_style_carries_one_cell_per_upstream_state`
按条件顺序 + 逐属性键名断言；`Every_cell_names_a_toggle_row_and_never_a_palette_brush`
再断一遍"格子里不许直接写调色板键"。

## 4. 四类证据

**构建**：`tools/Test-AstraGates.ps1` 串行 restore→build→test→调色板漂移，
**294/294 全绿，0 警告，0 skip**。用例数变化：+22（`AstraToggleButtonTests`）、
+4（闸口收录四份按钮族字典）、−1（`AstraSelectionTests` 里那份 indeterminate 键名表
是新表格的真子集，删掉并在保留项上写明为什么留）。

**行为**（`AstraToggleButtonTests`，全部在已上屏 host 里建树/推帧后读回）：
格子全表、三条实例读回、勾选+禁用的胜出者、`{x:Null}` 探针（含可逆）、
休息位是 `false`、两态/三态点击循环 + 三个事件、禁用抛错、本地值优先序、
共享焦点框在 ToggleButton 上也亮、自有 `BorderThickness` 行。
交互驱动用框架的 `IToggleProvider.Toggle()`（S5），**不是**合成指针。

**视觉**：休息底（`ControlFillColorDefault` 哨兵 >6000 px，稳定捕获）、
混合态探针底（强调色哨兵 >6000 px，且设回 false 后为 0 px）、
Light↔Dark 静息面必须不同、两态下品牌绿 `#207245` 均 0 px。
勾选态像素沿用 `AstraButtonTests.A_checked_toggle_and_a_resting_repeat_button_paint_their_upstream_fills`，
不重复录一份。

**硬件输入**：**本批没有新增**。真指针读数仍只有 Button 批那一次性的悬停
（`audits/button-input-raw.txt` run 5，依赖物理鼠标且没人同时动鼠标，不进闸口）。
`Pressed` 与键盘/触摸路径仍只有结构证据；`press` 模式会向桌面真按左键，未经同意不运行。
Task #13（真指针像素通路）继续欠着。

**Gallery**：Buttons 页的家族卡此前只有"静置 + 勾选"两个开关，没有三态示例，
也没有把"混合态在像素上等于休息态"这件事写在页面上。本批补一个三态开关
（`IsThreeState` + 代码里设 null，理由见 button.md Known Gap 8 的标记侧实测：
`IsChecked="{x:Null}"` 写在标记里会得到 `false`），并把该卡 parity 从 `audited`
维持住、欠账逐条进 `Catalog.json`。

## 5. 不声称清单（Known Gaps）

1. **混合态没有独立像素判据**：上游把它映射成与休息位同色的刷（S2），
   所以"混合态看起来对不对"这句话我们只能说"格子命中了、实例接上了"，
   不能说"像素证明了它是混合态"。
2. 模板形状不同：上游单 `ContentPresenter` 根，我们 `Grid + Surface Border + ContentPresenter`。
   颜色一致，边框/内容排布的逐位关系不声称。
3. **样式格子 vs 本地值的优先序与上游相反**（S4）：这是行为差异，不是视觉差异。
4. `PointerUpThemeAnimation`/`PointerDownThemeAnimation` 全部缺失（12 态里 8 态带）。
5. 共享模板上的 `TransitionProperty="Background, BorderBrush"` 83ms 是我们自加的，
   上游状态换刷是离散的（`DiscreteObjectKeyFrame KeyTime="0"`）；
   既是 parity 偏差也是"刚换状态时读到的不是调色板实例"的身份风险（动效批处理）。
6. `BackgroundSizing=OuterBorderEdge` 无对应属性。
7. elevation 描边（4 行）用实底近似上游 3DIP 渐变，顶/底边不会分层。
8. 自绘环 ≠ 上游系统焦点框 + `FocusVisualMargin=-3`：2026-09-22 起环挂在 `FocusVisualStyle` 上、由框架 `ShowFocusCues` 门决定出不出（`audits/focus-visual.md`），形状与两枚令牌未变。
9. `Pressed`、键盘（空格/回车）、触摸三条输入通路的像素未证（§4 硬件输入）。
10. 文字色仍走"建树 + 读回实例"，不能用像素断（Button 批第 9 条框架限制）。
11. 上屏逐位一致未证；高对比逐键断言、减动效键化属于并行工作流，未在本批。
