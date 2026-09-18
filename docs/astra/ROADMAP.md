# Astra 1.0 路线：五条工作流

定义采纳这句：**FluentJalium 1.0 是"基于 Jalium 原生运行时、按 WinUI 3 源码审计实现的
Fluent 控件与主题系统"**，不是"兼容 WinUI 3 的 Jalium 运行时"。
所有条目都挂在 `adaptation/00`、`01`、`02` 三份实测之上；标 🬡 的是仍未证、需先探的点。

## 地基事实（全部实测，不是文档转述）

| 事实 | 来源 |
|---|---|
| 框架**没发布 Generic 主题**，163 类型 0 个默认样式 → 我们就是主题 | `01` |
| `Application.ThemeMode` 是唯一的主题驱动；`Window.ThemeMode` 也存在（逐窗岛） | `00` + `Window` 成员表 |
| `{ThemeResource}` 生效；`{StaticResource}` 解析期冻结；`<StaticResource x:Key ResourceKey/>` 可用 | `00` S0-a/S0-c |
| `x:Double` 无法解析；`Color`/`CornerRadius`/`Thickness`/`Duration` 可以 | `00` S0-b |
| 标记写 VSM 抛异常；但 `Storyboard`/`BeginStoryboard`/`VisualTransition`/`AnimationFactory`/`TransitionHost` 全 public | `00` S0-d + `MOTION` |
| 材质**存在且丰富**：`BackdropEffect` → `AcrylicEffect`/`MicaEffect(UseAlt)`/`FrostedGlassEffect`/`BackdropBlurEffect`/`ColorAdjustmentEffect`/`CompositeBackdropEffect`；元素级 `BlurEffect`/`DropShadowEffect`/`InnerShadowEffect`/`OuterGlowEffect`/`EffectGroup` | `MATERIAL` |
| `Window.SystemBackdrop`、`TitleBar`/`TitleBarStyle`/`CustomTitleBarStyle`/`Left(Right)WindowCommands`/`AdornerLayer`/`AllowsTransparency` 均 public | `WINDOW` |
| 像素可在进程内回读：`RenderTargetBitmap(w,h,dpiX,dpiY,PixelFormat)`+`Render(Visual)`+`CopyPixels` | `02` |
| 颜色天花板只剩：4 个实时 `ThemeColors.Accent` 读取点（`CheckBox`/`RadioButton` 冻结勾选 glyph 一条**已撤回**） | `02`、`06` |
| 无公开高对比驱动；无公开系统减弱动画 API；`FluentSystemIcons` 平面外码点成方框 | `00`、`01` |

## A. 取色系统

目标：一套 WinUI 命名的语义令牌，同时驱动我们的样式和（能驱动的地方）框架渲染。

- **A1 ~~调色板升级为真 `ThemeDictionaries`~~ → 混合模型（阶段 1 已落地，用户决定）**。
  保留单份就地改色的调色板（笔刷实例跨主题不变），外加一次 `Application.ThemeMode` 赋值驱动
  框架原生默认。理由与实测见 `adaptation/00` 的「阶段 1 落地」节；键名逐字照抄上游
  `Common_themeresources_any.xaml` 这一点不变。
- **A2 别名层不再展平**。S0-c 证明 `<StaticResource x:Key="ButtonBackground" ResourceKey="…"/>`
  可用，所以 WinUI 每个 `X_themeresources.xaml` 的别名块**原样转录**进对应控件字典。
  这修正了 `00` 里"生成期展平"的旧决定：原样别名既能保结构可比对，又天然共享刷子实例。
- **A3 强调色三态**。(i) 默认：把 `ThemeManager.SystemAccentResolver`（public setter）接到 DWM，
  框架与我们的键一起跟；(ii) 显式：`ThemeManager.ApplyAccent(color)`（`02` 已证明它能改变
  Slider/ProgressBar 的像素）+ 我们自己那一族 accent 键原地改色保标识；
  (iii) `OverrideBrush(key,color)` 保留。
- **A4 ✅ 勾选 glyph：撤回，不是天花板**。`06` 查明那条"冻结"读数出自未推帧的捕获；
  判据修好后 `Check_mark_follows_the_selected_accent` 直接通过——`ApplyAccent` 之后哨兵色出现在
  勾选像素里。模板覆写实验与"毕业换自有类型"两条都不再需要。`RadioButton` 的圆点未单独量。
- **A5 数值令牌**：继续内联字面量，但每个字面量必须在 `<control>.parts.md` 里有上游出处；
  闸口脚本比对两边，防手抄漂移。
