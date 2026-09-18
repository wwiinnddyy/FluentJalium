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
| S0-f `ThemeColors` 可否桥接 | **71 个 public 静态 `Color`，零 public setter**；四种公开写入口全部无效 | 天花板只限读这张表的自绘代码，见 `01-jalium-control-census.md` |

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
