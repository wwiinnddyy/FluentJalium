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

**视觉缺陷批第十一段（"输入补全的下拉框明显偏小"，2026-09-20）**：用户第四次报问题，这次点到 `AutoSuggestBox`
的建议列表——"和其他下拉框、flyout 控件比都偏小"。先把它拆成三条能量、且互不相干的轴，再逐轴给数：
(1) **宽度不是缺陷。** 真上屏帧（`spike/RightGapProbe/out/`，`spike/VisualQA/scan-line.ps1` 逐行读色区间）：
440 DIP 的盒子里列表卡片 `#2C2C2C` 落在 x=217..982（766 px），控件本体 `#222222` 落在 x=218..982（765 px），
dpi=168——差的 1 px 是边框，不是"右边空一块"。
(2) **高度上限是一条真缺陷，已改。** `AutoSuggestListMaxHeight` 的 374 字面量原本落在**卡片**
（`SuggestionsContainer`）上，而卡片自己带 4 padding + 2 border，于是列表只有 368，比上游少 6 DIP
（旧 gap 7 记的就是这个）。上游那个 374 挂在 `SuggestionsList` 上、卡片在它之外再加厚，所以字面量移回
`PART_DropDownScrollViewer`（本宿主里"列表"的那一层）。改后 `PopupRoot.Height` 与卡片同为 **380** = 374+4+2。
(3) **行数不是缺陷，是数据。** Gallery 那 10 个水果按前缀过滤，输入 "a" 只剩 2 条，而弹层根高按内容钉
（实测 41.78 / 77.6 / 380 三档），"看着小"来自条目少而不是被压扁。

**本段公开撤回上一段的一条结论**：那次写的"`MaxDropDownHeight=200` 封顶了列表"不成立——探针里的 `ItemFilter`
参数写反了（本运行时是 `(text, item)`，WinUI 是 `(item, text)`），于是零命中、弹层根本没开，读到的"200"是
**闭合控件自己的高**。重测后量到 `AutoCompleteBox.MaxDropDownHeight` 在本运行时**是惰的**（默认 200，改 120/374/700
表面都不动），而 `ComboBox.MaxDropDownHeight` 默认 504 **确实生效**（24 行量到 506）。这条签名差异同时作废
`right-gap.md` 里"探针里 `IsDropDownOpen=true` / `Text` setter 都送不上屏"那条 gap：弹层能开，之前的"闭合"读数
是过滤器的假象，已在该文档结清，顺带把"建议列表宽度只在 harness 一侧有证据"一并了结。
三条样式够不到的新账进 `audits/autosuggestbox.md` §6.4（gap 14–16）：条目容器带框架本地 `MinHeight=28`
（combo 条目从我们的样式读 32，所以建议条目肉眼可见地比下拉条目矮）；`AutoSuggestListPadding` 的 `-1,0,-1,0`
由框架盖在实化的 `ScrollViewer` 上，把行改成 `0,0,0,0` 也压不过（本批这样改过一次，验证无效后回滚）；
以及打开的列表左缘有一圈环加一个 "(" 形标记，**整棵子树里没有任何元素能解释它**——这是这一格最终要起自有
`AutoSuggestBox` 类型最硬的一条证据。落点：`Styles/AutoSuggestBox.jalxaml`（字面量移层 + 注释里两条作废读数
改写）、`ThemeResources/AutoSuggestBox.jalxaml`（padding 回滚并记录实测）、`AstraAutoSuggestBoxTests` 31→32 条、
`audits/autosuggestbox.md` §6、`audits/right-gap.md` gap 1 结清、`adaptation/s0z-suggestion-surface.txt`
（4 个 pass 的原始日志）、Catalog 该行 gaps 6→9 条、`spike/RightGapProbe` 新增 `listheight`/`clamp`/`itemstyle`/
`suggest10` 四个 pass 与 `capture-suggest10.ps1`、`spike/VisualQA/scan-line.ps1`。四类证据：构建 = 串行闸口
**667/667 全绿、0 skip**、调色板三档 checked=True（666→667 就是新那条尺寸用例）；行为 = 新增 1 条
（上限落在列表不在卡片：`MaxDropDownHeight=200`、`scroller.MaxHeight=374`、`container.MaxHeight=∞`、
卡片高 = 列表高 + 6 且 ≤380、宽与盒相等）+ 1 条旧用例按新落点改判 + 条目 `MinHeight=28` 钉住；
视觉 = 上屏帧那条 766/765 px，**行高 380 只在探针日志里**——共享测试宿主即使把 host 拉到 460、
先 `Focus()` 再赋值、泵 8 轮，弹层仍只实化 **1 个**容器，这条宿主限制写进了新用例注释而不是假装测过；
硬件输入 = **仍为零**（弹层由赋值 `Text` 打开，没有真指针或触摸）。
闸口首跑还带进两条不属于本批的编译警告：`MainWindow.jalxaml.cs:289-290` 对 `PrimaryButtonText` /
`SecondaryButtonText` 赋 `null`（框架把这两个属性标成非空 `string`，而"没有按钮"正是靠 `null` 表达，
`AstraContentDialogTests` 里就钉着 `PrimaryButtonText=null` 那几格）。已在调用点用 `null!` 消注解差、
不改语义，Gallery 单项目重编 **0 警告 0 错误**；`tools/Test-AstraGallerySmoke.ps1 -Page inputs` 出窗口并在
8.3 秒内干净关闭、无残留进程。

**视觉缺陷批第十二段（"有的控件圆角好像都不对"，2026-09-20）**：用户第二次报的那半句"圆角不对"这次变成数。
`spike/FlyoutSurfaceProbe`（`tree`/`radius`/`sweep`/`context`/`context14`，原始输出
`adaptation/s1a-menu-surface-raw.txt`）量出四层：

(1) **`ContextMenu` 的卡片角是 14、不是上游的 8，一条 setter 改掉。** 框架在 `ContextMenu.Open` 时**在我们模板
之外再造一层 `Border`**，把控件的 `Background`/`BorderBrush`/`BorderThickness`/`CornerRadius` 抄成它的本地值——
`MenuFlyoutPresenter*` 那几格正是经这一次复制才落到像素。抄的条件是"控件**被告知**的值"而不是"任何有效值"：
控件没被写过 → 表面留在框架自带的 14；显式写 0 → 表面 0（所以 0 是真实请求、不是"未设置"哨兵）；往那层
`Border` 自己身上写不是出路（清掉之后掉到 0，它背后没有样式）；`MenuFlyout`/`FlyoutBase`/`Popup`/`PopupRoot`
整条链都没有半径属性，只有 `ContextMenu` 有 ⇒ **样式 setter 是这张表面唯一听我们的入口**。同一条 `radius` 模式
改前读 **14,14,14,14 LOCAL**、改后读 **8,8,8,8 LOCAL**，且清掉本地值后回到 8 而不是 14——改的只有那一格。
上游证据：`MenuFlyout_themeresources.xaml:285` 用 `OverlayCornerRadius`，键值 8 在
`CornerRadius_themeresources.xaml:6`；ModernWpf `MenuFlyout.xaml:20` 同键，其 WPF `ContextMenu.xaml:37` 硬编码
8；菜单表面没有 `KeepInteriorCornersSquare`。
(2) **全族半径表收齐，没有一格还留在 14**：`Menu` 子菜单卡 8（同一条复制通路）、`MenuFlyout` 卡 8（框架自己写的）、
`ComboBox` 下拉卡 8、`AutoCompleteBox` 建议卡 8（这两张是我们的模板）、条目 4（与上游同值）。
(3) **gap 11 那笔"presenter 与 scroll host 谁是谁"的账结了**：`MenuFlyoutPresenter` 在 26.10.9 上是
`internal sealed : Control`，自己不声明任何依赖属性，按类型键与按名字键查隐式样式都是 `null` → 样式改不动它、
C# 里点名直接 CS0122；但它打开时带 `radius=8,8,8,8 LOCAL` + `bt=1,1,1,1 LOCAL`、`Background`/`BorderBrush`
**为 null**，弹层还在自己那层顶层 `PopupWindow` 里 ⇒ 半径本来就和上游一致、卡片色完全归框架。`ContextMenu`
那棵树里**根本没有 presenter**，所以 §0.5 原来那条"外壳是 `MenuPopupScrollHost`"改写成两种表面分别记账。
(4) **弧用真上屏帧对照**：同一张 130 px（=74.27 DIP×1.75）宽、同一条左边 x=350..479 的卡片，只差半径——
**弧深**在 8 那一档两次重采都是 9~10 行、14 那一档 17 行；控件显式写 0 时弧消失。**"顶行咬进几 px"这一格被撤下
当论据**：同一个 `context` 模式重采一次就从 12 px 变到 9 px（两帧弹窗落点差 0.67 DIP），所以本批只取弧深这
一档稳定的量。弧的绝对尺寸也不等于 半径×1.75（8 DIP 应 14 px、量到 9~10；14 DIP 应 24.5、量到 17），两档短
同一个系数 ⇒ 比值干净、绝对值不干净，帧只证"变了"，半径数值以读回为准。是渲染器把弧画短还是测法吃掉像素，
需要跨 DPI 对照才能分开，本机给不了 ⇒ **未解释**，进不声称清单。

落点：`Styles/Menus.jalxaml`（一条 setter + 注释写清复制契约）、`AstraMenuTests` 104→**110**、
`audits/menu-flyout.md`（§0.5 更正、新 §0.12/0.13、§2 该行改写、新 §4c 半径表与三帧、§5.3/5.4/5.5 更正、
**§5.11 结清**、新 §5.12/5.13）、`adaptation/00` 新 **S1-a**、`adaptation/s1a-menu-surface-raw.txt`、
新工具 `spike/VisualQA/corner-edge.ps1`（按"非底色列"读外沿弧，自动定位卡片）、
`spike/VisualQA/corner-stair.ps1`（严格填充色那一档）、`spike/FlyoutSurfaceProbe`（探针 + `capture-context.ps1`），
Catalog `ContextMenu` 行 gaps 2→3 条，**含一条公开撤回**：原写"presenter 行只落进我们模板的表面、外面那层宿主
碰不到"，实测正好相反——像素恰恰只经外面那层复制才出现，我们模板里的 `LayoutRoot` 在它里面。
四类证据：构建 = 串行闸口 **673/673 全绿、0 skip**、调色板三档 checked=True（667→673 就是新那六条）；
行为 = +6 条（样式那格读回 `OverlayCornerRadius`、抄出来的 `Border` 戴我们的行与上游半径、子菜单表面、
presenter 类型存在但不可样式、它在树里却不画我们的色、本地写 3 就走 3 且 `BorderThickness` 同理）；
视觉 = 上面那两帧（原图 `spike/FlyoutSurfaceProbe/out-*.png` 没有入库——顺带更正一处旧说法：这些探针帧路径
**不在 `.gitignore` 里**（`git check-ignore` 空返回），不入库只是因为没人 `git add`，`audits/autosuggestbox.md` §6
与 `adaptation/s0z` 那句"被 gitignore 挡住"是错的，本批一并改），逐行数已抄进原始文件），而
**原本要写的那条"角上第一个填充列"单点像素断言没有留下**：同一个 `Open()` 在探针自己 show 的窗口里给
74.27x70 的表面，在共享 xunit 宿主里给 0×0，`PixelHarness.PixelAt` 直接抛"has no layout to sample"——
这条宿主限制写进了 §4c 与用例注释，没有把测不到的东西伪装成测到；硬件输入 = **仍为零**，本批还复测量到
`MenuBarItem` 下拉依旧开不起来（`IsSubmenuOpen` 不是它的属性，合成 MouseDown 后条目仍挂在 `Window` 根下），
只有 `Menu` 的子菜单开得动。

**阶段 4 第五段（TeachingTip 起自有类型；Card 查无此控件，2026-09-20）**：这一段的开工前提是三条"未证"，
全部先量后写（`spike/TeachingTipProbe` modes `all`/`tail`/`place`/`tip`/`room`/`parent`，原始输出
`adaptation/s1b-teachingtip-host-raw.txt`）：

(1) **模板内 `Popup` 这条路成立，代价是卡片不在宿主树里。** 调过 `UseTemplateContentManagement()` 的
`ContentControl` 能把 `<Popup Name="PART_Popup">` 建起来并实化整张卡片（`Container 320x168.12` /
`ContentRootGrid 304x152.12` / 按钮 `130x32.78` / `TailPolygon 9x21`），**但带 `PlacementTarget` 的弹层落在自己
的顶层 `PopupWindow` 里**（父链 `Border < PopupRoot < PopupWindow`）——判据必须从 `popup.Child` 往下走，
从宿主窗口往下读一条也读不到；不带 target 时才 graft 进宿主 `OverlayLayer`。收起时全套件读 `0x0`，
那是静息形状而不是失败。
(2) **放置只有六个把手，不是上游的十八个。** `PlacementMode` 12 值里没有任何 per-edge / per-corner，
`Relative` 把子元素原点钉在目标左上角（偏移 `0,0` 读回 `-0.29,-0.29`、`50,50` 精确、`-120,-50` 精确），
四条边值各把子元素推到 `y=目标高` / `y=-子高` / `x=-子宽` / `x=目标宽` ⇒ 控件写 `EffectivePlacement` +
两个偏移算术，模板格子读属性搬尾巴。自动朝向与回退序照 ModernWpf `TeachingTip.cs:643-682`。
尾巴"在哪条边就清哪条边"的描边走**不带 TargetName 的格子写控件 `BorderThickness` + 卡片 `TemplateBinding`**
（实测 `Right` 读回 `0,1,1,1`）；格子也赢过具名部件的标记属性（D3 三项全改）。
(3) **`GridLength` 行能解析，进不去的是落点。** `8` 与 `*` 都读回原值，但
`ColumnDefinition.Width="{ThemeResource …}"` **被静默丢弃**（五列全停在默认 `*`）——`ColumnDefinition` 不在
渲染树里，动态查找没有继承上下文，于是 `8|10|*|10|8` 只能写字面量；`ContentDialog` 批那条"ButtonSpacing 也
解析不了"的**理由**当场换成这条（结论不变，旧理由是错的）。顺带把 S0-b 那句"x:Double 进不去"改准：它是
**整份字典解析失败**，不是一行被丢。
(4) **推翻本批自己的一条主张。** 首版 `FluentTeachingTip` 带一个返回 `new Size()` 的 `MeasureOverride`，
理由写"实测占掉 320x168 布局"——那行读数其实是卡片在**自己那个 `PopupWindow`** 里的实现尺寸。同一支探针在
有 / 无 override 两种 build 下逐列相同（`desired 0x0`、压在下面的邻居 `y=20`，收起 / 打开 / 带 target 再打开
三档都是）⇒ override 是死代码，已删；那条 fact 同时被证明**当时没有牙**，改成读邻居落点、只当"模板根必须
还是 Popup"的形状闸口（`adaptation/00` 新 **S1-b·6**）。
(5) **结构闸口把"找自己所在窗口"逼上逻辑树。** `AstraGateTests` 全文本禁产品代码走渲染树（连注释里写出那个
API 名字都判红），改用 `FrameworkElement.Parent`：挂载链 `FluentTeachingTip < Grid < StackPanel < Window`。
`Application.Current.MainWindow` 一并量到并**拒绝**——它给整个应用只点名一个窗口（S1-b·7）。

落点：`Controls/Popup/FluentTeachingTip.cs`（自有类型 + 两个枚举 + 16 个 DP + 4 个事件）、
`ThemeResources/TeachingTip.jalxaml`（**上游 50 个行名发 21 条**：5 别名 + 16 `Thickness`，逐条有消费点，
其余 **29 条逐名配反向断言**）、`Styles/TeachingTip.jalxaml`（命名样式 + 隐式样式 + 内联模板，
**16 格触发器 / 54 个 setter**，逐格数自 grep 而非估算）、`Themes/Manifest.txt` 34→**36** 份、
新测 `AstraTeachingTipTests` **73 条**、`audits/teachingtip.md`（§1 行名数更正为 20+30、§3 表逐行改成实测落点、
§5 缺口 8→12 条、新增 §6 Card 改判）、`adaptation/00` 新 **S1-b**、`adaptation/s1b-teachingtip-host-raw.txt`、
`audits/content-dialog.md` 与 `Styles/ContentDialog.jalxaml` 的旧理由更正、Gallery Surfaces 页两张新卡
（Teaching tip：有 target / 换边 / 无 target / 尾巴开关 / 按钮开关 + 读数条；Cards：三档底色配方）、
`Catalog.json` 新 `FluentJalium.Controls.FluentTeachingTip` 行（parity `own-type`，9 条 gap）。
**Card 改判**：WinUI 3 全仓没有 `Card` 运行时类 / 模板 / 主题字典（`class Card` 与 `runtimeclass Card` 均 0 命中），
Gallery 的卡片是手搓 `Border` + `CardStrokeColorDefaultBrush` + 一档 `CardBackgroundFillColor*`，ModernWpf 也没有
Card ⇒ 本段交付**令牌 + 配方**（`Styles/Common.jalxaml` 的 `CardBorderStyle` 早就是那份配方），不起空类型。

四类证据：构建 = 串行闸口 **747/747 全绿、0 警告、0 skip**、调色板三档 checked=True（Light/Dark 各 83 源色
101 刷，HC 101 映射 + 3 条上游键因调色板无对应而按住）；行为 = 5 条别名身份 + 16 条几何值 + 29 条"不发即名"
+ 部件树 + 逐朝向尾格子 + 无 target + 尾可见性 + 标题/副标题/内容三态 + 单按钮幸存 + 关闭键与动作键的契约差
（只有 `CloseButtonClick` 之后 `IsOpen` 变假）+ 命令转发 + 打开/关闭事件与 `PART_Popup.IsOpen` 同向 +
四条"尾巴贴在目标边上"的几何不变量 + 放不下就换边 + Auto/Center 语义 + 零占位仍实化 + `8|10|*|10|8` 带 +
尺寸盒 + 自动化 peer 调用；视觉 = 一条弹层像素（给 `SolidBackgroundFillColorTertiaryBrush` 打洋红哨兵，
在卡片中心读回 `#FF00FF` 且品牌绿 0 命中）；硬件输入 = **仍为零**（任务 13），按钮走
`ButtonAutomationPeer`/`IInvokeProvider`，放置走 `TranslatePoint` 读回。

不声称：不声称点外关闭、图标、hero、右上角关闭、顶部高光、入场动画、`ThemeShadow`、窄窗口缩放与十八向贴边
（§5.9-5.12 各有一条理由）；不声称真指针 / 触摸 / 键盘下的打开与关闭；不声称卡片在宿主窗口里可被像素采样
（它在自己的 `PopupWindow`，那条洋红断言是从 `popup.Child` 那棵子树上采的）；不声称 `EffectivePlacement`
有上游那样的连续跟随——`LayoutUpdated` 只在目标挂过的时候接，滚动容器里的重排未测。
Gallery 侧 8 页全部上屏并优雅关闭、无残留进程；新卡片本身在折叠线以下，**未目视**。

**阶段 5 第一段（ListBox + ListBoxItem，"左右边距不一样长"在列表上结清，2026-09-20）**：阶段 4 收尾后开列表族，
先量后写。`spike/ListProbe`（10 个模式：types/mount/retmpl/virtual/select/gutter/itemstyle/theme/itemtmpl/attach，
原始输出 `adaptation/s1c-list-host-raw.txt`，含样式落地后复测的第二趟）量出七件承重的事：

(1) **宿主能接我们的模板，不需要解锁调用**（`ListBox : Selector : ItemsControl : Control` 那条链不调
`UseTemplateContentManagement`），而条目宿主的契约是**类型不是名字**：`ItemsPresenter` 改名照样出容器，换成普通
`StackPanel` 就 0 个。(2) **容器样式只有隐式类型键这一条干净**：应用级（或窗口级）合入
`<Style TargetType="ListBoxItem">` 后框架生成的容器**当场就吃**，而容器自己的 `Style` 属性永远读 null；
`ItemContainerStyle` 压过它并把 `Style` 写成非 null——所以本批发隐式键、`ListBox` 样式**不写**
`ItemContainerStyle`，并把这条钉成断言（写了就是收走消费方的覆写权）。同时顺带**撤回一条旧预测**：
`AstraItemHostTests` 那句"本地赋值，因为列表批会走这条路"是错的预测，改成带理由的更正而不是静默删掉。
(3) **选中本来就被画出来了，画的是 `#99680081`**——原生条目 `PART_BackgroundBorder` 在 `SelectedIndex=0` 后读这个值，
正是上游 `SystemControlHighlightListAccentLowBrush`（`generic.xaml:301` = 强调色 @0.6）落在 OS 默认强调色上；
原生 resting padding 实测 10,6,10,6 而上游那格是 12,9,12,12。这两条差就是"必须重模板"的量化理由。
(4) **0.6 挂不进调色板，于是透明度搬到部件**：`Light.jalxaml` / `Dark.jalxaml` 是
`tools/Sync-AstraPalette.ps1` 生成的（手工加行会被生成器和调色板闸口抹掉），我们的强调色只有 1/0.9/0.8 三档，
所以三条选中行只带颜色（别名到 `AccentFillColorDefaultBrush`，跟着应用强调色走），
0.6 / 0.8 / 0.9 由同一状态写高亮部件的 `Opacity` 带——单层实色乘出来的像素与上游那支 brush 等价。
(5) **12 DIP 右槽在列表面上被数出来**（用户那半句"边距不一样长"的第三处根）：我们自己的壳里左 1 右 13、
原生左 3 右 15，而 `IsOverlayScrollBarEnabled="True"` 把两档都拉成 0/0（5 条和 50 条一样），
`IsScrollBarAutoHideEnabled` 对此**完全没用**；第二趟还量到 `Hidden` 根本不扣布局，所以已发货的 ComboBox 下拉
与建议列表不在这条缺陷上。复测里另一个可见后果：行从 33.78 长到 40.78 之后，5 条就溢出，滚动条真的出来了。
(6) **附着 setter 这条路在本读法下是死的**：mode J 四格全读 `viewer.Auto/False`——带 `ScrollViewer.` 前缀的
setter 解析成 Style 却到不了部件，`{TemplateBinding ScrollViewer.X}` 也读不到（回落到类型默认），
只有把属性字面写在部件上才生效；仓库既有 11 个样式文件里 `Property="ScrollViewer.` 出现 **0 次**，与此一致。
代价是应用改不动我们列表的滚动条开关，进 Catalog gaps。(7) **选择语义整个是框架的**（`HandleArrowKey`/
`HandleSpaceKey`/`HandleDragSelect`/`Select{Single,Multiple,Extended,Range,All}` + 三个 UIA provider），
1000 项只实现 11 个容器，`ScrollToVerticalOffset` 与 `IScrollItemProvider.ScrollIntoView()` 都动得起来 ⇒
**不起自有类型**；另外 `ControlTemplate` 只许一个视觉根（并列两根本地抛 `XamlParseException` 并带走整本字典），
`ListBoxItem` 没有 `IsHighlighted`/`IsPressed`/`IsSelectionActive`，这两条决定了状态映射的形状。

落点：`ThemeResources/ListBox.jalxaml`（**12 行**：10 条别名 + 1 条描边厚 + 1 条条目 padding；上游 27 个行名里
15 条 8.1 世代 `*ThemeBrush` 不发，因为上游自己也无人读）、`Styles/ListBoxes.jalxaml`（两张模板、**17 个属性
setter、6 格触发器、15 条部件级 setter**，两条隐式样式）、`Themes/Manifest.txt` 两行、
`audits/listbox.md`（§1 逐行表 + §3 八态映射 + §5 八条差异 + §6 不声称）、
`adaptation/00` 新 **S1-c**（10 条）、`adaptation/s1c-list-host-raw.txt`（两趟）、`AstraListBoxTests`
（**22 个方法 / 45 条断言事实**，含 15 条"不发即名"）、`AstraResourceKeyTests` 新增本字典的逐行消费闸口、
`AstraGalleryCatalogTests` **删掉 `ListBox` / `ListBoxItem` 两条欠账豁免**（留着就等于允许以后把列表样式删掉还判绿）、
Catalog 新增两行（各带 4-5 条 gap）、Gallery `selection` 页加一条多选溢出列表 + 一句诚实说明。
新工具 `spike/ListProbe`。四类证据：构建 = 串行闸口 **793/793 全绿、0 skip、本批 0 新增警告**（构建报的 18 条
警告全部落在 `AstraMenuTests` / `AstraAppBarTests` / `AstraContentDialogTests` 三个既有文件的可空性告警上，与本批无关、未清），
调色板三档 checked=True
且 **83 源色 / 101 刷未变**（本批没碰生成层，这是 §5.3 那条改法的直接后果）；行为 = 隐式键落地（`Style` 读 null
而模板对象与 `GetStyle` 里那条同实例）、部件名换血（`PART_OuterBorder` 消失、`LayoutRoot`/`PressedBackground` 出现）、
padding 行吃掉原生 10,6,10,6、行宽=列宽且左右隙同为 0、溢出时仍为 0、逐模式的多选与单选排他、1000 项只实现一屏、
长文本不换行、滚动 400 后顶行真的上去、`ScrollIntoView` 把贴边的行完整带进来、UIA provider 能力；
视觉 = 4 条（表面 token 上屏、选中把那一格从底色推向强调色【两帧对比，不假装算得出混合色】、框架紫 0 命中、
Light↔Dark 顶色不同）；硬件输入 = **仍为零**（任务 13），本批还实测到 `ISelectionItemProvider.Select()`
从容器自建的 peer 调用**不改** `SelectedIndex`，因此只声称能力、不声称驱动。
上屏 = `Test-AstraGallerySmoke.ps1 -Page selection,menus,overview` 三页全部 mount 后优雅关闭、无残留进程
（本工具故意不截屏，理由记在 `adaptation/06`）；新增的两条列表在 `selection` 页折叠线以下，**未目视**。

不声称：不声称悬停 / 按下 / 拖选 / 键鼠导航有任何真实输入证据；不声称焦点框（原生模板树里没有焦点部件、
本运行时也没有 `UseSystemFocusVisuals` 槽位，不自造外观）；不声称选中色的**叠加**结果与上游逐位相同；
不声称 `SelectedItems` 可用（实测写 `SelectedIndex` 之后仍读空，故多条选择的断言全部改读容器自己的 `IsSelected`）；
不声称 ListView / GridView / TreeView / DataGrid 沾了这批的光——它们各自的模板与条目类型还要各自量。

**阶段 5 第二段（ListView + ListViewItem，WinUI 的 GridView 被量掉，2026-09-20）**：接着列表族往下走，还是先量后写。
`spike/ListViewProbe`（10 模式：types/mount/retmpl/itemstyle/states/gutter/grid/cells/bar/panel，原始输出
`adaptation/s1d-listview-host-raw.txt`，含样式落地后复测的那一趟）先推翻两条前提，再量出六件承重的事：

(1) **列表族是继承出来的**：`ListView : ListBox : Selector : ItemsControl : Control`，`ListViewItem : ListBoxItem`
且**自己一个成员都不声明**，唯一的自有 DP 是 `View`。所以 S1-c 那张六格状态矩阵与"宿主契约是类型不是名字"
"条目样式只走隐式类型键（容器 `Style` 恒 null）"三条在派生宿主上原样成立，本批是**复用**不是重做。
(2) **WinUI 的 GridView 在这个运行时不存在**：`Jalium.UI.Controls.GridView` 是 WPF 那个"给 ListView 装列"的
`ViewBase : DependencyObject`，`is Control=False`、构造出来不能当内容（实测 `InvalidCastException`），
`GridViewItem` 未导出；WinUI 列表行为面逐条打点全是缺（`IsItemClickEnabled`/`ShowSelectionChecks`/`MultiSelect`/
`ItemContainerTransitions`/`ItemWidth`/`SemanticZoom`/`ItemsRepeater`/`ListViewItemPresenter` 等 16 条）。
唯一能走的卡片路线是换 ItemsPanel，实测真能走（`WrapPanel` 下 6 行排成 4+2，第二行 y=32.78），但那只是"能排成网格"，
**不是 WinUI 的 `GridView`**——本批不交、也不声称。(3) **上游根本没有 `ListView*` 宿主级主题行**：整棵参考树 grep 不到
`ListViewBackground`/`ListViewStyle`，也没有 `controls/dev/ListView/` 目录（宿主外壳在闭源 dxaml/generic.xaml），
所以宿主吃已发的 `ListBox*` 行且**不自造名**（两条 `Assert.Null` 钉住没造名，一条像素断言钉住"一次覆写同时动两个宿主"）。
(4) **容器的部件名连内容契约都不是**：四种自造条目模板（保留 / 改名 / 去掉 `PART_CellsPanel` / 只留 presenter）里内容
全照常渲染，`IsSelected` 也照常随 `SelectedIndex` 变 True；更关键的是出厂模板选中时那支 `#99680081` 在我们模板下
**一次都没再出现**——它是出厂模板自己的格子画的，不是框架按名字写进部件的。唯一硬要求仍是有个 `ContentPresenter`。
(5) **WinUI 的 ListView 选中行不是强调色块**：上游 8 条底色行与 6 条字色行**全部**落在 subtle/text 令牌上，强调色只在
左边那根药丸里（`SelectionIndicator*Brush`）。照抄 ListBox 那套强调色底色就是把两个控件画成一个控件，所以
`A_selected_row_is_a_subtle_fill_and_not_an_accent_block` 与"药丸像素数落在 20~400 之间"两条把形钉住。
可读到的只有宽 4 与半径 1.5，**药丸长度 16 是我们定的**（上游藏在 C++ 的 `ListViewItemPresenter` 里），
已按自加值记进 §5.2；隔壁 `NavigationView` 的指示条反而是可读 XAML（`NavigationViewSelectionIndicator{Width,Height,Radius}`
三行，`NavigationView_themeresources.xaml:602`），留给 NavigationView 批抄。
(6) **"条宽 40"这条老疑问结清了**：开 overlay 之后 `ScrollBar` 元素报 40x200，看着像条盖住行右侧 40 DIP，
走到子树才看清带填充的是 `Border 'ThumbBorder' 2x40 #8BFFFFFF`——40 是命中区，画出来是 WinUI 那根 2 DIP 细条；
**已发货的 ListBox 宿主读数一字不差**，所以这是底座性质。两个测试类各钉一条，防的就是有人拿"40 太宽"当理由把 overlay 关掉。

产物：`ThemeResources/ListView.jalxaml`（上游 76 行 + 2 flag 里发 **20 行**：8 底色 + 6 字色 + 4 指示器 + 2 半径；
其余 58 条逐名给理由——14 条 x:Double/x:Boolean/枚举本 reader 读不动故以字面量进模板、4 条焦点无承载面、
16 条勾选面无驱动、5 条拖放/占位、15 条上游自己也死的 8.1 世代 `*ThemeBrush`），`Styles/ListViews.jalxaml`
（宿主 + 条目两张模板、两条隐式样式、**七格**触发器，第七格 `IsSelected + IsEnabled=False` 是反向消费闸口当场抓出来的：
`ListViewItemBackgroundSelectedDisabled` 发了却没人读，`Transcribed_control_rows_are_read_by_a_template` 红了一条），
`Themes/Manifest.txt` 两行（字典 38 → 40）、`audits/listview.md`（§0 前提、§1 家族表、§3 七格映射、§5 八条差异、§6 不声称）、
`adaptation/00` 新 **S1-d**（10 条）、`AstraListViewTests`（**23 个方法 / 65 条断言事实**）、
`AstraResourceKeyTests` 消费闸口扩到本字典、Catalog 新增两行（各 4-5 条 gap）、Gallery `selection` 页加一条 ListView
（含一行自加的禁用行）与两句边界说明。

四类证据分开记：构建 = 串行闸口 **859/859 全绿、0 skip、本批 0 新增警告**（18 条落在 Menu/AppBar/ContentDialog 三个
既有文件上），调色板三档 checked=True 且 83 源色 / 101 刷未变；行为 = 宿主与条目的模板对象身份读回、出厂部件名换血
（`PART_OuterBorder`、`PART_ColumnHeadersBorder` 消失，`LayoutRoot`/`ContentBorder`/`BorderBackground`/`SelectionIndicator` 出现）、
padding 与行高换成上游 16,0,12,0 与 40（原生 10,5,10,5 与 30）、`SelectedIndex` 带动容器 `IsSelected`、选中抬起底色与药丸、
`SelectedDisabled` 那格、逐模式多选与"直接写容器会留两行亮"、1000 项只实现一屏、长文本不换行、WrapPanel 排成 4+2、
行隙静置与溢出时同为 0/0、overlay 命中区 40 而 `ThumbBorder` 画 2；视觉 = 4 条（共享行一次覆写同时动两个宿主、
药丸上屏且像素数落在"是药丸不是色块"的区间、框架紫 0 命中、Light↔Dark 顶色不同）；上屏 = `selection,menus,overview`
三页 mount 后优雅关闭、无残留进程，新加的 ListView 在折叠线以下**未目视**；硬件输入 = **仍为零**（任务 13）。

不声称：不声称 `GridView` 已交付（§0）；不声称药丸长度与上游一致；不声称宿主级 disable 会把行压暗——实测
`IsEnabled` 继承到了容器、而那一格仍读 Opacity 1，只有行级 disable 落 0.3，两条按现状各钉一条；不声称 `ListView.View`
的列头可用（我们的模板没有列头面，设了 View 就丢）；不声称悬停 / 按下 / 拖选 / 键鼠导航有真实输入证据；不声称焦点框；
不声称 TreeView / DataGrid / TabView / NavigationView 沾了这批的光。

**阶段 5 第三段（TreeView + TreeViewItem，形状对不上、`Expander` 什么都不钩、`ContentPresenter` 没有 `Foreground`，2026-09-20）**：
列表族走到树上，还是先量后写。`spike/TreeViewProbe`（11 模式：types/mount/retmpl/itemstyle/states/variants/indent/
gutter/depth/gate/fg，原始输出
`adaptation/s1e-treeview-raw.txt`，四趟：量 → 补量 → 复测已发货样式 → 字色通路对照）先推翻一条前提，再量出承重的事：

(1) **WinUI 的 TreeView 与原生 TreeView 根本不是同一种东西**：上游 `DefaultTreeViewStyle` 的模板体只有一个
`controls:TreeViewList`（`TreeView.xaml:26`），而 `MUX_TreeViewItemStyle` 是 `BasedOn DefaultListViewItemStyle`
（`TreeViewItem.xaml:3`），缩进来自 `TreeViewItemTemplateSettings.Indentation`（:131）——**一张展平的行列表**。
这个运行时：`TreeView : ItemsControl : Control`（**不是 Selector**，自有 DP 只有 `SelectedItem/SelectedValue/SelectedValuePath`），
`TreeViewItem : HeaderedItemsControl`（自有 `IsExpanded/IsSelected/IsSelectionActive` + `HasItems`）——一棵嵌套的容器树。
`TreeViewList`/`TreeViewNode`/`TreeViewItemPresenter`/`TreeViewItemTemplateSettings` 均未导出，`SelectionMode`、多选、
拖放、invocation、`Glyph*`、`Content` 逐条打点全是缺。结论是**让嵌套的行穿上 WinUI 那张行皮**，不是起自有类型。
(2) **`Expander` 这个名字什么都不钩**：出厂树里 `ToggleButtons=0`，我们自己往模板里塞一个 `Name="Expander"` 的
ToggleButton、再用自动化 peer 把它 `Toggle()`，`IsExpanded` 一字不变（S1-c 的"按名字挂 attached 属性"在这里也不成立，
因为树根本没有那个 attached 名）。唯一通路是 `IsChecked="{Binding IsExpanded, RelativeSource TemplatedParent, Mode=TwoWay}"`，
两个方向都实测可写——这是继 peer 驱动之后找到的**第二条**通路。
(3) **没有属性承载深度**：把 level-1 行与 level-2 行的全部可读属性、连同整条继承链的 public static DP 逐个差分，
只有 `IsExpanded`/`Header`/布局记账/`Parent` 类型/`PersistId` 不同。出厂模板靠 `PART_IndentSpacer`（L1 宽 0、L2 宽 16、
行 x 不变）画缩进，而那个机制读不出来。我们的缩进只能由嵌套本身给出：子层 `ItemsPresenter Margin="16,0,0,0"`。
代价是**子层底色每深一层窄 16 DIP**（上游的行横贯整个列表），16 是出厂的步进而非自加。
(4) **"容器 `Style` 恒 null"这条不变量在此翻车**：运行时自带 `TreeView`/`TreeViewItem` 隐式样式，`Style` 读回来非 null，
所以本批一律只断言效果、不断言属性来源。并且是**时序**的：容器已经生成之后再并隐式条目样式不会重贴皮，
生成之前则完整生效——我们的字典落在任何容器存在之前。
(5) **`ContentPresenter` 没有 `Foreground` 成员**：这意味着仓库里所有 `Setter TargetName="…" Property="Foreground"`
的字格**从来就是静默失效的**（不报错、字典照常加载、颜色永远不变）。可用通路是 `Style.Triggers` 不带 `TargetName`、
写容器自己的 `Foreground`，生成的 `TextBlock` 继承下去（实测 `item=#FF112233 → text=#FF112233`，逐行独立、兄弟行不动）。
本批照此修正，并把其它族的历史账立成任务 31。
(6) **树有 `IsSelectionActive`（列表没有），仍然没有 `IsPressed`/`IsPointerOver`**，所以按下态照旧近似 `IsMouseCaptureWithin`；
而选择的排他性**会**裁决直接写 `IsSelected`（与 ListView 那批"直接写会留两行亮"相反）。
(7) **上游同样不发布任何 `TreeView*` 宿主行**（宿主样式只设 `IsTabStop`/拖放 flag/转置/Template）⇒ 宿主只发
`Background=Transparent`、`BorderThickness=0`，不借名。于是"一次覆写同时动两个宿主"那条断言在此**反转为
"动 ListBox、必须不动 TreeView"**。药丸几何这次三个数全读得到（`Rectangle Width=3 Height=16 RadiusX=2 RadiusY=2`，
`TreeViewItem.xaml:130`），不像 `ListViewItemPresenter` 藏在 C++ 里——全部照抄，零自加。12 DIP 的左右隙在第三个宿主上
复现（出厂 1/13、我们 1/13、行宽 300），探针里那个 1/1 是探针自己把边框厚度 `TemplateBinding` 出来的，产品样式钉 0。
(8) **一处行高被发货样式自己撑坏**：Fluent `ToggleButton` 自带 `MinHeight=32` 把树行顶到 40，复测已发货样式当场抓到
（ContentBorder 292x**40**，上游 28）→ 命中区钉 `Width=20 Height=20 MinHeight=0`，复测 28、容器 32，正好是上游的算术
（20 内容 + 3 + 5 + 4）。

产物：`ThemeResources/TreeView.jalxaml`（上游 39 行/分支发 **31 行**：8 底色 + 8 描边色 + 8 字色 + 4 指示器色 +
1 条 `BorderThemeThickness` + `PresenterMargin`/`PresenterPadding` 两条度量；按住 8 条并逐名给理由——4 条勾选面无驱动、
1 条多选描边无承载、3 条 x:Double 本 reader 读不动，其中 MinHeight 28 与 ContentHeight 20 以字面量进模板），
`Styles/TreeViews.jalxaml`（宿主 + 条目两张模板、两条隐式样式、
模板内 7 格状态 + 3 格展开/箭头 + `Style.Triggers` 7 格字色，`IsSelected+IsEnabled=False` 那格排最后以赢下同权重）、`Themes/Manifest.txt` 两行
（字典 40 → 42）、`audits/treeview.md`（§0 两条前提与那条被推翻的不变量、§1 家族表、§2 上下游件树与我们的部件映射、
§3 13 视觉态 → 10 格映射（含为何不用 `IsSelectionActive`、SelectedDisabled 的排序契约）、§5 十三条差异与 Known Gaps、§6 不声称）、
`adaptation/00` 新 **S1-e**（11 条）、`AstraTreeViewTests`（**24 个方法：21 Fact + 3 Theory 带 39 组 InlineData，跑出 60 条事实**）、
`AstraResourceKeyTests` 反向消费闸口扩到本字典、Catalog 新增两行（各 4 条 gap）、Gallery `selection` 页加一棵树（两层展开、
一行选中、一根禁用的根）与一句边界说明。