- **A6 键清单 + 消费点反查**（最高优先，因为缺失键是静默失败）：消费点反查**已落地** —
  `tests/FluentJalium.Tests/Resources/AstraResourceKeyTests.cs` 断言每个引用都有声明、
  `{ThemeResource}` 键在每个分支都在、Light/Dark 键集对称；`HighContrast.map` 与调色板键集
  由 `AstraThemeRuntimeTests` 对齐。剩下的 `docs/astra/resources/keys.md`
  （`path|key|type|theme-scope|upstream-blob` 全量清单）仍未写。

## B. 基础系统

- **B1 主题门面收敛 ✅（阶段 1）**：`FluentThemeVariant → ApplicationThemeDriver → Application.ThemeMode`
  一条赋值，且 `ApplicationThemeDriver.cs` 是全仓唯一抑制 `WPF0001` 的文件。
  `ApplyMotionPolicy` 整树递归、每窗口 `InvalidateVisual()`、`ApplyHighContrastPalette`
  键名启发式全部删除；高对比改为上游逐键映射表 `adaptation/04-high-contrast-substitution.md`。
  待补：源码文本闸口（门面里不得出现 `VisualTreeHelper`/`InvalidateVisual`）。
- **B2 逐窗主题岛** 🬡：`Window.ThemeMode` 存在，验证能否做 WinUI 那种
  "某元素/某窗口单独强制 Dark"的 `ThemeDictionary.SetKey` 等价物。
- **B3 触发器语法标准化**：状态名/部件名照抄 WinUI，但用 `ControlTemplate.Triggers`/`MultiTrigger`
  表达；动效允许 `Storyboard`+`BeginStoryboard`（public，能力比 `00` 里假设的强）。
  维护一张 `WinUI VisualState → Jalium trigger` 映射表，等同 ModernWpf 的
  `winui-visualstate-setters-audit.md`。
- **B4 像素回归基座 ✅（判据可信，阶段 2 第 0 步做完）**：`PixelHarness` 改为**共用一个宿主窗口**、
  换样本只换 `Content`、反复推帧直到两次直方图相同且非空才算稳定（`06` 的四条读数各对应一处）。
  `AstraPixelTests` 7 条全绿、0 skip：哨兵 DP、令牌跟主题、隐式样式→原生 Button 像素、
  本地 `Background` 压过 Setter、命名样式吃 accent 令牌、整窗 Light↔Dark、
  未样式的 Slider/ProgressBar **0 px 品牌绿**。仍欠：RTB 与上屏合成的一致性 🬡、
  半透明令牌与混合 DPI 下的计数一致性 🬡。
- **B5 动效令牌 🔺 现在是 reduced-motion 的前置**：模板过渡时长目前是标记里的字面量，
  删掉整树递归后 `ReduceMotion` 只能管住 Astra 自己代码驱动的动画（页面入场、导航指示器）。
  要做 `Metrics.jalxaml` 增加 `ControlFastDuration` 等时长/节拍令牌，
  模板写 `TransitionDuration="{ThemeResource …}"`（实测 0.167↔0.333 随主题变），
  `ReduceMotion` 只原地改这几个条目。
- **B6 焦点与自动化**：接 `FocusVisualAdorner`/`FocusVisualManager` 取代现在的 opacity 双环；
  普查各控件的 `*AutomationPeer` 缺口并逐个补 🬡。
- **B7 闸口**：`Test-AstraGates.ps1` 增 A6 键反查、B4 品牌绿污染检测、E4 "Gallery 未样式化控件"检测。

## C. 材质系统

先前"元素级亚克力做不到"的判断是**错的**（它走 Effect 不走 Brush），这条工作流因此成立。

- **C1 层语义映射表**：把 WinUI 的层刷语义（`CardBackgroundFillColor*`、
  `LayerFillColorDefault`、`SolidBackgroundFillColor*`、`AcrylicInAccountFillColor*`、
  `SystemControl*Acrylic*`）逐条落到「实色画刷」或「挂在表面元素上的 `BackdropEffect`」。
  映射表进 `docs/astra/adaptation/`，每条注明是实色还是真材质。
- **C2 🬡 背衬可调参数**：`AcrylicEffect`/`BackdropBlurEffect` 自身 `props=[]`，
  旋钮大概率在基类 `BackdropEffect` 上（Tint/Luminosity/Blur/Noise 之类）。
  材质工作流的第一个动作就是把这些参数与生效范围（窗口背衬 vs 子元素 vs Popup）测清楚，
  再决定 C1 里哪些能真做。
- **C3 高程 elevation**：WinUI 的 `ControlElevationBorderBrush` 是渐变描边 ——
  `LinearGradientBrush` 可用，把现在 Astra 的"实色描边适配器"升级成真渐变；
  阴影用 `DropShadowEffect`（有 BlurRadius/Direction/OffsetX/Y/Opacity）。
