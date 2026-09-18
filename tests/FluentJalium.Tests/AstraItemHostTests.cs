using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Tests;

/// <summary>
/// The item-hosting base every list control in stage 5 will sit on: which container the framework
/// generates, whether a container style can be reached at all, and whether Astra's scroll-host style
/// applies to the ScrollViewer a list builds inside itself. Nothing here touches
/// Application.Resources (docs/astra/audits/scrollviewer.md explains why that would poison the
/// pixel claims that run after it).
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraItemHostTests
{
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraItemHostTests(AstraThemeRuntimeFixture fixture) => _fixture = fixture;

    [Fact]
    public void The_list_host_builds_real_containers_in_a_virtualizing_panel()
    {
        // Measured tree: Grid > Border > ScrollViewer > { ItemsPresenter > VirtualizingStackPanel >
        // ListBoxItem…, ScrollBar, ScrollBar }. The containers are per-control CLR types, so there is
        // no shared "item container" theme surface to style at the base - each list control brings
        // its own, which is also how WinUI is organised.
        _fixture.Run(() =>
        {
            var list = new ListBox { ItemsSource = new[] { "a", "b", "c" } };
            PixelHarness.Render(list, 200, 120);

            Assert.NotNull(PixelHarness.Descendant<VirtualizingStackPanel>(list));
            Assert.NotNull(PixelHarness.Descendant<ItemsPresenter>(list));
            var item = PixelHarness.Descendant<ListBoxItem>(list);
            Assert.NotNull(item);
            Assert.NotNull(PixelHarness.Descendant<ScrollViewer>(list));
        });
    }

    [Fact]
    public void A_container_style_reaches_the_generated_item()
    {
        // Locally assigned, because that is the route the list batch will use; the value read back is
        // what proves the style landed rather than the tree merely surviving.
        _fixture.Run(() =>
        {
            var style = new Style(typeof(ListBoxItem));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(11)));
            var list = new ListBox { ItemsSource = new[] { "a", "b" }, ItemContainerStyle = style };
            PixelHarness.Render(list, 200, 120);

            Assert.Equal(new Thickness(11), PixelHarness.Descendant<ListBoxItem>(list)!.Padding);
        });
    }

    [Fact]
    public void The_scroll_host_style_reaches_a_list_owning_viewer()
    {
        // Every list, combo drop-down and menu builds its own ScrollViewer, so the implicit host style
        // is what carries IsTabStop and the transparent surface into those internals.
        _fixture.Run(() =>
        {
            var list = new ListBox { ItemsSource = new[] { "a", "b" } };
            PixelHarness.Render(list, 200, 120);
            var viewer = PixelHarness.Descendant<ScrollViewer>(list)!;

            Assert.False((bool)viewer.GetValue(Control.IsTabStopProperty));
            Assert.Equal(ScrollBarVisibility.Auto, viewer.VerticalScrollBarVisibility);
        });
    }
}
