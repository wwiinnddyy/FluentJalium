# 像素归因：隐式样式到底有没有落到像素上

阶段 2 的阻塞项。原始读数在 `06-pixel-attribution-raw.txt`，由 `spike/PixelAttribution` 跑出来：
真实 `Application` + 已上屏窗口，八个对照样本 × 两套主题 × 四种捕获时机。
结论只引用那几行读数，不引用"看起来对了"。

## 三个问题，三个判定

| 问题 | 判定 | 证据 |
|---|---|---|
| Astra 的隐式样式对原生控件生效吗 | **生效**，应用级和树内级都生效，树内级还压过应用级 | run1 `2-appimplicit`、`5-implicit-local` |
| 正确的捕获时机是什么 | 上屏之后**渲染若干帧**，且要按"稳定"判定而不是按固定帧数 | run1 每样本的 `s0`→`s3` 四行 |
| 模板控件能不能单独捕获到内容 | **能**，但必须先在窗口里被渲染过 | run1 `self=` 列 `s0` 空、`s1` 起有值 |

细节：

- `1-default/Light/s0` 的 `self` 是 `#00000000x26950`（200×44 DIP 的控件在 350×77 缓冲里全是空），
  同一行 `s2` 起就是 `#00FFFF00x8296`。**空像素从来不是"样式没生效"，是没给帧。**
- 样本 1 和样本 2 构造上完全相同（都是裸 `new Button`），探针根本造不出"没有 Astra 样式"的原生 Button：
  `<Style TargetType="Button">` 在 `Application.Resources` 里就足以命中。它俩读数一致（`#FFFFFF00x8071`）。
- 元数据行：`style=null template=ControlTemplate(ButtonBase)`。
  隐式样式**不写** `FrameworkElement.Style` DP（与 WPF 一致），所以判"生效"要看 `Template`，
  读 `Style` 会得出假阴性。`5-implicit-local` 的 `Template` 是 `ControlTemplate(Button)`——
  被 `Grid.Resources[typeof(Button)]` 覆盖，证明逐元素资源查找链在 Jalium 里也走得通。
- `Control` 的 `Template` 落到 `ButtonBase` 层：Astra 的按钮模板来自
  `ButtonLayoutStyle`/`DefaultButtonStyle`（`TargetType="ButtonBase"`），经
  `<Style TargetType="Button" BasedOn="{StaticResource DefaultButtonStyle}" />` 传给原生 Button。

## 时机不是"一帧"，是"稳定"

`Border` 上的 `TransitionProperty="Background, BorderBrush" TransitionDuration="0:0:0.083"` 让刷色是**动过去**的：

- `s1`（4 帧）读到 `#FF969B04`、`#FF8F6205` 这类中间色；
- `2-appimplicit/Dark/s2`（累计 8 帧）还是 `#FFE8E94E`，到 `s3`（累计 16 帧）才 `#FFFFFF00`。

所以按"等 N 帧"写死会偶发取到过渡中间色。基座必须**采样到不变为止**（相邻两次直方图相同即稳定），
帧数只做上限。另外 run1 里出现过一次 `pumped 8/16`——画面静止后 `CompositionTarget.Rendering`
就不再来帧，所以"等帧"必须有超时兜底，不能无限等。

## `ForceRenderFrame()` 不是可用的替代

run2 用 `Window.ForceRenderFrame()` 连推 2/4/8/16 次，不等真帧：`1-default`、`2-appimplicit`、
`3-explicit-astra` 的 `self` 始终 `#00000000x26950`，而 `4-explicit-local`（纯本地模板、无 `{ThemeResource}` 过渡）
从 `s0` 就是 `#FFFF00FFx8800`。强推帧能出画面但出不了主题刷——动效/资源重解析要真帧才走。
**判据不能建在它上面。**

## 没有 `Application.Run` 也能拿帧

run3 复刻 xunit fixture 的处境：不跑应用循环，只 `Show()` + 嵌套 `Dispatcher.PushFrame`。
`pumped 4/4`、`pumped 8/8`、`pumped 16/16` 都真到帧，`#FFFFFF00x8071` 照样出现。
→ 像素断言可以留在现有线程化 fixture 里，不必为测试引入真进程。

## DPI 与 alpha 两条读数规则

- 本机 `DpiScale=1.75`。`RenderTargetBitmap.Render(element)` 把视觉按 **DIP 尺寸 1:1** 画进缓冲：
  200×44 DIP 的控件在 350×77 的缓冲里只占 8800 px，正好等于 DIP 面积。
  所以按 `width×height`（= 控件 DIP 尺寸）开缓冲时，**像素计数与 DPI 无关**，断言可以写死数字。
  整窗缓冲则随缩放变（`711x443`），只能做"变没变"的断言，不能做计数断言。
- `self` 路径的 **alpha 字节不可信**：走 `{ThemeResource}` + `TransitionProperty` 的 `Border`，
  缓冲里写回的是 `#00FFFF00`（A=00，RGB 正确）；纯本地模板和普通 `Border` 写 `#FFFF00FF`（A=FF）。
  直方图只比 RGB 是对的，但要记着：这条路径证明不了"合成后真的可见"。

