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
   **第一段（Expander + InfoBar）已落地，见 `audits/expander.md`、`audits/infobar.md`**：Expander 走
   原生重模板，三条部件名是功能契约（头部由控件自己接 `MouseDown`，所以头部不能再放按钮；`PART_Chevron`
   必须是 `Shapes.Path`，固定 +90°）；InfoBar 的模板要 `ContentControl.UseTemplateContentManagement()`
   这一行才生效，而框架没在它的构造函数里调用，所以交付 `FluentInfoBar : InfoBar` 一行子类
   （实测缺口，不是抄 WinUI 类型名）。**"实时读 Accent"这条按实测更正**：四类严重级别的底色走的是
   `SystemFillColor*BackgroundBrush` 调色板行，不是 `ThemeColors`，因此桥接可覆盖并有哨兵断言；
   框架自绘路径下那份 `#2D2D37` 兜底已在模板化后归零并钉成断言。
   **第二段（菜单族）已落地，见 `audits/menu-flyout.md`**：能不能重模板是**分类型**的——flyout 一族与
   `Menu`/`ContextMenu`/`MenuBarItem` 都吃我们的模板并解析到调色板实例，而 `MenuItem` 存下 `Template`
   却永不实例化、`MenuBar` 根本没有模板可装，两者只拿只上色的样式；上游 81 条主题行只落 26 条别名，
   其余按"没有 DP 就没有格子"逐条给理由不发布。
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
  **第一块地基已就位**（NumberBox 批）：`--page <id>` 让任意一页在启动时挂上屏，
  冒烟不再只看 Overview；仍欠逐卡入镜（滚动/裁剪）与进程内像素断言。

## 阶段与提交边界

