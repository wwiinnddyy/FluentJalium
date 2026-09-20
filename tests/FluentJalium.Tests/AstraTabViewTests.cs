using FluentJalium.Controls;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The TabView batch's behaviour and read-back facts (docs/astra/audits/tab-view.md). Both types are own, and the
/// first fact is the reason: a <c>TabControl</c> or <c>TabItem</c> handed a template builds no tree at all, so the
/// geometry this control draws - the line the selected tab collapses, the header that pulls 1 DIP wider over it,
/// the body the tab's content moves into - exists nowhere else. Spike/TabViewStyle recorded the native pair's
/// behaviour side by side with a <c>ListBox</c> that does build; these facts hold the own types to the same bar.
///
/// Every state read here is taken off the realized element the trigger was aimed at, not off the markup, because
/// two earlier batches in this repo shipped rows that parsed, built green and never reached a pixel
/// (docs/astra/adaptation/00 S1-f, S1-g).
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraTabViewTests : IDisposable
{
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraTabViewTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    public void Dispose() => GC.SuppressFinalize(this);

    [Fact]
    public void A_tab_view_builds_its_tabs_from_the_style_and_selects_none_of_them()
    {
        // Upstream's contract, and the one thing the runtime's own TabControl does differently: adding tabs does
        // not pick one. The part reads are also the proof the template built at all - the native pair leaves the
        // tree unchanged however the template is delivered.
        _fixture.Run(() =>
        {
            var view = Mounted();
            var strip = Part(view, "TabListView");
            Assert.Multiple(
                () => Assert.IsType<StackPanel>(strip),
                () => Assert.Equal(3, ((Panel)strip).Children.Count),
                () => Assert.True(Tabs(view).All(item => Part(item, "TabContainer") is Border)),
                () => Assert.Equal(-1, view.SelectedIndex),
                () => Assert.Null(view.SelectedItem),
                () => Assert.Null(view.SelectedContent));
        });
    }

    [Fact]
    public void Selecting_a_tab_collapses_its_line_raises_its_border_and_takes_the_body()
    {
        _fixture.Run(() =>
        {
            var view = Mounted();
            view.SelectedIndex = 1;
            PixelHarness.Settle(20);
            var (first, second) = (Tab(view, 0), Tab(view, 1));
            Assert.Multiple(
                () => Assert.False(first.IsSelected),
                () => Assert.True(second.IsSelected),
                () => Assert.Equal(Visibility.Visible, ((FrameworkElement)Part(first, "BottomBorderLine")).Visibility),
                () => Assert.Equal(Visibility.Collapsed, ((FrameworkElement)Part(second, "BottomBorderLine")).Visibility),
                () => Assert.Equal(Thickness("TabViewSelectedItemHeaderMargin"), ((Border)Part(second, "TabContainer")).Margin),
                () => Assert.Equal((Thickness)Resource("TabViewSelectedItemBorderThickness"), ((Border)Part(second, "TabContainer")).BorderThickness),
                () => Assert.Equal(Visibility.Visible, ((FrameworkElement)Part(second, "SelectedBackgroundSurface")).Visibility),
                () => Assert.Equal(Visibility.Collapsed, ((FrameworkElement)Part(first, "SelectedBackgroundSurface")).Visibility),
                () => Assert.Same(second.Content, view.SelectedContent),
                () => Assert.Same(second.Content, ((ContentPresenter)Part(view, "TabContentPresenter")).Content));
        });
    }

    [Fact]
    public void The_selection_event_names_the_tab_it_left_and_the_one_it_arrived_at()
    {
        _fixture.Run(() =>
        {
            var view = Mounted();
            var raised = new List<string>();
            view.SelectionChanged += (_, args) => raised.Add($"{args.OldSelectedItem?.Header}->{args.NewSelectedItem?.Header}");
            view.SelectedIndex = 2;
            view.SelectedIndex = 0;
            view.SelectedIndex = 0;
            Assert.Multiple(
                () => Assert.Equal(new[] { "->tab 3", "tab 3->tab 1" }, raised),
                () => Assert.Equal(0, view.SelectedIndex));
        });
    }

    [Fact]
    public void A_tab_body_shows_the_selected_content_once_and_never_in_the_strip()
    {
        // The item's template has no presenter for Content on purpose: if it had one, the element would be asked
        // to sit in two trees at once and the body would come up empty.
        _fixture.Run(() =>
        {
            var view = Mounted();
            view.SelectedIndex = 0;
            PixelHarness.Settle(20);
            var body = (FrameworkElement)Part(view, "TabContentPresenter");
            var content = Tab(view, 0).Content as FrameworkElement;
            Assert.Multiple(
                () => Assert.NotNull(content),
                () => Assert.Same(body, AncestorOf(content!, typeof(ContentPresenter))),
                () => Assert.Null(AncestorOf(content!, typeof(FluentTabViewItem))));
        });
    }

    [Fact]
    public void A_close_request_names_the_tab_and_leaves_it_where_it_is()
    {
        // Upstream removes nothing: the event is the whole contract, and an app that wants the tab gone calls
        // TabItems.Remove itself.
        _fixture.Run(() =>
        {
            var view = Mounted();
            FluentTabViewTabCloseRequestedEventArgs? requested = null;
            view.TabCloseRequested += (_, args) => requested = args;
            view.SelectedIndex = 1;
            Invoke((Button)Part(Tab(view, 1), "CloseButton"));
            PixelHarness.Settle(6);
            Assert.Multiple(
                () => Assert.NotNull(requested),
                () => Assert.Same(Tab(view, 1), requested!.Item),
                () => Assert.Equal(3, view.TabItems.Count),
                () => Assert.Equal(1, view.SelectedIndex));
        });
    }

    [Fact]
    public void A_closable_tab_that_is_closed_by_the_app_moves_the_selection_to_its_place()
    {
        _fixture.Run(() =>
        {
            var view = Mounted();
            view.SelectedIndex = 1;
            var second = Tab(view, 1);
            view.TabCloseRequested += (_, args) => view.TabItems.Remove(args.Item);
            Invoke((Button)Part(second, "CloseButton"));
            PixelHarness.Settle(10);
            Assert.Multiple(
                () => Assert.Equal(2, view.TabItems.Count),
                () => Assert.DoesNotContain(second, view.TabItems),
                () => Assert.Equal(-1, view.SelectedIndex),
                () => Assert.Null(view.SelectedItem),
                () => Assert.Equal(2, ((Panel)Part(view, "TabListView")).Children.Count));
        });
    }

    [Fact]
    public void The_add_button_is_there_before_it_is_shown_and_reports_only_after_it_is()
    {
        _fixture.Run(() =>
        {
            var view = Mounted();
            var button = (Button)Part(view, "AddButton");
            var clicks = 0;
            var executed = 0;
            view.AddTabButtonClick += (_, _) => clicks++;
            view.AddTabButtonCommand = new DelegateCommand(() => executed++);
            Assert.Multiple(
                () => Assert.False(view.IsAddButtonVisible),
                () => Assert.Equal(Visibility.Collapsed, button.Visibility));
            Invoke(button);
            PixelHarness.Settle(6);
            Assert.Multiple(
                () => Assert.Equal(0, clicks),
                () => Assert.Equal(0, executed));
            view.IsAddButtonVisible = true;
            PixelHarness.Settle(10);
            Invoke(button);
            PixelHarness.Settle(6);
            Assert.Multiple(
                () => Assert.Equal(Visibility.Visible, button.Visibility),
                () => Assert.Equal(1, clicks),
                () => Assert.Equal(1, executed));
        });
    }

    [Fact]
    public void Arrows_browse_the_strip_and_a_pointer_press_is_what_selects()
    {
        // Upstream sets SingleSelectionFollowsFocus=False on the strip (TabView.xaml:56), so these two paths must
        // stay apart: moving focus with the keyboard must not move the selection, and a press must not need focus
        // first. KeyEventArgs cannot be constructed in this build (spike/TabViewProbe section C), so the handlers
        // are driven through the same internal calls they make.
        _fixture.Run(() =>
        {
            var view = Mounted();
            view.SelectedIndex = 0;
            Tab(view, 0).Focus();
            view.MoveFocus(Tab(view, 0), 1);
            PixelHarness.Settle(6);
            Assert.Multiple(
                () => Assert.Equal(0, view.SelectedIndex),
                () => Assert.True(Tab(view, 1).IsKeyboardFocused));
            Tab(view, 2).SelectFromPointer();
            PixelHarness.Settle(6);
            Assert.Equal(2, view.SelectedIndex);
        });
    }

    [Fact]
    public void Ctrl_tab_cycles_selection_through_the_enabled_tabs_only()
    {
        // The accelerator's own target list: a disabled tab is skipped going either way and the cycle wraps.
        _fixture.Run(() =>
        {
            var view = Mounted();
            view.SelectedIndex = 0;
            Tab(view, 1).IsEnabled = false;
            view.MoveSelection(1);
            Assert.Equal(2, view.SelectedIndex);
            view.MoveSelection(1);
            Assert.Equal(0, view.SelectedIndex);
            view.MoveSelection(-1);
            Assert.Equal(2, view.SelectedIndex);
        });
    }

    [Fact]
    public void A_disabled_tab_refuses_a_press_and_loses_its_label_to_the_disabled_row()
    {
        _fixture.Run(() =>
        {
            var view = Mounted();
            var last = Tab(view, 2);
            last.IsEnabled = false;
            last.SelectFromPointer();
            PixelHarness.Settle(6);
            Assert.Multiple(
                () => Assert.Equal(-1, view.SelectedIndex),
                () => Assert.Same(Resource("TabViewItemHeaderForegroundDisabled"), last.Foreground));
        });
    }

    [Fact]
    public void A_tab_without_a_close_button_gives_its_inset_back_to_the_header()
    {
        _fixture.Run(() =>
        {
            var view = Mounted();
            var first = Tab(view, 0);
            var second = Tab(view, 1);
            second.IsClosable = false;
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.Equal(Thickness("TabViewItemHeaderPaddingWithCloseButton"), ((Border)Part(first, "TabContainer")).Padding),
                () => Assert.Equal(Thickness("TabViewItemHeaderPaddingWithoutCloseButton"), ((Border)Part(second, "TabContainer")).Padding),
                () => Assert.Equal(Visibility.Collapsed, ((FrameworkElement)Part(second, "CloseButton")).Visibility));
        });
    }

    [Fact]
    public void An_icon_takes_its_column_only_when_there_is_one()
    {
        _fixture.Run(() =>
        {
            var view = Mounted();
            var first = Tab(view, 0);
            first.Icon = new TextBlock { Text = "#" };
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.True(first.HasIcon),
                () => Assert.Equal(Visibility.Visible, ((FrameworkElement)Part(first, "IconBox")).Visibility),
                () => Assert.Equal(Visibility.Collapsed, ((FrameworkElement)Part(Tab(view, 1), "IconBox")).Visibility),
                () => Assert.Same(Resource("TabViewItemIconForeground"), ((ContentControl)Part(first, "IconHost")).Foreground));
        });
    }

    [Fact]
    public void The_tab_next_to_the_selected_one_steps_its_line_short()
    {
        _fixture.Run(() =>
        {
            var view = Mounted();
            view.SelectedIndex = 1;
            PixelHarness.Settle(20);
            var line = (Border)Part(Tab(view, 0), "BottomBorderLine");
            var right = (Border)Part(Tab(view, 2), "BottomBorderLine");
            Assert.Multiple(
                () => Assert.True(Tab(view, 0).IsLeftOfSelected),
                () => Assert.Equal(new Thickness(0, 0, 2, 0), line.Margin),
                () => Assert.True(Tab(view, 2).IsRightOfSelected),
                () => Assert.Equal(new Thickness(2, 0, 0, 0), right.Margin));
        });
    }

    [Fact]
    public void The_two_buttons_of_the_strip_paint_on_borders_and_not_on_their_presenters()
    {
        // Upstream paints both buttons with the ContentPresenter itself. That presenter declares no Background on
        // this runtime (adaptation/00 S1-h), so the same shape would be ten dead cells; the read-back has to name
        // the element that now holds the fill, and the glyph colour has to be the button's own.
        _fixture.Run(() =>
        {
            var view = Mounted();
            view.IsAddButtonVisible = true;
            PixelHarness.Settle(20);
            var tab = Tab(view, 0);
            var close = (Button)Part(tab, "CloseButton");
            var addPaint = (Button)Part(view, "AddButton");
            Assert.Multiple(
                () => Assert.IsType<Border>(Part(close, "ContentRoot")),
                () => Assert.Same(Resource("TabViewItemHeaderCloseButtonBackground"), ((Border)Part(close, "ContentRoot")).Background),
                () => Assert.Same(Resource("TabViewButtonBackground"), ((Border)Part(addPaint, "ContentRoot")).Background),
                () => Assert.Same(Resource("TabViewItemHeaderCloseButtonForeground"), close.Foreground),
                () => Assert.Equal(new CornerRadius(4, 4, 4, 4), ((Border)Part(close, "ContentRoot")).CornerRadius));
        });
    }

    [Fact]
    public void The_selected_fill_reaches_pixels_and_the_two_themes_disagree_about_it()
    {
        // Upstream paints the selected tab's own fill with TabViewItemHeaderBackgroundSelected, which is the
        // opaque solid tertiary background - so the exact colour has to be findable, in both themes, and not the
        // framework's resting green that the native strip paints (spike/TabViewTint: #1E793F).
        _fixture.Run(() =>
        {
            var selected = FillOf("TabViewItemHeaderBackgroundSelected");
            var dark = Render();
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var light = Render();
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var lightSelected = FillOf("TabViewItemHeaderBackgroundSelected", FluentThemeVariant.Light);
            var green = Color.FromRgb(0x1E, 0x79, 0x3F);
            Assert.Multiple(
                () => Assert.True(dark.Count(selected) > 200, $"selected fill absent in Dark: {dark.Top(4)}"),
                () => Assert.NotEqual(selected, lightSelected),
                () => Assert.Equal(0, dark.Count(green)));
        });
    }

    [Fact]
    public void The_line_under_the_strip_is_one_dip_and_the_header_sits_eight_above_it()
    {
        _fixture.Run(() =>
        {
            var view = Mounted();
            view.SelectedIndex = 0;
            PixelHarness.Settle(20);
            var line = (Border)Part(view, "LeftBottomBorderLine");
            var tab = (Border)Part(Tab(view, 0), "TabContainer");
            var overlay = (CornerRadius)Resource("OverlayCornerRadius");
            Assert.Multiple(
                () => Assert.Equal(1d, line.Height, 0.01),
                () => Assert.Same(Resource("TabViewBorderBrush"), line.Background),
                () => Assert.Equal(Thickness("TabViewHeaderPadding"), view.Padding),
                () => Assert.Equal(32d, Tab(view, 0).ActualHeight, 0.01),
                () => Assert.Equal(24d, ((FrameworkElement)Part(Tab(view, 0), "CloseButton")).ActualHeight, 0.01),
                () => Assert.Equal(new CornerRadius(overlay.TopLeft, overlay.TopRight, 0, 0), tab.CornerRadius));
        });
    }

    /// <summary>
    /// The rows audits/tab-view.md §1 withholds stay unpublished. A deferred row is a claim about the future, and
    /// the only way to keep it from quietly becoming a gap is to read it back: publishing one of these names
    /// without its state existing would promise an app that overriding the name moves pixels, when nothing on this
    /// runtime ever asks for it.
    /// </summary>
    [Theory]
    // The overflow scroll buttons this batch does not ship.
    [InlineData("TabViewScrollButtonBackground")]
    [InlineData("TabViewScrollButtonBackgroundPointerOver")]
    [InlineData("TabViewScrollButtonBackgroundPressed")]
    [InlineData("TabViewScrollButtonBackgroundDisabled")]
    [InlineData("TabViewScrollButtonForeground")]
    [InlineData("TabViewScrollButtonForegroundPointerOver")]
    [InlineData("TabViewScrollButtonForegroundPressed")]
    [InlineData("TabViewScrollButtonForegroundDisabled")]
    [InlineData("TabViewScrollButtonBorderBrush")]
    [InlineData("TabViewScrollButtonBorderBrushPointerOver")]
    [InlineData("TabViewScrollButtonBorderBrushPressed")]
    [InlineData("TabViewScrollButtonBorderBrushDisabled")]
    [InlineData("TabViewItemScrollButtonPadding")]
    [InlineData("TabViewItemLeftScrollButtonContainerPadding")]
    [InlineData("TabViewItemRightScrollButtonContainerPadding")]
    // The active-tab state the + button is not raised into, and the drag surface with no drag path.
    [InlineData("TabViewButtonBackgroundActiveTab")]
    [InlineData("TabViewButtonForegroundActiveTab")]
    [InlineData("TabViewItemHeaderDragBackground")]
    // A metric the item template does not read: the With/WithoutCloseButton pair carries that padding.
    [InlineData("TabViewSelectedItemHeaderPadding")]
    // The x:Double metrics this reader cannot parse, so the literals sit at the use sites instead.
    [InlineData("TabViewItemMinHeight")]
    [InlineData("TabViewItemMinWidth")]
    [InlineData("TabViewItemMaxWidth")]
    [InlineData("TabViewItemHeaderFontSize")]
    [InlineData("TabViewShadowDepth")]
    public void A_withheld_upstream_row_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(Application.Current!.TryFindResource(key)));
    }

    /// <summary>
    /// The order markup uses is the one an index-typed property has to survive: the generator writes attributes
    /// before it appends children, so <c>SelectedIndex="0"</c> lands while <c>TabItems</c> is still empty. A range
    /// check there is not strictness, it is a crash at window start - which is how the Gallery died on the
    /// navigation page before this fact existed.
    /// </summary>
    [Fact]
    public void An_index_written_before_the_tabs_arrive_selects_the_tab_it_named()
    {
        _fixture.Run(() =>
        {
            var view = new FluentTabView { SelectedIndex = 0 };
            Assert.Null(view.SelectedItem);

            for (var index = 0; index < 3; index++)
            {
                view.TabItems.Add(new FluentTabViewItem { Header = $"tab {index + 1}" });
            }

            PixelHarness.Build(view, 420, 160);
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.Equal(0, view.SelectedIndex),
                () => Assert.Same(view.TabItems[0], view.SelectedItem),
                () => Assert.True(view.TabItems[0].IsSelected),
                () => Assert.False(view.TabItems[1].IsSelected));
        });
    }

    [Fact]
    public void An_index_below_minus_one_is_refused_and_an_index_past_the_tabs_is_not()
    {
        _fixture.Run(() =>
        {
            var view = Mounted();
            Assert.Throws<ArgumentOutOfRangeException>(() => view.SelectedIndex = -2);

            // Nothing names a tab past the end, and upstream's SelectedIndex is a plain index property with no
            // range check either: the honest reading is "no selection", not an exception at the worst moment.
            view.SelectedIndex = 9;
            Assert.Multiple(
                () => Assert.Equal(9, view.SelectedIndex),
                () => Assert.Null(view.SelectedItem),
                () => Assert.DoesNotContain(view.TabItems, static tab => tab.IsSelected));
        });
    }

    // ---------- helpers ----------

    private FluentTabView Mounted()
    {
        var view = new FluentTabView();
        for (var index = 0; index < 3; index++)
        {
            view.TabItems.Add(new FluentTabViewItem
            {
                Header = $"tab {index + 1}",
                Content = new TextBlock { Text = $"body {index + 1}" },
            });
        }

        PixelHarness.Build(view, 420, 160);
        PixelHarness.Settle(20);
        return view;
    }

    private PixelHarness.Sample Render()
    {
        var view = Mounted();
        view.SelectedIndex = 1;
        PixelHarness.Settle(20);
        return PixelHarness.Render(view, 420, 160);
    }

    private static FluentTabViewItem Tab(FluentTabView view, int index) => view.TabItems[index];

    private static IReadOnlyList<FluentTabViewItem> Tabs(FluentTabView view) => view.TabItems;

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name}.");

    private static object Resource(string key) => Application.Current!.TryFindResource(key)!;

    private static Thickness Thickness(string key) => (Thickness)Resource(key)!;

    private static Color FillOf(string key, FluentThemeVariant? variant = null)
    {
        var previous = FluentThemeManager.CurrentTheme;
        if (variant != null && variant != previous)
        {
            FluentThemeManager.ApplyTheme(variant!.Value);
            PixelHarness.Settle(6);
        }

        var color = ((SolidColorBrush)Resource(key)).Color;
        if (variant != null && variant != previous)
        {
            FluentThemeManager.ApplyTheme(previous);
            PixelHarness.Settle(6);
        }

        return color;
    }

    private static DependencyObject? AncestorOf(DependencyObject node, Type kind)
    {
        for (var current = VisualTreeHelper.GetParent(node); current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (kind.IsInstanceOfType(current))
            {
                return current;
            }
        }

        return null;
    }

    private static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();
}

/// <summary>A command with no body, for the + button's wiring.</summary>
internal sealed class DelegateCommand : System.Windows.Input.ICommand
{
    private readonly Action _execute;

    internal DelegateCommand(Action execute) => _execute = execute;

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _execute();
}
