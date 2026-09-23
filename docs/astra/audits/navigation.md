# NavigationView 九步出口审计（阶段 5 第七段，2026-09-20）

## 0 · 依据

- 上游：`microsoft-ui-xaml` @`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
  - `controls/dev/NavigationView/NavigationView_themeresources.xaml`，blob `aa3ff11b2c3128dd198d7140417594cfa6fe5c74`
    （ThemeDictionaries：Default 5-68、Light 71-134、HighContrast 137-200；度量行 203-265；左栏 presenter 样式 440-500；
    指示条 602）
  - `controls/dev/NavigationView/NavigationBackButton.xaml` 26-52（`NavigationViewButton*` 六行的真正消费者）
- 方法参照：`ModernWpf` @`23555a6c00623b2f80e67f20d7f1df49a1d28ad8`
  - `ModernWpf/ThemeResources/{Light,Dark,HighContrast}.xaml` 各发 52 条 `NavigationView*` 行（Light 1382 起、Dark 1379 起、
    HC 1357 起），三条指示条度量行它**没有**发、直接写死在模板里；
  - `ModernWpf.Controls/NavigationView/NavigationView.xaml:176`：左栏条目 `CornerRadius = {DynamicResource OverlayCornerRadius}`
    ——与上游 :447 同一把读法。两家权威在这条上完全一致，这是 §3 判定的根据。
- 运行时：NuGet Jalium.UI 26.10.9（不是隔壁源码树）。

## 1 · 上游键清单（逐条数过，不是目测）

用脚本数 `x:Key=` 的出现次数：

| 位置 | 行数 | 内容 |
| --- | --- | --- |
| ThemeDictionaries / Default | 64 | 全部是 `<StaticResource>` 别名行，0 条 SolidColorBrush |
| ThemeDictionaries / Light | 64 | 与 Default **同名同目标**：按 (name → target) 排序后的多重集完全相等 |
| ThemeDictionaries / HighContrast | 64 | 名字集合与 Default 相同，目标改指 `SystemControl*` / `SystemColor*` |
| ThemeDictionaries 之外 | 63 | 40 条 `Thickness` + 20 条 `x:Double` + 3 条 `CornerRadius` |

两条结论直接影响本批的形状：

1. **Light 与 Default 完全同文**，所以本库发一份与主题无关的别名层就是忠实的转录，不需要复制两份（这正是
   `ThemeResources/ComboBox.jalxaml` 早就落地的判法）。高对比不是"另一份"，而是**同名改指**；我们的
   `ThemeResources/HighContrast.map` 已经在别名目标的下面把 `SubtleFillColorSecondaryBrush` 这类 token 重映射，
   所以"往下指一层"在高对比下自动跟上，不必抄第二块。
2. **63 条度量行里 20 条是 `x:Double`**，而本 reader 对 `x:Double`/`x:Int32`/`x:String` 资源行的实测行为是
   **整份字典炸掉**（`adaptation/00` S0-b 更正后的读数）。也就是说指示条那三个可读的名字（`Width` 3 / `Height` 16 /
   `Radius` 2，上游 220-222 且 602 行真的在消费它们）**在本运行时不可能成为资源行**，只能以字面量进模板。
   隔壁 TabView 批量的六条同类账、DataGrid 批量的 `ListAccentLowOpacity` 都是同一条锁。

## 2 · 差分：64 条别名行里落 19 条，45 条按组给理由

落地清单（全部由 `Styles/Navigation.jalxaml` 消费，键消费闸口逐名可查）：
条目填充 8 名（`NavigationViewItemBackground{,PointerOver,Pressed,Disabled,Selected,SelectedPointerOver,SelectedPressed,SelectedDisabled}`）、
条目文字 8 名（同一套后缀的 `Foreground` 版）、宿主 3 名（`NavigationViewContentBackground`、
`NavigationViewContentGridBorderBrush`、`NavigationViewSelectionIndicatorForeground`）。

未落地的 45 条，按"为什么这里没有它"分组，**每条**都在
`AstraNavigationTests.A_row_this_control_does_not_build_is_not_published` 里被钉成"不得发布"（45 条别名 + 5 个读不动的度量名 = 50 例）：

| 组 | 条数 | 理由 |
| --- | --- | --- |
| `TopNavigationViewItem*` | 13 | 顶部导航条这一形态本控件根本不构建（无 top mode）。发一条没人读的名字只会显得可覆盖 |
| 条目 `BorderBrush*` | 12 | 上游左栏根是 `Grid`（:451），它不画边框：12 条里有 8 条别名到 `SubtleFillColorTransparentBrush`，另外 4 条 checked 版同属一个不存在的描边。描边厚度行 `NavigationViewItemBorderThickness`=1 也是这个不存在的面的 |
| `NavigationViewItem*Checked*`（bg 4 + fg 4） | 8 | 我们的 `FluentNavigationItem` 只暴露 `IsSelected`，没有 checked 态。上游那四条走的是 `NavigationViewItemPresenter` 的 Checked 视觉态，本类型没有那个状态 |
| `NavigationViewButton*` | 6 | 真正的消费者是 `NavigationBackButton.xaml:26-52`（其中 `BackgroundDisabled` 上游自己都没人读），而本控件没有返回按钮 |
| 三条 pane 背衬（`DefaultPaneBackground`/`ExpandedPaneBackground`/`TopPaneBackground`） | 3 | 值分别是 `AcrylicInAppFillColorDefaultBrush`/透明/透明：那是**材质**，属于并行任务"材质参数与生效范围摸底"，本批不猜它的生效范围。 pane 现在落在 `SolidBackgroundFillColorBaseBrush` 上，差异记进 §6 |
| `NavigationViewItemIconBackground` | 1 | 别名到 `SystemControlTransparentBrush`，上游给图标盒用；我们的图标格是 `ContentPresenter`，没有可着的盒 |
| `NavigationViewItemSeparatorForeground` / `NavigationViewItemHeaderForeground` | 2 | 分组头与分隔线：本控件的条目集合只有 item，没有 header/separator 容器 |

度量行那边：`NavigationViewItemButtonMargin`（`4,2`，228）落地并被条目 `Margin` 消费，且断言"落在条目上"而不只是"声明在字典里"；
其余 39 条 `Thickness` 属于 top/overflow/auto-suggest/header 这些本控件没有的面；3 条 `CornerRadius` 是上游内容网格
（`8,0,0,0` 那一族）的形状，我们的卡片由自己的模板画。剩下 20 条 `x:Double` 全部以字面量进模板，本批把其中
6 个值和它们的落点对上号：36（条目 MinHeight）、40（图标列宽 / pane 按钮宽）、16（图标与指示条高）、48（compact 宽，
在代码侧 `CompactPaneLength` 默认值上）、3 与 2（指示条宽与圆角）。

## 3 · 结清一条自造名：`NavigationViewItemCornerRadius`

这条是本批存在的理由，也是 `ThemeResources/Metrics.jalxaml` 头注释里挂着的那笔欠账。实测：

- `git grep NavigationViewItemCornerRadius` 在 19e3bdc3c 的整个 `microsoft-ui-xaml` 里 **0 命中**；
- 上游左栏条目样式 `MUX_NavigationViewItemPresenterStyleWhenOnLeftPane`（:440-451）把 `CornerRadius` 设成
  `{ThemeResource OverlayCornerRadius}`；现代 WPF 权威（`NavigationView.xaml:176`）也是 `OverlayCornerRadius`；
- 我们发的那行是 `4`——**恰好等于 `ControlCornerRadius`**，也就是"ControlCornerRadius 的值穿了个导航的名字"。
  上游的 `OverlayCornerRadius` 是 8。

处理：撤名（`Metrics.jalxaml` 里那行删掉，字典数 48→49 但 `Metrics` 少一行），模板改读 `{StaticResource OverlayCornerRadius}`
（它是与主题无关的度量，不是调色板 token，所以 `StaticResource` 不违反"token 不得冻进 StaticResource"那条闸口）。
读回与像素各量一次：条目 `CornerRadius` 与模板 `Root` 边框的 `CornerRadius` 都等于 `OverlayCornerRadius`，
且断言它**不等于** `ControlCornerRadius`——这条会抓住"哪天又滑回 4"；被撤的名字由测试钉成"不得再发布"。

顺带一条同源发现：这条账不是"多写了个名字"，而是**一条视觉缺陷**——库里的侧栏条目圆角从 Astra 落地那天起就比 WinUI 少一半。
它属于用户点名的那一族问题（"有的这种控件，它的圆角好像都不对"），所以 §6 把它单列，而不是混在键清单里。

## 4 · 上游视觉态 → 本运行时触发器映射

上游是 `VisualStateManager` 的一态一 setter 块；这里是 `ControlTemplate.Triggers`，靠**声明顺序**决定谁赢，
所以映射表必须带顺序。左栏 presenter（:455-470 的 PointerStates / DisabledStates）对应：

| 上游态 | 本批写法 | 落点 |
| --- | --- | --- |
| Normal | 样式 setter | `Root.Background` ← `…Background`，父级 `Foreground` ← `…Foreground` |
| PointerOver | `Trigger IsMouseOver` | 同上 |
| Pressed | `Trigger IsPressed` | 同上 |
| Disabled | `Trigger IsEnabled=False` | 同上（本批新加填充；原先只有文字） |
| Selected（左栏即"选中就有 pill"） | `Trigger IsSelected` | 同上 |
| Selected+PointerOver | `MultiTrigger`（三个条件含 `IsEnabled=True`） | 两名分别指到 `…SelectedPointerOver` |
| Selected+Pressed | `MultiTrigger` | `…SelectedPressed` |
| Selected+Disabled | `MultiTrigger`（本批新增） | `…SelectedDisabled` |

**为什么这两格不是混合出来的**：`…BackgroundPressed`=Tertiary 而 `…BackgroundSelectedPressed`=Secondary，
即"在选中行上按下"会把填充**退回一档**；文字同理从 Primary 退到 Secondary。上游为每一对单独发一个名字，
就是在说这不是透明度可以合成出来的东西——所以必须两名两触发器，不能一条通用 alpha。

前景的落点是父级控件而不是某个 presenter：`ContentPresenter.Foreground` 在本运行时是哑写（`adaptation/00` S1-g
那条"前景要写在真的画字的东西上"），而控件自身 `Foreground` 的继承确实把字与图标一起带走了（Gallery 侧栏一直在用）。
图标是否被继承带走**没有量**，见 §6。

## 5 · 四类证据

- **构建**：串行闸口 `tools/Test-AstraGates.ps1`（本批末尾一次），`docs/astra/adaptation/s1k-navigation-raw.txt` 收了读数。
- **行为/结构**：`AstraNavigationTests` 80 例（4 例原有布局契约 + 74 例本批 + 2 例 #94）：
  19+1 条键逐名"已发布"、45+5 条逐名"不得发布"、别名**同一实例**（8 条 `Assert.Same`）、
  半径读回（条目 + 模板 `Root`，且 ≠ `ControlCornerRadius`）、`NavigationViewItemButtonMargin` 落到 `item.Margin`、
  指示条几何钉在动画器常数上（`NavigationIndicatorAnimator.RestingHeight` == 模板 16 == 上游行 16）、
  一个别名行跨主题变色（Light↔Dark 不同色，因为变的是目标）。
  写的时候踩到两条闸口外的账：见下面"两条 harness 读数"。
- **像素**：选中行相对未选中的**同一个色键增量** 8 220 px（`#EAEAEA` = SubtleFillColorSecondary 5.54% 叠在 pane 的
  `#F3F3F3` 上），阈值取 6 000；品牌绿 `#207245` 0 px；指示条那 1×8 那个点采样到的就是强调色刷的 RGB。
  #94 另加两条：实时换档把条目的墨交给图标（删掉交付那一行即红），图标自带前景时不被覆盖（去掉判据即红）。
