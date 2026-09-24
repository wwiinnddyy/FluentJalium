# Astra 缺口清点（四类：控件 / 样式 / 动画 / 布局）

<!-- 手写的横向清点，不是生成物，也不在闸口里。每个数都写清是哪一天算的、用什么命令当场重算。
     读数日期 2026-09-25，树 `e1f3366`。运行时权威 NuGet Jalium.UI 26.10.9；上游权威
     microsoft-ui-xaml @`19e3bdc3c`——本批对参考仓只读不改，脚本顺带打印了它的工作树状态。 -->

这份文档只做一件事：把散在 45 篇 `audits/*.md`、13 篇 `adaptation/*.md` 与 `ROADMAP.md` 里的"还差什么"排成一张可清点的表。
它**不新增任何主张**——某个控件的某一行能不能上屏、某个状态有没有驱动器，判据一律在那些文件里，
这里只给指针；三条生成物（`audits/keys.md`、`audits/known-gaps.md`、`adaptation/05-native-control-vacuum.md`）
仍然是权威，本文件与它们冲突时以它们为准。

## 1. 四个数

| 量 | 读数 | 怎么重算 |
|---|---|---|
| 样式宇宙 | 60 条隐式 TargetType = **48 audited / 11 own-type / 1 no-upstream-control / 0 ported**，13 页 Gallery | `grep -o '"parity": "[a-z-]*"' samples/FluentJalium.Gallery/Catalog.json \| sort \| uniq -c` |
| 控件真空 | 原生 163 类型 / 范围 84 名 / 可走样式层 56 / **已上隐式样式 30** / **真空且无替身 23** / 需自有类型 28 / 范围外未样式 92 | `powershell -File tools/Report-Control-Vacuum.ps1`（本批重跑后落库，被它冲掉的手写复测按 §6.1 拼回） |
| 上游主题键 | `controls/dev/**/*_themeresources.xaml` 共 **2903** 个 `x:Key` 名；我们发布 1063 个名字；**重合 798，未转录 2105** | `bash spike/GapInventory/keys-diff.sh`（读数已存 `spike/GapInventory/keys-diff-raw.txt`） |
| 我们的键 | `audits/keys.md` **1312** canonical 行 / 68 份字典（368 调色板、784 令牌、160 样式） | `powershell -File tools/Report-AstraResourceKeys.ps1 -Check` |

## 2. 差什么控件

### 2.1 原生有类型、我们一条隐式样式都没上（23）

补样式就能解决的那一类。`Report-Control-Vacuum.ps1` 的 A 段现在就是这 23 行，按"会不会被真实界面撞上"排：

| 会不会被用户撞上 | 名单 |
|---|---|
| 会——WinUI 界面里高频出现 | `TabControl`、`Label`、`Calendar`、`DatePicker`、`TimePicker`、`ColorPicker`、`RichTextBox`、`ToolBar`、`StatusBar`、`GridSplitter`、`GroupBox` |
| 底座类，宿主可能自己搭 | `ContentControl`、`HeaderedContentControl`、`ItemsControl`、`Frame`、`GroupItem` |
| 实测判死，补样式解决不了 | `ScrollBar`（隐式样式与 `Template` 都测不动像素，只有命名刷可达）、`TitleBar`（只承接框架 8 个钩子里的 6 个）、`Window` |
| 与 WinUI 无对应物，本层刻意不做 | `NavigationWindow`、`ResizeGrip`、`Thumb`、`UserControl` |

`ScrollBar` 那一行的判据在 `audits/scrollbar.md`；`Thumb` 挂在 ROADMAP D 节第 1 步"与悬停/拖拽输入证据同批"。
另有六个**类型面存在、但不在这 23 行里**的（§2.2 重测量出来的，因为工具量的是 `Control` 宇宙）：
`MenuFlyout`、`CommandBarFlyout`、`GridView`、`Hyperlink`、`Page`、`WrapPanel`——
"要不要给它们一份外观"这个判断本仓**还没做过**（`MenuFlyout` 那层表面另说，它按弹层量过，见 `audits/menu-flyout.md`）。

### 2.2 原生没类型（D 段 28 → 重测真缺 22 → 待决 14）

