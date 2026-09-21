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

## S1-b：模板里的 `Popup` 有自己的顶层窗口——卡片读得到，它在你树上占的位置读不到（TeachingTip 批，2026-09-20）

测量：`spike/TeachingTipProbe`（modes `all` / `tail` / `place` / `tip` / `room` / `parent`，原始输出
`docs/astra/adaptation/s1b-teachingtip-host-raw.txt`）。判据：`tests/FluentJalium.Tests/AstraTeachingTipTests.cs`
**73 条**。上游：`controls/dev/TeachingTip/TeachingTip.xaml`（blob `50cc6d2d6cb7648df90e21faa0cc9e6aa816a9bd`）
与 `TeachingTip.cpp`；方法照 ModernWpf 的 `TeachingTip.cs:643-682` 那条自动朝向 + 回退序。

**1 · 自有类型 + 模板内 `Popup` 这条路走得通，但卡片不在宿主树里。** 一个调用
`ContentControl.UseTemplateContentManagement()`（S0-m 那把锁）的类型，能在它的 `ControlTemplate` 里放
`<Popup Name="PART_Popup">` 并让它在 `IsOpen=true` 后建出整棵卡片：`Container 320x168.12`、
`ContentRootGrid 304x152.12`、两颗按钮 `130x32.78`、`TailPolygon 9x21`。**但父链是
`Border < PopupRoot < PopupWindow`**——带 `PlacementTarget` 的弹层落在自己的顶层窗口里，不是宿主窗口的
`OverlayLayer`（不带 target 时才 graft 进 `OverlayLayer`，mode C 与 mode F 的差别就在这一处）。后果是判据必须换
查找起点：**名字要从 `popup.Child` 往下走**，从宿主窗口往下走一条也读不到，而按 `IsOpen` 收起的那一侧全部读
`0x0`——"部件在树里但没尺寸"是这条路的静息形状，不是失败。

**2 · `Placement=Relative` 的锚点是目标的左上角，偏移是加法 DIP。** 一张 120x50 的卡片对着 100x40 的目标：
偏移 `0,0` 读回 `-0.29,-0.29`、`50,50` 读回 `50,50`、`-120,-50` 读回 `-120.29,-50`（那 0.29 是描边，与弹层无关）。
四条边值各自把子元素推到 `y=目标高` / `y=-子高` / `x=-子宽` / `x=目标宽`，`Center` 居中。而
`PlacementMode` 全部 12 个值里**没有**上游那 18 个 per-edge / per-corner（`LeftEdgeTopEdge` 一族一个都不在），
所以 `FluentTeachingTipPlacementMode` 只有六个值，且"贴哪条边"完全由两个偏移算术决定。这不是偷懒，是基座不给更多把手。

**3 · 行类型清单，外加一条对 S0-b 的更正。** `GridLength` 解析成功（`8` 与 `*` 都读回原值），
`Thickness` / `CornerRadius` / `SolidColorBrush` / `StaticResource` 别名 / `FontFamily` 同样能过；
`x:Double` 与 `x:Int32` **不是被丢掉，是整份字典炸掉**（`XamlParseException: Cannot resolve type 'Double'`）——
S0-b 那句"x:Double 进不去"读起来像一行一个结果，实际是一行毁一份，这个区别决定了转录清单为什么必须逐行试。
另一半：**`ColumnDefinition.Width` 上写 `{ThemeResource}` 被静默丢弃**，五列全停在类型默认的 `*`
（mode tail D2），因为 `ColumnDefinition` 不在视觉树里、动态资源查找没有继承上下文。所以
`8|10|*|10|8` 那五条带只能写字面量，`ContentDialog` 批"`ButtonSpacing` 进不去"的理由也一并换成这条
（结论不变，旧理由错了）。

**4 · 谁赢：格子 > 标记属性，无 `TargetName` 的格子 > 样式 setter。** 触发器写具名部件的属性时把标记上那份
盖掉（D3：`Points`、`Grid.Row`、`Grid.Column` 三项全被改），所以"每个朝向一套尾几何"在模板里是可行的。
反过来，想让放格子写的描边到得了卡片，只能走控件自己：不带 `TargetName` 的格子写 `BorderThickness`，卡片用
`{TemplateBinding BorderThickness}` 带走——实测 `Right` 放置读回 `0,1,1,1`。上游那把
`TeachingTipContentBorderThickness<Placement>`（尾在哪条边就清哪条边）因此原样成立。

**5 · `Polygon` 是能用的尾部件。** `Points` 是 `Jalium.UI.Shapes.Polygon` 自己声明的依赖属性（所以格子写得动），
`Fill` / `Stroke` / `StrokeThickness` 从 `Shape` 继承，参数构造可公开 new。三角缺口 = 卡片底色 + 描边同色，
与上游同一个做法。

**6 · 本批推翻了自己的一条主张：控件"占了多少布局"不能从别的窗口里某个部件的尺寸推出来。**
第一版 `FluentTeachingTip` 带一个返回 `new Size()` 的 `MeasureOverride`，理由写的是"实测 320x168 的布局被占了"，
证据就是 mode F 那行 `Container:Border 320x168.12`。那是**卡片在它自己的 `PopupWindow` 里的实现尺寸**，
不是页面布局。同一支探针在两种 build（有 / 无那个 override）下逐列相同：`tip desired 0x0`、
压在下面的邻居 `y=20`（收起、打开、带 target 再打开三档都是）——模板根的 `Popup` 对宿主贡献 0x0，
override 是死代码，已删除。同时 `A_tip_holds_no_layout_room_and_still_realizes_its_card` 那两条断言
在两种 build 下都通过，说明它当时**没有牙**：现在它测的是邻居的落点而不是本元素的槽位，作用是钉住
"模板根必须还是 Popup"，不再声称证明了什么修复。推广两条：① 量"占位"只能读同一块面板里邻居的坐标；
② `ActualWidth` 是面板给的槽位，拉伸元素在 Grid 里读 820x620 而 DesiredSize 是 0——两者都不是"占了多少"。

**7 · 结构闸口把"找自己所在窗口"逼上了逻辑树。** 放置的回退判定要拿卡片和目标所在的窗口边界比，而
`AstraGateTests.Theme_kernel_stays_free_of_repair_loops_and_reflection` 按 AGENTS.md 禁了产品代码里的渲染树走查
（那条闸口是**全文本匹配**，连注释里写出那个 API 的名字都会判红）。可用的是
`FrameworkElement.Parent`：挂载后的链是 `FluentTeachingTip < Grid < StackPanel < Window`（mode parent），
`FindWindow()` 因此走逻辑父链。顺带量到并被**拒绝**的另一条：`Application.Current.MainWindow` 确实指向上屏那个
窗口，但它给整个应用只点名一个窗口，第二窗口场景会拿错的边界去裁卡片——所以用它不是省事，是换一个会错的答案。

## S1-c：列表条目容器的样式走隐式类型键——容器自己的 `Style` 永远是 null（阶段 5 第一段，2026-09-20）

写 `ListBox` / `ListBoxItem` 之前必须量清的十件事，全部来自 `spike/ListProbe` 一趟 `all`（原始输出
`adaptation/s1c-list-host-raw.txt`）。样式与两张模板的参考是上游
`controls/dev/CommonStyles/ListBox_themeresources.xaml`（blob `df9ca8dfe48b7cd4487b24b4f7954c7a74e683f8`），
那几格 `SystemControl*` 目标到底等于多少查的是 `dxaml/xcp/dxaml/themes/generic.xaml:243/300-304`，
方法是 ModernWpf 的 `Styles/ListBox.xaml` + `Styles/ListBoxItem.xaml`。

**1 · `ListBox` 接我们的模板不需要解锁。** `ContentControl.UseTemplateContentManagement` 声明在
`ContentControl` 上，而列表那条链（`ListBox : Selector : ItemsControl : Control`）没有调用它——同一支探针里
`Template = 解析出来的模板` 直接生效，`Style` 仍是 null（S0-h 那个"没有默认样式 ≠ 没有默认长相"在列表族复现）。
原生树是 `ListBox > Grid > Border 'PART_OuterBorder' > ScrollViewer > ItemsPresenter >
VirtualizingStackPanel > ListBoxItem > Grid > Border 'PART_BackgroundBorder' > ContentPresenter`。

**2 · 条目宿主的契约是类型，不是名字。** 把 `ItemsPresenter` 改名成 `NotTheItemsPresenter` 照样出 4 个容器；
把它换成一个具名的普通 `StackPanel` 就 0 个容器。S0-i 那笔"具名部件 vs 类型"的账在列表上站在类型一侧。

**3 · 容器的样式通路里只有隐式类型键这一条没有副作用。** 往 `Application.Resources`（或 `Window.Resources`）
合一本带 `<Style TargetType="ListBoxItem">` 的字典，框架生成的容器**当场就吃**：探针里条目的 `Background`
从 `#00FFFFFF` 变成分组色、`Padding` 从 10,6,10,6 变成 31,7,31,7，而容器自己的 `Style` 属性读回仍是 `null`
——所以断言只能测效果，测不了"样式挂上了"。反过来 `list.ItemContainerStyle = 样式` 压过隐式那条（两个标记
同时改成它那份）并把 `Style` 写成非 null。字典**移除**时已实例化的容器当场退回原生值，不需要修复回路。
上游自己的写法就是 `<Style TargetType="ListBoxItem" BasedOn="{StaticResource DefaultListBoxItemStyle}" />`，
因此这批按隐式键发货，`ListBox` 样式里**不**写 `ItemContainerStyle`——写了等于把想覆写条目的应用挡在门外。

**4 · `ItemContainerTheme` 这个 WinUI 名字在这里不存在**（按属性名反射：not exported），
`ItemContainerStyle` 从 `ItemsControl` 继承，`ListBox` 自己不声明。

**5 · 原生条目已经把"选中"画出来了，画的是 OS 强调色 @0.6。** 未选中时 `PART_BackgroundBorder` 读
`#00FFFFFF`，`SelectedIndex=0` 之后同一格读 `#99680081`——正是上游 `ListBoxItemBackgroundSelected` 指向的
`SystemControlHighlightListAccentLowBrush`（`generic.xaml:301`：`Color={ThemeResource SystemAccentColor}
Opacity="0.6"`）落在 OS 默认强调色上的值。原生 resting `Padding` 是 10,6,10,6，而上游那格是
`<Thickness x:Key="ListBoxItemPadding">12,9,12,12</Thickness>`（同文件 89 行）。两个差都是可断言的数值差，
也是"这批必须重模板"的理由。

**6 · 那 0.6 做不成一条调色板行，于是透明度搬到部件上。** 强调色家族在我们这里只有 `Opacity` 1 / 0.9 / 0.8
三档（`AccentFillColor{Default,Secondary,Tertiary}Brush`），而 `Light.jalxaml` / `Dark.jalxaml` 是
`tools/Sync-AstraPalette.ps1` 从上游生成的，往里加键会被生成器和调色板闸口抹掉。所以条目的选中格 `Fill`
写强调色 token，同一状态另外写那个部件的 `Opacity`（0.6 / 0.8 / 0.9，数值逐个来自 `generic.xaml:300-302`）；
单层实色乘出来的像素与上游那支 brush 等价，而且跟着强调色和主题走。

