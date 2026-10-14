# TreeView / TreeViewItem 适配审计（阶段 5 第三段，2026-09-20）

上游：`microsoft-ui-xaml` @`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`（只读）。

- `controls/dev/TreeView/TreeView_themeresources.xaml` blob `8e7a107255310706dda5e4e5ca6edf99b6b14eb9`（127 行，
  三个 ThemeDictionaries：Default 4-44、Light 45-85、High Contrast 86-126，每个分支 39 行）
- `controls/dev/TreeView/TreeView.xaml` blob `9f419035a19cc68910b97cec4fd5ce264cdfefde`（31 行，
  `TreeViewItemDataTemplate` 3-7、`DefaultTreeViewStyle` 9-30）
- `controls/dev/TreeView/TreeViewItem.xaml` blob `e6655da4feacad75dd68395ec2ddb231e6ad5cfb`（154 行，
  `MUX_TreeViewItemStyle` 3-153：属性 4-11、VisualStateGroups 16-129、部件 130-147）

运行时权威：NuGet Jalium.UI **26.10.9**。方法参照 ModernWpf @`23555a6c0`（`ModernWpf/Styles/TreeViewItem.xaml`，
blob 未取——它是 WPF 形状的对标物而不是本批的转录源，本批转录源全部来自 WinUI 上面三个文件）。
测量件：`spike/TreeViewProbe`（`Program.cs` + `ProbePass2.cs`，11 个模式）；原始读数
`adaptation/s1e-treeview-raw.txt`；底座结论 `adaptation/00-jalium-theme-capabilities.md` S1-e 1-11。

## 0 · 本段的两条前提，以及一条被推翻的旧前提

1. **形状对不上是前提。** WinUI 的树是扁平列表：`DefaultTreeViewStyle` 的模板整体是一个 `controls:TreeViewList`
   （`TreeView.xaml:26`），条目样式 `BasedOn DefaultListViewItemStyle`（`TreeViewItem.xaml:3`），缩进来自
   `TreeViewItemTemplateSettings.Indentation` 的绑定（:131）。本运行时是嵌套形：
   `TreeView : ItemsControl : Control`（不是 `Selector`）、`TreeViewItem : HeaderedItemsControl : ItemsControl`。
   `TreeViewList`、`TreeViewNode`、`TreeViewItemPresenter`、`TreeViewItemTemplateSettings` 未导出。所以本批交的是
   **"这个运行时的嵌套树，穿 WinUI 的行"**，不是 WinUI 的 TreeView——Gallery 目录项与下面的 §6 都按这句话界定范围。
2. **不起自有类型。** 缺的是行为面（`SelectionMode`/多勾/拖拽/invocation/`ExpandAll`），不是外观面；自有类型补不回
   运行时没有的行为属性，反而违反 AGENTS.md 的"只有证明的行为缺口才起自有类型"。外观所需的三条通道（条目宿主、
   展开态、缩进）全部实测可达。
3. **被推翻的旧前提：容器 `Style` 恒为 null。** S1-c/S1-d 的不变式（隐式类型键命中而容器自己的 `Style` 保持 null）
   在树族不成立——本运行时自带 `TreeView`/`TreeViewItem` 两条隐式样式（探针模式 A 读 `Application.TryFindResource`
   两个都是 `Style`），所以容器的 `Style` 读数非 null。本批所有断言因此只读效果，不读"样式是否被赋上"。

## 1 · 上游 39 行家族：发 31，withhold 8

三个分支的 39 个行名**逐字相同**（分支间只有目标不同），因此下表一次覆盖三个分支。目标列写上游原文的 `ResourceKey`。

