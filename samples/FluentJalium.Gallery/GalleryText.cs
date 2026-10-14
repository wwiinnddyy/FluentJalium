using FluentJalium.Localization;

namespace FluentJalium.Gallery;

/// <summary>
/// Every user-visible string the Gallery shows, in both languages.
/// </summary>
/// <remarks>
/// <para>
/// The English half of each pair is the literal the page already declares - in <c>MainWindow.jalxaml</c> for markup
/// text, in <c>MainWindow.jalxaml.cs</c> for the report lines and the generated menu, command bar and tab labels.
/// <see cref="TextSweeper"/> matches on that literal, so this file is the translation index rather than a second
/// copy of the copy: a string that leaves the page without leaving here shows up in
/// <see cref="UiText.MissingChinese"/> and fails the coverage test.
/// </para>
/// <para>
/// Technical identifiers stay in English on purpose - token names, control type names, resource keys, enum members,
/// file paths and version numbers are the vocabulary the rest of the documentation is written in, and translating
/// them would break every cross-reference in <c>docs/astra</c>. The prose around them is translated.
/// </para>
/// <para>
/// A string with no letters in it is an icon code point rather than prose, and the coverage test skips those.
/// </para>
/// </remarks>
internal static class GalleryText
{
    /// <summary>Registers every Gallery string. Idempotent.</summary>
    public static void Register()
    {
        RegisterShell();
        RegisterOverview();
        RegisterTokens();
        RegisterMaterials();
        RegisterMotion();
        RegisterButtons();
        RegisterInputs();
        RegisterSelection();
        RegisterNavigation();
        RegisterSurfaces();
        RegisterStatus();
        RegisterMenus();
        RegisterCommandBar();
        RegisterSettings();
        RegisterRuntimeText();
    }

    private static void RegisterShell()
    {
        Add("FluentJalium  /  Gallery", "FluentJalium  /  画廊");
        Add("{0} · {1}", "{0} · {1}");
        Add("Explore familiar Windows controls, built on Jalium.", "探索熟悉的 Windows 控件，构建于 Jalium 之上。");
    }

    private static void RegisterOverview()
    {
        Add("FluentJalium", "FluentJalium");
        Add("Astra", "Astra");
        Add("A shared design language", "一套共享的设计语言");
        Add("Try the controls, navigate with a keyboard, and switch themes while your input stays in place.",
            "试用这些控件、用键盘导航，并在切换主题时保持输入状态不变。");
        Add("Start exploring", "开始探索");
        Add("Buttons and selection", "按钮与选择");
        Add("Pointer, pressed, disabled, selected, and focus states.",
            "指针、按下、禁用、选中与焦点状态。");
        Add("Navigation that follows you", "随你而动的导航");
        Add("Resize the window and watch the shared selection indicator.", "调整窗口尺寸，观察共享的选中指示器。");
    }

    private static void RegisterTokens()
    {
        Add("Tokens", "令牌");
        Add(
            "Every fill this gallery paints comes from one palette of 101 brush tokens, and this page is the whole set at once: " +
            "the swatches are built from the canonical key inventory, so a token that no longer resolves shows up as a hole in " +
            "the grid and is named in the readout below.",
            "本画廊绘制的每一种填充都来自同一套包含 101 个画刷令牌的调色板，本页一次性展示全部：色块依据规范键清单构建，" +
            "因此任何无法解析的令牌都会在网格中留下一个空洞，并在下方读数中列名。");
        Add(
            "Switch the theme on the Settings page and come back: the fills move because each name resolves to one instance " +
            "the facade re-tints in place, which is also why the aliases the framework's own dictionaries read follow the same switch.",
            "在设置页切换主题后再回来：填充之所以会变化，是因为每个名称都解析到同一个实例，由外观层就地重新着色；" +
            "这也正是框架自身字典所读取的别名会随之切换的原因。");
        Add(
            "{0} palette tokens resolved, {1} painted, none missing.",
            "{0} 个调色板令牌已解析，{1} 个已绘制，无缺失。");
        Add("{0} of {1} tokens painted; unresolved: {2}", "已绘制 {1} 个令牌中的 {0} 个；未解析：{2}");
    }

    private static void RegisterMaterials()
    {
        Add("Materials", "材质");
        Add(
            "What this runtime can actually compose, shown as nested fills rather than as a list of names: an alpha layer is only " +
            "a layer because of what sits behind it, so the ramp below paints the base, then the layer over it, then the card " +
            "over that, and the stroke on the card is the same alpha token doing its work against two different backgrounds.",
            "这是本运行时真正能够合成的效果，以嵌套填充而非名称清单来呈现：一层带透明度的填充之所以是「层」，正因为它背后有东西。" +
            "因此下面的色阶先画基底，再在其上画该层，再在层上画卡片，而卡片上的描边是同一个 alpha 令牌在两种不同背景上各工作一次。");
        Add("SolidBackgroundFillColorBaseBrush  →  LayerFillColorDefaultBrush (#80FFFFFF)",
            "SolidBackgroundFillColorBaseBrush  →  LayerFillColorDefaultBrush（#80FFFFFF）");
        Add("CardBackgroundFillColorDefaultBrush (#B3FFFFFF) with CardStrokeColorDefaultBrush (#0F000000) on its edge",
            "CardBackgroundFillColorDefaultBrush（#B3FFFFFF），边缘为 CardStrokeColorDefaultBrush（#0F000000）");
        Add(
            "The stroke reads darker over the base than over the card, which is the point: an alpha stroke has no single colour, " +
            "and that is why the ramp has to be nested to be read at all.",
            "描边在基底上比在卡片上显得更深，而这正是重点：带透明度的描边没有单一颜色，也正因如此这个色阶必须嵌套起来才能读懂。");
        Add("Where a floating surface cannot composite", "浮动表面无法合成之处");
        Add(
            "Element-level material is not available on 26.10.9: the runtime ships no AcrylicBrush, MicaBrush or RevealBrush " +
            "type at all (the shipped-type inventory in docs/astra/adaptation/s0v-runtime-type-inventory-raw.txt), so a floating " +
            "surface here is a solid fill plus an alpha stroke, and CardStrokeColorDefaultSolidBrush is the #EBEBEB twin used " +
            "where a border cannot composite over what it floats on. Window-level backdrop is a different story and is real - " +
            "WindowBackdropType = None / Auto / Mica / Acrylic / MicaAlt, measured in " +
            "docs/astra/adaptation/01-jalium-control-census.md:146 - which is why the switch for it sits on the Settings page, " +
            "over the one surface that can actually take it.",
            "26.10.9 不提供元素级材质：运行时根本没有 AcrylicBrush、MicaBrush 或 RevealBrush 这三个类型" +
            "（随包类型清单见 docs/astra/adaptation/s0v-runtime-type-inventory-raw.txt），因此这里的浮动表面是实色填充加一层 " +
            "alpha 描边；CardStrokeColorDefaultSolidBrush 则是 #EBEBEB 的孪生项，用于描边无法与其所浮于其上的内容合成之处。" +
            "窗口级背景则是另一回事，而且真实可用——WindowBackdropType = None / Auto / Mica / Acrylic / MicaAlt，" +
            "实测记录于 docs/astra/adaptation/01-jalium-control-census.md:146——这正是它的开关被放在设置页、压在唯一真正" +
            "能承载它的表面之上的原因。");
    }

