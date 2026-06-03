using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Documents;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentNavigationControlsTests
{
    private static readonly string[] NavigationResourceKeys =
    [
        "NavigationViewPaneBackground",
        "NavigationViewContentBackground",
        "NavigationViewItemBackground",
        "NavigationViewItemBackgroundHover",
        "NavigationViewItemBackgroundPressed",
        "NavigationViewItemBackgroundSelected",
        "NavigationViewItemBackgroundSelectedHover",
        "NavigationViewItemForeground",
        "NavigationViewItemForegroundSecondary",
        "NavigationViewItemForegroundDisabled",
        "TabStripBackground",
        "TabStripBorder",
        "TabContentBackground",
        "TabItemHoverBackground",
        "TabItemSelectedBackground",
        "TabItemIndicator",
        "FrameBackground",
        "FrameBorderBrush"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeNavigationTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in NavigationResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwNavigationStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWNavigationView, NavigationView>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItem, NavigationViewItem>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemHeader, NavigationViewItemHeader>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemSeparator, NavigationViewItemSeparator>(app.Resources);
            AssertBasedOnStyle<FWTabControl, TabControl>(app.Resources);
            AssertBasedOnStyle<FWTabItem, TabItem>(app.Resources);
            AssertBasedOnStyle<FWFrame, Frame>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineNavigationBaseStylesAndFluentSetters()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        var navigationViewStyle = AssertStyle<NavigationView>(dictionary);
        AssertSetter(navigationViewStyle, Control.BackgroundProperty);
        AssertSetter(navigationViewStyle, NavigationView.PaneBackgroundProperty);
        AssertSetter(navigationViewStyle, NavigationView.ContentBackgroundProperty);

        var navigationItemStyle = AssertStyle<NavigationViewItem>(dictionary);
        AssertSetter(navigationItemStyle, Control.BackgroundProperty);
        AssertSetter(navigationItemStyle, TextElement.ForegroundProperty);
        AssertSetter(navigationItemStyle, Control.CornerRadiusProperty);
        AssertSetter(navigationItemStyle, FrameworkElement.MarginProperty);

        var tabControlStyle = AssertStyle<TabControl>(dictionary);
        AssertSetter(tabControlStyle, Control.BackgroundProperty);
        AssertSetter(tabControlStyle, TabControl.TabStripBackgroundProperty);
        AssertSetter(tabControlStyle, TabControl.TabStripBorderBrushProperty);
        AssertSetter(tabControlStyle, TabControl.TabStripHeightProperty);

        var frameStyle = AssertStyle<Frame>(dictionary);
        AssertSetter(frameStyle, Control.BackgroundProperty);
        AssertSetter(frameStyle, Control.BorderBrushProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWNavigationView_ShouldExposePaneHeaderFooterModesAndSelectionEvents()
    {
        var navigationView = new FWNavigationView
        {
            PaneTitle = "FluentJalium",
            PaneHeader = new FWTextBox { Text = "Search" },
            PaneFooter = new FWTextBlock { Text = "vNext" },
            PaneDisplayMode = NavigationViewPaneDisplayMode.Left,
            IsBackButtonVisible = NavigationViewBackButtonVisible.Visible,
            IsBackEnabled = true,
            OpenPaneLength = 220,
            CompactPaneLength = 48
        };
        var home = new FWNavigationViewItem { Content = "Home" };
        var controls = new FWNavigationViewItem { Content = "Controls", IsExpanded = true };
        var documentOptions = new FWNavigationViewItem
        {
            Content = "Document options",
            SelectsOnInvoked = false
        };
        documentOptions.MenuItems.Add(new FWNavigationViewItem { Content = "Create new" });
        var settings = new FWNavigationViewItem { Content = "Settings" };
        var itemInvoked = 0;
        var selectionChanged = 0;
        var opened = 0;
        var closed = 0;

        navigationView.ItemInvoked += (_, _) => itemInvoked++;
        navigationView.SelectionChanged += (_, _) => selectionChanged++;
        navigationView.PaneOpened += (_, _) => opened++;
        navigationView.PaneClosed += (_, _) => closed++;
        navigationView.MenuItems.Add(home);
        navigationView.MenuItems.Add(controls);
        navigationView.MenuItems.Add(documentOptions);
        navigationView.FooterMenuItems.Add(settings);
        navigationView.SelectedItem = home;
        navigationView.SelectedItem = controls;
        InvokeNavigationItem(navigationView, documentOptions);
        navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
        navigationView.IsPaneOpen = false;
        navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
        navigationView.IsPaneOpen = true;

        Assert.Equal("FluentJalium", navigationView.PaneTitle);
        Assert.IsType<FWTextBox>(navigationView.PaneHeader);
        Assert.IsType<FWTextBlock>(navigationView.PaneFooter);
        Assert.Single(navigationView.FooterMenuItems);
        Assert.Equal(NavigationViewPaneDisplayMode.Top, navigationView.PaneDisplayMode);
        Assert.Equal(NavigationViewBackButtonVisible.Visible, navigationView.IsBackButtonVisible);
        Assert.True(navigationView.IsBackEnabled);
        Assert.True(documentOptions.IsExpanded);
        Assert.Equal(3, itemInvoked);
        Assert.Equal(2, selectionChanged);
        Assert.Equal(controls, navigationView.SelectedItem);
        Assert.Equal(1, closed);
        Assert.Equal(1, opened);
    }

    [Fact]
    public void FWTabControl_ShouldExposePlacementSwipeAndDisabledTabState()
    {
        var first = new FWTabItem
        {
            Header = "Overview",
            Content = "Overview content"
        };
        var second = new FWTabItem
        {
            Header = "Details",
            Content = "Details content"
        };
        var disabled = new FWTabItem
        {
            Header = "Disabled",
            Content = "Disabled content",
            IsEnabled = false
        };
        var tabControl = new FWTabControl
        {
            TabStripPlacement = Jalium.UI.Controls.Dock.Left,
            IsSwipeEnabled = false
        };
        tabControl.Items.Add(first);
        tabControl.Items.Add(second);
        tabControl.Items.Add(disabled);

        tabControl.SelectedIndex = 1;
        tabControl.TabStripPlacement = Jalium.UI.Controls.Dock.Bottom;
        tabControl.IsSwipeEnabled = true;

        Assert.Equal(Jalium.UI.Controls.Dock.Bottom, tabControl.TabStripPlacement);
        Assert.True(tabControl.IsSwipeEnabled);
        Assert.True(second.IsSelected);
        Assert.False(first.IsSelected);
        Assert.False(disabled.IsEnabled);
        Assert.Equal("Details content", tabControl.SelectedContent);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises Frame page activation by type.")]
    public void FWFrame_ShouldNavigateWithBackForwardStackAndCache()
    {
        var frame = new FWFrame
        {
            CacheSize = 2
        };
        var navigated = 0;
        frame.Navigated += (_, _) => navigated++;

        Assert.True(frame.Navigate(typeof(NavigationOverviewPage), "overview"));
        Assert.True(frame.Navigate(typeof(NavigationDetailsPage), "details"));
        Assert.Equal(typeof(NavigationDetailsPage), frame.SourcePageType);
        Assert.True(frame.CanGoBack);
        Assert.Equal(1, frame.BackStackDepth);

        Assert.True(frame.GoBack());
        Assert.Equal(typeof(NavigationOverviewPage), frame.SourcePageType);
        Assert.True(frame.CanGoForward);
        Assert.Equal("overview", frame.CurrentPage!.NavigationParameter);

        Assert.True(frame.GoForward());
        Assert.Equal(typeof(NavigationDetailsPage), frame.SourcePageType);
        Assert.Equal("details", frame.CurrentPage!.NavigationParameter);
        Assert.Equal(4, navigated);
    }

    private sealed class NavigationOverviewPage : Page
    {
    }

    private sealed class NavigationDetailsPage : Page
    {
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static Style AssertStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        return Assert.IsType<Style>(value);
    }

    private static void AssertBasedOnStyle<TFluentControl, TJaliumControl>(ResourceDictionary dictionary)
        where TFluentControl : TJaliumControl, IFluentJaliumControl
        where TJaliumControl : FrameworkElement
    {
        var baseStyle = AssertStyle<TJaliumControl>(dictionary);
        var fluentStyle = AssertStyle<TFluentControl>(dictionary);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Same(baseStyle, fluentStyle.BasedOn);
    }

    private static void AssertSetter(Style style, DependencyProperty property)
    {
        Assert.Contains(style.Setters, setter => setter.Property == property);
    }

    private static void InvokeNavigationItem(NavigationView navigationView, NavigationViewItem item)
    {
        var method = typeof(NavigationView).GetMethod("HandleItemClicked", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method.Invoke(navigationView, [item]);
    }

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }
}