四类证据分开记：构建 = 串行闸口 **920/920 全绿、0 skip、本批 0 新增警告**（18 条落在 Menu/AppBar/ContentDialog 三个既有文件），
调色板三档 checked=True 且 83 源色 / 101 刷未变；行为 = 两张模板对象身份读回、出厂部件名换血（`PART_IndentSpacer`/
`PART_ExpanderBorder`/`PART_ItemsHost` 消失，`ContentBorder`/`SelectionIndicator`/`ChevronHitTarget`/`ChildHost` 出现）、
`Toggle()` 双向驱动 `ChildHost` 可见、折叠仍实现子容器（与出厂一致）、叶隐藏箭头且保留 20 DIP 列、缩进 16/层、行宽 300
两侧等隙、500 项只实现一屏、长标题不换行、逐格换色与选中/禁用组合、选择的排他性与 `SelectedItem` 跟随；视觉 = 4 条
（药丸上屏 20~64 像素、非强调色块、框架紫 0 命中、Light↔Dark 顶色不同）；上屏 = `selection,surfaces,overview` 三页
mount 后优雅关闭、无残留进程，新加的树在折叠线以下**未目视**；硬件输入 = **仍为零**（任务 13）。

不声称：不声称 `IsTabStop` 与上游一致（未抄）；不声称悬停 / 按下 / 键盘导航有真实输入证据（箭头键与 `Enter` 通路本批未量）；
不声称宿主级 disable 会把整棵子树压暗；不声称 `Path` 箭头等于上游的字体 glyph；不声称子层那 16 DIP 缩进与上游像素相同；
不声称 DataGrid / TabView / NavigationView 沾了这批的光。

**阶段 5 第四段（TabView + TabViewItem：把"自有类型"从计划变成三条通路的测量，2026-09-20）**：目标里这一行写的是
"TabView(自有)"，本批没有照抄这个判断，而是先花三遍探针把它变成读数（`spike/TabViewProbe` pass 1 类型普查与模板赋值、
`spike/TabViewTint` pass 2 逐杠杆着色、`spike/TabViewStyle` pass 3 模板到达通路 `tab-style1.txt`），量出的承重结论：

(1) **"能不能重模板"的位置在类型链上**：反射读 `UseTemplateContentManagement` 的声明类型，它是 `ContentControl` 上的
`protected` 成员。`TabItem : HeaderedContentControl : ContentControl` 够得着，`TabControl : Selector : ItemsControl :
Control` 链上根本没有 `ContentControl`，任何子类都够不着。而"够得着"**不是**充分条件：pass 3 让 `TabItem` 的子类在
构造函数里自己开这个开关，部件仍然不在树里、模板根上的 lime 仍然 0 像素，482×36 整块还是它自己的 `OnRender` 画的
（选中时 `#3A3A3C` + 964 像素的 `#1E793F` 指示条）。同一段里 `ListBox` 作阳性对照（样式 setter → 部件全在、54600 像素），
`ContentControl` 子类开同一个开关也建（9640 像素）。**三条通路 + 一个开关全测过**，"原生 tab 族不能重模板"才是读数不是偏好。
(2) **要模板反而更坏**：给 `TabControl` 写上 `Template` 之后它自己的条 measure 成 `StackPanel 0x0`、页签宽 0 却仍在画
（没有 `Template` 时同一宿主量到 `420x36`）——失败模式不是"退回原样"而是"退回一个塌陷的原样"。
(3) **原生那套的行为是好的、外观通路是死的**：`SelectedIndex` 改到 1 之后 `SelectedContent`、可见正文、`IsSelected`
三处都跟（pass 1/2 实测），所以自有类型保的是它的**语义形状**（选中不跟随焦点、`SelectionChanged` 报两端），换掉的只有绘制。
宿主 `FluentTabView : ContentControl`、条目 `FluentTabViewItem : HeaderedContentControl`——后者选这个基类是为了保住上游
"`Header` 在条上、`Content` 在 body"那一刀切分，与 `FluentNavigationView` 走的是同一条被证明能建树的链。
(4) **第三种静默：这里的 `ContentPresenter` 是纯内容宿主**。上游两个按钮样式的模板根正是 `ContentPresenter`，
并在它身上写 `Background`/`BorderBrush`/`BorderThickness`/`CornerRadius`/`VerticalContentAlignment`
（`TabView.xaml:191`、`:242`）——照搬过来被结构闸口逐条点出（"ContentPresenter 'ContentPresenter' has no Background" ×5、
`has no BorderBrush` ×5）。这与 S1-f（格子的 `Foreground`）、S1-g（元素属性）同族：那两次读的是名字存不存在，
这次要记的是**这个类型的 presenter 根本没有画笔层**。笔挪到 `Border ContentRoot`，presenter 只留内容与居中。
(5) **两条会咬人的度量读数**：① 本运行时 `Button` 默认 `MinHeight=36`，样式已写 `Height=24` 仍量到 36，
于是整页签从上游的 32 顶到 44（`Assert.Equal(32d, …)` 读出 44）；补 `MinHeight=0` 后回到 24/32。上游没有这行也不需要——
**"上游没有的行"有时正是这个运行时的坑**。② `FontSize` 写在 `ContentPresenter` 上到不了它生成的 `TextBlock`
（标签量到 19.78 = 14 的字高），挪到条目样式（`Control` 真有这个成员）之后量到 17.24。
另记一条：`AutomationProperties` 不能作为前缀属性写进 `.jalxaml`（闸口原文 "does not export"），名字一律在代码里 `SetName`。
(6) **计数也要被自己数出来的东西打脸一次**：审计 §1 第一版写"发 46 / withhold 17"，本批把上游 `sed` 切成
分支块（68）与字典外度量块（31）、把我们这份数成 64 之后，两边做集合差分才看清真数是**分支 53 + 度量 11 = 64**，
withhold 是 **15 条分支名 + 20 条度量行**（12 条滚动按钮色、2 条 active-tab、1 条拖拽、3 条滚动容器度量、
1 条 `TabViewSelectedItemHeaderPadding`、16 条 `x:Double`），并且自造名 0 个。已按差分改写。
同一趟差分还发现 `ThemeResources/ContentDialog.jalxaml` **从来没进过消费点闸口**（那一批漏接），本批一起接进去。

产物：`ThemeResources/TabView.jalxaml`（64 行全部上游原名，`Light` 与 `Default` 两支逐字节相同故一份覆盖，
High Contrast 整支按住并在 §1 列名）、`Styles/TabView.jalxaml`（4 份样式：`TabViewButtonStyle`、`TabViewCloseButtonStyle`、
宿主与条目两份；上游 9 个 VisualState 组 → 19 条 `Trigger` + 2 条 `MultiTrigger`，映射表在 §3，
含 `HasIcon`/`IsLeftOfSelected`/`IsRightOfSelected`/`TopCornerRadius` 四条代码维护的布尔）、
`Controls/Navigation/{FluentTabView,FluentTabViewItem,FluentTabViewItemCollection,FluentTabViewEventArgs}.cs`、
`Themes/Manifest.txt` 两行（字典 42 → 44）、`audits/tab-view.md`（§0 路线表、§1 计数与 withhold、§2 部件映射、
§3 状态映射、§5 六条偏离、§6 不声称、§7 八条 Known Gaps）、`adaptation/00` 新 **S1-h**（8 条）、
`AstraTabViewTests`（**19 个方法：18 条事实 + 1 条 24 组 InlineData 的反向发布闸口，共 42 条**）、
`AstraResourceKeyTests` 消费闸口扩到本字典（并补上 ContentDialog 那份）、Catalog 新增两行（各 8 / 4 条 gap）、
Gallery `navigation` 页加"Tab view"卡。

四类证据分开记：构建 = 串行闸口（restore→build→test→调色板）**979/979 全绿、0 skip、Debug 构建 0 警告 0 错误**
（951 → 979 多的 28 条就是本批：24 组反向发布闸口 + 2 行新字典进消费点闸口 + 2 条 `SelectedIndex` 顺序事实，
见下面那条更正），调色板三档 checked=True 且 83 源色 / 101 刷未变（本批零改动）；行为 = 两张模板对象身份读回与"原生 `TabControl` 存下 `Template` 而不建"的对照事实、
选中三处联动（线折叠、描边抬起 1 DIP、正文搬进 body）、`SelectionChanged` 报两端、关闭请求点名而不自删、
应用自删后选中落到邻居、+ 按钮"先在那里、可见之后才报"、方向键浏览与**按下-松开才选中**、Ctrl+Tab 只走启用页签、
禁用页签拒绝按下并交出字色、无关按钮时把内缩还给标签、有图标才占那一列、邻居页签的线缩短 2 DIP、
两个按钮的笔落在 `Border` 而不是 presenter；视觉 = 3 条（选中底色以其精确实例上屏 >200 像素、Light↔Dark 不同、
框架绿 `#1E793F` 0 命中）+ 底部线 1 DIP 与 32/24 的几何读数；上屏 = Gallery `navigation` 页挂载后优雅关闭、无残留进程，
新卡片在折叠线以下**未目视**；硬件输入 = **仍为零**（任务 13），且 `KeyEventArgs` 在本运行时造不出来
（`spike/TabViewProbe` section C），键盘通路是按它调用的同一内部方法（`MoveFocus`/`MoveSelection`/`OnCloseRequested`）证明的，
不是按一条真实按键。

不声称：不声称逐像素等于 WinUI（选中页外翻那 4 DIP 的钩在本批是方的，§7 第一条）；不声称高对比；
不声称触摸与键盘端到端；不声称条目有自己的 `AutomationPeer`；不声称 `TabWidthMode`、拖拽、重排、溢出滚动按钮、
`TabStripHeaderTemplate`、`StripPlacement`、`DataPanes`、`ToolTipTitle/Text` 存在；不声称宿主底部线的合并/缩短状态已交。

**这一段的一条更正（冒烟抓出来的真崩溃）**：`Test-AstraGallerySmoke -Page navigation` **第一次跑就把窗口弄死了**，
栈是 `FluentJalium.Controls.FluentTabView.set_SelectedIndex` ← `MainWindow.InitializeComponent()`
（`FluentJalium_Gallery_MainWindow.g.cs:2486`）——`System.ArgumentOutOfRangeException: SelectedIndex must name a tab or be -1`。
根因是本批自加的范围闸不管**顺序**：源生成器先写属性再挂子元素，所以 `SelectedIndex="0"` 落在 `TabItems` 还是空的时刻。
**那棵树上前一遍 977/977 全绿、Debug 构建 0 警告，同一条崩溃一次都没被看见**——这正是"上屏"这一类证据存在的理由，
也是"构建通过不是结论"的一条实测。修法：上界不校验（值留着，页签真的进来时由 `OnItemsChanged` 落地），
只拒 `< -1`；两条新事实**先在崩溃版本上红过**（红的 message 就是上面那条原文），修后 42/42 绿，
冒烟 `navigation,overview,selection` 三页各 6.4~6.8 秒挂载并优雅关闭、无残留进程。审计 §5 第 6 条记了这次判断的改变。
**另记一条测试基座的账**：这一批把整套顺序跑了四遍，四种结果——前两遍（951 那一版树）分别 20 条红
（19 条 "ContentDialog could not resolve a host window" + 1 条 `AstraForegroundRoutingTests`
"the open dropdown never reached the overlay layer"）与 1 条红（"capture never settled:
`#FFFFFFx34848 #FF00FFx17025 #FFB3FFx7357 …`"），后两遍（977 与最终 979）各 0 条红。
原始日志全在 `spike/TabViewStyle/suite-run{1..5}-*.txt`（第 5 遍是最终这棵树，仍 0 红）。
而 `AstraContentDialogTests` 单独跑 38/38、与 `AstraTabViewTests` 配对跑 54/54 都全绿——
**同一棵树三种结果**，因此这是跨类时序 flake，既不是本批引入的交互，也不是可稳定复现的缺陷；
机制未取证（不猜），已立任务在下次复现时先把宿主窗口自己的 `IsVisible`/`IsLoaded` 写进异常消息再决定修法。

**哑格批（68 条状态格子永远不到达像素，2026-09-20）**：S1-e 第 7 条那一枚反射读数往下挖的一批结算——不是新控件，
是把已经"发货"的八张样式的状态格子逐条查一遍有没有写达。先量后改。

(1) **先立结构闸口，让死格无法再发货**：`AstraGateTests.State_cells_name_properties_the_template_parts_actually_have`
解析每个 `ControlTemplate`，把 `Name=` 的**声明元素类型**记成部件表，再拿每条 `<Setter TargetName Property>` 去反射查
owner。首跑 **68 条命中**：`Foreground` 写到 `ContentPresenter` 上 **50** 条（`CheckLabel` 11、`RadioLabel` 7、
`ComboBoxItem` 9、`PART_SelectionPresenter` 5、`ListBoxItem` 6、`ListViewItem` 5、菜单 `IconContent` 6、
`HeaderContentPresenter` 1），`BorderBrush` 写到 `Grid` 上 **18** 条（`CheckRoot`/`RadioRoot`，与上游 `RootGrid` 同形）。
"格子指向模板未声明的部件" **0** 条——名字全对、属性不存在，所以资源键反查闸一路绿灯。
(2) **修法只有一条被量到可用：同一条状态写到控件/容器自己身上**（删 `TargetName`），生成的 `TextBlock` 靠属性继承取到。
发货量：重定向 **61** 条（`Selection` 36 / `Inputs` 14 / `ListBoxes` 6 / `ListViews` 5）、**删除** 6 条（菜单那 6 条是
`Style.Triggers` 活格的重复副本，且 `IconContent` 自己已 `TemplateBinding Foreground`）、**换承载元素** 1 条
（NumberBox 头部 `ContentPresenter` → `ContentControl`，因为只有它有 `Foreground` 成员，而头部必须与编辑区异色，
控件级路线不可用）。
(3) **本地值压过格子，两处据此反向收行**：NumberBox 头部**故意不带** `Foreground=` 属性（带上就是把 disabled 格再锁死），
静止色由盒子继承（同一支笔刷）；`ThemeResources/TextBox.jalxaml` 因此把 `TextControlHeaderForeground` **收回不发布**，
只留 `…Disabled` 与 `TextBoxTopHeaderMargin`。
(4) **框架会在禁用时给生成的文字盖一个本地值**：`box=#FFAEAEB2 boxLocal=False text=#FFAEAEB2 textLocal=True
token=#5C000000`——disabled 标签色不是我们的。四条 disabled 事实因此钉**测量值**并在注释里点名本该生效的行名，
当作"框架哪天不盖章就回到 token"的哨位，而不是四条通过。
(5) **这批唯一肉眼可见的缺陷是 ComboBox 占位符**：可观测性地图量完后（复选/单选/列表行的状态前景除 disabled 全别名同一支
`TextFillColorPrimaryBrush`），只有 `ComboBoxPlaceHolderForeground`(Secondary) 对 `ComboBoxForeground`(Primary)
既不带指针、又不被框架盖章抢走——空框把占位符涂成选中色正是它，选中与清空两侧都进断言。
(6) **18 条 `BorderBrush` 重定向后仍不落地，但这与上游一致**：这些行上游全别名 `SubtleFillColorTransparentBrush`
（`CheckBox_themeresources.xaml:29,205`），可见环是 `CheckSurface`/`RadioRing` 那批 stroke 行，因此本批不声称任何描边像素变化。

产物：`Styles/{Selection,Inputs,ListBoxes,ListViews}.jalxaml` 61 格重定向、`Styles/Menus.jalxaml` 删 6 格并清掉一格随之变空的
`Trigger`、`Styles/TextInput.jalxaml` 头部换 `ContentControl` 承载、`ThemeResources/TextBox.jalxaml` 收回 1 行、
`AstraGateTests` 新闸口（查 100+ 条格子）、`AstraForegroundRoutingTests`（新，**8 条事实**：形状不变量 1 + disabled 归属 4 +
占位符 1 + NumberBox 头 1 + 菜单图标 1）、`AstraComboBoxTests`/`AstraSelectionTests`/`AstraMenuTests`/`AstraNumberBoxTests`
按新形状更新；`adaptation/00` 新 **S1-f**（10 条）；`spike/ForegroundSweep/{census,reroute}.py` 与 14 份读数；
六份审计文档（checkbox-radiobutton / combobox / listbox / listview / menu-flyout / numberbox）各加一节带日期的更正，
Catalog 里 CheckBox / RadioButton / ListBox / ListView / NumberBox / TreeViewItem 六条 gap 文案同步更正。

四类证据分开记：构建 = 串行闸口 **928/928 全绿、0 skip、调色板三档 checked=True、83 源色 / 101 刷未变**，
非增量整解重建 **18 条警告、0 错误**且与基线逐名相同（全在 `AstraMenu`/`AstraAppBar`/`AstraContentDialog`
三个既有文件的 nullability）；行为 = 死格先失败后通过（`routing-before.txt` 8 条里 6 失败 → 修完 0 失败），
中间态 `suite-after.txt` 的 11 条失败逐条是旧测试形状把死格读数写死，最终 928（基线 920：+8 条路由事实、
−1 条随死格一起删掉的 `InlineData`）；上屏 = `selection,inputs,menus,overview` 四页各自 mount 后优雅关闭、
无残留进程；视觉 = **本批 0 条新增像素捕获**，读回值不等于像素；硬件输入 = **仍为零**（任务 13），
hover/press 那一半格子没有一条经真指针验证。收尾两次重跑各抓到一个改完没同步的契约
（`gates-menu-cleanup-fail.txt` 的菜单状态集合、Catalog 副本比对），两条都按新形状修断言而不是回滚改动。

不声称：不声称 61 条重定向里除占位符与菜单图标之外的每一条都改变了像素（多数状态行别名同一支 Primary，读回本来相同）；
不声称 disabled 标签色归我们（框架盖章）；不声称 NumberBox 头部的 disabled 色到达像素（止于承载元素，差一跳）；
不声称模板**属性**也干净（闸口只查格子，属性那一层另立任务 32）；不声称悬停 / 按下通路有效（无输入证据）；
不声称这批之后 Styles 里再没有静默失效——只再没有**这一类**。

**属性死写批（35 条属性写到没有该成员的要素上，2026-09-20）**：上一批末尾按住的那条欠账（"模板**属性**也干净"）
这次结掉，同一套做法往下走一层：普查每个元素的**声明类型**是否真有该成员。先量后改。

(1) **结构闸口先立**：`AstraGateTests.Template_attributes_name_members_the_element_type_actually_has`
把每个可解析元素的直接属性与带前缀的附加属性都反射查一遍。首跑普查 **2530 个元素 / 5263 条属性**、
命中 **35 条**（附加属性 **0**），发货后复测 2533 个元素、直接 0 / 附加 0。命中的形状就是那几个"看着像成员"的名字：
`ContentPresenter` 的 `Foreground` 15 / `TextWrapping` 2 / `*ContentAlignment` 2 / `Background` 1，
`Grid` 的 `BorderBrush` 4 / `BorderThickness` 4 / `CornerRadius` 4 / `Padding` 2，`StackPanel` 的 `Padding` 1。
同一元素上的 `Background` / `Margin` 是活的，所以布局看起来一切正常——构建、字典加载、资源键反查闸三样全不响。
(2) **四处真正看得见的后果，这批不是为了清洁**：NumberBox Spinner 弹层（半径 + 描边三条落在 Grid 上 → 直角无边框，
承载换 Border、名字留在画者上）；InfoBar 两条 padding（内容根 Grid 与 `Panel` StackPanel 都没有 `Padding` 成员 →
内容贴死表面，`InfoBarContentRootPadding` 挪到 `RootBorder` 且 `MinHeight=48` 跟过去、
`…PanelVerticalOrientationPadding` 由新增的 `PanelSurface` 承载）；TeachingTip 卡片（三条边框/半径落在
`ContentRootGrid` 上 → 有底色、直角、无边框，画者换成第一个孩子 `ContentRootSurface`）；
MenuBarItem 根 Grid 与圆角 Border 绑同一支笔刷（**静置与悬停都看不见**，因为静止行是
`SubtleFillColorTransparentBrush`；要应用自设 `Background` 才溢出，见 (4)）。
(3) **删除之前先量"继承有没有已经代偿"**：15 条 `Foreground` 与 2 条 `TextWrapping` 能删，依据是 S1-f 量到的
控件级前景 + 生成文字继承，以及 `Styles/Navigation.jalxaml` 里那条更早的测量（本运行时生成文字默认 `Wrap`）。
留一条事实钉住"行为没变"（复选 / 单选标签删属性后仍 `Wrap`）。Expander 的两条对齐改绑 presenter 自身的
`HorizontalAlignment` / `VerticalAlignment`——那是复选、单选模板一直在用的路线，不是新发明。
(4) **读数本身也会静默**：修 MenuBarItem 那条的第一版断言写的是 `root.GetValue(Control.BackgroundProperty)`，
而 `Grid` 不是 `Control`，那格 `DependencyProperty` 与它自己声明的 `Background` 不是同一个对象——
元素带着 `#00FFFFFF` 时读数仍 `null`，是一条永远为真的断言。两行并排放一次就分出来，改成读 Grid 自己的那格后
**在修复前跑红、修复后跑绿**（`spike/AttributeSweep/menuitem-fill-first.txt`）。
按 `GetValue(<类型>.<X>Property)` 扫全测试工程又抓到同一处错的兄弟一条（`AstraTeachingTipTests` 卡片事实里
"布局格不再声明 `Background`"那行，按同一条机制它在坏标记上也是绿的），一并改读元素自己声明的那格。
立此一条通用要求：修静默失效的断言，必须先在缺陷还在的时候红一次。
(5) **闸口与普查覆盖面不同，数字留档**：闸口的类型宇宙是两个程序集里的 `DependencyObject` public 子类
（本次读回 1972 条属性），普查多带 `Jalium.UI.Core` / `Media` 所以走到 5240 条；哪天两边背离先看这两个计数。
`PixelHarness.Named` 只看子树，弹层表面的名字落在 `Popup.Child` 自己身上，因此那条事实直接读 child 并断言其 `Name`
（同一陷阱 `AstraTeachingTipTests` 早写过，这次重新踩）。

产物：`Styles/{Common,Inputs,Menus,Navigation,Selection,Surfaces,TeachingTip,TextInput}.jalxaml` 属性与承载修正；
`AstraGateTests` 新闸口（带 1500 条 vacuity 下限）、`AstraSurfaceGeometryTests`（新，**6 条事实**）、
`AstraTeachingTipTests` 部件清单补 `ContentRootSurface` + 1 条卡片事实；`adaptation/00` 新 **S1-g**（9 条）；
`spike/AttributeSweep/{Program.cs,strip.py}` 与 6 份读数；六份审计文档（numberbox / infobar / expander /
teachingtip / menu-flyout / checkbox-radiobutton）各加一节带日期的更正，Catalog 里 NumberBox / Expander /
InfoBar / TeachingTip / MenuBarItem 五条 gap 文案同步。

四类证据分开记：构建 = 串行闸口 **935/935 全绿、0 skip**、调色板两档 `checked=True`、83 源色 / 101 刷未变
（`gates-2.txt`；基线 928 → 935 是本批 +6 条表面事实与 +1 条卡片事实）；行为 = 死属性先普查（35）后复测（0），
四处表面几何按属性读回钉住，MenuBarItem 那条额外留下"修复前红 / 修复后绿"两份读数；
视觉 = **本批 0 条新增像素捕获**，弹层只打开到属性层、卡片四角未裁帧；上屏 = Gallery 冒烟单独记；
硬件输入 = **仍为零**（任务 13）。

不声称：不声称那四处表面几何在像素上变了（全部只到属性读回）；不声称 MenuBarItem 的方角溢出肉眼可见
（静止行透明，未测过应用自设填色的那一帧）；不声称属性名合法但资源名不存在的写法被治住（那是资源键闸口的地盘）；
不声称 `Style` / `Setter` 等标记类型上的属性名拼错会被查到（闸口的类型过滤把它们排除在外）；
不声称真指针 / 触摸路径——一条都没有；不声称 Styles 里再没有静默失效，只再没有**这一类**。

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
- 不声称弹层圆角的**绝对尺寸**有像素证据：第十二段那几帧只证明"半径改了角就变了"（弧深 8→9~10 行、
  14→17 行、0→弧消失），而弧的量在 dpi 168 下比 `半径×1.75` 短同一个系数（8 DIP 应 14 px、量到 9~10）。
  该系数**未解释、未定性**：可能是渲染器的弧本身短，也可能是"第一个非底色列"这条测法吃掉了边界像素。
  同一次复测还量到**顶行咬入这一格自己就有 3 px 抖动**（同一模式重采 12 → 9，弹窗落点差 0.67 DIP），
  所以它已经退出论据、只留数。要分清需要跨 DPI 或跨已知半径的对照，本批没有，故只声称比值不声称绝对值；
  半径数值一律以属性读回为准。这条限制适用于本项目**每一个**圆角，不只是菜单。
- 不声称 `MenuFlyout` 的卡片色可换：它的表面 `MenuFlyoutPresenter` 是 `internal sealed`、`Background` 读回 `null`、
  弹层在自己顶层 `PopupWindow` 里，帧上那颗 `#FF2C2C2E` 灰由框架给；我们发布的 `MenuFlyoutPresenter*` 三行
  只在 `ContextMenu` 那层复制 `Border` 上兑现（`Menu` 子菜单同一条通路）。
- 不声称 Gallery 页脚的度量/排布宽度差已治好：本批只把 parity 条改成单元素 + 固定高度滚动宿主绕开它，
  框架侧"换行文本按更宽尺寸量、按实际宽度排"的行为仍在（`spike/TextWrapProbe` case E）。
- 不声称间距已逐控件与上游量化：目视逐页看过，把 padding/margin/MinHeight 对到上游数值并配断言的只有
  InfoBar（`InfoBarContentRootPadding` 等 7 条 Thickness）、Menu 条目（38 的 `MinHeight` 字面量）、命令栏条目
  （64/48 内容带、2,6,2,6 高亮内缩、0,16,0,2 图标槽、2,8,2,8 分隔线）、`ComboBoxItem`（5,2,5,2 行内缩）与
  `Slider`（18×18 拇指、12 内芯、4 刻度间隙、14/Auto/14 三段带），其余仍只到"转录时用的是上游度量行原值"
  这一层，没有逐控件的截图差分；`Slider` 的环在最大值处超出控件 2 DIP 是框架给拇指留 16 的结果，未治。

- 不声称本层存在"能随主题切换的数值令牌"：`x:Double` 行让整份字典解析失败；`clr-namespace:System` 的
  `<sys:Double>` / `<sys:Int32>` / `<sys:TimeSpan>` 能解析、CLR 类型也对、**值恒为该类型的默认值**
  （文件里写 4、字典里读回 0）；`sys:String` 的 `"4"` 喂给 `MinHeight`/`Padding` 同样不做转换。
  同一次实验里 `Thickness` 行是正常对照组（`Padding=4,4,4,4`），按主题切换只对 String / Thickness / CornerRadius
  成立（`adaptation/00` §S1-r 第 1、2 条，原始读数 `adaptation/s1r-double-row-raw.txt`）。
  因此上游那些分主题的度量一律写成字面量，并且每个省略掉的行配一条反向 theory，钉的是"这一行发不出去"，
  不是"我们选择不发"。
- 不声称高对比度 parity：公开管线无驱动入口，只有上游逐键映射 + 一个自加键的自行判断（`SliderThumbStrokeBrush`
  已在 Slider 批回收成上游真名 `SliderThumbBorderBrush`，`ToolbarSurfaceBrush` 更早因无消费点删除），
  且逐控件的 HC 视觉状态覆盖未移植（见 `adaptation/04`）。
- 不声称 `Application.ThemeMode` 稳定：它是 `[Experimental("WPF0001")]`，框架明说将来可能改或删；
  我们钉在 26.10.9 并有运行时断言，删除即编译失败（单文件），行为退化即测试失败。
- 不声称跟随系统"减弱动画"设置：Windows 侧无 API。
- 不声称逐位一致的上屏合成：RTB 离屏与上屏一致性未证。
- `Symbol` 的 764 个成员已逐个查过 cmap（`spike/SymbolCmap/sweep.py`，原始读数 `adaptation/s2-symbol-cmap-raw.txt`）：
  **729** 个不同码点、**35** 处两个名字共用同一码点（`Settings`/`Setting`、`Find`/`Search`、`FavoriteStar`/`Favorite`…）、
  **0** 个落在 PUA 之外；Segoe Fluent Icons 命中 **762/764**（缺 `AlarmClock` U+E919、`ScreenCapture` U+E7A0），
  Segoe MDL2 Assets 命中 **755/764**（另 7 个只在这条字体上缺）。
  仍不声称两件事：这份表读的是兄弟源码树 `Jalium.UI.Controls/Symbol.cs`，**与钉住的 26.10.9 程序集是否逐字一致还没对账**
  （成员数 764 与普查表记的相同，成员集合未 diff）；"码点在 cmap 里"只证明字形存在，
  不证明形状是 Fluent 风格，也不证明它进得了本基座的任何捕获通路（§S1-r 第 3 条 / #50）。
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
  **阶段 6 第四段把这条扩到另一条通路**：整窗合成的 `Host()` 同样拿不到字形墨——最朴素的白字 `TextBlock`
  压在灰底面板上，一张图里 `#FFFFFF` 计数 **0**，同一张图加不加这行文字只差布局挪动（6136/279 对 6118/288），
  所以这不是离屏 `Render` 路径的产物，而是本基座**两条**捕获通路共同的限制（`adaptation/00` §S1-r 第 3 条、
  `audits/info-badge.md` §5）。凡像素列写过"文字、图标、数字印出来了"的批次都按这条收缩：PipsPager、TabView、
  菜单文本、InfoBadge 的数字格，能主张的只剩色块与颜色身份。
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
  【第七段已结】NavigationView 的九步出口已补 `audits/navigation.md`；那条"欠审计"的账在此了结，
  而它顺带量出侧栏条目圆角自造名 `NavigationViewItemCornerRadius`=4 上游根本没有，上游左栏用
  `OverlayCornerRadius`=8（见下面第七段）。
- 一条**已撤回的改动**：把 83ms `TransitionProperty` 从 Button/导航扩散到 CheckBox/RadioButton/
  ComboBox/ComboBoxItem/TextBox/BottomStroke（6 处）。实测过渡在飞期间 `Background` 是**新造的插值实例**
  （`sc#0.66,0,0.12,0.43`），直接顶穿别名层"处处同一实例"的不变量，而 WinUI 这些状态填充本就是
  `KeyTime="0"` 离散换刷。三个样式字典已回退到 HEAD，全套 67/67 复绿。既有三处（Button/NavigationView/
  ToggleSwitch）不动，但要知道它们的画刷实例同样只在稳态等于调色板实例。

**阶段 5 第五段（DataGrid：表格族没有 WinUI 上游，而"能不能重模板"要按层回答，2026-09-20）**：
上一段把账量齐（`379c44e` 那份测量：框架自带一套表格主题、11 个 token 名、两条品牌绿缺陷、部件契约），这一段落地。
落地之后表格**一个字都没显示**——于是这一段真正的产出不是样式，是一条契约。

(1) **先死一次再说为什么**：宿主+行+单元格+两种表头全装上，绑定单元格的 `ContentPresenter` 报
`content=TextBlock`、`vis=Visible`、`desired=0,0`，视觉树里根本没有那个 `TextBlock`；同一棵树不加载 Astra
字典时段落是 `47.45x19.78`。逐块撤样式二分（一格一次构建一次读数）：只装**单元格**样式→正常；
只装**行**样式→死；全撤只留 token→正常。结论 **叶子能换模板，装单元格的容器行不能换**——
行的模板被换等于重建内容 visual 所在的子树，26.10.9 的 `ContentPresenter` 没有"模板拆除时归还托管 visual"
那一步（参考树里后来长出 `ReleaseContentElementForTemplateTeardown`，注释逐字就是这个卡死）。
证据边界写进文档：私有字段没读到，只有"撤了好、装了（只在行）就坏"的行为一致。见 `adaptation/00` S1-i 9、
`audits/datagrid.md` §5，两份读数 `s1i-datagrid-cells-raw.txt` / `s1i-datagrid-content-raw.txt`。
所以 `Styles/DataGrid.jalxaml` 只发 **4 份样式**（宿主/单元格/列头/行头，各有隐式样式），
**不发 `DataGridRow` 样式**，`The_row_keeps_the_frameworks_template` 把这条钉住（名字查不到 + 行里没有我们独有的部件）。

(2) **行不能换，行读的名字就得改**：行选中底是框架模板的 `{ThemeResource AccentBrush}`，而那在这个运行时是
品牌绿渐变——本仓硬闸口"品牌绿不出现"过不去。新增 `ThemeResources/FrameworkRetints.jalxaml`（清单 46→47 份），
把 `AccentBrush` **别名**（不是重定义）到 `AccentFillColorDefaultBrush`：同一实例，`ApplyAccent`/`OverrideBrush`
推得动。作用域实测：框架字典 12 份共 58 处读这个名字，今天全铺品牌绿。这条推翻了上一段 §3
"绝不动框架 token"的决定，原文留着记账。**代价**：上游 `ListAccentLowOpacity` 0.4 那层半透明拿不到
（x:Double 发不出去、行模板又装不上），选中行按强调色不透明铺满——写进 Known Gaps，不粉饰。

(3) **跨批作用域立刻抓到一次**：整套顺序跑第一次红了 2 条。除了目录差集（本批新增 4 个隐式 TargetType，
Catalog 46→50 行），另一条是**弹层角批**的 `A_suggestion_row_pays_our_token_instead_of_the_frameworks_accent_gradient`：
它断言框架写在生成 `ComboBoxItem` 上的**本地值是渐变**，而 `AccentBrush` 一别名，那个派生刷变成了
`SolidColorBrush`。这是**好处**（框架那支品牌绿渐变少了一处），但那条事实的前提不再成立，已按"所有权 + 类型"
两半重写（所有权是约束，类型是"以后谁再把渐变变回来必须解释"）。改完定向跑 100/100。

(4) **四类证据分开记**。行为/结构：`AstraDataGridTests` **54 条**（21 条别名行 + 4 条度量行 + 17 条
"没有消费者就不发布"的反向闸口 + 部件契约/几何/交替线/禁用优先序/两主题重绘），其中
`A_bound_cell_renders_its_text` 是本批的牙齿——它读的是生成 `TextBlock` 的**实际尺寸与在树与否**，
不是 `Content` 是否非空。像素：表面哨兵色 >2 000 像素、Light↔Dark 顶部颜色不同、选中网格
`#1D733C`/`#2B804A`/品牌绿各 **0** 像素、强调色换成哨兵色后选中行 >200 像素。视觉/Gallery：
`Catalog.json` 加 4 行（`DataGrid`/`DataGridCell`/`DataGridColumnHeader`/`DataGridRowHeader`，全 audited，
gaps 逐条写明"本族没有 WinUI 上游""行保留框架模板的原因""排序字形由代码写本地值"），
`selection` 页挂上真表格，`tools/Test-AstraGallerySmoke.ps1 -Page selection` 9 秒干净关窗。
构建：串行闸口 `tools/Test-AstraGates.ps1` 四步全绿——restore 全部最新；Debug 构建 **0 警告 0 错误**；
整套顺序跑 **1034/1034 通过、0 失败、0 跳过**（5 m 25 s，含本批新增 54 条与被本批改写的弹层角批那条）；
调色板漂移 Light 83 源色 / 101 刷、Dark 同、HighContrast 101 映射键，三行 `checked=True`。
不声称：列宽拖拽/重排/排序点击/单元格提交没有任何真输入证据；`TreeDataGrid` 没有落地（节点喂法是下一段）；
行头那四枚 gripper Thumb 与十七格状态矩阵没做；高对比下表格走哪条路未测；强调色 0.4 未拿到。
【同段更正】其中"`TreeDataGrid` 没有落地"已被第六段结掉，且它给的理由（节点类型 internal ⇒ 喂法未量）被推翻，
见下面第六段与 `audits/treedatagrid.md` §1；"行头没做 gripper/状态矩阵"仍成立，但行头**画模型 `ToString()`**
这条已上线的缺陷也在第六段修掉了。

## 阶段 5 第六段：TreeDataGrid 落地，顺手挖出表格族两条静默 markup 账（2026-09-20）

上一段把 `TreeDataGrid` 挡在库外的只有一句话："`TreeDataGridNode` 是 internal，层级数据怎么喂进去尚未量"。
对着 26.10.9 公版程序集把成员表拉出来，这句话的推论断了（`audits/treedatagrid.md` §1、`adaptation/00` S1-j）：

- 喂法是 `ItemsSource`（普通 `IEnumerable`）+ `ChildrenPropertyPath`（字符串）+ `TreeColumnIndex`（树列是索引不是列类型）；
  节点类型确实 internal、`TreeDataGridRow` 只有 `IsSelected` 公开，但**消费者一辈子不点名它**。
- 展开闭环全公开：`ExpandAll()` / `CollapseAll()` / `IsExpanded(int)` / `FlattenedCount` +
  `NodeExpanding/NodeExpanded/NodeCollapsed`。实测 2 根 → 展开 6 行（落地文字 6→14 条）→ 收回 2 行，
  不需要一次 OS 输入，也不需要反射进私有字段。
- 缩进 `IndentSize` 默认就是 16，与上游 TreeView 步长一致，父子文字 X 位移有断言。

本批交付：`Styles/TreeDataGrid.jalxaml` 一份宿主隐式样式（部件契约按实测七件套写，`PART_RowHeaderCorner` 这种
树没有的名字不塞），**不新造任何资源键**——运行时没有 tree-only 的 cell/列头/行头，DataGrid 族那三条隐式样式
直接接管这棵树，用模板实例同一性 + 32 DIP 下限证明。行保留框架模板：S1-i 那条"能不能换模板要逐层判"在第二个
控件上复现，而且这里更硬——每条行的 `Background` 都是控件写上去的本地值（实测偶 `#00FFFFFF`、奇 `#0FFFFFFF`），
样式格子在那层本来就赢不了。

挖出来的两条静默 markup 账（`adaptation/00` S1-j，七种配置在 `s1j-treedatagrid-columns-raw.txt`）：

1. `DataGridColumn.Width` 是 `DataGridLength`，**markup 里的裸数字不转换、静默停在 `Auto`**，不报错、构建绿；
   `MinWidth`/`MaxWidth` 是普通 `double` 才写得进。后果分家：`DataGrid` 把 `Auto` 摊成实宽（120），
   `TreeDataGrid` 把 `Auto` 量成 **0** —— 单元格 `ActualWidth=0`，文字有 `desired` 没宽度，整棵树看不见字。
   **Gallery 的表格从上线第一天起三条列宽全是 `Auto`**，本批全改 `MinWidth` 并在两侧各钉一条断言。
2. 未知属性名同样静默：`DefinitelyNotAProperty='42'` 解析通过并上屏；`TreeDataGrid` 根本没有
   `AutoGenerateColumns`（也不带滚动条可见性属性，宿主模板那两处只能写常量），Gallery 里那句一直是空写，已删。
   另有一条：列宽必须在首次度量之前定，挂载后改 `Width` 列自己读到 200、单元格仍是旧布局那一版。