| 族 | 行数 | 上游 | 我们的处理 |
| --- | --- | --- | --- |
| 背景 | 8 | `TreeViewItemBackground`→`SubtleFillColorTransparentBrush`；`…PointerOver`→`Subtle…Secondary`；`…Pressed`→`Subtle…Tertiary`；`…Disabled`→`Subtle…Disabled`；`…Selected`→`Subtle…Secondary`；`…SelectedPointerOver`→`Subtle…Tertiary`；`…SelectedPressed`→`Subtle…Secondary`；`…SelectedDisabled`→`Subtle…Disabled` | 全部照发（`ThemeResources/TreeView.jalxaml`）。静止/悬停/按下/禁用/选中四格吃 `ContentBorder.Background`，选中+悬停、选中+按下、选中+禁用三格吃对应组合行；由 `AstraTreeViewTests.Selecting_raises_the_pill_and_leaves_the_fill_subtle`、`A_disabled_row_drops_its_pill_and_a_selected_disabled_row_keeps_it` 断言 |
| 前景 | 8 | `…Foreground{,PointerOver,Selected,SelectedPointerOver}`→`TextFillColorPrimary`；`…Pressed`、`…SelectedPressed`→`TextFillColorSecondary`；`…Disabled`、`…SelectedDisabled`→`TextFillColorDisabled` | 全部照发，但**写在不带 `TargetName` 的 `Style.Triggers` 上**（条目自己的 `Foreground`，生成的文本靠继承取到）。理由见 §5.3：`ContentPresenter` 在本运行时没有 `Foreground` 成员，上游那种"写 presenter"的格子在这里是哑的。由 `An_alias_row_resolves_to_its_target`（28 条身份断言）与 `A_state_recolours_the_text_through_the_container` 断言 |
| 描边 | 8 | 八条全部→`SubtleFillColorTransparentBrush`；配 `TreeViewItemBorderThemeThickness`：Default/Light `0`、High Contrast `1` | 全部照发并各配一格写 `ContentBorder.BorderBrush`。在 Light/Dark 里它们画不出东西（厚度 0），仍发是因为"关于一个名字的 parity 断言必须活过状态映射"（与 ComboBox 的九条惰性条目边框同理） |
| 选中指示器 | 4 | `…SelectionIndicatorForeground{,PointerOver,Pressed}`→`AccentFillColorDefaultBrush`，`…Disabled`→`TextFillColorDisabledBrush` | 全部照发，喂给 `SelectionIndicator` 的 `Fill`（上游也是拿 `Foreground` 行喂 `Rectangle.Fill`，见 `TreeViewItem.xaml:23,27,45`）。名字与喂法一起保留 |
| 度量 | 4 | `TreeViewItemBorderThemeThickness` 0；`TreeViewItemPresenterMargin` `4,2`；`TreeViewItemPresenterPadding` `0,3,0,5`；`TreeViewItemMultiSelectSelectedItemBorderMargin` 0 | 前三条照发（分别喂 `BorderThickness`、`ContentBorder.Margin`、`ContentBorder.Padding`），第四条随多勾面一起 withhold |
| 高度 | 3 | `TreeViewItemMinHeight` 28；`TreeViewItemMultiSelectCheckBoxMinHeight` 28；`TreeViewItemContentHeight` 20（全是 `x:Double`） | **不发**：本读者不能解析 `x:Double` 资源（先例：Expander、AppBar、AutoSuggestBox、Metrics）。28 与 20 作为字面量进样式（`MinHeight` 与 presenter 的 `MinHeight`），28 与 3+5 相加正是上游行高的算法；第二个 28 随多勾面无消费者 |
| 多勾 | 4 | `TreeViewItemMultiSelectBorderBrushSelected`、`TreeViewItemCheckBoxBackgroundSelected`、`TreeViewItemCheckBoxBorderSelected`、`TreeViewItemCheckGlyphSelected` | **不发**：本运行时的 `TreeView` 没有 `SelectionMode`/`MultiSelect`/勾面（探针模式 A 的 absent 列表），发出来必然无人可读 |

withhold 合计 8（4 勾面行 + 1 勾面度量 + 3 `x:Double`），逐名断言在
`AstraTreeViewTests.A_withheld_upstream_row_is_not_published`；发出的 31 行每条都有消费者，由反向闸
`AstraResourceKeyTests.Transcribed_control_rows_are_read_by_a_template`（本字典已入闸表）守住。

## 2 · 上游模板形状与本批部件对照

`MUX_TreeViewItemStyle`（`TreeViewItem.xaml:14-149`）：

