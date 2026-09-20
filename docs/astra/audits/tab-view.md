# TabView / TabViewItem 适配审计（阶段 5 第四段，2026-09-20）

上游：`microsoft-ui-xaml` @`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`（只读）。

- `controls/dev/TabView/TabView_themeresources.xaml` blob `e4db61f8793560b59fa8390ade0a59cc9addeb13`
  （270 行：ThemeDictionaries 为 Default 82-159、Light 4-81、High Contrast 160-237，每支 68 行；
  字典外还有 239-269 的 31 行度量）
- `controls/dev/TabView/TabView.xaml` blob `c0e732a4604036f8cce7fee1aed757a9fc4bacdb`（649 行：
  `DefaultTabViewStyle` 5-53、`TabViewListView` 样式 54-131、`TabScrollViewerStyle` 133-172、
  `TabViewCloseButtonStyle` 174-226、`TabViewButtonStyle` 228-290、`TabViewItem` 样式 292-599、
  `TabViewScrollButtonStyle` 601-649）
- `controls/dev/TabView/TabViewItem.cpp` blob `5c2479d2984739a3ce5e38e00ee2b5690ffe30ae`
  （`UpdateTabGeometry` 98-122：选中页轮廓那段 Geometry 是 C++ 现拼字符串再 `XamlReader::Load`）
- `controls/dev/TabView/TabViewListView.cpp` blob `4ff0c810110204b5b47e39412d242cabafafba7f`（110 行，
  条目容器类型，`SingleSelectionFollowsFocus` 的持有者）

运行时权威：NuGet Jalium.UI **26.10.9**。测量件：`spike/TabViewProbe`（pass 1，类型普查与模板赋值）、
`spike/TabViewTint`（pass 2，逐杠杆着色与归属）、`spike/TabViewStyle`（pass 3，模板到达通路，
`tab-style1.txt`）；上游行清单 `spike/TabViewProbe/upstream-keys.txt`。底座结论
`adaptation/00-jalium-theme-capabilities.md` S1-h。

## 0 · 这一段的决定：自有类型，而且是两处

计划里写的是"TabView(自有)"。这一段没有照抄这个判断，而是先把它变成一次测量：**原生这一对到底能不能重模板？**
`AGENTS.md` 要求"原生控件优先重模板，只有证明的行为缺口才自有类型"，所以缺口必须先量出来。

pass 3（`spike/TabViewStyle/tab-style1.txt`）在同一窗口里同时跑一个 `ListBox` 作阳性对照，用两条不同的通路喂模板：

| 段落 | 被喂模板的类型 | 通路 | 部件是否出现 | 模板底色像素 | 结论 |
| --- | --- | --- | --- | --- | --- |
| C | `ListBox` | 隐式样式的 `Template` setter（每个 Astra 列表样式用的就是这条） | 是（`ProbeListRoot`/`ProbeListItems` 都在树里） | 54600 | 通路本身有效 |
| D | `TabControl` | 同上 | 否 | **0**（420×130 里 magenta 一个没有） | 样式通路不建 |
| E | `TabItem` | 同上 | 否 | 0 | 条目同理 |
| G | `TabControl` | 本地值 `Template=…` | 否 | 0 | 与 pass 1 一致 |
| H | `TabItem` 子类 + 构造函数里 `UseTemplateContentManagement()` | 样式 setter | 否 | 0 | **连开关都救不了**：`TabItem` 的 `OnRender` 照画自己那套 |
| I | `ContentControl` 子类 + 同一个开关 | 样式 setter | 是 | 9640 | 这条链能建，也就是 `FluentNavigationView` 一直在走的路 |

另外三条同批量到的事实：

1. **开关的位置**（section A，反射读声明类型）：`UseTemplateContentManagement` 是 `ContentControl` 上的
   `protected` 成员。`TabItem : HeaderedContentControl : ContentControl` 够得着；
   `TabControl : Selector : ItemsControl : Control` **链路上根本没有 `ContentControl`**，所以任何子类都够不着——
   宿主不是"没做"，是"做不到"。
