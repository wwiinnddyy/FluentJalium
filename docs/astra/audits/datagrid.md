# DataGrid 与 TreeDataGrid · 审计与决策（阶段 5 第五段）

运行时权威：NuGet Jalium.UI **26.10.9**。本批所有运行时读数出自
`spike/DataGridProbe`（`--mode all` / `pass2` / `pass3`），原文副本在
`docs/astra/adaptation/s1i-datagrid-host-raw.txt`、`s1i-datagrid-tokens-raw.txt`、
`s1i-datagrid-ownership-raw.txt`，结论写进 `adaptation/00` S1-i。
引用仓库全部只读，未执行任何 git 写操作。

## 0 · 这一步先推翻了两条既有结论

1. **"能不能重模板看类型链"作废**（S1-h 第 1 条）。`DataGrid : MultiSelector : Selector : ItemsControl :
   Control` 与 `TabControl` 同在 `Selector` 支系、链上没有 `ContentControl`，但三条通路
   （隐式应用级样式 / 显式 `Style`+`Template` / `Template` 本地值）**全部**把我们的模板落到它身上：
   每次都能在挂载树里读到 `ProbeTemplateRoot(Border)` 与 `ProbeItems(ItemsPresenter)`，同一轮里
   `ListBox` 挂同一支模板作为阳性对照也成功。真正的判别量改成：
   **`TryFindResource(typeof(X))` 取回的那份框架样式里有没有 `Template` 格子**。
2. **"框架 163 个控件零默认样式、未覆盖的控件裸奔"作废**（`adaptation/00` 开头那条、`adaptation/05` 的
   预测列）。框架确实自带每个控件一份主题字典——sibling 树 `git describe` = **v26.10.9**，与 NuGet
   同版本，`src/managed/Jalium.UI.Controls/Themes/Controls/` 下 31 本，其中
   `DataGrid.jalxaml`（195 行）与 `TreeDataGrid.jalxaml`（111 行）就是表格族那份。它的 11 个格子
   （`Background`/`Foreground`/`BorderBrush`/`BorderThickness`/`RowHeight`/`ColumnHeaderHeight`/
   `HorizontalGridLinesBrush`/`VerticalGridLinesBrush`/`AlternatingRowBackground`/`RowBackground`/
   `Template`）在挂载后**全部落地**，而 `grid.Style` 仍是 `null`、八个相关 DP 的 `DefaultValue`
   全是 `null`。当年那个"0"是在未挂载实例上读公开 `Style` 属性得到的。
   **Astra 是在覆盖框架自带的一套近似 Fluent 的主题**，不是"我们就是主题"。

## 1 · 上游没有 WinUI DataGrid：本批"照抄上游"没有对象

在 `../microsoft-ui-xaml`（commit `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`）：

- `git ls-files | grep -ic datagrid` = **0**；`git ls-tree -r HEAD | grep -ic datagrid` = 0；
  `git branch -a --list "*datagrid*"` 空；全 ref 历史按 datagrid 找新增只命中 TableView 的提交。
- `controls/dev/` 无 DataGrid 目录。33 处 "DataGrid" 文本命中全是无关串：
  `packaging/Intellisense/generated/Microsoft.WinUI.xml:36482` 的 `AutomationControlType.DataGrid` 枚举、
  UIA 窗口类名、TableView 设计稿提及。
- `TreeDataGrid` 在 microsoft-ui-xaml / ModernWpf / UI.WPF.Modern / wpfui / uno / FluentAvalonia
  六棵树里 **0 命中**——它属于 CommunityToolkit.WinUI，本机没有那棵树。

上游真正的表格是 **TableView**（`controls/dev/TableView/`，dll-tabular，MUX_PREVIEW）：

