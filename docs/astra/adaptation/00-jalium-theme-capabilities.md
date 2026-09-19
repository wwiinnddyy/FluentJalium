# Jalium.UI 26.10.9 主题与模板能力实测

阶段 0 闸口结论。来源是 `spike/AstraSpike`（一次性探针工程，不在 `FluentJalium.slnx` 内），
每条结论后面是实际测到的值，不是从文档推断的。**这些结果修正了计划书 B 节的三处机制判断**，
后续所有控件工作以此文为准。

运行时：NuGet `Jalium.UI` 26.10.9，`net10.0-windows`，Impeller 后端，真实已显示窗口。

## 结论一览

| 闸口 | 结论 | 对计划的影响 |
|---|---|---|
| S0-a `{ThemeResource}` 能否重解析 | **能**，但只有 `Application.ThemeMode` 是驱动 | 决策 3 成立，机制换掉 |
| S0-b 非刷子令牌能否进 `ThemeDictionaries` | **`Color`/`CornerRadius`/`Thickness`/`Duration` 能；`x:Double` 不能** | 数值令牌继续内联字面量 |
| S0-c WinUI 别名元素形式 | **能解析且解析到真实刷子实例** | 上游 `_themeresources.xaml` 可近乎逐字转录 |
| S0-d 标记里写 `VisualStateManager` | **不能，两种写法都抛异常** | 模板改走 `ControlTemplate.Triggers` |
| S0-e 编译期 `JalxamlPage` 产出字典 | **产出 0 个类型** | 字典保持 Embedded + `XamlReader.Load` |
| S0-g `Trigger` 条件能匹配什么值 | **`{x:Null}` 能、`Value=""` 在 ComboBox 上不能**；AutoSuggestBox 批证明空串条件不是永久失效 | 判据仍要逐属性读回，不许按语法推断 |
| S0-h 原生控件到底有没有默认外观 | **有：代码构建的 `ControlTemplate`，不是样式**（NumberBox 批实测） | 重模板可行，普查里"自绘"与"0 个有默认样式"两句话要分开读 |
| S0-i 换掉框架模板要守住什么 | **弹层部件树是契约**：四个名字 + items host 必须是面板；框架还占有条目底色与禁用前景 | 弹层类控件先量部件名，再写模板 |
| S0-j 弹层能不能断像素 | **能断"某色存在"，不能断"画面干净"**：裁剪含框架渐变的条目，`Stable` 与品牌绿闸口在此失效 | 弹层像素只作存在性证据，洁净度留给自有类型 |
| S0-f `ThemeColors` 可否桥接 | **71 个 public 静态 `Color`，零 public setter**；四种公开写入口全部无效 | 天花板只限读这张表的自绘代码，见 `01-jalium-control-census.md` |
| S0-k 不改模板怎么驱动交互，样式格子排在谁下面 | **`IToggleProvider.Toggle()` 就是框架自己的 `OnClick→OnToggle`**；样式格子输给本地值，`{x:Null}` 在样式格子里也命中 | 按钮族循环先走自动化模式；状态要覆盖本地底色就得自有类型 |
| S0-l 弹层打开态、部件名与"样式生效"怎么读 | **`FlyoutBase.IsOpen` 是只读 CLR 属性不是依赖属性**（打开态既绑不了也当不了格子条件）；**模板部件名是功能契约**（改名后照样建树照样上色，点了却没反应）；**隐式样式从不写进 `Style`**；星形列尊重子元素对齐 | `FlyoutOpen` 不声称；命名正反两向都断；生效判据改用模板对象身份；半区显式 `Stretch` |
| S0-m 原生控件"不接受模板"是真不接受吗 | **不是：`UseTemplateContentManagement()` 是 `ContentControl` 的 protected 方法，Expander 构造里调了、InfoBar 没调**，所以 InfoBar 的 `Template` 既走样式也走本地值都不成树；派生类补一句即成。另外 `UIElement.RaiseEvent` 是 public，能合成 `MouseDown` 驱动控件自己装的处理器 | 遇到"模板无效"先量开关再决定自绘/自有类型；交互行为可走路由事件，但像素洁净度仍要真指针 |
| S0-n 同一族的"能不能重模板"能外推吗 | **不能**：菜单族里 `Menu`/`ContextMenu`/`MenuFlyoutItem` 全家成树，`MenuItem` 存而不建（`OnRender` 自绘），`MenuBar` 无模板可装；`IsHighlighted`/`IsSubMenuOpen` 只是 CLR getter 不是 DP | 状态格只挂真 DP；逐类型量，不按基类外推 |
| S0-o 行遮蔽与探针模板的边界 | **遮蔽与隐式样式覆盖都实测成立**（后合并者胜、只对之后建造的控件）；但 `XamlReader.Parse` 的临时模板里 `ControlTemplate.Triggers` 永不生效，`VisualStateManager` 类型根本不存在而单独 Parse 返回 ok | 状态普查只能用编译字典；"解析没抛"不算能力证据；`IsMouseOver` 写不进但路由按下会置真，hover 行可进像素 |
| S0-p 屏幕上「重影」的第二位画者是谁 | **是基础类自己的 `OnRender`，而它让路的开关是模板部件名**：`InfoBar.OnApplyTemplate` 用 IL 字面量找 `RootBorder` / `PART_CloseButton`，找不到 `RootBorder` 就在我们的模板之上再画一遍图标、标题、消息和关闭叉；改名之后「上样式」与「把 `OnRender` 覆写成空」两种条逐像素相同（7 色 / 936 ink，改名前多 178 px 框架蓝）。普查：93 个导出控件自带 `OnRender`，`NumberBox`（`PART_LayoutRoot`）、`Slider`（`PART_Segments`）、`ComboBox`（`PART_SelectionPresenter`）的部件名仍有缺口 | 重模板之后要逐控件确认基础类已让路；部件名契约能用反射读 IL `ldstr` 机器取出来，不用猜，也不用私有字段反射 |

## S0-a：主题切换的真实驱动

同一份内嵌字典（`XamlReader.Load` 运行时解析）里 `{ThemeResource SpikeBrush}`，
在 `ThemeDictionaries` 下声明 Light/Dark/HighContrast 三套值，窗口已 `Show()`：

```
baseline Light:                                    theme=#FFFF0000  static=#FFFF0000
[1] CurrentThemeKey=Dark 单独:                      theme=#FFFF0000  static=#FFFF0000   ← 无效
[2] + ThemeManager.ApplyTheme(ThemeVariant.Dark):    theme=#FFFF0000  static=#FFFF0000   ← 无效
[3] Application.ThemeMode = ThemeMode.Dark:          theme=#FF00FF00  static=#FFFF0000   ← 生效
[4] Application.ThemeMode = ThemeMode.System:        theme=#FF00FF00  static=#FFFF0000
```

同一轮里 `CornerRadius 4,4,4,4 -> 8,8,8,8`、`Thickness 8,4,8,4 -> 16,8,16,8`、
`Duration 00:00:00.167 -> 00:00:00.333` 全部随之改变，而用 `{StaticResource}` 的对照组
**始终停在 `#FFFF0000`**。

- `ResourceDictionary.CurrentThemeKey` 是 public 可写，但它是**结果不是开关**；
  手动赋值不重驱动已解析的订阅。
- `ThemeManager.ApplyTheme(ThemeVariant)` 也不驱动应用级合并字典。
- `Application.ThemeMode`（类型 `Jalium.UI.ThemeMode`，静态成员 `None/Light/Dark/System`）
  是唯一 public 的应用级驱动，且**不需要任何额外通知**。

**因此**：`FluentThemeManager` 的公开切换入口应当只做一件事——把 `FluentThemeVariant`
映射成 `Application.ThemeMode`。现有实现里对每个窗口的 `InvalidateVisual()`、
整棵可视树的 `ApplyMotionPolicy` 递归，以及 `Application.NotifyThemeResourcesChanged`
（该成员在文档里有，但**不是 public**，产品代码调不到）全部可以删除。
这是对 AGENTS.md 两条禁术的正当替代，而不是换个写法的等价物。

未测：切换时刷子实例标识是否保持。上面 `[2]` 行的 `brushIdentityPreserved=True` 是**无效读数**——
那一步根本没有发生重解析，比较的是同一个未变的值。阶段 1 需在 `ThemeMode.Dark` 之后重测
`ReferenceEquals`。

## S0-b：`x:Double` 是唯一进不去的令牌类型

```
Embedded/Doubles.jalxaml:        PARSE-FAIL  Cannot resolve type 'Double' in
                                       namespace 'http://schemas.microsoft.com/winfx/2006/xaml'
Embedded/ThemedDoubles.jalxaml:  PARSE-FAIL  同上（放进 ThemeDictionaries 也一样）
Embedded/Palette.jalxaml:        OK  Light/Dark/HighContrast 三套
  SolidColorBrush / Color / CornerRadius / Thickness / Duration 全部解析并随主题改变
```

运行时 reader 解析不了 xaml 语言命名空间下的基元类型，与是否主题化无关。
后果与处置：

- WinUI 的**数值**度量（`*DependentRadius`、`OverlayPopupThemeTransition` 之类不含，但
  `ButtonPadding`、最小高度、字号等）继续按字面量内联，并在每个控件的 `.parts.md` 契约表里
  逐项对照上游数值，防止手抄漂移。
- 消费一个不存在的键是**静默失败**：探针里 `Width="{ThemeResource SpikeDouble}"` 没有报错，
  最终值就是默认 `NaN`。这类"绿灯但没绑上"的家族与已知的 `{x:Bind}` 静默丢弃同源，
  所以资源键清单必须能反查每个被消费的键确实存在（阶段 3 的键闸口）。

## S0-c：WinUI 别名语法可用

```xml
<StaticResource x:Key="SpikeAliasBrush" ResourceKey="SpikeBrush" />
```

解析通过，且取到的值是 `SolidColorBrush` 本体。这意味着 WinUI 每个控件
`X_themeresources.xaml` 里的别名块可以近乎逐字搬进来，是"逐字照抄上游"这条方法学
在 Jalium 上最实在的支持。

注意：这与 S0-a 不冲突——别名在解析期绑定到同一个实例，主题切换时该实例的 `Color`
被框架原地更新，所以两者叠加仍然正确。

## S0-d：标记里的 VisualStateManager 不可用

WinUI 风格的两种写法在模板应用时都抛同一个异常：

```
VsmStoryboardStyle:    APPLY-THREW ArgumentException:
                       Object of type 'Jalium.UI.VisualStateGroup' cannot be converted to 'System.Collections.IList'
VsmWinUiSettersStyle:  同上
```

`VisualStateManager.VisualStateGroups` 的附加属性类型是 `IList`，而标记解析器交来的是
单个 `VisualStateGroup`。**结论**：不能把 WinUI 模板里的 VSM 块直接复制过来。

处置：状态视觉一律用 `ControlTemplate.Triggers` + `MultiTrigger`（inkcanvas 已验证可行），
状态名与部件名仍照抄 WinUI 以便审计对照，但在审计文档的 `Host Substitutions` 里必须记为
"状态由触发器表达，非 `VisualStateManager`"。
`VisualStateManager.SetVisualStateGroups(FrameworkElement, IList)` 是 public，
理论上可由代码构建状态组；阶段 2 若发现触发器表达不出某个 WinUI 状态迁移再评估，不预先启用。

## S0-e：编译期路径不产出字典

```
assembly types: 6; ResourceDictionary-derived: []
```

把 `ResourceDictionary` 写成 `JalxamlPage` 编译项，源生成器**不产出任何可实例化类型**。
加上 S0-b 的失败是类型解析问题、编译路径也没给出可加载的东西，A 节的问题就此定案：

**字典保持 `EmbeddedResource` + 运行时 `XamlReader.Load`**，`EnableJalxamlCodeGeneration=false`
不变。这条同时排除了计划书里"迁移到编译期降低"的选项。

## S0-f：`ThemeColors` 读得到、写不进

