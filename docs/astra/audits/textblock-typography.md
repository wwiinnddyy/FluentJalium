# TextBlock / Typography 审计（目标项 3：`ported` → `audited`）

运行时权威：NuGet Jalium.UI 26.10.9。WinUI 参考树：`../microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`。
方法参考：`../ModernWpf` @ `23555a6c00623b2f80e67f20d7f1df49a1d28ad8`。参考树只读，blob 用
`git rev-parse <commit>:<path>` 现取（未 checkout、未 patch）。
上游文件：`controls/dev/CommonStyles/TextBlock_themeresources.xaml`，blob
`08cbcf9cc6a9146b7d192f1f294b949dd2841cfd`，共 17 行 = 7 个 `x:Double` + 10 个 `Style`。
方法文件：`ModernWpf/Styles/TextStyles.xaml`，blob `202e5161adf223f5554e301cb7e8173b265485a4`，`:7-48`。
原始读数：`docs/astra/adaptation/13-typography-property-transcription.md`（`spike/DoubleRowProbe` 模式
`typography`，另有同工程 `spellings` / `pertheme` 两模式量数值行）。
跨控件结论：`adaptation/13`（本行逐项）、`adaptation/01` §排版（普查表旧句）、#50（文本字形不落墨）。

本层的落点：`src/FluentJalium/ThemeResources/Typography.jalxaml`，11 行 = 10 个上游样式 + 1 个我们自己的隐式
`TextBlock` 样式（§4）。消费点全部在 Gallery（`Body`/`BodyStrong`/`Caption`/`Subtitle`/`Title` 共 522 行引用），
产品样式里没有一条继承排印样式（`Typography.jalxaml` 自己除外），所以这次重排的爆炸半径不进任何控件模板。

## 1. 上游 17 行逐键对账

| 上游行（`:行号`） | 上游值 | 本层 | 理由 |
| --- | --- | --- | --- |
| `CaptionTextBlockFontSize`（:3） | `x:Double` 12 | **不发** | 数值行在本运行时带不住数字（§3）；12 以字面量落在 `Caption` 的 setter 上 |
| `BodyTextBlockFontSize`（:4） | `x:Double` 14 | **不发** | 同上 |
| `BodyLargeTextBlockFontSize`（:5） | `x:Double` 18 | **不发** | 同上 |
| `SubtitleTextBlockFontSize`（:6） | `x:Double` 20 | **不发** | 同上 |
| `TitleTextBlockFontSize`（:7） | `x:Double` 28 | **不发** | 同上 |
| `TitleLargeTextBlockFontSize`（:8） | `x:Double` 40 | **不发** | 同上 |
| `DisplayTextBlockFontSize`（:9） | `x:Double` 68 | **不发** | 同上 |
| `BaseTextBlockStyle`（:10-18） | 7 个 setter | **发 5 个** | `TextLineBounds`、`OpticalMarginAlignment` 宿主无成员；`FontFamily` 见 §2 |
| `CaptionTextBlockStyle`（:19-22） | 12 + Normal | 逐字发 | — |
| `BodyTextBlockStyle`（:23-25） | Normal | 逐字发 | 旧文件在这里额外钉了 `TextFillColorPrimaryBrush`，删 |
| `BodyStrongTextBlockStyle`（:26） | 空样式（继承 Base 的 SemiBold） | 逐字发 | 旧文件在这里额外写 `FontWeight`，如今交给 Base |
| `BodyLargeTextBlockStyle`（:27-31） | Normal + 18 + 裁剪 | **发 2 个** | `OpticalMarginAlignment` 无成员 |
| `BodyLargeStrongTextBlockStyle`（:32-35） | 18 + 裁剪 | **发 1 个** | 同上 |
| `SubtitleTextBlockStyle`（:36-39） | 20 + 裁剪 | **发 1 个** | 同上 |
| `TitleTextBlockStyle`（:40-43） | 28 + 裁剪 | **发 1 个** | 同上 |
| `TitleLargeTextBlockStyle`（:44-47） | 40 + 裁剪 | **发 1 个** | 同上；旧文件把这个键名叫 `LargeTitleTextBlockStyle`，改名（零消费点，不留别名） |
| `DisplayTextBlockStyle`（:48-51） | 68 + 裁剪 | **发 1 个** | 同上；旧文件没有这一行 |

链形同步改：上游十个样式全挂 `BaseTextBlockStyle`，旧文件是 `Caption`→`Body`、`Subtitle`/`Title`/`LargeTitle`→
`BodyStrong`。差别不是风格——挂在 `Body` 下面会连带吃到 `Body` 的 `FontWeight=Normal` 与（旧文件里）`Foreground`，
`BodyLargeStrong` 一抄就废。

## 2. `FontFamily="XamlAutoFontFamily"` 为什么单独删

