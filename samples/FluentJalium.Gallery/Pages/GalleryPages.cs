using FluentJalium.Controls;
using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Pages;

public sealed record GalleryEntry(string Key, string Group, string Title, string Description, FluentIconRegular Icon, Func<Action<GalleryEntry>, UIElement> CreateContent);

/// <summary>Catalog for the first-party Gallery. Every entry is a real FluentJalium sample.</summary>
public static class GalleryPages
{
    public static readonly GalleryEntry About = new("about", "Resources", "About", "The FluentJalium control library and its design principles.", FluentIconRegular.BookInformation24, _ => AboutPage());

    public static IReadOnlyList<GalleryEntry> All { get; } =
    [
        new("home", "Getting started", "Overview", "A focused tour of FluentJalium controls and Fluent Design foundations.", FluentIconRegular.Home24, navigate => HomePage(navigate)),
        new("controls", "Getting started", "All controls", "A live inventory of the FluentJalium control surface, grouped by capability.", FluentIconRegular.DocumentBulletList24, _ => ControlsPage()),
        new("buttons", "Basic input", "Buttons", "Command surfaces with clear hierarchy, density, and keyboard focus.", FluentIconRegular.Cursor24, _ => ButtonsPage()),
        new("forms", "Basic input", "Forms", "Text input, selection, and toggles composed into a familiar form.", FluentIconRegular.Edit24, _ => FormsPage()),
        new("selection", "Basic input", "Selection", "Choose one or many options with clear selected states.", FluentIconRegular.CheckmarkCircle24, _ => SelectionPage()),
        new("range", "Basic input", "Range & progress", "Sliders and progress indicators make value and state legible.", FluentIconRegular.DataBarVertical24, _ => RangePage()),
        new("layout", "Layout", "Layout", "Responsive spacing and content surfaces built from Fluent layout primitives.", FluentIconRegular.Grid24, _ => LayoutPage()),
        new("collections", "Layout", "Collections", "Present repeated content with predictable rhythm and grouping.", FluentIconRegular.ContentView24, _ => CollectionsPage()),
        new("advancedcollections", "Layout", "Advanced collections", "Data grids and list surfaces for dense, keyboard-friendly information architecture.", FluentIconRegular.Table24, _ => AdvancedCollectionsPage()),
        new("navigation", "Navigation", "Navigation", "NavigationView, tabs, and command affordances in a coherent shell.", FluentIconRegular.Navigation24, _ => NavigationPage()),
        new("selectors", "Input & data", "Selectors & inspectors", "Hierarchical selection and property inspection with explicit state and density contracts.", FluentIconRegular.Branch24, _ => SelectorsPage()),
        new("datetime", "Input & data", "Date & time", "Pickers and calendars follow the same density and focus contract.", FluentIconRegular.CalendarLtr24, _ => DateTimePage()),
        new("charts", "Input & data", "Charts", "Data visualization controls with axes, legends, and accessible values.", FluentIconRegular.ChartMultiple24, _ => ChartsPage()),
        new("datainspectors", "Input & data", "Data inspectors", "Inspect JSON, source changes, and binary payloads using Jalium.UI data controls.", FluentIconRegular.Database24, _ => DataInspectorsPage()),
        new("interaction", "Interaction", "Interaction", "Scrolling, zooming, pull-to-refresh, and annotated scroll affordances.", FluentIconRegular.ArrowSync24, _ => InteractionPage()),
        new("menus", "App structure", "Menus & commands", "Menu bars and commands keep actions discoverable and keyboard-friendly.", FluentIconRegular.List24, _ => MenusPage()),
        new("disclosure", "App structure", "Disclosure", "Expanders and settings surfaces reveal complexity progressively.", FluentIconRegular.PanelLeft24, _ => DisclosurePage()),
        new("visuals", "Visuals", "Visuals & icons", "Shapes, avatars, and icon elements for expressive but consistent UI.", FluentIconRegular.Image24, _ => VisualsPage()),
        new("inputmedia", "Visuals", "Input & media", "Color, ink, and media controls for rich interactive surfaces.", FluentIconRegular.Color24, _ => InputMediaPage()),
        new("motion", "Design", "Motion", "Connected transitions and animated visuals that preserve context.", FluentIconRegular.SlideTransition24, _ => MotionPage()),
        new("shell", "App structure", "Shell & window", "Title bars, panes, and window-level composition primitives.", FluentIconRegular.Window24, _ => ShellPage()),
        new("materials", "Design", "Materials", "Mica, Acrylic, and Liquid Glass surfaces with semantic elevation.", FluentIconRegular.Sparkle24, _ => MaterialsPage()),
        new("status", "Design", "Status & progress", "Communicate state with progress, info, warning, and success treatments.", FluentIconRegular.Alert24, _ => StatusPage())
    ];

