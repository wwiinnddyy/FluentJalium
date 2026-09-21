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

## 6. Known Gaps

1. **字形有没有画出来，测不出来。** 同一张白底上放一个普通 `TextBlock`（纯文本、有字号、有前景）也印不出墨——
   这条路对文本是瞎的（`adaptation/00` S1-r 第 3 条，本段在 spike/IconFamilyProbe 里复测）。因此本族**不声称**
   任何 `SymbolIcon`/`FontIcon` 的字形到达像素；能声称的只有 `PathIcon` 的几何填充（400 px / 4096 px 两个数）。
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

## 7. 证据分类

- 构建：串行 `tools/Test-AstraGates.ps1`（restore→build→test→调色板漂移）的读数记在 `ROADMAP.md`
  本段末尾的"闸口读数"里，本文件不重复抄一遍数（抄了就会过期）。
- 行为：`AstraIconFamilyTests`——类型面、成员面、枚举号、markup 名字解析、未知名静默替换、盒子随字号/不随字形、
  4 个偏差名字与 26 个缺席名字的漂移闸、`SymbolThemeFontFamily` 不可发布的复测。
- 视觉：只有 `PathIcon` 的几何填充（`A_closed_geometry_reaches_the_pixel_and_fills_the_slot_it_is_given`）。
  字形墨不可得，见第 6 节第 1 条。
- 硬件输入：无（见第 6 节第 7 条）。

原始读数：`adaptation/s2-symbol-surface-raw.txt`（类型面 + 枚举三方 diff）、
`adaptation/s2-symbol-cmap-raw.txt`（两份名单 × 两个字体逐枚命中）、`adaptation/s2-icon-family-raw.txt`（挂载、markup、墨、宿主四组）、
仪器：`spike/SymbolCmap`（Program.cs 反射 + 三方解析，cmap-surface.py 查字形）、`spike/IconFamilyProbe`（挂载与捕获）。