| 文件 | 形状 |
|---|---|
| `TableView_themeresources.xaml`（70 行） | Light 10–35 / Default 36–51 / HighContrast 52–68，各 **12 条、纯度量无 brush**（度量与画笔分家的原因写在该文件 11–19 行注释：合并会 duplicate-key 编译失败） |
| `controls/dev/CommonStyles/TabularSurfaces_themeresources.xaml`（305 行） | 189 键，`TabularSurface*` 前缀，Default 44–132 / Light 133–221 / HC 222–304 |
| `TableView.xaml`（393 行） | 15 个 `PART_` 部件；行 `CommonStates` **8 格**（Normal / PointerOver / Pressed / Disabled / Selected / SelectedPointerOver / SelectedPressed / SelectedDisabled 同组，405 行注释说明共享一组是为了不依赖 GoToState 顺序）；组头另有 `ExpansionStates`、`ExpandabilityStates` |

**但 TableView 的形状对不上本运行时的 DataGrid**：运行时是 WPF 语义（列集合、`DataGridColumn` 21 个 DP、
`DataGridCell : ContentControl`、`DataGridRow` 14 个 DP、`MultiSelector` 多选、编辑与 `IEditableObject`
通路），TableView 只有 group 层级、`SelectionMode` 只到 Single、无行冻结、无列重排成员。
`adaptation/00` S0-v 早就量过运行时"路线图上标自有的控件有三分之一本来就有"，这一批是又一例。

**所以本批的样式与键权威是 `../ModernWpf/ModernWpf/Styles/DataGrid.xaml`（831 行、53 键）——
它自己抄的是 dotnet/wpf 官方 Fluent，不是 WinUI**（`ModernWpf/docs/datagrid-wpf-fluent-source-audit.md`
82 行是这份差异的逐字说明）。主题键在 `ModernWpf/ThemeResources/Light.xaml` **994–1053**
（DataGrid 块约 62 键；1055–1067 是混进来的 DatePicker/DateTimeFlyout 键，**不算 DataGrid 的**）、
`Dark.xaml` 991 起、`HighContrast.xaml` 974 起；Light↔Dark 实际只差 5 处
（`ListAccentLowOpacity` 0.4/0.6、`ListAccentMediumOpacity` 0.6/0.8、`InvalidBrush`、
`ScrollBarsSeparatorBackground`、`SystemControlGridLinesBaseMediumLowBrush`）
加 `DataGridRowSelectedBackgroundThemeBrush`（Light 用 `SystemAccentColorDark1Brush`、Dark 用 `Light3`）。
ModernWpf 未新增语义键，全部是补官方模板要求的别名（该文档第 59 行）。

## 2 · 框架那份表格主题：部件契约与几何基线（全部实测）

部件名是契约。**只给 `Border + ItemsPresenter` 的模板**换上去之后：行仍然生成（`DataGridRow` 各带框架
`PART_RowBorder`/`PART_CellsPanel`），但 `DataGridCell=0`、列头整排消失、`PART_OuterBorder` 不存在、
行宽从 466 涨到 480（`PART_RowHeaderCorner` 的 20 DIP 槽没了）。宿主模板要背的部件：
`PART_OuterBorder`(Border) → `PART_ColumnHeadersBorder`(Grid) → `PART_ColumnHeadersScrollViewer` →
`PART_ColumnHeadersHost`(StackPanel)、`PART_RowHeaderCorner`(Grid)、`PART_DataScrollViewer` →
`PART_RowsHost`(StackPanel)、`PART_DragOverlay`(Canvas，`Grid.RowSpan=2`、`IsHitTestVisible=False`、
初始 `Collapsed`)。条目层各有可独立覆盖的框架样式：`DataGridRow`、`DataGridCell`、
`DataGridColumnHeader`、`DataGridRowHeader`、`DataGridDetailsPresenter`、
`DataGridCellsPresenter`/`DataGridRowsPresenter`/`DataGridColumnHeadersPresenter`。

框架基线几何（Light 下未加 Astra 时实测；Dark 值见 raw）：

