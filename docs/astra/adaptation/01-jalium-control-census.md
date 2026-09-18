# Jalium.UI 26.10.9 控件层普查

回答一个前置问题：**做 Fluent 控件库，脚下到底有什么。**
方法：`spike/ControlCensus`（反射 + 真实窗口 + dump 框架自带主题），
`01-census-raw-output.txt` 是原始输出。只记录测到的东西。

## 结论 1：框架**没有发布任何 Generic 主题**，163 个控件零默认样式

```
GenericThemeResourceName = Jalium.UI.Controls.Themes.Generic.jalxaml
ControlsAssembly         = Jalium.UI.Managed
Jalium.UI.Managed embedded theme-ish resources: 0 []
LoadGenericTheme()       -> null
GetGenericThemeStream()  -> null
ResolveThemeStyle        is not public
public concrete Control-derived types: 163
generic theme defines styles for 0 target types, 0 keyed styles
SUMMARY: template-driven=0 self-drawn=0 no-default-style=163
```

`ThemeManager` 的文档描述了完整的 Generic 主题管线（`LoadGenericTheme`、
`GetGenericThemeStream`、`ResolveThemeStyle`、`TryBuildFromPrebuiltRegistry`），
但 NuGet 发布集成的资源里**一个 `.jalxaml` 都没有**，所以那条管线在 26.10.9 上是空转的。

**含义（也是我之前说错的地方）**：不存在"Jalium 原生控件默认样式"这层可以去覆盖。
FluentJalium 不是在重模板，**FluentJalium 就是这套控件的主题本身**。
心智模型应当从「WinUI 样式覆盖 Jalium 样式」改成「我们提供 100% 的外观」，
这正是 ModernWpf 的 `ControlsResources.xaml` 所处的位置。

## 结论 2：应用级隐式样式确实落到原生控件上

没有 Generic 主题不等于没有隐式样式查找。实测（代码构造 `Style` 塞进
`Application.Resources[typeof(T)]`，窗口 `Show()` 后读数）：

```
Button     via Application.Resources[typeof(Button)]: bg=#FF123456 fg=#FFABCDEF   ← 生效
ProgressBar implicit:                                 fg=#FF010203                 ← 生效
Button with Style assigned directly:                  bg=#FFFEDCBA                 ← 生效
after ThemeMode.Dark, 上述隐式 Button bg 仍为 #FF123456
```

最后一行是**符合预期**而不是缺陷：代码 setter 给的是静态值，不随主题变；
要随主题变就必须在标记里写 `{ThemeResource}`。这条与
`00-jalium-theme-capabilities.md` 的 S0-a 一致，也说明
「样式里用 `{ThemeResource}`」不是风格偏好，而是主题跟随的**唯一机制**。

## 结论 3：163 个类型按绘制方式分两类

| 类别 | 数量 | 判据 | 对还原 WinUI 的影响 |
|---|---|---|---|
| 纯组合/模板绘制 | **104** | 未 override `OnRender`/`OnPaint` | 外观完全由我们的模板决定，可按 WinUI 逐像素还原 |
| 有自绘代码 | **59** | override 了 `OnRender` 或 `OnPaint` | 需逐个确认自绘读的是 DP 还是 `ThemeColors` |

**59 个自绘控件完整名单**（阶段 4 排批时必须按此标注）：

```
AppBarSeparator  AutoCompleteBox  Calendar  CalendarButton  CalendarDayButton  CalendarItem
CameraView  ChartLegend  ChartTooltip  ColorPicker  CommandBar  DataGridCell
DataGridColumnHeader  DataGridRowHeader  DatePicker  DiffViewer  DockItem  DockLayout
DockTabPanel  EditControl  HexEditor  HyperlinkButton  InfoBar  ItemsControl  Label  MapView
Menu  MenuBar  MenuBarItem  MenuFlyoutItem  MenuFlyoutSeparator  MenuFlyoutSubItem  MenuItem
NavigationViewItemHeader  NavigationViewItemSeparator  NumberBox  PasswordBox  ProgressBar
QRCode  RangeSlider  ResizeGrip  RichTextBox  ScrollBar  ScrollViewer  Separator  Slider
Sparkline  Split  StatusBar  StatusBarItem  SwipeControl  TabControl  TabItem  TextBox
Thumb  TimePicker  ToastNotificationItem  ToggleMenuFlyoutItem  Window
```

⚠️ 两个必要的限定，别把这张表读成判决书：

1. **override `OnRender` ≠ 改不动**。很多控件自绘时读的是 `Background`/`BorderBrush` 这类
   DP，而 DP 正是我们的样式能写的。真正的天花板只有**直接读 `ThemeColors` 静态表**的那部分。
   这张表是**待查清单**，不是已判死刑清单。
2. `parts=0` 是普遍的（连 `Button` 都没有 `PART_` 常量字段），所以**不能**用"有没有
   `PART_` 字段"判断可模板性；判据只能是自绘与否 + 实测。

但这份名单本身已经足够重要，因为它包含了 Fluent 最核心的表面：
`ScrollBar`、`TextBox`、`PasswordBox`、`Slider`、`ProgressBar`、`TabControl/TabItem`、
`Menu/MenuFlyout` 全家族、`CommandBar`、`NumberBox`、`DatePicker/TimePicker`、`InfoBar`、
`ItemsControl`、`HyperlinkButton`、`ScrollViewer`。

## 结论 4：天花板的来源是 `ThemeColors` 这张静态表

