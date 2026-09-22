using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace PopupLadderProbe;

/// <summary>
/// #47's first member was settled on one shape: a read taken after `Settle(n)` frames is not measuring the call
/// that opened the surface, it is measuring "did anything steal this thread's active window during those n
/// frames" - the two named closers (a ContentDialog, a deactivation) both close every light-dismiss popup on the
/// host's overlay, and `FlyoutBase.IsOpen` is `_popup?.IsOpen == true` rather than a stored flag. The submenu
/// test was reshaped onto the frame ladder below. #77 asks the same question of the remaining popup reads in the
/// suite, one rung at a time: for each open call the shipped tests use, what is already true at rung 0 - inside
/// the call that opens it - and what only arrives later?
///
/// Each shape is measured the way the test measures it (same call, same read), and reported at 0/1/2/4/8/20
/// pumped frames. A read that throws is reported as the throw, so "the surface is not in this tree yet" is a
/// reading rather than a silent false. Every shape closes what it opened before the next one starts, so no row
/// below is taken with a stale popup on the overlay.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly int[] Rungs = [0, 1, 2, 4, 8, 20];

    private static Window _host = null!;

    [STAThread]
    private static int Main()
    {
        var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
        renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();
        var application = new Application();
        FluentThemeManager.Apply(application);

        _host = new Window { Content = new Grid(), Width = 480, Height = 360, Title = "Popup ladder host" };
        _host.Show();
        Pump(8);
        if (application.MainWindow is null) application.MainWindow = _host;

        S1_FlyoutIsOpen();
        S2_FlyoutItemSkin();
        S3_ContextMenuIdentity();
        S4_ContextMenuSurface();
        S5_RadiusCopyAfterOpen();
        S6_FlyoutPresenter();
        S7_BarOverflow();
        S8_SuggestionDropdown();
        S9_SurfaceSurvivesClose();
        S10_ItemSkinSurvivesHide();

        _host.Close();
        Pump(4);
        var path = Path.Combine(AppContext.BaseDirectory, "popup-ladder-probe.txt");
        File.WriteAllLines(path, Lines);
        foreach (var line in Lines)
        {
            Console.WriteLine(line);
        }

        Console.WriteLine($"wrote {path}");
        return 0;
    }

    /// <summary>Showing_a_flyout_opens_it_and_hides_it_closes_it: Settle(30) sits between ShowAt and IsOpen.</summary>
    private static void S1_FlyoutIsOpen()
    {
        Section("S1  MenuFlyout.ShowAt -> flyout.IsOpen");
        var anchor = Place(new Button { Content = "host" }, 140, 36);
        Pump(4);
        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuFlyoutItem { Text = "item" });
        Ladder("IsOpen", () => flyout.ShowAt(anchor), () => $"flyout.IsOpen={flyout.IsOpen}");
        flyout.Hide();
        Pump(6);
        Note($"     after Hide: flyout.IsOpen={flyout.IsOpen}");
    }

    /// <summary>The_opened_flyout_paints_our_item_skin: Settle(40) before reading the item's template and its part.</summary>
    private static void S2_FlyoutItemSkin()
    {
        Section("S2  MenuFlyout.ShowAt -> the item wears our template");
        var anchor = Place(new Button { Content = "host" }, 140, 36);
        Pump(4);
        var item = new MenuFlyoutItem { Text = "flyout item" };
        var flyout = new MenuFlyout();
        flyout.Items.Add(item);
        Ladder("skin", () => flyout.ShowAt(anchor), () =>
        {
            var ours = ReferenceEquals(Template(FluentThemeManager.GetStyle("DefaultMenuFlyoutItemStyle")), item.Template);
            return $"item.Template is ours={ours} Named(LayoutRoot)={(Named(item, "LayoutRoot") is null ? "null" : "there")} " +
                   $"realized={item.VisualParent?.GetType().Name ?? "null"}";
        });
        flyout.Hide();
        Pump(6);
    }

    /// <summary>A_context_menu_opens_on_a_point_with_its_own_surface: Settle(40) before IsOpen plus the two rows.</summary>
    private static void S3_ContextMenuIdentity()
    {
        Section("S3  ContextMenu.Open(Point) -> IsOpen, and the control wears our rows");
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = "cut" });
        Ladder("identity", () => menu.Open(new Point(120, 120)), () =>
            $"menu.IsOpen={menu.IsOpen} Background={Key(menu.Background, "MenuFlyoutPresenterBackground")} " +
            $"Template={(menu.Template?.GetType().Name ?? "no-template")}");
        menu.IsOpen = false;
        Pump(6);
    }

    /// <summary>A_context_menu_surface_is_the_frameworks_border_wearing_our_rows_and_upstreams_radius:
    /// Settle(40) before climbing out of the item to the Border the framework built.</summary>
    private static void S4_ContextMenuSurface()
    {
        Section("S4  ContextMenu.Open(Point) -> the copied surface, read by climbing out of the item");
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = "cut" });
        menu.Items.Add(new MenuItem { Header = "copy" });
        Ladder("surface", () => menu.Open(new Point(120, 120)), () =>
        {
            var surface = SurfaceOf(menu.Items[0] as DependencyObject);
            return $"radius={surface.CornerRadius} border={surface.BorderThickness} " +
                   $"Background={Key(surface.Background, "MenuFlyoutPresenterBackground")} localRadius=" +
                   $"{surface.ReadLocalValue(Border.CornerRadiusProperty) != DependencyProperty.UnsetValue} " +
                   $"scrollHost={(HostOf(surface) is null ? "null" : "there")}";
        });
        menu.IsOpen = false;
        Pump(6);
    }

    /// <summary>The_copied_surface_takes_the_radius_and_the_edge_it_is_told_to_take, second half: the surface
    /// follows a later write to the control's radius, and Settle(10) sits between the write and the read.</summary>
    private static void S5_RadiusCopyAfterOpen()
    {
        Section("S5  after the surface is up, menu.CornerRadius = 3 -> the copied surface");
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = "cut" });
        menu.Open(new Point(120, 120));
        Pump(20);
        var before = SurfaceOf(menu.Items[0] as DependencyObject).CornerRadius;
        Ladder("re-copy", () => menu.CornerRadius = new CornerRadius(3), () =>
        {
            var surface = SurfaceOf(menu.Items[0] as DependencyObject);
            return $"control={menu.CornerRadius} surface={surface.CornerRadius} (surface at open was {before})";
        });
        menu.IsOpen = false;
        Pump(6);
    }

    /// <summary>A_flyouts_presenter_is_in_the_tree_but_paints_nothing_of_ours: Settle(40) before the presenter
    /// is found above the flyout item.</summary>
    private static void S6_FlyoutPresenter()
    {
        Section("S6  MenuFlyout.ShowAt -> the flyout's own presenter");
        var anchor = Place(new Button { Content = "host" }, 140, 32);
        Pump(4);
        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuFlyoutItem { Text = "cut" });
        Ladder("presenter", () => flyout.ShowAt(anchor), () =>
        {
            var presenter = PresenterOf(flyout.Items[0] as DependencyObject);
            var control = (Control)presenter;
            return $"{presenter.GetType().Name} radius={control.CornerRadius} border={control.BorderThickness} " +
                   $"localRadius={presenter.ReadLocalValue(Control.CornerRadiusProperty) != DependencyProperty.UnsetValue} " +
                   $"Background={(control.Background is null ? "null" : "set")} host={(HostOf(presenter) is null ? "null" : "there")}";
        });
        flyout.Hide();
        Pump(6);
    }

    /// <summary>The_open_bar_shows_its_overflow_outside_the_surface_this_capture_can_reach: the click on the
    /// ellipsis, then Settle(80), then the two open reads are captured into locals.</summary>
    private static void S7_BarOverflow()
    {
        Section("S7  CommandBar.IsOpen = true -> bar.IsOpen and its Popup.IsOpen");
        var bar = new CommandBar { Width = 420, Height = 48 };
        bar.SecondaryCommands.Add(new AppBarButton { Label = "secondary" });
        _host.Content = bar;
        _host.UpdateLayout();
        Pump(40);
        var more = FindChild<Button>(bar);
        Note($"     ellipsis button realized: {more is not null}");
        Ladder("open", () => bar.IsOpen = true, () =>
        {
            var popup = FindChild<Popup>(bar);
            return $"bar.IsOpen={bar.IsOpen} popup={(popup is null ? "null" : $"IsOpen={popup.IsOpen}")}";
        });
        Note("     ... and the same read after a synthetic click on the ellipsis");
        bar.IsOpen = false;
        Pump(10);
        if (more is not null)
        {
            Ladder("click", () =>
            {
                RaiseMouse(more, UIElement.MouseDownEvent);
                RaiseMouse(more, UIElement.MouseUpEvent);
                more.ReleaseMouseCapture();
            }, () =>
            {
                var popup = FindChild<Popup>(bar);
                return $"bar.IsOpen={bar.IsOpen} popup={(popup is null ? "null" : $"IsOpen={popup.IsOpen}")}";
            });
        }

        bar.IsOpen = false;
        Pump(6);
    }

    /// <summary>The AutoSuggestBox reads (A_long_result_set..., The_open_suggestion_surface...): the open is a
    /// write to Text, so this is the one shape whose delay could be a debounce rather than a build pass.</summary>
    private static void S8_SuggestionDropdown()
    {
        Section("S8  AutoCompleteBox.Text = \"b\" -> IsDropDownOpen and the popup's parts");
        var box = new AutoCompleteBox { Width = 260 };
        Place(box, 260, 32);
        Pump(8);
        box.ItemsSource = new[] { "Apple", "Banana", "Cherry" };
        Ladder("dropdown", () => box.Text = "b", () =>
        {
            var container = Named(_host, "SuggestionsContainer");
            return $"box.IsDropDownOpen={box.IsDropDownOpen} SuggestionsContainer={(container is null ? "null" : "there")} " +
                   $"text={box.Text}";
        });
        box.IsDropDownOpen = false;
        Pump(6);
        Note($"     after closing: box.IsDropDownOpen={box.IsDropDownOpen} SuggestionsContainer=" +
             $"{(Named(_host, "SuggestionsContainer") is null ? "gone" : "still there")}");
    }

    /// <summary>The other half of the question. A rung-0 read proves the open call does the work, but it only
    /// proves <em>that</em> the surface is there - so the probe also asks how long a closed surface stays
    /// readable, which is what decides whether a surface read alone can stand for "the menu is open".
    /// A close-then-read mutation of the suite's context-menu surface test is what put this on the list: the
    /// mutation stayed green, so the question is how long that stays true.</summary>
    private static void S9_SurfaceSurvivesClose()
    {
        Section("S9  ContextMenu opened then closed - how long the copied surface stays in the tree");
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = "cut" });
        menu.Open(new Point(120, 120));
        Pump(4);
        menu.IsOpen = false;
        var elapsed = 0;
        foreach (var rung in Rungs)
        {
            if (rung > elapsed)
            {
                Pump(rung - elapsed);
                elapsed = rung;
            }

            Note($"   closed +{rung,2} frame(s): menu.IsOpen={menu.IsOpen} " + Safe(() =>
            {
                var surface = SurfaceOf(menu.Items[0] as DependencyObject);
                return $"surface radius={surface.CornerRadius} bg={(surface.Background is null ? "null" : "set")} " +
                       $"visibility={surface.Visibility} parent={surface.VisualParent?.GetType().Name ?? "null"}";
            }));
        }
    }

    /// <summary>Same question on the flyout path: does an item keep our template after the flyout is hidden?
    /// If it does, <c>The_opened_flyout_paints_our_item_skin</c> needs its own open read to mean what it says.</summary>
    private static void S10_ItemSkinSurvivesHide()
    {
        Section("S10  MenuFlyout shown then hidden - how long the item keeps our template");
        var anchor = Place(new Button { Content = "host" }, 140, 36);
        Pump(4);
        var item = new MenuFlyoutItem { Text = "flyout item" };
        var flyout = new MenuFlyout();
        flyout.Items.Add(item);
        flyout.ShowAt(anchor);
        Pump(4);
        flyout.Hide();
        var elapsed = 0;
        foreach (var rung in Rungs)
        {
            if (rung > elapsed)
            {
                Pump(rung - elapsed);
                elapsed = rung;
            }

            Note($"   hidden +{rung,2} frame(s): flyout.IsOpen={flyout.IsOpen} " + Safe(() =>
            {
                var ours = ReferenceEquals(Template(FluentThemeManager.GetStyle("DefaultMenuFlyoutItemStyle")), item.Template);
                return $"item.Template is ours={ours} parent={item.VisualParent?.GetType().Name ?? "null"}";
            }));
        }
    }

    // ---------- the ladder ----------

    /// <summary>Run one shape's open call, then read at cumulative 0/1/2/4/8/20 pumped frames. Rung 0 is the
    /// answer to "is this load-bearing": a read that is already true there needs no frames to be true.</summary>
    private static void Ladder(string label, Action open, Func<string> read)
    {
        var elapsed = 0;
        open();
        foreach (var rung in Rungs)
        {
            if (rung > elapsed)
            {
                Pump(rung - elapsed);
                elapsed = rung;
            }

            Note($"   {label} +{rung,2} frame(s): {Safe(read)}");
        }
    }

    private static string Safe(Func<string> read)
    {
        try
        {
            return read();
        }
        catch (Exception failure)
        {
            return $"threw {failure.GetType().Name}: {Shorten(failure.Message)}";
        }
    }

    private static string Shorten(string text) => text.Length <= 90 ? text : text[..90] + "...";

    /// <summary>The same lookup the shipped tests use: the resource the application resolves by name, not a brush
    /// from our palette registry (the presenter keys are framework rows we only forward to).</summary>
    private static string Key(Brush? brush, string expected)
    {
        if (brush is null) return "null";
        var resource = Application.Current!.TryFindResource(expected);
        return ReferenceEquals(resource, brush) ? expected : brush.GetType().Name;
    }

    // ---------- tree reads, copied from the tests they measure ----------

    private static Border SurfaceOf(DependencyObject? inside)
    {
        var current = inside;
        for (var hops = 0; hops < 24 && current is not null; hops++)
        {
            if (current is Border border && border.Background is not null) return border;
            current = VisualTreeHelper.GetParent(current);
        }

        throw new InvalidOperationException("no surfaced Border above the menu item - the popup is not in this tree");
    }

    private static DependencyObject PresenterOf(DependencyObject? inside)
    {
        var current = inside;
        for (var hops = 0; hops < 24 && current is not null; hops++)
        {
            if (current.GetType().Name == "MenuFlyoutPresenter") return current;
            current = VisualTreeHelper.GetParent(current);
        }

        throw new InvalidOperationException("no MenuFlyoutPresenter above the flyout item");
    }

    private static DependencyObject? HostOf(DependencyObject? root)
    {
        if (root is null or not Visual) return null;
        if (root.GetType().Name == "MenuPopupScrollHost") return root;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            if (HostOf(VisualTreeHelper.GetChild(root, index)) is { } match) return match;
        }

        return null;
    }

    private static FrameworkElement? Named(DependencyObject root, string name)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is FrameworkElement element && element.Name == name) return element;
            if (child is not null && Named(child, name) is { } deeper) return deeper;
        }

        return null;
    }

    private static T? FindChild<T>(DependencyObject? root) where T : DependencyObject
    {
        if (root is not Visual visual) return null;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
        {
            var child = VisualTreeHelper.GetChild(visual, index);
            if (child is T match) return match;
            if (FindChild<T>(child) is { } deeper) return deeper;
        }

        return null;
    }

    private static ControlTemplate? Template(Style style) =>
        style.Setters.Cast<object>().OfType<Setter>()
            .FirstOrDefault(setter => (setter.Property?.Name ?? setter.PropertyName) == nameof(Control.Template))
            is { } found ? (ControlTemplate?)found.Value : null;

    // ---------- mounting ----------

    private static FrameworkElement Place(FrameworkElement element, int width, int height)
    {
        element.Width = width;
        element.Height = height;
        if (_host.Width < width + 64) _host.Width = width + 64;
        if (_host.Height < height + 64) _host.Height = height + 64;
        var root = new Grid();
        root.Children.Add(element);
        _host.Content = root;
        if (element is Control control) control.ApplyTemplate();
        _host.UpdateLayout();
        return element;
    }

    private static void RaiseMouse(UIElement element, RoutedEvent routedEvent) =>
        element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = routedEvent,
            Source = element,
        });

    private static void Section(string title)
    {
        Note(string.Empty);
        Note($"=== {title} ===");
    }

    private static void Note(string line) => Lines.Add(line);

    private static void Pump(int frames, int budgetMilliseconds = 800)
    {
        if (frames <= 0) return;
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        var deadline = Environment.TickCount64 + budgetMilliseconds;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || Environment.TickCount64 > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var watchdog = new System.Threading.Timer(_ => dispatcher.InvokeAsync(() => frame.Continue = false));
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds * 2L), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }
}