`adaptation/05` 的 §F 早就给自己定过一条规矩："以后缺什么类型只按 `Type.Name` 全程序集量"。
本批照这条把 D 段 28 个名字逐个在 26.10.9 的导出类型表里查了一遍（读数在
`spike/GapInventory/keys-diff-raw.txt` 末段，命令 `bash spike/GapInventory/keys-diff.sh`），结果是
**6 条假阴性**：`CommandBarFlyout`（`Jalium.UI.Controls.CommandBarFlyout`）、`GridView`
（`Jalium.UI.Controls.GridView`，WPF 那一族 `ViewBase`，不是 WinUI 的列表控件）、
`Hyperlink`（`Jalium.UI.Documents.Hyperlink`）、`MenuFlyout`（§F 已判过那条）、
`Page`（`Jalium.UI.Controls.Page`）、`WrapPanel`（`Jalium.UI.Controls.WrapPanel`）。
它们不是"没有类型"，而是**不是 `Control`，所以进不了 163 那个普查宇宙**——这六个的缺口该按 §2.1 判
（有类型、零样式），其中 `WrapPanel`/`Page`/`Hyperlink` 连"要不要做"都没人表过态。

**真缺 22 个里，已用自有类型交付 8 个**：`BreadcrumbBar`、`DropDownButton`、`InfoBadge`、`ProgressRing`、
`RadioButtons`、`RatingControl`、`TabView`、`TeachingTip`。**剩下待决 14 个**：

| 类别 | 名单 |
|---|---|
| 列表/布局基座（整块，见 §5.1） | `Repeater`/`ItemsRepeater`、`LayoutPanel`、`ItemsView`、`ItemContainer`、`SplitView`、`TwoPaneView`、`AnnotatedScrollBar` |
| 弹层与菜单 | `Flyout`、`RadioMenuItem`（`audits/menu-flyout.md:15` 还点名了不在 D 段里的 `FlyoutPresenter`、`MenuScroller`、`MenuScrollViewer`、`RadioMenuFlyoutItem`、`SplitMenuFlyoutItem`——它们不在 ModernWpf 的分母里，所以工具数不到） |
| 导航与外壳 | `NavigationBackButton` |
| 现代控件 | `PagerControl`、`PersonPicture`、`SelectorBar` |
| 外观已交付、真名仍缺 | `AutoSuggestBox`：交出去的是 `AutoCompleteBox` 穿的行，`audits/autosuggestbox.md` 的不声称第一条就是"不声称这是一个 AutoSuggestBox" |

`Card` **不在待决名单里**——它已经判过：`audits/teachingtip.md:136` 的标题逐字是
"Card：不做起自有类型，因为 WinUI 里没有这个控件"。
除它之外，`PagerControl`/`PersonPicture`/`SelectorBar`/`TwoPaneView`/`AnnotatedScrollBar`/`ItemsView`/`LayoutPanel`
这七个名字在 `docs/astra/**` 里**一条判例都没有**（逐名 grep）——"待决"在这里是字面意思，不是委婉说法。

`PipsPager` 与 `Divider` 已交付，但**不在这份分母里**——ModernWpf 没做 PipsPager（它自己推到 1.1），
WinUI 那个 commit 没有 Divider。所以"清单上有没有"与"我们做没做"是两回事，别拿这张表当全集。

### 2.3 范围之外、会露框架素外观的 92 个

Jalium 独有、ModernWpf 名单里没有：`Ribbon` 全族、图表全族（`BarChart`/`PieChart`/`Sparkline`/…）、
`Markdown`/`FlowDocument`/`JsonTreeViewer`/`Terminal`/`HexEditor`/`QRCode`/`MapView`，
以及三个**看起来最该管**的：`TabItem`（§2.1 里 `TabControl` 的条目）、`SwipeControl`、`RangeSlider`。
本层的规矩是"Gallery 里出现任何一个都必须显式标未样式化"（`adaptation/05` E 段），
所以这一类的缺口不是没数，是没做。

## 3. 差什么样式

按行类型看那 2105 个未转录的上游名（同一名字在两份文件里有两种行类型时会重复入桶，桶和 2187 > 2105）：