| 位置 | 实测 |
|---|---|
| `PART_OuterBorder` | `CornerRadius=12,12,12,12`、`ClipToBounds=True`、`BorderThickness=1,1,1,1`、`bg=#FF2C2C2E`、`bc=#FF48484A` |
| 行 / 列头带 | 30 / 34 |
| 单元格 padding / 列头 padding | `10,5,10,5` / `10,7,6,7` |
| `PART_ResizeGrip` | 8x34，**`IsHitTestVisible=False`**（可见分隔来自 `PART_HeaderBorder` 的 `bt=0,0,1,0`） |
| `PART_SortIndicator` | 空串（由代码填），`FontSize={ThemeResource CaptionFontSize}`=`10` |
| 框架 `GridSplitter` | 类型在（`GridSplitter : Thumb`，DP：`DragIncrement`/`KeyboardIncrement`/`PreviewStyle`/`ResizeBehavior`/`ResizeDirection`/`ShowsPreview`），但表格模板里不用它 |

框架那份样式读 **11 个 token 名**，Astra 目前只定义其中 2 个（`LayerFillColorAltBrush`、
`SubtleFillColorSecondaryBrush`）：`SurfaceBackground`、`TextPrimary`、`TextSecondary`、`TextDisabled`、
`ControlBorder`、`ControlBorderFocused`、`AccentBrush`、`TextOnAccent`、`CaptionFontSize` 九条没有。

**撞名确实能换色**（A2 别名层第一次拿到实测边界）：`FluentThemeManager.Apply` 前后各读一次同一支网格，
`LayerFillColorAltBrush` 从框架 `#FF3A3A3C` 翻成我们的 `#0DFFFFFF`、`SubtleFillColorSecondaryBrush` 翻成
`#0FFFFFFF`，两处都是"部件携带的笔刷 == 合并查找取回的实例"；`SurfaceBackground`/`TextPrimary`/
`ControlBorder`/`TextDisabled` 同理指向框架实例。`IsEnabled=False` 那条 `TextDisabled` + `Opacity=0.56`
也实测落地。

**成本模型**：我们的隐式样式是**叠在**框架那份上，不是顶掉。只写一个 `Template` 格子时，
`RowHeight=30`、`ColumnHeaderHeight=34`、`BorderThickness=1,1,1,1`、`GridLinesVisibility=All`、
`RowBackground=Transparent`、`AlternatingRowBackground=<框架实例>` 六条读数与不装我们样式时逐条相同。
好的一面是 `Styles/DataGrid.jalxaml` 不必重抄 11 条；坏的一面是**漏写也杀不掉任何东西**。
（未结：`Application.Resources` 上 `ContainsKey(typeof(DataGrid))=True`、索引器取回的对象与合并查找同一个，
但 `Keys.Count=0`——两个访问器说法不一致，框架那份到底住哪个作用域没量清。）

## 3 · 两条必须压掉的框架缺陷（不是"照抄上游"能解决的）

1. **选中行铺品牌绿。** `AccentBrush` 在这套运行时是一个 `LinearGradientBrush`（S0-w 角批量到的
   `#1D733C..#2B804A`），框架行模板的 `IsSelected` 格子把它写进 `PART_RowBorder.Background`，
   实测选中行确实拿到该渐变。
2. **焦点边框是品牌绿。** `ControlBorderFocused = #FF1E793F`，挂在 `PART_OuterBorder.BorderBrush` 的
   `IsKeyboardFocused=True` 格子上。
   修法只有一条符合纪律：**用我们自己的模板与行/列头/单元格样式压掉**，不在应用级字典里重定义
   `AccentBrush`/`ControlBorderFocused`——那是框架 token 层，别的框架控件也读它，重定义等于给全仓改语义
   （S0-n/NumberBox 批那条"应用级名字撞车、最后写入者赢"的账）。
   ［本节第 1 条的结论已在 §5 推翻，保留原文以记账：行模板装不上，所以选中行的底只能从 `AccentBrush`
   这个名字改；焦点边框那条仍然成立——宿主模板是我们的，格子根本没写进去。］