| 阶段 | 内容 | 出口 |
|---|---|---|
| 1 | **B1 ✅ + A1 ✅（改混合模型）+ A6 ✅（反查部分）+ A2 部分 + B4 基座**（门面收敛、键消费点反查、高对比逐键映射、像素断言基座） | 已到：`dotnet test` 12/12、门面内无 `VisualTreeHelper`/`InvalidateVisual`、Light↔Dark 笔刷实例保持并有断言。仍欠：A2 别名转录、`resources/keys.md` |
| 2 | **像素归因 ✅（`06`）+ Button 纵向样板**（走完 9 步流水线，锁死后续样板） | 已到：判据可信；Button 两段（Default/Accent/Subtle + Toggle/Repeat/Hyperlink）审计 `audits/button.md` + **四份字典 96 行别名逐行身份断言**（82 行逐字、14 行偏差就地标注）+ 状态映射 19 条触发器按键名断言 + 布局/命名空间形状读回 + 休息/勾选/禁用/换刷四组像素证据 + 真指针悬停通路（`spike/PointerProbe`，一次性、不进闸口）+ **第 7 步 Gallery 页落地**（Buttons 页补齐 Toggle/Repeat/Hyperlink 家族卡，Overview 页加内嵌 ScrollViewer 与静置 ToolTip 表面卡，底部 parity 条改由 `Catalog.json` 驱动）。全套 60/60 全绿 0 警告 0 skip。仍欠：按下/键盘/触摸的输入证据、`SplitButton`/`DropDownButton`、新卡片自身的可见结果（E4 逐页渲染）。三态通路已在选择批第七段结清（`audits/togglebutton.md`），那一段同时补了本行四条 hover 前景与两条自有边框厚度行 |
| 3 | **A3 + C2 🬡**（强调色三态、材质参数摸底） | 强调色改动能被像素断言（A4 已撤回，不再是出口）；C1 映射表可执行 |
| 4 | **D1 底座批 ✅ + E1 Gallery 目录闸口 ✅（`10`）+ E3 三个系统页 🬡** | 已到：ScrollBar、Popup/FlyoutPresenter、ToolTip、ScrollViewer 宿主、条目宿主基面、窗口外壳（TitleBar 钩子 + 背衬普查）六项，审计在 `audits/{scrollbar,flyout-presenter,tooltip,scrollviewer,window-shell}.md` + `adaptation/09-item-host-base.md`；外壳新增 4 条行为/读回断言 + 2 条像素/高对比断言（`AstraWindowShellTests` 6 条）。**E1 落地**：`Catalog.json` 17 条隐式样式宇宙 + 双向差集 + 名字必须命中唯一真类型 + 每条声明引存在的证据 + 反向"Gallery 不许用没样式控件"（3 条带理由豁免），6 条新断言，全套 60/60 全绿 0 skip，调色板 102 刷漂移 checked=True。仍欠：Thumb（与悬停/拖拽输入证据同批）、外壳的输入证据与材质合成；Tokens/Materials/Motion 三页有证据 |
| 5+ | D2…D8 按批推进；C1/C3/C4 材质随批落地。**已开工：目标阶段 3 的选择批**（`adaptation/11` 量清三态语义，`AstraSelectionTests` 7 条断言，ToggleButton 12 条 indeterminate 键接上消费点）。**选择批第一段落地**：`CheckBox` 72 行 + `RadioButton` 40 行别名逐字转录（0 与 9 处替换，就地标注）+ 两份样式全量改吃上游键名（12 格 / 8 格状态矩阵，每槽按键名断言）+ 新结构闸口 `Transcribed_control_rows_are_read_by_a_template`（逐行反查别名行有无消费点，112 行全有）+ 审计 `audits/checkbox-radiobutton.md`。全套 82/82 全绿 0 警告 0 skip。顺带量到 `Setter.PropertyName` 这条延迟解析契约（跨部件 setter 的 `Property` 在读取期是 null，见审计）与"勾形确实进像素"（更正目录里旧的 capture-path gap）。**选择批第二段落地**：`ThemeResources/TextBox.jalxaml` 从 blob `6934b646` 声明 16 行（13 别名 + 3 度量，3 处值替换就地标注），`TextBox`/`PasswordBox` 两份样式改吃上游键名并补出四格状态（Normal/PointerOver/Focused/Disabled，上游文本框**没有 Pressed 格**）；先反射量出 26.10.9 的真实属性面再决定转录范围（`SelectionBrush`/`CaretBrush` 有、`PlaceholderText`/`Header` 没有，而 `ComboBox` 有 `PlaceholderText`），并把这条前提钉成闸口测试。**新增两条通用闸口**：`Style_setters_name_properties_the_controls_actually_have`（不带 TargetName 的 setter 属性名必须真的存在于目标类型，防静默丢弃）、消费点闸口扩到 3 份字典。同时删掉自造的 `TextControlPadding`=`11,5,11,6`，换成上游 `TextControlThemePadding`=`10,5,6,6`。**新测到的架构账单**：把原生默认交回 Jalium 之后，框架在个别状态上设的是**本地值**，优先级高于样式 setter 与模板触发器——禁用态 `TextBox.Foreground` 读回 `#FFAEAEB2`、`PasswordBox` 静息 `Background` 读回 `#D9FFFFFF`，两处都写成 `Assert.NotSame` 钉住，不允许再用"setter 上有键"当像素结论。全套 **104/104 全绿 0 警告 0 skip**，调色板三档 checked=True；键盘焦点态第一次有读回证据（真 `Focus()` + `1,1,1,2`）。**选择批第三段落地（Slider）**：`ThemeResources/Slider.jalxaml` 从 blob `3804392c` 转录 23 个上游行名中的 20 行 + 3 行度量（3 行故意不抄并各配一条反射断言：无 `Header`、两个枚举都没有 `Inline`），横竖两份模板全量改吃上游键名并接上真正的 `TickBar` 部件，自造键 `SliderThumbStrokeBrush` 连同其 `Light/Dark/HighContrast.map` 三处行一起删除（调色板 102→101 刷，映射表同步）。这一段真正的产出是**两条基座缺陷**：① 作为字典资源存放的 `ControlTemplate`，其每个 `Trigger.Property` 读取期为 null——格子在、setter 在、条件永不匹配，Slider 的 8 格因此全程失效（`ButtonSurfaceTemplate` 同病，一并改内联），新闸口 `State_cells_are_not_written_into_a_keyed_template_resource` 在 markup 层挡住；② `PixelHarness.Pump` 的看门狗在 `System.Threading.Timer` 回调里取 `Dispatcher.CurrentDispatcher`，释放的是线程池上没人跑的调度器，于是"不带动画的状态变化"（聚焦一个 Slider）必挂 60 秒——选择批两次"未归因超时"就是它。修好后立刻多出一条过去做不到的证据：`A_focused_slider_raises_its_focus_ring` 与 `A_focused_button_raises_its_focus_ring`（真 `Focus()` → `Opacity=1`），并补上文本框聚焦格的像素断言，同时撤销 `audits/textbox-passwordbox.md` 里那条"不声称聚焦态有像素证据"。行为侧另有：`TickPlacement` 四值 × 横竖的刻度条可见性与尺寸、值跟随（`PART_SelectionRange` 宽 0/51/102/153/204 与 `PART_Thumb.Margin` 逐值相等，实测出这两个部件名是框架契约）、禁用格 6 个实例 + 内芯 12 DIP。全套 **137/137 全绿 0 警告 0 skip**，Gallery 冒烟：窗口上屏、优雅退出、无残留进程（Inputs 页未目视）。**选择批第四段落地（ComboBox）**：`ThemeResources/ComboBox.jalxaml` 从 blob `4578f4c9` 的 68 行现行请求转录 **59 行**（58 别名 + `ComboBoxDropdownBorderPadding`），**9 行故意不抄且每行配一条反射或计数断言**——两条 Header、一条 light-dismiss、一条在上游就是死键（参考文件里 `grep` 计数为 3 且都在自身区块）、一条 `ComboBoxBackgroundUnfocused`、`ComboBoxPlaceHolderForegroundDisabled`（框架本地值压过，见不声称清单）与三条 item `*SelectedUnfocused`；3 处值替换就地标注。`ComboBox` 样式 15 格 / `ComboBoxItem` 9 格，全部内联（吸取 Slider 教训）并逐格按键名断言，其中 `SelectedIndex=-1 + IsKeyboardFocused + IsMouseCaptureWithin` 是**第一条三条件 MultiTrigger**，实证水合正确；上游 `ComboFocus` 描边环换成 WinUI 实际的 `HighlightBackground` 光晕（`2,7` 内缩 + 透明度格），箭头由嵌套模板的 `DropDownGlyph` 承接并证明**框架靠改写 `Path.Data` 翻转**。框架所有权四条先反射量清再写断言：部件名 `PART_ToggleButton`/`PART_Popup`/`PART_EditableTextBox`/`PART_SelectionPresenter`/`PART_DropDownArea`、可编辑/演示器的 `Visibility` 与 `Text` 双向同步、`PART_Popup.Width` 等于控件宽度、收起时自己的树里**没有** `PART_PopupBorder`（打开才嫁接进 `OverlayLayer`，故下拉结论必须从宿主窗口读）。产出两条**新的基座契约**（`adaptation/00` S0-g）：`Trigger Value=""` 永不匹配、`SelectionBoxItem` 装的是占位串，两者都属"结构全绿但屏幕上不动"的静默失效，因此本批把"每格必有读回"升为规则。顺带更正前一批一处误判：`ComboBoxPadding` 是**上游真键、我们抄错了值**（现改 `12,5,0,7` 并从 `Metrics.jalxaml` 迁移），不是自造键。消费点闸口扩到 5 份字典，新测 `AstraComboBoxTests` **71 条**（58 行逐字身份 + 度量 + 省略理由 + 两份模板逐格 + 样式行 + 框架占有 + 打开/未选/按下聚焦/可编辑禁用 + 选择药丸 + 静置像素与 Light↔Dark + 逐格禁吃调色板直链）。全套 **209/209 全绿 0 警告 0 skip**，调色板三档 checked=True，Gallery Selection 页加"Combo boxes"卡（四例 + 两条明说主张边界的说明，可见结果未目视）。**选择批第五段落地（NumberBox）**：这一段先推翻自己的前提——目标写的是"NumberBox(自有)"，但实测 `new NumberBox()` 是 `Style == null` 而 **`Template != null`（框架代码构建的默认模板，部件名 `OuterBorder`/`PART_ContentHost`/`PART_UpSpinButton`/`PART_DownSpinButton` 全在）**，`Style` 的 `Template` setter 能整个换掉它，所以按 AGENTS.md 的"只有证明的行为缺口才自有类型"这条，NumberBox **走原生重模板**，自有类型不在本批开工；`adaptation/01` 补了更正节（"零默认样式 ≠ 零默认外观"），`Report-Control-Vacuum.ps1` 的自绘列加了免责声明并按新判据重算（`done 11→13`、`HARDGAPS 43→41`，NumberBox 与 ScrollViewer 退出硬缺口名单）。资源侧 `ThemeResources/NumberBox.jalxaml`（blob `3b532d4b`）13 行抄 8 行（4 度量 + 4 别名），5 行不抄且各有理由（4 个 `x:Double` 度量本 reader 解析不了、`NumberBoxPopupShadowTheme` 是 `ThemeShadow` 对象）；顺带把上一批留在 `TextBox.jalxaml` 的 11 行补齐到 21 行有消费点（`TextBoxTopHeaderMargin` + `TextControlHeaderForeground*` + 8 条 `TextControlButton*`——它们正是 spin 按钮要的那批）。**上游模板局部的 `RepeatButton*` 别名块故意不搬**：搬到应用级会抢走 `ThemeResources/RepeatButton.jalxaml` 的同名行（清单里后入者胜），实测打挂两条 Button 断言；现在 spin 按钮直读这些行的目标令牌，并钉一条 `The_repeatbutton_names_still_belong_to_the_repeat_button` 防回归。样式侧 `NumberBox` + 内联 `NumberBoxSpinButtonStyle` 两份模板共 7 格（含第一条**枚举条件**格 `SpinButtonPlacementMode=Inline/Hidden/Compact` 与 `MultiTrigger(Compact + IsKeyboardFocusWithin) → UpDownPopup.IsOpen`），三条框架契约进断言：`PART_ContentHost` 必须是**面板**（写成 `ContentPresenter` 时框架不嫁接 `TextBoxContentHost`、捕获塌成两色）、框架在 spin 按钮上写本地 `BorderThickness=1,0,0,0`（我们的 `0,1,1,1` 到不了，第四处本地值账单）、生成的标题文字元素带框架本地 `#FF1D1D1F`（标题两行只到 presenter）。新测 `AstraNumberBoxTests` **27 条**，全套 **237/237 全绿 0 警告 0 skip（17 秒）**，调色板三档 checked=True，消费点闸口扩到 6 份字典。**Gallery 侧本轮补了 E4 的第一块地基**：`--page <id>` 启动参数（`SetStartPage`/`NavigateToPage`，导航落在 `Loaded` 所以主题广播不丢），于是"只有 Overview 会上屏"这条老限制结束——同机对比 `overview` 与 `inputs` 两页截图，内容区 161 000 采样点 **39% 不同**，Inputs 页的 parity 条与本批写进 `Catalog.json` 的 NumberBox gap 文案确实读到了屏幕上；NumberBox 卡本身仍未入镜（在第一节之下 + 一个第三方防火墙授权框压在页面中部，未点击、非本仓库进程）。**选择批第六段落地（AutoSuggestBox，宿主替换成原生 `AutoCompleteBox`）**：这一段又是先推翻前提再动手——目标写的是"AutoSuggestBox(自有)"，本运行时确实没有 `AutoSuggestBox` 类型，但原生 `AutoCompleteBox : TextBoxBase` 在（11 个 DP、5 个事件、`FilteredItems`/`ItemFilter`/`PlaceholderText`/`IsDropDownOpen`；没有 `Items` 集合、没有 `QueryIcon`/`Header`/`Description`），它的外观换得动、行为只差一处，所以按 AGENTS.md **外观走原生重模板、行为缺口留给后续那层薄自有类型**（不在本批开工）。这一段真正的产出是**弹层部件树契约**（`adaptation/00` 新增 S0-i）：`PART_Popup` 没有 Child 就开成空 20 DIP 根、`PART_DropDownItemsHost` 必须是**面板**（写成 ItemsControl 框架永不填）、`PART_Popup.Width` 是框架本地值；资源侧 `ThemeResources/AutoSuggestBox.jalxaml`（`7cd762eb` + `39e8d87f` + generic `def26a61`）19 行抄 5 行（2 别名 + 3 度量，上游 `AutoSuggestListMargin`/`AutoSuggestListPadding` 名字与落点相反这件事原样保留），14 行不抄且逐行给理由（6 个 `x:Double`、6 行上游本无消费者、其余是宿主没有查询按钮/删除按钮/标题/light-dismiss 面）；文本半不重复定义，直读 `TextBox.jalxaml` 的 `TextControl*`（上游 `AutoSuggestBoxTextBoxStyle` 本就是 `DefaultTextBoxStyle` 的手抄分叉）。样式 + 内联模板 3 格，与上游同形（28 个 `DiscreteObjectKeyFrame` 全 `KeyTime="0"`、`TransitionProperty` 出现 0 次）。三条新结论：① **弹层令牌第一次进像素**（哨兵是该裁剪第一大色 1092 px），同时量到这个裁剪包含带框架本地渐变的条目，所以 `Stable` 与"品牌绿不出现"在弹层裁剪上**不可断言**（S0-j）；② S0-g 那条"空串判据永不匹配"被**降级**——同一写法在静息真的为 `""` 的 `AutoCompleteBox.Text` 上会触发并有断言，规则收紧成"判据只按属性读回，不按语法推断"；③ **换模板会丢框架行为**：`UpdateTextOnSelect`（选中建议回填文本）实测消失，已写成断言而不是缺口清单里的一行字。框架占有再记两笔（第六、七处）：条目容器 `Background` 是本地渐变、禁用态控件前景是框架 `#FF636366`。新测 `AstraAutoSuggestBoxTests` **30 条**，全套 **268/268 全绿 0 警告 0 skip**，消费点闸口扩到 7 份字典，`Report-Control-Vacuum.ps1` 重跑（`outOfScope 106→105`，`AutoCompleteBox` 进"已样式但 ModernWpf 名单没有"的差集；`AutoSuggestBox` 仍留在 D 节，因为行为层确实还欠一个类型）。Gallery Inputs 页加"Suggested text"卡（可编辑 + 禁用两例，卡上两条明说主张边界的文案）。**同批收尾**：`ThemeResources/Metrics.jalxaml` 也进消费点闸口（第 8 份），当场删掉三行——两行自造键 `RadioButtonContentMargin`/`MenuFlyoutItemCornerRadius`（上游根本没有这两个名字，间距是样式 setter）与一行上游真名但没人读的 `NavigationViewItemButtonMargin`，三条 `audits/*.md` 里的"等收尾"记录同步结清；`NavigationViewItemCornerRadius` 留着但记为仍欠（我们有消费点、上游无此名，等 NavigationView 批定案）。闸口重跑 **269/269 全绿 0 警告 0 skip**（多的那一条就是 Metrics 这份新用例）。**选择批第七段落地（ToggleButton，阶段 3 收尾）**：这一段没有新字典（37 行 Button 批就抄完了），产出全在"把看不见的主张变成断言"。① `{x:Null}` 在**样式**格子里命中且可逆——Button 批留的"本运行时没有这项证据"就此结清（CheckBox 批证的是模板格子，两条解析路径不能互推）；而上游把 `*Indeterminate*` 映射到与休息位**同名**的调色板刷（同一实例），所以混合态与休息态在像素上**天然不可区分**，那条原有的 indeterminate 像素断言其实是代理信号：能断的只剩"格子命中"（用只带 null 条件的探针样式，把混合底色指到控件上没人读的强调色刷）与"实例接上"（挂载读回），`audits/button.md` Known Gap 8 的两个"仍未测"当场结清一个、更正一个。② 交互通路找到不需要合成指针的钥匙：`ToggleButtonAutomationPeer` + `IToggleProvider.Toggle()` 就是框架自己的 `OnClick→OnToggle`，两态 false→true→false、三态 false→true→null→false、三个事件按次触发、禁用时抛 `InvalidOperationException("Cannot toggle a disabled control.")`（与 WinUI 该模式在禁用元素上抛 `ElementNotEnabledException` 同侧契约）全部进断言；`new ToggleButton()` 休息位是 `false` 不是 `null` 也钉住——不钉的话哪天框架改成 null 起步，屏上一点都看不出来。③ 键消费闸口 8 份→**12 份**（Button 全族四份进闸），当场抓出 8 条没人读的行：4 条 `…ForegroundPointerOver`（上游每个 hover 态都写 Background/BorderBrush/**Foreground** 三条，我们的格子只写两条）补齐；`RepeatButtonBorderThemeThickness` 在上游自己也是死键（整棵参考树只有两个资源加载测试夹具引用它），按 AutoSuggestBox 那 6 行的同一处置**删行**而不是接个假消费点；`ToggleButtonBorderThemeThickness` 与 `HyperlinkButtonBorderThemeThickness` 是真消费点，接上。`TitleBar.jalxaml`/`FlyoutPresenter.jalxaml` 明确**不进**闸口并写明原因：它们的读者是框架自己的窗口外壳与弹层样式，应用级别名覆盖有效，但本程序集里没有消费点可反查。④ 新的宿主差异账单：**样式格子输给本地值**（`IsChecked=true` 而本地 `Background` 仍赢），上游 VisualState 故事板恰好反过来压过本地值——应用给开关设过本地底色，在 WinUI 勾选后变色、在这里永不变色；TextBox 批那张"框架本地值压过样式"的账单由此延伸到我们自己格子的一侧，按实测钉成断言。⑤ 新测 `AstraToggleButtonTests` **22 条**（12 态全表 33 个消费点 + 逐格禁吃调色板直链 + 三态实例读回 + 勾选×禁用"后写的赢" + 混合探针的标记与像素两半 + 循环与事件 + 焦点框 + 静息底像素 + Light↔Dark + 品牌绿 0），选择批里那份 indeterminate 键名表是它的真子集，删掉并给保留的那条像素读数写明理由。审计 `audits/togglebutton.md`。Gallery Buttons 页补三态开关（`IsThreeState` 走标记、null 走代码，因为标记里的 `IsChecked="{x:Null}"` 解析成 `false`），并把"混合态与休息态同色、第三态只有读数看得见"写到页面上；目录里那条"勾选 glyph 与混合条像素未归因"是**误挂**（ToggleButton 没有 glyph，那是 CheckBox 的事），换成四条真 gap。闸口重跑 **294/294 全绿 0 警告 0 skip**（+22 新类、+4 闸口用例、−1 并入的子集用例）。仍欠：AutoSuggestBox 的 `QuerySubmitted`/`QueryIcon`/`Header`/`Description`/light-dismiss 与那层自有类型；ToggleButton 的 `Pressed`/键盘/触摸三条输入通路像素（Task #13）、`PointerUp`/`PointerDown` 主题动画与共享模板那条自加的 83ms 淡入淡出（动效批）。**阶段 2 收尾批落地（SplitButton + DropDownButton）**：先用一个探针把这条批的前提全量问清（`spike/SplitButtonProbe`，3129 个导出类型名 + SplitButton 的完整属性面 + 挂载树 + 弹层签名），当场否掉目标里两条假设——`ContentDialog`/`Expander`/`InfoBar`/`Menu` 全族/`CommandBar` 都是**原生已有**（阶段 4 不该按"自有类型"开工），而 `GridView` 是 `ViewBase` 不是控件；`DropDownButton`/`ToggleSplitButton` 确实没有。资源侧 `ThemeResources/SplitButton.jalxaml`（blob `e9601d1f`）30 行抄 17 行（15 别名 + 边框厚度 + `SplitButtonPadding`），两条描边按按钮族既有判断换成 `ControlStrokeColorDefaultBrush`（上游的 `ControlElevationBorderBrush` 是渐变，不能随原地重染色走），13 条 `*Checked*` 与 `SplitButtonInAppBarUnfocusedPointerOver` **不转录**并配反向闸口 `A_row_with_no_consumer_is_not_published`（前者属不存在的 `ToggleSplitButton`，后者属阶段 4 的 CommandBar）；`ThemeResources/DropDownButton.jalxaml`（`c1e68023`）整份就 3 行箭头前景，表面颜色上游用的也是 `Button*`。样式侧原生 `SplitButtonStyle` + 两份半区样式 + 自有类型 `FluentDropDownButton : Button`（`Flyout` + `IsExpanded`，走 `Button` 的 Click/Command 语义），上游 15 个状态里 6 个可达、9 个 `Checked` 族与 `FlyoutOpen`/`TouchPressed` 无驱动。产出四条基座契约（`adaptation/00` **S0-l**）：① **模板部件名是功能契约**——半区必须叫 `PrimaryButton`/`SecondaryButton`，改名之后建树、上色、布局全对而点击彻底失效，所以闸口正反两向都测；② **`FlyoutBase.IsOpen` 是只读 CLR 属性不是依赖属性**，`SplitButton` 也没有 `IsDropDownOpen`、`CreateAutomationPeer()` 返回空，打开态既绑不了也当不了格子条件（不伪造驱动、不反射私有字段，直接写进缺口）；③ **隐式样式从不写进 `FrameworkElement.Style`**（Button/Toggle/Hyperlink/SplitButton 挂载后 `Style` 全 null 而像素是我们的），于是"样式生效"的读法换成**模板对象身份**；④ **星形列尊重子元素对齐**——共享布局样式的 `Left` 把主半区缩到 53.09×34.78（控件 220×36），4832 像素直接不画，修成显式 `Stretch` 并补 `Both_halves_stretch_into_their_columns`。另两条"框架替我们做了事"：照抄上游把 `Command` 绑到主半区会**执行两次**（实测 2 次，改成不绑并断言恰好一次），没有 `Flyout` 时框架**自己禁用次半区**（读回 `ControlFillColorDisabledBrush`）。新测 `AstraSplitButtonTests` **52 条**，消费点闸口 12 份→**14 份**字典，Gallery Buttons 页加"Split and drop-down"卡（三例 + 边界说明），目录里 `MenuFlyoutItem` 进**豁免队列**（阶段 4 菜单族的排队项，不是放行）。闸口重跑 **349/349 全绿 0 警告 0 skip**，调色板三档 checked=True（Light/Dark 各 83 源色 101 刷，HC 101 映射 + 3 条上游键因调色板无对应而按住），Gallery 冒烟 `--page buttons` 上屏、优雅退出、无残留进程，parity 条读到 "SplitButton audited, FluentDropDownButton own-type · 6 of 21"；新卡片本身在折叠线以下未目视。审计 `audits/splitbutton.md`。**阶段 4 第一段落地（Expander + InfoBar）**：这一段的第一件事是**推翻"这个原生控件不吃模板"这句话**——`InfoBar` 的 `Template`（本地值和样式两条路）实测完全无效、`OnRender` 用硬编码几何自绘（图标 20、关闭键 32、命中矩形）和硬编码兜底色（Informational `#2D2D37`），看起来正是普查表里的"自绘硬缺口"；但反射量到 `ContentControl.UseTemplateContentManagement()` 这个 **protected 方法**，`Expander` 的构造函数调了它而 `InfoBar` 没有——于是"重模板 vs 自有类型"这道选择题有了第三种答案：**一行构造函数调用**的自有类型 `FluentInfoBar : InfoBar` 就是把框架自己那把锁打开，符合 AGENTS.md 的"只有证明的行为缺口才自有类型"（缺口是量出来的，不是抄 WinUI 类型名）。这条已写进 `adaptation/00` **S0-m**，并且当场给 Task #8 开了一张返工单：**普查表所有"自绘"结论都要先问"它有没有开这个开关"**，之前按 `Template==null`/改模板无反应判定的硬缺口名单不可信。Expander 侧交付原生重模板，三条名字契约先探针量清再写标记（`adaptation/00` S0-m 2）：`PART_HeaderBorder` 是控件自己 `AddHandler(MouseDownEvent)` 的地方（所以头部**绝不能**再放 ToggleButton，会双触发）、`PART_ContentBorder` 的 `Visibility` 由控件写、`PART_Chevron` 必须是 `Shapes.Path`（控件固定旋转 **+90°**），正反两向都有断言（改名后点击静默失效那条是 `A_renamed_header_does_not_expand`），且 `ExpandDirection` **不被尊重**所以 `ExpanderContentUpBorderThickness` 不转录；`Expander` 根本没有 `IsPressed` 属性——**属性名闸口**（Button 批建的那条）当场抓到"按下格永远不可能命中"，5 条 `*Pressed*` 行随之撤出转录清单并写进反向闸口，代价量化到具体像素面（只有 chevron 药丸底色，其余四条本来就别名到同一实例）。资源侧 `ThemeResources/Expander.jalxaml`（blob `27044714`）21 行 = 6 度量 + 15 别名，`InfoBar.jalxaml`（`1056af57`）23 行 = 8 度量 + 15 别名，无消费点的行一律不发布并配 `A_row_with_no_consumer_is_not_published`；InfoBar 里框架自绘时读的 9 个键名（`InfoBarInformationalBackground` 等）**故意不发布**——我们一旦模板化就不再走那条读法，发出去等于承诺"覆盖它能改像素"而承诺不成立。另外两条运行时事实：`IsOpen=false` 在模板化后**不会**自动收起（控件只量高度），所以样式必须自己写 `RootBorder` 的 `Visibility` 格（当时这格叫 `ContentRoot`，视觉缺陷批按 S0-p 把名字改回来了）；`UIElement.RaiseEvent` 是 **public** 且 `MouseButtonEventArgs` 可直接构造，于是"驱动控件真实 click 处理器"第一次不依赖自动化模式也不依赖 OS 输入——但它是**进程内合成**，不算硬件输入证据，`SourceName` 在无指针条件下如何求值因此仍未证（进不声称清单）。产出：`Surfaces.jalxaml` 两份样式 + 两份内联模板（Expander 5 格 / InfoBar 6 格，逐格按键名断言，`Severity=Error/Warning/Success` 三条枚举条件格与 `IsOpen=False` 收起格）。`AstraExpanderInfoBarTests` **57 条**、消费点闸口 14 份→**16 份**字典、全套 **408/408 全绿 0 警告 0 skip**、调色板三档 checked=True（本批零改动）。**Gallery 侧**新 Surfaces 页（Expander 卡 + 四态 InfoBar 卡）与目录两条声明；**工具侧**新 `tools/Test-AstraGallerySmoke.ps1`：启动 → 等输入空闲 → 断言窗口句柄与标题 → 关窗 → 断言退出码与无残留进程，**故意不截屏**（截图不是证据，像素归断言，理由见 `adaptation/06`），本批跑 4 页全清。审计 `audits/expander.md`、`audits/infobar.md`。仍欠：两控件的按下/键盘/触摸真机输入、InfoBar 的水平朝向（上游 `Orientation` 分支依赖 `x:Double` 度量，本 reader 读不了）、MenuBar/MenuFlyout 全族、CommandBar（含欠着的 `SplitButtonInAppBarUnfocusedPointerOver` 与 `AppBarButton*` 行、以及 InfoBar 关闭键的样式重映射）、ContentDialog、TeachingTip、Card。**阶段 4 第二段落地（菜单族：Menu / MenuItem / MenuBar / MenuBarItem / ContextMenu / MenuFlyout 全族）**：这一段先花六遍探针把"能不能重模板"问清楚再说别的（`spike/MenuProbe`，`menu-probe1..6.txt` 全留），结论是**同一族里两个世界**——`Menu`、`ContextMenu`、`MenuFlyoutItem`、`ToggleMenuFlyoutItem`、`MenuFlyoutSubItem`、`MenuFlyoutSeparator`、`MenuBarItem` 都会实例化出厂主题里的模板（pass 6 直接量出厂主题，画刷按 `ReferenceEquals` 认到调色板实例），而 **`MenuItem` 存下 `Template` 却永不实例化**（`OnRender` + `ResolveBackgroundBrush`/`ResolveMenuBrush` + `DrawCheckMark`/`DrawSubmenuArrow` 自绘），`MenuBar` 根本没有模板可装，两者因此只拿"只上色"的样式。第二件事是**推翻自己上一遍的结论**：pass 4 看到"点击后框架把 `Background` 写成本地值 `#FF3A3A3C`"，pass 6 在出厂主题下复测发现合成 MouseDown 只把 `IsPressed` 置真、`Background` 仍是我们的行实例且**无本地值**——那条"框架本地值危险"在本批配置下不成立，已按实测改写（S0-n 2）。状态面收窄到能数的三格：`IsMouseOver`、`IsEnabled=False`、`IsChecked`，因为 `MenuFlyoutItem.IsHighlighted` 与 `MenuFlyoutSubItem.IsSubMenuOpen` 只是 **CLR getter 不是 DP**（连 `SetValue` 的路都没有），上游 11 条 `*Pressed*` + 2 条 `*SubMenuOpened*` 无处可挂，加上 24 条 reveal、3 条 placeholder/narrow-padding、`MenuFlyoutItemTextTrimming`、`MenuFlyoutSystemBackdrop`、`MenuFlyoutLightDismissOverlayBackground`、4 条 `x:Double` 与 Split 系全部不发布（上游 81 条主题行 → 本实现 26 别名）；`MenuBarItem` 自有 DP 只有 `Title`，上游第四个状态字面就叫 `Selected` 而无对应属性，四条 pressed/selected 行同样按住。资源侧 `MenuFlyout.jalxaml`（blob `6f9f3fd3`）26 别名 + 8 度量、`MenuBar.jalxaml`（`96cf1b1e`）6 别名 + 3 度量，一处值替换（`MenuFlyoutPresenterBackground`→`AcrylicInAppFillColorDefaultBrush`，沿用 FlyoutPresenter 先例），两处**名字归并**并就地写明理由：`Menu`/`MenuBar` 读 `MenuBar*`（上游没有 `Menu_themeresources.xaml`，ModernWpf 也是这么做的），`ContextMenu` 读 `MenuFlyoutPresenter*`（同一对颜色，本仓库只发上游名）——顺带把 ModernWpf 的 `ContextMenu*`/`Menu*`/`MenuItem*` 自造名一律拒绝进公开键清单。样式侧 `Styles/Menus.jalxaml` 九份样式（六份内联模板 + `MenuBar` 只上色 + `MenuItem` 只上色 + `MenuBarItem` 一格 hover），三条新账单：① **`MenuFlyoutItem` 自己度量并且自己画**（`MeasureOverride` 用常量封顶高度、`OnRender` 还画背景），上游 `11,8,11,9` padding + `4,2,4,2` margin + 17 高标签在实测 34 的盒子里被压成 13，于是条目样式带一条 **38 的 `MinHeight` 字面量**（4+17+17，正是那些行应有的几何），而 32 那条只能属于表面（放条目上就挤压）；② **整族不读任何模板部件名**（`GetTemplateChild`/`PART_` 零命中，子菜单弹窗由控件 `new Popup` 自建），所以 `LayoutRoot`/`IconContent`/`CheckGlyph`/`SubItemChevron` 是上游对齐而非功能契约——与 Expander 那三条名字恰好相反，两件事不许互推；③ **弹层外壳归框架**：`MenuPopupScrollHost` 那层 Border（`#FF2C2C2E`/`#FF48484A`）碰不到，`ContextMenu.MinWidth=140` 不被宿主尊重（实测 62.86 宽），因此 `MenuFlyoutPresenter*` 两行的承诺只对 `ContextMenu` 自绘那层表面成立。另有两条"上游有我们没有"的形状差：`ToggleMenuFlyoutItem` 的自动化模式是 **Invoke 而不是 Toggle**（写成断言而不是修），`RadioMenuFlyoutItem`/`SplitMenuFlyoutItem`/`MenuFlyoutPresenter` 类型缺失。像素侧还学到一条基座边界：**纯文字主体在本 harness 上写不出任何像素**（`PixelHarness.Build` 的注释即此），所以菜单的像素主张全部改由**表面填充行**与**描星号的路径**（chevron、勾形、分隔线）承担，标签色只到实例身份那一层。两个上游键（`MenuFlyoutItemBackgroundPressed`、`MenuBarItemBackgroundPressed`）在框架自己的字典里确实存在，因此"不发布"不能写成查不到——换成一条正向断言：它们**不等于**上游别名目标的那个调色板实例，即不是我们的行。产出 `AstraMenuTests` **90 条**（18 条别名身份 + 8 条度量 + 29 条不发布 + 可重模板类型 + `MenuItem` 存而不建 + 三处部件名 + 格子全表与逐格行名 + ShowAt/Hide + Invoke 跑命令 + Toggle 模式形状 + 勾选圈可逆 + `ContextMenu.Open` + `Menu` 仍宿主两项 + 四条像素），消费点闸口 16 份→**18 份**字典，全套 **501/501 全绿 0 警告 0 skip**，调色板三档 checked=True（本批零改动）。Gallery 新 **Menus 页**（`Menu` + `MenuBar` 上屏、flyout 与右键菜单代码构建并 `ShowAt`/`Open` 驱动）与目录九条声明，`MenuFlyoutItem` 从豁免队列里**删除**；`Test-AstraGallerySmoke.ps1 -Page menus` 上屏、优雅退出、无残留进程（页内弹层需点击，冒烟不点，故弹层外观无目视证据）。审计 `audits/menu-flyout.md`。仍欠：菜单族零硬件输入（hover/press/submenu/bar-open 四条通路全在等真指针，Task #13）、`MenuItem` 自绘三态、CommandBar（含 `SplitButtonInAppBarUnfocusedPointerOver`、`AppBarButton*` 行与 InfoBar 关闭键样式重映射）、ContentDialog、TeachingTip、Card。**阶段 4 第三段落地（CommandBar 族）**：上游 86 条 Light 别名行（`AppBarButton` 25 + `AppBarToggleButton` 51 + `AppBarSeparator` 1 + `CommandBar` 9）里发布 **31 条别名 + 4 条 Thickness**（30 条逐字照抄 + 56 条不发布 = 86，第 31 条 `CommandBarBorderBrush` 是宿主名，上游叫 `…BorderBrushOpen`），56 条不发布逐组给实测理由（9 条 checked×hover 复合态、5 条高亮遮罩、8 条勾形前景、12 条加速器文本、5 条 chevron、4 条子菜单打开、7 条 bar 打开态……）。这一段的主要产出是**四条新的基座事实**（`adaptation/00` S0-o）：① **行遮蔽与隐式样式覆盖都是主方法**——26.10.9 自己发 `AppBarButton`/`AppBarToggleButton`/`AppBarSeparator` 的隐式样式并从 9 条自有行画它的 bar（含硬编码紫 `#680081`），我们后合并的别名按 `ReferenceEquals` 赢，但**只对主题装好之后建造的控件**；② `CommandBar` 存下 `Template` 而永不实例化（`MenuItem` 之后第二例），所以 bar 表面是唯一还能碰到的 CommandBar 像素面；③ **`XamlReader.Parse` 的临时模板里 `ControlTemplate.Triggers` 永不生效**——pass 4 那批"状态格不动"的读数因此全是假阴性，两条结论当场撤回重测，状态普查只能用编译字典或反射；④ 共享宿主窗口让"整帧哨兵像素计数"不可判定（同一条断言类内隔离通过、全量跑失败，因为别的类留着一个开着的亚克力弹层），于是这类主张一律改成**同帧两次捕获的差分**。另外量到 `IsMouseOver` 写不进也读不到 key，但一次路由 MouseDown+MouseUp 会把它置真，hover 行因此可进像素；`CommandBarLabelPosition`/`CommandBarDefaultLabelPosition` 两个枚举与上游 `controls2.idl:83/:100` 逐字相同，而**每个成员都挪不动任何东西**。审计 `audits/app-bar.md`（含 8 条 Known Gaps），新测 `AstraAppBarTests`，消费点闸口 18 份→**20 份**字典，Gallery 新 **Command bar 页**（`CommandBar` + 四类 `AppBarButton`/`AppBarToggleButton`/`AppBarSeparator` + 溢出与紧凑开关）与目录四条声明。**视觉缺陷批（用户实测反馈「很多控件有重影、间距不合 Fluent」）**：截图先于一切结论，`spike/VisualQA/capture-pages.ps1` 逐页上屏捕获（PrintWindow + `PW_RENDERFULLCONTENT`，dpi=168 打在原图上）。目视抓到三处，逐处量到根因并修：**① 信息条整条被画两遍**——`Styles/Surfaces.jalxaml` 的模板根用了上游的名字 `ContentRoot`，而运行时 `InfoBar.OnApplyTemplate` 找的是 `RootBorder`（这次不再推测：直接解 IL 的 `ldstr` 字面量，`spike/InfoBarGhostProbe` pass 2；顺带纠正一次"两种字节扫描都读不到因此契约是编的"的误判——`ldstr` 在 `#US` 堆，偶数偏移重排会整体错位）。名字没对上时基础类没拿到它的部件，`OnRender` 就不让路，在我们模板之上又画一遍图标/标题/消息/关闭叉。改名即修复，判据是「上样式的条」与「把 `OnRender` 覆写成空的同类」捕获**逐像素相同**（7 色 / 936 ink；改名前 14 色、多 178 px 框架蓝）。**② 消息换行后第二行画到条外面**——`InfoBar.MeasureOverride` 返回的是基础类自己那套单行字面布局的高度（对外 69.8，内部模板要 107.1），`FluentInfoBar` 因此多一个 `MeasureOverride` 取两者较大；断言打在 `DesiredSize` 且放在竖直 `StackPanel` 里（宿主给固定高度时缺陷会被排掉），撤掉修复即失败。**③ `Menu` 的条目竖成一列**——补模板时没带 `ItemsPanel`，条目从框架默认横排退回 `ItemsControl` 的竖排；补一条横向 `StackPanel` 恢复，撤掉 setter 即失败。附带把 Gallery 页脚 parity 行与「Not claimed」行叠在一起的问题改成「单 `TextBlock` + 固定高度滚动宿主」，**这是绕开不是治好**：换行文本的度量宽度与排布宽度不一致这条框架行为仍未定位（`spike/TextWrapProbe` case E 里先布局后设文本连重测都没发生）。产出 `adaptation/00` **S0-p**（7 条，含 93 个自带 `OnRender` 的导出类型普查与各族部件名契约表）、`audits/infobar.md` 第 6 段更正（审计早写对了 `RootBorder`，是代码没照做）、三条新回归断言，并把 Task #8 那张"自绘结论要重读"的返工单从推测变成名单。**视觉缺陷批第二段（flyout 重影，2026-09-19）**：用户报"点开 flyout 里面又有重影"。`spike/FlyoutGhostProbe` 解四个 flyout 行类型 `OnRender` 的 IL 字面量，量到**标签、加速键文案、勾选标记、子项箭头、分隔线全是控件自绘**，而我们的模板又各摆了一份——S0-p 那句"菜单族读不出部件名开关"的另一半就是"没有开关不等于不画"。判据只能走合成窗口：进程内裁剪读不出这两层（shipped 与"空 `OnRender` 双胞胎"的亮像素计数一模一样），`PrintWindow` 抓 `PopupWindow` 自己的顶层窗口才行；同一 pass 连抓两次逐像素比对差 **0 px**，排除交换链伪影。修法先在探针里试出来（本地 `Template` 值胜过样式 setter，`stripped`/`bare`/`tint` 三个 pass 一次跑完）再落到产品：三份 flyout 模板只留表面与图标槽（`bare` 证明控件不画 `Icon`，而 `Icon` 的类型就是 `System.Object`，`ContentPresenter` 是正确承载），分隔线模板改成透明占位；标签的 hover/disabled 两格搬到 `Style.Triggers` 写 `Foreground`——tint pass 量到控件的画者**读属性**（品红 `Foreground`、`FontSize=20` 原样出现在弹窗里）但**不读我们发布的行**（覆盖 `TextFillColorSecondaryBrush` 动不了加速键/勾选/箭头/分隔线），于是那 11 条上游行撤回发布并写进 Known Gaps，而不是留着当装饰。`MenuBarItem` 那位第二画者（自绘 `Title`）按 S0-p 的 1431→603 测量收尾：`ContentButton.Content` 留空并钉断言。数值：修前我们那层标签 `#FFFFFF` 1194 px 压在框架自己的 `#F5F5F7` 1670 px 上，修后 shipped/stripped/bare 三个 pass 全为 **0**，框架层逐色计数与参照 pass 一致。产出：`AstraMenuTests` 90→**126 条**（单画者缺席契约、样式触发器标签态、勾选让位给标记、`MenuBarItem` 空 Content、反向像素断言"哨兵为 0 而框架灰仍在"），全套 **616/616 全绿 0 警告 0 skip**，`adaptation/00` 新增 **S0-q**（7 条），`audits/menu-flyout.md` 的 0.9-0.11 与 §1-§5 按实测改写（含撤回上一版"分隔线与 chevron 覆盖后可见"这条已被证伪的主张）。仍欠：hover 两层填充是否叠加仍无真指针证据；间距批（11 项上游度量差）未开工。

**视觉缺陷批第三段（间距批，2026-09-19）**：用户抱怨的第二半——"间距不合 Fluent"——按上游逐条量。命令栏条目改成上游的三层形状（`Root` 无填充 / `AppBarButtonInnerBorder` 高亮 2,6,2,6 / `ContentRoot` 带 64），图标槽 `Height=16` + `0,16,0,2`（新发布行 `AppBarButtonContentViewboxCollapsedMargin`），Compact 同时写 2,6,2,22 与 48，`AppBarSeparator` 撤掉自造的 `MinHeight=40` 并把 2,8,2,8 还给矩形 margin；`ComboBoxItem` 模板根补上游那条 `5,2,5,2` 行内缩（`AstraComboBoxTests` 从排布尺寸读回：高亮宽 = 条目宽 − 10、高 = 条目高 − 4）；`Slider` 拇指改回上游 18×18 + 环 `Margin=-2` + 内芯 12，刻度条与轨道之间 4 的间隙连同 14/Auto/14 三段带一起照抄，并量出**框架给拇指留的是字面量 16**（220 控件行程 204，换 16 拇指与换 18 拇指两次同值），所以 18 的环在最大值处超出控件 2 DIP —— 写进 gap，而不是把拇指缩回 16 把它藏掉。上一段当时最"值钱"的那条基座边界（原 `adaptation/00` **S0-r**："嵌套层的笔刷过渡终值进不了合成帧"）在**同一天被自己的实验推翻**，命令栏的 83ms 淡入已按上游装回，全部过程见下一段。四类证据：构建 Debug/Release 两通道 0 错误；行为/资源 = 全套 **620/620 全绿 0 skip**，Debug/Release 两通道各一遍（新增命令栏几何 1 条、栏内像素 2 条、上屏后置勾选 1 条、ComboBox 排布 2 条，并把 Slider 的行程与尺寸断言按实测改写，12 处部件名从 `Root` 迁到 `AppBarButtonInnerBorder`）；视觉 = `spike/VisualQA/out/command-bar.png` 里 Bold 格子有强调蓝、图标在标签之上、条目带 64 高（该图的 0/7786 计数出自还没有空帧闸口的采集器，同日由第四段重做）；硬件输入 = 仍为零。调色板三档 checked=True（本批零改动）。 

**视觉缺陷批第四段（S0-r 更正，2026-09-19）**：上一段那条边界不成立——**过渡不改帧，改的是那次读数的可信度**。`src/FluentJalium/Styles/AppBar.jalxaml` 两个 bar 按钮的高亮层按上游带回 `TransitionProperty="Background, BorderBrush" TransitionDuration="0:0:0.083"`。两段实验：① `spike/TransitionProbe` 的 17 格判别矩阵（一次运行、三次抓帧、逐格唯一色、PrintWindow 客户端区），每格只比邻居差一个变量——模板根/嵌套、有/无过渡、83ms/0、加载时写/上屏后写、`{ThemeResource}`/字面、TemplateBinding 与格子双写同一属性、模板外/模板内、笔刷过渡/`Width` 过渡、属性列表带空格/不带，外加命令栏三层形状的完整复刻——**17/17 在三次抓帧里逐次同值全部落帧**（90×90 DIP 格 24649 px、`Width` 30→90 亦 24649、命令栏形状格 18968）。② 原现场 A/B 用新写的带空帧闸口的 `spike/VisualQA/grab-page.ps1` + `count-colors.ps1` 重做：同一 Gallery 页 `--page command-bar`、只改那一个属性的两次构建，`#60CDFF` 都是 **8832 px**、栏底 `#323232` 都是 958227 px，而抓到可用帧之前分别重试了 15 次（无过渡）和 7 次（有过渡）。换进来的真结论是**判据本身**：PrintWindow 能在窗口已摆好、也在出帧时返回没画过的帧，于是"某色 0 px"与"整帧空白"完全同形；而 `PixelHarness` 的 `Render`/`Host`/`Chrome` 是同一条 `RenderTargetBitmap` 通路（`Chrome(窗口)` 也不是屏幕），结构上看不见这件事。矩阵第一版还顺手复现了 keyed 模板 `Trigger.Property` 为 null 那条老缺陷（九格属性读回全是静止色），改成 keyed `Style` + 内联模板后读数才成立——属性读回与像素读回必须成对出现，两个方向的假阳性都才抓得住。任务 #22 随之关闭：19 处 `TransitionProperty` 不需要逐点重测，被怀疑的机制不存在（这些站点仍只有属性/栅格证据，那是另一件没做完的事）。淡入这条声明本身进了断言（`The_template_carries_the_part_names_upstream_uses` 读回高亮层的 `TransitionProperty`/`TransitionDuration`），防的是它再被一次误判悄悄删掉。四类证据：构建 = 闸口 Debug 与 Release 两通道各 0 错误；行为 = **620/620 全绿 0 skip**，两通道各一遍（620 不变，因为新读数是并进既有用例而不是新增用例）；视觉 = `spike/TransitionProbe/out/transition-capture1..3.png` 与 `spike/VisualQA/out/ab-notransition.png` / `ab-transition.png` 这一对可逐像素对照的 A/B；硬件输入 = 仍为零。| 每批全 9 步 + 全闸口 |

**视觉缺陷批第五段（间距全量复核 + 弹层分隔线，2026-09-19）**：把"间距不合 Fluent"从逐控件改成**全量机械比对**。`spike/SpacingSweep/survey.py` 读上游所有 `*_themeresources.xaml` 的 `Thickness` 行与本仓 `ThemeResources/` 逐键对照：**我们发布的 61 条里 56 条与上游逐字相同，值冲突 0 条**（唯一一条 `RadioButtonBorderThemeThickness` 是已记录的类型债，不是数值错），另有 53 条"在-play"上游行未发布——它们各自已在对应审计里给了不发布理由，本批没有新增死键。第一版脚本把参考仓的示例页（`BorderThicknessPage.xaml`、`CommonStylesPage.xaml`）也算进规范，因此误报过一条 `RadioButtonBorderThemeThickness`=2；判据改成"只认主题字典"后那条自己消失了，这条判据写进脚本注释。行落进 `spike/SpacingSweep/survey.txt`。第二步用带空帧闸口的 `spike/VisualQA/capture-all-pages.ps1` 把 Gallery **9 页全部**重拍（dpi=168，逐页一个进程，全部 attempt0 命中），再按 DIP 量像素而不是目视：命令栏勾选格的强调框 **52 DIP 高、内缩 2,6,2,6**（64 的格子里）✓，Expander 头部 **48 DIP** 且标签从边缘进 **16 DIP**（= `ExpanderHeaderPadding`）✓，CheckBox 方框 20 / 行距 32 ✓——三处都对得上上游，之前那张没有闸口的 `command-bar.png` 的结论这次由 `page-command-bar.png` 重证一遍。**真正量出来的偏差在弹层**：`MenuFlyoutSeparator` 在本运行时把自己量到 **9 DIP 高**，上游是 **3**（1 高的线 + 它自己 `-4,1,-4,1` 的 padding），于是每条带分隔线的菜单都多 6 DIP。样式里先补 `Margin=0` 只改得动属性（读回 `0,0,0,0`）改不动盒子，改成 `Height=3` 才落到 3；两侧都有读回——属性侧新用例 `The_flyout_rows_measure_upstreams_heights`（条目 38 / `LayoutRoot` 34 / 分隔线 3 / 线盒 1 且 margin `-4,1,-4,1`），像素侧新工具 `spike/VisualQA/capture-pid-windows.ps1` 把弹窗与宿主分别 PrintWindow（`flyout-400x356-168.png`：线仍在 y≈122.3..124.6 DIP 横穿整行，弹窗总高 203.4 DIP 与"5x38 + 3 + 上下各 5"逐数吻合）。这一段也量出三条**判据侧**的底座事实（`adaptation/00` **S0-s**）：弹层顶层窗口会被宿主窗口的剩余空间裁掉（宿主缩到 240x140 → 弹窗只剩 125 DIP/三行，看着像"条目丢了"）；弹层根节点在进程内 `RenderTargetBitmap` 是**全黑**（S0-j 同一族，这次在 MenuFlyout presenter 上）；读探针日志必须在进程退出之后——一次 `--hold` 未落盘时读到的是上一遍，据此得出的"Height 没起作用"是假的。Gallery 窗口约需 17 s 才枚举得到（9 s 预算会静默丢页），这条写进采集器而不是缺陷清单。**顺带更正一条文档数**：`audits/menu-flyout.md` 写的"AstraMenuTests 90→126 条"是当时数错的，实测本批前 103 条、加这条 104 条。四类证据：构建 = 闸口 Debug 与 Release 两通道各 0 错误；行为 = 全套 **621/621 全绿 0 skip**（+1 新用例），两通道各一遍；视觉 = `spike/VisualQA/out/pages/page-*.png` 九张带闸口的整页捕获 + `flyout-400x356-168.png` 这张弹窗合成图（其行带坐标已量化成数字）；硬件输入 = 仍为零。调色板三档 checked=True（本批零改动）。

**这一段有一条更正**：上面那句"两通道各一遍 / 621 全绿"当时只有 Release 成立。Debug 通道那一遍是 2 失败，随后一次 `--no-build` 重跑列出 4 条（`AstraSliderTests` 两条 `Assert.Same` 同色不同实例、`AstraNumberBoxTests.A_focused_compact_numberbox_opens_its_spinner_popup`、`AstraAutoSuggestBoxTests.A_resting_box_paints_its_rows_and_nothing_invented`）。下一段批的闸口里这 4 条全绿，而那次闸口开头清掉了两个上一遍遗留的 `testhost.exe`——它们共用同一个宿主窗口。所以那 4 条是采集器串台的假阳性，不是产品缺陷；但"当时就写了两通道各一遍"是这一段的错，按实测改在 `audits/right-gap.md` §4。

**视觉缺陷批第六段（右侧空隙 + 导航不换行，2026-09-19）**：用户第二次报问题——"下拉框或者说一些控件，它的右边是空了很大一块的，明显这个空隙不存在……它长度和左边的空隙是不对齐的"，以及一条要记住的规则"导航栏的文本是不需要自动换行的"。第一步先排除控件内部：`ComboBoxPadding` `12,5,0,7`、`TextControlThemePadding` `10,5,6,6`、`ExpanderChevronMargin` `20,0,8,0`、`ComboBoxEditableTextPadding` `11,5,38,6` 与上游逐字相同，`spike/RightGapProbe` 的树读回也对得上（260 宽的 ComboBox：文本左缩 12、箭头右缩 14、箭头列 38）。**空隙在弹层里**：260 DIP 的 `AutoCompleteBox` 打开建议列表，弹窗自己的顶层窗口是 260x80 DIP，画出来的卡片只到 148 DIP，右边 **112.6 DIP 是纯 `#000000`**、左边贴到 0——"和左边的空隙不对齐"就是这个数。根因是框架给 `Popup` 元素写了本地 `Width`（读回 `width=260 localWidth=260`）却用**无穷大**量子节点，于是 `HorizontalAlignment=Stretch` 无效（加上之后帧逐像素不变），只有 `MinWidth` 够得着盒子。第二层坑：`SuggestionsContainer` 上 `{TemplateBinding}` 与两种 `{RelativeSource AncestorType=…}` 都读回 **0**，而字面量 `MinWidth="260"` 读回 260——丢的不是属性是绑定；`{Binding ActualWidth, ElementName=OuterBorder}` 读回 260。对照 ComboBox：同一写法 `{TemplateBinding ActualWidth}` 在它的 `PART_PopupBorder` 上就能读到 260。差别是弹层被接到哪一棵树（下拉进宿主 overlay 层 / 建议列表有自己的顶层窗口）。底座事实记进 `adaptation/00` **S0-t**（四条，含"两通道给出相反答案"这一条）。导航那条先按直觉写 `TextWrapping="NoWrap"` 在 `PART_Label` 上，用例照旧红：`Expected NoWrap, Actual Wrap`、行高 36 → **55.34**——presenter 上这个属性是死格子，够得着的写法是把隐式 `TextBlock` 样式放进 `ContentPresenter.Resources`（ToolTip 批为反方向用过同一条路）。顺带量到 `NumberBox` 无 Header 时高 **39** 而不是 32（`HeaderContentPresenter` 的本地 `0,0,0,8` 在 Content 为空时照扣），且 `TextInput.jalxaml` 里"空 Header 让 Auto 行自己收到 0"那句注释是错的，已按实测改写；这条没修，记成 Known Gap。四类证据：构建 = Debug 闸口 **623/623、0 skip**，Release 因机器上有**另一个项目**的 `testhost.exe` 在跑而被闸口拒绝（不杀别人的进程），改手工跑同三步：build 0 错误、test 第一遍 **1 失败 / 622**、第二遍 **623/623**、调色板三档 checked=True——**那条失败没有名字**（重跑即绿，抓不到用例名），这是一条约 1/623 的未命名 flake，不声称已定位；行为 = ComboBox 下拉宽度并进既有用例、建议表面与导航标签各 1 条新用例（621→623），其中导航那条改前红、改后绿；视觉 = ComboBox 下拉在真上屏窗口里量到 **242.9..502.3 DIP = 259.4**（与它挂着的 243..504 同宽，`rightgap-combo.png-1330x1575-168.png`），修前对照 `rightgap-open.png-455x140-168.png`，**建议列表修后仍 148/260**（`rightgap-suggest3.png` 与 `rightgap-suggest2.png` 的 `painted` 计数逐位相同）；硬件输入 = 仍为零。新审计 `audits/right-gap.md` 六条 Known Gaps，第一条就是"建议列表的右侧空隙没关掉"。

**这一段的一条读数作废**：上面"建议列表修后仍 148/260"用的 `rightgap-*.png-455x140-168.png` 后来发现**不是弹层**——它在 `closed` / `combo` / `suggest2` / `suggest3` / `suggest4` 五种模式下 `painted` 计数与非黑扫描范围逐位相同（10864 / 19082 / 2041278；0..148 DIP），而 `closed` 模式什么都不开。所以那是进程里另一个 260x80 DIP 的可见顶层，"148/260"是一次读错窗口。正确说法是**上屏未测**，不是"上屏无效"；`audits/right-gap.md` §7·1 与 `adaptation/00` **S0-u·1** 已按此更正。

**视觉缺陷批第七段（左右边距不等长的真根因，2026-09-20）**：用户第三次报同一件事并把范围点明到"下拉框 / flyout 这一类型"，同时新增"有的这种控件圆角好像都不对"。这次先换判据：`spike/VisualQA/grab-screen.ps1`（新）用 `CopyFromScreen` 抓**整屏**，绕开"这个顶层窗口是谁"的问题——PrintWindow 枚举可见顶层分不清弹层，整屏没有歧义。第一件被推翻的就是上一段那条 148/260（见上）。第二件是方向：**弹层一族本身没有左右不等长**。`MenuFlyout` 用 `ShowAt(anchor)` 真开上屏后逐行扫，无图标条目文本左 **16.0 DIP**，与上游 `1(border)+0(presenter padding)+4(MenuFlyoutItemMargin)+11(MenuFlyoutItemThemePadding)` 逐数吻合；带勾选列的 44.6 DIP 多出来的正是 `CheckGlyph` 的 `12+16`（`MenuFlyout_themeresources.xaml:483-487` 三列 `Auto/*/Auto`）；样式侧读回 `LayoutRoot 210.5x34 margin=4,2,4,2 padding=11,8,11,9 radius=4,4,4,4`，`OverlayCornerRadius=8` / `ControlCornerRadius=4` 逐字对上；`Expander` 头部圆角在 `IsExpanded=True` 时已经由 `Surfaces.jalxaml:95` 切到 `4,4,0,0`，与上游 `Expander.xaml:35/64` 的 Top/Bottom filter 同形。**圆角这一项本批没有量出缺陷**，不声称它不存在，只声称这些位置是对的。真根因在基座：整屏量 `page-selection.png`，样卡离内容区**左 25.1 DIP、右 36.6 DIP**，而声明是 `Margin="24,8,24,24"`——差的 11.5 DIP 正是 `AstraNavigationTests` 早就钉住的 **`ScrollViewer` 在 `Auto` 下只在右侧扣走 12 DIP 布局宽度**（WinUI 的滚动条是 overlay，不占布局）。这条不针对某个控件，而是**每个会溢出的表面右边都窄一条**：页面宿主、`ComboBox` 的 `PART_ScrollViewer`、`AutoCompleteBox` 的 `PART_DropDownScrollViewer` 全中；同一页里样卡右边缘 1820、Live output 右边缘 1841 这两条不平的线也是它。改法沿用仓内既有决定 `Auto` → `Hidden`（`A_hidden_bar_still_lets_the_pane_scroll` 已证明滚轮与键盘照常），四处落点各写一条注释。四类证据：构建 = Debug 闸口 **623/623、0 skip**、0 错误、调色板三档 checked=True；行为 = ComboBox 与 AutoSuggestBox 各加 2 条断言（表面宽度 + 条模式），全套 102/102 与闸口 623/623；视觉 = 改前后同一张 `page-selection.png` 的 DIP 复测，skew **11.5 → −0.6**，两条右边缘并成一条；硬件输入 = **仍为零**，且这一段新量出一条限制：**进程内开不出 `ComboBox` / `AutoCompleteBox` 的弹层**（`IsDropDownOpen=true` 读回 True 但整屏里控件是闭合的，`PART_Popup` 高度停在 32 = 控件自身高度），所以建议列表的宽度到底有没有落到屏幕上，仍未测。代价也写清楚：`Hidden` 之后**可见滚动条没有了**，上游溢出时是显示条的，这是基座替代不是等价。

**视觉缺陷批第八段（结清这一批自己能证的两条，2026-09-20）**：第七段留下三条不需要真指针就能做的账，这一段结两条。
**其一，`NumberBox` 无 Header 时高 39 而不是 32。** 上一段写的是"样式里给不出 Header 非空这种触发条件，
`Trigger Property=Header Value={x:Null}` 未验证"——这句判断没做实验就按下了，而实验很便宜：加上那一格，
无 Header 的盒子 **39 → 32**，与 ComboBox / AutoCompleteBox 同高。要点是 `Collapsed` 把本地 margin 一起
请出布局，空 Content 不会。反向也钉住：给回 Header，presenter 回 `Visible`，控件按"文本行高 + 8"长回去
（新用例 `A_headerless_numberbox_does_not_pay_the_headers_gap`）。这一格不在上游的 cell 清单里，所以
`The_numberbox_template_carries_one_cell_per_upstream_state` 的期望表把它单列一行、注明是替代不是转录——
清单守卫第一次真的拦住一次样式增格，而不是拦住一个错抄。
**其二，三处死掉的 `TextWrapping="Wrap"` 格子，只有一处是真缺陷。** 逐处对上游之后结论分两边：上游**写了**
`Wrap` 的（`CheckBox_themeresources.xaml:611`、`RadioButton_themeresources.xaml:378`）我们照抄一个死格子，
而这条运行时的默认值恰好也是 Wrap，**后果相同，不动**；上游**没写**的（`ComboBox_themeresources.xaml:764`
的 `ComboBoxItem` presenter）默认值反而错了——长条目会换行把行撑高，必须显式补 `NoWrap`，且只能补在
`ContentPresenter.Resources` 的隐式 `TextBlock` 样式上（`A_long_combo_item_stays_on_one_line`）。
"死格子"不等于"缺陷"，这条判断写进 `adaptation/00` **S0-u·5/6**。四类证据：构建 = Debug 闸口
**625/625、0 skip、0 错误**（623→625，+2 用例），调色板三档 checked=True；行为 = 两条新用例双向读回；
视觉 = 另一个进程的树读回独立确认 39 → 32（与闸口不同通道）；硬件输入 = **仍为零**。
一条采集器纪律顺带量到：新用例最初把下拉留在打开态，下一个走 overlay 的用例就读到了它的条目
（`ComboBoxItemBackground` 断成 PointerOver 色）——**开过 overlay 的用例必须先关掉再落断言**。

**视觉缺陷批第九段（"圆角不对"查到一个真缺陷，但它不在圆角上，2026-09-20）**：用户第二次报的是
`flyout` 那一类控件"圆角好像都不对"。先立一条基座事实再看控件：`spike/RightGapProbe --open corner` 用三张
已知颜色的图量出 **`Border` 会磨圆自己的填充但不裁剪子元素**（r=12 白表面 (0,0)/(2,2) 读到蓝底、(4,4) 起读到白；
塞进一个 60x60 红子元素后 (0,0) 到 (30,30) 全红，`Clip` 读回空；r=0 对照组全红，排除采样器）。
含义是**样式里的半径 token 赢不过一个铺到角的内容**，于是"圆角不对"必须逐表面量几何而不是查 token。
`--open flyout` 全树读回：`ComboBox` 下拉表面 r=8 而行让开 5,2,5,2 + 0,4（弧在 x=5 处只吃 y≈1.1 DIP）、
`MenuFlyout` 行 4,2,4,2 + 行自身 r=4、`Expander` 靠 `IsExpanded` 那格把头部下圆角改方 — **三处安全**；
`ComboBox` 的 `PART_ToggleButton` 读回 `radius=10` 看着可疑，但它的模板只有 `Grid + Path`、没有跟这个半径
的填充或描边 → 惰属性。唯一"内容会盖住圆弧"的是 `SuggestionsContainer`（r=8、padding `0,2,0,2`、内层
`Margin=-1,0,-1,0`，行左右零内缩）。
**顺着它量到的缺陷不是圆角，是品牌绿。** `PixelHarness` 加单点读回 `PixelAt` 之后，打开的建议列表裁剪直方图
是 `#F9F9F9x2020` 后面紧跟 `#1D733Cx450 #2B804Ax450 #1E743Dx420 #2A7F49x420 …`，中点 (130,20) = `#247A43`：
框架把它的条目容器（类型就是 `ComboBoxItem`）的 `Background` 写成**本地 `LinearGradientBrush`**（accent 绿对角
渐变、`CornerRadius=3`），而本地值排在 setter 与所有 trigger 之上——AutoSuggestBox 批记下的"框架本地值"账单
在这里第一次被量出**具体是什么颜色**，而答案是每条像素闸口专门要挡住的那个绿。
修法只有一条路能让样式说话：**模板不去读那个属性**。`Selection.jalxaml` 里 `ComboBoxItem` 的 `LayoutRoot`
从 `{TemplateBinding Background}` 换成 `{ThemeResource ComboBoxItemBackground}`；九个状态格子本来就往
`LayoutRoot` 写 token，所以 ComboBox 自己的行一格没变（`AstraComboBoxTests` 全绿）。修后同一测量：
中点 `#F9F9F9`、直方图 `#F9F9F9x10012 #DDDDDDx302 #F6F6F6x254 #000000x42`、绿通道占优像素 **0**、
四个角仍读回未上色（圆弧没被盖住）。代价写在模板注释里：消费者给 `ComboBoxItem.Background` 设的本地值
不再进像素，上游会进——这正是阶段 2 起手必修那句"模板根 Border 不吃本地 Background"搬到条目容器上。
同一批把阶段 4/5/6 的另一个前提结清：**全量类型清单**（`--open types`，3 157 行原始输出
`adaptation/s0v-runtime-type-inventory-raw.txt`，`Jalium.UI.Controls.*` 960 个）——`ContentDialog`、
`ProgressBar`、`SymbolIcon`、`GridView`、`DataGrid`、`TreeDataGrid`、`ListView`、`ListBox`、`TreeView`、
`NavigationView`、`ToggleSwitch`、`TitleBar`、`CommandBarFlyout` 都是原生已有，而 `TeachingTip`、`Card`、
`InfoBadge`、`ProgressRing`、`RatingControl`、`PipsPager`、`TabView`、`BreadcrumbBar`、`Divider`、
`BitmapIcon`、`AutoSuggestBox` 确实没有；**并且运行时根本没有 `FlyoutPresenter` 也没有
`MenuFlyoutPresenter` 这两个类型**——"给 flyout 表面写一个样式"在这个基座上是不可执行的任务，表面要么是我们
在别的模板里画的那层，要么是框架自绘（S0-u·3、S0-n 那两条都是这一条的下游）。写成 `adaptation/00`
**S0-v / S0-w** 两节 + 审计 `audits/corner-radius.md`。四类证据：构建 = 新增 3 条用例编译进闸口；
行为 = 修前后同一条属性读回（宽高不变、本地渐变仍在）；视觉 = `RenderTargetBitmap` 单点 + 直方图两通道一致，
**但它按属性重画，所以本段没有一条上屏帧主张**；硬件输入 = **仍为零**。
仍欠：建议行"选中"到底是框架哪个状态（只量到过滤后自动选中的第一行）；`SuggestionsContainer` 的行左右零内缩
本批没改（现在行不画不透明底色所以盖不住圆角，一旦某格写上不透明底色缺陷会回来，真解是阶段 5 做
`ListBoxItem` 时给行带 `Margin` + r=4）；`MenuBar` 下拉的半径没测。本段当时对 `ContentDialog` 只读了
`DialogSurface.CornerRadius=8` 这一条属性就收口——**那条收口是错的**，第十段按像素逐角复测后在里面发现两处
方角缺陷并修掉（见下）。教训一并写进 S0-x.5：圆角只能逐角读像素，读外层属性不算检查过。