## 被这套读数推翻的先前结论

| 先前写的 | 现在 |
|---|---|
| "覆盖我们的令牌后原生 Button 像素不动"（`00-pixel-harness-raw-output.txt` 归因探针） | **错**。是时机问题；给帧之后令牌色完整覆盖按钮区域。 |
| "带模板的控件单独捕获是空像素，只有 Window 能光栅化" | **错**。同上，给帧之后 `Render(element)` 有内容。 |
| 计划里"起手必修：模板根 Border 不吃本地 Background" | **证伪**。`6-local-background` 的 `#FFFFA500x8071`（窗口）/`#00FFA500x8296`（自身）说明本地 `Background` 经 `{TemplateBinding Background}` 完整落到像素，本地值也确实压过 Setter。 |
| "`#F5F5F7`/`#1C1C1E` 那 9680 px 是 Button 自己的表面" | **要更正**。本轮按钮区域被令牌染色，另有一块 `x12880` 的 `#F5F5F7`↔`#1C1C1E` 区域与它不重叠，且不受令牌影响——那是跟随 `Application.ThemeMode` 的框架表面，不是 Astra 令牌。旧读数的 9680 是在 DPI 1.75 下用 320×200 缓冲量出来的，数字本身不可信。 |
| `05-native-control-vacuum.md` 的"声明是否落到像素仍未证" | 该条限定可以缩小：**Button 这条链已证**（隐式样式 → 模板 → 令牌 → 像素）。其余 42 个真空控件仍未证。 |

## 基座已按这套读数改完（同日）

`tests/FluentJalium.Tests/Pixel/PixelHarness.cs` 现在做四件事，每一件都对应上面一条读数：

1. 所有捕获走**同一个宿主窗口**（`EnsureHost`），换样本只换 `Content`。
   这是探针没直接测出、但在测试里被逼出来的一条：宿主窗口一关掉，同线程再开新窗口就拿不到渲染帧，
   第二主题化控件起一律捕获成全黑（`The_implicit_astra_style...` 的第二次 `Render` 当时稳定复现全黑）。
   fixture 在 `Dispose` 里回 UI 线程 `ReleaseHost()`。
2. `Capture` 反复推帧直到两次直方图相同，且**空图不算稳定**（`PaintedPixels > 0` 才算）——
   把"没画"当成"稳定"正是第一批读数得出"主题不落到原生控件"的原因。
3. `Pump` 带预算（默认 400 ms）与看门狗：画面静止后 `CompositionTarget.Rendering` 会停发，
   只等帧会挂住。整轮再套 4 s 总预算。
4. 断言只比 RGB；`Sample.Subject` 记下样式管线解析到什么（`implicit/ControlTemplate` 之类），
   失败信息能一眼分清"样式没上"还是"像素没来"。

`Host(...)` 保留整窗捕获：那条路径的 alpha 字节是写的，用来说明合成后可见性；
计数随显示器缩放变，所以只断"变没变"。

## 补一条：状态**变化**之后的捕获时机（Button 批量出来的）

上面第 2 条的"两次相同才算稳定"对静态样本够用，但换刷/换状态这类**动态**断言必须有它：
模板面带 `TransitionProperty="Background"` 时，状态切换是一段真在跑的过渡（上游同一设计，83 ms），
固定帧数捕获会采到中间值——`spike/PointerProbe` 的 `swap`/hover 读数里出现过
`#E482E4`（品红↔绿的中间色）与 `#CB74CB`、`#CA73CA`，都是同一次采样的混合值，不是调色板里的颜色。

- 判据写法：**重复捕获直到两张直方图一致**（`PixelHarness.Capture` 与探针的 `CaptureStable` 同一个意思），
  不能写成"等 N 帧然后断言"。
- `distinct` 数量不能用来判断"是不是还在过渡中"：一个 200×44 的静止按钮本来就 distinct=7
  （主体 + 两层抗锯齿 + 文字），过渡中间值也只是把这三档换成中间色。要判断就比**颜色本身**。
- 换刷本身没问题：`transitionedAdopted=True`（与无过渡模板 `plainAdopted=True` 一致）说明
  首帧之后再赋一个新的 `Brush` 对象，带过渡的模板面照样采纳。已钉成常跑断言
  `AstraButtonTests.A_swapped_brush_object_reaches_the_transitioning_surface`。
- 顺带记一条与判据有关的框架事实：样式触发器的 `IsMouseOver`/`IsPressed` 都是
  **`UIElement` 上的属性**（探针打印 `trigger IsMouseOver owner=UIElement`），
  所以由真实输入置起来的状态与测试里直接设属性走的是同一个 DP——但只有真输入能证明"输入→状态"这半段。

## 两条天花板因此撤回

判据可信之后重跑，原先因"像素不动"而 skip 的两条断言直接通过，已取消 skip：

- `Hosted_surfaces_show_no_brand_emerald`：无样式的 Slider 与 ProgressBar 在 220×40 里
  **0 px 品牌绿**。`02-render-ceiling.md` 里"未样式 Slider 画 #207245 共 954 px"那条像素读数不成立；
  它记录的 IL 事实（绘制码读 `ThemeColors`）不在本文件撤回范围内。
