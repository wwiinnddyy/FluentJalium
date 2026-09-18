# 审计：FlyoutPresenter / 弹层表面（Popup 外壳）

- 上游：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
  - `controls/dev/CommonStyles/FlyoutPresenter_themeresources.xaml`，blob `621bf7d16825ae37cdd0b0ad05b7b5a49ddcd4c4`（52 行，全文读取）
  - 对照用：`controls/dev/CommonStyles/ToolTip_themeresources.xaml`，blob `281d1efcf02c33f81b32fd04e2c691da9173a323`
  - 对照用：`controls/dev/Materials/Acrylic/AcrylicBrush_themeresources.xaml`（`AcrylicInAppFillColorDefaultBrush` 的三个分支）
- 方法参照：`ModernWpf` @ `23555a6c`（别名层照抄，不展平）
- 分级：**host substitution**。Jalium.UI 26.10.9 里**没有 `FlyoutPresenter` 类型**，
  `Popup` 只是窗口化的内容宿主，没有样式面。因此这一项交付的是上游的**别名层**，
  不是它的 `DefaultFlyoutPresenterStyle`。

## 上游键清单与处置

`FlyoutPresenter_themeresources.xaml` 只有 4 个公开键（其余是 `DefaultFlyoutPresenterStyle`
与其 `BasedOn` 的隐式样式）：

| 上游键 | 上游值（Default / Light） | 上游值（HighContrast） | 我们的处置 |
|---|---|---|---|
| `FlyoutPresenterBackground` | `StaticResource` → `AcrylicInAppFillColorDefaultBrush` | → `SystemColorWindowColorBrush` | 逐字转录，别名指向同名调色板笔刷 |
| `FlyoutBorderThemeBrush` | `StaticResource` → `SurfaceStrokeColorFlyoutBrush` | → `SystemColorWindowTextColorBrush` | 逐字转录 |
| `FlyoutBorderThemeThickness` | `Thickness` 1 | `Thickness` 2 | 转录为 1；**高对比的 2 未落地**（见 Gaps） |
| `FlyoutContentPadding` | `Thickness` 16,15,16,17（分支外，全模式同值） | 同 | **不声明**：没有消费点，声明即死键 |

样式里另外消费的 `FlyoutThemeMinWidth/MaxWidth/MinHeight/MaxHeight` 与
`OverlayCornerRadius` 不属于这个文件；`OverlayCornerRadius` 已在 `Metrics.jalxaml`，
弹层外壳直接用它。四个 `FlyoutTheme*` 尺寸键同样因无 `FlyoutPresenter` 类型而**不声明**。

## 别名为什么能跟着主题走

调色板里的每个笔刷都是**同一批对象被就地改色**（`RefreshPalette` 保身份），
所以 `<StaticResource x:Key="A" ResourceKey="B"/>` 在解析期绑定的正是那个对象：
`FlyoutPresenterBackground` 与 `AcrylicInAppFillColorDefaultBrush`
**`Assert.Same` 成立**，翻主题与高对比都透过这同一个笔刷到达消费者。
这条断言在 `AstraThemeRuntimeTests.Upstream_flyout_aliases_resolve_to_the_palette_brush_instance_and_follow_the_theme`。

反过来说：如果在 `Styles/*.jalxaml` 里写
`<SolidColorBrush x:Key="FlyoutPresenterBackground" Color="{ThemeResource …}">`，
它会在解析期物化出自己的实例，从此不跟随翻色——所以别名层放在
`ThemeResources/FlyoutPresenter.jalxaml`，与调色板同一加载序列里。

## `AcrylicInAppFillColorDefaultBrush`：从自加键改成上游真名

生成器原先有一个**上游不存在**的键 `FlyoutPresenterBackgroundBrush`
（值 `#F9F9F9` / `#2C2C2C`）。上游真正的链条是
`FlyoutPresenterBackground` → `AcrylicInAppFillColorDefaultBrush`，
而后者在 `AcrylicBrush_themeresources.xaml` 里是 `AcrylicBrush`（Jalium 无此类型，生成器读不到）。
改名后：

