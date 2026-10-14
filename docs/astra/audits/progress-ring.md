# ProgressRing（自有类型 `FluentProgressRing`）上游审计与适配

运行时权威：NuGet Jalium.UI 26.10.9。WinUI 参考：`../microsoft-ui-xaml`，commit
`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`。方法参考：`../ModernWpf`，commit
`23555a6c00623b2f80e67f20d7f1df49a1d28ad8`。两份参考仓库在本次改动中只读。
探测逐字转录：`docs/astra/adaptation/s1p-ring-raw.txt`（`spike/RingProbe`，模式
`surface` / `ink` / `anim` / `xform` / `motion` / `surface2`）。

## 1. 源文件与 blob

| 路径 | blob |
| --- | --- |
| `controls/dev/ProgressRing/ProgressRing_themeresources.xaml` | `f3fa38ea83ea910200a6a539140a379b128bbf94` |
| `controls/dev/ProgressRing/ProgressRing.xaml` | `df15b1794bbd0cda30279b5ab0674cafa544a0cd` |
| `controls/dev/ProgressRing/ProgressRing.idl` | `4ab074e9c8c65714fefd7892fa75236f919463bf` |
| `controls/dev/ProgressRing/ProgressRing.h` | `208152aa309ee51da581320032677088fa816154` |
| `controls/dev/ProgressRing/ProgressRing.cpp` | `46aaf1a82be04175f7eb6a8f6f2481ac7db7be15` |
| `controls/dev/ProgressRing/ProgressRingTemplateSettings.h` | `62940cad2761151706148c539791b48eb362eed4` |
| `controls/dev/ProgressRing/ProgressRingTemplateSettings.cpp` | `7a7326a2601469a5d186217f2088441f60987d66` |
| `controls/dev/Generated/ProgressRing.properties.cpp` | `f6de36b2e5d998be3e15ed0eeaea0d5a8d35bc05` |
| `controls/dev/ProgressRing/AnimatedVisuals/ProgressRingDeterminate.cpp` | `7d5234db42e92d9d4411009bd0086e7a2a1f0e31` |
| `controls/dev/ProgressRing/AnimatedVisuals/ProgressRingIndeterminate.cpp` | `48b0ac924c6010a5bc4ba7aa4d588c200d807907` |
| `controls/dev/ProgressRing/ProgressRingAutomationPeer.h` / `.cpp` | `71d6d8b6607ee18a8311efc948f0a0f3ca2f4b18` / `cd10c66d1322c5b4620bdcdadcfd5bb6c49fb856` |

ModernWpf 侧：`ModernWpf.Controls/ProgressRing/ProgressRing.cs` `9a2f2fcce4cf0aaf722272629c16bd942a6ca350`、
`ProgressRing.xaml` `b1032901f8316b20644e9e4ece6c4d0a19c5ebe6`、
`ProgressRingIndicator.cs` `8b071121c20f8e63ddb78ccd5b3d93c5b4dc5f8c`、
`ProgressRingTemplateSettings.cs` `94c799db70619678e9057fc48680d7721042672b`。

该 commit 下没有 `themes/ProgressRing.xaml`，`ProgressRing_themeresources.xaml` 是唯一的一份样式令牌源。

## 2. 基型选择：自有类型，而且几何也得自己算

`spike/ProgressProbe` 模式 census 已量过：`ProgressRing` 与 `LottiePlayer`/`Lottie` 在 26.10.9 里都是
ABSENT（`s1o-progress-raw.txt` §A、`s1p-ring-raw.txt` §A）。上游模板树只有 `Grid LayoutRoot` +
`AnimatedVisualPlayer LottiePlayer`（`ProgressRing.xaml:18,32`），对 `Ellipse|Path|Arc|Storyboard` 是 0 命中，
也**没有任何"播放器失败就退回静态几何"的代码路径**（`ProgressRing.cpp:212-257` 总是构造 Lottie 源）。
所以这条不是"要不要重模板"，而是"没有可模板的对象"：起自有类型 `FluentProgressRing`，几何由控件算。

算几何之前先量死四条通路（下表每一格都是同一次捕获里的读数，不是"markup 解析通过"）：

| 通路 | 读数 | 结论 |
| --- | --- | --- |
| `Ellipse` + `StrokeDashArray="20 200"` | 墨量 556、bbox 50x50，与不写 dash 的整圈**逐行相同**；把 `StrokeDashOffset` 写成 0/40/95，三张图一模一样 | dash 被存下并被忽略 |
| markup 里的 `<PathFigure><ArcSegment/></PathFigure>` | `Data` 读回是 `PathGeometry`，`Figures.Count=0`，**无墨**；改成显式 `<PathFigure.Segments>` 仍是 0 | geometry 集合的元素内容不填充 |
| markup 里 `Data="M 30,5 A 25,25 0 1 1 29.9,5"` | 解析成 `PathGeometry` 并出墨（620 像素 / 54x54） | 迷你语言是唯一能**从 markup** 得到图形的路 |
| 代码赋 `Path.Data` | 90°/135°/359.9° 在同一 60x60 格里分别 169 / 248 / 620 像素；代码 new 的 `PathGeometry+PathFigure+ArcSegment` 与字符串版同为 169 | 换 `Data` 会重绘——确定态与旋转都走这条 |

