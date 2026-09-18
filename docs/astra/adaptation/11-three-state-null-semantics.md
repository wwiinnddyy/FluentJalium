# 11 · 三态与 `{x:Null}`：一次量清，两侧都改

选择批的第一问不是颜色，而是一个卡着两个控件的语义问题：**本运行时认不认
`Trigger Property="IsChecked" Value="{x:Null}"`**。上游 WinUI 的三态就是这么写的；
如果这条被静默丢掉（JALXAML 对不认识的标记扩展是"不报错也不生效"，见 `adaptation/00`），
那么 `ToggleButton` 那 12 条 `*Indeterminate*` 键永远没有消费点，Gallery 里那个"三态选项"
也只是一个看起来正确的复选框。

## 原始读数

一次性的进程内诊断（跑完即删，读数已转成 `AstraSelectionTests` 的断言）：

```
TRIGGERS: trigger IsMouseOver=Boolean:True | trigger IsPressed=Boolean:True
        | trigger IsChecked=String:True | trigger IsChecked=<null>
        | multi[IsChecked=String:True,IsMouseOver=Boolean:True]
        | multi[IsChecked=String:True,IsPressed=Boolean:True]
        | trigger IsKeyboardFocused=Boolean:True | trigger IsEnabled=Boolean:False
        | multi[IsChecked=String:True,IsEnabled=Boolean:False]
        | multi[IsChecked=<null>,IsEnabled=Boolean:False]
ISCHECKED = Boolean:False / <null> / Boolean:True
MIXEDGLYPH: Border.Opacity=1
SURFACE: #0078D4   accent(AliasAccentFillColorDefaultBrush, Light) = #0078D4
MARKUP: parsed IsChecked=Boolean:False threeState=True
```

## 四条结论

1. **`{x:Null}` 作为触发器条件是有效的**：值被存成真正的 `null`（不是字符串），
   且当 `IsChecked == null` 时命中 —— 混合条 `MixedGlyph.Opacity` 变成 1、
   `CheckSurface.Background` 就是调色板那个 `AccentFillColorDefaultBrush` 实例。
2. **条件值的存法按属性类型分岔**：可空 `bool?` 的 `IsChecked` 把 markup 的 `True` 存成
   **字符串** `"True"`、把 `{x:Null}` 存成 **null**；而 `IsMouseOver`/`IsPressed`/`IsEnabled`
   这些普通 `bool` 存成 **`Boolean`**。运行时两边都能命中（上面的读数就是证据），
   但这意味着"按类型比较条件值"的断言写法是错的 —— 这条形状现在被
   `Exactly_one_template_trigger_matches_a_null_checked_value_and_keeps_it_null` 钉住。
3. **`IsChecked="{x:Null}"` 写在标记里会得到 `false`，不是 `null`**。解析不报错，值落错。
   这是"标记看起来对、结果是另一回事"的又一例（与 `{x:Bind}` 被丢弃、
   不存在的 `{ThemeResource}` 键留默认值同族）。**三态只能在代码里设**。
4. **`Border` 不是 `Control`**（`Jalium.UI.Controls.Border` 转 `Control` 抛 `InvalidCastException`）。
   部件层的断言要按实际类型读，别按"控件"这个统称读。

## 因此改了什么

- **产品侧**：`DefaultToggleButtonStyle` 补上四组 indeterminate 触发器
  （休息 / +悬停 / +按下 / +禁用），把 12 条 `ToggleButton*Indeterminate*` 别名从"声明了没人用"
  变成有消费点；`CheckBox` 模板本来就有 indeterminate 通路，现在被断言而不是被假设。
- **Gallery 侧**：`MixedCheckBox` 不再写 `IsChecked="{x:Null}"`，改为在 `WireSelection()` 里
  `IsChecked = null`，并注明原因 —— 否则那个示例一直是"未勾选"，而标记读起来完全正常。
- **目录侧**：`CheckBox`/`ToggleButton` 的 gaps 去掉"三态没通路"，换成仍然欠的项
  （上游键未逐字转录、glyph 不做像素归因、点击/键盘/触摸未测）。

## 新增断言（7 条，`AstraSelectionTests`）

三态/勾选/未勾选三档的部件读回 + 三态表面的像素哨兵（20×20 的框，改调色板实例就能看到）；
条件值形状；标记 `{x:Null}` 落到 `false` 的实测语义；ToggleButton 四组 indeterminate 触发器
带的是哪 12 个键；indeterminate 的 ToggleButton 画的是休息底而不是强调色。

## 这些读数不支持什么

1. **不支持"三态的上屏结果是对的"**：混合条与勾形都没有单独的像素归因
   （字形不在这条离屏通路里，`adaptation/06`），能断言的是"表面刷是这个 token、部件 Opacity 是 1"。
2. **不支持"三态能被真实操作出来"**：没有任何点击/键盘/触摸断言。这里全部是**直接设属性**。
   真实循环（点击三态框 → null → true → false）归输入证据那一批。
3. **不支持"禁用 + 三态"的组合有视觉证据**：那两条 MultiTrigger 只有结构断言（带的是哪个键）。
4. **不支持 RadioButton 三态**：上游 RadioButton 没有 indeterminate 概念，本批也未转录其键层。
5. 一次诊断运行时 UI 线程挂过 60 秒（`AstraThemeRuntimeFixture` 报超时），同样的代码第二次跑就正常
   报出了异常 —— **超时不等于挂死，也可能是 UI 线程上的异常没被搬运出来**；
   这条没定位，先记为诊断写法上的注意项（分步记日志能一次定位）。