> **本节两处已被 `02-render-ceiling.md` 更正**：
> 1. "四种公开写入口全部无效"只对 `ThemeColors` 这张**表**成立；实测像素证明
>    `ThemeManager.ApplyAccent(color)` **能**驱动 Slider / ProgressBar 等控件的强调色渲染。
>    当时我在 `ApplyAccent` 之后又调了一次空参数 `ApplyBrandTheme(options)`，
>    把强调色重置回品牌默认值，是我自己引入的干扰变量。
> 2. "重模板改不动一整类控件"言过其实：163 类型 / 8461 方法的 IL 扫描（带校准对照）显示
>    真正的实时读取点只有 **4 处，且全部只读 `Accent`、全部不在 `OnRender` 里**。

`Jalium.UI.Controls.Themes.ThemeColors` 暴露 71 个 public 静态 `Color` 属性，
**没有一个有 public setter**（逐个反射确认）。清单里与 WinUI 控件直接相关的包括
`SliderTrack`、`SliderThumb`、`ScrollBarThumb/Hover/Track`、`TitleBar*`（含
`TitleBarButtonHover`、`TitleBarCloseButtonPressed`、`TitleBarGlyph`）、`TabStripBackground`、
`TabItemSelectedBackground`、`TabItemIndicator`、`ToggleChecked*`、`CheckMark`、
`ProgressBarFill`、`DropdownBackground`、`TextBoxBackground`、`SelectionBackground`、
`WindowBackground`、`Accent*`。

**含义**：凡是由这些颜色支撑的部件，**重模板改不动**。也就是说"原生优先"必须按部件类型分层：

- 纯模板绘制（`Border`/`ContentPresenter` 组成的可见部件）→ 重模板可以完全还原 WinUI；
- `OnRender` 或 `ThemeColors` 驱动（Slider 轨道与拇指、滚动条三件套、标题栏、
  Tab 条与指示器、进度条填充、下拉底色）→ 模板只能改外围，核心颜色仍是框架的品牌绿。

这不再只是 inkcanvas 报过的 Slider 一个个例，而是一整类。受影响的候选控件在阶段 4 排批时
要单独标注 `ThemeColors-bound`，并对每个决定是否按 C.9 毕业为自有类型。

## 高对比度：公开管线里没有入口

`ThemeVariant` 枚举只有 `{Dark, Light}`；`ThemeMode` 静态成员只有 `{None, Light, Dark, System}`；
**没有任何公开的高对比入口**。实测把 `CurrentThemeKey` 强行设为 `"HighContrast"`：

```
ThemeMode.Dark + forced CurrentThemeKey=HighContrast: theme=#FF00FF00 radius=8,8,8,8  ← 仍是 Dark 的值
```

所以计划 B.6 里"生成真正的 HighContrast `ThemeDictionaries`"**做不到**（除非上游补 API）。
处置：高对比继续是 `FluentThemeManager` 自己的一层显式刷子重映射，但要改成
**语义角色映射表**（WinUI 的 HC 主题字典本身就是逐键覆盖，照抄那张表），
而不是现在这套按键名猜 `Text`/`Stroke`/`Accent` 子串的启发式；
并且必须如实写成 host substitution，不得声称 HC  parity。

## 阶段 1 落地（2026-09-18，提交 `bbf3847`）

原计划五条，实测后处置如下。探针：`spike/ThemeRoute`（只读元数据与 IL，不渲染），
原始输出 `00-theme-route-raw-output.txt`。

### 公开面上确实没有替代驱动

`Jalium.UI` 26.10.9 里带 `[Experimental]` 的 public 成员一共只有 4 个，全在同一族：

```
Jalium.UI.ThemeMode            [type]      Experimental(WPF0001)
Jalium.UI.ThemeModeConverter   [type]      Experimental(WPF0001)
Jalium.UI.Application.ThemeMode [property] Experimental(WPF0001)
Jalium.UI.Window.ThemeMode      [property]  Experimental(WPF0001)
```

诊断文本是「仅用于评估，在将来的更新中可能会被更改或删除。**取消此诊断以继续**」——
框架给的唯一豁免方式就是按 ID 抑制。IL 调用图解释了 S0-a 的读数：

- `Application.set_ThemeMode` → `ThemeManager.ApplyTheme(ThemeVariant)`（public）
  + `ApplyWindowsSystemThemePreference`/`ApplyCachedSystemThemePreference`；
- `ThemeManager.ApplyTheme` 只做 `ResourceDictionary.CurrentThemeKey` + 私有
  `ForceThemeRefresh` + `CompositionTarget.RequestImmediateFrame`；
- 真正广播重解析的 `Application.NotifyThemeResourcesChanged`（遍历
  `Window.SnapshotOpenWindows`/`PopupWindow.SnapshotOpenPopupWindows` +
  `ResourceLookup.InvalidateResourceCache`）是 **internal**，
  `ResourceDictionary.InvalidateMergedLookupCaches`/`NotifyKeysChanged` 同样 **不是 public**。

所以单独调用 public 的 `ThemeManager.ApplyTheme` 不动已解析的订阅，而 `ThemeMode` 会——
它内部调 `ApplyTheme` 再补上那次广播。**结论：没有非实验性的替代路径。**
`Application.NotifyThemeResourcesChanged` 的文档文本还顺带确认了混合模型是对的：
「主题字典跨变体保持同一批 `Style`/`ControlTemplate` 实例，控件需要刷的是
`ResourcesChanged` 钩子里手动缓存的刷子和保留的绘制命令」。

### 五条计划的最终处置

1. **门面收敛**——`ApplicationThemeDriver` 是全仓唯一 `#pragma warning disable WPF0001`
   的文件，模式以字符串进出，实验性类型不外泄到 Astra 公共面；`ApplyMotionPolicy`
   整树递归、逐窗口 `InvalidateVisual()` 已删除（三处产品控件与 Gallery 调用点一并移除）。
2. ~~**调色板升级为真 `ThemeDictionaries`**~~ → 改为**混合模型**（用户决定）：保留单份就地
   改色的调色板，笔刷实例跨主题不变，这样 `OverrideBrush`、强调色、`SystemColor*` 水合
   都只有一处写入；再额外赋值 `Application.ThemeMode`，让框架自有的托管字典与原生默认
   一起跟。放弃 ThemeDictionaries 的理由：S0-a 已证明就地改色足够驱动 `{ThemeResource}`
   消费者，而双份实例会让"覆盖单个笔刷"变成多次写入。
3. **强调色**——`ThemeManager.SystemAccentResolver`/`ApplyAccent` 不动 Astra 的键；
   我们的 accent 族仍由 `FluentThemeManager.ApplyAccent(Color?)` 原地改色，
   `OverrideBrush` 优先。DWM 强调色接入留到 Gallery 的 Tokens 页一起做。
4. **键闸口**——`tests/FluentJalium.Tests/Resources/AstraResourceKeyTests.cs` 解析全部内嵌
   字典，断言每个 `{ThemeResource|StaticResource|DynamicResource}` 引用都有声明，
   且 `{ThemeResource}` 键在每个主题分支都存在；Light/Dark 键集必须一一对应。
5. **补测 `ThemeMode` 翻转后的实例标识**——`AstraThemeRuntimeTests` 断言
   `Assert.Same` 跨 Light→Dark 成立且应用级查找回到同一实例，已通过。
   附带发现：xunit 每个测试换一个工作线程，因此夹具自带一条 STA 线程并串行投递，
   UI 线程守卫保持为真而不是为测试放宽。

### 像素基座的三个新读数（`tests/FluentJalium.Tests/Pixel/PixelHarness.cs`）

1. **直接 `RenderTargetBitmap.Render(visual)` 只吃自绘内容。** 裸 `Border` 的 40×40 实心底色
   1600 px 全中；未挂窗口的原生 `Slider`/`ProgressBar` 也拿到了像素（见下面第 3 条）；
   但一个**有模板**的 `Button`，即使 `IsVisible=True`、`IsLoaded=True`、`Template != null`、
   `ActualWidth=200`，直接捕获仍是 8800 px 全黑——只有把 `Window` 本身交给 `Render`
   才出得来 `#F5F5F7` 那类真实表面色。所以带模板的控件必须**经窗口捕获**，
   harness 因此有 `Render`（自绘/形状）与 `Host`（模板）两条路径。
2. **主题驱动到像素这一步已经证到**：同一颗 `Button` 经 `Host` 在 Light 与 Dark 下的
   主色分布不同（`A_hosted_control_rasterises_and_changes_with_the_theme` 绿）。
   这是本仓第一条"渲染输出随主题改变"的断言，不再只是字典里的值变了。
3. **未样式化的原生 Slider/ProgressBar 仍涂品牌绿**：`Slider` 954 px `#207245`、
   `ProgressBar` `#1D733C`–`#2B804A`。与 `02` 的 IL+DP 结论一致——框架没发布 Generic 主题，
   这两个是自绘的。已写成 `Skip` 断言并注明是天花板而非回归。

**顺带查出的一个 Astra 缺陷**：给 `Button` 设本地 `Background` 在像素里看不到
（模板根 `Border` 没走 `TemplateBinding Background`）。WinUI 的 `Button` 根 Border 是跟随 `Background` 的，
这条归阶段 2 的 Button 样板一起修，别丢。

**仍未证**：`ThemeMode.System` 是否真的跟住 OS 切换（测试只断言赋值生效）；
`Host` 捕获到的是 `RenderTargetBitmap` 里的窗口合成结果，与真正上屏的帧是否逐像素一致仍未核对
（本机没有可用的截屏核对手段，见 AGENTS 之外的既有结论）。

## S0-g：触发条件的可用值形式（ComboBox 批，2026-09-18）

`ControlTemplate.Triggers` 的条件不是"标记写了就算"。同一份模板里三种形式的实测结果：

| 条件写法 | 运行时 | 结论 |
|---|---|---|
| `<Condition Property="IsChecked" Value="{x:Null}" />` | 三态 CheckBox 命中（选择批已证） | null 判据可用 |
| `<Trigger Property="Text" Value="" />` | 结构读取 `Property` 解析成功、setter 键名正确，但控件实际前景仍是静止值；换判据后立即命中 | ComboBox 上不生效；AutoSuggestBox 批给出反例，见本节末 |
| `<Trigger Property="SelectedIndex" Value="-1" />` | 命中，占位符格读到 `ComboBoxPlaceHolderForeground` 实例，且 `ReadLocalValue=UnsetValue` | 数值判据可用 |

`Value=""` 这一条最阴：结构与消费点两类闸口全绿，像素与读回全是静止态——和字典模板丢 `Trigger.Property`
那一类是同一个坑的另一种形态（"看起来接上了，其实没匹配"）。因此本仓的规矩是：**每条状态格都必须有一条
读回断言**，只有 markup 结构断言的格子不算证据（`AstraComboBoxTests` 里占位符那三条就是这么来的）。

同时补一条控件级实测：ComboBox 的占位符不是独立可视元素，框架把占位串塞进 `SelectionBoxItem`
（`SelectedIndex=-1` 时读回 `"Pick one"`），所以"没有选中项"这件事在 `SelectionBoxItem` 上没有 null 表示。

## S0-h：原生控件自带的是"模板"，不是"样式"（NumberBox 批，2026-09-18）

一个全新 `Jalium.UI.Controls.NumberBox` 挂上屏后：`Style` 是 null、`Template` 却非 null，树里已经
有 `OuterBorder` / `PART_LayoutRoot` / `PART_ContentHost` / `PART_UpSpinButton` / `PART_DownSpinButton`，
而 `Template` 上读不到本地值。也就是说 `adaptation/01` 那句"163 个控件 0 个有框架默认样式"是对的，
但它推不出"这些控件没有外观"，更推不出 `adaptation/05` 里"NumberBox 自绘，模板只能改外围"——
带 `Template` setter 的样式实测能把整套默认外观换掉（`ReferenceEquals(box.Template, ours) == true`，
哨兵色进像素）。**判"能不能重模板"要看 `Control.Template` 能否被样式替换，不要看有没有样式。**

换模板之后必须守住的三条框架契约（都有断言，见 `audits/numberbox.md`）：

1. 文本宿主只能长在**面板**上：名为 `PART_ContentHost` 的元素是 `Grid` 时框架才把 `TextBoxContentHost`
   嫁接进去；换成 `ContentPresenter` 就不嫁接，捕获直接塌成 2 个颜色。