    private static void RegisterMotion()
    {
        Add("Motion", "动效");
        Add("Beat", "触发");
        Add(
            "Three durations are published as resource rows, and this page runs them side by side on the one property shape this " +
            "runtime actually animates: a Double dependency property on the element itself.",
            "三个时长以资源行的形式发布，本页把它们并排跑在本运行时真正会做动画的那一种属性形态上：元素自身的 Double 依赖属性。");
        Add("ControlFasterAnimationDuration", "ControlFasterAnimationDuration");
        Add("ControlFastAnimationDuration", "ControlFastAnimationDuration");
        Add("SplitViewPaneAnimationOpenDuration", "SplitViewPaneAnimationOpenDuration");
        Add("{0} = {1}", "{0} = {1}");
        Add("{0} unresolved", "{0} 未解析");
        Add("reduce motion on", "减少动效：开");
        Add("reduce motion off", "减少动效：关");
        Add(
            "The bars move their Width rather than a transform because a transform never ticks on this runtime, and the row " +
            "stays a Duration so a theme flip re-resolves it; the reduce-motion switch is on the Settings page and is " +
            "deliberately not duplicated here, since it rewrites the live row for the whole application and two switches over " +
            "one global would only be able to disagree.",
            "这些条移动的是 Width 而不是变换，因为本运行时的变换根本不会 tick；而该资源行保持为 Duration，主题切换时会重新解析。" +
            "减少动效开关放在设置页，这里刻意不重复：它会为整个应用重写活动资源行，同一个全局量上挂两个开关只会互相矛盾。");
    }

    private static void RegisterButtons()
    {
        Add("Buttons", "按钮");
        Add("Choose the emphasis that matches an action.", "选择与动作相称的强调级别。");
        Add("Emphasis", "强调");
        Add("Click a button to see its action in the live output below.", "点击按钮，动作会显示在下方实时输出中。");
        Add("Standard", "标准");
        Add("Accent", "强调");
        Add("Subtle", "轻淡");
        Add("Disabled", "禁用");
        Add("The rest of the family", "这一族的其余成员");
        Add("Toggle", "切换");
        Add("Checked", "选中");
        Add("Three state", "三态");
        Add("Click, drag, or press Space to toggle.", "点击、拖动或按空格键切换。");
        Add("Hover, press and the drag-select gesture are not claimed: the row states here are driven by selection in a test, " +
            "never by a pointer.",
            "悬停、按下与拖选手势不在声明范围内：本页的行状态由测试中的选择驱动，而非由指针驱动。");
        Add("Repeat", "重复");
        Add("Hold the repeat button to see it fire.", "按住重复按钮看它连续触发。");
        Add("Repeat button fired {0} times.", "重复按钮已触发 {0} 次。");
        Add("Split", "拆分");
        Add("More", "更多");
        Add("Left half acts, right half opens.", "左半部分执行，右半部分展开。");
        Add("Split primary action ran {0} time(s).", "拆分按钮的主操作已执行 {0} 次。");
        Add("Split flyout opened - the left half did not run.", "拆分按钮的浮出层已展开——左半部分未执行。");
        Add("Split flyout closed.", "拆分按钮的浮出层已关闭。");
        Add("Drop-down opened; IsExpanded={0}.", "下拉已展开；IsExpanded={0}。");
        Add("Drop-down closed; IsExpanded={0}.", "下拉已关闭；IsExpanded={0}。");
        Add("Disabled split", "禁用的拆分按钮");
        Add("Disabled text field", "禁用的文本框");
        Add("This field is disabled", "此字段已禁用");
        Add("Use Tab to move between buttons and Space or Enter to activate the focused action.",
            "用 Tab 在按钮间移动，用空格或 Enter 激活当前焦点上的动作。");
        Add("Navigation that follows you", "随你而动的导航");
        Add("Hyperlink", "超链接");
        Add("Keyboard", "键盘");
        Add("{0} button activated. Total actions: {1}.", "「{0}」按钮已激活。累计操作：{1} 次。");
    }

    private static void RegisterInputs()
    {
        Add("Inputs", "输入");
        Add("Edit text, choose a value, and adjust native controls.", "编辑文本、选择数值，并调整原生控件。");
        Add("Display name", "显示名称");
        Add("Astra", "Astra");
        Add("You can press Enter here", "在此处按 Enter 即可");
        Add("Only the number of password characters is shown in the output.", "输出中只显示密码字符的数量。");
        Add("Quantity", "数量");
        Add("Disabled fruit box", "禁用的联想框");
        Add("Blueberry", "蓝莓");
        Add("Number boxes", "数字框");
        Add("Compact spinners", "紧凑的微调按钮");
        Add("Tick marks and a disabled slider", "刻度与禁用的滑块");
        Add(
            "These four wear the implicit NumberBox style: the surface, border and spinner fills come from the transcribed " +
            "TextControl* and NumberBox* rows, and the spinner popup in the compact box opens when the control takes keyboard focus.",
            "这四个控件使用隐式 NumberBox 样式：表面、边框与微调按钮的填充来自转录的 TextControl* 与 NumberBox* 资源行；" +
            "紧凑框中的微调浮层会在控件获得键盘焦点时展开。");
        Add("Suggested text", "建议文本");
        Add("Fruit", "水果");
        Add("Focus and selection", "焦点与选择");
        Add("Focused", "已聚焦");
        Add(
            "Not claimed here: the spinner border is the framework's own local value, the header text keeps a local colour the " +
            "tokens cannot reach, and nothing on this page was measured on screen.",
            "此处不作声明：微调按钮的边框是框架自身的局部值，标题文本保留着令牌无法触及的局部颜色，" +
            "并且本页没有任何内容是在屏幕上实测的。");
        Add("Independent options, a single choice, and a list.", "多个独立选项、单选，以及一个列表。");
        Add("Check boxes", "复选框");
        Add("Show a preview", "显示预览");
        Add("Include details", "包含详细信息");
        Add("Three-state option", "三态选项");
        Add("Unchecked", "未选中");
        Add("Radio buttons", "单选按钮");
        Add("Comfortable", "宽松");
        Add("Compact", "紧凑");
        Add("Radio button list", "单选按钮列表");
        Add("Transport", "出行方式");
        Add("A header, generated radios, and a column count the layout reads back off the control: nothing here sets a style on the rows.",
            "一个标题、由容器生成的单选按钮，以及一个布局会从控件上读回的列数：本页没有为任何行设置样式。");
        Add("Two columns", "两列");
        Add("Train", "火车");
        Add("Ferry", "轮渡");
        Add("Tram", "有轨电车");
        Add("Cycle", "自行车");
        Add("On foot", "步行");
        Add("Bus", "公交车");
        Add("Radio list choice: {0}.", "单选列表选择：{0}。");
        Add("Radio list choice: none.", "单选列表选择：无。");
        Add("Combo boxes", "组合框");
        Add("Window side", "靠窗一侧");
        Add("Aisle side", "靠走道一侧");
        Add("Left", "左侧");
        Add("Right", "右侧");
        Add("Note", "备注");
        Add(
            "Nothing here names a style: each combo is wearing the implicit one, and each surface colour comes from upstream's " +
            "ComboBox* rows.",
            "本页没有任何一处指定样式：每个组合框都穿着隐式样式，每种表面颜色都来自上游的 ComboBox* 资源行。");
        Add("Apply values", "应用数值");
        Add("Reset", "重置");
        Add("Input values reset.", "输入数值已重置。");
        Add("Density choice: Comfortable.", "密度选择：宽松。");
        Add("Density choice: Compact.", "密度选择：紧凑。");
    }

