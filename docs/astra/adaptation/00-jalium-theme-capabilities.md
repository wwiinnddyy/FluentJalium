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