顺手结掉一条已上线的可见缺陷：这条运行时会把**行数据本身**塞进 `DataGridRowHeader.Content`
（完整树路径在探测日志里），而我们上一批的行头模板带内容呈现器，于是 20 DIP 的行头槽里画的是模型 `ToString()`。
修法是行头**不呈现内容**（上游 WPF Fluent 的行头也只画"当前行"记号，而本运行时不暴露那个读数），
`DataGridRowHeaderForeground` 随之失去消费者、撤出别名表并进"不发布"闸口，Gallery 表格另加 `HeadersVisibility='Column'`。
这是本仓第二次"上一批的模板形状在下一批被量成缺陷"——上一次是弹层角批把建议列表的渐变底判成缺陷。

四类证据分开记：
- 行为/结构：`AstraTreeDataGridTests` 11 条（喂法/展开闭环/缩进/部件契约七件套/与 DataGrid 共享样式/行本地填充/
  交替实例/表面刷与半径/Auto-vs-MinWidth 两组/行高列头高落部件/两主题重绘），`AstraDataGridTests` 加 2 条（行头不出文字、markup 列宽静默）。
  写的时候自己也踩了一次：两条树测试一开始用探测数据里的旧节点名断言，红了才改正——红的是断言，不是产品。
- 像素：选中树行 `#1D733C`/`#2B804A`/品牌绿各 0 像素（并扫裁剪区内所有渐变持有者）、Light↔Dark 顶行颜色不同。
- 视觉/Gallery：`Catalog.json` 50→51 行（`TreeDataGrid`，parity `audited`，8 条 gaps 逐条写明"没有 WinUI 上游"
  "行本地填充""Auto=0""未知属性静默"），`selection` 页挂上真的两层树，`Test-AstraGallerySmoke.ps1 -Page selection`
  10.8 秒干净关窗。
- 构建：串行闸口 `tools/Test-AstraGates.ps1` 在**最终这棵树上**四步全绿——restore 全部最新；Debug 构建
  **0 警告 0 错误**；整套顺序跑 **1047/1047 通过、0 失败、0 跳过**（5 m 23 s，比上段 1034 多 13 条 =
  本段树表 11 + 表格 2）；调色板漂移 Light 83 源色 / 101 刷、Dark 同、HighContrast 101 映射键，
  三行 `checked=True`。记账顺序如实写下来：第一条闸口跑（1046/1046，7 m 3 s）之后又补了
  `The_row_and_header_heights_we_set_are_the_heights_the_parts_get`（行高要落到生成的行上，不只是样式装在宿主上），
  于是整条串行闸口重跑了一次，提交的就是重跑验证过的那棵树。

不声称：chevron 的命中区、hover、键盘 `Alt+Right/Left`、触摸展开**零真输入证据**（展开只证明公开调用还活着）；
列拖拽/重排/排序/编辑提交同样零证据；行状态矩阵（hover/selected/focus 成套 VisualState）一格没写；
虚拟化下的行回收没测；高对比未测；缩进只量到"子在父右"，没量到 DIP 精度；
markup 属性名/属性类型这一层闸口仍缺（`AstraGateTests` 查不到）。

## 阶段 5 第七段：NavigationView 补样式——结清一条自造名，顺手把侧栏圆角改回上游的 8（2026-09-20）

这一段的入口是目标里那句"NavigationView 补样式"，落到实物上是一条**上线即错**的视觉账：
库里发布的 `NavigationViewItemCornerRadius`=4。逐名查上游（blob `aa3ff11b2`）量到三件事——

1. **这个名字在上游不存在**：`git grep NavigationViewItemCornerRadius 19e3bdc3c` 整个仓库 0 命中；
2. **上游左栏用的是 `OverlayCornerRadius`**（:447，`MUX_NavigationViewItemPresenterStyleWhenOnLeftPane`），
   也就是 8；方法权威 ModernWpf 同一条读法（`NavigationView.xaml:176`）；
3. 我们那行的值 4 **恰好等于 `ControlCornerRadius`**——不是"另一个上游键"，而是"ControlCornerRadius 的值
   穿了个导航的名字"。判据写成三条断言：条目与模板 `Root` 都等于 `OverlayCornerRadius`、
   且**不等于** `ControlCornerRadius`（这条会抓住哪天滑回 4）、被撤的名字由测试钉成"不得再发布"。

键层那边是本批的主体。上游 64 条别名行里落地 19 条、按名撤 45 条并逐组给理由
（`TopNavigationViewItem*` 13 条＝没有顶部条；条目 `BorderBrush*` 12 条＝上游左栏根是 `Grid`，那块描边
在上游自己也不画；`*Checked*` 8 条＝我们的条目只有 `IsSelected`；`NavigationViewButton*` 6 条＝那六行的
真消费者是 `NavigationBackButton.xaml:26-52` 而本控件没有返回按钮；pane 背衬 3 条＝材质未摸底；
`Separator`/`Header`/`IconBackground` 3 条＝没有那些面）。45 条**逐名**进"不得发布"闸口，不是只记一个数。
Light 与 Default 两支**同名且同目标**（把 (name→target) 做成排序多重集比出来 `diffs=[]`），所以一份与主题
无关的别名层就是忠实转录；高对比靠 `HighContrast.map` 在别名目标的下面重映射，不必抄第二块。
另有 20 条 `x:Double` 度量行（含上游 220-222 那三条指示条键，上游 602 行真的在消费它们）在本 reader 上
是"一行毁整份字典"，只能以字面量进模板——值仍与上游逐一对上（36/40/16/48/3/2），且指示条那个 16
被钉在代码侧的 `NavigationIndicatorAnimator.RestingHeight` 上：同一个数有两处，就必须有断言把两处绑住。

四类证据分开记：
- 构建：串行闸口 `tools/Test-AstraGates.ps1` 在本批最终树上跑（读数见本节末"闸口读数"）。
- 行为/结构：`AstraNavigationTests` 4→78 例（+74）：19+1 条键逐名已发布、45+5 条逐名不得发布、
  别名**同一实例** 8 条 `Assert.Same`、半径读回、`NavigationViewItemButtonMargin` 落到 `item.Margin`、
  指示条几何与动画器常数绑定、一条别名行跨主题变色（Light↔Dark 不同色，证明变的是目标不是行）。
- 像素：选中 pill 在同一色键上的增量 8 220（整类跑）/ 7 993（单跑），未选中那张图这个键恒 0；
  品牌绿 `#207245` 0 像素；指示条 1×8 那个点采到的就是强调色刷的 RGB。
  两条 harness 边界（`Host()` 与 `Build()` 父级互斥；第一次 `Host()` 与后续不同源，故只能比单色键增量）
  已写进 `adaptation/06`，原始读数归档在 `adaptation/s1k-navigation-raw.txt`。
- 视觉/Gallery：`Catalog.json` 里 `FluentNavigationView`/`FluentNavigationItem` 两条从 `own-type` 改判 `audited`
  （legend 对 `own-type` 的措辞是"Jalium 没有这个原生类型"，而实测**有**、只是没有样式，见下），
  证据补 `audits/navigation.md` 与 raw 日志，gaps 按 §2/§6 重写；`Test-AstraGallerySmoke.ps1 -Page navigation`
  5.8 秒干净关窗（Gallery 侧栏就是这个控件本身，所以冒烟顺带盖住了半径改动后的真实上屏路径）。

一处**改判**要留在账上：`Catalog` 的 `own-type` 标签一直暗示"原生没有 NavigationView 才自建"。普查读数其实是
`Jalium.UI.Controls.NavigationView : ContentControl`，16 个自声明属性，`style=False render=False parts=0`、
`templateLock=Void:null-or-void`（`adaptation/01:92-95`、`s0y-outstanding-names.txt:16`）——**类型在，样式与部件树不在**。
所以"换原生重模板"省不下任何事，反倒要重新量 16 个属性的契约；本批继续用自有类型，但把这个判断连它的读数
一起写进 `audits/navigation.md` §9，而不是让它住在标签里。

不声称：hover/press 只有触发器在位的结构证据，**零真指针输入帧**；8 DIP 圆角只读回到属性，角上是否真把
填充留出去没采；条目前景的状态变化是否走到 `SymbolIcon` 字形没读；pane 背衬是纯色，上游那一层是亚克力
（材质摸底未做）；高对比一次没量；指示条滑动动画无逐帧证据；条目 `StackPanel` 无虚拟化。

**闸口读数（第七段，串行 `tools/Test-AstraGates.ps1`，exit 0）**：restore 全部最新；Debug 构建
**24 条警告 / 0 错误**；整套顺序跑 **1120/1120 通过、0 失败、0 跳过**（5 m 27 s；比上段 1047 多 73 条
＝导航类 6→78 的 +72，加资源键闸口那条字典行的 +1）；调色板漂移 Light 83 源色 / 101 刷、Dark 同、
HighContrast 101 映射键，三行 `checked=True`；`All Astra gates passed.`

那条 **24 条警告**要单独记账，因为它推翻了本文件前面几段的一个读数口径：第 5、6 段写的"Debug 构建 0 警告 0 错误"
是**增量构建没重编测试工程**的结果——同一份日志这次真编了，xUnit 分析器与可空性警告就出来了。分布：
`AstraAppBarTests.cs` CS8604 16 条、`AstraContentDialogTests.cs` CS8601 6 + CS8605 2 + CS8602 2 条、
`AstraMenuTests.cs` CS8604/8602/8600 共 8 条、`AstraTreeDataGridTests.cs` CS8604 4 条、
`AstraDataGridTests.cs` CS8604/8602/8600 共 6 条（后两个文件是本仓前两段自己写的，那 10 条是我们的账），
`AstraNavigationTests.cs` **0 条**。清警批与"警告数必须先确认它真编译过"这条读数纪律一起记进任务 #41。

**阶段 5 第八段（RadioButtons：撤掉一条挡住自造条目宿主的旧纪律，2026-09-20）**：

- 开工前先量底座，而不是先写控件。`adaptation/09` 从"裸 `ItemsControl` 上屏后 UI 线程 60 秒不回应"
  读出机制＝"`Template` 为 null ⇒ 框架走进不产出帧的路径"，并立了纪律"自造条目宿主不要继承 ItemsControl"——
  这条纪律一直在把 `BreadcrumbBar`/`RadioButtons`/`PipsPager` 往"手写命名面板塞子元素"上推。
  `spike/ItemHostProbe` 三处读数把它拆开：同形状挂载 **4 帧返回**、无模板也有回退条目宿主
  （`UsesFallbackItemsHost`/`ItemsHostInternal` 在属性面里读得到，`ContentPresenter×3` 真在树里）、
  `RenderTargetBitmap.Render` 计时 0.03s/0.00s 两侧都不是成本。剩下的解释是 `PixelHarness.cs:259-268`
  自己记着的看门狗缺陷（静态场景不发 `Rendering`，释放被排到没人泵的线程池 dispatcher 上）。
  **那是 harness 的 bug，不是 `ItemsControl` 的性质**；纪律撤销，原文与读数在 `adaptation/09`，
  转录在 `adaptation/s1l-itemhost-raw.txt`，能力行记进 `adaptation/00` S1-l。
- 量出来的正面通路：`GetContainerForItemOverride`/`IsItemItsOwnContainerOverride`/
  `PrepareContainerForItemOverride` 等在 26.10.9 里是 **protected/virtual**（签名同 WPF），
  我们的模板（`Border > ItemsPresenter`）本地值与 `Style` 的 `Template` setter 两条路都跑得通，
  `ItemsPanel` 赋值换面板有效（`WrapPanel` 按内容宽换行、`UniformGrid Columns=4` 每格正好 105x80）。
  `FluentRadioButtons : ItemsControl` 因此是本库**第一个**继承 ItemsControl 的类型。
- 上游与选型：`RadioButtons : Control`（`RadioButtons.idl:9`）不是 ItemsControl，条目由 repeater 的
  element factory 造真 `RadioButton`（`RadioButtonsElementFactory.cpp:49-87`）——本层用 ItemsControl 的
  容器覆写走同一条机制，不是"退而求其次"。整个键面只有 5 行：2 条前景别名（Light 与 Default 逐行相同，
  HC 两条重指 `SystemControl*`）+ 3 条度量，其中只有 `RadioButtonsTopHeaderMargin` 是 Thickness 能发布，
  两条 x:Double 间距落到面板属性默认值。全表与逐条去处见 `audits/radio-buttons.md` §1-§2。
- 一处**必须由断言撑着**的推断：条目面板要拿到 `MaxColumns`，上游靠模板绑定，本层
  `ItemsPanelTemplate` 只携带 `PanelType`——`FrameworkElementFactory` 那个构造器**只抄 `root.Type`、
  丢掉全部 SetValue**（源码 `DataTemplateSelector.cs`），所以列数只能由面板自己找宿主。
  第一版只读 `TemplatedParent`，实测条目全挤在 X=100 一列（静默退化，除位置断言外谁也抓不到），
  改走公开的 `Parent` 链（框架自己在 `ItemsPresenter.cs:168` 也是这么走的）才通过。
  `MaxColumns_reaches_the_layout_and_fills_the_first_column_before_the_second` 用
  `TranslatePoint` 读回位置钉死两件事：确实分了三列，且是先填满第一列。
- 另一条读数纪律的实例：探针里生成的 `RadioButton` 容器 `Style` 读回 **null**，但树里
  `#RadioRing/#RadioDot/#RadioLabel` 全是我们阶段 3 的部件、颜色是调色板实例——
  框架给原生类型解析隐式样式时不写 `Style` 属性。**判据是建出来的树，不是 `Style` 的读回值**，
  所以本批第一条断言在部件上而不是在样式对象上。
- 视觉判据差点选错：Dark 下圆点填的就是标签那支白，勾选时该色键**少了 165 个像素**，
  真正长出来的是勾选环的强调色（+216）。像素断言改成盯勾选环那一支的单色键增量。
- 并行任务 #41 一并结掉：测试工程 24 条警告清零（8 处 `ColorOf(Brush)` 改可空参数并保留响亮断言、
  两处 `Walk` 收可空节点、`HostOf`/`TextsIn` 同理、`AstraContentDialogTests` 三处按钮文本改用
  `SetValue(...TextProperty, …)` 因为 null 是有意义的值、两处 xUnit 分析器改 `Assert.DoesNotContain`）。
  这一版是**真重编**后的 0 警告，不是第七段那种增量构建的读数；`git ls-files --eol` 顺带纠正了一个口径：
  本仓测试 `.cs` 是 LF（30 个里 28 个），不是 ModernWpf 那条 CRLF 规则说的 C# 一律 CRLF。
- 视觉/Gallery：Selection 页新增"Radio button list"卡片（6 条字符串条目、表头、两列开关），
  `Catalog.json` 加 `FluentJalium.Controls.FluentRadioButtons`（`own-type`，gaps 按 §5 重写）；
  `Test-AstraGallerySmoke.ps1 -Page selection` 10.3 秒干净关窗。

不声称：勾选全部由写 `IsChecked` 驱动，**零真指针/键盘/触摸帧**；方向键跨行导航一次没测；
面板取全局最大格子且不做虚拟化（上游按列取最大且可虚拟化）；两条间距不是主题行，应用侧覆盖不到资源键；
高对比一次没量；`HeaderTemplateSelector`/`ContainerContentChanging` 这类上游公开面本层没有。

**闸口读数（第八段，串行 `tools/Test-AstraGates.ps1`，exit 0）**：restore 全部最新；Debug 构建
**0 条警告 / 0 错误**（真重编，非增量）；整套顺序跑 **1137/1137 通过、0 失败、0 跳过**（6 m 47 s；
比第七段 1120 多 17 条 ＝ 新控件 16 条 + 资源键闸口那条字典行的 +1）；调色板漂移 Light 83 源色 / 101 刷、
Dark 同、HighContrast 101 映射键，三行 `checked=True`；`All Astra gates passed.`

**阶段 5 第九段（PipsPager：一枚"确实存在"的字形可以一滴墨都不出，2026-09-20）**：目标写的是
"PipsPager(自有)"，这一段把它改成**可证的**自有类型，并且顺带把阶段 6 图标族的前提量死。

- **字形这条路在本运行时不是"不好看"，是"读不到"**。上游正常点用 `Symbol` 的 `EA3B`；`spike/SymbolCmap/check.py`
  （fontTools 直读 cmap）量到 **cmap 命中**：1 条轮廓、边界框 **0.938x0.938 em**，也就是一枚几乎铺满 em 的实心圆，
  MDL2 同样有。挂载侧一切正常——`FontIcon` 建成、字号 24 量到 26x26、6 量到 6x6——但逐格数墨 **0**，
  而同进程里 `Ellipse` 画的 6 DIP 圆数到 **60**。**布局框、cmap、控件树三项全绿而像素为空**，
  这是本仓第一次把"glyph 不出墨"从猜测变成两列对照读数。结论落在设计上：点改成按测到的墨径画的
  `Ellipse`（5.6 / 3.8 ＝ 0.938 x 6 / 0.938 x 4），阶段 6 的 `SymbolIcon`/`FontIcon` 批从此分两支判——
  **码点命中查 cmap，"画出来了"必须另找判据**。
- **选型是对上一段结论的反向使用**：第八段量出 `ItemsControl` 的覆写面可继承并据此建了 `FluentRadioButtons`；
  本段仍然落回 `Control` + 自管 `Panel` 子元素，因为上游 `PipsPager` 根本没有 `ItemsSource`，
  element factory 只从 `TemplateSettings.PipsPagerItems` 取 **1-based 页号**——继承 `ItemsControl`
  等于白递 `Items`/`ItemContainerStyle` 一整面公开 API。**"能继承"不等于"该继承"**，两条结论各管各的前提。
- 上游那些反直觉的规则全部按读数抄，不按常识抄：N 页 ⇒ N 枚点（**没有前后缀点、没有省略号点**），
  `MaxVisiblePips` 只是把 ScrollViewer 钳到 `(k-1)*默认 + 选中`；导航一次 **±1 页**；
  `SelectedIndexChanged` 的参数是**空的**；越界写先钳制、只发一次（含"钳回当前值"那条臂——本运行时
  写回同一个值仍会进 changed 回调，且 `OldValue` 是被拒的那个值，第一版就是被它顶出两次事件）；
  方向键只搬焦点、从不搬选择；启用性是 `isGenerallyVisible` 那条合取（边缘 + 0 页 + `MaxVisiblePips=0` 三种情形）；
  `NumberOfPages` 默认 **-1**（无界行，只增不减，且要在 `Clear()` 之前拿到旧计数，否则 8 枚缩回 5 枚）。
  禁用侧还买回一条通用判据：`ButtonAutomationPeer`/`IInvokeProvider.Invoke()` 在禁用按钮上抛
  `InvalidOperationException`，于是"到边即禁"不再只能读 `IsEnabled`。
- 键面：上游那一份 51 行（**27 条画刷别名 + 12 条字典外度量 + 6 条 `x:String` 字形/字号 + 6 个 Style**）
  在令牌层发 **29 条**（27 条前景/底色别名 + 2 条 `Thickness`），6 个 Style 用上游同名键落在样式层；
  **16 条不发**（10 个 `x:Double` + 6 个 `x:String`——本 reader 解析这两类行会毁掉整份字典），
  值改成控件常量与样式字面量并逐名配反向断言；其中 `PipsPagerButtonWidth`/`PipsPagerButtonHeight`
  两条**上游自己也没读**（整棵参考树除自身区块与 TestUI 控件命名外无人点名）。
  Light 与 Default **逐行同名同目标**、HC 把 27 条全部重指 `SystemColor*`，所以别名层照旧走调色板实例。
- 四条静默退化（`adaptation/00` 新 **S1-m**）：① `<ControlTemplate.Triggers>` 必须是 `ControlTemplate` 的
  **直接子元素**，写进模板根内部在解析期抛 `Cannot find attached property setter for`（三份模板各撞一次）；
  ② **`Style` 的 setter 够不到模板部件**——`TargetName="SelectedDot"` 静默失效，同一处写入放进模板触发器才生效，
  所以选中/正常改成用 pip 的 `Tag` 由模板触发器切两点 `Visibility`；③ 一个 `Border` 里两个平铺 `Ellipse`
  会让**命名部件整个消失**（`#RootGrid` 仍在、无错无警告），包一层 `Grid` 就都找得到；
  ④ **拿自己的度量结果去钳自己的可视区会把回路饿死**（`UpdateViewport` 在任何度量之前跑完，
  本该重跑它的 `SizeChanged` 再也不来，因为宿主宽度正是被钳的量），改用控件自己写的脚印常量。
  另有一条像素账：Light 下点色 `#5E000000` 与 harness 黑背衬打包后同键，`PixelKey` 在 9600 像素样本里数到
  105966、加点读数不变 ⇒ 判据换成**白底卡片 + 两次捕获的直方图差值**（选中点 19 px，第二枚再涨），
  且比较只能在**同一个被宿主元素**跨三次页面数之间做（同窗宿第二个元素时第一个仍挡在前面）。
- 落点：`Controls/Navigation/FluentPipsPager.cs`（自有类型 + 1 枚举 + 8 DP + 1 事件）、
  `ThemeResources/PipsPager.jalxaml`、`Styles/PipsPager.jalxaml`（5 份样式、导航键 chevron 走 `Path` +
  `ScaleHost` 按下缩放，`Manifest.txt` 51→**53** 份）、新测 `AstraPipsPagerTests` **29 条**、
  资源键闸口 +1 份字典、`audits/pips-pager.md`（§2 运行时差表把"没有的 API"逐名量过：
  `Button.FocusVisualMargin`/`UseSystemFocusVisuals`/`ScrollViewer.*ScrollMode`/`Is*ScrollChainingEnabled`/
  `GettingFocus`/`LosingFocus` 全 0 命中，所以上游那条"滚动跟随选中"的钳制只在可视区一层，本层不做平滑滚动）、
  `adaptation/s1m-pips-pager-raw.txt`、Gallery Navigation 页"Pips pager"卡（横向 10 页钳 5、纵向、
  隐藏箭头、下一页按钮 + 读数条）、`Catalog.json` 新行（`own-type`，9 条 gap）。

四类证据分开记：**构建** = 串行闸口（文末读数）；**行为** = 29 条（部件名与"一页一键"、脚印进容器、
Invoke 选页并换两份 Style 且只发一次、越界钳制的两条臂、无界行只增、0 页禁两侧、±1 导航与到边即禁
（含"禁用键 Invoke 必抛"）、可视区钳制横 60/120/24 与竖 24、朝向换脚印与 `RotateTransform.Angle`、
三种箭头可见性、两点身份与可见性、`Page {i}` + `PositionInSet`/`SizeOfSet`）；
**视觉** = 白底卡片上"加一枚点多出多少墨"（选中点 > 正常点、且都 > 0）、品牌绿 0 命中、
Dark↔Light 两张直方图不同且都画满；**硬件输入** = **仍为零**（选中由 `IInvokeProvider` 与写属性驱动，
hover/press 两格只有触发器定义、没有任何输入帧能把它们打开）。

不声称：hover/pressed 两格无像素证据（任务 13 同一堵墙）；钳制出的可视区不做平滑滚动、不把选中点滚进视野
（上游 `ScrollAlong` 依赖 `ScrollViewer` 的 `ScrollMode`/`ScrollChain` 那族 API，本运行时逐个 0 命中）；
`TemplateSettings.PipsPagerItems` 那一层没有对应物，页号由控件自己编号；无界行没有"到底就停"的判据；
`PreviousButtonStyle`/`NextButtonStyle`/`SelectedPipStyle`/`NormalPipStyle` 四格是自有 API 而非上游键；
高对比一次没量；Gallery 那张卡没有目视过（冒烟只证窗口上屏与干净关窗）。

**闸口读数（第九段，串行 `tools/Test-AstraGates.ps1`，同一棵树跑两遍）**：第一遍 **exit 1**——
1166 通过 / **1 失败** / 0 skip（9 m 55 s），失败的是 `AstraDataGridTests.The_grid_surface_paints_its_background_row`
的"capture never settled"（调色板步骤因此没跑到）。单独跑该类 **56/56** 绿，再与两个同样吃洋红哨兵、
共用同一个宿主窗口的类（PipsPager + TeachingTip）合跑 **159/159** 绿，两次都归不了本批的账 ⇒
按任务 #35 那条序列 flake 记账，不当成"已验"也不当成回归。第二遍（本机无其他进程）**exit 0**——restore 全部最新；
Debug 构建 **0 条警告 / 0 错误**（真重编）；整套 **1167/1167 通过、0 失败、0 跳过**（5 m 35 s，
比第八段 1137 多 30 条 ＝ 新控件 29 条 + 资源键闸口那份新字典行的 +1）；调色板漂移 Light 83 源色 / 101 刷、
Dark 同、HighContrast 101 映射键（3 条上游键因调色板无对应而按住），三行 `checked=True`；`All Astra gates passed.`
Gallery 冒烟 `-Page navigation` 12.8 秒干净关窗、无残留进程。

**测试基座批（公开资源键清单 `audits/keys.md`，结清任务 #11 与 #40，2026-09-20）**：目标里"测试基座补公开资源键清单
keys.md"这条从第一天起就没做，因为一直没定"谁来保证它不烂"。这一批的答法是**两个独立解析器互相核对**，
而不是一个解析器自己检查自己。

- `tools/Report-AstraResourceKeys.ps1` 用**行读法**（先剥掉注释块，再按 `<Type x:Key=...>` 取键与别名目标/字面值）
  生成 `docs/astra/audits/keys.md`：55 份字典共 **1230 行**（调色板 368 ＝ Light/Dark 各 184、令牌 734、样式层 128），
  另有 `HighContrast.map` 的 **101 条映射行**；类型普查 `StaticResource 609 / SolidColorBrush 202 / Color 166 /
  Thickness 107 / Style 75 / ImplicitStyle 60 / CornerRadius 10 / LinearGradientBrush 1`。
  无键的隐式样式按"类型可查、名字不可命名"单列，计数进总账但进不了键名集合。
- `AstraPublicKeysInventoryTests` 用 **`XDocument`** 重读同一批文件，把"文件里真实存在的键名集合"与
  "文档表格里列出的键名集合"做双向差集（缺失 / 凭空多出各报前 20 个），并把 `Totals:` 那一行的四个数
  重新数一遍。闸口新增第 5 步跑工具的 `-Check`（比对整份文件与文档尾部的 `canonical-lines`/`sha256`）——
  测试能证明"清单与字典一致"，只有 `-Check` 能证明"这份文档是刚生成的、没人手改过"，两件事不互相替代。
- 牙量过三次，每次先确认改动真的落进文件：删一行真键 ⇒ `Not in the document: PipsPagerSelectionIndicatorBackgroundPointerOver`；
  插一行假键 ⇒ `Not in the dictionaries: InventedKeyForTeethCheck`；改 `Totals:` 的数 ⇒ 计数用例红；还原 ⇒ 全绿。
  **第一次尝试是假实验**：`sed` 删的键名 `PipsPagerSelectedPipForeground` 在文件里根本不存在（`grep -c` 读回 **0**），
  于是"该红的没红"看起来和"锁有效"一模一样。往后所有 A/B 先断言改动落地（行数或 grep 计数）再看结果。
- 不声称：keys.md 只说"我们发了哪些键"，不说"上游有哪些键"——后者仍在各控件 `audits/*.md` 的逐名反向断言里；
  它也不证明任何键走到像素；写在模板内部的带键元素（今天只有 `Styles/Common.jalxaml` 那一支渐变刷）
  作用域是该模板，已作为口径注记写进文档开头而不是偷偷算进"公开"。

**闸口读数（测试基座批，串行 `tools/Test-AstraGates.ps1`，exit 0）**：restore 全部最新；Debug 构建
**0 条警告 / 0 错误**（真重编）；整套 **1169/1169 通过、0 失败、0 跳过**（5 m 28 s，比第九段 1167 多的 2 条
就是这份清单的双向用例）；调色板三行 `checked=True`；**新增第 5 步**"public resource key inventory"读回
`keys.md is current: 1230 canonical lines.`；`All Astra gates passed.` 硬件输入 / 视觉：本批不动产品码，各 0 条。

**阶段 5 第十段（BreadcrumbBar 落地——三条"看起来对"的通路里两条是假的，2026-09-21）**：
基型选 `ItemsControl` 而不是上一段 PipsPager 的 `Control`。判据不是"哪个更顺手"，是上游公开了什么：
`BreadcrumbBar.idl` 真的只有 `ItemsSource` + `ItemTemplate` 加一枚 `ItemClicked`，所以继承条目管线不会白递给
应用一面它不该有的 API；S1-l 量到 `ItemsControl` 的四个 protected 容器覆写与 `ItemsPanelTemplate` 可换，
S1-m 量到"能继承"不等于"该继承"，这一段就是那个判据的正面用例。条目是 `FluentBreadcrumbBarItem : ContentControl`，
行是 `FluentBreadcrumbPanel`（`ItemsPanelTemplate` 只带类型，横向 `StackPanel` 无从设起）。

三条测量把"照抄上游"顶回去两次，读数都在 `adaptation/s1n-breadcrumb-upstream-raw.txt`：
- **零矩形不是"藏起来"**（[G4]）。上游 `BreadcrumbLayout.cpp:89-93` 收条目用 `Arrange(new Rect(0,0,0,0))`，
  从不碰 `Visibility`。四枚自带 `100x40` 的 `Border` 实测：被收的两枚仍是 `100x40`、只是叠回原点、
  **照旧画笔**（直方图黄 4000 + 绿 4000，蓝 0 只因被压住），不带自身宽度的子元素才会变 `0x0`——上游活的正是后者那个世界。
  照抄会得到一条"隐藏条目挤在第一个可见条目底下、只有读 `ActualWidth` 的人看不见"的行。
- **`Collapsed` 也不是答案**（[G7a]）。它确实让有尺寸的子元素不画（[G5]：红 4000 绿 4000，蓝/黄 0），
  但它把容器整个从 `Panel.Children` 里摘掉：收掉第一枚后下一趟量到的和从 307 掉到 225（只剩三枚），
  "放不下"当场翻成"放得下"，行不再声明溢出而那一枚仍没被摆位。最终落 `Visibility=Hidden`——
  留得住子元素与 `DesiredSize`，由 arrange 自己拒绝给位置，墨量结论与 `Collapsed` 一致（像素用例两向都断）。
- **判据不能用自己那列的宽度**（[G7b]）。省略号在左侧列，显示它就是改窄本列，拿 `finalSize.Width` 判定
  等于"答案改判据、判据再改答案"。改成比宿主条宽（应用给的那个数），显示时补一趟 measure 让预留量取到真数。

另有一条跨控件的基座事实（[G6]，已写进 `adaptation/00` S1-n）：`ContentControl` 派生的自有类型
**不主动展开自己的模板**。症状是"部件找不到"，而资源全绿——`Style` 查得到、隐式键查得到、`Template` 已是
`ControlTemplate`、`LoadContent()` 能展开出三部件，可落地控件的唯一子元素是一枚裸 `TextBlock`。
缺的是 `UseTemplateContentManagement()` + `DefaultStyleKey`（本层另外四个 `ContentControl` 派生类型早就都调了）。
顺带推翻一种抄法：靠"构造时 `Style` 还是 null"判定"没人选过样式"的兜底对这类类型**永不成立**，
那个槽位一开始就坐着框架自己的 `ContentControl` 默认模板。

键账被闸口逼成诚实的两类：上游 29 行，本层发布 **10**、扣住 **19**。扣住里有 6 行是本管线读不出来
（`x:String`/`x:Double`/`x:FontWeight`，加一枚指向未发布字号行的别名、一枚指向调色板没有那支刷子的别名），
另外 13 行是"本层没有能读到它的模板"：四枚 `Current*` 属于最后一枚条目那个被收起的按钮（上游死码），
七枚 `EllipsisDropDownItem*` 与两枚 flyout 表面行属于"下拉列表改由菜单族呈现"这条替换。
第二类的存在正是 `Transcribed_control_rows_are_read_by_a_template` 那条闸口的价值——第一遍跑它就把
`BreadcrumbBarFocusForegroundBrush` 抓成红（我以为写了焦点格，其实那次补丁在写文件前就异常退出了）。

四类证据分开记：
1. 构建：`dotnet build FluentJalium.slnx -c Debug` → **0 警告 / 0 错误**（真重编）；`Manifest.txt` 53→55。
2. 行为：新增 `AstraBreadcrumbBarTests` 38 条（容器与三部件、只标最后一枚、无溢出时逐枚紧挨**零间距**、
   溢出时藏的是前缀且省略号与最后一枚必留、`ItemClicked` 报条目集合里的序号、省略号列表自深至浅、
   `PositionInSet/SizeOfSet` 只数可见条目、10 条发布行解析得到、19 条扣住的名字取不到）。
   **A/B 有牙**：把隐藏机制换回上游的零矩形后 3 条转红（像素、后缀、自动化重排），先断言改动落地再看结果，随后还原。
3. 视觉：`The_dropped_crumb_paints_nothing_in_the_capture` 用三枚 100x24 不透明哨兵白底卡片直方图，
   按容器实际状态双向断言（藏者 0 像素、留者满格），并要求"确有条目被藏"以免空转；
   `Dark_and_light_...` 断言 Light/Dark 前景不同且品牌绿 `#207245` 为 0 像素。箭头字形只断刷子与内衬，
   不断墨（S1-m 第 1 条：字体路径在 RTB 捕获里不产像素）。Gallery 冒烟：新 markup 入树后进程 12 s 不退出、
   截图 `spike/VisualQA/out/breadcrumb-gallery.png`——但导航页没有滚到，所以**没有任何一条上屏帧主张**。
4. 硬件输入：**仍为零**。hover/press/focus 三支触发器、真指针点条目与省略号条目、方向键遍历全未量（任务 #13）。

**闸口读数（第十段，串行 `tools/Test-AstraGates.ps1`，跑两遍）**：第一遍 **exit 1**，2 条红 1206 条绿——
`AstraGalleryCatalogTests` 抓到隐式样式与目录差集（`FluentBreadcrumbBarItem` 有隐式样式却没登记），
`AstraResourceKeyTests` 抓到一行没人读的键；两处都是补真东西（目录加该类型一行 + 焦点触发器真写进模板），
不是放宽闸口。第二遍 **exit 0**：Debug **0 警告 / 0 错误**（真重编）；整套 **1208/1208 通过、0 失败、0 跳过**
（5 m 17 s）；调色板三行 `checked=True`；清单 `keys.md is current: 1245 canonical lines.`；`All Astra gates passed.`
仍欠：省略号下拉条目的"关闭后发事件"顺序没测；下拉不吃应用的 `ItemTemplate`（只 `ToString()`）；
`LandmarkType`/`AccessibilityView`/`IsTemplateFocusTarget` 运行时没有对应属性；RTL 箭头无路径；
高对比那三行整组没实现（含 1→2 的描边宽度）；字号三连 token 本层根本没有，条目文字继承运行时默认值。

## 阶段 6 第一段：ProgressBar 落地——比例是部件名契约，尺寸只能交给包裹层（2026-09-21）

阶段 6 起手的第一件事是改掉一条旧主张：`adaptation/02-render-ceiling.md` 给未换模板的 Slider 记过 954 像素的
`#207245` 品牌绿，而那条捕获从来没有渲染帧。本轮把 `ProgressBar` 接进本层时，顺手在 `AstraPixelTests` 里
把那条注释改成"两套声明都由各自的控件用例接管"，并留下"未上屏的捕获里没有绿、也没有任何别的东西"这一句。

基型选择没有翻案：运行时里 `ProgressBar` 是公开原生类型、`Orientation` 是它的公开属性，因此走原生重模板。
真正改变计划的是 `spike/ProgressProbe` 的七个模式（`census|mount|retemplate|anim|later|alias|axis`）——
它量出"能不能换模板"在这一层不是一句话，而是**同一模板内部要逐条判**（`adaptation/00` §S1-o）：

1. **部件名可以是几何契约**。宿主会按 `Value` 比例改宽 `PART_Indicator`（300 宽的条，25/50/75/100 →
   75/150/225/300），而 `{TemplateBinding Value}` 给的是 DIP 不是比例；`PART_GlowRect`、`PART_Decorator`
   这些上游名字**从来没被读过**。所以本层的比例不是算出来的，是让宿主继续算——我们只负责把它算出的东西放进能看的带子里。
2. **宿主把本地值写在部件身上**。模板里给 `PART_Indicator` 写 `Height=4`，读回来是 `NaN` + `VerticalAlignment=Stretch`：
   那条值被宿主覆盖了。尺寸只能搬到包裹层（`Band`），搬完仍然是 3 DIP 的带 + 随 Value 伸缩的宽。
   这条推翻了"照抄上游模板结构就行"——上游那份是 5 层嵌套的宿主驱动，抄结构等于抄一个被覆盖的槽位。
3. **`Color` 值别名递到 `Brush` 属性是静默消失**。`Background` 读回 `null`，不报错、构建绿；
   刷子的孪生键正常。于是 `ProgressBarBackground` 保留上游键名但指向 `ControlStrongStrokeColorDefaultBrush`。
   `CornerRadius`/`Thickness` 那两类行是落得进去的（读回 `4,4,4,4`），所以"静默"是值类型的问题，不是行类型的问题。
4. **动画通路量死在一半**。重复的 `DoubleAnimation` 在元素级 double 依赖属性上会走表（`Width`、`Canvas.Left`、`Opacity`），
   在 Freezable 变换上**永不走表**（`TranslateTransform.X`、`RotateTransform.Angle` 三帧全 0）；
   markup 里的 `Storyboard` 实例化出来是**零子元素**（attached-property 与 `TargetName`/`TargetProperty` 两种写法都是）；
   `BeginAnimation` 接受**类型不匹配**的依赖属性并且什么都不做。
   后果直接落在计划上：不确定态在这一层做不出来，`IsIndeterminate` 只能是一张静图（用例名就这么叫），
   而下一段 ProgressRing 的旋转不能走变换，得走 `Width`/`Canvas.Left` 或代码环。
5. `<x:Double x:Key>` 在这份读取器里是**硬解析错误**（`Cannot resolve type 'Double'`），不是静默丢——
   上游那几枚数值行因此扣住，`ProgressRingStrokeThickness` 更上游就是死码。
6. `Trigger` setter 里的 `{TemplateBinding}` **会解析**（`MinHeight=7` 两轴都成 7），且 `"Auto"` 与 `"NaN"` 都落到未设置哨兵。
   竖向那条触发器因此能写 `{TemplateBinding MinHeight}` 而不是抄一个常量。

另外量到宿主的不确定态默认模板本身：indeterminate 时它是一个 `CornerRadius=999` 的胶囊铺满整框（50% 处 5704 支色像素），
宿主指标不随帧变化（额外 2 帧与 40 帧后同一个 90x40 块）——所以"沿用宿主模板"在这条控件上等于"交出一个铺满的胶囊"，
这是本层必须自带模板的实测理由，不是审美选择。

键账：上游 `ProgressBar_themeresources.xaml` 10 行，本层发布 **6**、扣住 **4**。
四枚扣住的都在用例里反向钉住（`Resource(...)` 取不到），发布的六枚逐字照抄上游键名，
其中 `ProgressBarBorderThemeThickness`/`ProgressBarCornerRadius`/`ProgressBarTrackCornerRadius` 三枚真的被模板读到，
第二类的"写了没人读"由 `Transcribed_control_rows_are_read_by_a_template` 那条闸口看着（本轮给它加了 `Styles/ProgressBar.jalxaml` 一行）。