2. **要模板反而更坏**（section D/F）：一旦给 `TabControl` 写上 `Template`，它自己的条 measure 成
   `StackPanel 0x0`、页签宽 0，但仍在画。也就是说重模板失败不是"退回原样"，而是"退回一个塌陷的原样"。
3. **选中机制跟模板无关**（section F）：`SelectedIndex` 改到 1 之后 `SelectedContent`、可见正文、
   `IsSelected` 三处都跟着走。所以原生那套的**行为**是好的、**外观通路**是死的——本批因此保留它的语义形状
   （选中不跟随焦点、`SelectionChanged` 报两端），只换掉它的绘制。

结论：宿主与条目都起自有类型，基类都走 `ContentControl` 这条被证明能建树的链
（`Controls/Navigation/FluentTabView.cs`、`FluentTabViewItem.cs`）。`FluentTabViewItem` 选
`HeaderedContentControl` 是为了保住上游"`Header` 在条上、`Content` 在 body"这一刀切分，而不是随手挑的基类。

## 1 · 上游 68×3 + 31 行：发 64（分支 53 + 度量 11），withhold 15 + 20 条度量

先给计数本身，因为它就是这一节的证据。三个集合都是把上游文件按行号切出来 `grep x:Key=` 数出来的
（分支块 `sed -n '4,81p'` 读回 69 个名字，多出来的那一个是第 4 行 `<ResourceDictionary x:Key="Light">` 本身，
扣掉即 68 条行），我们这一份按同样方式数：

`Light`(4-81) 与 `Default`(82-159) **逐字节相同**（`diff` 两个 68 行块：无输出），所以 `ThemeResources/TabView.jalxaml`
一份覆盖两支，跟其他控件字典一样。High Contrast(160-237) 的差异是**目标**不同而非名字不同，本运行时没有
HC 作用域（`adaptation/00` S1 系列），因此整支 withhold，只在下面列名：

- 全部 `TabView*`/`TabViewItem*` 行改指 `SystemColor*` 系列（45 行的 `ResourceKey` 换目标）；
- `TabViewBorderBrush` 从 `StaticResource` 别名变成真正的 `SolidColorBrush`（`{ThemeResource SystemColorHighlightColor}`）；
- `TabViewButtonBorderThickness` 与 `TabViewItemHeaderCloseButtonBorderThickness` 由 `0` 变 `1`；
- `TabViewSelectedItemBorderBrush` 渐变的终点色换成 `SystemColorHighlightColor`。

有消费者、因此发布的 53 条分支行（分组相加即 53）：`TabViewBackground`、`TabViewBorderBrush`、
`TabViewItemBorderBrush` 3 条，5 条 header 底色、5 条 header 前景、5 条 icon 前景、`TabViewItemSeparator`，
`TabViewButton*` 13 条（`Background`/`Foreground`/`BorderBrush` × rest/PointerOver/Pressed/Disabled 共 12，加 `BorderThickness`），
`TabViewItemHeaderCloseButton*` 12 条（3 支底色、5 支描边色含 `…BorderBrushSelected`、3 支字色、加 `BorderThickness`）、
`TabViewItemHeader{PointerOver,Pressed,Selected,Disabled}CloseButton{Background,Foreground}` 8 条、
`TabViewSelectedItemBorderBrush` 1 条。
另外从字典外那 31 条度量里发 11 条（`TabViewHeaderPadding`、`TabViewItemHeaderPadding`、
`…PaddingWithCloseButton`、`…PaddingWithoutCloseButton`、`TabViewItemHeaderIconMargin`、`TabViewItemHeaderCloseMargin`、
`TabViewItemSeparatorMargin`、`TabViewItemBorderThickness`、`TabViewItemAddButtonContainerPadding`、
`TabViewSelectedItemBorderThickness`、`TabViewSelectedItemHeaderMargin`——后两条只存在于度量块，分支块里没有它们）。
53 + 11 = 64，正是 `ThemeResources/TabView.jalxaml` 的 `x:Key=` 计数。

withhold 的名与理由（正向由消费点闸口盯着：`AstraResourceKeyTests.Transcribed_control_rows_are_read_by_a_template`
本批把 `ThemeResources/TabView.jalxaml` 接了进去；反方向由 `AstraTabViewTests.A_withheld_upstream_row_is_not_published`
的 24 个名字钉住，下面这些名一旦漏进来那条就会红）：