| 上游行类型 | 上游条数 | 我们没发 | 为什么 |
|---|---|---|---|
| `StaticResource` 别名 | 1520 | **913** | 绝大多数属 §2 里没排到的控件族；已排到的族按"没有 DP 就没有格子"逐条给过理由不发 |
| `SolidColorBrush` | 499 | 462 | 同上，另含 elevation 渐变那一族（本层改实底并有标注） |
| `Style` | 178 | **120** | 见 §3.1 |
| `Thickness` | 334 | 230 | 能发但没消费点的一律不发（闸口判红死键）；其余属未排到的族 |
| `x:Double` | 285 | **283** | **运行时载不动**：`x:Double` 行让整份字典解析失败，`sys:Double` 读回恒 0（`ROADMAP.md:923-929`） |
| `x:String` | 48 | 47 | 时长/曲线串：本运行时没有 Storyboard 吃它（§4） |
| `AcrylicBrush` | 44 | 43 | **运行时没有 `AcrylicBrush` 类型**（`audits/flyout-presenter.md:85`） |
| `Color` / `FontWeight` / `ControlTemplate` / `x:Boolean` / 对齐枚举 | 31/13/13/7/10 | 31/13/13/7/10 | 多为模板内联件与未排到族的资源 |
| `CornerRadius` | 25 | 13 | 含 `CornerRadiusFilterConverter` 一族（本层靠模板里手写四角，如 `Surfaces.jalxaml:91`） |

未转录名里含 acrylic/reveal/smoke/mica 的共 **284** 条；按控件族最多的是
`AppBar` 135、`Calendar` 104、`NavigationView` 76、`ListView` 69、`ToggleSwitch` 62、`Menu` 58、
`GridView` 52、`DatePicker` 47、`TitleBar` 46、`ToggleButton` 45、`TimePicker` 43、`CommandBarFlyout` 42、
`Slider` 41、`TabView` 37。

### 3.1 命名 Style：上游 178 条，同名我们只发 58 条

（"58"是 178 减去下表那 120 条之差，不是"我们一共只有 58 条样式"——`audits/keys.md` 里
`| Style |` 行有 104 条，多出来的是上游没有同名键的自有样式；生成器按层另给的"160 style rows"
是**按层切**的另一个口径，两个数不能互相引用。）

没发的那 120 条里最要紧的三簇（完整名单在 `spike/GapInventory/keys-diff-raw.txt`）：

1. **Reveal hover 整族 26 条**——`ButtonRevealStyle`、`AppBarButtonRevealStyle`、`ListViewItemRevealStyle`、
   `GridViewItemRevealStyle`、`MenuFlyoutItemRevealStyle`、`ComboBoxItemRevealStyle`、`SplitButtonRevealStyle`…
   运行时没有 `RevealBrush` 类型（Gallery Materials 页 :128 已把这句话写进界面文案），
   所以这一簇不是"忘了抄"，是**这条通路在 26.10.9 上不存在**。它该在账上有自己一条整族账，现在只有散在各族的说法。
2. **NavigationView 的条目呈现样式 11 条**——`MUX_NavigationViewItemPresenterStyleWhenOnLeftPane`／
   `…WhenOnTopPane`／`…WithRevealFocus`、`NavigationViewPaneSearchButtonStyle`、
   `NavigationViewOverflowButtonStyleWhenPaneOnTop`、`PaneToggleButtonStyle`。
   自有 `FluentNavigationView` 目前只有左栏静息一态：**顶栏模式（Top nav）整条没有**。
3. **`Default*Style` 一族**——`DefaultToggleSwitchStyle`、`DefaultProgressBarStyle`、`DefaultScrollBarStyle`、
   `DefaultSplitViewStyle`、`DefaultGridViewItemStyle`、`DefaultFlyoutPresenterStyle`、
   `DefaultRadioMenuFlyoutItemStyle`、`DefaultSplitMenuFlyoutItemStyle`、`DefaultRichEditBoxStyle`、
   `DefaultAutoSuggestBoxStyle`、`SplitButtonCommandBarStyle`（`audits/app-bar.md:89` 第 6 条点名的就是它）、
   `InfoBarCloseButtonStyle`、`ExpanderHeaderDownStyle`/`UpStyle`、`ItemContainerSelectionCheckboxStyle`。

### 3.2 已覆盖控件里欠着的状态格