**7 · S0-u 那条"滚动条先扣布局宽度"在列表面上量到 12 DIP。** 用我们自己的模板（`Border > ScrollViewer >
ItemsPresenter`）：不带任何属性时左隙 1、右隙 13；`VerticalScrollBarVisibility="Auto"` 仍是 13（收起的条照样
占 12）；`="Visible"` 时条读 12x178；`IsOverlayScrollBarEnabled="True"` 把这些都拉成 **1 / 1**，50 条溢出时一样
（overlay 条出现时读 40x178 却不扣布局）；`IsScrollBarAutoHideEnabled="True"` 对这件事**没有任何作用**
（13 原样留着）。原生 `ListBox` 那侧同一条链读 3 / 15。这就是用户两次报的"flyout / 下拉左右边距不一样长"
落在列表面上的那一半根因。

**8 · 虚拟化是真活着的。** 1000 条塞进 260x180 只实现 11 个容器；`ScrollViewer.ScrollToVerticalOffset(400)`
可用，滚完 `Content` 仍是 `ItemsPresenter`。任何"遍历一遍容器"的写法在这个族上不只是被闸口禁，是**根本看不全**。

**9 · 选择语义整个是框架的。** `ListBox` 自己声明的处理器包含 `HandleArrowKey`、`HandleSpaceKey`、
`HandleDragSelect`、`SelectSingle/Multiple/Extended/Range/All`、`UpdateContainerSelection`、
`TryResolveSelectionItem`；`SelectionMode` 三档 `{Single, Multiple, Extended}` 可写；
`ListBoxAutomationPeer` 与 `ListBoxItemAutomationPeer`（后者实现 `ISelectionItemProvider`、
`IVirtualizedItemProvider`、`IScrollItemProvider`）都在。→ 列表族**不起自有类型**，只重模板。

**10 · `ControlTemplate` 在这里只能有一个视觉根。** 探针那份并列 `Border` + `ContentPresenter` 作根的条目模板
解析时抛 `XamlParseException: ControlTemplate can only have one visual tree root element.`，而且是在解析整本
`ResourceDictionary` 时抛的（`BOOT threw`）——一颗坏树根吃掉整本字典。上游 WinUI 的条目模板正是并列两个根
（`ListBox_themeresources.xaml:188-189` 的 `Rectangle` + `ContentPresenter`），照抄就得裹一层 `Grid`。
另外 `ListBoxItem` 上没有 `IsHighlighted` / `IsPressed` / `IsSelectionActive`（三条都 not exported），格子只能用
`IsSelected` / `IsMouseOver` / `IsMouseCaptureWithin` / `IsEnabled`；`MinHeight`、`CornerRadius`、`Padding`
的 setter 实测都落到容器上（读回 34、4,4,4,4、12,9,12,12），换成我们的树之后 `Content` 与 `TextBlock`
都还在（"Item 1" 原样上屏），条目宽 = 呈现器宽、左右隙各 1。


## S1-d：`ListView : ListBox`、`ListViewItem : ListBoxItem`，而 WinUI 的 `GridView` 在这个运行时不存在（阶段 5 第二段，2026-09-20）

工具 `spike/ListViewProbe`（10 个模式：types/mount/retmpl/itemstyle/states/gutter/grid/cells/bar/panel），
原始输出 `adaptation/s1d-listview-host-raw.txt`（含样式落地后复测的那一趟）。下面每条都是量出来的，不是从 WPF 或 WinUI 推的。

**1 · 列表族是继承出来的，不是并列出来的。** `ListView : ListBox : Selector : ItemsControl : Control : FrameworkElement`，
`ListViewItem : ListBoxItem : ContentControl : Control`。`ListView` 唯一的自有 DP 是 `View`（WPF 的列视图），
`ListViewItem` **一个成员都不声明**（`DeclaredOnly` 的 DP 与属性都是空集）。两条直接后果：
① 我们给 `ListBox` 发的隐式条目样式**不会**落到 `ListViewItem` 上（实测静置 padding 仍是原生 10,5,10,5、
`MinHeight=30`，而 ListBoxItem 那套是 12,9,12,12）——隐式键按**精确类型**取，不沿基类走；
② 条目可用的状态面与 `ListBoxItem` 一字不差（`IsSelected`/`IsMouseOver`/`IsEnabled`/`IsFocused` 四条 DP，
`IsPointerOver`/`IsSelectionActive`/`IsHighlighted`/`CheckMode`/`ShowsCheckHint`/`IsDragging`/`SemanticState` 全没有），
所以 S1-c 那张六格状态矩阵可以整体复用，不必再猜一次。

**2 · 宿主能接我们的模板，跟 ListBox 一样不需要解锁调用。** 赋 `Border 'LayoutRoot' > ScrollViewer 'ScrollViewer' >
ItemsPresenter` 之后容器照出（4/4），`ItemsPresenter` 改名照出（4/4），换成没有 `ItemsPresenter` 的普通
`StackPanel` 就 0 个——S1-c 第 2 条"条目宿主契约是类型不是名字"在派生宿主上原样成立。

**3 · 容器的部件名同样不是功能契约，而且比 ListBox 更干净。** 四种自造条目模板（保留 `PART_BackgroundBorder` +
`PART_CellsPanel` / 两个都改名 / 去掉 cells 面板 / 只留 presenter）里，**内容全都照常渲染**（"Item 1" 在四种树下都读得到），
`container.IsSelected` 在我们模板下也照常随 `SelectedIndex` 变 True。关键的一条：出厂模板选中时
`PART_BackgroundBorder` 读到的 `#99680081`，在四种自造模板下**一次都没有再出现**——那支刷是出厂模板自己的格子画的，
不是框架按部件名写进去的。所以列表条目这一层没有任何"必须叫这个名字"的部件；唯一的硬要求还是**有一个
`ContentPresenter`**。

**4 · 隐式类型键这一条在 `ListViewItem` 上一字不差地复现。** 应用级合入 `<Style TargetType="ListViewItem">`
→ 生成容器当场吃（padding 10,5,10,5 → 12,9,12,12、Background → 标记色 `#FFFF00FF`），容器自己的 `Style`
**仍读 null**；移除字典后活容器回落。`ItemContainerStyle` 出厂读 null，且（S1-c 第 3 条）它会压过隐式键——
所以 `ListView` 样式同样**不写** `ItemContainerStyle`，并钉成断言。

**5 · 12 DIP 右槽在第二个列表面上复现得一模一样，overlay 那条修法同样管用；顺手把"S0-u 会不会盖住行"这条结清了。**
出厂 `ListView`：左 1 / 右 13（5 条与 50 条都一样）；我们的模板关 overlay：0 / 12；开 overlay：**0 / 0**。
`ScrollBar` 元素在开 overlay 后报 **40x200**，看上去像"条盖住了行右侧 40 DIP"，走到子树才看清带填充的那层是
`Border 'ThumbBorder' 2x40`，刷是 `#8BFFFFFF`——40 是**命中区**，画出来的是 2 DIP 细条（正是 WinUI 的 overlay 形），
行仍然是整 300 DIP。同样的读数在**已经发货的 ListBox 宿主**上一字不差地出现，所以这是底座的性质而不是某个模板的性质；
`AstraListViewTests` 与 `AstraListBoxTests` 各钉一条，防的就是有人拿"条宽 40"当理由把 overlay 关掉。

**6 · 虚拟化、`ScrollIntoView`、`SelectionMode` 三档都从基类带过来。** 1000 项只实现一屏（`<30` 个容器）；
`SelectionMode ∈ {Single, Multiple, Extended}`。但排他只在控件自己的选择路径里：`Single` 模式下**直接写第二行的
`IsSelected=true`，两行会同时亮**（实测读回 2）。本批因此所有状态主张都走 `SelectedIndex`，并把这条边界原样钉住。

**7 · 宿主级 disable 走不到条目的 disable 格。** `list.IsEnabled=false` 之后容器自己的 `IsEnabled` 确实变 false
（继承到了），但模板里 `IsEnabled=False` 那格的 `Opacity` 仍读 **1**；把 `IsEnabled=false` 直接写在行上，0.3 立刻落下来。
两条都进断言（一真一现状），写成现状而不是"已支持"。

**8 · WinUI 的 `GridView` 没有对应类型：`GridView` 是 WPF 的列视图。** `Jalium.UI.Controls.GridView : ViewBase :
DependencyObject`，`is Control=False`、`is ItemsControl=False`，`Activator.CreateInstance` 出来**不能**当内容塞进面板
（`InvalidCastException: Unable to cast GridView to FrameworkElement`），自有 DP 是一族列头相关
（`ColumnCollection`、`ColumnHeaderContainerStyle`、`AllowsColumnReorder`…），而 **`GridViewItem` 未导出**。
本运行时的 `ListView` 也没留任何 WinUI 列表行为面：`IsItemClickEnabled`、`ItemClick`、`ShowSelectionChecks`、
`MultiSelect`、`IsSwipeEnabled`、`ItemContainerTransitions`、`GroupStyleSource`、`ContainerContentChanging`、
`ChoosingItemContainer`、`RecyclingRatio`、`ShowsScrollingPlaceholders`、`ItemWidth`/`ItemHeight`、`SemanticZoom`、
`ItemsRepeater`、`ItemsView`、`ListViewItemPresenter` 逐条打点全是 `-`。
唯一可行的卡片路线是换 ItemsPanel，而且实测真能换：`ItemsPanel` 是 `ItemsPanelTemplate` 类型的可写属性，
喂 `WrapPanel` 之后 6 条行在 300 DIP 里排成 4+2（第二行 y=32.78）。但这是"能排成网格"，**不是 WinUI 的
`GridView`**——卡片自己的选中描边、`ItemWidth`、语义缩放、点击语义都没有承载面，要 1:1 就得起自有类型。

**9 · 上游 `ListViewItemPresenter` 是 C++ 的，几何读数因此只有一半可读。** 参考树里能读到的只有
`ListViewItemSelectedBorderThemeThickness=4`（`ListViewItem_themeresources.xaml:11`）与
`ListViewItemSelectionIndicatorCornerRadius=1.5`（:60）加上四条 `SelectionIndicator*Brush`；
药丸的**长度**在那个 presenter 内部，本树没有。所以我们的实现发 4 与 1.5、把长度记成自加的 16 并在审计里标明。
另一个可对照的读数在隔壁：`NavigationView` 的指示条是**可读的 XAML**
（`NavigationView_themeresources.xaml:602` 的 `Rectangle x:Name="SelectionIndicator"` 吃
`NavigationViewSelectionIndicator{Width,Height,Radius}` 三行），NavigationView 批可以照它抄，ListView 批不能。