    private static UIElement HomePage(Action<GalleryEntry> navigate)
    {
        var body = PageBody("Overview", "A gallery built with FluentJalium itself — browse controls, inspect recipes, and compose your own experience.");
        body.Add(new GalleryHero());
        body.Add(SectionTitle("Explore the system"));
        var cards = new FWWrapPanel { HorizontalSpacing = 16, VerticalSpacing = 16 };
        foreach (var entry in All.Skip(1)) cards.Children.Add(new GalleryLinkCard(entry, navigate));
        body.Add(cards);
        return Scroll(body);
    }

    private static UIElement ControlsPage()
    {
        var body = PageBody("All FluentJalium controls", "Every public control marked with IFluentJaliumControl is discoverable here. Choose a family to learn its interaction model and theme contract.");
        var grid = new FWWrapPanel { HorizontalSpacing = 12, VerticalSpacing = 12 };
        var groups = typeof(FWButton).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(FrameworkElement).IsAssignableFrom(type) && typeof(IFluentJaliumControl).IsAssignableFrom(type))
            .OrderBy(type => type.Namespace, StringComparer.Ordinal)
            .ThenBy(type => type.Name, StringComparer.Ordinal)
            .GroupBy(type => FormatCategory(type.Namespace));

        foreach (var group in groups)
        {
            var section = new FWStackPanel { Orientation = Orientation.Vertical, Spacing = 10, Margin = new Thickness(0, 6, 0, 14) };
            section.Children.Add(SectionTitle(group.Key));
            var cards = new FWWrapPanel { HorizontalSpacing = 12, VerticalSpacing = 12 };
            foreach (var type in group) cards.Children.Add(new GalleryControlCatalogCard(type, group.Key));
            section.Children.Add(cards);
            body.Add(section);
        }

