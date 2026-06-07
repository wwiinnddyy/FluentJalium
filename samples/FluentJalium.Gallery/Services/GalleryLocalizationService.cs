using FluentJalium.Gallery.Models;

namespace FluentJalium.Gallery.Services;

internal sealed class GalleryLocalizationService
{
    public const string EnglishCultureName = "en-US";
    public const string ChineseCultureName = "zh-Hans";

    private readonly Dictionary<string, Dictionary<string, string>> _localizedText = CreateLocalizedText();
    private GalleryLanguageOption _currentLanguage;

    public GalleryLocalizationService()
    {
        SupportedLanguages =
        [
            new GalleryLanguageOption(EnglishCultureName, "English", "English"),
            new GalleryLanguageOption(ChineseCultureName, "Chinese", "简体中文")
        ];
        _currentLanguage = SupportedLanguages[0];
    }

    public event EventHandler? LanguageChanged;

    public IReadOnlyList<GalleryLanguageOption> SupportedLanguages { get; }

    public GalleryLanguageOption CurrentLanguage => _currentLanguage;

    public bool IsChinese => string.Equals(_currentLanguage.CultureName, ChineseCultureName, StringComparison.OrdinalIgnoreCase);

    public void SetLanguage(GalleryLanguageOption language)
    {
        SetLanguage(language.CultureName);
    }