**视觉缺陷批第十段 / 阶段 4 第四段（ContentDialog：量一个对话框要先把它打开，2026-09-19）**：
路线图上这一格写的是"ContentDialog(自有)"，S0-v 的类型清单把它改判成原生重模板——原生类型成员面完整，
`spike/RightGapProbe` 的 `dialog` pass 量到它有框架在挂载时代码建好的树（`PART_Root`/`PART_Overlay`/
`PART_DialogCard`/`PART_TitleHost`/`PART_ContentScrollViewer`/`PART_ButtonPanel` + 三个按钮）。
**这一段最值钱的读数不是样式，而是"什么状态才算量过它"**：挂载未打开的实例是
`Visibility=Collapsed`、`0×0`，本批第一版 15 条断言全读那棵空树，于是"卡片没有本地尺寸值""标题没有样式"
"像素画不出""点了没反应"四条假象同时出现——`ShowAsync()` 真开一次之后四条全部改写。
控件把对话框搬进窗口的 `ContentDialogOverlayHost`，并且**拒绝**打开已经在树里的实例（同步抛
`must not already be attached to the visual tree`），所以 Gallery 的 Surfaces 页面上没有 `<ContentDialog>`
元素：卡片给三个按钮（三/两/一个）+ 两个开关（默认按钮、full size），点击时才 new。
量到的三条基座写进 `adaptation/00` **S0-x**：
(1) **模态宿主按部件名回写本地值**——控件把自身 Min/Max 映射到 `PART_DialogCard` 并用"宿主宽 − 48"封顶
（宿主 886.3 → `card.MaxWidth` 本地 838.2857…；应用写 `MaxWidth=548` → 本地就是 548），本地值在所有格子之上，
所以 548 那条默认上限放不住：卡片拆成两层，`PART_DialogCard` 只接名字与 24 DIP 外边距，
`DialogSurface` 承上游尺寸盒（320/548/184/756）与全部填充行，实测 `surface.max=548 localMax=UnsetValue`、
`radius=8,8,8,8`；
(2) **格子的可达面测清了**：八种文本组合下五列恒为 `*,8,*,8,*`——`Value="0"`（存成 `Double`）和
`Value="0*"`（真 `GridLength`）都写不动具名 `ColumnDefinition.Width`，而元素上的 `Visibility`/`Grid.Column`/
对齐都写得住；`UniformGrid` 折叠子元素后**不回收**格子（`35.9/34.8/35.4` → `35.9/35.4`），不是替代品；
按钮的 `Style` 是 `TemplateBinding` 写出的本地值，格子压不过——改写成不带 `TargetName` 的格子去写控件自身的
`PrimaryButtonStyle` 就通（`buttonStyleChanged=True`、拿到的正是格子里那个 `AccentButtonStyle` 实例、
`DefaultButton=None` 后还原）；
(3) **生成的文本元素选择性吃继承**：`PART_TitleHost` 上 `FontWeight=SemiBold` 进得去、`FontSize=20` 进不去
（读回 `size=14 localSize=UnsetValue`），`ContentPresenter.Resources` 的隐式 `TextBlock` 样式在此是惰的
（`style=null`，与 `ComboBoxItem` 那条 NoWrap 不同作用域）——20/SemiBold 最终落在**样式级默认
`TitleTemplate`** 自建的 `TextBlock` 上（`size=20 weight=SemiBold localSize=20`），代价是非字符串 `Title`
需要应用自给模板。
行为面把部件名这条契约从"注释里的希望"变成读数：Invoke 我们模板里的 `PART_PrimaryButton` →
`PrimaryButtonClick` 与 `Closed` 各一次、`ShowAsync` 以 `Primary` 完成、`Visibility` 回 `Collapsed`；
禁用该按钮后一次点击都不发。视觉面两条：哨兵换掉 `SolidBackgroundFillColorBaseBrush` 后卡片画哨兵
（>5 000 px、品牌绿 0 px，注意裁剪里上部条带占更大面积）与 Light/Dark 各开一张卡 `Top(3)` 直方图不同、
两张都画满。`ContentDialog` 的半径这条**量出一个真缺陷并已修**：`DialogSurface` r=8 而卡片不裁剪子元素，
于是两张填到卡片边缘的方形 `Border` 把弧盖掉了——`TitleStrip` 让 (1,1)/(2,1)/(1,2) 读成自己的 `#FFFFFF`，
`PART_ButtonArea` 让 (1,h-2)/(2,h-2) 读成卡片的 `#F3F3F3`（同色，所以在表面上完全隐形，只有弧外那格露出方形
"耳"）。解法是每层内表面自带半径 `8,8,0,0` / `0,0,8,8`，改后同一区读回完整弧的斜坡；基座结论写进
S0-x.5：**同色不等于无罪，圆角表面要逐角读像素**。落点：`ThemeResources/ContentDialog.jalxaml` +
`Styles/ContentDialog.jalxaml`（进 Manifest）、`audits/content-dialog.md`、`AstraContentDialogTests` 38 条、
Catalog 行 + Surfaces 页卡片、原始日志 `adaptation/s0x-content-dialog-shown-raw.txt`。四类证据：构建 = 两份字典 + 38 条编译进闸口，串行闸口 **666/666 全绿 0 警告错误 0 skip**、调色板三档 checked=True；
行为 = 38 条（含八格几何表与四角）；视觉 = 两条 `Chrome` 裁剪，**含文本的弹窗
裁剪不总收敛**，因此稳定性只挂在哨兵那条，另加四角单点读数；硬件输入 = **仍为零**（按钮的悬停/按下沿用
Button 批）。闸口首跑还当场抓到**一条闸口自身的缺陷**：`The_gallery_project_carries_the_catalog_it_reads`
把 `samples/FluentJalium.Gallery/bin/**` 下**所有配置**的 catalog 副本都拿来与源文件比对，于是一次 Debug
串行闸口被另一配置留下的一份旧副本判红（`expected ContentDialog / actual Expander`）。这不是产品回归而是
判据写错——它比对的不是"这次构建发出去的那份"。已改成按测试程序集自身路径推出的配置只比对该配置的副本，
并补一条"该配置下必须有副本"的反空过断言（否则目录不再被复制也会静默全绿）。
仍欠（全部记在 `audits/content-dialog.md` §5）：两按钮不是上游的两半而是"两个 1/3 + 一端空 1/3"、
单按钮是 1/3 而非右侧半宽（真解要么给列宽接一个转换器，要么按 AGENTS.md 起自有面板类型——先证明这条差异
值得多一个类型）；`MaxLines=2` 的标题封顶未落地（本运行时是否支持没量）；开合动画为 0；
`ContentDialogOverlayHost` 是否自带第二层烟幕属性面读不到（不暴露 `Background`），需要上屏帧才能结，
这正是用户报的"重影"那一类，暂不声称已排除。

