# 图标族审计：SymbolIcon / FontIcon / PathIcon 与 `Symbol` 枚举

上游权威：WinUI 3 `microsoft-ui-xaml` @`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
运行时权威：NuGet `Jalium.UI` 26.10.9（与测试工程解析到的同一套程序集）

本族不是"重模板"控件，因此这份审计的出口与别的控件不同：**没有可写的模板，也没有可抄的样式**。
它要回答的是三件事——运行时到底给了什么、名字对上之后画的是不是同一枚字形、这一层还欠什么。

## 0. 上游的文件与锚点

| 事实 | 锚点 |
| --- | --- |
| `Symbol` 枚举声明（197 个成员，首个显式值 `Previous = 57600`，其余递增） | `dxaml/xcp/dxaml/idl/winrt/controls/microsoft.ui.xaml.controls.controls2.idl:165` |
| 同一枚举的生成侧镜像（逐个写出十六进制值，用作第二个解析器） | `dxaml/xcp/tools/XCPTypesAutoGen/XamlOM/Model/Microsoft.UI.Xaml.Controls.cs:353` |
| 枚举号 → 字形的转换表：`CSymbolIcon::ConvertSymbolValueToGlyph`，197 个 `case`，未命中原样返回 | `dxaml/xcp/core/core/elements/icon.cpp:461-662`，blob `7ae8225654f1d98d0da2e0a7535c32b35384ebe6` |
| SymbolIcon 的字体字符串 `"Segoe Fluent Icons,Segoe MDL2 Assets"` | `icon.cpp:14` |
| SymbolIcon 的可视要素由代码构造（`ApplyTemplate` 造 TextBlock 并写 FontFamily/Text） | `icon.cpp:272/312/323-400/427-459` |
| 同一张映射表的设计文档（第三份读数） | `docs/design-notes/symbol-enum-spec.md`（197 行表格） |

上游**不给** IconElement/SymbolIcon/FontIcon/PathIcon 任何默认样式：在 `*_themeresources.xaml` 里搜
`PathIconStyle`/`FontIconStyle` 等一个都没有。图标的前景色靠继承，尺寸靠代码里那条 TextBlock。
（继承在本运行时只给值、不给重绘，那半边差别见第 8 节。）
所以"资源键逐字照抄上游"在本族的落点只有一个候选——`SymbolThemeFontFamily`——见第 5 节为什么它没被发布。

## 1. 运行时导出了什么（in-proc 反射，非源码树）

```
Jalium.UI.Controls.IconElement   <- FrameworkElement <- UIElement <- Visual <- DependencyObject
Jalium.UI.Controls.SymbolIcon    <- IconElement      声明成员：Symbol
Jalium.UI.Controls.FontIcon      <- IconElement      声明成员：Glyph, FontFamily, FontSize
Jalium.UI.Controls.PathIcon      <- IconElement      声明成员：Data
Jalium.UI.Controls.IconElement   声明成员：Foreground
Jalium.UI.Controls.Symbol        enum, Int32, 764 个成员（Jalium.UI.Managed 声明）
```

缺席（同名不存在，逐个数过）：`BitmapIcon`、`ImageIcon`、`IconSource`、`FontIconSource`、`SymbolIconSource`、
`BitmapIconSource`。上游 XAML 能写的这六种名字，这里一个都写不出来。

两条量出来的仪器事实，写在这里是为了推翻更早的两次读法：

- `Jalium.UI.Controls.dll` 的导出类型数是 **0**——它是一具转发壳。类型的真正宿主是 `Jalium.UI.Managed.dll`（3014 个导出类型）。
- Windows PowerShell 5.1 跑在 .NET Framework 上，`LoadFrom` 一个 net10.0 程序集后 `GetTypes()` 会抛；
  脚本里一个空 `catch` 会把这次抛错读成"没有叫 Symbol 的类型"。本族前两次"枚举不存在"的结论都死在这上面。
  现在读它的是 `spike/SymbolCmap`（同进程、同一套包）。

两台错仪器已经撤掉，免得下次又被它们骗一次：`spike/SymbolCmap/Dump-ShippedSymbol.ps1`（就是上面那条空 `catch`）
与 `sweep.py`（输入是同级源码树的 `Symbol.cs`，正是 AGENTS.md 说"可能更新"的那份）都删了，
逐枚命中改由 `cmap-surface.py` 读 `Program.cs` 写出的两份 TSV。撤之前先核对过：两者读数已被本文件的
in-proc 读数取代，`check.py`/`crosscheck.py`（查装机字体 cmap 的）仍在，被 `cmap-surface.py` 复用同一思路。

## 2. `Symbol` 的数与上游画的字

两侧都不是"名字对上就完事"，因为上游存的是**旧号**、画之前先换算：

| 读法 | 数量 |
| --- | --- |
| 上游 IDL 声明的号（旧号，E100 起一段递增） | 197 |
| 两份上游读数（IDL 与生成镜像）名字与数字的冲突 | 0 |
| 同一名字两侧都有的 | 171 |
| 其中：我们存的号 == 上游换算后画的号 | **167** |
| 其中：我们存的号 != 上游画的号 | **4** |
| 上游有、我们没有的名字 | **26** |
| 两侧共有名字里，号与上游旧号相同的 | 3 |

那 4 个名字（`s2-symbol-surface-raw.txt` [F]）：

| 名字 | 上游画 | 这里存 |
| --- | --- | --- |
| `Account` | U+E910 | U+E77B（上游这张表里是 `Contact`） |
| `Map` | U+E707 | U+E826 |
| `MapPin` | U+E7B7 | U+E707（上游这张表里是 `Map`） |
| `Page` | U+E729 | U+E7C3（上游这张表里是 `Page2`） |

26 个缺席名字里，12 个的字形号在这套枚举中**任何成员都没有承载**（`FourBars/OneBar/TwoBars/ThreeBars/ZeroBars`
整条信号柱、`Manage`、`Placeholder`、`Priority`、`ReShare`、`SetTile`、`StopSlideShow`、`Target`）；
另外 14 个只是换了拼写就存在（`GlobalNavigationButton`→`GlobalNavButton`、`GoToToday`→`GotoToday`、
`MailFilled`→`MailFill`、`OutlineStar`→`Favorite/FavoriteStar`、`Pictures`→`Picture`、`SolidStar`→`FavoriteStarFill`、
`UnPin`→`Unpin`、`UnSyncFolder`→`UnsyncFolder`、`WebCam`→`Webcam`、`SetLockScreen`→`SetlockScreen`、
`Character`→`Characters`、`ClosedCaption`→`CC`、`Bullets`→`BulletedList/Designer`、`Page2`→`Page`）。

逐枚命中（`s2-symbol-cmap-raw.txt`，两个已装字体各查一遍 cmap）：

| 名单 | Segoe Fluent Icons | Segoe MDL2 Assets |
| --- | --- | --- |
| 我们枚举里的 764 个号 | 762 命中（缺 U+E919 `AlarmClock`、U+E7A0 `ScreenCapture`） | 755 命中 |
| 上游 IDL 声明的 197 个旧号 | 197 命中 | 197 命中 |
| 上游换算后要画的 197 个号 | 196 命中（缺 U+F5F0 `Target`） | 196 命中 |

三件事值得单独记：旧号段在两个字体里**都有字形**，所以换算不是补漏而是换图；上游自己的 `Symbol.Target`
在装机字体里无字形（画不出东西的是上游，不是我们）；这套枚举里 `AlarmClock`/`ScreenCapture` 两枚无字形。
另外，从源码树解析出的 764 个成员与钉住的程序集反射出的 764 个成员，号与名字**完全一致**——
"同级源码树可能更新"的这条告诫在本族不构成差异。

## 3. 挂载之后量到的行为（`spike/IconFamilyProbe`，真实窗口 + 上屏 + 帧后读回）

| 要素 | DesiredSize | 视觉子元素 | 备注 |
| --- | --- | --- | --- |
| `SymbolIcon{Add}` | 20x20 | 0 | 换 `Remove`/`Calories`/`Delete`/`GlobalNavButton` 仍是 20x20：盒子不跟字形走 |
| `FontIcon{E710}` | 8/20/40 → 8/20/40 | 0 | 盒子 == FontSize，随字号线性变化 |
| `PathIcon{闭合矩形}` | 20x20（给了 Width/Height 时） | 0 | 不给尺寸时铺满被排到的槽位（64x64 槽里印满 4096 px） |
| `TextBlock{IIII, 20}` | 25.52x27.4 | — | 对照组：文本确实参与测量 |
| `Border{20x20, 实心}` | 20x20 | — | 对照组：捕获确实看得见几何墨（400 px） |

结论：三个类型都不是"空壳"——`FontIcon` 的盒子跟着字号走、`PathIcon` 真的印得出墨、`SymbolIcon` 报的是
上游同款的 20x20。它们**内部自己画**（`IconElement` 派生自 `FrameworkElement` 而非 `Control`，`VisualTreeHelper`
下子元素为 0），所以"没有子元素"不等于"没有渲染"。

## 4. 能不能重模板：不能，且不需要

`IconElement` 系不是 `Control`，没有 `Template` 可交给它，样式里也没有可命名部件可写。上游同样如此——
它的图标可视要素在 `icon.cpp:323` 用 C++ 造。要改外观只有两条路：派生（实测可行：`ProbeIcon : IconElement` 能编译、
`MeasureOverride` 被调用、`base.MeasureOverride` 返回 0x0）或包一层。本族的判断是**两条都不走**：
量到的盒子、成员、字号行为与上游一致，没有可指认的行为缺口；而"字形到底画没画"这一项目在两种捕获通路上都测不出来
（见第 6 节），拿一条测不出来的怀疑去新造三个公开类型，代价是 API 面，收益为零。

## 5. 这一层发布什么：什么都不发布

唯一候选是上游的 `SymbolThemeFontFamily`（`FontFamily` 行，值 `"Segoe Fluent Icons,Segoe MDL2 Assets"`）。
试过之后量死：

| 写法 | 读回 |
| --- | --- |
| `<FontFamily x:Key>K">名字</FontFamily>` | `FontFamily.Source == ""` |
| `<FontFamily x:Key=K Source=名字 />` | `FontFamily.Source == ""` |
| `<FontFamily x:Key=K FamilyName=名字 />` | `FontFamily.Source == ""` |
| `<x:String x:Key=K>名字</x:String>` + `{ThemeResource}` 喂给 `TextBlock.FontFamily` | 文本存住了，属性没吃到 |