- `Check_mark_follows_the_selected_accent`：`ApplyAccent` 之后勾选 glyph 出现在像素里，
  ROADMAP 的 A4"冻结勾选刷"天花板对勾选标记撤回。

闸口现状：`tools/Test-AstraGates.ps1` 全绿，54 通过 / 0 跳过 / 0 失败。

## 又一条判据边界：文字不在这条通路里（Button 第二段量出来的）

`Render(Visual)` 到 Bgr32 缓冲这条路**看不见字形**，三个读数：

- 一个只有文字的捕获（`TextBlock`，前景不透明、字号 14–20）：`painted=0`，`Stable=False`，
  并且耗时到能撞上 fixture 的 60 s UI 线程上限——一次失败连坐同集合后面 4 条测试。
- `HyperlinkButton` 给不透明红底：`8800` px 里 `8788` 是底色、`12` 是边框，**没有任何一像素是字色**。
- 同一个控件的文字元素在树里确实建出来了，`Foreground` 就是调色板那个实例
  （`Assert.Same` 过），`Text="link"`、`FontSize=14`。

所以规则是：**像素断言只用于"面"（填充、描边），文字色一律走读回**。
为此基座加了 `PixelHarness.Build(element, width, height)`——放进已上屏宿主、推帧、建树，
但**不做光栅化**；文字类主题用它，别用 `Render`。
这条限制说的是测量通路，不是"上屏没有字"：Gallery 里字是看得见的，只是我们的离屏缓冲里没有。

**它反过来质疑了一条既有断言**：`Check_mark_follows_the_selected_accent` 数的是 44×44 里的强调色像素，
按上面这条规则，那些像素更可能来自**勾选方框的填充**而不是勾形本身（上游的勾是白字底、不是强调色）。
名字里的"check mark"因此是过强的说法——证据支持的是"勾选态的强调色面跟随 `ApplyAccent`"。
选择批做 `CheckBox` 时要重做这条归因（把勾形单独裁剪或用非强调色的哨兵把它和框分开）。

## Slider 批：看门狗释放错了调度器（2026-09-18）

选择批留了两条"未归因的 60 秒超时"。做 Slider 时同一形状第三次出现，这次跑到底归了因：
`PixelHarness.Pump` 的看门狗是 `System.Threading.Timer`，回调里写的是

```
Dispatcher.CurrentDispatcher.InvokeAsync(() => frame.Continue = false)
```

`CurrentDispatcher` 在**线程池线程**上求值，拿到的是那个线程自己的、没有任何人运行的调度器；
`frame.Continue = false` 因此入队后永不执行。也就是说：只要一次状态变化不带动画、
`CompositionTarget.Rendering` 在预算内不再来帧，`PushFrame` 就永远不返回，
60 秒后由夹具看门狗判超时——而 `PushFrame` 的内嵌循环本来是会处理投递到**它自己那个调度器**的项的。

判据（同一次运行、同样的三步）：`Button` 聚焦后推帧 8 ms 返回；框架自带模板的 `Slider` 也返回；
我们的 `Slider` 修复前挂死 60 s、修复后返回并读到焦点环 `Opacity=1`。
修法是在推帧前抓住调用线程的调度器实例：`var dispatcher = Dispatcher.CurrentDispatcher;`，回调里用它投递。

这条修复把"键盘焦点态"从不可测变成可测：本仓现在有两条真实 `Focus()` 读回
（`A_focused_text_box_widens_only_the_bottom_edge`、`A_focused_slider_raises_its_focus_ring`），
并且 Slider 那条同时是 §"时机不是一帧"里 `Settle()` 的验收。

## `--no-build` 不是"跳过重编"，是"跳过真相"（视觉缺陷批，2026-09-19）

同一份树、同一批断言，两小时内出现两次假读数，都是 `dotnet test --no-build` 给的：

| 读数 | 当时的判据 | 真相 |
| --- | --- | --- |
| 605 条里 1 条红：`AstraScrollHostTests.An_opaque_scroll_host_background_covers_its_own_bar` | "我新加的 300×200 宿主污染了共享窗口" | 二进制比源码旧 4 小时 37 分；重编后 605/605 全绿，那条单独跑、整批跑都不复现 |
| 500 条里 2 条红：MenuBar 两行无消费点 + 目录 36/32 不符 | "CommandBar 批漏改两个闸口" | 同一份过期二进制在读当前 `Catalog.json`；重编后这两条也在 605 里绿了 |

代价不是浪费时间，是**结论方向错**：第一条把注意力整个领到"测试互相污染"这条不存在的路径上，
而真正的前提（源码 15:01/15:24 改过、dll 还是 10:54/10:58）一眼可见却没有被查。
测试数从 605 掉到 500 本来就是最强的信号——用例不会因为换窗口大小而少 105 条。

规则：任何闸口读数之前先比一次时间戳（源码 vs `bin/<cfg>` 里的 `FluentJalium*.dll`），或干脆不带
`--no-build`。读数一旦用来支撑"某条主张不成立"，还要写下它是在哪次构建上取的——
`tools/Test-AstraGates.ps1` 之所以把 restore→build→test 串成一条，理由就是这条。