2. 框架会在它认识的部件名上写**本地值**：`PART_UpSpinButton` 拿到本地 `BorderThickness=1,0,0,0` 与
   `CornerRadius=0,4,0,0`（圆角的 4 来自控件自己的 `ControlCornerRadius`，即我们的令牌在驱动它）。
   本地值压过样式 setter 与模板格子，所以上游 `NumberBoxSpinButtonBorderThickness`（`0,1,1,1`）到不了。
3. 换模板之后 `SpinButtonPlacementMode` 不再被框架处理：三种取值来回切，我们的部件毫无反应，
   可见性与弹层只能由模板格子自己实现。

另外三条与探针方法直接相关：

- **裸模板不水合条件。** 把单独的 `ControlTemplate` 交给 `XamlReader.Parse`，每个 `Trigger.Property`
  读回 null，连 `IsEnabled=False` 都不触发；同一标记放进 `ResourceDictionary → Style → Setter → 内联模板`
  的生产形状就全部正常。第一轮据此得到的"枚举条件永不匹配"是探针伪影，已作废。
- **枚举条件会触发。** 生产形状下 `Trigger Property="SpinButtonPlacementMode" Value="Hidden"` 实测改掉了
  部件可见性，`Trigger Property="Header" Value="Count"`（Object 属性配字符串）也会命中。
- **同一目标上后写的格子赢。** 探针里 Compact 格被更靠后的 Header 格压掉，看起来像"条件不匹配"。
  写断言时要么让每格只碰自己的目标，要么按文档顺序断言优先级。

## S0-i：弹层部件树是契约，不是实现细节（AutoSuggestBox 批，2026-09-18）

`AutoCompleteBox`（本运行时的 AutoSuggestBox 替身）在挂载时才拿到框架自建的模板，未挂载时
`Style` 与 `Template` 双双为 null——所以"能不能重模板"这类判断对它是**挂载后**才能测的那件事。
换掉它的模板时，框架仍然按名字找它的弹层，实测到的最小可用形状是：

```
Popup 'PART_Popup' (Placement=Bottom)      ← 框架写本地 Width，等于控件宽度
  └ Border（上游叫 SuggestionsContainer）    ← 我们的行落在这里
      └ Grid 'PART_DropDownBorder'
          └ ScrollViewer 'PART_DropDownScrollViewer'
              └ Panel  'PART_DropDownItemsHost'   ← 必须是面板
```

三种失败都有读数：`PART_Popup` **没有 Child** → 弹层开成空的 20 DIP PopupRoot；
`PART_DropDownItemsHost` 是 **ItemsControl** → 框架永不填它（容器数 0）；换成 `StackPanel` → 容器照常生成，
类型是 `ComboBoxItem`。框架还会在两处写自己的值：条目容器的 `Background`（一个本地渐变，压过样式）与
禁用态控件前景（`#FF636366`，不是我们的 `TextControlForegroundDisabled`）。这两条都只能用
`Assert.NotSame` 钉成"已知损失"。

**框架的模板里还藏着一处行为。** 用框架模板时"选中建议 → 把补全文本写进 `Text`"（上游的
`UpdateTextOnSelect`）存在；换模板后同一操作不再写文本（聚焦、`IsTextCompletionEnabled=true` 也不写）。
资源行找不回它——这是"外观走重模板、行为要自有类型"的分界线，本批把它写成断言而不是缺口清单里的一行字。

## S0-j：弹层裁剪的像素证据有两条读不出的噪声（同上）

`PixelHarness.Chrome(弹层部件)` 能证明弹层表面令牌真的到了像素（哨兵是该裁剪的第一大色，1092 px），
但这个裁剪**包含条目容器**，容器带框架本地渐变，于是：

- 连拍两轮永不同 → `Sample.Stable` 在此裁剪下不可断言；
- 裁剪里会出现非调色板的颜色 → "品牌绿 `#207245` 不出现"这条闸口在弹层裁剪上也不成立。

结论：**弹层像素只断"某色存在且够大"，不断"画面干净"**。要断后者得先把条目容器排除在裁剪之外，
而条目底色本身是框架占有的（S0-i），本运行时做不到。

## S0-k：不动模板怎么驱动交互，样式格子排在谁下面（ToggleButton 批，2026-09-18）

阶段 0 到阶段 2 为止，"交互"在这套闸口里是空的：测试不能设 `IsMouseOver`/`IsPressed`
（那是 `UIElement` 上的框架内部状态），真指针要物理鼠标且会被同时用鼠标的人打断，
真按左键又会点到用户桌面上的任何东西。这一批量到一条**不碰输入管线**的通路：

```csharp
((IToggleProvider)new ToggleButtonAutomationPeer(toggle)).Toggle();
```

`Jalium.UI.Automation.Peers.ToggleButtonAutomationPeer` 与
`Jalium.UI.Automation.Provider.IToggleProvider` 都是 public，`Toggle()` 走的就是框架自己的
`OnClick → OnToggle → OnIsCheckedChanged`，所以两态循环 `false→true→false`、
三态 `false→true→null→false`、`Checked/Unchecked/Indeterminate` 三个事件的次数，
全部能在闸口里、无指针、无桌面副作用地断。**限制要说清**：它证明的是"框架的激活路径还在"，
不是"鼠标点得动"——指针/键盘/触摸三条通路的像素仍欠（Task #13）。
附带一条契约：控件禁用时 `Toggle()` **抛** `InvalidOperationException("Cannot toggle a disabled control.")`，
不是静默忽略（与 WinUI 该模式抛 `ElementNotEnabledException` 同侧），
应用侧从自动化驱动开关要准备好接这个异常。

同一批量到两条优先级读数和一条格子形式读数：

1. **本地值压过样式格子**。挂载 + `Background` 本地刷 + `IsChecked=true` → 读回还是本地刷。
   上游 `ToggleButton` 的三属性由 `VisualState` 故事板驱动，故事板压过本地值，
   方向恰好相反。于是"给开关设个本地底色"这种 WinUI 里常见的写法，
   在这里会产出一个**永不换色**的开关。这条与 S0-h/`audits/textbox-passwordbox.md` 那张
   "框架本地值压过样式"的账单是同一族，但这次输的是**我们自己的格子**，
   故按实测钉成 `A_local_fill_keeps_the_surface_over_the_checked_cell` 而不是写进缺口清单。
2. **两条同时成立的格子，标记里后面的赢**（`IsChecked=True` 与 `IsChecked=True+IsEnabled=False`
   同时成立时落地的是后者）。这意味着格子顺序是语义，不是排版。
3. **`{x:Null}` 条件在样式格子里同样命中且可逆**。S0-g 记的是模板格子（CheckBox），
   两条是不同的解析路径，不能互推——探针（只带 null 条件、混合底色指到没人读的强调刷）
   才证明得到，并且设回 `false` 后底色回到休息位，排除了"一次性误命中"。

**方法论收获**：上游把 `ToggleButton*Indeterminate*` 全部映射到与休息位同名的调色板刷
（同一实例），所以混合态在像素上与未勾选态**不可区分**——这不是我们的缺口，是上游的定义。
凡是"某状态看起来对不对"的主张，先问它有没有**可区分的像素**；没有，就只能给探针 + 读回，
并且必须把这条写进不声称清单，免得后来者把那条代理信号当证据。

## S0-l：一处名字、一处只读属性、一处对齐（SplitButton / DropDownButton 批，2026-09-19）

这一批没有新增任何"能做什么"，它量到的是**四条读不出来的东西**，每条都会让一套看起来完整的闸口失效。

**1 · 弹层的打开态在本运行时不可读，所以 `FlyoutOpen` 画不出来。**
`SplitButton` 只声明 `Command`/`CommandParameter`/`Flyout`，**没有** `IsDropDownOpen`，
`CreateAutomationPeer()` 返回空；`FlyoutBase` 一侧有 `ShowAt(FrameworkElement)`、`Hide()`、
`Opened/Closed` 事件，但 `IsOpen` 是**只读 CLR 属性**，`DependencyProperty.FromName` 取不到它（探针 F/J）。
本运行时的格子条件只能读被模板控件自身的属性，绑定也只能走依赖属性，于是上游那 15 个状态里
`FlyoutOpen` 与 `TouchPressed` 两个**没有任何驱动**。我们既没有伪造一个 `IsFlyoutOpen` 自有类型，
也没有反射私有字段（`AGENTS.md` 明令禁止），而是把它写进缺口：菜单打开时整体压暗这件事，本库现在做不到。

**2 · 模板部件名是功能契约，结构和像素都抓不到它坏掉。**
自建模板里两个半区叫 `PrimaryButton`/`SecondaryButton` 时：Invoke 次半区 → `Flyout.IsOpen=true`，
Invoke 主半区 → 控件 raise 一次 `Click`；把它们改名成 `…Z` 后，**建树成功、上色正常、布局不变**，
但 Invoke 之后 `IsOpen=false`、`Click` 一次都没有（探针 I）。也就是说这类破坏能同时骗过
"模板结构对不对"和"像素像不像 WinUI"两类断言。闸口因此必须**正反两向**都测
（`The_named_halves_drive_the_flyout_and_the_click_and_the_renamed_ones_do_not`）。
推论：以后凡是"框架靠名字接线"的控件（`Expander`、`ContentDialog`、`CommandBar`），
模板落地第一件事是量出它认哪些名字。

**3 · 隐式样式不写进 `FrameworkElement.Style`。**
挂上主题之后 `Button`、`ToggleButton`、`HyperlinkButton`、`SplitButton` 的 `Style` **全是 null**，
而 `TryFindResource(该类型)` 拿得到样式，像素也确实是我们令牌的颜色（探针 K）。
所以"`control.Style` 非空"不是生效证据，任何这样写的断言都是假阳性；
本批改用**模板对象身份**（`Assert.Same(Template(ours), control.Template)`）作为读法。

**4 · 星形列尊重子元素对齐，共享布局样式的 `Left` 会把面缩掉。**
`ButtonLayoutStyle` 给按钮族设了 `HorizontalAlignment=Left`（上游按钮的默认值），
把它当半区样式用时，`Width="*"` 列里那一半只拿到内容宽：**220×36 的控件上主半区实测 53.09×34.78**，
剩下 131px 宽（4832 像素）在捕获里是空的——而 `Stable` 是真的稳定，所以这不是时序问题。
修法是半区显式 `Stretch`/`Stretch`（上游 227 行也是这么写的），
并由 `Both_halves_stretch_into_their_columns` 与休息底色阈值（>5000 px）一起把关。
推论：凡在 `*` 列里承载可换色面的模板，都要显式声明对齐。

另外两条属于"框架替我们做了事，照抄上游就会错"：

- **命令会被执行两次**。上游把 `Command` 绑到主半区（`SplitButton.xaml` 227 行），因为 WinUI 的
  `SplitButton` 自己不执行命令；本运行时会，于是一次 Invoke 实测 `Execute` 两次。模板因此**不**绑命令。
- **没有 `Flyout` 时框架自己禁用次半区**（挂载实例的次半区读回 `ControlFillColorDisabledBrush`，探针 C）。
  这不是缺陷（与 WinUI 语义一致），但意味着我们的禁用格子会被框架的启用状态驱动，
  写样例时"没弹层的 SplitButton 右侧是禁用色"是正常现象，不是样式没生效。

## S0-m：一处开关、三处名字、一处公开事件口（Expander / InfoBar 批，2026-09-19）

来源：`spike/ExpanderInfoBarProbe`（同一探针跑三遍：部件名契约 → 开关可及性 → 子类实测）、
`spike/SurfaceProbe`（阶段 4 表面普查）。运行时 26.10.9。

