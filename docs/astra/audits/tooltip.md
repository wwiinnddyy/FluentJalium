# 审计：ToolTip

- 上游：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
  - `controls/dev/CommonStyles/ToolTip_themeresources.xaml`，blob `281d1efcf02c33f81b32fd04e2c691da9173a323`，
    **全文 77 行**，键与样式都在同一个文件里（上游没有单独的 `ToolTip.xaml`）
- 方法参照：`ModernWpf` @ `23555a6c` 的 `ModernWpf/Styles/ToolTip.xaml`
  与 `docs/tooltip-wpf-fluent-source-audit.md`（它把 ToolTip 当"库存 WPF 控件"处理，
  权威源是官方 WPF Fluent 而不是 WinUI 3；我们这里 Jalium 的 ToolTip 是原生控件，
  但视觉权威仍按本分支的既定目标——WinUI 3 一比一——取上游那份）
- 分级：**template-only**。Jalium 有原生 `ToolTip`（`ContentControl` 族），
  有 `Template`，且 26.10.9 的隐式应用样式对它生效（`AstraPixelTests` 断到了它的模板像素）。

## 上游键清单与处置

`ToolTip_themeresources.xaml` 一共 10 个公开键。全部逐个处置，不批量转录：

| 上游键 | 上游值（Default=Light） | 上游值（HighContrast） | 处置 |
|---|---|---|---|
| `ToolTipForeground` | `TextFillColorPrimaryBrush` | `SystemControlForegroundBaseHighBrush` | **不声明**：样式消费的是 `ToolTipForegroundBrush`，两个名字指向同一个刷；声明两个只多一个死键 |
| `ToolTipBackground` | `SystemControlBackgroundChromeMediumLowBrush` | 同一个键 | **不声明**：见"未解的一半" |
| `ToolTipBorderBrush` | `SurfaceStrokeColorFlyoutBrush` | `SystemColorWindowTextColorBrush` | 转录（`ThemeResources/ToolTip.jalxaml`） |
| `ToolTipForegroundBrush` | `TextFillColorPrimaryBrush` | `SystemColorWindowTextColorBrush` | 转录 |
| `ToolTipBackgroundBrush` | `AcrylicInAppFillColorDefaultBrush` | `SystemColorWindowColorBrush` | 转录 |
| `ToolTipBorderThemeThickness` | `Thickness` 1 | `Thickness` 1 | 转录；**这一项没有高对比缺口**（三分支同值） |
| `ToolTipContentThemeFontSize` | `x:Double` 12 | 12 | **不能转录**：运行时读取器解析不了 `x:Double`，样式里写字面量 12 |
| `ToolTipBorderPadding` | `Thickness` 9,6,9,8 | 同 | 转录（我们原先写的是 `8,6`，与上游不符，已改） |
| `ToolTipMaxWidth` | `x:Double` 320 | 同 | 不能作资源键，改为样式里的 `MaxWidth="320"` 字面量，并用断言钉住 |
| `ToolTipBackgroundThemeBrush` / `ToolTipBorderThemeBrush` / `ToolTipForegroundThemeBrush` | 遗留 phone 时代实底刷（`#FFFFFFFF`/`#FF808080`/`#FF666666`） | 走系统色 | **不声明**：上游样式自己也不消费它们，属于历史遗留面 |

`DefaultToolTipStyle` 另消费的 `ContentControlThemeFontFamily` 我们没有这个键，
`ControlCornerRadius`（=4）有，已改为消费资源而不是写字面量 4。

## 未解的一半：`ToolTipBackground`

上游把 `ToolTipBackground` 指到 `SystemControlBackgroundChromeMediumLowBrush`，
而这个键**在 `microsoft-ui-xaml` @19e3bdc 里根本没有定义**（`git grep` 只找到消费点，
找不到声明），它是 UWP `generic.xaml` 的遗留系统刷。ModernWpf 给了 WPF Fluent 的值
（`SystemChromeMediumLowColor`，Dark `#FF2B2B2B`），那是另一套权威的数，不是 WinUI 3 的。
所以这里不猜：样式消费同文件里**有定义**的 `ToolTipBackgroundBrush`（→ 弹层共用表面），
把 `ToolTipBackground` 这个名字留着不声明。声明一个我们只能靠猜来填值的键，
比留一个缺口更糟。

## 模板与状态映射

