# ListView / ListViewItem 审计（阶段 5 第二段）

上游：`microsoft-ui-xaml` commit `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`，
`controls/dev/CommonStyles/ListViewItem_themeresources.xaml`（blob `b292e8fa9aa1f6024d16b8a5eb452e4427d45bda`，
698 行）。ThemeDictionaries 三档：Default 4-79、High Contrast 80-155、Light 156-231，每档同一批 76 个行名 +
2 个 flag；样式三条在分支外：`DefaultListViewItemStyle` 234-259、`ListViewItemExpanded` 261 起、
`ListPickerFlyoutPresenterItemStyle` 640 起。方法参考：`ModernWpf` commit `23555a6c00623b2f80e67f20d7f1df49a1d28ad8`，
`ModernWpf/Styles/ListViewItem.xaml`。宿主与容器的实测契约在 `docs/astra/adaptation/00-jalium-theme-capabilities.md`
**S1-d**，原始输出在 `adaptation/s1d-listview-host-raw.txt`（`spike/ListViewProbe`，模式
types/mount/retmpl/itemstyle/states/gutter/grid/cells/bar/panel）。

## 0. 这一批的前提被量掉了一半

目标里写的是"ListView/GridView 一起"。量完之后的结论是：**ListView 有，WinUI 的 GridView 在这个运行时没有**。

- `ListView : ListBox : Selector : ItemsControl : Control`，`ListViewItem : ListBoxItem : ContentControl`，
  而 `ListViewItem` 自己**一个成员都不声明**（mode A：own DPs 为空、DeclaredOnly 属性为空）。也就是说运行时的
  列表族是**继承**出来的，不是并列出来的。
- `Jalium.UI.Controls.GridView` 确实在，但它是 `GridView : ViewBase : DependencyObject`——WPF 那个"给 ListView
  装列"的 View，`is Control=False`、`is ItemsControl=False`、构造出来不能塞进 `Content`
  （mode G 实测 `InvalidCastException: Unable to cast ... GridView to FrameworkElement`），而 `ListView.View` 是
  它唯一的自有 DP。**`GridViewItem` 根本没有导出**。
- WinUI 的 GridView（卡片网格）在本运行时没有对应类型。可走的路只有一条、而且实测能走：把 `ListView` 的
  `ItemsPanel` 换成 `WrapPanel`（mode J：面板真的换过去了，6 条行在 300 DIP 宽里排成 4 + 2，
  第二行 y=32.78）。这条路径被 `AstraListViewTests.A_wrap_panel_items_panel_lays_the_rows_side_by_side` 钉住。
  **但它是"能排成网格"，不是"WinUI 的 GridView"**：没有 `ItemWidth`/`ItemHeight`、没有 `IsItemClickEnabled`、
  没有语义缩放、卡片自己的选中描边与多选勾选全都没有承载面。所以卡片网格仍要一层自有类型，本批不做、也不声称。

顺带量到：WinUI 的 ListView 侧那一族成员，本运行时**一个都没有**——`IsItemClickEnabled`、`ItemClick`、
`ShowSelectionChecks`、`MultiSelect`、`IsSwipeEnabled`、`ItemContainerTransitions`、`GroupStyleSource`、
`ContainerContentChanging`、`ChoosingItemContainer`、`RecyclingRatio`、`ShowsScrollingPlaceholders`、
`ItemWidth`/`ItemHeight`、`SemanticZoom`、`ItemsRepeater`、`ItemsView`、`ListViewItemPresenter` 全为 `-`
（mode A 逐条打点）。这不是"我们没抄"，是**宿主没有可挂的行为面**。

## 1. 上游键清单（Default 档 76 行 + 2 flag；本实现发 20 行）

**先看一件承重的事：本批没有 `ListView*` 可抄。** 整棵参考树里 grep 不到 `ListViewBackground`、
`ListViewBorderThemeThickness`、`x:Key="ListViewStyle"`，也没有 `controls/dev/ListView/` 目录——WinUI 的
ListView 宿主外壳在**闭源的 dxaml/generic.xaml**，开源树里只有 ITEM 那一层的行。因此宿主样式吃
`ThemeResources/ListBox.jalxaml` 已发的四行，而**不自造** `ListView*` 名字（`AstraListViewTests` 用两条
`Assert.Null` 钉住"没造名"，用一条像素断言钉住"两个宿主吃的是同一行"）。

Default 档 76 行按家族分（每档同一批名字，High Contrast 档只是把部分目标换成 `SystemColor*`）：