**1 · "原生控件没有模板通道"通常只是没开 `UseTemplateContentManagement()`。**
`InfoBar` 挂载后 `Template==null`、可视子节点 0；把同一个 `ControlTemplate` 作为本地值赋上去，树里仍然只有
`ContentPresenter` 呈现的内容——而同一个赋值在 `Expander` 上立刻成树（pass 2 M1、pass 3 Q）。
差别不在解析，而在这个类型没调用模板通道开关：`Expander` 构造函数调了，`InfoBar` 没调。
该开关是 **`ContentControl` 的 `protected` 方法**（反射签名实测），因此派生类构造里一句
`UseTemplateContentManagement()` 就够，不需要私有反射、不需要伪造自绘类型：模板成树之后，
基础类自己的 `OnRender` 一进门就检查有没有找到名为 `RootBorder` 的部件，找到即让路（pass 3 Q + 像素侧不含框架硬编码底色）。
推论：**以后遇到"这个原生控件不吃模板"，第一件要量的事是它有没有开这个开关**，
而不是直接接受"它只能自绘"——`01-jalium-control-census.md` 里那批"自绘"结论因此要重读一遍（任务 #8）。

**2 · `Expander` 的部件名是功能契约，且头部件不能是可点击控件。**
控件在 `OnApplyTemplate` 里按名字取三样：`PART_HeaderBorder` 上 `AddHandler(MouseDownEvent, …)`、
`PART_ContentBorder` 的 `Visibility` 由它写、`PART_Chevron` 必须是 `Shapes.Path`（它按 `Path` 强转，
放 `TextBlock` 字形会静默失效），旋转角固定 **+90°**、不随 `ExpandDirection` 变。
改名模板照样构建、照样布局、照样上色，只是永不展开——正反两向都由测试钉住。
因为点击处理器就挂在 `PART_HeaderBorder` 这个元素上，头**不能**再放 `ToggleButton`：
一次点击会被框架与自己各答一次（与阶段 2 的 `SplitButton.Command` 双执行同一形状）。
焦点与键盘（Space/Enter）由控件自身承担，所以焦点环属于 `Expander` 而非头部件。

**3 · 带模板之后 `IsOpen=false` 不收起任何东西。**
原生 InfoBar 只在自己的度量里响应 `IsOpen`；有模板后控件仍按 `MinHeight` 占位、照样可见（pass 3 Q3）。
上游靠 `InfoBarCollapsed` 状态写 `ContentRoot`，因此这格必须由我们的模板自己写，并有断言把关。

**4 · `UIElement.RaiseEvent` 是 public，`MouseButtonEventArgs` 无需输入设备即可构造。**
签名 `(MouseDevice, int, MouseButton)`，配合 `Mouse.PrimaryDevice` 就能合成一个左键 `MouseDown`，
走的是控件真装的那个处理器（比自动化 Invoke 模式更接近真实点击）。
这让"禁用不响应""一次点击恰好翻一次"这类判断成为可断言的行为测试；但它**不是硬件输入**——
指针没动过，hover/press 像素与触摸路径仍欠（任务 #13）。

**5 · "样式格子里点的属性名根本不存在"由闸口抓出，而不是靠人眼。**
`AstraGateTests.Style_setters_name_properties_the_controls_actually_have` 会解析每格点名的属性，
它直接拒绝 `Expander.IsPressed`：`Expander` 没有这个属性，那一格永远不会触发。
结果是上游 5 条 `*Pressed*` 行本次不发布（其中 4 条与静置行别名同一实例，本来也无像素差）——
承诺一个读不到的键，比推迟它更糟。

**6 · 框架自绘路线另有 9 条键名。**
`InfoBarInformational/Success/Warning/ErrorBackground`、`InfoBarInfo/Success/Warning/ErrorBrush`、
`InfoBarForeground` 在框架自带主题里确实存在并可被 `TryFindResource` 命中（pass 2 O 段逐条读出），
各自另有硬编码回退色（Informational 底色是 `#2D2D37`）。走模板路线后本批**不发布**这 9 条：
没有东西再读它们，发布等于承诺"覆盖能到像素"却无人消费。

## S0-n：能不能重模板是分类型的，控件还会自己量自己画（菜单族批，2026-09-19）

`spike/MenuProbe` 六遍、`menu-probe6.txt` 用**出厂主题**（`Styles/Menus.jalxaml` 与两本新字典本身）复测。

**1 · 同一个运行时里，一半菜单能重模板、另一半不能。**
建窗之前装好隐式样式之后：`Menu`、`ContextMenu`、`MenuFlyoutItem`、`ToggleMenuFlyoutItem`、
`MenuFlyoutSubItem`、`MenuFlyoutSeparator`、`MenuBarItem` 全部实例化我们的模板，且解析出的画刷
与调色板对象**同一实例**（用 `ReferenceEquals` 认定，而不是比颜色）；
`MenuItem` 存下 `Template` 却**永不实例化**——它自绘（`OnRender` + `ResolveBackgroundBrush` /
`ResolveMenuBrush` + `DrawCheckMark` + `DrawSubmenuArrow`）。所以 `MenuItem` 只能拿一条只上色的样式，
高亮/勾选/箭头三处仍是框架色。`MenuBar` 无模板可装（它的项由框架自己宿主），同样只上色。
> 结论用法：**不要按"基类"外推能否重模板**，`MenuItem` 与 `MenuFlyoutItem` 是同一族里的两个世界。

**2 · pass 4 的一条结论被 pass 6 更正。**
pass 4 在只装哨兵的窗口里看到"点击后框架把 `Background` 写成本地值 `#FF3A3A3C`"；装上出厂主题后复测，
合成 MouseDown 使 `IsPressed=True` 而 `Background` **仍是我们的行实例、无本地值**。
即：自绘控件在样式到位时不会被本地值盖掉，先前那条"框架本地值危险"不成立于本批配置。

**3 · `MenuFlyoutItem` 自己度量、还顺便自绘。**
`MeasureOverride` 用文本宽度算宽、用一个常量封顶高（26.10.9 实测 34），并且 `OnRender` 仍画背景。
后果有两条：上游 `11,8,11,9` padding + `4,2,4,2` margin + 17 高的标签在 34 的盒子里被压成 13（文字被挤扁），
因此条目样式里带一条 **38 的 `MinHeight` 字面量**（4+17+17，正好是上游那些行应有的几何）；
以及真指针 hover 下框架自绘层是否盖过我们的格子**未知且无法在无指针下证明**（任务 #13）。
`MinHeight=32` 放在条目上会挤压、放在表面上无事，因此它只属于 `ContextMenu`（presenter 角色）。

**4 · 菜单族完全不读模板部件名。**
`GetTemplateChild` / `PART_` 在整族源码里零命中，子菜单弹窗由控件 `new Popup` 自建。
于是 `LayoutRoot` / `IconContent` / `CheckGlyph` / `SubItemChevron` 这些名字是上游对齐，
**不是**功能契约——这与 Expander 的三个名字（改名即失效）相反，两件事都不许互相外推。

**5 · 状态可达性再收窄：CLR getter 不是 DP。**
`MenuFlyoutItem.IsHighlighted`、`MenuFlyoutSubItem.IsSubMenuOpen` 只有 CLR getter，
`SetValue` 路都没有（属性根本不是 DP），因此上游 11 条 `*Pressed*`、2 条 `*SubMenuOpened*` 无处可挂，本次不发布。
可挂的三格是 `IsMouseOver`、`IsEnabled=False`、`IsChecked`（Toggle），全部有断言。
`MenuBarItem` 自有 DP 只有 `Title`，上游第四个状态字面就叫 `Selected` 而无对应属性，四条 pressed/selected 行同样不发布。

**6 · 展开类动作在进程内可调用但不可观察。**
`MenuFlyout.ShowAt/Hide` 真开真关（`IsOpen` 可读）；`ContextMenu.Open(Point)` 真开（`IsOpen=True`），
但弹层外面那层 `MenuPopupScrollHost` Border 是框架硬编码色，我们的 `MenuFlyoutPresenter*` 两行
只对 `ContextMenu` 自绘的那层表面兑现承诺；`ContextMenu.MinWidth=140` 不被宿主尊重（实测 62.86 宽）。
`MenuFlyoutSubItem.OpenSubMenuAndFocusFirstItem` / `EnsureSubPopup` / `FocusFirstSubMenuItem`、
`MenuBarItem.OpenFromKeyboard` / `OpenMenuAndFocusFirstItem` / `FocusFirstMenuItem` 都能调用且抛不出东西，
但 `IsSubMenuOpen` 依旧 false——子菜单是 mouse-enter 驱动的，没指针就没结论。

## S0-o：行遮蔽可以是主方法，但临时模板量不出状态（命令栏批，2026-09-19）

`spike/AppBarProbe` 五遍，`appbar-probe1..5.txt` 全留。审计：`audits/app-bar.md`。

**1 · 这是第一批"现代 WPF 方法（改行名）本身就是主杠杆"的控件。**
26.10.9 自己带 `AppBarButton` / `AppBarToggleButton` / `AppBarSeparator` 的隐式样式，并且用**它自己的 9 条行**
画 `CommandBar`——其中 `AppBarButtonForeground` 是强调派生的紫 `#680081`。两条通道都实测到位（pass 3，
按 `ReferenceEquals`）：我们后合并的同名行**赢下遮蔽**（只对合并之后建造的控件），我们的同 `TargetType`
隐式样式**直接替掉**框架那份。所以这一族的颜色不依赖重模板，模板只为把几何从宿主的 44 宽/10px 标签/20 盒
拿回上游的 68/12/16。
> 结论用法：宿主已经用行名上色时，先量遮蔽，再决定要不要出模板——反过来说，只有行名不够（宿主几何不对）。

**2 · `XamlReader.Parse` 出来的临时模板，`ControlTemplate.Triggers` 一律不生效。**
pass 4 用探针模板量格子，四种写法（字面值部件 / `TemplateBinding` 部件 / 单条件 / `MultiTrigger`）
**全部读回静置色**；同一个状态在已编译的 `.jalxaml` 样式里都能读回（pass 5 段 A、本批行为用例）。
因此 pass 4 的两条结论当场作废：① "合成 MouseUp 不清 `IsPressed`"——真模板上实测清（pass 5，已进断言）；
② `LabelPosition` 的"接受 Right/Hidden 但无事发生"——真枚举只有 `Default`/`Collapsed`（见 5）。
**同一件事也使 pass 4 的 `MultiTrigger` 读数不可用**：本批不发布 9 条 checked 变体行，理由换成属性面证据
（一格一个条件、无 overflow 标志），不再引用那条。
> 结论用法：探针模板可以量**结构、类型面、像素**；量**状态**必须用编译字典，否则会得到"格子全灭"的假阴性。

**3 · S0-d 的 VisualStateManager 结论补强成两条独立证据。**
`Jalium.UI.Controls.VisualStateManager` 类型**在全部已加载程序集里不存在**；带
`VisualStateManager.VisualStateGroups` 的模板装进已上屏窗口实例化时抛
`XamlParseException: Property 'VisualStateGroups' not found on type 'Grid'`——而**同一份 markup 单独
`XamlReader.Parse` 返回 ok**（模板惰性解析，pass 5 段 E）。
> 结论用法："解析没抛"永远不等于"能用"；能力判据只能来自实例化上屏之后的读数。

**4 · `IsMouseOver` 写不进去，但路由按下会自己把它置真，于是 hover 行能落到像素。**
`UIElement.IsMouseOverPropertyKey` 既非 public 也不存在（pass 4/5 都是 `MissingFieldException`）。
但一次 `RaiseEvent(MouseDown)` + `RaiseEvent(MouseUp)` 之后 `IsMouseOver=True`，根 `Border` 读到
`#09000000`＝hover 行，而静置挂载读到 `#00FFFFFF`、按下读到 `#06000000`（三条都是可区分读数）。
S0-n/菜单批那条"hover 只能断行名不同"因此**不适用于本族**：这里能断进行名对应的那层表面。
仍欠的是"指针移动进/出"这条循环（任务 #13），不是这一格。

**5 · 上游与本机在 label 枚举上是同一套，缺的是落点。**
`CommandBarLabelPosition` 上游只有 `Default`/`Collapsed`（`controls2.idl:100`），本机一字不差；
`CommandBarDefaultLabelPosition` 上游 `Bottom`/`Right`/`Collapsed`（:83），本机也一字不差。
并排标签无处可挂的原因不是枚举缺值，而是它挂在 `CommandBar` 上而 bar **存下 `Template` 却从不实例化**
（0.4，`MenuItem` 之后的第二例）。两个成员单独都不动标签、不动 padding（`LabelPosition_names_two_states_and_moves_neither`）。