## `Host()` 的两条边界（阶段 5 第七段量到，2026-09-20）

整窗合成路径（`PixelHarness.Host`）是"半透明叠色"唯一可信的取景方式，但它自己有两条约定的边界，
都是 NavigationView 批撞出来的，原始读数在 `adaptation/s1k-navigation-raw.txt` 的 [C]/[D]：

1. **`Host()` 与 `Build()` 的父级互斥。**`Host()` 把主体挂到它自己的窗口上，所以走过 `Build()`（挂在 Grid 里）
   的元素再交给 `Host()` 会抛 `The logical child already has a parent (child: FluentNavigationView,
   current parent: Grid, attempted parent: Window)`。要合成读数就必须一开始只走 `Host()`，别先 `Build()`。
2. **第一次 `Host()` 的读数与后续不同源。**同一对"选中前/选中后"的整窗捕获，用整张直方图算差值，
   两种跑法分别量到 **30 054** 与 **16 624** 个变化像素——差的不是控件，是窗体背衬与上一位主体留下的像素
   要在这次与下次之间才稳。所以跨捕获比较只能是**单个色键的增量**：同一条断言换成数 `#EAEAEA`
   （选中 pill 叠出来的那个色）之后，两次跑法给出 8 220 / 7 993，未选中那张图里这个键恒为 0。
   阈值取低于两次读数的 6 000。

规则：拿 `Host()` 做判据时，(a) 主体只经 `Host()` 上屏；(b) 断言写成"某个色键在 A 里比在 B 里多 N 像素"，
不要写成"两张直方图差 N 个像素"；(c) N 先量再一次写死，且把两种跑法的读数都记下。

## 判据自己的耐心不够：settle 上限小于它自己要做的两次等待（阶段 6 第三段，2026-09-21）

四遍串行闸口在同一棵树上给出三种结果（绿 / `AstraContentDialogTests` 整类 + 一条菜单 / 一条 NavigationView pill /
一条 TeachingTip 落边），把 pill 那条单跑三次拿到 **1 通过 + 2 次"the pane never settled"**，而那条失败消息里
`{sample.Subject}` 打出来是**空的**——两件事一起指向判据本身而不是控件：

1. **算术**：`Capture` 的判据是"连续两轮抓到同一张图"，一轮最多花 `Pump` 的 400 ms 帧预算；而它给自己的总上限
   也正好是 **400 ms**。也就是说**一轮花满就没有第二轮**，于是一幅完全静止、只是这一拍迟迟不来帧的画面，
   会被判成"never settled"。这不是"图在动"，是**问问题的时间不够问完**——之前"仍未证"第三条记的那次
   "8/16 超时、很可能就是静止不来帧"就是同一件事，只是当时没留下读数。
2. **报告**：失败路径上 `Sample` 的 `Subject` 从没被赋值（`Host()` 不像 `Render()` 会补一句描述），
   所以红字里只剩"the pane never settled: "，既不知道是哪张图、也不知道它试了几轮、花了多久。

改法（都在 `tests/FluentJalium.Tests/Pixel/PixelHarness.cs`，判据的形状不变）：
`Pump` 的预算与 `Capture` 的上限不再各写各的字面量——`SettleBudgetMilliseconds = PumpBudgetMilliseconds * 3`，
即"至少够问完两轮再加一轮余量"；三轮是能回答这个问题的最小上限，真正在动的图照样不过，只是先被问过。
超时那条路现在保留 `Rounds` 与新增的 `CaptureMilliseconds`，`Host()` 也补上 `Subject`。
于是"never settled"从一句话变成两种可读形状：**轮数多 = 图真的在变**，**轮数 1~2 且 ms ≈ 上限 = 帧来晚了**
（后一种现在是判据的事，不是被测控件的事）。

配套一条 measured 记录：pill 那条改后**单跑 4/4 绿**，再在 8 个 bash 空转进程的 CPU 负载下**仍 3/3 绿**
（1 s / 4 s 一次），随后 `AstraNavigationTests + AstraTeachingTipTests + AstraDividerTests` 三类
**165/165 绿**（2 m 35 s，Debug 0 警告 0 错误）；再之后**整套串行闸口第五遍全绿**——
Debug 0 警告 0 错误、**1288/1288、0 失败、0 跳过**（5 m 28 s）、调色板三行 `checked=True`、
`keys.md is current: 1257 canonical lines.`、`All Astra gates passed.`，而这是连着三遍红之后的第一遍绿。
**不能**把这遍绿当"已修"：改后再没能抓到一次红，所以"来帧慢"的确切触发条件仍未证——它只在整套顺序跑
（多个窗口与多个 DX12 设备并存）时出现过；#35 / #47 / #48 因此都还开着。这次真正的收获是：
下次再红，红字会自己说是哪一种（轮数与毫秒都会写在消息里）。

## 宿主窗口是谁给的：ContentDialog 整类失效的成因（阶段 6 第四段补，2026-09-21）

#35 从阶段 3 挂到现在，形状一直是"单跑全绿、整套顺序跑从某一格起整类抛
`ContentDialog could not resolve a host window.`"。这次不再猜时机，去量**这个进程里到底谁算宿主窗口**。