    private static void RegisterSelection()
    {
        Add("Selection", "选择");
        Add("List selection", "列表选择");
        Add("First", "第一项");
        Add("Second", "第二项");
        Add("Third", "第三项");
        Add("List selection: {0}.", "列表选择：{0}。");
        Add("List view", "列表视图");
        Add("List view selection: {0}.", "列表视图选择：{0}。");
        Add("Go to Overview", "转到概览");
        Add("Go to Settings", "转到设置");
        Add("Toggle pane", "切换窗格");
        Add("Navigation pane expanded.", "导航窗格已展开。");
        Add("Navigation pane compact.", "导航窗格已收起。");
        Add("Tab view", "选项卡视图");
        Add("Add button", "添加按钮");
        Add("Select first", "选中第一个");
        Add("Document", "文档");
        Add("Icon", "图标");
        Add("No close", "不可关闭");
        Add("Disabled", "禁用");
        Add("A disabled tab keeps the resting fill and loses only its label.", "禁用的选项卡保留静止填充，只丢失标签。");
        Add("New tab", "新建标签页");
        Add("Tab {0}", "标签页 {0}");
        Add("Added by the + button as {0}.", "由 + 按钮添加，序号 {0}。");
        Add("{0} closed; {1} left.", "「{0}」已关闭；剩余 {1} 个。");
        Add("{0} selected.", "已选中「{0}」。");
        Add("Nothing selected.", "未选中任何项。");
        Add("Document selected.", "已选中「文档」。");
        Add("Add button visible", "显示添加按钮");
        Add(
            "Clicking a tab selects it and Ctrl+Tab moves the selection; the arrow keys only move focus, because upstream sets " +
            "SingleSelectionFollowsFocus=False on the strip. Closing a tab raises TabCloseRequested and removes nothing until this " +
            "page asks for it.",
            "点击选项卡即选中，Ctrl+Tab 移动选中项；方向键只移动焦点，因为上游在选项卡条上设置了 " +
            "SingleSelectionFollowsFocus=False。关闭选项卡会引发 TabCloseRequested，但在本页要求之前不会移除任何内容。");
        Add("Pips pager", "页码点导航器");
        Add("Vertical", "垂直");
        Add("Hide the arrows", "隐藏箭头");
        Add("Next page", "下一页");
        Add("Page 1.", "第 1 页。");
        Add("Page {0} of {1}; {2} pips generated, {3} of them in the window.",
            "第 {1} 页，共 {0} 页；已生成 {2} 个页码点，其中 {3} 个在窗口内。");
        Add(
            "Ten pages in a five-pip window: the row generates all ten pips and the viewport is clamped to what MaxVisiblePips " +
            "allows, which is upstream's rule rather than a paging one. The arrows move a single page and disable themselves at " +
            "the ends; a pip is chosen by clicking it, and the arrow keys only move focus. The readout is built from the control's " +
            "own values, because this markup parser drops an attribute it does not recognise instead of rejecting it.",
            "十页内容放在五点的窗口里：这一行会生成全部十个页码点，而视口被 MaxVisiblePips 允许的范围裁剪，" +
            "这是上游的规则而非分页规则。箭头每次移动一页，并在两端自行禁用；页码点通过点击选中，方向键只移动焦点。" +
            "读数由控件自身的值构建，因为本标记解析器会丢弃无法识别的属性而不是报错。");
    }

    private static void RegisterNavigation()
    {
        Add("Navigation", "导航");
        Add("Adaptive pane", "自适应窗格");
        Add("The Gallery sidebar is a FluentNavigationView.", "本画廊的侧边栏是一个 FluentNavigationView。");
        Add("One moving indicator", "一个会移动的指示器");
        Add(
            "Move between menu and footer destinations to see the indicator stretch and settle. Rapid selection changes retarget " +
            "the visible bar.",
            "在菜单区与页脚区的目标之间移动，观察指示器伸缩并落定。快速切换选择会重新定位可见的指示条。");
        Add(
            "The icons, labels, 68 DIP width, the 64 DIP cell with its icon 16 below the top edge, the highlight inset 2,6,2,6 and " +
            "the accent fill of the checked toggle are ours: the framework's own skin measured 44 wide with a 10 pixel label, a " +
            "20 DIP icon box and a purple foreground. That highlight carries no brush fade on purpose - a nested transitioning " +
            "border kept its accent in every property and offscreen read yet painted nothing in the composited window. The bar's " +
            "own surface and its overflow list are the honest gap - that control stores a Template and never builds one, so only " +
            "the rows its code reads can be steered, and the overflow row was measured back out of the publication list because " +
            "no pixel of it moved.",
            "图标、标签、68 DIP 宽度、64 DIP 的单元格（其图标距顶边 16 DIP）、2,6,2,6 的高亮内缩，以及选中切换项的强调色填充，" +
            "都属于我们：框架自带的皮肤实测为 44 宽、10 像素标签、20 DIP 图标框和紫色前景。那处高亮刻意不带画刷渐变——" +
            "一个嵌套的过渡边框在每个属性和离屏读数里都保有其强调色，却在合成窗口中什么也没画出来。" +
            "导航条自身的表面与它的溢出列表是诚实的缺口——该控件只保存 Template 而从不构建，因此只有其代码会读取的行才能被引导；" +
            "而溢出行因没有任何一个像素发生移动，已被实测从发布清单中剔除。");
        Add("The selection marker stays with the active page until a destination is activated.",
            "在某个目标被激活之前，选中标记会一直留在当前页面上。");
        Add(
            "Resize below 800 DIPs or use the pane toggle. Settings rows also stack when space is limited.",
            "把窗口缩到 800 DIP 以下，或使用窗格切换按钮。设置项在空间不足时也会改为堆叠。");
    }