**10 · 本运行时不导出 `ListView*` 宿主级主题行，而且 `FluentThemeManager.GetStyle` 对不存在的键是抛 `KeyNotFoundException`，不是返回 null。** 整棵参考树 grep 不到 `ListViewBackground` / `ListViewStyle`
（宿主外壳在闭源 dxaml/generic.xaml），所以宿主样式只能吃已发的 `ListBox*` 行且**不自造名**；
而探针里 `GetStyle("DefaultListViewStyle")` 在未发货前抛异常这件事，对本仓库所有测试是一个契约：
读 keyed 样式做"未发布"断言时不能走 `GetStyle`，要走 `Application.Current.TryFindResource`（缺失返回 null）。

---

## S1-e：`TreeView` 的形状对不上、`Expander` 这个名字什么都不钩，而 `ContentPresenter` 没有 `Foreground`（阶段 5 第三段，2026-09-20）

原始读数：`adaptation/s1e-treeview-raw.txt`（探针 `spike/TreeViewProbe`，`Program.cs` 第一轮、`ProbePass2.cs` 第二到第四轮）。
上游三文件与 blob：`controls/dev/TreeView/TreeView_themeresources.xaml` `8e7a107255310706dda5e4e5ca6edf99b6b14eb9`、
`TreeView.xaml` `9f419035a19cc68910b97cec4fd5ce264cdfefde`、`TreeViewItem.xaml` `e6655da4feacad75dd68395ec2ddb231e6ad5cfb`。

**1 · 这一段的主前提是形状对不上，而不是缺几个属性。** WinUI 的树是**扁平列表**：`DefaultTreeViewStyle` 的模板整体
是一个 `TreeViewList`（`TreeView.xaml:26`），`MUX_TreeViewItemStyle` 甚至 `BasedOn DefaultListViewItemStyle`
（`TreeViewItem.xaml:3`）。本运行时是 WPF 的嵌套形：`TreeView : ItemsControl : Control`（**不是** `Selector`），
`TreeViewItem : HeaderedItemsControl : ItemsControl`。`TreeViewList`、`TreeViewNode`、`TreeViewItemPresenter`、
`TreeViewItemTemplateSettings` 四个全部未导出；`TreeView` 自有 DP 只有 `SelectedItem`/`SelectedValue`/`SelectedValuePath`，
`TreeViewItem` 自有 DP 只有 `IsExpanded`/`IsSelected`/`IsSelectionActive`（外加 `HasItems` 属性）。WinUI 侧的
`SelectionMode`、`SelectedNode`、`ExpandAll`/`CollapseAll`、`MultiSelect`、`ItemInvoked`、`CollapsedGlyph`/`ExpandedGlyph`/
`GlyphBrush`/`GlyphOpacity`/`GlyphSize`、`CanDragItems`/`CanReorderItems`/`AllowDrop`、`ItemContainerTransitions` 逐条打点全是空。
结论是"外观可 1:1、行为面一半无处安放"，所以起原生重模板，不起自有类型：缺的是运行时的行为面，不是模板能补的东西。

**2 · 没有锁，条目宿主契约是 TYPE（列表族第三次确认）。** 我们的宿主模板直接挂得上；把 `ItemsPresenter` 改名照样出容器；
删掉 `ItemsPresenter` 出 **0** 容器；连 `ScrollViewer` 都不要也能出容器（第一行 300x28）——保留 `ScrollViewer` 是为了
滚动条与 overlay 契约，不是因为容器需要它。

**3 · 名字不是契约这件事，在树上走得更远：`Expander` 什么都不钩。** 在模板里放一个 `Name='Expander'` 的 `ToggleButton`，
用它的点击通路（automation `Toggle`）打下去，`IsExpanded` 纹丝不动；而 stock 模板里**根本没有** ToggleButton
（`toggle-button parts in the tree: 0`）。唯一能用的模板内路线是属性：
`IsChecked="{Binding IsExpanded, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}"` 实测**两个方向都通**
（点击→`IsExpanded=True`→子容器落地；`IsExpanded` 写 True→`IsChecked=True`；直写 `IsChecked=False`→`IsExpanded=False`）。
这和 S1-c 那条"附加属性 Setter 到不了部件"不矛盾：**附加 Setter 不行，TemplatedParent 双向绑定行**——这是模板驱动
控件状态的第二条通路，本仓库第一次量到。

**4 · 折叠不销毁容器，且触发器要写对Targets。** 展开使容器数增加、再折叠数不回去（stock 与我们一致），所以可见效果只能
看子宿主的 `Visibility`。四种静止值写法（只有反向格 / 属性上 `Visibility='Collapsed'` + 正向格 / 正反双格 / 外层 `Border`
承载门）实测**全部有效**。第一轮里那个"失效"的变体是另一回事：它在代码里构造 `Style` 并塞进一个**没有 `TargetType`** 的
解析模板，那条 `Trigger Property='IsExpanded'` 就永不触发；换成带 `TargetType='TreeViewItem'` 的模板立刻正常。
发货写法因此必须让 `ControlTemplate` 带 `TargetType`（我们的 `.jalxaml` 一直如此）。叶子行用
`Trigger Property='HasItems' Value='False' → Visibility='Hidden'` 收掉箭头，`Hidden`（不是 `Collapsed`）才保住 20 DIP 的
列位、让叶子和兄弟的标签对齐。

**5 · 深度没有任何可读承载面，缩进只能由嵌套表达。** 对 level-1 与 level-2 两行做**全部可读值差分**（含整条继承链上
每个 public static DP 逐个 `GetValue`）只差 `IsExpanded`、`Header`、尺寸与布局记账、`Parent` 类型——没有任何
indentation/depth。stock 用 `PART_IndentSpacer` 画缩进（level-1 宽 0、level-2 宽 16、行 x 不动），但那个宽度不是可读
属性。于是我们的实现把 16 放在子宿主的 `Margin` 上，让递归自己叠出层级；步长不是自加的（stock 实测同为 16）。
代价进 Known Gaps：子行的填充比父行窄 16 DIP，而 WinUI 的行永远铺满列表宽。

**6 · 树族的容器 `Style` 不是 null，且隐式样式只吃"生成之前"的那一份。** `Application.TryFindResource(typeof(TreeView))`
与 `(typeof(TreeViewItem))` 都返回 Style——本运行时自带这两条隐式样式，所以 S1-c/S1-d 的"容器 Style 恒为 null"在这里
**不成立**，跨族断言只能读效果。另外量到时序：先把树挂上再生效的应用级隐式条目样式**不更新已生成的容器**
（padding 仍是 stock 的 6,3,6,3），生成之前合并则六种模板变体全部吃到。本仓库的字典在任何容器存在之前就已应用，
所以这条不是产品缺陷，但它是"运行时热改主题能改到已存在条目"这个设想的硬限制。

**7 · 硬缺陷类：`ContentPresenter` 没有 `Foreground` 成员，指向它的状态前景格全是哑格。** 反射读数
`NO - ContentPresenter has no Foreground member`。往它写 `Setter TargetName="ContentPresenter" Property="Foreground"`
不报错、字典照常加载、颜色永远不变。可用路线是同一条状态写在**条目控件自己**的 `Foreground` 上（`Style.Triggers`，
不带 `TargetName`），生成的 `TextBlock` 靠属性继承取到：实测 `item=#FF112233 → text=#FF112233`，
`IsEnabled=False` 格逐行生效且不动兄弟行。本批按这条路线发八条前景行；已上库的 ListBox/ListView/ComboBox/菜单族里
同类格另立修复任务（#31），因为资源键反查闸只查"名字有没有被读"，查不出"写的目标根本没有这个属性"。

**8 · 树比列表多一个 `IsSelectionActive`，少的是同一批。** 可选信号：`IsExpanded`、`IsSelected`、`IsSelectionActive`、
`HasItems`、`IsMouseOver`、`IsMouseCaptureWithin`、`IsEnabled`、`IsFocused`；仍没有 `IsPressed`/`IsPointerOver`，
所以按下态照旧只能用 `IsMouseCaptureWithin` 近似。选择互斥与 S1-d 相反：**在两行上直写 `IsSelected` 最终只有一行选中**，
`tree.SelectedItem` 跟着走，因此这里不需要"直写绕过互斥"那条边界断言。

**9 · 上游连一条 `TreeView*` 宿主行都没有（第二次遇到，但这次连可借的名也没有）。** `DefaultTreeViewStyle` 只设
`IsTabStop`、三个拖放开关、`ItemContainerTransitions` 和 `Template`，背景靠 `TemplateBinding` 递给内部列表。
所以宿主发 `Background=Transparent`、`BorderThickness=0`，不 Borrow 也不自造名；S1-d 那条"一次令牌覆盖同时移动两个宿主"
的断言在这里反转成"同一个覆盖移动 ListBox 而**不**移动 TreeView"，两边都进断言，防止有人日后把树刷成实心底。

**10 · 药丸几何这次整份可读。** `TreeViewItem.xaml:130` 明写 `Rectangle Width=3 Height=16 RadiusX=2 RadiusY=2`，
不像 ListViewItem 那样把数藏在 C++ presenter 里（S1-d 第 9 条），所以本批三个数全部照抄、没有自加几何。
上游那四条指示器行名字带 `Foreground` 而实际喂 `Fill`，我们保持同名同喂法。

**11 · 12 DIP 右空隙在第三个宿主上复现，并由同一个开关修掉。** stock `left=1 right=13`；我们的宿主模板 overlay 关
`1/13`、开 `1/1`（行 298）。那 1 DIP 来自探针模板的 `BorderThickness={TemplateBinding BorderThickness}`（该控件默认
1）；产品样式把 `BorderThickness` 钉成 0，测试实测左右相等且行宽 300。叠加条的读数与列表族一致：条元素报 40 宽是
命中区，真正画出来的是 `ThumbBorder` 2 DIP。

## S1-f：格子的目标必须真的有那个属性——68 条哑格的普查、重定向与一条可见缺陷（哑格批，2026-09-20）

原始读数：`spike/ForegroundSweep/`（`census.txt` 普查、`gate-before.txt` 闸口首跑 68 条、`reroute-report.txt` 发货变换、
`census-after.txt` 复测 0 条、`routing-before.txt`/`routing-after.txt` 效果事实先失败后通过、`suite-after.txt` 中间全量、
`gates-menu-cleanup-fail.txt` 与 `gates-final.txt` 收尾两次串行闸口（`gates-before-cleanup.txt` 是中间那一次）、
`build-noincremental.txt` 非增量重建警告数、
`gallery-smoke.txt` 四页上屏）。哑格这一类的第一手测量是 `spike/TreeViewProbe/treeview-probe-fg.txt` 第 7 行
（`does a ContentPresenter carry Foreground at all? NO`），即 S1-e 第 7 条。
结构闸口：`tests/FluentJalium.Tests/AstraGateTests.cs` 的 `State_cells_name_properties_the_template_parts_actually_have`；
效果闸口：`tests/FluentJalium.Tests/AstraForegroundRoutingTests.cs`（8 条事实）。