```
Grid 'ContentPresenterGrid'  Margin={TreeViewItemPresenterMargin} Padding={TreeViewItemPresenterPadding}
                               Background/BorderBrush/BorderThickness={TemplateBinding …} CornerRadius={ThemeResource ControlCornerRadius}
├─ Rectangle 'SelectionIndicator'  Width=3 Height=16 RadiusX=2 RadiusY=2 Fill=…IndicatorForeground Opacity=0 左对齐垂直居中
└─ Grid 'MultiSelectGrid'  Margin={…MultiSelectSelectedItemBorderMargin} Padding={Binding …TemplateSettings.Indentation}
   ├─ Grid[0]  CheckBox 'MultiSelectCheckBox' 32 宽 / Border 'MultiArrangeOverlayTextBorder'（拖拽计数）
   ├─ Grid 'ExpandCollapseChevron' Padding=14,0  TextBlock 'CollapsedGlyph' / 'ExpandedGlyph' 12x12，Visibility 由 TemplateSettings 决定
   └─ ContentPresenter 'ContentPresenter'  MinHeight={TreeViewItemContentHeight} Margin={TemplateBinding Padding}
```

`DefaultTreeViewStyle`（`TreeView.xaml:9-30`）：`IsTabStop=False`、`CanDragItems`/`CanReorderItems`/`AllowDrop`=True、
`ItemContainerTransitions`（Content/Reorder/Entrance 三条），模板体是
`<controls:TreeViewList Name="ListControl" Background="{TemplateBinding Background}" …/>`。**没有 ScrollViewer、没有
Border、不读任何颜色行**——`TreeViewList` 自带这些。

本批对照（`Styles/TreeViews.jalxaml`）：

| 上游 | 我们 | 依据 |
| --- | --- | --- |
| `TreeViewList` 宿主 | `Border 'LayoutRoot' > ScrollViewer 'ScrollViewer' > ItemsPresenter` | 运行时没有 `TreeViewList`；宿主用与 ListBox/ListView 同形的可滚动外壳（§5.1 记下 `IsTabStop` 为什么不抄） |
| `ContentPresenterGrid` | `Border 'ContentBorder'`（同名同职责：Margin/Padding/Background/BorderBrush/BorderThickness/CornerRadius 六件事） | 本读者的 `ControlTemplate` 只能有一个视觉根，Grid+Border 合并为 `Border`；圆角吃共享行 `ControlCornerRadius`（上游 `TreeViewItem.xaml:10,131` 同一个） |
| `SelectionIndicator` | `Rectangle 'SelectionIndicator'`，3x16、半径 2、Opacity 0 起步 | 整份几何可读，逐数照抄（§5.4 与 ListViewItem 那次对比） |
| `MultiSelectGrid` 的列 + `MultiSelectCheckBox` | **不发** | 无多勾面 |
| `ExpandCollapseChevron` + 两个 glyph `TextBlock` | `Grid` 内 `ToggleButton 'ChevronHitTarget'`（命中区）+ 两个 `Path`（`CollapsedGlyph`/`ExpandedGlyph`） | 上游的 glyph 是一对、按 TemplateSettings 换可见性；这里换成一对 `Path`（本仓库既有的箭头画法，见 `Styles/Common.jalxaml`、`Surfaces.jalxaml`），换可见性的格子同形。命中区必须是真控件且走双向绑定，因为名字不钩（S1-e 3） |
| `ContentPresenter` | `ContentPresenter 'ContentPresenter'`，`Content={TemplateBinding Header}` + `HeaderTemplate`/`HeaderStringFormat`，`MinHeight=20`，内嵌 `TextBlock` 不换行样式 | 本运行时的内容属性是 `Header`（`HeaderedItemsControl`）而不是 `Content`（WinUI 的 `TreeViewItem : ListViewItem`）——这是形状差异的第二处，见 §5.2 |
| （无对应：WinUI 不需要） | `ItemsPresenter 'ChildHost'`，第二行，`Margin=16,0,0,0` | 嵌套形的必然产物；WinUI 的层级在扁平列表里由 TemplateSettings 缩进表达（§5.2） |