| 名 | 上游行 | 为什么现在不发 |
| --- | --- | --- |
| `TabViewScrollButton{Background,Foreground,BorderBrush}{,PointerOver,Pressed,Disabled}` | 12 行 | 溢出滚动的两个 `RepeatButton` 本批不交（§7） |
| `TabViewButtonBackgroundActiveTab`、`TabViewButtonForegroundActiveTab` | 2 行 | 上游把它们给"有页签处于活动态"时的 + 按钮；本批不升这个状态 |
| `TabViewItemHeaderDragBackground` | 1 行 | 无拖拽通路（§7）；它的 `TabDragVisualContainer` 部件也就不存在 |
| `TabViewItemScrollButtonPadding`、`TabViewItemLeft/RightScrollButtonContainerPadding` | 3 行 | 同上，是那两个滚动按钮的容器度量 |
| `TabViewSelectedItemHeaderPadding` | 1 行 | 上游选中页的 padding 与 `…PaddingWith/WithoutCloseButton` 两条重叠，模板读的是后两条 |
| 其余 16 个 `x:Double` 度量 | `TabViewItemMinHeight` 32、`MaxWidth` 240、`MinWidth` 100、`HeaderFontSize` 12、`HeaderIconSize` 16、`HeaderCloseButton{Height,Width,Size}`、`HeaderCloseFontSize` 12、`ScrollButton{Width,Height}`、`ScrollButonFontSize` 8、`AddButton{Width,Height,FontSize}`、`TabViewShadowDepth` 16 | 这个 reader 解析不了 `x:Double` 资源（`adaptation/00` S0-c）。取值以字面量落在使用处，每个使用点旁边都写了它对应哪个上游名 |

## 2 · 形状对照：上游部件 → 本批部件

| 上游（`TabView.xaml`） | 本批（`Styles/TabView.jalxaml`） | 说明 |
| --- | --- | --- |
| `Grid TabContainerGrid` + 4 列（`LeftContentColumn`/`TabColumn`/`AddButtonColumn`/`RightContentColumn`）:32-36 | 同名同列数 | `TabListView` 名字保留，但它在本批是 `StackPanel` 而不是 `TabViewListView` |
| `primitives:TabViewListView` :38 | `ScrollViewer TabScrollHost` > `StackPanel TabListView` | 运行时不给自有类型 `ItemsPresenter` 契约，容器由视图自己摆放（`RefreshStrip`），与 `FluentNavigationView` 同一做法 |
| `Border AddButtonContainer` > `Button AddButton`（`E710`）:40-41 | 同名同字形 | 可见性由 `IsAddButtonVisible` 触发器写 |
| `ContentPresenter TabContentPresenter` :46 | 同名 | `Content={TemplateBinding SelectedContent}`；上游由 `TabViewListView` 的选中项驱动 |
| `LeftBottomBorderLine` / `RightBottomBorderLine`（1 DIP，`TabViewBorderBrush`）:37-38 | 同名同高 | |
| 条目：`Grid LayoutRoot` 3 列 + `BottomBorderLine` + `LeftRadiusRenderArc`/`RightRadiusRenderArc`（4×4 两段字面 Path data）+ `SelectedBackgroundPath` + `TabSeparator` + `TabContainer` + `IconColumn`/`IconBox`/`IconControl` + `ContentPresenter` + `CloseButton`（`E711`）:303-541 | 同名部件齐全，除两点：`SelectedBackgroundPath` 换成 `SelectedBackgroundSurface`（一个 `Border`），`LayoutRoot` 的 `Padding` 挪到 `TabContainer` | `Grid` 没有 `Padding` 成员——这正是属性死写批抓出的那一类静默失效（`adaptation/00` S1-g），上游那行照抄过来只会是死写 |
| `TabContainer` 的 `CornerRadius` 走 `TopCornerRadiusFilterConverter` :505 | 条目自己的只读行 `TopCornerRadius`，在 `MeasureOverride` 里由 `CornerRadius` 滤出上两角 | 没有转换器可用；断言读的是 `Border` 上的实际四元值 |