3. **选中行的反色文字到不了单元格。** 行样式触发把 `row.Foreground` 写成 `TextOnAccent`
   （实测 `#FFFFFFFF`），但 `DataGridCell` 自己的样式格子写着 `Foreground = TextPrimary`，
   实测 `cell.Foreground=#FFF5F5F7` 不变——渐变上盖常态字色。这条要在我们的条目样式里定夺
   （WPF Fluent 的做法是选中行不换字色、只换半透明强调底，两者都要比一遍再写）。

`AccentBrush` 那一行在 raw §G 里 `sameInstance=False` **不算反证**：那处比的是未选中行的 `Transparent`；
真凭据是 §J 选中之后的读数。凡是实例同一性的断言都要在状态真的那一侧读。

## 4 · Known Gaps（本批不声称）

1. 指针/键盘/触摸硬件输入路径零证据：列宽拖拽（`PART_ResizeGrip` 框架自己 `IsHitTestVisible=False`）、
   列重排（`DataGridColumn.CanUserReorder`/`DragIndicatorStyle`）、排序点击、单元格编辑提交
   全靠 API 与树读数，没有一次真输入。
2. `IsMouseOver` 类格子（行 hover、列头 hover）没读到：本运行时不能合成指针位置，也没有公开的写入口子。
3. 框架样式住在应用字典还是独立主题作用域未结（§2 末）。
4. TableView 那一套（`TabularSurface*` 189 键、8 格行状态组）与 WPF 语义的 `DataGrid` 对不上，
   本批**不**按 TableView 抄键名；如果有人以后主张"WinUI 3 的表格长这样"，必须先解释运行时形状差异。
5. `TreeDataGrid` 只量到外壳与行（`TreeDataGrid : Control`，模板部件与 DataGrid 同名 + `PART_DragOverlay`，
   行内 `TreeDataGridRow` 直接生成、无 Expander/ToggleButton）。它需要自己的 `ItemsSource`/节点形状，
   运行时 `TreeDataGridNode` 是 `internal`——层级数据怎么喂进去尚未量，属下一段。
   **【下一段已结，且这条推论被推翻】** 节点类型是 internal 不假，但喂法根本不点名它：`ItemsSource` 收普通
   `IEnumerable`、层级由 `ChildrenPropertyPath`（字符串）反射，`ExpandAll/CollapseAll/IsExpanded(int)/FlattenedCount`
   全公开。"数据喂不进去"是从一个真读数多跨了一级台阶。见 `audits/treedatagrid.md` §1。
6. 高对比（HC）下表格的行为未测：框架那份字典只有 Light/Dark 两套，HC 走哪条路未知。
7. 像素级证据尚未产出（本批只有树与实例读数）。像素批必须包含"选中行不出现品牌绿主导像素"这条硬闸口，
   并说明裁剪区含框架自己那套条目（S0-j/S0-w 的教训）。
8. `PART_SortIndicator` 由代码填文本，我们换模板之后排序指示是否还在，未测。

## 5 · 落地形状：一条实测出来的"行不能重模板"契约（2026-09-20 收尾）

第一批样式装完之后，**表格里一个字的像素都没有**：单元格的 `ContentPresenter` 报告
`content=TextBlock`、`vis=Visible`，但 `desired=0,0`，而且那棵子树里根本找不到 `TextBlock`；
框架自己的表格（不加载 Astra 字典，`--mode content`）在同一棵树上量到 `47.45x19.78 / 43.45x19.78`。
四份日志：`adaptation/s1i-datagrid-cells-raw.txt`（Astra）、`s1i-datagrid-content-raw.txt`（框架基线）、
`bisect-Styles-DataGrid.jalxaml`（被拆散的原始样式）。二分按"整块撤下→单块装回"走，每一格都是一次构建一次读数：

