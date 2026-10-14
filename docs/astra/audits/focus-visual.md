# 键盘焦点环审计：环为什么在鼠标点击时也出，以及它现在挂在哪儿

上游权威：WinUI 3 `microsoft-ui-xaml` @`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
运行时权威：NuGet `Jalium.UI` 26.10.9（与测试工程解析到的同一套程序集）
方法参照：`../ModernWpf` @`23555a6c00623b2f80e67f20d7f1df49a1d28ad8`

这一批不属于任何控件族。它修的是用户报的缺陷：**用鼠标点一下控件，出现的选中框和 Tab 键选中时一模一样**。
出口是三件事：把每一枚环从控件模板里搬出来、搬到框架自己的焦点视觉属性上、并且把"哪些输入种类真的能让它出"
量成读数。没有新增自有类型——这个缺口不缺类型，缺的是一个用对属性的接线。

## 0. 缺陷的形状（改之前的样子）

库里的每一枚环都是模板里的一个双 `Border`：静息 `Opacity=0`，由一格 `<Trigger Property="IsKeyboardFocused" Value="True">`
抬到 1。共 12 处，分布在 5 个样式字典：

| 样式键 | 部件名 | 偏移（环 Border 的 Margin） | 描边 |
| --- | --- | --- | --- |
| `ButtonLayoutStyle`（Button / ToggleButton / DropDownButton / 两个 SplitButton 半区共享） | `FocusOutline` | `1` | 外 `FocusStrokeColorOuterBrush` + 内 `FocusStrokeColorInnerBrush` |
| `SplitButtonSecondaryButtonStyle` | `FocusOutline` | `1` | 双描边 |
| `SplitButtonStyle`（根） | `FocusOutline`（`Grid.ColumnSpan=3`） | `1` | 双描边 |
| `DefaultDropDownButtonStyle` | `FocusOutline` | `1` | 双描边 |
| `FluentToggleSwitchStyle` | `SwitchFocus` | `1` | 只外描边 |
| `DefaultSliderStyle` 横排 | `SliderFocus` | `0,2` | 只外描边 |
| `DefaultSliderStyle` 竖排（`Style.Triggers` 的 `Orientation` 格） | `SliderFocus` | `2,0` | 只外描边 |
| `DefaultCheckBoxStyle` | `CheckFocus` | `-2,1` | 只外描边 |
| `DefaultRadioButtonStyle` | `RadioFocus` | `-2,1` | 只外描边 |
| `ExpanderStyle` | `FocusOutline`（`Grid.RowSpan=2`） | `1` | 双描边 |
| `FluentNavigationItemStyle` | `FocusOutline` | `1` | 双描边 |
| `FluentNavigationPaneToggleButtonStyle` | `FocusOutline` | `1` | 双描边 |

根因一句话：**在这个运行时，鼠标点击同样拿走键盘焦点**，所以 `IsKeyboardFocused` 在鼠标路径和键盘路径上都是
`True`，那一格条件分不出两种输入。探针读数（`spike/FocusCueProbe`，mode `cue`，改之前）：

```
rest                    : IsKeyboardFocused=False ShowFocusCues=False ourRingOpacity=0
Focus()                 : IsKeyboardFocused=True  ShowFocusCues=False ourRingOpacity=1   ← 缺陷本身
PreviewMouseDown        : IsKeyboardFocused=True  ShowFocusCues=False ourRingOpacity=1
PreviewKeyDown Tab      : IsKeyboardFocused=True  ShowFocusCues=True  ourRingOpacity=1
```

`Focus()` 那一行了不该有环，`PreviewMouseDown` 那一行也不该有——它们都有。

## 1. 上游权威：环是 `FocusState` 的函数，不是"有没有键盘焦点"的函数

| 事实 | 锚点 |
| --- | --- |
| `FocusState` 四值：`Unfocused / Pointer / Keyboard / Programmatic` | `dxaml/inc/controls/focus.h`（枚举）；`UIElement` 上的 `FocusStateCore` |
| 焦点视觉只在 `Keyboard`（`UIElement::UpdateFocusStates` 一路走到 `NotifyFocusVisualChange`） | `dxaml/xcp/core/core/elements/UIElement.cpp:6437-6490` |
| 指针按下把状态写成 `Pointer`，键盘导航写成 `Keyboard` | `dxaml/xcp/core/core/dll/focusmgr.cpp:2704-2752` |
| 控件用两个属性声明它：`UseSystemFocusVisuals` + `FocusVisualMargin` / `FocusVisualPrimaryBrush` / `FocusVisualSecondaryBrush` | `controls/dev/CommonStyles/Common_themeresources_any.xaml` 与各控件 `.xaml` |
| 环的颜色行 | `FocusStrokeColorOuterBrush` / `FocusStrokeColorInnerBrush`（我们转录过的那两条） |

所以"鼠标点出环"不是某个控件的格子写错，是**判据选错了属性**：上游问"焦点是怎么来的"，我们问"有没有键盘焦点"。

ModernWpf 那条路不能照抄：它用 WPF 的 `Control.FocusVisualStyle` + `AdornerLayer` + 自己的 `FocusVisualHelper`
（`ModernWpf/Controls/Helper/FocusVisualHelper.cs`），依赖 WPF 已经分得清 `KeyboardFocused` 与鼠标——那是宿主能力，
不是可以搬过来的写法。本运行时把同样的判据公开成了 `FocusVisualManager.ShowFocusCues`，所以方法一致（环交给焦点视觉）
而机制是运行时自己的。

## 2. 这个运行时的对应物（26.10.9 读数）

| 事实 | 读数 |
| --- | --- |
| `FrameworkElement.FocusVisualStyle` | 公开，`Style` 型，读写都有；DP 所有者就是 `FrameworkElement` |
| `FrameworkElement.FocusVisualMargin` / `FocusVisualPadding` | **不存在**（mode `api`：`ABSENT`）——所以上游那些 `-3`、`-1` 的边距没有落脚点 |
| `Jalium.UI.Controls.FocusVisualManager` | 公开静态类，成员只有 `.Property ShowFocusCues`（只读）与 `.Method EnsureInitialized` |
| 门什么时候开 | `PreviewKeyDown` 且键在 `Tab / Left / Right / Up / Down / Home / End / PageUp / PageDown` 时置真；`PreviewMouseDown` 置假 |
| `Jalium.UI.Controls.FocusVisualAdorner` | 公开，基类 `Adorner`；`AttachAdorner` 拒绝未启用的要素 |
| 环住在哪儿 | `FocusVisualAdorner < AdornerLayer < Window`（mode `product` 的父链读数），里面还有一个 `FocusVisualHost` |
| 环被排在什么尺寸 | 被装饰要素自己的边界：200x44 的 Button 得到 200x44 的 adornee、200x44 的环 |
| 框架自带的默认焦点视觉 | `Application.TryFindResource("DefaultFocusVisualStyle")` 命中，`TargetType=Control`、5 个 setter |
| 输入种类能不能当门用 | `InputMode` 只有 `Foreground / Sink` 两值，跟指针/键盘无关——不是可用的判据 |

搬到产品上之后，11 个挂载对象逐族读数（mode `product`，`spike/GalleryRender/prod-run3.log`）。每一族三行：

| 挂载 | `Focus()` 之后 | Tab 之后 | 指针按下之后 |
| --- | --- | --- | --- |
| Button / DropDownButton / SplitButton / CheckBox / RadioButton / Slider 横 / Slider 竖 / ToggleSwitch / Expander / NavigationItem / PaneToggleButton（共 11 行 ×） | `IsKeyboardFocused=True`、`ShowFocusCues=False`、焦点视觉要素 **0** | `ShowFocusCues=True`、焦点视觉要素 **2**、环 2px+1px、半径 4/2、描边与调色板**同一实例** | `IsKeyboardFocused=True`、`ShowFocusCues=False`、焦点视觉要素 **0** |

三点照实记：

- Tab 那一行是**探针**量到的，不是测点量到的（原因见 §5）。
- 没有交环的两族（ComboBox、TextBox，`FocusVisualStyle` 读回 `null`）Tab 之后照样有焦点视觉要素 **2**：框架自带的
  `DefaultFocusVisualStyle` 被实例化，形状是**一圈 3px 单描边**、半径 4、无内圈，描边实例与我们的
  `FocusStrokeColorOuterBrush` 是同一个对象；鼠标按下后同样回到 **0**（`prod-run4.log`）。这条改前改后一样——
  那两族本来也没有模板环，装饰层一直在画它。
- 环的尺寸第一遍读出来有 4 族是 `0x0`（Slider、ToggleSwitch、Expander、PaneToggleButton）。补一次 `UpdateLayout()`
  再读，全部等于被装饰要素的边界。adorner 在同一趟布局里被量，那时 adornee 还没有 render size——真实窗口必然再走一趟，
  所以这是仪器的假象，不是产品的缺陷。写在这儿是因为它是读到的数，不该被"看起来对了"抹掉。

## 3. 产品侧改了什么

新增 `src/FluentJalium/Styles/FocusVisuals.jalxaml`，四个键：`FocusVisualRingStyle`（双描边）、`FocusVisualCheckStyle`
（`-2,1`）、`FocusVisualSliderStyle`（`0,2`）、`FocusVisualSliderVerticalStyle`（`2,0`）。后三个 `BasedOn` 第一个。
清单里那 12 处的环 Border 与抬它的格子删掉，各自主人加一行：

```xml
<Setter Property="FocusVisualStyle" Value="{ThemeResource FocusVisualRingStyle}" />
```

刻意没动的东西：**两枚描边令牌、2px+1px 的双环、半径 4/2、各族自己的偏移**。这批改的是"谁决定环什么时候出"，
不是环长什么样。三处机械性后果：

1. 偏移从控件模板里的 Margin 搬进这四个模板里的 Margin——adorner 排在要素自身边界上，而 `FocusVisualMargin` 不存在。
2. 竖排 Slider 的环换成了 `Style.Triggers` 里 `Orientation=Vertical` 那一格写 `FocusVisualStyle`（以前那是第二个部件）。
   读数：竖排挂到 `FocusVisualSliderVerticalStyle`，横排挂到 `FocusVisualSliderStyle`（mode `product` identity 列）。
3. 每枚环原来还有一格 `IsEnabled=False` → `Opacity=0`，现在没有了：`FocusVisualManager.AttachAdorner` 本身就拒绝
   未启用的要素，框架已经保证禁用态不出环。这一条是 `api` 模式读到的方法语义，不是我们猜的。

`Themes/Manifest.txt` 里这条排在其余 `Styles/*` 之前——下面所有族都通过 `{ThemeResource}` 读它。

## 4. 测点

新增 `tests/FluentJalium.Tests/AstraFocusVisualTests.cs`（19 条）：

| 测点 | 判据 |
| --- | --- |
| `A_family_that_own_a_ring_hands_it_to_the_focus_visual` ×12 | 挂上去的控件读到的是**那一个** `FocusVisual*Style` 实例（`Assert.Same`）——`{ThemeResource}` 真解析了，不是形状像 |
| `No_style_in_the_library_draws_a_ring_from_keyboard_focus` | 全字典源码扫：没有任何 `*Focus*` 命名的模板部件，没有任何 `IsKeyboardFocused` 格子写 `*Focus*` 部件的 `Opacity`；扫过 20+ 文件，否则报"闸口空转" |
| `The_ring_a_family_hands_over_keeps_the_geometry_its_template_used` ×4 | 双描边的粗细/半径/Margin 逐项，加两枚描边与调色板**同实例** |
| `A_pointer_press_keeps_keyboard_focus_without_raising_the_ring` | 按下前后：`IsKeyboardFocused=True`（旧格子的条件确实成立）、`ShowFocusCues=False`、宿主窗口里焦点视觉要素 0 |
| `The_ring_paints_and_a_resting_button_does_not` | 正控制：环样式自己画得出外/内两枚描边的合成墨；负：同尺寸静息 Button 的外描边墨 0 像素 |

改判的老测点（它们当初写的就是"键盘焦点抬环"这套语义，那语义本身是缺陷）：
`AstraButtonTests.A_button_carries_its_ring_on_the_focus_visual_and_not_in_its_template`（原 `A_focused_button_raises_its_focus_ring`）、
`AstraToggleButtonTests.The_shared_focus_ring_reaches_a_toggle_too`、
`AstraSliderTests.A_slider_carries_the_ring_that_fits_its_orientation`（原 `A_focused_slider_raises_its_focus_ring`）、
`AstraSliderTests.Both_slider_templates_carry_one_cell_per_upstream_state`（状态表去掉 `IsKeyboardFocused` 那一行；
`VerticalTemplate()` 原来按位置取格子的第一个 setter，现在按值类型取——环的 setter 插在了 Template 之前）、
`AstraSplitButtonTests.The_shared_focus_ring_reaches_a_split_button`（连带删掉 `RootRing()` 那个按名字找根环的助手）、
`AstraSliderTests.A_disabled_slider_drops_every_surface_to_its_rows`（去掉 `SliderFocus.Opacity` 那一腿）。

## 5. 不声称

1. **不声称键盘导航真能看见环** —— `ShowFocusCues` 只有框架内部的按键路径会置真，而这个运行时公开的
   `KeyEventArgs` 构造函数要一个拿不到的 `PresentationSource`（探针走的是框架内部那个 6 参构造，诊断专用）。
   测点里因此只有"门关着"这一半，没有"开着的时候屏幕上是什么"这一半。Tab 那一半是 §2 的探针读数。
2. **不声称逐像素与 WinUI 的环一致** —— 上游环画在 `FocusVisualMargin` 给的偏移上，我们没有那个属性，偏移进的是
   环自己的模板；形状与颜色同值，落脚机制不同。
3. **负向墨迹只断言外描边** —— 内描边 `#B3FFFFFF` 落在浅底上是 `#FCFCFC`，与 Button 自己的填充撞色（实测一片 120x32
   里有 1486 像素同色），只有外描边那枚暗色可归因给环。
4. 未受影响的两处 `IsKeyboardFocused` 格子照旧：ComboBox 的 `HighlightBackground` 光晕与 TextBox 的边框——上游
   `FocusStates/Focused` 对指针焦点同样触发，我们这格也是，两种输入看着一样正是上游的行为，不是这个缺陷。
5. 高对比档下环用 `SystemColorWindowTextColor`（`HighContrast.map:48-49` 原有映射），这批没重测。

## 6. Known Gaps

- 焦点环"键盘态可见"缺一条测点通路：需要一个公开的按键注入口，或一个能在测点里安全构造 `KeyEventArgs` 的公共构造。
  归 #13（真指针输入的像素通路）同一族——那批要的是 hover/press 的像素，这批要的是 key-down 的门。
- adorner 住在窗口的装饰层，元素的裁剪框之外，所以"环在屏幕上第几像素"这类页级断言要窗口级捕获；#63 那条闸
  现在是按页裁剪的，接不接得到环还没量。
- **键盘框的形状在库里现在有两套**：交给我们环的 12 处是 2px+1px 双圈（WinUI 的标准形状），没交出的那些族
  （ComboBox、TextBox、列表项、标签页…）由框架 `DefaultFocusVisualStyle` 画一圈 3px 单描边。要把后者也统一到
  双圈，得逐族对着上游审 `UseSystemFocusVisuals` 与 `FocusVisualMargin`（上游确实有族显式关掉系统框，例如
  ContentDialog 的按钮），不属于本批"只换判据、不动像素"的范围。挂在视觉余账（与 #21/#23 同族）。
