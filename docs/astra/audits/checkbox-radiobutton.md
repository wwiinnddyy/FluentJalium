# 审计：CheckBox / RadioButton（选择批第一段：别名层落地 + 模板接键）

- 上游：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
  - `controls/dev/CommonStyles/CheckBox_themeresources.xaml`，blob `71e21d32b764c8b7fc6fe7312e7a6847a0fb5c89`
  - `controls/dev/CommonStyles/RadioButton_themeresources.xaml`，blob `223a28385186f36a4629d354928e5fb705ab10b3`
  - `controls/dev/RadioButtons/RadioButton.xaml` 在这个 commit 里只有 3 行（`BasedOn` 转发），
    真正的 `DefaultRadioButtonStyle` 与 `DefaultCheckBoxStyle` 一样住在上面的 `*_themeresources.xaml` 里。
- 分级：**原生控件 + 我们的重模板**；颜色层是上游别名层的逐字转录。
- 出口口径：这一步只完成九步里的 1-4 与 6（审计、键、模板、状态映射、像素）。
  硬件输入（步 5）与 Gallery 上屏（步 7）没有做，写在文末 Known Gaps 里，不用相邻证据顶替。

## 转录规模

| 控件 | 别名行 | 逐字 | 替换 | 额外度量行 | 模板消费点 |
|---|---|---|---|---|---|
| CheckBox | 72 | 72 | 0 | `CheckBoxPadding`、`CheckBoxBorderThickness` | 12 格 × 6 槽 |
| RadioButton | 40 | 31 | 9 | `RadioButtonBorderThemeThickness` | 3 setter + 4 本体 + 7 格 |

72 = 6 族（`Foreground`/`Background`/`BorderBrush`/`CheckBackgroundFill`/`CheckBackgroundStroke`/
`CheckGlyphForeground`）× 12（三态 × 四态）。CheckBox 一侧**没有任何替换**：它要的 72 个目标
在本调色板里全部存在。RadioButton 一侧 9 处替换全部是"海拔渐变边框"族，见下。

替换的 9 行（每行在文件里就地写明理由，不集中到本文档冒充证据）：

- `RadioButtonCheckGlyphStroke{,PointerOver,Pressed,Disabled}` ← 上游 `CircleElevationBorderBrush`
  （一个从 `ControlStrokeColorDefault` 到 `ControlStrokeColorSecondary` 的包围盒渐变）→ 本调色板无渐变行，
  改指 `ControlStrokeColorDefaultBrush`。
- `RadioButtonCheckGlyphStroke{Checked,CheckedPointerOver,CheckedPressed}` ← `AccentControlElevationBorderBrush`
  → `ControlStrokeColorOnAccentDefaultBrush`。
- `RadioButtonCheckGlyphStrokeCheckedDisabled` ← `ControlElevationBorderBrush`（3 DIP 绝对渐变）→ `ControlStrokeColorDefaultBrush`。
- `RadioButtonCheckGlyphFillDisabled` ← 上游指向 `TextOnAccentFillColorPrimary`（一个 **Color**，不是 Brush），
  本运行时没有实测过的 Color→Brush 强制转换，改指同名 Brush 行。

## 结构偏差（不是配色，是形状）

上游 CheckBox 的勾选标记是 `controls:AnimatedIcon` + `AnimatedAcceptVisualSource`，
`FallbackIconSource` 是一个 `FontIconSource`（`CheckBoxCheckedGlyph` = `&#xE73E;`）。
本运行时既没有 `AnimatedIcon`（Button 审计已记录：26.10.9 的 61 个 dll 查不到该名字），
`SymbolThemeFontFamily` 的码点又已知成片渲染成方框（`adaptation` 里的图标字体缺陷），
所以标记用几何：`Path`（对勾）+ `Border`（混合条）。**代价**：`CheckBoxCheckedGlyph` /
`CheckBoxIndeterminateGlyph` 两行 `x:String` 因此没有转录对象，`CheckGlyph.Foreground` 一个上游槽
在我们这里落成 `Path.Stroke` + `Border.Background` 两个 setter。