三种写法都造出一个空值 `FontFamily`，即 markup 里的值被**静默丢弃**——与 `x:Double` 行读回 0（S1-r）、
负的 `StackPanel.Spacing` 按 0 排版（`audits/rating-control.md` 第 10 条）同一类。
因此 `ThemeResources/Icons.jalxaml` 没有落地，`SymbolThemeFontFamily` 在 keys.md 里不存在，
各模板继续写字面量 `"Segoe Fluent Icons"`；`AstraIconFamilyTests` 把"这个键查不到"和"三种写法都空"
一起钉住，等哪天运行时能承载这行，测试会红，那时再抄。

**本节第 5 行管不到"字面量属性"这一形**（#96/#97 复量，2026-09-23）。上表四行的值都要先经一次资源查找
（三种 `x:Key` 写法）或一次 `{ThemeResource}` 交接，被丢掉的是那一步；直接在要素上写 `FontFamily="名字"` 是另一条解析路。
`spike/GlyphInkProbe` 把两形各读一次：

| 写法 | 读回 | 墨 |
| --- | --- | --- |
| `<FontIcon Glyph="&#xE700;" FontSize="20" FontFamily="Segoe Fluent Icons" />` | `FontFamily.Source == "Segoe Fluent Icons"`，`Glyph=U+E700`，`FontSize=20` | 见下 |
| `<TextBlock Text="MW" FontSize="20" FontFamily="Segoe MDL2 Assets" />` | `FontFamily.Source == "Segoe MDL2 Assets"` | — |