四类证据分开记：
1. 构建：串行闸口内 Debug **0 警告 / 0 错误**（真重编）；`Manifest.txt` 55→**57** 份字典。
2. 行为：新增 `AstraProgressBarTests` 35 条（`[Theory]` 把 25/50/75/100 的比例逐档读回、宿主持有指示器尺寸所以带子必须自己扛、
   上游九条 setter、竖向触发器换轴、`MinHeight` 两轴同大小、不确定态是静图、
   上游点名颜色的那行在本层是刷子（`Assert.Same` 指着 `ControlStrongStrokeColorDefaultBrush`）、6 发布 + 4 扣住键）。
   **A/B 有牙**：把模板里的 `Name="PART_Indicator"` 改名为 `Indicator` → **11 红 / 24 绿**（先断言改名落地再看结果，随后还原，
   还原后 `Name="PART_Indicator"` 计数回到 1）。这条 A/B 就是上面第 1 点的证据：名字本身就是契约。
3. 视觉：`The_band_paints_across_the_row_and_never_the_whole_box` 白底卡片直方图，主张带子墨量在 `200~1500` 之间
   （宿主自己的模板在同一个主题下是 5704 像素的满框胶囊，所以这条不是"随便取个区间"而是把两套模板分开）；
   品牌绿 `#207245` 为 0 像素；`Light_and_dark_hand_over_different_rail_brushes` 断两种主题的铁轨刷子不同。
   Gallery 冒烟：新增第 10 页 Status（进度条 + 竖向条 + 不确定态勾选 + 读数），进程 10.5 s 干净退出——
   **本轮故意不截图**，因为没有上屏帧可读，所以这一页只主张"markup 入树且启动不崩"。
4. 硬件输入：**仍为零**。hover/press 没有分支可量（这条控件不吃指针），真指针拖 `Slider` 改值未量（任务 #13）。

**闸口读数（第一段，串行 `tools/Test-AstraGates.ps1`，跑一遍）**：**exit 0** 一次通过——Debug **0 警告 / 0 错误**；
整套 **1244/1244 通过、0 失败、0 跳过**（5 m 16 s）；调色板三行 `checked=True`；清单
`keys.md is current: 1252 canonical lines.`；`All Astra gates passed.`

仍欠：不确定态没有动画（通路被 S1-o 第 4 条量死，要么走 `Width`/`Canvas.Left` 要么等代码环，ProgressRing 那台动画器会一并结这条）；
上游 Error/Paused 两个状态在这一层根本没有驱动属性（`ShowError`/`ShowPaused` 读不到），因此 11 个 VisualState 只映射成 1 条 Trigger；
`TemplateSettings`（`IndicatorLengthDelta`、`Container*AnimationPosition`、`ClipRect`）在本层没有对应物，
所以"裁切窗口"这条上游机制无法复刻；高对比那一组上游行没进本层键账。

## 阶段 6 第二段：ProgressRing 落地——弧要自己算，旋转只能交给帧循环（2026-09-21）

目标里这一条标了"自有"，实测支持它：`spike/ProgressProbe` 模式 `census` 报 `ProgressRing` 在 26.10.9 缺席，
连 `AnimatedVisualPlayer` 与 Lottie 那套也没有对应物——上游模板里能放的东西，这一层一样都放不进去。
基型跟着上游走而不是跟着"进度条是 RangeBase 的后代"走：审计确认上游 `ProgressRing : Control` 自带
`Minimum`/`Maximum`/`Value` 三枚自有 DP，所以本层也是 `Control` + 自有量程（运行时里 `RangeBase` 确实公开，
但把它当基型是一种"顺手继承实现细节"，与上游不符）。

这一段真正的产出是 `spike/RingProbe` 的七条账（`adaptation/00` §S1-p），因为它们决定了几何只能怎么写：

1. **画一条弧有四条路，三条是死的**。markup 里 `<Path.Data><PathGeometry><PathFigure><ArcSegment>` 两种写法
   都能实例化，但 `Figures.Count` 读回 **0**、一个像素不印——要素名和属性名一样不被校验（S1-j 的姊妹账）。
   字符串 `Data="M … A …"` 会印，代码 `new PathGeometry(){Figures={…}}` 也印且与字符串同像素数。
   后果直接砸在下一阶段：`SymbolIcon`/`FontIcon`/`PathIcon` 族里上游那批**要素写法**的几何不能照抄。
2. **虚线描边是存而不用**。`StrokeDashArray="20 200"` 逐字读回 `[20,200]`、`LineCap` 也在，画面上与不写虚线
   **逐行相同**；`StrokeDashOffset` 写 0/40/95 三次捕获**字节全等**。上游 `ProgressRing` 的 `IsIndeterminate`
   在别的框架里常靠虚线做旋转错觉，这条路在本层直接不存在。
3. **"能画不能动"补了一维**。静态 `RotateTransform` 是**生效**的（界宽 44x50 → 90° 后 51x43 → 180° 后 43x50），
   而 `Forever` 的 `AngleProperty` 动画永不走表——与 S1-o 第 4 条合成一条完整结论：变换能画、不能动。
4. **`HoldEnd` 的残值会活过 `BeginAnimation(dp, null)`**：本地值读回 markup 里那个数，画面却停在动画终值
   （同一个椭圆 556/50x50 → 812/44x50）。这条 retro-justifies `NavigationIndicatorAnimator` 里的
   `FillBehavior.Stop`，不是巧合。
5. **零长度弧 + 圆帽仍会印 2 个像素**，所以 0% 必须交回空 `PathGeometry` 而不是"宽度为 0 的弧"。
6. `Geometry.Parse(string)` 在，`PathGeometry.Parse`/`StreamGeometry.Parse` 不在；`Rect.Empty` 的 Extent 不是有限值，
   空几何要断 `Figures` 空或 `IsEmpty`，不能断 `Width`。
7. 命名纪律：`Path` 在这台运行时与 `System.IO.Path` 同名（编译期要别名），`Grid.GetValue(Control.BackgroundProperty)`
   这种"借别类型的 DP 读"在 Jalium 里读回 null——DP 身份绑声明类型，读回必须写元素自己的属性。

几何数字来自两份 Lottie 资产**公布出来**的参数，且两态各一套、没有被抹平（ModernWpf 是同一套 determinate
数字画两态）：determinate 半径 `8×1.77/32`、描边 `1.5×1.77/32`；indeterminate 半径 `7×5/80`、描边 `1.5×5/80`、
恒定 180°（`TrimEnd=0.5`）；一圈 `900°/2 s`。起点 `-90` 与封顶 `359.9` 明确记成 ModernWpf 的发明（上游
`Rotation` 是 -0.008°，路径起点由 Lottie 二进制决定，本仓库没有能读它的工具）；它的
`IndeterminateStartAngle=305`、`Sweep=160`、`1.6 s` 三个数在参考仓库 A 全文 0 命中，**一个都没抄**。
旋转最后写在弧的起始角里而不是变换上——不是因为变换被证明更差（第 3 条只证明静态变换能画），
而是第一版把这条误当成空画面的原因，改完之后没有再做对照实验，所以"支点落在哪"作为 Known Gap 6 留着。

**自己的 bug 由自己的用例抓出来**：`IndeterminateRadiusFactor` 写成 `7 * 5 / 80`，整数除法得 0，
不确定环只印 3 个像素。修法 `7d * 5 / 80`；更重要的是先把归因撤回——我最初把空画面写成"变换支点问题"，
那条主张已经连根删掉，换成了 Known Gap 6 的"仍未量"。

键账：上游 `ProgressRing_themeresources.xaml` 3 个键名 / 7 个站点，本层发布 **2**、扣住 **1**
（`ProgressRingStrokeThickness` 是 `x:Double` 行——S1-o 第 5 条的硬解析错——且上游 `grep` 显示**没有任何模板读它**，
两头都不成立）。HighContrast 那一组重指向仍留在调色板层（`HighContrast.map`），与其他控件同一条账。
状态映射：上游 3 个 VisualState → 本层 1 条 Trigger（`IsActive=False` → `LayoutRoot.Opacity=0`）；
`Inactive` 的第二条设置 `AccessibilityView=Raw` 无处可写，所以**不活动的环还在无障碍树里**——这是可证明的差别，
写在 Known Gap 5，不当"未测"处理。

四类证据分开记（`audits/progress-ring.md` §7 是同内容的展开）：
1. 构建：`Manifest.txt` 57→**59** 份字典（新增 2 份），三个工程重出 dll。
2. 行为：新增 `AstraProgressRingTests` **30** 条——上游 12 条 setter、两条别名 `Assert.Same`、部件契约与
   "单 `PathFigure` 挂 `ArcSegment`"、五档进度 → 0/90/180/270/359.9、五档 `IsLargeArc`、量程改写、退化量程交回空几何、
   不确定态恒 180° 且不读 Value、因子在 32 与 64 两种盒子上各读一次、帧循环用**前后差**而不是阈值、
   `IsActive=False` → `Opacity=0`、2 发布 + 7 扣住的 theory。
   **A/B 有牙**（提交前复跑）：把 `Name="ProgressRingArc"` 改名（`grep -rn` 先确认全仓 1 处、改后 `grep -c` 读回 0）
   → **16 红 / 14 绿**；还原 → 同一过滤器 **30/30 绿**。
3. 视觉：四条像素主张全部读实测量——半环 vs 整环比值 `whole > half*1.4`；0% 与不活动印 **0** 个强调色像素，
   且同帧 `PaintedPixels>0` 当白卡守门（否则"零墨"与"没截到图"不可分）；不确定态墨点 40~320；品牌绿 0；
   Light↔Dark 直方图不同。Gallery：Status 页加四枚环卡（确定态/自转/64 DIP/不活动）+ 读数，
   `-Page status` "closed cleanly in 5.8 s；1 page(s) mounted and closed；no Gallery process left."，全 10 页 6.1 s 干净退出；
   目录加 1 行 `FluentJalium.Controls.FluentProgressRing`（`parity: own-type`，4 条证据路径 + 9 条 gap）。
   **故意不截屏**，所以这一页只主张 markup 入树不崩。
4. 硬件输入：**仍为零**。环照抄 `IsHitTestVisible=False`/`IsTabStop=False` 并各钉一条断言，因此没有输入臂可测——
   这不等于指针路径已验证（任务 #13 依旧是全仓库欠账）。

**闸口读数（第二段，串行 `tools/Test-AstraGates.ps1`，跑一遍）**：**exit 0** 一次通过——Debug **0 警告 / 0 错误**；
整套 **1275/1275 通过、0 失败、0 跳过**（5 m 6 s；比上一段 1244 多 31 = 新控件 30 条 + 消费点闸口那份新字典 1 条）；
调色板三行 `checked=True`（本批零改动）；清单 `keys.md is current: 1256 canonical lines.`；`All Astra gates passed.`
闸口跑在一处**注释订正**之前（模板注释误称控件还写 `RenderTransform`），订正只动注释、随后单跑环的 30 条全绿。

仍欠：不确定态的**尾部收窄**没做——上游一圈里同时把 `TrimStart` 从 0 推到 0.5，本层是恒定 180° 弧在转；
两份 Lottie 资产本体不进本仓库，外观是从资产公布的数字重建的近似而非同一渲染源；
`ProgressRingTemplateSettings` 三行没有绑定面；`ProgressRingStrokeThickness` 不发；
**`CompositionTarget.Rendering` 这条通路已经量通并且在本段用上了，因此上一段"进度条不确定态做不出来"的前提部分失效——
但本段没有顺手去接那根线**（`ProgressBar` 的 `IsIndeterminate` 至今仍是静图，Known Gap 1 原样保留，
不用相邻证据结清）；变换支点归属未量（Known Gap 6）。

## 阶段 6 第三段：Divider 落地——上游没有这个控件，权威只能来自令牌（2026-09-21）

九步出口的**第一出口**在这一段只能以"搜过什么、没搜到什么"的形式交付：钉住的 commit 里
`git ls-files | grep -i divider` **0 命中**（同一命令对 `progressring` 有 125 命中，证明树是完整的），
`.idl`/`.h` 里 `Divider` 只剩三处注释；运行时侧 `adaptation/05` §F 早量过 `Divider` 类型也不存在。
两头都没有 ⇒ 这一行**不能**声称与 WinUI 的某个控件逐字对齐，目录里给它新加一个图例值
`no-upstream-control` 而不是 `audited`（`AstraGalleryCatalogTests` 的取证闸口是按图例驱动的，加值不用改测试）。

唯一可用的权威因此只剩两样：**令牌行**与**同角色模板**。七处同角色分隔线逐处读到行号与 blob
（应用栏 `51801458…`:14/16/18/49、菜单 `6f9f3fd3…`:254/258/733、分裂项竖线同文件 :262/606、
日期 `8d19f300…`:291-304、时间 `2cfb8a7d…`:294-305、侧栏 `a21c87b4…`:572-605、标签页 `c0e732a4…`:553），
共同点只有一个：**跨轴厚度 1**。七处没有一处带朝向属性（侧栏靠 VSM 的 `VerticalLine` 换整套数字、
应用栏靠 `Overflow` 状态、日期/时间直接放两套要素），各处 margin 也互不相同（2,8,2,8 / -4,1,-4,1 / 0,3,0,4 / 0,8,0,8），
所以本层一个 margin 都不抄——不是忘了抄，是没有任何一个能代表通用分隔线（Known Gap 3）。
颜色三行 `DividerStrokeColorDefault` / `…Brush` / HC 重指向本层调色板早就有，本段**发布 0 行、消费 1 行**。

基型这一格被量成一个正向结论：运行时里有原生 `Jalium.UI.Controls.Separator : Control`，自己声明 `OnRender`，
自有 DP 只有 `Orientation` / `StrokeBrush` / `StrokeThickness` 三枚，出厂 `Style=null Template=null
IsHitTestVisible=False`。于是 AGENTS.md 的"原生控件优先"这一条**成立**，起自有类型反而违规；
而且 `spike/DividerProbe` 量到不套模板就能换色（thickness 1 → 600 像素 / 2 行，4 → 1200 / 4 行，红色刷子那 4 行印红），
所以 `Styles/Divider.jalxaml` 整个文件就是**两条 setter、零模板**，并把它钉成文本形状闸口。

这一段最值钱的产出是 §S1-q 七条账里的三条**跨控件**结论：

1. **1 DIP 的 `Rectangle` 什么都不印**（上/中/下三种对齐与 1 DIP 描边写法全 0；同盒子 `Border` 600、`Line` 604；
   `Rectangle` 到 2 DIP 才恢复，3 DIP 仍只印 2 行）。机制未量，后果是一条已经欠下的账：仓库里**已发布五处**这种写法
   （`Styles/AppBar.jalxaml:194`、`Styles/DataGrid.jalxaml:53/59/60`、`Styles/TreeDataGrid.jalxaml:52`，本段逐行复核过，
   全库再无 1 DIP 的 `Line`/`Rectangle`），它们现在是看不见的线。**不在 Divider 提交里顺手改**，另开 #46 独立缺陷批；
   顺带记下一条需要重量的旧理由——`DataGrid.jalxaml:48` 的注释写着"用 Rectangle 而不是 border，免得被表头项盖住"。
2. **`{ThemeResource}` 只在并入主题字典之后解析**：单独 `XamlReader.Parse` 出来的 `Border` 写令牌 `Background` 印 0 墨。
   这是 S1-p"读不到不等于没有"的坑第二次遇到，差别在于这次三个空格子有两个被第 1 条（死的 1 DIP `Rectangle`）污染，
   只有一个 `Border` 格子是干净证据——先在代码里解析刷子才拿到真读数。凡是"标记写不写得出东西"的验证，
   都要区分并入后与单独解析后，**后者不能当作前者的反证**。
3. **暗令牌配白卡等于没放**（`#15FFFFFF` 叠白实测 `ink=0`，Border 与控件自绘两条路都是 0），
   所以"两档主题不一样"的像素主张必须给样本配一张同暗度的卡（本层 `#202020`）。这条给 S0 那批像素判据补了前提。

**状态映射那一格是空的，而且这不是"未测"**：上游没有这个控件就没有 VisualState 可映射，运行时侧 `Separator`
是静态元素、唯一会变的是宿主给的 `Orientation`（那是控件自己的属性）。审计 §5 把这句原样写出来，
不用"暂未覆盖"糊过去。

四类证据分开记（`audits/divider.md` §7 是同内容的展开）：
1. 构建：清单 59 → **60** 份字典（实测两遍计数），`keys.md` 重出 1256 → **1257** 行 / 61 → **62** 份，
   新增行就是 `ImplicitStyle → Separator`；闸口里 Debug 0 警告 / 0 错误。附带一条运行时约束：清单与随包文件双向对账，
   注掉一行而留着文件会让主题加载抛 `… is not listed in Themes/Manifest.txt`。
2. 行为：`AstraDividerTests` **13** 条——隐式样式落地用**实例身份**读（`Assert.Same(调色板刷子, separator.StrokeBrush)`
   并断它不等于出厂灰）、`StrokeBrush` 是刷子不是颜色、`StrokeThickness==1`、`IsHitTestVisible=False`、
   换轴后两条 setter 仍在、消费行类型 theory、五个自造名反向 theory、两条 setter 零模板的文本形状闸口。
   **A/B 有牙**：删 `<Style>` 整块（先 `grep -c` 读回 1 → 0）→ **5 红 / 8 绿**，还原 → 13/13。第一次 A/B 改的是清单，
   结果拿到一堵 1 毫秒全红的假墙（红因是主题加载被拒，不是断言），已在 §S1-q 第 6 条结清。
   这条 A/B 还顺带量出：**三条几何断言（600/600/200）在没有样式的构建里照样绿**——框架那条灰线印同样多的墨，
   所以几何不是令牌的证据，令牌的主张只住在颜色断言里。
3. 视觉：横线 600 = 2 行 × 300、竖线 200 = 2 列 × 100（`Render()` 的 DIP 像素实测）；Light 两行 `#F8F8F8` 且
   出厂灰 `#A3A3A4` 计数 0；Dark 换深卡后线行比卡亮、两档线色不同；两档品牌绿 `#207245` 计数 0。
   Gallery：Status 页加 Divider 卡（正文夹横线 + 左中右带竖线）；冒烟 `-Page status` 单页 10.7 s 干净关闭，
   全 10 页一遍 **exit 0**（status 5.7 s、最慢 command-bar 11.2 s、无残留进程）。**故意不截屏**。
4. 硬件输入：**仍为零**。`IsHitTestVisible=False` 是出厂值并被断言，这条控件没有可测的输入臂——
   但这不等于指针通路已验证（#13 依旧是全仓库欠账）。

**闸口读数（第三段，串行 `tools/Test-AstraGates.ps1`，四遍三种结果）**：第 1 遍全绿——Debug **0 警告 / 0 错误**，
整套 **1288/1288 通过、0 失败、0 跳过**（5 m 6 s；比第二段 1275 多 13 = 本段新增的 13 条，没有一条靠削减别处得到），
调色板三行 `checked=True`，`keys.md is current: 1257 canonical lines.`。第 2~4 遍都红，**但红的都不是本段的用例**：
第 2 遍 22 失败（`AstraContentDialogTests` 整类 13 名 + 1 条菜单子菜单，异常逐字仍是 #35 那句
`ContentDialog could not resolve a host window.`）、第 3 遍 1 失败（NavigationView pill 增量 `1306`——因为"静止"那张里
#EAEAEA 仍有 8040，压根没拍到取消选中的画面）、第 4 遍 1 失败（TeachingTip 翻转落边 `Expected: Bottom / Actual: Top`）。
四遍之间产品与测试源码完全相同（第 1 遍就已带着这 13 条），只差 `Catalog.json` 一处四空格缩进；HEAD 全程 `e425208`，
没有别的会话提交进来。三条红登记在 #35 / #47 / #48。
**下一步因此不是继续堆控件**：把固定 `Settle` 换成"轮询到稳定或超时"、并查 `Host()` 双拍与顺序跑抢占这条公共成因——
判据不稳时后面每一段的绿都不算数（阶段 0 的老规矩）。

**同日补一段：红的那三遍里有一条是判据自己的错**。pill 那条单跑三次拿到 1 通过 + 2 次"never settled"，
而红字里 `{sample.Subject}` 是**空的**——顺着查下去是一处算术：`Capture` 要"连续两轮抓到同一张图"才算稳，
一轮最多花 `Pump` 的 400 ms 帧预算，可它给自己的总上限**也正好是 400 ms**，于是"一轮花满就没有第二轮"，
一幅完全静止、只是这一拍迟迟不来帧的画面必然被判成不稳。修法不再让两个数各写各的字面量：
`SettleBudgetMilliseconds = PumpBudgetMilliseconds * 3`（够问完两轮再加一轮余量，真正在动的图照样不过），
并且把 `Rounds`、新增的 `CaptureMilliseconds` 与 `Host()` 的 `Subject` 一路带到失败消息里——
"never settled"从此有两种可读形状：**轮数多 = 图真在变**，**轮数 1~2 且 ms 顶到上限 = 帧来晚了（判据的事）**。
验证分开记：改后 pill 单跑 4/4 绿、加 8 个 CPU 空转进程仍 3/3 绿、三个像素重的类 165/165 绿（2 m 35 s），
**整套串行闸口第五遍全绿**（1288/1288、5 m 28 s、0 警告 0 错误、调色板三行 True、keys 1257、
`All Astra gates passed.`）。但这遍绿**不等于已修**：改后再没能抓到一次红，"什么条件下帧会迟到到花满一轮"
仍未证，#35 / #47 / #48 继续开着。详见 `adaptation/06` 的"判据自己的耐心不够"一节。

仍欠：出厂线色 `#A3A3A4` 的来源（哪一行令牌还是硬编码）没查；`Separator.Background` 在这个类型上画不画东西没查，
所以那条 setter 不写；12 DIP 盒子是运行时自己的度量而非上游数字（上游同角色线的总高是 3 / 8 / 20）；
1 DIP `Rectangle` 的机制未量；五处已发布死线另批处理（#46）。

## 阶段 6 第四段：InfoBadge 落地——数值令牌有两种死法，字形墨两条通路都到不了（2026-09-21）

上游 `InfoBadge/InfoBadge_themeresources.xaml`（blob `b09b56572ac8a2159bf9ba2dda7f063ec5460c6a`，147 行）声明
**12 个键 × 三套字典 + 16 个 Style**。本层发 6 行（2 别名 + 4 Thickness），16 个 Style 全量转录加隐式行共 17 条。
不发的那 6 行不是"选择不发"，是量出来的两种死法（`adaptation/00` **§S1-r** 第 1、2 条，原始读数
`adaptation/s1r-double-row-raw.txt`，探针 `spike/DoubleRowProbe`）：`<x:Double>` 让**整份字典解析失败**；
`clr-namespace:System` 的 `<sys:Double>` 能解析、CLR 类型也对、**值恒为该类型默认值**（写 4 读回 0），
`sys:String` 的 `"4"` 喂 `MinHeight` 同样不做转换——同一批实验里 `Thickness` 行是好的（`Padding=4,4,4,4`），
所以"发一条读回来永远是 0 的令牌"比不发更坏：所有消费者看起来都正常。
顺带结掉一个本来要做的选择题：`InfoBadgeIconHeight` 是唯一分主题的数值行（Default 8 / Light+HC 9），
而全仓 grep 它在上游**只有声明处命中、零消费者** ⇒ 8 与 9 之争不存在，两条死行一条都不写。

自有类型的理由是先量宿主再定的：`spike/ControlCensus` 对 `InfoBadge` 与 `Badge` 都给 no type with this name，
没有可重模板的原生宿主，基类跟上游一样是 `Control`（`Value` 只是"显示不显示数字"的开关，不是范围，所以不是 `RangeBase`）。
两处公开 API 偏离按实测写：上游输入是 `IconSource`，而 `IconSource`/`FontIconSource`/`SymbolIconSource`/
`PathIconSource`/`BitmapIconSource`/`ImageIconSource` 在本运行时**一个都没有**，那个属性没有可放的类型，
故 `Icon : IconElement`（缩放通路不变，`Viewbox` 在运行时确实是 `Decorator` 子类）；另一处是一个样式的
`Setter.Value` 只装**一个**元素实例，5 个 `*IconInfoBadgeStyle` 各持一个图标元素——树级独立性有断言
（两个 presenter 的 `Content` 都在且各等于本徽章的 `Icon`），**视觉独立性不可测**，理由见下面第 3 类证据。

状态映射那一格：上游 4 个 `DisplayKindStates` 收进 3 条触发器（`Dot` 那格在上游就没有 setter，本层也不写），
每条带两行——parts 的 `Visibility` 加各自那条 margin，所以"哪条触发器命中"是拿边距读的，不是拿可见性读的。
半径 = `ActualHeight/2` 这条（`InfoBadge.cpp:91-98`）连同它"本地设过就不动"的逃生阀一起抄到了：
写在控件自己的 `CornerRadius` 上、用 `SetCurrentValue` 保类型、模板 `TemplateBinding` 接走，
两半各有一条断言（`The_pill_radius_is_half_the_measured_height` 与
`A_locally_set_radius_is_left_alone_the_way_upstream_leaves_it`）。结构偏离保留：本运行时的 `Grid` 没有
`CornerRadius` 成员（编译器直说），所以根是名为 `RootGrid` 的 `Border` 套一个 `Grid`。

这一段真正的产出是两条**基座级**事实，都不限于 InfoBadge：

1. **模板里的 `TextBlock` 不继承控件的 `Foreground`**。上屏读回是框架默认 `#E4000000`，而徽章自己的
   `Foreground` 是 `#FFFFFFFF`；上游那行什么都没写、靠继承。本层加 `TemplateBinding` 绕开，
   但**其余自有样式里所有靠继承拿前景色的文本部件还没有逐处审**（另立 #51）。
2. **字形墨在本基座两条捕获通路都拿不到**。先量到 `Value=12` 的图与点徽章的图**逐键相同**（920/40），
   再排除模板因素：一个最朴素的 `TextBlock` 白字压蓝板 960 像素全是 `#0078D4`、压 `#202020` 卡 960 全 `#202020`；
   整窗合成的 `Host()` 也一样，`#FFFFFF` 计数 **0**，加不加那行文字只差布局挪动（6136/279 对 6118/288）。
   这条把 §S1-m"符号字形不进像素"扩成**所有文字**，也因此回头削弱 PipsPager、TabView、菜单文本几批的像素列——
   "数字印出来了"这句在本仓库当前不可证，`The_number_is_laid_out_and_wears_the_badges_own_foreground`
   是它的退让落点（`Text`/`Visibility`/刷子实例身份/度量盒四样）。颜色主张仍然有牙。

目录那一格还被抓出一条自己的错：`Catalog.json` 里 `markup` 先写成显示名 `FluentInfoBadge`，
反射目录闸口当场红（"59 restyled, 59 catalogued"却差集非空——own-type 行必须写全限定名）。
同一段里 `Test-AstraGallerySmoke.ps1 -Page status` **两轮都过**，可见"页能上屏"绝替不了目录闸口，
这条闸口的价值恰恰是在冒烟全绿时红的。改完单独重跑该类：6 条里 5 绿，剩的一条是
`The_gallery_project_carries_the_catalog_it_reads`——测试跑 `--no-build`，Gallery 输出目录里还是旧副本，
完整构建之后才会一致，这是它该红的样子。

四类证据分开记（`audits/info-badge.md` §7 是同内容的展开）：
1. 构建：清单 60 → **62** 份字典（`Themes/Manifest.txt` 非注释行实测 62）；`keys.md` 1257 → **1280** 行
   （新增 23 = `ThemeResources/InfoBadge.jalxaml` 6 行 + `Styles/InfoBadge.jalxaml` 17 行）；
   Debug **0 警告 / 0 错误**（1 m 20 s）。
2. 行为：`AstraInfoBadgeTests` **42** 条（17 个方法 + theory 展开），整套 1288 → **1330**（+42，没有削减别处）。
   **A/B 有牙**：三处定向改动（数值行改成哨兵、Critical 那行刷子换掉、`ValueTextBlock` 的 `Foreground` 摘掉）
   → **失败 3 / 通过 39**，红的正是那三条名字；改前先 `grep -c` 读回锚点为 1、改后读回为 0 才认定还原。
   第一轮 A/B 因为脚本里用了相对路径，mutate 静默没落地却报"42/42 通过"，已换绝对路径重跑。
3. 视觉：pill 的 accent `#0078D4` **920** 像素、两条圆角边混色 40、品牌绿 `#207245` 计数 **0**、
   `Informational` 在 Light 与 Dark 不同色（都走 `Render`）；两条负面读数（`Value` 与 `Dot` 同图、
   `Host` 白字 0 墨）按原样记进审计 §5，**不当作通过**。Gallery Status 页加 InfoBadge 卡（六徽章一行 + 说明）；
   冒烟 `-Page status` 第一轮报"30 s 未空闲"，重跑 **17.1 s** 干净关闭——**记成重试，不记成一次通过**。
4. 硬件输入：**仍为零**。`IsTabStop=False` 是出厂值并被断言，本控件没有可测的输入臂，
   但这不等于指针通路已验证（#13 依旧是全仓库欠账）。

**闸口读数（第四段，串行 `tools/Test-AstraGates.ps1`，两遍 + 两步补跑）**：第 1 遍在 `dotnet test` 第 31 秒就红，
红的就是上面那条目录名写错，当场停跑、改 `Catalog.json`、重跑。第 2 遍整套 **1330 条：失败 20、通过 1310、跳过 0**
（9 m 26 s），**InfoBadge 的 42 条一条没红**；20 条红里 **19 条**是 `AstraContentDialogTests` 整类同一句
`ContentDialog could not resolve a host window.`（#35 恶化：上一段是 13 名，这次整类）、第 20 条是
`AstraAppBarTests.The_open_bar_shows_its_overflow_outside_the_surface_this_capture_can_reach`
（`popupOpened` 读回假，弹层没为点击打开——同族第四个成员，记进 #35 那条成因下，另开读数账）。
脚本在 test 步 `Write-Error` 退出，**后两步因此根本没跑**，所以本段不谎称"闸口给了调色板与键清单读数"，
改为单独执行同两个脚本：`Sync-AstraPalette.ps1 -Check` 三档 `checked=True`（Light/Dark 各 83 源色 101 刷，
HC 101 映射 + 上游三行因调色板无对应而按住），`Report-AstraResourceKeys.ps1 -Check` 报
`keys.md is current: 1280 canonical lines.`，两条 exit 0。**这套闸口当前不是全绿**，也不接受按全绿记。
判据不稳因此压过继续堆控件：#35 一次吃掉整类，等于这套基座对 ContentDialog 全族已经失去判据——
本段没有顺手去修它，那是下一段的第一件事。

仍欠（Known Gaps，全量在 `audits/info-badge.md` §6）：`IconSource` 面的公开 API 偏离；
共用图标元素的**视觉**独立性不可证；6 条数值行发不出去（4 条写成字面量、2 条上游死行不写）；
文本/字形墨在任何捕获通路都不可见（#50）；模板 `TextBlock` 前景不继承，其余文本部件未逐处审（#51）；
HighContrast 的两条别名重指向没进 `ThemeResources/HighContrast.map`（与 ProgressBar/ProgressRing/PipsPager 同账）；
无 AutomationPeer（上游也没有）、无 `Severity` 枚举、`DisplayKind` 是本层加的可写属性而上游藏在 C++ 里；
硬件输入零。

**阶段 6 第四段补（结清 #35：宿主窗口是线程亲和的持久状态，判据修在 harness 侧，2026-09-21）**：
上一段末尾那句"那是下一段第一件事"兑现了。成因、探针逐字读数与修法全文在
`adaptation/06-pixel-attribution.md` 的"宿主窗口是谁给的"一节，这里只记结论加四类证据。

一句话成因：`ContentDialog` 解析宿主时**先问 Win32 要调用线程的 active 窗口**，要不到才回退
`Application.MainWindow`；本仓库的测试宿主从来不走 `app.Run(window)`，而 `Window.Show()` 自己不写
`MainWindow`，所以第二条通路在我们的进程里**永远断着**，每个对话框都吊在第一步。更要紧的是 active 会因
**别的窗口关闭**变成 0 并且一直 0 下去（探针 B 读：`neighbour closed: active=0 … dialog threw: ContentDialog
could not resolve a host window.`）。于是"单跑一类全绿、整套顺序跑从某一格起整类红"挂了三个阶段的东西
根本不是竞态，是一条**线程亲和的持久状态**——顺序跑里任何一次开合窗口（弹层自己的顶层窗口首当其冲）
都能永久制造它。

修法落在判据侧，产品代码一行未改：`PixelHarness.EnsureHost` 建宿主窗口时，`Application.Current.MainWindow`
为 null 就把它指过去（只在 null 时；别的地方命名过就不动）。

四类证据分开记：
1. 构建：`dotnet build FluentJalium.slnx -c Debug -t:Rebuild` → **0 警告 / 0 错误 / 7.94 s**。特意重编而不是
   沿用闸口里那次 2.87 s 的 up-to-date 构建，并按三个输出 DLL 的 mtime（00:02:59 / 00:03:01 / 00:03:02）
   确认真的重新产出过——增量空跑里的"0 警告"不算证据。
2. 行为：新增 `AstraContentDialogTests.The_dialog_resolves_the_host_the_harness_names`，整套 1330 → **1331**。
   **A/B**：只加测试、不加 harness 那三行 → 该条红（`Assert.Same()` 读回 null）；加上 → `AstraContentDialogTests`
   整类 **39/39 绿**（补跑一次实测：39 条 / 37 s），整套里 `could not resolve a host window.` 一句**不再出现**。
   中间态按原样记，不藏：第一版回归测试照探针的样子在测试内部再开一个邻居窗口然后关掉，ContentDialog 是绿了，
   可整套红从 20 涨到 **30 条、散在 15 个类**（一片 `No part named SuggestionsContainer` /
   `PART_DropDownItemsHost`、几张 `capture never settled: #000000x7920` 空屏）——**在同一根共享 UI 线程上关一个
   窗口会毒掉之后所有依赖覆盖层的断言**，探针里那句 `active=0` 影响面比对话框大得多。复现因此退回一次性探针
   进程，测试只做不变式读回，注释里写明不许在这条通路上开合窗口。
3. 视觉：本段**没有任何像素主张**，也没重跑视觉测试——改的是测试宿主，不是任何控件的样式或模板。
   上一段那两条负面读数（文本与字形墨到不了任何捕获通路）不受影响，仍是 #50。
4. 硬件输入：**仍为零**，本段没碰输入通路。

**闸口读数（串行 `tools/Test-AstraGates.ps1`，全绿一次，管道退出 0）**：`已通过! - 失败: 0，通过: 1331，
已跳过: 0，总计: 1331，持续时间: 5 m 23 s`；`Sync-AstraPalette.ps1 -Check` 三档 `checked=True`（Light/Dark 各
83 源色 101 刷，HC 101 映射 + 上游三行因调色板无对应而按住）；`Report-AstraResourceKeys.ps1 -Check` 报
`keys.md is current: 1280 canonical lines.`；末行 `All Astra gates passed.`。
**这是阶段 6 开工以来第一次整套全绿**，上一段"这套闸口当前不是全绿"那句读数因此被本段取代（原文按段保留，
不删）。上一段那条非对话框族的红 `AstraAppBarTests.The_open_bar_shows_its_overflow_outside_the_surface_this_capture_can_reach`
在同一次全绿里也绿了：它同属"弹层要一个活宿主"这张因果图，但**本段没有单独证这条的因果**，只记"它不再红"。
#47（子菜单 `IsSubmenuOpen` 读回）与 #48（NavigationView 选中指示器像素增量）同样在这条绿跑里没复发，
仍然挂着——**一次不红不等于一次证清**。

仍欠：解析顺序是"先 active 后 MainWindow"，所以"active 恰好是别人的窗口"那一格本段没治（探针 A 读里那次
"邻居活着时开对话框"其实把对话框开进了**邻居**窗口）；harness 只有长期唯一宿主，正常路径碰不到，但弹层还开着
时建对话框仍可能挂到弹层的顶层窗口上，这条留在 #35 的账下继续观察。另：`Symbol` 成员集合与钉住的 26.10.9
程序集仍未逐字 diff（#37）。

**阶段 6 第五段（RatingControl 落地——"一颗星多宽"这个问题问错了三层，2026-09-21）**：
`RatingControl`/`Rating`/`RatingItemInfo`/`RatingItemFontInfo`/`RatingItemImageInfo` 五个名字在真正装着控件的
程序集（`Jalium.UI.Managed`，2 958 个公开类型）里全部 ABSENT（`s1s` [A]），自有类型是被量出来的。
键账：上游 16 把，本层发布 **13**（8 别名 + 1 字形条目 + 4 模板），扣住 3 把 `x:Double`（32 / 8 / -12.5）；
前两条成常量与字面量并由测试从控件读回，**第三条整条不写**——它是上游配 `TextLineBounds="Tight"` 的补偿，
本运行时的标题没有那个旋钮。`MUX_` 前缀照留。六个 `CommonStates` 一比一变成六条模板触发器，
目标仍是上游那个 `ForegroundContentPresenter`，但部件类型换成 `ContentControl`
（26.10.9 的 `ContentPresenter` 不声明 `Foreground`，写上去就是 #31/#33 那类死 setter）。

三层"问错"是被三次红读数一层层顶出来的，全部按原样记：
- **第一层：间距。** 照抄上游 `Spacing = 8-(advance-actual)`（cpp:249-268）后测试红：期望 24、实测 **34**。
  探针 [C] 给出口径——`Spacing=-10` 属性读回 -10、三格 16 宽 desired 仍 48、pitch 仍 16：
  **负号留在属性里、排布按 0 算**，这是继 `{x:Bind}` 与 `Clip` 之后第三类静默失效，而且最阴（属性读回会"证明你写了"）。
  同一份 -10 拆成正 8 + 后续格 `Margin.Left=-18` 时 pitch 实测 24，负边距确实动布局。
- **第二层：一颗星到底多宽。** `cpp:190-203` 用裸探针 run 的 `DesiredSize.Width` 覆盖 `m_scaledFontSizeForRendering`，
  `cpp:51-55` 再取一半——所以模型里的星宽是**实测步进的一半**（本机 34 → 17），不是配置对子暗示的 16。
  写死 16 的那版测试期望 24、实测 34，两条断言一起红。
- **第三层：盒子选错。** 上游条目注释自己说了 "32 = 2 * [default fontsize]" 与 "-8,-8 are to compensate for the
  default scale down"，而那道缩放是 `ApplyScaleExpressionAnimation`（cpp:372-392）：焦点停在哨兵值时二次式被夹到
  **静止 0.5**，悬停向 0.8（鼠标）/1.0（触摸）抬。本运行时既无自动缩小也无可挂表达式的合成视觉，两件事都得自己写。
  墨盒 34 当格子还会让半星非线性：墨居中在盒内，裁到 25% 时一刀切在墨左边界之前，什么都不显示。
  最终**格子 = 墨盒 17**、面板只带公开的 8、条目 `RenderTransform` 缩放 0.5 且左移 8.5：
  于是 `17+8=25` 同时是排布间距与模型间距，不需要任何补偿，也就没有可被吃掉的东西；
  实测星宽 17、pitch 25、第五格 X=100、行右边界 117 = `5*17+4*8`，与指针模型逐位相符（`cpp:961/969` 同一套算术）。

仪器自己也错过一次：探针先按"捕获目标自身"量缩放，读到 3 600 像素"缩放不落墨"；换更宽祖先才读到
`x=15..44`、900 像素——**自捕获对目标自身的 RenderTransform 是瞎的**，已写进 `adaptation/06` 同名新节。

四类证据分开记：
1. 构建：`dotnet build FluentJalium.slnx -c Debug -t:Rebuild` → **0 警告 / 0 错误 / 17.25 s**。
   闸口里那次 3.79 s 的"0 警告"是空跑，真重编第一遍确实抖出一条 CS8602（可空局部变量没收敛），改完再重量才归零。
   `Manifest.txt` 64→**66** 份字典。