**1 · 先量后改，把整棵 `Styles/` 的格子逐条反射。** 闸口做法：解析每个 `ControlTemplate`，把 `Name=` 的**声明元素类型**
记成部件表，再拿每条 `<Setter TargetName Property>` 去 `GetType().GetProperty(property, Public|Instance)` 查一遍。
首跑命中 **68 条**：`Foreground` 写到 `ContentPresenter` 上的 **50** 条（`CheckLabel` 11、`RadioLabel` 7、
`ComboBoxItem` 的 `ContentPresenter` 9、`PART_SelectionPresenter` 5、`ListBoxItem` 6、`ListViewItem` 5、菜单三条
item 样式的 `IconContent` 6、`DefaultNumberBoxStyle` 的 `HeaderContentPresenter` 1），`BorderBrush` 写到 `Grid` 上的
**18** 条（`CheckRoot` / `RadioRoot`——两条根都是 Grid，和上游的 `RootGrid` 同形）。
"格子指向模板里根本没声明的部件"这一类是 **0** 条，所以名字是对的、属性是不存在的。
这类缺陷的隐蔽处在于：字典照收、构建照绿、条件照触发，只是那一格永远无事——本仓库此前的六种证据里没有一种能看见它，
资源键反查闸只问"这个名字有没有被读"，答案永远是"读了"。

**2 · 可用路线是同一条状态写在控件/容器自己身上，共 61 条重定向。** 做法是删掉 `TargetName`，让格子落到
`TemplatedParent`，生成的 `TextBlock` 靠属性继承取到（TreeView 批量的实测：`item=#FF112233 → text=#FF112233`）。
按文件：`Selection.jalxaml` 36、`Inputs.jalxaml` 14、`ListBoxes.jalxaml` 6、`ListViews.jalxaml` 5。
`ControlTemplate.Triggers` 里不带 `TargetName` 的 Setter 落到控件本身，这条在 ComboBox 占位符的测量里复量过一次
（改动前那格是死的、改动后颜色到位，说明落点确实是控件而不是模板根）。

**3 · 菜单那 6 条是删除而不是重定向。** `MenuFlyoutItem` / `ToggleMenuFlyoutItem` / `MenuFlyoutSubItem` 的两条标签行
**早已**作为 `Style.Triggers` 的活格写在控件上（同一个 `Foreground`、同一批行名），模板里那 6 条 `IconContent` 格子是
它们的重复副本，且 `IconContent` 自己的 `Foreground="{TemplateBinding Foreground}"` 已经把图标接到控件的 Foreground 上。
留着反而埋一个日后与活格抢优先级的死格，所以删掉，资源行的读者数不变。

**4 · 需要独立颜色的头部不能走控件级路线，于是换承载元素：`ContentPresenter` → `ContentControl`。**
NumberBox 的头必须与编辑区异色，写到自己身上就把两者并成一条，所以头部只能保住部件级写法。反射读数是
`ContentControl` 有 `Foreground`、`ContentPresenter` 没有，于是承载元素改成 `ContentControl`（其余属性一字不动），
那一格立刻可达。**同时故意不写静止色**：元素的 `Foreground=` 属性是本地值，本地值压过一切格子，写上就等于把
disabled 格再次锁死；静止色由盒子继承过来（两个名字都指向 `TextFillColorPrimaryBrush`，同一支笔刷），
disabled 格因此是唯一的写者。字典侧随之把 `TextControlHeaderForeground` **收回**——没有读者的行不发布，
只留 `TextControlHeaderForegroundDisabled` 与 `TextBoxTopHeaderMargin`。
测到的止点：格子现在确实落在承载元素上，而它生成的那一枚字形仍保持**构建时**继承到的颜色，所以 disabled 头部
离像素还差一跳，按原样写进事实与 Known Gaps，不做相邻替代。

**5 · 框架会在禁用时给生成的文字盖一个本地值，disabled 标签色因此不是我们的。** 诊断读数
`box=#FFAEAEB2 boxLocal=False text=#FFAEAEB2 textLocal=True token=#5C000000`：控件自己不带本地值，文字元素带，
颜色是 `#FFAEAEB2`，而所有 disabled 行要的是 `TextFillColorDisabledBrush` = `#5C000000`。本地值排在格子前面，
`TextBlock` 上那一个我们读得到、改不动（除非再写一个本地值，那就是伪造）。所以四条 disabled 事实钉的是**测量值**，
并在注释里点名本该生效的行名；这是"哪天框架不再盖章，就该回到 token"的四个哨位，不是四个通过。

**6 · 这批唯一肉眼可见的缺陷是 ComboBox 的占位符。** 可观测性地图（不带指针就能分得开、且不被框架盖章抢走的行）：
`CheckBox`/`RadioButton`/`ListBoxItem`/`ListViewItem` 的状态前景除了 disabled 全部别名同一支
`TextFillColorPrimaryBrush`，`ComboBoxItem` 的 pressed 走 Secondary、disabled/selected-disabled 走 Disabled，
于是真正能读回的只有 `ComboBoxPlaceHolderForeground`（Secondary）对 `ComboBoxForeground`（Primary）——空框把占位符
涂成选中色正是本批修掉的可见缺陷，选中与清空两侧都断言。

**7 · 18 条 `BorderBrush` 重定向后仍然不落地，但这与上游一致。** 这些行在上游全部别名
`SubtleFillColorTransparentBrush`（`CheckBox_themeresources.xaml:29,205`），本来就画不出东西；真正的可见环是
`CheckSurface` / `RadioRing` 那批 `CheckBoxCheckBackgroundStroke*` 行。改它们的动机是结构诚实（闸口绿、格子可达），
不是外观，因此不声称任何描边像素变化。

**8 · 闸口的边界要说清：它查格子，不查模板属性。** 同一种静默失效在属性上还在——例如根 Grid 上
`BorderBrush=` / `BorderThickness=` 这类该类型没有的属性——本批没有把它一起扫，另立任务 #32，
不把"格子全绿"说成"模板全绿"。

**9 · 四类证据的落点。** 构建：`gates-final.txt`（串行闸口全绿：928/928、0 skip、调色板三档 checked=True）与
`build-noincremental.txt`（非增量整解重建 18 条警告 / 0 错误，全部落在 `AstraMenu`/`AstraAppBar`/`AstraContentDialog`
三个既有文件的 nullability 上，与基线逐名相同）；行为：死格先失败后通过（`routing-before.txt` 8 条里 6 失败 →
修完只剩被测量的框架止点），中间态 `suite-after.txt` 的 11 条失败逐条是旧测试形状把死格读数写死；
上屏：`gallery-smoke.txt` 里 `selection,inputs,menus,overview` 四页各自 mount 后优雅关闭、无残留进程；
视觉：本批**没有**新增像素捕获，读回值不等于像素；硬件输入：仍未做（#13），指针态（hover/press）一条格子都没有用真指针验证过。

**10 · 改完之后的两次重跑各抓到一个"看代码看不出来"的错误，这两条留在证据里。**
第一次：删掉那 6 条菜单死格之后，`ToggleMenuFlyoutItem` 的 `IsEnabled=False` 触发器变成空壳，撤掉它之后
`AstraMenuTests.Each_item_style_carries_one_cell_per_reachable_state` 报该样式的状态集合多了一条
（`gates-menu-cleanup-fail.txt`，1 失败 / 927 通过）——契约本身没错，错在我改了模板却没同步契约，
按新形状改断言而不是把空触发器塞回去。第二次：改了 `Catalog.json` 的 gap 文案之后单跑测试项目，
`The_gallery_project_carries_the_catalog_it_reads` 报不一致——那条闸口比的是**源文件与 Gallery 输出目录里的副本**，
而 `dotnet test tests/…` 根本不构建 Gallery 项目，于是一次完全正常的编辑被读成"复制清单坏了"。
教训写在这里：改过 `.jalxaml` 或 `Catalog.json` 之后，唯一算数的读数是把整解重建的串行闸口再跑一遍，
不是"上一次全绿所以这次也算绿"。

## S1-g：属性也一样要"目标真的有这个成员"——35 条死写属性与四处方角表面（属性死写批，2026-09-20）

原始读数：`spike/AttributeSweep/`（`Program.cs` 普查、`attrsweep-1.txt` 首跑 35 条、`strip-report.txt` 逐条删除清单、
`attrsweep-2.txt` 复测 0 条、`menuitem-fill-first.txt` 一处读数的先失败后通过、`gates-1.txt` 与 `gates-2.txt` 两轮串行闸口）。
结构闸口：`AstraGateTests.Template_attributes_name_members_the_element_type_actually_has`；
效果闸口：`AstraSurfaceGeometryTests`（6 条事实）与 `AstraTeachingTipTests.The_card_paints_on_a_border_that_can_hold_its_radius`。

**1 · S1-f 那道闸口只反射格子，属性是同一类缺陷的下一层。** 普查用同一套做法（把每个元素的**声明类型**记成成员表，
再逐条反射属性名），范围扩到 `src/FluentJalium` 下每一个可解析元素：首跑 **2530 个元素 / 5263 条属性**，
命中 **35 条**；带前缀的附加属性 **0** 条（`Grid.Row`、`ScrollViewer.*` 那一批全都有主）。发货后复测
2533 个元素，直接命中 0、附加命中 0。

**2 · 命中的形状就是那几个"看着像成员"的名字。** `ContentPresenter` 的 `Foreground` 15 条、`TextWrapping` 2 条、
`HorizontalContentAlignment` / `VerticalContentAlignment` 各 1 条、`Background` 1 条；`Grid` 的 `BorderBrush` 4 条、
`BorderThickness` 4 条、`CornerRadius` 4 条、`Padding` 2 条；`StackPanel` 的 `Padding` 1 条。
`Grid` 有 `Background`、`ContentPresenter` 有 `Margin`/`HorizontalAlignment`——这正是它们能"看起来正常"的原因，
也是为什么资源键反查闸、构建、字典加载三样全都不会响。

**3 · 四处真正看得见的后果，这批不是为了清洁。**
(1) **NumberBox 的 Spinner 弹层**：`PopupContentRoot` 是 Grid，`OverlayCornerRadius`、`NumberBoxPopupBorderBrush`、
`NumberBoxPopupBorderThickness` 三条落在它身上永远读不到 → 弹层直角、无边框。承载元素换成 Border，行名 `PopupContentRoot`
留在画者身上，行布局挪进内层 Grid。
(2) **InfoBar 的两条 padding**：`InfoBarContentRootPadding`（`16,0,0,0`）写在 Grid、
`InfoBarPanelVerticalOrientationPadding`（`0,14,0,18`）写在 StackPanel，两处都没有该成员 → 内容贴死在表面边上。
前者挪到 `RootBorder`（同一个盒子，`MinHeight=48` 跟着挪，48 才仍然含 padding），后者由新增的 `PanelSurface` Border 承载，
`Panel` 名字与 StackPanel 类型都不动。
(3) **TeachingTip 的卡片**：`ContentRootGrid` 上的 `BorderBrush`/`BorderThickness`/`CornerRadius` 读不到 → 卡片直角无边框。
布局格保留原名与原职责，画者换成它第一个孩子 `ContentRootSurface`（跨全部行、被内容盖在上方）。
(4) **MenuBarItem 的静止底色画了两次**：根 Grid 与内层圆角 Border 绑同一支笔刷，Grid 那份没有圆角可说。
**但这条在静置与悬停两态都看不见**：`MenuBarItemBackground` 别名 `SubtleFillColorTransparentBrush`，
底层方角画的是 `#00FFFFFF`——什么都盖不出来。会看见的条件是应用自己设 `Background`（本地值经 `TemplateBinding`
同时喂两层）。修法仍是只让 Border 画。读数与"第一版断言其实是空的"记在
`spike/AttributeSweep/menuitem-fill-first.txt`；**四角没有像素捕获**。