    private static void RegisterSurfaces()
    {
        Add("Surfaces", "表面");
        Add("Base surfaces", "基础表面");
        Add("WinUI 3 styling · Jalium layout, input, and rendering", "WinUI 3 样式 · Jalium 的布局、输入与渲染");
        Add("Cards", "卡片");
        Add("WinUI 3 has no Card control - no runtime class, no template, no theme dictionary - and its Gallery composes cards " +
            "from a Border, the stroke row and a background tier. That recipe is what this page already uses for every card on it, " +
            "so the three tiers below are the same three rows the rest of the page paints with.",
            "WinUI 3 没有 Card 控件——没有运行时类、没有模板、没有主题字典——它的 Gallery 用一个 Border、一条描边资源行和一级背景" +
            "来组合卡片。本页的每一张卡片已经在用这个配方，因此下面三层就是页面其余部分一直在用的那三行。");
        Add(
            "CardBorderStyle carries the recipe: the default fill, the stroke row at 1 DIP, ControlCornerRadius and 16 DIP of " +
            "padding. Upstream's sample card rounds at 8 and its tiles at 4; this page keeps 4 on its section containers, so the " +
            "corner claim is deliberately the tile row rather than the CardPage row.",
            "CardBorderStyle 承载着这个配方：默认填充、1 DIP 的描边行、ControlCornerRadius 以及 16 DIP 的内边距。" +
            "上游的示例卡片圆角为 8、其磁贴为 4；本页的分节容器保持为 4，因此圆角的声明刻意针对磁贴行而非 CardPage 行。");
        Add("Two containers whose look comes from the theme kernel: an expander the control operates for you, and an info bar " +
            "that had to become an own type before any of its colours could be themed.",
            "两个外观来自主题内核的容器：一个由控件代你操作的展开器，以及一个必须先变成自有类型、其颜色才能被主题化的信息栏。");
        Add("Expander", "展开器");
        Add("Weekly digest", "每周摘要");
        Add("The body is any content at all. The control writes the content part's visibility and rotates the chevron, so nothing " +
            "on this page animates either of them.",
            "主体可以是任意内容。控件会写入内容部件的可见性并旋转箭头，因此本页没有任何代码去动效这两者。");
        Add("State inside the body survives a collapse", "主体内的状态在折叠后依然保留");
        Add("Collapsed.", "已折叠。");
        Add(
            "Expanded. The control wrote the content part's visibility and rotated its chevron; this page drives neither.",
            "已展开。控件写入了内容部件的可见性并旋转了箭头；本页没有驱动其中任何一项。");
        Add(
            "Click the header, or Tab to the expander and press Space. The expander itself is the focusable element: the framework " +
            "installs its own click handler on the header part, so a toggle button in that slot would answer one click twice.",
            "点击标题，或用 Tab 聚焦到展开器后按空格。展开器自身才是可获焦的元素：框架会在标题部件上安装自己的点击处理器，" +
            "因此若在该位置放一个切换按钮，同一次点击会被响应两次。");
        Add(
            "The chevron runs on a two-way binding to IsExpanded, because a part named Expander hooks nothing on this runtime. " +
            "The subtree's fill is 16 DIP narrower than its parent's - the cost of an indent built by nesting rather than by a " +
            "depth value, which this runtime does not expose.",
            "箭头通过对 IsExpanded 的双向绑定来驱动，因为本运行时上名为 Expander 的部件不会挂接任何东西。" +
            "子树中的填充比父级窄 16 DIP——这是用嵌套而非深度值来构造缩进的代价，而本运行时并不暴露深度值。");
        Add("Info bar", "信息栏");
        Add("Weekly digest ready", "每周摘要已就绪");
        Add("Nine new items are waiting. Closing this bar raises CloseButtonClick, which is the base class's own event.",
            "有九条新内容在等待。关闭此栏会引发 CloseButtonClick，那是基类自身的事件。");
        Add("Informational", "信息");
        Add("Success", "成功");
        Add("Warning", "警告");
        Add("Error", "错误");
        Add("Show again", "再次显示");
        Add("Closable", "可关闭");
        Add("Nothing closed yet.", "尚未关闭任何内容。");
        Add("IsOpen=true again.", "IsOpen 已重新置为 true。");
        Add("Info bar closed.", "信息栏已关闭。");
        Add("Severity {0}: the fill is upstream's severity row.", "严重级别 {0}：其填充来自上游的严重级别资源行。");
        Add(
            "CloseButtonClick came from the base class, which also set IsOpen=false; the style collapses the bar.",
            "CloseButtonClick 来自基类，基类同时把 IsOpen 置为 false；样式负责收起该栏。");
        Add(
            "Every fill here is an upstream row: the four severity backgrounds alias the system fill colours, so severity, icon disc, " +
            "border and both text colours follow the theme. The icon is a 16 DIP disc carrying the upstream glyph and the close " +
            "button is a Path rather than a SymbolIcon.",
            "这里每一种填充都来自上游资源行：四种严重级别背景都是系统填充颜色的别名，因此严重级别、图标圆盘、边框与两种文字颜色" +
            "都会跟随主题。图标是一个承载上游字形的 16 DIP 圆盘，关闭按钮则是一个 Path 而非 SymbolIcon。");
        Add("Content dialog", "内容对话框");
        Add("Three buttons", "三个按钮");
        Add("Two buttons", "两个按钮");
        Add("Only Cancel", "仅取消");
        Add("Default is primary", "默认项为主操作");
        Add("Full size", "全尺寸");
        Add("Nothing shown yet.", "尚未显示任何内容。");
        Add(
            "This card holds no dialog element on purpose: 26.10.9 hosts a shown ContentDialog in the window's own overlay layer " +
            "and refuses to open one that is already placed in a tree, so the dialog is built on the click. Under the Astra template " +
            "it is a smoke layer, a 24 DIP gutter, a card capped at 548 with the overlay corner radius, and a 20 SemiBold title over " +
            "the body. The control owns the card's width and maps the app's own MaxWidth onto it, so the template's cap is the " +
            "resting value rather than a limit an app cannot pass. The title strip and the command row each carry their own half " +
            "of that 8 DIP arc: a border here rounds what it fills and never clips a child, so a square band running to the card's " +
            "edge paints the corner back on.",
            "这张卡片刻意不放对话框元素：26.10.9 把已显示的 ContentDialog 承载在窗口自己的叠加层中，并拒绝打开已经放在树里的实例，" +
            "因此对话框是在点击时构建的。在 Astra 模板下，它是一层薄雾、24 DIP 的留白、一张上限为 548 且带叠加层圆角的卡片，" +
            "以及主体上方 20 号 SemiBold 的标题。控件掌握卡片的宽度，并把应用自己的 MaxWidth 映射到它上面，" +
            "因此模板的上限只是静止值，而不是应用无法突破的限制。标题条与命令行各承担那段 8 DIP 圆弧的一半：这里的 Border " +
            "只会给它所填充的区域倒圆角、从不裁剪子元素，因此一条延伸到卡片边缘的方带会把圆角重新画回来。");
        Add("Dialog closed: {0}.", "对话框已关闭：{0}。");
        Add("ShowAsync completed with {0}; the click that ended it came from the template's own part.",
            "ShowAsync 以 {0} 完成；结束它的点击来自模板自身的部件。");
        Add("Teaching tip", "教学提示");
        Add("Show the tip", "显示提示");
        Add("Point from the right", "从右侧指出");
        Add("Point at nothing", "指向空处");
        Add("Tail", "尖角");
        Add("Both buttons", "两个按钮");
        Add("Not shown.", "未显示。");
        Add("Upstream draws this card in code; Astra writes it as a template over a Popup, and the control only decides which edge " +
            "the tail sits on.",
            "上游用代码绘制这张卡片；Astra 把它写成覆盖在 Popup 之上的模板，控件只决定尖角落在哪条边上。");
        Add(
            "The card is 320 DIP wide at rest and capped at 336 like upstream's, and the 8 DIP band the tail grid reserves outside " +
            "it is what lets the control put the tail's point exactly on the anchor's edge: a tip placed above its target sits its " +
            "container's height away, not that height minus eight. The border loses whichever edge the tail sits on, so the seam " +
            "under the triangle is the fill rather than a hairline crossing it. Preferred sides that would push the card out of the " +
            "window fall back to one that fits, and the tail moves with the result.",
            "卡片静止时宽 320 DIP，上限与上游同为 336；尖角网格在其外侧预留的 8 DIP 带，正是让控件能把尖角尖端准确落在锚点边缘的余量：" +
            "置于目标上方的提示会离其容器高度那么远，而不是高度减八。尖角所在的那条边会失去边框，因此三角形下方的接缝是填充色，" +
            "而不是横贯其上的发丝线。若首选方向会把卡片挤出窗口，则回退到一个放得下的方向，尖角也随之移动。");
        Add("Divider", "分隔线");
        Add("Two lines of type, one rule between them.", "两行文字，中间一条分隔线。");
        Add(
            "WinUI has no Divider control at the commit this rewrite is pinned to, so the look here is built from the two things " +
            "that commit does carry: the DividerStrokeColorDefault token and the 1 DIP thickness every divider-shaped template in " +
            "it shares. The box the line sits in - 12 DIP across on the running axis - is the runtime's own measurement, not an " +
            "upstream number, and the line is drawn by the control itself because a 1 DIP Rectangle prints nothing on this runtime " +
            "at all.",
            "在本次重写所固定的提交上，WinUI 并没有 Divider 控件，因此这里的外观由该提交确实具备的两样东西构建：" +
            "DividerStrokeColorDefault 令牌，以及其中每个分隔线形状模板共用的 1 DIP 粗细。线条所在的盒子——沿走向 12 DIP——" +
            "是运行时自身的测量值而非上游数字；线条由控件自行绘制，因为 1 DIP 的 Rectangle 在本运行时上根本画不出任何东西。");
        Add("The rule is the runtime's own Separator, carrying the palette's divider token at 1 DIP.",
            "这条线就是运行时自带的 Separator，以 1 DIP 承载调色板的分隔线令牌。");
    }

