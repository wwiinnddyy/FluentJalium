# 审计：窗口外壳（TitleBar 与背衬）

- 上游：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
  - `controls/dev/TitleBar/TitleBar_themeresources.xaml`，blob `b22068a7909c99426a1f1811e227db4ad11baa1c`，241 行
  - `controls/dev/TitleBar/TitleBar.idl`（API 形状）、`TitleBar.xaml`（模板）
- 分级：**底座，框架自持部件 + 公开 `{ThemeResource}` 钩子可达**。
- 结论：**外壳不是"没有可主题化表面"**。框架给 `TitleBar`/`TitleBarButton` 定的那套默认外观一共消费
  **8 个** `{ThemeResource}` 名字，全部能在应用级字典里被我们的调色板覆盖；另有
  `Window.CustomTitleBarStyle`/`Window.TitleBarStyleKey` 两个公开样式入口。
  落地的 6 条钩子在真窗口的客户区像素上验证过了。

## 先更正我自己

本轮开工前的判断是"窗口外壳没有任何可主题化表面，因此 `ToolbarSurfaceBrush` 只能删掉、
TitleBar 只能记为不可达"。**那是错的**，错因值得记下来：我只查了"我们的字典里有没有外壳键"，
没有去查"框架自己的隐式样式消费哪些键名"。正确的查法是反过来——
从 `Themes/Controls/TitleBar.jalxaml` 里 grep `{ThemeResource ...}`，再验证应用级同名键能不能压过框架默认值。
`audits/scrollbar.md` 用的是同一套判据（先找框架自持的样式，再找它读的键名），我在那一项做对了、
在这一项做漏了。删 `ToolbarSurfaceBrush` 仍然成立（它是我们自造、无人消费的名字），
但它"为什么无人消费"的理由不是"外壳没有表面"，而是"外壳的表面另有 8 个真实名字"。

## 26.10.9 的外壳形状（反射 + 建树实测）

> 术语注意：`adaptation/00` 的普查说 26.10.9 "没有通用主题、0 个控件有框架默认样式、
> 程序集里嵌入 0 个 `.jalxaml`"。这与"外壳读 8 个 `{ThemeResource}` 名字"**不矛盾**——
> 本轮证明的是那 8 个名字在 26.10.9 上确实走应用级查找（我们的别名压过了 `#F5F5F7`），
> 至于 26.10.9 里那份样式是 DP 默认值还是别的来源，本运行时没有证据，不声称；
> 上面引用的 `Themes/Controls/TitleBar.jalxaml` 属于 sibling 源码树（可能比 26.10.9 新），
> 只用来定位键名与默认值，不作为 26.10.9 内部实现的证据。


- `Window` 上一层就是它自己搭的 `TitleBar`（在窗口视觉树里，`PixelHarness.Descendant<TitleBar>` 取到，
  静止 `ActualWidth≈786`、`Height=32`）。`Window` 另有公开属性 `TitleBar`。
- `TitleBar : Control`，公开面：`Title`、`IsMaximized`、`ShowMinimizeButton`、`ShowMaximizeButton`、
  `ShowCloseButton`、`LeftWindowCommands`、`RightWindowCommands`、`IsShowIcon`、`IsShowTitle`、`WindowIcon`，
  事件 `MinimizeClicked`/`MaximizeRestoreClicked`/`CloseClicked`。模板部件名：
  `PART_RootBorder`、`PART_CloseButton`、`PART_MaximizeButton`、`PART_MinimizeButton`、
  `PART_RightWindowCommandsHost`、`PART_WindowIconHost`、`PART_TitleText`、`PART_LeftWindowCommandsHost`
  （`PART_CloseButton` 等名字在 26.10.9 的程序集里查得到）。
- `TitleBarButton : Control`，只多两个公开面：`Kind`、`GlyphSize`。静止宽 **46**。
- `Window` 外壳相关默认值（逐项读回）：`TitleBarHeight=32`、`TitleBarFontSize=14`、`IsShowTitleBar=true`、
  `TitleBarStyle=Custom`（枚举 `Native,Custom`）、`TitleBarStyleKey=null`、`CustomTitleBarStyle=null`、
  `SystemBackdrop=None`。
- `WindowBackdropType` 词汇表：**`None,Auto,Mica,Acrylic,MicaAlt`**，默认 `None`。
- 框架自己给这 8 个钩子的默认值偏 macOS 味：Light `#F5F5F7`/`#1D1D1F`、Dark `#1C1C1E`/`#F5F5F7`、
  关闭键悬停 `#FF3B30`、按下 `#FF6961`。

