# 命令栏族审计（阶段 4 第三段）

上游：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
- `controls/dev/CommonStyles/AppBarButton_themeresources.xaml`，blob `60bcf2af4daf2ab40dd79709d3a0a7907f79b8c3`（Light 分支 25 条别名行）
- `controls/dev/CommonStyles/AppBarToggleButton_themeresources.xaml`，blob `2c3e6c5ad5aa3d6c4cd6fe1e3c8c5688b8e31f18`（Light 分支 51 条）
- `controls/dev/CommonStyles/AppBarSeparator_themeresources.xaml`，blob `51801458b679a563d21471423f4ebb94f0b13ad0`（Light 分支 1 条）
- `controls/dev/CommonStyles/CommandBar_themeresources.xaml`，blob `f524c6d543ea735b7b4e833294891eec448b8b5f`（Light 分支 9 条）
- 四份文件的模板都在同名 `.xaml`（不在 `Generic`）里：`AppBarButton.xaml` 的 `Root`/`ContentViewbox`/`Content`/`LabelText`、`AppBarSeparator.xaml` 的 `RootGrid`/`SeparatorRectangle`——本批照抄名字，但下面 0.8 说明这些名字在本运行时**不是**契约。
- 上游合计 **86 条 Light 别名行**（25+51+1+9），另有分支外的 `Thickness` / `x:Double` 度量若干。
- 仍欠一条：`SplitButton_themeresources.xaml`（blob `e9601d1f`）的 `SplitButtonInAppBarUnfocusedPointerOver`（:34/:66/:98）只被 `SplitButtonCommandBarStyle`（:188/:195/:201/:208）消费，那条 CommandBar 作用域的 SplitButton 样式本批没做，行继续按 SplitButton 批的反向闸口按住。

运行时：NuGet Jalium.UI **26.10.9**。测量：`spike/AppBarProbe`（pass 1-5，原始日志 `appbar-probe1..5.txt` 全部留在原处）。

## 0. 先量后写：pass 1-5 量到的事实

1. **框架自己就带这一族的外观**：`AppBarButton` / `AppBarToggleButton` / `AppBarSeparator` 有出厂隐式样式，`CommandBar` 由代码从**它自己的 9 条行**上色——`CommandBarBackground` `#2C2C2E`、`CommandBarBorderBrush` `#48484A`、`CommandBarOverflowBackground` `#3A3A3C`、`AppBarButtonBackground` `#001C1C1E`、`…BackgroundHover` `#3A3A3C`、`…BackgroundPressed` `#48484A`、`AppBarButtonForeground` **#680081（强调派生的紫）**、`…ForegroundDisabled` `#673B71`、`AppBarSeparatorForeground` `#48484A`（pass 4 段 G 按 `ReferenceEquals` 读回，不是猜色）。
2. **行遮蔽成立**（ModernWpf 的主方法，本批第一次成为主杠杆）：我们字典在框架之后合并，同名行对**合并之后建造的**每个控件都是我们那条；之前建造的保留宿主实例。pass 3 用 `ReferenceEquals` 读回。
3. **我们的隐式 `Style` 直接赢过框架同 `TargetType` 的隐式样式**（pass 3：模板对象身份即我们的那份）。
4. **能不能重模板又是分类型的**：三种 app-bar 元素都实例化我们给的模板；**`CommandBar : Control` 存下 `Template` 却从不建**（代码自建 `StackPanel` + `Button` "⋯" + `Popup`，与 `MenuItem` 同案），所以它的表面、more 按钮（40×32）与溢出弹层只跟着它代码读的那几行走。`AppBarSeparator` 没有框架模板（`OnRender` + `ResolveSeparatorBrush` 自绘），但会建我们给的。
5. **没有视觉状态管理器**（pass 5 两条独立证据）：`Jalium.UI.Controls.VisualStateManager` 类型在所有已加载程序集里**不存在**；带 `VisualStateManager.VisualStateGroups` 的模板装进已上屏窗口实例化时抛 `XamlParseException: Property 'VisualStateGroups' not found on type 'Grid'`，而**同一份 markup 单独 `XamlReader.Parse` 返回 ok**（模板惰性解析）。→ "解析通过"不是能力证据。
6. **本运行时的一条方法教训（pass 4 的三条结论因此作废）**：`XamlReader.Parse` 出来的临时模板，其 `ControlTemplate.Triggers` **一次都没有生效**——pass 4 的 A/B/C 与 pass 5 的 B/C/D 在字面值部件、`TemplateBinding` 部件、单条件、`MultiTrigger` 四种写法下全部读回静置色，而同一状态在**已编译的 `.jalxaml` 样式**里都能读回。于是状态普查只能走编译字典（`AstraAppBarTests`），临时模板只能读类型面与像素。作废的两条：① pass 4 "合成 MouseUp 不清 `IsPressed`"（pass 5 在真模板上实测 `False`）；② pass 4 写的 `LabelPosition` 行为（下面 0.7）。
7. **状态面实测**（pass 5 A + 本批行为用例）：`IsPressed` 进（`#06000000`）出（`#09000000`）正常；`IsMouseOver` **没有公开 setter、也没有可读的 readonly key**（pass 4/5 都是 `MissingFieldException: UIElement.IsMouseOverPropertyKey`），但一次路由 `MouseDown`+`MouseUp` 之后它自己变 `True`，于是 hover 行**能**落到像素——缺的是"指针移动进/出"这条循环，不是这一格；`IsCompact=True` 撤标签并把内框换成 `2,6,2,22`；`AppBarToggleButton.IsChecked=True` 上强调底。`CommandBarLabelPosition` 上游与本机都只有 `Default`/`Collapsed`（`controls2.idl:100`），两者单独都不动任何东西；上游的并排标签来自 bar 的 `CommandBarDefaultLabelPosition`（本机 `Bottom`/`Right`/`Collapsed`，与 `controls2.idl:83` 相同），而 bar 不建模板 → `LabelOnRight` 一族无处可挂。
8. **部件名不读**：四个类型都没有 `OnApplyTemplate` 覆写、也没有任何命名子元素查找（pass 1），所以 `Root`/`ContentViewbox`/`Content`/`LabelText`/`RootGrid`/`SeparatorRectangle` 全是**上游对齐**；正反对测都在（改名照样工作、名字齐全）。对比 `Expander` 的三个名字与 SplitButton 的两半区名——那才是契约。
9. **宿主几何不是上游几何**：框架 app-bar 按钮实测 44 宽、随窗拉伸、10px 标签、20×20 图标盒；上游 68 宽、12px 标签、16 高内容盒、bar 48 高。这是本批除了颜色之外还要出模板的理由。