**6 · "框架没有模板"不等于"框架不画"。**
`AppBarSeparator` 没有出厂模板（`OnRender` + `ResolveSeparatorBrush` 自绘），但会实例化我们给的那份——
于是同一个像素有两条可能通路，我们的行是捕获读到的那条，另一条只能在真机上看（Known Gap 4）。

**7 · 共享宿主窗口上的"整窗哨兵计数"单独不可判，只能做差分。**
`The_open_bar_shows_its_overflow_outside_the_surface_this_capture_can_reach` 的第一版断言
"覆盖 acrylic 令牌后，开放 bar 的整窗捕获里 0 个哨兵像素"——**同类内跑通过、全量套件里以 9088 个哨兵像素失败**，
因为宿主窗口在类之间共享，别的类留开的 acrylic 弹层自己就把画面染绿。可用形态只有两种：
① **同一覆盖下的两帧做差**（别的读者对两帧贡献相同，差值只剩被测主体的贡献）；② 读控件自己的状态对象
（本例里是 `Popup.IsOpen`，它证明的是"那一面根本没显示"，比"显示了但不跟令牌"更窄也更硬）。
> 结论用法：任何"没有像素动"的主张都必须自带差分或状态读数；单帧绝对计数在共享窗口里既不能证真也不能证伪。
>
> 边界（视觉缺陷批补）：这条只对**整窗捕获**（`PixelHarness.Host` / `Chrome`）成立。
> `PixelHarness.Render` 走的是控件自己的裁剪，别的类留在宿主里的弹层进不了那帧——
> "某条裁剪断言类内通过、全量失败"因此**不是**共享窗口的证据。本批真遇到一次这样的读数，
> 归因是过期二进制（见 `06` 的 `--no-build` 一节），不是污染。

## S0-p：重影的第二位画者是基础类的 `OnRender`，开关是部件名（视觉缺陷批，2026-09-19）

来源：`spike/InfoBarGhostProbe`（pass 1 反射签名与像素归因、pass 2 IL `ldstr`、pass 3 改名前后对照、
pass 4 全量自绘普查）、`spike/TextWrapProbe`（pass 1 度量与排布）、`spike/VisualQA/capture-pages.ps1`
逐页截图。运行时 26.10.9。触发点是用户实测反馈：很多控件有重影，间距也不合 Fluent。

**1 · 「改了模板没反应」和「模板生效了却被画两遍」是两件事。** 前面各批把原生控件的自绘读成「模板无效」
（S0-h、S0-m、S0-n）。`InfoBar` 这一例把另一面摊开了：模板成树了、上色了、部件都在，基础类的 `OnRender`
却一行没让，于是同一个标题在屏幕上出现两次、错位十几像素。截图里那不是残影，是两位画者。判据因此要拆成两条：
成树只证明模板通道通，不证明自绘层已退出。

**2 · 让路条件是一个部件名，而这个名字可以用反射从 IL 里读出来。** `InfoBar.OnApplyTemplate` 的 `ldstr`
字面量只有两条：`RootBorder` 与 `PART_CloseButton`。前者是 `OnRender` 的让路开关（基础类把它存进私有
`_rootBorder`），后者是 `CloseButtonClick` 的接线点。上游 `InfoBar.xaml:15` 的根叫 `ContentRoot`，
所以照抄上游命名的模板永远拿不到让路。本库模板根因此改用运行时的名字，偏离记在 `audits/infobar.md` 第 6 段。
读法：`GetMethodBody().GetILAsByteArray()` 扫 `0x72` 操作码，`Module.ResolveString(token)` 解出字面量。
不需要私有反射、不改运行时行为，比试名字便宜，也比照抄上游诚实。同一读法一次给出了 `Expander`
（`PART_HeaderBorder`/`PART_ContentBorder`/`PART_Chevron`）、`SplitButton`（`PrimaryButton`/`SecondaryButton`）、
`NumberBox`、`Slider`、`ComboBox`、`AutoCompleteBox`、`NavigationView` 各自的部件契约。
顺带纠正一次误判：先前用 ASCII 与 UTF-16 两种字节扫描都读不到 `RootBorder`，据此怀疑过这条契约是编的——
`ldstr` 字面量存在 `#US` 堆里，按文件偶数偏移重排字节会整体错位，扫不到不等于不存在。IL 解码才是这条的正确量法。

**3 · 像素判据用「与把 `OnRender` 覆写成空的同类逐像素相同」。** 只数颜色会误判：改名前后样式条都有蓝，
因为我们自己的图标也是蓝。真正强的对照需要一个把基础类那一层彻底关掉的同类——`OnRender` 是 `InfoBar`
自己声明的 `protected virtual`（`ContentControl`/`Control`/`UIElement` 都没声明），所以一行
`protected override void OnRender(DrawingContext) { }` 就够。改名前：样式条 14 色、框架蓝 178 px；
关掉那一层：7 色、0 px。改名后：两者都是 7 色、936 ink、top 色逐条相同。这条同时封掉「我们少画了」
和「基础类多画了」两个方向，比「某色不该出现」强。

**4 · 原生 `InfoBar.MeasureOverride` 只算它自己那套字面布局。** 300 DIP 宽、消息两行的条，内部
`RootBorder` 要 107.1，控件对外只报 69.8；同一控件在宿主给固定高度时又被排成 200。所以断言必须打在
`DesiredSize` 上，并且放在竖直 `StackPanel` 里——页面就是这种形状：给无限高度、取控件要的。
撤掉 `FluentInfoBar.MeasureOverride` 里取较大者那句，这条断言立刻失败；装回去通过（反向验证已做）。
同一缺口的第二个症状更隐蔽：先构造、后设 `Message` 的条，`Title` 的 `ActualHeight` 是 0。

**5 · 自带 `OnRender` 的控件远不止 `InfoBar`。** 26.10.9 的 `Jalium.UI.Controls` 有 93 个导出类型声明了
自己的 `OnRender`（pass 4 全表）。其中本库重模板或重上色的：`HyperlinkButton`、`TextBox`、`PasswordBox`、
`NumberBox`、`AutoCompleteBox`、`Slider`、`Menu`、`MenuItem`、`MenuBar`、`MenuFlyoutPresenter`、
`MenuFlyoutItem`、`MenuFlyoutSubItem`、`MenuFlyoutSeparator`、`ToggleMenuFlyoutItem`、`CommandBar`、
`AppBarSeparator`、`ScrollViewer`、`TabControl`、`TabItem`、`InfoBar`。这份是**候选清单，不是缺陷清单**：
只有 `InfoBar` 一例按第 3 条量过。`Button`/`ToggleButton`/`CheckBox`/`RadioButton`/`ComboBox`/`Expander`/
`AppBarButton` 不在名单里（不声明 `OnRender`，绘制全在模板内）。

**6 · 补模板会连带换掉条目面板。** `Menu` 的模板只有一个 `ItemsPresenter`、没有 `ItemsPanel` 设定，
顶栏条目于是从框架默认的横排退回 `ItemsControl` 的竖排，`File/Edit/View/Help` 竖成一列
（`spike/VisualQA/out/menus.png`）。补一条 `ItemsPanel` = 横向 `StackPanel` 即恢复，并由
`AstraMenuTests.The_menu_lays_its_top_level_items_out_in_a_row` 钉住（撤掉 setter 即失败）。
这与 S0-i 同源：模板替换的不只是外观，还包括模板没写的那部分默认行为。

**7 · 换行文本的度量宽度与排布宽度不一致，会让相邻元素叠在一起。** Gallery 页脚的 parity 行与
「Not claimed」行重叠：前者按更宽的尺寸量成一行、按实际宽度排成两行，后一个兄弟就被放在它溢出的第二行上。
同样的容器形状在探针里（文本先设好、宽 600）不复现，在页面上稳定复现，因此根因尚未定位。本轮把页脚改成
「单个 `TextBlock` + 固定高度滚动宿主」，让这条声明不再依赖那次度量（`MainWindow.jalxaml`、
`MainWindow.jalxaml.cs` 各留原因注释）。**这是绕开，不是治好**：框架侧的度量/排布宽度差仍在，
`TextWrapProbe` case E（先布局、后设文本）里甚至连重测都没发生（`DesiredSize` 停在 0x0）。

## S0-q：控件的画者读属性、不读我们的行——重模板的边界是"谁画"，不是"谁命名部件"（flyout 重影批，2026-09-19）

用户报"点开 flyout 里面又有重影"。这一批量到的不是新缺陷，而是 S0-p 那条结论的另一半：

**1 · "没有部件名开关"不等于"不画"。** `spike/FlyoutGhostProbe` 解 `OnRender` 的 IL 字面量，四个 flyout
类型全在自绘内容：`MenuFlyoutItem` = `OneSurfaceHover, MenuFlyoutItemBackgroundHover, OneTextDisabled,
TextDisabled, Segoe MDL2 Assets, OneTextSecondary, TextSecondary`；`ToggleMenuFlyoutItem` = `✓`；
`MenuFlyoutSubItem` = `OneTextSecondary, TextSecondary`（且 `sealed`，连"空重写双胞胎"这条路都堵死）；
`MenuFlyoutSeparator` = `MenuFlyoutPresenterBorderBrush`。我们的模板又摆了一份标签、加速键、勾选、箭头、
分隔线，于是每行两层。既然读不出开关，能做的只有**不重复画**。

**2 · 判据只有合成窗口这一条。** 进程内 `RenderTargetBitmap` 裁剪读不出这两层谁画的（同一张裁剪图里
shipped 与"空 OnRender 双胞胎"的亮像素计数一模一样），必须 `PrintWindow` + `PW_RENDERFULLCONTENT` 抓
`PopupWindow` 自己的顶层窗口。为排除"这是交换链伪影"的反解释，同一 pass 连抓两次逐像素比对：**差异 0 px**，
重影是确定性的，不是抓帧时机。

**3 · 控件的画者读依赖属性，这一点让标签态活了下来。** tint pass 给第 1 行与切换行本地写
`Foreground=#FF00FF`、`FontSize=20`，抓回来的弹窗里那两行标签就是品红、就是 20 号。所以标签的
hover/disabled 变色不必放弃：从模板格子搬到 `Style.Triggers` 写 `Foreground` 即可，`MenuFlyout*Foreground*`
行继续有消费点。

**4 · 但画者不读我们发布的行。** 覆盖 `TextFillColorSecondaryBrush` 之后，加速键文案、勾选标记、子项箭头、
分隔线的颜色纹丝不动（控件自绘的灰 `#6E6E73` 仍在原位）——与 S0-n 那条"33 个哨兵键装入后菜单像素不动"
同构。结论：那 11 条上游行（加速键 3×2、chevron 3、`MenuFlyoutItemChevronMargin`、
`MenuFlyoutSeparatorBackground`）**撤回发布**而不是留着当装饰，并把"这几处颜色拿不到"写进审计的 Known Gaps。

**5 · 撤内容这一步先在探针里试，别改产品。** 本地 `Template` 值胜过样式 setter，所以 `stripped`（只留表面
+ 图标槽）与 `bare`（只留表面）两个 pass 用探针里的内联字典就能装出来，一次运行同时拿到"控件到底画了哪些
格子、几何能不能用"的答案，再动 `Styles/Menus.jalxaml`。省掉一整轮"改产品→重建→重抓"的循环。

**6 · `MenuFlyoutItem.Icon` 的类型是 `System.Object`。** 控件不画图标（`bare` 的图标区是空的），所以图标槽
必须留在我们的模板里，`ContentPresenter` 正是承载任意内容对象的部件。探针早期用反射往 `Icon` 塞了一个
`IconSource` 实例，图标位于是打出 `System.Object` 字样——那是探针的错，不是模板的错，记下来免得下次把它
当成"图标渲染缺陷"。

**7 · 数值。** 修前弹窗合成图里属于我们那层标签的 `#FFFFFF` 有 1194 px，压在框架自己的 `#F5F5F7` 1670 px 上；
修后 shipped / stripped / bare 三个 pass 的 `#FFFFFF` 全为 **0**，框架层逐色计数（`#F5F5F7x1659`、
`#D6D6D7x556`、`#242424x737`）与参照 pass 完全一致——即"只剩一位画者，且它画的东西没被改动"。

