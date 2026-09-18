# 原生控件真空清单（Fluent 外观覆盖到哪一步）

由 `tools/Report-Control-Vacuum.ps1` 生成，**勿手改**。范围权威是 ModernWpf 的两层：
``ModernWpf/Styles/*.xaml``（宿主已有控件的 Fluent 样式）与 ``ModernWpf.Controls/<Name>/``（宿主没有的 WinUI 派生类型）。
原生类型与绘制分级来自 ``01-census-raw-output.txt``。

| 量 | 数 |
|---|---|
| Jalium public 具体 `Control` 类型 | 163 |
| ModernWpf 承诺覆盖的控件名（并集） | 84 |
| 其中 Jalium 有同名原生类型（可走样式层） | 56 |
| 其中我们已声明隐式样式 | 13 |
| **其中真空，且无自有类型顶替** | **41** |
| 其中真空但已有自有类型顶替 | 2 |
| ModernWpf 得自己写类型、Jalium 也无同名原生类型 | 28 |
| Jalium 独有、不在这份范围内（只标注，不还原） | 106 |
| 我们已样式、但 ModernWpf 无同名文件（按别名/内联实现） | 1 |

## A. 真空：范围内、Jalium 有原生类型、我们还没有样式（41）

绘制方式列只说明**能不能**靠模板解决；排批顺序仍以"底座先于叶子"为准（见 ROADMAP D 节）。

这一列是启发式：类型重写了 OnRender 或 OnPaint 就记作"自绘"。NumberBox 批实测它会把话说死——
`NumberBox` 重写绘制，却仍带一个可替换的代码构建 `ControlTemplate`（样式 setter 换得动，
见 `adaptation/00` S0-h 与 `audits/numberbox.md`）。**排到某个控件时先量一遍再决定要不要自有类型，
不要把这一列当结论。**

| 控件 | 绘制方式 | ModernWpf 放在哪一层 |
|---|---|---|
| Calendar | 自绘，模板只能改外围 | Styles |
| ColorPicker | 自绘，模板只能改外围 | Controls |
| CommandBar | 自绘，模板只能改外围 | Styles + Controls |
| ContentControl | 纯模板，可完全覆盖 | Styles |
| ContentDialog | 纯模板，可完全覆盖 | Controls |
| ContextMenu | 纯模板，可完全覆盖 | Styles |
| DataGrid | 纯模板，可完全覆盖 | Styles |
| DatePicker | 自绘，模板只能改外围 | Styles |
| Expander | 纯模板，可完全覆盖 | Styles |
| Frame | 纯模板，可完全覆盖 | Styles |
| GridSplitter | 纯模板，可完全覆盖 | Styles |
| GroupBox | 纯模板，可完全覆盖 | Styles |
| GroupItem | 纯模板，可完全覆盖 | Styles |
| HeaderedContentControl | 纯模板，可完全覆盖 | Styles |
| InfoBar | 自绘，模板只能改外围 | Controls |
| ItemsControl | 自绘，模板只能改外围 | Styles |
| Label | 自绘，模板只能改外围 | Styles |
| ListBox | 纯模板，可完全覆盖 | Styles |
| ListBoxItem | 纯模板，可完全覆盖 | Styles |
| ListView | 纯模板，可完全覆盖 | Styles + Controls |
| ListViewItem | 纯模板，可完全覆盖 | Styles |
| Menu | 自绘，模板只能改外围 | Styles |
| MenuBar | 自绘，模板只能改外围 | Controls |
| MenuItem | 自绘，模板只能改外围 | Styles |
| NavigationWindow | 纯模板，可完全覆盖 | Styles |
| ProgressBar | 自绘，模板只能改外围 | Styles |
| ResizeGrip | 自绘，模板只能改外围 | Styles |
| RichTextBox | 自绘，模板只能改外围 | Styles |
| ScrollBar | 自绘，模板只能改外围 | Styles |
| Separator | 自绘，模板只能改外围 | Styles |
| SplitButton | 纯模板，可完全覆盖 | Controls |
| StatusBar | 自绘，模板只能改外围 | Styles |
| TabControl | 自绘，模板只能改外围 | Styles |
| Thumb | 自绘，模板只能改外围 | Styles |
| TimePicker | 自绘，模板只能改外围 | Controls |
| TitleBar | 纯模板，可完全覆盖 | Controls |
| ToolBar | 纯模板，可完全覆盖 | Styles |
| TreeView | 纯模板，可完全覆盖 | Styles |
| TreeViewItem | 纯模板，可完全覆盖 | Styles |
| UserControl | 纯模板，可完全覆盖 | Styles |
| Window | 自绘，模板只能改外围 | Styles |

## B. 已上隐式样式（13）

'Button' 'CheckBox' 'ComboBox' 'HyperlinkButton' 'NumberBox' 'PasswordBox' 'RadioButton' 'RepeatButton' 'ScrollViewer' 'Slider' 'TextBox' 'ToggleButton' 'ToolTip'

## C. 真空但已有自有类型顶替（2）

- NavigationView  ←  'FluentNavigationView'
- ToggleSwitch  ←  'FluentToggleSwitch'

## D. Jalium 无同名原生类型（28）

补样式解决不了：要么自有类型，要么明确放弃。名单里含 ModernWpf 的工程目录名（非控件），保留不筛。

'AnnotatedScrollBar' 'AutoSuggestBox' 'BreadcrumbBar' 'CommandBarFlyout' 'DropDownButton' 'Flyout' 'GridView' 'Hyperlink' 'InfoBadge' 'ItemContainer' 'ItemsView' 'LayoutPanel' 'MenuFlyout' 'NavigationBackButton' 'Page' 'PagerControl' 'PersonPicture' 'ProgressRing' 'RadioButtons' 'RadioMenuItem' 'RatingControl' 'Repeater' 'SelectorBar' 'SplitView' 'TabView' 'TeachingTip' 'TwoPaneView' 'WrapPanel'

## E. Jalium 独有、这份范围不覆盖（106）

不做 Fluent 还原，但**它们会露出框架素外观**：Gallery 里出现任何一个都必须显式标"未样式化"，
这也是把 Gallery 当成唯一回归面的原因。

'CalendarDayButton' 'CalendarButton' 'RangeSlider' 'DiffViewer' 'HeaderedItemsControl' 'DataGridColumnHeader' 'PropertyGrid' 'MarkdownListPresenter' 'MarkdownHeadingPresenter' 'MarkdownQuotePresenter' 'Split' 'NavigationViewItemHeader' 'TabItem' 'PieChart' 'TreeSelectorItem' 'AppBarButton' 'ChartTooltip' 'Terminal' 'RibbonSplitMenuItem' 'MarkdownTableCellPresenter' 'RibbonQuickAccessToolBar' 'Sparkline' 'RibbonApplicationMenuItem' 'RibbonTab' 'SankeyDiagram' 'SwipeControl' 'MenuFlyoutSeparator' 'MarkdownTablePresenter' 'HexEditor' 'StickyNoteControl' 'RibbonGalleryItem' 'DatePickerTextBox' 'FlowDocumentPageViewer' 'TransitioningContentControl' 'RibbonMenuButton' 'NavigationViewItem' 'BarChart' 'RibbonToggleButton' 'RazorItemsHost' 'DockLayout' 'NavigationViewItemSeparator' 'RibbonMenuItem' 'TreeDataGridRow' 'GaugeChart' 'MarkdownDiagramPresenter' 'MarkdownCodePresenter' 'RibbonContextualTabGroup' 'JsonTreeViewer' 'RibbonGallery' 'DataGridCell' 'RibbonSeparator' 'TreeMap' 'MarkdownImagePresenter' 'MarkdownFootnotePresenter' 'ChartLegend' 'DevToolsWindow' 'AppBarToggleButton' 'CalendarItem' 'DataGridCellsPresenter' 'FlowDocumentReader' 'EditControl' 'DockTabPanel' 'DocumentViewer' 'QRCode' 'MenuFlyoutSubItem' 'RibbonComboBox' 'TitleBarButton' 'GridViewColumnHeader' 'MarkdownListItemPresenter' 'RibbonButton' 'TreeDataGrid' 'Markdown' 'DockItem' 'TreeSelector' 'RibbonApplicationMenu' 'AutoCompleteBox' 'RibbonCheckBox' 'RibbonTextBox' 'ScatterPlot' 'LineChart' 'StatusBarItem' 'MarkdownParagraphPresenter' 'MenuBarItem' 'GanttChart' 'ToggleMenuFlyoutItem' 'NetworkGraph' 'RibbonApplicationSplitMenuItem' 'FlowchartDiagram' 'CameraView' 'MenuFlyoutItem' 'RibbonGalleryCategory' 'AppBarSeparator' 'ToastNotificationItem' 'Control' 'CandlestickChart' 'RibbonSplitButton' 'MapView' 'FlowDocumentScrollViewer' 'Heatmap' 'DataGridRowHeader' 'MarkdownRulePresenter' 'MermaidDiagram' 'DataGridRow' 'DataGridColumnHeadersPresenter' 'RibbonGroup' 'Ribbon'

## 这份清单证明不了什么

- 只统计**声明**。"声明是否落到像素"这条限定现在只对 **Button 撤了**（``06-pixel-attribution.md``：
  隐式样式 -> 模板 -> 令牌 -> 像素已有断言），其余真空控件仍未证。``00-pixel-harness-raw-output.txt``
  里"像素不动"那组读数出自未推帧的捕获，不再作为证据。
- 绘制方式列来自 census 的 ``OnRender``/``OnPaint`` 判定，不区分读 DP 还是读死的 ``ThemeColors``；
  后者见 ``02-render-ceiling.md``。
- 范围以 ModernWpf 为**文件/目录名**为锚，所以按别名或内联在父控件文件里的条目样式会漏计
  （``TabItem``/``ComboBoxItem``/``MenuFlyoutItem`` 这类条目容器就是假阴性来源）。下面这份
  "已样式但名单没有"的差集是人工核对入口，不要当成"范围外"：

'ComboBoxItem'