上游 RadioButton 把环拆成 `OuterEllipse` + `CheckOuterEllipse`、点拆成 `CheckGlyph` + `PressedCheckGlyph`，
它自己的注释写明理由：**两个正交状态组不能碰同一个属性**。我们的 `ControlTemplate.Triggers`
是一条按优先级排列的单一列表，一个环 + 一个点就能覆盖 8 格，第二对椭圆在这里是死部件。
不搬。但 `PressedCheckGlyph` 承担的按压挤压动画我们确实没有（见 Gaps）。

三处一致的自由度偏差：焦点框用模板自绘 `Border`（系统焦点框没有可写属性），
因此 `UseSystemFocusVisuals` / `FocusVisualMargin` 两个 setter 不上，
`CheckBoxFocusVisualMargin` 这行也**不转录**——声明一个没有属性可读的键只是看起来能移植。
`CornerRadius`（上游挂在 RootGrid 上）同理不上：我们的根没有可见边框。
`FontFamily="{ThemeResource ContentControlThemeFontFamily}"` 与
`FontSize="{ThemeResource ControlContentThemeFontSize}"` 两行本主题清单里没有对应键
（闸口"每个被引用的键都必须声明"会直接拒），所以字号仍是 `14` 字面量。

度量行的处置：`CheckBoxBorderThickness` / `RadioButtonBorderThemeThickness` 上游是 `x:Double`
（喂给 `Rectangle.StrokeThickness` / `Ellipse.StrokeThickness`），本读者解析不了 `x:Double` 资源，
同一个 1 改走 `Thickness` 喂给 `Border.BorderThickness`。`CheckBoxSize` 20、`CheckBoxGlyphSize` 12、
`CheckBoxHeight` 32、`RadioButtonCheckGlyphSize` 12/14/10 留在模板字面量里。
`CheckBoxPadding` 与 `MinWidth` 120 这次真的接上了（见下）。

## 状态映射表

CheckBox 上游只有一个 `CombinedStates` 组、12 个状态，每格写同样 6 个槽——这正是我们
逐格展开的原因：本运行时的 `MultiTrigger` 不继承基格，漏一格那一格就退回本体值。

| 上游状态 | 我们的触发器 | 槽（6 族，每格逐个断言） |
|---|---|---|
| `UncheckedNormal` | 样式本体 + 部件本体（无触发器） | `*Unchecked` |
| `UncheckedPointerOver` | `Multi[IsChecked=False, IsMouseOver=True]` | `*UncheckedPointerOver` |
| `UncheckedPressed` | `Multi[IsChecked=False, IsPressed=True]` | `*UncheckedPressed` |
| `CheckedNormal` | `Trigger IsChecked=True` | `*Checked` |
| `CheckedPointerOver` / `CheckedPressed` / `CheckedDisabled` | `Multi[IsChecked=True, {IsMouseOver,IsPressed,IsEnabled=False}]` | `*Checked{PointerOver,Pressed,Disabled}` |
| `Indeterminate{Normal,PointerOver,Pressed,Disabled}` | `Trigger`/`Multi[IsChecked={x:Null}, …]` | `*Indeterminate{,PointerOver,Pressed,Disabled}` |
| 焦点框 | `Trigger IsKeyboardFocused=True` → `CheckFocus.Opacity` | `FocusStrokeColorOuterBrush` |

RadioButton 上游是两个正交组：`CommonStates`（Normal/PointerOver/Pressed/Disabled，写环 + 点 + 根 +
标签，并动画 `CheckGlyph.Width/Height` 12→14→10→14）与 `CheckStates`（Checked 做 4 个部件的
opacity 交叉淡入并把点描边换成 `*StrokeChecked`，Unchecked/Indeterminate 为空）。
我们摊平成 7 格（本体承担 UncheckedNormal）：