**普查复测批（不占控件名额，2026-09-19）**：下一格开工前先问"我们那份名单还成立吗"。`adaptation/05` §D 是按
**名字匹配**的快照，而 `spike/ControlCensus` 新 `OutstandingNames` pass 按 `Type.Name` 全程序集找 + 打基链 +
打公开构造器，当场量出三条假阴性：**`MenuFlyout` 在**（`: FlyoutBase`，菜单批就是用它 `ShowAt` 开的弹层）、
**`MenuFlyoutPresenter` 在**（`: Control`，唯一公开构造器 `MenuFlyoutPresenter(MenuFlyout)`，所以无参
`Activator.CreateInstance` 抛 `MissingMethodException`——"造不出"被记成了"不存在"）、**`FlyoutBase` 在**
（`Jalium.UI.Controls.Primitives`，而 §D 只查 `Jalium.UI.Controls`）。同时 19 个名字**确认为真缺**，阶段 4 剩下的
`TeachingTip`/`Card` 与阶段 5、6 挂着"(自有)"的名单从此有实测依据。还量回一条 S0-m 的边界：
`UseTemplateContentManagement` 返回 **void**，所以那把锁**只能调用不能读回**，`Expander`/`NavigationView`
挂载读回都是 `style=False factoryTemplate=False`——"锁开没开"在属性面永远没有读数。三份文档按实测更正
（`adaptation/05` 新增 F 节 + §D 顶部标过期、`audits/menu-flyout.md` §0.1 与 §5.5 更正、§5.11 新记一条未结：
`MenuFlyoutPresenter` 到底是不是 `MenuFlyout` 那层表面，它同时是 `MenuBar` 下拉半径欠账的入口）。日志
`adaptation/s0y-outstanding-names.txt`。四类证据：**构建 = 只动 spike 与文档，闸口不构建 spike，因此本条不声称跑过闸口**
（上一格提交的闸口 666/666 仍成立，产品程序集未变）；行为 / 视觉 / 硬件输入 = **各 0 条**，本条只是名单复测。

