# TreeDataGrid 审计（阶段 5 第六段）

- 运行时权威：NuGet **Jalium.UI 26.10.9**（`src/FluentJalium/FluentJalium.csproj`）。
- WinUI 参照：`../microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229` —— 该仓库**没有** DataGrid 也没有
  TreeDataGrid（`git ls-files | grep -ic datagrid` = 0，见 `audits/datagrid.md` §1）。市面上那个 TreeDataGrid 属于
  CommunityToolkit，不是 WinUI。**所以本族没有 WinUI 上游可抄**，样式与度量的权威是 dotnet/wpf 官方 Fluent 主题
  （由 ModernWpf @ `23555a6c00623b2f80e67f20d7f1df49a1d28ad8` 转录），运行时形状权威是本机 26.10.9。
- 参考树 `C:\git\Jalium\Jalium.UI`（只读，可能比 NuGet 新）只用于定位"框架自己那份字典吃了哪些名字"，
  其中行号一律标 `[参考树]`，不当成 26.10.9 的断言。
- 原始读数：`docs/astra/adaptation/s1j-treedatagrid-feed-raw.txt`、`s1j-treedatagrid-columns-raw.txt`
  （`spike/DataGridProbe`，mode `feed` / `feedbare` / `cols`，均在已上屏窗口 + `CompositionTarget.Rendering` 帧后读）。

## 1 · 喂数据与展开：上一段那句话是错的，而且错在挡路的方向

`audits/datagrid.md` §4 第 5 条写的是"TreeDataGrid 需要自己的 `ItemsSource`/节点形状，运行时 `TreeDataGridNode`
是 internal——层级数据怎么喂进去尚未量"。这句话的推论是"消费者喂不进数据 ⇒ 控件不能样式化、不能进 Gallery、
不能测"，所以它把整个控件挡在库外。对着 26.10.9 的程序集量完，推论不成立：

| 成员（公开） | 26.10.9 实测 | 意义 |
| --- | --- | --- |
| `ItemsSource : IEnumerable` | 普通 POCO 列表即可 | 不需要节点类型 |
| `ChildrenPropertyPath : String` | 设成 `"Children"` 生效 | 层级由属性路径反射出来 |
| `ChildrenSelector : Func<object,IEnumerable>` / `HasChildrenSelector : Func<object,Boolean>` | 存在，且**不是**字符串属性（写字符串抛 `ArgumentException`） | 需要委托时用它们 |
| `TreeColumnIndex : Int32` | 树列是指定索引，不是某种列类型 | 没有 `DataGridTreeColumn` |
| `IndentSize : Double` | 默认 16 | 上游 TreeView 的缩进步长 |
| `ExpandAll()` / `CollapseAll()` / `IsExpanded(int)` / `FlattenedCount` | 全部公开 | **无指针输入也能驱动展开闭环** |
| `NodeExpanding` / `NodeExpanded` / `NodeCollapsed` | 公开事件 | |
| `TreeDataGridNode` | `internal`、sealed、公开构造函数 1 个（但类型不可见） | 消费者一辈子不用点名它 |
| `TreeDataGridRow : Control` | 公开声明属性只有 `IsSelected` 一条 | 行状态（层级/展开）全在控件侧 |

行为读数（同一进程、同一窗口）：2 条根 → `ExpandAll()` → 6 行且 `FlattenedCount=6`、`IsExpanded(0)=true`、
展开后落地 `TextBlock` 从 6 条涨到 14 条；`CollapseAll()` → 回到 2 行。

**规则**：判"控件能不能落地"之前，先把**公版程序集**的成员表拉出来。上一段那句 internal 是真的，
但从"节点类型 internal"推到"数据喂不进去"跨了一个没有证据的台阶。

## 2 · 部件契约与共享样式

`TreeDataGrid` 的宿主部件与 `DataGrid` 同名（少一个行头角落）：`PART_OuterBorder`(Border)、
`PART_ColumnHeadersBorder`(Grid)、`PART_ColumnHeadersScrollViewer`(ScrollViewer)、`PART_ColumnHeadersHost`(StackPanel)、
`PART_DataScrollViewer`(ScrollViewer)、`PART_RowsHost`(StackPanel)、`PART_DragOverlay`(Canvas)。
行内是 `PART_RowBorder`/`PART_CellsPanel`，单元格是 `PART_CellBorder` + 内容呈现器——**全部由控件在代码里生成**，
树形 chevron 是控件塞进树列单元格内容里的一条 `Path`（每行 1 条，实测 `path=1`、`toggle=0`、`expander=0`）。

因为运行时**没有 tree-only 的 cell / 列头 / 行头类型**，`Styles/DataGrid.jalxaml` 那三条隐式样式直接接管了这棵树
（测试 `The_tree_shares_the_cell_and_column_header_styles_of_the_plain_grid` 用模板实例同一性 + 32 DIP 下限证明）。
所以本批只新增一份宿主模板，且**不新造任何资源键**——表面、行、列头读的都是 DataGrid 族已有的行。

## 3 · 列宽的三条静默规则（本批最贵的一段）

七种配置一遍跑完（`s1j-treedatagrid-columns-raw.txt`）：

| 配置 | 列 `Width` 读回 | 列 `ActualWidth` | 单元格 `ActualWidth` |
| --- | --- | --- | --- |
| DataGrid，markup 写 `Width='200'` | **Auto** | 120 | 120 |
| TreeDataGrid，markup 写 `Width='200'` | **Auto** | **0** | **0** |
| DataGrid，markup 写 `MinWidth='200'` | Auto | 200 | 200 |
| TreeDataGrid，markup 写 `MinWidth='200'` | Auto | 200 | 200 |
| 两种网格，挂载**前**代码写 `Width=200` | 200 | 200 | 200 |
| 两种网格，挂载**后**代码写 `Width=200` | 200 | 200 | 120 / 0（不变） |

