# 减动效与时长键化审计

上游权威：WinUI 3 `microsoft-ui-xaml` @`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
运行时权威：NuGet `Jalium.UI` 26.10.9（与测试工程解析到的同一套程序集）

这一段不属于任何控件族，它落在目标里"并行"那一行的**减动效资源键化**。出口是三件事：把模板过渡时长改成上游同名的公开资源键、
让 `FluentThemeManager.ReduceMotion` 真的写进那些键、并把"哪些行形状能载住值"量死。没有新增自有类型——动效在这个运行时
不需要新类型，需要的是一个能被重写的值。

## 0. 上游的文件与锚点

| 事实 | 锚点 |
| --- | --- |
| 三条控制过渡时长 + 一条 after 时长 + 一条 spline，全部 `<x:String>` | `controls/dev/CommonStyles/Common_themeresources_any.xaml:602-606` |
| 滚动条四条时长（167/83/83/167） | `controls/dev/CommonStyles/ScrollBar_themeresources.xaml:173-176` |
| 分隔线两条时长（各 100） | `controls/dev/CommonStyles/ScrollViewer_themeresources.xaml:16,19` |
| `ScrollView` 三条（100/100/500） | `controls/dev/ScrollView/ScrollView_themeresources.xaml:16,19,23` |
| ComboBox 条目缩放 167 | `controls/dev/ComboBox/ComboBox_themeresources.xaml:330` |
| SplitView 面板 200/199.99/100——NavigationView 的面板就是 SplitView | `controls/dev/SplitView/SplitView_themeresources.xaml:10-12` |

上游这些字符串是给 `Storyboard` 的 `Duration` 属性吃的（WPF/XAML 的 `DurationConverter`）。本运行时没有
`VisualStateManager`，模板动效一律走 `UIElement` 的自动过渡，所以**没有一个 Storyboard 能吃这些字符串**——这是下面
"行形状"问题的来源：值的种类从"字符串 + 转换器"变成了"依赖属性本身就要求 `Duration` 对象"。

## 1. 运行时的动效模型（全部为 26.10.9 读数，源码树只用来提问）

| 事实 | 读数 |
| --- | --- |
| `TransitionDuration` 是什么 | `Duration` 型依赖属性，注册在 `UIElement`（不在 Control 上，每个要素都有），默认 **180ms** |
| 什么时候读它 | 过渡将要启动的那一刻现读；`has no TimeSpan` 或 `<= 0` 直接**不起动画** |
| 三种状态怎么区分 | `default(Duration)` → `Automatic`；刻意的 0 → `HasTimeSpan=True` 且 `00:00:00`；被丢弃的行也是 `Automatic` |
| 框架自己的减动效门 | `Application` 静态构造里：`AutomaticTransitionsEnabledProvider = () => SystemParameters.ClientAreaAnimation && UIEffects`（`Application.cs:55`） |
| 时长转换器吃什么 | `"0:0:0.083"`/`"00:00:00.083"`/`"0"` 都好；**`"Auto"` 抛 FormatException** |
| 时序曲线 | `TransitionTimingFunction` 是枚举（`Recommended` 等），没有可放 `KeySpline` 的属性 |

两点决定设计。第一，OS 的可访问性开关**框架已经自己管**——这条门是 internal 的，我们既不该也不需用反射去碰
（AGENTS.md 禁反射进框架私有成员）。所以"减动效资源键化"要解决的是**应用内自服的开关**，也就是 Gallery 设置页那个。
第二，既然 arm 时才读、且 0 就是不动，那么"把键写成 0"就是正确的机制——问题只剩"资源行能不能载住一个 `Duration`"。

## 2. 行形状矩阵（`spike/MotionProbe`，原始读数 `adaptation/s6-motion-probe-raw.txt`）

四种消费形状都量了：要素属性 `{ThemeResource}`、属性 `{StaticResource}`、隐式样式 Setter、**ControlTemplate 内部**。

| 行写法 | 字典里存住的 | 四种形状的读数 |
| --- | --- | --- |
| `<Duration>`（框架 xmlns 下的无限定名） | `Duration 00:00:00.0830000` | **83ms**，四种全过，模板内也过 |
| `clr-namespace:Jalium.UI;assembly=Jalium.UI.Core` | 行解析失败 | 类型解析不到（转发壳那笔账，见 `audits/icon-family.md` §1） |
| `clr-namespace:Jalium.UI;assembly=Jalium.UI.Managed` | 行解析失败 | 同上——运行时类型只能用框架自己的 xmlns 引 |
| `<x:String>0:0:0.083</x:String>`（**上游写法**） | `String "0:0:0.083"` | **180ms**：不报错、不落地，交给框架默认值 |
| `<x:String>00:00:00.083</x:String>`（上游字面文本） | `String "00:00:00.083"` | 同上，180ms |
| `<sys:TimeSpan>00:00:00.083</sys:TimeSpan>` | `TimeSpan "00:00:00"` | 180ms；**且小数在存行时就丢了** |
| `<x:Double>` | 行解析失败 | 与样式文件里已有的"x:Double 这个 reader 读不了"一致 |
| 字面量 `TransitionDuration="0:0:0.083"` | — | 83ms（正对照，证明探针没坏） |

结论一句话：**上游的名字与数字照抄，行类型必须换成框架自己的 `Duration`。** 这不是"能不能"的选择——`x:String` 那一行
是静默失效家族的第四个成员（前有 `x:Double`→0、错枚举名→另一成员、`FontFamily`→空 Source），而且比前几个更阴：
它交付的不是 0 而是 180ms 的框架默认值，看上去"有动效"，实际谁也不知道时长从哪来。

## 3. 落地设计

| 部件 | 做法 |
| --- | --- |
| 键的归属 | 新增 `ThemeResources/Motion.jalxaml`（3 行 `<Duration>`），进 `Themes/Manifest.txt`，与 Metrics 同层 |
| 模板侧 | 21 处 `TransitionDuration` 字面量全部改读键：16×`ControlFasterAnimationDuration`、4×`ControlFastAnimationDuration`、1×`SplitViewPaneAnimationOpenDuration`（NavigationView 的 `PART_PaneRoot` 宽度，正是上游 SplitView 那条） |
| 写键的一侧 | `FluentThemeManager` 持有该字典实例；`ReduceMotion` 置位时逐键写 `Duration.Zero`，复位时写回设计值 |
| 设计值的权威 | 启动时从**已加载的行**里读回（`CaptureMotionDesigns`），代码里不再抄一份 83/167/200 |
| 静默失效的兜底 | 若字典里读不回任何 `Duration` 行，`Apply()` 直接抛——行丢了就是启动失败，不再退化成 180ms |
| 主题切换 | Motion 字典独立于 Light/Dark 调色板，`RefreshPalette()` 不碰它，所以换主题不会把动画还回去（有测试钉住） |

## 4. 键清单（进 `audits/keys.md`，1295→1298）

| 类型 | 键 | 值 |
| --- | --- | --- |
| Duration | `ControlFasterAnimationDuration` | `00:00:00.083` |
| Duration | `ControlFastAnimationDuration` | `00:00:00.167` |
| Duration | `SplitViewPaneAnimationOpenDuration` | `00:00:00.2` |

上游其余时长键**不发布**：`ThemeResources` 与键消费闸口一起管——一条没人读的行会被闸口判红（Metrics.jalxaml 顶上
记着这条规矩的来历）。`ControlFastOutSlowInKeySpline` 是另一回事：本运行时的过渡时序是枚举，没有属性接 spline，
抄过来就是一行永远读不到的字符串。

## 5. Known Gaps

1. 上游 13 条时长键只转录了本库消费的 3 条；其余随消费它的批次一起进来，不预先占名。
2. `ReduceMotion` 只管**下一次**过渡：已经在跑的过渡按自己的时钟走完（arm 时读值，运行中不重读）。
3. 代码侧动画器里 NavigationView 指示器的 600/200ms 与页面淡入的 167ms 是上游没有键的数，仍按
   `AnimationsEnabled` 整体开关，不发布伪键；`ProgressRing` 的持续自转不受减动效影响（上游也未证，见 `audits/progress-ring.md`）。
4. 测试套件不断言"动画确实在跑"——那依赖机器的 `ClientAreaAnimation`/`UIEffects`。这条只在探针里量过，读数与前置
   条件一起记在 `s6-motion-probe-raw.txt`；套件断言的是属性读回值。
5. 高对比与减动效的交叉未量（上游也无对应物）。
6. 真指针 hover 的时长验证仍缺（与 #13 同一笔账）：现在能证明"时长来自键、键能改"，还不能证明"鼠标进去第 N 帧落在哪个色"。
7. Gallery 的 Motion 系统页仍未建（#10 未完的部分）；设置页那张卡的文案已按真实覆盖面改写。

## 6. 四类证据

- **构建**：串行闸口的数与结论写在 `docs/astra/ROADMAP.md` 本段末尾，本文件不重复抄一遍数（抄了就会过期）。
- **行为**：`tests/FluentJalium.Tests/AstraMotionTests.cs` 9 条——三键逐条读回毫秒数、行形状与字面文本的转录闸、
  已实现模板三个部件各读自己的 `TransitionDuration`、翻 `ReduceMotion` 后同一要素从 83ms 变 0ms、减动效期间新建的
  控件"生下来就是静的"、切主题不还回动画、以及全库 21 处过渡声明逐处要求它读已发布键（字面量即判红）。
  **牙的验证**：把 setter 里的 `ApplyMotionKeys()` 摘掉后重跑，正好 3 条红（其余 6 条绿），证明红的就是减动效那三条。
- **视觉**：`spike/MotionProbe` [4]——400ms 的红→蓝过渡中途读回 `#DB0093` 且 1600/1600 像素两端都不像；同一行经
  `{ThemeResource}` 喂进去读回 `#DC0091`、1600 在中间；行写 0 之后立刻 `#0000FF`、1600 全是终色、0 像素在中间。
  正反对照同一次跑，判据本身不背这个结论。
- **硬件输入**：本段不动输入路径；鼠标/触摸/键盘三条沿用各控件族已有的读数。
