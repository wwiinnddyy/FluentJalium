using System.Threading;
using FluentJalium.Controls;
using Jalium.UI.Markup;

namespace FluentJalium.WinUI;

/// <summary>
/// Makes WinUI markup resolve to FluentJalium's controls. Call once from the application entry
/// point, after <c>ThemeManager.Initialize</c> and <c>FluentThemeManager.Apply</c> and before any
/// window or page is constructed.
/// </summary>
/// <remarks>
/// Jalium resolves a markup element by its <em>simple name</em>, before it ever consults an
/// <c>xmlns</c> mapping (<c>XamlReader.ResolveTypeUncached</c> step 2). That is what lets a WinUI
/// document keep its default
/// <c>xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"</c> untouched: re-registering
/// a name replaces whatever the framework had for it, process-wide.
///
/// Two consequences worth knowing:
/// <list type="bullet">
///   <item>Order is load-bearing. FluentJalium's own theme dictionaries use plain Jalium type names,
///         so <see cref="Initialize"/> must run after theming is applied.</item>
///   <item>Layout and decoration types (<c>Grid</c>, <c>StackPanel</c>, <c>Canvas</c>, <c>Border</c>,
///         <c>Image</c>, <c>ContentPresenter</c>) are deliberately not aliased. Jalium's originals
///         already answer the markup, and overriding them globally reaches into every framework
///         template that names them.</item>
/// </list>
/// </remarks>
public static class WinUICompatibility
{
    private static int _initialized;

    /// <summary>Whether <see cref="Initialize"/> has already registered the WinUI aliases.</summary>
    public static bool IsInitialized => Volatile.Read(ref _initialized) != 0;

    /// <summary>
    /// Registers the WinUI type-name aliases and the <c>using:Microsoft.UI.Xaml.*</c> namespace
    /// mappings. Idempotent.
    /// </summary>
    public static void Initialize()
    {
        if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
        {
            return;
        }

        // The assembly-level XmlnsDefinition / XmlnsCompatibleWith declarations normally arrive on
        // their own through AppDomain.AssemblyLoad. Scan deterministically here instead, so a page
        // parsed before any type from this assembly is touched still sees them.
        XmlnsDefinitionRegistry.EnsureInitialized();
        XmlnsDefinitionRegistry.ScanAssembly(typeof(WinUICompatibility).Assembly);

        ControlAliases.Register();
    }
}