上游模板根是 `ContentPresenter`（自带 Background/BorderBrush/BorderThickness/CornerRadius/
MaxWidth/TextWrapping/Padding）。Jalium 的 `ContentPresenter` **没有 `TextWrapping` 属性**
（编译期确认，见 Gaps 2），换行只能落在它生成的文本元素上。因此我们的模板是
`Border > ContentPresenter`，并在 `ContentPresenter.Resources` 里放一条
`Style TargetType="TextBlock"` 把 `TextWrapping` 设为 `Wrap`——这正是 ModernWpf 的做法。

| WinUI `VisualState` | 我们的映射 |
|---|---|
| `OpenStates/Closed` → `FadeOutThemeAnimation`（TargetName=LayoutRoot） | **未映射**：`VisualStateManager` 写在标记里会抛（`adaptation/00`），而 `ToolTip` 的开关由框架内部驱动，模板触发器没有可挂的属性 |
| `OpenStates/Opened` → `FadeInThemeAnimation` | 同上 |

即：ToolTop 的淡入淡出**没有移植**，出现/消失是硬切。

## 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行 restore→build→test→调色板漂移全绿；
  键闸口要求"被引用的键必须声明"，`ToolTip.jalxaml` 必须进 `Themes/Manifest.txt`
  否则清单双向校验会抛，所以五个键的加载有静态保证。
- **行为**：`AstraThemeRuntimeTests.Upstream_tooltip_aliases_resolve_to_the_same_palette_instances`
  ——三个刷别名与调色板对象 `Assert.Same`，两个 `Thickness` 键非空
  （这个读取器对缺失键静默失败，所以"解析出来了"本身就是断言对象）。
- **视觉**：`AstraPixelTests`
  1. `The_flyout_surface_token_paints_the_shared_popup_chrome`：覆盖
     `AcrylicInAppFillColorDefaultBrush` 的哨兵色在 ToolTip 模板上落到 >6000 px；
  2. `The_flyout_surface_follows_the_theme_and_shows_no_brand_emerald`：
     Light `#F9F9F9` / Dark `#2C2C2C` 各自 >6000 px，两帧都没有品牌绿；
  3. `The_tooltip_respects_the_upstream_maximum_width`：强设 600 宽 + 400 字内容，
     实际宽度被夹到 **320**，并从构建出来的部件树上读回 `TextBlock.TextWrapping == Wrap`。
- **硬件输入**：**未做**。真实悬停/键盘聚焦弹出 ToolTip 的路径要等宿主控件（Button 族）
  的输入证据一起补；这里只证明了样式化的 ToolTip 能画出来，没证明它会被弹出来。

## 顺带证明的一条框架能力

模板部件自己的 `Resources` 里放隐式样式**是生效的**
（`TextBlock` 的 `TextWrapping` 从部件树上读回来是 `Wrap`）。
上游和 ModernWpf 都用这一招，之前我们没有验证过它在 26.10.9 上成立。

## Known Gaps（不许用相邻证据替代）

1. **淡入淡出没有移植**，见上面状态映射表；出现/消失是硬切。
2. **`ContentPresenter.TextWrapping` 不存在**，换行靠部件内隐式样式落到 `TextBlock` 上。
   附带发现：标记里给部件写一个它没有的属性，**读取器不报错也不生效**
   （第一版模板就是这么写了 `TextWrapping="Wrap"` 并"绿灯"通过）。
   这与"不存在的 `{ThemeResource}` 键静默留默认值"是同一族失败，见 `adaptation/00`。
3. **`ToolTipBackground` 未声明**，其上游目标值在本仓库权威源里不存在（见上）。
   实底表面用的是同文件里有定义的 `ToolTipBackgroundBrush`。
4. **`ToolTipContentThemeFontSize` / `ToolTipMaxWidth` 是 `x:Double` 资源，转录不了**，
   只能写成样式字面量；12 这个数目前没有断言（只有 320 有）。
5. **`BackgroundSizing="InnerBorderEdge"` 没有对应属性**，1px 边框是压在内容区外侧还是内侧未对齐。
6. **没有断言"多行文本真的换行了"**：只证明了 `TextWrapping` 的值落到了文本元素上。
   捕获时高度被宿主强制，换行的可见结果没有单独测。
7. 弹出路径（悬停计时器、键盘焦点、触摸长按）与 `Placement`/`PlacementTarget` 行为未测。
