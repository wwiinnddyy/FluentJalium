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

**顺序按"底座先于叶子"**，因为框架没有默认主题、未样式化的控件会露出框架外观：

1. **底座**：`ScrollViewer`、`ScrollBar`、`Thumb`、`Popup`、`ItemsControl`、`ContentPresenter`、
   窗口外壳。`02` 已证明 `ScrollBar` 认 DP，这一批是可做的。
2. **Button 族**（纵向样板，锁流程）：Default/Accent/Subtle/Compound/Link/Repeat/Toggle +
   `SplitButton`；`DropDownButton` 无原生类型 → 自有类型开端。
   原计划起手要修的"模板根 Border 不吃本地 `Background`"**已被 `06` 证伪**：本地值经
   `{TemplateBinding Background}` 完整落到像素，Button 的哨兵断言也已进了 `AstraPixelTests`。
   Button 批的活因此回到"逐键对齐上游 + 状态映射 + Gallery 页"，不含颜色通路修复。
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

- **E1 目录来自反射 + curated JSON**（沿用已定决策）：反射枚举**被我们赋了隐式样式的 `TargetType`**，
  与目录求差集 → 差集非空即失败。这就是"未样式化控件"的检测点。
- **E2 页结构对齐 WinUI Gallery**：页头 + 示例卡 + 可运行源码 + 上游 parity 状态 + 该页用到的令牌清单。
- **E3 三个系统级页面**（对应 A/C/B 三条工作流，而不是控件页）：
  **Tokens** 全色板网格 + 缺失键诊断；**Materials** 层/高程/背衬对照（开/关同屏）；
  **Motion** 时长节拍与 `ContentTransition` 对照。这三页是"系统"层是否自洽的直接证据。
- **E4 证据钩子**：每页可被测试渲染到像素（复用 B4），使"它真的画出来了"成为断言而非目视；
  启动参数支持隔离 profile 与关窗，遵守 AGENTS.md。

## 阶段与提交边界

| 阶段 | 内容 | 出口 |
|---|---|---|
| 1 | **B1 ✅ + A1 ✅（改混合模型）+ A6 ✅（反查部分）+ A2 部分 + B4 基座**（门面收敛、键消费点反查、高对比逐键映射、像素断言基座） | 已到：`dotnet test` 12/12、门面内无 `VisualTreeHelper`/`InvalidateVisual`、Light↔Dark 笔刷实例保持并有断言。仍欠：A2 别名转录、`resources/keys.md` |
| 2 | **像素归因 ✅（`06`）+ Button 纵向样板**（走完 9 步流水线，锁死后续样板） | 已到：判据可信、`AstraPixelTests` 7/7 全绿 0 skip、隐式样式与令牌到像素有断言。仍欠：Button 审计文档、状态映射表、Gallery 页与逐键对齐 |
| 3 | **A3 + C2 🬡**（强调色三态、材质参数摸底） | 强调色改动能被像素断言（A4 已撤回，不再是出口）；C1 映射表可执行 |
| 4 | **D1 底座批 + E1/E3 Gallery 骨架与三个系统页** | 目录差集为空；三个系统页有证据 |
| 5+ | D2…D8 按批推进；C1/C3/C4 材质随批落地 | 每批全 9 步 + 全闸口 |
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
- 不声称硬件触摸笔与混合 DPI 已经过真机验证。
