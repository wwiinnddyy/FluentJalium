using System.Collections;
using System.Reflection;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;

namespace AstraSpike;

/// <summary>
/// Stage-0 gate. Prints one line per measured fact and exits; it asserts nothing on purpose,
/// because the point is to learn what Jalium actually does, not to be green.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];
    private static Application _application = null!;

    [STAThread]
    private static int Main()
    {
        DumpSurface();

        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            _application = new Application();
            ResourceDictionary.CurrentThemeKey = "Light";
            Run(_application);
        }
        catch (Exception exception)
        {
            Note("BOOT", exception);
        }

        foreach (var line in Lines) Console.WriteLine(line);
        return 0;
    }

    private static void Run(Application application)
    {
        var palette = LoadEmbedded(application, "Embedded/Palette.jalxaml");
        var styles = LoadEmbedded(application, "Embedded/Styles.jalxaml");
        LoadEmbedded(application, "Embedded/Doubles.jalxaml");
        LoadEmbedded(application, "Embedded/ThemedDoubles.jalxaml");
        LoadEmbedded(application, "Embedded/StaticConsumer.jalxaml");
        var alias = LoadEmbedded(application, "Embedded/Alias.jalxaml");
        LoadEmbedded(application, "Embedded/VsmStoryboard.jalxaml");
        LoadEmbedded(application, "Embedded/VsmWinUiSetters.jalxaml");
        ProbeCompiled();
        if (alias is not null)
            Note("S0-c", $"alias value resolves to {alias["SpikeAliasBrush"]?.GetType().Name ?? "null"} (expected SolidColorBrush)");
        if (palette is null) { Note("S0-a", "palette dictionary absent; measurements skipped"); return; }
        if (styles is null) return;

        var themeButton = new Button { Style = styles["ThemeBrushButtonStyle"] as Style, Content = "theme" };
        var staticButton = new Button { Style = application.Resources["StaticBrushButtonStyle"] as Style, Content = "static" };
        var tokenBorder = new Border { Style = styles["TokenBorderStyle"] as Style, Child = new TextBlock { Text = "tokens" } };
        var doubleButton = new Button { Style = styles["TokenDoubleButtonStyle"] as Style, Content = "double" };
        var durationButton = new Button { Style = styles["TokenDurationButtonStyle"] as Style, Content = "duration" };

        var root = new StackPanel();
        root.Children.Add(themeButton);
        root.Children.Add(staticButton);
        root.Children.Add(tokenBorder);
        root.Children.Add(doubleButton);
        root.Children.Add(durationButton);

        var window = new Window { Content = root, Width = 640, Height = 480, Title = "Astra spike" };
        window.Loaded += (_, _) =>
        {
            try
            {
                MeasureThemeSwitch(themeButton, staticButton, tokenBorder, doubleButton, durationButton);
            }
            catch (Exception exception)
            {
                Note("S0-a", "MEASURE-THREW " + exception.GetType().Name + ": " + Trim(exception.Message));
            }
            MeasureVisualStates(application, root);
            window.Close();
        };
        application.Run(window);
    }

    /// <summary>
    /// Each VSM subject is built and applied inside its own guard: applying a template that
    /// declares VisualStateGroups has been observed to throw, and that must not erase the
    /// results measured above it.
    /// </summary>
    private static void MeasureVisualStates(Application application, Panel host)
    {
        foreach (var label in new[] { "VsmStoryboardStyle", "VsmWinUiSettersStyle" })
        {
            try
            {
                var button = new Button { Style = application.Resources[label] as Style, Content = label };
                if (button.Style is null) { Note("S0-d", $"{label}: style not found"); continue; }
                host.Children.Add(button);
                MeasureVisualStates(button, label);
            }
            catch (Exception exception)
            {
                Note("S0-d", $"{label}: APPLY-THREW {exception.GetType().Name}: {Trim(exception.Message)}");
            }
        }
    }

    private static void MeasureThemeSwitch(Button themeButton, Button staticButton, Border tokenBorder, Button doubleButton, Button durationButton)
    {
        var lightBrushInstance = themeButton.Background;
        var lightRadius = tokenBorder.CornerRadius.ToString();
        var lightPadding = tokenBorder.Padding.ToString();
        var lightWidth = doubleButton.Width.ToString();
        var lightDuration = durationButton.TransitionDuration.ToString();
        Note("S0-a", $"baseline Light: theme={Color(themeButton)} static={Color(staticButton)}");

        ResourceDictionary.CurrentThemeKey = "Dark";
        Note("S0-a", $"[1] CurrentThemeKey=Dark alone: theme={Color(themeButton)} static={Color(staticButton)}");

        ThemeManager.ApplyTheme(ThemeVariant.Dark);
        Note("S0-a", $"[2] + ThemeManager.ApplyTheme(Dark): theme={Color(themeButton)} static={Color(staticButton)} brushIdentityPreserved={ReferenceEquals(lightBrushInstance, themeButton.Background)}");
        Note("S0-b", $"CornerRadius {lightRadius} -> {tokenBorder.CornerRadius}; Padding {lightPadding} -> {tokenBorder.Padding}");
        Note("S0-b", $"Width(x:Double) {lightWidth} -> {doubleButton.Width}; TransitionDuration {lightDuration} -> {durationButton.TransitionDuration}");

        ResourceDictionary.CurrentThemeKey = "HighContrast";
        ThemeManager.ApplyTheme(ThemeVariant.Light);
        ResourceDictionary.CurrentThemeKey = "HighContrast";
        Note("S0-b", $"HighContrast via CurrentThemeKey: theme={Color(themeButton)} radius={tokenBorder.CornerRadius} width={doubleButton.Width}");

        var modeProperty = typeof(Application).GetProperty("ThemeMode");
        var modeType = modeProperty?.PropertyType;
        if (modeType is null) { Note("S0-a", "Application.ThemeMode absent"); return; }
        Note("S0-a", $"ThemeMode is {modeType.FullName}, {modeType.Name} members: {string.Join(", ", modeType.GetMembers(BindingFlags.Public | BindingFlags.Static).Select(static member => member.Name))}");
        foreach (var staticMember in modeType.GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            var value = staticMember.GetValue(null);
            try
            {
                modeProperty.SetValue(_application, value);
            }
            catch (Exception exception)
            {
                Note("S0-a", $"ThemeMode.{staticMember.Name}: rejected ({exception.InnerException?.GetType().Name ?? exception.GetType().Name})");
                continue;
            }
            Note("S0-a", $"[ThemeMode.{staticMember.Name}]: theme={Color(themeButton)} static={Color(staticButton)} radius={tokenBorder.CornerRadius} padding={tokenBorder.Padding} duration={durationButton.TransitionDuration} width={doubleButton.Width}");
        }
        ResourceDictionary.CurrentThemeKey = "HighContrast";
        Note("S0-b", $"ThemeMode.Dark + forced CurrentThemeKey=HighContrast: theme={Color(themeButton)} radius={tokenBorder.CornerRadius} duration={durationButton.TransitionDuration}");
        modeProperty.SetValue(_application, Enum.GetNames(modeType).Length > 0 ? modeType.GetProperty("Light")?.GetValue(null) : null);
        Note("S0-b", $"back to ThemeMode.Light (CurrentThemeKey left at HighContrast): theme={Color(themeButton)} radius={tokenBorder.CornerRadius} duration={durationButton.TransitionDuration}");
    }

    private static void MeasureVisualStates(Button button, string label)
    {
        button.ApplyTemplate();
        var onControl = Count(VisualStateManager.GetVisualStateGroups(button));
        var host = VisualTreeHelper.GetChild(button, 0);
        var onTemplateRoot = host is null ? -1 : Count(VisualStateManager.GetVisualStateGroups(host as FrameworkElement ?? throw new InvalidOperationException()));
        var went = VisualStateManager.GoToState(button, "PointerOver", false);
        Note("S0-d", $"{label}: groupsOnControl={onControl} groupsOnTemplateRoot={onTemplateRoot} GoToState(PointerOver)={went} rootOpacity={OpacityOfNamedPart(host, "Root")}");
        VisualStateManager.GoToState(button, "Normal", false);
        Note("S0-d", $"{label}: back to Normal rootOpacity={OpacityOfNamedPart(host, "Root")}");
    }

    private static string OpacityOfNamedPart(DependencyObject? current, string name)
    {
        if (current is null) return "no-tree";
        if (current is FrameworkElement element && element.Name == name && current is UIElement ui) return ui.Opacity.ToString();
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(current); index++)
        {
            var found = OpacityOfNamedPart(VisualTreeHelper.GetChild(current, index), name);
            if (found != "no-tree") return found;
        }
        return "no-tree";
    }

    private static int Count(object? groups) => groups is ICollection collection ? collection.Count : groups is null ? -1 : -2;

    private static string Color(Button button) =>
        (button.Background as SolidColorBrush)?.Color.ToString() ?? button.Background?.GetType().Name ?? "null";

    private static ResourceDictionary? LoadEmbedded(Application application, string resourceName)
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null) { Note("LOAD", $"{resourceName}: not an embedded resource"); return null; }
        try
        {
            var dictionary = XamlReader.Load(stream) as ResourceDictionary;
            if (dictionary is null) { Note("LOAD", $"{resourceName}: parsed to null/non-dictionary"); return null; }
            var themeKeys = dictionary.ThemeDictionaries is IDictionary themed ? string.Join(",", themed.Keys.OfType<object>().Select(static key => key.ToString())) : dictionary.ThemeDictionaries?.GetType().Name ?? "null";
            Note("LOAD", $"{resourceName}: OK directKeys={dictionary.Count} themeDictionaries={themeKeys}");
            application.Resources.MergedDictionaries.Add(dictionary);
            DescribeThemeEntries(resourceName, dictionary);
            return dictionary;
        }
        catch (TargetInvocationException exception)
        {
            Note("LOAD", $"{resourceName}: PARSE-FAIL {exception.InnerException?.GetType().Name}: {Trim(exception.InnerException?.Message)}");
            return null;
        }
        catch (Exception exception)
        {
            Note("LOAD", $"{resourceName}: PARSE-FAIL {exception.GetType().Name}: {Trim(exception.Message)}");
            return null;
        }
    }

    private static void DescribeThemeEntries(string resourceName, ResourceDictionary dictionary)
    {
        if (dictionary.ThemeDictionaries is not IDictionary themed) return;
        foreach (DictionaryEntry entry in themed)
        {
            if (entry.Value is not IDictionary inner) continue;
            foreach (DictionaryEntry token in inner)
                Note("TOKEN", $"{resourceName} {entry.Key}.{token.Key} -> {token.Value?.GetType().Name ?? "null"}");
        }
    }

    private static void ProbeCompiled()
    {
        Type[] types = [];
        try
        {
            types = typeof(Program).Assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            types = [.. exception.Types.OfType<Type>()];
            Note("S0-e", $"ReflectionTypeLoadException, {exception.LoaderExceptions.Length} loader errors");
        }
        Note("S0-e", $"assembly types: {types.Length}; ResourceDictionary-derived: [{string.Join(", ", types.Where(static type => typeof(ResourceDictionary).IsAssignableFrom(type)).Select(static type => type.FullName))}]");
        Note("S0-e", $"VisualStateGroups attached property type: {typeof(VisualStateManager).GetField("VisualStateGroupsProperty", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)?.GetType().Name ?? "no public DP field"}");
        var candidates = types.Where(static type => typeof(ResourceDictionary).IsAssignableFrom(type) && !type.IsAbstract).ToList();
        Note("S0-e", $"ResourceDictionary types emitted by the generator: {candidates.Count}");
        foreach (var candidate in candidates)
        {
            try
            {
                if (Activator.CreateInstance(candidate) is not ResourceDictionary dictionary) { Note("S0-e", $"{candidate.Name}: instantiated to null"); continue; }
                Note("S0-e", $"{candidate.Name}: directKeys={dictionary.Count} themeDictionaries={dictionary.ThemeDictionaries?.GetType().Name ?? "null"}");
                foreach (var key in dictionary.Keys.OfType<object>())
                    Note("S0-e", $"  {candidate.Name}[{key}] -> {dictionary[key]?.GetType().Name ?? "null"} = {dictionary[key]}");
                if (dictionary.ThemeDictionaries is IDictionary themed)
                {
                    foreach (DictionaryEntry entry in themed)
                    {
                        if (entry.Value is not IDictionary inner) continue;
                        foreach (DictionaryEntry token in inner)
                            Note("S0-e", $"  {candidate.Name} {entry.Key}.{token.Key} -> {token.Value?.GetType().Name ?? "null"} = {token.Value}");
                    }
                }
            }
            catch (Exception exception)
            {
                Note("S0-e", $"{candidate.Name}: {exception.GetType().Name}: {Trim(exception.Message)}");
            }
        }
    }

    private static void DumpSurface()
    {
        void Surface(string label, Type? type)
        {
            if (type is null) { Note("SURFACE", $"{label}: TYPE ABSENT"); return; }
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(static member => member is PropertyInfo or FieldInfo or MethodInfo);
            Note("SURFACE", $"{label} ({type.FullName}): {string.Join(", ", members.Select(static member => member.Name))}");
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.GetSetMethod(false) is not null) continue;
                Note("SURFACE", $"  {label}.{property.Name}: {property.PropertyType.Name} (no public setter)");
            }
        }

        Surface("ThemeColors", typeof(ThemeColors));
        Note("SURFACE", $"ThemeVariant values: {string.Join(", ", Enum.GetNames<ThemeVariant>())}");
        var themeManager = typeof(ThemeManager);
        Note("SURFACE", $"ThemeManager methods: {string.Join(", ", themeManager.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Select(static method => method.Name + "(" + string.Join(",", method.GetParameters().Select(static parameter => parameter.ParameterType.Name)) + ")"))}");
        Note("SURFACE", $"NotifyThemeResourcesChanged is public on Application: {typeof(Application).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Any(static method => method.Name == "NotifyThemeResourcesChanged")}");
        Note("SURFACE", $"Application public Theme/Resource members: {Join(typeof(Application).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(static member => member.Name.Contains("Theme", StringComparison.Ordinal) || member.Name.Contains("Resource", StringComparison.Ordinal)))}");
        Note("SURFACE", $"ThemeLoader public methods: {Join(typeof(ThemeLoader).GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))}");
        Note("SURFACE", $"ResourceDictionary public Theme members: {Join(typeof(ResourceDictionary).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(static member => member.Name.Contains("Theme", StringComparison.Ordinal)))}");
        Note("SURFACE", $"FrameworkElement public Theme/Resource members: {Join(typeof(FrameworkElement).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(static member => member.Name.Contains("Theme", StringComparison.Ordinal) || member.Name.Contains("Resource", StringComparison.Ordinal)))}");
        Note("SURFACE", $"Application.ThemeMode type: {typeof(Application).GetProperty("ThemeMode")?.PropertyType.FullName ?? "absent"}");
        var themeModeType = typeof(Application).GetProperty("ThemeMode")?.PropertyType;
        if (themeModeType is not null && themeModeType.IsEnum) Note("SURFACE", $"Application.ThemeMode values: {string.Join(", ", Enum.GetNames(themeModeType))}");
        Note("SURFACE", $"FrameworkElement.SetResourceReference overloads: {string.Join(" | ", typeof(FrameworkElement).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(static method => method.Name == "SetResourceReference").Select(static method => "(" + string.Join(",", method.GetParameters().Select(static parameter => parameter.ParameterType.Name)) + ")"))}");
        Note("SURFACE", $"ThemeDictionaries type: {typeof(ResourceDictionary).GetProperty(nameof(ResourceDictionary.ThemeDictionaries))?.PropertyType.Name}");
        Note("SURFACE", $"VisualTransition exists: {Type.GetType("Jalium.UI.VisualTransition, Jalium.UI.Managed")?.FullName ?? "absent"}");
        Note("SURFACE", $"VisualStateNames exists: {Type.GetType("Jalium.UI.VisualStateNames, Jalium.UI.Managed")?.FullName ?? "absent"}");
    }

    private static string Join(IEnumerable<MemberInfo> members) => string.Join(", ", members.Select(static member => member.Name).Distinct().Order(StringComparer.Ordinal));

    private static string Trim(string? message)
    {
        var single = (message ?? string.Empty).ReplaceLineEndings(" ");
        return single.Length > 220 ? single[..220] : single;
    }

    private static void Note(string category, string message) => Lines.Add($"[{category}] {message}");

    private static void Note(string category, Exception exception) => Lines.Add($"[{category}] {exception.GetType().Name}: {Trim(exception.Message)}");
}