- **C4 窗口外壳**：`Window.SystemBackdrop` + `WindowBackdropType{None,Auto,Mica,Acrylic,MicaAlt}`
  + `TitleBar`/`TitleBarStyle`/`CustomTitleBarStyle`/`Left(Right)WindowCommands`，
  做 Fluent 标题栏与 Mica 客户区；必须保留原生最小/最大/关闭行为与非客户区拖拽。
- **C5 噪声**：WinUI 亚克力含噪点；现有 `BitmapEffect` 家族带 `Noise` 但不是表面噪声纹理。
  找不到等价物就记为未满足，不假称。
- **C6 `LiquidGlassParameters`（折射/色差/融合）** 确实存在，但 **WinUI 没有这个视觉**：
  只作为可选展示，默认路径一律不用，避免把 Fluent 做成另一种东西。

## D. 控件

每个控件进阶段前，先由探针给它一个**渲染分级**（不靠名字猜）：
`template-only` / `self-drawn-DP-honored` / `frozen-brush` / `live-ThemeColors`。
分级决定是否允许纯模板，以及 `Known Gaps` 怎么写。

`08` 之后这条有了可执行的问法，不必每个控件重跑探索：**先往应用级资源装一个同名哨兵刷看像素动不动**
（动 = `resource-driven`，能驱动），**再看它是跟 `Application.ThemeMode` 还是跟系统强调色**
（`ThemeColors` 本身是 public 只读、且不吃应用级资源，`Application` 上也没有任何 Accent/Color 属性 →
框架自绘的强调色推不进去，只能记边界）。ScrollBar 就是第一种，Slider/Toggle 是第三种的样本。

**顺序按"底座先于叶子"**，因为框架没有默认主题、未样式化的控件会露出框架外观：