**4 · 删除之前先量"继承有没有已经代偿"，不猜。** 15 条 `Foreground="{TemplateBinding Foreground}"` 一律删除：
S1-f 已经量到控件级 `Foreground` 是本运行时唯一通路，生成的文字靠继承取到（`item=#FF112233 → text=#FF112233`），
那条属性本来就是空转。2 条 `TextWrapping="Wrap"` 也删，依据是 `Styles/Navigation.jalxaml` 里那条更早的测量——
**本运行时 ContentPresenter 生成的文字默认就是 `Wrap`**（当初为"别换行"才不得不写进 `ContentPresenter.Resources`
的隐式 `TextBlock` 样式）。删完仍留一条事实把"行为没变"钉住：`AstraSurfaceGeometryTests.A_check_box_label_and_a_radio_label_still_wrap_without_the_attribute`。

**5 · 内容对齐换的是同一条已验证路线，不是新发明。** `ContentPresenter` 没有 `*ContentAlignment` 成员，
但有 `HorizontalAlignment`/`VerticalAlignment`——复选、单选两个标签模板一直就是这么写的。Expander 的内容 presenter
照同一形状改绑，读回随 `HorizontalContentAlignment` 两个取值走。

**6 · 弹层里读部件要读"孩子本身"，按名字查找只往下走。** `PixelHarness.Named(popup.Child, "PopupContentRoot")` 返回 null：
名字就落在那个 child 上，而名字查找只测子树。改成把 `popup.Child` 直接当画者读、断言它的 `Name`——
这条陷阱 `AstraTeachingTipTests` 早就写过一遍，这次是我重新踩。

**7 · 闸口与普查的覆盖面不同，把数字留在这儿。** 闸口的类型宇宙是"两个程序集里的 `DependencyObject` public 子类"
（本次读回 1972 条属性），普查多带 `Jalium.UI.Core`/`Media` 所以能走到 5240 条——`Color`、`Thickness`、几何、
关键帧这类没有部件语义的类型因此只有普查在看。两边现在都是 0 命中；哪天两边数字背离，先看这两个计数。

**8 · 读数本身也会静默——它必须认元素的类型。** `AstraSurfaceGeometryTests` 那条 MenuBarItem 事实的**第一版**写的是
`root.GetValue(Control.BackgroundProperty)`，而 `root` 是 `Grid`：`Control` 不是它的基类，那格 `DependencyProperty`
与 `Grid` 自己声明的 `Background` 不是同一个对象，所以元素明明带着 `#00FFFFFF`，读数仍是 `null`——
一条永远为真的断言，和它要防的缺陷是同一类（名字对、通路不存在）。两行并排放一次就分出来了：
只有读 `root.Background` 的那行会响（`spike/AttributeSweep/menuitem-fill-first.txt`）。
把整个测试工程按 `GetValue(<类型>.<X>Property)` 扫一遍，同一处错还有一个兄弟：`AstraTeachingTipTests` 里"布局格不再声明
`Background`"那一行读的也是 `Control.BackgroundProperty`，按上面同一条机制它在坏标记上也是绿的，已改读 `Grid.Background`
（这条**没有**单独再破一次标记去量，依据是上面那次实测的机制）。
剩下的跨类型读数两类：接收元素本来就是 `Control`（`ScrollViewer`、`SplitButton`），或者读错了会**当场抛**而不是静默
（`(double)element.GetValue(UIElement.OpacityProperty)` 拿到 null 就炸）。
**推论：修静默失效的断言，要先在缺陷还在的时候跑红一次**，否则"绿"可能只是又一次读错了格子。

**9 · 这批没做的。** 属性名合法但 `{ThemeResource}` 资源名不存在的情况（资源键闸口管）；`Style`/`Setter` 这些
非 `DependencyObject` 标记类型上的属性名拼错（闸口的类型过滤把它们排除在外）；真指针输入（#13）一条都没有；
`ContentRootGrid` 那类"格子里名字对了但部件不存在"的组合仍由 S1-f 那道闸口负责。

## S1-h：模板能不能建，取决于类型链上有没有 `ContentControl`——原生 tab 族两条通路都不建，第三种静默是 `ContentPresenter` 不画笔（阶段 5 第四段，2026-09-20）

原始读数：`spike/TabViewProbe/`（pass 1 类型普查 `tab-probe1.txt`、上游行清单 `upstream-keys.txt`）、
`spike/TabViewTint/`（pass 2 逐杠杆着色 `tint-1.txt`、`tint-2.txt`）、`spike/TabViewStyle/`（pass 3 模板到达通路
`tab-style1.txt`）。消费方：`Controls/Navigation/FluentTabView.cs`、`FluentTabViewItem.cs`、
`Styles/TabView.jalxaml`、`ThemeResources/TabView.jalxaml`、`docs/astra/audits/tab-view.md`；
效果闸口：`AstraTabViewTests`（16 条事实）。

**1 · "能不能重模板"是有位置的，位置在类型链上。**〔下一段作废了这条的普适性，见 S1-i 第 1 条：
`DataGrid` 同样不在 `ContentControl` 链上，三条通路却全部落上我们的模板。真正的判别量是"框架给那个类型的
样式里有没有 `Template` 格子"。下面这段保留，因为它记录的 `TabControl` 那条测量仍然成立——错的是我
把它写成了通用判据。〕反射读 `UseTemplateContentManagement` 的声明类型：它是
`ContentControl` 上的 `protected` 成员。链上有 `ContentControl` 的（`TabItem : HeaderedContentControl :
ContentControl`、`ListBoxItem`、`ContentControl` 本身）子类够得着；链上没有的（`TabControl : Selector :
ItemsControl : Control`、`ListBox`、`ItemsControl`）任何子类都够不着。注意这条**不是**"够得着就能建"的充分条件，
见第 2 条。同时它也解释了为什么 `ListBox` 能被样式重模板而 `TabControl` 不能：`ItemsControl` 系的模板走的是另一套
内部展开（`Control.ExpandTemplateContent`/`ClearTemplateContent` 都是 private），而 `ContentControl` 系要先被
那个开关放行。

**2 · 同一个窗口里两条通路、一个阳性对照，原生 tab 族两处都不建。** pass 3 用 `ListBox` 证明探针看得见建起来的
模板（样式 setter → 部件 `ProbeListRoot`/`ProbeListItems` 都在树里、54600 像素着色），然后：
`TabControl` 喂样式 setter → 部件全无、模板根 magenta **0 像素**、 realized 树与不喂时逐行相同；
`TabControl` 喂本地值 → 同上；`TabItem` 喂样式 setter → 同上；
**`TabItem` 的子类在构造函数里自己开开关** → 仍然不建（部件 absent、lime 0 像素，482×36 整块由它自己的
`OnRender` 画，选中时 `#3A3A3C`、还带 964 像素的 `#1E793F` 指示条）。
同一段里 `ContentControl` 的子类开同一个开关 → 三个部件全在、9640 像素着色。
所以"原生 tab 族不能重模板"不是偏好，是三条通路全测过之后的读数。

**3 · 要模板反而更坏。** 给 `TabControl` 写上 `Template` 之后，它自己的条 measure 成 `StackPanel 0x0`、
页签宽 0，却仍在画（section D/F 两组读数；没有 `Template` 时同一宿主量到 `420x36`）。失败模式不是"退回原样"，
而是"退回一个塌陷的原样"。

**4 · 第三种静默：`ContentPresenter` 在这里是纯内容宿主。** 上游两个按钮样式的模板根就是
`ContentPresenter`，并在它身上写 `Background`/`BorderBrush`/`BorderThickness`/`CornerRadius`/
`VerticalContentAlignment`（`TabView.xaml:191`、`:242`）。照搬过来被结构闸口逐条点出
（"ContentPresenter 'ContentPresenter' has no Background" ×5、`has no BorderBrush` ×5）。
这与 S1-f（格子的 `Foreground`）、S1-g（元素属性）是同一族的第三种：前两次读的是名字存不存在，
这次要记的是**这个类型的 presenter 根本没有画笔层**。修法同族：笔挪到 `Border ContentRoot`，
presenter 只留内容与居中；`Foreground` 仍然写在按钮自己身上，靠继承到达它生成的字形。

**5 · 两条会咬人的度量读数。**
(1) **`Button` 的默认 `MinHeight` 是 36。** 按钮样式已经写了 `Height=24`，两个按钮仍量到 36，
于是整页签从上游的 32 顶到 44（断言 `Assert.Equal(32d, …)` 读出 44）。补 `MinHeight=0` 之后回到 24/32。
上游不需要这一行，它的 `Button` 没有这个默认最小值——**"上游没有的行"有时正是这个运行时的坑**。
(2) **`FontSize` 写在 `ContentPresenter` 上到不了它生成的 `TextBlock`**：标签量到 19.78（14 的字高），
挪到条目样式（`Control` 真有这个成员）之后量到 17.24。

**6 · `AutomationProperties` 不能作为前缀属性写进 `.jalxaml`。** 闸口原文
"names a prefix type AutomationProperties the runtime does not export"。本仓既有做法是在代码里
`AutomationProperties.SetName(...)`（`FluentNavigationItem`、`FluentNavigationView` 都是），模板里只留 `ToolTip`。

**7 · 判据仍然是"读数先红一次"。** 本段所有断言都读实化元素自己的成员，而且这套判据在同一批里当场抓到自己两条
空读数：pass 3 首跑的 `Named()` 按**类型名**匹配部件，于是 `ListBox` 阳性对照被报成"部件不存在"，而同一次运行
两行下面的树打印里 `Border 'ProbeListRoot'` 明明在——改成读 `FrameworkElement.Name` 之后才与像素、树一致。
规则和 S1-g 第 5 条同一条：**断言先要在缺陷还在的时候红过一次，且必须点名接收者的类型。**

