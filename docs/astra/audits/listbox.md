# ListBox / ListBoxItem 审计（阶段 5 第一段）

上游：`microsoft-ui-xaml` commit `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`，
`controls/dev/CommonStyles/ListBox_themeresources.xaml`（blob `df9ca8dfe48b7cd4487b24b4f7954c7a74e683f8`，233 行）。
`SystemControl*` 那几格的目标值查 `dxaml/xcp/dxaml/themes/generic.xaml`（243、300-304 行）。
方法参考：`ModernWpf` commit `23555a6c00623b2f80e67f20d7f1df49a1d28ad8`，`ModernWpf/Styles/ListBox.xaml`
与 `Styles/ListBoxItem.xaml`。宿主与容器的实测契约在 `docs/astra/adaptation/00-jalium-theme-capabilities.md`
**S1-c**，原始输出在 `adaptation/s1c-list-host-raw.txt`（`spike/ListProbe`，模式 types/mount/retmpl/virtual/
select/gutter/itemstyle/theme/itemtmpl/attach）。

## 1. 上游键清单（26 个分支内行名 × 3 档 + 1 条分支外行 + 2 条样式）

分支：Default（4-31）、High Contrast（32-59）、Light（60-87）。三档**同一批 26 个名字**，其中 11 行是当前世代、
15 行是 Windows 8.1 世代的 `*ThemeBrush`。分支外只有 `ListBoxItemPadding`（89 行，三档同值）。

| 上游行名 | Default 档目标 | 本仓库 | 说明 |
| --- | --- | --- | --- |
| `ListBoxBorderThemeThickness` | 0（HC 2） | ✅ 0 | HC 那档由调色板的重映射承担，不做分支行 |
| `ListBoxForeground` | TextFillColorPrimary | ✅ 同名别名 | |
| `ListBoxBackground` | SystemControlBackgroundChromeMediumLow | ⚠️ 别名到 `SolidBackgroundFillColorBaseBrush` | §5.1 |
| `ListBoxBorder` | TextFillColorPrimary | ✅ | 0 厚描边，两档正常主题下画不出像素 |
| `ListBoxItemForeground` | TextFillColorPrimary | ✅ | |
| `ListBoxItemForegroundDisabled` | TextFillColorDisabled | ✅ | |
| `ListBoxItemBackgroundPointerOver` | SubtleFillColorSecondary | ✅ | |
| `ListBoxItemBackgroundPressed` | SubtleFillColorTertiary | ✅ | |
| `ListBoxItemBackgroundSelected` | SystemControlHighlightListAccentLow（强调色 @0.6） | ⚠️ 别名到强调色本体，0.6 由部件 Opacity 带 | §5.3 |
| `ListBoxItemBackgroundSelectedPointerOver` | …Medium（@0.8） | ⚠️ 同上，0.8 | §5.3 |
| `ListBoxItemBackgroundSelectedPressed` | …High（@0.9） | ⚠️ 同上，0.9 | §5.3 |
| 15 条 `ListBox*ThemeBrush` | 8.1 世代实色 | ❌ 不发布 | 上游自己也没人读：全仓 grep 只命中这三档声明与 perf2026 副本 |
| `ListBoxItemPadding`（分支外） | 12,9,12,12 | ✅ 原值 | 原生 resting 实测是 10,6,10,6 |

**行数账单：上游 27 个行名 → 发布 12 个、不发布 15 个**，不发布的那 15 个在
`AstraListBoxTests.A_dead_upstream_row_is_not_published` 里逐个点名。

## 2. 上游模板结构

`DefaultListBoxStyle`（195-233）：`Border 'LayoutRoot' > ScrollViewer 'ScrollViewer' > ItemsPresenter`，
样式带 11 条 `ScrollViewer.*` 附着 setter 与 `ItemsPanel = VirtualizingStackPanel`。
`DefaultListBoxItemStyle`（90-194）：`Grid 'LayoutRoot' > Rectangle 'PressedBackground' + ContentPresenter`，
`Grid.Resources` 里两套 ContentPresenter 字体样式，`CommonStates` 8 态。

本实现落点（`Styles/ListBoxes.jalxaml`）：两张模板都照这个形状，两处被迫改动——条目必须裹成单根（§5.4），
`ItemsPanel` 不写（§5.5）。

## 3. 上游状态映射（1 组 8 态）与本实现的落点

上游每个态只写两件事：`PressedBackground.Fill` 与 `ContentPresenter.Foreground`。

| 上游态 | 触发信号 | 本实现 |
| --- | --- | --- |
| Normal | —— | 基础 setter：`Fill=Transparent`、`Opacity=1`、前景继承 |
| Disabled | `IsEnabled=False` | ✅ 写 `ListBoxItemForegroundDisabled` + `Opacity=0` |
| PointerOver | `IsMouseOver`（上游另有 `IsHighlighted`） | ✅ `IsMouseOver` 单信号：本类型没有 `IsHighlighted`（mode I 三条都 not exported） |
| Pressed | 上游 `PointerPressed` 驱动 | ✅ 用 `IsMouseCaptureWithin`（本类型没有 `IsPressed`，与 ComboBoxItem 同一个替代） |
| Selected | `IsSelected` | ✅ `ListBoxItemBackgroundSelected` + `Opacity=0.6` |
| SelectedUnfocused | `IsSelected` + 控件失去焦点 | ⚠️ 与 Selected 合并（§5.6） |
| SelectedPointerOver | `IsSelected` + hover | ✅ MultiTrigger + `Opacity=0.8` |
| SelectedPressed | `IsSelected` + press | ✅ MultiTrigger + `Opacity=0.9` |

