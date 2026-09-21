# BreadcrumbBar 审计（阶段 5 第十段）

上游：WinUI 3 `microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`（只读引用）。
方法参照 ModernWpf 1.0 `23555a6c00623b2f80e67f20d7f1df49a1d28ad8`。

## 1. 源文件与 blob

| 文件 | blob | 用途 |
| --- | --- | --- |
| `controls/dev/Breadcrumb/BreadcrumbBar_themeresources.xaml` | `af094a1594bb76d73034e91d8c6e9677d7c67e15` | 29 行 token × 3 段（Default 5-33 / Light 36-64 / HighContrast 67-95） |
| `controls/dev/Breadcrumb/BreadcrumbBar.xaml` | `d7d0b16aae852dc792122bcfc1d788237f784209` | 条目样式 3-327（内含按钮样式 169-259 与 `PART_EllipsisFlyout` 113-152）、条样式 328-338 |
| `controls/dev/Breadcrumb/BreadcrumbLayout.cpp` | `e9bcb9f1fe7fe2e6f9e67e7bc3048de96237aa62` | 度量 41-71、收条目 89-93、首可见条目 101-118、行高 120-135、摆位 139-195 |
| `controls/dev/Breadcrumb/BreadcrumbBarItem.cpp` | `eddccd5245273d209389a7039795d472d72f2777` | 省略号条目 137-148、条目点击 181-192、下拉列表 257-299、开合 301-320 |
| `controls/dev/Breadcrumb/BreadcrumbBar.cpp` | `2c84e298b228b8455143b3eaa84d847782ab99eb` | 索引与自动化重排 326-353、事件 287-299 |

运行时读数与两处推翻照抄的证据在 `docs/astra/adaptation/s1n-breadcrumb-upstream-raw.txt`（[G1]-[G7]），
跨控件的那五条写进 `docs/astra/adaptation/00-jalium-theme-capabilities.md` S1-n。

## 2. 基型选择

上游 `BreadcrumbBar : Control` + `ItemsRepeater`；本层 `FluentBreadcrumbBar : ItemsControl`。
理由与 PipsPager 相反：上游这一条**确实**公开 `ItemsSource` 与 `ItemTemplate`（`BreadcrumbBar.idl`），
而 26.10.9 有 `ItemsControl` 及其四个 protected 容器覆写（[G1]/[G2] 量到），所以条目管线是能用的原生基面，
不需要自造集合。`BreadcrumbBarItem : ContentControl` 一对一照抄为 `FluentBreadcrumbBarItem : ContentControl`。

面板是 `FluentBreadcrumbPanel`（`ItemsPanelTemplate` 在本运行时只带 `PanelType`，横向 `StackPanel` 无从设起）。

## 3. 资源键清单

29 行，本层发布 10、扣住 19。发布判据不是"上游有没有这个名字"，而是本层那条闸口：
`AstraResourceKeyTests.Transcribed_control_rows_are_read_by_a_template` 要求每一条已发布的控制行都被某个模板读到，
所以"发布了但没人读"在这套闸口里是红的。扣住的 19 行分两类，两类的性质不同：

发布（名字逐字照抄上游，值也照抄）：`BreadcrumbBarChevronPadding`(2,0)、
`BreadcrumbBar{Normal,Hover,Pressed,Disabled,Focus}ForegroundBrush`、`BreadcrumbBarCurrentNormalForegroundBrush`、
`BreadcrumbBarForegroundBrush`、`BreadcrumbBarBackgroundBrush`、`BreadcrumbBarBorderBrush`。

**A 类：本管线读不出来**（6 行）——

| 名字 | 上游写法 | 为什么 |
| --- | --- | --- |
| `BreadcrumbBarChevronLeftToRight` / `…RightToLeft` | `x:String` ``/`` | `x:String` 一行会报废整本字典；两枚码点写成模板字面量 |
| `BreadcrumbBarChevronFontSize` | `x:Double` 12 | 同上，写成字面量 12 |
| `BreadcrumbBarItemFontWeight` | `x:FontWeight` Normal | 本层 `keys.md` 类型普查里没有 `FontWeight` 这一类 |
| `BreadcrumbBarItemThemeFontSize` | `StaticResource → ControlContentThemeFontSize` | 别名目标本层根本没有（字号 token 一条都没发布） |
| `BreadcrumbBarEllipsisFlyoutPresenterBackground` | `StaticResource → AcrylicBackgroundFillColorDefaultBrush` | 别名目标不在 Astra 调色板里 |

**B 类：本层没有能读到它的模板**（13 行）——`BreadcrumbBarCurrent{Hover,Pressed,Disabled,Focus}ForegroundBrush` 四行：
上游那四个状态挂在最后一枚条目的按钮上，而该按钮被 `LastItem` 收起，状态永远到不了（上游死码）；
`BreadcrumbBarEllipsisDropDownItem*` 七行 + `BreadcrumbBarEllipsisFlyoutPresenter{BorderBrush,BorderThemeThickness}` 两行：
省略号列表由 `MenuFlyout` + `MenuFlyoutItem` 呈现，表面/内衬/悬停全部走菜单族已翻译的键，
从条目的 token 反向覆写只会把菜单族自己的状态格子压成本地值（S1-i 那类"本地值压过我们的格子"）。
下拉表面因此不是上游那层 `AcrylicBackgroundFillColorDefault`，而是 `MenuFlyoutPresenter` 的既有表面——这条记在 §6。

