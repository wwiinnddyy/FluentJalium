# InfoBadge 审计（阶段 6 第四段，2026-09-21）

运行时权威：NuGet Jalium.UI **26.10.9**。上游权威：`../microsoft-ui-xaml` commit
`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`。方法参照：`../ModernWpf` commit
`23555a6c00623b2f80e67f20d7f1df49a1d28ad8`。引用仓库全程只读，未做任何 checkout/patch。

## 1 上游文件与 blob

| 文件 | blob | 行 | 这一层用到什么 |
| --- | --- | --- | --- |
| `controls/dev/InfoBadge/InfoBadge_themeresources.xaml` | `b09b56572ac8a2159bf9ba2dda7f063ec5460c6a` | 147 | 12 个键 × 三套字典 + 16 个 Style（:47-146） |
| `controls/dev/InfoBadge/InfoBadge.xaml` | `1f6ab40e7cc23c5bb7311a85006ec780e6a27d4f` | 3 | 隐式样式 BasedOn `DefaultInfoBadgeStyle` |
| `InfoBadge.idl` | — | :21, :25-26 | `InfoBadge : Control`；`Value : int = -1`；`IconSource : IconSource`；只读 `TemplateSettings`。无事件、无 `[templatedpart]`、无 `[templatevisualstate]` |
| `InfoBadge.cpp` | — | :29-37, :46-49, :59-83, :91-98 | 方形化度量、`Value < -1` 抛、四态选择、半径 = 高/2（本地值优先） |
| `InfoBadgeTemplateSettings.{h,cpp}` | — | — | `IconElement` / `InfoBadgeCornerRadius` 两个载体，本层没有对应物 |

上游**没有** `InfoBadgeSeverity` 枚举（严重级别完全靠 15 个样式键表达），也**没有** AutomationPeer。
HighContrast 的 12 行写在同一文件的第三套字典里，不是单独文件。

## 2 键账：12 个上游键，这一层发 6 个

| 上游键 | 类型 | 三套字典的值 | 消费点 | 这一层 |
| --- | --- | --- | --- | --- |
| `InfoBadgeForeground` | 别名 | `TextOnAccentFillColorPrimaryBrush`（HC：`SystemControlHighlightAltChromeWhiteBrush`） | 样式 :53 | 发（`ThemeResources/InfoBadge.jalxaml`） |
| `InfoBadgeBackground` | 别名 | `AccentFillColorDefaultBrush`（HC：`SystemControlHighlightAccentBrush`） | 样式 :51 | 发 |
| `InfoBadgeMinHeight` / `MinWidth` | `x:Double` | 4 / 4（三套相同） | 样式 :48-49 | **不发**→ 样式里写字面量 |
| `InfoBadgeMaxHeight` | `x:Double` | 16（三套相同） | 样式 :50 | 不发→字面量 |
| `InfoBadgeValueFontSize` | `x:Double` | 11（三套相同） | 模板 :82 | 不发→字面量 |
| `InfoBadgeIconHeight` | `x:Double` | **Default 8 / Light 9 / HC 9** | 无（全仓搜索仅声明处命中） | 不发，且不选边：这是死行 |
| `InfoBadgeIconWidth` | `x:Double` | 12（三套相同） | 无 | 不发 |
| `InfoBadgePadding` | `Thickness` | 0,0,0,0 | 样式 :53 | 发 |
| `IconInfoBadgeFontIconMargin` | `Thickness` | 4,0,4,2 | VisualState :70 | 发 |
| `ValueInfoBadgeTextMargin` | `Thickness` | 4,0,4,2 | VisualState :76 | 发 |
| `IconInfoBadgeIconMargin` | `Thickness` | 4,4,4,4 | VisualState :65 | 发 |

不发的那 6 条不是偷懒，是量出来的限制（`adaptation/00` §S1-r 第 1、2 条）：`x:Double` 行让整份字典解析失败，
`clr-namespace:System` 写法能解析、类型也对、**值恒为该类型的默认值**（文件里写 4，字典里读回 0），
而字符串行喂给数值属性也不做转换（`sys:String` 的 "4" 落到 `MinHeight` 仍是 0）。
一条读回来永远是 0 的"已发布令牌"比不发更坏：所有消费者看起来都正常。