动画通路（上一段已量死，`s1o-progress-raw.txt` §F）：重复 `DoubleAnimation` 在 Freezable 变换上永不走表，
markup `Storyboard` 实例化为零子元素。而 `CompositionTarget.Rendering` 里逐帧直写 `RotateTransform.Angle`
确实推进（第 10 帧 370、第 30 帧 1110，两张图 bbox 分别 31x25@(0,8) 与 25x17@(0,17)）。
因此动画器 `Motion/ProgressRingAnimator.cs` 是**帧循环**，不是 storyboard。驱动它的上游条件也只有两条：
`ProgressRing.cpp:327-341` 在 `IsActive && IsIndeterminate` 下播放、`:355-360` 在非激活时停——上游不读动画策略，
所以本库的 `ReduceMotion` 同样不门它（#78 判掉，测点 `ReduceMotion_does_not_stop_the_indeterminate_spin`）。

角度落在哪里是第二个决定：第一版把角度交给 `RenderTransform`，画面只剩 3 个色像素——
事后查明那是 `7 * 5 / 80` 的**整数除法**（半径算成 0），不是支点问题，所以"支点该落哪"这条至今未量。
最终把角度写进图形起点（`FluentProgressRing.SetAngle`），这条正是上表第四行量过的路。
`mode xform` 另有一条独立事实：静态赋值的变换是**被应用**的（90° 把 bbox 从 44x50 换成 51x43），
只是没人驱动它——这与 S1-o 第 4 条合起来构成"能画不能动"。

## 3. 资源键清单（上游 3 个键 / 7 个声明位 → 本层 2 个）

`ProgressRing_themeresources.xaml` 的 ThemeDictionaries 只有 `Light`、`Default`、`HighContrast` 三组
（**没有 Dark**），两个键都是"别名的别名"：

| 键 | 上游目标 | 本层 |
| --- | --- | --- |
| `ProgressRingForegroundThemeBrush` | `AccentFillColorDefaultBrush`（HC：`SystemControlHighlightAccentBrush`），:5,9,13 | 发布，指向同一目标 |
| `ProgressRingBackgroundThemeBrush` | `ControlFillColorTransparentBrush`（HC：`SystemControlBackgroundBaseLowBrush`），:6,10,14 | 发布，指向同一目标 |
| `ProgressRingStrokeThickness` | `x:Double` 4，:17（主题无关） | **扣住**：本读取器对 `x:Double` 行是硬解析错误（S1-o 第 5 条），且上游模板对它 0 读取（`ProgressRing.xaml` 全文无该名字，另一处命中只是 baseline 清单 `APITests/BaselineResources2dot5stable.cs:1734`） |

扣住的这条在 `AstraProgressRingTests` 里反向钉住。另有两条测试用例记着一条易踩的坑：运行时**自己的**字典里
已经回答了 `ProgressRingForeground` / `ProgressRingBackground` 这两个短名字（census 读到
`ProgressRingForeground = SolidColorBrush(#FF1E793F)`），这也是本层坚持上游长键名、不去复用短名的理由。

上游没有已废弃/改名的 ProgressRing 键（`Deprecated_themeresources*.xaml` 0 命中）。

## 4. 状态映射（上游 3 个 VisualState → 本层 1 条 Trigger）

`CommonStates` 组（`ProgressRing.xaml:20`）三支：

| 上游状态 | 上游写了什么 | 本层 |
| --- | --- | --- |
| `Inactive`（:21-26） | `LayoutRoot.Opacity=0`、`LottiePlayer` 的 `AutomationProperties.AccessibilityView=Raw` | `<Trigger Property="IsActive" Value="False">` 只落第一件；第二件在 26.10.9 **没有可写的成员**（同一事实已记在 BreadcrumbBar） |
| `DeterminateActive`（:27） | 空 Setters | 无对应物：它的全部内容是把播放器源换成 determinate 资产 |
| `Active`（:28） | 空 Setters | 同上，换成 indeterminate 资产 |

模板里 `{TemplateBinding}` 0 处、`TemplateSettings.*` 0 处（审计确认），所以样式十二个 Setter 之外
没有任何声明式分支——上游的"状态"其实是代码在 `UpdateStates`（`ProgressRing.cpp:327-364`）里换源与播放。
本层把这一半放在控件里：`IsActive`/`IsIndeterminate` 决定换哪种几何与是否起循环。