    private static void RegisterStatus()
    {
        Add("Status", "状态");
        Add("Progress bar", "进度条");
        Add("Progress ring", "进度环");
        Add("Ring: 40 of 100.", "进度环：100 中的 40。");
        Add("40 of 100.", "100 中的 40。");
        Add("Indeterminate", "不确定");
        Add("Progress: indeterminate.", "进度：不确定。");
        Add("Progress: determinate.", "进度：确定。");
        Add("Indeterminate: a third of the row, and it stays put.", "不确定：占三分之一宽度，并保持不动。");
        Add("{0} of {1}.", "{1} 中的 {0}。");
        Add("Ring: indeterminate, a half circle carried round by a frame loop.", "进度环：不确定，由帧循环推动的半圆。");
        Add("Ring: {0} of {1}, an arc of {2} degrees.", "进度环：{1} 中的 {0}，圆弧 {2} 度。");
        Add(
            "The filled bar is three DIP tall and the rail behind it one, which is upstream's own geometry; dragging the slider " +
            "moves the end of the fill because the control sizes that part and the template only hands it a band to land in. " +
            "Checking indeterminate is honest about this runtime: the block stops moving, because the slide upstream animates " +
            "has no route here.",
            "已填充的条高 3 DIP，其后的轨道高 1 DIP，这是上游自身的几何；拖动滑块会移动填充的末端，因为控件决定该部件的尺寸，" +
            "模板只是给它一条可以落脚的带。勾选「不确定」对本运行时是诚实的：滑块会停止移动，因为上游给的那段滑动在这里没有通路。");
        Add(
            "The first slider above carries no Style attribute, so it is the implicit Slider style doing the work: the thumb body " +
            "is upstream 18x18 with its ring 2 wider on every side, and the track is inset by half a thumb because this framework " +
            "moves the thumb by its left edge, which leaves the ring 2 DIP past the control at maximum.",
            "上面第一个滑块没有 Style 属性，因此起作用的是隐式 Slider 样式：滑块主体是上游的 18x18，其描边环每边宽出 2；" +
            "轨道内缩半个滑块，因为本框架按滑块的左边缘移动它，这会让描边环在最大值时越出控件 2 DIP。");
        Add(
            "This runtime has no ProgressRing and no animated-visual player, so the ring is a type of ours that draws its own arc: " +
            "radius and stroke come out of the two Lottie canvases upstream ships (they differ, and the ring keeps that), a " +
            "determinate arc is regenerated whenever the value moves, and the spin is a frame loop writing the arc's start angle " +
            "- an animation clock never reaches a transform here. The second ring is indeterminate, the third is the same " +
            "control at 64 DIP, and the fourth is inactive: it holds its place and paints nothing.",
            "本运行时既没有 ProgressRing，也没有动画视觉播放器，因此进度环是一个自绘圆弧的自有类型：半径与描边取自上游随附的两张" +
            "Lottie 画布（两者并不一致，进度环保留了这种差异），确定型圆弧会在数值变化时重新生成，旋转则由一个帧循环改写圆弧" +
            "起始角——动画时钟在这里永远到不了变换。第二个环是不确定型，第三个是同一控件的 64 DIP 版本，第四个未激活：" +
            "它保留位置但不绘制任何东西。");
        Add("InfoBadge", "信息徽章");
        Add("Balanced", "平衡");
        Add("In review", "评审中");
        Add("In progress", "进行中");
        Add("Blocked", "已阻塞");
        Add("Ready to ship", "可以发布");
        Add(
            "The runtime has no InfoBadge either, so this is our type: the four display kinds come from Value and Icon exactly " +
            "as upstream's C++ walks them, the pill radius is half the measured height, and the five severities are the sixteen " +
            "style keys transcribed verbatim. Two of them show a number - which is the honest limit of this card: a glyph run " +
            "never reaches a capture on this runtime, so the digits are asserted in the tree (text, visibility, foreground) and " +
            "not in the picture.",
            "运行时也没有 InfoBadge，因此这是我们的类型：四种展示形态完全按上游 C++ 的遍历方式来自 Value 与 Icon，" +
            "胶囊圆角是实测高度的一半，五种严重级别则逐字转录自十六个样式键。其中两个显示数字——这正是这张卡片诚实的上限：" +
            "字形串在本运行时上永远到不了截图，因此数字是在树中断言的（文本、可见性、前景色），而不是在画面中断言的。");
        Add("Rating control", "评分控件");
        Add("Retro", "复古");
        Add("Relaxed", "放松");
        Add(
            "The runtime has no RatingControl, so this is our type. A star is drawn at upstream's 32 and shown at half, which is " +
            "the resting value of the composition scale upstream puts on every item - this runtime gives an item no visual to " +
            "animate, so the same 0.5 and the same quadratic lift toward 0.8 under a mouse (1.0 under a finger) are evaluated " +
            "in code. Half stars are cut by a ClipToBounds host rather than upstream's Clip, which paints nothing here. What this " +
            "card cannot show is the ink of the star itself: a glyph run reaches no capture on this runtime, so the size, the pitch " +
            "and the crop are asserted in the tree - and the row really is 17 wide per star at a pitch of 25, which is the number " +
            "the pointer maths divide by.",
            "运行时没有 RatingControl，因此这是我们的类型。星形按上游的 32 绘制，并以一半显示——这正是上游施加在每个项上的" +
            "合成比例的静止值。本运行时不给任何项可用于做动效的视觉，因此同样的 0.5、以及鼠标下同样的二次曲线抬升至 0.8" +
            "（手指下为 1.0）都在代码中求值。半星由一个 ClipToBounds 宿主裁切，而不是上游那个在此处什么都不画的 Clip。" +
            "这张卡片无法展示的是星形本身的墨迹：字形串在本运行时上到不了截图，因此尺寸、间距与裁切都在树中断言——" +
            "而且这一行确实每颗星宽 17、间距 25，这正是指针运算所除的数。");
    }