三条跨段事实要记下：
1. `BreadcrumbBarEllipsisDropDownItemBackground` 在上游本身就是**死键**（没有任何状态读它，只有它后面三枚 PointerOver/Pressed/Disabled 被读）。
2. `BreadcrumbBar{Background,Border}Brush` 两段之间**换了值类型**（Default/Light 是 `SolidColorBrush Transparent`，
   HighContrast 是 `SystemControlTransparentBrush` 别名）。本层按 Default 写成字面透明刷子：三套主题读到的都是透明，
   这是该行唯一要回答的事。
3. 唯一会跨段变的数字是 `…EllipsisFlyoutPresenterBorderThemeThickness` 1→2（HighContrast），而它整行属于 B 类，
   因此高对比下省略号列表的描边宽度这条没有任何实现——见 §6。

## 4. 状态映射（WinUI VisualState → 本层 Triggers）

条目模板三组状态（`BreadcrumbBar.xaml:22-110`）：

| 上游状态组/状态 | 上游写的属性 | 本层落点 | 状态 |
| --- | --- | --- | --- |
| `ItemTypeStates/Inline` | （空） | 模板静置态 | 已覆盖 |
| `ItemTypeStates/EllipsisDropDown` | 9 条（按钮/箭头收起、下拉 presenter 展开、内衬 11,7,11,9、外边距 5,3、焦点边距 -3、两处 `IsTemplateFocusTarget`） | 省略号列表改由 `MenuFlyout` + `MenuFlyoutItem` 承载（`Flyout` 类型在本运行时不存在，[G1]），条目视觉与内衬来自菜单族已发布的样式 | **偏离**，见 §6 |
| `InlineItemTypeStates/Default`、`/Ellipsis` | `PART_ChevronTextBlock.Text = `；省略号态另有两笔 `Visibility` | 箭头是模板里的静态字面量；省略号不在条目里（见 §5 第 3 条），其 `E712` 写在条样式模板的 `PART_EllipsisButton` 内 | 部分覆盖 |
| `InlineItemTypeStates/DefaultRTL`、`/EllipsisRTL` | 换 `` | 无 | **未实现**，见 §6 |
| `InlineItemTypeStates/LastItem` | 按钮与箭头 `Collapsed`、`PART_LastItemContentPresenter` `Visible`、两处 `IsTemplateFocusTarget` | `Trigger Property="IsLastItem" Value="True"` 写前三笔；最后一枚的前景色改写在条目自身（`ContentPresenter` 在本运行时不声明 `Foreground`，写在它上面是 S1-f 那类死格） | 已覆盖（焦点目标两笔无对应属性） |
| `EllipsisDropDownItemCommonStates/{Normal,PointerOver,Pressed,Disabled}` | 背景 + 前景（`Normal` 只有一笔 `PointerUpThemeAnimation`） | 由菜单项样式的同名状态承担；`PointerUpThemeAnimation` 本层无对应 | 部分覆盖 |
| 内层按钮 `CommonStates` 10 态（`BreadcrumbBar.xaml:186-242`） | 全部只写 `PART_ContentPresenter` 的 `Foreground`/`Background`/`BorderBrush` | `IsMouseOver`/`IsPressed`/`IsEnabled` 三态触发器，前景写在按钮自身让它继承到文字 | 已覆盖；`Current*` 四态**不实现**（最后一枚的按钮被收起，状态到不了——上游死码），`Focus`/`CurrentFocus` 无按键路径可触发（任务 #13） |

## 5. 布局算法对照

逐步照抄 `BreadcrumbLayout`：求和判溢出（:62-69，**只加真实条目**）、从右往左找首个放得下的条目
（:101-118，种子＝最后一枚 + 省略号，下界＝省略号 + 最后一枚）、可见集合必为后缀、摆位从左到右累加自身宽度
且**无任何间距**（:74-81，整份上游文件没有一条间距 token）、行高取渲染条目的最大高并在显示省略号时把它的也纳入
（:120-135）、渲染后重排自动化集合（:188-192）。

三处必要偏离：
1. 隐藏机制由零矩形改为 `Visibility=Hidden`：零矩形在本运行时不藏住自带尺寸的子元素（[G4]），
   而 `Collapsed` 会让容器整个离开 `Children`、把下一趟判据掏空（[G7a]）。墨量结论与 `Collapsed` 一致（像素测试两向都测）。
