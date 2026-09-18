# 自绘控件的颜色天花板：IL + DP 元数据 + 实测像素三重定位

回答 `01-jalium-control-census.md` 留下的唯一悬案：59 个自绘控件里，到底哪些是样式真的压不住的。
方法不是再推断一次，而是三路交叉：**IL 解码**（带校准对照）、**依赖属性默认值元数据**、
**离屏渲染后逐像素取样**。探针 `spike/RenderCeiling`，原始输出 `02-ceiling-raw-output.txt`。

## 结论：真正的天花板只有 4 个类型，而且只锁 `Accent` 一个色

IL 扫描覆盖 `Jalium.UI.Managed` 的 **163 个类型 / 8461 个方法**：

```
CALIBRATION  KnownDirty:   decoder reports 2 -> KnownDirty -> get_Accent; KnownDirty -> get_SliderThumb
CALIBRATION  KnownClean:   decoder reports 0 ->
SCAN         types=163 methods=8461 renderOverriding=59 themeColorsReferencing=25
SPLIT        .cctor-only = 21 types; live reads = 4 types
```

校准对照组走的是同一套解码器：已知含引用的方法报出 2 处真引用，已知不含的报 0，
**这套扫描在这批 IL 上没有假阳也没有漏报**。

引用 `ThemeColors` 的 25 个类型里，21 个**只在 `.cctor` 里读**——那是"进程启动时算一次、
存成静态刷子或 DP 默认值"的形态，样式可以直接改写它。真正每次执行都读实时值的只有 4 处：

```
CEILING  DataGrid      StartColumnDrag -> get_Accent
CEILING  TreeDataGrid  StartColumnDrag -> get_Accent
CEILING  InfoBar       GetSeverityColors -> get_Accent
CEILING  TimePicker    BuildPopup        -> get_Accent
```

**全部只读 `Accent`，且全部不在 `OnRender` 里**，而在列拖拽高亮、严重性配色、弹层构建这类辅助方法中。

## 实测像素：`ApplyAccent` 是有效的，DP 是认的

`RenderTargetBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormat)` + `Render(Visual)` +
`CopyPixels(Int32Rect, byte[], stride, offset)` 构成**完整的进程内像素回读**。三遍对照（`Bgr32`，
已按 B,G,R 还原成 R,G,B 读序）：

```
[1-default       ] Slider      magenta=False emerald=False top=[#700E88 …]
[1-default       ] ProgressBar magenta=False emerald=False top=[#FFFFFFx660 #63017A …]
[1-default       ] CheckBox    magenta=False emerald=True  top=[#207345x34 #1C7F43x34 #1E7944x25]
[1-default       ] RadioButton magenta=False emerald=True
[1-default       ] ScrollBar   magenta=False emerald=False top=[#FFFFFFx12304 #D2D2D2x56]

[2-ApplyAccent]    Slider      top 变为 [#F505F5x38 #F707F7x38 #F808F8x38]   ← 跟着强调色走了
[2-ApplyAccent]    ProgressBar top 变为 [#F303F3x48 #F606F6x48 #F909F9x48]   ← 同上
[2-ApplyAccent]    CheckBox    仍是 #207345 / #1C7F43                        ← 无动于衷
[2-ApplyAccent]    RadioButton 仍是 #207345                                  ← 无动于衷
[2-ApplyAccent]    ScrollBar   不变

[3-Background+Foreground=magenta] Slider    magenta=True  882px
[3-Background+Foreground=magenta] ProgressBar magenta=True 660px
[3-Background+Foreground=magenta] ScrollBar magenta=True  9064px
[3-Background+Foreground=magenta] CheckBox  magenta=False，依旧品牌绿
[3-Background+Foreground=magenta] RadioButton magenta=False，依旧品牌绿
```

三点读数：

1. **`ThemeManager.ApplyAccent(color)` 确实能驱动控件渲染**——Slider 与 ProgressBar 的强调色像素
   整片从原色变成品红族。
2. **自绘不等于不认 DP**——Slider / ProgressBar / ScrollBar 都把我们设的 `Background` 画出来了。
   inkcanvas 当年"Slider 是 `OnRender` 所以模板改不动"的经验，**在颜色这一项上被证伪**。
3. **`CheckBox` / `RadioButton` 的勾选 glyph 是真正的硬钉子**：强调色改了不走，
   `Background`/`Foreground` 设成品红也不走，三遍恒定 `#207345`（即 `ThemeColors.Accent` 抗锯齿后
   的值）。注意它**既不在 59 自绘名单、也不在 25 引用名单里**——我的扫描按 `DeclaredOnly`
   归因，说明这个刷子是在别处（基类型或共享辅助类）冻结的，属于扫描归因盲区。

## 更正我先前两处错误说法

- **`00-…capabilities.md` 的 S0-f 写"四种公开写入口全部无效"**：对 `ThemeColors` 这张**表**成立，
  但我当时把结论扩大成了"强调色改不动"。**那是错的**：像素证明 `ApplyAccent` 能驱动控件渲染。
  该轮实验里我还在 `ApplyAccent` 之后调了一次空参数的 `ApplyBrandTheme(options)`，
  它把强调色重置回了品牌默认值，是我自己制造的干扰变量。
- **"59 个自绘控件是一整类风险"**：实测只剩 4 个实时读取点 + `CheckBox`/`RadioButton` glyph
  这一族冻结刷子。名单里被点名的 `Slider`、`ScrollBar`、`ProgressBar`、`TextBox`、`TabControl`、
  `Menu`、`CommandBar`、`DatePicker`、`Window` **一个都不在实时读取清单上**。

## 顺带得到的能力：像素验收不再是障碍

原计划断言"本环境做不了像素比对（computer-use 截图不可信）"，替代证据链只能靠声明式数值 +
`DesiredSize`。现在 **`RenderTargetBitmap` 可以在测试进程内直接渲染控件并回读像素**，
不需要截图、不需要人工目视。这把逐控件"像素还原"的证据链从三层升级为四层，
且第 (iv) 项人工目视可以降级为可选。

未定：离屏 RTB 的成像是否与上屏合成完全一致（本次只用它做"变了没变"的比较，未做绝对色值断言）；
`CheckBox`/`RadioButton` 那族冻结刷子的确切来源尚未定位；其余 57 个自绘控件未做像素对照。