1. **底座**：`ScrollViewer`、`ScrollBar`、`Thumb`、`Popup`、`ItemsControl`、`ContentPresenter`、
   窗口外壳。开工前的运行时摸底在 `07`；**ScrollBar 已按实测落地，见 `audits/scrollbar.md`**：
   它在代码里搭真的 `RepeatButton`/`Track`/`Thumb`/`Border`/`Path` 部件树，但
   `ControlTemplate`、`ScrollBar`/`RepeatButton`/`Thumb`/`ScrollViewer` 的隐式样式**全部测不动像素**，
   可达的只有命名刷 `ScrollBarThumb`/`ScrollBarTrack` 与 `ScrollBarStyle.Background`（后者有副作用，不发）。
   所以"原生控件优先重模板"这条对 ScrollBar 不成立，颜色层改走调色板生成刷；
   静止尺寸（栏 12、拇指 8）已经与上游一致并有断言，箭头静止可见与全部悬停/展开态进 Known Gaps。
   同一条判据现在可复用：`PixelHarness.RenderPart<T>` 能把框架内部件单独裁出来断言。
   **Popup/FlyoutPresenter 也已落地，见 `audits/flyout-presenter.md`**：26.10.9 没有
   `FlyoutPresenter` 类型，所以交付的是上游那一层**别名**（`FlyoutPresenterBackground`、
   `FlyoutBorderThemeBrush`、`FlyoutBorderThemeThickness`），ComboBox 弹层外壳与 ToolTip 边框
   都改成消费上游名。别名解析出的就是调色板那个对象（`Assert.Same` 有断言），所以翻主题/高对比
   能穿过别名到达像素。顺带把一个自造键改回上游真名：
   `FlyoutPresenterBackgroundBrush` → `AcrylicInAppFillColorDefaultBrush`，
   值不变（就是上游自己的 `FallbackColor`），高对比值也不再是我们的判断而是上游那一行。
   `FlyoutContentPadding` 与四个 `FlyoutTheme*` 尺寸键**不声明**（无消费点，声明即死键）。
   **ToolTip 也已按上游转录落地，见 `audits/tooltip.md`**：上游 10 个键逐个处置——
   5 个进 `ThemeResources/ToolTip.jalxaml`，`ToolTipBackground` 故意不声明（它指向上游根本没定义的
   UWP 遗留刷，宁可留缺口也不猜值），三个遗留 phone 时代的刷不声明（上游样式自己也不消费）。
   两处顺带修正：内边距 `8,6` → 上游的 `9,6,9,8`；圆角改为消费 `ControlCornerRadius`。
   两条新的框架结论对后面每一批都管用：
   **(a) 模板部件自己的 `Resources` 里放隐式样式是生效的**（`TextBlock.TextWrapping` 从部件树读回
   是 `Wrap`），上游/ModernWpf 靠这一招穿过 presenter 传文本属性；
   **(b) 给部件写一个它没有的属性，读取器不报错也不生效**（`ContentPresenter` 没有 `TextWrapping`，
   第一版模板"绿灯"通过却什么都没发生）——标记属性必须读回有效值才算证据。
   **ScrollViewer 宿主也已落地，见 `audits/scrollviewer.md`**：它是 `ContentControl`，
   `Template` 默认 null，部件树（两条 ScrollBar）由代码搭，上游模板结构搬不过来；
   上游 7 个键**一个都不声明**（宿主与分隔条在上游本来就是透明的，且没有消费点）。
   样式只带可达且安全的 setter：`IsTabStop=False`（框架默认 `True`，这是唯一真差异）、
   `Padding/BorderThickness=0`、`BorderBrush/Background=Transparent`、两个内容对齐。
   `Horizontal/VerticalScrollMode`、`Is{Horizontal,Vertical}RailEnabled`、`UseSystemFocusVisuals`
   **在本运行时没有对应属性**，`VerticalScrollBarVisibility=Visible` 则**故意不抄**
   （它依赖的 indicator 状态做不到，照抄等于把灰色箭头常驻铺到每个滚动面上）。
   另外量到两条框架陷阱：`Application.Resources` 增删条目会让 `{ThemeResource}` 重新解析、
   从此不指向调色板实例（表现是后面两条 Button 像素断言无声失败）；
   宿主设不透明 `Background` 会盖掉自己的滚动条（8800 全哨兵、灰色归 0）。
   **条目宿主基面已摸底，见 `adaptation/09`**：`ListBox` 的树是
   `Grid>Border>ScrollViewer>{ItemsPresenter>VirtualizingStackPanel>ListBoxItem…, ScrollBar×2}`——
   容器是每控件自己的 CLR 类型，**没有**通用的"条目基样式"可抄（上游也不是这样组织的），
   所以这一项交付的是通路证明：本地 `ItemContainerStyle` 能落到生成的容器上（Padding 读回 11），
   且默认虚拟化面板与内嵌 ScrollViewer 都在（后者自动吃我们的宿主样式）。
   另记一条坑：**裸 `ItemsControl` 放进宿主会挂住推帧循环**（60s 无回应，与裸 ScrollBar 同族），
   测试与 Gallery 都别放裸的。
   **窗口外壳也已按实测落地，见 `audits/window-shell.md`**：我先前判它"没有可主题化表面"是错的，
   错因是只查"我们的字典里有没有外壳键"，没查"框架自持的隐式样式消费哪些键名"——
   而 `CustomTitleBarStyle` 早在上面第 17 行的能力普查里就记着 public，我没读自己写的普查。
   正确的是：`TitleBar`/`TitleBarButton` 的框架样式消费 **8 个** `{ThemeResource}` 名字，
   应用级字典能压过框架默认值（与 `ScrollBarThumb` 同一条路），另有
   `Window.CustomTitleBarStyle`/`TitleBarStyleKey` 两个公开样式入口（前者已实测能落到那颗标题栏上）。
   `ThemeResources/TitleBar.jalxaml` 承接其中 6 个：底色走 `SolidBackgroundFillColorBaseBrush`
   （背衬默认 `None`，取背衬自己的回落实心），文字/字形走 `TextFillColorPrimaryBrush`，
   按钮三态走 `SubtleFillColor{Transparent,Secondary,Tertiary}Brush`——逐条对应上游
   `TitleBar_themeresources.xaml`（blob `b22068a79…`）里的同名语义。
   关闭键那 2 个钩子**故意不接**：上游该 commit 没有关闭键专用资源名，唯一候选
   `SystemFillColorCritical` 在 Dark 是文本用的浅红 `#FF99A4`，宁可留框架的红也不给没有依据的数。
   上游 40 个键里 18 个度量（14 个 `x:Double` + 4 个 `Thickness`）在本运行时读不了或无消费点，20 个别名里只承接有对应
   部件的那几个，返回键/窗格键/`Subtitle`/中央自定义内容/拖拽区/`ExtendsContentIntoTitleBar`
   **在本运行时没有对应公开面**，逐条进 1.0 的 API 缺口清单。静止高 32 与上游一致并有断言，
   按钮宽 46 与上游 40 不是同一种按钮、不声称一致。
   顺带删掉自造键 `ToolbarSurfaceBrush`（Light/Dark/HC 三处 + 生成器两行，全无消费点）。