先说清楚证据的分量：解析顺序是从兄弟源码树读来的（`Jalium.UI.Controls/ContentDialog.cs:786-798` →
`DialogOwnerResolver.ResolveWindow`：先 Win32 `GetActiveWindow()`，再 `Application.Current.MainWindow`），
而兄弟树**可能比钉住的 26.10.9 新**，所以那句话只算线索，结论一律以 26.10.9 上的实测为准。
`spike/HostWindowProbe` 一个进程四次读数，逐字如下：

```text
A  host shown, MainWindow unassigned: host=0x1CA15FC active=0x1CA15FC foreground=0x2871238 app.MainWindow=null
   dialog opened: Visibility=Visible
   neighbour shown: neighbour=0x1A910D8 active=0x1A910D8 app.MainWindow=null
   dialog opened: Visibility=Visible
B  neighbour closed: active=0 foreground=0x2871238 app.MainWindow=null
   dialog threw: InvalidOperationException: ContentDialog could not resolve a host window.
C  app.MainWindow = host: active=0 app.MainWindow=Probe host
   dialog opened: Visibility=Visible
```

三件事一次量清：

1. `Window.Show()` **不会**把窗口写成 `Application.MainWindow`（A/B 两读都是 null；源码里那条赋值只出现在
   `Application.Run` 与 `JaliumApp` 的启动路径上）。本仓库的测试宿主从来不用 `app.Run(window)`，
   所以框架那条回退通路在我们的进程里**永远是断的**，每个对话框都吊在第一步上。
2. 线程的 active 窗口**会因为"另一个窗口关闭"而归零**（B 读），归零之后对话框直接抛——
   顺序跑里任何一处开合窗口（弹层自己的顶层 `PopupWindow` 首当其冲）都能制造这个状态，
   而且它是**线程亲和的持久状态**：一旦归零，之后每一个对话框都失败，直到有窗口重新拿到激活。
   这就是"整类从某一格起全红"的机制，也是为什么单跑这一类永远是绿的。
3. 把宿主窗口写成 `Application.MainWindow` 之后，active 仍是 0 而对话框照样打开（C 读）——
   回退通路一旦可用，判据就不再依赖前台激活这种本不该由测试赌的东西。

修法落在判据侧而不是产品侧：`tests/FluentJalium.Tests/Pixel/PixelHarness.cs` 建宿主窗口时，
若 `Application.Current.MainWindow` 为 null 就把它指过去（只在 null 时，别的地方命名过就不动）。
回归测试是 `AstraContentDialogTests.The_dialog_resolves_the_host_the_harness_names`
（读回 `Application.Current.MainWindow` 就是 harness 那一个宿主窗口，并真的开一次对话框）。
**A/B**：只加测试不改 harness → 红（`Assert.Same()` 不同实例，MainWindow 是 null）；改 harness →
ContentDialog 整类 **39/39 绿**，整套里 `ContentDialog could not resolve a host window.` 这句**一条也不剩**。

这一段同时踩到一个**新判据坑**，按原样记下来比藏掉值钱：第一版回归测试照探针的样子在测试里开一个邻居窗口再关掉，
ContentDialog 整类照样绿，可整套跑的红从 20 涨到 **30 条、散在 15 个类**，错因换成一片
`No part named SuggestionsContainer` / `PART_DropDownItemsHost` / `the open dropdown never reached the overlay layer`
外加几张 `capture never settled: #000000x7920` 的空屏。也就是说**在同一根共享 UI 线程上关一个窗口，
会把之后所有依赖覆盖层的断言一起毒掉**——探针里那句 `active=0` 不是只影响对话框。
复现因此留在探针（一次性进程）里，测试只做不变式检查，注释里写明不许再在这条通路上开合窗口。

仍要留一条**未证**：解析顺序是先 active 后 MainWindow，所以"active 恰好是别人的窗口"这一格本段没有治——
B 之前那次"邻居活着时开对话框"其实把对话框开进了**邻居**窗口（active 就是它），
而 harness 只有长期唯一宿主，正常路径碰不到；若某个类在弹层还开着的时候建对话框，它可能挂到弹层的顶层窗口上。
`AstraAppBarTests.The_open_bar_shows_its_overflow_...`（`popupOpened` 读回假）与 #47 / #48 都在这一带的
同一张因果图上，本段没有把它们结清，只是少了一个会持续污染整类的假红来源。

## 仍未证

- 半透明刷（`ControlFillColorSecondaryBrush` 这类带 alpha 的令牌）在 `self` 路径上能否被正确区分——
  alpha 字节不可信，这条只能等整窗裁剪基座做出来再验。**NavigationView 批已经把整窗那条路走通了**
  （见上面"`Host()` 的两条边界"：叠色用单色键增量断言），但 `self` 路径本身仍未解决。