## 1. 行去向表（上游 86 条 Light 别名行 → 本实现 31 条别名 + 4 条 Thickness）

| 上游组 | 上游条数 | 落到 | 未落地的原因（测量出处） |
| --- | --- | --- | --- |
| AppBarButton Background/Foreground/BorderBrush 的 rest·hover·pressed·disabled | 12 | `ThemeResources/AppBar.jalxaml` 12 条 | — |
| AppBarButton `*KeyboardAcceleratorTextForeground*` | 4 | 不发布 | 类型上只有 `Icon`/`Label`/`LabelPosition`/`IsCompact`/`DynamicOverflowOrder`（pass 1 反射） |
| AppBarButton `*SubItemChevronForeground*` | 5 | 不发布 | 没有子菜单也没有 `Flyout` 属性 |
| AppBarButton `*SubMenuOpened*` | 4 | 不发布 | 没有 `IsSubMenuOpen` 之类可读属性 |
| AppBarToggleButton 基色 12 + `*Checked` 三条 | 15 | 15 条（`BorderBrushChecked` 见第 2 节） | — |
| AppBarToggleButton `*CheckedPointerOver/Pressed/Disabled*` | 9 | 不发布 | 一格只盯一个条件，`IsChecked AND IsMouseOver` 写不出来（pass 5 的 `MultiTrigger` 读数因 0.6 作废，按属性面判定） |
| AppBarToggleButton `*HighLightOverlay*` | 5 | 不发布 | 同上，且覆盖层属溢出布局 |
| AppBarToggleButton `*CheckGlyphForeground*` | 8 | 不发布 | 运行时不在 bar 里暴露勾选字形，且无 overflow 标志 |
| AppBarToggleButton `*OverflowLabelForeground*` | 6 | 不发布 | 没有可选中溢出布局的标志 |
| AppBarToggleButton `*KeyboardAcceleratorTextForeground*` | 8 | 不发布 | 同 AppBarButton |
| AppBarSeparatorForeground | 1 | 1 条 | — |
| CommandBar `CommandBarBackground` / `CommandBarForeground` | 2 | 2 条 | — |
| CommandBar 其余（`…BackgroundOpen`、`…BorderBrushOpen`、`…HighContrastBorder`、`…EllipsisIconForegroundDisabled`、`…LightDismissOverlayBackground`、两条 overflow-presenter） | 7 | 不发布 | 全由代码自绘（pass 4 段 F：改遍可达行都不动 bar 的几何、more 按钮与弹层） |
| `AppBarButtonWidth`=68 / `AppBarButtonContentHeight` / `AppBarThemeMinHeight`=64 / `AppBarThemeCompactHeight`=48 / `AppBarSeparatorWidth`=1 / `AppBarSeparatorCornerRadius`=0.5 / chevron 字号与边距 | 度量 | 4 条 `Thickness` + 字面量 | `x:Double` 本 reader 解析不了；`2,6,2,6`、`2,6,2,22`、`2,0,2,8`、`2,8,2,8` 是上游分支外原值 |