    private static void RegisterMenus()
    {
        Add("Menus", "菜单");
        Add("A bar with commands", "带命令的菜单栏");
        Add("View", "视图");
        Add("Help", "帮助");
        Add("No bar item opened yet.", "尚未展开任何菜单栏项。");
        Add("Flyout and context menu", "浮出层与上下文菜单");
        Add("Open the flyout", "打开浮出层");
        Add("Open the context menu", "打开上下文菜单");
        Add("Nothing chosen yet.", "尚未选择任何项。");
        Add("Flyout closed.", "浮出层已关闭。");
        Add("Rename", "重命名");
        Add("Archive", "归档");
        Add("Show read receipts", "显示已读回执");
        Add("Share with", "共享给");
        Add("Team channel", "团队频道");
        Add("Direct link", "直接链接");
        Add("Delete", "删除");
        Add("Copy path", "复制路径");
        Add("Open containing folder", "打开所在文件夹");
        Add("Properties", "属性");
        Add("{0} clicked - the label and the disabled colour are ours; the highlight and the check mark are the control's own drawing.",
            "已点击「{0}」——标签与禁用色是我们的；高亮与勾选标记是控件自身的绘制。");
        Add(
            "The flyout is built in code because 26.10.9 gives a plain Button no Flyout property: MenuFlyout.ShowAt(target) is the " +
            "door. Its items carry every state this runtime can reach - hover, disable, checked - and no others, so the pressed and " +
            "submenu-opened rows upstream publishes are not published here either.",
            "浮出层用代码构建，因为 26.10.9 没有给普通 Button 提供 Flyout 属性：MenuFlyout.ShowAt(target) 才是入口。" +
            "它的条目只承载本运行时能到达的状态——悬停、禁用、选中——没有别的，因此上游发布的按下态与子菜单展开态在这里也没有发布。");
        Add(
            "Both bars read MenuBar's rows for their surface and label. The items inside the first one are the honest gap: a " +
            "MenuItem stores a Template and never builds one, so its colours come from a style while its highlight, check mark and " +
            "submenu arrow stay the framework's own drawing.",
            "两条菜单栏的表面与标签都取自 MenuBar 的资源行。第一条里的条目则是诚实的缺口：MenuItem 只保存 Template 而从不构建，" +
            "因此它的颜色来自样式，而高亮、勾选标记与子菜单箭头仍是框架自身的绘制。");
        Add(
            "Two menu shapes in one runtime: the WPF-style Menu / MenuItem pair, which 26.10.9 paints itself, and the " +
            "WinUI-named MenuBar, MenuFlyout and MenuFlyoutItem family, which takes a template for its surface. Even there the " +
            "row content is the control's own paint - label, accelerator text, check mark, chevron and rule - so Astra's skin stops " +
            "at the surface and the icon slot, and presenting either of those twice is the ghost this page used to show.",
            "同一运行时里的两种菜单形态：WPF 风格的 Menu / MenuItem 对，由 26.10.9 自行绘制；以及采用 WinUI 命名的 MenuBar、" +
            "MenuFlyout 与 MenuFlyoutItem 一族，其表面接受模板。即便在那里，行内容仍由控件自行绘制——标签、加速键文本、勾选标记、" +
            "箭头与分隔线——因此 Astra 的皮肤止步于表面与图标槽，把其中任何一个呈现两次就是本页过去展示的「鬼影」。");
    }

    private static void RegisterCommandBar()
    {
        Add("Command bar", "命令栏");
        Add("A bar with commands", "带命令的菜单栏");
        Add("Open the overflow", "打开溢出区");
        Add("Compact the buttons", "紧凑化按钮");
        Add("Show the labels", "显示标签");
        Add("Clear the check", "清除勾选");
        Add("Nothing invoked yet.", "尚未调用任何命令。");
        Add("Save", "保存");
        Add("Bold", "加粗");
        Add("Share", "共享");
        Add("No icon", "无图标");
        Add("Version history", "版本历史");
        Add("Save invoked - the fill under the label is AppBarButtonBackground, and the press that lights it is the IsPressed cell.",
            "已调用「保存」——标签下方的填充是 AppBarButtonBackground，点亮它的按下态来自 IsPressed 单元格。");
        Add("Share invoked through the same routed handler a click reaches.", "「共享」经由与点击相同的路由处理器被调用。");
        Add(
            "Bold checked: the accent fill is AppBarToggleButtonBackgroundChecked, which is the whole mark - upstream shows no " +
            "glyph in a bar.",
            "已勾选「加粗」：强调色填充是 AppBarToggleButtonBackgroundChecked，这就是全部标记——上游在命令栏中不显示任何字形。");
        Add("Bold unchecked: the resting row is transparent, so the button reads as the bar again.", "已取消「加粗」：静止行是透明的，按钮重新融入菜单栏。");
        Add("A disabled bar button answered a click.", "一个禁用的命令栏按钮响应了点击。");
        Add("Check cleared: the accent fill left with it.", "勾选已清除：强调色填充也随之消失。");
        Add(
            "IsOpen={0} - the overflow list is a popup the bar builds for itself, and its colour is the one row this batch had to " +
            "withhold.",
            "IsOpen={0}——溢出列表是该栏为自己构建的一个弹层，而它的颜色正是本批次不得不放弃的那一行。");
        Add(
            "IsCompact dropped every label - the one application-view state this runtime exposes as a property a cell can watch.",
            "IsCompact 去掉了所有标签——这是本运行时唯一以单元格可监视属性的形式暴露出来的应用视图状态。");
        Add(
            "Labels back: the side-by-side layout upstream has is picked by the bar's DefaultLabelPosition, and the bar never " +
            "builds a template of ours, so no label position but this one is reachable.",
            "标签回来了：上游那种并排布局由该栏的 DefaultLabelPosition 决定，而该栏从不构建我们的模板，因此除这一种之外，" +
            "其他标签位置都无法触达。");
        Add(
            "The one family in Astra that arrives with a theme already underneath it: 26.10.9 ships its own implicit styles for " +
            "the three app-bar elements and paints its bar from nine rows of its own, so this page is mostly what a shadowed row " +
            "buys - and one surface a shadow cannot reach.",
            "Astra 中唯一一个自带主题的控件族：26.10.9 为三个命令栏元素随包发布了隐式样式，并用自有资源行中的九行绘制其菜单栏，" +
            "因此本页大体上展示的是一条带阴影的资源行能买到什么——以及一处阴影触不到的表面。");
        Add(
            "The pill is 4 DIP wide and 1.5 rounded by upstream's own rows; its 16 DIP length is ours, because upstream keeps " +
            "that number in a C++ presenter this reference does not carry. Disabling the list would not dim these rows - only a " +
            "row-level IsEnabled reaches the cell.",
            "胶囊宽 4 DIP、圆角 1.5，取自上游自有资源行；其 16 DIP 的长度是我们的，因为上游把那个数字保存在本参考实现并不携带的" +
            "C++ presenter 里。禁用整个列表不会让这些行变暗——只有行级的 IsEnabled 才能作用到单元格。");
    }

    private static void RegisterSettings()
    {
        Add("Settings", "设置");
        Add("Change the Gallery appearance while keeping control state.", "在保持控件状态的同时更换画廊外观。");
        Add("Appearance", "外观");
        Add("Theme", "主题");
        Add("Choose a theme or follow Windows.", "选择主题或跟随 Windows。");
        Add("Light", "浅色");
        Add("Dark", "深色");
        Add("Use system setting", "使用系统设置");
        Add("Accent color", "强调色");
        Add("Apply one accent across actions and selection.", "用一个强调色贯穿所有动作与选择。");
        Add("Default", "默认");
        Add("Blue", "蓝色");
        Add("Violet", "紫色");
        Add("Green", "绿色");
        Add("Motion", "动效");
        Add("Reduce motion", "减少动效");
        Add(
            "Settle navigation immediately, turn off the page entrance, and rewrite the three published animation durations to " +
            "zero, so a hover or a press snaps instead of easing. A transition already running keeps its clock, and a control " +
            "built while this is on is born still.",
            "让导航立即落定、关闭页面入场动效，并把三个已发布的动画时长重写为零，使悬停或按下变为瞬切而非缓动。" +
            "已经在运行的过渡会保留其时钟，而在此开关打开期间构建的控件天生就是静止的。");
        Add("Windows animation preferences are also respected.", "同时也会尊重 Windows 的动画偏好设置。");
        Add("Replay page entrance", "重放页面入场动效");
        Add("Restore appearance", "恢复外观");
        Add("Appearance restored to system theme and default accent.", "外观已恢复为系统主题与默认强调色。");
        Add("Theme: {0}.", "主题：{0}。");
        Add("Accent: {0}.", "强调色：{0}。");
        Add("Reduce motion set to {0}.", "减少动效已设为 {0}。");
        Add("Page entrance replayed.", "页面入场动效已重放。");
        Add("Page shown immediately because motion is reduced.", "因已减少动效，页面立即显示。");
    }