- **视觉/Gallery**：`Catalog.json` 两条 `NavigationView`/`NavigationItem` 行改指本审计并补齐 gaps；
  Gallery 侧栏就是这个控件本身，`Test-AstraGallerySmoke.ps1` 跑 `navigation` 页干净关窗。

两条 harness 读数（都各花了一个周期，已写进 `adaptation/06`）：
`PixelHarness.Host()` 把主体挂到它自己的窗口上，所以走过 `Build()` 的元素再走 `Host()` 会抛
"The logical child already has a parent"；而**第一次** `Host()` 的读数与后续不同源（同一对捕获在两次运行里
分别量到 30 054 与 16 624 个变化像素，差在窗体背衬自己还没稳），所以本批的像素断言只用**单色键增量**，
不用整张直方图差值。

## 6 · Known Gaps（不声称）

1. **hover / press 零真输入证据**：状态格子只能量到"触发器在位且写在对的要素上"，指针进出的实际帧没有采过
   （并行任务 #13 还没通）。
2. **圆角只量到属性，没量到像素**：`Root` 边框的 `CornerRadius` 读回 8，8 DIP 圆角在角上真的把填充留出去了，
   这一条没有采样（`PixelAt` 的角点采样在下一批补，宁可不写也不猜）。
3. **图标是否跟着前景状态变色**：条目前景换档时图标不重着色这条已经读到像素并修掉了（#94，见 §10）；
   仍未读的是 hover / pressed 状态下图标跟不跟标签一起变（§10 的测点只覆盖 Light↔Dark）。