**8 · 源生成器先写属性、后挂子元素，所以"值必须落在稍后才填的集合里"这种校验必炸。**
`FluentTabView.SelectedIndex` 第一版两界都抛异常，`<FluentTabView SelectedIndex="0">` 加三段
`<FluentTabViewItem>` 的 markup 就在 `InitializeComponent` 里抛了
`ArgumentOutOfRangeException`（`FluentJalium_Gallery_MainWindow.g.cs:2486`，
`spike/TabViewStyle/crash-navigation-page.txt`）——属性那行在前，`TabItems.Add` 在后，
写下的索引当场"越界"。这条与 S1-e 第 4 条（隐式样式必须在容器生成之前并入）同族：**markup 的执行顺序
不是声明顺序的直觉**，凡"属性引用另一个成员稍后才会有的状态"都要写成"先接受、集合变了再落地"。
改法与账单在 `audits/tab-view.md` §5 第 6 条。
**推论给后面每一批：这类崩溃测试工程一条都看不见**——同一棵树上 40 条 TabView 事实与 977 条全套全绿，
只有把那一页真的挂上屏（`tools/Test-AstraGallerySmoke.ps1 -Page navigation`）才炸得出来。
## S1-i：表格族推翻 S1-h 的判据——能不能重模板不看类型链，看框架那份样式里有没有 `Template` 格子（阶段 5 第五段，2026-09-20）

原始读数：`s1i-datagrid-host-raw.txt`（653 行，`spike/DataGridProbe --mode all`）、
`s1i-datagrid-tokens-raw.txt`（100 行，`--mode pass2`）。运行时 = NuGet Jalium.UI 26.10.9；
框架自己的表格标记在 sibling 树 `src/managed/Jalium.UI.Controls/Themes/Controls/DataGrid.jalxaml`（195 行）
与 `TreeDataGrid.jalxaml`（111 行），该树 `git describe` = **v26.10.9**，与运行时同版本，所以它的
部件名与 token 名可以当名单用；每一条仍在本进程复核过。

**1 · 先更正上一段刚写下的判据。** S1-h 第 1 条把"链上没有 `ContentControl` 就不能重模板"当成了
分类型开关。`DataGrid : MultiSelector : Selector : ItemsControl : Control`——和 `TabControl` 同一条
`Selector` 支系——但三条通路全部落上我们的模板：隐式应用级样式、显式 `Style` + `Template` 格子、
`Template` 本地值，每次都能在同一次挂载里读到 `ProbeTemplateRoot(Border)` 与 `ProbeItems(ItemsPresenter)`，
`TabControl` 三条全塌（`spike/TabViewStyle`）。所以真正的判别量不是链，而是**框架给这个类型的那份样式里
到底有没有 `Template` 格子**：`Application.TryFindResource(typeof(DataGrid))` 返回一个
`TargetType=DataGrid` 的 `Style`，11 个格子里第 11 个就是 `Jalium.UI.Controls.ControlTemplate`
（`s1i-datagrid-tokens-raw.txt` §I）；`TabControl` 那边根本没有这份样式。改判据一句话：
**问"框架样式有没有模板格子"，别问"链上有没有 ContentControl"。**

**2 · "框架不嵌 .jalxaml、163 个控件零默认样式"这条要收窄，本批把它证伪了一半。** 类型清单里明明白白躺着
`__JalxamlGenerated._Dict_Jalium_UI_Managed_Themes_Controls_DataGrid_jalxaml` 和
`_Dict_..._TreeDataGrid_jalxaml`——标记在，只是编译成预构建字典类而不是资源流，所以
`LoadGenericTheme()` / `GetGenericThemeStream()` 返回 null 说的是流那条路，不等于"框架没有主题"。
更要紧的是**读取时机**：未挂载的 `new DataGrid()` 报 `Template=null Style=null`，挂载 + 10 帧之后报
`Template=SET`，而 `Style` 仍然是 `null`——当年那 163 个 `Style == null` 全是在未挂载实例上读的，
这个属性在这条运行时里根本不报告主题样式。所以"Astra 站在 ModernWpf `ControlsResources` 的位置、
我们没画的控件会裸奔"这句要改成：**Astra 是在覆盖框架自带的一套近似 Fluent 的主题**；没覆盖的控件不是
朴素，是框架那套（表格里就是 r=12 圆角 + 强调色渐变行）。`adaptation/05` 第 36 行
"DataGrid | 纯模板，可完全覆盖 | Styles" 从预测升为已测。

**3 · 框架那份表格主题一共读 11 个名字，Astra 只定义其中 2 个。** 逐个查 `x:Key`：
`LayerFillColorAltBrush`、`SubtleFillColorSecondaryBrush` 在 `ThemeResources/Light.jalxaml` 与
`Dark.jalxaml` 里（WinUI token 名），其余 9 个——`SurfaceBackground`、`TextPrimary`、`TextSecondary`、
`TextDisabled`、`ControlBorder`、`ControlBorderFocused`、`AccentBrush`、`TextOnAccent`、
`CaptionFontSize`——是框架自己的 token 层，Astra 一条都没有。**身份才是钩子的证据**：挂载后
`PART_OuterBorder.Background` 与 `TryFindResource("SurfaceBackground")` 是**同一个实例**
（`sameInstance=True`，`TextPrimary`/`ControlBorder` 同），并且 `IsEnabled=False` 那条
`TextDisabled` 也是同实例 + `Opacity=0.56` 落地。同一份网格在
`FluentThemeManager.Apply` 前后各读一次：`LayerFillColorAltBrush` 从框架的 `#FF3A3A3C` 翻成我们的
`#0DFFFFFF`、`SubtleFillColorSecondaryBrush` 翻成 `#0FFFFFFF`，都是实例同一性成立——
**名字撞上去就能给框架自己的模板换色，一行代码都不用**。这是 A2 别名层（并行任务 #12）第一次拿到
"撞名字能到什么程度"的实测边界。

**4 · 撞名也撞出一条品牌绿缺陷，两条选中文本缺陷。** 未加 Astra 的框架基线：
`ControlBorderFocused = #FF1E793F`（品牌绿）挂在 `PART_OuterBorder.BorderBrush` 的
`IsKeyboardFocused=True` 格子上；`AccentBrush` 是一个 **LinearGradientBrush**（S0-w/角批那支
`#1D733C..#2B804A` 渐变），选中行 `PART_RowBorder.Background` 实测就是它——**整行铺品牌绿渐变**。
第二条：行样式触发把 `row.Foreground` 写成 `TextOnAccent`（`#FFFFFFFF`，实测到了），但
`DataGridCell` 自己的样式格子写着 `Foreground = TextPrimary`，实测 `cell.Foreground=#FFF5F5F7`
不变——**选中行的"反色文字"到不了单元格**，渐变上盖的是常态字色。两条都是本批必须用我们自己的样式
压掉的缺陷，不是可以"照抄上游"的东西：上游没有 WinUI DataGrid 可抄（第 6 条）。
`AccentBrush` 那一行在 §G 里 `sameInstance=False` 不算反证——那处比的是未选中行的 `Transparent`，
真凭据是 §J 选中后的读数。**凡是"实例同一性"的断言都要在状态真的那一侧读。**

**5 · 部件名是契约，我们的模板不给那些名字就掉功能。** 换上一个只有 `Border + ItemsPresenter` 的模板之后：
行仍然生成（`DataGridRow` 2 个，各自带框架 `PART_RowBorder`/`PART_CellsPanel`），但
`DataGridCell=0`、列头整排消失、`PART_OuterBorder` 不存在、行宽从 466 涨到 480（`PART_RowHeaderCorner`
的 20 DIP 槽没了）。也就是说宿主模板负责列头带、滚动带、拖拽层与行头角，条目/单元格/列头各有自己的
框架样式可以单独覆盖（`DataGridRow`、`DataGridCell : ContentControl`、`DataGridColumnHeader : ButtonBase`、
`DataGridRowHeader : ButtonBase`、`DataGridDetailsPresenter : ContentPresenter`、
`DataGridCellsPresenter`/`DataGridRowsPresenter`/`DataGridColumnHeadersPresenter`）。
框架那份的几何基线（全部实测，非读码）：外框 `r=12,12,12,12` + `ClipToBounds=True`、
`bt=1,1,1,1`；行 30；列头带 34；单元格 padding `10,5,10,5`；列头 padding `10,7,6,7`；
`PART_ResizeGrip` 8x34 且 `IsHitTestVisible=False`；`PART_SortIndicator` 空串。
和 WinUI/ModernWpf 的距离（行 40、半径 4/8、列头 40）全部要写进差异表。`GridSplitter` 类型在
（`GridSplitter : Thumb`，6 个 DP 含 `ResizeBehavior/ResizeDirection/ShowsPreview`），
WinUI 侧的"列头里嵌 gripper"形状在这里是另一种做法——这条只登记，不在本批动。

**6 · 上游没有 WinUI DataGrid，本批的"照抄上游"没有对象。** 在只读引用树
`../microsoft-ui-xaml`（commit `19e3bdc3c`）里：`git ls-files | grep -ic datagrid` = **0**，
`git ls-tree -r HEAD | grep -ic datagrid` = 0，`controls/dev/` 无 DataGrid 目录，全 ref 历史按
datagrid 找新增只命中 TableView 的提交；33 处 "DataGrid" 文本命中全是 `AutomationControlType.DataGrid`
枚举、UIA 窗口类名和 TableView 设计稿里的提及。`TreeDataGrid` 在 microsoft-ui-xaml / ModernWpf /
UI.WPF.Modern / wpfui / uno / FluentAvalonia 六棵树里全为 0——它属于 CommunityToolkit.WinUI，
本机没有那棵树。上游真正的表格是 `controls/dev/TableView/`（dll-tabular，MUX_PREVIEW）：
`TableView_themeresources.xaml` 70 行、三分支各 12 条**纯度量无 brush**（度量与画笔分家的原因写在
该文件 11–19 行注释：合并会 duplicate-key 编译失败），画笔另在
`controls/dev/CommonStyles/TabularSurfaces_themeresources.xaml`（305 行 189 键，`TabularSurface*` 前缀），
`TableView.xaml` 393 行 15 个 `PART_` 部件、行状态组 `CommonStates` 8 格（含
`Selected/SelectedPointerOver/SelectedPressed/SelectedDisabled` 同组）。
**所以本批的样式与键权威是 ModernWpf 的 `ModernWpf/Styles/DataGrid.xaml`（831 行、53 键）——
它自己抄的是 dotnet/wpf 官方 Fluent，不是 WinUI**（`ModernWpf/docs/datagrid-wpf-fluent-source-audit.md`
82 行是这份差异的逐字说明，含它删掉的 WinUI 猜测层清单）。把这条写死，免得下一批又去找不存在的
`DataGrid_themeresources.xaml`。路线图第 221 行"列拖拽实时读 Accent"这句要按 §4 重读：
运行时确实把 `AccentBrush` 用在选中行上，但那是框架自己那份模板的行为，不是上游契约。