- 混合 DPI（1.0 / 1.25 / 1.75）下的计数一致性：只在本机 1.75 上证过 1:1 这条规则。
- 静止场景"来帧"的确切条件：run1 出现过一次 8/16 超时，阶段 6 第三段又抓到两次同形状的
  "the pane never settled"。**判据侧的那一半已经归因**（见上一节：上限只够一轮，静止但迟来的帧必然被判不稳），
  并且把上限改成派生量之后**再没能复现出红**（单跑 4/4、8 个 CPU 空转下 3/3、三类 165/165）。
  仍然未证的是另一半：**什么条件下帧会迟到到花满一轮**——它只在整套顺序跑时出现过，改后没有再红过，
  所以不能宣布已修（#35 / #47 / #48 保持打开）。
- 除 Button / ScrollViewer+ScrollBar / Popup+FlyoutPresenter / ToolTip / 窗口外壳之外的隐式样式归因。
  另外**真实输入→状态**这一半只在悬停上证过一次，且不进闸口（见 `audits/button.md` 的指针通路一节）。

## 阶段 6 第五段补：捕获目标看不见它自己的 RenderTransform

RatingControl 要把上游那条"画在 32、显示在 0.5"搬到本运行时，第一件事是问 `RenderTransform` 到底动不动墨。
探针（`spike/RatingProbe`，读数 `adaptation/s1s-rating-raw.txt` [E]）前两行给的答复是"不动"：60×60 实色块挂上
`ScaleTransform(0.5,0.5)`、原点 `0.5,0.5`，属性读回正确，`Count` 仍是 3 600 像素——与 `Clip` 那条死通路一个形状。

差别在捕获对象。**换成更宽的祖先再量，缩放是真的落墨**：同一枚块从 200×200 宿主里读到 `x=15..44 y=15..44`、
900 像素，无变换对照读到 `x=0..59`、3 600 像素。也就是说 `RenderTargetBitmap.Render(target)` 这张图
**按目标的布局盒出图、对目标自身的 RenderTransform 没有分辨力**，这与本文件早已写下的"捕获目标的尺寸会伪装出裁剪"
（`ClipToBounds` 那一节）是同一条仪器账的两种表现：**要看一个要素自己会不会改变自己的成像，必须从祖先那边看。**

对本段的影响：控件的墨量断言一律走祖先；本段的 `PixelHarness.Render` 通路（自捕获）不参与星级断言，
所以 `AstraRatingControlTests` 里 0 条像素用例不是漏，是判据在这条通路上没有分辨力。
仍**未证**：`LayoutTransform` 与 `RenderTransform` 在自捕获下是否同样表现（本段没量前者）；
祖先捕获在 DPI≠1.75 下的像素数比例（沿用本文件既有的"只在本机 1.75 证过"这条限制）。

## 阶段 6 第六段补：几何有墨、文本无墨，同一张白底同时量到（`spike/IconFamilyProbe`）

图标族需要一个"能不能主张墨"的判定，做法是把被测要素和一枚必须出墨的东西放进同一张 64×64 白底里读：

| 要素（都在 `Foreground/Background = #0078D4`） | 非白像素 | accent 像素 | 墨盒 |
| --- | --- | --- | --- |
| `Border` 20×20 实心（正对照） | 400 | 400 | 20×20 @22,22 |
| `PathIcon` 闭合方块，显式 20×20 | 400 | 400 | 20×20 @0,0 |
| `PathIcon` 同一几何、不给尺寸 | 4 096 | 4 096 | 铺满 64×64 |
| `PathIcon` 开口折线 `M0,0 L16,16` | 0 | 0 | 空 |
| `TextBlock` "IIII" 20px（负对照） | 0 | 0 | 空 |
| `TextBlock` 图标字体字形 U+E710 | 0 | 0 | 空（DesiredSize 22×22，说明它确实测量了） |
| `SymbolIcon` / `FontIcon`（默认与显式前景各一次） | 0 | 0 | 空 |

三条账：

1. **几何墨到得了像素，文本墨到不了**——同一张图里 `Border` 和闭合 `PathIcon` 各读回 400 像素，而纯文本 `TextBlock`
   读回 0。这把 §S1-r 第 3 条"字形无墨"的定位收紧了：瞎的不是图标控件，是**文本这条路**；
   所以图标族的像素列只允许由 `PathIcon` 的填充承担（`A_closed_geometry_reaches_the_pixel_and_fills_the_slot_it_is_given`），
   `SymbolIcon`/`FontIcon` 的"画没画"在现有通路上**不可证**，退到树上读数与度量盒。
2. **开口几何无墨不是判据坏了**：`PathIcon` 只填不描，`M0,0 L16,16` 面积为零。用一条折线去问"图标能不能成像"
   会得出和 §S1-r 一样的错误结论——问法要先能出墨。
3. **`PathIcon` 在没有显式尺寸时按槽位铺满**（64 槽 → 4 096 像素，等于 Stretch=Fill 语义）。凡以"墨量 = 尺寸²"
   做断言的用例，被测要素必须给死尺寸，否则量到的是宿主窗口的大小；这与本文件早先"捕获目标的尺寸会伪装出裁剪"
   同族——**先确认墨盒属于谁，再读墨盒**。

对照环境：`RenderContext` 自动后端 + `RenderingEngine.Impeller`，真实 `Application` + 已 `Show()` 的窗口 +
`CompositionTarget.Rendering` 计帧后读回；窗口只在本机存在期间被读取，不落用户目录。