    private static void RegisterRuntimeText()
    {
        // Catalog page titles. The navigation item carries the same word in the markup, and one registration
        // serves both because the catalog keys on the English.
        Add("Overview", "概览");

        // Shared control captions that several pages draw on.
        Add("Bars", "进度条");
        Add("Copy", "复制");
        Add("Icons", "图标");
        Add("Open", "打开");
        Add("Owner", "负责人");
        Add("Password", "密码");
        Add("ScrollViewer", "ScrollViewer");
        Add("Seat", "座位");
        Add("Secondary", "次要");
        Add("Tertiary", "第三层");
        Add("ToolTip", "工具提示");
        Add("Archived", "已归档");
        Add("Ticket office", "售票处");
        Add("Unavailable option", "不可用选项");

        // Readouts and report lines built in code.
        Add("Ready. Choose a control to try it.", "就绪。选择一个控件来试用。");
        Add("Output cleared. Choose a control to try it.", "输出已清空。选择一个控件来试用。");
        Add("Live output", "实时输出");
        Add("Clear", "清除");
        Add("Profile", "配置文件");
        Add("Open the list and choose a profile.", "展开列表并选择一个配置文件。");
        Add("Notifications", "通知");
        Add("Volume", "音量");
        Add("Profile: {0}.", "配置文件：{0}。");
        Add("Volume: {0}%.", "音量：{0}%。");
        Add("Applied locally: {0} · {1} · Volume {2}% · Notifications {3}",
            "已本地应用：{0} · {1} · 音量 {2}% · 通知 {3}");
        Add("Display name changed: {0} characters.", "显示名称已更改：{0} 个字符。");
        Add("Password changed: {0} characters.", "密码已更改：{0} 个字符。");
        Add("Fruit filter: {0} characters.", "水果筛选：{0} 个字符。");
        Add("Fruit suggestion: {0}.", "水果建议：{0}。");
        Add("This PC", "本机");
        Add("Documents", "文档");
        Add("Projects", "项目");
        Add("Work item", "工作项");
        Add("Ship", "发布");
        Add("Review", "评审");
        Add("Design", "设计");
        Add("Develop", "开发");
        Add("Shipped", "已发布");
        Add("Review", "评审");
        Add("Blocked", "已阻塞");
        Add("Backlog", "待办");
        Add("Closed today", "今日已关闭");
        Add("Closed.", "已关闭。");
        Add("Four levels.", "四个级别。");
        Add("Push a level", "推入一级");
        Add("Pop one", "弹出一级");
        Add("{0} {1} levels, {2} of them in the row", "{0} {1} 个级别，其中 {2} 个在行内");
        Add(
            "The row keeps the last crumb and as many of the ones before it as fit, and everything that does not fit moves " +
            "behind the ellipsis - so dragging the width slider collapses the head of the path and brings it back, which is " +
            "upstream's rule read straight off its layout: the floor is the ellipsis and the last crumb, and crumbs are packed " +
            "with no spacing. The last crumb is plain text rather than a button, so clicking it reports nothing, and the " +
            "ellipsis reports nothing either: it only opens its list.",
            "这一行会保留最后一级以及它之前放得下的若干级，放不下的全部移到省略号之后——因此拖动宽度滑块会收起路径的开头再把它" +
            "放回来，这正是直接从上游布局读出的规则：下限是省略号加最后一级，各级之间不留间距。最后一级是纯文本而非按钮，" +
            "因此点击它不会有任何反馈；省略号同样没有，它只负责展开自己的列表。");
        Add("Breadcrumb bar", "导航栏");
        Add("Overflow list last change: {0}.", "溢出列表最后变化：{0}。");
        Add("{0} is not in the catalog.", "{0} 不在目录中。");
        Add("\"{0}\" is not a gallery page id. Options: {1}.", "「{0}」不是画廊页面 id。可选：{1}。");
        Add("Catalog.json did not reach the output folder, so no parity claim on this page is checked.",
            "Catalog.json 没有进入输出目录，因此本页的平价声明均未校验。");
        Add("Navigated to {0}.", "已导航到 {0}。");
        Add("Tree selection: {0}.", "树选择：{0}。");
        Add("Tree view", "树视图");
        Add("Data grid", "数据网格");
        Add("Tree data grid", "树数据网格");
        Add("Wrap", "换行");
        Add("Rows overflow", "行溢出");
        Add("Horizontal", "水平");
        Add(
            "Indent is 16 DIP per level, the runtime's own IndentSize. Unlike the plain grid, a tree column left at Auto lays out " +
            "at zero width, so every column here carries a minimum; the chevron is drawn by the control inside the tree cell, " +
            "not by this template. Expanding is a public call here (ExpandAll / IsExpanded), so the gallery's expand evidence " +
            "is API-driven, not a pointer.",
            "每级缩进 16 DIP，即运行时自身的 IndentSize。与普通网格不同，留为 Auto 的树列会以零宽度布局，因此本页每一列都带有最小宽度；" +
            "箭头由控件在树单元格内部绘制，而不是由这个模板绘制。此处的展开是公开调用（ExpandAll / IsExpanded），" +
            "因此画廊的展开证据由 API 驱动，而非由指针驱动。");
        Add(
            "Column sort writes a glyph into PART_SortIndicator itself, so the template only owns its size; the row line is the " +
            "framework's, the cell line is ours. Editing a cell still needs the runtime's own commit path - see the parity strip.",
            "列排序会自行把字形写入 PART_SortIndicator，因此模板只掌管它的尺寸；行分隔线来自框架，单元格分隔线才是我们的。" +
            "编辑单元格仍然需要运行时自身的提交路径——参见平价条。");
        Add("Upstream ships a multi-line text box on this page; TextWrapping and the line counts are the parts our template has " +
            "to keep painting from the same tokens at a taller surface.",
            "上游在本页随包发布了多行文本框；TextWrapping 与行数正是我们的模板必须继续用同一批令牌、在更高的表面上绘制的部分。");
        Add("Upstream ships this as AutoSuggestBox; the native type here is AutoCompleteBox, and it wears the same TextControl* " +
            "rows as the text box above plus the AutoSuggestBox* rows for the suggestion list. Type into the first box to open the list.",
            "上游把它作为 AutoSuggestBox 发布；这里的原生类型是 AutoCompleteBox，它穿着与上方文本框相同的 TextControl* 资源行，" +
            "外加用于建议列表的 AutoSuggestBox* 资源行。在第一个输入框中键入即可展开列表。");
        Add("Not claimed here: choosing a suggestion does not write its text back into the box once the framework template is " +
            "replaced, the suggestion rows keep the framework's own fill, and there is no query button, header or description to place.",
            "此处不作声明：一旦框架模板被替换，选择某条建议并不会把它的文本写回输入框；建议行保留框架自身的填充；" +
            "而且这里没有查询按钮、标题或说明可供摆放。");
        Add("Adaptive pane", "自适应窗格");
        Add("Wrapping notes", "换行说明");

        // Captions and paragraphs the earlier sections did not claim, filled in by the coverage gate.
        Add("Astra styles", "Astra 样式");
        Add("Car", "汽车");
        Add("Choices and values", "选项与数值");
        Add("Close window", "关闭窗口");
        Add("Design sync", "设计同步");
        Add("Explore inputs", "探索输入控件");
        Add("One", "第一项");
        Add("Open buttons", "打开按钮页");
        Add("Open navigation", "打开导航页");
        Add("Quarterly review", "季度评审");
        Add("Rolled back", "已回滚");
        Add("Ship train", "发布列车");
        Add("Split and drop-down", "拆分按钮与下拉按钮");
        Add("Test base", "测试基线");
        Add("Text and passwords", "文本与密码");
        Add("Two", "第二项");
        Add("Expanded or compact", "展开或紧凑");
        Add("Flyout surface, solid fill (this runtime has no AcrylicBrush; the key holds upstream's own FallbackColor), 1px stroke, 320 DIP cap",
            "浮出层表面为实色填充（本运行时没有 AcrylicBrush；该键保留的是上游自己的 FallbackColor）、1px 描边、320 DIP 上限");
        Add("Shown resting in the tree, because the hover timer that would normally open it is not gated.",
            "以静止状态留在树中，因为本应打开它的悬停计时器并未被门控。");
        Add(
            "An icon column is 16 DIPs and disappears entirely when a tab has no icon, so the header's left inset stays where the text is.",
            "图标列宽 16 DIP，当选项卡没有图标时会完全消失，因此标题的左内缩始终停在文字所在的位置。");
        Add(
            "Every page here scrolls through this host. The bar only appears while it is needed, and the thumb width is the upstream one.",
            "这里的每一页都通过这个宿主滚动。滚动条只在需要时才出现，滑块宽度则沿用上游的值。");
        Add(
            "Astra does not retemplate the scroll host: the framework owns its parts, and the theme reaches it through the two " +
            "generated brush hooks and the implicit style. Measured on 26.10.9: the resting bar paints, the thumb follows the " +
            "theme, an opaque host background covers its own bar, and overlay mode takes the repeat arrows away. What is not " +
            "measured is dragging, inertia and touch panning, so scroll this box yourself and treat the picture as the claim " +
            "rather than the physics.",
            "Astra 不会为滚动宿主重新套用模板：部件归框架所有，主题是通过两个生成的画刷钩子与隐式样式触达它的。" +
            "在 26.10.9 上实测：静止态滚动条会绘制，滑块跟随主题，不透明的宿主背景会盖住自己的滚动条，而覆盖模式会去掉重复箭头。" +
            "未被实测的是拖动、惯性与触摸平移，因此请自行滚动这个盒子，并把画面当作声明而非物理特性。");
        Add(
            "Hover, press and the dropdown animation are not claimed: the pressed states here are driven by mouse capture in a test, never by a pointer.",
            "悬停、按下与下拉动画不在声明范围内：本页的按下态由测试中的鼠标捕获驱动，而非由指针驱动。");
        Add(
            "These four take their fills from their own upstream alias files - ToggleButton, RepeatButton and HyperlinkButton each " +
            "carry a transcribed key list, and the hyperlink inherits the layout style rather than the subtle button's fill. The " +
            "third-state toggle cycles off, on, mixed; mixed paints the same brushes off does, because that is what upstream's own " +
            "alias rows say, so the live readout is the only place the third state shows.",
            "这四个控件的填充各自取自它们自己的上游别名文件——ToggleButton、RepeatButton 与 HyperlinkButton 各带一份转录的键清单，" +
            "而超链接继承的是布局样式而非轻淡按钮的填充。三态切换按钮在关、开、混合之间循环；混合态绘制的画刷与关闭态相同，" +
            "因为上游的别名资源行就是这么写的，所以第三种状态只会在实时读数中显现。");
        Add(
            "These three are the runtime's own types, and this card deliberately ships no style for them: an icon here derives " +
            "from FrameworkElement rather than Control, so there is no template to hand it - upstream builds the visual in C++ and " +
            "publishes no default style either. What was measured instead: the symbol box is a constant 20 DIP whatever the symbol, " +
            "the font icon's box is its font size, and the path icon is the only member whose ink a capture can actually see, which " +
            "is why it is the one drawn as a solid figure. The honest limits are in docs/astra/audits/icon-family.md - the symbol " +
            "enum here paints the same glyph as upstream for 167 of the 171 names both sides share, four of them a different one, and " +
            "the icon font cannot be published as a resource row at all on this runtime, so every template repeats the family name as a " +
            "literal.",
            "这三个是运行时自带的类型，而这张卡片刻意不为它们发布任何样式：这里的图标派生自 FrameworkElement 而非 Control，" +
            "因此没有模板可交给它——上游用 C++ 构建视觉，也没有发布默认样式。实际测到的是：无论符号为何，符号框恒为 20 DIP；" +
            "字体图标框等于其字号；路径图标是唯一能被截图真正看到墨迹的成员，因此它是唯一以实心图形绘制的。" +
            "诚实的边界记录在 docs/astra/audits/icon-family.md——在双方共有的 171 个名称中，这里的符号枚举有 167 个绘制出与上游" +
            "相同的字形，4 个不同；而图标字体在本运行时上根本无法作为资源行发布，所以每个模板都把字族名重复写成字面量。");
        Add(
            "The line under the strip stops at a selected tab: its own piece collapses, the two neighbours shorten by 2 DIPs, and " +
            "the 4 DIP corner hooks are drawn in the line's colour so the tab reads as grown out of the surface below.",
            "选项卡条下方那条线会在被选中的选项卡处停下：它自己那一段收起，两侧各缩短 2 DIP，4 DIP 的圆角钩子用这条线的颜色绘制，" +
            "于是该选项卡看起来像是从下方表面长出来的。");
        Add(
            "The split button is native, restyled from upstream's SplitButton* rows; the drop-down is an Astra-owned type, because " +
            "26.10.9 ships no DropDownButton to retemplate. On a split button only the left half is the action: the divider and the " +
            "right chevron belong to the flyout, and a split button with no flyout has its right half disabled by the framework. " +
            "Neither control's open state reaches the paint here - upstream asks its own internal flags for that and this runtime " +
            "keeps them private.",
            "拆分按钮是原生的，按上游的 SplitButton* 资源行重新着色；下拉按钮则是 Astra 的自有类型，因为 26.10.9 没有随包发布可供" +
            "重新套模板的 DropDownButton。在拆分按钮上只有左半部分是动作：分隔线与右侧箭头属于浮出层，而没有浮出层的拆分按钮，" +
            "其右半部分会被框架禁用。这两个控件的展开状态都到不了这里的绘制——上游向自己的内部标志取该状态，而本运行时把那些标志保持为私有。");
        Add(
            "State the user reads but does not operate. The bar below is the framework's own control wearing our template; what " +
            "follows on this page will be the types this runtime does not have at all.",
            "供用户阅读、而非操作的状态。下面那条栏是框架自带的控件穿着我们的模板；本页接下来要展示的，则是本运行时根本没有的类型。");
        Add(
            "Use Up, Down, Home, and End to move focus between destinations. Press Enter or Space to select the focused destination.",
            "用上、下、Home、End 在各目标之间移动焦点。按 Enter 或空格选中当前焦点上的目标。");
        Add("Without a close button the header takes the 8 DIP right inset back.", "没有关闭按钮时，标题会收回那 8 DIP 的右内缩。");
    }

    private static void Add(string english, string simplifiedChinese) =>
        UiText.Add(english, simplifiedChinese);
}