| 缺口 | 判据在哪 |
|---|---|
| `CommandBar` 开放态/溢出整条（7 行按住不发） | `audits/app-bar.md:84-86` |
| `SplitButton` 的 `FlyoutOpen`/`TouchPressed` + 九条 Checked | `audits/splitbutton.md` 缺口 1 |
| 菜单族 11 行撤回（框架自绘只读运行时自己那张调色板）、子菜单进程内开不出 | `audits/menu-flyout.md:141` |
| DataGrid 的十七格状态矩阵、行头四枚 gripper | `audits/datagrid.md` §5 |
| `TabView.jalxaml:38` 那行禁用格（载体只在溢出时实现，至今是预测不是读数） | `audits/foreground.md` 7.6、`ROADMAP.md:4296` |
| 逐控件高对比视觉态：只有 101 键映射，一条 HC 状态格都没移植 | `adaptation/04`、不声称清单 `ROADMAP.md:930` |
| 间距/度量逐控件量化：只有 InfoBar、Menu 条目、命令栏条目、`ComboBoxItem`、`Slider` 五处对过数 | `ROADMAP.md:917` |

## 4. 差什么动画

### 4.1 三条把整块判死的实测

| 机制 | 读数 | 后果 |
|---|---|---|
| 没有 `VisualStateManager` 通路 | 标记里写 `VisualStateManager.VisualStateGroups` **两种写法都抛**（`adaptation/00:16`，S0-d 补强 :520-528：`Jalium.UI.Controls.VisualStateManager` 在已加载程序集里不存在；`Jalium.UI.VisualStateManager` 这个名字在 `s0v` 名单里有，但没人查状态组） | 上游 VSM 里所有 Storyboard **一条都搬不过来** |
| 变换不参与动画时钟 | 静态赋值的 `RotateTransform` 生效，同一 `Angle` 挂 `Forever` 的 `DoubleAnimation` 三个采样点仍是 0（`adaptation/00:1808-1811`）；markup `Storyboard` 水合为 0 子元素 | 要动只能**帧循环直写属性**；本层只有两台：`Motion/NavigationIndicatorAnimator.cs`、`Motion/ProgressRingAnimator.cs` |
| 数值/时长行载不动 | `x:Double` 整份字典失败、`sys:Double`/`sys:TimeSpan` 恒默认值（`ROADMAP.md:923-929`）；`x:String` 时长行交付的是 180 ms 框架默认值（`audits/motion.md:44-57`） | 上游 13 条时长键只转录 3 条 `<Duration>`；模板侧 21 处过渡全吃这 3 个键（`audits/motion.md:64,84`） |

没有 `KeySpline` 可放（`TransitionTimingFunction` 是枚举），所以上游那条
`ControlFastOutSlowInKeySpline` 永远只是枚举近似。

### 4.2 具体缺哪些动效

| 上游有的 | 我们现在的 | 判据 |
|---|---|---|
| `AnimatedIcon` + `AnimatedAcceptVisualSource`（勾选逐帧）、`AnimatedChevronUpDownSmallVisualSource`、按钮状态换帧 | 全族没有（26.10.9 的 61 个 dll 查无此名），折角用 `Path` + 固定 +90° | `audits/button.md:43,190`、`audits/checkbox-radiobutton.md:36`、`audits/expander.md:76` |
| ProgressBar 两段不定指示器滑移 | **不定进度是静图**（只交付宿主给的静态 1/3 段），无 `TemplateSettings` 可转录 | `audits/progress-bar.md:80,84,87` |
| ScrollBar 拇指 8↔12 与 0.167 s 关键帧、`AnnotatedScrollBar` | 部件宽度与动画都拿不到 | `audits/scrollbar.md:93` |
| Slider 拇指挤压 `ScaleX/Y` 0.86/1.167 | 改成 `Width/Height` 12→14→10→12 的近似，**没有过程证据** | `audits/slider.md:81` |
| `ThemeShadow`（TeachingTip 的 300/200 ms 位移 + 阴影） | 运行时没有 `ThemeShadow` | `audits/teachingtip.md:116`、`audits/numberbox.md:78` |
| 真指针 hover/press 的时长与换色过程 | 只在一次性探针里过（`SetCursorPos`），**不进常跑闸口**；触摸与笔从未真机验证 | `ROADMAP.md:1034`、#13、#73 |
| `ReduceMotion` 的覆盖 | 只管**下一次**过渡；ProgressRing 自转按上游证据故意不受门 | `audits/motion.md:85,87-92` |

### 4.3 这一族里从未进过账的那批