| 1.0 | CLR API 清单 + 公开资源键清单冻结 + 每控件审计 + Light/Dark 像素证据 + 真实键鼠触证据 + 仅 NuGet 消费者冒烟 | 见 `docs/astra/resources`、`audits`、`testing` |

## 不声称清单（写进每个审计文档，不许被"构建通过"替代）
- 不声称命令栏的打开态（溢出条）有任何像素证据：`IsOpen=true` 只动属性，bar 自己的 `Popup` 在本进程内不开；
  点它自己的省略号确实能开 `Popup`，但那层表面在宿主窗口捕获里贡献 0 像素，因此本批把 7 条打开态行按住不发，
  `AstraAppBarTests.The_open_bar_never_shows_its_popup_in_process_so_no_overflow_row_can_be_promised` 钉的是这个空结果。
- 不声称"原生控件不吃模板 / 只能自绘"这条普查结论现在普遍可推：S0-p 的名单给出 93 个自带 `OnRender` 的导出类型，
  其中本库重模板或重上色的 20 个里**已按"与 `OnRender` 覆写成空的同类逐像素相同"或合成窗口差分量的有 5 例**
  （`InfoBar`、`MenuFlyoutItem`、`ToggleMenuFlyoutItem`、`MenuFlyoutSeparator`、`MenuBarItem`）。
  剩下 15 例仍只到"读得出/读不出部件名"那一层。
  `NumberBox`（要 `PART_LayoutRoot`）、`Slider`（`PART_Segments`）、`ComboBox`（`PART_SelectionPresenter`）
  是**待量的重影候选**，不是已确认的缺陷，也不是已排除的安全项。