- 键名与别名链**全部是上游原样**，公开键面上少一个自造名；
- 实底值仍是上游自己写的 `FallbackColor`：Light `#F9F9F9`、Dark `#2C2C2C`，逐字符照抄；
- 高对比值不再是我们猜的：上游 `HighContrast` 分支就把这个键声明为
  `<SolidColorBrush Color="{ThemeResource SystemColorWindowColor}">`。
  那张表不在生成器读取的 `Common_themeresources` 里，所以仍是一行显式适配项，但有出处了。

代价：`FlyoutPresenterBackgroundBrush` 这个公开键被删除。它是分支内自造的，
1.0 冻结前不算破坏契约；消费者只有 `Styles/Common.jalxaml` 的 ToolTip 与本次新加的弹层外壳。

## 消费点

- `Styles/Selection.jalxaml` ComboBox 的 `PART_Popup` 外壳：原先是写死
  `StrokeThickness="1"` / `RadiusX="8"` 的 `Rectangle`，现在换成消费三个上游名的 `Border`。
- `Styles/Common.jalxaml` `DefaultToolTipStyle`：边框从 `SurfaceStrokeColorDefaultBrush`
  改成上游真正指定的 `FlyoutBorderThemeBrush`（即 `SurfaceStrokeColorFlyoutBrush`），
  粗细改走 `FlyoutBorderThemeThickness`。背景仍指向弹层表面，理由写在文件注释里：
  上游 `ToolTipBackground` → `SystemControlBackgroundChromeMediumLowBrush`
  是 UWP 遗留系统刷，`microsoft-ui-xaml` @19e3bdc **不定义**它，值无从照抄。

## 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行 restore→build→test→调色板漂移，全绿；
  键闸口要求"被引用的键必须声明"，三个上游名的消费点因此有静态保证。
- **行为**：`AstraThemeRuntimeTests` 新增 1 例（别名与调色板笔刷同一实例、
  Light `#F9F9F9` → Dark `#2C2C2C`、`FlyoutBorderThemeThickness` 非空）。
  `FlyoutBorderThemeThickness` 只断言非空：这个读取器对不存在的键**静默失败**，
  而 `Thickness` 值本身在文件里是字面量，断它的值不如断它解析出来了。
- **视觉**：`AstraPixelTests` 新增 2 例，用 ToolTip（模板根就是弹层外壳，
  且能在进程内捕获）断言：覆盖 `AcrylicInAppFillColorDefaultBrush` 的哨兵色
  落到 >6000 像素；Light 读到 `#F9F9F9`、Dark 读到 `#2C2C2C`；两帧都没有品牌绿。
- **硬件输入**：**本项不适用**。弹层外壳是纯样式/资源面，没有输入路径；
  真正的开合路径归 ComboBox / MenuFlyout / ToolTip 各自那一项。

## Known Gaps（不许用相邻证据替代）

1. **高对比的 2px 边框没有落地**。我们的度量键不分模式，因此高对比下弹层边框仍是 1px。
2. **弹层材质是实底，不是 acrylic**。Jalium 没有 `AcrylicBrush` 类型，
   窗口级 `Mica`/`Acrylic` 背衬与元素级 acrylic 是两回事（见 `adaptation/03`）。
   这里用的是上游自己的 `FallbackColor`，不声称材质等价。
3. **`FlyoutContentPadding` 与四个 `FlyoutTheme*` 尺寸键未声明**，因为没有 `FlyoutPresenter`
   类型可挂；弹层的内边距目前由各控件模板自己写。等 ContentDialog / MenuFlyout 那几项时，
   要么给它们各自的键，要么在这条链上补一个自有外壳类型——不提前造键。
4. **ComboBox 弹出外壳本身没有被像素断言**。`Popup` 的内容在自己的可视根上，
   现有 harness 裁剪不到；这一项用同一条链上的 ToolTip 外壳作证，
   ComboBox 那层要到 ComboBox 自己的条目补测，不能当成已经证明。
5. **ToolTip 背景仍不是上游的那个值**，因为上游那个值不在这份上游里（见上）。
   边框与粗细已经对齐；背景要在 ToolTip 条目里定：要么给 `SystemControlBackgroundChromeMediumLowBrush`
   一个有出处的映射，要么改用 `ComboBoxDropDownBackground` 同源的实底并记录偏差。
6. 未声称上屏合成与离屏捕获逐位一致（RTB 离屏），沿用全局不声称清单。
