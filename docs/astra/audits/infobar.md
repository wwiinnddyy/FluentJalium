# InfoBar 审计（阶段 4 第一段）

上游依据：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`

- `controls/dev/InfoBar/InfoBar_themeresources.xaml` blob `1056af57d0340a7e9aa2d5f9e2f2f177b841897c`（95 行）
  - Light 分支 **5–21 行**：17 条别名 + `InfoBarBorderThickness`=1；Default 分支 24–40 同值；
    High Contrast 分支把 8 条严重度底色/图标底改指 `SystemColorWindowColor`/`HighlightColor`/`HighlightTextColor`、
    文本指 `SystemColorWindowTextColor`、超链接指 `SystemColorHotlightColor`、描边指 `SystemControlTransientBorder`，
    并把 `InfoBarBorderThickness` 抬到 2
  - 分支外 82–95：`InfoBarTitleFontSize`/`InfoBarMessageFontSize`（x:Double）、两条 `FontWeight`、
    `InfoBarMinHeight`=48、`InfoBarCloseButtonSize`=38、`InfoBarCloseButtonGlyphSize`=16、`InfoBarIconFontSize`=16、
    五条 `x:String` 字形（F136/F13F/F13D/F13C/F13E）、`InfoBarIconBackgroundGlyph`、若干 `Thickness`、
    `InfoBarCloseButtonSymbol`（`Symbol` 元素）与整份 `InfoBarCloseButtonStyle`
- `controls/dev/InfoBar/InfoBar.xaml` blob `1f036889684c7af85187811a1ef776d059eed2a8`（186 行）
  - `Style TargetType=InfoBar` 3–185；模板 14–182；`ContentRoot` 15；
    `SeverityLevels` 17–43（4 态，每态写 4 个值）、`IconStates` 44–58（3 态）、可关闭组 59–66（2 态）、
    `InfoBarVisibility` 67–74（2 态）、前景组 75–83（2 态）、横幅内容组 84–91（2 态）
  - `Title` 113、`Message` 114、动作槽 `ContentPresenter` 115–124（内含一段 `HyperlinkButton` 隐式样式）、
    `ContentArea` 126、`CloseButton` 127–179（128–175 是三份把 `Button*` 键改写到 `AppBarButton*` 上的主题字典）

运行时：NuGet Jalium.UI **26.10.9**，`Jalium.UI.Controls.InfoBar` 原生存在（`ContentControl` 之下）。
公开面是 `ActionButton`(`ButtonBase`)、`IsClosable`、`IsIconVisible`、`IsOpen`、`Message`、`Severity`
{Informational,Success,Warning,Error}、`Title`，事件 `CloseButtonClick`/`Closed`，方法只有 `OnApplyTemplate`。
**没有** `CloseButtonCommand`、`CloseButtonStyle`、`TemplateSettings`（`spike/SurfaceProbe` B2/B3、
`spike/ExpanderInfoBarProbe` K 段逐项确认为假）。

一句话结论：**这个控件的问题不是样式不对，而是根本没有样式入口**——它不接受模板，自己用硬编码几何画一遍，
因此本批的唯一正确出路是补上那句"我要用模板"的声明，而不是画一张更像的皮。

## 0. 先量后写：决定性事实

**S1 · 原生 `InfoBar` 的 `Template` 完全无效。**
挂载后 `Template==null`、可视子节点 0；把同一个 `ControlTemplate` 作为本地值赋上，树里仍然只有
`ContentPresenter` 呈现的内容，没有 `RootBorder`、没有关闭按钮（pass 2 M1、pass 3 Q 第一行）。
同一份模板赋给 `Expander` 立刻成树，所以这不是解析问题，是这个类型没打开模板通道。

**S2 · 开关是 `ContentControl` 上一个 `protected` 方法。**
`Expander` 构造函数调了它，`InfoBar` 没调。因为它是 `protected`，**派生类可以在构造里合法调用**，
不需要私有反射、不需要伪装自有类型：`FluentInfoBar : InfoBar` 的构造函数里一句
`UseTemplateContentManagement()` 之后，整份模板成树（pass 3 Q 第三行：`RootBorder` + 三列网格 + `PART_CloseButton`）。
测试 `The_native_info_bar_ignores_a_template_and_ours_does_not` 把"原生不吃、我们的吃"钉成断言。

**S3 · 基础类仍owns两件事：`IsOpen` 的度量与 `PART_CloseButton`。**
带模板后 `PART_CloseButton`（必须正好是这个名字、`Button` 这个类型）的 `Click` 由基类接走：
一次 `IInvokeProvider.Invoke()` → `CloseButtonClick` 计数 1、`Closed` 计数 1、`IsOpen=false`（pass 3 Q2）。
这是本批唯一能 raised 出 `CloseButtonClick` 的途径——它没有 `OnCloseButtonClick` 之类的可调用出口。

**S4 · 基类的自绘在有模板时让路。**
`OnRender` 一进来就检查是否找到名为 `RootBorder` 的部件，找到即返回。像素侧的印证是：
带模板的 Error 条里没有框架那条硬编码严重度底色 `#2D2D37`
（`The_error_severity_paints_the_critical_token_and_not_the_frameworks_fill`）。

