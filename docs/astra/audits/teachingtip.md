# TeachingTip 与 Card 审计（阶段 4 第五段）

上游：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
- `controls/dev/TeachingTip/TeachingTip_themeresources.xaml`，blob `50cc6d2d6cb7648df90e21faa0cc9e6aa816a9bd`（171 行：Dark 4-25、Light 26-47、HighContrast 48-69、分支外 71-100、样式 101-170）
- `controls/dev/TeachingTip/TeachingTip.xaml`，blob `edcd4dba11ce2e28e2538724d42c9172749843ba`（360 行：隐式样式 3、`DefaultTeachingTipStyle` 4-360、模板 15-357）
- `controls/dev/TeachingTip/TeachingTip.idl`（公开面 94-172，枚举 6-49）、`TeachingTip.cpp` / `TeachingTip.h`（弹窗与动画在代码里）
- 卡片色令牌在 `controls/dev/CommonStyles/Common_themeresources_any.xaml`（见第 5 节）

方法参照：`ModernWpf` @ `23555a6c00623b2f80e67f20d7f1df49a1d28ad8`，`ModernWpf.Controls/TeachingTip/`
（`TeachingTip.cs:15` 是 `public partial class TeachingTip : ContentControl`，模板 `TeachingTip.xaml`，
在 `ModernWpf.Controls/Themes/Generic.xaml:37` 合并）。

运行时：NuGet Jalium.UI **26.10.9**。类型普查见 `adaptation/s0v-runtime-type-inventory-raw.txt`：
**运行时既没有 `TeachingTip` 也没有 `Card`**（清单里只有 `QR.Payloads.MeCardPayload` / `VCardPayload` 两个同名干扰项），
所以这两格只能由本库提供——这与菜单族那种"运行时本来就有、只是造不出来"的情况不同。

## 1. 上游键清单（20 个主题行名 × 3 档 + 30 条分支外行 + 1 条样式）

主题档（三档**同名 20 个行名**，只有指向的源不同）：`TeachingTipBorderBrush`、`TeachingTipTopHighlightBrush`
（唯一一条真 `SolidColorBrush`：Dark `#0DFFFFFF` / Light `#99FFFFFF` / HC `Transparent`）、
`TeachingTipTransientBackground`、`TeachingTipForegroundBrush`、`TeachingTipBackgroundBrush`、
`TeachingTipTitleForegroundBrush`、`TeachingTipSubtitleForegroundBrush`，加
`TeachingTipAlternateCloseButton{Background,Foreground,BorderBrush}{,PointerOver,Pressed,Disabled}` 12 条
与 `TeachingTipAlternateCloseButtonBorderThickness`（`Thickness 1`）。

分支外 30 行（71-100）按类型分：**`Thickness` 20 条**（按钮面板/主内容/标题栈/图标/四条边裁剪/四个尾巴 margin/
`TeachingTipContentMargin=12`/`TeachingTipTopHighlightOffsetForBorder=0,1,0,0`）、
**`x:Double` 8 条**（`MinHeight 40`、`MaxHeight 520`、`MinWidth 320`、`MaxWidth 336`、
`AlternateCloseButtonSize 40`、`AlternateCloseButtonGlyphSize 16`、`TopHighlightHeight 1`、`BorderThickness 1`）、
**`GridLength` 2 条**（`TeachingTipTailShortSideLength 8`、`TeachingTipTailMargin 10`）。

上游明确**没有**的键，逐条查过、不是猜的：没有圆角键（模板 9 行用 `OverlayCornerRadius`、
`AlternateCloseButtonStyle` 115 行用 `ControlCornerRadius`）；没有阴影键（`ThemeShadow` 在
`TeachingTip.cpp:2256-2293` 代码里造）；没有过渡时长键（300 ms 展开 / 200 ms 收起是 `TeachingTip.h:234-235`
两个 C++ 字段，只有 `TeachingTipTestHooks` 能改）；尾巴形状没有几何键（是每条状态里内联的
`Polygon.Points`）。`TeachingTip_themeresources_perf2026.xaml` 与主版本逐键相同，只把 Storyboard 改写成
`VisualState.Setters`，因此不作第二份对照。

## 2. 上游模板结构（决定本实现能照抄到哪一层）