## 暗色半透明表面在离屏捕获里读成"白"——一条被误记成产品缺陷的仪器账（#58 复查，2026-09-21）

结清五处 1 DIP 分隔线那批（`adaptation/00` §S1-s）加暗色腿时，同一张表格在两档主题下白像素数几乎相等
（Light `#FFFFFFx85668` / Dark `#FFFFFFx86230`），当时据此记下一条"暗色下表格族那片表面仍是白的"的缺陷（#58）。
**这条主张撤回**：复查量到的不是产品行为，是捕获读法。三点实测（`AstraDataGridTests.A_dark_surface_token_only_reads_true_over_a_matching_backdrop`）：

1. 暗色卡片表面令牌 `CardBackgroundFillColorDefaultBrush` 是**半透明白** `#0DFFFFFF`（上游同形：暗色卡片是叠在页背景上的一层），
   所以"表面是不是白的"这个问题本身，在这套调色板里没有一块不透明的暗面可问。
2. 给同一个网格一张 `#202020` 的页底，采样点读回 **`#2B2B2B`**，正是 `32 + (255-32)*13/255` 的 source-over 结果；
   换成洋红底（`#FF00FF`）读回 **`#FF0DFF`**。两条一起说明：合成是对的，而且灰底单独不构成证据（白叠灰还是灰，
   只有非灰底能把"丢 alpha"和"真合成"分开）。
3. 什么底都不给时同一采样点读回 `#FFFFFF`，而把我们的 `Background` setter 改成 `{x:Null}` 之后同一点读回 **`#000000`**。
   所以那张图里"白"是**部分透明像素的 RGB 三元组**：`PixelAt`/`Sample` 走 `Bgr32`，不读 alpha 字节——
   "有没有东西在它后面"这件事在读数里根本不存在。

跨批意义（比这条本身重要）：**凡是暗色腿用半透明令牌（卡片面、`SubtleFillColor*`、`ControlStrokeColor*`、`DividerStrokeColor*`）
做的像素主张，样本必须自带一张不透明底**，否则它测的是 alpha 被丢掉之后的 RGB。本仓库里 `Render()` 的裸裁剪区一直是
按"数非背景像素"和"比两档 Top(2) 不同"在用，这两种写法都不会因此报错，但也都不证明暗色表面落到了像素——
要证就按第 2 条那样给底并断准混色值。

对测（牙）：把 `Styles/DataGrid.jalxaml` 的 `<Setter Property="Background" Value="{ThemeResource DataGridBackground}" />`
改成 `{x:Null}` → 该用例红，消息点名读数 `the grid surface reads #202020 over a #202020 page, not the card token's
composite #2B2B2B (token #0DFFFFFF)`；改回 → 三个相关类合跑 **75/75 绿**（`0 警告 / 0 错误`）。

### 补：这条读法当场抓出一处指错的墨（滑块族，同一天）

把同一把尺子拿到滚动条上去量"暗色滑块到没到像素"，得到三条新账：

4. **同 RGB 的两个令牌，裸捕获分不开——而且会认错人**。暗色下 `ScrollBarThumb` 是 `#8BFFFFFF`、`ScrollBarTrack` 是
   `#0FFFFFFF`，两者 RGB 都是白，所以老断言 `dark.Count(Colors.White) > 48` 其实一直在数**轨道**的墨。把轨道钉成不透明
   洋红诱饵之后：白键整个消失（0 格），滑块的墨出现在它真正落在的地方——洋红之上，`#FF8BFF` 实测 **64 格**，正是
   `Over(#FF00FF, #8BFFFFFF)`。跨批意义：断言"某个半透明令牌到了像素"之前，要先回答"这张图里还有谁带同一组 RGB"，
   办法是把别人钉成诱饵或给自己配底，然后断**合成值**而不是断颜色名。
5. **`RenderPart<T>` 把部件单独成像，祖先不进去**：把 `Backdrop(plate)` 包在 `RenderPart<ScrollBar>` 外面，裁剪图里
   既没有 `#202020` 也没有任何合成值（读数仍是 `#000000/#FFFFFF/#D2D2D2`）。要"带底合成"必须走 `Render(wrapper)` 或
   `PixelAt(wrapper,…)`，也就是**由包裹层往下成像**；`RenderPart` 只适合"这个部件自己画没画"那种不依赖背衬的主张。
6. 顺带量到：`light.Count(Colors.White) == 0` 说明滚动条族的裸裁剪里"白"只可能来自暗色那两支半透明白，
   而不是背景——这条使得上面那句"认错人"只在暗色腿成立，两档共用一条断言时要分开写。

这两条判据各自验过牙（A/B，读数细节在 ROADMAP 同批）。把 `OnPlate` 的页底改成 alpha=0、期望算式一律不动 → 表格与树两条各红，
`ink=#FFFBFBFB count=0`，而同一裁剪图的众数是 `#FFFFFF` 51 908 格——第 3 条"没有底时报自己的 RGB"就此在断言里现形。另一条只把
`ScrollBarThumb` 钉成不透明绿、诱饵腿不动 → 滑块用例红（`ink=#FFFF8BFF count=0`），并且那 64 格整块从 `#FF8BFF` 搬到
`#00FF00`：换掉谁，墨就跟着谁走，所以被数的确实是滑块自己那支刷子（第 4 条的归因由此成立，而不是只靠"数量对得上"）。