2. **Button 族**（纵向样板，锁流程）。队列里的 **Compound 是我凭印象写的**：
   `19e3bdc3c` 的 `Button_themeresources.xaml` 只有 **Default / Accent / Subtle 三个样式**，
   没有 `CompoundButtonStyle` 也没有 `CompactButtonStyle`（`CompoundButton` 是
   CheckBox/RadioButton 的基类，归选择批）。实际清单：这三个 + Toggle/Repeat/Hyperlink 三个控件
   自己的样式 + `SplitButton`/`DropDownButton`（无原生类型 → 自有类型开端）。
   原计划起手要修的"模板根 Border 不吃本地 `Background`"**已被 `06` 证伪**：本地值经
   `{TemplateBinding Background}` 完整落到像素，Button 的哨兵断言也已进了 `AstraPixelTests`。
   **第一段（Default/Accent/Subtle）已落地，见 `audits/button.md`**：上游 36 行别名逐字转录进
   `ThemeResources/Button.jalxaml`（5 行偏差就地标注：1 条 Color→Brush 形式、4 条 elevation
   渐变改实底——上游自己的高对比分支也是实底），样式改成只吃上游键名，并补上原本缺的
   `HorizontalAlignment=Left`/`VerticalAlignment=Center`/`FontWeight=Normal`（**布局可见**：
   静止按钮不再横向拉满）。`BackgroundSizing`、`UseSystemFocusVisuals`、`FocusVisualMargin`、
   `ContentTransitions`、`AnimatedIcon` 五个名字在 26.10.9 的 61 个 dll 里**全查不到**，进 API 缺口清单。
   两条新框架事实对后面每一批都管用：**样式 setter 里的 `{ThemeResource}` 存的是惰性引用**
   （读出来是 `DynamicResourceReference{ResourceKey=…}`，所以触发器只能按键名断言，
   这也解释了翻主题能重绘已应用的样式）；**`GetBrush` 只看得见调色板键**，别名键要走应用级查找。
   **输入通路已在这一段建立（`spike/PointerProbe` + `audits/button-input-raw.txt`）**：
   user32 `SetCursorPos` 是真实 `WM_MOUSEMOVE`，一次 run 走通
   `IsMouseOver=True → 触发器 → 悬停刷 → 8296 px 悬停哨兵色 → 离手回休息位`；
   但它依赖物理鼠标、且同一时间有人用鼠标就会失败（run 3/4 的"属性绿像素品红"就是这个），
   **所以不进常跑闸口**。常跑的是无指针等价段：`A_swapped_brush_object_reaches_the_transitioning_surface`
   （首帧后换 `Background` 刷对象，带 `TransitionProperty` 的模板面照样采纳，`transitionedAdopted=True`）。
   顺带量到一条判据规则：**状态变化后的断言必须重复捕获到两张图一致**，固定帧数会采到过渡中间值
   （实测 `#E482E4`），已写进 `adaptation/06`。按下/键盘/触摸仍无证据：`press` 模式会真按键、
   可能点到用户桌面上的任何东西，未经同意不运行。
   **第二段（Toggle/Repeat/Hyperlink）也已落地**：另三份 blob 各自一份字典
   （36/12/12 行别名，偏差 7/2/0），样式全部改吃各自的上游键名，`RepeatButton` 从"借用 Button 样式"
   改成自己的 `DefaultRepeatButtonStyle`；勾选态与休息态都有了像素证据，文字色改走读回
   （量到一条硬边界：**字形不在这条离屏捕获通路里**，只画文字的捕获写 0 像素并且能挂住 UI 线程，
   基座因此加了不光栅化的 `PixelHarness.Build`）。三态的 12 条键按上游声明但**没有消费点**
   （`{x:Null}` 能否被解析未测），与 `CheckBox` 三态并到选择批。
   另记一条 API 形状：`ToggleButton`/`RepeatButton`/`ButtonBase` 在 `Jalium.UI.Controls.Primitives`，
   `Button`/`HyperlinkButton` 在 `Jalium.UI.Controls`——抄 WinUI 的 using 清单会编译不过。
3. **文本录入**：`TextBox`、`PasswordBox`、`NumberBox`、`AutoSuggestBox`、`RichEditBox`。
4. **选择**：**`CheckBox`/`RadioButton`（含 A4 冻结 glyph 实验）**、`ToggleSwitch`、`Slider`、
   `RatingControl`(自建)。
5. **表面与弹层**：Card/`Expander`/`InfoBar`(实时读 Accent，需重点验)/Flyout/`MenuFlyout` 全族/
   `CommandBar`/`ToolTip`；`ContentDialog`+`TeachingTip` 归入"对话框基座"一个独立里程碑。
6. **列表**：`ListView`/`GridView`/`TreeView`/`DataGrid`/`TreeDataGrid`(列拖拽实时读 Accent)/
   `ItemsRepeater`(自建)。
