using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace DoubleRowProbe;

/// <summary>
/// One question, asked of every spelling this markup reader might accept: can a numeric token row be published at
/// all? S0-b recorded "x:Double 进不去" and a later clause read the failure back as
/// <c>Cannot resolve type 'Double' in namespace 'http://schemas.microsoft.com/winfx/2006/xaml'</c> - a type-resolution
/// error on the *element name*, which leaves open whether another route to the same CLR type is open. Every segment
/// since has paid for that reading with literals (PipsPager, InfoBadge), and InfoBadge is the first one whose
/// numeric row differs per theme, so that difference is only transcribable if a row can carry a number.
///
/// Each candidate goes through the route the library itself uses - <c>XamlReader.Load(Stream)</c> on a file, because
/// S1-q clause 3 measured that a standalone <c>XamlReader.Parse</c> element resolves <c>{ThemeResource}</c>
/// differently from a dictionary - and is read back three times:
///   - the key straight off the parsed dictionary (type *and* value; a row that parses to the wrong number is worse
///     than one that is rejected, because nothing downstream can see it),
///   - the same key through Application.Resources once the dictionary is merged,
///   - a style setter of <c>{ThemeResource N}</c> onto a double property (MinHeight), read off a button in a shown
///     window - the only consuming route a numeric token has.
///
/// The style lives in the same dictionary as the row for the same reason.
///
/// Modes: spellings (default) | pertheme (the row inside ThemeDictionaries, read under Light and Dark).
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private const string Root =
        "xmlns=\"http://schemas.jalium.ui/2024\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"";

    private const string SysRuntime = "xmlns:sys=\"clr-namespace:System;assembly=System.Runtime\"";

    private const string SysCoreLib = "xmlns:sys=\"clr-namespace:System;assembly=System.Private.CoreLib\"";

    private static string _mode = "spellings";

    private static string _out = string.Empty;

    [STAThread]
    private static int Main(string[] arguments)
    {
        _mode = arguments.Length > 0 ? arguments[0].ToLowerInvariant() : "spellings";
        _out = Path.Combine(AppContext.BaseDirectory, "dict");
        Directory.CreateDirectory(_out);
        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            FluentThemeManager.Apply(application);
            if (_mode == "pertheme")
            {
                PerTheme(application);
            }
            else
            {
                Spellings(application);
            }
        }
        catch (Exception exception)
        {
            Note("BOOT", "threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        var path = Path.Combine(AppContext.BaseDirectory, $"double-row-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        foreach (var line in Lines)
        {
            Console.WriteLine(line);
        }

        return 0;
    }

    private static IEnumerable<(string Label, string Namespaces, string Row)> Candidates()
    {
        yield return ("x:Double", string.Empty, "<x:Double x:Key=\"N\">4</x:Double>");
        yield return ("sys:Double(runtime)", SysRuntime, "<sys:Double x:Key=\"N\">4</sys:Double>");
        yield return ("sys:Double(corelib)", SysCoreLib, "<sys:Double x:Key=\"N\">4</sys:Double>");
        yield return ("sys:Double(corelib,4.5)", SysCoreLib, "<sys:Double x:Key=\"N\">4.5</sys:Double>");
        yield return ("sys:String(corelib)", SysCoreLib, "<sys:String x:Key=\"N\">4</sys:String>");
        yield return ("sys:String(4,4,4,4)", SysCoreLib, "<sys:String x:Key=\"N\">4,4,4,4</sys:String>");
        yield return ("sys:TimeSpan(corelib)", SysCoreLib, "<sys:TimeSpan x:Key=\"N\">00:00:04</sys:TimeSpan>");
        yield return ("Thickness(control)", SysCoreLib, "<Thickness x:Key=\"N\">4,4,4,4</Thickness>");
    }

    private static void Spellings(Application application)
    {
        foreach (var (label, namespaces, row) in Candidates())
        {
            var dictionary = LoadDictionary(application, label, namespaces, row);
            if (dictionary is null)
            {
                continue;
            }

            Note(label, $"dict[N]={Read(Peek(dictionary, "N"))}");
            Note(label, $"merged app[N]={Read(Peek(application.Resources, "N"))}");
            Consume(application, label, dictionary);
            Pop(application);
        }
    }

    private static void PerTheme(Application application)
    {
        // The shape InfoBadge needs: one numeric key whose value differs between Light and Dark (upstream's
        // InfoBadgeIconHeight is 8 in the dark dictionary and 9 in the light two).
        var rows = new (string Label, string Namespaces, string Markup)[]
        {
            ("sys:Double(corelib)", SysCoreLib, "<sys:Double x:Key=\"N\">LIGHT</sys:Double>"),
            ("sys:Double(runtime)", SysRuntime, "<sys:Double x:Key=\"N\">LIGHT</sys:Double>"),
            ("sys:Int32(corelib)", SysCoreLib, "<sys:Int32 x:Key=\"N\">LIGHT</sys:Int32>"),
            ("sys:String(corelib)", SysCoreLib, "<sys:String x:Key=\"N\">LIGHT</sys:String>"),
            ("Thickness(control)", SysCoreLib, "<Thickness x:Key=\"N\">LIGHT,LIGHT,LIGHT,LIGHT</Thickness>"),
        };
        foreach (var (label, namespaces, template) in rows)
        {
            var light = template.Replace("LIGHT", "8");
            var dark = template.Replace("LIGHT", "9");
            var markup =
                $"<ResourceDictionary {Root} {namespaces}>" +
                "<ResourceDictionary.ThemeDictionaries>" +
                $"<ResourceDictionary x:Key=\"Light\">{light}</ResourceDictionary>" +
                $"<ResourceDictionary x:Key=\"Dark\">{dark}</ResourceDictionary>" +
                "</ResourceDictionary.ThemeDictionaries></ResourceDictionary>";
            var file = Path.Combine(_out, $"theme-{Sanitize(label)}.jalxaml");
            File.WriteAllText(file, markup);
            try
            {
                using var stream = File.OpenRead(file);
                if (XamlReader.Load(stream) is not ResourceDictionary dictionary)
                {
                    Note("pertheme " + label, "LOAD -> null");
                    continue;
                }

                application.Resources.MergedDictionaries.Add(dictionary);
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
                var lightRead = Read(Peek(application.Resources, "N"));
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
                var darkRead = Read(Peek(application.Resources, "N"));
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
                Note("pertheme " + label, $"light -> {lightRead} / dark -> {darkRead}");
                Pop(application);
            }
            catch (Exception exception)
            {
                Note("pertheme " + label, "LOAD-FAIL " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            }
        }
    }

    /// <summary>
    /// Do the two consuming legs fire at all? "S" writes the row onto a double property (MinHeight) and "SP" onto a
    /// Thickness property (Padding), read through the plain getters - a style setter lands in the style layer, so a
    /// local-value read would report UnsetValue for a row that applied perfectly well. The Thickness candidate is
    /// the positive control for "SP"; without it a dead leg looks like a dead row.
    /// </summary>
    private static void Consume(Application application, string label, ResourceDictionary dictionary)
    {
        if (dictionary["S"] is not Style style || dictionary["SP"] is not Style paddingStyle)
        {
            Note(label, "consume -> styles missing from that dictionary");
            return;
        }

        try
        {
            var doubleHost = new Button { Style = style };
            var thicknessHost = new Button { Style = paddingStyle };
            var host = new Grid { Width = 200, Height = 120 };
            host.Children.Add(doubleHost);
            host.Children.Add(thicknessHost);
            var window = new Window { Content = host, Width = 200, Height = 120 };
            window.Show();
            Pump();
            Note(label, $"consume -> MinHeight={doubleHost.MinHeight} Padding={thicknessHost.Padding} " +
                $"(detached {doubleHost.MinHeight}/{thicknessHost.Padding})");
            window.Close();
            Pump();
        }
        catch (Exception exception)
        {
            Note(label, "consume threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }
    }

    private static ResourceDictionary? LoadDictionary(Application application, string label, string namespaces, string row)
    {
        var file = Path.Combine(_out, $"{Sanitize(label)}.jalxaml");
        File.WriteAllText(file,
            $"<ResourceDictionary {Root} {namespaces}>{row}" +
            "<Style x:Key=\"S\" TargetType=\"Button\"><Setter Property=\"MinHeight\" Value=\"{ThemeResource N}\" /></Style>" +
            "<Style x:Key=\"SP\" TargetType=\"Button\"><Setter Property=\"Padding\" Value=\"{ThemeResource N}\" /></Style>" +
            "</ResourceDictionary>");
        try
        {
            using var stream = File.OpenRead(file);
            if (XamlReader.Load(stream) is not ResourceDictionary dictionary)
            {
                Note(label, "LOAD -> null");
                return null;
            }

            application.Resources.MergedDictionaries.Add(dictionary);
            return dictionary;
        }
        catch (Exception exception)
        {
            Note(label, "LOAD-FAIL " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            return null;
        }
    }

    private static void Pop(Application application) =>
        application.Resources.MergedDictionaries.RemoveAt(application.Resources.MergedDictionaries.Count - 1);

    private static void Pump(int frames = 4, int budgetMilliseconds = 800)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        var deadline = Environment.TickCount64 + budgetMilliseconds;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || Environment.TickCount64 > deadline)
            {
                frame.Continue = false;
            }
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var watchdog = new System.Threading.Timer(_ => dispatcher.InvokeAsync(() => frame.Continue = false));
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds * 2L), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }

    private static string Sanitize(string label) => new(label.Select(static c => char.IsLetterOrDigit(c) ? c : '-').ToArray());

    private static object? Peek(ResourceDictionary dictionary, string key)
    {
        try
        {
            return dictionary[key];
        }
        catch (Exception exception)
        {
            return "<" + exception.GetType().Name + ">";
        }
    }

    private static string Read(object? value) => value is null ? "null" : $"{value.GetType().Name}={value}";

    private static string Trim(string? text) => (text ?? string.Empty).Replace('\n', ' ').Replace('\r', ' ').Trim();

    private static void Note(string tag, string message) => Lines.Add($"{tag}: {message}");
}