**S5 · `IsOpen=false` 不会自己收起模板。**
带模板时控件只是不再参与自己的度量，条仍在原处按 `MinHeight` 占位、照样可见（pass 3 Q3）。
上游靠 `InfoBarCollapsed` 状态把 `ContentRoot` 折起来，所以我们必须自己写这格——已在模板里，并被断言钉住。

**S6 · 框架自绘那套读的是另一批键名。**
`InfoBarInformationalBackground`、`InfoBarSuccess/Warning/ErrorBackground`、`InfoBarInfo/Success/Warning/ErrorBrush`、
`InfoBarForeground` 这九条在框架自带主题里存在（pass 2 O 段逐条命中），且各自有硬编码回退色。
本批**不发布**这九条：走模板路线后它们不再被任何东西读，发布它们等于承诺"覆盖能到像素"却无人消费。
（若哪天要保留原生自绘路线，这九条得连同像素证据一起回来。）

## 1. 行去向表（上游 17 别名 + 分支内 1 条 Thickness + 分支外约 20 行）

| 上游 | 处置 |
| --- | --- |
| 4 条严重度底色、4 条图标底色、4 条图标前景、`InfoBarTitleForeground`、`InfoBarMessageForeground`、`InfoBarBorderBrush` = **16 条别名** | 逐字转录到 `ThemeResources/InfoBar.jalxaml`，模板以 `{ThemeResource}` 消费 |
| `InfoBarBorderThickness` | 转录（Light/Default=1）；HC=2 不发布，见 §5.5 |
| `InfoBarContentRootPadding`、`InfoBarIconMargin`、`InfoBarPanelMargin`、`InfoBarPanelVerticalOrientationPadding`、`InfoBarTitle/Message/ActionVerticalOrientationMargin` | 转录（`Thickness` 可解析），模板逐条读取 |
| `InfoBarHyperlinkButtonForeground` + `InfoBarHyperlinkButtonMargin` | **不发布**：动作槽放的是 `Button`，上游那段模板内隐式 `HyperlinkButton` 样式在本运行时未被证明可用；反向测试钉住 |
| 4 条 horizontal-orientation 行 + `InfoBarPanelHorizontalOrientationPadding` | **不发布**：没有 `InfoBarPanel`，本模板永远竖排（§5.4） |
| `InfoBarMinHeight`=48、`InfoBarCloseButtonSize`=38、`InfoBarCloseButtonGlyphSize`=16、`InfoBarIconFontSize`=16、`InfoBarTitle/MessageFontSize`=14 | 字面量（`x:Double` 不可解析） |
| 两条 `FontWeight`、五条 `x:String` 字形、`InfoBarCloseButtonSymbol`、`InfoBarCloseButtonStyle` | 不发布：`SemiBold`/`Normal` 写字面量，图标与关闭符改为图形（§2） |

## 2. 宿主替换清单

| WinUI 侧 | 本实现 | 依据 |
| --- | --- | --- |
| 完全模板化，`InfoBarPanel` 自定义面板 | 三列 + 两行 `Grid`，标题/消息/动作固定竖排 | S1（模板通道）＋§5.4 |
| `TemplateSettings.IconElement` + `Viewbox` | 无该属性；只有严重度标记，`IsIconVisible` 控制其显隐 | 探针 K |
| 图标=两枚 `SymbolThemeFontFamily` 字形（F136 底盘 + 严重度字形） | `Ellipse`（16 DIP，填严重度图标底行）+ `Segoe Fluent Icons` 字形（保留上游码点） | S1 之后的模板 |
| 关闭按钮 `SymbolIcon Cancel` + `CloseButtonStyle` + `CloseButtonCommand` | `Path` 折叉 + 基类监听的 `PART_CloseButton`；样式改指 `SubtleButtonStyle` | S3；`AppBarButton*` 那三套改写属于 CommandBar 批，未转录 |
| 6 个 VisualStateGroup | `ControlTemplate.Triggers` 格子：3 条严重度 × 4 值、`IsIconVisible`、`IsClosable`、`IsOpen` | §3 |
| `BackgroundSizing`、`AutomationProperties.*`、`InfoBarPanel.*OrientationMargin` 附加属性 | 无对应；边距改为直接写在元素上 | — |