7. **导航**：`TabView`、`NavigationView`、`BreadcrumbBar`(自建)、`PipsPager`(自建)、
   `RadioButtons`(自建)。
8. **状态与图标**：`ProgressBar`、`ProgressRing`(自建)、`Divider`、`IconElement` 全族 +
   `Symbol` 764 码点的 cmap 命中验证、`AnimatedIcon` 替代方案。

> 关于外部建议的"五个纵向样板"（Button/TextBox/ToggleSwitch/NavigationView/ContentDialog）：
> 采纳前四个，**把 `ContentDialog` 换成 `CheckBox`** —— 前者需要一个全新的对话框基座
> （Jalium 只有 `MessageBoxDialog`），不是样板而是另一条地基；后者是我们已经用像素证明存在的缺陷，
> 且是 Fluent 界面最高频可见的元素。

## E. Gallery

Gallery 不只是演示，它是**这套架构唯一的回归面**：没有 Generic 主题意味着漏样式的控件
会静默露出框架外观。

- **E1 目录来自反射 + curated JSON ✅（`adaptation/10`）**：反射枚举**被我们赋了隐式样式的 `TargetType`**，
  与目录求差集 → 差集非空即失败。这就是"未样式化控件"的检测点。
  实测宇宙是 **17 个**（16 个在 `Styles/`，第 17 个是 `ThemeResources/Typography.jalxaml` 的隐式
  `TextBlock` —— 按目录结构去数就会漏，这条正是闸口第一次跑逼出来的）。
  另加反向一半：`MainWindow.jalxaml` 里能解析成 `Control` 的元素必须在目录里或在带理由的豁免表里
  （现状 3 条：`Window` 走外壳钩子、`ListBox`/`ListBoxItem` 欠列表批）。
  parity 三档 `audited`/`ported`/`own-type` = 6/8/3，每条声明引用的证据文件必须真实存在。
- **E2 页结构对齐 WinUI Gallery**：页头 + 示例卡 + 可运行源码 + 上游 parity 状态 + 该页用到的令牌清单。
  第一刀已落（`adaptation/12`）：Gallery 那条横跨整窗的头部带从窗口根移进内容列，pane 改为客户区顶起，
  第一项从 **126 DIP 抬到 90 DIP**；侧边栏几何第一次有了上屏量尺（pane 240、行高 36、指示条 3×16 全对）。
- **E3 三个系统级页面**（对应 A/C/B 三条工作流，而不是控件页）：
  **Tokens** 全色板网格 + 缺失键诊断；**Materials** 层/高程/背衬对照（开/关同屏）；
  **Motion** 时长节拍与 `ContentTransition` 对照。这三页是"系统"层是否自洽的直接证据。
- **E4 证据钩子**：每页可被测试渲染到像素（复用 B4），使"它真的画出来了"成为断言而非目视；
  启动参数支持隔离 profile 与关窗，遵守 AGENTS.md。

## 阶段与提交边界

