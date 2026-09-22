using System.Collections;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace CalendarProbe;

/// <summary>
/// Throwaway probe for #70: the calendar family (Calendar / DatePicker / TimePicker) recorded `tree=0` next to a
/// Button's 4 children, and that reading is what closed #69's `TextOnAccent` row as "reader yes, surface no" and
/// opened #70 as a user-visible defect. The reference source says something else about the shape:
/// `Jalium.UI.Controls/Calendar.cs:768` is an `OnRender` override whose comment at :776 says `CalendarItemStyle` is
/// "normally consumed by PART_CalendarItem" and that this control reads the style setters directly. A control that
/// draws itself has NO visual children by design, so `tree=0` cannot separate "self-drawn" from "renders nothing" -
/// and this library has already been fooled by exactly that distinction (`ScrollBar`).
///
///   draw - mount each family member, a Button and an empty Grid in a shown window at a fixed 300x300 box, then
///          report (1) the recursive visual-child count, ActualWidth/Height and DesiredSize, so a zero-desired-size
///          self-drawn control is distinguishable from a stretched slot, (2) what the runtime's own implicit style
///          for it carries (setter count, triggers, whether any of them is `Control.Template`), (3) what its date
///          defaults are - a calendar with no display date is a different defect from one that cannot draw, and
///          (4) the pixel histogram of its own crop against the empty-Grid baseline, which is the only reading here
///          that can say "it paints". The last leg installs a probe brush under `TextOnAccent` and under
///          `AccentBrush` in a merged dictionary for one dispatcher turn and reports which colour counts moved:
///          if an `OnRender` path reads those names per paint, an alias row reaches pixels that no element property
///          ever shows, which would reverse #69's "no surface" verdict.
///
/// Diagnostic-only: opens its own window, closes it, writes nothing into the user's profile, raises no real input.
/// Glyph ink is #50's known wall, so a day number that does not appear in a histogram is NOT evidence of absence.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = new();

    private static int Main(string[] arguments)
    {
        ThemeLoader.Initialize();
        var application = new Application();
        FluentThemeManager.Apply(application, FluentThemeVariant.Light);

        var mode = arguments.Length > 0 ? arguments[0] : "draw";
        var results = mode switch
        {
            "draw" => Draw(application),
            "select" => Select(),
            _ => new[] { $"unknown mode {mode}" },
        };

        var path = Path.Combine(AppContext.BaseDirectory, $"calendar-{mode}.txt");
        File.WriteAllLines(path, results);
        Console.WriteLine($"wrote {path} ({results.Length} lines)");
        return 0;
    }

    /// <summary>
    /// Does a state reach a control that has no parts to write onto? The calendar draws itself, so a selected day and
    /// a disabled calendar can only show up as pixels - and the only lever is the name its drawing code resolves.
    /// </summary>
    private static string[] Select()
    {
        var accent = FluentThemeManager.CurrentAccentColor;
        var accentKey = $"#{accent.R:X2}{accent.G:X2}{accent.B:X2}";
        Say($"=== the calendar's own accent mark, resting / selected / disabled (token {accentKey}) ===");
        var calendar = new Calendar { Width = 300, Height = 300 };
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        panel.Children.Add(calendar);
        var window = new Window
        {
            Title = "CalendarProbe.Select",
            Content = panel,
            Width = 620,
            Height = 420,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = 220,
            Top = 220,
        };
        window.Show();
        Pump(30);
        window.UpdateLayout();
        Pump(20);

        Report(calendar, "resting", accentKey);
        calendar.SelectedDate = new DateTime(2026, 9, 15);
        Pump(20);
        Report(calendar, "SelectedDate=2026-09-15", accentKey);
        calendar.IsEnabled = false;
        Pump(20);
        Report(calendar, "and disabled", accentKey);
        calendar.IsEnabled = true;
        calendar.SelectedDate = null;
        Pump(20);
        Report(calendar, "back to resting", accentKey);

        window.Close();
        Pump(6);
        return Lines.ToArray();
    }

    private static void Report(FrameworkElement element, string label, string accentKey)
    {
        var histogram = Capture(element);
        var ordered = histogram.OrderByDescending(static pair => pair.Value).Take(8);
        Say($"{label,-24} children={Count(element)} accent={histogram.GetValueOrDefault(accentKey, 0)} "
            + $"distinct={histogram.Count} top: {string.Join(" ", ordered.Select(pair => $"{pair.Key}={pair.Value}"))}");
    }

    private static string[] Draw(Application application)
    {
        Say("=== 1. what each subject is: tree, geometry, its own implicit style, its defaults ===");
        var subjects = new (string Name, FrameworkElement Element)[]
        {
            ("Calendar", new Calendar { Width = 300, Height = 300 }),
            ("DatePicker", new DatePicker { Width = 300, Height = 300 }),
            ("TimePicker", new TimePicker { Width = 300, Height = 300 }),
            ("Button (calibration)", new Button { Content = "ok", Width = 300, Height = 300 }),
            ("Grid (baseline)", new Grid { Width = 300, Height = 300 }),
        };

        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var subject in subjects)
        {
            panel.Children.Add(subject.Element);
        }

        var window = new Window
        {
            Title = "CalendarProbe.Draw",
            Content = panel,
            Width = 1600,
            Height = 460,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = 200,
            Top = 200,
        };
        window.Show();
        Pump(30);
        window.UpdateLayout();
        Pump(30);

        foreach (var (name, element) in subjects)
        {
            Say($"{name,-20} children={Count(element)} actual={element.ActualWidth:0.#}x{element.ActualHeight:0.#} "
                + $"desired={element.DesiredSize.Width:0.#}x{element.DesiredSize.Height:0.#} "
                + $"visible={element.IsVisible} style={DescribeStyle(element)}");
            foreach (var line in Defaults(element))
            {
                Say($"{"",-20} {line}");
            }
        }

        Say("");
        Say("=== 2. does it paint? histogram of each crop against the empty-Grid baseline ===");
        var baseline = Top(Capture(subjects[4].Element));
        Say($"baseline (Grid 300x300) : {baseline}");
        foreach (var (name, element) in subjects[..4])
        {
            var leg = Stable(element);
            Say($"{name,-20} {Top(leg.Histogram)} (two consecutive captures agreed={leg.Agreed}, "
                + $"distinct={leg.Histogram.Count})");
        }

        Say("");
        Say("=== 3. do the names an OnRender path would read reach these pixels? ===");
        foreach (var key in new[] { "TextOnAccent", "AccentBrush" })
        {
            var probe = new SolidColorBrush(Color.FromRgb(0x11, 0x22, 0x33));
            var dictionary = new ResourceDictionary { [key] = probe };
            var merged = application.Resources.MergedDictionaries;
            merged.Add(dictionary);
            try
            {
                Say($"  probe under {key} resolves at app scope to: {Describe(application.TryFindResource(key))}");
                Pump(6);
                foreach (var (name, element) in subjects[..4])
                {
                    var leg = Stable(element);
                    var moved = leg.Histogram.GetValueOrDefault("#112233", 0);
                    Say($"    {name,-20} probe-colour pixels={moved} total={Top(leg.Histogram)}");
                }
            }
            finally
            {
                merged.Remove(dictionary);
                Pump(4);
            }
        }

        window.Close();
        Pump(6);
        return Lines.ToArray();
    }

    private static IEnumerable<string> Defaults(FrameworkElement element) => element switch
    {
        Calendar calendar =>
        [
            $"DisplayDate={calendar.DisplayDate:yyyy-MM-dd} SelectedDate={(calendar.SelectedDate is { } s ? s : DateTimeOffset.MinValue) :yyyy-MM-dd} "
                + $"SelectedDates.Count={calendar.SelectedDates.Count}",
        ],
        DatePicker picker => [$"SelectedDate={(picker.SelectedDate is { } d ? d.ToString("yyyy-MM-dd") : "null")}"],
        TimePicker picker => [$"SelectedTime={(picker.SelectedTime?.ToString() ?? "null")} ClockIdentifier={picker.ClockIdentifier}"],
        _ => [],
    };

    private static string DescribeStyle(FrameworkElement element)
    {
        var style = Application.Current!.TryFindResource(element.GetType()) as Style;
        if (style is null)
        {
            return "no implicit style by type";
        }

        var setters = style.Setters.Cast<object>().OfType<Setter>().Select(static setter => setter.Property?.Name ?? setter.PropertyName ?? "UNRESOLVED").ToList();
        var triggers = style.Triggers.Cast<object>().Select(static trigger => trigger.GetType().Name).ToList();
        return $"setters={setters.Count}[{string.Join(",", setters)}] triggers={triggers.Count} template-cell={setters.Contains("Template")}";
    }

    private static int Count(DependencyObject root)
    {
        var total = 0;
        foreach (var child in ChildrenOf(root))
        {
            total += 1 + Count(child);
        }

        return total;
    }

    private static IEnumerable<DependencyObject> ChildrenOf(DependencyObject root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is not null)
            {
                yield return child;
            }
        }
    }

    private static (Dictionary<string, int> Histogram, bool Agreed) Stable(FrameworkElement element)
    {
        var first = Capture(element);
        Pump(6);
        var second = Capture(element);
        var agreed = first.Count == second.Count && !first.Except(second).Any();
        return (second, agreed);
    }

    private static Dictionary<string, int> Capture(FrameworkElement element)
    {
        var width = (int)Math.Max(1, Math.Ceiling(element.ActualWidth));
        var height = (int)Math.Max(1, Math.Ceiling(element.ActualHeight));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(element);
        var buffer = new byte[width * height * 4];
        bitmap.CopyPixels(buffer, width * 4, 0);
        var histogram = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var offset = 0; offset + 2 < buffer.Length; offset += 4)
        {
            var key = $"#{buffer[offset + 2]:X2}{buffer[offset + 1]:X2}{buffer[offset]:X2}";
            histogram[key] = histogram.GetValueOrDefault(key) + 1;
        }

        return histogram;
    }

    private static string Top(Dictionary<string, int> histogram) => string.Join(" ", histogram
        .OrderByDescending(static pair => pair.Value)
        .Take(5)
        .Select(pair => $"{pair.Key}={pair.Value}"));

    private static string Describe(object? value) => value switch
    {
        null => "null",
        SolidColorBrush brush => $"SolidColorBrush #{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}",
        _ => value.GetType().Name + " " + value,
    };

    private static void Say(string text) => Lines.Add(text);

    private static int Pump(int frames)
    {
        var pumping = Dispatcher.CurrentDispatcher;
        var frame = new DispatcherFrame();
        var seen = 0;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames)
            {
                frame.Continue = false;
            }
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var watchdog = new System.Threading.Timer(
            _ => pumping.InvokeAsync(() => frame.Continue = false), null, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return seen;
    }
}
