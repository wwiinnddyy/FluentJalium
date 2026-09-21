# RatingControl 审计：一颗星到底多宽，是谁说了算

阶段 6 第五段。上游基线 `microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`，
运行时权威 NuGet Jalium.UI **26.10.9**（AGENTS.md 钉住），方法对照 ModernWpf @ `23555a6c0`。
本批原始读数全部在 `docs/astra/adaptation/s1s-rating-raw.txt`（下称 s1s），本文只写结论与出处。

## 0 · 依据

| 上游文件 | blob | 用在哪里 |
| --- | --- | --- |
| `controls/dev/RatingControl/RatingControl_themeresources.xaml` | `0d9fccdf29a38836bd89fabbc222f6605d104b59` | `ThemeResources/RatingControl.jalxaml`（8 别名 + 1 字形条目 + 4 模板） |
| `controls/dev/RatingControl/RatingControl.xaml` | `6229cfe469d53d5d466f0baead99c19a5cc7eb6b` | `Styles/RatingControl.jalxaml`（默认样式 + 隐式行 + 6 个状态） |
| `controls/dev/RatingControl/RatingControl.idl` | 公开面 3-45 | `Controls/Status/FluentRatingControl.cs` 与 `FluentRatingItemInfo.cs` 的属性面 |
| `controls/dev/RatingControl/RatingControl.cpp` | 44-55、181-268、300-366、372-446、846-970 | 度量、条目生成、裁剪、缩放动画、指针与宽度模型 |
| `controls/dev/RatingControl/RatingControlAutomationPeer.cpp` | 全文 | 未照搬，见第 11 节 |

## 1 · 键清单（逐字照抄的结果）

上游这一份 56 行、**16 个不同键**：8 个画刷别名在 Light/Default/HighContrast 三份字典里各写一遍（:5-12、:16-23、:27-34），
`MUX_RatingControlDefaultFontInfo` 同样三份各一行（:13、:24、:35），字典外 3 个 `x:Double` 度量行（:39-41）与
4 个 `DataTemplate` 行（:42-55）。

本批发布 **13 个**：8 个别名一字不改，字形条目原样（`&#xE735;` / `&#xE734;`），4 个模板按上游的键名与结构转录，
全部消费点走 `{ThemeResource}`。不发布 3 个 `x:Double`（32 / 8 / -12.5）—— 本读取器对这一类行有两种死法
（adaptation/00 S1-r）：`<x:Double>` 整份字典被拒，`clr-namespace` 数值行留着但读回 0。

替代位置：32 成为 `RenderedItemSize` 常量与模板字面量，8 成为 `ItemSpacing`，两者都有测试从控件上读回；
**-12.5（`RatingControlCaptionTopMargin`）整条不写** —— 它是上游为 `TextLineBounds="Tight"` 配的补偿，本运行时
的标题没有那个旋钮，写了就是把标题顶出这一行。

## 2 · 运行时面差异（s1s [A][B]）

`RatingControl`、`Rating`、`RatingItemInfo`、`RatingItemFontInfo`、`RatingItemImageInfo` 五个名字在真正装着控件的
程序集（`Jalium.UI.Managed`，2 958 个公开类型）里全部 **ABSENT**，全进程命中 `*Rating*` 的只有我们自己的三个类型。
自有类型因此是被量出来的，不是选的。同一次普查顺手量到 `BitmapIcon` 也不在（阶段 6 尾批的账）。

成员面按 typeof 直读：`StackPanel.Spacing` 存在（Double），`UIElement.Clip` 与 `ClipToBounds` 都存在，
`FrameworkElement.RenderTransform` 存在 —— 存在不等于生效，[C][D][E] 三段逐个量。

## 3 · 条目模板与那两条 -8 边距

上游字形模板的 `Margin="-8,-8,0,0"` 带着它自己的注释："-8, -8 are to compensate for the default scale down"。
被补偿的那一步在第 9 节。本批模板不写这两条边距，原因不是"不需要"，而是**同一份度量不能有两条来源**：
步进 34 是本运行时实测的（s1s [C] 末两行），不是上游设计时的 32，边距因此必须由控件从实测值算，
写在标记里就是一个没人校的第二个副本。缩放与左移都落在 `FluentRatingControl.CustomizeItem`。

`FontFamily` 写字面量 `Segoe Fluent Icons`：本调色板根本没有 `SymbolThemeFontFamily` 这个键，
BreadcrumbBar 与 TeachingTip 的字形已经是这么写的。

## 4 · VisualState → Trigger 映射