| 阶段 | 内容 | 出口 |
|---|---|---|
| 1 | **B1 ✅ + A1 ✅（改混合模型）+ A6 ✅（反查部分）+ A2 部分 + B4 基座**（门面收敛、键消费点反查、高对比逐键映射、像素断言基座） | 已到：`dotnet test` 12/12、门面内无 `VisualTreeHelper`/`InvalidateVisual`、Light↔Dark 笔刷实例保持并有断言。仍欠：A2 别名转录、`resources/keys.md` |
| 2 | **像素归因 ✅（`06`）+ Button 纵向样板**（走完 9 步流水线，锁死后续样板） | 已到：判据可信；Button 两段（Default/Accent/Subtle + Toggle/Repeat/Hyperlink）审计 `audits/button.md` + **四份字典 96 行别名逐行身份断言**（82 行逐字、14 行偏差就地标注）+ 状态映射 19 条触发器按键名断言 + 布局/命名空间形状读回 + 休息/勾选/禁用/换刷四组像素证据 + 真指针悬停通路（`spike/PointerProbe`，一次性、不进闸口）+ **第 7 步 Gallery 页落地**（Buttons 页补齐 Toggle/Repeat/Hyperlink 家族卡，Overview 页加内嵌 ScrollViewer 与静置 ToolTip 表面卡，底部 parity 条改由 `Catalog.json` 驱动）。全套 60/60 全绿 0 警告 0 skip。仍欠：按下/键盘/触摸的输入证据、三态通路、`SplitButton`/`DropDownButton`、新卡片自身的可见结果（E4 逐页渲染） |
| 3 | **A3 + C2 🬡**（强调色三态、材质参数摸底） | 强调色改动能被像素断言（A4 已撤回，不再是出口）；C1 映射表可执行 |
| 4 | **D1 底座批 ✅ + E1 Gallery 目录闸口 ✅（`10`）+ E3 三个系统页 🬡** | 已到：ScrollBar、Popup/FlyoutPresenter、ToolTip、ScrollViewer 宿主、条目宿主基面、窗口外壳（TitleBar 钩子 + 背衬普查）六项，审计在 `audits/{scrollbar,flyout-presenter,tooltip,scrollviewer,window-shell}.md` + `adaptation/09-item-host-base.md`；外壳新增 4 条行为/读回断言 + 2 条像素/高对比断言（`AstraWindowShellTests` 6 条）。**E1 落地**：`Catalog.json` 17 条隐式样式宇宙 + 双向差集 + 名字必须命中唯一真类型 + 每条声明引存在的证据 + 反向"Gallery 不许用没样式控件"（3 条带理由豁免），6 条新断言，全套 60/60 全绿 0 skip，调色板 102 刷漂移 checked=True。仍欠：Thumb（与悬停/拖拽输入证据同批）、外壳的输入证据与材质合成；Tokens/Materials/Motion 三页有证据 |
| 5+ | D2…D8 按批推进；C1/C3/C4 材质随批落地。**已开工：目标阶段 3 的选择批**（`adaptation/11` 量清三态语义，`AstraSelectionTests` 7 条断言，ToggleButton 12 条 indeterminate 键接上消费点）。**选择批第一段落地**：`CheckBox` 72 行 + `RadioButton` 40 行别名逐字转录（0 与 9 处替换，就地标注）+ 两份样式全量改吃上游键名（12 格 / 8 格状态矩阵，每槽按键名断言）+ 新结构闸口 `Transcribed_control_rows_are_read_by_a_template`（逐行反查别名行有无消费点，112 行全有）+ 审计 `audits/checkbox-radiobutton.md`。全套 82/82 全绿 0 警告 0 skip。顺带量到 `Setter.PropertyName` 这条延迟解析契约（跨部件 setter 的 `Property` 在读取期是 null，见审计）与"勾形确实进像素"（更正目录里旧的 capture-path gap） | 每批全 9 步 + 全闸口 |
| 1.0 | CLR API 清单 + 公开资源键清单冻结 + 每控件审计 + Light/Dark 像素证据 + 真实键鼠触证据 + 仅 NuGet 消费者冒烟 | 见 `docs/astra/resources`、`audits`、`testing` |

## 不声称清单（写进每个审计文档，不许被"构建通过"替代）

- 不声称高对比度 parity：公开管线无驱动入口，只有上游逐键映射 + 三个自加键的自行判断，
  且逐控件的 HC 视觉状态覆盖未移植（见 `adaptation/04`）。
- 不声称 `Application.ThemeMode` 稳定：它是 `[Experimental("WPF0001")]`，框架明说将来可能改或删；
  我们钉在 26.10.9 并有运行时断言，删除即编译失败（单文件），行为退化即测试失败。
- 不声称跟随系统"减弱动画"设置：Windows 侧无 API。
- 不声称逐位一致的上屏合成：RTB 离屏与上屏一致性未证。
- 不声称 `Symbol` 全 764 码点可用：需逐个 cmap 命中验证。
- 不声称液态玻璃/折射是 Fluent 的一部分。
- 不声称 Button 状态已完成：`PointerOver` 除"触发器带的是上游键名"这层结构证据外，多了一次真指针像素
  （`SetCursorPos` → 悬停哨兵色 8296 px），但它依赖物理鼠标、被人同时用鼠标打断过，**不在闸口里**；
  `Pressed` 仍只有结构证据，键盘激活与触摸完全没有证据。焦点框是我们自绘的，
  `BackgroundSizing`/`AnimatedIcon`/`UseSystemFocusVisuals` 在本运行时无对应属性（`audits/button.md`）。
- 不声称窗口外壳完成：标题栏只承接了框架 8 个钩子里的 6 个，悬停/按下/拖拽/三个点击事件无输入证据，`SystemBackdrop` 只钉住了枚举与默认 `None`，Mica/Acrylic 的合成效果本环境无法验证；返回键、窗格键、`Subtitle`、中央自定义内容、`ExtendsContentIntoTitleBar` 没有对应公开面（`audits/window-shell.md`）。
- 不声称弹层是 acrylic 材质：`FlyoutPresenterBackground` 走的是上游自己的 `FallbackColor` 实底，
  运行时没有 `AcrylicBrush` 类型（见 `audits/flyout-presenter.md`）。