下列名字在 `docs/astra/**` 与 `src/FluentJalium/**` 里的命中数是 0 或 1，也就是说**连"做不到"都没写过**：
`ContentThemeTransition`、`AdditionRemoveThemeTransition`、`RepositionThemeTransition`、
`EntranceThemeTransition`、`PopupThemeTransition`、`ConnectedAnimation`、`MotionController`、
`ParallaxView`、`PullToRefresh`/`RefreshContainer`、`UniformGridLayout`、`FlowLayout`、`InkToolbar`。
`known-gaps.md` 自己写明"这份计数是坦率的地板、不是完备性证明"——§8 就是照着这句话补的第一批。

## 5. 差什么布局

### 5.1 运行时没有那一层布局基座

按 `Jalium.UI.Managed` 的 3014 个导出类型逐个点名（`s0v-runtime-type-inventory-raw.txt`，末段命令可重跑）：
`ItemsRepeater`、`ItemsView`、`LayoutPanel`、`StackLayout`、`UniformGridLayout`、`FlowLayout`、
`SplitView`、`TwoPaneView`、`Card`、`FlyoutPresenter`、`MenuScroller`、`PagerControl`、`PersonPicture`、
`SelectorBar`、`AnnotatedScrollBar` 全部 **0 命中**；`WrapPanel` 与 `VirtualizingWrapPanel` **有**
（`Jalium.UI.Controls.WrapPanel`，:1195），但本层没给它样式；`GridView` 也有名字，那是 WPF 的
`ViewBase` 而不是 WinUI 的列表控件（§2.2 已按这条改读）。

后果说白：WinUI 那套"虚拟化 + 可换布局 + 条目容器与控件解耦"的列表基座**整块缺失**，
本层今天只有 `VirtualizingStackPanel` 与框架自带的 `ListBox`/`ListView`/`TreeView`/`DataGrid`/`TreeDataGrid`；
`NavigationView` 的面板在上游是 `SplitView`，我们的 `FluentNavigationView` 只能自造 `PART_PaneRoot`。

### 5.2 度量不能随主题走

`x:Double` 与 `sys:Double` 那两条（§4.1 第三行）意味着**上游分主题切换的度量一律只能写成字面量**：
切主题不会重排任何东西，而 283 条 `x:Double` 上游行因此永远发不出去。
`Metrics.jalxaml` 顶上记着这条规矩的来历。

### 5.3 已经量到、还开着的布局级缺陷

| 缺陷 | 现状 | 判据 |
|---|---|---|
| `DataGridRow` 不能换模板（内容 visual 丢在模板拆除里） | 行**保留框架模板**，只有宿主/单元格/两种表头四份样式发得出 | `audits/datagrid.md:166,173`、`ROADMAP.md:1077` 起第五段 |
| `ScrollViewer` 在 `Auto` 下**只在右侧扣走 12 DIP** 布局宽度 | 已按 `Auto → Hidden` 在四处落点改掉；代价是**可见滚动条没有了**（本框架没有覆盖式滚动条） | `ROADMAP.md:278` |
| 圆角绝对尺寸测不准 | 弧长比 `半径×1.75` 短同一个系数、系数未解释；只声称比值不声称绝对值，**适用于本层每一个圆角** | `ROADMAP.md:906` |
| 弹层偏移（right-gap）一族 | 弹层外壳尺寸与那层 Border 归框架，本层只拿到内缩与色 | `audits/right-gap.md` §5（该节逐条在册，行数只认 `known-gaps.md`） |
| 评分子格被裁右半边 | #98 已修（一条横向定量通道 + 三件事同时翻绿）；**填充色到达像素仍无断言**，且捕获里出现过一次的填充穿继承色那一幕（机制未定） | `ROADMAP.md:5039` 起、`audits/rating-control.md` §13/§14 |
| 标题栏与窗口外壳 | 返回键、窗格键、`Subtitle`、中央自定义内容、`ExtendsContentIntoTitleBar` **无公开面**；Gallery 那 90 DIP 头部带里 42 DIP 是等外壳接汉堡的欠账 | `audits/window-shell.md`、`ROADMAP.md` 不声称清单 |

## 6. 账本自己的四条账（①②本批已结，③④还开着——这一节决定下一批做什么）

### 6.1 `adaptation/05` 为什么能过期一整个阶段，以及现在怎么防止