外层 `Border 'Container'`（16 行，`HorizontalAlignment=Left`、`Background=Transparent`）→ 两层 5×5 网格
（`TailOcclusionGrid` 297 行填满 5 列 5 行，列宽 `8,10,*,10,8`）→ 内容卡 `ContentRootGrid`（312 行，
占中间 3×3，戴 `Background`/`BorderBrush`/`BorderThickness`/`CornerRadius`）→ 卡片内三行：
`HeroContentBorder`(0) / `NonHeroContentRootGrid`(1) / 无。中间那行是 `ScrollViewer` + `StackPanel(Margin=12)`，
其下依次为图标 `IconPresenter`、标题栈（`TitleTextBlock` SemiBold + `SubtitleTextBlock`，**两者默认
`Visibility=Collapsed`，由状态打开**）、`MainContentPresenter`、按钮行（`ActionButton` / `CloseButton`，
各套 `ActionButtonStyle`/`CloseButtonStyle`），右上角另有一个 `AlternateCloseButton`（`E711` 字形、40×40）。
尾巴是同级的 `Polygon 'TailPolygon'`（353 行，`Fill` 跟卡片同色、`Stroke` 跟边框、`StrokeThickness=1`）。
**模板里没有 Popup**——弹窗由控件在代码里自建（`TeachingTip.h:103,176,282`）。

## 3. 上游状态映射（9 组 41 态）与本实现的落点

已交付的格子在 `Styles/TeachingTip.jalxaml` 里，逐格有消费点（该字典已进
`AstraResourceKeyTests.Transcribed_control_rows_are_read_by_a_template`）。"未做"的那几行不是遗漏，
是第 5 节里各有一条理由的缺口。

| 上游组 | 态数 | 上游写的东西 | 本实现 |
| --- | --- | --- | --- |
| `PlacementStates` | 14 | 尾巴的可见性、`Points`、行/列、对齐、margin；`Untargeted` 只收尾巴 | **已做 5 格**：`EffectivePlacement` 的 Top/Bottom/Left/Right/Center，每格写尾巴的行列、`Points`、对齐、margin 与"尾在哪条边就清哪条边"的描边；`Target={x:Null}` 一格收尾巴并回到 `Untargeted` 描边。上游另外 8 个 per-edge / per-corner 态**无处可挂**（`Popup.Placement` 那 12 个值里一个都没有，S1-b·2） |
| `ButtonsStates` | 4 | 两个按钮的可见性 + margin + `Grid.Column`/`ColumnSpan` | **已做**：按 `ActionButtonContent`/`CloseButtonContent` 是否为空各两格，另有双空收整行与单按钮 `ColumnSpan=2` |
| `LightDismissStates` | 2 | 把尾巴/卡/主内容/hero 的填充换成 `TeachingTipTransientBackground` | **未做**（5.9）：没有点外关闭的通路，`TeachingTipTransientBackground` 因此不进字典 |
| `ContentStates` | 2 | `MainContentPresenter.Margin` 12↔0 | **已做**：`Content` 是否为空 |
| `CloseButtonLocations` | 2 | 标题栈右让 28 + `AlternateCloseButton` 开关 | 只做 `FooterCloseButton`；右上角那一格 13 行不发（5.4） |
| `IconStates` | 2 | `IconPresenter.Margin` 12↔0 | **未做**（5.10）：宿主没有 `IconSource`，两条图标 margin 不进字典 |
| `HeroContentPlacementStates` | 2 | hero 落在第 0 或第 2 行 + 圆角过滤器 | **未做**（5.5） |
| `TitleBlockStates` / `SubtitleBlockStates` | 2+2 | 两个 `TextBlock` 的可见性 | **已做**：`Title`/`Subtitle` 是否为空，各两格（`{x:Null}` 与空串——后者是 S0-g 里那条"空串判据"在本运行时的正向证据） |

驱动方式是**控件写属性、模板读属性**：上游由 C++ 挑一个 `PlacementStates` 态，这里由
`FluentTeachingTip.UpdatePlacement()` 写 `EffectivePlacement` 与两个偏移，格子只认前者。原因是标记里的
`VisualStateManager` 在本运行时不可用（S0-d），不是可选风格。

## 4. 公开面与命名（照 ModernWpf 的做法）

上游 `TeachingTip : ContentControl`，属性：`Title`、`Subtitle`、`IsOpen`、`Target`、`TailVisibility`、
`ActionButtonContent/-Style/-Command/-CommandParameter`、`CloseButton` 同四条、`PlacementMargin`、
`ShouldConstrainToRootBounds`、`IsLightDismissEnabled`、`PreferredPlacement`、`HeroContentPlacement`、
`HeroContent`、`IconSource`、`TemplateSettings`（只读）。
**更正一处常见误解**：上游**没有** `Icon`、没有 `XOffset`/`YOffset`、没有 `Variant`——图标是 `IconSource`
（经 `TemplateSettings.IconElement` 变成元素），位置是 `PreferredPlacement`。