## S0-r：撤回——"嵌套层的笔刷过渡进不了合成帧"是采集器读到的空帧（间距批，2026-09-19；同日更正）

结论先行：**这条边界不成立**。命令栏高亮层的 83ms 笔刷过渡已按上游恢复。下面是原始读数、推翻它的
两个实验，以及真正值得记下来的那条底座事实。

### 原始读数（间距批当天记下的，保留以免被后来者再踩一遍）

触发点：把命令栏条目改成上游的三层形状（`Root` 无填充 / `AppBarButtonInnerBorder` 高亮 / `ContentRoot`
管高度）之后，Gallery 里那个默认勾选的 `AppBarToggleButton` 看起来只剩黑字没有蓝底。

**1 · 属性侧是对的。** 离屏 harness 里 `((Border)Part(toggle,"AppBarButtonInnerBorder")).Background`
等于被哨兵改写后的 `AccentFillColorDefaultBrush`；格子命中、目标名解析、行取值都没问题。

**2 · 离屏栅格也是对的。** 三条通路各自断言 `Count(SentinelMagenta) > 1000` 全绿：裸控件 `Render`、栏内
`Render`、栏内**先上屏后置 `IsChecked`** 再 `Chrome`。

**3 · 当时那次真窗口捕获读到 0。** `spike/VisualQA/capture-pages.ps1` 里 Bold 格子 100x150 区域
`#60CDFF` 计数 **0**；把那条嵌套 `Border` 的 `TransitionProperty` 删掉重建，同一坐标变成 **7786/9108 px**。
当时据此写下"带笔刷过渡的嵌套层，其终值不落进合成帧"。

### 推翻它的两个实验

**4 · 17 格判别矩阵（`spike/TransitionProbe`，一次运行、三次抓帧、逐格唯一色、PrintWindow 客户端区）。**
每格只比相邻格差一个变量：模板根 vs 嵌套、有过渡 vs 无、83ms vs 0、加载时写 vs 上屏后写、
`{ThemeResource}` vs 字面、TemplateBinding 与格子双写同一个属性、裸视觉树 vs 模板内、笔刷过渡 vs
`Width` 过渡、属性列表 `"Background, BorderBrush"`（带空格/不带）以及**命令栏三层形状的完整复刻**。
17 格在三次抓帧里全部落帧，数值逐次一模一样：90x90 DIP 格 = 24649 px（157.5² 扣掉边缘），
`Width` 过渡格从 30 写到 90 也拿到 24649 px，命令栏形状格 18968 px（78x86 内缩矩形）。
所以"嵌套 + 过渡"、"两个属性名的列表"、"列表里那个空格"、"`{ThemeResource}` 笔刷"、"TemplateBinding
与格子同时喂同一个属性"——没有一个能挡住终值进帧。

**5 · 原现场的 A/B 重做，这次带空帧闸口（`spike/VisualQA/grab-page.ps1` + `count-colors.ps1`）。**
同一个 Gallery 页 `--page command-bar`，只改 `AppBar.jalxaml` 的那一个属性，两遍都等到"帧里确实画了东西"
（非黑像素 >1000）再计数：

| 构建 | `#60CDFF` | 栏底 `#323232` | 抓到可用帧前重试次数 |
|---|---|---|---|
| 无过渡（间距批提交态） | 8832 px | 958227 px | 15 |
| 有过渡（上游 83ms 恢复后） | 8832 px | 958227 px | 7 |

**逐像素相同。**过渡不改帧，改的是那次读数的可信度。

### 真正要记住的底座事实

- **PrintWindow 能在窗口已经摆好、也在出帧的情况下返回没画过的帧。** 上面两行"重试次数"就是它的表现：
  15 次和 7 次抓到的都是没内容或几乎没内容的帧。空帧会让"某个颜色 0 px"和"这个控件没画"**完全同形**——
  这才是当初那条结论的来源。任何外部抓帧必须先过"这帧画了没有"的闸口（`grab-page.ps1` 用非黑像素数），
  再谈颜色计数；`capture-pages.ps1` 只有 `WaitForInputIdle` 这道闸，而它超时时是 `continue`（连文件都不写），
  报的"never went idle"其实说的是采集失败，不是页面在动。
- **仓内 harness 三条通路是同一台栅格器。** `PixelHarness.Render`、`Host`、`Chrome` 都走
  `RenderTargetBitmap.Render`——包括 `Chrome(窗口)`，它并不是屏幕。过渡直接把终值写进依赖属性
  （见 adaptation/12），所以栅格器**永远**看得到终值，也就**永远不可能**发现"帧没跟上"。凡是要问合成帧，
  只能出进程抓。
- **格子只在生产形状下生效。** 矩阵第一版把九个模板放进字典里当 keyed `ControlTemplate`，属性读回全是静止色——
  这就是已记录的 keyed 模板 `Trigger.Property` 缺陷（audits/slider.md），一度被误读成"过渡又没落帧"。
  矩阵因此改成 keyed `Style` + 内联模板，与生产文件同形。**属性读回和像素读回必须成对出现**，否则两个
  方向的假阳性都抓不住。
- 任务"过渡终值落帧审计"（原 #22）随之关闭：19 处 `TransitionProperty` 不需要逐点重测，因为被怀疑的
  机制本身不存在；这些站点仍然只有属性/栅格证据，那是另一件没做完的事，不是这里的一条边界。

## S0-s：控件自己量出来的高度只有 `Height` 够得着，弹层窗口会被宿主窗口裁掉（间距全量复核批，2026-09-19）

**1 · 分隔线：样式改得动属性，改不动盒子。** `MenuFlyoutSeparator` 在本运行时量到 **9 DIP 高**，上游是 **3**
（1 高的线 + 它自己 `-4,1,-4,1` 的 padding）。先按上游补 `Margin=0`（框架给这个类型的是 `0,4,0,4`）——属性读回
确实变成 `0,0,0,0`，`actual` 仍然是 9。换成 `Height=3` 才落到 3。这与 S0-n 那条"`MenuFlyoutItem` 自己度量
并且自己画"是同一件事的另一半：**度量走代码的那一侧，样式里只有直接约束尺寸的属性够得着**，padding / margin
只是给它让路。两侧都有读回：属性侧是新用例 `The_flyout_rows_measure_upstreams_heights`（条目 38 / `LayoutRoot`
34 / 分隔线 3 / 线盒 1 且 margin `-4,1,-4,1`），像素侧是弹层自己那张 PrintWindow（`spike/VisualQA/out/`
`flyout-400x356-168.png`，线仍在 y≈122.3..124.6 DIP 横穿整行——把行压到 3 没有把框架画的那根线挤出去）。

**2 · 弹层窗口的尺寸受宿主窗口剩余空间限制。** 为了让采集器"优先抓到弹窗"，把探针宿主窗口从 760x620 改成
240x140，flyout 的顶层窗口跟着从 400x356 px 缩到 400x219 px，也就是只剩 125 DIP、三行——读起来像"条目丢了"，
实际是被裁。**"把宿主缩小好让弹窗最大"是个陷阱。** 正确做法是宿主保持够高，改用
`spike/VisualQA/capture-pid-windows.ps1`：它把该进程的**每个**顶层窗口分别落盘（文件名带 WxH 与 dpi），
弹窗和宿主各一张，不用猜哪个最大。

**3 · 弹层根节点在进程内栅格化是全黑。** `RenderTargetBitmap.Render(popupRoot)` 量到 219x199 的纯黑位图
（`flyout-shipped.bmp` 逐行扫过：0 个非黑像素）。这是 S0-j 那条不对称的同一族，这次落在 MenuFlyout 的
presenter 上——所以弹层里"线还在不在"这种问题只能出进程问，仓内 harness 会一致地回答"什么都没有"。

**4 · 读探针日志要在进程退出之后。** 一次 `--hold` 的运行还没写文件时，读到的是**上一遍**留下的那份，
据此得出的"Height=3 没起作用"是假的，下一遍才看见 3。凡是"改了就跑就读"的循环，先确认文件 mtime 晚于本次
启动，或者让进程自己打一行结束标记。

**5 · Gallery 的窗口约 17 s 才枚举得到。** `capture-all-pages.ps1` 第一遍 9 页里 command-bar 与 settings
两页十次 attempt 全空；把 settle 从 9 s 抬到 17 s 之后 9/9 命中，而且都是 attempt0。单独启动同一页 25 s 后
窗口句柄与标题都在，所以这不是页面缺陷，是采集器的等待预算不够——与第 5 条 S0-r 更正里的空帧闸口是两件
不同的事：一个是"窗口还没出现"，一个是"帧还没画"。

## S0-t：弹层表面的宽度只有 `MinWidth` 够得着，而够得着的那条绑定要看弹层被接到哪一棵树上（右侧空隙批，2026-09-19）

用户读数："下拉框或者说一些控件，它的右边是空了很大一块的，明显这个空隙不存在……它长度和左边的空隙是不对齐的。"
把它量成一个数：`spike/RightGapProbe` 把嫌疑控件按同一宽度摆在已上屏窗口上，进程外 PrintWindow 抓该进程的
**每个**顶层窗口，再按 DIP 逐行扫非背景像素。

**1 · 框架给 `Popup` 本身写了本地 `Width`，但用无穷大度量它的子节点。** 260 DIP 的 `AutoCompleteBox` 打开建议
列表，弹窗自己的顶层窗口是 **260x80 DIP**，画出来的卡片只到 **148 DIP**——右边 113 DIP 是纯 `#000000`，
左边贴到 0。这就是"右边多了一块、和左边的空隙不对齐"。`HorizontalAlignment=Stretch` 在无穷大量度下没有
意义（加上之后帧逐像素不变，`painted=19082` 两遍完全相同）；能碰到盒子的是 `MinWidth`。

**2 · 弹层子节点丢了 TemplatedParent 和祖先链，`ElementName` 还在。** 同一个属性、三种写法，读回是三件事：

| 写法 | 落在哪个元素 | 读回 |
| --- | --- | --- |
| `{TemplateBinding ActualWidth}` | ComboBox `PART_PopupBorder` | **260**（`localValue=UnsetValue`，即值来自绑定） |
| `{TemplateBinding ActualWidth}` / `{RelativeSource AncestorType=AutoCompleteBox\|Popup}` | AutoCompleteBox `SuggestionsContainer` | **0** |
| `{Binding ActualWidth, ElementName=OuterBorder}` | 同上 | **260** |
| 字面量 `MinWidth="260"` | 同上 | **260**（证明属性本身没被读丢掉，丢的是绑定） |

差别不在控件、在**弹层被接到哪一棵树**：ComboBox 的下拉被 graft 进宿主窗口的 overlay 层（探针从 `window`
往下走就能撞见 `PART_PopupBorder 260x117.3`），`AutoCompleteBox` 的建议列表有自己的顶层窗口，从宿主往下走
**根本找不到** `SuggestionsContainer`。所以"绑定不解析"和"读不到"是同一个根因的两种表现。

**3 · 两条通道可以给出相反的答案，这次是采集范围不同、不是其中一条在撒谎。** 属性侧（harness 里
`The_suggestions_surface_spans_the_box_instead_of_its_longest_row`）`ElementName` 版本读到 260；像素侧
（真窗口的 PrintWindow）仍然是 148。合理解释是：harness 的弹层走 overlay 那条路，绑定解析得了；真窗口的
独立弹窗顶层解析不了。**结论按控件分开记，不许合并成"已修"**——见 `audits/right-gap.md` 的 Known Gaps。

**4 · `ContentPresenter.TextWrapping` 是个死属性，而生成的 `TextBlock` 默认是 `Wrap`。** 先按直觉给
`PART_Label` 写 `TextWrapping="NoWrap"`，用例照旧失败：`Expected NoWrap, Actual Wrap`，行高从 36 涨到
**55.34 DIP**。这与 S0-n 那条"控件自己画"不同族——属性根本没进类型解析，和 `AstraPixelTests` 里
"the attribute is accepted by the reader and then goes nowhere"是同一条。够得着的写法是把隐式
`TextBlock` 样式放进 `ContentPresenter.Resources`（ToolTip 批为了反向的理由用的就是这条路）。改完
`A_long_pane_label_stays_on_one_line` 三项一起过：`NoWrap` / 行高 36 / 文本盒 <30。