而且属性读回不为空这件事在像素上算数：同一张 764 枚的格子，`FontFamily` 由代码写的 `fluent` 档与由标记写的
`markup` 档逐格比签名，**761 格同时有墨、761 格签名完全相同（100.0%）**（`spike/GlyphInkProbe/readings-96.txt`）。
所以"标记送不到字体"这条结论的范围要收窄成"**送不到资源键那一形**"，字面量属性是通的——这既是本族的更正，
也是 #97 修法的前提（见第 9 节）。

## 6. Known Gaps

1. **字形墨在进程内通路上仍然测不出来，在整屏通路上测得出来——本条已按 96 重画边界，不再是"测不出来"。**
   进程内捕获会重跑渲染，对白底上的 `TextBlock` 与字形都是瞎的（`adaptation/00` S1-r 第 3 条）；监视器抓取看得见。
   `spike/GlyphInkProbe` 量到的数：`SymbolIcon` 764 格里 **644 格有墨**，`FontIcon`+Segoe Fluent Icons **761**，
   +Segoe MDL2 Assets **762**（`symbol` 档跑了两次并逐格比对：729 个不同码点的墨数与签名全部相同；`fluent`/`mdl2`
   各两次的汇总行相同）。同屏的文本对照格 272 px 墨、空白格 0 px，所以"整片为 0"读作仪器坏，读不作字体结论。
   第 9 节给全数与仪器账。