**7 · 我们的隐式样式不是"顶掉"框架那份，而是叠在上面——所以漏写的格子会留下框架值。** 这条本段先记成
"待补测"，`spike/DataGridProbe --mode pass3`（`s1i-datagrid-ownership-raw.txt`）量完直接结清：
`DataGrid.RowHeight/ColumnHeaderHeight/BorderThickness/GridLinesVisibility/RowBackground/`
`AlternatingRowBackground/Background/Foreground` 八个 DP 的 `DefaultValue` **全是 null**，
所以 30 / 34 / `1,1,1,1` / `All` 这些读数不可能是默认值兜出来的；把只有一个 `Template` 格子的
`Application.Resources[typeof(DataGrid)]` 装上去之后，这六条读数与"什么都没装"的框架基线**逐条相同**，
`Background`/`Foreground` 也仍然指向 `SurfaceBackground`/`TextPrimary` 那个实例。
结论两半：(a) 省格子不会掉外观，`Styles/DataGrid.jalxaml` 不必把 11 条重抄一遍；
(b) 反过来，**漏写也杀不掉任何东西**——品牌绿的 `AccentBrush` 选中行与
`ControlBorderFocused` 焦点边框不能靠"我们不写"来消除，只能靠换模板（pass 1 证明我们的模板真的落）
或改那两个 token 名。第二个办法有全局风险：`AccentBrush` 是框架 token 层，别的框架控件也读它，
在应用级字典里重定义等于给全仓改语义（S0-n/NumberBox 批那条"应用级名字撞车、最后写入者赢"的账）。
**本段决定：绿由模板与行/列头/单元格样式压，不动 `AccentBrush`。**
［本段这条决定在收尾时被推翻了一半，见下面第 9 条：行模板装不上，所以选中行的 `AccentBrush` 只能改名字；
焦点边框那条仍然成立，因为宿主模板是我们的。原句留着记账。］
另一条没量清的先挂着：`Application.Resources` 上 `ContainsKey(typeof(DataGrid))` 是 True、
索引器取回的 `Style` 与合并查找同一个对象，但 `Keys.Count` 是 **0**——两个访问器对同一本字典的说法不一致，
"框架那份样式到底住在应用字典里还是另一个作用域"这条**未结**；它只影响叙述，不影响上面的成本结论。

**8 · 判据本身要留一句：`Style` 属性不是读取点。** 本段三次读 `mounted.Style` 都是 null，而
`TryFindResource(typeof(DataGrid))` 明明白白返回一个 11 格的 `Style`——**"挂载后样式仍然不落在公开
`Style` 属性上"（S1-c 第 1 条）在表格族同样成立，且现在有了正向证据**：样式在合并查找里、在格子里、
在像素上，就是不在属性上。凡是"这个控件有没有默认样式"的问题，读 `TryFindResource(typeof(X))`
与挂载后的实际值，别读 `X.Style`。


**9 · 给容器换模板会把它孙辈的内容 visual 卡死——"能不能重模板"要按层回答，不能按控件回答。**
DataGrid 落地时表格一个字的像素都没有：`cell.Content` 是控制替绑定列生成的 `TextBlock`，
`ContentPresenter.Content` 也确实是它、`Visibility=Visible`，但 presenter `desired=0,0`，
而且从单元格往下走视觉树根本找不到那个 `TextBlock`；同一棵树不加载 Astra 字典时量到 `47.45x19.78`。
逐块撤样式的二分（每格一次构建一次读数，四份日志在 `s1i-datagrid-cells-raw.txt` /
`s1i-datagrid-content-raw.txt`，被拆散的原始样式留在 `spike/DataGridProbe/bisect-Styles-DataGrid.jalxaml`）：
只装**单元格**样式→文字正常；只装**行**样式→文字死；四块全不装、只留 token 字典→文字正常。
也就是说杀死文字的从来不是 token 层、不是单元格模板、也不是第 5 条那把部件名锁，而是**行的模板被换掉**：
行是控制往 `PART_CellsPanel` 里塞单元格的容器，换行的模板等于重建那批内容 visual 所在的子树，
而 26.10.9 的 `ContentPresenter` 没有"模板拆除时归还托管 visual"那一步（参考树里后来长出
`ReleaseContentElementForTemplateTeardown`，注释逐字描述的就是这个卡死：旧 presenter 仍持有该 visual 时，
新 presenter 渲染同一实例而它的 `VisualParent` 还指着退役的树，输入与布局失效停在那个断根上）。
**证据边界**：私有字段没读到，只量到行为一致——撤了就好、装了（只在行这一层）就坏。
所以三条规则给后面每一批：
1. **"这个控件能不能重模板"要拆成"它的哪一层能换"**。宿主/叶子（单元格、列头、行头）能换，
   中间那层容器（持有由控制自己生成的子视觉的行）不能换；判据不是类型链（S1-h 那条已经被本段第 1 条推翻），
   也不是"我们的模板落没落地"（落了，落地恰恰是破坏点）。
2. **凡是"内容 visual 由控制生成、再交给 presenter 托管"的控件（`DataGridCell`、`DataGridColumnHeader` 这类
   `ContentControl` 容器），换它的父级模板之后必须量一次孙辈文字的实际尺寸**，别量 presenter 的 `Content`——
   `Content` 非空、`Visibility=Visible`、样式全部命中，文字可以照样是零。树里找不到那个 `TextBlock` 才是信号。
3. **父级模板不能换，它读的资源名就成了唯一的入口。**本段因此改判：新增
   `ThemeResources/FrameworkRetints.jalxaml` 把框架的 `AccentBrush` 别名到 `AccentFillColorDefaultBrush`
   （别名而不是重定义：同一个 brush 实例，`ApplyAccent`/`OverrideBrush` 才推得动），作用域实测是框架字典里
   58 处读这个名字、目前全部铺品牌绿。这一层是"撞名重着色"这条已有账的正面用法，代价是选中行拿到的是
   强调色不透明底，上游 `ListAccentLowOpacity` 0.4 那层丢了——x:Double 发不出去，行模板又装不上，
   没有第三条路。这个字典故意不进"每行都要有消费者"的资源键闸口：它的读者是框架模板，
   要求我们的样式引用它只会逼人写一条假引用（与 `TitleBar.jalxaml`/`FlyoutPresenter.jalxaml` 同族豁免）。

## S1-j：markup 里两种"写了没生效"——`DataGridLength` 不吃裸数字、未知属性名静默丢弃（阶段 5 第六段，2026-09-20）

`spike/DataGridProbe` mode `cols`，七种配置一次跑完（`s1j-treedatagrid-columns-raw.txt`），加上 mode `feed` 的
两条对照（`s1j-treedatagrid-feed-raw.txt`）。这一段和主题能力没有直接关系，它打的是"markup 写得对不对"这条闸口，
因为两种静默失败都没有任何报错，只能靠读回值。

1. **`DataGridColumn.Width` 是 `DataGridLength`，markup 的裸数字不转换，静默停在 `Auto`。**
   `<DataGridTextColumn Width='200'/>` 解析成功、构建成功、挂载成功，读回 `Width=Auto`、`MinWidth=20`（默认）。
   同一列改写成 `MinWidth='200'` 就读回 200。`Width` 只有从代码里赋（`DataGridLength` 构造）才落。
   后果分两种网格：`DataGrid` 把 `Auto` 摊成实宽（120），`TreeDataGrid` 把 `Auto` 量成 **0**，
   于是树上单元格 `ActualWidth=0`、文字有 `desired` 没宽度——看得见控件看不见字。
   Gallery 的表格从上线第一天起就在走这条静默路径（三条列宽全是 `Auto`），本批改成 `MinWidth` 并钉了
   `A_column_width_written_in_markup_is_silently_auto_while_min_width_lands`（DataGrid 侧）与
   `A_tree_column_at_auto_lays_out_at_zero_width_while_min_width_reaches_the_cells`（树侧）。
2. **未知属性名同样静默**：`<TreeDataGrid DefinitelyNotAProperty='42'/>` 解析通过、控件建成、上屏 460x60。
   所以 Gallery 树 markup 里那句 `AutoGenerateColumns='False'` 一直没起作用——`TreeDataGrid` 公开声明的 26 个名字
   里根本没有它（`TreeDataGrid : Control`，不是 `ItemsControl`；它也没有滚动条可见性属性，宿主模板里那两处只能写常量）。
3. **列宽要在首次度量之前定**：挂载之后再改 `Width`，列自己读到 200，单元格的本地宽度仍是旧布局那一版
   （120 / 0）。这条和 S0/S1 的"本地值优先于格子"是同一堵墙的不同层——控制自己写在本地的东西，事后改属性不重排。

给后面每一批的规则：**markup 里凡属性类型不是 `double`/`string`/枚举（长度类、`GridLength` 类、`DataGridLength` 类、
`Duration` 类），必须读回那个值确认它落下了**，不能只看解析没抛；能构建不等于写进去了。这与 `{x:Bind}` 静默丢弃、
`{ThemeResource}` 空字符串条件（S0-g→AutoSuggestBox 批下调）同族：**判据是挂在树上的读回值，不是属性的名字**。
现存 markup 闸口（`AstraGateTests`）只查键消费与格子目标，查不了属性名是否存在、也查不了属性类型能不能吃裸数字；
这条记在闸口欠账里。

同段还结掉一条挡路的旧推论：`TreeDataGridNode` 是 `internal`（sealed，消费者不可见），但**喂数据不需要点名它**——
`ItemsSource` 收普通 `IEnumerable`、层级走 `ChildrenPropertyPath`，`ExpandAll/CollapseAll/IsExpanded(int)/FlattenedCount`
全公开，所以展开闭环可以像 ToggleButton 批用 automation peer 那样无 OS 输入地驱动。上一段"节点内部类型 ⇒ 数据喂不进去"
是从一个真读数多跨了一级；判"控件能不能落地"先把**公版程序集**的成员表拉出来。行的处境仍然硬：
`TreeDataGridRow` 只有 `IsSelected` 公开，且每行 `Background` 都是控件写上去的本地值（偶 `#00FFFFFF`、奇 `#0FFFFFFF`），
所以样式格子在那一层永远赢不了——本批只出宿主模板，行保留框架模板（S1-i 第 9 条的逐层判据在第二个控件上复现）。

顺带清掉一条已上线的可见缺陷：这条运行时会把**行数据本身**塞进 `DataGridRowHeader.Content`，
而我们上一批的行头模板有个内容呈现器，于是 20 DIP 的行头槽里画的是模型的 `ToString()`。
修法不是加裁剪而是**不呈现内容**（上游 WPF Fluent 的行头也只画"当前行"记号，而本运行时不暴露那个读数），
`DataGridRowHeaderForeground` 随之失去消费者、撤出别名表并进"不发布"闸口；
Gallery 的表格另加 `HeadersVisibility='Column'`。回归：`The_row_header_carries_the_item_but_draws_no_text_of_its_own`。

## S1-l：条目管线是可继承的基面——`ItemsControl` 的覆写面是 protected、模板与面板都能换（阶段 5 收尾段选型，2026-09-20）

`spike/ItemHostProbe`（mode `census`/`api`/`mount`/`panel`/`capture`，读数转录在 `s1l-itemhost-raw.txt`）。
这一段是阶段 5 收尾批（GridView 判断 + BreadcrumbBar/RadioButtons/PipsPager）的选型输入，
同时**撤回了 `adaptation/09` 的一条纪律**。