顺带一条没修的：`NumberBox` 无 Header 时量到 **39 DIP**（`OuterBorder` 31 + `HeaderContentPresenter`
的 `0,0,0,8` 本地 margin），上游是 32——上游那个 presenter 默认 `Collapsed`、由代码在 Header 存在时才打开，
而 `NumberBox` 在这里是原生类型，样式里给不出"Header 非空"这种触发条件。记在 `audits/right-gap.md`。

## S0-u：`Auto` 滚动条只在右侧扣布局宽度，所以"左右边距不等"是基座问题；S0-t 的弹层那条读数作废（追批，2026-09-20）

**1 · 先更正 S0-t。** 那节里"真上屏的独立弹窗仍只画 148/260"用的是 `rightgap-*.png-455x140-168.png`。
这张图在 `closed` / `combo` / `suggest2` / `suggest3` / `suggest4` 五种模式下的 `painted` 计数与非黑扫描
范围**逐位相同**（10864 / 19082 / 2041278；0..148 DIP）——闭合的探针里不可能有弹层，所以那个 260x80 DIP
的可见顶层不是建议列表。**"枚举该进程的每个可见顶层"仍然分不清哪个是弹层**：要判"上屏对不对"，只能出
进程抓整屏（`spike/VisualQA/grab-screen.ps1`，`CopyFromScreen` + `SetProcessDpiAwarenessContext(-4)`），
它没有窗口身份问题，量到的就是显示器上的几何。

**2 · `ScrollViewer` 的 `Auto` 只在右边扣 12 DIP，这一条足以解释"左右边距不一样长"。** 整屏量 Gallery
`page-selection.png`：样卡离内容区**左 25.1 DIP、右 36.6 DIP**，而声明是 `Margin="24,8,24,24"`。
差的 11.5 DIP 就是 `AstraNavigationTests.The_bar_mode_decides_whether_the_pane_loses_layout_width`
钉住的那个槽位——**内容不溢出时不扣，溢出时右边窄一条**。WinUI 的滚动条是 overlay，不占布局，所以这条
差异不在任何控件的度量里，而在每一个会溢出的表面上：页面宿主、`ComboBox` 的 `PART_ScrollViewer`、
`AutoCompleteBox` 的 `PART_DropDownScrollViewer` 全中。同一页里样卡右边缘 1820、Live output 右边缘 1841
也是这 12 DIP 造成的两条线。改成 `Hidden` 后复测：左 25.1 / 右 24.6，skew 11.5 → −0.6 DIP，两条右边缘
并成一条。代价是可见滚动条没有了，这是替代不是等价。

**3 · 进程内开不出 `ComboBox` / `AutoCompleteBox` 的弹层。** `combo.Focus(); combo.IsDropDownOpen = true;`
读回 `True`，`AutoCompleteBox` 走 `Text` setter 也一样，但整屏截图里两个控件都是闭合的，
`PART_Popup` 的 `ActualHeight` 停在 32（控件自身的高度，不是下拉的高度）。`MenuFlyout` 是唯一能用
`ShowAt(anchor)` 真正开上屏的弹层——所以弹层宽度这类问题只有 `MenuFlyout` 能在离线探针里出像素证据。

**4 · flyout 一族量下来是对称的，别再往它身上找。** `MenuFlyout` 上屏逐行扫：无图标条目文本左
**16.0 DIP**，正好等于上游 `1(border) + 0(presenter padding) + 4(MenuFlyoutItemMargin) +
11(MenuFlyoutItemThemePadding)`；带勾选列的行 44.6 DIP，多出来的正是 `CheckGlyph` 的 `12 + 16` margin
（`MenuFlyout_themeresources.xaml:483-487` 的 `Auto/*/Auto`）。样式侧读回
`LayoutRoot 210.5x34 margin=4,2,4,2 padding=11,8,11,9 radius=4,4,4,4`。`MenuBar` 的下拉缩进 43 DIP
不是样式写的：26.10.9 的 `MenuItem` 存了 Template 却从不构建，缩进归框架。

**5 · `Trigger Property="Header" Value="{x:Null}"` 在本运行时是会触发的。** 这条之前记成"未验证"，因为它
是 S0-t 那批里唯一没做实验就按下的可能修法。做了：`NumberBox` 样式加一格把 `HeaderContentPresenter` 收成
`Collapsed`，无 Header 的盒子从 **39 → 32**（闸口用例与另一个进程的树读回各一遍），给回 Header 时 presenter
回 `Visible`、控件按"文本行高 + 8"长回去。要点是 **`Collapsed` 连本地 margin 一起请出布局**，而空 Content
不会——这正是"样式里给不出 Header 非空"那句判断的出处，也是它错在哪。上游不需要这一格：它的 presenter 是
`Collapsed` + `x:DeferLoadStrategy="Lazy"`，由代码在 Header 存在时才建。

**6 · "上游不写 TextWrapping"和"上游写了 TextWrapping"在这条基座上后果不同，必须分开处理。**
`ContentPresenter` 的 `TextWrapping` 是死格子（S0-t 已证），而这里生成的文本元素默认 **Wrap**。于是：
上游写了 `Wrap` 的（`CheckBox_themeresources.xaml:611`、`RadioButton_themeresources.xaml:378`）我们照抄
一个死格子，**后果恰好相同**——运行时默认就是 Wrap，不需要动；上游**没写**的
（`ComboBox_themeresources.xaml:764` 的 `ComboBoxItem` presenter）默认值反而错了，长条目会换行撑高行，
必须显式补 `NoWrap`，且只能补在 `ContentPresenter.Resources` 的隐式 `TextBlock` 样式上。
`A_long_combo_item_stays_on_one_line` 钉住这一格。
顺带一条采集器纪律：这条用例一开始把下拉留在打开态，下一个走 overlay 的用例就读到了它的条目
（`ComboBoxItemBackground` 断成 PointerOver 色）。**开过 overlay 的用例必须在断言前关掉再落断言。**


## S0-v：路线图上标"自有"的控件，有三分之一运行时本来就有（全量类型清单，2026-09-20）

SplitButton 批那次探针已经当场否过一条假设（`ContentDialog`/`Expander`/`InfoBar`/`Menu` 全族/`CommandBar`
原生都有，见 `ROADMAP.md` 阶段 2 收尾那段），但那是**顺手量到的**，覆盖不全。这一节是把整张表补齐，
因为"只有证明的行为缺口才起自有类型"这句纪律要能执行，得先有一张完整的清单：运行时到底自带哪些类型？
凭记忆答会答错，所以从**已加载的程序集**里写出来，而不是从文档、也不是从旁边的源码树。
原始输出：`docs/astra/adaptation/s0v-runtime-type-inventory-raw.txt`（`spike/RightGapProbe --open types`，
3 157 行；`Jalium.UI.Managed` 3 014 个公开类型、`Jalium.UI.Xaml` 138、`Jalium.UI.Desktop` 2，
其中 `Jalium.UI.Controls.*` 命名空间 960 个）。

按名字逐个复核，本路线图标"(自有)"的那几格里**已经有**的：

| 图上写的 | 运行时 | 结论 |
| --- | --- | --- |
| `ContentDialog`(自有) | `Jalium.UI.Controls.ContentDialog` 在，`ContentDialogButton/Result/Placement/Closing/Closed/Opened` 全在 | **改判：原生重模板**。反射确认 `Template` 可写（declaredBy=Control）、`Title`/`TitleTemplate`、三个 `*ButtonText`、`IsPrimaryButtonEnabled`、三个 `*ButtonStyle`、`DefaultButton`、三个 `*ButtonCommand` |
| `ProgressBar` | 在 | 阶段 6 那格不用起类型 |
| `SymbolIcon` | 在 | 同上 |
| `GridView` / `DataGrid` / `TreeDataGrid` / `ListView` / `ListBox` / `TreeView` / `NavigationView` / `ToggleSwitch` / `TitleBar` | 都在 | 阶段 5 整批是补样式，不是补类型 |
| — | `CommandBarFlyout` 在 | 命令栏批当时按"没有"处理过，回头要复核 |

**确实没有**、因此自有类型站得住的：`TeachingTip`、`Card`/`InfoCard`、`InfoBadge`、`ProgressRing`、
`RatingControl`、`PipsPager`、`TabView`、`BreadcrumbBar`、`Divider`、`BitmapIcon`、`AutoSuggestBox`
（只有 `AutoCompleteBox`，本库已经用它做宿主替换）。

还有一条与弹层直接相关：**运行时没有 `FlyoutPresenter`，也没有 `MenuFlyoutPresenter` 这两个类型**。
所以"给 flyout 表面写一个样式"在这个基座上不可能——表面要么是我们自己在别的控件模板里画的那层
（`ContextMenu` 的 `LayoutRoot`、`PART_PopupBorder`、`SuggestionsContainer`），要么是框架自己画的
（`MenuBar` 的下拉、`MenuFlyout` 的宿主层）。S0-u·3、S0-n 那两条"弹层开不上屏 / 缩进归框架"都是这同一条
的下游表现。

## S0-w：圆角表面不裁剪子元素——圆角对不对，一半不在样式里（弹层圆角批，2026-09-20）

**1 · `Border` 会把自己的填充磨圆，但不会裁剪压在上面的子元素。** 三张已知颜色的图（`spike/RightGapProbe
--open corner`，原始输出 `docs/astra/adaptation/s0w-corner-probe-raw.txt`）：蓝底上放
`CornerRadius=12` 的白 Border，(0,0)、(2,2) 读到蓝、(4,4) 起读到白——**填充是圆的**；同样的白表面里塞一个
60x60 的红色子 Border，(0,0) 到 (30,30) 全读红，`Clip` 读回空——**子元素不裁**；半径 0 的对照组全读红，
排除采样器本身。闸口用例 `AstraFlyoutCornerTests.A_rounded_border_rounds_its_fill_but_never_clips_its_child`
在 harness 里读到同样六个像素，两条通路一致。

这条基座事实决定了"圆角不对"这句抱怨怎么查：**样式里的半径token赢不过一个铺到角的内容**。WinUI 靠
`Border` 裁剪兜住的东西，这里只能靠表面自己把行让开圆弧。于是逐个表面复核几何：
`ComboBox` 的 `PART_PopupBorder`(r=8) 里条目带 `Margin=5,2,5,2` + `ItemsPresenter` 的 0,4，行离边 5 DIP、
离顶 6 DIP，r=8 的弧在 x=5 处只吃掉 y≈1.1 DIP，**安全**；`MenuFlyout` 行同理（4,2,4,2 + r=4 的行）安全；
`Expander` 靠 `IsExpanded=True` 那格把头部的下圆角改方（S0-m 已落），安全；
`AutoCompleteBox` 的 `SuggestionsContainer`(r=8) padding 是 `0,2,0,2`、内层 `Margin=-1,0,-1,0`，
行左右**没有让开**。

**2 · 建议列表真正上屏的缺陷不是圆角，是品牌绿。** 量 `SuggestionsContainer` 的裁剪（harness `Chrome` +
新加的 `PixelAt` 单点读回）：修前直方图 `#F9F9F9x2020` 之后紧跟 `#1D733Cx450 #2B804Ax450 #1E743Dx420
#2A7F49x420 …`——一条**accent 绿的对角渐变**铺在弹层里，中点 (130,20) 读 `#247A43`；条目类型读回来是
`ComboBoxItem`，`Background` 是**框架写的本地值**（`local=LinearGradientBrush`，半径 3）。本地值排在
setter 和所有 trigger 之上，所以样式改不动它。能改的是我们的模板**不去绑定它**：`Selection.jalxaml` 里
`ComboBoxItem` 的 `LayoutRoot` 从 `{TemplateBinding Background}` 换成 `{ThemeResource ComboBoxItemBackground}`，
九个状态格子本来就在往 `LayoutRoot` 写 token，所以 ComboBox 自己的行一格都没变。
修后同一条测量：中点 `#F9F9F9`，直方图 `#F9F9F9x10012 #DDDDDDx302 #F6F6F6x254 #000000x42`，
**绿通道占优的像素为 0**，四个角仍读回未上色（圆角活着）。
`An_item_takes_our_text_row_but_keeps_the_frameworks_own_fill` 继续钉住"属性还归框架"，
新增的 `A_suggestion_row_pays_our_token_instead_of_the_frameworks_accent_gradient` 钉住"像素归我们"。