2. 行为：新增 `AstraRatingControlTests` 37 个方法 / **64** 例（钳位从不抛、`MaxRating` 先落自己再拉低 Value/Placeholder、
   `ValueChanged` 无增量守卫、清除被拒时落 1.0、方向键含 RTL、只读返回未处理、拖动越界清除、
   字形角色回退表、六态画刷身份、逐格裁剪分数、状态先发布后 dressing、无字典也能建）。整套 1331 → **1395**。
   **A/B 三次**：撤掉格边距 → pitch 42 且边距读回 0，两条红；退回上游那行负 `Spacing` → pitch 34 红；
   还原 → 64/64 绿。keys.md 那两条红见下。
3. 视觉：本段**没有星墨主张**，且新类里 **0 条像素用例**——不是漏，是自捕获对条目缩放没有分辨力（上段那条仪器账）。
   像素列的可信主张只有探针里实色块那两组：`ClipToBounds` 按比例（30/60→1 800、45/60→2 700、20 宽盖 512 墨→恰 320），
   `Clip` 完全不裁（3 600/3 600，markup 写法还读回 `rect=Empty`），以及 0.5 缩放经祖先读到 900 像素。
4. 硬件输入：**仍为零**（#13）。钳位、分数、放大数值都走 routed 处理器调用的内部入口
   （`TryHandleKey` / `PreviewPointerAt` / `CommitPointerAt` / `LeavePointer`），`MouseMove`→`PreviewAt` 的接线未证。

**闸口自己把清单的盲点逼了出来**：第一次整套跑两条红——`Not in the document: MUX_RatingControlDefaultFontInfo`
与 `Totals` 计数差 1。根因在**生成器**：`Report-AstraResourceKeys.ps1` 的元素名正则只吃 `[A-Za-z0-9.]`，
`<controls:FluentRatingItemFontInfo x:Key=...>` 这种带命名空间前缀的键行整条看不见，而测试那台独立的
`XDocument` 解析器照数。修的是工具不是断言：字符集放行 `:`，并给这类"对象行"补上"值就是它自己的属性"这一格口径。
1280 → **1295** 行。这正是 #40 当初要做双解析器的理由：**工具与测试口径不一致时，红的是清单而不是某个人的记性。**

**闸口读数（串行 `tools/Test-AstraGates.ps1`，管道退出 0）**：整套 **1395/1395 通过、0 失败、0 跳过**（6 m 21 s）；
调色板三档 `checked=True`；`keys.md is current: 1295 canonical lines.`；末行 `All Astra gates passed.`

不声称（同 `audits/rating-control.md` 第 12 节）：星的墨（#50 不变）；真指针/真触摸/真键（#13）；
放大在星心与地板之间那一段的取值；焦点环外观与手柄通路；`ItemInfo` 图片路径的位图显示；
高对比逐控件重指（审计第 6 节）；`RatingControlAutomationPeer` 与 `AccessibilityView`；
以及"墨正好居中在 34 的盒子里"这一条对称假定——它若错，表现为整行左右偏一点，而这里无墨可量。

## 阶段 6 第六段：图标族（SymbolIcon / FontIcon / PathIcon 与 `Symbol` 枚举）——量完之后决定"不写模板"

结论先说：**这一段没有新样式、没有新类型、没有新资源键**，出口是"把族量清楚 + 把差异钉成测试 + 把不能声称的写死"。
理由与读数都在 `audits/icon-family.md`；ROADMAP 只记改变判断的那几笔。

1. **两台仪器都错了，而且错成同一种。** 前两次"运行时没有 `Symbol` 这个类型"的读法都出自 PowerShell 5.1：
   .NET Framework 的 `LoadFrom` 装 net10.0 程序集后 `GetTypes()` 会抛，脚本里一个空 `catch` 把抛读成缺席。
   换成同进程反射（`spike/SymbolCmap`）后：类型在，764 个成员，宿主是 `Jalium.UI.Managed`
   （`Jalium.UI.Controls.dll` 导出类型数实测 0，是一具转发壳）。**"缺席"这种主张以后必须点名它数过哪些接收者。**
2. **枚举的号不是上游画的那枚字。** 上游 IDL 存旧号（`Add = 0xE109`），出图前经
   `icon.cpp:461 ConvertSymbolValueToGlyph` 换算（197 个 `case`）；这套枚举存的已经是换算后的号。
   于是"168 个名字号不同"这种读法完全是问错了问题——按**画出来的字**比：
   两侧共有的 171 个名字里 **167 个画同一枚字形**，4 个不同（`Account`/`Map`/`MapPin`/`Page`），
   另有 26 个上游名字这边没有，其中 12 个连字形号都没人承载。三份上游读数（IDL、生成镜像、设计文档表）
   互相核对为 0 冲突，才敢出这张 diff。逐枚命中另列 `s2-symbol-cmap-raw.txt`：
   旧号段在装机两字体里 197/197 有字形（所以换算是换图不是补漏），而上游自己要画的 `Target = U+F5F0`
   **在两个字体里都没有字形**——画不出东西的是上游。
3. **唯一能抄的键抄不动**：上游的 `SymbolThemeFontFamily` 是 `FontFamily` 行，三种写法（元素文本 / `Source=` /
   `FamilyName=`）造出来的全是 `Source == ""` 的空家族；换 `<x:String>` 存住了文本却喂不进 `FontFamily` 属性。
   这是"类型化令牌行"家族里第四个成员（已写进 `adaptation/00` 第 5 条），因此**不发布该键**，
   并把"这个键查不到"钉成测试——将来能承载时测试会红。
4. **判据收窄了一条**：同一张白底里 `Border` 与闭合 `PathIcon` 各印出 400 像素，纯文本 `TextBlock` 印出 0
   （`adaptation/06` 新增段）。所以"字形无墨"是**文本这条路**瞎，不是图标控件瞎；本族像素列由 `PathIcon` 承担，
   `SymbolIcon`/`FontIcon` 是否落墨在现有通路上不可证。附带量死一条新默认：`PathIcon` 不给尺寸就铺满槽位。
5. **为什么不起自有类型**：`IconElement` 系不是 `Control`（没有 `Template` 可交），派生路线实测可编译可用
   （`ProbeIcon : IconElement`，`MeasureOverride` 被调、`base` 返 0x0），但量到的成员、盒子行为
   （`SymbolIcon` 恒 20、`FontIcon` == 字号）与上游一致，唯一"没画"的怀疑恰好落在测不出来的那条路上。
   用一条测不出来的怀疑去换三个公开类型，判据不支持这笔交易。

四类证据：**构建**见下闸口读数；**行为**新增 `AstraIconFamilyTests` 32 条（类型面/成员面/枚举号与画出来的字/
markup 名字解析与未知名静默替换/盒子随不随内容/4+26 个差异的漂移闸/键不可发布的复测）；
**视觉**只有 `PathIcon` 填充那一条；**硬件输入**本族无交互面（上游亦然），未证项见审计第 7 节第 7 条。

**闸口读数（串行 `tools/Test-AstraGates.ps1`，管道退出 0）**：整套 **1427/1427 通过、0 失败、0 跳过**（6 m 18 s），
比上段 1395 恰好多 32 条＝本段新类；`0 个警告 / 0 个错误`（21 s 真编译，非 up-to-date）；
调色板三档 `checked=True`（Light/Dark 各 83 源色→101 刷，HighContrast 101 键映射）；
`keys.md is current: 1295 canonical lines.`——**行数不变正是本段的结论**：没有新资源键落地，
因为唯一可抄的 `SymbolThemeFontFamily` 被量出载不住值（`adaptation/00` 第 5 条）；末行 `All Astra gates passed.`

不声称（同 `audits/icon-family.md` 第 6 节）：`SymbolIcon`/`FontIcon` 的字形到达像素（#50：文本这条路瞎，
本段把范围从"图标字形"收窄为"文本运行"）；上游 4 个偏差名与 26 个缺席名的修复（号在运行时枚举里）；
高对比下图标前景逐控件重指；`BitmapIcon`/`ImageIcon`/`IconSource` 全家（运行时无这些类型）；
`PathIcon` 无显式尺寸即铺满槽位这一默认与上游 16x16+Uniform 的差异；本族硬件输入通路。

## 阶段 6 尾批（并行项：减动效资源键化）

出口：模板过渡时长改成上游同名的公开键、`ReduceMotion` 真的写进那些键、并把"哪种资源行载得住时长"量死。
审计 `audits/motion.md`，原始读数 `adaptation/s6-motion-probe-raw.txt`。

1. **上游的时长行在这台运行时交付的是属性的默认值，且不报错**：`<x:String>00:00:00.083</x:String>` 存成字符串、
   四种消费形状（属性 `{ThemeResource}` / `{StaticResource}` / 隐式 Setter / **ControlTemplate 内部**）一律读回
   **180ms**——`UIElement.TransitionDuration` 自己的默认值。同一位置换成无限定名的 `<Duration>` 行，四种形状全部
   读回 83ms。所以本段的做法是**名字与数字逐字照抄上游，只把行类型换成框架类型**。这条同时把上一段留下的过度概括
   改回来：`adaptation/00` 新增第 6 条，分界是"框架类型 + 无限定名"（`CornerRadius`/`Thickness`/`Duration` 可行），
   不是"类型化令牌行都不行"（`x:Double`、`FontFamily` 才是不行那族）。
2. **减动效的机制框架已经给了一半**：`Application` 静态构造里把所有自动过渡挂在
   `SystemParameters.ClientAreaAnimation && UIEffects` 上（internal，反射能碰但不该碰——AGENTS.md 禁），
   而 `TransitionDuration` 是**过渡要启动那一刻现读**，`<= 0` 或无 `TimeSpan` 就直接不起动画。
   于是应用内自服开关（Gallery 设置页那个）的正确形状是"把键写成 0"，不是走视觉树——
   探针 [3b] 量到：写 0 之后 4 帧内，一个**早已实例化**的模板部件从 83ms 变 0ms；[4] 量到翻转前后
   中途读数 `#DC0091`（1600/1600 像素在两端之间）与 `#0000FF`（1600 全终色、0 在中间）。
3. **落地面**：新增 `ThemeResources/Motion.jalxaml`（3 行 `<Duration>`）进清单；样式里 21 处字面时长全改读键
   （16×`ControlFasterAnimationDuration`、4×`ControlFastAnimationDuration`、1×`SplitViewPaneAnimationOpenDuration`
   ——`Navigation.jalxaml` 那处 `0.200` 是 `PART_PaneRoot` 的宽度，对照上游正是 SplitView 面板开合那条，不是自造名）；
   `FluentThemeManager` 持字典实例，`ReduceMotion` 逐键写 0/回写设计值，设计值**只从已加载的行读回**（代码里不留第二份数），
   一行都读不回 `Duration` 就启动抛。Motion 字典独立于 Light/Dark，切主题不会把动画还回去（有测试钉）。
4. **只发布被消费的 3 条**：上游另有 10 条时长行与 1 条 spline，都不抄——`ThemeResources` 与键消费闸口一起管，
   空转行会被判红；spline 更是无处可落（本运行时过渡时序是 `TransitionTimingFunction` 枚举）。键清单因此
   1295→1298，恰为本段三行。
5. **探针自撤一次**：首轮把"探针用错 assembly 名"当成"运行时写不出这个类型"——`Jalium.UI.Core` 与
   `Jalium.UI.Managed` 两种限定拼写都解不出 `Duration`（图标族那笔转发壳的账在这里复发），补量之后才知道
   **只有框架自己 xmlns 下的无限定名可行**。顺带量死两条：`DurationConverter` 吃 `"0"` 但 `"Auto"` 抛
   `FormatException`；被丢弃的行（`Automatic`）与刻意归零的行（`00:00:00`）从属性上可辨，所以"减动效生效了"
   与"键掉了"不会互相冒充。

四类证据：**构建**见下闸口读数；**行为**新增 `AstraMotionTests` 9 条（三键逐条读毫秒、行形状与字面文本的转录闸、
已实现模板三部件各读自身 `TransitionDuration`、翻 `ReduceMotion` 后同一要素 83ms→0ms、减动效期间新建的控件生下来即静、
切主题不还回动画、21 处过渡声明逐处要求读已发布键——出现字面量即判红）。**牙的验证**：摘掉 setter 里的写键调用后重跑，
正好这 3 条红、其余 6 条绿。**视觉**只有探针 [4] 那三行中途色读数（套件不断言动画在跑，理由见下）。
**硬件输入**本段不动输入路径。Gallery 设置页"Reduce motion"那张卡的文案按实测覆盖面改写（含"已在跑的过渡走完自己的时钟"）。

**闸口读数（串行 `tools/Test-AstraGates.ps1`，管道退出 0）**：整套 **1436/1436 通过、0 失败、0 跳过**（6 m 16 s），
比上段 1427 多 9 条＝本段新类；`0 个警告 / 0 个错误`（3.77 s，含 Gallery 重编）；调色板三档 `checked=True`
（Light/Dark 各 83 源色→101 刷，HighContrast 101 键映射）；`keys.md is current: 1298 canonical lines.`；
末行 `All Astra gates passed.`

不声称（同 `audits/motion.md` 第 5 节）：动画确实在跑这件事不由套件断言（依赖机器 `ClientAreaAnimation`/`UIEffects`，
探针那次两档皆 True 并已记录）；已在跑的过渡被中途翻转 `ReduceMotion` 打断；上游其余 10 条时长键与 spline；
真指针 hover 的"第 N 帧落在哪个色"（与 #13 同一笔账）；`ProgressRing` 自转是否该受减动效影响（上游未证）；
Gallery 的 Motion 系统页仍未建（#10 剩余部分）。


## 缺陷批：全库前景继承审计（结清 #51，以及 #55 的前景一半）

结清 `adaptation/00` 第 4 条留下的那句"未审的相邻面"：把这一层写前景的形状列成清单逐条问"到不到"，而不是再逐控件猜。
读数与仪器纠错在 `audits/foreground.md`，原始日志 `adaptation/s6-foreground-probe-raw.txt`，闸口
`tests/FluentJalium.Tests/AstraForegroundAuditTests.cs`（14 条）。

量到的事实：

1. 这一层写前景有三种形状，而之前只数过第一种——attribute 15 行（9 个令牌键 + 6 条 `{TemplateBinding}`）、
   `Setter Property="Foreground"` **228** 行、外加 `DataTemplate` 里 1 条无名行（评级未选星，前两种的扫描漏它）。
2. **无一死键、无一处不到达**：两档主题各把 243 条点名的键解析一遍，全是 `SolidColorBrush`；可达的具名声明逐条比对
   部件自身前景全对；42 个可读文本节点全部随 Light↔Dark 翻色。InfoBadge 那类哑色在本层是孤例，且已经修掉。
3. **低对比只剩 5 条且与上游同源**：评级满星用强调色（Light 4.08 / Dark 9.06），那是强调色文本在浅档页面上的固有值。
4. 钉住两对容易改坏的组合：InfoBar 严重度标记 `#FFFFFF` 画在**兄弟** `Ellipse.IconBackground` 的 `#0078D4` 圆盘上
   （4.53 / 11.67），评级未选星 Light `#9E000000`、Dark `#C5FFFFFF`（上游 61% 黑 / 77% 白）——断言连 alpha 一起钉。
5. 框架事实（省掉后面每一族一次试验）：`Control`、`TextBlock`、`TextElement` 的 `ForegroundProperty` 是**同一个对象**
   （`DependencyProperty.AddOwner` 直接 `return this`，`Jalium.UI.Core/DependencyProperty.cs:397-416`）且
   `inherits: true`，所以一次翻主题能同时判全库，"读部件自己的属性"也不必在两个 DP 之间挑。
6. 仪器自己错了三次，每次都产出一条假缺陷：`ReadLocalValue` 看不见 markup 写；沿祖先链找背景看不见兄弟图元、且共用
   宿主会把别的被测控件当成"前面的兄弟"；打印色不带 alpha 把 61% 黑读成纯黑。第三次顺带揭出"全库只有 16 处写前景"
   这句是错的——那只是 attribute 形状。

四类证据：**构建**见下闸口读数；**行为**新增 14 条（两档键解析、attribute 键清单 + TemplateBinding 计数、InfoBar
三部件两档读数、6 族文本节点随主题翻转、未选星 alpha、标记对圆盘的对比下限）；**视觉**本段没有像素断言（#50 未结，
文本拿不到墨），色身份由树上读数与合成对比承担；**硬件输入**本段不动输入路径。

**牙的验证（A/B）**：把 `Styles/Surfaces.jalxaml:170` 的 `InfoBarTitleForeground` 改成不存在的键，重建后跑这 14 条 →
**3 红**（解析闸 Light/Dark 两档 + 清单闸）、11 绿；改回 → 14/14 绿，该文件 `git diff` 为空。这次同时量出闸口的灵敏
边界：**"读部件自身前景"那条保持绿**，因为标题的令牌色与继承墨本来就同色——只有令牌≠继承色的配对（`IconGlyph`
白对继承黑、InfoBadge 值对强调底）才由读数发现死键，所以真正灵敏的是"键必须解析成刷"那道，读数断言是第二道。

**闸口读数（串行 `tools/Test-AstraGates.ps1`，管道退出 0）**：整套 **1450/1450 通过、0 失败、0 跳过**（6 m 16 s），
比上段 1436 多 14 条＝本段新类；`0 个警告 / 0 个错误`（3.57 s，含 Gallery 重编）；调色板三档 `checked=True`
（Light/Dark 各 83 源色→101 刷，HighContrast 101 键映射，另有 3 个上游键因 palette 无对应条目而按记录扣住）；
`keys.md is current: 1298 canonical lines.`；末行 `All Astra gates passed.`
本段翻主题的那 14 条跑在既有主题套件之间没有把任何一条邻居断言带红（整套 0 失败），所以"审计批会污染主题态"这条
担忧在本次读数下不成立。

不声称：探针没有 subject 行、因此**未测到达与否**的 7 个 owner（`AppBarToggleButton`、`DataGridCell`、
`DataGridColumnHeader`、`MenuBarItem`、`MenuFlyoutItem`、`MenuFlyoutSubItem`、`ToggleMenuFlyoutItem`），以及 5 处
树上取不到的具名声明（TeachingTip 标题/副标题、`FluentTabViewItem.IconHost`、
`FluentBreadcrumbBarItem.PART_ChevronTextBlock`、`ComboBox.PART_ScrollViewer`）——它们只被"键必解析"覆盖；
兄弟图元判底用的是"重叠布局面板 + 尺寸同阶"的启发式（本运行时没有可用的跨要素变换读数）；高对比档下这些前景走不走
映射本段没测；228 条状态 Setter 只测了键能不能解析，没测"该状态下那一格真被画上"；#46 五处 1 DIP `Rectangle`
无墨那条**已结**——见下面"缺陷批：五处 1 DIP 分隔线换形状"一段，那一批同时补上了这条主张该用的判据（折叠后逐键减量）。

## 缺陷批：五处 1 DIP 分隔线换形状（结清 #46，并新立 #58）

用户可见的那类缺陷里最安静的一种：命令栏分隔线与两张表格的表头底线**本来就不在屏幕上**。§S1-q 第 2 条已经量死运行时
事实（1 DIP `Rectangle` 在任何对齐下印 0 墨，同盒子 `Border` 印 600，`Rectangle` 要 4 DIP 才恢复），这批把那条事实
落到**已发布的五处站点**，并且把"这条线到了像素"这句主张改成能证的形式。改动：`Styles/AppBar.jalxaml` 的
`SeparatorRectangle`、`Styles/DataGrid.jalxaml:53/59/60`、`Styles/TreeDataGrid.jalxaml:52` 五条 1 DIP `Rectangle`
换成同盒子、同令牌、同对齐的 `Border`（`Fill`→`Background`，`RadiusX/Y=0.5`→`CornerRadius=0.5`）；部件名照上游，
上游没有名的三条不新起名字。新测试 `tests/FluentJalium.Tests/AstraOneDipRulesTests.cs` 7 条。

1. **五处站点逐处量到墨，两档主题都量**。同一份 witness、同一张随主题的卡：表格两例 Light/Dark 各 **338** 像素，
   应用栏两例各 **64**。64 是可解释的：线在 48 格内去掉 2,8,2,8 边距是 32 DIP 高，1 DIP 宽在这台缩放跨两个设备列，
   所以墨是 32x2；`Border` 会印，但"1 DIP"不等于"1 列"。
2. **判据换了一次，而且第一次是错的**。第一版数"改前的颜色在改后彻底不见"，表格线只读到 **2 与 3** 像素——而它
   其实每次都印出 338 格：混出来的 `#9F9F9F` 在同一张裁剪图里也被字形反锯齿产出，那个键从来没"消失"过。改成
   **逐键减量** `sum(max(0, with-without))` 之后同一份 markup 读到 338。这条与 §S1-m/§S1-r"几何计数不是令牌证据"同族，
   补的是另一半：**同一个混色键可以有多个来源，所以差分只能按减量算，不能按有无算**。
3. **witness 必须自带表面**。上游表头带是透明的（`DataGridColumnHeaderBackground → SubtleFillColorTransparentBrush`），
   暗色第一次跑读到 0 且 with/without 逐键相同——不是"暗色里线又不印"，是 §S1-q 第 4 条那个坑在 witness 侧复现：
   半透明白线画在白上等于没画。给每条主张配一张卡（白 / `#202020`）之后两档同数。
4. **折叠必须整族一起、且按声明盒子认部件**。DataGrid 三条线共享同一个 `#29000000`，只折一条时那个键由另外两条
   供给、减量读回 0；找部件用 markup 写的 `Width/Height=1`，不用实测几何——滚动条自己的轨道也满足"某边实际是 1"。
5. **路线被从源头闸死**：`No_shipped_template_draws_a_one_dip_rectangle` 扫 `src/FluentJalium` 全部 `.jalxaml`，
   出现任何 1 DIP `Rectangle` 即红。钉的是路线不是当下的数量（将来运行时能画薄 `Rectangle` 也不改这条判断，因为
   `Border` 是实测会印墨的那个形状）。
6. **顺手量到的一条"新缺陷"当天就被自己推翻**（记在这里，不悄悄删掉）：那轮读数写成"暗色下挂载的表格族那片表面
   仍是白的"并立了 #58。复查量到的其实是仪器的读法——`PixelAt`/`Sample` 走 `Bgr32`、不读 alpha 字节，裸捕获里那一片
   "白"是一个部分透明像素的 RGB；给同一张网格一张不透明底之后，采样点读回的就是令牌的正牌合成（`#202020` 底
   `#2B2B2B`、洋红底 `#FF0DFF`）。这条判据与撤回的完整账目在 `adaptation/06` 最后一节，钉它的用例是
   `AstraDataGridTests.A_dark_surface_token_only_reads_true_over_a_matching_backdrop`（把我们的 `Background` setter
   改成 `{x:Null}` 即红，消息点名 `reads #202020 … not the card token's composite #2B2B2B`）。**跨批后果**：任何
   暗色腿用半透明令牌做的像素主张，样本必须自带不透明底，否则测的是丢掉 alpha 之后的 RGB；本仓库既有那批
   "数非背景像素 / 比两档 Top(2) 不同"的暗色断言按这条读法看**证明力为零**，另立一批补（见下面待办）。

四类证据：**构建**——闸口里那次真重编（52.5 s，三个工程重出 dll）**0 警告 / 0 错误**；本批其余轮次跑的是 Release
增量构建，只看到"0 个错误"，警告数不单独主张（没有重编就没有警告读数）。**行为**——`AstraAppBarTests` 里
`SeparatorRectangle` 的类型断言按上游偏离改成 `Border`（这条是本批唯一被改的既有断言，它同时是"名字保留、形状换掉"的
见证）。**视觉**——上面 1..3 的六个数；本轮另有一条流程账：一次 `dotnet build` 失败后用 `;` 串起来的
`dotnet test --no-build` 跑了旧 dll，打印"通过: 4"看着像绿读数，实际那次构建根本没成功（此后每条命令用 `&&` 串联，
且先看到"0 个错误"再读测试数）。**硬件输入**——本批不动输入路径；#13 照欠。

**牙的验证（A/B）**：三份 markup 退回 HEAD 的 `Rectangle`、测试与卡都不动 → **7 红 / 0 绿**，六条 witness 读数
**一律 0**，且每条的 with/without 直方图逐键相同（折叠那条线对屏幕没有任何影响）；markup 闸那条读
`Assert.Empty() Failure: Collection was not empty`。换回 `Border` → 7/7 绿；四个受影响类
（OneDipRules / AppBar / DataGrid / TreeDataGrid）合跑 **175/175 绿**。第一轮用错判据时同一份退回 markup 读到的是
2 与 3 而不是 0——那也是"有牙"，但牙口弱到会把"印不出"读成"几乎印不出"，所以第 2 条的判据换形是这条主张的一部分。

**闸口读数**（串行 `tools/Test-AstraGates.ps1`，管道退出 **0**）：整套 **1457/1457 通过、0 失败、0 跳过**（6 m 25 s），
比上段 1450 多 7 条＝本批 7 条（其中三条 fact 在加主题轴后变成 theory，所以本批内部净 +3）；调色板三档 `checked=True`
（Light/Dark 各 83 源色→101 刷，HighContrast 101 键映射 + 3 条按住）；`keys.md is current: 1298 canonical lines.`
（键数不变——本批只换形状，不发布也不撤销任何键）；末行 `All Astra gates passed.`。

不声称：这六条主张全是离屏 `Render()` 合成，真窗口里的命令栏与表格表面没有截屏账（#10、#21 照欠），高对比档下
这五条线走什么颜色本批没测（#57 照欠）；`CornerRadius=0.5` 与上游 `RadiusX/Y=0.5` 只在"同一个 1 DIP 盒子"这一级对齐，
半设备圆角本身在 1 DIP 上不可见、没单独量；`MenuFlyoutSeparator`、`ScrollBar` 等**同族但不同形状**的线没有在本批
被扫进主张（markup 闸只认 1 DIP `Rectangle`，别的薄写法不在其列）；#58 那条"暗色表面是白的"已经**撤回**（成因见上面
第 6 条：读法不读 alpha），但由此暴露的"既有暗色像素断言证明力为零"这笔账还没补（新立一条，见 ROADMAP 待办）；
`Styles/Divider.jalxaml` 那条"运行时默认线色从哪来"照旧未查。

## 复批判：撤回 #58，把"暗色半透明表面"记成仪器账（2026-09-21，同日）

上一批顺手立的那条"暗色下表格族那片表面仍是白的"当天就被推翻，而且推翻它的方式本身就是这笔账的内容：
不是产品变了，是**读法**。`PixelHarness` 的捕获走 `RenderTargetBitmap(Bgr32)`，只取三个颜色字节，所以一个
"背后什么都没有"的像素读回来是它自己的 RGB——暗色卡片令牌 `#0DFFFFFF` 在裸捕获里报 `#FFFFFF`，看上去就像"画了白"。
三点实测（新用例 `AstraDataGridTests.A_dark_surface_token_only_reads_true_over_a_matching_backdrop`）：

1. 同一张暗色网格，给一张 `#202020` 的页底后采样点读 `#2B2B2B`，正是 `32 + (255-32)*13/255` 的 source-over 结果：合成没坏。
2. 再给一张**洋红**底（`#FF00FF`）读 `#FF0DFF`。这条不是重复第 1 条——白叠灰还是灰，只有非灰底能把"alpha 被丢掉"和
   "真按 source-over 合成"分开。断言还加了第三条腿：两张底读数必须不同，否则等于什么都没叠。
3. 把我们的 `<Setter Property="Background" Value="{ThemeResource DataGridBackground}" />` 改成 `{x:Null}`（先确认改动落进
   文件，再重建）→ 该用例红，消息直接点名 `the grid surface reads #202020 over a #202020 page, not the card token's
   composite #2B2B2B (token #0DFFFFFF)`；同一采样点在裸捕获里从 `#FFFFFF` 变成 `#000000`。改回 → 三个相关类合跑
   **75/75 绿**，`0 警告 / 0 错误`。

跨批后果（立新账 #59）：任何"暗色腿 + 半透明令牌"的像素主张，样本必须自带不透明底并把期望算成 source-over 值；
本仓库既有那批"数非背景像素""比两档 `Top(2)` 不同"的暗色断言，按这条读法**证不到"半透明表面落到了像素"**
（"背后什么都没有"与"正好合成出该颜色"这两种情况给出同类读数），逐个补底与补算值另起一批。账目与判据落在 `adaptation/06` 最后一节，§S1-s 第 3 条与 `s1s-one-dip-rules-raw.txt`
里的旧表述都已就地改成"撤回"（原文不删，改在原地写清谁被推翻）。#58 结清为仪器账，不是产品缺陷。

四类证据：**构建**见下闸口读数；**行为**新用例一条（读令牌 → 两张底 → 三点断言）；**视觉**即上面 1、2 两个混色值；
**硬件输入**本批不动输入路径。

**闸口读数**（串行 `tools/Test-AstraGates.ps1`，管道退出 **0**）：整套 **1458/1458 通过、0 失败、0 跳过**（6 m 25 s），
比上一批 1457 多 1 条＝本批复判的新用例；Debug 真重编 `0 个警告 / 0 个错误`；调色板三档 `checked=True`；
`keys.md is current: 1298 canonical lines.`（键不变）；末行 `All Astra gates passed.`。

## 复批判 第二步：暗色像素断言配底——三条改判，其中一条是**认错了人**（2026-09-21，#59 开工）

上一批立下的判据（"暗色腿 + 半透明令牌"的像素主张必须自带不透明底、期望要算成 source-over 值）先落到仪器上：
`PixelHarness` 新增 `Backdrop(subject, plate)`（给主体配一张不透明页底并**由包裹层往下成像**）与 `Over(plate, ink)`
（按渲染器的整数量化算 source-over，`round(back + (front-back)*A)`），两条都带"为什么存在"的注释并指向 `adaptation/06`。
本批把三条既有主张改成能证伪的形状。

1. **表格族两档重绘**（`AstraDataGridTests.The_grid_repaints_between_the_two_themes`、
   `AstraTreeDataGridTests.The_tree_repaints_between_the_two_themes`）。老断言是"两档 `Top(2)` 不同 +
   `PaintedPixels > 1_000`"——按新读法这证不到"表面令牌落到了像素"（一片没画的透明与正好合成出该颜色给同类读数）。
   现在两档各自配底（亮 `#F3F3F3`、暗 `#202020`），期望值**从树上读到的令牌算出来**而不是点名常量：
   `SurfaceOver(plate) = Over(plate, DataGridBackground)`，两档都要 `> 2_000` 格，再加一条跨档腿
   `dark.Count(lightInk) == 0`。合成值本身：亮档 `#B3FFFFFF` 叠 `#F3F3F3` → `#FBFBFB`，暗档 `#0DFFFFFF` 叠
   `#202020` → `#2B2B2B`（与上一批的实测一致）。
2. **滚动条滑块**（`AstraPixelTests.The_scroll_bar_thumb_follows_the_theme_and_shows_no_brand_emerald`）。
   这一条的问题比"没配底"更糟：**它一直在数别人的墨**。暗色下 `ScrollBarThumb = #8BFFFFFF`、
   `ScrollBarTrack = #0FFFFFFF`，两支 RGB 都是白，所以老断言 `dark.Count(Colors.White) > 48` 数的是**轨道**。
   把轨道钉成不透明洋红诱饵（`OverrideBrush("ScrollBarTrack", #FF00FF)`）之后白色整个消失，滑块的墨出现在它真正
   落在的地方——洋红之上：`Over(#FF00FF, #8BFFFFFF) = #FF8BFF`，实测 **64 格**。断言改成三腿：诱饵色自身
   `> 48`（先证明钉底到达，否则下面的读数什么都不是）、`#FF8BFF > 48`、亮档同键 0。跨批判据：**断"某个半透明令牌
   到了像素"之前，要先回答"这张图里还有谁带同一组 RGB"**——把别人钉成诱饵或给自己配底，然后断合成值而不是颜色名。
3. 量这条时顺手撞开 `RenderPart<T>` 的边界：它把部件单独成像，**祖先不进裁剪图**，所以包在它外面的页底既不产生
   `#202020` 也不产生任何合成值（读数仍是 `#000000/#FFFFFF/#D2D2D2`）。"带底合成"只认 `Render(wrapper)` /
   `PixelAt(wrapper,…)`；`RenderPart` 只配得上"这个部件自己画没画"这类不依赖背衬的主张（记 `adaptation/06` 第 5 条）。

四类证据：**构建**见下闸口读数；**行为**三条主张改判后各自带读数消息（点名 plate、令牌、算出的合成值与实测格数）；
**视觉**即上面 `#FBFBFB`/`#2B2B2B` `> 2_000` 格与 `#FF8BFF` 64 格；**硬件输入**本批不动输入路径。

**牙的验证（A/B）**：两条突变各撞一条腿，一次跑完（先存 A/B 前的 `git diff` 快照）。把 `OnPlate` 的页底改成 alpha=0，
其余一律不动（`Over()` 仍按同一算式给期望值）→ 表格与树两条各红，消息点名 `ink=#FFFBFBFB count=0`，而同一裁剪图的众数
变成 `#FFFFFF` 51 908 格——正是"没有底时半透明白报自己的 RGB"这条仪器账本身。另一条只把 `ScrollBarThumb` 钉成不透明绿、
诱饵腿不动 → 滑块用例红，`ink=#FFFF8BFF count=0`，同时那 **64 格**从 `#FF8BFF` 整块搬到 `#00FF00`
（`top=#FF00FFx144 #00FF00x64`）：被数的确实是滑块自己的那支刷子，不是这片布局里任何别的东西。三条改回 → 三个类合跑
**82/82 绿**（23 s），改回后的 `git diff` 与 A/B 前快照逐字节相同（脚本读回 `REVERT_EXACT`），Debug 真重编 `0 警告 / 0 错误`。

**闸口读数**（串行 `tools/Test-AstraGates.ps1`，管道退出 **0**）：整套 **1458/1458 通过、0 失败、0 跳过**（6 m 27 s）——
本批只把三条既有断言改判，不新增用例，所以条数与上一批持平；`0 个警告 / 0 个错误`（14.31 s，含 Gallery 重编）；
调色板三档 `checked=True`（Light/Dark 各 83 源色→101 刷，HighContrast 101 键映射 + 3 条按住）；
`keys.md is current: 1298 canonical lines.`（键不变，本批不动资源只动判据）；末行 `All Astra gates passed.`。
闸口跑在改判后的树上，A/B 之后的工作副本与它逐字节相同，所以这笔读数对最终状态成立。

不声称：**同族还有 11 条暗色/两档像素主张没有补底**，本批只结清最吃重的三条，其余按同一配方另起一批——
`AstraMenuTests.cs:908`、`AstraTreeViewTests.cs:579`、`AstraAppBarTests.cs:889`、`AstraSplitButtonTests.cs:381`、
`AstraSplitButtonTests.cs:475`、`AstraToggleButtonTests.cs:395`、`AstraExpanderInfoBarTests.cs:361`、
`AstraExpanderInfoBarTests.cs:607` 八处仍是 `PaintedPixels > 0` 两档对；`AstraComboBoxTests.cs:652`、
`AstraAutoSuggestBoxTests.cs:519`、`AstraNumberBoxTests.cs:428` 三处仍是"`Top(4)` 不同 + `DistinctColors > 1`"。
这 11 条按现读法只能证"这块区域有东西且两档不同"，证不到"某个半透明表面令牌落到了像素"；
另外本批全是离屏 `Render()` 合成，真窗口里没有配底账（#10、#21 照欠），高对比档下这三条走什么色也没测（#57 照欠）；
`ScrollBarTrack` 诱饵只量了暗色腿，轨道自身在亮色下的墨没测；`Over()` 的整数量化按"每通道一次 round"拟合，
遇到两层以上半透明叠加时未与渲染器对齐验证过。

## 复批判 第三步：余下 11 条两档像素断言一次改判（2026-09-21，#60 结清）

上一批"不声称"里点名的 11 条，本批按同一配方全部改判。做法是先做一次**统一测量**：临时探针（`AstraPlateProbeTests`，
读数抄进本节、文件随即删除，不留在仓库里）把这 11 个主体各配一张不透明页底、两档各成像一次，报告"建好之后树上到底带着
哪些背景刷"，再按每支刷子的合成值数格子。测出来的东西决定了每条该换成什么形状：

1. **9 条换成"自己的刷子合成到了页底上"**（新 `PixelHarness.AssertSurfaceLands`）：主体包在不透明页底里
   （亮 `#F3F3F3`、暗 `#202020`，两把常量进了 harness：`LightPage/DarkPage`），刷子**从被建好的那个部件自己读**
   （`Self`，或按部件名取），期望 = `Over(plate, brush)`，两档各断"落格数 ≥ 地板"，再加 `NotEqual(lightInk, darkInk)`。
   实测落格数（亮/暗）：SplitButton 6212/6216、DropDownButton 4686/4686、ToggleButton 8296/8296、ComboBox 6480/6480、
   AutoCompleteBox 7468/7468、NumberBox 6248/6248、Expander 头 5656/5660、InfoBar 底 20420/20428、树内容边 7648/7660。
   地板一律取实测的一半以下（2_000 / 8_000 / 1_000），不是挑出来的整数。
2. **1 条换成反主张**（新 `AssertNoSurfaceLands`）：`MenuFlyoutSubItem` 两档**一棵背景刷都没有**，9120 格里被盖掉的只有
   38 格（子菜单箭头那一点）。上游本就把菜单项底给 `SubtleFillColorTransparent`，所以这里唯一能证伪的像素主张是
   "它不该盖住任何页底"——探针两档各确认一次（0 支刷、页底保留 ≥99%）。
3. **1 条量出它压根不是"表面"主张**：`AppBarToggleButton` 带 `IsChecked=true` 时，树上唯一带墨的是
   `AppBarButtonInnerBorder` 的**勾选底**（亮 `#0078D4`、暗 `#60CDFF`，各 3080 格，不透明），控件自己的底两档都是透明的。
   这条因此改判成"勾选底落到了像素"，断言消息与用例内的注释都写清是勾选态，不再冒充"整个族跟着主题走了表面"。

三件顺带量死的仪器账（写进 `adaptation/06` 同一节末尾）：本运行时一棵树里有**多个同名 `ContentBorder`**（每行一个 +
控件自己一个），按名取部件的 `Named()` 会先取到全透明那支（`#00000000`），使合成值恰好等于页底、被"不可证伪"那道守卫当场
判死——因此加了只认"背景刷 alpha>0"的 `NamedSurface`；`ContentPresenter` **没有 `Background` 成员**（编译期 CS1061），
树上取背景刷只能走 Border/Panel/Control/Shape 四条路；InfoBar 那片曾被记成"硬编码 severity 底"的其实是 `#80F6F6F6`，
50% 灰而不是不透明常量。

四类证据：**构建**见下闸口读数；**行为**11 条主张换形状（9 条合成落格 + 1 条反主张 + 1 条换主语）；**视觉**即上面
9×2 组实测落格数、38/9120 那一条与 appbar 的 3080 格；**硬件输入**本批不动输入路径。

**牙的验证（A/B）**：一条突变照全库——把 `AssertSurfaceLands` 的期望从 `Over(plate, brush)` 换成 `brush` 本身
（等于宣布"配不配底无所谓"），重建后跑这 9 个类 → **9 红 / 530 绿**，每条消息点名自己的部件、刷子与页底，例如
`implicit/ControlTemplate@200x44 carries #B3FFFFFF which composites to #B3FFFFFF over #FFF3F3F3, but that colour lands
0 pixels, under the floor of 2000. top=#FBFBFBx8296…`；改回后 `git diff` 与 A/B 前快照逐字节相同（`REVERT_EXACT`）。
这条突变**照不到第 10 条**：appbar 那支是不透明的 `#0078D4`，合成值恰好等于刷子本身，所以它不红——它的证明力来自
"点名部件 + 点名刷子"，不来自配底。第 11 条（菜单反主张）由"合成值==页底"这道守卫与"页底保留 ≥99%"这条地板负责。