- 不声称 flyout 每一处文字都吃我们的令牌：S0-q 量到控件自绘的加速键文案、勾选标记、子项箭头与分隔线
  只读运行时自己调色板的 `TextSecondary`/`TextDisabled`/`MenuFlyoutPresenterBorderBrush`，覆盖同名上游行不动像素；
  这 11 条上游行因此撤回发布。标签走 `Foreground` 是活的，但**标签 hover 变色没有真指针证据**（Task #13），
  两层 hover 填充会不会叠深同样未知。
- 不声称 Gallery 页脚的度量/排布宽度差已治好：本批只把 parity 条改成单元素 + 固定高度滚动宿主绕开它，
  框架侧"换行文本按更宽尺寸量、按实际宽度排"的行为仍在（`spike/TextWrapProbe` case E）。
- 不声称间距已逐控件与上游量化：目视逐页看过，把 padding/margin/MinHeight 对到上游数值并配断言的只有
  InfoBar（`InfoBarContentRootPadding` 等 7 条 Thickness）、Menu 条目（38 的 `MinHeight` 字面量）、命令栏条目
  （64/48 内容带、2,6,2,6 高亮内缩、0,16,0,2 图标槽、2,8,2,8 分隔线）、`ComboBoxItem`（5,2,5,2 行内缩）与
  `Slider`（18×18 拇指、12 内芯、4 刻度间隙、14/Auto/14 三段带），其余仍只到"转录时用的是上游度量行原值"
  这一层，没有逐控件的截图差分；`Slider` 的环在最大值处超出控件 2 DIP 是框架给拇指留 16 的结果，未治。