**3 · 度量工具的边界。** 单点像素读回只有 `RenderTargetBitmap` 这条路能给，而它按当前属性值重画，
结构上看不见"落后于属性的那一帧"（`gui-verification-without-pixels`）。所以本节的断言全部是
"属性/合成图"这一类（裁剪、颜色、几何），**没有一条声称上屏帧**；建议列表的真上屏宽度仍是 S0-u·3 记着的
未测项。

## S0-x：模态宿主自己写尺寸盒，格子只认元素不认列（ContentDialog 批，2026-09-19）

测量：`spike/RightGapProbe`（`dialog` + `shown`，日志 `right-gap-shown.txt`）与
`tests/FluentJalium.Tests/AstraContentDialogTests.cs`（37 条）。结论分三条，都是基座级：

**1 · "放进树里"不等于"打开"，量错状态会一次废掉整批断言。** 挂载未打开的 `ContentDialog` 是
`Visibility=Collapsed`、`0×0`。本批第一版全部读这棵树，于是同时得到四个假象：卡片尺寸盒"没有本地值"、
标题"没有样式"、像素"什么都没画"、按钮"点了没反应"。真打开（`ShowAsync()`）后同一批读数全部改写：
卡片带着本地 `MaxWidth`、按钮样式换得动、事件与结果齐全。`ShowAsync()` 对**已挂载**的实例同步抛
`InvalidOperationException("Popup-hosted ContentDialog must not already be attached to the visual tree.")`，
并把控件搬进窗口的 `ContentDialogOverlayHost`——所以对话框类的出口必须包含"真开一次"，
`PixelHarness.Place/Render` 这条路对它根本不通（本批改用 `Chrome` 就地裁剪）。

**2 · 模态宿主按部件名写本地值，模板的槽位要给两层。** 控件把自身 Min/Max 映射到名为 `PART_DialogCard`
的元素并用"宿主宽 − 48"封顶：宿主 886.3 → `card.MaxWidth` 本地值 838.2857…；应用写 `MaxWidth=548` →
本地值 548；`MinWidth=320` 同样直达。本地值在所有格子之上，所以上游 `ContentDialogMaxWidth`（548）那条
**默认**上限没法以字面量放在这个名字上。解法是拆两层：`PART_DialogCard` 只接名字与 24 DIP 外边距，
`DialogSurface` 承载上游尺寸盒与所有填充行——控件只认名字，写不到第二层。推广：**凡框架自己建过模板的
控件，重模板时都要预期它会按名字回写本地值**；判断方法是"读回 `ReadLocalValue`"，不是"看模板里写了什么"。

**3 · 格子的可达面测出来了：元素写得住、列写不住、`TemplateBinding` 写不住；不带 `TargetName` 能写模板父级。**
八种按钮文本组合的列读回全是 `*,8,*,8,*`：`Value="0"` 在格子里被存成 `Double(0)`、`Value="0*"` 是真
`GridLength`，两种形状都改不动 `ColumnDefinition.Width`。同一批格子里，元素上的 `Visibility`、`Grid.Column`、
`VerticalAlignment` 全部生效；按钮的 `Style` 因为是 `TemplateBinding` 写出的本地值而无效（`local=set` 不变）。
改写到父级属性（`<Setter Property="PrimaryButtonStyle">` 无 `TargetName`）就通：`buttonStyleChanged=True`、
`dialog.PrimaryButtonStyle` 即格子里那个实例、`DefaultButton=None` 后还原。另测：`UniformGrid` 折叠子元素后
**不回收**其格子（`35.9/34.8/35.4` → 折叠中间仍是 `35.9/35.4`），所以它不是"按可见项数均分"的替代品。
推广：上游用 VisualState 改列宽/改样式的那类状态，在本运行时要么改写成"写元素 + 写父级属性"，
要么就是真做不到——按 Known Gap 记，不要留一条永不生效的格子当交代。

**4 · 生成的文本元素选择性吃继承：字重进得去、字号进不去。** `PART_TitleHost`（`ContentControl`）上写
`FontSize=20 FontWeight=SemiBold`，生成的 `TextBlock` 读回 `weight=SemiBold size=14 localSize=UnsetValue`；
`ContentPresenter.Resources` 里的隐式 `TextBlock` 样式在此读回 `style=null`（与 `ComboBoxItem` 那条 NoWrap
不是同一作用域）。唯一量到的通路是**样式级 `TitleTemplate`** 里放一个自建的 `TextBlock`（自建本地值，
`size=20 weight=SemiBold localSize=20`）。代价：`Title` 传非字符串对象时需要应用自己给模板。
推广：文本属性要么落在我们建的那个元素上，要么就承认落不到——`ContentPresenter` 与生成元素之间
没有第三条路。

**5 · 圆角要靠每层内表面自带半径，`Border` 从不裁剪子元素——这条这次是像素断言抓到的，不是看出来的。**
`DialogSurface` 有 r=8，但卡片不会裁剪它的子树：任何填到卡片边缘的方形 `Border` 都会把弧覆盖成方角。
标题条 `TitleStrip`（`ContentDialogTopOverlay` = `#FFFFFF`，与卡片 `#F3F3F3` 不同色）直接把 (1,1)、(2,1)、
(1,2) 三格刷成自己的颜色；命令行 `PART_ButtonArea`（`Background={TemplateBinding Background}`，即卡片同色）
把 (1,h-2)、(2,h-2) 三格画成 `#F3F3F3`——在表面上完全看不见，只有弧外那格露出方形「耳」，所以**同色不等于
无罪**。解法不是裁剪（本运行时的 `Border` 做不到），是每层内表面自带对应半径：`8,8,0,0` 与 `0,0,8,8`。
推广：**给圆角表面做重模板时，逐角读回像素，且把"内表面与外表面同色"当成额外风险**——前一批 `MenuBar`
下拉、`AutoSuggestBox` 建议行的圆角缺陷都是同一形状，只是这次有断言守着。判据在
`AstraContentDialogTests.The_two_inner_surfaces_leave_the_cards_round_corners_alone`。

## S1-a：弹层表面的圆角是框架抄过去的本地值——控件不开口，它就留在自己的 14（菜单半径批，2026-09-20）

测量：`spike/FlyoutSurfaceProbe`（`tree` / `radius` / `sweep` / `context` / `context14`，原始输出
`docs/astra/adaptation/s1a-menu-surface-raw.txt`，两帧落在 `spike/FlyoutSurfaceProbe/out-*.png`——**它们不是被
`.gitignore` 挡住的**（`git check-ignore` 对这些路径空返回），只是没有 `git add`，所以逐行数抄进原始文件才是
传得下去的那一份）。判据：`tests/FluentJalium.Tests/AstraMenuTests.cs` 末尾六条。

**1 · `ContextMenu.Open` 会在我们模板外面再造一层 `Border`，并把控件的四个属性写成它的本地值。**
树形：`PopupRoot > Border(bg LOCAL, bb LOCAL, bt=1 LOCAL, radius LOCAL) > MenuPopupScrollHost > ScrollViewer > 条目`。
写的正是 `MenuFlyoutPresenterBackground` / `MenuFlyoutPresenterBorderBrush` / `BorderThemeThickness` /
`CornerRadius` 那几格——**上游 presenter 的这几行在这个运行时只经这一次复制才落到像素**。复制是活的：
开起来之后改控件的 `CornerRadius=3`，表面立刻读 3；清掉本地值，又回到样式给的 8。

**2 · 复制的条件是"控件被告知的值"，不是"任何有效值"——没被告知就留在框架自带的 14。**
同一条 `radius` 模式，改前改后各跑一次，只有两行自己变了：

| | 表面开起来读 | 清掉本地值后读 |
|---|---|---|
| 修前（样式没写半径） | **14,14,14,14 LOCAL** | 14,14,14,14 |
| 修后（`CornerRadius={ThemeResource OverlayCornerRadius}`） | **8,8,8,8 LOCAL** | 8,8,8,8 |

而"显式写 0"会把表面带到 0——所以 0 是一个真实请求，不是"未设置"的哨兵；真正未设置才吃框架的 14。
上游飞出的表面是 `OverlayCornerRadius`=8（`MenuFlyout_themeresources.xaml:285` + `CornerRadius_themeresources.xaml:6`），
于是缺陷读法确定为：**样式不写这一格，右键菜单就是 14 度角，且没有任何别的路径能改它**
（`MenuFlyout` / `FlyoutBase` / `Popup` / `PopupRoot` 整条链上都没有半径形状的属性，只有 `ContextMenu` 有）。
往 `Border` 自己身上写不是出路：清掉之后表面读 0，因为那层 `Border` 自己没有任何样式兜着。

**3 · `MenuFlyout` 的表面改不动，但也已经是对的。** `MenuFlyoutPresenter` 在 26.10.9 上是
`internal sealed : Control`，公开构造函数唯一签名 `(MenuFlyout)`，**自己没有声明任何依赖属性**，
按类型键与按名字键查隐式样式都读回 `null` → C# 里点名它编译期就报 CS0122，标记里也没有样式能吃住它。
但它带着框架自己写的 `radius=8,8,8,8 LOCAL` + `bt=1,1,1,1 LOCAL`，同时 `Background`/`BorderBrush` 是
`null`：**它不画我们那张卡片**。而且它的弹层是独立的 `PopupWindow`（父链
`... < MenuFlyoutPresenter < PopupRoot < PopupWindow`），不 graft 进宿主窗口，宿主树里一条也读不到。
所以"上游菜单卡片色"这一格归框架，我们给的 `MenuFlyout` 表面无色——记成缺口，不用相邻证据替代。

**4 · 反射挂一个手搓的 presenter 能量出"这些本地值是类型自己写的"。** 挂进我们树里的
`MenuFlyoutPresenter` 照样长出 `MenuPopupScrollHost`、`ScrollViewer`、条目（`MenuFlyoutItem` r=4、
`LayoutRoot` margin 4,2,4,2）、分隔线（`Border 238x1 margin=-4,1,-4,1`），我们的条目样式全吃到；
`style: null` 而 `radius/bt` 仍是 LOCAL → **它不是样式查找的结果**。这条只是量边界：产品代码不反射
框架私有成员（AGENTS.md），记录、不使用。

**5 · 弧的帧对照能证明"变大了/变小了"，不能当尺子；顶行那一格连自己都不稳。** 同一张 130 px（=74.27 DIP×1.75）
宽的卡片、同一条左边（x=350..479），只差半径：**弧深**在 8 那一档两次重采都是 9~10 行、14 那一档 17 行；控件
显式写 0 时弧消失。但"顶行咬进几 px"同一个 `context` 模式重采一次就从 12 变到 9（两帧弹窗落点差 0.67 DIP），
所以那一格不是论据，本批只取弧深这一档稳定的量。弧的绝对尺寸也不等于 半径×1.75：8 DIP 应是 14 px 弧、
量到 9~10，14 DIP 应是 24.5、量到 17——**两档都短同一个系数**，所以比值干净、绝对值不干净。结论按证据分级：
半径数值以读回为准（第 2 条），弧的形状以帧为准且只证变化。是渲染器把弧画短、还是"第一个非底色列"这条测法
吃掉几个像素，需要跨 DPI 或跨已知半径的对照才能分开，一台 175% 显示器给不了——留成上游开放问题，
它会挪动全项目每一个圆角，不只是菜单。

**6 · 共享测试宿主给不了 graft 弹层尺寸。** 同一个 `Open()`，探针自己 show 的窗口里表面 74.27x70，
xunit 共享宿主里 0×0，`PixelHarness.PixelAt` 直接抛"has no layout to sample"。所以那条角上的像素断言
改成钉复制契约（宿主读得到的那一层），弧留给真帧。**没有把测不到的东西伪装成测到**。
推广：以后凡是要在像素上断言弹层几何的，先量宿主能不能给尺寸，再决定判据放哪一层。