## 3. 状态映射

| 上游 | 本实现 |
| --- | --- |
| `SeverityLevels/Informational` | 静置值（模板根 `Background`、图标 `Fill`、字形 `Text`/`Foreground` 直接写行）——与上游空状态一致 |
| `SeverityLevels/Error`、`Warning`、`Success` | `Trigger Severity=X`：同一部件的底色、图标底盘、字形码点、字形前景四条 |
| `IconStates/StandardIconVisible` | `Trigger IsIconVisible=True` → `IconArea.Visibility=Visible` |
| `IconStates/UserIconVisible` | **无**（没有图标槽，§5.2） |
| `IconStates/NoIconVisible` | 静置（`IconArea` 默认 `Collapsed`） |
| 可关闭组两态 | `Trigger IsClosable=False` → 关闭按钮 `Collapsed` |
| `InfoBarVisible` / `InfoBarCollapsed` | 静置 / `Trigger IsOpen=False` → `RootBorder` `Collapsed`（S5） |
| `ForegroundNotSet` / `ForegroundSet` | **无**：本运行时没有"该属性是否有本地值"这种条件 |
| `BannerContent` / `NoBannerContent` | **无**：`Content` 槽固定在第二行 |

## 4. 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行通过。
- **行为**：`AstraExpanderInfoBarTests` —— 原生模板无效而我们的有效（S2）、`PART_CloseButton` 一次点击恰好
  触发一次 `CloseButtonClick` + `Closed` + `IsOpen=false`（S3）、`IsOpen=False` 后 `RootBorder` 不再可见（S5）、
  三条严重度格各自点名底色/图标/前景行并改字形码点、`IsIconVisible`/`IsClosable` 各有一格、
  标题与消息部件的前景确实是 `InfoBarTitleForeground` 那个实例。
- **视觉**：覆盖调色板键 `SystemFillColorCriticalBackgroundBrush` 为哨兵色后 Error 条像素 >3000 且不含 `#2D2D37`；
  覆盖 `SystemFillColorAttentionBrush` 后图标底盘 `Ellipse` 引用的正是该实例且像素 >100；
  Light↔Dark 顶部色不同；品牌绿 `#207245` 为零。
- **硬件输入**：**未做**。点击关闭按钮用的是自动化 Invoke 模式，指针设备没有动过；真指针/触摸与 hover 仍欠（任务 #13）。

## 5. Known Gaps

1. 关闭条不响应触摸/笔的真机路径；CloseButtonClick 只由自动化 Invoke 证明。
2. 没有用户图标槽，`UserIconVisible` 不可达。
3. 动作槽的 `HyperlinkButton` 上色行（`InfoBarHyperlinkButtonForeground`/`Margin`）未转录。
4. 永远竖排，上游按可用宽度横排 title/message/action 的 `InfoBarPanel` 行未转录。
5. `InfoBarBorderThickness` 只有 1；上游 High Contrast 是 2（内核只换色不换厚度）。
6. 严重度字形依赖机器上装有 `Segoe Fluent Icons`；本批像素断言只覆盖底盘颜色，不覆盖字形是否命中。
7. 焦点不在控件上（`IsTabStop=False`，与上游一致），因此关闭按钮的可达性只由它自己的 Tab 停靠点保证，未做键盘遍历实测。
8. 标题/消息为 `Wrap`；上游的 `WrapWholeWords` 在本运行时的枚举里没有对应项。
9. 框架自绘路线的九条 `InfoBar*` 键（S6）本批故意不发布；若哪天要回退到自绘，它们必须带自己的像素证据回来。
10. 本节第 6 段的普查只证明"哪些原生控件自带 `OnRender`、它们各自按名字要哪些部件"，没有证明除 `InfoBar` 之外
    任何一条模板是否已经拿到让路所需的名字。`NumberBox`（要 `PART_LayoutRoot`）、`Slider`（要 `PART_Segments`）、
    `ComboBox`（要 `PART_SelectionPresenter`）三条是**待量的重影候选**，不是已确认的缺陷。

## 6. 视觉缺陷批的更正（2026-09-19，用户实测反馈"重影"）

审计写对了，代码没照做：本节记录这条落差在屏幕上造成的真实缺陷，以及把它对齐的两处改动。探针 `spike/InfoBarGhostProbe`
（pass 1–4，日志 `ghost-probe*.txt` 在树内）、`spike/TextWrapProbe`（pass 1）。