上游 `CommonStates` 六个状态，每个只改一处目标 —— `ForegroundContentPresenter.Foreground`（:25-50）。
六个状态变成六条模板触发器，`TargetName` 沿用上游名，无一条合并、无一条丢：

| 上游状态 | 写入的画刷键 | 本批触发器 |
| --- | --- | --- |
| Set | `RatingControlSelectedForeground` | `ActiveRatingState=Set` |
| PointerOverSet | 同上（上游 :45 与 :50 本就共用） | `PointerOverSet` |
| Placeholder | `RatingControlPlaceholderForeground` | `Placeholder` |
| PointerOverPlaceholder | `RatingControlPointerOverPlaceholderForeground` | `PointerOverPlaceholder` |
| PointerOverUnselected | `RatingControlPointerOverUnselectedForeground` | `PointerOverUnselected` |
| Disabled | `RatingControlDisabledSelectedForeground` | `Disabled` |

状态由控件在 dressing 条目**之前**发布，因为条目要读的就是这条触发器刚写上的画刷。
部件保留上游名字但是 `ContentControl`：26.10.9 的 `ContentPresenter` 不声明 `Foreground`，往它身上写就是
#31/#33 那一类死 setter。

样式里三条 setter 没写：`UseSystemFocusVisuals`、`FocusVisualMargin=-8,-7,-8,0`、`IsFocusEngagementEnabled`
（本运行时不声明这三个成员，未知属性是静默丢弃而非报错）。焦点环因此继续像本层其它控件一样自绘，
而 `IsFocusEngagementEnabled` 撑起来的手柄按下/抬起契约在这里没有对应物 —— 见第 5 节"没有手柄通路"。

## 5 · 行为照抄清单

钳位、事件、按键、指针、只读、清除六组全部对着 cpp 走，并有断言：

- `Value`/`MaxRating` 钳位**从不抛**：`<0 → -1`、`≤1 → 1`、`>Max → Max`；`Placeholder` 保留小数并夹到 `max(1,Max)`；
  `MaxRating` 先落自己纠正后的值，再把 `Value`/`Placeholder` 拉下来。
- `SetRatingTo` 在值没变时**照样**发 `ValueChanged`（cpp:616 无增量守卫），但"已经未设还要再清"整段跳过、不发事件。
- `IsClearEnabled=false` 时清除落到 1.0 而不是哨兵；再点当前星只从指针通路清除。
- 键：上/右 +1，下/左 -1（阅读方向不参与），Home 清、End 满，从满值再上是原地，小数先取整再动，只读按键返回未处理。
- 指针：移动只预览不写值；释放按整行宽度分（不减首格偏移），拖动才让"拖出行外即清除"可达。
- 上游移动与释放两条通路对首格偏移的不对称在本布局下是**可见但为零**的：格子从面板原点起排，
  `FirstItemOffset` 读回 0，测试把这个 0 钉住而不是含糊过去。
- 没有手柄（gamepad）通路：`IsFocusEngagementEnabled` 无对应成员，上游那套"按下才聚焦"的语义无处落。

## 6 · 高对比（Known Gap）

上游 HighContrast 字典把 8 个别名里的 7 个重指到 `SystemControl*`（`RatingControlDisabledSelectedForeground` 除外）。
本层不在控件字典里重指：调色板的高对比覆盖走 `ThemeResources/HighContrast.map`，那层只认调色板刷键、
不认逐控件别名 —— 与 InfoBadge、ProgressBar、ProgressRing、PipsPager 同一笔账，未结清。

## 7 · 裁剪通路（s1s [D]）

上游给每个前景条目挂一个 `RectangleGeometry`（cpp:336-366）。本运行时 `UIElement.Clip` **不裁墨**：
代码写的矩形留着、3 600 像素一点不少；markup 写同一段几何，逗号式与空格式都读回 `rect=Empty`（静默丢弃的新形态）。
`ClipToBounds` 相反：30/60 → 1 800，45/60 → 2 700，20 宽盖住 512 墨 → 恰好 320，按比例且标记里也生效。
所以半星是一个 `ClipToBounds` 宿主，且裁剪题必须捕获更宽的祖先 —— 按目标出图的捕获对"裁到目标之外"没有分辨力。

## 8 · 半星与分数

上游按 `RenderingRatingFontSize()` 取整宽、按 `value - floor(value)` 取分数、超出部分取 0（cpp:336-366）。
本批同一条三分支，但盒子的宽度换成墨盒（第 9 节）：分数乘的是看得见的那一段，
所以 0.25 裁掉的是四分之一颗星，而不是"墨左边界之前的一段空白"。宿主宽度的断言是逐格的，
`A_partial_rating_crops_the_boundary_item_and_empties_the_rest` 用 `(值, 每格分数)` 成组喂进去。