## 5. 几何对照

样式十二个 Setter（`ProgressRing.xaml:4-15`）逐条照抄：`Foreground`、`Background`、`IsHitTestVisible=False`、
`HorizontalAlignment/VerticalAlignment=Center`、`MinHeight/MinWidth=16`、`IsTabStop=False`、`Width/Height=32`、
`Maximum=100`、`Template`。上游**从不设** `StrokeThickness`（厚度在资产里），本层也不设给样式，而由控件按盒子算。

两份资产是**两个不同的文件**，几何数字也不同，本层保留这个差别（ModernWpf 用同一套 determinate 数字画两态）：

| | determinate 资产 | indeterminate 资产 | 本层因子 |
| --- | --- | --- | --- |
| 画布 | 32x32 | 80x80 | — |
| 半径 x 形状缩放 | 8 x 1.77（`Det.cpp:171,183,194`、`Scale:<1.77,1.77>`） | 7 x 5（`Indet.cpp:178,190,202`、`Scale:5,5`） | 0.4425 / 0.4375 |
| 描边 | 1.5 x 1.77（`Det.cpp:207,222,237`） | 1.5 x 5（`Indet.cpp:216`） | 0.0830 / 0.09375 |
| 弧长 | `TrimEnd` = 播放进度 0→1（`Det.cpp:254,269-277`） | `TrimEnd=0.5`、`TrimStart` 0→0.5（`Indet.cpp:305,318`） | 360 x 进度（截到 359.9）/ 恒定 180 |
| 一圈 | 2.0 s（`op_durationSeconds{20000000}` / 60fps 120f） | 2.0 s 内 0→450→900°（`Indet.cpp:289-294`） | 900°/2 s，线性 |

32 DIP 盒子上：determinate 半径 14.16、描边 2.66；indeterminate 半径 14、描边 3。两条都被用例逐位读出。

起点与封顶的**来源要说清**：`-90`（12 点方向）与 `359.9`（整圆不能一笔画）取自 ModernWpf
（`ProgressRingIndicator.cs:189,201-214`），是本层对资产的解释而非资产本身——资产里的
`Rotation` 是 -0.008°，路径起点在哪由 Lottie 文件决定，本仓库没有能读它的工具。
`IndeterminateStartAngle=305`、`Sweep=160`、`1.6 s` 那三个 ModernWpf 数字**不在**上游任何文件里
（审计在 `controls/dev/ProgressRing/*` 全文 0 命中），因此一个都没抄。
两端圆帽（`StrokeStartLineCap`/`StrokeEndLineCap=Round`）照两份资产的 `StrokeStart/EndCap Round`。

上游 `ApplyTemplateSettings`（`ProgressRing.cpp:366-400`）：`diameter = width*0.1 + (width<=40 ? 1 : 0)`、
`anchor = width/2 - diameter`，写 `EllipseDiameter`/`EllipseOffset`/`MaxSideLength`，只看 `ActualWidth`。
本层没有 `TemplateSettings`，这套算式没有任何消费者，于是"随盒子缩放"这件事直接落在上面的因子里，
并由 `A_bigger_box_scales_the_arc` 在 64 DIP 上复量。

## 6. Known Gaps（不声称清单）

1. 不确定态的**尾部收窄**没做：上游资产在转一圈的同时把 `TrimStart` 从 0 推到 0.5，弧的尾端在追头端；
   本层是恒定 180° 弧在转。视觉上少了那个"甩尾"，动画周期与角度速率是按资产数字来的。
2. 两份 Lottie 资产本身不进本仓库：`DeterminateSource`/`IndeterminateSource`（`IAnimatedVisualSource`，
   MUX_PREVIEW）没有对应物，本层外观是从资产**公布出来的数字**重建的近似，不是同一渲染源。
3. `ProgressRingStrokeThickness` 不发布（见 §3）；厚度是盒子的函数，写在控件里。
4. `ProgressRingTemplateSettings` 整体缺席，`EllipseDiameter`/`EllipseOffset`/`MaxSideLength` 三行没有绑定面。
5. `Inactive` 的 `AccessibilityView=Raw` 无处可写：不活动的环只靠 `Opacity=0` 退出画面，
   **仍在无障碍树里**——这是可证明的差别，不是猜测。
6. 旋转交给图形而非变换，这条**没有对照实验**：第一版的空画面被证明是常数写错，所以"支点落在哪"仍未量。
7. `-90` 起点与 `359.9` 封顶是 ModernWpf 的读法（见 §5），本层无法对着上游资产复核。
8. 高对比那一组（两条重指向行）在控件层没落，靠调色板别名 underneath 解决，与其他控件同一条账。
9. 硬件输入证据为零。上游设 `IsHitTestVisible=False`/`IsTabStop=False`，本层照抄并各钉一条断言，
   因此这条控件没有交互臂可测——但这不等于已验证过指针路径。