- 不声称文字颜色有像素证据：**字形不在这条离屏捕获通路里**（只画文字的捕获写 0 像素、且慢到能挂住
  UI 线程），文字色一律只证明到"生成出来的文字元素接上了调色板那个实例"这一层（`adaptation/06`）。
  这条只管**字体字形**，不管几何：选择批已按名字重做那条归因——把表面令牌与标记令牌染成两种哨兵色
  一次拍摄，勾形（几何 `Path`）自己贡献 12 实心 + 22 抗锯齿像素，`Check_mark_follows_the_selected_accent`
  那句"名字过强"的批评就此了结（`audits/checkbox-radiobutton.md`）。
- 不声称像素 harness 能连拍：一次 `_fixture.Run` 里做两次离屏捕获（第一张还是近黑、永远"不稳"）
  实测顶穿 fixture 的 60 秒看门狗；E4 逐页渲染要么一次一页，要么先把 `Capture` 的轮次与代价修掉。
  这条未归因，别把它当成"离屏通路不可信"。
- 不声称三态是**操作得出来**的：`{x:Null}` 触发条件实测有效、12 条 `ToggleButton*Indeterminate*` 键已有消费点，但断言全部是直接设属性得到的；真实点击循环与混合条的像素归因仍未测。附带一条会咬人的实测：`IsChecked="{x:Null}"` 写在标记里得到的是 `false`，三态只能在代码里设（`adaptation/11`）。
- 不声称硬件触摸笔与混合 DPI 已经过真机验证。
- 不声称 Gallery 每页画对了：目录闸口证明的是"样式宇宙与目录一致、声明有存在的证据"，
  运行时冒烟只看到 Overview 一页（parity 条文案确实来自 `Catalog.json`，故读取通路成立）；
  新加的家族卡与表面卡**可见结果未目视、未断言**，等 E4 逐页进程内渲染（`adaptation/10`）。
- 不声称目录里的 `gaps` 是全集：它是手写欠账快照，闸口只验"存在且被引用"，不验完整；
  推进控件时 gaps 要人工减项，这是该机制的已知弱点。
- 方法边界：**屏幕坐标点击一律不用**。本轮一次冒烟用 `SetCursorPos`+`mouse_event` 点自己的窗口，
  但前景锁下 `BringWindowToTop` 不保证生效，点击落到了用户另一个应用上（截图可证矩形里盖着别的窗口）。
  Gallery 冒烟从此只做"启动 → 截图 → 关窗"；输入证据只在进程内做，`press` 注入仍未经同意不跑。
- 方法边界（同轮新增）：**屏幕捕获脚本必须先 `SetProcessDpiAwarenessContext(-4)` 并打印
  `GetDpiForWindow`**。这台机器是 175%，不感知 DPI 时 `GetWindowRect` 给的是 DIP（1100×820），
  按它去 `CopyFromScreen` 只截到真实窗口的左上约 80% 且横向偏 153px——看起来像"布局溢出/左边有重影"，
  其实是捕获坐标错了。另外捕获前要确认没有系统对话框压在目标窗口上：本轮 Windows 安全中心的
  scrim 把强调色压到阈值以下，指示条一度"测不到"。
- 不声称侧边栏**交互**对了：两轮都只有静息几何量尺。悬停/按下/键盘焦点无上屏证据，汉堡后的折叠动画
  与指示条 200/400ms 移动动画无逐帧证据。折叠态那条真缺陷已修并有断言：`ScrollViewer` 的 `Auto`
  **即使不画滚动条也扣 12 DIP 视口**，把 48 DIP 折叠 pane 的项挤成 28，高亮框比 40 DIP 图标列还窄，
  图标右半截露在框外；改 `Hidden`（实测不扣且仍可滚，`A_hidden_bar_still_lets_the_pane_scroll` 断言）
  后高亮框回到 4…44 DIP（`adaptation/12`）。代价：长列表没有可拖滑块——本框架没有覆盖式滚动条。
- 不声称 90 DIP 就是 WinUI 的 52 DIP：余下 42 DIP 是汉堡带，要等窗口外壳把汉堡接进标题栏才压得掉。
  NavigationView 的九步出口仍欠 `audits/navigation.md`。
- 一条**已撤回的改动**：把 83ms `TransitionProperty` 从 Button/导航扩散到 CheckBox/RadioButton/
  ComboBox/ComboBoxItem/TextBox/BottomStroke（6 处）。实测过渡在飞期间 `Background` 是**新造的插值实例**
  （`sc#0.66,0,0.12,0.43`），直接顶穿别名层"处处同一实例"的不变量，而 WinUI 这些状态填充本就是
  `KeyTime="0"` 离散换刷。三个样式字典已回退到 HEAD，全套 67/67 复绿。既有三处（Button/NavigationView/
  ToggleSwitch）不动，但要知道它们的画刷实例同样只在稳态等于调色板实例。