## 上游 40 个键的处置

上游字典层是 **20 个别名刷**（Light/Default 各 20 条同内容，HC 分支另 20 条指向 `SystemControl*`）
\+ **18 个度量键**（14 个 `x:Double` + 4 个 `Thickness`）+ 2 个命名样式（`TitleBarBackButtonStyle`、
`TitleBarPaneToggleButtonStyle`）。

| 上游键族 | 处置 | 理由 |
|---|---|---|
| `TitleBarForegroundBrush`、`TitleBarSubtitle*Brush`、`TitleBarDeactivatedForegroundBrush` | **不声明为公开键** | 我们没有消费它们的模板；对应语义改由下面 6 条钩子承担 |
| `TitleBarBackButton*`、`TitleBarPaneToggleButton*`（各 8 条） | **不声明** | 本运行时的 `TitleBar` 没有返回键/窗格键，声明了就是死键 |
| `TitleBarCompactHeight=32`、`TitleBarExpandedHeight=48`、`TitleBarBackButtonWidth=40` 等 18 条度量 | **不声明** | ①`x:Double` 本运行时的标记读取器解析不了（`adaptation/00`，与 ToolTip 字号同一原因）；②没有消费点。数值只在审计里对照 |
| `TitleBarBackButtonStyle`、`TitleBarPaneToggleButtonStyle` | **不声明** | 无对应部件 |

## 落地的 6 条钩子

`ThemeResources/TitleBar.jalxaml`（新增，进 Manifest）。全部是
`<StaticResource x:Key=钩子名 ResourceKey=调色板键>`，也就是别名层的同一套做法：解析到的是
调色板**同一个刷对象**，因此主题翻转与高对比自动跟随，不需要为它们另写 HC 行。

| 框架钩子 | 指向 | 上游同语义键（blob `b22068a7…`） |
|---|---|---|
| `TitleBarBackground` | `SolidBackgroundFillColorBaseBrush`（`#F3F3F3`/`#202020`，HC `SystemColorWindowColor`） | 上游用背衬画标题栏；`SystemBackdrop` 默认 `None`，所以取背衬自己回落实心的那个键，`#F3F3F3` 也正是 WinUI 3 页面底色 |
| `TitleBarText` | `TextFillColorPrimaryBrush` | `TitleBarForegroundBrush` |
| `TitleBarGlyph` | `TextFillColorPrimaryBrush` | `TitleBarBackButtonForeground` / `TitleBarPaneToggleButtonForeground` |
| `TitleBarButtonBackground` | `SubtleFillColorTransparentBrush` | `TitleBarBackButtonBackground` |
| `TitleBarButtonHover` | `SubtleFillColorSecondaryBrush` | `TitleBarBackButtonBackgroundPointerOver` |
| `TitleBarButtonPressed` | `SubtleFillColorTertiaryBrush` | `TitleBarBackButtonBackgroundPressed` |

**两条没动的钩子**：`TitleBarCloseButtonHover`、`TitleBarCloseButtonPressed`（框架默认红）。
上游在这个 commit 的标题栏字典里**没有**关闭键专用资源名；唯一的红色候选
`SystemFillColorCritical` 在 Dark 是 `#FF99A4`（文本语义的浅红），当作按钮底色明显不对。
所以这里不留没有依据的数，也不自造键名——保留框架的红，记进 Known Gaps。
（WinUI 3 应用在 Windows 11 上看到的关闭键红由系统非客户区画，不在 Xaml 控件资源里。）

## 可达性证据（本轮实测）

1. **别名压过框架默认值**：`TitleBarBackground` 落地前读回 `#F5F5F7`（框架默认），
   落地后读回 `#F3F3F3`（我们的 `SolidBackgroundFillColorBaseBrush`）；`TitleBarText`
   由 `#1D1D1F` 变 `#E4000000`。应用级字典能压过框架自持样式，与 `ScrollBarThumb` 同一条路。
2. **像素**：直接捕获那颗 `TitleBar`（新增 `PixelHarness.Chrome`，因为外壳没有可以放主体的
   content 槽位）。Light 下框架默认 `#F5F5F7` 计数 0、`#F3F3F3` > 1000；Dark 下 `#1C1C1E` 计数 0、
   `#202020` > 1000；两幅都没有品牌绿。