样式键账：上游 16 个 Style 全部转录（`DefaultInfoBadgeStyle` + 5 严重级别 × Dot/Value/Icon），
加上 `InfoBadge.xaml` 那条隐式行，共 17 条；本段另发 6 个非样式键。`keys.md` 里新增 23 行。

## 3 基类与自有类型的理由

`spike/ControlCensus`：`InfoBadge` 与 `Badge` 都 **no type with this name**，所以没有可重模板的原生宿主。
基类跟上游：`Control`，不是 `RangeBase`——上游 `Value` 只是"要不要显示数字"的开关，不是范围。

**公开 API 偏离（Known Gap 1）**：上游的输入是 `IconSource`，框架经 `TemplateSettings.IconElement` 变成
`IconElement`。26.10.9 里 `IconSource`/`FontIconSource`/`SymbolIconSource`/`PathIconSource`/`BitmapIconSource`/
`ImageIconSource` **一个都没有**，这个属性没有可放的类型。所以 `FluentInfoBadge.Icon` 直接收 `IconElement`
（运行时确有 `FontIcon`/`SymbolIcon`/`PathIcon`）。缩放通路不变：`Viewbox` 在运行时是
`Jalium.UI.Controls.Viewbox : Decorator`，模板里仍用它。

**第二处偏离（Known Gap 2）**：一个样式里的 `Setter.Value` 只装**一个**元素实例，5 个 `*IconInfoBadgeStyle` 因此
各自持有一个 `FontIcon`/`SymbolIcon`，两个徽章共用同一样式就共用同一个元素。上游用 IconSource 正是为了绕开这点。
本段量了：`Two_badges_off_one_icon_style_both_reach_their_presenter` 绿——两个 presenter 的 `Content` 都在，
且各自等于本徽章的 `Icon`。这条只到"树里两个都还在"，**没有**到"两边都印出来"（文本/字形在本基座里根本不到达
捕获，见 §5），所以它不能声称视觉独立。

## 4 状态映射：4 个 VisualState → 3 条触发器

| 上游 | 本层 | 依据 |
| --- | --- | --- |
| `DisplayKindStates/Dot`（无 setter） | 不写——模板静止态即两 parts `Collapsed` | `:61` |
| `DisplayKindStates/Icon`（presenter Visible + `IconInfoBadgeIconMargin`） | `Trigger DisplayKind=Icon` 两条 setter | `:62-67` |
| `DisplayKindStates/FontIcon`（presenter Visible + `IconInfoBadgeFontIconMargin`） | `Trigger DisplayKind=FontIcon` 两条 setter | `:68-73` |
| `DisplayKindStates/Value`（text Visible + `ValueInfoBadgeTextMargin`） | `Trigger DisplayKind=Value` 两条 setter | `:74-79` |
| `TemplateSettings.IconBadgeCornerRadius` 绑定 | `Control.CornerRadius` + `TemplateBinding` | `cpp:91-98` |
| `TemplateSettings.IconElement` 绑定 | 控件把 `Icon` 塞进模板里的 `ContentPresenter` | `cpp:63-66` |

`DisplayKind` 是这层加的：触发器要有可读的属性，而上游把同样的判断藏在 C++ 里（`cpp:59-83`）不外露。
私有 setter 的可写属性，不是只读 key——这运行时的 `DependencyPropertyKey` 没有 `.Property` 可交回。

结构偏离：上游根是 `<Grid CornerRadius>`；这运行时的 `Grid` 没有 `CornerRadius` 成员（编译器直说），
所以根是 `Border`（名为 `RootGrid`，带 Background/Padding/CornerRadius）套一个 `Grid`。

**本段发现的第二处运行时缺陷**：模板里的 `TextBlock` **不继承**控件的 `Foreground`——上屏后读回是框架默认
`#E4000000`，而不是徽章自己的 `InfoBadgeForeground`（`#FFFFFFFF`）。上游那行没写 Foreground，靠继承。
所以本层加了 `Foreground="{TemplateBinding Foreground}"`（样式里注明的理由）。同一处还解释了为什么"数字"
在像素上看不见并不能只归给这条：见 §5。

## 5 像素与读数

直接捕获（`PixelHarness.Render`，DIP 边界）：