三条规则：

1. **`DataGridColumn.Width` 是 `DataGridLength`，markup 里的裸数字不转换、静默停在 `Auto`**——解析不报错、
   构建不报错、IDE 不报错。`MinWidth`/`MaxWidth` 是普通 `double`，写得进。和 `{x:Bind}` 静默丢弃同一族：
   能构建不等于写进去了。
2. **树网格把 Auto 列量成宽度 0**，普通网格把 Auto 摊成实宽。同一份 markup 在一个网格上没事、在另一个上整表
   文字宽度 0（看得见树、看不见字）。所以 `MinWidth` 是这个控件在 markup 里唯一的宽度杠杆。
3. **列宽必须在首次度量之前定好**：挂载后改 `Width`，列自己读到 200，单元格的本地宽度还是旧布局那一版。
   与"本地值优先于格子"是同一条账的不同层。

## 4 · 行层仍然不能重模板，而且行的填充是本地值

沿用 `audits/datagrid.md` §5 的逐层判据：叶子（cell / 列头 / 行头）可换模板，装单元格的容器行不可换。
`TreeDataGridRow` 的处境更硬——每条行的 `Background` 都是控件**写上去的本地值**（实测偶行 `#00FFFFFF`、
奇行 `#0FFFFFFF` 交替），所以即便换得上模板，样式格子也赢不了这一项。本批因此只出宿主样式，行保留框架模板；
`AstraTreeDataGridTests.Alternating_rows_come_through_on_the_tree_too` 读的是行上真正生效的那个实例。

## 5 · 框架自己那份树字典吃了哪些名字（方法照旧：数它读什么，不是数我们有没有）

`[参考树] src\managed\Jalium.UI.Controls\Themes\Controls\TreeDataGrid.jalxaml`（111 行，两份隐式样式，无 `x:Key`）
消费的 `{ThemeResource}` 名：`SurfaceBackground`、`TextPrimary`、`ControlBorder`、`SubtleFillColorSecondaryBrush`、
`LayerFillColorAltBrush`、`ControlBorderFocused`、`TextDisabled`、`AccentBrush`、`TextOnAccent`
（代码里另外取 `TextSecondary`，`[参考树]` DataGrid.cs:1023）。

其中两条正是上一批已经在管的账：`AccentBrush`（选中行铺品牌绿）经
`ThemeResources/FrameworkRetints.jalxaml` 之后已经是应用强调色；`ControlBorderFocused` 仍是框架的
品牌绿 `#FF1E793F`，本批的处理和 DataGrid 一样——**换掉宿主模板，让这条焦点线根本不在我们的树上**，
不在应用级字典里重定义框架 token。

## 6 · 与上游的差异（逐条，不粉饰）

1. 上游没有 WinUI 3 的树表可抄（§0），所以行/列头度量取的是 dotnet/wpf Fluent：行 32、列头 32、缩进 16。
2. 行保留框架模板 ⇒ 上游那张行状态矩阵（hover/selected/focus 的成套 VisualState）我们一格都没写。
3. 行头：上游 WPF Fluent 的行头带"当前行"记号；本运行时不把任何部件名暴露给行头，也没有当前行读数入口，
   所以我们的行头模板**只画表面不画内容**（画内容会把模型 `ToString()` 铺进 20 DIP 的槽里，见 §7 第 1 条）。
4. Auto 列在树上量成 0 宽（§3 规则 2），上游没有这个行为。

## 7 · Known Gaps（本批不声称）

1. **真指针输入零证据**：chevron 的命中区、hover、键盘 `Alt+Right/Left`、触摸展开全没测；展开闭环走的是
   `ExpandAll/CollapseAll/IsExpanded` 公开调用，只证明"激活通路在换模板之后还活着"。
2. 列拖拽/重排/缩放/单元格提交没有任何证据（`CanUserReorderColumns`/`CanUserResizeColumns`/`CanUserSortColumns`
   只读到属性存在）。
3. 行 hover / 选中 / 焦点的成套状态没做（§6.2）；`TreeDataGridRow` 只有 `IsSelected` 是公开的。
4. 虚拟化：`EnableRowVirtualization`/`EnableColumnVirtualization` 关掉之后列宽仍然是 0，所以 §3 的账不是
   虚拟化造成的，但虚拟化下的行回收没测。
5. 高对比未测：本控件框架字典只有 Light/Dark 两套。
6. 像素证据只覆盖"表面/交替/选中不含品牌绿 + 两主题重绘"；缩进到 DIP 精度、chevron 旋转角、
   列分隔线在树列里的行为都没量化。
7. **未知属性名在这条解析路径上静默丢弃**（直接量出来的，不是推断）：`<TreeDataGrid DefinitelyNotAProperty='42' …>`
   解析通过、控件建成、上屏 460x60，不报错也不留痕（`s1j-treedatagrid-feed-raw.txt` 的 `unknown attribute markup` 两行）。
   `TreeDataGrid` 也**没有** `AutoGenerateColumns` 属性（公开声明的 26 个名字里没有），所以 Gallery 的树 markup 里那句
   一直没起作用，本批把它删了。拼写错的属性名 ⇒ 静默无效，和 §3 规则 1 是同一类账；
   现存的 markup 闸口（`AstraGateTests`）查不到属性名是否存在，这条归到"markup 形状闸口"欠账。