`Jalium.UI.Controls.Themes.ThemeColors` = **71 个 public 静态 `Color`，零 setter**，
且类型文档自陈 "for the **dark** theme"。实测四种公开写入口**全部无效**，读数一字不变：

```
baseline / after ApplyAccent(magenta) / after ApplyBrandTheme(options) /
after ThemeMode.Light / after ThemeMode.Dark  →  五组读数完全相同
WindowBackground=#1E1E1E  ControlBackground=#373737  Accent=#207245  SliderThumb=#207245
ToggleCheckedBackground=#207245  TabItemIndicator=#207245  ProgressBarFill=#207245
TextBoxBackground=#2D2D2D  TitleBarBackground=#202020  ScrollBarThumb=#505050  CheckMark=#FFFFFF
```

`#207245` 是 Jalium 自己的品牌森林绿，`#1E1E1E` 系是深色。
所以自绘代码里凡读这张表的，**在 Light 主题下会给出深色常量**——
这不是"不够 Fluent"，是显色错误。inkcanvas 的源码审计已在 `Jalium.Slider.cs` 里
抓到过这个模式（`static SolidColorBrush` + `ThemeColors.SliderThumb`）。

注意它与「框架 DP 默认值」不是一回事：**DP 默认值是可主题化的**，实测未加样式的原生控件：

```
native Button   Light bg=#FFFFFF fg=#1D1D1F  ->  Dark bg=#2C2C2E fg=#F5F5F7   changed=True
native CheckBox 同上                                                              changed=True
native Slider   Light bg=#37000000 -> Dark bg=#28FFFFFF                          changed=True
```

`#1D1D1F`/`#F5F5F7`/半透明叠加层是 Win11 味道，说明框架作者另有一套主题化默认值来源；
`ThemeColors` 是其中**没被接进去**的那部分。这条差异只能逐控件验，不能靠推断。

## 结论 5：Fluent Design 其他支柱在 Jalium 上对应什么

| Fluent 支柱 | Jalium 26.10.9 实测 | 判定 |
|---|---|---|
| 材质 Material | `WindowBackdropType = {None, Auto, Mica, Acrylic, MicaAlt}`，窗口级公开可用 | 窗口背衬**有** |
| 元素级 Acrylic/Reveal | `Jalium.UI.Media` 里 `AcrylicBrush`/`MicaBrush`/`RevealBrush` **全部不存在**；只有 Solid/Linear/Radial | 元素级亚克力与 Reveal **做不到**，浮动层只能用不透明实色（与 inkcanvas 的选择一致） |
| 图标 Icons | `SymbolIcon` 文档自陈"mirrors WinUI's SymbolIcon"，取 Segoe 图标字体；`Symbol` 枚举 **764** 个成员 | 原生图标路**可用**，且不依赖出问题的 FluentSystemIcons；仍需逐码点验 cmap |
| 排版 Typography | `ThemeManager.ApplyTypography(display, body, mono[, size])` 公开；当前实测落在 `Microsoft YaHei UI` / `Cascadia Code` / bodySize 12 | 公开可设为 Segoe UI Variable；**但 `x:Double` 进不了字典**，`Type*` 样式的字号只能内联字面量 |
| 动效 Motion | 无公开系统减弱动画 API（只有 `LinuxDesktopPortal.TryReadReducedMotion` 被文档提到）；`TransitionDuration` 可主题化（实测 0.167↔0.333） | 令牌级动效**能**做；跟随系统开关**不能**声称支持 |
| 无障碍接触 Exposure | 无高对比公开入口（见 00 文档） | **做不到**，需显式刷子重映射 |

## 对已批准计划的修正

1. **B.5「不覆盖 GenericThemeResourceName，让 Jalium Generic 做地板」这条作废**——
   根本没有地板。改为：FluentJalium 提供完整隐式样式，未样式化的原生控件退回框架 DP 默认值。
   风险随之反转：**任何我们没写样式的控件都会显示框架外观而非 Fluent 外观**，
   所以 Gallery 必须遍历到每一个我们声称支持的控件，缺样式要能被发现。
2. E 节 triage 里「对着原生 `NavigationView` 重写」仍成立（隐式样式有效），
   但工作量估计要上调：那是**从零提供完整模板**，不是覆盖一层。
3. 阶段 4 排批给每个控件加一个前置动作：**跑一次自绘探针**
   （是否在名单里 + 自绘是否读 `ThemeColors`），据此决定纯模板还是自有类型。
   名单里的 `ItemsControl`/`ScrollViewer`/`ScrollBar`/`Window` 属于基座，
   优先级高于任何单个交互控件。
4. 新增一条闸口需求：因为「消费不存在的键」和「样式没落地」都是**静默**的，
   键清单闸口要同时反查消费点，并且要有"该控件是否真的用上了我们的样式"的断言。

## 还没测

- 59 个自绘控件里，具体哪几个真正读 `ThemeColors`（需要 IL 级或逐控件视觉验证）。
- 只有 Button/CheckBox/Slider 三个控件验证过「DP 默认值随主题」，其余 160 个未抽样。
- `Symbol` 764 成员的实际字形是否为 Fluent 风格、码点在已装字体 cmap 中的命中率。
- 隐式样式在 `DataTemplate` 内、`Popup` 内、以及未显示窗口里的查找行为。
- 主题翻转时刷子实例标识是否保持（00 文档留下的同一处空白）。