算术核对：30 条逐字照抄 + 56 条不发布 = 86；发布表里第 31 条 `CommandBarBorderBrush` 是**宿主名**（上游没有这行，上游叫 `CommandBarBorderBrushOpen`），因为 bar 表面是本批唯一还能碰到的 CommandBar 像素面。
哨兵实验一处（本批自我更正的一条）：`CommandBarOverflowBackground` 第一版发布过，判据写成"覆盖其背后令牌后，开放态 bar 在整宿主窗口捕获里 0 个哨兵像素"。这条判据**本身不可判**——同一断言在本类内跑通过、在全量套件里以 9088 个哨兵像素失败，因为宿主窗口是共享的，另一个类留开的 acrylic 弹层自己就会把画面染绿。改写后的读法用差分（两帧都带同一覆盖，别的读者对两帧贡献相同）加控件自己的 `Popup` 状态，结论也比原先更窄也更硬：**设 `IsOpen` 只改这个属性，bar 的 `Popup` 在进程内始终不开**，所以那 7 条开放态行不是"我们测了它不动"，而是"那一面根本没显示过"，一条从未显示的面没有可承诺的像素（pass 4 段 F 早就看到 `Popup popup(open=False)` 与逐字节相同的捕获）。

## 2. 值/名替换与宿主替换清单

| WinUI | 本实现 | 原因 |
| --- | --- | --- |
| `AppBarToggleButtonBorderBrushChecked` → `AccentControlElevationBorderBrush` | → `ControlStrokeColorOnAccentDefaultBrush` | 上游那条是 3 DIP 绝对渐变，本管线发不出去（与强调按钮描边同一先例，`ThemeResources/Button.jalxaml`） |
| `CommandBarBorderBrush`（宿主名） | 指向 `CardStrokeColorDefaultSolidBrush` | 上游 9 条里没有这个名字；bar 的描边只有这一条路能碰 |
| `AppBarButton` 可重模板 + 行遮蔽 | 两者都用 | 0.2/0.3：模板拿回几何，行拿回 `CommandBar` 的表面 |
| `CommandBar` 可重模板 | 只上色 + 只给几何 | 0.4：存而不建 |
| `AppBarSeparator` 的 `Rectangle`（上游也是） | 同名同形 | 框架自绘路径仍在，见 Known Gaps 4 |
| `FontIcon` 勾选字形（overflow） | 无 | 无 overflow 标志可挂 |
| `AppBarThemeCompactHeight`=48 等 | 不落地 | `x:Double`；bar 只写 `MinHeight=48` 字面量 |

## 3. 状态映射（WinUI VisualState → ControlTemplate.Triggers）

| 上游组/状态 | 本实现格子 | 写入的行 |
| --- | --- | --- |
| CommonStates / PointerOver | `IsMouseOver=True` | Background/BorderBrush/LabelText 前景的 `*PointerOver` |
| CommonStates / Pressed | `IsPressed=True` | `*Pressed` 三条 |
| CommonStates / Disabled | `IsEnabled=False` | `*Disabled` 三条（排在最后，赢过前面所有格） |
| CheckStates / Checked（toggle） | `IsChecked=True` | `*Checked` 三条（单条件，所以 checked×hover 的强调变体无处可写） |
| ApplicationViewStates / Compact | `IsCompact=True` | 标签 `Visibility=Collapsed` + 内框 `2,6,2,22` |
| ApplicationViewStates / LabelOnRight、LabelOnLeft | 无 | —（0.7：属性在 bar 上，bar 不建模板） |
| DisplayModeStates / AvailableCommandsStates / DynamicOverflowStates | 无 | —（0.4：`CommandBar` 不实例化模板，`IsOpen` 无处可看） |
| AppBarSeparator（上游无状态组） | 0 格 | — |

格子顺序即优先级：hover → press → checked → disabled（`AstraAppBarTests.Each_cell_writes_the_rows_upstream_writes_in_that_state` 逐格按键名核对）。