2. `Symbol` 的 4 个名字画的字与上游不同（第 2 节表），这一层无法修正：号在运行时枚举里，读它的类型只认这个枚举。
   应用要拿到上游那张图，得自己写 `FontIcon.Glyph`。
3. 26 个上游名字在本族枚举里不存在，其中 12 个连字形号都没人承载。
4. 上游有 `BitmapIcon`/`ImageIcon` 与整个 `IconSource` 家族，这里没有；`FluentInfoBadge` 的图标入口因此是
   `IconElement` 而不是 `IconSource`。
5. 高对比度下上游靠 `IconElement.Foreground` 的继承链改变图标颜色；这里 `Foreground` 默认读回 null 且没有可挂的
   隐式样式（运行时不给非 `Control` 应用样式的路子未验证），高对比的图标颜色**未声称**。
6. `PathIcon` 在没有显式尺寸时铺满槽位，与上游"16x16 + Uniform"不同；本层不改（改了就是替运行时发明默认值），
   只把量到的数记在这里，并在测试里钉住铺满这一半。
7. 指针/键盘/触摸在本族没有对应路径（`IconElement` 不是可交互控件，上游也一样），因此本段无硬件输入证据。
8. **`SymbolIcon` 有 120 枚画不出任何墨，而这 120 个码点在两个已装图标的字体里都有。** `match-cmap.py` 逐枚查
   `SegoeIcons.ttf`（2033 码点）与 `segmdl2.ttf`（1833）：这 120 格**一个都不能用"字体里没有"解释**（0/120），
   而同一批码点交给 `FontIcon` 有 118 格出墨（两档都是 118）。洞在运行时的 symbol→字形那段，不在字体，
   也不在本层——本层拿不到那段（`SymbolIcon` 没有 `FontFamily`，`spike/GlyphInkProbe` 读它只读到 `<no FontFamily property>`）。
9. **`SymbolIcon` 画的是哪套字形，没定出来。** 它与 `FontIcon`+MDL2 的签名一致率 2.3%、与 +Fluent 的 1.0%，
   而"两个不同字体走同一个要素"的对照是 19.1%（都限定在墨数相差 10% 以内的格上，排掉尺寸这一混淆）。
   这只能说"它跟两个具名字体都不一样"，不能说它是第三个哪一种——候选扫完 13 个文件：机器上没装 `segoesym.ttf`
   （Segoe UI Symbol），`segoeui.*`／`segoepr*`／`segoesc*` 都在 609 个有墨格上直接矛盾，`symbol.ttf` 读不出 cmap。
10. **cmap 有 ≠ 画得出，画得出 ≠ cmap 有。** `AlarmClock/U+E919` 与 `ScreenCapture/U+E7A0` 不在
    `SegoeIcons.ttf` 的 cmap 里，`SymbolIcon` 那两格却有墨；反过来 `DataSenseBar/U+E7A5` 两档具名字体都有号，
    走 `FontIcon` 时是空的。因此第 2 节那张命中表与 s2 的 cmap 差分量不了"用户看得见几个图标"，
    两条通路要分开记（本批把像素那半补上了，cmap 那半不动）。