- 不声称高对比度 parity：公开管线无驱动入口，只有上游逐键映射 + 一个自加键的自行判断（`SliderThumbStrokeBrush`
  已在 Slider 批回收成上游真名 `SliderThumbBorderBrush`，`ToolbarSurfaceBrush` 更早因无消费点删除），
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
- 不声称菜单族有任何硬件输入：本批四类证据里"输入"一栏是**零**。`MenuFlyout.ShowAt`、`ContextMenu.Open(Point)`、
  `RaiseEvent(MouseDown)`、自动化 Invoke 全在进程内；`MenuFlyoutSubItem` 与 `MenuBarItem` 的展开是 mouse-enter /
  键盘驱动，可调用但读不出展开状态，`MenuItem` 的高亮/勾选/箭头三处仍是框架自绘（`audits/menu-flyout.md`）。
- 不声称弹出菜单的外壳可染色：`MenuPopupScrollHost` 那层 Border（`#2C2C2E`/`#48484A`）与 popup 自身尺寸
  （`ContextMenu.MinWidth=140` 实测不生效，宿主给 62.86 宽）都归框架，`MenuFlyoutPresenter*` 两行只对
  `ContextMenu` 我们自绘的那层表面兑现承诺；整族另有 33 个上游名哨兵**改不动框架菜单像素**的实测。
- 不声称菜单尺寸是上游数值：条目样式里那条 `MinHeight=38` 是对 `MenuFlyoutItem` 自己度量并自绘的补偿
  （实测 34 高会把上游 `11,8,11,9` padding 压成 13 高的标签），上游 `MenuFlyoutThemeMinHeight`=32 未发布。