`keys.md` 与 `known-gaps.md` 各有生成器 + `-Check`，串行闸口每次重算；**真空清单原本两样都没有**。
落库的那份停在"已上隐式样式 13 / 真空 41"，而当前树现算是 **30 / 23**。原因很具体：
**生成器只发 A-E 五段与那个"证明不了什么"的尾巴，而后来有人在同一个文件上手写了 §D 的失效警告与
整段 §F 复测（2026-09-19 对着 26.10.9 量出三条假阴性）——再跑一次工具，这两块就被删了。**
这份文件因此处在"不跑就过期、跑就丢结论"的两难里，而过期是无声的那一支。

本批把两难拆开了，三条都已落地：

| 落地 | 内容 |
|---|---|
| 生成器 | 哨兵 `<!-- hand notes: … 原样保留这一行往下的一切内容 -->` 以下**逐字带回**；生成的尾巴上加一行 `<!-- generated-lines=N sha256=… -->`，只盖生成段，所以改手写注记永远不会被当成过期 |
| `-Check` | **整文档比较**，不是只比戳。第一版写的就是只比戳，验牙当场量出它的瞎处：手改生成段里那个 `30`→`29`，戳没动、重跑出来的生成段也没动，`-Check` 照样绿——正是它该抓的那种漂移 |
| 闸口 | `Test-AstraGates.ps1` 末尾加一步 `==> control vacuum inventory`（缺 `../ModernWpf` 参考仓时**响亮地失败**，不给自己一个空绿的余地） |

验牙：`bash spike/GapInventory/check-teeth.sh` 七步 `TEETH OK`——①重跑并快照 ②`-Check` 绿
③**手改生成段必红**（报"first difference at line 12"）④按字节快照还原必绿
⑤哨兵以下加一条注记 → 重跑必须还在、`-Check` 必须仍绿 ⑥把 `ToolBar` 从分母里删掉，生成段
42 行不同（源侧漂移也红）⑦末态与快照字节相同。当前读数：`69 generated lines, 21 hand lines`，
文档 116 行、行尾统一 CRLF。剩下没做的一件事：`spike/ControlCensus` 那份普查输出仍是
2026-09-18 的，重跑它才算把"分母也跟上运行时"这条闭上。

### 6.2 文案与账本相互矛盾的一处（本批已结）

Gallery 的弹层那张卡悬停提示原本写着 **"Flyout surface, acrylic fill"**
（`samples/FluentJalium.Gallery/MainWindow.jalxaml:186`），而 `audits/flyout-presenter.md:85` 第 2 条
逐字是**"弹层材质是实底，不是 acrylic"**——界面向用户主张的比账本乐观。
已按账本改口为 "solid fill (this runtime has no AcrylicBrush; the key holds upstream's own FallbackColor)"，
同一句里另外两个数先各自核过再留下：描边 1 是 `ThemeResources/ToolTip.jalxaml` 那条
`ToolTipBorderThemeThickness`（每个分支都是 1），320 是上游 `ToolTipMaxWidth` 因 `x:Double`
载不动而落成的样式字面量（`audits/tooltip.md:27,72`）。

证据分两栏，不许互相顶：**构建** = `dotnet build samples/FluentJalium.Gallery -c Debug`
`0 个警告 / 0 个错误`（1 m 18 s，增量重跑读数相同）。**冒烟未拿到** =
`tools/Test-AstraGallerySmoke.ps1` 在同一步 `WaitForInputIdle(30s)` 上，**本批未动的 `surfaces`
与动过的 `overview` 一样红**，所以这条落 #47/#35/#90 那一族（机器负载下的等待判据），
不归给这次改动；这一句的判据是那次 A/B，不是推测。

### 6.3 材质工作流（C 段）几乎没开工

`src/FluentJalium` 里 `BackdropEffect`/`AcrylicEffect`/`MicaEffect`/`DropShadowEffect` 的命中数是 **0**，
`SystemBackdrop` 只出现在 Gallery 窗口的 `SystemBackdrop="None"`；`ThemeResources/MenuFlyout.jalxaml:20`
留着"deferred to the materials batch"这句话。C1 的层语义映射表（ROADMAP 要求它落进 `adaptation/`）
**没有这份文件**，只有摸底原始输出 `adaptation/03-material-motion-raw-output.txt`。
窗口级背衬是真的（`WindowBackdropType` 五档，Settings 页有开关），元素级材质确实没有类型可用——
但 C2（背衬可调参数）、C3（elevation 真渐变：现在除 `TabViewSelectedItemBorderBrush` 一处手写渐变外全是实底替代）、
C1（映射表）三条一条都没结。