## 7. 证据分类

- 构建：串行 `tools/Test-AstraGates.ps1`（restore→build→test→调色板漂移）的读数记在 `ROADMAP.md`
  本段末尾的"闸口读数"里，本文件不重复抄一遍数（抄了就会过期）。
- 行为：`AstraIconFamilyTests`——类型面、成员面、枚举号、markup 名字解析、未知名静默替换、盒子随字号/不随字形、
  4 个偏差名字与 26 个缺席名字的漂移闸、`SymbolThemeFontFamily` 不可发布的复测。
- 视觉：`PathIcon` 的几何填充（`A_closed_geometry_reaches_the_pixel_and_fills_the_slot_it_is_given`），加上 #96/#97
  这一批的**整屏字形墨**：逐格墨数、8x8 签名与四档对照（`spike/GlyphInkProbe/glyph-ink-{symbol,fluent,mdl2,markup}.csv`，
  汇总 `readings-96.txt`）。进程内通路仍然拿不到字形，两条通路分记，见第 6 节第 1 条与第 9 节。
- 硬件输入：无（见第 6 节第 7 条）。

原始读数：`adaptation/s2-symbol-surface-raw.txt`（类型面 + 枚举三方 diff）、
`adaptation/s2-symbol-cmap-raw.txt`（两份名单 × 两个字体逐枚命中）、`adaptation/s2-icon-family-raw.txt`（挂载、markup、墨、宿主四组）、
`spike/GlyphInkProbe/readings-96.txt`（四档逐格对照 + 13 个字体文件的 cmap 命中）与 `probe-<variant>.log`（每次抓屏自己的几何与反射读数）。
仪器：`spike/SymbolCmap`（Program.cs 反射 + 三方解析，cmap-surface.py 查字形）、`spike/IconFamilyProbe`（挂载与捕获）、
`spike/GlyphInkProbe`（整屏逐格墨量：Program.cs 铺格子 + shoot-count.ps1 抓屏计数 + compare-variants.py 逐格对照 +
match-cmap.py 对齐已装字体）。


## 8. 换档重绘批（#94/#95，2026-09-23）：继承给的是值，不是重绘

第 1 节那句"图标的前景色靠继承"只说完了一半。继承确实把**值**给到了——派生类探针在一棵活树上读
`GetEffectiveForeground()`，Light → Dark → 回到 Light 三读都是当前档的刷（`spike/NavIconRecolor/probe-ink.log`），
而图标自己那格一直是 `<null>`。缺的是另一半：**没人让它重画**。框架只在 `IconElement` 自己的
`Foreground` 变化时 `InvalidateVisual()`，换档改的是那条上溯答案，不是图标那格，于是屏幕上留着上一次落笔的墨。
用户报的"浅色切深色，侧栏图标还是黑的"就是这个；抓屏 A/B 的量法与逐色读数在 `audits/navigation.md` §10，
本批把同一量法铺到全部宿主上（§11 与 `ROADMAP.md` #95）。

修法是一处宿主侧补偿，两个入口（`Controls/IconInk.cs`）：

- **我们的控件**有代码钩子：`FluentNavigationItem.OnIconChanged` 调 `IconInk.Apply(item, icon)`，图标没有本地前景时
  把宿主的 `Foreground` 绑到图标那格。本地值优先，这一条与上游"图标可覆盖宿主"一致。
- **框架控件的模板**没有钩子，就让标记说出两样东西：`fluent:IconInk.Carrier` 是"抄谁的墨"，`fluent:IconInk.Icon`
  是"图标在哪"（`{Binding Icon, RelativeSource={RelativeSource TemplatedParent}}`）。两条都是普通绑定，回调只在
  两者都到位时交付；因为绑定本来就是活的，**换上去的图标也会重新交付**。写图标自己的属性，是这条运行时里唯一
  既换值又标脏的路。