## 3 · VisualState → Triggers 映射

上游三组共 13 个状态（`TreeViewItem.xaml:17-128`）。本运行时 `TreeViewItem` 可选信号的实测集合
（模式 E）：`IsExpanded`、`IsSelected`、`IsSelectionActive`、`HasItems`、`IsMouseOver`、`IsMouseCaptureWithin`、
`IsEnabled`、`IsFocused`；**没有** `IsPressed`、`IsPointerOver`。

| 上游状态 | 上游写的东西 | 本批格子 | 状态源 |
| --- | --- | --- | --- |
| `Normal` | 无 | 无（静止值） | — |
| `PointerOver` | bg=`…BackgroundPointerOver`，fg=`…ForegroundPointerOver`，pill Fill=`…IndicatorForegroundPointerOver`，两个 glyph Foreground，border，pill Opacity 0 | `Trigger IsMouseOver=True` → `ContentBorder.Background/BorderBrush`、两个 `Path.Stroke`、`SelectionIndicator.Fill`、pill Opacity 0；`Style.Triggers` 里同格写条目 `Foreground` | 同 S1-c/S1-d：没有 `IsPointerOver`，`IsMouseOver` 是唯一悬停信号 |
| `Pressed` | bg=`…Pressed`，fg=`…ForegroundPressed`，pill，border，Opacity 0 | `Trigger IsMouseCaptureWithin=True` | 没有 `IsPressed`，捕获内是近似（沿用列表族同一近似并同样不声称） |
| `Selected` | bg=`…Selected`，fg，pill，border，**Opacity 1** | `Trigger IsSelected=True` | 上游没有 `SelectedUnfocused`，所以 `IsSelectionActive` 在这里没有用武之地（信号存在但状态不需要） |
| `Disabled` | bg=`…Disabled`，fg=`…ForegroundDisabled`，pill Fill=`…Disabled`，border（Opacity 保持 0，即禁用且未选中的行没有药丸） | `Trigger IsEnabled=False` | — |
| `PointerOverSelected` | bg=`…SelectedPointerOver`，fg，pill，border，Opacity 1 | `MultiTrigger IsSelected + IsMouseOver` | — |
| `PressedSelected` | bg=`…SelectedPressed`，…，Opacity 1 | `MultiTrigger IsSelected + IsMouseCaptureWithin` | — |
| `SelectedDisabled` | bg=`…SelectedDisabled`，fg=`…ForegroundSelectedDisabled`，pill Fill=`…IndicatorForegroundDisabled`，Opacity **1** | `MultiTrigger IsSelected + IsEnabled=False`，排在 `IsEnabled=False` 单格之后 | 本读者的触发格是叠加的、后写者取值（ListView 段量到的同一规则），顺序因此是契约，测试 `A_disabled_row_drops_its_pill_and_a_selected_disabled_row_keeps_it` 钉住 |
| `ReorderedPlaceholder`（FadeOut `MultiSelectGrid`） | 拖拽重排动画 | **无** | 无 `CanReorderItems`，无 `MultiSelectGrid` |
| `TreeViewMultiSelectDisabled/EnabledUnselected/EnabledSelected` | 勾可见性、chevron padding、`MultiSelectGrid` 底色 | **无** | 无多勾面（§1 的 5 条 withhold 行就是它的全部料） |
| `DragStates.NotDragging/MultipleDraggingPrimary` | 拖拽遮罩 | **无** | 无拖拽 |
| （无对应） | — | `Trigger IsExpanded=False` → `ChildHost` Collapsed；`Trigger IsExpanded=True` → 两个 glyph 换可见性；`Trigger HasItems=False` → chevron 与两个 glyph Hidden | 嵌套形自己的状态；四条静止值写法实测全通（S1-e 4），`HasItems` 用 `Hidden` 保住列位 |

## 4 · 命名与公开面

- 发 31 条 `TreeView*` 行（全部上游名）、`DefaultTreeViewStyle`/`DefaultTreeViewItemStyle` 两条 keyed 样式与两条
  隐式样式（`TargetType=TreeView`、`TargetType=TreeViewItem`）。不发 `TreeViewBackground` 等名，因为上游没有。