    public void SetLanguage(string cultureName)
    {
        var language = SupportedLanguages.FirstOrDefault(option =>
            string.Equals(option.CultureName, cultureName, StringComparison.OrdinalIgnoreCase));
        if (language == null || string.Equals(language.CultureName, _currentLanguage.CultureName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _currentLanguage = language;
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string Text(string key)
    {
        if (_localizedText.TryGetValue(key, out var values))
        {
            if (values.TryGetValue(_currentLanguage.CultureName, out var localized))
            {
                return localized;
            }

            if (values.TryGetValue(EnglishCultureName, out var english))
            {
                return english;
            }
        }

        return key;
    }

    public string Text(string key, params object[] args)
    {
        return string.Format(Text(key), args);
    }

    public string PageTitle(string pageId) => Text($"page.{pageId}.title");

    public string PageDescription(string pageId) => Text($"page.{pageId}.description");

    public string PageKeywords(string pageId) => Text($"page.{pageId}.keywords");

    public string GroupName(string groupId) => Text($"group.{groupId}");

    private static Dictionary<string, Dictionary<string, string>> CreateLocalizedText()
    {
        var text = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        void Add(string key, string english, string chinese)
        {
            text[key] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [EnglishCultureName] = english,
                [ChineseCultureName] = chinese
            };
        }

        Add("shell.title", "FluentJalium", "Fluent Jalium");
        Add("shell.subtitle", "Control gallery", "控件库");
        Add("shell.searchPlaceholder", "Search controls, materials, and samples", "搜索控件、材质和示例");
        Add("shell.language", "Language", "语言");
        Add("shell.noResultsTitle", "No results", "无结果");
        Add("shell.noPages", "No Gallery pages are available.", "没有可用的 Gallery 页面。");
        Add("shell.noSearchMatches", "No Gallery pages match \"{0}\".", "没有与“{0}”匹配的 Gallery 页面。");

        Add("sample.example", "Example", "示例");
        Add("sample.states", "States", "状态");
        Add("sample.properties", "Properties", "属性");
        Add("sample.codeNotes", "Code / Notes", "代码 / 说明");
        Add("sample.state.normal", "Normal", "正常");
        Add("sample.state.pointerOver", "Pointer over", "指针悬停");
        Add("sample.state.pressedFocused", "Pressed/focused", "按下/聚焦");
        Add("sample.state.disabled", "Disabled", "禁用");
        Add("sample.state.selectedChecked", "Selected/checked", "已选/已勾选");
        Add("sample.state.opened", "Opened", "已打开");
        Add("sample.state.active", "Active", "活动");
        Add("sample.property.themeResources", "Theme resources", "主题资源");
        Add("sample.property.accent", "Accent", "强调色");
        Add("sample.property.variant", "Variant", "变体");
        Add("sample.property.content", "Content", "内容");
        Add("sample.property.isEnabled", "IsEnabled", "IsEnabled");

        Add("status.preview", "Preview", "预览");
        Add("status.diagnostic", "Diagnostic", "诊断");
        Add("doc.winuiGallery", "WinUI Gallery", "WinUI Gallery");
        Add("doc.fluentJaliumSource", "FluentJalium source", "FluentJalium 源码");
        Add("doc.relatedFwControl", "Related FW control", "相关 FW 控件");

        Add("group.Home", "Home", "主页");
        Add("group.Catalog", "Catalog", "目录");
        Add("group.Design", "Design", "设计");
        Add("group.Control surfaces", "Control surfaces", "控件表面");
        Add("group.Input", "Input", "输入");
        Add("group.Layout and media", "Layout and media", "布局与媒体");
        Add("group.Collections and data", "Collections and data", "集合与数据");
        Add("group.Materials", "Materials", "材质");
        Add("group.Motion", "Motion", "动效");
        Add("group.App structure", "App structure", "应用结构");
        Add("group.Diagnostics", "Diagnostics", "诊断");

        AddPage(text, "overview", "Overview", "概览", "Theme, typography, and accent controls for validating FluentJalium across variants.", "用于验证 FluentJalium 多种变体的主题、排版和强调色控件。", "home design system theme typography accent light dark high contrast 主页 设计系统 主题 排版 强调色 亮色 暗色 高对比度");
        AddPage(text, "allcontrols", "All Controls", "全部控件", "Filterable index of FluentJalium control pages with source paths, API namespaces, base classes, and related FW controls.", "可筛选的 FluentJalium 控件页索引，包含源码路径、API 命名空间、基类和相关 FW 控件。", "all controls catalog filter metadata FW index 全部 控件 目录 筛选 元数据");
        AddPage(text, "newcontrols", "New Controls", "新增控件", "First-wave Gallery entries marked with IsNew metadata.", "标记为 IsNew 的第一波 Gallery 条目。", "new controls catalog filter IsNew first wave FW index 新增 控件 目录");
        AddPage(text, "updatedcontrols", "Updated Controls", "更新控件", "Gallery entries marked with IsUpdated metadata for recently expanded samples.", "标记为 IsUpdated 的近期扩展示例条目。", "updated controls catalog filter IsUpdated improved FW index 更新 控件 目录");
        AddPage(text, "previewcontrols", "Preview Controls", "预览控件", "Preview-status control pages that need design and API validation before becoming stable.", "成为稳定页前仍需设计和 API 验证的预览状态控件页。", "preview controls catalog filter status FW index 预览 控件 目录");
        AddPage(text, "diagnosticcontrols", "Diagnostic Controls", "诊断控件", "Diagnostic-status Gallery pages used to audit states, resources, and catalog health.", "用于审计状态、资源和目录健康度的诊断状态 Gallery 页面。", "diagnostic controls catalog filter status state matrix FW index 诊断 控件 目录");
        AddPage(text, "themearchitecture", "Theme Architecture", "主题架构", "How FluentJalium splits stable theme entry points, design resources, control dictionaries, and FW control surfaces.", "FluentJalium 如何拆分稳定主题入口、设计资源、控件字典和 FW 控件表面。", "Generic FluentResources FluentControls FluentThemeManager theme resources controls dictionary architecture 主题 架构 资源 控件 字典");
        AddPage(text, "colors", "Colors", "颜色", "Accent, text, fill, and semantic color tokens for FluentJalium themes and FW controls.", "FluentJalium 主题和 FW 控件使用的强调色、文本、填充与语义颜色令牌。", "color colors accent text fill semantic token 颜色 强调色 文本 填充 语义 令牌");
        AddPage(text, "typography", "Typography", "排版", "Font families, type ramp, and control text roles used by FluentJalium themes.", "FluentJalium 主题使用的字体族、字号层级和控件文本角色。", "typography font type ramp text 排版 字体 字号 文本");
        AddPage(text, "geometry", "Geometry", "几何", "Corner radius, stroke, and elevation tokens for FluentJalium control surfaces.", "FluentJalium 控件表面的圆角、描边和层级令牌。", "geometry radius corner stroke border elevation 几何 圆角 描边 边框 阴影");
        AddPage(text, "motiontokens", "Motion Tokens", "动效令牌", "Duration, connected animation, and transition role tokens that keep FluentJalium motion aligned with WinUI pacing.", "让 FluentJalium 动效与 WinUI 节奏保持一致的时长、连接动画和过渡角色令牌。", "motion duration animation transition 动效 时长 动画 过渡");
        AddPage(text, "buttons", "Buttons", "按钮", "Button and command surfaces, including split, drop-down, app bar, toolbar, and material command decks.", "按钮与命令表面，包括拆分、下拉、应用栏、工具栏和材质命令面板。", "button command split dropdown appbar toolbar 按钮 命令 拆分 下拉 工具栏");
        AddPage(text, "switches", "Switches", "开关", "ToggleButton and ToggleSwitch states, events, keyboard toggles, and material-aware setting rows.", "ToggleButton 和 ToggleSwitch 的状态、事件、键盘切换与材质感知设置行。", "toggle switch checked keyboard 开关 切换 选中 键盘");
        AddPage(text, "textinput", "Text Input", "文本输入", "TextBox, PasswordBox, NumberBox, AutoCompleteBox, AutoSuggestBox, and RichTextBox surfaces with states, filtering, and material input panels.", "TextBox、PasswordBox、NumberBox、AutoCompleteBox、AutoSuggestBox 与 RichTextBox 的状态、筛选和材质输入面板。", "text input textbox password number autocomplete autosuggest rich 文本 输入 密码 数字 自动完成 自动建议");
        AddPage(text, "selection", "Selection", "选择", "CheckBox, RadioButton, ComboBox, ComboBoxItem, and RatingControl surfaces with selection states and ratings.", "CheckBox、RadioButton、ComboBox、ComboBoxItem 和 RatingControl 的选择状态与评分表面。", "selection checkbox radio combo rating 选择 复选 单选 下拉 评分");
        AddPage(text, "range", "Range", "范围", "Slider, RangeSlider, ProgressBar, and ProgressRing controls with live values, snapped ranges, and material progress states.", "Slider、RangeSlider、ProgressBar 和 ProgressRing 的实时值、吸附范围与材质进度状态。", "range slider progress value 范围 滑块 进度 值");
        AddPage(text, "dateandtime", "Date and Time", "日期和时间", "DatePicker, TimePicker, and Calendar controls with bounds, clock formats, blackout dates, and material planning surfaces.", "DatePicker、TimePicker 和 Calendar 的边界、时钟格式、禁用日期和材质规划表面。", "date time calendar picker 日期 时间 日历 选择器");
        AddPage(text, "contentandlayout", "Content and Layout", "内容与布局", "TextBlock, AccessText, Border, content hosts, panels, Grid, transitioning content, and LiquidGlass layout surfaces.", "TextBlock、AccessText、Border、内容宿主、面板、Grid、过渡内容和 LiquidGlass 布局表面。", "content layout grid panel wrap 内容 布局 网格 面板");
        AddPage(text, "visuals", "Visuals", "视觉", "Fluent icon library, image stretch and zoom, label targets, separators, Viewbox scaling, and LiquidGlass visual surfaces.", "Fluent 图标库、图像拉伸与缩放、标签目标、分隔线、Viewbox 缩放和 LiquidGlass 视觉表面。", "visual icon image label separator viewbox 视觉 图标 图像 标签 分隔线");
        AddPage(text, "interaction", "Interaction", "交互", "ScrollViewer, SwipeControl, and GridSplitter controls with offset commands, swipe actions, keyboard increments, and LiquidGlass interaction surfaces.", "ScrollViewer、SwipeControl 和 GridSplitter 的偏移命令、滑动操作、键盘步进和 LiquidGlass 交互表面。", "interaction scroll swipe splitter keyboard 交互 滚动 滑动 分割 键盘");
        AddPage(text, "inputandmedia", "Input and Media", "输入与媒体", "ColorPicker, InkCanvas, InkPresenter, and MediaElement controls with alpha, hex, ink modes, stroke presentation, playback surfaces, and LiquidGlass media workbenches.", "ColorPicker、InkCanvas、InkPresenter 和 MediaElement 的透明度、十六进制、墨迹模式、笔画呈现、播放表面和 LiquidGlass 媒体工作台。", "input media color ink video 输入 媒体 颜色 墨迹 视频");
        AddPage(text, "collections", "Collections", "集合", "ListBox, ListView, TreeView, DataGrid, and TreeDataGrid controls with selection, hierarchy, table options, and material data surfaces.", "ListBox、ListView、TreeView、DataGrid 和 TreeDataGrid 的选择、层级、表格选项和材质数据表面。", "collections list tree data grid 集合 列表 树 数据 表格");
        AddPage(text, "advancedcollections", "Advanced Collections", "高级集合", "FWItemsRepeater virtualization, layout switching, and collection cache settings for first-wave collection coverage.", "用于第一波集合控件覆盖的 FWItemsRepeater 虚拟化、布局切换和集合缓存设置。", "FWItemsRepeater virtualized layout cache advanced collections first wave 高级 集合 虚拟化");
        AddPage(text, "selectorsandproperties", "Selectors and Properties", "选择器与属性", "TreeSelector and PropertyGrid surfaces for hierarchical selection, object editing, cascade checks, and material property panels.", "TreeSelector 和 PropertyGrid 的层级选择、对象编辑、级联检查和材质属性面板。", "selector property tree grid 选择器 属性 树 编辑");
        AddPage(text, "datainspectors", "Data Inspectors", "数据检查器", "DiffViewer, HexEditor, and JsonTreeViewer developer surfaces with diff, binary, JSON, and material inspection workbench states.", "DiffViewer、HexEditor 和 JsonTreeViewer 的差异、二进制、JSON 与材质检查工作台状态。", "data diff hex json inspector 数据 差异 十六进制 检查器");
        AddPage(text, "charts", "Charts", "图表", "Line, bar, pie, scatter, heatmap, sparkline, gauge, treemap, candlestick, network, Gantt, Sankey, legend, and tooltip surfaces for FluentJalium analytics views.", "用于 FluentJalium 分析视图的折线、柱状、饼图、散点、热力图、迷你图、仪表盘、树图、蜡烛图、网络、甘特、桑基、图例和提示表面。", "FWLineChart FWBarChart FWPieChart FWScatterPlot FWHeatmap FWSparkline FWGaugeChart FWTreeMap FWCandlestickChart FWNetworkGraph FWGanttChart FWSankeyDiagram FWChartLegend FWChartTooltip charts analytics dashboard data visualization 图表 分析 仪表盘 数据 可视化");
        AddPage(text, "navigation", "Navigation", "导航", "NavigationView, pane modes, hierarchy, tabs, Frame navigation, and material app shell states.", "NavigationView、窗格模式、层级、选项卡、Frame 导航和材质应用外壳状态。", "navigation frame tab pane shell 导航 框架 选项卡 窗格 外壳");
        AddPage(text, "windowbackdrops", "Window Backdrops", "窗口背景", "DWM-backed shell materials for Jalium windows, including Mica, Mica Alt, Acrylic, and solid shell fallback.", "Jalium 窗口的 DWM 外壳材质，包括 Mica、Mica Alt、Acrylic 和纯色外壳回退。", "window backdrop mica acrylic 窗口 背景 云母 亚克力");
        AddPage(text, "materialsand effects".Replace(" ", string.Empty), "Materials and Effects", "材质与效果", "Element backdrops, material roles, and WinUI-style layering for FluentJalium surfaces.", "FluentJalium 表面的元素背景、材质角色和 WinUI 风格分层。", "materials effects acrylic mica glass 材质 效果 亚克力 云母 玻璃");
        AddPage(text, "materialprimitives", "Material Primitives", "材质基础控件", "FWBackdrop and FWAcrylicBrush samples for lower-level material primitives not covered by the primary material surface page.", "展示主材质页面未覆盖的 FWBackdrop 与 FWAcrylicBrush 等底层材质基础控件。", "FWBackdrop FWAcrylicBrush material primitives acrylic mica backdrop brush 材质 基础");
        AddPage(text, "motionandtransitions", "Motion and Transitions", "动效与过渡", "Connected animation, shared element motion, and FW content transitions for FluentJalium navigation continuity.", "用于 FluentJalium 导航连续性的连接动画、共享元素动效和 FW 内容过渡。", "motion transition connected animation 动效 过渡 连接 动画");
        AddPage(text, "animatedcontrols", "Animated Controls", "动画控件", "FWAnimatedIcon and FWAnimatedVisualPlayer controls with playback, looping, fallback icon, and motion state coverage.", "包含播放、循环、回退图标和动效状态覆盖的 FWAnimatedIcon 与 FWAnimatedVisualPlayer 控件。", "FWAnimatedIcon FWAnimatedVisualPlayer animated visual player motion first wave 动画 控件");
        AddPage(text, "menus", "Menus", "菜单", "MenuBar, Menu, ContextMenu, MenuFlyout, and CommandBarFlyout surfaces with submenu, shortcut, and LiquidGlass workbench states.", "MenuBar、Menu、ContextMenu、MenuFlyout 和 CommandBarFlyout 的子菜单、快捷键和 LiquidGlass 工作台状态。", "menu flyout context command 菜单 弹出 上下文 命令");
        AddPage(text, "disclosure", "Disclosure", "披露", "Expander, ToolTip, ContentDialog, and GroupBox controls with command states and LiquidGlass disclosure panels.", "Expander、ToolTip、ContentDialog 和 GroupBox 的命令状态与 LiquidGlass 披露面板。", "disclosure expander tooltip dialog groupbox 披露 展开 提示 对话框 分组");
        AddPage(text, "advancedinteraction", "Advanced Interaction", "高级交互", "FWRefreshContainer, FWScroller, and FWAnnotatedScrollBar samples for interaction controls that were implemented but underrepresented in Gallery.", "补充已实现但 Gallery 展示不足的 FWRefreshContainer、FWScroller 和 FWAnnotatedScrollBar 交互控件。", "FWRefreshContainer FWScroller FWAnnotatedScrollBar pull refresh snap scrollbar interaction 高级 交互");
        AddPage(text, "status", "Status", "状态", "InfoBar, InfoBadge, ToastNotification, and StatusBar controls with severity, queue, and material operation states.", "InfoBar、InfoBadge、ToastNotification 和 StatusBar 的严重性、队列和材质操作状态。", "status notification info toast badge 状态 通知 信息 徽章");
        AddPage(text, "design", "Design", "设计", "Gallery shell, catalog, sample card, and material entry design rules exposed as a footer navigation entry.", "作为页脚导航入口公开的 Gallery 外壳、目录、示例卡片和材质入口设计规则。", "design gallery shell catalog sample card 设计 目录 示例 卡片");
        AddPage(text, "settings", "Settings", "设置", "Theme, accent, language, and Gallery diagnostics exposed as a footer navigation entry.", "作为页脚导航入口公开的主题、强调色、语言和 Gallery 诊断。", "settings theme accent language diagnostics 设置 主题 强调色 语言 诊断");
        AddPage(text, "statematrix", "State Matrix", "状态矩阵", "Cross-control normal, selected, disabled, and flyout state checks.", "跨控件的正常、选中、禁用和弹出状态检查。", "state matrix normal selected disabled 状态 矩阵 正常 选中 禁用");

        Add("settings.language.title", "Language", "语言");
        Add("settings.language.description", "Switch Gallery navigation, search, metadata, and sample labels between English and Simplified Chinese.", "在英文和简体中文之间切换 Gallery 导航、搜索、元数据和示例标签。");
        Add("settings.language.current", "Current language: {0}.", "当前语言：{0}。");
        Add("settings.language.code", "GalleryLocalizationService.SetLanguage(\"zh-Hans\");", "GalleryLocalizationService.SetLanguage(\"zh-Hans\");");

        return text;
    }

    private static void AddPage(Dictionary<string, Dictionary<string, string>> text, string id, string englishTitle, string chineseTitle, string englishDescription, string chineseDescription, string keywords)
    {
        text[$"page.{id}.title"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [EnglishCultureName] = englishTitle,
            [ChineseCultureName] = chineseTitle
        };
        text[$"page.{id}.description"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [EnglishCultureName] = englishDescription,
            [ChineseCultureName] = chineseDescription
        };
        text[$"page.{id}.keywords"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [EnglishCultureName] = keywords,
            [ChineseCultureName] = keywords
        };
    }
}