三条本批新量到的运行时事实（写进 `adaptation/00` S1-h）：

- **`Button` 的默认 `MinHeight` 是 36。** 给 + 与 × 两个按钮样式写了 `Height=24` 之后，它们仍量到 36，
  于是整页签从 32 顶到 44（`Assert.Equal(32d, …)` 读出 44）。样式里补 `MinHeight=0` 后回到 24/32。
  上游没有这一行，它也不需要：它的按钮样式同样是 `Height` 24，但它的 `Button` 没有这个默认最小值。
- **`FontSize` 写在 `ContentPresenter` 上到不了它生成的 `TextBlock`。** 标签量到 19.78（14 的字高）；
  把 `FontSize=12` 挪到条目样式（`Control` 自己有这个成员）之后量到 17.24。这与 S1-f 的
  "`ContentPresenter` 没有 `Foreground`" 是同一族的第二条例子：**能写不等于到达**。
- **`ContentPresenter` 是纯内容宿主，不画笔。** 它的 `Background`/`BorderBrush`/`BorderThickness`/`CornerRadius`/
  `VerticalContentAlignment` 五个成员一个都没有（闸口原文："ContentPresenter 'ContentPresenter' has no
  Background"），而上游两个按钮样式的模板根正是 `ContentPresenter`（`TabView.xaml:191`、`:242`），照搬那个形状
  等于写 10 个死格。本批因此把笔挪到 `Border ContentRoot`，presenter 只留内容与居中——S1-f/S1-g 那一族静默的
  第三种：前两次是前景与描边，这次是整个填充层。

## 3 · VisualState → Triggers 映射表

上游条目有 9 个组（`TabView.xaml:312-543`）。Jalium 没有 `VisualStateManager`，全部走
`ControlTemplate.Triggers`。空格子（写出的属性→值）在上游那一列，本批那一列是实际落到哪个元素。

| 上游组/态 | 上游写的东西 | 本批条件 | 落点 |
| --- | --- | --- | --- |
| `CommonStates/Normal` | 无 | — | 样式默认值 |
| `PointerOver` | `TabContainer.Background`、`ContentPresenter.Foreground`、`IconControl.Foreground`、`CloseButton.{Background,Foreground}`、`TabSeparator.Opacity=0` | `IsMouseOver=True` | 同前四项 + `Foreground`（写在条目上，靠继承到生成的文本元素）+ 分隔线透明度 |
| `Pressed` | 同上五组，Pressed 行 | `IsMouseCaptureWithin=True` | 同上 |
| `Selected` | 折线隐藏、两段弧可见、`SelectedBackgroundPath` 可见+填色、`TabContainer.{Margin,BorderBrush,BorderThickness}`、文字/图标/关闭钮前景、`FontWeight=SemiBold`、`LayoutRoot.Background=Transparent` | `IsSelected=True` | 同一批，`SelectedBackgroundSurface` 取代 Path；上游那条 `TabContainer.Background=TabViewItemHeaderBackground` 未照抄——条目的 `Background` 本来就 `TemplateBinding` 到同一个行，写它是空转 |
| `PointerOverSelected` / `PressedSelected` | 选中形状 + hover/press 底色 | `MultiTrigger`（`IsSelected` + `IsMouseOver`/`IsMouseCaptureWithin`） | 只覆盖 `SelectedBackgroundSurface.Background`，其余留在 `Selected` 格子上 |
| `DisabledStates/Disabled` | 底色回透明行、文字/图标/关闭钮前景、关闭描边 | `IsEnabled=False` | 同上 |
| `IconStates/NoIcon` | `IconBox.Visibility=Collapsed` | `HasIcon=False`（条目维护的只读布尔） | 同上；上游用状态是因为它由 `IconSource` 是否为 null 推，属性触发器没法比 null |
| `CloseIconStates` | 有/无关按钮切换 `TabContainer.Padding`（`8,3,4,3` / `8,3,8,3`）与按钮可见 | `IsClosable=True/False` | 两条触发器都写，保持上游"状态机写显式值"的形状 |
| 底部线组（条目级）`LeftOfSelectedTab`/`RightOfSelectedTab`/`NoBottomBorderLine` | `BottomBorderLine.Margin` = `0,0,2,0` / `2,0,0,0`，或整条隐藏 | `IsLeftOfSelected` / `IsRightOfSelected`（视图算好写给邻居）；选中隐藏走 `IsSelected` | 同上 |
| `TabWidthModes/Compact` | 收成 16 DIP 图标宽 | 未实现 | §7 |
| `ForegroundNotSet/ForegroundSet` | 把 `Foreground` 局部值透给图标与文字 | 未实现（`Foreground` 直接由状态格子写） | §7 |
| `ReorderHint{Bottom,Top,Right,Left}`、`DragStates/*`（11 态）、`DataPlaceholder` | 拖拽与重排的整族 | 未实现 | §7 |
| 宿主 `NormalBottomBorderLine`/`SingleBottomBorderLine`/`NoBottomBorderLine`、`Left/RightBottomBorderLine{Normal,Short}` | 两段线的合并/缩短（由 `TabView.cpp` 布局时决定） | 未实现，交的是上游默认态 | §7 |
| `TabViewButtonStyle`/`TabViewCloseButtonStyle`/`TabViewScrollButtonStyle` 各自的 `CommonStates` | `ContentPresenter` 的 `Background/Foreground/BorderBrush` | `IsMouseOver`/`IsMouseCaptureWithin`/`IsEnabled` | 前两个按钮已交；滚动按钮未交 |