本实现类型名 `FluentTeachingTip : ContentControl`（`Controls/Popup/`），跟本库既有的自有类型一致：
`FluentDropDownButton`、`FluentInfoBar`、`FluentNavigationView`、`FluentSettingsRow` 都带 `Fluent` 前缀。
这是**有意的命名偏离**并记在这里：WinUI 与 ModernWpf 都叫裸 `TeachingTip`。
样式仍按本库惯例：命名样式 `DefaultFluentTeachingTipStyle` + 构造函数与 `Loaded` 里套用
（`FluentInfoBar.cs:44-54` 同一模式）。

## 5. 差异、宿主替换与 Known Gaps

原来 1-3 三条"要先量"的问题已经量完，结论在下面第 1-3 条里，原始读数是
`adaptation/s1b-teachingtip-host-raw.txt`（`spike/TeachingTipProbe` modes `all` / `tail` / `place` / `tip` / `room`）。

1. **弹窗宿主：模板内 `Popup` 这条路成立，代价是卡片不在宿主树里。** 上游在代码里 `new Popup`，
   ModernWpf 把 `Popup` 写进模板（`TeachingTip.xaml:412`）再加 `Container`(417) 与
   `ContentRootGridShadowChrome`(447)——本实现走 ModernWpf 那条。实测：`ContentControl` 调过
   `UseTemplateContentManagement()` 之后，模板里的 `PART_Popup` 会建、会开，`IsOpen=true` 后卡片实化成
   `Container 320x168.12` / `ContentRootGrid 304x152.12` / 按钮 `130x32.78` / `TailPolygon 9x21`。
   **但带 `PlacementTarget` 的弹层在自己的 `PopupWindow` 里**（父链 `Border < PopupRoot < PopupWindow`），
   所以两条老限制在这里换了方向：`audits/menu-flyout.md` §4c 那条"共享宿主给 graft 弹层 0×0"量不到它，
   而**从宿主窗口往下走也找不到部件**——判据改成从 `popup.Child` 往下读。收起时全套件都读 `0x0`，
   那是这条路的静息形状。
2. **尾巴几何：`Polygon` 可用，`Points` 是依赖属性。** `Jalium.UI.Shapes.Polygon : Shape`，公开非抽象、
   有无参构造，自己声明 `Points`，`Fill`/`Stroke`/`StrokeThickness` 从 `Shape` 继承；触发器写 `Points`
   实测生效（D1/D3）。不需要退化成 `Path`，也就没有名字偏离要记。
3. **`GridLength` 行进得去，进不去的是落点。** `<GridLength x:Key=…>8</GridLength>` 读回 `GridLength = 8`，
   `*` 同理；但 `ColumnDefinition.Width="{ThemeResource …}"` **被静默丢弃**（五列全停在类型默认的 `*`，D2），
   因为 `ColumnDefinition` 不在视觉树里、动态查找没有继承上下文。所以 `8|10|*|10|8` 五条带是模板里的字面量，
   两条 `GridLength` 行不进字典（进了也没读者，会被消费点闸口拦下）。顺带更正 S0-b 的读法：
   `x:Double`/`x:Int32` 行不是"读不到"，是**整份字典解析失败**，8 条 `x:Double` 度量因此全是字面量。
4. **头/尾关闭按钮两种摆位**：上游靠 `CloseButtonLocations` 组在模板里切；本批只落 `FooterCloseButton`
   （底部按钮行里的 `CloseButton`），`AlternateCloseButton`（右上角 40×40、`E711`）先不发，
   因为它带的 13 条 `TeachingTipAlternateCloseButton*` 行需要一个我们没有的 `BackgroundSizing`
   与 `SymbolThemeFontFamily`（图标字体批欠，见 `memory: jalium-icon-font-and-text-defects`）。
5. **hero 内容**：`HeroContentPlacementStates` 依赖 `Top/BottomCornerRadiusFilterConverter`（把圆角按边
   过滤成 `8,8,0,0`/`0,0,8,8`）。本运行时没有这两个转换器，圆角分边在 `ContentDialog` 批是**手写死两格**
   绕过的（`adaptation/00` S0-x·5）。hero 本批不做，记为缺口。
6. **`ThemeShadow` 与过渡时长**：运行时没有 `ThemeShadow`；300/200 ms 在上游是 C++ 私有字段。
   本库既不发阴影行也不发时长行，`Motion/` 里要不要给 TeachingTip 一条自有时长，等材质/动画批（任务 8）统一决。
