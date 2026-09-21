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