## 4 · 命名与公开面

- 公开键名全部是上游原名（`TabView*`、`TabViewItem*`、`TabViewButtonStyle`、`TabViewCloseButtonStyle`）；
  两条样式键 `DefaultFluentTabViewStyle`/`DefaultFluentTabViewItemStyle` 与本仓其他自有类型同一形状。
- CLR 面（本批新立的名，无上游对应）：`FluentTabView.TabItems`、`SelectedIndex`、`SelectedItem`、
  `SelectedContent`、`IsAddButtonVisible`、`AddTabButtonCommand{,Parameter}`、`TabStripHeader`、`TabStripFooter`、
  `SelectionChanged`、`AddTabButtonClick`、`TabCloseRequested`；
  `FluentTabViewItem.Header/Icon/Content/IsSelected/IsClosable/HasIcon/IsLeftOfSelected/IsRightOfSelected/TopCornerRadius`。
  事件参数类名沿用 `FluentNavigationView` 那一族的形状（`FluentTabViewSelectionChangedEventArgs` 等），
  而不是 WinRT 的 `TabViewSelectionChangedEventArgs`——本仓的公开类型一律带 `Fluent` 前缀。
- 模板部件名沿用上游（`TabContainerGrid`、`TabListView`、`AddButton`、`TabContentPresenter`、`TabContainer`、
  `TabSeparator`、`CloseButton`、`IconBox`、`BottomBorderLine`、`LeftRadiusRenderArc`、`RightRadiusRenderArc`），
  只有 `SelectedBackgroundSurface` 是本批的名——它不是 Path 了，叫 `SelectedBackgroundPath` 会是撒谎。

## 5 · 偏离

1. **宿主基类**：`ContentControl` 而不是 `ItemsControl`。理由见 §0；代价是容器由视图手工摆放，
   没有虚拟化（页签数量级本来小）。
2. **选中不自动补**：`SelectedIndex` 默认 `-1`。上游 `TabView` 也不自动选，但本运行时的原生 `TabControl` 会；
   这里跟上游。
3. **删除选中页后不猜**：`DetachItem` 只把选中清成 `-1`，邻居选择交给应用（Gallery 页里就演示了一次）。
4. **`+` 按钮在不可见时不吃自动化 Invoke**：`Visibility=Collapsed` 已经关掉指针命中，但 `ButtonAutomationPeer`
   仍能被递到，所以 `OnAddButtonClick` 里加了 `IsAddButtonVisible` 的闸（行为事实
   `AstraTabViewTests.The_add_button_is_there_before_it_is_shown_and_reports_only_after_it_is`）。