| 家族 | 行数 | 本实现 | 理由 |
| --- | --- | --- | --- |
| 表面/状态底色 `ListViewItemBackground*`、`ListViewItemBorderBackground` | 8 | ✅ 全发 | 模板里 `BorderBackground` 那一格逐态吃 |
| 文字 `ListViewItemForeground*` | 6 | ✅ 全发 | 上游六行**全部**别名到 `TextFillColorPrimaryBrush`，即选中不改字色——所以本控件的选中形是"底色 + 药丸" |
| 选中指示器 `ListViewItemSelectionIndicator*Brush` | 4 | ✅ 全发 | 前三条同一实例（强调色），disabled 走 `AccentFillColorDisabledBrush` |
| 半径 `ListViewItemCornerRadius` 4、`ListViewItemSelectionIndicatorCornerRadius` 1.5 | 2 | ✅ 全发 | CornerRadius 这一族本 reader 读得动 |
| x:Double / x:Boolean / 枚举度量（MinWidth 88、MinHeight 40、SelectedBorderThemeThickness 4、DisabledThemeOpacity 0.3、ContentOffsetX -40.5、Drag/Reorder 四行、两条 VisualEnabled flag、CheckMode） | 14 | ❌ 不发，**值以字面量进模板/样式** | 同一份 x:Double 限制（`adaptation/00` 早期条目）；`CompactSelectedBorderThemeThickness` 虽是 Thickness 但本运行时没有 compact 态可挂（与 ComboBoxItem 同一处置） |
| 焦点 `ListViewItemFocus*` | 4 | ❌ | 宿主没有焦点可视面（与 ListBox 批同一理由），发出去等于承诺"改它能改像素" |
| 勾选框 `ListViewItemCheck*`、`ListViewItemCheckBox*` | 16 | ❌ | mode A 读不到 `CheckMode`/`IsMultiSelectCheckBoxVisible`/`ShowsCheckHint`，mode B 在出厂模板树里读不到 `MultiSelectSquare` |
| 拖拽/重排/占位 `ListViewItemDrag*`、`MultiArrangeOverlay*`、`Placeholder*` | 5 | ❌ | 本运行时的列表没有拖放与占位面 |
| 8.1 世代 `*ThemeBrush` | 15 | ❌ | 上游自己也无人读（与 ListBox 批同一结论，名字逐条进反向断言） |

发 20 / 上游 78（76 行 + 2 flag），其余 58 条**逐名**有理由，`A_withheld_upstream_row_is_not_published` 拿 24 个名字点名。

## 2. 上游模板与本实现的形状

上游 `DefaultListViewItemStyle` 的模板只有一个部件：`ListViewItemPresenter x:Name="Root"`（256 行），把上面所有
行名当属性喂进去——**这个 presenter 是 C++ 的，参考树里没有它的实现**（mode A：`ListViewItemPresenter` 未导出）。
同文件里可读的 XAML 版是 `ListViewItemExpanded`（261 起）与同族的 XAML 版
`ListViewItem_themeresources_perf2026.xaml`（652 行；本批只按行号引用它，没有为它取 blob），它们的格是：

```
Grid 'ContentBorder'（半径、底色、Disabled 整行 Opacity 0.3）
├─ Rectangle 'BorderBackground'  Fill=ListViewItemBorderBackground  Opacity=0     ← 静息不画
├─ Grid 'ContentPresenterGrid' > ContentPresenter 'ContentPresenter' Margin={Padding}
└─ Border 'MultiSelectSquare' 20x20 Visibility=Collapsed（多选勾选面，本宿主无驱动）
```

CommonStates 六格（287-346）**只写两样**：`BorderBackground` 的 Fill+Opacity=1，和 `ContentPresenter.Foreground`。
本实现照此，另加一条 `SelectionIndicator`（WinUI 现世代选中行左边那根药丸），并在 `IsSelected` 那格把它抬起来。

## 3. WinUI 状态 → 本实现触发器（九步出口的映射表）

| 上游 VisualState | 本实现的格子 | 驱动信号 | 结论 |
| --- | --- | --- | --- |
| `Normal` | 无格（静息） | — | ✅ BorderBackground Opacity 0、药丸 Opacity 0 |
| `PointerOver` | `IsMouseOver=True` | 框架维护 | ✅ 底色 + 字色读回 |
| `Pressed` | `IsMouseCaptureWithin=True` | 框架维护 | ✅（`ListViewItem` 没有 `IsPressed`，mode E） |
| `Selected` | `IsSelected=True` | 容器 DP，写 `SelectedIndex` 会带到容器（mode H 四格全 True） | ✅ 底色 + 药丸 |
| `PointerOverSelected` | `MultiTrigger(IsSelected, IsMouseOver)` | 同上 | ✅ |
| `PressedSelected` | `MultiTrigger(IsSelected, IsMouseCaptureWithin)` | 同上 | ✅ |
| `Disabled`（Enabled 组） | `IsEnabled=False` | 行自身 | ⚠️ 行级 `IsEnabled=false` 会把整行压到 0.3；**宿主级 disable 走不到这格**（§5.4） |
| `SelectedDisabled`（presenter 的 `SelectedDisabledBackground`） | `MultiTrigger(IsSelected, IsEnabled=False)` | 两格都要成立 | ✅ 底色留在 `ListViewItemBackgroundSelectedDisabled`、药丸降到 disabled 刷；这一格排在 disable 格**之后**，因为本运行时后写的赢 |
| `SelectedUnfocused` | 并入 `Selected` | 无 `IsSelectionActive` | ❌ 不转录，与上游同值（上游那格也是同一底色） |
| `Selecting` / `MultiSelect*` | — | 无 CheckMode、无勾选面 | ❌ 不转录 |
| `DisabledStates` 的 `Enabled` | 静息 | — | ✅ |
| `ReorderHint*`、`Drag*`、`DataPlaceholder` | — | 无拖放/虚拟化占位面 | ❌ 不转录 |
| `Focus*`（`UseSystemFocusVisuals`） | — | 出厂树里没有焦点部件 | ❌ 不声称焦点框 |