3. **高对比**：那颗 `TitleBar.Background` 仍是调色板实例，颜色随
   `SolidBackgroundFillColorBaseBrush` 的 HC 行落到 `SystemColors.WindowColor`，
   `Foreground` 落到 `SystemColors.WindowTextColor`。
4. **样式入口**：设 `Window.CustomTitleBarStyle = new Style(typeof(TitleBar))` 后，
   框架自持那颗 `TitleBar.Style` 读回就是它、哨兵 `Background` 落在上面；`TitleBarStyleKey` 未测（同一条解析路径的按键版）。

## 上游 API 形状差（1.0 的 CLR API 清单要逐条列）

| WinUI 3 `TitleBar` / `Window` | 本运行时 | 影响 |
|---|---|---|
| `Subtitle` | 无 | 只能自绘标题区 |
| `IconSource` | `WindowIcon`（`ImageSource`）+ `IsShowIcon` | 类型不同，语义可映射 |
| `LeftHeader` / `Content` / `RightHeader` | `LeftWindowCommands` / `RightWindowCommands`，**中间自定义内容无槽位** | 中央只能显示 `Title` 文本 |
| `IsBackButtonVisible`/`IsBackButtonEnabled`/`BackRequested` | 无 | 无返回键，上游 8 条返回键资源也无消费点 |
| `IsPaneToggleButtonVisible`/`PaneToggleRequested` | 无 | 同上，窗格切换键得自己放进 `LeftWindowCommands` |
| `TemplateSettings`、拖拽区（`AutoRefreshDragRegions`、`RecomputeDragRegions`） | 无 | 上游靠它算可拖区域；本运行时的拖动由框架自己处理，未验证 |
| `Window.ExtendsContentIntoTitleBar` | 无（dll 里查无此名） | 内容不能延伸到标题栏下 |
| — | `IsMaximized`、`Show{Minimize,Maximize,Close}Button` + 三个 `Clicked` 事件 | WinUI 把这三个键放在 `AppWindow` 非客户区，不在控件里；本运行时相反 |

静止高度 32 与上游 `TitleBarCompactHeight` 一致（这条有断言）；按钮宽 46 与上游
`TitleBarBackButtonWidth=40` **不是同一种按钮**，没有对照意义，所以不声称一致，也不声称与 Win11 一致。

## 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行全绿（restore → build 0 警告 → 44/44、0 skip → 调色板漂移 checked=True，102 刷）。
- **行为**：`AstraWindowShellTests.The_title_bar_hooks_reach_the_chrome_the_window_builds`
  （4 条 `Assert.Same` 实例同一性）、`The_shell_numbers_the_chrome_is_built_with_are_pinned`、
  `A_backdrop_is_off_by_default_and_names_five_materials`、
  `The_supported_style_hook_reaches_the_chrome_title_bar`。
- **视觉**：`The_title_bar_surface_reaches_the_pixels_and_follows_the_theme`
  （Light/Dark 两幅、框架默认值计数为 0、无品牌绿）、
  `High_contrast_moves_the_chrome_with_the_palette`。
- **硬件输入**：**未做**。`TitleBarButtonHover`/`Pressed` 与关闭键两个钩子只能读回别名解析结果，
  悬停/按下/拖拽/双击最大化都要真指针输入；`MinimizeClicked` 等三个事件同样未触发验证。
  和滚动条的悬停证据一起归 Button 批。

## Known Gaps（不许用相邻证据替代）

1. Mica/Acrylic/MicaAlt 的实际**合成效果无法在本环境验证**：需要真桌面在窗口后面，
   离屏渲染没有可断言的背衬。本项只钉住了枚举词汇表与"默认 None"这一事实。
2. 关闭键悬停/按下色沿用框架默认红，未对齐 Win11 非客户区红（无可引用的上游资源键）。
3. 标题栏按钮的悬停/按下/拖拽/双击、三个 `Clicked` 事件、触摸与笔路径全部未测。
4. 上游 40 个键里只有 6 个语义被我们承接；返回键、窗格键、Subtitle、中央自定义内容、
   拖拽区计算、`ExtendsContentIntoTitleBar` 在本运行时**没有对应公开面**，1.0 不做不声称。
5. 高对比下的标题栏只有实例/颜色读回证据，没有像素证据（本进程 `SystemColors` 是用户的实际主题色，
   断言颜色等价可以，断言像素不行）。
6. `TitleBarStyleKey`（按键解析那条路）未测，只测了 `CustomTitleBarStyle`。
7. 标题栏与内容区在窗口内的层序/占位关系未审计（`IsShowTitleBar=false` 时内容是否顶上未测）。