## 4. 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行（restore → build → test → 调色板漂移）；`ThemeResources/AppBar.jalxaml` 与 `Styles/AppBar.jalxaml` 已登记进 `Themes/Manifest.txt`（32 本）。
- **行为/资源**：`tests/FluentJalium.Tests/AstraAppBarTests.cs` **100 条** —— 31 条别名同一性、4 条 Thickness 数值、29 条未发布行、2 条"宿主行仍归框架"、可重模板类型逐型（含 `Style==null` + 模板对象身份）、`CommandBar` 存而不建、bar 持有它自绘读的那几条行、开放态溢出对未发布行不动像素、可达状态有格 / 不可达状态无格（4/5/0）、逐格写的行名、按下与释放与禁用与勾选与紧凑与 hover 的行为、命令恰好一次、peer（`IInvokeProvider` 与 `IToggleProvider` 两种答案）、改名照样工作、部件名齐全、bar 宿主条目走我们的模板。
- **视觉**：同文件像素用例 —— 我们样式赢过框架调色板、勾选把强调令牌送进像素、分隔线只画自己那 1 列（20-200 px）、整族跟随 Light↔Dark、bar 表面是上游的透明那条而非框架灰。**按下与悬停的颜色读数用 `ColorOf(...)` 而不是实例身份**（根 `Border` 带 83ms 刷过渡，飞行中是插值出的新实例——`PixelHarness` 的既有注记）。文字不进像素断言（纯文字主体在离屏通路上写 0 像素）。
- **硬件输入**：**本批为零**。全部驱动仍在进程内（`RaiseEvent(MouseDown/MouseUp)`、`IsChecked`、`IsCompact`、peer）；没有一条真指针/触摸/键盘路径被证明，`IsMouseOver` 也无法从外部写入（任务 13）。

## 5. Known Gaps

1. `CommandBar` 只拿到表面色 + 样式几何：它的 `StackPanel`、40×32 more 按钮与溢出 `Popup` 都是代码自绘；更根本的是 **`IsOpen=true` 在进程内不打开那个 Popup**（pass 4 段 F 与本批差分读数是同一结论），所以开放态既不可样式化也无从"测它跟不跟随某行"，7 条相关行不发布。真指针点 more 按钮之后是否就正常了，属于任务 #13 未证的部分。
2. 溢出（secondary commands）布局不做：上游溢出列表是 48 高行 + 标签并排 + 勾选字形，本运行时没有可选中这套布局的标志，所以次级命令在弹层里保持 bar 几何。
3. `LabelOnRight` 一族不落地：属性在 bar 上而 bar 不建模板（0.7）。
4. `AppBarSeparator` 有两条画线路径：框架在 `OnRender` 里读自己的行自绘，同时会建我们的模板；捕获读到的是我们那条色，但"宿主某版本只自绘不建模板"时屏幕会是框架灰——无法从外部证明不会发生。
5. 悬停只到"路由按下之后 `IsMouseOver=True` 且 hover 行进像素"，真指针的进入/离开循环未测；按下同样是合成事件，非硬件输入（任务 13）。
6. `SplitButton` 在 bar 里仍用非 app-bar 样式：`SplitButtonCommandBarStyle` 与 `SplitButtonInAppBarUnfocusedPointerOver` 本批欠着。
7. 4 条 `Thickness` 之外的度量全是字面量（68 宽、64/48 内容带高、48 bar 高、0.5 圆角、12/16 字号与图标盒），因为 `x:Double` 资源本 reader 解析不了。`AppBarThemeMinHeight`=64 与 `AppBarThemeCompactHeight`=48 是 `x:Double` 行，只能以字面量落在 `ContentRoot.MinHeight` 上。
8. 本批把"临时模板的格子永不生效"（0.6）记成方法约束：任何状态结论都必须来自已编译字典或反射类型面，`XamlReader.Parse` 的探针模板只能读结构与像素。这条同时使 pass 4 关于 `MultiTrigger` "能解析但从未观察到生效" 的读数**不能**当作"复合状态写不出来"的证据——本批按属性面（只读 DP/无对应属性）判定，而不是按那条读数。

## 6. 间距批（2026-09-19）：把条目改成上游的三层形状

用户反馈"间距不合 Fluent"之后，命令栏是第一批量出来的偏差。上游 `AppBarButton_themeresources.xaml`
（`controls/dev/CommonStyles/`，@19e3bdc3c）第 351–369 行的形状是三层：`Root` 网格无填充、
`AppBarButtonInnerBorder` 是带 `Margin=AppBarButtonInnerBorderMargin`（2,6,2,6）的独立高亮层、
`ContentRoot` 网格带 `MinHeight={ThemeResource AppBarThemeMinHeight}`（64），图标 `Viewbox` 只有
`Height=16` 且带 `AppBarButtonContentViewboxCollapsedMargin`（0,16,0,2），标签在第二行。