**闸口读数**（串行 `tools/Test-AstraGates.ps1`，管道退出 **0**）：整套 **1458/1458 通过、0 失败、0 跳过**（6 m 49 s）——
本批只换 11 条既有断言的形状，不新增也不删除用例，条数与前两批持平；`0 个警告 / 0 个错误`；调色板三档 `checked=True`
（Light/Dark 各 83 源色→101 刷，HighContrast 101 键映射 + 3 条按住）；`keys.md is current: 1298 canonical lines.`
（键不变，本批不动资源）；末行 `All Astra gates passed.`。闸口跑在 A/B 改回之后的树上（改回经 `git diff` 逐字节核对），
所以这笔读数对最终状态成立。

不声称：**appbar 那条不在本批 A/B 的覆盖范围里**（上面写明的原因），它目前只到"点名部件 + 点名刷子 + 数格 3080"这一级；
9 条合成落格断言都断"≥ 地板"而不是"恰好这些格"，地板取实测的一半以下，因此布局漂移（行高、内边距变了）不会立刻被察觉；
`PART_HeaderBorder`（Expander）与 `RootBorder`（InfoBar）仍按 `Named()` 取，"这个名字在树里唯一"是从探针报告读出来的，
没有另做标记审计，只有树的 `ContentBorder` 换成了按 alpha>0 过滤的 `NamedSurface()`；树那 7648/7660 格是同色若干部件
（每行一支 + 控件自己一支）的**总和**，主张只到"有这样一支刷落到了像素"，不到"这一格是行还是树"；
`SurfaceBrushes` 报出的第二支刷（`BottomEdge` 的描边、`SecondaryButton` 的次级底、滑块 `Thumb`）本批没有纳入主张；
高对比档下这 11 条走什么色仍未测（#57 照欠），真窗口/上屏路径没有配底账（#10、#21 照欠）；
菜单项那 38 格是子菜单箭头的抗锯齿墨，本批没把"箭头该占几格"写成主张（#50 文本字形判据缺口照旧）；
承担本批测量的 `AstraPlateProbeTests` 是一次性探针，读数抄进本节与 `adaptation/06` 之后文件已删除，
这些数字因此没有可重跑的守卫——要复用先做成用例。

## 测试基座批：高对比 101 键逐键断言（结清 #57 的"签入表 + 到达"，并量出这台主机没有平台色）

目标里那条"高对比逐键断言"原本已有两半：`HighContrast.map` 是签入表（`tools/Sync-AstraPalette.ps1` 从上游
`Common_themeresources_any.xaml` 的 HighContrast 分支生成），`High_contrast_mapping_covers_every_palette_brush`
断言"每个调色板键都有行"，`High_contrast_maps_semantic_roles_to_system_colors` 只读其中 **4** 个键的值。
本批补上逐键那一半（`Every_high_contrast_row_drives_its_own_brush`，+1 条用例）：先把 101 个键在 Light 下各读一次存表，
翻到 High Contrast 后**逐键**比对"该键自己的刷 == 该行点名的系统色"，再要求这次翻主题真地把调色板 moved 了
≥70 个（实测 90/101；剩下 11 个在亮档就已经等于它的高对比值，逐个是谁/为什么没审）。
读数：**101 行全部驱动了自己的刷**，一条不错。测试侧的系统色用运行时真实成员名
（`ControlColor`/`ControlTextColor`/`HotTrackColor` 对应 WinUI 命名的 ButtonFace/ButtonText/Hotlight），
未知行名一律抛错而不是跳过。

**A/B 第一次是无效的，而且它自己就是一条读数。** 先把测试解析器里 `ButtonFace↔ButtonText` 两条腿互换 → 用例照绿。
换掉不等于改动了期望值：这道红需要第二条突变才出来——把 `ButtonFace` 那条钉成 `#112233` → **红，消息点名 22 个键**
（`ControlFillColorDefaultBrush names SystemColorButtonFaceColor = #FF112233 but the brush is #FFFF00FF`）。
两件事因此被量死：其一，这条断言确有牙（一条不可能的期望值立刻 22 红）；其二，**这个无头测试主机里
`SystemColors.ControlColor` 与 `SystemColors.ControlTextColor` 解析成同一个颜色，而那片刷在 High Contrast 下读回
`#FF00FF`**——也就是这台主机没有把平台系统色填进来，未设置的槽就是洋红。
后果写进了用例的 remarks：**这条逐键断言钉的是"哪个键接到哪个系统色槽"的接线，不是用户最终看见的颜色值**；
"高对比下窗口文字该是白"这种值主张必须拿真平台色，本套件做不到（与 #13 同一条欠账）。

四类证据：**构建**见下闸口读数；**行为**+1 条逐键用例，含"翻主题真动了调色板"的到达腿；**视觉**本批没有像素断言
（高对比档下没有任何一条用例拿到过墨——#50 未结，且现在多一条已知限制：主机连平台系统色都没有）；
**硬件输入**本批不动输入路径。

**闸口读数**（串行 `tools/Test-AstraGates.ps1`，管道退出 **0**）：整套 **1459/1459 通过、0 失败、0 跳过**（6 m 47 s），
比上一批 1458 多 1 条＝本批新用例；`0 个警告 / 0 个错误`；调色板三档 `checked=True`（Light/Dark 各 83 源色→101 刷，
HighContrast 101 键映射 + 3 条按住）；`keys.md is current: 1298 canonical lines.`（键不变）；末行 `All Astra gates passed.`。

不声称：上面写的"101 行全部驱动了自己的刷"只在**这台无头主机的系统色状态下**成立，洋红槽意味着值层未被检验；
剩下 11 个"翻档不动"的键没逐个查是谁、该不该动；测试侧解析器与产品侧 `FluentThemeManager.SystemColor` 是**两处**
独立声明（这正是它能抓错的原因，但也意味着两边同时错会静默）；`SystemColors.*` 在无头主机取到什么，本批只量到
ButtonFace/ButtonText 这一对相同，其余 7 个槽没逐个比对；控件级高对比视觉态覆盖（上游每条 HighContrast 分支里的
VisualState 改写）照旧不在范围内，签入表也只到调色板这一层。

## 判据批：把 #50"文本拿不到墨"从主张改成量死并设防（2026-09-21）

#50 一直写着"文本字形在任何捕获通路都拿不到墨"，而它当初的证据是"直方图里没有字该有的那种颜色"。上一批在菜单项的
裸裁剪里读到 38 格非页底墨（当时顺手记成箭头），这与 #50 冲突，所以拿新仪器重判一次。仪器换成了**同主体带字 / 不带字
的差分**——只有两次直方图不同才叫"字出了墨"，这样就不必先知道裁剪里还有什么。新用例 `AstraTextInkTests`（3 条 × 两档
= 6 条 theory）。

1. **字形确实为 0**：`Button{Content="Hello World"}` 与 `Button{Content=""}` 在亮/暗 × 白底/暗底/裸捕获下读数逐格相同
   （亮档表面 8296 + 描边 456 + 12 + 8）；`MenuFlyoutSubItem` 带不带 `Text` 都是同样那 38 格；`TreeViewItem` 带不带
   表头都 0；孤立 `TextBlock` 在两张不透明纸上 off-plate 恒为 0。`Host()` 真窗口那条路也做了测量（同样无差），
   但**没做成用例**——`Host()` 首帧与后续帧本就不可比（`adaptation/06` 里 `Host()` 那一条），守卫交给可确定的配底腿。
2. **正控制证明这个 0 不是仪器瞎**：菜单项那 38 格钉在不透明洋红底上颜色一字不变（`#6E6E73x16`，暗档 `#D1D1D6x16`），
   也就是自带 alpha 的不透明墨且随主题翻转——那是箭头的几何墨。**几何与表面到得了像素，字形到不了**，所以 #50 的
   准确说法收窄为"文本 run 不打印"，不再是"捕获看不见细的东西"。
3. **`Stable=False` 的第二种成因**：孤立 `TextBlock` 裸捕获从不稳定、整幅全黑；配底后才稳定并读出 0。在这类主体上
   `Stable=False` 是"没东西可画"的信号，不是"帧泵不够"的信号。

四类证据：**构建**见下闸口读数；**行为**+6 条哨兵用例（含正控制腿，缺墨即红）；**视觉**即上面 0 差分的三组配底读数与
38 格不变色的洋红对照；**硬件输入**本批不动输入路径。

**牙的验证（A/B）**：给孤立 `TextBlock` 钉一支不透明 `Background(#123456)` → 该用例两档各红（off-plate 不再是 0），
其余 4 条不动；改回 → 6/6 绿，并对 `tests/`、`src/` 全量 grep `A/B mutation` 零命中。**方向要说清**：这条哨兵断言的是
"字不出墨"，所以它变红意味着**上游开始打印字形**——那时该删掉哨兵、把逐控件的文本像素主张打开，而不是松判据。

**闸口读数**（串行 `tools/Test-AstraGates.ps1`，管道退出 **0**）：整套 **1465/1465 通过、0 失败、0 跳过**（7 m 6 s），
比上一批 1459 多 6 条＝本批 3 条 theory × 两档；`0 个警告 / 0 个错误`；调色板三档 `checked=True`；
`keys.md is current: 1298 canonical lines.`（键不变，本批不动产品代码）；末行 `All Astra gates passed.`。

不声称：差分只覆盖 4 个主体（Button / MenuFlyoutSubItem / TreeViewItem / 孤立 TextBlock）与 3 条捕获路，
"任何控件的文字都不出墨"是从这些读数外推的，不是逐控件量出来的；字号、字族（Segoe 与 FluentSystemIcons）、
`TextBox` 的实际输入文本、`RichTextBlock` 之类的排版路径都没进这张差分表；`Host()` 那条腿只是测量没有做成用例；
哨兵钉的是"当前不打印"，因此它**不会**在字形仍缺失时帮我们判断"该显示的字对不对"——那部分主张照旧只能走在树上读
前景令牌（#56 的余账）。产品侧的用户可见后果没有因为这批而变小：Gallery 里所有页面依然拿不到文本像素证据（#10）。

## flake 批（#48）：NavigationView 选中指示器换到不漂的通路——断言从"差值"升级成"点名的合成色"

那条 flake 的机制早就写在注释里：`Host()` 的**第一次成像与之后的不可比**（同一对捕获在两次运行里读出 30 054 与
16 624 个变化像素，因为宿主窗口自己的背衬在两次之间才沉下去），所以断言只能退成"两次计数之差 > 6000"。
#59/#60 两批把配底成像做成仪器之后，这条主张不必再借宿主窗口：把整个 `FluentNavigationView` 包在
`PixelHarness.LightPage`（`#F3F3F3`）里用 `Render()` 成像，选中的药丸就落在纸上，颜色是
`Over(#F3F3F3, SubtleFillColorSecondaryBrush)`——于是"差值"改回**绝对主张**：选中那张纸必须有该合成色 `> 6000` 格，
未选那张必须**一格都没有**。

- 实测：`8220` 格 `#EAEAEA`。这条同时反过来验了 `Over()`：老用例里手抄的常量 `PillOverPane = #EAEAEA` 与算出来的
  值同色（该常量随这次改判删除），而老读数在"整套 8220 / 单跑 7993"之间摆动，新通路上落在同一个 8220。
- **稳定性**：`AstraNavigationTests` 整类连跑 **4 次，78/78 全绿**（每次 5 s；前 3 次同一构建 `--no-build`，第 4 次在
  删掉那条悬空文档注释后**重新编译**再跑）；
  串行闸口（整套顺序跑）**1465/1465、0 失败 0 跳过、7 m 4 s、管道退出 0**，末行 `All Astra gates passed.`，
  `0 个警告 / 0 个错误`，调色板三档 `checked=True`，`keys.md is current: 1298 canonical lines.`。
  闸口那次构建里还带着那条注释（它漂在 `BrandEmerald` 与 `Pane()` 之间），清掉后的独立重建读回 `0 个警告 / 0 个错误`。
- **A/B**：把"选中"那张也改成不选（`select: false`）→ 该用例红，消息直接给
  `the selected pill painted 0 pixels of #FFEAEAEA over #FFF3F3F3`；改回 → 类内再次全绿，`A/B mutation` 零残留。

四类证据：**构建**见闸口读数；**行为**用例换了通路但主张变强（绝对计数 + 未选必须 0）；**视觉**即 8220 格 `#EAEAEA`
与未选 0 格；**硬件输入**本批不动输入路径（选中是用 `SelectedItem` 驱动的，与 #47 那条真指针通路无关）。

不声称：这条 flake 的原始观察是"**整套顺序跑时偶发**"，本批的重复证据是**同类 4 次 + 整套 1 次**，不等于整套连续多次绿；
宿主窗口那条路本身没有变好或变坏——只是这条主张不再依赖它，`Host()` 首帧不可比这条限制照旧写在 `adaptation/06`，
其他仍走 `Host()` 的用例（如窗口外壳那批）没被这次改动覆盖；未选态只断"这一色为 0"，没断"未选态自己画了什么"；
药丸的**位置**（选中第一项应落在第一行）本批没有量，只量了墨的**量**。

## 前景审计余账批（#56）：6 个没有 subject 行的 owner 补上"到达"读数——并量出别名键打不进 sentinel

`audits/foreground.md` 7.1 挂着 7 个 owner："前景只由键解析闸覆盖，没有到达部件的读数"。本批把它们接进
`AstraForegroundRoutingTests`：新增 14 条（6 个 owner × 两档 = 12，加 `AppBarToggleButton` 勾选态、列头禁用态）。
carrier 不是一处：菜单族与两个表格容器把行写在**自己**身上、标签由控件 `OnRender` 按该值画，
`AppBarToggleButton` 的标签是具名部件 `LabelText`（勾选行换到 `AppBarToggleButtonForegroundChecked`）。

1. **两种 sentinel 通路都不可用，这是本批的量出项**：(a) `OverrideBrush` 根本打不到别名键——`GetBrush` 只读生成调色板，
   于是 `DataGridRowForeground` 等 6 个全部 `KeyNotFoundException`；(b) 改打它别名指向的 `TextFillColorPrimaryBrush`、
   或者**挂好之后**再翻档，六条全数停在 `#E4000000`。后者就是"广播只到已上屏窗口的根"那条老账的另一面：
   已 mount 未上屏的要素保留建它时解析到的刷实例。所以可判定的仪器只剩一种——**在该档下 mount**，两档各一条。
2. **A/B 有牙**：抽掉 `Styles/Menus.jalxaml:288` 那行 `MenuBarItemForeground`，只有 `menu-bar-item` 两档红，
   且回落值是**继承墨** `#FF1D1D1F`（暗 `#FFF5F5F7`）而不是属性默认黑——所以干活的是"等于令牌"这条，
   `!= Colors.Black` 只是地板。该行改回后 22/22 绿，`git diff` 对该文件为空。
3. **同色盲区没有被本批消掉**：这 6 条 carrier 全落在同一个 `TextFillColorPrimaryBrush` 上，等值断言能证明
   "这一格到了 carrier"，证明不了"这一格与其他同色格互不相同"。别名身份那条（`ReferenceEquals(palette, 键解析结果)`）
   钉的是"逐字转录的别名行、不是抄色"，这条同时是 #12 的前置事实。

四类证据：**构建**——串行闸口（整套顺序跑）**1479/1479、0 失败 0 跳过、7 m 15 s、管道退出 0**，末行
`All Astra gates passed.`，`0 个警告 / 0 个错误`，调色板三档 `checked=True`，`keys.md is current: 1298 canonical lines.`
（比上一批 1465 多 14 条＝本批新增，键不变）；**行为**22 条（含 8 条老账不回归）；**视觉**本批全是树上读数，没有像素断言——
理由仍是 #50 字形不打印，而 carrier==控件自身是菜单项/表格单元能拿到的最强读数；**硬件输入**本批不动输入路径
（勾选与禁用都用属性驱动，hover/press 行照旧归 #13）。

不声称：只量了 2 个状态格（勾选、列头禁用），其余 5 个 owner 的 hover/pressed/disabled 行没测；
`audits/foreground.md` 7.2 那 5 处具名取不到、7.6 那 228 条状态 Setter 的落格，本批不结；
disabled 的表格单元与菜单项**没有**单独断言（框架在无头主机的禁用盖章只在 CheckBox/RadioButton/列表行/组合框行上量过）；
高对比档下这些 carrier 走不走映射，仍只在逐键闸那一层（#61 余账）。

## A2 别名层（#12）：量到了"别名行够得到框架绘制"，但本批不落地——下面是逐名字的账

试做内容：把 S1-i 普查里"Astra 没定义、框架自己读"的名字按 `<StaticResource x:Key=名 ResourceKey=我们的刷子>`
接进 `ThemeResources/FrameworkRetints.jalxaml`（那里原先只有 `AccentBrush` 一行，是 DataGrid 批为结品牌绿开的先例）。
别名而不是重定义，是为了两侧同实例——`ApplyAccent`/`OverrideBrush` 那支刷才还带着它们走。
**这批整体撤回，未提交**：逐名字的 blast radius 没量完，半收的状态比不收更糟。已量到的记在这里，下一批直接接手。

- **机制已证**：框架那支禁用墨是**按名字现取**的，不是硬编码常量——`Menu.cs:1172`、`Calendar.cs:1000`、
  `Primitives/TextBoxBase.cs:3032`、`MenuFlyoutItem.cs:208`（先试 `OneTextDisabled` 再试 `TextDisabled`）都走
  `ResolveThemeBrush(名, <框架兜底>, …)`。所以发布别名行能改到**我们不能重模板**的绘制。
- `TextDisabled`（→ `TextFillColorDisabledBrush`）一次翻掉**五条**别人的老账：表格宿主禁用
  （`#FFAEAEB2` → `#5C000000`，且第一次读数在 20 帧内还停在常态 `#E4000000`——这格要 ~40 帧才沉底）、
  禁用 `TextBox` 的墨（`AstraTextInputTests:229` 的 `Assert.NotSame` 反了）、
  CheckBox / RadioButton / ListBox 行三处禁用标签（共用字段 `FrameworkDisabledText`）。
  新墨在 Fluent 侧是对的（正是我们禁用格子要的值），但"三条同族事实 + 两条表格/文本框事实"要逐条改名、
  重测、并把"框架拥有禁用墨"这句老结论在文档里一起结掉——不是一轮闸口能收的账。
- `TextPrimary` / `TextSecondary` 不止改一个控件：撤掉 `TextDisabled`、只留 6 个名字后，串行闸口仍剩 **2 条红**，
  都在 `AstraMenuTests`——`The_controls_paint_their_own_rule_and_text_and_our_rows_reach_neither` 与
  `The_sub_items_own_paint_follows_the_theme`，两处像素主张从 `0` 格变 **`38` 格**（框架自绘的菜单箭头/分隔改跟
  我们的主题走；38 正是 #50 那批量到的子项箭头墨）。也就是"我们的行到哪为止"这条边界要按别名层重写。
- 已备好、可直接复用的部分：`ControlBorderFocused`（框架值 `#FF1E793F` 品牌绿，挂在 `PART_OuterBorder.BorderBrush`
  的 `IsKeyboardFocused` 格）结清用例 `The_frameworks_focused_border_name_no_longer_resolves_to_brand_emerald`
  与 7 对别名的 `Assert.Same` 理论都写测过（`0 个警告 / 0 个错误`，三表类 107/107 绿），随撤回一起回到工作树外，
  下一批照这段重写即可；`keys.md` 那 6–7 行是**框架的名字**、不是 WinUI token，进清单只为让漂移闸看得住这一层。
- `CaptionFontSize` 是 `Double`，本运行时标记带不了类型化数值行（S1-n：`x:Double` 元素名不可解析），
  无论这批收不收都留在 Known Gap。
- 下一批的开工顺序（就是这批学到的形状）：**一个名字一轮**——先按该名字全库 grep 读点、把会被改写的既有事实列全、
  再发布别名行、再跑闸口；不要一次发七个。

### 逐名字读点普查（按上面那条规则做的第一轮 grep，只读、未发布任何别名行）

**测量基座要先声明**：计数来自 sibling 源树 `../Jalium.UI`（AGENTS.md：参考用、可能比运行时权威 26.10.9 新），
"代码现取"= 该名字出现在 `Resolve*Brush(名, …)` 里的次数，"标记读取"= 框架 `.jalxaml` 里以名字读它的次数。
`AccentBrush` 那条 58 读/12 文件的账是**发布字典**上量的，与这张表不同基座，不能横向比大小。

| 名字 | 代码现取 | 标记读取 | 发布别名要付的账 |
|---|---|---|---|
| `SurfaceBackground` | 1 `ResolveApplicationBrush` + 2 `ResolvePopupBrush` | 8 | 窗口/弹层背衬一起翻，`audits/window-shell.md` 与 flyout 那批"我们的行到哪为止"要重读 |
| `TextPrimary` | 9 `ResolveThemeBrush` + 2 `ResolvePopupBrush` + 1 `ResolveMenuBrush` + 1 `ResolveCalendarBrush` | 19 | 全库墨色，表里最大的一格；已实测它会改写 `AstraMenuTests` 两条像素主张（0 → 38 格） |
| `TextSecondary` | 4 `ResolveThemeBrush` + 4 `ResolveCalendarBrush` + 1 `ResolveMenuBrush` | 14 | 日历与菜单两处框架自绘的次级文本一起跟 |
| `TextOnAccent` | 2 `ResolveThemeBrush` + 1 `ResolveCalendarBrush` | 8 | 与 `AccentBrush` 成对：反色文字只有跟着强调色走才成立 |
| `ControlBorder` | 3 `ResolveThemeBrush` + 3 `ResolvePopupBrush` + 1 `ResolveApplicationBrush` | 18 | 描边族；已被我们重模板的控件不读它，弹层那族读 |
| `ControlBorderFocused` | 6 处 `TryFindResource`（见下面的更正） | 14 | 表里原写"无代码 resolver"是**错的方向**——它只是没有 `Resolve*Brush` 形状的读点 |
| `TextDisabled` | 4 处现取（`Menu.cs:1172`、`Calendar.cs:1000`、`Primitives/TextBoxBase.cs:3032`、`MenuFlyoutItem.cs:208`） | 未单独量 | 本批已量：一次翻掉五条老账，见上一节 |

**普查口径本身错了半边（下一轮之前先读这条）**：判"这个名字够不够得到"不能只 grep `Resolve*Brush`。控制代码里
按名字查资源的另一条通路是 `TryFindResource("名")`，`src/managed/Jalium.UI.Controls` 里共 **107 处**，
名次与上表不同：`TextSecondary` 10、`AccentBrush` 9、`TextPrimary` 8、`ControlBorder` 7、`TextPlaceholder` 6、
`ControlBorderFocused` 6、`SurfaceBackground` 4，另有本批此前没记到的名字 `TextPlaceholder`、`SelectionBackground`、
`ControlBackground`、`CommandBarBackground`、`MenuFlyoutPresenterBackground`/`BorderBrush`、`WindowBackground`、
`TitleBarGlyph`。**口径修正：可达性按"`TryFindResource` 或 `Resolve*Brush` 或标记 `{ThemeResource}`"三条并集算。**

（自我更正，同一天）上面这一句初稿还把 `FocusStrokeColorOuterBrush` / `FocusStrokeColorInnerBrush` 算成"框架控件代码在读的名字"，
那是**归属错**：按 `FocusStrokeColor` grep `src/managed` 命中 **0**，这两个名字只出现在框架自带的
`FocusedBorderThemeTests.cs:33-34` 里，以 `app.Resources.TryGetValue("FocusStrokeColorOuterBrush", …)` 的形式被断言。
**但这句更正对本层是利好而不是利空**：框架的测试**要求宿主主题层提供这两个 token**，也就是"焦点环的两支刷由应用字典负责"
是上游写进测试的契约——而 `audits/keys.md` 显示 Astra 已经画了：Light `#E4000000`/`#B3FFFFFF`、Dark 反向
`#FFFFFF`/`#B3000000`，高对比档逐键映射到 `SystemColorWindowTextColor`/`SystemColorWindowColor`（清单第 446-447 行），
消费点在 `Styles/Common.jalxaml`、`Styles/Inputs.jalxaml`、`Styles/Navigation.jalxaml` 的焦点框 `Border` 上
（`grep` 三个文件均命中；**逐处计数未做**，上面这条清单是 head 截断的输出，别当全量）。
所以焦点环**不需要**别名层：它是我们的模板画的、用我们的 token。**留给别名层的只有框架自己那圈焦点描边**
（`ControlBorderFocused` 走的是框架控件代码路径，与本层已重模板的控件无关）。

`ControlBorderFocused` 的专项更正：上一段那条"六族是 `.cctor` 一次性取值、别名可能整族吃不到"的警告**撤回**。
读消费形状即可判定（`AutoCompleteBox.cs:27`/`:1328`、`DatePicker.cs:248`/`:612`、`NumberBox.cs:60`/`:1363`、
`PasswordBox.cs:219`/`:813`、`TextBox.cs:74`/`:1702`、`TimePicker.cs:143`/`:679`）：

```csharp
private static readonly SolidColorBrush s_fallbackFocusBorderBrush = new(ThemeColors.ControlBorderFocused);
...
return TryFindResource("ControlBorderFocused") as Brush ?? s_fallbackFocusBorderBrush;
```

`.cctor` 里那支是 **`??` 右边的兜底**，不是取值路径；`ThemeColors.ControlBorderFocused` 本身是
`=> Color.FromRgb(32, 114, 69)`（`#207245`，与 `08-themecolors-raw.txt` 的读数一致）的表达式属性。
更硬的凭据是**框架自带的测试** `tests/Jalium.UI.Tests/FocusedBorderThemeTests.cs`：它
`Assert.Same(app.Resources["ControlBorderFocused"], ResolveFocusedBorderBrush(control))`，对 AutoCompleteBox /
DatePicker / NumberBox / PasswordBox / TextBox 五族逐个钉**实例同一性**。也就是"在应用级 `Resources` 放这个名字的刷子、
框架就照它画"是**上游自己测过的设计入口**，不是我们撞运气——A2 别名层的立场由此从"机制可用"升为"上游背书"。

仍欠的一手（不许用上面这条代替）：上面全部读自 sibling 源树，`02-ceiling-raw.txt` 的 IL 账证明 26.10.9 里 `.cctor`
那支在，但**没证明 26.10.9 的取值路径已经是 `TryFindResource ?? 兜底` 这个形状**。发布这一行之前要在 NuGet 权威上做
一次读数：`发布别名行 → 挂载并聚焦一个 TextBox → 读焦点描边是不是我们那支实例`。这一步没做，`ControlBorderFocused`
就仍然只是"最该先做的那一个"，不是"已结清的那一个"。

**2026-09-22 补测：这一手已在 26.10.9 上做了（`tests/FluentJalium.Tests/AstraFrameworkNameResolutionTests.cs`，2/2 绿）。**
仪器不发布产品行：在测试里往 `Application.Current.Resources` 插一支哨兵刷 `#112233`、按家族调框架私有
`ResolveFocusedBorderBrush`、同一次派发里 `Remove`（留着会给共享宿主后面的每个类换色）。读数：
TextBox / PasswordBox / NumberBox / AutoCompleteBox **四族全部返回我们那支实例**（`ReferenceEquals` 成立），
把哨兵撤掉则回落到框架自己的绿。结论落地为一条判据：**26.10.9 的焦点描边确实是"按名字现查、`.cctor` 那支只当
`??` 兜底"**，别名层这条杠杆是真的，与源树形状一致。

三条限制照实写：(1) 这测的是**解析路径**，不是像素——没挂载、没聚焦、没读 `PART_OuterBorder.BorderBrush`，
"焦点框在屏幕上变成我们的颜色"仍由"重模板把这条线排除掉"的既有事实负责；(2) 回落那条只断到"G ∈ {0x72, 0x79, 0x80}"
这种松度，**具体是哪支绿没记下来**，别把它当色值账；(3) 反射只出现在测试里（AGENTS.md 禁的是产品代码反射框架私有成员，
`AstraGateTests.Theme_kernel_stays_free_of_repair_loops_and_reflection` 仍管着产品侧），而且它按**方法名**找——
将来上游改名会让这条红，那是有意的告警不是脆弱。这一批**只加了测点，没发布任何别名行**，`keys.md` 不变（没有新发布的键）。
全量验证已在同日补上：`tools/Test-AstraGates.ps1` 在含该测点的提交 `a79887a` 上串行跑完 ——
build `0 个警告 / 0 个错误`、整套 **1481/1481**（0 失败 0 跳过，7 m 24 s，含新类那 2 条）、调色板三档 `checked=True`、
`keys.md is current: 1298 canonical lines.`、末行 `All Astra gates passed.`、退出码 `0`。所以这条测点不只在本类绿，
在共享宿主的顺序全量下也没给别的类换色。

## 判据批：别名单元吃不到 sentinel（补进 #56，2026-09-21 已提交）

见"前景审计余账批（#56）"第 1 条：`OverrideBrush` 经 `GetBrush` 只认生成调色板，别名键直接 `KeyNotFoundException`；
改打别名指向的调色板刷或 mount 后翻档，carrier 一律不动。



## 交接（回合用尽，2026-09-22）：闸口全量绿在 `d7259b2`，下一手是 ControlBorderFocused 的 twin

- 已验基线：`tools/Test-AstraGates.ps1` 在 `a79887a`（含 `AstraFrameworkNameResolutionTests`）上 **1481/1481、0 跳过、
  调色板三档 checked=True、keys.md 1298 行当前、All Astra gates passed.**；`src/ tests/ tools/` 无未提交改动。
- 别名层仍未发布任何新行（`ThemeResources/FrameworkRetints.jalxaml` 只有 `AccentBrush`）。杠杆已证：焦点描边按名字现查。
- 下一步第一手：查 WinUI 焦点态描边到底用哪条 token。**别再猜上游目录**：在 `../microsoft-ui-xaml`（commit `19e3bdc`，
  只读）这份工作副本里 `dev/`、`resources/styles`、`resources/themes` 三条路径都不存在（两次 grep 均
  `No such file or directory`，因此本会话没有拿到任何 token 读数）。先 `ls` 定布局，再 grep，再决定发布。

### ControlBorderFocused 的 twin 已量到（上游 `controls/dev/…/TextBox_themeresources.xaml`，commit `19e3bdc`）

链条：`TextControlBorderBrushFocused` →（`StaticResource`）→ **`TextControlElevationBorderFocusedBrush`**，
后者**不是实色**，是一支 `LinearGradientBrush MappingMode="Absolute" StartPoint="0,0" EndPoint="0,2"` +
`ScaleTransform ScaleY="-1" CenterY=0.5`，两个 `GradientStop` **都在 Offset 1.0**：一支
`{ThemeResource SystemAccentColorLight2}`（Light，第 62 行）/ `SystemAccentColorDark1`（Dark，第 169 行），
另一支 `{StaticResource ControlStrokeColorDefault}`（第 63/107/170 行）。也就是 WinUI 焦点框的真身在 2 DIP 带上做
"强调色底环 + 常态描边"的分层，不是单色。

**这条读数直接改变 #12 那一行的做法**：`<StaticResource x:Key="ControlBorderFocused" ResourceKey=某实色>` 无论指向哪支
实色都是**偏离上游**（框架侧接受的是 `Brush`，实色能过解析、拿不到环）。上游忠实转录要写一支同样几何的渐变行——
而渐变行在本运行时的可达性还没测（`adaptation/00` S1-r 只结清过 `x:Double`/`x:Int32` 无载体，`LinearGradientBrush`
元素行未验），且 `MappingMode=Absolute` 的 2 DIP 假设是按文本框边框尺寸设计的，套到 `PART_OuterBorder` 上是另一次外推。
两个候选因此都记下，未择一：A) 发实色别名（先结掉"品牌绿不许出现在我们树上"这条硬约束，承认环为偏差）；
B) 发渐变别名（先量 `LinearGradientBrush` 行在 26.10.9 标记里到底落不落，再谈几何外推）。
本批**未发布任何行**，两案都要过一次整套串行闸口；起点基线仍是 `d7259b2` 的 1481/1481。

## A2 别名层第一段落地（#12）：`ControlBorderFocused` 一行发布，品牌绿焦点框从查表里退场

按"一个名字一轮"发了**一个**名字：`ThemeResources/FrameworkRetints.jalxaml` 加
`<StaticResource x:Key="ControlBorderFocused" ResourceKey="AccentFillColorDefaultBrush" />`。
twin 不用新决策——上游同一个构造（`TextControlBorderBrushFocused` → `TextControlElevationBorderFocusedBrush`，
`controls/dev/CommonStyles/TextBox_themeresources.xaml:57/101/164` @`19e3bdc`）是**渐变**，而
`ThemeResources/TextBox.jalxaml:21-33` 早已为同一理由（别名后面挂渐变会冻在 Light 那档，因为
`FluentThemeManager.RefreshPalette` 只原地重染 `SolidColorBrush`）把自己的行接成 `AccentFillColorDefaultBrush`；
这一行只是把那一次决定延到框架读的名字上，环的偏差记在一处而不是两处。

- **构建**：`tools/Test-AstraGates.ps1` 全量 `0 个警告 / 0 个错误`。
- **行为**：新增/改共 3 条（`AstraFrameworkNameResolutionTests`）——四族按名现查（哨兵刷）、发布后顶层 Remove 撤不下
  合并进来的行且解析值 **就是** 调色板强调色那一支实例（`Assert.Same`，本机读数 `#FF0078D4` = `ApplyAccent(null)`
  下调色板停在系统强调色）、`TryFindResource` 与 `GetBrush` 同实例。
  **A/B 有牙**：把那行临时改指 `ControlStrokeColorDefaultBrush` → 恰好 2 条"发布"事实红、探针那条绿
  （它不依赖指向哪支）；还原后复验绿，突变零残留（`git diff` 只剩本批的 18 行插入）。
- **视觉**：本批无像素断言——测的是解析路径，焦点框在屏幕上的墨仍由"重模板把这条线排除在我们树上"的既有事实管。
- **硬件输入**：不动输入路径（焦点由反射调用与属性驱动，无真指针）。
- **清单**：`keys.md` 1298 → **1299** 行（框架的名字，不是 WinUI token，进清单只为让漂移闸看得住这一层）；
  调色板三档 `checked=True`；整套 **1482/1482、0 跳过、7 m 27 s**，末行 `All Astra gates passed.`。

不声称：第一次全量跑里 `AstraFlyoutCornerTests.The_suggestions_surface…` 报过一次红
（`The suggestion list never reached the overlay layer`，不是颜色变化）。单跑该类 **两次 3/3 绿**，
带本批改动的第二次全量也绿，所以它**未复现**；把它归进 #35/#47/#23 那族"顺序跑时 overlay/宿主取不到"的
flake 是**假设，未证**——机制仍未结，别当已排除。`DatePicker`/`TimePicker` 这一族没进断言（本机没有可挂载验证的
稳定通路，且我们不改它们模板），别名对它们的实际效果只由源树消费形状支持；
26.10.9 权威上"焦点框像素真的换色"仍无断言（#50 那族限制之外的另一手：需要真键盘焦点）。
下一轮名字候选与代价照旧列在上面那张普查表里。

## #12 差距表：剩下 6 个名字在 26.10.9 上各读成什么（量完即摘仪器，不留故意红的测点）

一次性仪器：`Application.TryFindResource(框架名)` 与候选 twin 比**实例**，红消息带真实色值；跑一轮取数后从
`AstraFrameworkNameResolutionTests` 移除（闸口里不许住着一堆"预期红"）。框架那一层是**一整套 iOS 灰**，不是 WinUI
Fluent 值——所以别名层的价值是实的，但每个名字要单独判：

| 名字 | Light 实测 | Dark 实测 | 候选 twin（我们的值） | 判定 |
|---|---|---|---|---|
| `SurfaceBackground` | `#FFFFFFFF` | `#FF2C2C2E` | `SolidBackgroundFillColorBaseBrush` `#FFF3F3F3` / `#FF202020` | 真差距，第二轮 |
| `ControlBorder` | `#FFD2D2D7` | `#FF48484A` | `ControlStrokeColorDefaultBrush` `#0F000000` / `#12FFFFFF` | 真差距（我们是带 alpha 的），第二轮 |
| `TextPrimary` | `#FF1D1D1F` | `#FFF5F5F7` | `TextFillColorPrimaryBrush` `#E4000000` / `#FFFFFFFF` | 真差距，代价已知：`AstraMenuTests` 两条 0→38 格 |
| `TextSecondary` | 未取到（输出截断） | `#FFD1D1D6` | `TextFillColorSecondaryBrush` `#9E000000` / `#C5FFFFFF` | Dark 侧真差距；Light 读数补测后再判 |
| `TextDisabled` | `#FFAEAEB2` | `#FF636366` | `TextFillColorDisabledBrush` `#5C000000` / `#5DFFFFFF` | 真差距，代价最大：五条禁用墨老账要一起改 |
| `TextOnAccent` | 未取到（输出截断） | `#FFFFFFFF` | `TextOnAccentFillColorPrimaryBrush` `#FF000000`(Dark) | **不盲接**：框架在 Dark 用白字、我们的 twin 是黑字，对不对取决于它当时铺在哪支强调填充上；要先量"这个名字被读的那些表面实际填充是哪支刷"，否则别名会把反色文字接到反了的值上 |

两条未取到的 Light 读数**不猜**：`TextSecondary[Light]` 与 `TextOnAccent[Light]` 在下一轮补测。
`ControlBorderFocused` 已在第一段发布（`bfca518`），不在表内。第二轮的范围因此定为 `SurfaceBackground` +
`ControlBorder`（差距实、无已知事实依赖框架那两支灰），`TextOnAccent` 挪到"先量表面填充"之后。

## A2 别名层第二段（#12）：`SurfaceBackground` + `ControlBorder` 发布，全套像素主张一格没动

按上面差距表挑的两个名字（不是按清单顺序）：框架那两支是 `#FFFFFFFF`/`#FF2C2C2E` 与 `#FFD2D2D7`/`#FF48484A`，
接成我们的 `SolidBackgroundFillColorBaseBrush`（`#FFF3F3F3`/`#FF202020`）与
`ControlStrokeColorDefaultBrush`（`#0F000000`/`#12FFFFFF`）。

- **构建**：全量 `0 个警告 / 0 个错误`。
- **行为**：新测点 `A_retint_row_follows_the_theme_flip_to_the_variant_it_declares` 6 条腿（3 个已发布名字 × 两档），
  断的不是"能解析"而是**别名转发的就是调色板当下那支实例** + 在两档里都是 `SolidColorBrush` —— 也就是
  "别名会不会冻在建字典那一档"从假设变成断言（渐变那条冻结偏差是同一个机制的反例，写在
  `ThemeResources/FrameworkRetints.jalxaml` 的注释里）。
  **A/B 有牙**：把 `ControlBorder` 临时改指 `SolidBackgroundFillColorBaseBrush` → 恰好它的 Light/Dark 两条腿红、
  其余 7 条绿；还原后 9/9 绿，`git diff` 只剩本批 14 行插入，无突变残留。
- **视觉**：本批**没有任何既有像素主张变化**——1488/1488 里包含暗色表面、弹层半径、表格族底那批断言，全部原样绿。
  这既说明我们已断言到的表面都不读这两个框架名字（都被自己模板接管），也是一条**未覆盖警告**：
  框架用这两个名字画的那些面**还没有像素断言**，所以"接上了"目前只有树上读数。