2. 溢出判据用宿主条宽而非本列宽（[G7b]）。
3. 省略号不是"合成出的第 0 号条目"（上游 `BreadcrumbIterator` 尺寸 = Count+1），而是条模板里的 `PART_EllipsisButton`。
   可观察差异只有两处：它不进 `Items`，以及它的悬停高亮范围含自己那枚箭头（上游箭头在按钮外）。

## 6. Known Gaps（不声称清单）

- **硬件输入证据为零**：hover/press 两支触发器、方向键焦点环、真指针点击省略号条目，本段没有任何一条测到。
  条目与省略号的激活走 `ButtonAutomationPeer.Invoke()`（任务 #13）。
- 省略号下拉条目的**关闭后再发事件**这一顺序没有测到：`MenuFlyoutItem.Click` 与弹层关闭的先后由运行时决定，
  本层没有可读的 `IsOpen` 落点（`FlyoutBase.IsOpen` 只读，S2 的 `FluentDropDownButton` 已记过同一形状）。
- 下拉条目文本取 `item.ToString()`，不套应用的 `ItemTemplate`（上游用同一条目模板的 `EllipsisDropDown` 态）。
- RTL 箭头与 `EllipsisRTL` 未实现；本层没有 RTL 测试设施。
- 高对比：省略号列表那三行整体属于 B 类（见 §3），因此上游把描边宽度从 1 抬到 2 这件事在本层没有任何实现；
  `BreadcrumbBar{Hover,Pressed,Focus}ForegroundBrush` 上游高对比指向 `SystemColorHighlightColorBrush`，
  本层经调色板映射读到 `SystemColorWindowTextColor`；`…DropDownItem*` 上游指向 `SystemControlTransparentBrush`，
  本层读到 `SubtleFillColor*` 的高对比值。逐键断言是任务 #12 的欠账。
- `AutomationProperties.LandmarkType`（条上的 Navigation 地标）与 `AccessibilityView`（把省略号从 UIA 树里摘出去）
  在本运行时无对应属性，两条均未实现；`IsTemplateFocusTarget` 同理。
- 上游两处自认的焦点缺陷（`BreadcrumbBar.cpp:528-538` 的 `TryMoveFocus` 逃生口、条目间焦点跳动）无对应对象：
  `FocusManager.TryMoveFocus`/`FindFirstFocusableElement` 不存在（[G2]），焦点离开条的方式由运行时自己的遍历决定。
- 本层新增的上游没有的公开成员：`ContainerFromIndex`、`IsEllipsisRendered`、`EllipsisFlyout`、`IsLastItem`。
  前三条是 `ContainerFromItem` 缺失下的反查与观察口，`IsLastItem` 是上游由内部 `m_isLastItem` 承担的状态。
- 字体三连（family/size/weight）没有 token 可用（本层一条字号 token 都没发布），条目文字因此继承运行时默认字体设置。

## 7. 四类证据

1. **构建**：`dotnet build FluentJalium.slnx -c Debug` → 0 警告 / 0 错误（真重编）；`Themes/Manifest.txt` 53→55
   （新增 `ThemeResources/BreadcrumbBar.jalxaml` 与 `Styles/BreadcrumbBar.jalxaml`，两本都被清单双向校验）。
2. **行为**：`AstraBreadcrumbBarTests` 38 条全绿——容器数与三部件、只标最后一枚并把它的按钮/箭头收起、
   无溢出时逐枚紧挨（`x[i+1] = x[i] + width[i]`，零间距）、溢出时藏的是前缀且省略号与最后一枚必留、
   藏者不越过省略号右缘、`ItemClicked` 报条目集合里的序号与条目本体、省略号列表自深至浅且不含最后一枚、
   `PositionInSet/SizeOfSet` 只数可见条目、10 条发布行解析得到、19 条扣住的名字取不到。
   **A/B 有牙**：把隐藏机制换回上游的零矩形（`Arrange(0,0,0,0)`）后 3 条转红——像素、后缀、自动化重排；
   先断言改动落地（替换计数）再看结果，随后还原并重建通过。
3. **视觉**：`The_dropped_crumb_paints_nothing_in_the_capture` 白底卡片 + 三枚 100x24 不透明哨兵，
   按容器**实际状态**双向断言（藏者 0 像素、留者满格 ≥ 2000 px），并要求确有条目被藏，免得"什么都没画"冒充"藏住了"；
   `Dark_and_light_hand_the_row_different_foregrounds_and_never_the_brand_green` 断 Light/Dark 前景不同且
   品牌绿 `#207245` 为 0 像素。箭头字形 `` 只断言刷子与内衬（字体路径的墨在 RTB 捕获里不产像素，S1-m 第 1 条），
   不断言像素。Gallery 冒烟：新 markup 入树后进程 12 s 未退出（截图 `spike/VisualQA/out/breadcrumb-gallery.png`），
   但页面没滚到导航页，因此**本段没有任何一条上屏帧主张**。
4. **硬件输入**：**零条**。悬停/按下/聚焦三支触发器、真指针点条目与点省略号条目、方向键遍历，全部只到"markup 与
   handler 存在"这一步（任务 #13）。
