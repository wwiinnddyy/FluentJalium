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

## 阶段 1 因此要做的事

1. `FluentThemeManager` 的切换入口收敛为 `Application.ThemeMode`；删掉
   `ApplyMotionPolicy` 整树递归、`window.InvalidateVisual()`、
   `ApplyHighContrastPalette` 的键名启发式（换成逐键映射表）。
2. 调色板改为真正的 `ThemeDictionaries`（Light/Dark 两套 + HC 由映射表覆盖），
   样式里的可变令牌从 `{StaticResource …Brush}` 改为 `{ThemeResource …}`。
3. 强调色：`ThemeManager.SystemAccentResolver` 有 public setter，可以把 DWM 强调色接进来；
   `ThemeManager.ApplyAccent(Color)` 是 public。两者都待阶段 1 实测是否驱动我们的键。
4. 数值令牌保持字面量 + `.parts.md` 对照；键闸口要能查出被消费但不存在的键。
5. 阶段 1 补测：`ThemeMode` 翻转后刷子实例标识是否保持（S0-a 的空白）。
