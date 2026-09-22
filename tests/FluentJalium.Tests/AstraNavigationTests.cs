using FluentJalium.Controls;
using FluentJalium.Motion;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The pane's layout contract. Compact mode is what exposes it: the pane is 48 DIP, an item spends 8 of
/// that on its own margin and 40 on the icon column, so any width the menu host keeps for itself makes
/// the selection pill narrower than the icon that is supposed to sit inside it. Measured on screen first
/// (docs/astra/adaptation/12), pinned here so the reading cannot rot.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraNavigationTests
{
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraNavigationTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    /// <summary>
    /// <c>Auto</c> takes 12 DIP out of the viewport for the bar even while the bar is not shown; that is
    /// what made a 48-DIP compact pane hand its items 36. <c>Hidden</c> reserves nothing, which is why the
    /// pane menu host uses it.
    /// </summary>
    [Theory]
    [InlineData(ScrollBarVisibility.Auto, 36d)]
    [InlineData(ScrollBarVisibility.Hidden, 48d)]
    [InlineData(ScrollBarVisibility.Disabled, 48d)]
    public void The_bar_mode_decides_whether_the_pane_loses_layout_width(ScrollBarVisibility mode, double expected)
    {
        _fixture.Run(() =>
        {
            var panel = new StackPanel();
            for (var index = 0; index < 5; index++)
                panel.Children.Add(new Border { Height = 40 });
            var scroll = new ScrollViewer
            {
                Width = 48,
                Height = 120,
                VerticalScrollBarVisibility = mode,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = panel,
            };
            PixelHarness.Build(scroll, 48, 120);
            Assert.Equal(expected, panel.ActualWidth);
        });
    }

    /// <summary>Hiding the bar must not cost the ability to scroll - only the thumb.</summary>
    [Fact]
    public void A_hidden_bar_still_lets_the_pane_scroll()
    {
        _fixture.Run(() =>
        {
            var panel = new StackPanel();
            for (var index = 0; index < 20; index++)
                panel.Children.Add(new Border { Height = 40 });
            var scroll = new ScrollViewer
            {
                Width = 48,
                Height = 120,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                Content = panel,
            };
            PixelHarness.Build(scroll, 48, 120);
            scroll.ScrollToVerticalOffset(40);
            Assert.Equal(40d, scroll.VerticalOffset);
        });
    }

    [Fact]
    public void A_compact_pane_gives_an_item_its_full_40_dip_icon_column()
    {
        _fixture.Run(() =>
        {
            var view = new FluentNavigationView { Width = 400, Height = 420, IsPaneOpen = false };
            var first = new FluentNavigationItem { Content = "Overview" };
            view.MenuItems.Add(first);
            view.MenuItems.Add(new FluentNavigationItem { Content = "Buttons" });
            PixelHarness.Build(view, 400, 420);
            PixelHarness.Settle();

            var pane = (Border)PixelHarness.Named(view, "PART_PaneRoot")!;
            var menu = PixelHarness.Named(view, "PART_MenuItems")!;
            // The invariant that broke: the menu host gets the whole pane, and the item is that minus its
            // own 4+4 margin. With the bar in Auto the pane lost 12 to a gutter and the pill came out
            // narrower than the icon column it is supposed to sit behind.
            Assert.Equal(pane.ActualWidth, menu.ActualWidth, 0.01);
            Assert.Equal(pane.ActualWidth - 8, first.ActualWidth, 0.01);
            // 48 is the CompactPaneLength the view sets once the width transition has landed.
            Assert.Equal(48d, pane.Width, 0.01);
        });
    }

    /// <summary>
    /// A pane label is one line, full stop. Upstream gets that from a TextBlock default it never has to name
    /// (and restates on the header style, NavigationView_themeresources.xaml:891 / :1062 / :1085); here the
    /// attribute on a ContentPresenter is accepted by the reader and goes nowhere, so the only place the
    /// promise can live is the text element the presenter builds. A long label is what makes the difference
    /// observable: a wrapped line would grow the row past the 36 DIP the pill is sized against.
    /// </summary>
    [Fact]
    public void A_long_pane_label_stays_on_one_line()
    {
        _fixture.Run(() =>
        {
            var view = new FluentNavigationView { Width = 400, Height = 420, IsPaneOpen = true };
            var item = new FluentNavigationItem { Content = "Teaching tip, dialogs and everything else in this pane" };
            view.MenuItems.Add(item);
            PixelHarness.Build(view, 400, 420);
            PixelHarness.Settle();

            var label = PixelHarness.Named(view, "PART_Label")!;
            var text = PixelHarness.Descendant<TextBlock>(label)!;
            Assert.Multiple(
                () => Assert.Equal(TextWrapping.NoWrap, text.TextWrapping),
                () => Assert.Equal(36d, item.ActualHeight, 0.01),
                () => Assert.True(text.ActualHeight < 30d, $"the label took {text.ActualHeight:0.#} DIP, more than one line"));
        });
    }

    // ---------- the token layer: docs/astra/audits/navigation.md §1-§3 ----------

    /// <summary>
    /// Before this batch the pane drew the right colours by reading the palette directly, so there was no
    /// NavigationViewItem* name an application could override. These are upstream's names, and every one of
    /// them has a reader in Styles/Navigation.jalxaml - which is also what the consumption gate in
    /// Resources/AstraResourceKeyTests.cs holds the dictionary to.
    /// </summary>
    [Theory]
    [InlineData("NavigationViewItemBackground")]
    [InlineData("NavigationViewItemBackgroundPointerOver")]
    [InlineData("NavigationViewItemBackgroundPressed")]
    [InlineData("NavigationViewItemBackgroundDisabled")]
    [InlineData("NavigationViewItemBackgroundSelected")]
    [InlineData("NavigationViewItemBackgroundSelectedPointerOver")]
    [InlineData("NavigationViewItemBackgroundSelectedPressed")]
    [InlineData("NavigationViewItemBackgroundSelectedDisabled")]
    [InlineData("NavigationViewItemForeground")]
    [InlineData("NavigationViewItemForegroundPointerOver")]
    [InlineData("NavigationViewItemForegroundPressed")]
    [InlineData("NavigationViewItemForegroundDisabled")]
    [InlineData("NavigationViewItemForegroundSelected")]
    [InlineData("NavigationViewItemForegroundSelectedPointerOver")]
    [InlineData("NavigationViewItemForegroundSelectedPressed")]
    [InlineData("NavigationViewItemForegroundSelectedDisabled")]
    [InlineData("NavigationViewContentBackground")]
    [InlineData("NavigationViewContentGridBorderBrush")]
    [InlineData("NavigationViewSelectionIndicatorForeground")]
    [InlineData("NavigationViewItemButtonMargin")]
    public void Every_navigation_row_the_pane_reads_is_published(string key)
    {
        _fixture.Run(() => Assert.NotNull(Application.Current!.TryFindResource(key)));
    }

    /// <summary>
    /// An alias, not a copy: the row must resolve to the very brush instance the palette retints, or a theme
    /// switch or an <c>ApplyAccent</c> would move the token and leave the navigation row behind. This is the
    /// same identity the FrameworkRetints layer is held to for the same reason.
    /// </summary>
    [Fact]
    public void The_navigation_rows_alias_one_brush_rather_than_copying_it()
    {
        _fixture.Run(() => Assert.Multiple(
            () => Assert.Same(Brush("SubtleFillColorTransparentBrush"), Brush("NavigationViewItemBackground")),
            () => Assert.Same(Brush("SubtleFillColorSecondaryBrush"), Brush("NavigationViewItemBackgroundSelected")),
            () => Assert.Same(Brush("SubtleFillColorTertiaryBrush"), Brush("NavigationViewItemBackgroundPressed")),
            () => Assert.Same(Brush("TextFillColorSecondaryBrush"), Brush("NavigationViewItemForegroundPressed")),
            () => Assert.Same(Brush("TextFillColorDisabledBrush"), Brush("NavigationViewItemForegroundDisabled")),
            () => Assert.Same(Brush("AccentFillColorDefaultBrush"), Brush("NavigationViewSelectionIndicatorForeground")),
            () => Assert.Same(Brush("CardStrokeColorDefaultBrush"), Brush("NavigationViewContentGridBorderBrush")),
            () => Assert.Same(Brush("LayerFillColorDefaultBrush"), Brush("NavigationViewContentBackground"))));
    }

    /// <summary>
    /// The row this batch exists to settle. <c>NavigationViewItemCornerRadius</c> was in the public key list with
    /// a value of 4, and microsoft-ui-xaml carries no such row: the left-pane presenter sets
    /// CornerRadius to OverlayCornerRadius (NavigationView_themeresources.xaml:447), which is 8. So the name was
    /// ControlCornerRadius's value wearing a navigation name, and the pill it rounds was 4 DIP round where WinUI
    /// draws 8. Both halves are asserted: the row the pane now reads, and the fake name staying unpublished.
    /// </summary>
    [Fact]
    public void The_pane_item_is_rounded_at_the_radius_upstream_uses()
    {
        _fixture.Run(() =>
        {
            _ = Pane(out var item);
            var root = (Border)PixelHarness.Named(item, "Root")!;
            Assert.Multiple(
                () => Assert.Equal(CornerRadius("OverlayCornerRadius"), item.CornerRadius),
                () => Assert.Equal(CornerRadius("OverlayCornerRadius"), root.CornerRadius),
                () => Assert.True(item.CornerRadius != CornerRadius("ControlCornerRadius"),
                    "the pane item is back on the 4 DIP control radius"),
                () => Assert.Null(Application.Current!.TryFindResource("NavigationViewItemCornerRadius")));
        });
    }

    /// <summary>
    /// The one metric row this layer can carry at all (upstream's other metric rows are x:Double, which the
    /// reader cannot parse) - asserted where it lands, not just where it is declared: 4,2 is what leaves the
    /// item 8 DIP narrower than the pane, which the compact-mode test above depends on.
    /// </summary>
    [Fact]
    public void The_item_margin_row_reaches_the_item()
    {
        _fixture.Run(() =>
        {
            _ = Pane(out var item);
            Assert.Multiple(
                () => Assert.Equal(new Thickness(4, 2), (Thickness)Resource("NavigationViewItemButtonMargin")),
                () => Assert.Equal((Thickness)Resource("NavigationViewItemButtonMargin"), item.Margin));
        });
    }

    /// <summary>
    /// The selected pill is a translucent subtle brush over the pane, so the claim needs a composited route. It used
    /// to take that route through <c>Host()</c>, and that is what made it wobble: the first capture through the shown
    /// window is not comparable with a later one (the same pair read 30 054 and 16 624 changed pixels across two runs
    /// of this file, because the window's own backdrop settles in between). Wrapping the view in an opaque page and
    /// rendering it costs the window nothing here - the pill lands on the page as <c>Over(page, subtle)</c> - and both
    /// captures become independent, so the claim can stop being a delta and name the colour instead.
    /// </summary>
    [Fact]
    public void Selecting_a_pane_item_moves_pixels_without_a_brand_green()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var plate = PixelHarness.LightPage;
            var subtle = Assert.IsType<SolidColorBrush>(Brush("SubtleFillColorSecondaryBrush")!).Color;
            var pill = PixelHarness.Over(plate, subtle);

            var selected = PaneOnPage(select: true, plate);
            var resting = PaneOnPage(select: false, plate);

            Assert.True(selected.Stable && resting.Stable,
                $"a pane never settled: selected {selected.Rounds} rounds / resting {resting.Rounds} rounds");
            Assert.True(selected.Count(pill) > 6_000,
                $"the selected pill painted {selected.Count(pill)} pixels of {pill} over {plate}; top={selected.Top(4)}");
            Assert.Equal(0, resting.Count(pill));
            Assert.Equal(0, selected.Count(BrandEmerald));
        });
    }

    /// <summary>A fresh two-item pane, optionally with the first item selected, captured on an opaque page.</summary>
    private static PixelHarness.Sample PaneOnPage(bool select, Color plate)
    {
        var view = new FluentNavigationView { Width = 400, Height = 420, IsPaneOpen = true };
        var item = new FluentNavigationItem { Content = "Overview" };
        view.MenuItems.Add(item);
        view.MenuItems.Add(new FluentNavigationItem { Content = "Buttons" });
        if (select)
        {
            view.SelectedItem = item;
        }

        var host = PixelHarness.Backdrop(view, plate);
        PixelHarness.Build(host, 400, 420);
        PixelHarness.Settle(60);
        return PixelHarness.Render(host, 400, 420);
    }

    /// <summary>
    /// The pill's geometry is three x:Double rows upstream (:220-222), so the values ride as literals in the
    /// template - and a literal that a second copy of the same number depends on has to be pinned to it, because
    /// the animator's <c>RestingHeight</c> is what centres the bar against a 36 DIP row. Height 16 is also what
    /// upstream draws, so the three literals are one fact rather than three guesses.
    /// </summary>
    [Fact]
    public void The_indicator_measures_what_the_animator_centres_on()
    {
        _fixture.Run(() =>
        {
            var view = Pane(out var item);
            view.SelectedItem = item;
            PixelHarness.Settle(60);
            var indicator = (Border)PixelHarness.Named(view, "PART_SelectionIndicator")!;
            var accent = ((SolidColorBrush)Brush("NavigationViewSelectionIndicatorForeground")).Color;
            Assert.Multiple(
                () => Assert.Equal(3d, indicator.Width, 0.01),
                () => Assert.Equal(16d, indicator.Height, 0.01),
                () => Assert.Equal(NavigationIndicatorAnimator.RestingHeight, indicator.Height, 0.01),
                () => Assert.Equal(new CornerRadius(2), indicator.CornerRadius),
                () => Assert.Same(Brush("AccentFillColorDefaultBrush"), indicator.Background),
                () => Assert.Equal(PixelHarness.PixelKey(accent), PixelHarness.PixelAt(indicator, 1, 8)));
        });
    }

    /// <summary>
    /// What the alias buys: the row itself never changes between themes, its target does. Read through one
    /// instance, so a copied brush would show the same colour twice and pass silently.
    /// </summary>
    [Fact]
    public void A_navigation_row_follows_the_theme_because_it_aliases_a_theme_token()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var light = ((SolidColorBrush)Brush("NavigationViewItemForegroundPressed")).Color;
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = ((SolidColorBrush)Brush("NavigationViewItemForegroundPressed")).Color;
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            Assert.True(light != dark, $"NavigationViewItemForegroundPressed held {light} across the theme switch");
        });
    }

    /// <summary>
    /// Upstream rows this library deliberately does not publish, each for a reason that is about a surface the
    /// control does not build rather than about a number it cannot read. audits/navigation.md §2 carries the
    /// per-name table; the gate's job is to keep an omission from quietly becoming an invented default.
    /// </summary>
    [Theory]
    // Our item exposes IsSelected, never IsChecked: upstream's four checked pairs belong to a state this type lacks.
    [InlineData("NavigationViewItemBackgroundChecked")]
    [InlineData("NavigationViewItemBackgroundCheckedPointerOver")]
    [InlineData("NavigationViewItemBackgroundCheckedPressed")]
    [InlineData("NavigationViewItemBackgroundCheckedDisabled")]
    [InlineData("NavigationViewItemForegroundChecked")]
    [InlineData("NavigationViewItemForegroundCheckedPointerOver")]
    [InlineData("NavigationViewItemForegroundCheckedPressed")]
    [InlineData("NavigationViewItemForegroundCheckedDisabled")]
    // Upstream's left-pane root is a Grid that draws no border; the eight rows go to a border nobody renders.
    [InlineData("NavigationViewItemBorderBrush")]
    [InlineData("NavigationViewItemBorderBrushPointerOver")]
    [InlineData("NavigationViewItemBorderBrushPressed")]
    [InlineData("NavigationViewItemBorderBrushDisabled")]
    [InlineData("NavigationViewItemBorderBrushSelected")]
    [InlineData("NavigationViewItemBorderBrushSelectedPointerOver")]
    [InlineData("NavigationViewItemBorderBrushSelectedPressed")]
    [InlineData("NavigationViewItemBorderBrushSelectedDisabled")]
    // Read by NavigationBackButton.xaml:26-52, and this control builds no back button.
    [InlineData("NavigationViewButtonBackgroundPointerOver")]
    [InlineData("NavigationViewButtonBackgroundPressed")]
    [InlineData("NavigationViewButtonBackgroundDisabled")]
    [InlineData("NavigationViewButtonForegroundPointerOver")]
    [InlineData("NavigationViewButtonForegroundPressed")]
    [InlineData("NavigationViewButtonForegroundDisabled")]
    // The top pane, which this control does not build at all - all thirteen rows.
    [InlineData("TopNavigationViewItemForeground")]
    [InlineData("TopNavigationViewItemForegroundPointerOver")]
    [InlineData("TopNavigationViewItemForegroundPressed")]
    [InlineData("TopNavigationViewItemForegroundDisabled")]
    [InlineData("TopNavigationViewItemForegroundSelected")]
    [InlineData("TopNavigationViewItemForegroundSelectedPointerOver")]
    [InlineData("TopNavigationViewItemForegroundSelectedPressed")]
    [InlineData("TopNavigationViewItemBackgroundPointerOver")]
    [InlineData("TopNavigationViewItemBackgroundPressed")]
    [InlineData("TopNavigationViewItemBackgroundSelected")]
    [InlineData("TopNavigationViewItemBackgroundSelectedPointerOver")]
    [InlineData("TopNavigationViewItemBackgroundSelectedPressed")]
    [InlineData("TopNavigationViewItemSeparatorForeground")]
    // Upstream gives the icon box a background of its own; ours is a bare presenter with nothing to fill.
    [InlineData("NavigationViewItemIconBackground")]
    // Surfaces this template does not have: a material pane backdrop, a header, a separator.
    [InlineData("NavigationViewDefaultPaneBackground")]
    [InlineData("NavigationViewExpandedPaneBackground")]
    [InlineData("NavigationViewTopPaneBackground")]
    [InlineData("NavigationViewItemHeaderForeground")]
    [InlineData("NavigationViewItemSeparatorForeground")]
    // x:Double rows, which this reader cannot parse at all; their values ride as literals.
    [InlineData("NavigationViewSelectionIndicatorWidth")]
    [InlineData("NavigationViewSelectionIndicatorHeight")]
    [InlineData("NavigationViewSelectionIndicatorRadius")]
    [InlineData("NavigationViewItemOnLeftMinHeight")]
    [InlineData("NavigationViewIconBoxWidth")]
    public void A_row_this_control_does_not_build_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(Application.Current!.TryFindResource(key)));
    }

    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    /// <summary>An open pane with two items, laid out and settled; the first one is handed back unselected.</summary>
    private static FluentNavigationView Pane(out FluentNavigationItem item)
    {
        var view = new FluentNavigationView { Width = 400, Height = 420, IsPaneOpen = true };
        item = new FluentNavigationItem { Content = "Overview" };
        view.MenuItems.Add(item);
        view.MenuItems.Add(new FluentNavigationItem { Content = "Buttons" });
        PixelHarness.Build(view, 400, 420);
        PixelHarness.Settle(60);
        return view;
    }

    private static object Resource(string key) => Application.Current!.TryFindResource(key)!;

    private static Brush Brush(string key) => (Brush)Resource(key);

    private static CornerRadius CornerRadius(string key) => (CornerRadius)Resource(key);
}