## 4. 命名与公开面

- 发：`DefaultListViewStyle`、`DefaultListViewItemStyle`（keyed）+ 两条隐式 `TargetType` 样式；
  `ThemeResources/ListView.jalxaml` 的 20 行全是公开资源键。
- 不发明任何上游没有的名字；不自造 `FluentListView`（AGENTS.md：只有量出来的行为缺口才起自有类型，
  而选择语义是框架自己的：`Selector` 的 `HandleArrowKey`/`SelectSingle…` 族在 ListBox 批就量过了，
  `ListView` 继承同一条链）。
- `Themes/Manifest.txt` 两行，字典数 38 → **40**。

## 5. 与本实现的差异 / Known Gaps

1. **宿主吃 ListBox 行**（见 §0/§1）：好处是不自造名；代价是**一次 `ListBoxBackground` 覆写会同时动两个宿主**。
   这条在 Catalog 里明写。
2. **药丸的 16 DIP 长度是我们定的**。可读到的只有宽 4（`ListViewItemSelectedBorderThemeThickness`，行 11）
   与半径 1.5（行 60）；上游把长度藏在 C++ 的 `ListViewItemPresenter` 里，参考树里没有。这是自加值，
   不声称与上游逐位相同。
3. **`ListView.View`（WPF 列）在我们模板里没有承载面**：出厂模板里的 `Grid 'PART_ColumnHeadersBorder'` +
   `StackPanel 'PART_ColumnHeadersHost'` 在 `View=null` 时量得 0x0（mode J），我们直接不发这块（WinUI 没有 WPF
   式列头）。后果：谁给 `ListView` 设了 `View`，列头就没了。写进 Catalog。
4. **宿主 disable 走不到行的 disable 格**：mode 复测里 `list.IsEnabled=false` 之后，容器自己的 `IsEnabled` 确实变成
   false（`AstraListViewTests` 断住了这条传播），但那一格的 `Opacity` 仍读 1——`IsEnabled` 的继承变化不会重跑
   模板格子；把 `IsEnabled` 直接写在行上，0.3 才落下来。已按现状钉成断言（两种写法各一条），不猜成因。
5. **虚拟化列表的选中排他是控件的事**：在 `Single` 模式下**直接写第二行的 `IsSelected=true` 会留下两行亮着**
   （实测 2），排他只在控件自己的选择路径里生效。因此本批所有状态主张都走 `SelectedIndex`，并把这条写进断言
   而不是藏起来。
6. **12 DIP 右槽在列表面上复现并被拉平**（mode F）：出厂宿主左 1 右 13（5 条与 50 条一样），我们的模板关 overlay
   是 0/12、开 overlay 是 **0/0**。**并结清一批旧疑问**（mode I）：开 overlay 之后 `ScrollBar` 元素报 40x200，
   看起来像"条盖住了行右侧 40 DIP"，走到子树才发现带填充的是 `Border 'ThumbBorder' 2x40 #8BFFFFFF`——
   40 是命中区，画出来的是 2 DIP 细条，正是 WinUI 的形；同样的读数在**已发货的 ListBox 宿主**上一字不差，
   所以这是底座的性质，不是某个模板的性质。`AstraListViewTests` 与 `AstraListBoxTests` 各钉一条。
7. **卡片网格没有实现**（§0）：WinUI 的 `GridView`/`GridViewItem` 在本运行时无对应类型，WrapPanel 只是
   "能排成网格"的最低事实；`IsItemClickEnabled`、`ItemWidth`、语义缩放、多选勾选面都没有承载。
8. **没有分组**：本运行时导出 `GroupStyle`（mode A），但我们的列表没有写过分组面板，上游 `GroupStyle*` 那一族
   行与 `ListViewItem` 无涉，本批不碰。

## 6. 不声称清单

不声称悬停 / 按下 / 拖选 / 键鼠导航有任何真实输入证据（任务 13 仍为零）；不声称焦点框；不声称药丸长度与上游
一致；不声称宿主级 disable 会把行压暗；不声称 `GridView` 已交付；不声称 `ListView.View` 的列头可用；
不声称 `SelectedItems` 可用（ListBox 批量到的那条不继承到这里的断言，本批只按 `SelectedIndex` 与容器 `IsSelected`
说话）；不声称 ListView 沾了 GridView 的光，也不声称 GridView 沾了 ListView 的光。