| 上游 (组, 状态) | 我们的触发器 | 环 | 点 |
|---|---|---|---|
| (Common, Normal) | 部件本体 | `OuterEllipse{Fill,Stroke}` | `CheckGlyph{Fill,Stroke}` |
| (Common, PointerOver) | `Multi[IsChecked=False, IsMouseOver=True]` | `…Fill/StrokePointerOver` | `CheckGlyph{Fill,Stroke}PointerOver` + 14 |
| (Common, Pressed) | `Multi[IsChecked=False, IsPressed=True]` | `…Pressed` | `…Pressed` + 10 |
| (Common, Disabled) | `Trigger IsEnabled=False` | `…Disabled` | `…Disabled` + 14 |
| (Check, Checked) | `Trigger IsChecked=True` | `OuterEllipseChecked{Fill,Stroke}` | `CheckGlyphFill` + `CheckGlyphStrokeChecked` + 12 |
| (Common×Check) | `Multi[IsChecked=True, {IsMouseOver,IsPressed}]` | `OuterEllipseChecked…{PointerOver,Pressed}` | `CheckGlyphFill…` + `CheckGlyphStrokeChecked…` |
| (Checked + Disabled) | `Multi[IsChecked=True, IsEnabled=False]` | `OuterEllipseCheckedFill/StrokeDisabled` | `CheckGlyphFillDisabled` + `CheckGlyphStrokeCheckedDisabled` |

优先级按"更具体的格排在更后面"写死：`Checked` 在 `Unchecked*` 之后、`*Disabled` 在
`*PointerOver/Pressed` 之后，所以禁用 + 悬停落在禁用格。**每格都写全 7 个属性**（不依赖基格继承）。
格的顺序同时被断言：`AssertCell` 找不到格时会把该格实际带的 setter 列表打印出来。

## 这一批新建立的一条框架事实（对后面每一批都管用）

模板 setter 的目标属性有**两种解析形态**，只读 `Setter.Property` 会把第二种当成"无名 setter"：

- 属性在样式 `TargetType` 上存在（`Background`、`BorderBrush`、`Foreground`、`Width`）→ 解析期就落成
  `DependencyProperty`，`Setter.Property` 非空；
- 属性只在被点名的部件上存在（`Shape.Fill`、`Shape.Stroke`）→ 解析期存成 `Setter.PropertyName`
  字符串，应用模板时再对着真实部件类型解析。

原始读数（第一次断言 7 槽时 `Fill`/`Stroke` 报"没有 setter"，打印格内实际内容）：

```
unchecked + pointer over: no RadioDot.Fill setter; the cell carries
  RadioLabel.Foreground, RadioRoot.Background, RadioRoot.BorderBrush,
  RadioRing.Background, RadioRing.BorderBrush, RadioDot., RadioDot.,
  RadioDot.Width, RadioDot.Height
```

两个 `RadioDot.` 就是 `PropertyName` 形态。运行时是**生效**的：
`A_checked_radio_button_drops_the_accent_ring_behind_the_dot` 断到 `dot.Stroke` 等于
`ControlStrokeColorOnAccentDefaultBrush`（替换目标），而不是本体的 `ControlStrongStrokeColorDefaultBrush`。
测试侧统一走 `NameOf(setter) = setter.Property?.Name ?? setter.PropertyName`。
这条不是我们的技巧，是 Jalium 自己的 `Setter.PropertyName` 契约（`Style.cs`，注释原文：
"the property name is stored here and resolved at runtime against the actual target element type"）。

## 顺带对齐的两个度量

上游 CheckBox/RadioButton 都是 `MinWidth=120`、`Padding=8,5,0,0` / `8,6,0,0` +
`VerticalContentAlignment=Top`（图标列 20 宽，RadioButton 那一列再包一层 `Height=32` 顶对齐）。
我们原先是 `MinWidth=0`、`Padding=0` + 标签 `Margin=8,0,0,0` 居中。
参考项目 `LanStartWrite.inkcanvas` 的 Fluent 主题同样带 0 宽/居中——那不是上游形状，
所以这次按 WinUI 改：`MinWidth=120` 写进两个样式，标签间距改走 `{TemplateBinding Padding}`，
CheckBox 的 `Padding` 由 `CheckBoxPadding` 行驱动。

## 四类证据

1. **构建/闸口**：`tools/Test-AstraGates.ps1` 串行全绿 —— **82 通过 / 0 失败 / 0 跳过 / 0 警告**，
   调色板漂移三侧 `checked=True`（Light/Dark 各 83 源色 102 brush；HC 仍扣住 3 个海拔渐变键，正是上面替换的那一族）。
2. **结构闸口新增**：`Transcribed_control_rows_are_read_by_a_template`（Theory，已列入 CheckBox 与
   RadioButton）——逐行反查"声明的别名行有没有模板真的读它"。两份文件里 72+40 行全部有消费点，
   与 `Every_referenced_key_is_declared` 合起来构成双向集合相等。后面每一批把新文件加进 Theory 即可。
