using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// ListBox and its item container. The stage 5 plan opens with the list family, and the whole family ships
/// natively: <see cref="ListBox"/> is <c>Selector : ItemsControl : Control</c> with the selection semantics the
/// library owns (its own handlers cover arrow keys, space, drag-select and range select), so AGENTS.md's rule
/// sends this down the re-template path and no <c>Fluent*</c> type appears.
/// <para>
/// The one claim that shapes every test below is measured in adaptation/00 S1-c: a container's style arrives
/// through the implicit type key and the container's own <see cref="FrameworkElement.Style"/> stays null, so
/// nothing here asserts that a style "was applied" - it asserts the effect (the row's padding, the row's fill,
/// the row's width) and reads <c>Style</c> only to pin that it is still null. The same measurement is why the
/// ListBox style deliberately does not set ItemContainerStyle: writing that would outrank an application's own
/// implicit item style, which is the route upstream uses.
/// </para>
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraListBoxTests : IDisposable
{
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);
    private static readonly Color FrameworkSelectionPurple = Color.FromRgb(0x68, 0x00, 0x81);
    private static readonly Color SurfaceSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color AccentSentinel = Color.FromRgb(0x00, 0xE0, 0x00);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraListBoxTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    public void Dispose()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", null);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
        });
    }

    // ---------- the rows ----------

    /// <summary>The nine alias rows, by upstream name and upstream target.</summary>
    [Theory]
    [InlineData("ListBoxForeground", "TextFillColorPrimaryBrush")]
    [InlineData("ListBoxBackground", "SolidBackgroundFillColorBaseBrush")]
    [InlineData("ListBoxBorder", "TextFillColorPrimaryBrush")]
    [InlineData("ListBoxItemForeground", "TextFillColorPrimaryBrush")]
    [InlineData("ListBoxItemForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("ListBoxItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("ListBoxItemBackgroundPressed", "SubtleFillColorTertiaryBrush")]
    [InlineData("ListBoxItemBackgroundSelected", "AccentFillColorDefaultBrush")]
    [InlineData("ListBoxItemBackgroundSelectedPointerOver", "AccentFillColorDefaultBrush")]
    [InlineData("ListBoxItemBackgroundSelectedPressed", "AccentFillColorDefaultBrush")]
    public void An_alias_row_resolves_to_its_target(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    /// <summary>
    /// The three selected rows share one brush instance on purpose: the alpha that separates them rides on the
    /// template part (0.6 / 0.8 / 0.9, from generic.xaml:300-302), so the accent override reaches every one of
    /// them. A row with a baked-in alpha would have been a colour an application accent could not move.
    /// </summary>
    [Fact]
    public void The_three_selected_rows_are_the_same_brush_the_accent_override_moves()
    {
        _fixture.Run(() =>
        {
            Assert.Same(Res("AccentFillColorDefaultBrush"), Res("ListBoxItemBackgroundSelected"));
            Assert.Same(Res("ListBoxItemBackgroundSelected"), Res("ListBoxItemBackgroundSelectedPointerOver"));
            Assert.Same(Res("ListBoxItemBackgroundSelected"), Res("ListBoxItemBackgroundSelectedPressed"));
        });
    }

    [Theory]
    [InlineData("ListBoxBorderThemeThickness", "0,0,0,0")]
    [InlineData("ListBoxItemPadding", "12,9,12,12")]
    public void A_geometry_row_carries_upstreams_value(string key, string value)
    {
        _fixture.Run(() => Assert.Equal(value, Application.Current!.TryFindResource(key)!.ToString()));
    }

    /// <summary>
    /// The fifteen legacy rows are dead in upstream itself - grep over the reference commit finds each name only
    /// in its own three declaration blocks and in the perf2026 twin - so publishing them would put Windows 8.1
    /// hexes into a surface no template reads. Naming them here is what keeps that a decision and not a hole.
    /// </summary>
    [Theory]
    [InlineData("ListBoxBackgroundThemeBrush")]
    [InlineData("ListBoxBorderThemeBrush")]
    [InlineData("ListBoxDisabledForegroundThemeBrush")]
    [InlineData("ListBoxFocusBackgroundThemeBrush")]
    [InlineData("ListBoxForegroundThemeBrush")]
    [InlineData("ListBoxItemDisabledForegroundThemeBrush")]
    [InlineData("ListBoxItemPointerOverBackgroundThemeBrush")]
    [InlineData("ListBoxItemPointerOverForegroundThemeBrush")]
    [InlineData("ListBoxItemPressedBackgroundThemeBrush")]
    [InlineData("ListBoxItemPressedForegroundThemeBrush")]
    [InlineData("ListBoxItemSelectedBackgroundThemeBrush")]
    [InlineData("ListBoxItemSelectedDisabledBackgroundThemeBrush")]
    [InlineData("ListBoxItemSelectedDisabledForegroundThemeBrush")]
    [InlineData("ListBoxItemSelectedForegroundThemeBrush")]
    [InlineData("ListBoxItemSelectedPointerOverBackgroundThemeBrush")]
    public void A_dead_upstream_row_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(Application.Current!.TryFindResource(key)));
    }

    // ---------- the templates land ----------

    [Fact]
    public void The_list_takes_our_style_rather_than_the_framework_chrome()
    {
        _fixture.Run(() =>
        {
            var list = Mount(List());
            var ours = FluentThemeManager.GetStyle("DefaultListBoxStyle");

            // An implicit style never lands in FrameworkElement.Style on this runtime, so the template object is
            // the readable proof that ours is the effective one.
            Assert.Null(list.Style);
            Assert.Same(TemplateOf(ours), list.Template);
            Assert.NotNull(PixelHarness.Named(list, "LayoutRoot"));
            Assert.NotNull(PixelHarness.Named(list, "ScrollViewer"));
            Assert.NotNull(PixelHarness.Named(list, "ItemsPresenter"));
            Assert.Null(PixelHarness.Named(list, "PART_OuterBorder"));
        });
    }

    [Fact]
    public void The_container_carries_our_item_template_and_its_content()
    {
        _fixture.Run(() =>
        {
            var list = Mount(List());
            var item = Container(list);
            Assert.Null(item.Style);
            Assert.NotNull(PixelHarness.Named(item, "PressedBackground"));
            Assert.NotNull(PixelHarness.Named(item, "ContentPresenter"));
            Assert.Null(PixelHarness.Named(item, "PART_BackgroundBorder"));
            var text = PixelHarness.Descendant<TextBlock>(item)!;
            Assert.Equal("Item 1", text.Text);
        });
    }

    /// <summary>
    /// The container's padding is the row's, not the runtime's own 10,6,10,6 - the effect-based read that stands
    /// in for "the implicit item style applied", which the null <c>Style</c> cannot show.
    /// </summary>
    [Fact]
    public void The_row_geometry_is_upstreams_padding_not_the_runtime_default()
    {
        _fixture.Run(() =>
        {
            var item = Container(Mount(List()));
            Assert.Equal(new Thickness(12, 9, 12, 12), item.Padding);
            Assert.True(item.ActualHeight > 36, $"a row at upstream's padding measured {item.ActualHeight:0.##}");
        });
    }

    /// <summary>
    /// The host must not write ItemContainerStyle: measured (S1-c) that it outranks an application's own implicit
    /// item style, which is the only lever a list consumer has over its rows.
    /// </summary>
    [Fact]
    public void The_list_style_leaves_item_container_style_to_the_application()
    {
        _fixture.Run(() => Assert.Null(Mount(List()).ItemContainerStyle));
    }

    // ---------- the geometry the user reported ----------

    /// <summary>
    /// The rows span the surface: the left gap and the right gap are the same number, and the row is as wide as
    /// the list. This is the 12 DIP reservation adaptation/00 S0-u recorded, measured on a list at 1 versus 13 in
    /// the stock host, and the defect the user has reported twice as "它左右的边距没有对齐".
    /// </summary>
    [Fact]
    public void A_row_spans_the_surface_with_equal_gaps()
    {
        _fixture.Run(() =>
        {
            var list = Mount(List());
            var (left, right, width) = Gaps(list);
            Assert.True(width > 200, $"the row measured {width} wide inside a 260 list");
            Assert.Equal(left, right);
            Assert.True(left < 1.5, $"the surface still insets its rows by {left}");
        });
    }

    /// <summary>
    /// The same promise while the bar is actually showing. Without IsOverlayScrollBarEnabled the reservation is
    /// there whether or not the bar paints, which is what makes this a separate fact: 60 rows in a 180-tall list
    /// overflow, and the moment the bar appears the rows used to lose 12 DIP on one side only.
    /// </summary>
    [Fact]
    public void An_overflowing_list_does_not_give_one_side_more_room_than_the_other()
    {
        _fixture.Run(() =>
        {
            var list = List();
            for (var index = 0; index < 60; index++)
            {
                list.Items.Add($"Item {index + 1}");
            }

            var (left, right, width) = Gaps(Mount(list));
            Assert.Equal(left, right);
            Assert.True(width > 250, $"with the bar showing the row shrank to {width}");
        });
    }

    // ---------- behaviour: selection, virtualization, scrolling ----------

    [Fact]
    public void Selecting_by_index_marks_the_container_and_publishes_the_item()
    {
        _fixture.Run(() =>
        {
            var list = Mount(List());
            list.SelectedIndex = 0;
            PixelHarness.Settle(20);
            Assert.Equal("Item 1", list.SelectedItem);
            Assert.True(Container(list).IsSelected);
        });
    }

    /// <summary>
    /// What the item's own peer promises an assistive client, asserted as a capability rather than as a behaviour:
    /// calling <c>Select()</c> on a peer constructed from the container leaves the list's SelectedIndex at -1
    /// (measured in the first run of this file), while the same construction reaches IScrollItemProvider - which
    /// the scroll fact below does exercise. Driving selection through the peer is therefore not claimed here;
    /// audit §5.6 records it as an open input path next to the pointer and keyboard paths.
    /// </summary>
    [Fact]
    public void An_item_peer_exposes_the_selection_and_scroll_providers()
    {
        _fixture.Run(() =>
        {
            var peer = new ListBoxItemAutomationPeer(Container(Mount(List()), 2));
            Assert.IsAssignableFrom<ISelectionItemProvider>(peer.GetPattern(PatternInterface.SelectionItem));
            Assert.IsAssignableFrom<IScrollItemProvider>(peer.GetPattern(PatternInterface.ScrollItem));
        });
    }

    /// <summary>
    /// The selection semantics belong to the framework and the re-template must not disturb them: in Multiple mode
    /// two rows hold the flag at once, and in Single mode the second write pushes the first one out. Both are read
    /// back off the containers, since the row's own fill is what a user sees.
    /// </summary>
    [Fact]
    public void The_selection_mode_still_governs_how_many_rows_are_selected()
    {
        _fixture.Run(() =>
        {
            var list = Mount(List());
            list.SelectionMode = SelectionMode.Multiple;
            var rows = Rows(list);
            rows[0].IsSelected = true;
            rows[1].IsSelected = true;
            PixelHarness.Settle(20);
            Assert.True(rows[0].IsSelected && rows[1].IsSelected, "Multiple mode dropped one of two rows");

            list.SelectionMode = SelectionMode.Single;
            rows[2].IsSelected = true;
            PixelHarness.Settle(20);
            Assert.True(rows[2].IsSelected, "the third row did not take the selection");
            Assert.False(rows[0].IsSelected, "Single mode let a second row keep its selection");
        });
    }

    /// <summary>
    /// The re-template must not cost the list its virtualization: 1000 items still realize a windowful of
    /// containers. This is also why no test here walks the containers to assert a theme - the walk would miss
    /// nine hundred of them.
    /// </summary>
    [Fact]
    public void A_thousand_items_still_realize_a_windowful_of_containers()
    {
        _fixture.Run(() =>
        {
            var list = List(items: 0);
            for (var index = 0; index < 1000; index++)
            {
                list.Items.Add($"Item {index + 1}");
            }

            Mount(list);
            var realized = Rows(list).Count;
            Assert.Equal(1000, list.Items.Count);
            Assert.True(realized is > 0 and < 60, $"{realized} containers realized for 1000 items in a 180 DIP list");
            Assert.NotNull(PixelHarness.Descendant<VirtualizingStackPanel>(list));
        });
    }

    [Fact]
    public void The_row_stays_on_one_line_however_long_the_text_is()
    {
        _fixture.Run(() =>
        {
            var list = List();
            list.Items.Add("A row whose text is far longer than the two hundred and sixty DIP it has been given");
            Mount(list);
            var rows = Rows(list);
            Assert.Equal(rows[0].ActualHeight, rows[^1].ActualHeight, 1);
        });
    }

    [Fact]
    public void Scrolling_moves_the_items_presenter_inside_our_scroll_viewer()
    {
        _fixture.Run(() =>
        {
            var list = List();
            for (var index = 0; index < 60; index++)
            {
                list.Items.Add($"Item {index + 1}");
            }

            Mount(list);
            var first = Container(list).TranslatePoint(new Point(), list).Y;
            var viewer = (ScrollViewer)PixelHarness.Named(list, "ScrollViewer")!;
            viewer.ScrollToVerticalOffset(400);
            PixelHarness.Settle(30);
            var after = Rows(list).Min(row => row.TranslatePoint(new Point(), list).Y);
            Assert.True(after < first - 100, $"the top row moved from {first:0.##} to {after:0.##} after a 400 DIP scroll");
        });
    }

    [Fact]
    public void A_row_partly_off_the_bottom_can_be_brought_fully_into_view()
    {
        _fixture.Run(() =>
        {
            var list = List();
            for (var index = 0; index < 60; index++)
            {
                list.Items.Add($"Item {index + 1}");
            }

            Mount(list);
            var rows = Rows(list);
            var edge = rows[^1];
            var before = edge.TranslatePoint(new Point(), list).Y;
            ((IScrollItemProvider)new ListBoxItemAutomationPeer(edge).GetPattern(PatternInterface.ScrollItem)!).ScrollIntoView();
            PixelHarness.Settle(30);
            var after = edge.TranslatePoint(new Point(), list).Y;
            Assert.True(after < before, $"the last realized row sat at {before:0.##} and stayed at {after:0.##}");
            Assert.True(after + edge.ActualHeight <= list.ActualHeight + 0.5,
                $"after ScrollIntoView the row still hangs below the list at {after:0.##}+{edge.ActualHeight:0.##}");
        });
    }

    // ---------- pixels ----------

    [Fact]
    public void The_surface_paints_its_background_token()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", SurfaceSentinel);
            var capture = PixelHarness.Render(List(items: 3), 260, 180);
            Assert.True(capture.Stable, $"capture never settled: {capture.Top(6)}");
            Assert.True(capture.Count(SurfaceSentinel) > 3_000,
                $"the list surface token did not reach pixels; top={capture.Top(6)}");
        });
    }

    /// <summary>
    /// Selection has to change pixels at a coordinate inside the first row. The capture compares two frames of the
    /// same list rather than a computed colour, because the part carries the accent at 0.6 opacity and the blend
    /// is the renderer's to do - what is asserted is that the state moves the picture and that it moves it toward
    /// the accent the palette is showing, not away from it.
    /// </summary>
    [Fact]
    public void A_selected_row_moves_its_pixels_toward_the_accent()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", AccentSentinel);
            var list = List(items: 3);
            PixelHarness.Build(list, 260, 180);
            PixelHarness.Settle(60);
            var rest = PixelHarness.PixelAt(list, 40, 20);

            list.SelectedIndex = 0;
            PixelHarness.Settle(60);
            var selected = PixelHarness.PixelAt(list, 40, 20);

            Assert.NotEqual(PixelHarness.Hex(rest), PixelHarness.Hex(selected));
            var (restGreen, selectedGreen) = (Green(rest), Green(selected));
            var (restRed, selectedRed) = (Red(rest), Red(selected));
            Assert.True(selectedGreen > restGreen || selectedRed < restRed,
                $"{PixelHarness.Hex(rest)} -> {PixelHarness.Hex(selected)} did not move toward #{PixelHarness.Hex(PixelHarness.PixelKey(AccentSentinel))}");
        });
    }

    /// <summary>
    /// The framework's own row selection paints #99680081 - the OS accent at 60 %, measured in mode I. Ours is
    /// the palette accent, so that purple has no business appearing in a capture of a styled list.
    /// </summary>
    [Fact]
    public void The_selection_is_the_palette_accent_not_the_frameworks_purple()
    {
        _fixture.Run(() =>
        {
            var list = List(items: 3);
            list.SelectedIndex = 0;
            var capture = PixelHarness.Render(list, 260, 180);
            Assert.Equal(0, capture.Count(FrameworkSelectionPurple));
            Assert.Equal(0, capture.Count(BrandEmerald));
        });
    }

    [Fact]
    public void The_list_follows_the_theme()
    {
        _fixture.Run(() =>
        {
            var light = PixelHarness.Render(List(items: 3), 260, 180);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = PixelHarness.Render(List(items: 3), 260, 180);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.True(light.PaintedPixels > 0, $"light capture is empty: {light.Top(6)}");
            Assert.True(dark.PaintedPixels > 0, $"dark capture is empty: {dark.Top(6)}");
            Assert.NotEqual(light.Top(2), dark.Top(2));
        });
    }

    // ---------- helpers ----------

    private static ListBox List(int items = 5)
    {
        var list = new ListBox { Width = 260, Height = 180 };
        for (var index = 0; index < items; index++)
        {
            list.Items.Add($"Item {index + 1}");
        }

        return list;
    }

    private static ListBox Mount(ListBox list)
    {
        PixelHarness.Build(list, 300, 220);
        PixelHarness.Settle(60);
        return list;
    }

    /// <summary>
    /// The realized rows, in the order the virtualizing panel laid them out. There is no
    /// <c>ItemContainerGenerator</c>-equivalent on this runtime (no GetContainerFromIndex), so the containers are
    /// read off the tree - which is sound here and only here, because virtualization means a walk sees the
    /// realized window and nothing else (asserted by the thousand-item fact).
    /// </summary>
    private static List<ListBoxItem> Rows(ListBox list)
    {
        var rows = new List<ListBoxItem>();
        Collect(list, rows);
        Assert.NotEmpty(rows);
        return rows;
    }

    private static void Collect(DependencyObject node, List<ListBoxItem> rows)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if (child is ListBoxItem item)
            {
                rows.Add(item);
            }

            Collect(child!, rows);
        }
    }

    private static ListBoxItem Container(ListBox list, int index = 0)
    {
        var rows = Rows(list);
        Assert.True(index < rows.Count, $"only {rows.Count} containers are realized, and index {index} is not among them");
        return rows[index];
    }

    /// <summary>Left gap, right gap and row width, read off the first container's position in the list.</summary>
    private static (double Left, double Right, double Width) Gaps(ListBox list)
    {
        var row = Container(list);
        var origin = row.TranslatePoint(new Point(), list);
        return (Math.Round(origin.X, 2), Math.Round(list.ActualWidth - origin.X - row.ActualWidth, 2), row.ActualWidth);
    }

    private static ControlTemplate TemplateOf(Style style)
    {
        foreach (var setter in style.Setters.OfType<Setter>())
        {
            if (setter.Property?.Name == "Template")
            {
                return (ControlTemplate)setter.Value!;
            }
        }

        throw new InvalidOperationException("the shipped list style carries no Template setter");
    }

    private static Brush Res(string key) => (Brush)Application.Current!.TryFindResource(key)!;

    private static byte Red(uint packed) => (byte)(packed >> 16);

    private static byte Green(uint packed) => (byte)(packed >> 8);
}