| 装上的样式 | 单元格文字 |
| --- | --- |
| 全装（宿主+行+单元格+两种表头） | `desired=0,0`，树里无 TextBlock |
| 撤宿主+单元格（只留行/列头/行头） | 仍然 0 |
| 只留行样式 | 仍然 0 |
| 四块全撤、只留 token 字典 | `43.45x19.78`，正常 |
| 只留单元格样式（带我们自己的模板） | 正常，单元格 120x32 |
| **宿主+单元格+列头+行头，不留行样式** | **正常，全部事实绿** |

结论一句话：**能给单元格换模板，不能给行换模板。** 行是控制自己往 `PART_CellsPanel` 里塞单元格的容器，
换掉行的模板等于把那批内容 visual 所在的子树重建一遍，而 26.10.9 的 `ContentPresenter` 没有
"模板拆除时把托管的 visual 释放回去"这一步——参考树里后来长出了这个方法
（`Jalium.UI.Controls/ContentPresenter.cs` 的 `ReleaseContentElementForTemplateTeardown`，注释逐字写着
"旧的 presenter 继续持有该 visual 时，替换上来的 presenter 会渲染同一个实例而它的 `VisualParent` 还指向退役的树，
输入与布局失效就停在那个断根上"）。运行时版本没有它，于是同一根 `TextBlock` 卡在退役子树里，新 presenter
拿着 Content 却测不出尺寸。这条推论的证据边界要说清：**没读到运行时的私有字段，只量到行为一致**
（撤样式即恢复、装样式即失效、且只在行这一层）。
两个直接后果写进了产品代码：
1. `Styles/DataGrid.jalxaml` 只发四块样式，行那一块连隐式样式都不注册；
   `AstraDataGridTests.The_row_keeps_the_frameworks_template` 钉住"名字查不到、部件里也没有我们独有的
   `RowSelectionBackground`/`PART_DetailsPresenter`"。
2. 选中行的底色归框架行模板所有，它读 `AccentBrush`——于是第 3 节"绝不重定义框架 token"那条被推翻了：
   新增 `ThemeResources/FrameworkRetints.jalxaml`，把 `AccentBrush` 别名到 `AccentFillColorDefaultBrush`
   （同一实例，所以 `ApplyAccent`/`OverrideBrush` 照样能推）。作用域实测：框架字典里 58 处读这个名字
   （RangeControls 26、Dialogs 8、PropertyGrid 6、Calendar 5、ToggleControls 4、Containers 2、Navigation 2、
   TreeView 2、DataGrid/TreeDataGrid/DockLayout/MenusToolbars 各 1），今天全部铺品牌绿，改完之后全部跟随应用强调色；
   我们重模板过的控件不读它。代价：`ListAccentLowOpacity` 0.4 那层半透明没了（x:Double 发不出去，
   行模板又装不上），选中行按强调色不透明铺满。两条都在测试里点名：
   `An_alias_row_resolves_to_its_target`（`AccentBrush`↔`AccentFillColorDefaultBrush` 实例同一）、
   `The_selected_row_fills_through_the_frameworks_own_name_...`（读在 `PART_RowBorder` 自己身上、断言不是渐变）、
   `Overriding_the_accent_moves_the_selected_row_fill_through_both_names`（把强调色换成哨兵色，选中行像素 >200）、
   `A_selected_grid_never_paints_the_frameworks_emerald_gradient`（#1D733C/#2B804A/品牌绿各 0 像素）。

顺带把三条老账结清：第 4 节第 7 条（"像素证据未产出"）由
`The_grid_surface_paints_its_background_row`、`The_grid_repaints_between_the_two_themes` 与上面那条
无品牌绿闸口交付；那 30 个读不出来源的 `#1D733C` 随宿主/行模板的形状一起消失，不再挂账；
单元格模板里的 `ContentPresenter` 一律显式绑 `Content`/`ContentTemplate`/`ContentTemplateSelector`/
`ContentStringFormat`——这是 S1-h 那条"裸 presenter 在这个运行时只是内容宿主"的同一笔账，列头与行头也一并补上。