**缺陷 1 · 整条信息条被画两遍。** `Styles/Surfaces.jalxaml` 的模板把根 `Border` 命名为上游的 `ContentRoot`，
而运行时 `InfoBar.OnApplyTemplate` 找的名字是 `RootBorder`——这一点本轮用 IL 直接读出来了（`ldstr` 字面量：
`RootBorder`、`PART_CloseButton`），不再依赖推测。名字没对上时基础类没找到它的部件，于是 `OnRender` 不让路，
在我们模板之上又画一遍自己的图标、标题、消息和关闭叉号：`spike/VisualQA/out/before-fix-infobar-ghost.png`（改名前 surfaces 页的局部截图）里那两层错位文字。
把部件改名成运行时要的名字即修复。像素侧的对照：

| 量法 | 改名前 | 改名后 |
|---|---|---|
| 样式条（隐藏图标与关闭）捕获的颜色数 | 14 | **7** |
| 其中基础类图标蓝 `#0A84FF` | 178 px | **0 px** |
| 样式条 vs 另一条把 `OnRender` 覆写成空的同类 | 不同 | **逐像素相同**（7 色 / 936 ink） |

第三行是关键对照：改名之后，"我们的模板"与"模板 + 基础类完全不画"两种情况捕获一致，说明模板里已经没有任何东西
依赖基础类那一层。这个对照在改名前不成立（基础类多画 178 px 蓝）。

**缺陷 2 · 消息换行后第二行画到条外面去。** `InfoBar.MeasureOverride` 返回的是基础类自己那套单行字面布局的高度，
不是模板树要的：300 DIP 宽、消息两行的条，内部要 107.1，控件对外只报 69.8，于是第二行落在表面之下
（`spike/TextWrapProbe` pass 1 case B）。`FluentInfoBar` 因此多一个 `MeasureOverride`：先让基础类量它自己那套，
再用本次可用尺寸量模板根，取两者较大。上游不需要这一句，因为它的条完全由模板量出来。

**两处偏离上游，都记在这里：**
- 部件名：上游 `InfoBar.xaml:15` 用 `x:Name="ContentRoot"`，本库用 `RootBorder`。改名不是风格选择，是这个运行时
  的 `OnApplyTemplate` 只认后者；认不到就不让路（缺陷 1）。
- 度量：上游没有 `MeasureOverride`，本库的自有类型有。理由同上，是可测的行为缺口，不是提前抽象。

**回归闸口**（三条都做过反向验证：把修复撤掉即失败，装回去即通过）：
`A_templated_bar_leaves_the_base_class_nothing_to_paint`、
`A_wrapped_message_grows_the_bar_instead_of_painting_below_it`，以及原有的
`The_native_info_bar_ignores_a_template_and_ours_does_not`（部件名断言已跟着改名）。

**仍未证明**：`InfoBar` 之外那些"自带 `OnRender`"的原生控件里，我们的模板是否都拿到了基础类让路所需的部件名。
本轮普查（`ghost-probe4.txt` 第 C 段）给出的候选是 `NumberBox` 要 `PART_LayoutRoot` 而 `Styles/TextInput.jalxaml`
没有这个名字；`Slider` 要 `PART_Segments`、`ComboBox` 要 `PART_SelectionPresenter`，本库都没有——三者是否真的因此
多画一层，尚未按缺陷 1 那样量过，见 Known Gaps 第 10 条。

## 更正（属性死写批 2026-09-20，`adaptation/00` S1-g）

本审计发的两条 padding 行**从来到不了元素**：`InfoBarContentRootPadding`（`16,0,0,0`）写在内容根的 `Grid` 上、
`InfoBarPanelVerticalOrientationPadding`（`0,14,0,18`）写在 `Panel` 的 `StackPanel` 上，
而 Grid 与 StackPanel 都没有 `Padding` 成员（同一元素上的 `Background`/`Margin` 有，所以布局看起来一切正常）。
修法：前者挪到 `RootBorder`（同一个盒子，`MinHeight=48` 跟着挪过去，48 才仍然"含 padding"），
后者由新增的 `PanelSurface` Border 承载，`Panel` 的名字与 StackPanel 类型不动；
内容根 Grid 上那条同样无处落地的 `CornerRadius` 删除。读回见
`AstraSurfaceGeometryTests.The_info_bar_insets_its_content_with_the_rows_that_now_have_a_painter`。
残留缺陷（进 Catalog）：条自身的 `Background` 仍由那层 Grid 画，它没有圆角可说，
所以**应用设了 Background 时不会被根半径裁掉**——本批没给它画者，也没量过这条会不会真看见。