第一版不是这样：它只有一个附着属性，挂在包住图标的要素上，`Loaded` 之后走视觉树广度优先转发。串行闸口的套件步
把它拦下了——`AstraGateTests` 那条结构闸按 AGENTS.md 的三条禁令把 `VisualTreeHelper` 整个挡在库外
（`spike/NavIconRecolor/gate-95.log`）。改法不是放宽那条闸，而是换一条不需要找元素的路；读数的变化与两条新事实
记在 `audits/navigation.md` §11 与 `ROADMAP.md` 的 #95 一节。

这条不是框架的修复。根因在框架：主题换档时没人让 `IconElement` 失效，上游 WinUI 不需要宿主补偿是因为它整棵树
随主题重绘。本层的接线只是让用户看见的那片墨跟上档，框架侧的账提给上游（与 #62 那条"字形不打印"同一族）。

对本族第 6 节的两处更正：

- 第 5 条说"没有可挂的隐式样式路子"。路子确实还没有（应用级隐式样式落不到 `SymbolIcon` 上，本批复测过），
  但 `IconInk.Carrier` / `IconInk.Icon` 是一对**能挂**的附着属性路子，宿主侧补偿因此不再受"只能靠继承"限制。高对比档的图标墨
  仍然**未声称**——本批两条腿都是 Light↔Dark，见下。

## 9. 字形墨与字体通路批（#96/#97，2026-09-23）：整屏读得到字形了，读出来是运行时钉死的那套

用户报的形状是"这里全是 Windows 10 那代的 Fluent 图标"。这条既不能按印象结，也不能停在"看着像 MDL2"。
两个问题分开量：**墨读不读得到**（#96，决定第 6 节第 1 条那条边界还成不成立）与**是谁在画、换得动吗**（#97）。

### 9.1 仪器：四档 × 766 格，一格一个形状签名

`spike/GlyphInkProbe` 把 764 个 `Symbol` 值铺进 24 DIP 的白底格子，红框圈住整张表，抓监视器后逐格数墨：
两档阈值（`ink`＝R/G/B 全 <128，`light`＝任一通道 <215）加每格一个 8×8 **签名**，另带两格对照——
16 px 的 `TextBlock`（必须有墨）与纯白 `Border`（必须没有）。四档只差"这个码点由谁来画"：

| 档 | 要素与字体来源 | 有墨格 | 完全无墨 | 中位墨 |
| --- | --- | --- | --- | --- |
| `symbol` | `SymbolIcon{Symbol}`，走运行时自己那条路 | 644 / 764 | 120 | 244 px |
| `fluent` | `FontIcon{Glyph}`，`FontFamily` 由**代码**写 `"Segoe Fluent Icons"` | 761 | 3 | 230 px |
| `mdl2` | 同上，代码写 `"Segoe MDL2 Assets"` | 762 | 2 | 226 px |
| `markup` | 同上，字体名由**标记字面量**给 | 761 | 3 | 230 px |

四档的对照格都是 `text=272 px`、`blank=0 px`，因此表里的 0 读作"这里什么都没画"，读不作"仪器看不见字"。
`symbol` 档跑了两次并逐格比对：729 个不同码点的 `ink`/`light`/签名全同。逐格原始数在
`glyph-ink-<variant>.csv`，对照与 cmap 汇总在 `readings-96.txt`。

### 9.2 三条读数

1. **第 5 节那句"markup 送不到字体"只覆盖了资源键那一形。** 字面量 `FontFamily="Segoe Fluent Icons"` 不但读回非空，
   还到达像素：`markup` 档与代码写的 `fluent` 档在 761 个同时有墨的格上**签名 100.0% 相同**。
   修法因此可以留在模板里，不需要为送一个字体名再造代码钩子。
2. **`SymbolIcon` 画的不是任何已装具名字体在枚举号上的那套字。** 与 `mdl2` 档的签名一致率 2.3%、与 `fluent` 档 1.0%，
   而"两个不同字体走同一个要素"的对照（`fluent` vs `mdl2`）是 19.1%——三个数都限定在墨数相差 10% 以内的格上，
   尺寸这一混淆被排掉了。它还有 120 格完全无墨，而这 120 个码点在 `SegoeIcons.ttf` 与 `segmdl2.ttf` 里**都有**
   （`match-cmap.py`：0/120 能用缺席解释），同一批号交给 `FontIcon` 有 118 格出墨。
   洞在运行时的 symbol→字形那段。哪套字形，本批定不出来，见第 6 节第 9 条。