## 4. 公开面与命名

行名逐字照抄上游（含 `ListBoxItemPadding` 的拼法），样式键 `DefaultListBoxStyle` / `DefaultListBoxItemStyle`
加两条隐式 `<Style TargetType=… BasedOn=…/>`，与上游 90/195 行同形。**不做自有类型**：选择语义整个是框架的
（`HandleArrowKey`、`HandleSpaceKey`、`HandleDragSelect`、`SelectSingle/Multiple/Extended/Range/All`、
`ListBoxItemAutomationPeer : ISelectionItemProvider + IVirtualizedItemProvider + IScrollItemProvider`），
宿主能接我们的模板且不需解锁调用。

`ListBox` 样式**不写** `ItemContainerStyle`：mode G 实测显式赋值压过应用层的隐式类型键样式，写了就等于把
消费方的覆写权收走。这一条由 `The_list_style_leaves_item_container_style_to_the_application` 钉住。

## 5. 差异、宿主替换与 Known Gaps

1. **表面色换token（值差）**：上游 `SystemControlBackgroundChromeMediumLowBrush` 在
   `generic.xaml:243` 是 `SystemChromeMediumLowColor` = Light `#F2F2F2` / Dark `#2B2B2B`。我们调色板里没有
   带这两个值的 token（`Light.jalxaml`/`Dark.jalxaml` 由 `tools/Sync-AstraPalette.ps1` 生成，不能手工加行），
   最近的是 `SolidBackgroundFillColorBaseBrush`：`#F3F3F3` / `#202020`。Light 差 1/255，Dark 差 11。
2. **附着 setter 走不通（能力差）**：上游 11 条 `ScrollViewer.*` 样式 setter 在这里没有等价物。
   mode J 四格全读 `viewer.Auto/False`：带附着名的 setter 解析成 Style 但到不了部件，
   `{TemplateBinding ScrollViewer.X}` 也读不到（读回的是类型默认值），只有把属性**字面写在部件上**才生效。
   仓库既有的 11 个样式文件里 `Property="ScrollViewer.` 出现 **0 次**，与本测一致。后果：应用无法再通过
   `ScrollViewer.VerticalScrollBarVisibility` 改我们列表的滚动条——已写进 Catalog 的 gaps。
3. **选中透明度搬到部件（结构性照抄）**：`SystemControlHighlightListAccent{Low,Medium,High}Brush` 是
   强调色 @0.6/0.8/0.9（`generic.xaml:300-302`），我们强调色只有 1/0.9/0.8 三档且不能往生成层加行，
   所以行只带颜色、部件带 `Opacity`。旁证：原生条目选中实测 `#99680081`——正是"强调色 @0.6"落在 OS 默认
   强调色上，说明这个乘法就是被复制的那个形状。**未声称**：三层叠加（选中条叠在带 alpha 的 token 上）时的
   精确像素与上游是否逐位相同，本机没有对照面。
4. **单根限制**：上游条目模板并列 `Rectangle` + `ContentPresenter` 两个根，我们的一颗字典里两根本地抛
   `XamlParseException: ControlTemplate can only have one visual tree root element.`（整本字典报废），
   所以裹了一层 `Grid`。
5. **`ItemsPanel` 不写**：框架默认已是 `VirtualizingStackPanel`（mode D 实测树里就有），而
   `<Setter Property="ItemsPanel"><ItemsPanelTemplate>…` 这个形状在本读法下未经测量，不为它冒"静默丢掉"的风险。
6. **`SelectedUnfocused` 合并**：本类型没有 `IsSelectionActive`（声明在 `Selector` 上，条目拿不到），
   `ListBoxItem` 也不在焦点态里暴露任何东西。上游这两态的目标行本来就是同一个
   （`ListBoxItemBackgroundSelected`），所以合并**不丢颜色**，只丢"以后若上游分家"的可能性。
7. **`SelectedItems` 不跟 `SelectedIndex` 走**：写 `SelectedIndex=0` 之后 `SelectedItems` 仍读空
   （本文件测试第一版的实测），因此多条选择的断言全部改成读容器自己的 `IsSelected`；Gallery 的读数条也不列
   `SelectedItems`。这是框架语义，不是我们的模板造成的，但它是本批**没有能力**断言的一点。
8. **无焦点视觉**：上游条目靠 `UseSystemFocusVisuals` + 系统焦点框。原生模板树里没有任何焦点部件
   （mode B 的 `ListBoxItem > Grid > Border 'PART_BackgroundBorder' > ContentPresenter` 就是全部），
   我们也不自造一个（那是发明外观）。**未声称**：键盘聚焦时这个列表到底画不画框，本批没有可测的面。

## 6. 不声称清单

- 真指针 / 真触摸 / 真键盘：hover、press、drag-select、方向键与空格选择都**没有**硬件输入证据；
  状态是用 `SelectedIndex`、`IsSelected` 写、`ScrollToVerticalOffset`、`IScrollItemProvider.ScrollIntoView()`
  驱动并读回的。`An_item_peer_exposes_the_selection_and_scroll_providers` 只声称能力，因为实测
  `ISelectionItemProvider.Select()` 从容器自建的 peer 上调用不改 `SelectedIndex`。
- 条目像素只声称"选中把那一格从底色推向强调色"（两帧对比），不声称与 WinUI 截图逐位一致。
- 虚拟化只声称"1000 项仍只实现一屏容器"，不声称回收模式（`VirtualizationMode.Recycling`）下的行为。
- ListView / GridView / TreeView / DataGrid 由后续批次负责；本批只把 `ListBox : Selector` 这条链量穿。