### 补二：把"按名取部件"当成可靠抓手之前，先量它取到的是谁（同一天，#60 全库改判）

11 条两档像素主张一起换形状时，撞到三条只会靠"读回"才现形的仪器账：

7. **同名部件在一棵树里可以有好几个，而先命中的那个可能全是透明的**。`TreeView` 的 `ContentBorder` 每行一个、控件自己
   还有一个，`Named()` 的深度优先取到的是行上那支 `#00000000`——它的合成值恰好**等于页底**，于是"这条主张不可能被证伪"
   的守卫把它当场判死（读数：`The page and implicit/no-template@292x28's brush composite to the same colour`）。
   跨批意义：**按名取部件不等于取到了被主张的那个要素**；要么按 alpha>0 过滤（本仓库加了 `NamedSurface`），要么就在
   断言里把"取到谁"打出来。全透明的部件本身也是一条事实：它说明该处根本没有可主张的表面。
8. **本运行时的 `ContentPresenter` 没有 `Background` 成员**（编译期 CS1061）。从树上读"谁带着背景刷"只能走
   `Border`/`Panel`/`Control`/`Shape.Fill` 四条路。把 `ContentPresenter` 写进这类遍历会在编译期就红，不会静默漏掉——
   这条与 §S1-q"未知属性在标记里静默丢弃"是相反方向的坑，值得记住哪一类是响的。
9. **"硬编码的颜色"要先读回再定罪**。InfoBar 那片底曾被当成不透明字面量，实际读出来是 `#80F6F6F6`——50% 灰。
   它是否合理是另一回事，但"它是常量还是令牌""它透不透明"这两个问题都必须从建好的树上读，而不是从标记的字面或记忆里读。

## 文本字形到不到像素：用"同主体带字/不带字"的差分把 #50 量死（2026-09-21）

`AstraOneDipRulesTests` 那批留了一条 38 格的读数（菜单项裁剪里唯一不是页底的墨），当时顺手记成"子菜单箭头"。
把它当反例重新量一遍，结论是 #50 站得住，而且现在有了能重跑的守卫（`AstraTextInkTests`，6 条 theory）：

1. **差分才是对的仪器**。同一个主体拍两次——带文本与不带文本——只有两次histogram 不一样才说明"字出了墨"。
   这不需要先知道裁剪里还有什么（表面、描边、箭头、窗口外壳都同进同出）。实测：
   `Button{Content="Hello World"}` 与 `Button{Content=""}` 在 200x44、亮/暗、白底/暗底/裸捕获下**直方图逐字节相同**
   （亮档：表面 8296 + 描边 456 + 12 + 8）；`MenuFlyoutSubItem` 带不带 `Text` 也都是同样那 38 格；
   `TreeViewItem` 带不带表头都 0；独立 `TextBlock` 在配底的两张纸上 off-plate 恒为 0。
2. **正控制证明"0"不是仪器瞎**。菜单项那 38 格在不透明洋红底上**颜色一字不变**（`#6E6E73x16`，暗档 `#D1D1D6x16`），
   即它是自带 alpha 的不透明墨、且随主题翻转——那就是箭头的几何墨。**几何与表面到得了像素，字形到不了**，
   所以 #50 的准确说法要收窄成"文本 run 不打印"，而不是"捕获通路看不见细的东西"。
3. **裸捕获一个孤立 `TextBlock` 连"稳定"都拿不到**：`Stable=False`、整幅全黑。带底之后才稳定并读出 0。
   推论：`Stable=False` 在这种主体上是"没有东西可画"的信号，不是"帧泵不够"的信号。
4. `Host()`（真上屏窗口那条路）也试了：带字与不带的空 `TextBlock` 读数同样一致（`#F3F3F3x12848` 那块是主体自己的
   尺寸占位，随主题翻的 `#1A1A1Ax30 / #FFFFFFx30` 是窗口外壳），因此没有把字形带回来。这条只做成了测量，
   **没有做成用例**（`Host()` 的首帧与后续帧本来就不可比，见上面第 5 节），守卫用可确定的配底腿承担。

**A/B**：给孤立 `TextBlock` 钉一支不透明 `Background(#123456)` → 该用例两档各红（`Assert.Equal` 读出 off-plate 不为 0），
其余 4 条不动；改回 → 6/6 绿，`tests/` 与 `src/` 里 `A/B mutation` 零命中。牙是这么证的，不是"它现在绿"推出来的。

后果与用法：任何"某个控件的**文字**该长什么样"的主张，在本运行时都不能靠像素断言，只能靠树上读前景令牌
（`audits/foreground.md` 那套）；`AstraTextInkTests` 是这条限制的**哨兵**——上游一旦开始打印字形，它会红，
届时把哨兵删掉、把逐控件的文本像素主张打开。#50 因此从"未测"变成"测死并已设防"，但**它仍是缺陷**（用户看不见文字），
只是那一条属于上游渲染，不属于本仓库能改的形状。