### 6.4 闸口读数（不粉饰）

本批跑到的：三条 `-Check` 各自绿（`known-gaps.md is current: 1022`、`keys.md is current: 1312`、`control-vacuum.md is current: 69 generated + 21 hand`）与一次 Gallery 构建（`0 个警告 / 0 个错误`）。**没跑** `dotnet test` 套件，也没跑串行闸口与页级像素闸。串行闸口上一次全绿是 `feb45af` 的树
（`spike/PagePixelsPlateDiag/gate-93.log`，套件 1594/1594 + 页闸 13×3 零 offender），
其后 #96/#97/#98 三批连着三次顺序跑各红，红集互不重合（3 / 10 / 13 条），
`ROADMAP.md` 末尾自己写的下一手是"先把 #90/#47/#80 那一族复现并归因，否则任何一批都收不了口"。

## 7. 如果按这份清点继续，建议的顺序

1. **先结账口那三次红**（#90/#47/#80 归因）。这不是可选的整理活——不结，后面每一批都只能停在"未收口"。
2. **把 §4.3 那批从未进账的机制补成 marker 行**，并给 Reveal / theme transitions 各立一条正式的不声称。
   这份仓库的规则是"做不到的要写下来"，而 §4.3 是唯一一处规则没覆盖到的地方。
3. **再开 §2.1 里高频那一排**：`TabControl` + `Label` + `ToolBar`/`StatusBar`/`GridSplitter`/`GroupBox`，
   日期时间族（`Calendar`/`DatePicker`/`TimePicker`）单独一批——它们自绘重、要先给渲染分级再决定要不要自有类型
   （ROADMAP D 节第 1 步那条"先量再判"就是从 NumberBox 批来的）。
   （§6.1 那三项本批已落，`-Check` 与哨兵保留不再是待办。）
4. **NavigationView 顶栏模式 / Reveal 那两簇要不要声称**，是个方向性决定，不是补格子：
   要么立自有类型与近似方案，要么照本仓的规矩明写"本层不做"，不能继续只以 11 条未转录键的形式存在。

## 8. Known Gaps

- 本文件是**快照不是台账**：四个数记于 2026-09-25 / `e1f3366`，没有 `-Check` 守着，样式或字典一动就会漂；重算命令写在 §1，引用它们时先重跑。
- 键差集**只按名字比，不比值**：798 个重合名里有多少逐字同值、有多少是本层已标注的形式替换（Color→Brush、渐变改实底），本批未量。
- §2.1/§2.2 的名单继承 `adaptation/05` 的两处已知不可靠：绘制方式列是 `OnRender`/`OnPaint` 启发式（NumberBox 批证伪过），D 段按名字匹配 `Control` 宇宙（§F 判出 `MenuFlyout` 一条假阴性，本批照同一把尺重量又判出五条：`CommandBarFlyout`、`GridView`、`Hyperlink`、`Page`、`WrapPanel`）。
- §5.1 的运行时缺席靠的是 `s0v-runtime-type-inventory-raw.txt` 那份 2026-09-20 的类型普查，本批**没有对 26.10.9 重跑普查**；若运行时升版，这张表整段要重算。
- §4.3 那份"从未进账"的名单是**一次字符串扫描的结果，不是完备性证明**：换个名字写法（例如上游 C++ 侧的 `FadeInThemeAnimation`）就扫不到，`audits/progress-bar.md:68` 恰好提过它一次。
- "1:1 复刻"的目视那一半仍然没有替身，本批也没做：三档像素闸判的是"我们的令牌上了屏、品牌绿没上屏、上游占位符没上屏"，不判与 WinUI 截图一致（`ROADMAP.md:4308` 同一条）。
- 本批没跑 `dotnet test` 套件、没跑串行闸口与页级像素闸；§6.4 的三条 `-Check` 与那次 Gallery 构建是本批自己的读数，而 `feb45af` 全绿与其后三次红是引来的旧读数，两者不要混着引用。
- Gallery 冒烟在本批**未通过判据**，且已按 A/B 判给仪器：`surfaces`（未动）与 `overview`（动过）在同一步 `WaitForInputIdle(30s)` 上一样红；这句不是“这次改动没问题”的证明，只是“它不是那条红的原因”。