| 上游 | 本批前 | 本批后 |
| --- | --- | --- |
| `ContentRoot.MinHeight` = 64（`AppBarThemeMinHeight`） | 样式 setter `MinHeight=40` | `ContentRoot.MinHeight="64"`（字面量，x:Double 行读不出） |
| 高亮层 `Margin` 2,6,2,6（与内容同级的兄弟 Border） | 把 2,6,2,6 当 `Root.Padding`，高亮铺满整格 | 同名兄弟层，`Margin` 走已发布行 |
| `Viewbox` Height 16 + Margin 0,16,0,2 | `Width=16 Height=16`、垂直居中 | Height 16 + `AppBarButtonContentViewboxCollapsedMargin`（新增发布行） |
| Compact：`AppBarButtonInnerBorder.Margin` → 2,6,2,22；`LabelCollapsed`：`ContentRoot.MinHeight` → 48 | 只改 `Root.Padding` | 一个 `IsCompact` 格子同时写 `Margin` 与 `MinHeight`（本运行时只有一个属性） |
| `AppBarSeparator`：`Padding`=2,8,2,8 落在矩形 `Margin` 上，控件无高度 | `Margin` 落在控件上 + 自造 `MinHeight=40` | 与上游同：`Padding` setter + 矩形 `Margin={TemplateBinding Padding}`，撤掉 `MinHeight` |
| `AppBarToggleButtonTextLabelMargin`（2,0,2,8） | 复用按钮同名行 | 值一致，仍只发布按钮那条名（记为键名债） |

证据：`AstraAppBarTests` 新增 `The_bar_geometry_is_the_cells_upstream_writes`（图标槽 0,16,0,2、高亮 2,6,2,6、
标签 2,0,2,8、`ContentRoot.MinHeight` 64→48、分隔线矩形 2,8,2,8 且样式里没有 `MinHeight` setter）、
`A_checked_toggle_inside_the_bar_sends_its_accent_into_the_pixels`（栏内离屏 + 上屏 `Chrome` 两条通路）、
`A_checked_fill_written_after_the_bar_is_live_reaches_the_shown_window`（先上屏、后置勾选）；
`The_template_carries_the_part_names_upstream_uses` 改成三层类型断言。视觉半边是
`spike/VisualQA/out/command-bar.png`（PrintWindow，dpi=168）：Bold 格子内 `#60CDFF` 7786 px，图标在标签之上，
条目带 64 高。这张图出自还没有空帧闸口的 `capture-pages.ps1`，它下面的 S0-r 更正就是它带来的。

**这一批当时据此写下的一条"基座缺陷"在同一天被自己的实验推翻**：把高亮层上的
`TransitionProperty="Background, BorderBrush"` 当成 PrintWindow 读到 0 px 的原因，那两次捕获其实都落在
还没画过的帧上。`spike/TransitionProbe` 的 17 格判别矩阵（模板根 vs 嵌套、有 vs 无过渡、83ms vs 0、
加载时写 vs 上屏后写、`{ThemeResource}` vs 字面、TemplateBinding 与格子双写同一属性、模板外 vs 模板内、
笔刷过渡 vs `Width` 过渡、属性列表带空格 vs 不带，外加本三层形状的完整复刻）三次抓帧 17/17 全部落帧；
把过渡加回这份模板、改用带空帧闸口的 `spike/VisualQA/grab-page.ps1` 重捕同一页，`#60CDFF` 与无过渡时
逐像素相同（8832 px；抓到可用帧前重试 7 次 vs 15 次）。过渡因此留在样式里，与上游一致。原始读数、
推翻它的两个实验与"为什么属性/离屏三条通路全绿而屏幕读 0"都记在 `adaptation/00` 的 S0-r 更正节。

9. 这条更正留下的真缺口是**判据本身**：harness 的 `Render`/`Host`/`Chrome` 是同一条 `RenderTargetBitmap`
   通路（`Chrome(窗口)` 也不是屏幕），所以它们既看不到"帧没跟上属性"，也就永远证不了合成帧；要问合成帧
   只能出进程抓，而外部抓帧必须先过"这一帧画了没有"的闸口（非黑像素计数），否则"某色 0 px"与"整帧空白"
   同形——这正是本批第一条结论的来源。三条互补断言（属性 / 离屏 / 带闸口的外部捕获脚本）留在测试与
   `spike/TransitionProbe`、`spike/VisualQA/grab-page.ps1` 里，防的是分歧再次被当成通过；真指针输入的
   像素通路仍欠（任务 #13）。