3. **Win10 那代字形的来源在运行时里，钉在两处默认值上。** 对**出货的那份** `Jalium.UI.Managed` 读私有静态
   （那份 DLL 自己不 stamp 版本——FileVersion/ProductVersion 都是 0.0.0.0，"出货的"这件事由探针工程钉的
   `Jalium.UI.Desktop` 26.10.9 说，不是程序集自报的）
   （探针里读，库里读会被 `AstraGateTests` 的结构闸拦下）：

   ```
   witness SymbolIcon.SymbolFontFamily='Segoe MDL2 Assets'
   witness FontIcon.DefaultFontFamily='Segoe MDL2 Assets'
   witness FontIcon.FontFamily after a code write='Segoe Fluent Icons' default instance='<null>'
   ```

   `SymbolIcon` 整个类型面上没有 `FontFamily`（第 1 节那条"0 个视觉子元素、读不到字体"同一条），所以宿主改不动它；
   `FontIcon` 有公开的 `FontFamily` 面，且第 1 条已量到代码与标记两条路都能把它送到像素。上游钉的是
   `SymbolThemeFontFamily = "Segoe Fluent Icons"`（`audits/icon-family.md` 第 5 节），本运行时两处默认都是 MDL2——
   用户看到的 Win10 形状就是这个默认值，不是本层挑的。

### 9.3 仪器账（三次红读换回来的，写下来免得下次再交一遍）

- **抓屏进程必须自己 DPI-aware。** 这台显示器 175%（DPI 168，2560×1600），未声明感知的 PowerShell 拿到的
  `GetWindowRect` 是虚拟化坐标、`CopyFromScreen` 拿到的却是物理像素——于是每次抓到的都是窗口左上那一块，
  红框的右/下两边被切在图像边界上。前三次运行都被读成"窗口装不下格子"，改的其实是 `GlyphInkNative.SetProcessDPIAware()`。
- **窗口尺寸单位与内容单位不是一套。** `Window.Width/Height` 走的是设备像素那一侧，内容按 DIP 排版，
  所以按盒子大小开窗口必然裁掉边框；现在改成由格子自己算再留余量，并让计数器用红框宽/高两个方向各自推出的
  比例互为校验（`px-per-dip` 由窗口自己报，读回 1.75）。
- **红框按"长边"找，不按 min/max。** 一次运行里有一颗游离红点落在抓取范围的右下角，包围盒从 1680×840 变成 1807×984，
  红像素总数却没变（15111 对 15177）——只看包围盒会把好数据读成"被盖住"。现在取长于 50 px 的红行/红列。
- **第一次 `markup` 档 0/764 是仪器的锅。** 那一版没给图标前景，`IconElement.GetEffectiveForeground()` 在裸窗口里
  上溯不到 `Control`，落到 `"TextPrimary"` 资源查找——这条路上没应用主题，查不到就用静态默认刷，屏幕上就是没有墨。
  补一行 `Foreground = Black`（只补墨、不补字体，字体仍由标记给）之后才是上面那行数。

### 9.4 这批没做的事

产品代码一行没动：#97 的修法形状现在有了测量的前提（标记能送字体名），但从"能送"到"换上去"要先把
`Symbol`→`Glyph` 那层映射定下来——模板绑的是 `Icon`（`Symbol` 值），`FontIcon` 要的是字符串，
逐宿主换形状是另一批的账，且要先回答第 6 节第 9 条那句"现在到底是谁在画"。
本批只改了 `spike/` 与文档。串行闸口在这份树上跑了两次，两次都停在套件步，且两次的红集不重合——
读数与判记在 `ROADMAP.md` 本节末尾（结论：环境占用下的像素断言，闸口状态记为**未过**）。本批的结论不依赖套件：
四档的墨数与签名是仪器自己从屏上打出来的。