4. **pane 背衬是纯色不是亚克力**：上游 `NavigationViewDefaultPaneBackground`=`AcrylicInAppFillColorDefaultBrush`；
   本批不接（材质摸底未做），差一层透明/模糊。
5. **高对比未测**：本层的别名向下指到 `HighContrast.map` 已重映射的 token，但导航这一族在 HC 下的读数一次都没量。
6. **没有 top mode、没有返回按钮、没有 header/分隔线/设置项**（§2 那 13+6+2+1 条键就是这些面）；
   条目也没有 `IsChecked`，所以 8 条 checked 键在这个类型上没有对应状态。
7. **虚拟化/超长列表**：本控件是 `StackPanel` 摆条目，没有回收路径，条目多了会全部实例化。
8. **20 条 `x:Double` 度量值是字面量**，不是可覆盖的行；换主题不能改它们，应用侧也覆盖不了。

## 9 · 为什么仍是自有类型（带读数的判断）

运行时 26.10.9 有 `Jalium.UI.Controls.NavigationView`（普查 `adaptation/01:92-95`：`style=False render=False
parts=0`；`s0y-outstanding-names.txt:16`：`templateLock=Void:null-or-void`，16 个自声明属性），也就是
**类型在、样式与部件树不在**。因此"换成原生重模板"省不下任何事——整棵模板照样要自己写，还要另外量清那 16 个
属性的契约，而这一棵树的每一格都已经由 `FluentNavigationView` 量过、并且是 Gallery 侧栏正在用的那条路径。