上游 :11 那行是本文件里唯一一条"看起来能抄、抄了反而坏"的：`FontFamily` 属性存在、markup 加载不报错、
setter 也确实落上（`adaptation/13` 的 `setter-FontFamily` 腿读回 `FontFamily=XamlAutoFontFamily`），但
本运行时没有 `XamlAutoFontFamily` 这个标记值，于是它成为一个**名字叫 `XamlAutoFontFamily` 的字体族**，
实际渲染落到 fallback——比不写更糟，因为不写会拿到系统 UI 字体（这台机器实测 `Microsoft YaHei UI`）。
这一条要写在文件头而不只在审计里：它是那种"闸口全绿、屏幕上却错了"的形状，光看 keys.md 看不出来。

## 3. 七个数值行：能落属性，不能过 markup

三条路的读数（同一 key、同一 `FontSize` 落点，`adaptation/13` 结论三）：字面量 → 18 落上；markup
`<sys:Double>18</sys:Double>` → 行读回 `Double=0`、属性停在默认 14；同一个 key 由 C# 放 `18d` → 18 落上。
`x:Double` 元素直接解析失败（`Cannot resolve type 'Double' in namespace '…/xaml'`），`sys:Int32` 一样归零，
`sys:String` 保住 `"18"` 这个文本但过不到 Double 属性。所以"C# 种这七行 + 样式改吃 `{ThemeResource}`"
是可行方案而不是唯一方案，本批**明确不做**，三条理由与反悔入口都写在 `adaptation/13` 结论四。

代价按名字结清：一个应用写 `TryFindResource("BodyTextBlockFontSize")` 在本层拿不到东西，而在 WinUI 拿得到。

## 4. 我们比上游多出来的一行

`<Style TargetType="TextBlock">` — 上游没有隐式 `TextBlock` 样式（本文件 17 行没有一条不带 `x:Key`），
所以"裸 `TextBlock` 长什么样"这件事上游交给框架默认值，本层这一行替它定了三件事：`FontSize=14`、
`Foreground=TextFillColorPrimaryBrush`、`TextWrapping=Wrap`。实测对照（`adaptation/13` 的
`detached` / `mounted` 两腿）：宿主默认是 14 / Normal / None / **NoWrap**，所以这一行今天真正改变的只有
`Wrap` 一项，另两项与宿主默认或别名层结果重合。

这一行**不再** `BasedOn` 任何排印样式。挂上去会顺带吃到 `CharacterEllipsis`，那是给全库每一处裸文本
换截断行为，而文本墨在本仪器下一条像素都看不见（#50），这种改动不能借"转录"的便车进来。想收口的话，
`Wrap` 与钉墨各自都是独立一笔（导航文字不许换行那条旧账就挂在 `Wrap` 上）。

## 5. 出口与四类证据

- **构建**：`0 警告 / 0 错误`。
- **行为**：`AstraTypographyTests` 31 条。两条 Theory 分开的理由是**形状断言看不见落不上属性的行**：
  `A_transcribed_style_writes_exactly_the_rows_upstream_writes`（逐样式 setter 名单逐字对账，多一条少一条都红）
  与 `A_transcribed_style_lands_upstreams_size_and_weight_on_a_live_block`（挂载后读回 12/14/18/20/28/40/68 与
  Normal/SemiBold，另有 `TextTrimming`/`TextWrapping` 两腿，因为宿主默认是 None/NoWrap，所以它们能证伪）。
  另有：两条"宿主没有该成员"（反射）、七条"上游数值键名未发布"、一条"样式不写墨时块仍吃我们那只实例"、
  一条"markup 数值行丢数、代码数值行到得了属性"。
- **视觉**：**本行零像素主张**。#50 量死了文本字形在 `Render` 与 `Host` 两条捕获通路都不落墨，
  所以 `audited` 定义里那一格由"证明到不了"顶替（`Catalog.json` 图例本次同步改写，理由与争议点摆在该字段与 ROADMAP）。
- **硬件输入**：本行不涉及输入通路。

## 6. Known Gaps（这一行的）

1. 上游七个 `*TextBlockFontSize` 数值键名未兑现（§3）。不拿"字面量里的值相同"当兑现。
2. `TextLineBounds`、`OpticalMarginAlignment` 两条上游 setter 无法转录（宿主无成员，§1）。
3. `FontFamily` 交给操作系统，`XamlAutoFontFamily` 那套自动切换在 WinUI 才有；本层的字体回退**未量**。
4. 文本的一切可见结果都没有像素证据（#50）：字号、字重、截断、颜色四项都只是依赖属性读数。
5. 隐式 `TextBlock` 行（§4）是本层自加，仍钉主墨并全局换行，两处都偏离上游，改它需要能看见文本的仪器。