## 9 · 度量：32 是双倍渲染，17 才是模型里的一颗星

三行 cpp 把这件事说全了：`cpp:190-203` 用一个裸探针 `TextBlock` 的 `DesiredSize.Width` 覆盖
`m_scaledFontSizeForRendering`；`cpp:51-55` 让 `ActualRatingFontSize = RenderingRatingFontSize / 2`；
`cpp:961`、`cpp:969` 的星心与行宽都用这个一半的数。上游模板注释里那句 "32 = 2 * [default fontsize] - because of
double size rendering" 和条目上挂的 `ApplyScaleExpressionAnimation`（`cpp:372-392`）是同一件事的两端：
那颗星**画**在 32、**显示**在 0.5 缩放 —— 表达式在焦点停在哨兵值（`c_noPointerOverMagicNumber`）时被二次项压到
`max(..., 0.5)` 这条地板，也就是静止 0.5，悬停向 0.8（鼠标）/1.0（触摸）抬起。

本运行时没有那套自动缩小，也没有可挂表达式动画的合成视觉，所以两件事自己写：条目 `RenderTransform` =
`ScaleTransform`，原点 `0.5,0.5`，静止值 0.5，`Magnify` 每次状态变更按上游同一条二次式重算。
0.5 是否真的落墨是量过的（s1s [E]）：60×60 实色块经更宽祖先到 x=15..44、900 像素，无变换对照 x=0..59、3 600 像素。
顺带钉住一条仪器账：**捕获目标自身对它自己的 RenderTransform 是瞎的**，前两行读数差点把结论写反。

`_itemAdvance` 用裸 run 量（不是模板条目），否则实测值会被自己写上去的 -8.5 左移污染。图片条目走上游 `cpp:202`
那支：没有字形可量，直接取配置值 32。

## 10 · 间距矛盾结清（s1s [C][F]）

审计开量前留着的矛盾：上游标记的边距让**排布**的间距是一个数，而宽度与指针模型用的是另一个数。
本批把两条并成一条，代价是先量出这个运行时的第三条静默失效：

- `Spacing = -10` 属性读回 -10，三格 16 宽仍然一格 16、desired 仍是 48 —— **负号留在属性里、排布按 0 算**。
  上游正是往 `Spacing` 上写这个负数（cpp:249-268），照抄等于写一条永远不生效的赋值。
- 同一份 -10 拆成"正 8 + 后续格 `Margin.Left=-18`"时 pitch 实测 24：负边距确实动布局。

最终取的是第三条路：格子不再用步进盒 34，改用墨盒 17（= 实测 34 的一半，正是模型里那一颗星的宽度），
面板只带公开的 8，于是 `17 + 8 = 25` 同时是排布间距和模型间距 —— 不需要任何补偿，也就没有可被吃掉的东西。
实测（s1s [F] 第 2 条）：星宽 17、pitch 25、第五格 X=100、行右边界 117 = `5*17 + 4*8`，与指针模型逐位相符。

A/B 验牙：把这条改回上游那行（负 `Spacing`、格宽 34）→ pitch 读回 34，断言红；
只撤边距不撤缩放 → pitch 42、格边距读回 0，两条红；还原 → 复绿。

## 11 · 无障碍与自动化

上游 `RatingControlAutomationPeer` 与条目上的 `AutomationProperties.AccessibilityView="Raw"` 都没有照搬：
本运行时不声明 `AutomationProperties.*`，也没有可继承的对等 peer 基类。屏幕阅读器因此拿到控件而拿不到星数。
`Rating`（上游另有的旧控件）未做，也不在阶段 6 的清单里。

## 12 · Known Gaps 汇总与不声称清单

不声称：
- 星的墨。字形墨两条捕获通路都到不了（#50），本控件的视觉列只有"实色块证明裁剪与缩放这条路能落墨"加上
  树内几何断言，没有一张有星的图。
- 真鼠标 / 真触摸 / 真键盘。本套件无合成输入（#13），钳位、分数、放大数值都走 routed 处理器调用的内部入口，
  `MouseMove` 到 `PreviewAt` 的接线未被证明；悬停放大只在星心与地板上取值精确，中间一段未测。
- 焦点环外观、手柄通路、`ItemInfo` 图片路径的实际图像显示（条目已转录，无位图断言）。
- 高对比逐控件重指（第 6 节）。
- 星墨与格子的水平配准：墨在 34 的盒子里是否正好居中，无墨可量，只能按对称假定；这条若错，表现为整行左右偏一点。