本批按这个判断继续用自有类型。`Catalog.json` 的 `parity` 因此不再写 `own-type`（legend 对它的措辞是"Jalium
没有这个原生类型"，这句在 NavigationView 上不成立），改写 `audited`，并把这条判断连它的读数写在 §9，
而不是留在标签里当结论。

## 10 · 用户可见缺陷 #94：实时换档时侧栏图标不重新着色

用户报的现象：浅色下侧栏图标不是黑的，切到深色这些图标还是黑的。两边都对上了，只是方向相反——
**图标停在它第一次落笔那一档的墨上**，标签照常换，图标不动。

读数分三层，各占一类证据：

1. **解析层（不是缺陷）**：从派生类调 `GetEffectiveForeground()`，在一棵活树上挂载 Light、翻 Dark、再翻回 Light，
   三次读到的都是**当前档**的刷（`#E4000000` / `#FFFFFFFF` / `#E4000000`），而图标自己的 `Foreground` 一直是
   `<null>`（`spike/NavIconRecolor/probe-ink.log`）。也就是说框架那条"沿视觉树上溯到某个 `Control` 的 `Foreground`"
   是通的，答案一直是对的。
2. **像素层（缺陷在这）**：进程内 `RenderTargetBitmap` 看不见它——它重跑一遍渲染，拿到的永远是当前档。
   只有抓屏能回答"用户看见什么"。`spike/NavIconRecolor/shoot.ps1` 把 Gallery 的起始档钉在开窗之前，
   再在活窗口上翻档，两次抓屏按图标列（物理 x 349-415、y 200-610，只有 pane 底色、选中胶囊与字形墨）数颜色：

   | 腿 | pane 底色 | 字形墨 | 判读 |
   |---|---|---|---|
   | 无修法：Light 起手、未翻档 | `#F3F3F3` 21567 | `#1A1A1A` 463 | 挂载时是对的 |
   | 无修法：同一窗口实时翻到 Dark | `#202020` 21563 | `#030303` 463、`#0B0B0B` 129 | **深色 pane 上一片黑墨** |
   | 有修法：Light → Dark | `#F3F3F3` / `#202020` | `#1A1A1A` 463 → `#FFFFFF` 545 | 跟档 |
   | 有修法：Dark → Light | `#202020` / `#F3F3F3` | `#FFFFFF` 545 → `#1A1A1A` 463 | 跟档 |

   早一版脚本把起始档放在 `window.Show()` 之后才应用，于是首帧是 Dark，六枚图标全部量到 `#FFFFFF` 636 落在
   `#F3F3F3` 的 pane 上——同一个缺陷的反方向。两条腿的计数逐行对得上（`545 / 127 / 119 / 103 / 91 / 89`），
   这说明量的确实是同一批像素。