- 更正一条本批自己的旧结论：pass 4 说"框架在 `MenuItem` 上写本地 `Background` 压过样式"，pass 6 在出厂主题下
  复测为否（`IsPressed=True` 而 `Background` 仍是我们的行实例、无本地值）。同一控件在不同装配状态下的结论
  可以相反，凡"框架本地值"账单必须写清当时装的是什么主题。
- 不声称文字颜色有像素证据：**字形不在这条离屏捕获通路里**（只画文字的捕获写 0 像素、且慢到能挂住
  UI 线程），文字色一律只证明到"生成出来的文字元素接上了调色板那个实例"这一层（`adaptation/06`）。
  这条只管**字体字形**，不管几何：选择批已按名字重做那条归因——把表面令牌与标记令牌染成两种哨兵色
  一次拍摄，勾形（几何 `Path`）自己贡献 12 实心 + 22 抗锯齿像素，`Check_mark_follows_the_selected_accent`
  那句"名字过强"的批评就此了结（`audits/checkbox-radiobutton.md`）。
- ~~不声称像素 harness 能连拍~~ **这条已被归因并撤销**（Slider 批，2026-09-18）：当初顶穿 60 秒看门狗的不是
  "一次调用里拍两张"，而是 `PixelHarness.Pump` 的看门狗释放了线程池上另一个没人跑的调度器——
  只要一次状态变化不带动画、不再来帧，`PushFrame` 就永不返回（`adaptation/06` 的"Slider 批"一节）。
  修好后一次调用里建树 + 聚焦 + 捕获 + 读回全部在毫秒级完成（全套 137 条 11 秒）。
  仍然成立的是另一半测量：**只画文字的控件**捕获写 0 像素且慢，所以文字色继续只走读回，E4 逐页渲染别指望
  用离屏截文字。
- ~~不声称三态是**操作得出来**的~~ **这条已结清**（ToggleButton 批，2026-09-18）：框架的 `IToggleProvider.Toggle()` 就是它自己的 `OnClick→OnToggle`，两态与三态循环、`Checked`/`Unchecked`/`Indeterminate` 三个事件、禁用时抛 `InvalidOperationException` 全部进了断言，`{x:Null}` 样式格子的命中与可逆也有探针证据。**仍在的是**：这条通路是自动化模式，不是指针/键盘/触摸，那三条输入通路循环仍未证（Task #13）；且一条会咬人的实测保留——`IsChecked="{x:Null}"` 写在标记里得到的是 `false`，三态只能在代码里设（`adaptation/11`、`audits/togglebutton.md`）。
- 不声称混合态有像素判据：上游把 `ToggleButton*Indeterminate*` 映射到与休息位**同名**的调色板刷，混合与未勾选看着同一个颜色。因此"这张截图证明它是混合态"这句话在这个控件上没有任何捕获能支持，能给的只有探针（格子命中）与读回（实例接上）。选择批里那条基于 `ControlFillColorDefault` 哨兵的 indeterminate 像素断言按此重新定位成"混合态不是强调色"的回归闸，而不是三态证据。
- 不声称 `SplitButton` 的打开态有任何视觉：`FlyoutOpen`/`TouchPressed` 在本运行时无驱动
  （`FlyoutBase.IsOpen` 不是依赖属性、控件也没有 `IsDropDownOpen`），菜单打开时上游会整体压暗，
  我们做不到，也没有伪造一个自有类型去假装做到（`audits/splitbutton.md` 缺口 1）。
- 不声称"部件名写错"这类破坏能被结构或像素证据发现：改名后的模板照样建树、照样上色、布局不变，
  只有点击会静默失效。凡是框架靠名字接线的部件，结论必须来自一次真实的驱动，且要配反例
  （`The_named_halves_drive_the_flyout_and_the_click_and_the_renamed_ones_do_not`）。
- 不声称 `control.Style` 能读回隐式样式：本运行时挂上主题后 `Button`/`ToggleButton`/`HyperlinkButton`/
  `SplitButton` 的 `Style` 全是 null，而像素确实是我们令牌的。任何"`Style` 非空 = 样式生效"的断言都是假阳性，
  读法改成模板对象身份（`adaptation/00` S0-l）。
- 不声称 `SplitButtonBorderBrush*` 三条描边别名覆盖得出差异：上游的 elevation 渐变不能随原地重染色走，
  休息/悬停/按下三条都落在同一个 `ControlStrokeColorDefaultBrush` 实例上，
  所以"覆盖这个键画面会变"这句话在描边一侧是空话（`audits/splitbutton.md` §1）。
- 不声称样式格子压得过本地值：`IsChecked=true` 而控件带本地 `Background` 时，读回仍是本地刷——本运行时的样式格子排在本地值之下，上游 VisualState 故事板恰好相反。应用若照抄 WinUI 的"给按钮设个底色，状态还会盖回来"写法，在这里会做出一个永不换色的开关（`A_local_fill_keeps_the_surface_over_the_checked_cell`，`audits/togglebutton.md` S4）。
- 不声称"setter 上写了键 = 屏幕上就是这个色"。阶段 1 把原生控件默认交回 Jalium（`Application.ThemeMode`），
  代价这轮才量到：框架在个别状态上给控件设的是**本地值**，压过样式 setter 与模板触发器。实测两处——
  禁用挂载的 `TextBox.Foreground` 读回 `#FFAEAEB2`（不是我们的 `TextControlForegroundDisabled`），
  静息 `PasswordBox.Background` 读回 `#D9FFFFFF`（不是 `TextControlBackground`）。文本框**模板里的面**
  （`OuterBorder`/`BottomEdge`）不受影响，照常命中实例。两处结论都写成 `Assert.NotSame` 钉在闸口里，
  PasswordBox 因此一格像素主张都不给（`audits/textbox-passwordbox.md`）。
  ComboBox 批量到**第三处**：禁用挂载的 `ComboBox.Foreground` 读回同一个 `#FFAEAEB2`，
  于是 `ComboBoxForegroundDisabled` 只到表面与字形、到不了文字，`ComboBoxPlaceHolderForegroundDisabled`
  没有杠杆可用因此不转录（`audits/combobox.md`）。
- 不声称"结构断言过了 = 状态格会生效"。ComboBox 批实测两类静默失效：`Trigger Value=""` **永不匹配**
  （属性解析正常、setter 键名正确、消费点闸口全绿，控件却纹丝不动；`SelectedIndex=-1` 立刻有效），
  以及框架把占位串直接放进 `SelectionBoxItem`（未选中时它等于 `PlaceholderText` 而不是 `null`，
  所以"没有选中"没法写成 `SelectionBoxItem={x:Null}`）。对策写进 `adaptation/00` S0-g：
  **每一条状态格都要配一条读回断言**，只验 markup 结构不算证据。
- 不声称框架本地值只落在"控件级"。NumberBox 批量到**第四、五处**，而且落在**部件**上：
  框架往 `PART_UpSpinButton`/`PART_DownSpinButton` 写本地 `BorderThickness=1,0,0,0`，
  上游的 `NumberBoxSpinButtonBorderThickness`（`0,1,1,1`）因此到不了；换进去的模板里
  `ContentPresenter` 生成的标题文字元素带本地 `Foreground=#FF1D1D1F`（不是我们调色板那个实例），
  所以 `TextControlHeaderForeground*` 两行只写到 presenter、到不了字形。
  两处都写成断言钉住（`The_framework_keeps_what_it_took_ownership_of`），不拿"setter 上有键"当结论（`audits/numberbox.md`）。
- 不声称上游的模板局部别名块能搬到应用级：本运行时的资源名是**全局一份**，清单里后入者胜。
  把上游 NumberBox 模板自带的 `RepeatButton*` 十行搬到 `ThemeResources/NumberBox.jalxaml`，
  直接盖掉 `ThemeResources/RepeatButton.jalxaml` 的同名行并打挂两条 Button 断言。
  现在这些名字归 RepeatButton，spin 按钮改读它们的目标令牌，并钉
  `The_repeatbutton_names_still_belong_to_the_repeat_button`（`adaptation/00` S0-h）。
- 不声称普查表的"自绘"列可信：`05-native-control-vacuum.md` 那一列是 `OnRender`/`OnPaint`
  反射启发式，NumberBox 批实测证伪——`Style == null` 不等于没有默认外观，框架给模板控件
  **在代码里构建了 `ControlTemplate`**（`Template != null`，部件名齐全，且 `Style` 的 `Template`
  setter 能整个换掉）。该表已加免责声明并按新判据重算（`done 11→13`、`HARDGAPS 43→41`）；
  结论 1 的更正见 `adaptation/01`，三条框架契约见 `adaptation/00` S0-h。
- 不声称"空串判据永不匹配"是语法级结论：AutoSuggestBox 批在同一写法上量到**相反**结果
  （`AutoCompleteBox.Text` 静息真的是 `""`，格子触发、输入一个字符后失效）。两次读数差在哪
  （属性静息值还是控件类型）没取证，能下的结论只有"判据必须逐属性读回"（`adaptation/00` S0-g 已按此降级）。
- 不声称弹层像素能证明"画面干净"：弹层裁剪里含框架自己上了渐变的条目，`Sample.Stable` 与
  "品牌绿不出现"在那一层都不可断言，只能断"某个哨兵色存在且够大"（S0-j）。
- 不声称换模板只换外观：换上我们的模板之后 `UpdateTextOnSelect` 也没了——选中建议不再把文本写回框内
  （聚焦、`IsTextCompletionEnabled=true` 也一样）。这条已写成断言；"外观走标记、行为要么放弃要么自有类型"
  的分界线在本批第一次有读数（`audits/autosuggestbox.md` §0.8）。
- 不声称 `AutoSuggestBox` 已经交付：交出去的是宿主替身的外观。`QueryIcon`/`Header`/`Description`/
  `QuerySubmitted`/light-dismiss 仍在，`AutoSuggestBox` 这个名字也仍在真空清单 D 节"无同名原生类型"里。
- 不声称硬件触摸笔与混合 DPI 已经过真机验证。
- 不声称 `RaiseEvent` 合成的 `MouseDown` 是输入证据：它是进程内路由事件，绕过命中测试、窗口输入栈与
  指针捕获，只能证明"控件自己的处理器接到这个事件后做了什么"。Expander/InfoBar 的鼠标、触摸、笔三条
  真实通路仍然**一条都没有**（`audits/expander.md`、`audits/infobar.md` §4 的"硬件输入"一栏写的是"未做"）。
- 不声称跨部件状态格（`Trigger SourceName="PART_HeaderBorder"` 的悬停格）在无指针条件下会被求值：
  本批断言的是格子的**结构与键名**（属性名合法、setter 指向的行有消费点、观察的部件名正确），
  而"手放上去这一格真的赢"没有任何读数——`RaiseEvent` 不产生 `IsMouseOver`，像素基座也没有指针。
  这条与 Button 批的真指针通路（Task #13）同欠，等那条通路进常跑闸口再撤。
- 不声称"原生控件不吃模板"是原生 `InfoBar` 的固有属性、也不声称反过来那件事：实测到的只是**这两个**
  控件在 `UseTemplateContentManagement()` 上的差别（`Expander` 调、`InfoBar` 不调）。
  其余原生控件有没有同一把锁、开锁后 `OnRender` 是否都像这里一样让路，**一个都还没量**，
  所以既不能拿它当"重模板一定可行"的普适前提，也不能继续拿"改了模板没反应"当自绘证据
  （`adaptation/00` S0-m 末条，普查表 `01`/`05` 的返工单）。
- 不声称 Gallery 每页画对了：目录闸口证明的是"样式宇宙与目录一致、声明有存在的证据"。
  NumberBox 批起 `--page <id>` 能把任意一页挂上屏；AutoSuggestBox 批进一步读到页首那条 parity 条
  （`--page inputs` 截图里 "Inputs · AutoCompleteBox audited, … · 6 of 19 restyled types" 与本批六条 gap
  文案逐条入镜），所以**"目录文案 → parity 条 → 屏幕"这条通路现在是看得见的**。但**卡片本身在折叠线以下时
  依旧没有入镜**，新加的家族卡与表面卡仍**未逐卡目视、未逐卡断言**，等 E4 的逐页/逐卡渲染（`adaptation/10`）。
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
