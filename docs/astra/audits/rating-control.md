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
- 真鼠标 / 真触摸 / 真键盘。本套件无合成输入（#13），钳位、分数、放大数值都走 routed 处理器调用的内部入口，
  `MouseMove` 到 `PreviewAt` 的接线未被证明；悬停放大只在星心与地板上取值精确，中间一段未测。
- 焦点环外观、手柄通路、`ItemInfo` 图片路径的实际图像显示（条目已转录，无位图断言）。
- 高对比逐控件重指（第 6 节）。
- 选中填充色的到达只有整屏捕获里看过，没有断言：#98 量测时有一张同形状的捕获，星带内 9 549 个灰像素、
  蓝色 0 个（填充穿了继承来的文本色），其后四次跑不复现，机制未定 —— 见第 14 节末与 `spike/RatingInkProbe/README.md`。

第 9 节留的那条"墨在盒子里是否正好居中，只能按对称假定"已经不再是假定：#98 把它量出来了，而且量出来是错的，
见第 13 节。

## 13 · #98：裁剪的宿主同时也在定量，星被切掉右半边（用户可见）

用户报的形状是"整行向右平移，星星右边一部分被遮住"。这条**不是**第 10 节那次间距结清的余账 —— 间距、步进、
指针模型三个数都对，错的是**墨落在格子内的哪一段**。

`spike/RatingInkProbe` 把四档值（3.5 / 5 / 2 / 4.7）挂进真窗口，逐格打印宿主宽、是否裁、run 的 desired/arranged、
字族、边距、缩放，以及 run 的盒变换到控件空间的水平段。同一次运行再抓这张窗口的像素，两套数才能对上。

量到的因果链只有一环：宿主是 `Grid{Width=17, ClipToBounds=True}`，而 `Grid` 在定量时把 17 + 8.5（边距）交给孩子，
那颗**画**在 34 上的 run 于是被夹到 25.5 —— 它从没拿到过自己的自然宽。0.5 缩放的原点是盒子的中心，盒子被夹在右边，
中心就从 8.5 掉到 4.25：

| | run arranged | 第 0 格墨盒 | 第 4 格墨盒 | 分数格（宿主 8.5） |
| --- | --- | --- | --- | --- |
| 修法前 | 25.5 | x=-2.13 w=12.75 | x=97.88 w=12.75 | 夹到 17 → 墨 8.5 |
| 修法后 | 34 | x=0 w=17 | x=100 w=17 | 34 不动，宿主裁到 8.5 |

一颗静止的星该占满 17 的格子（`17 × 0.5 = 8.5` 半边在格子外是错的读法：边距 -8.5 经同样 0.5 缩放，正好回到 0）。
修法前它只有 12.75 宽、还往左出格 2.13 —— 每格右边空 6.4 DIP，而指针模型按 25 的步进算星心，
于是**看得见的位置**比**点得中的位置**整体偏左 4.25 DIP：这就是"向右平移"与"右边被遮"两句话的同一件事。
半星那一格更糟：宿主 8.5 时盒子被夹到 17，墨 8.5，画出来是原星的二分之一再乘二分之一。

修法是给格子加一条**只负责裁**的宿主和一条**负责定量**的通道：`host(Grid,ClipToBounds) → lane(StackPanel 横向) → run`。
横向 `StackPanel` 用无限宽量孩子，run 因此回到自己的 34，宿主仍然照旧切。这一步不是自造形状：
上游那三行裁剪代码（`RatingControl.cpp:336-366`）把条目直接放在横向面板里、靠 `UIElement.Clip` 切，
条目拿到的定量本来就是自然宽；本运行时 `Clip` 静默失效（第 7 节），裁剪只能挪到宿主上，
挪的同时就必须把定量还给无限宽，否则两件事会像这样咬在一起。

像素侧独立复核（`scan-row.ps1 -BlueOnly`，星行中部 y=145 只数填充层）：修法前各格墨段 19/20/20 px、
半格 9 px；修法后 22/23/23 px、半格 13 px，且整段右移 3 px ≈ 1.8 DIP，与树上的 -2.13 对得上。
同一条扫描线上按颜色数（`sample-colors.ps1`）：前 `blue=5487`、后 `blue=6162`。
两张图 `spike/RatingInkProbe/rating-before.png` / `rating-after.png` 是同一次坐下来的 A/B，只差 `mutate-lane.ps1` 那一步。

验牙：`A_flush_left_ink_box_starts_at_the_cell_origin` 钉三件事 —— run 的 `ActualWidth` 等于步进 34、
墨盒左边界等于 0、墨盒宽等于 `ActualItemSize`。撤掉那条通道（把 `host.Children.Add(item)` 写回去）→
断言红在 `Expected: 34 / Actual: 25.5`；还原 → 64/64 复绿。测试侧不再自己数 `host.Children[0]`，
一律走控件的 `CellRun`：控件动画与裁剪的是哪一个元素，测试读的就是那一个，否则一条断言可以在"画的不是它"的树上照样绿。

顺带结清第 9 节那条外推：静止 0.5 的墨确实落进 17 的格子里，但**前提是盒子有 34 宽** ——
"盒子被自己写的边距夹住"这一形当时没有列进不声称清单，因为它不在预期失败的位置上。

## 14 · 选中填充色的到达：一次不复现的读数

#98 的抓图里有一张（`rating-anomaly-noaccent.png`，同一无通道形状）星带内 9 549 个灰像素、蓝色 0 个，
即填充层穿了 `TextPrimary` 而不是 `RatingControlSelectedForeground`。`StateBrush` 的读法是
`_foregroundPresenter?.Foreground ?? Foreground`（`FluentRatingControl.cs:630`），回退支正是继承来的文本色，
所以那张图能对上"读回空"这一形。其后四次跑（含通道有/无两形状）都是 5 400–6 200 个蓝像素，不复现，
也没有一条断言钉住"填充色到达像素"。这条留在不声称清单里，等一次能复现的读数再定性。

顺手划掉一条容易接错的桥：`gate-98.log` 的红里有一条 `A_settled_row_wears_the_selected_brush…`，
消息是 `Expected: Set / Actual: PointerOverSet` —— 测试自己没喂指针，读回的是真鼠标落在新建窗口上的 hover，
改的是**状态**不是墨的到达；而 `Set` 与 `PointerOverSet` 两档上游共用同一个 accent（第 4 节表），
接不出"星是黑的"。这两件事不是同一件，谁也不能替谁结。