3. **机制**：`IconElement` 只在**自己**的 `Foreground` 变化时 `InvalidateVisual()`；换档改的是条目的属性，
   图标没有属性变化就没有重绘，屏幕上留着上一次落笔的墨。所以修法不是"补一次 `InvalidateVisual()`"，
   而是把条目的墨交给图标的 `Foreground`——这条运行时里只有"值变了"这一条路既换值又标脏，而且留下一个
   读得到的值给回归事实（`Controls/Navigation/FluentNavigationItem.cs:OnIconChanged`）。图标自带前景时不覆盖：
   那是本地值，上游也让它优先。

突变见证（`spike/NavIconRecolor/teeth.sh`，两条事实各自只被自己的突变弄红）：交付那一行换成空块 →
只有换档那条红，报 "the pane icon carries `<null>` while its item moved to `#E4000000`"；去掉"图标自带墨"的判据 →
只有保留那条红。每一腿都打印测试工程输出目录里 `FluentJalium.dll` 的 sha1，证明跑的就是重建后的那份。

一条踩过的坑要留字：第一次做这个 A/B 时只 `dotnet build src/FluentJalium` 再用 `dotnet test --no-build` 跑，
测试进程加载的还是上一版 `FluentJalium.dll`，突变根本没进被测进程，于是把一条有牙的事实读成了哑的。
要刷新生效的是**测试工程**的构建，且必须有"消费到的 DLL 变了"的见证。