- **硬件输入**：不动输入路径。
- **清单/漂移**：`keys.md` 1299 → **1301**；三档 `checked=True`；整套 **1488/1488、0 跳过、8 m 41 s**，末行
  `All Astra gates passed.`。

不声称：#58（"暗色下表格族那片表面仍是白的"）**不随本批结**——那处的读数是按既有断言原样绿推出来的，
不等于那片表面现在跟上了 `#202020`；要结它得在暗色下对那块表面重新成像。`TextOnAccent` 仍按上一节的判定停在
"先量它铺在哪支强调填充上"；`TextPrimary`/`TextSecondary`/`TextDisabled` 的代价（2 + 5 条事实）照旧记在普查表旁，
每一轮都要一起改，不许放松旧断言混过去。

## #10 第一段：Gallery 的 Tokens 系统页落地（色板网格 + 缺失键诊断），差集闸不动

页契约先摸清再动手（`MainWindow.jalxaml` 的 `PageHost` 子元素 + `.cs` 的 `_pages`/`_pageIds` 两处字典 +
`Catalog.json` 的 `pages` 条目 + `WireTokens()`），所以这一页是按现成形状加的，不是新造一套宿主。

- **构建**：`dotnet build samples/FluentJalium.Gallery` `0 个警告 / 0 个错误`（`Symbol="Color"` 与
  `Application.Current.TryFindResource` 的用法都在这一步验过，不是猜的）。
- **行为**：新测点 `AstraGalleryTokenTests.The_grid_the_tokens_page_paints_is_the_palette_the_kernel_publishes`——
  自己解析 `keys.md` 的 Light 块 `SolidColorBrush` 行，自己正则解析生成的 `TokenCatalog.cs`，两-reader 双向 + 顺序
  比对（与目录闸同一个立场：两个读者必须各自独立，否则会同向漂移）。Gallery 全类 **7/7 绿**。
  token 数组**由脚本从清单生成**、不手抄（101 个），生成方式写在文件注释里，闸口管得住重生成后的漂移。
- **视觉**：`Test-AstraGallerySmoke.ps1 -Page tokens` 真启动、导航到该页、优雅关闭，18.8 s，无残留进程。
  **但这一页还没有像素断言**：冒烟只证明窗口出现并关闭，不证明 101 个 swatch 全上了色。
- **硬件输入**：本页无可交互控件（纯展示 + 代码构建），不动输入路径。

不声称：①"每个 token 真的解析出刷"目前没有运行时逐条断言——页上的诊断行会把没解析的键**点名**给人看，
但没人读那行也算绿；要结它得在主题运行时测点里逐 token `TryFindResource` 断非空（两档各一次）。
② 每页可渲染到像素仍未做，卡在一个未决设计：测试工程只引用 `src/FluentJalium`、不引用 Gallery（`WinExe` +
`internal` 类型），要么给页内容建一个可共享的工厂，要么把渲染自证放进 Gallery 自己的启动参数里。
③ 这一页的 parity 不是 `Catalog.json` 的 controls 行——tokens 不是控件，硬造一行会让目录闸变成假账。

### #10 第一段的闸口读数（补记，2026-09-22）

`tools/Test-AstraGates.ps1` 在含 Tokens 页的 `be73b3e` 上串行跑完：build `0 个警告 / 0 个错误`，整套 **1489/1489**
（0 失败 0 跳过，7 m 33 s，多的那 1 条就是 `AstraGalleryTokenTests`），调色板三档 `checked=True`，
`keys.md is current: 1301 canonical lines.`，末行 `All Astra gates passed.`，脚本自己写的 `GATE-EXIT=0`。
所以 Tokens 页不只在自己那类绿：目录闸的"每页都要有宿主与 id""页里的控件必须入库或豁免""bin 下 Catalog.json 与源一致"
三条在顺序全量下都跟着过了。上一节写下的三条不声称（逐 token 运行时断言、每页渲染到像素、tokens 不占 controls 行）
不因这次绿而改变。

## #10 第二段：Materials 与 Motion 两页落地，三个系统页齐了

页形状与 Tokens 一致（`PageHost` 子元素 + 导航项 + `_pages`/`_pageIds` + `Catalog.json` pages 行），两页都不新造宿主、
不新增控件类型，所以目录差集闸不必改。

- **Materials**：层与高程按"能合成"的方式画——base → `LayerFillColorDefaultBrush` → card → alpha 描边**嵌套**呈现
  （α 层没有独立颜色，脱离底就不可读），另一张卡说清边界：本运行时**没有** `AcrylicBrush`/`MicaBrush`/`RevealBrush`
  类型（`adaptation/s0v-runtime-type-inventory-raw.txt` 的 shipped-type 清单），所以浮起面只能是实色 + α 描边，
  `CardStrokeColorDefaultSolidBrush`（#EBEBEB）就是描边不能合成时用的实色 twin；窗口级背衬是另一回事且真实存在
  （`WindowBackdropType = None/Auto/Mica/Acrylic/MicaAlt`，`adaptation/01-jalium-control-census.md:146`），
  开关留在真正吃得到它的 Settings 页，不在此重复。
- **Motion**：三条已发布的 `Duration` 行（`ControlFasterAnimationDuration` 83 ms、`ControlFastAnimationDuration`
  167 ms、`SplitViewPaneAnimationOpenDuration` 200 ms）各挂一支 bar，`TransitionProperty="Width"` +
  `TransitionDuration="{StaticResource 行名}"`，点 "Beat" 写新 Width 让框架按各自时长补间——走的是本运行时**唯一会 tick
  的属性形状**（元素上的 Double DP），不碰 transform（transform 从不动）。读数条报的是三行**此刻解析到的值**与
  `FluentThemeManager.ReduceMotion` 状态，不是文件里写的期望值，所以减动效改写活行时页面自己会跟着变。
  减动效开关**故意不在这页复制一份**：它是应用级全局、两个开关只会互相说谎。
- **构建**：`dotnet build samples/FluentJalium.Gallery` `0 个警告 / 0 个错误`（导航图标 `Symbol="Globe"`、
  `Symbol="CalculatorAnimation"` 都在这一步验掉，不是猜的枚举名）。
- **视觉/输入**：`Test-AstraGallerySmoke.ps1 -Page tokens,materials,motion` 三页各自真启动、导航、优雅关闭
  （16.8 / 14.4 / 12.1 s），无残留进程。

不声称：① 冒烟不点 "Beat"，所以 Motion 页的补间**没被驱动过**——那条路径的行为凭据仍只有 `AstraMotionTests` 的树上读数，
页面只是把它可视化；② "每页可渲染到像素"这一条仍未结（卡在同一个未决设计：测试工程不引用 Gallery），
三页齐不等于这一项完成；③ Materials 页的 α 合成主张是"看得到"级别的证据，不是断言——没有针对该页的像素读数。

### #10 第二段的闸口读数（补记，2026-09-22）

三页批次（`3c8794b`）之后的整套串行闸口：build `0 个警告 / 0 个错误`，整套 **1489/1489**（0 失败 0 跳过、7 m 25 s）、
调色板三档 `checked=True`、`keys.md is current: 1301 canonical lines.`、末行 `All Astra gates passed.`、
脚本自记 `GATE-EXIT=0`。测点数**未增**（1489 与 Tokens 段同值）——Materials 与 Motion 两页是声明式内容 + 一个按钮，
没带新测点，所以这次绿只证明"新页没破坏任何既有主张"，不证明它们自己正确；它们各自的凭据仍是上面写的那三条不声称。

### Tokens 页的缺口①已结：101 个 token 在两档下逐一解析成 Brush 现在是断言

`AstraGalleryTokenTests.Every_token_the_grid_paints_from_resolves_to_a_brush`（两档各一条腿，Gallery 类 **9/9 绿**）。
上面"#10 第一段"那节写的不声称①（"页内诊断行没人读也算绿"）就此结掉：读数条仍给人看，但它不再是唯一会发现的途径。

过程里踩了一次自己的规矩：**给测试类加 fixture 参数必须同时挂 `[Collection(...)]`**。第一次跑整类 3 条全红，
消息是 `The following constructor parameters did not present matching fixture data`——包括**原本绿的**漂移闸，
因为类构造失败会带走整类。补 `[Collection(AstraThemeRuntimeCollection.Name)]` 后 9/9。
这条对 #47/#35 那族也有用：整类同刻同因失败，不是 flake，别按 flake 记。

### 缺口①批次的闸口读数（补记）

`9d86e12` 之后：build `0 警告 / 0 错误`，整套 **1491/1491**（0 失败 0 跳过、7 m 12 s，多的 2 条就是两档 token
解析腿），调色板三档 `checked=True`，`keys.md is current: 1301 canonical lines.`，`All Astra gates passed.`，
`GATE-EXIT=0`。至此 #10 的三页 + Tokens 页的清单/解析两条闸都在全量下绿；仍欠的是"每页渲染到像素"与
Motion 补间的驱动证据。

## #10 缺口②：页级像素闸建起来了、13/13 绿、牙齿验过，但**没有落地**——它一进顺序全量就弄红 3 条既有测点

先前把这一项写成"未决设计"的两个理由，实测都不成立：

- **"测试工程不引用 Gallery"**：加上 `<ProjectReference …FluentJalium.Gallery.csproj />` 就能编译。耦合只在测试侧，
  产品侧的禁令不受影响；`MainWindow` 与 `NavigateToPage`/`SetStartPage` 本来就是公开的，反射一处都不需要。
- **"要渲染就得真 Show 窗口"**：恰恰不能。`PixelHarness` 全部捕获都走它自己那唯一一台宿主窗口，而**同一线程上只有
  第一个被 Show 的窗口收得到渲染帧**（harness 自己的记录），第二个真窗口只会得到全黑。于是不 Show：构造
  `MainWindow` → `NavigateToPage(pageId)` → `PixelHarness.Render((FrameworkElement)window.Content!, 1100, 820)`，
  `Place` 会把这棵内容树临时搬到 harness 的宿主里。窗口因此不会走 `Closed`，它的主题订阅在整个 run 里留着——13 份，
  代价写在注释里而不是藏起来。

闸本身是 `AstraGalleryRenderTests.Every_gallery_page_paints_a_fluent_surface_in_both_variants`：页 id 由**另一个**
`Catalog.json` 读取器枚举（不是 Gallery 自己那份，理由同目录闸：两个读者必须对上），每条腿在同一窗口里对 Light/Dark
各拍两张——挂页一张、`ContentHost.Children.Clear()` 后一张。底板那张是**这个闸真正的牙齿**：只看页数/色彩数无法区分
"这一页画了东西"和"导航面板画了东西"。

`PageHost` 通过代码后置自己那个私有 `ContentHost` 视图读到；反射只在测试装配里，`AstraGateTests` 那条产品侧禁令没动。

**阈值全部量出来，不是挑出来的。** 2026-09-22 的探路跑（13 页 × 2 档 × (页, 空底板) = 52 帧，1100×820）：
每帧 `Stable=True`；非黑像素 901,927..902,000（满帧 902,000）；本页档底色 208,720..268,600、空底板
208,720..208,722；去重色彩页 25(materials/Dark)..178(selection/Dark) 对空底板 19..24；`#1E793F` 品牌绿 52 帧全 0。
断言贴在下界：非黑 ≥900,000、本档底色 ≥200,000、Dark 里 Light 底板 == 0（这就是"翻主题翻到页身上"的那条）、
页数**严格大于**同窗口空底板页数。

- **单独跑是绿的，且有牙齿**：`--filter ~AstraGalleryRenderTests` **13/13 通过、2 m 9 s**；把 `Children.Clear()`
  改成永不执行（让"底板"退化成同一张页图）→ **13/13 全红**，红在具名那条上，消息带着量值
  （`light 64 colours against 64, dark 80 against 80`）；改回即绿。**承重的是"页 > 空底板"这一条**，
  其余四条在 A/B 下照旧绿——别把它们当门牙。
- **但它破坏了顺序全量，所以当天就撤了**。加上这条闸之后的整套串行闸口：**1500 通过 / 4 失败**（1504），
  `GATE-EXIT=1`。4 条红是 `AstraTeachingTipTests.A_side_with_no_room_for_the_card_loses_to_one_that_has`
  与 `AstraAutoSuggestBoxTests` 的三条建议列表测点。用三类一起单进程复现（本闸 + 那两个类）拿到同样的 4 红，
  116 通过——**不是我改的东西碰巧红了，是这条闸放进去才红的**；同一批测点在 3fb4302 的全量里是 1491/1491 绿。
- **其中 1 条的机制已经量清**：`PixelHarness.EnsureHost` 只会把共享宿主窗口**长大、从不缩回**，而 1100x820 的页捕获
  把它撑到 1150x877，后面所有弹层的摆放都是照这块屏幕矩形算的——本类的 `Dispose` 把宿主的 Width/Height 还回去之后，
  TeachingTip 那条**立刻回绿**。这是 harness 的一条真实约束，值得单独记住：任何比 884x684 更大的捕获都会污染后续几何。
- **剩下 3 条的机制当时没找到**（下一节结掉）。一个假设被实测排除，另一个只当候选记着：① "旧内容留在宿主里、按名字找部件会找到 Gallery 的那一个"——
  探路测点（`spike/GalleryRender/probe-a.log`、`probe-b.log`）在"先拍一张 Gallery 页 + 还原宿主"之后
  仍只数到 `containers=1`、`Border 260x41.78 min=260 visible=True`，与不拍的那条腿一模一样；
  ② "档位被带走"——当时**没测过**，只记成候选；现在已被后面的节否证（根因是覆盖层里嫁接着的部件，与档位无关）。
  红的读数是
  `container.MinWidth 0`（应为 260）与 `container.ActualWidth 98.06`（应为 260），还有一条
  `Assert.Same` 拿到的是**禁用文字**色（一次 #FF636366＝Dark 的 TextDisabled、一次 #FFAEAEB2＝Light 的）——
  即找到的那个 item 不是它自己弹出的那一个，但*为什么*不是还没量出来。
- **处置**：类与 `ProjectReference` 都从工作树撤了；闸的完整实现曾留在 `spike/GalleryRender/AstraGalleryRenderTests.cs.parked`
  （不在任何工程里，不会被编译；它那份 Dispose 里的宿主还原按上面量到的事实写着——还**没有**跑过绿，见下一段）。
  下一批要接这一步，**先解这条交互**再落地，起手就带着上面那个已排除的假设，别重走。
  （后来：这条"在共享宿主进程里逐页拍"的路线在下一节被判死，`.parked` 文件随之删除，只留这里的记述与两份探路日志。）
  测量数据（52 帧那组上下界）照抄在下面，重新实现时不必再探一遍：非黑像素 901,927..902,000；
  本档底色 208,720..268,600；底板 208,720..208,722；去重色彩 25..178 对底板 19..24；品牌绿 52 帧全 0；
  每页每档都要拍，成本 2 m 9 s。
- **仍未声称**：页内几何与间距、"哪一块表面多出来的"、与 WinUI Gallery 的像素对齐——一条都没有；字形不打印（#50）
  在这条闸上是已知上限（纯文本页过不了"严格大于底板"）。

### 撤回之后的闸口读数（补记，2026-09-22）

撤掉 `AstraGalleryRenderTests` 与测试工程对 Gallery 的 `ProjectReference` 之后重跑串行闸口：build `0 警告 / 0 错误`，
整套 **1491/1491**（0 失败 0 跳过、7 m 0 s），调色板三档 `checked=True`（Light 83 源色 / 101 刷、Dark 同、
HighContrast 101 映射键），`keys.md is current: 1301 canonical lines.`，`All Astra gates passed.`，`GATE-EXIT=0`。
测点数与 `9d86e12` 那次读数同值（1491），也就是**回到本页批开始前的基线**，工作树里没有留下半条闸。
"每页可渲染到像素"因此仍是 #10 未结的缺口，改由任务 #63 记账。

## #10 缺口②第二次尝试：机制查到了，闸搬到独立进程 `tools/AstraPagePixels`

### 机制：宿主窗口不是被"撑大"坏的，是被"按名字找部件"这条读法坏的

上一节记的"宿主尺寸只涨不缩"只是四条红里的**一条**（TeachingTip 翻侧）。另外三条建议列表测点的根因，用一次
自包含探路量出来了（`spike/GalleryRender/probe2.log`）：在**同一进程**里渲染 6 张 Gallery 页（3 页 × 两档）之后，
从共享宿主窗口按名字找部件得到

```
host 484x364 box=260 open=True containers=5 itemsHosts=11
  container 98.07x41.78 min=0   visible=True      <- 4 个是 Gallery 页留下的
  container 260x41.78  min=260  visible=True      <- 这条测点自己要的那一个
```

也就是：每渲染一页，那页模板里 realize 出来的下拉子树就被嫁接到宿主窗口的**覆盖层**（overlay layer），
它不是 `Window.Content` 的子节点——所以把 `host.Content` 换成新 Grid 也带不走它（上一节那个"排除"因此是对的、
也是不完整的：内容层清得掉，覆盖层清不掉）。而建议列表那族测点正是从宿主窗口按 `Name` 深度优先找部件，
**第一个命中**就是 Gallery 留下的 98.07/`min=0` 那一个，于是断言读到 0 与 98.06。

试过并把这条路走死的一件事：在 `Dispose` 里调 `PixelHarness.ReleaseHost()` 关掉宿主重建一台。它确实解掉那三条
（覆盖层随窗口一起没了），但把**更糟**的带回来——同一批三类合跑变成 **20 红**（TeachingTip 一族 7 条整片红），
正是记忆里"在同一条 UI 线程上 Show/Close 窗口会毁掉后续弹层断言"的那族：关掉持有激活的窗口把 Win32 激活句柄留在 0。
结论：**这条闸不能住在共享宿主的进程里**，不是阈值问题，也不是尺寸问题。

### 落地：一次性进程，拍"整窗"与"页槽"两张，再拿空槽当底板

`tools/AstraPagePixels/`（Exe，故意**不**进 `FluentJalium.slnx`，由 `tools/Test-AstraGates.ps1` 单独 build + run，
进程名也进了闸口脚本的清理名单）。它真 `Show()` 出出货的 `MainWindow`（设计尺寸 1100x820，内容实测 848,166 px），
每页每档拍三张：

1. **整窗**（`window.Content`）：承托"底色翻档"和品牌绿两条全局主张；
2. **页槽**（`PageHost` 那块 Grid，788x490..2918）：页面自己画进自己格子里多少；
3. **空槽**（把该页从槽里摘下后再拍同一块）：底板。断言"槽内色彩数 **严格大于** 空槽"，并且**空槽必须真的没墨**
   （`Painted(emptySlot) == 0`）——后者是这条闸的牙齿：底板一旦失效，比较就变成自证。

测量（26 条腿，`--report`，`spike/GalleryRender/pagepixels-slot.log`）：整窗点亮像素 848,082..848,166；本档底色
197,022..257,809；**另一档底色在整窗里恒为 0**（这就是 Light↔Dark 真的翻到页身上）；品牌绿 26 帧全 0；
槽内色彩 48..404、槽内点亮 182,017..1,531,859；空槽恒 1 色 0 墨。门限贴在下界：≥800,000 / ≥150,000 /
≥150,000 / ≥40 色。落帧用真的重试循环（最多 10 轮 × 12 帧），每轮泵 900 ms 封顶——静态场景不再发
`CompositionTarget.Rendering`，第一版给了 20 s 预算，结果 5 条腿就吃掉 400 s 超时。

一处形状问题如实记下，别当已通过：**槽是滚动范围、不是视口**，所以 Status 页那两档的槽**永远不落帧**
（`stable=True/False`）——ProgressRing 在折叠线以下一直在 tick，而整窗那张因为它在视口外所以是稳的。
因此"稳定"只对**视口**主张，槽只按"画了多少"判定，不拿稳定性当门槛。

### 四类证据与牙齿

- **构建**：`dotnet build tools/AstraPagePixels` `0 警告 / 0 错误`（探针工程独立）。
- **行为**：26 条腿各自导航到页、拍完摘下再挂回，摘/挂用 `ContentHost.Children`（经代码后置自己的私有视图读到），
  不改 Gallery 任何一行产品代码。
- **像素**：上面那组上下界逐条进断言；`--report` 与断言模式同一套读数。
- **输入**：**无**。这一批仍然不碰真指针，hover/press 的像素通路仍欠（任务 #13）。
- **A/B（承重条验过）**：把"摘下该页"改成永不执行 → 退出码 1，**26 条** `the slot was not actually emptied`
  加 **26 条** `printed nothing its empty slot does not already print`，其余断言照旧绿。改回即 `PASS 13 pages x 2 variants, 0 offender(s)`。
  承重的是"槽 > 空槽"与"空槽无墨"这一对；整窗那四条是全局护栏，单独拿它们当门牙会看错。

### 这条闸不声称的事

① 没有逐页指纹：只说"这页往自己格子里画了东西、比空格子多"，不说画的是不是**该页的那些**控件——
把两页内容互换它不会红（要指纹得给每页签一个色/面积表，未做）；② 无几何与间距主张（页内控件画在哪儿、
离边多远一概没量，#21/#23 的账照旧）；③ 字形不打印（#50）在这条闸上仍是上限：纯文本页过不了"≥40 色"；
④ 不与 WinUI 截图比对；⑤ 入场动画状态（`FluentThemeManager.Enter`）没被驱动，减动效对页面的影响也没量。

### 带这条闸的串行闸口读数（补记，2026-09-22）

`tools/Test-AstraGates.ps1` 加了 "build page pixel gate" 与 "gallery page pixels (13 pages x light, dark)" 两步之后
串行跑完（`spike/GalleryRender/gate-pagepixels.log`）：build `0 警告 / 0 错误` → 整套 **1491/1491**（0 失败 0 跳过、
7 m 4 s）→ 页像素闸 **`PASS 13 pages x 2 variants, 0 offender(s)`** → 调色板三档 `checked=True`
（Light 83 源色 / 101 刷、Dark 同、HighContrast 101 映射键）→ `keys.md is current: 1301 canonical lines.` →
`All Astra gates passed.` → `GATE-EXIT=0`。整轮墙钟约 12 分钟（08:29:07→08:40:53）；两步各自多久没量（闸日志只打顺序、
不带时间戳），要按成本决策的话先给 `Invoke-Step` 加计时再谈。
测点数仍是 1491：这条闸**不在测试装配里**，所以"测点没增"这一次不代表"没新证据"——它带的是自己那 26 条腿的读数，
两件事分开记才不会看错（这也是为什么全量绿不等于页级主张成立）。

## #12 A2 别名层第三轮（`TextPrimary`）：不发行的理由被实测**反过来**了

目标这一手写着"给 `TextPrimary` 发行（会重写 `AstraMenuTests` 的像素主张）"。起手按老规矩做三形读取普查，
结果**否掉了这个前提本身**，所以这一轮没有新行进 `ThemeResources/FrameworkRetints.jalxaml`——它停在四行
（`AccentBrush`/`ControlBorderFocused`/`SurfaceBackground`/`ControlBorder`），净变化只有测试装配里多一条常驻事实。
连那句括号里的担忧也没有落点：`AstraMenuTests.cs` 里 `grep` 到 `TextPrimary`/`TextFillColor`/`1D1D1F`/`6E6E73`/
`Foreground` **全 0 命中**，菜单族的像素主张从不按这个名字写，所以无论发不发都不会"重写"它——下一轮别再照目标原文去找这条不存在的主张。

- **源树侧确实像该发**：`TextPrimary` 在参考树有 8 处按名读点（上面的名次表），这是"看着可发"的全部依据。
- **出货侧量下来恰好相反**。`adaptation/08-themecolors-raw.txt` 第 52 行把 `TextPrimary` 记成
  `tc TextPrimary = #FFFFFFFF | app-resource=SolidColorBrush`——也就是说框架自己就把这个名字**投影成应用级资源**，
  不是"树上没人发、我们补一行"。在一个装了 Astra 门面的活进程里两档各读一次（本轮实测，权威压过那张 harvest 表）：

  ```
  Light: TextPrimary=#FF1D1D1F  twin(TextFillColorPrimaryBrush)=#E4000000  同实例=否  Label前景=#FF6E6E73
  Dark:  TextPrimary=#FFF5F5F7  twin=#FFFFFFFF                              同实例=否  Label前景=#FFD1D1D6
  ```

  三件事：① 这个名字**不是空的**，它随 `Application.ThemeMode` 翻档，落到一支**框架自己的**不透明近黑
  （`#FF1D1D1F`/`#FFF5F5F7`）——注意它**不是**我们出货的 token；② 我们逐字照抄上游的 twin `TextFillColorPrimaryBrush`
  是半透 WinUI 字面（`#E4000000`/`#FFFFFFFF`），与名字投影**不同实例、不同色**；③ 我们从不重模板的那个原生 `Label`
  读**第三种**框架默认墨（`#FF6E6E73`/`#FFD1D1D6`），既不是名字值也不是 twin 值。
- **不发行的理由只剩"没有实证读者"——这一句更正我本批先前的一处判断**：投影 `#FF1D1D1F` 并非我们的 token，
  `#E4000000` 才是 WinUI 字面、才是 1:1 的目标；所以 `<StaticResource TextPrimary → TextFillColorPrimaryBrush>`
  会把按这个名字取值的表面**推向** 1:1，而不是拉离。先前把它写成"改朝 1:1 的反方向"是**反了**。这与 `ControlBorderFocused`
  那轮不同（那轮框架投影是**错的品牌绿** `#FF1E793F` 且有实证读者 `ResolveFocusedBorderBrush`，覆盖既修 bug 又提纯度）。
  别名层进不进一个名字，判据只有一条：**有没有一个我们改不动、又确实按这个名字取值的出货读者**。`TextPrimary` 目前只量到
  `Label` 这一处文本面、而它读第三种墨——没有读者，所以暂不发；**哪天冒出读者，发它是提纯度、不是弄脏**。
- **我自己的前提也错了一次，如实记**：本想在测试里钉一条"这个名字没有出货读者"的 tripwire，写成了
  `Assert.Null(TryFindResource("TextPrimary"))`。第一次跑就红在 `Actual: SolidColorBrush(#FF1D1D1F)`——我先前那条
  "名字处处不出现"的记忆是**错的**（同一批早先的探针读数 `row=SolidColorBrush #E4000000` 本来就已经非空，是我把它记成了"缺席"）。
  改成把**实测的四对色值**钉成断言，而不是钉一个我以为成立的缺席。

- **构建**：全量 `0 警告 / 0 错误`。
- **行为**：新常驻测点 `AstraFrameworkNameResolutionTests.TextPrimary_is_a_theme_tracking_framework_name_the_layer_leaves_alone`
  把上面两档六支色值 + `TextPrimary ≠ twin` 全进断言，本类 **10/10 绿**（原 9 条 + 这一条）。
  **牙齿**：若将来真给 `TextPrimary` 发了别名行，查表拿到的就是 twin → `Assert.Equal(#1D1D1F, lightRow)` 与
  `Assert.NotEqual(lightRow, lightTwin)` 两条一起红；若哪天框架不再按 `ThemeMode` 翻这个名字，Dark 那条 `#F5F5F7` 红。
  也就是"该不该回头评估这个名字"从注释变成会响的门。
- **视觉**：本批**零既有像素主张变化**（不发行，就没有东西被重写）。
- **硬件输入**：不动输入路径。
- **清单/漂移**：`keys.md` 不变（本批不发布公开键，仍 1301 canonical lines）；三档 `checked=True`。

### TextPrimary 单条批次（提交 `efda4ff`）的闸口读数（补记，2026-09-22）

`tools/Test-AstraGates.ps1` 在含本批改动的树上串行跑完（`spike/GalleryRender/gate-textprimary.log`）：build
`0 警告 / 0 错误` → 整套 **1492/1492**（0 失败 0 跳过、7 m 26 s）→ 页像素闸 **`PASS 13 pages x 2 variants, 0 offender(s)`**
→ 调色板三档 `checked=True`（Light/Dark 各 83 源色 101 刷；HighContrast 101 映射键，3 条上游强调 elevation 键因调色板无对应而按住）
→ `keys.md is current: 1301 canonical lines.` → 末行 **`All Astra gates passed.`**。

**测点 1491 → 1492 恰好 +1**，就是本批新那条常驻事实——也就是这条"翻主题到 Dark 再翻回 Light"的共享夹具测点
**没有污染任何后续类**（这条闸的既有教训是夹具态泄漏会成批弄红，+1 无附带红正是它的反证）。
一处如实记下、别糊过去：我给闸日志追加的 `GATE-EXIT=$LASTEXITCODE` 那行**是空的**——`$LASTEXITCODE` 是 PowerShell
变量、外层是 bash，取不到值。所以"整个脚本跑完了"的证据是末行 `All Astra gates passed.`（`$ErrorActionPreference='Stop'`
下任一 `Invoke-Step` 非零都会先中止、打不出这行），不是那行空 echo。

`08-themecolors-raw.txt` 这张表**只当线索、不当结论**：它表头写 Light，可 71 行 `tc` 值全是暗色档
（白字、`WindowBackground=#FF1E1E1E`），与活进程读数系统性相反——归因未做（多半是 harvest 抓的是
`ThemeColors` 结构体的另一套底值，不是应用级投影色），但结论不依赖它：本轮用的是活进程里按 `ThemeMode` 现读的值。
不声称：① 没有对**全部**出货按名读者做穷尽普查（只测了 `Label` 一处原生文本控件；源树那 8 处读点在 26.10.9 的 IL 里
是否都还在、是否都跑在我们改不动的面上，没查）——这是"不发"的**唯一依据**：没有实证读者；若查到读者，发这行是提纯而非弄脏。
② `TextSecondary`/`TextDisabled`/`TextOnAccent` 各自要不要发行仍按同一判据单独量，本轮不替它们下结论。

## #12 A2 别名层第三轮·续：余下三个文本名 + `CaptionFontSize`，按同一把尺各量一次

`efda4ff` 的 `TextPrimary` 定了尺（"有没有一个改不动、又按这个名字取值的出货读者"）之后，这一手把余下三个
palette-family 文本名各量一次（活进程、两档、`TryFindResource(name)` vs 我们的 token），读数全部进了一条参数化事实
`A_remaining_text_name_is_a_live_projection_but_not_our_token`（本类从 10 条增至 **13 条**，多的 3 条腿就是它）：

```
name           Light 投影   Dark  投影   我们的 token(Light/Dark)                 同实例
TextSecondary  #FF6E6E73   #FFD1D1D6   TextFillColorSecondaryBrush #9E000000/#C5FFFFFF   否
TextDisabled   #FFAEAEB2   #FF636366   TextFillColorDisabledBrush  #5C000000/#5DFFFFFF   否
TextOnAccent   #FFFFFFFF   #FFFFFFFF   TextOnAccentFillColorPrimaryBrush #FFFFFF/#000000  否
```

- **`TextSecondary` / `TextDisabled`：投影都量到，但决策不是一条**。两者都是随档翻的不透明 macOS 味灰阶、与我们半透
  WinUI 字面 token **不同实例不同色**——且**发 `name→token` 是把读者推向 1:1，不是拉离**（token 才是 WinUI 字面）。所以
  "发不发"只取决于**有没有一个我们改不动、又按这个名字取值的读者**，而不是"投影对不对"。
  - `TextSecondary`：本批只在 `Label` 这类面上看，读到的是第三种默认墨、不是这个名字 → **没有量到自绘读者，暂缓**（未穷尽）。
  - `TextDisabled`：**不同——它是下一轮该发的那一个**。菜单批已把 `MenuItem` 判成"存模板却不实例化、只走 `OnRender`
    自绘 + `Resolve*Brush`"，即我们**重不了模板**；而参考树 census 记着框架按名字读 `TextDisabled`
    （`Menu.cs:1172`、`MenuFlyoutItem.cs:208` 先试 `OneTextDisabled` 再试 `TextDisabled`）。"参考树"不是 26.10.9 权威，
    所以**这一条要在出货运行时实测**（挂一个禁用的自绘 `MenuItem`、装哨兵 `TextDisabled`、看它跟不跟），不是照抄树。
    目标那句"`TextDisabled` 翻 5 条禁用墨迹事实"到这里才真正对上：一旦发行了，那 5 条从框架不透灰变成我们 WinUI 半透值，
    是**预期内的重钉**（#56 批的 retint 爆炸半径同款），不是回避理由。**本轮不发它，把它作为独立一轮：实测读者 → 发行 → 重钉。**
- **`TextOnAccent`：形状不同，仍暂缓，但记清楚它怪在哪**。它**不在** `08` 那张 71 行 `ThemeColors` 投影表里
  （源未归因，多半来自比 71 更广的框架资源面），可活进程里 `TryFindResource("TextOnAccent")` 非空，而且是
  **恒白**（Light/Dark 都 `#FFFFFFFF`），而我们的 token 是白/黑随档翻（`#FFFFFF`/`#000000`）。也就是说：谁按这个名字
  取值，在 Dark 下会拿到**恒定白**而非我们的档敏感值——这正是"强调底上文字"该翻没翻的隐患。但**本批没量到任何控件读它**
  （WinUI 把它用在选中药丸/开关/Calendar 的反色文字上，而这些我们要么自绘要么还没上：Calendar/DatePicker 不在 60 行宇宙里）。
  所以按同一把尺：**没量到读者 → 暂缓**；等哪天进 Calendar 族或实测到某面读它，这一轮的行是**提纯度**的活。
  它的恒白投影已被那条事实钉住（Dark 腿 `#FFFFFFFF` + `NotEqual(dark, darkTwin)` 就是这条怪的证人）。
- **`CaptionFontSize`：Known Gap，无载体**。它不是 `ThemeColors` 笔刷而是字号，本 reader 解析不了 `x:Double`/`sys:Double`
  资源行（`jalium-xaml-and-test-gotchas` 与 #54 已各自实测过这条静默丢弃），所以连"能不能按名字投影"都无从谈起——
  没有可断的活行，写进 Known Gaps，不硬造消费点。**别名层这条杠杆只吃 Brush 形名字；字号/Thickness 等没有框架按名
  现查的读者，就不在 A2 的射程内。**

- **构建**：全量 `0 警告 / 0 错误`。
- **行为**：新参数化事实 3 条腿（`AstraFrameworkNameResolutionTests` 13/13 绿）。**牙齿**：给任一名字发 `name→token`
  别名行 → 该腿读到的就是 token → 具名的 Light/Dark 色值断言与 `NotSame(row,twin)` 一起红；`TextOnAccent` 那条若哪天
  框架不再恒白（开始随档翻），Dark 腿 `#FFFFFFFF` 红。
- **视觉**：零既有像素主张变化（三个都不发）。
- **硬件输入**：不动。
- **清单/漂移**：`keys.md` 不变（仍 1301，本批不发布公开键）；`FrameworkRetints.jalxaml` 停在四行不变。

### 第三轮·续的串行闸口读数（补记，2026-09-22）

修掉一条新引入的 `CS8603`（`InkedForeground` 返回类型改 `Brush?`，出货全库唯一一处可能返回 null 的 helper）之后，
`tools/Test-AstraGates.ps1` 在含本批改动的树上串行跑完（`spike/GalleryRender/gate-textnames.log`）：build
`0 警告 / 0 错误` → 整套 **1495/1495**（0 失败 0 跳过、6 m 57 s）→ 页像素闸 **`PASS 13 pages x 2 variants, 0 offender(s)`**
→ 调色板三档 `checked=True` → `keys.md is current: 1301 canonical lines.`（本批不发布公开键）→ 末行
`All Astra gates passed.` → 脚本自记 **`GATE-EXIT=0`**（这回用 bash `$?` 取的真实退出码，不是上次那行空的 `$LASTEXITCODE`）。

**测点 1492 → 1495 = +3**，正是新参数化事实的三条腿；`TextDisabled` 三个名字都不发行，所以既有禁用墨迹那族事实**一条没翻**
（全量里照旧绿即反证）。这一批的产出是"量清 + 更正一处方向性误判 + 把 `TextDisabled` 立成下一轮"，不是发行；
`FrameworkRetints.jalxaml` 仍停在四行。

## #12 A2 别名层第三轮·再续：实测给 `TextPrimary` 找到了读者，同时削掉了 `TextDisabled` 的依据

上一手留的话是"`TextDisabled` 是下一轮该发的那一个，去实测读者"。这一手去测了，**方向是反的：真正有读者的是
`TextPrimary`，而 `TextDisabled` 连上一手引的那条依据都没测到**。
在 26.10.9 出货运行时上挂自绘 `MenuItem`、往 `Application.Resources` 装哨兵笔刷、逐次读框架自己的解析结果
（`MenuItem.ResolvePrimaryTextBrush`，非公开方法；反射**只在测试装配里用**，AGENTS.md 禁的是出货代码反射私有字段）：

```
启用态:  基准=#FF1D1D1F | +TextDisabled:#FF1D1D1F | +OneTextDisabled:#FF1D1D1F | +TextPrimary:SENTINEL
禁用态:  基准=#FF1D1D1F | +TextPrimary:SENTINEL
```

- **`TextPrimary` 有实证读者，而且是改不动的那种**。`MenuItem` 存了模板却从不实例化，墨色走 `OnRender` 自绘 +
  按名现查（与 `ControlBorderFocused` 那轮的 `ResolveFocusedBorderBrush` 同一形状）：装 `TextPrimary` 哨兵，它就跟；
  名字不装，它落框架投影 `#FF1D1D1F`。按 `efda4ff` 定下的唯一判据（"有没有一个改不动、又确实按这个名字取值的出货读者"），
  这一行现在是**该发**——而且发它是**提纯度**（把按名取值的表面从 macOS 不透灰推向 WinUI 字面 `#E4000000`），
  不是拉离 1:1。`efda4ff` 那条测点自己写了 tripwire："若哪天冒出读者，发它是提纯而非弄脏"——**读者冒出来了，
  这是它按设计响的第一次**。
- **`TextDisabled`：上一手那条"它是下一轮该发的"目前没有实证支撑，但也不能判死**。参考树记的 `Menu.cs:1172` /
  `MenuFlyoutItem.cs:208`（先试 `OneTextDisabled` 再试 `TextDisabled`）在我测的这条路径上**没有对应行为**：
  `MenuItem` 置禁用后 `ResolvePrimaryTextBrush` 的结果**一动不动**（仍 `#FF1D1D1F`），装 `TextDisabled` 或
  `OneTextDisabled` 哨兵也不跟。**这条判据只覆盖这一个方法**——我没有普查 `MenuItem` 是否另有禁用墨的解析入口，
  所以"自绘菜单不读禁用名"是实测，"`TextDisabled` 没有出货读者"**不是**（本轮没资格下这句）。
  而且有一条未解释的线索指向这里有活：出货运行时上禁用 `TextBox` 的前景是 `#FFAEAEB2`，与 `TextDisabled` 的 Light
  投影**同色**，而 `#FFAEAEB2` 正是我们够不到的那条框架本地值。所以 `TextDisabled` 欠的不是"再测一次菜单"，
  是一次**按名 vs 走 `ThemeColors`** 的判别（哨兵法，`ControlBorderFocused`/本批同款）：如果那片禁用墨是按名读的，
  发这行才既修表面又提纯度；如果是直接读 `ThemeColors`，那它就是 `jalium-theme-pipeline-constraints` 记过的那类
  "名字撞车不是钩子"，进 Known Gaps。**下一轮做这个判别，本轮不发。**
- **产出只有一条常驻事实 + 一处更正，没有新行**。`FrameworkRetints.jalxaml` 仍停在四行。测点方法
  `TextPrimary_is_a_theme_tracking_framework_name_the_layer_leaves_alone` 改名为
  `TextPrimary_projects_a_framework_ink_awaiting_its_row`——旧名"the layer leaves alone"讲的是一个已被推翻的判据，
  留着会把下一手误导回去；注释同时写明：**发行那天这条事实的色值断言会变红，那是行落地、不是回归**。