| 读数 | 值 |
| --- | --- |
| 点徽章的 pill（accent 纯蓝 `#0078D4`） | **920** 像素（60×16 盒，减圆角混色） |
| 同图的混色像素 `#000000` | 40（两条圆角边） |
| 框架品牌绿 `#207245` | **0** |
| `Value=12` 的同图 | 与点徽章**逐键相同**（920/40），即数字 0 墨 |
| 白字压蓝板的裸 `TextBlock` | 960 全是 `#0078D4`，白字 0 墨 |
| 白字压 `#202020` 卡的裸 `TextBlock` | 960 全是 `#202020`，白字 0 墨 |

整窗合成捕获（`PixelHarness.Host`）：

| 读数 | 值 |
| --- | --- |
| 灰底面板 + 白字裸 `TextBlock` + `Value=12` 徽章，一张图 | `#FFFFFF` 计数 **0**；灰底 6136、accent 279 |
| 同一图去掉文本行 | 灰底 6118、accent 288（差值只是布局挪动） |
| 白字蓝板的 Host | 只有窗口自身的 `#585858x19 #B5B5B5x19 #F1F1F1x16`，加文本行后这三个键不变 |

结论（这是本段最重要的一条，写成 §S1-r 第 3 条）：**这个基座的两条捕获通路都拿不到任何字形墨**——不是模板文本
专有，也不是符号字体专有，一个最朴素的 `TextBlock` 在整窗合成图里同样 0 墨。
所以"数字印出来了"这句主张在本仓库当前不可证，只能退到树上（`Text`/`Visibility`/`Foreground` 身份/度量盒），
`AstraInfoBadgeTests.The_number_is_laid_out_and_wears_the_badges_own_foreground` 就是这个退让的落点。
颜色主张仍然有牙：pill 的 `#0078D4` 920 像素与品牌绿 0，是 `Render` 路径能给的直接证据。

## 6 Known Gaps

1. `IconSource` 面没有对应类型，`Icon : IconElement` 是文档化的公开 API 偏离。
2. 5 个 `*IconInfoBadgeStyle` 共用一个元素实例；树级独立性已测，视觉独立性不可测（§5）。
3. 6 条 `x:Double` 度量行发不出去；4 条被消费的写成字面量，2 条上游死行不写。
   `InfoBadgeIconHeight` 的 8/9 主题差因此无处落地——但它上游本来就没有消费者。
4. **文本/字形墨在任何捕获通路都不可见**：所有含数字、图标、标题文本的状态，像素列只能主张色块与颜色身份。
   这条同时回头削弱了 PipsPager（pip 靠画椭圆而非字形）、TabView、菜单文本等批次的"像素"列。
5. 模板 `TextBlock` 不继承控件 `Foreground`——已用 `TemplateBinding` 绕过，但**没有**回头审其余自有样式里
   所有靠继承拿前景色的文本部件（另立任务）。
6. 无 AutomationPeer（上游也没有），无 `Severity` 枚举，`IsTabStop=False` 之外没有输入臂：
   本控件**硬件输入列为零**，且这不等于指针通路已验证（#13 仍是全仓库欠账）。
7. HighContrast 的两条别名重指向没有落到 `ThemeResources/HighContrast.map`，与 ProgressBar/ProgressRing/
   PipsPager 同一条欠账。

## 7 四类证据

1. 构建：串行闸口 `tools/Test-AstraGates.ps1` 的读数记在 `docs/astra/ROADMAP.md` 第四段块里（不在此预填）。
2. 行为：`AstraInfoBadgeTests` 42 条（17 个方法 + theory 展开）——部件身份逐类型读回、7 条 setter、别名指向、
   6 行 kind 优先序 theory、`Value<-1` 抛出且旧值留下、三种 kind 的**边距**（触发器证据，不是 Visibility）、
   半径 = 高/2 且本地半径按住、5 严重级别各自换刷且模板仍在、5 个图标样式带对元素、16 个键齐、
   6 条数值行反向 theory、5 个自造名反向 theory、两徽章共用一样例。
3. 视觉：pill 的 accent 920 像素 / 品牌绿 0 / Informational 在 Light 与 Dark 不同色（都走 `Render`）；
   `Value` 与 `Dot` 的图**逐键相同**这条负面读数按原样记进 §5，不当作通过。
4. 硬件输入：**仍为零**（§6 第 6 条）。
