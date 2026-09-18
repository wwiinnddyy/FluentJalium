# 高对比度：逐键映射而不是键名猜测

Astra 在 Jalium 上没有高对比度驱动可用（`ThemeVariant` 只有 `{Dark, Light}`，
`ThemeMode` 只有 `{None, Light, Dark, System}`，见 `00-jalium-theme-capabilities.md` S0-a 与
"高对比度" 一节）。所以高对比度必须是我们自己的一层重映射。这一层在阶段 1 之前是
**按键名猜语义**：`key.Contains("Disabled")`、`key.StartsWith("Accent")`、
`key.Contains("Text") || key.Contains("Stroke")`……这种启发式会静默地把错的系统色涂到
看起来像对的键名上，而且无法和上游对齐。现在换成上游的逐键表。

## 上游证据

`../microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`，
`controls/dev/CommonStyles/Common_themeresources_any.xaml` 的 `HighContrast` 分支：

- 184 个子元素：83 个 `Color` + 101 个 `SolidColorBrush`，0 个 `GradientBrush`；
- 101 个笔刷的目标只有 9 种取值，全部直接引用 Win32 系统色，没有一层中转：

| 目标 | 数量 |
|---|---|
| `SystemColorWindowColor` | 33 |
| `SystemColorWindowTextColor` | 27 |
| `SystemColorButtonFaceColor` | 21 |
| `SystemColorButtonTextColor` | 9 |
| `SystemColorGrayTextColor` | 5 |
| `SystemColorHighlightColor` | 1 |
| `SystemColorHighlightTextColor` | 1 |
| `SystemColorHotlightColor` | 1 |
| `Transparent` | 3 |

该分支里的 `Color` 条目全是 `#FF0000` 一类的占位值（Win32 高对比由系统着色，
XAML 里的常量不起作用），所以只转录笔刷，不转录 `Color`。

## 产出物

`tools/Sync-AstraPalette.ps1` 每次同步调色板时一并生成
`src/FluentJalium/ThemeResources/HighContrast.map`（内嵌名与之一致），
一行一个笔刷键：

```
AccentFillColorDefaultBrush=SystemColorWindowColor
TextFillColorPrimaryBrush=SystemColorWindowTextColor
SubtleFillColorTransparentBrush=Transparent
```

生成器有两条硬规则，缺任何一条直接失败而不是退回猜测：

1. **调色板里每个笔刷键都必须有上游决策**。上游少键即报错；
2. 上游值只接受 `{ThemeResource SystemColor*}` 或 `Transparent`，其它写法抛
   `Unmapped upstream high-contrast value`。

`Sync-AstraPalette.ps1 -Check` 逐字节比对，漂移即失败（`tools/Test-AstraGates.ps1` 已含此闸）。

## 运行时

`FluentThemeManager.RefreshPalette()` 的顺序固定：复制 Light/Dark → 水合 `SystemColor*` 键 →
高对比时套映射表 → 否则套强调色 → 最后套 `OverrideBrush`。表里的名字通过唯一的
`SystemColor(name)` 解析到 `Jalium.UI.Media.SystemColors`：

| 映射名 | Win32 索引 | Jalium |
|---|---|---|
| `SystemColorWindowColor` | COLOR_WINDOW | `SystemColors.WindowColor` |
| `SystemColorWindowTextColor` | COLOR_WINDOWTEXT | `SystemColors.WindowTextColor` |
| `SystemColorButtonFaceColor` | COLOR_BTNFACE | `SystemColors.ControlColor` |
| `SystemColorButtonTextColor` | COLOR_BTNTEXT | `SystemColors.ControlTextColor` |
| `SystemColorGrayTextColor` | COLOR_GRAYTEXT | `SystemColors.GrayTextColor` |
| `SystemColorHighlightColor` | COLOR_HIGHLIGHT | `SystemColors.HighlightColor` |
| `SystemColorHighlightTextColor` | COLOR_HIGHLIGHTTEXT | `SystemColors.HighlightTextColor` |
| `SystemColorHotlightColor` | COLOR_HOTLIGHT | `SystemColors.HotTrackColor` |
| `Transparent` | — | `Colors.Transparent` |

解析在**加载时**跑一遍（`ReadHighContrastMap` 对每行调用 `SystemColor`），
因此一个不可映射的名字会在启动时抛出，而不是等用户切到高对比度才炸。

## 与上游的三处偏差

1. **曾经有三个我们自加的键，现在只剩一个**。`SliderThumbStrokeBrush` 已经在
   `audits/slider.md` 这一批里整条删除：上游给滑块圆环描边起的名字是 `SliderThumbBorderBrush`，
   我们把它原样收进 `ThemeResources/Slider.jalxaml` 做别名，调色板层就不再需要为这个位置造一个
   没有出处的键，它的高对比行（`SystemColorWindowTextColor`）也跟着消失——那一行本来就是判断，
   不是上游逐键值。`ToolbarSurfaceBrush` 更早被删：自造且无消费点，它当初的理由
   （"外壳没有可主题化表面"）被 `audits/window-shell.md` 更正——外壳有 8 个真名钩子，
   那 8 个走别名层指向已有调色板键，因此不需要映射表行。
   剩下的一个是 `AcrylicInAppFillColorDefaultBrush`，它曾经也在这一类（叫
   `FlyoutPresenterBackgroundBrush`，一个上游没有的名字），`audits/flyout-presenter.md` 把它换成了
   上游真名，并且它的高对比值 `SystemColorWindowColor` 现在**有上游出处**：
   `Materials/Acrylic/AcrylicBrush_themeresources.xaml` 的 `HighContrast` 分支就是这么声明它的，
   只是那张表不在生成器读的 `Common_themeresources` 里。
   删除的记账方式：这两处都不在映射表里留"已删"行，而是由生成器
   （`tools/Sync-AstraPalette.ps1` 的两段注释）说明为什么不再需要——-Check 模式下
   调色板与映射表都必须与生成器逐字节一致，留下幽灵键会立刻被漂移闸口抓住。
2. **三个上游键未采纳**：`AccentControlElevationBorderBrush`、`CircleElevationBorderBrush`、
   `ControlElevationBorderBrush` 在 `HighContrast` 分支存在而 `Light` 分支不存在，
   我们的调色板也还没有这些键，因此不进映射表。将来补这些键时，生成器会要求同时补高对比
   决策（规则 1）。
3. **逐控件的高对比视觉状态没有移植**。WinUI 每个控件的 `*_themeresources.xaml` 里还有
   `HighContrast` 专属的 `SolidBackgroundFillColor*` 覆盖与 `*_BorderBrush` 提粗；
   本表只覆盖令牌层。这是 host substitution，**不得对外声称高对比 parity**。

## 一处对上游读数的更正

先前有人（另一会话的测试草稿）按 WinUI 文档假设高对比下强调填充应等于
`SystemColorHighlightColor`。上游实际是：

```xml
<SolidColorBrush x:Key="AccentFillColorDefaultBrush" Color="{ThemeResource SystemColorWindowColor}" />
<SolidColorBrush x:Key="AccentTextFillColorPrimaryBrush" Color="{ThemeResource SystemColorWindowTextColor}" />
```

即高对比下**强调填充退到窗体底色**，强调靠边框与文字表达。
`AstraThemeRuntimeTests.High_contrast_maps_semantic_roles_to_system_colors` 按上游断言。