## 7. 四类证据

**构建**（`tools/Test-AstraGates.ps1` 串行，一趟通过，`GATE_EXIT=0`）：restore 全部最新；Debug **0 警告 / 0 错误**，
且这不是空转的"0 警告"——Manifest 57 → 59 行（新增 `ThemeResources/ProgressRing.jalxaml` +
`Styles/ProgressRing.jalxaml`），三个工程都重新出了 dll，测试数从上一段的 1244 变成 1275，正好是新控件的 30 条
加消费点闸口那份新字典带来的 1 条 theory（`AstraResourceKeyTests` 多一行 `InlineData`），所以重编是真的；
整套 **1275/1275 通过、0 失败、0 跳过**（5 m 6 s）；调色板漂移三行 `checked=True`（Light 83 源色 / 101 刷、
Dark 同、HighContrast 101 映射 + 3 条按住，本批零改动）；`keys.md is current: 1256 canonical lines.`（表体
1473 → 1481 行，四行是两条别名行与两条样式行）；`All Astra gates passed.`

**行为**（`AstraProgressRingTests` 30 条）：上游 12 条 setter 逐条比对实例身份与字面值；两条别名行与调色板
目标 `Assert.Same`；模板部件契约（`LayoutRoot` + `ProgressRingArc`）与"弧是单一 `PathFigure` 挂 `ArcSegment`"；
`[Theory]` 五档进度 → 0/90/180/270/359.9 度，另五档钉 `IsLargeArc`；量程改写（`Minimum=90` 后同一 Value 变
22.5 度）；退化量程（`Minimum==Maximum`）交回空 `Figures` 而不是那个 2 像素圆帽；不确定态恒定 180 度且**不读**
Value；半径/描边因子在 32 与 64 两种盒子上各自读出（闭合环上量 `Bounds`，180 度弧的界宽只有一个半径）；
`The_spin_moves_the_arc_and_stops_with_the_ring` 用前后差而不是阈值——同一帧常数写进角度会让断言永远成立，
所以断的是"起转前后 `CurrentSweepAngle` 与 `Data` 都变了、停转后两次读数相同"；`IsActive=False` 落到
`LayoutRoot.Opacity==0`。

**A/B 有牙**（提交前复跑过一遍，不是上一段的记忆）：删掉模板里的 `Name="ProgressRingArc"`（先 `grep -rn` 确认全仓
只有 1 处命中，改后 `grep -c` 读回 0）→ **失败 16 / 通过 14**，红的正是几何、像素、量程与帧循环那几组；还原后
`grep -c` 回到 1，同一过滤器 **30/30 通过**。另外本段的真缺陷是自找的：`IndeterminateRadiusFactor` 写成
`7 * 5 / 80`，整数除法得 0，环只印 3 像素——是这条 A/B 之外的像素断言把它抓出来的，抓之前我先把它误记成
"变换支点问题"（已撤回，见 §6 第 6 条）。

上面那趟闸口跑在一处**注释**改动之前：模板注释里"控件还写 `RenderTransform`"是错的（旋转写在弧的起始角里），
订正只动了 `<!-- -->` 内的字，随后单独重跑过环的 30 条并全绿。因此"1275/1275"对应的源码字节与本段提交的
字节只差这几行注释，不差任何一条被断言的行为。

**视觉**：四条像素主张只读实测的量——半环 vs 整环的强调色像素数（`whole > half * 1.4`，比值来自同一被摄体两帧，
不是挑出来的阈值）；0% 与不活动的环**印 0 个强调色像素**，且同帧 `PaintedPixels > 0` 当白卡守门，否则"零墨"
和"没截到图"无法区分；不确定态墨点数落在 40~320 之间（32 DIP 环 180 度、描边 3 的理论值在中间）；品牌绿
`#207245` 计数 0；Light↔Dark 两档整图直方图不同。Gallery 侧：`tools/Test-AstraGallerySmoke.ps1 -Page status`
报"closed cleanly in 5.8 s；1 page(s) mounted and closed；no Gallery process left."，默认全 10 页 6.1 秒干净退出。
该脚本按 `adaptation/06` 的结论**故意不截屏**，所以新卡片没有目视帧——像素主张只在上面那四条里。

**硬件输入**：**仍为零**，并且本段没有可以测的输入臂：环照抄了 `IsHitTestVisible=False`/`IsTabStop=False`，
两条各钉了断言。全局真指针通路（Task #13）依旧是整个仓库的欠账，不因本段控件"无需输入"而结清。