        return Scroll(body);
    }

    private static string FormatCategory(string? ns)
    {
        var segment = ns?.Split('.').LastOrDefault();
        return string.IsNullOrWhiteSpace(segment) ? "Core" : segment;
    }

    private static UIElement ButtonsPage() => Scroll(PageBody("Buttons", "Buttons communicate actions through hierarchy, not decoration.",
        ExampleCard("Accent button", "Use an accent button for the single most important action.", new FWButton { Content = "Create project", HorizontalAlignment = HorizontalAlignment.Left }),
        ExampleCard("Button hierarchy", "Secondary and subtle actions recede while remaining discoverable.", new FWStackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Children = { new FWButton { Content = "Save" }, new FWButton { Content = "Cancel" }, new FWButton { Content = "More", Density = FWButtonDensity.Compact } } })));

    private static UIElement FormsPage() => Scroll(PageBody("Forms", "Inputs use consistent labels, focus visuals, and comfortable spacing.",
        ExampleCard("Profile", "A Jalxaml-defined form composed entirely from FluentJalium input controls.", new BasicInputSample(), "<fw:FWTextBox PlaceholderText=\"Enter your name\" />\n<fw:FWComboBox PlaceholderText=\"Choose a workspace\" />\n<fw:FWToggleSwitch Header=\"Enable product updates\" />")));

    private static UIElement LayoutPage() => Scroll(PageBody("Layout", "Use a clear rhythm: 4 px units, generous section spacing, and capped reading widths.",
        ExampleCard("Responsive surface", "Cards are content-first and adapt to the available width.", new FWWrapPanel { HorizontalSpacing = 12, VerticalSpacing = 12, Children = { MetricCard("4 px", "Base spacing unit"), MetricCard("8 px", "Control corner radius"), MetricCard("40 px", "Touch target") } })));

    private static UIElement SelectionPage() => Scroll(PageBody("Selection", "Selection controls make the current choice obvious and easy to change.",
        ExampleCard("Preferences", "Use a ComboBox when options need a compact menu.", new FWComboBox { Width = 300, PlaceholderText = "Choose a workspace", ItemsSource = new[] { "Design", "Engineering", "Research" } })));

    private static UIElement RangePage() => Scroll(PageBody("Range & progress", "Keep values visible and pair progress with a meaningful label.",
        ExampleCard("Volume", "Slider uses the same density and focus treatment as other inputs.", new FWStackPanel { Orientation = Orientation.Vertical, Spacing = 12, Children = { new FWSlider { Minimum = 0, Maximum = 100, Value = 68, Width = 380 }, new FWTextBlock { Text = "68%", Foreground = Brush("TextFillColorSecondaryBrush") } } })));

    private static UIElement CollectionsPage() => Scroll(PageBody("Collections", "Repeated content benefits from a consistent card, spacing, and alignment model.",
        ExampleCard("Recent files", "Keep secondary metadata quiet so the primary label remains scannable.", new FWWrapPanel { HorizontalSpacing = 10, VerticalSpacing = 10, Children = { MetricCard("Design.md", "Edited just now"), MetricCard("Tokens.json", "Edited yesterday"), MetricCard("Readme", "Edited Monday") } })));

    private static UIElement AdvancedCollectionsPage() => Scroll(PageBody("Advanced collections", "WinUI-style collection controls keep dense data scannable while preserving keyboard and automation contracts.",
        ExampleCard("Data grid and list", "Auto-generated columns and density-aware list items are composed from Jalxaml.", new CollectionsSample(), "<fw:FWDataGrid AutoGenerateColumns=\"True\" />\n<fw:FWListView />")));

    private static UIElement NavigationPage() => Scroll(PageBody("Navigation", "NavigationView keeps destinations visible and gives the current location a strong, quiet indicator.",
        ExampleCard("Command bar", "Pair navigation with a small set of contextual commands.", new FWStackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { new FWButton { Content = "New" }, new FWButton { Content = "Open" }, new FWButton { Content = "Share", Density = FWButtonDensity.Compact } } })));

    private static UIElement DateTimePage() => Scroll(PageBody("Date & time", "Pickers and calendars follow the same density and focus contract.",
        ExampleCard("Schedule", "Use the compact picker when a form needs a single date and time.", new FWStackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Children = { new FWDatePicker { Width = 220 }, new FWTimePicker { Width = 180 } } })));

    private static UIElement SelectorsPage() => Scroll(PageBody("Selectors & inspectors", "Hierarchical choices and object properties remain discoverable without sacrificing density.",
        ExampleCard("Tree selector and property grid", "The sample uses real TreeSelectorItem containers and a reflected profile object.", new SelectorsSample(), "<fw:FWTreeSelector IsSearchEnabled=\"True\" />\n<fw:FWPropertyGrid ShowSearchBox=\"True\" />")));

    private static UIElement ChartsPage() => Scroll(PageBody("Charts", "Data visualization controls expose values, axes, and semantic palette hooks.",
        ExampleCard("Line chart", "A Jalxaml-defined chart receives its data series in code-behind, keeping markup readable.", new ChartSample(), "<fw:FWLineChart x:Name=\"Chart\" ShowDataPoints=\"True\" />"),
        ExampleCard("Gauge", "A compact gauge communicates progress at a glance.", new FWGaugeChart { Width = 300, Height = 190, Title = "Completion", Value = 72, Maximum = 100, IsLegendVisible = false })));

    private static UIElement DataInspectorsPage() => Scroll(PageBody("Data inspectors", "Structured and binary data deserve first-class, theme-aware inspection surfaces.",
        ExampleCard("JSON, diff, and hex", "The controls receive real payloads in code-behind while their composition remains Jalxaml-first.", new DataInspectorSample(), "<fw:FWJsonTreeViewer x:Name=\"JsonViewer\" />\n<fw:FWDiffViewer x:Name=\"DiffViewer\" />\n<fw:FWHexEditor x:Name=\"HexViewer\" />")));

    private static UIElement InteractionPage() => Scroll(PageBody("Interaction", "Interaction primitives make scrolling and refresh behavior explicit, composable, and testable.",
        ExampleCard("Scroller and refresh", "Zoom, viewport diagnostics, pull-to-refresh, and annotation labels work together in one sample.", new InteractionSample(), "<fw:FWScroller x:Name=\"Scroller\" />\n<fw:FWRefreshContainer />\n<fw:FWAnnotatedScrollBar />")));

    private static UIElement MenusPage() => Scroll(PageBody("Menus & commands", "Menus preserve discoverability while supporting keyboard navigation.",
        ExampleCard("Application menu", "A Jalxaml-defined MenuBar is populated with command items in its code-behind.", new MenuSample(), "<fw:FWMenuBar x:Name=\"MenuBar\" />")));

    private static UIElement DisclosurePage() => Scroll(PageBody("Disclosure", "Progressive disclosure keeps advanced settings close without overwhelming the default view.",
        ExampleCard("More options", "Expander reveals secondary content on demand.", new FWExpander { Header = "Advanced settings", IsExpanded = true, Content = new FWStackPanel { Orientation = Orientation.Vertical, Spacing = 8, Children = { new FWCheckBox { Content = "Enable diagnostics", IsChecked = true }, new FWCheckBox { Content = "Show frame pacing" } } } })));

    private static UIElement VisualsPage() => Scroll(PageBody("Visuals & icons", "Use visuals to reinforce meaning while keeping interaction and contrast clear.",
        ExampleCard("Avatar and shapes", "A Jalxaml visual sample combines PersonPicture and shape primitives with Fluent tokens.", new VisualSample(), "<fw:FWPersonPicture Initials=\"FJ\" />\n<fw:FWRectangle Fill=\"{ThemeResource AccentFillColorDefaultBrush}\" />")));

    private static UIElement InputMediaPage() => Scroll(PageBody("Input & media", "Rich input controls remain responsive while honoring the Fluent density model.",
        ExampleCard("Color picker", "Use color as an intentional, accessible signal.", new FWColorPicker { Width = 340 }),
        ExampleCard("Ink surface", "InkCanvas provides a natural drawing surface for pen and touch.", new FWBorder { Width = 340, Height = 140, Background = Brush("LayerFillColorAltBrush"), Child = new FWInkCanvas() })));

    private static UIElement MotionPage() => Scroll(PageBody("Motion", "Motion should preserve spatial context and communicate change without distraction.",
        ExampleCard("Transition host", "TransitioningContentControl centralizes entrance and replacement choreography.", new FWTransitioningContentControl { Width = 360, Height = 90, Content = new FWTextBlock { Text = "Content enters with context", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } })));

    private static UIElement ShellPage() => Scroll(PageBody("Shell & window", "Window-level primitives help applications establish a consistent frame.",
        ExampleCard("Title bar commands", "TitleBar hosts left and right command groups while preserving window chrome.", new FWTitleBar { Title = "FluentJalium Workspace", LeftWindowCommands = new FWStackPanel { Orientation = Orientation.Horizontal, Children = { new FWButton { Content = "Back" } } }, RightWindowCommands = new FWStackPanel { Orientation = Orientation.Horizontal, Children = { new FWButton { Content = "Share" } } }, Width = 620, Height = 48 })));

    private static FWMenuBar CreateMenuBar()
    {
        var menu = new FWMenuBar { Width = 360 };
        var file = new FWMenuBarItem { Title = "File" };
        file.Items.Add(new FWMenuItem { Header = "New project" });
        file.Items.Add(new FWMenuItem { Header = "Open" });
        var view = new FWMenuBarItem { Title = "View" };
        view.Items.Add(new FWMenuItem { Header = "Appearance" });
        view.Items.Add(new FWMenuItem { Header = "Command palette" });
        menu.Items.Add(file);
        menu.Items.Add(view);
        menu.UpdateItems();
        return menu;
    }

    private static UIElement MaterialsPage() => Scroll(PageBody("Materials", "Materials establish depth and hierarchy while allowing content to remain the focus.",
        ExampleCard("Surface recipes", "The same semantic surface roles are available to every FluentJalium app.", new FWStackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Children = { Surface("Mica", FWFluentMaterialKind.Mica), Surface("Acrylic", FWFluentMaterialKind.Acrylic), Surface("Liquid Glass", FWFluentMaterialKind.LiquidGlass) } })));

    private static UIElement StatusPage() => Scroll(PageBody("Status & progress", "Status is communicated with color, iconography, and concise language.",
        ExampleCard("Loading", "Progress should be visible but never compete with the task.", new FWStackPanel { Orientation = Orientation.Vertical, Spacing = 14, Children = { new FWProgressBar { Value = 68, Maximum = 100, Width = 360 }, new FWInfoBar { Title = "Connected", Message = "Your changes are synced.", Severity = InfoBarSeverity.Success, IsOpen = true } } })));

    private static UIElement AboutPage() => Scroll(PageBody("About FluentJalium", "FluentJalium brings the Fluent Design System to Jalium UI with native, composable controls.",
        ExampleCard("Principles", "Accessible by default, theme-aware, responsive, and intentionally quiet.", new FWStackPanel { Orientation = Orientation.Vertical, Spacing = 8, Children = { Bullet("Use semantic colors and typography."), Bullet("Respect focus, keyboard, and touch."), Bullet("Prefer composition over one-off visuals.") } })));

    private static GalleryPageFrame PageBody(string title, string description, params UIElement[] content)
    {
        var body = new GalleryPageFrame(title, description);
        foreach (var item in content) body.Add(item);
        return body;
    }

    private static UIElement ExampleCard(string title, string description, UIElement sample, string? code = null)
    {
        return new GalleryExampleCard(title, description, sample, code);
    }

    private static UIElement SectionTitle(string text) => new FWTextBlock { Text = text, FontSize = 22, FontWeight = FontWeights.SemiBold, Foreground = Brush("TextFillColorPrimaryBrush"), Margin = new Thickness(0, 10, 0, 0) };
    private static UIElement MetricCard(string value, string label) => new FWCardSurface { Width = 190, Padding = new Thickness(16), Child = new FWStackPanel { Spacing = 6, Children = { new FWTextBlock { Text = value, FontSize = 28, FontWeight = FontWeights.SemiBold, Foreground = Brush("AccentTextFillColorPrimaryBrush") }, new FWTextBlock { Text = label, Foreground = Brush("TextFillColorSecondaryBrush") } } } };
    private static UIElement Surface(string title, FWFluentMaterialKind kind) => new FWFluentMaterialSurface { MaterialKind = kind, Width = 190, Height = 110, Padding = new Thickness(16), CornerRadius = new CornerRadius(8), Child = new FWTextBlock { Text = title, FontWeight = FontWeights.SemiBold, Foreground = Brush("TextFillColorPrimaryBrush") } };
    private static UIElement Bullet(string text) => new FWTextBlock { Text = "•  " + text, FontSize = 15, Foreground = Brush("TextFillColorPrimaryBrush") };
    // GalleryPageFrame already owns the scroll contract in Jalxaml. Keeping this helper as an
    // identity function lets concise page factories read the same way while avoiding nested
    // ScrollViewer instances that compete for wheel input.
    private static UIElement Scroll(UIElement content) => content;
    private static Brush Brush(string key) => Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush ? brush : new SolidColorBrush(Colors.Transparent);
}
