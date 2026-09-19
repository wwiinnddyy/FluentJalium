using FluentJalium.Controls;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;

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
}