/// <summary>
/// WinUI element name → implementation type. FluentJalium's <c>FW*</c> controls are thin
/// subclasses of the same-named Jalium control, so this table is mostly "strip the prefix".
/// Every entry names the WinUI type explicitly: <c>XamlTypeRegistry.RegisterType&lt;T&gt;()</c>
/// without a name registers under <c>typeof(T).Name</c>, i.e. <c>"FWButton"</c>, which markup
/// would never ask for.
///
/// Known WinUI names with no implementation here yet: <c>Pivot</c>, <c>FlipView</c>,
/// <c>RichEditBox</c>, <c>PasswordBox</c>. Markup that uses them fails to resolve rather than
/// silently picking a wrong type.
/// </summary>
internal static class ControlAliases
{
    public static void Register()
    {
        // Buttons and command surfaces
        XamlTypeRegistry.RegisterType<FWButton>("Button");
        XamlTypeRegistry.RegisterType<FWToggleButton>("ToggleButton");
        XamlTypeRegistry.RegisterType<FWHyperlinkButton>("HyperlinkButton");
        XamlTypeRegistry.RegisterType<FWRepeatButton>("RepeatButton");
        XamlTypeRegistry.RegisterType<FWSplitButton>("SplitButton");
        XamlTypeRegistry.RegisterType<FWDropDownButton>("DropDownButton");
        XamlTypeRegistry.RegisterType<FWToggleSplitButton>("ToggleSplitButton");
        XamlTypeRegistry.RegisterType<FWAppBarButton>("AppBarButton");
        XamlTypeRegistry.RegisterType<FWAppBarToggleButton>("AppBarToggleButton");
        XamlTypeRegistry.RegisterType<FWAppBarSeparator>("AppBarSeparator");
        XamlTypeRegistry.RegisterType<FWCommandBar>("CommandBar");
        XamlTypeRegistry.RegisterType<FWCommandBarFlyout>("CommandBarFlyout");
        XamlTypeRegistry.RegisterType<FWMenuBar>("MenuBar");
        XamlTypeRegistry.RegisterType<FWMenuBarItem>("MenuBarItem");
        XamlTypeRegistry.RegisterType<FWMenuFlyout>("MenuFlyout");
        XamlTypeRegistry.RegisterType<FWMenuFlyoutItem>("MenuFlyoutItem");
        XamlTypeRegistry.RegisterType<FWMenuFlyoutSeparator>("MenuFlyoutSeparator");
        XamlTypeRegistry.RegisterType<FWMenuFlyoutSubItem>("MenuFlyoutSubItem");
        XamlTypeRegistry.RegisterType<FWToggleMenuFlyoutItem>("ToggleMenuFlyoutItem");
        XamlTypeRegistry.RegisterType<FWMenuFlyoutPresenter>("MenuFlyoutPresenter");
        XamlTypeRegistry.RegisterType<FWFlyout>("Flyout");

        // Content and text
        XamlTypeRegistry.RegisterType<FWTextBox>("TextBox");
        XamlTypeRegistry.RegisterType<FWTextBlock>("TextBlock");
        XamlTypeRegistry.RegisterType<FWRichTextBlock>("RichTextBlock");
        XamlTypeRegistry.RegisterType<FWCheckBox>("CheckBox");
        XamlTypeRegistry.RegisterType<FWAutoSuggestBox>("AutoSuggestBox");
        XamlTypeRegistry.RegisterType<FWToolTip>("ToolTip");

        // Selection and items
        XamlTypeRegistry.RegisterType<FWComboBox>("ComboBox");
        XamlTypeRegistry.RegisterType<FWListView>("ListView");
        XamlTypeRegistry.RegisterType<FWListViewItem>("ListViewItem");
        XamlTypeRegistry.RegisterType<FWGridView>("GridView");
        XamlTypeRegistry.RegisterType<FWGridViewItem>("GridViewItem");
        XamlTypeRegistry.RegisterType<FWRadioButton>("RadioButton");
        XamlTypeRegistry.RegisterType<FWRadioButtons>("RadioButtons");
        XamlTypeRegistry.RegisterType<FWSelectorBar>("SelectorBar");
        XamlTypeRegistry.RegisterType<FWSelectorBarItem>("SelectorBarItem");
        XamlTypeRegistry.RegisterType<FWTabView>("TabView");
        XamlTypeRegistry.RegisterType<FWTabViewItem>("TabViewItem");
        XamlTypeRegistry.RegisterType<FWTreeView>("TreeView");
        XamlTypeRegistry.RegisterType<FWTreeViewItem>("TreeViewItem");
        XamlTypeRegistry.RegisterType<FWTreeDataGrid>("TreeDataGrid");
        XamlTypeRegistry.RegisterType<FWDataGrid>("DataGrid");
        XamlTypeRegistry.RegisterType<FWItemsRepeater>("ItemsRepeater");

        // Navigation
        XamlTypeRegistry.RegisterType<FWNavigationView>("NavigationView");
        XamlTypeRegistry.RegisterType<FWNavigationViewItem>("NavigationViewItem");
        XamlTypeRegistry.RegisterType<FWFrame>("Frame");
        XamlTypeRegistry.RegisterType<FWBreadcrumbBar>("BreadcrumbBar");

        // Range, status, and dialogs
        XamlTypeRegistry.RegisterType<FWSlider>("Slider");
        XamlTypeRegistry.RegisterType<FWProgressBar>("ProgressBar");
        XamlTypeRegistry.RegisterType<FWProgressRing>("ProgressRing");
        XamlTypeRegistry.RegisterType<FWRatingControl>("RatingControl");
        XamlTypeRegistry.RegisterType<FWNumberBox>("NumberBox");
        XamlTypeRegistry.RegisterType<FWInfoBar>("InfoBar");
        XamlTypeRegistry.RegisterType<FWInfoBadge>("InfoBadge");
        XamlTypeRegistry.RegisterType<FWSeparator>("Separator");
        XamlTypeRegistry.RegisterType<FWTeachingTip>("TeachingTip");
        XamlTypeRegistry.RegisterType<FWContentDialog>("ContentDialog");
        XamlTypeRegistry.RegisterType<FWTaskDialog>("TaskDialog");

        // Disclosure and adaptive layout
        XamlTypeRegistry.RegisterType<FWExpander>("Expander");
        XamlTypeRegistry.RegisterType<FWSplitView>("SplitView");
        XamlTypeRegistry.RegisterType<FWTwoPaneView>("TwoPaneView");
        XamlTypeRegistry.RegisterType<FWParallaxView>("ParallaxView");
        XamlTypeRegistry.RegisterType<FWRefreshContainer>("RefreshContainer");
        XamlTypeRegistry.RegisterType<FWSwipeControl>("SwipeControl");
        XamlTypeRegistry.RegisterType<FWScrollViewer>("ScrollViewer");
        XamlTypeRegistry.RegisterType<FWViewbox>("Viewbox");
        XamlTypeRegistry.RegisterType<FWSettingsCard>("SettingsCard");
        XamlTypeRegistry.RegisterType<FWSettingsExpander>("SettingsExpander");
        XamlTypeRegistry.RegisterType<FWTransitioningContentControl>("TransitioningContentControl");

        // Pickers
        XamlTypeRegistry.RegisterType<FWCalendarView>("CalendarView");
        XamlTypeRegistry.RegisterType<FWCalendarDatePicker>("CalendarDatePicker");
        XamlTypeRegistry.RegisterType<FWDatePicker>("DatePicker");
        XamlTypeRegistry.RegisterType<FWTimePicker>("TimePicker");
        XamlTypeRegistry.RegisterType<FWColorPicker>("ColorPicker");
        XamlTypeRegistry.RegisterType<FWPipsPager>("PipsPager");
        XamlTypeRegistry.RegisterType<FWPersonPicture>("PersonPicture");

        // IconElement family
        XamlTypeRegistry.RegisterType<FWFontIcon>("FontIcon");
        XamlTypeRegistry.RegisterType<FWSymbolIcon>("SymbolIcon");
        XamlTypeRegistry.RegisterType<FWPathIcon>("PathIcon");
        XamlTypeRegistry.RegisterType<FWBitmapIcon>("BitmapIcon");
        XamlTypeRegistry.RegisterType<FWAnimatedIcon>("AnimatedIcon");
        XamlTypeRegistry.RegisterType<FWAnimatedVisualPlayer>("AnimatedVisualPlayer");

        // Code-behind bases. These real CLR types exist so that `: UserControl` compiles in C#;
        // without a matching alias the markup element would resolve to Jalium's base class while
        // the generated partial derives from ours — two different types for one document.
        XamlTypeRegistry.RegisterType<Microsoft.UI.Xaml.Controls.Page>("Page");
        XamlTypeRegistry.RegisterType<Microsoft.UI.Xaml.Controls.UserControl>("UserControl");
    }
}