- 条目样式走**隐式类型键**，不设 `ItemContainerStyle`（`The_tree_style_leaves_item_container_style_to_the_application`
  断言样式里没有该 Setter、控件上该属性为 null）。
- CLR 公开面零改动：本批不新增类型、不改签名，故公开 API 清单不动；资源键清单由
  `AstraResourceKeyTests` 现读现比（`keys.md` 仍是未生成的旧账 #11）。

## 5 · 偏离与 Known Gaps

1. **`IsTabStop=False` 不抄。** 上游那值是给它内部可聚焦的 `TreeViewList` 的；本运行时宿主没有这一层，抄下来会把整棵树
   移出 Tab 序。留在运行时默认值。宿主背景发 `Transparent`、`BorderThickness=0`（上游宿主不读任何行）。
2. **`Content`→`Header`、扁平→嵌套。** 这是形状差异的两面：内容属性名不同（`HeaderedItemsControl.Header`），且子项
   生活在本行的第二行里。后果是 WinUI 的"行铺满列表宽 + 内容缩进"在这里变成"子行整体缩进、填充随层级变窄"
   （每级 16 DIP）。16 是 stock 自己的步长（`PART_IndentSpacer` 实测 level-1 宽 0、level-2 宽 16），不是自加的数。
   深度无处可读：两行之间全部可读值只差 `IsExpanded`/`Header`/布局记账（S1-e 5）。
3. **状态前景改写点。** 上游把八条前景行喂 `ContentPresenter.Foreground`；本运行时的 `ContentPresenter` 没有该成员，
   那条路线是哑的，于是八条行喂条目控件自己的 `Foreground`（`Style.Triggers`）+ 文本继承。**同一缺陷已存在于先前的
   ListBox/ListView/ComboBox/菜单族**，另立 #31 逐族修，本批不顺手改别人家的模板。
4. **箭头是两条 `Path`，不是字体 glyph。** 上游用 `SymbolThemeFontFamily` 的 `CollapsedGlyph`/`ExpandedGlyph`；本仓库
   已量到 FluentSystemIcons 缺字（记忆条目），沿用既有 `Path` 画法。另外 `TreeViewItem` 没有 `GlyphBrush`/`GlyphOpacity`/
   `GlyphSize` 这类属性面，那三行因此无处发。
5. **折叠不销毁容器。** `IsExpanded=False` 只收掉子宿主可见性；已生成的子容器留在树里（stock 同行为，模式 E）。断言
   因此看 `ChildHost.Visibility` 而不是容器数。
6. **无真指针、无触摸。** 悬停/按下两格只有触发器读数背书；本批像素证据是"选中态 + 令牌落像素"，不是"指针停在行上"。
7. **键盘展开未测。** stock 没有可命名的箭头部件、也没有按名钩子，控件自身是否响应方向键/`+`/`-` 未量。任何写
   `IsExpanded` 的路线都会驱动同一套视觉（属性→绑定→可见性已双向实测），但"键盘能写这个属性"这件事不在本批证据里。
8. **禁用宿主不下跑。** 与 ListView 同一读数：`IsEnabled` 会继承到容器，但状态格的重跑行为不在本批证据内；
   本批只断行级禁用（前景 + 填充 + 药丸三处取禁用值）。
9. **无焦点视觉。** 沿用列表族结论：本运行时没有供模板使用的 `UseSystemFocusVisuals` 槽，不声称。

## 6 · 不声称清单

不声称：WinUI 的 `TreeView` 行为面（多勾、`SelectionMode`、拖拽重排、invocation、`ExpandAll`/`CollapseAll`、
`SelectedNode`）；真指针与触摸下的悬停/按下；键盘展开/折叠路径；焦点视觉；行铺满列表宽的缩进；高对比分支下那 8 条
withhold 行的对应物；宿主实心底色（上游没有，本批也不给）；`TreeViewItem` 的 `Content`/`Glyph*` 属性面；
`ItemContainerStyle` 作为本批的交付路线（它留给应用）。