- **构建**：全量 `0 警告 / 0 错误`。
- **行为**：新常驻测点 `A_self_drawn_menu_item_reads_the_primary_text_name`（本类 13 → **14 条**）。
  **牙齿**：若 26.10.9 换成不按名现查（比如改成缓存/改读别的名字），`ReferenceEquals(sentinel, …)` 那条红并且
  失败信息直接说明"别名行够不到东西"；若框架哪天让禁用态改读 `TextDisabled`，本事实的禁用腿不成立——它没测禁用，
  所以不会假绿也不会假红，那一条仍归 `TextDisabled` 自己的普查。
- **视觉**：零既有像素主张变化。**没有**捕获任何显示中的菜单：反射只证明"名字被按次查询"，不证明像素，
  弹层/自绘面的真指针像素仍是 #13 的账。
- **硬件输入**：不动。
- **清单/漂移**：`keys.md` 不变（仍 1301）；三档 `checked=True`。
- **不声称**：① 没有对**其余** `Resolve*Brush` 自绘读者做穷尽普查（只测了 `MenuItem` 这一处的这一条解析），
  因此本批对 `TextPrimary` 是"量到了读者、该发"，对 `TextDisabled` 只是"**这一条路径**不读它"，两者不对称，别把后者当前者用；
  ② 没有量 `TextPrimary` 别名行的**爆炸半径**——发行之余哪些既有事实会翻，是发行那一轮要产出的清单，本轮不预判；
  ③ 发行不在本批；④ 反射读到的解析结果**不是像素**，`MenuItem` 禁用墨到底画在哪、由哪个入口决定，本批没有捕获。

### 这一手该发的那行：`TextPrimary → TextFillColorPrimaryBrush`（下一步，独立闸口批）

按目标那句"每轮：三形读取普查 → 列出将被重写的事实 → 发布行 → `Report-AstraResourceKeys.ps1` → 全量闸口"，
前三步已经走完（普查=上面两行哨兵读数；被重写的事实=已知至少 `TextPrimary_projects_a_framework_ink_awaiting_its_row`
的 Light/Dark 色值两对，其余要在 `A_retint_row_follows_the_theme_flip` 加 `TextPrimary` 两腿后由全量闸口报出来）。
发行单独成批，因为别名层的既往教训是**爆炸半径要一次一清**（`SurfaceBackground`/`ControlBorder` 那轮全套像素主张
一格没动，是因为先量过；不是因为安全）。

### 再续批（读者证明）的串行闸口读数（补记，2026-09-22）

`tools/Test-AstraGates.ps1` 串行跑完（`spike/GalleryRender/gate-menureader.log`，脚本自记 **`GATE-EXIT=0`**）：
build `0 警告 / 0 错误` → 整套 **1496/1496**（0 失败 0 跳过、7 m 31 s）→ 页像素闸 **`PASS 13 pages x 2 variants,
0 offender(s)`** → 调色板三档 `checked=True` → `keys.md is current: 1301 canonical lines.`（本批不发布公开键）
→ 末行 `All Astra gates passed.`。

**测点 1495 → 1496 = +1**，就是新那条读者事实；`FrameworkRetints.jalxaml` 没动，所以像素主张与键清单两条都不该变，
读回来的确没变（页闸 0 offender、`keys.md` 仍 1301）。

一处**时序上的如实交代**，别糊过去：闸口的 build 步在**本批最后一次编辑之前**就跑完了。那次编辑只改了两行**注释**
（把 `TextPrimary_projects_...` 里"别名行覆盖的是一个已经就是我们的墨"这句**方向反了**的话改成实测版），
没有改任何可执行代码，所以 1496 那个二进制的行为与提交树一致。为了让提交的字节也被编译验证过，
我在闸后又单独 build 了一次（`0 警告 / 0 错误`）并把本类重跑（**14/14 绿**）。也就是说：
**全量闸口跑的是行为等价的树，注释增量由 build + 本类focused 覆盖**，不是"整条闸在最终字节上绿过"。

顺带记一条**页闸读数里的不稳定**，属于 #47/#35 那一族而不是本批引入的：`status Dark: stable=True/False`
——13 页 × 两档里只有 status 的暗色第二拍判为不稳，但两拍的 `colours/over` 与 `painted` 完全一致
（`1189783px over 1 colours 0px`）， offender 仍 0，所以闸判 PASS。本批没碰 status 族也没碰页闸，
这条在 `cc3d802` 之前的日志里是不是常发**未查**，留给 #47/#35 那一次机制普查，别在这里当成新回归、也别当成旧账已清。

## #12 A2 别名层第三轮·再再续：第一次把"按名现查"量到**像素**——有像素读者的是 `TextSecondary`

上一手的读者证明只到反射为止。这一手把同一个哨兵换成**渲染计数**：在 `Application.Resources` 里按名字装一支
探针笔刷，拍 `MenuFlyoutSubItem`（240x38），数颜色。读数是这批里第一条真正到像素的（活进程、Light 档、
`spike/GalleryRender/diag-secondary.log` / `diag-secondary2.log`，仪器已摘除）：

```
label-ink(基准)=#FF6E6E73 | row(基准)          探针=0  灰字=38  top=#000000x9082 #6E6E73x38
                | row(+TextSecondary) 探针=38 灰字=0   label=#FF112233
                | row(+TextPrimary)   探针=0  灰字=38  label=#FF6E6E73
                | row(+TextDisabled)  探针=0  灰字=38  label=#FF6E6E73
                | row(撤哨兵后)       探针=0  灰字=38  label=#FF6E6E73
                | item(基准/+TextPrimary/+TextSecondary) 全画面只有 #000000x6400（200x32 无墨）
```

- **`TextSecondary` 是按名现查、而且它的值真的被画出来**。装哨兵把整片字形换色（38 像素全替、灰字归零），
  撤掉精确复原，所以这不是一次性解析而是每次调用重查；同一个名字还同时决定我们**从不重模板**的原生 `Label`
  的前景（`#FF6E6E73` → `#FF112233` → 复原）。`AstraMenuTests` 那句 `FrameworkRowText = #6E6E73` 由此**从"框架的
  第三种墨"升格成"某个名字的投影"**——它有一个能被别名层够到的读者。
- **上一手那条"第三种默认墨"是被精确化、不是被推翻**：`TextPrimary_projects_...` 里 Label 读到的既不是名字值也不是
  twin 值，这句仍然成立；但"第三种"不是第三种机制，它就是 `TextSecondary` 的投影值。测点注释里把这条写明，
  并指向新的证明事实。
- **`TextPrimary` 的读者仍只在反射里成立，像素侧本仪器量不到**：同一枚哨兵动不了 flyout 行文字，而 `MenuItem`
  那一拍整幅只有 `#000000`（200x32 无墨）——这与 #50"文本字形在任何捕获通路都拿不到墨"是同一堵墙。
  所以两行的**证据等级不同**：`TextSecondary` 有像素证人，`TextPrimary` 只有解析证人。**发行顺序按证据强度排，
  先 `TextSecondary` 再 `TextPrimary`**，上一手"TextPrimary 是该发的那一个"改成"该发，但它前面还排着一个更硬的"。

- **发行 `TextSecondary → TextFillColorSecondaryBrush` 会重写哪些事实（发之前逐条点名，不靠闸口去撞）**：
  1. `AstraMenuTests.The_controls_paint_their_own_rule_and_text_and_our_rows_reach_neither` 的
     `text.Count(FrameworkRowText) > 8` —— 行文字从 `#6E6E73` 变成 token 的 `#9E000000` 压在本底上的合成色，
     这条**必红**，要按合成值重钉（`PixelHarness.Over` 能算，不猜）。
  2. `AstraFrameworkNameResolutionTests.TextPrimary_projects_...` 的 `lightLabel`/`darkLabel` 两条腿（`#6E6E73`/`#D1D1D6`）
     与 `A_remaining_text_name_...` 的 `TextSecondary` 腿 —— 都**必红**，因为读的正是这个值。
  3. `A_retint_row_follows_the_theme_flip` 要加 `TextSecondary` 两腿（翻档跟到各自变体）。
  除这三处之外，`grep` 全测试工程 `6E6E73|D1D1D6` 没有第四个落点。目标那句"`TextSecondary` 会重写 `AstraMenuTests`
  的像素主张"至此**由实测成立**（上一手我按字面去找 `AstraMenuTests` 里的 `TextSecondary` 字样，0 命中就判它没有落点——
  那是找错了东西：这条主张是按**色值**写的，不是按键名）。

- **构建**：`0 警告 / 0 错误`。
- **行为 + 视觉**：新常驻测点 4 条腿（本类 14 → **18**）：`The_flyout_rows_own_text_is_painted_from_the_secondary_text_name`
  （3 腿，含两条阴性对照）与 `A_native_label_resolves_its_ink_from_the_same_name`。这批的测点**本身就是像素读数**，
  所以行为与视觉两类在这里是同一份证据，如实标注，不拆成两份充数。**牙齿**：阴性腿若哪天 `TextPrimary`/`TextDisabled`
  开始动这片墨 → 探针计数由 0 变正 → 红；框架哪天改成一次性解析 → 复原腿的 `灰字=38` 红；名字哪天不再决定
  `Label` 前景 → `Probe` 断言红。
- **硬件输入**：不动。
- **清单/漂移**：`keys.md` 不变（仍 1301，本批不发布公开键）；`FrameworkRetints.jalxaml` 仍停在四行。
- **不声称**：① 没有证明 `TextSecondary` 是**唯一**够到像素的按名读者（其它自绘面没测）；② 暗色档没在这台仪器上拍
  （只测了 Light 的像素翻转，Dark 只有投影色值）；③ 反射读数不是像素，`MenuItem` 那一幅是"无墨"不是"墨错了"；
  ④ 本批发行零行。

### 像素读者批的串行闸口读数（补记，2026-09-22）

`tools/Test-AstraGates.ps1` 在最终字节上串行跑完（`spike/GalleryRender/gate-secondarypixels.log`，
包装器自记 **`GATE-EXIT=0`**）：build `0 警告 / 0 错误` → 整套 **1500/1500**（0 失败 0 跳过、8 m 52 s）
→ 页像素闸 `PASS 13 pages x 2 variants, 0 offender(s)` → 三档 `checked=True` → `keys.md is current: 1301 canonical lines.`
→ 末行 `All Astra gates passed.`。**测点 1496 → 1500 = +4**，正是这一批的三条阴性/阳性腿加一条 `Label` 事实，
**没有附带红**——这一条在这批特别要紧：两条新事实都会往 `Application.Resources` 按名字装笔刷再摘掉，
而这条管线历史上"改一个应用级条目"就静默弄红过后面的 Button 像素主张（`jalium-theme-pipeline-constraints`）。
全量里其余 1482 条照旧绿就是这层泄漏没发生的反证。

同一份日志里 `status Dark` 第二回 `stable=True/False`，且这一页的 slot 读数在两次运行之间从 293 变成 295，
而这两批之间 `src/` 一格没动。也就是说这条不稳定**不来自本批**，而且它的抖动面比"某一拍不稳"更宽（同树同码两跑不同数）。
仍归 #47/#35 那族，本批不追；这里记下来是为了让那一族开工时知道**基线读数本身会漂**，别把漂当成回归。

## #12 A2 别名层第四轮：`TextSecondary → TextFillColorSecondaryBrush` 发行，这是这一层第一次改动**出货像素**

上一批点名的三条必红事实全部兑现，兑现的方式比预想的更有意思，所以逐条记。`ThemeResources/FrameworkRetints.jalxaml`
从四行变成五行。

- **红在哪、为什么红**（发行后第一次跑两个受影响类：**8 条腿红**，`spike/GalleryRender/publish-secondary1.log`）：
  - `AstraMenuTests.The_controls_paint_their_own_rule_and_text_and_our_rows_reach_neither` 红在
    `Assert.Equal(0, text.Count(ChevronSentinel))`，**Actual: 38**。这条的主张被**反过来了**：过去量到的是
    "我们改 `TextFillColorSecondaryBrush` 到不了自绘文字"，现在同一支哨兵**到了**——因为名字被别名到调色板实例，
    `OverrideBrush` 挪的就是那个实例。这正是 A2 这一层存在的理由，所以改的是**主张本身**：事实更名为
    `The_alias_layer_reaches_the_rows_own_text_while_our_rows_still_reach_the_rule`，分隔线那半仍然成立
    （装哨兵进去 `Count(SeparatorSentinel)=0`），文字这半从"够不到"改成"通过名字够到了"。
  - `AstraMenuTests.The_sub_items_own_paint_follows_the_theme` 红在 `light capture is empty: #000000x9120`——
    **这一条我上一批没点到名**。原因不是判据错，而是我的普查方法有洞：我按色值 `grep 6E6E73|D1D1D6` 找落点，
    这条事实是**比较两拍**而不是写死颜色，所以字面上没有可 grep 的值。教训写进下面的方法账。
- **半透明墨在黑色宿主上等于没有墨**：token 是 `#9E000000`，压在这台 harness 的默认底（黑）上合成结果就是黑，
  9120/9120 像素全黑。这不是"变暗了一点"，是**主张从可证伪变成不可证伪**。修法照 #59/#60 那两批的既有仪器：
  两档各配自己的不透明底板（`PixelHarness.Backdrop`），断言 `PixelHarness.Over` 算出来的合成值，
  不写死我手算的数、也不写回刷自己的字节。`AstraFrameworkNameResolutionTests` 的 flyout 像素事实同一处理。
- **其余五条腿**：`A_retint_row_follows_the_theme_flip` 补 `TextSecondary` 两腿（两档都读到调色板实例本身）；
  `A_remaining_text_name_is_a_live_projection_but_not_our_token` 的 `TextSecondary` 腿**删除**而不是改判——它三条主张
  （非空、不同实例、不同色）如今每条都按设计为假，留着就是拿一条永久假的事实守一个已经落地的决定；同一仪器在
  flip 两腿 + flyout 像素三腿 + `Label` 一条事实上覆盖得更好，这件事写在被删那条腿的注释里，没有静默消失。
  `TextPrimary_projects_...` 的两条 `Label` 腿改成 token 值（`#9E000000`/`#C5FFFFFF`）。
- **牙齿不是口头保证**：把这一行临时改名再跑一次两个类（`spike/GalleryRender/publish-ab.log`），
  **9 条腿红**，含 flyout 像素事实的全部三条腿（连阴性腿也红，因为底板基准色就是它守的）、flip 两腿、
  `Label` 事实、`TextPrimary` 事实、两条 menus 事实。也就是说上面每一条改判后的主张都**真的**依赖这五行里的第五行，
  不是一组怎么跑都绿的断言。

- **构建**：`0 警告 / 0 错误`（中途一次 `CS0246`：`Sample` 是 `PixelHarness` 的嵌套类型，helper 返回类型要写全）。
- **行为**：受影响两类 **128/128 绿**（127 → 128：flip +2 腿、remaining-text −1 腿）。
- **视觉**：这是 A2 层**第一次**让出货像素移动——菜单行文字与所有我们不改模板的原生文本面，从框架不透灰
  变成 WinUI 半透字面值。证据是 `AstraMenuTests` 与 `AstraFrameworkNameResolutionTests` 里那几条**逐像素计数**
  的事实（含底板合成值），**不是**任何截图主张。**这一条我先前写的话要更正**：我本来打算说"这次移动的可见性由
  #63 的页闸两档全拍覆盖"，实测**恰好相反**——页闸在发行前后对 menus 页给的是**一模一样**的读数
  （`menus Light slot 80 colours 335672px over 1 colours 0px`、`menus Dark slot 48 colours 335671px …`，
  两次运行逐字节相同）。原因是 #50：文本字形到不了捕获，所以"行文字换了墨"这件事在这台仪器上**根本看不见**。
  页闸能证的仍然只有表面身份/半径/绿味那几类，它既没证伪也没证实这一行。
  也没有真指针/键盘输入证据（#13 仍欠）。
- **硬件输入**：不动。
- **清单/漂移**：`FrameworkRetints.jalxaml` 四行 → 五行；`keys.md` 1301 → **1302**（新增
  `| StaticResource | `TextSecondary` | `TextFillColorSecondaryBrush` |` 一行，token 层 777 → 778）。
  这一族是**第四个落点、也是我上一批没点名的一个**：`AstraPublicKeysInventoryTests` 两条事实
  （逐键名册 + 文档里的 Totals 行）会因任何新公开键红，属于目标那句"`Report-AstraResourceKeys.ps1`"本来就预告过的
  一类——它是**清单**账不是**像素**账，重新生成即结清，不需要改判任何主张。
- **方法账（这轮学的一条）**：按**色值** grep 只能找到写死颜色的主张；**比较两拍**、**数极值**、
  **算合成色**这三类像素主张没有可 grep 的字面值，只能用"改一次、看谁红"来枚举。以后发行前的爆炸半径普查，
  以 A/B 红名单为准，色值 grep 只作为预筛。

### TextSecondary 发行批的串行闸口读数（补记，2026-09-22）

第一跑**是红的**，而且红得有价值（`spike/GalleryRender/gate-textsecondary.log`，包装器 `GATE-EXIT=1`）：
build `0 警告 / 0 错误` → 整套 **1499/1501，2 条失败**，两条都是
`Resources.AstraPublicKeysInventoryTests`（逐键名册 + 文档 Totals 行），也就是新公开键该引起的**清单**账，
不是行为回归。闸口在 test 步就中止，页闸/调色板/键清单三步没跑。
`tools/Report-AstraResourceKeys.ps1` 重新生成后 diff 干净：`keys.md` 1301 → 1302，`FrameworkRetints.jalxaml`
段 4 rows → 5 rows，token 层 777 → 778，只多出 `TextSecondary` 那一行。

第二跑全绿（`spike/GalleryRender/gate-textsecondary2.log`，包装器自记 **`GATE-EXIT=0`**）：build
`0 警告 / 0 错误` → 整套 **1501/1501**（0 失败 0 跳过、7 m 19 s）→ 页闸 `PASS 13 pages x 2 variants,
0 offender(s)` → 三档 `checked=True` → `keys.md is current: 1302 canonical lines.` → 末行
`All Astra gates passed.`。

两处如实标注，别让读的人以为我藏了：
1. **测点 1500 → 1501 = +1**，来自 flip 的 +2 腿减掉被删的那条 `TextSecondary` 腿。发行没有弄红任何**别的**类——
   第一批 8 条红全部落在点名范围内（两个受影响类），第二批 2 条红是清单账，合起来这批的移动面就是这些。
2. 这条闸的**后台任务通知写的是 "exit code 0"，而真实管道退出码是 1**。判断依据只能是包装器自己 echo 的
   `GATE-EXIT=` 和日志末行，不是调度器的完成通知（这是既有教训"通知会撒谎"的又一次命中，不是新事）。

和上一批同样的时序如实记一次：`GATE-EXIT=0` 那次全绿跑完之后，我又改了 `FrameworkRetints.jalxaml` 里
`TextSecondary` 那段的**注释正文**（把"行文字那半被反过来"写进行证据注释），并补了上面两条订正。所以那次闸
编进去的是注释增量之前的字典。为让提交的字节被验证过，改后重新 `dotnet build` 整套（`0 警告 / 0 错误`，
Jalxaml 源生成器吃得下这段注释）并把三个受影响类重跑（Name Resolution + Menu + PublicKeysInventory，
**130/130 绿**）。也就是说：全量闸口跑的是行为等价的树，注释增量由 build + 这三类覆盖，
不是"整条闸在最终字节上绿过"。

## #12 A2 别名层第五轮：`TextPrimary → TextFillColorPrimaryBrush` 发行——证据等级比上一行低，这事先写在行注释里

`FrameworkRetints.jalxaml` 第五行 → 第六行。这一行**有读者、没有像素证人**：`MenuItem` 自绘、按名现查
（`8f36012` 量到），但每一张拍下来的 `MenuItem` 都是整幅 `#000000`（#50 那堵字形的墙）。所以它买的是
**按名解析层的纯度**，不是一处看得见的外观修正——这句话写进了行注释本身，而不是只写在账本里，
免得下一只手把它的形状当成"像素证过的行"来复用。

- **爆炸半径比上一行小得多，而且第一红是仪器自己的错**（`spike/GalleryRender/publish-primary1.log`，
  单条红：`Expected: #E4000000 / Actual: #FFFFFFFF`）。原因值得单独记：
  **`Assert.Multiple` 把所有 lambda 推迟到块尾执行，而调色板是就地重染同一个实例**——所以跨档"举着刷子"等于
  举着最后一个档的颜色。per-variant 的色值主张必须**在读取当下把 `Color`（结构体）快照下来**，不能存 brush 引用。
  这条不是这一行造成的，是这台仪器一直如此、我第一次这样写。订正后新增两条正向读数：
  `Same(lightTwin, darkTwin)`（别名转发的确实是调色板那只活对象，两档共用一实例）与
  `Same(lightTwin, MenuItem 解析器返回值)`（**这一行真的够到框架自己那个改不动的读者**）。
- **`TextPrimary_projects_...` 那条"发行前基线"事实是替换、不是改判**：发行后那个基线**从查表里再也读不到了**
  （名字被别名占了），留着断言就是断言一件无法再观测的事。旧值 `#FF1D1D1F`/`#FFF5F5F7` 退到本账本存底，
  测点换成与 `ControlBorderFocused` 同形的 `The_published_row_moves_the_frameworks_primary_text_name_onto_our_ink`。
  另外两腿 `Label` 色值改为"仍读 secondary token"——两个名字必须还可分辨。
- **`AstraForegroundRoutingTests` 那 12 条腿的牙齿重验过一次，方法是换目标而不是删行**
  （`spike/GalleryRender/ab-primary-teeth.log`）：把 resting 格子的键换成 `MenuFlyoutSubItemForegroundDisabled`
  之后，两条腿立刻读成 `#5C000000`（Light）/ `#5DFFFFFF`（Dark），也就是**格子换了、像素跟着换**，主张还活着。
  为什么不删行来验：**发了别名之后"删行"这条反事实本身会失效**——格子不在时控件回落到继承来的墨，
  而那墨如今正是别名转发的同一实例，删了也不红。这是"修复把检验仪器一起改掉"的一个实例：
  以后验别名行的牙齿要用**换目标**，不能用**删格子**。

- **构建**：`0 警告 / 0 错误`。**行为**：受影响三类 **152/152 绿**。
- **视觉**：**本行零像素主张**（如上，`MenuItem` 拍不出墨）。#63 页闸与本行无关，它看不见文本墨。
- **硬件输入**：不动。
- **清单/漂移**：`keys.md` 1302 → **1303**（新增 `TextPrimary` 一行，retints 段 5 → 6 rows）。
- **不声称**：① 不声称这一行改变了任何看得见的地方；② 没有对 `TextPrimary` 做像素级普查（做不出来）；
  ③ `TextDisabled` / `TextOnAccent` 仍无读者、`CaptionFontSize` 仍是 Known Gap，本轮不替它们改判。

### TextPrimary 发行批的串行闸口读数（补记，2026-09-22）

`tools/Test-AstraGates.ps1`（`spike/GalleryRender/gate-textprimary.log`，包装器自记 **`GATE-EXIT=0`**）：
build `0 警告 / 0 错误` → 整套 **1503/1503**（0 失败 0 跳过、7 m 9 s）→ 页闸 `PASS 13 pages x 2 variants,
0 offender(s)` → 三档 `checked=True` → `keys.md is current: 1303 canonical lines.` → `All Astra gates passed.`。

**测点 1501 → 1503 = +2**：换掉的那条基线事实与新的 `The_published_row_...` 相互抵掉，净增的是 flip 那两条
`TextPrimary` 腿。**这一跑是在最终字节上跑的**（上一批那种"闸后又改注释"的时序问题这次没有发生：
行注释与订正都在闸前写完，闸后只动了本账本这份不被编译的文档）。

最后钉一次口径，因为这一行恰好是那种"换别的方法就验不动"的：本行的证据止于"框架自己的解析器返回我们那只实例"，
**没有任何一条断言说它长得好看了**；`MenuItem` 的可见墨仍归 #50/#13 的账。下一次谁要把这行的形状当模板，
先读行注释里那句"证据等级比上一行低"。

## 目标项 3：排印层从 `ported` 推到 `audited`——十个键逐字抄，两处宿主没有成员，七个数字只能空着（2026-09-22）

隐式样式宇宙里最后一条 `ported` 行是 `TextBlock` → `ThemeResources/Typography.jalxaml`，gap 写的是
"字号阶梯没有逐键对齐上游"。对齐之前有一件事必须先量而不是先猜：上游
`CommonStyles/TextBlock_themeresources.xaml`（`19e3bdc3`）那八个 setter 里有几个本运行时能表达，
以及那七个 `x:Double` 字号行到底能不能带住数字。仪器沿用 `spike/DoubleRowProbe`，加 `typography` 模式
（每个属性**单独成字典**——一个宿主没有的成员会让整份字典加载失败，八个一起放就分不清是谁），
读数全量落在 `adaptation/13`。

- **六个属性落得上、两个宿主没成员**：`TextLineBounds` / `OpticalMarginAlignment` 在 26.10.9 的
  `TextBlock` 上反射不到成员，写成 setter 直接 `XamlParseException`，所以删掉它们不是风格选择；
  `XamlAutoFontFamily` 那个值更阴——属性存在、加载不报错，读回来是一个**名字叫 `XamlAutoFontFamily` 的族**，
  落到 fallback 而不是系统 UI 字体，于是这条也删，并在文档里写明它"看起来正确"的那一面。
  留下的四条（`FontWeight`/`TextTrimming`/`TextWrapping`/`LineStackingStrategy`）里只有 `LineStacking` 的
  上游值恰好等于宿主默认（`MaxHeight`），读回分不出"落上了"和"没这条"，所以它只进形状断言不进到达断言，
  探它的时候改用宿主不用的 `BlockLineHeight`。
- **链形与键名一次改完**：旧文件六个键、链是 `Caption`→`Body`、`Subtitle`/`Title`/`LargeTitle`→`BodyStrong`，
  且把上游 `TitleLargeTextBlockStyle` 叫成 `LargeTitleTextBlockStyle`。现在十个键逐个挂 `Base`，
  补 `Base`/`BodyLarge`/`BodyLargeStrong`/`Display` 四个，改名那条**零消费点**（`src`/`samples`/`tests`/`tools` 全量
  反查），所以直接改、不留别名。全库产品样式里没有任何一条继承排印样式（只有 `Typography.jalxaml` 自己），
  爆炸半径是 Gallery 的 522 处引用（`Body`/`BodyStrong`/`Caption`/`Subtitle`/`Title` = 146/2/211/128/35 行）
  + 隐式 `TextBlock` 行；权威 blob 记在文件头（`08cbcf9c…` @`19e3bdc3`）。
- **删掉我们自造的两条 `Foreground`，墨还是我们的**：上游 `Body`/`Caption` 都不写墨。删掉之后一个
  样式不写 `Foreground` 的块读回 `TextFillColorPrimaryBrush` 那一个实例（宿主按 `TextPrimary` 现查，
  而别名层已把那个名字指向我们的令牌）。**这是这批唯一的用户可见变色**：`CaptionTextBlockStyle`
  在 Gallery 211 处从次级灰变初级墨，这才是上游的 Caption；某页真需要次级灰应由那页点名，不由共享样式替全库决定。
- **七个数字仍然发不出去，而且理由换了一个更准的**：不是"数值令牌没用"，是**这份 reader 把 markup 里的数字读成 0**
  （`x:Double` 直接加载失败、`sys:Double`/`sys:Int32` 归零、`sys:String` 保住文本却过不到 Double 属性），
  而 `{ThemeResource}` → setter → DP 这段路是活的——**同一个 key 由 C# 放 `18d` 就落到 `FontSize=18`**。
  所以"C# 种这七行"是可做的，本批**明确不做**并把三条理由写进 `adaptation/13` 结论四（代码侧目前只有
  `MergedDictionaries.Add` 整份并入这一条路，种单行会开新机制；`keys.md` 从 `.jalxaml` 生成，代码种的行会漏账；
  这七个数不随主题变，字面量与行承载的值相同）。代价照实记：按名字查 `BodyTextBlockFontSize` 的消费方查不到东西。

**测点 +31**，`AstraTypographyTests` 一个新类两条 Theory 分开钉：**形状**（每个样式的 setter 名单逐字等于上游那份，
多一条少一条都红）与**到达**（挂载后读回 `FontSize`/`FontWeight`/`TextTrimming`/`TextWrapping`）。
牙口是四处临时改坏验的：`Base` 字重拍平 → 7 条到达红（靠继承拿粗体的样式一起红，正好证明继承链在跑）、
`Caption` 12→13 → 1 条到达红、给 `Caption` 塞回 `Foreground` → 1 条形状红 + 墨迹事实红、给 `Base` 塞
`FontFamily` → 1 条形状红；全部改回后与原文件字节相同。

- **构建**：`0 警告 / 0 错误`。**行为**：新类单跑 **31/31**（含上面那轮改坏的RED清单）。
- **视觉**：**本行零像素主张**——#50 已量死文本字形在任何捕获通路都不落墨，所以这批全部是依赖属性层的主张，
  没有一条断言说"它长得好看了"；Gallery 那 211 处变色只能靠目视，未被任何闸口覆盖。
- **硬件输入**：不动。
- **改了一条治理规则**：`Catalog.json` 的 `audited` 定义原本硬写"至少一条像素断言"。这条在文本行上是
  **不可满足**的——#50 已经量死文本字形在任何捕获通路都不落墨，规则写下时那件事还没测。定义改成"像素断言，
  或在像素断言**可证明到不了**（而非没做）时，给出证明它的读数（这里就是 #50）＋活元素上的依赖属性到达读数"。
  改规则与受益行同批提交，理由摆在这里：这是一条会永久禁止一个已被测明无法达成的状态的规则，不是为过关而放宽门槛。
  `ported` 的定义不动，宇宙计数从 `47 audited / 1 ported` 变成 **48 audited / 0 ported**。
- **清单**：`keys.md` 1303 → **1307**（`Typography.jalxaml` 段 7 → 11 rows）。目录里 `TextBlock` 行
  `ported` → `audited`，gap 换成三条真账（七个数值行、字体回退未量、像素侧受 #50）+ 一条明说隐式 `TextBlock`
  行是我们自己的。`adaptation/01` 那句"`x:Double` 进不了字典"补了指向 `adaptation/13` 的读数出处。
  逐键对账表另成一份 `audits/textblock-typography.md`——第一版只写了 `adaptation/13`，被目录闸
  `Every_parity_claim_cites_evidence_that_exists` 弄红（`audited` 行必须引一份 `docs/astra/audits/` 下的上游
  审计 + 一份测试）。这条结构要求是对的，不是门槛太严：能力账（宿主能不能表达）与逐键账（上游那一行进没进来）
  不是一回事，第一版把两件事塞进了一个文件，红这一枪该挨。
- **不声称**：① 不声称这十个样式与 WinUI 在屏幕上逐像素一致（无字形墨）；② 不声称 `LineStackingStrategy`
  那一行有到达证据（宿主默认与上游同值，形状断言是它能承载的全部）；③ 隐式 `TextBlock` 行仍钉主墨并全局
  `Wrap`，这两条是我们替上游多做的，改它需要一批能看见文本的仪器；④ 七个上游数值键名未兑现，不拿
  "字面量值相同"当兑现。

### 排印转录批的串行闸口读数（2026-09-22）

`tools/Test-AstraGates.ps1`（`spike/GalleryRender/gate-typography2.log`，包装器自记 **`GATE-EXIT=0`**）：
build `0 警告 / 0 错误` → 整套 **1534/1534**（0 失败 0 跳过、7 m 18 s）→ 页闸 `PASS 13 pages x 2 variants,
0 offender(s)` → 三档 `checked=True`（Light/Dark 各 83 源色 101 刷，HC 101 映射 + 3 条上游键因调色板无对应而按住）
→ `keys.md is current: 1307 canonical lines.` → `All Astra gates passed.`。

**测点 1503 → 1534 = +31**，全部来自新类，一条老测点都没动——这一批改的是排印层，而排印层的键没有任何产品样式消费。

**第一跑在 11:50 停在 test 段**（`gate-typography.log`，1 条红：`Every_parity_claim_cites_evidence_that_exists`），
原因就是上面"清单"那条讲的证据文件归错目录；补 `audits/textblock-typography.md` 后重跑才是这一份读数。
中途还有一次假红值得记：只重建测试工程时
`The_gallery_project_carries_the_catalog_it_reads` 会红，因为它把 `Catalog.json` 与
`samples/FluentJalium.Gallery/bin/<cfg>/**` 里那份**运行期真读的**副本逐字节比对——改了目录就要连 Gallery 一起建。
这条与 #47/#35 那族无关，是本地局部重建的形状，闸口内部一律全量建所以不会撞上。

页闸这一跑的不稳定读数照实记：`status` 页两档 `stable=True/False`（其余 24 条 `True/True`），
这一族归 #47/#35 的机制账，本批不追，也不因为它是老账就把"0 offender"读成"逐页像素已结清"。

**这批的真正产出不是那 31 条，而是三条以前写错或没写的边界**：① 上游那八个 setter 里两个宿主没有成员、
一个值是本运行时不认的标记（`XamlAutoFontFamily` 会静默变成一个不存在的族名）；② "数值令牌进不来"要改成
"markup 进不来、代码进得来"，这决定了以后想兑现那七个键名该往哪走；③ `audited` 图例里那句"至少一条像素断言"
在文本行上是不可满足的，改定义这件事与受益行同批提交并摆出理由，不留成一条永远不会有人达标的死规则。

## #12 A2 别名层第六轮：`TextDisabled → TextFillColorDisabledBrush` 发行——它翻掉五条"禁用墨不归我们"的事实（2026-09-22）

任务 #65 卡在同一个问题上：这一层每个名字发行之前都要答"谁在按名查"，而 `TextDisabled` 的读者一直没量到。
前四轮的量法是"装探针→看像素/看属性"，对这个名字失效，因为**它的读数与不发行时的读数同色**：
`#FFAEAEB2`（Light）既是框架那个名字的投影，也可能是 `ThemeColors` 直读的结果，颜色本身分不开这两条路。

- **分开它们的是实例，不是颜色。** 临时仪器（一次性诊断类，跑完即删）把一枚 `#FF112233` 探针刷装到
  `Application.Resources["TextDisabled"]` 上，再禁用 CheckBox / RadioButton / ListBoxItem / ComboBoxItem：
  四张生成的标签前景全部变成探针，且**就是探针那个实例**；撤掉探针回到 `#FFAEAEB2`。
  结论：框架盖章时**每次按名查**，与 `ControlBorderFocused`、`TextPrimary` 同一条路 → 别名行有读者，该发行。
- **两条空腿照实记，不当证据用。** ① `TextBox` 没有生成的 `TextBlock` 可读，它的前景是框架写在**控件自己**
  身上的本地值（同一条名，同一个结果：`Assert.NotSame` 从此读 `Assert.Same`）；② 禁用的
  `DataGridColumnHeader` 在发行前就已经画我们的 `#5C000000`，这一路不产生"读者"证据。
- **发行**：`ThemeResources/FrameworkRetints.jalxaml` 第六段，`keys.md` 1307 → **1308**（7 段 → 8 段别名行）。
- **翻账清单**（十条断言换向 + 三处事实改名 + 八段文档改判）：
  `AstraForegroundRoutingTests` 四条 disabled 事实（名也从"the framework owns the disabled one"改成
  "comes from our token"）、`AstraTextInputTests` 的禁用 TextBox、`AstraComboBoxTests` 的禁用控件与
  选中-禁用条目两处、`AstraDataGridTests` 的禁用宿主、`AstraAutoSuggestBoxTests` 的禁用盒（这条是**全量闸口
  才抓到的**：我先跑的过滤集没含这个类，事实名 `A_disabled_box_keeps_the_frameworks_own_disabled_text_colour`
  已整体改写）、`AstraFrameworkNameResolutionTests` 把 `TextDisabled` 从"未发行的活投影"里删掉并补两条
  两档跟随主题的行腿。文档侧在 `adaptation/00`、`audits/{combobox,listbox,listview,checkbox-radiobutton,
  textbox-passwordbox,autosuggestbox}.md` 各挂一条带日期的改判注，历史读数保留。
- **优先级这件事没有翻。** 框架仍然在禁用时写本地值，本地值仍然压过我们所有格子；翻掉的是"那个值是谁的"，
  不是"谁赢"。所以 `AstraDataGridTests` 里"宿主禁用不落子元素格子"那条 `NotSame` 照旧红不了也照旧留着，
  `AstraComboBoxTests` 里"占位符禁用行没有杠杆"的结论也照旧——只是这根杠杆的两端现在同值，看不见差别。
- **新增一条判据**：别名行的到达证明必须是**实例同一性**（`Assert.Same(我们的 token, 框架写下的那个值)`），
  颜色相等不足以区分"按名查"与"同名巧合"。上一轮 `TextPrimary` 的"证据等级比上一行低"就低在这里，
  这一轮把 `Assert.Same` 做成了四条事实的主断言。
- **四类证据**：构建——见下节闸口读数；行为——挂载控件的 DP 读回（含实例同一性）；视觉——**零条**，
  一枚禁用 `MenuFlyoutItem` 的裁片从头到尾是页面色（#50 字形墨不打印），所以这一级的证据只到"值到达"；
  硬件输入——无。
- **不声称**：① 不声称任何禁用墨像素；② `ListViewItem` 那一路本批**未复量**，其审计里的 `#FFAEAEB2`
  读数原样保留（不据邻居证据改口）；③ 不声称 `AutoCompleteBox` 建议项底色因此可主题化（那是框架本地渐变，
  与本轮无关）；④ 不声称上游那档 Color 数值被逐位复刻——我们转发的是自己调色板里那支 WinUI 字面值。

### TextDisabled 发行批的串行闸口读数（2026-09-22）

`tools/Test-AstraGates.ps1`（`spike/GalleryRender/gate-textdisabled2.log`，包装器自记 **`GATE-EXIT=0`**）：
build `0 警告 / 0 错误` → 整套 **1535/1535**（0 失败 0 跳过、7 m 32 s）→ 页闸 `PASS 13 pages x 2 variants,
0 offender(s)` → 三档 `checked=True` → `keys.md is current: 1308 canonical lines.` → `All Astra gates passed.`。
测点总数与上一批同为 1535：这一批只换向断言、不新增测点（四条 Same 换向 + 两档主题腿补进既有 Theory）。

**第一跑停在 test 段，1 条红**（`gate-textdisabled.log`，1535 中 1 红）：
`AstraAutoSuggestBoxTests.A_disabled_box_keeps_the_frameworks_own_disabled_text_colour`。
我先跑的过滤集（ForegroundRouting / TextInput / ComboBox / DataGrid / FrameworkNameResolution）已经把十条红全部
抓到并钉好，漏了这一类——**这一条正好把"过滤集等于爆炸半径"这个想法证伪了**：改名换向这类账要由全量闸口收尾，
不能由我按名字猜出来的子集收尾。补钉后才是上面这一份读数。

**`AstraTextInkTests` 那族的不稳定照实记**：整套第一跑红过 `A_buttons_text_owes_it_no_pixels(Dark)`，
单独重跑时同一批腿又换成 `A_buttons_text_owes_it_no_pixels(Light)` 红、菜单腿全绿，且红消息是
"控件自己的表面没到页面上"——而消息里打印的直方图明明白白有 8296 个表面像素。同一仪器在不同跑序里指向不同腿，
这是 #47/#35 那族的形状（顺序相关），不是本批别名行的后果：本批两跑里该类其余腿都绿。
不据此声称已归因，机制账仍挂在 #47/#35。