3. **行为**：`AstraSelectionTests` 15 项全绿（本段新增 8 项）——7 个触发格的键名矩阵（每格 7 槽 +
   点尺寸 + 只有 Checked 格允许露点）与本体格一起覆盖 8 格、
   Unchecked 本体落 `ControlAltFillColorSecondary`/`ControlStrongStrokeColorDefault`、
   Checked 环落 `AccentFillColorDefault` 且点描边落替换目标、Disabled+Checked 保留点并退到
   `AccentFillColorDisabled`，外加两个控件的 `MinWidth=120`，以及RadioButton 的互斥：
   共用一个父面板即成组，后选的把先选的放开，不需要有人显式清（`A_shared_parent_panel_is_the_radio_group`）。
   三态与 `{x:Null}` 的旧断言（步 11 建的）一并复核：格矩阵化后 `IsMouseOver` 只出现在
   `MultiTrigger` 条件里，断言随之改形（不是放宽——仍然要求值的类型是 `Boolean`）。
4. **视觉**：`A_checked_radio_button_paints_the_accent_ring_to_pixels` —— 覆盖
   `AccentFillColorDefaultBrush` 为品红哨兵后离屏渲染 32×32，环（20 DIP 去 12 DIP 点）命中 >150 px，
   并先断 `sample.Stable`。CheckBox 一侧沿用已有的三态像素断言。

## 更正：目录里那条"勾选标记不在捕获路径里"是错的

`Catalog.json` 的 CheckBox 一直挂着 `the check glyph is not in the capture path`。
本次把表面令牌与标记令牌分别染成两个哨兵色再拍一次：同一张 32×32 直方图里
标记贡献 12 个实心品红像素 + 22 个"品红压在橙色上"的混合像素（1.5 DIP 描边在 175% 缩放下基本全是抗锯齿），
表面贡献 >150 个橙色像素。标记**确实**进了像素，那条 gap 已删。
顺带量到两条 harness 约束：一次 `_fixture.Run` 里连拍两次（第一张还是"只有 4% 黑在半透明填充下"
的近黑画面）会顶穿 fixture 的 60 秒看门狗；带文字的 subject 每张要几十秒且不落字。
所以这条断言是"单次捕获、无标题、两种哨兵色"，负控（未勾选时不该出现品红）没有做，
写成 Gaps 而不是拿别的证据顶。

## Known Gaps（不许用相邻证据替代）

- **勾形的像素证据只到"出现"**：没有做"未勾选时该哨兵色一个像素都不出现"的负控——那次连拍两次的
  尝试顶穿了 60 秒看门狗（见上），所以正负两半目前分属两次测量，负的一半没做。
- **没有真指针输入证据**：8 格里 `PointerOver`/`Pressed` 目前只有键名与结构断言，
  没有 SendInput 真 hover/press 落到像素（任务 #13）。触摸路径未测，键盘 Space/方向键未测。
- **Gallery 选择页未上屏**：`MinWidth` 120 与 `Padding` 驱动的间距改变只由离屏像素与结构断言覆盖，
  没在真实窗口里量过（E4 未开工，Gallery 也没有 `--page` 之类的启动参数可以定点导航）。
- **按压动画缺失**：`PressedCheckGlyph` 的 4→10/14 挤压与 `OuterEllipse`/`CheckOuterEllipse`
  的 opacity 交叉淡入未移植；点尺寸只在格上离散换值 + `TransitionProperty="Width, Height"`（167ms 字面量，B5 待键化）。
- **勾选标记不是 AnimatedIcon**：进框那一下的逐帧动画不存在；`CheckBoxCheckedGlyph` /
  `CheckBoxIndeterminateGlyph` / `CheckBoxFocusVisualMargin` 三行未声明（本运行时无对应可读属性）。
- **海拔渐变键仍未点亮**：3 个 `*ElevationBorderBrush` 在 HC 与 Light/Dark 都无行，
  9 处替换是"最近的实心令牌"，不是等价物；上屏逐位一致不声称。
- **高对比逐键断言未做**：别名行走的是同一 palette 实例，但 HC 下这 112 行的逐键值还没断言（任务 #11/#12 收尾项）。