1. **换我们的模板：两条路都通**。把 `Border #ProbeHostRoot > ItemsPresenter #ProbeItems` 的
   `ControlTemplate` 交给 `ItemsControl`——本地值一次、`Style` 的 `Template` setter 一次——
   两次都在 10 帧内读到同一棵树：`Border > ItemsPresenter > StackPanel > ContentPresenter×3 > TextBlock×3`，
   每项 `420x19.78`。也就是说条目宿主这一层和 ListBox/TabView 那些层的区别只在"框架有没有默认模板"，
   不在"能不能被重模板"（S1-i 第 9 条的逐层判据在这里第三次复现）。
2. **容器生成可以继承**：`GetContainerForItemOverride()`、`IsItemItsOwnContainerOverride(Object)`、
   `PrepareContainerForItemOverride(DependencyObject, Object)`、`ClearContainerForItemOverride(...)`、
   `GetContainerForItem(Object)` 反射读出的属性位全是 **protected/virtual**，签名与 WPF 一致。
   `ItemContainerStyle`/`ItemTemplate`/`ItemsPanel`/`AlternationCount` 等是 public。
   → 自造条目宿主不必再手写"把子元素塞进命名面板"。
3. **面板能换，版式就不用自造**：`ItemsPanel` 赋 `ItemsPanelTemplate` 后 8 帧读到
   `WrapPanel` → 条目按内容宽换行（每项 `68.36x40.78`）、`UniformGrid Columns=4` → 每项正好 `105x80`
   （420/4、160/2）、横向 `StackPanel` → `68.36x160`。换行/网格这类版式是面板赋值，不是控件类型。
4. **运行时的 `GridView` 不是 WinUI 的 `GridView`**：它是 `GridView : ViewBase`，同族 14 个导出类型全是
   WPF 的视图装饰器（`GridViewColumn`/`GridViewColumnHeader`/`GridViewRowPresenter(Base)`+peers），
   `GridViewRow`、`GridViewItem` 各 0 命中；`ListView` 声明的唯一 DP 就是 `View`。
   `BreadcrumbBar`/`PipsPager`/`RadioButtons`/`ItemsRepeater`/`CardAction` 五个名字 0 命中。
5. **撤回**：`adaptation/09` 说裸 `ItemsControl`（`Template` 为 null）会"让 UI 线程 60 秒不回应"，
   机制读成"框架走进了不产出帧的路径"，并据此禁止自造控件继承 `ItemsControl`。本轮同一形状挂载
   **4 帧返回**，且树里实实在在有回退条目宿主生成的 3 个容器（属性面也读得到 `UsesFallbackItemsHost`、
   `ItemsHostInternal`）；`RenderTargetBitmap.Render` 计时 `ListBox` 0.03s、裸 `ItemsControl` 0.00s，
   捕获侧也不是成本。剩下的解释就是 `PixelHarness.cs:259-268` 自己记下的那条 harness 缺陷：
   静态场景不发 `CompositionTarget.Rendering`，而当年的看门狗把释放排到了没人泵的线程池 dispatcher 上。
   **那是 harness 的 bug，不是 `ItemsControl` 的性质**；纪律撤销，原始三处读数与强度写在 `adaptation/09`。

## S1-m：一个"只有字形"的视觉在这个运行时量不到墨——PipsPager 的四类静默退化（阶段 5 第九段，2026-09-20）

`spike/PipsPagerProbe`（mode `census`/`api`/`cmap`/`chrome`/`mount`/`ink`，读数转录在 `s1m-pips-pager-raw.txt`），
外加 `spike/SymbolCmap/check.py`（fontTools 直读 cmap 与 glyph 边界框）。这一段是 PipsPager 的选型输入，
但它量出的四条 markup/渲染性质是跨控件的，所以写在这里。

1. **"字形画不出墨"是可测量的，不是猜测**。`FontIcon` 用 `Symbol` 码点 `EA3B`（上游 `PipsPager` 正常点的那一枚）：
   cmap **命中**（1 条轮廓、边界框 0.938x0.938 em，即一枚实心圆）、容器布局框正确（24 字号量到 26x26、6 字号 6x6），
   但逐格数墨全是 0；同一运行时里参照物——用 `Ellipse` 画出来的 6 DIP 圆——数得到 60 个像素。**字体路径上的
   glyph 在 RTB 捕获里不产像素**，而它的布局框、cmap、控件树一切都像画上了。所以本层的点不是 `FontIcon`，
   是按测到的墨径（0.938 x 6 ＝ 5.6、0.938 x 4 ＝ 3.8）画的两个 `Ellipse`。
   这条直接决定阶段 6 的 `SymbolIcon`/`FontIcon` 批：判"码点命中"要用 `check.py` 读 cmap，判"画出来了"要另找判据。
2. **`<ControlTemplate.Triggers>` 必须是 `ControlTemplate` 的直接子元素**。写进模板根元素的内部，解析期抛
   `Cannot find attached property setter for: ControlTemplate.Triggers`；三份模板在第一轮绿之前各撞一次。
3. **`Style` 的 setter 够不到模板部件**：`<Setter TargetName="SelectedDot">` 静默不生效（点仍是基础样式的 3.8），
   而同一处写入放进 `ControlTemplate.Triggers` 就成了——按钮焦点框一直是这么画的。选中/正常因此靠
   pip 的 `Tag` 由模板触发器切两个已画点的 `Visibility`，样式只带刷子。
4. **一个 `Border` 里两个平铺的 `Ellipse` 会让命名部件整个消失**：`Named(pip,"NormalDot")` 返回 null，
   同模板的 `#RootGrid` 却解析得到，且没有解析错误也没有警告。包一层 `Grid` 之后两点都找得到。
5. **用自己的度量结果去限自己的可视区，会把这条回路饿死**：`MaxVisiblePips` 的第一版按已生成容器的
   `DesiredSize` 算钳制宽度，于是 `UpdateViewport` 在任何人度量之前跑了一次，而本该重跑它的 `SizeChanged`
   再也没来——因为宿主宽度正是被钳制的那个量。读数：两枚点时候口仍是 12，第二枚点零墨。
   改成用控件自己写在容器上的脚印常量。
6. **半透明令牌在 harness 的黑色背衬上等于没画**：Light 下点色是 `#5E000000`，打包 RGB 与背衬同键，
   `PixelKey(brush)` 在 9600 像素的样本里数到 105966，加一枚点读数不变。判墨量因此改成**白底卡片 + 两次捕获的
   直方图差值**（选中点 19 px，加点第二枚再涨），而不是数某个色键。同段另一条：同一窗口里宿主第二个元素时，
   第一个仍在捕获前方——比较只能是同一个被宿主元素跨三次页面数的差值。
7. **写回同一个值的 DP 回调仍会触发，且 `OldValue` 是那个被拒的值**——第一版钳制因此对第二次越界写
   发了两次 `SelectedIndexChanged`。加上 `ButtonAutomationPeer.Invoke()` 对禁用按钮抛
   `InvalidOperationException`，边界禁用这条规则变成可断言的，而不只是读 `IsEnabled`。

选型上这一段也是对上一段的**反向使用**：`PipsPager` 上游没有 `ItemsSource`、条目纯粹由页面数派生
（`PipsPager.cpp` 的 element factory 只从 `TemplateSettings.PipsPagerItems` 取 1-based 页号），
继承 `ItemsControl` 会把 `Items`/`ItemContainerStyle` 这一整面公开 API 白递给应用，所以本层落回 `Control`
+ 自管 `Panel` 子元素——S1-l 量出的是"能继承"，不是"该继承"。

## S1-n：`ContentControl` 派生的自有类型不主动展开模板，而被收进的条目容器会整个离开 `Children`（阶段 5 第十段，2026-09-21）

`spike/BreadcrumbProbe`（mode `api`/`layout`/`ink`/`hidden`/`style`/`narrow`，读数转录在
`s1n-breadcrumb-upstream-raw.txt` [G1]-[G7]）。这一段是 BreadcrumbBar 的选型与两处返工输入，量出的五条性质跨控件。

1. **零矩形 arrange 在本运行时不是"藏起来"**（[G4]）。上游 `BreadcrumbLayout.cpp:89-93` 用
   `Arrange(new Rect(0,0,0,0))` 收条目，从不碰 `Visibility`。四枚自带 `Width=100 Height=40` 的 `Border` 实测：
   被"收进"的两枚仍是 `100x40`，只是叠回原点，**照旧画笔、照旧命中**（直方图黄 4000 + 绿 4000，蓝 0 因为被压住）；
   不带自身宽度的 `TextBlock` 才会变成 `0x0`。所以照抄上游会得到一条"隐藏条目挤在第一个可见条目底下、只有读
   `ActualWidth` 的人看不见"的行。`Visibility` 是这里唯一量得出"没了"的机制（[G5]：红 4000 绿 4000，蓝/黄 0）。
2. **"能读到值"不等于"样式落到了控件上"**（[G6]）。`Style` 找得到、隐式键找得到、`Template` 已是
   `ControlTemplate`、`LoadContent()` 能展开出三部件——而落地控件的唯一视觉子元素是一枚裸 `TextBlock`。
   `ContentControl` 派生的自有类型必须调 `UseTemplateContentManagement()` 并设 `DefaultStyleKey`，本层四个
   `ContentControl` 派生类型全都这么做。顺带推翻一种写法：靠"构造时 `Style` 还是 null"来判定"没人选过样式"
   的兜底，对这类类型**永远不成立**——那个槽位一开始就坐着框架自己的 `ContentControl` 默认模板。
3. **`Collapsed` 的条目容器会从 `Panel.Children` 里消失**（[G7a]）。实测收掉四枚中的第一枚之后，下一趟测量
   读到的宽度和从 307 掉到 225（只剩三枚），于是"放不下"翻成"放得下"，行不再声明溢出而那一枚仍没被摆位。
   `Hidden` 保留子元素与 `DesiredSize`，由 arrange 自己拒绝给位置——墨量结论不变（捕获里 0 像素）。
4. **溢出判据要用宿主的宽度，不能用自己那一列的宽度**（[G7b]）。省略号在左侧列里，显示它会让本列变窄；
   用 `finalSize.Width` 判定就是"答案改变判据、判据再改答案"。改用宿主条宽（应用给的那个数）后判定与结果解耦，
   显示省略号只需补一趟 measure 让预留量取到真数。S1-m 第 5 条是同一类错误的镜像：用自己的度量结果限自己。
5. **`ItemContainerGenerator` 一面都没有**（[G2]）。`ItemsControl.ContainerFromItem` 不存在，所以条目到容器
   的反查只能由控件自己记账（`FluentRadioButtons`/`FluentPipsPager`/`FluentBreadcrumbBar` 三处同一条），
   测试与自动化也只能走这个清单。这条是"面板 vs 宿主"分工的实际边界：面板能决定可见集合，宿主才能知道集合是谁。