7. **高对比度**：HC 档 21 条全部指向 `SystemColor*`，本运行时无公开 HC 入口（`adaptation/00`
   "高对比度：公开管线里没有入口"），因此 HC 分支不发布，与其余批次同一处理。
8. 真指针/触摸/键盘下的打开与关闭未证（任务 13）；按钮的调用走的是自动化 peer，放置走的是 `TranslatePoint` 读回。
9. **没有点外关闭**：本运行时的 `Popup` 有 `StaysOpen`，但"点外面之后谁来把 `IsOpen` 写回 false"这条通路
   没量过——一个在控件背后自己关掉的卡片只会让 `IsOpen` 与屏幕不一致，所以宁可不接。
   `TeachingTipTransientBackground`（上游 light-dismiss 那一格唯一读的行）因此也不发布。
10. **没有图标位**：宿主没有 `IconSource`（那是 WinUI 自己的一族类型），`TemplateSettings.IconElement` 更不存在，
    所以两条 `TeachingTipIconMargin` 不发。要做图标得先给本库一个图标源抽象，与 CheckBox/RatingControl 同一笔欠账。
11. **顶部高光不做，圆角因此没有代价。** 上游 `TeachingTipTopHighlightBrush` + `…OffsetForBorder` 是一条
    通宽的 1 DIP 亮线，压在 r=8 的弧上；`Border` 在本运行时**不裁剪子元素**（S0-w），那条线会露出直角头。
    `ContentDialog` 批用"每个带自己吃一半弧"绕开，高光这一条没有同样的解法，于是两行不发、卡片只留描边与底色。
12. **窄窗口不缩**：上游用 `ScaleX` 把卡片压进可用宽度，本实现只有 `MinWidth 320 / MaxWidth 336` 的字面量盒子，
    窗口再窄也不缩放——记为形状差，不当作已复刻。

**字典账单**：上游一共 **50 个行名**（三档同名的 20 个 + 分支外 30 条，那个 `AlternateCloseButtonStyle` 是样式
不是行，不计）。本批发布 **21 行**（5 条画刷别名 + 16 条 `Thickness`），全部有消费点；其余 **29 行**按上面
4-12 各条理由不发，**每一行都有名字**，逐行配一条 `A_row_with_no_consumer_is_not_published` 反向断言。

## 6. Card：不做起自有类型，因为 WinUI 里没有这个控件

路线图阶段 4 那行写的是"Card"，本段按上游证据改判。查证结果：
`microsoft-ui-xaml` 全仓 **没有 `Card` 运行时类**（`class Card` / `runtimeclass Card` 在所有 `.idl` 里 0 命中），
没有 `Card.xaml`，也没有 `Card_themeresources.xaml`；只有**颜色令牌**一族：
`CardStrokeColorDefault`（dark `#19000000` @46 / light `#0F000000` @250）、`CardStrokeColorDefaultSolid`
（`#1C1C1C` / `#EBEBEB`）、`CardBackgroundFillColorDefault`（`#0DFFFFFF` / `#B3FFFFFF`）、
`…Secondary`（`#08FFFFFF` / `#80F6F6F6`）、`…Tertiary`（`#12FFFFFF` / `#FFFFFF`），刷子在 136-148 / 340-352 /
HC 464-476。WinUI Gallery（`Samples/WinUIGallery`）的"卡片"是**手搓 Border**：
`SampleSupport/SamplePages/CardPage.xaml:50-58` = `BorderThickness 1` + `CardStrokeColorDefaultBrush` +
`CornerRadius 8`，页脚 81-84 是 `Padding 16,12` + `CardBackgroundFillColorDefaultBrush`，磁贴 25-31 用
`CornerRadius 4` + `AcrylicBackgroundFillColorBaseBrush`；`Styles/Grid.xaml:4-26` 的
`GalleryTileGridStyle`/`DesignRowCardStyle` 同理（圆角走 `OverlayCornerRadius`/`ControlCornerRadius`），
**都没有阴影**。ModernWpf 也没有 Card（只有 `ModernWpf.Gallery/Assets/Design/Cards.{light,dark}.png` 一张图）。

因此本段交付的是**令牌 + 配方**，不是类型：`Card*` 一族按上游键名与色值进主题字典，Gallery 侧用 `Border`
照 CardPage 的数值摆卡片面，并把"WinUI 没有 Card 控件"这条证据写进 Catalog。
若将来要一个真类型，得先证明有配方覆盖不了的行为缺口——按 AGENTS.md"只有证明的行为缺口才起自有类型"，
现在没有这个证据。