5. **焦点样式**：上游 `UseSystemFocusVisuals`/`FocusVisualMargin` 未照抄，本运行时的焦点框由自己的层画。
6. **`SelectedIndex` 不校验上界，只校验 `-1` 以下**。第一版把两界都写成抛异常，Gallery 的 `navigation` 页
   因此**根本起不来**：源生成器写的顺序是"先属性、后子元素"，`SelectedIndex="0"` 落在 `TabItems` 还是空的时候，
   `InitializeComponent` 直接抛 `ArgumentOutOfRangeException`（`MainWindow.g.cs:2486`，冒烟脚本第一次跑就是这条栈）。
   现在超界的值留着不选中，等页签真的进来时由 `OnItemsChanged` 落地；两条事实分别钉住
   （`An_index_written_before_the_tabs_arrive_selects_the_tab_it_named`、
   `An_index_below_minus_one_is_refused_and_an_index_past_the_tabs_is_not`，两条都在修之前红过，
   红 message 就是上面那条栈的原文）。上游 `SelectedIndex` 本来也就是个普通索引属性，不带这种范围闸。

## 6 · 不声称清单

- 不声称逐像素等于 WinUI：选中页轮廓那 4 DIP 的外翻钩在本批是方的（§7 第一条）。
- 不声称高对比：整支 HC 行未转录（§1）。
- 不声称触摸：触摸与鼠标共用同一 `MouseLeftButtonUp` 通路（本运行时把触点归到指针事件），
  但**没有真指针输入证据**——`Test-AstraGallerySmoke` 只上屏挂载并裁剪，不注入输入。
- 不声称键盘端到端：`KeyEventArgs` 在本运行时造不出来（`spike/TabViewProbe` section C），
  方向键/Ctrl+Tab/Delete 是通过它们所调用的同一内部方法（`MoveFocus`/`MoveSelection`/`OnCloseRequested`）
  证明的，不是通过一条真实按键。
- 不声称辅助功能：条目没有自己的 `AutomationPeer`，屏幕阅读器看到的不是 SelectionItem 模式。
- 不声称 `TabWidthMode`、拖拽、重排、溢出滚动按钮、`TabStripHeaderTemplate` 存在。

## 7 · 未实现的整族（Known Gaps）

1. **选中页的动态轮廓**：上游在 `TabViewItem.cpp:98-122` 用 `OverlayCornerRadius` 与实测宽高拼一段
   `F1 M0,h a 4,4 …` 几何交给 `SelectedBackgroundPath`。本批用一个 `Margin=-4,0,-4,0` +
   `CornerRadius=上两角` 的 `Border` 加两段字面 4×4 弧 Path 顶上；差的正是钩子外翻那 1 列像素。
2. **拖出/放下/重排**：`CanDragTabs`、`CanReorderTabs`、`AllowDropTabs`、`TabItemDragStarting`、
   `TabItemDropDetected`、`TabItemsReorderStarting/Ended`、`TabDragVisualContainer` 与 11 个拖拽状态。
   本运行时有没有可用的拖拽协议未测，因此不猜 API。
3. **溢出滚动**：`TabViewListView` 的两个 `RepeatButton`（`EDD9`/`EDDA`）、`TabViewScrollButtonStyle` 与其
   12 行色、`ComputedHorizontalScrollBarVisibility` 驱动的两个容器。本批只交了一个可横向滚动的宿主
   （`TabScrollHost`，滚动条设为 Hidden），页签不会被压扁也不会出现按钮。
4. **`TabWidthMode`**：`Equal`/`SizeToContent`/`Compact` 三态与 `TabViewListColumnLengthMin/Max` 一类宽度算术。
5. **宿主底部线状态**：`SingleBottomBorderLine`/`NoBottomBorderLine` 与 `Left/RightBottomBorderLineShort`。
6. **`StripPlacement`**：上游 `TabView` 只有 Top 一条可用路径，但本运行时的原生 `TabControl` 有个
   `TabStripPlacement`；本批既没照抄原生枚举（值集未测）也没自造名，直接不交。
7. **`DataPanes`/`TabItemSource` 数据驱动**：本批只有 `TabItems` 这一条元素集合。
8. **`TabViewItem` 的 `ToolTipTitle`/`ToolTipText`/`IconSource`**：上游那三行是 `DataTemplate`+`IconSource` 面，
   本批的 `Icon` 是一个元素槽。
