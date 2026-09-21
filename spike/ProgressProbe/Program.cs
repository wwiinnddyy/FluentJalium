using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace ProgressProbe;

/// <summary>
/// The readings the ProgressBar / ProgressRing slice cannot pick a base type or a geometry route without.
///   census     : upstream builds both looks out of TemplateSettings (Bindable) properties its C++ writes and
///                Storyboard-driven indeterminate states, and publishes rows of x:Double, Duration and
///                PointCollection type. The census says which of those names this runtime has at all, so the
///                transcription decision is made against the runtime's surface rather than WinUI's.
///   mount      : does the host's own bar move at all, and who owns its geometry? A value sweep (0/25/50/75/100)
///                and an indeterminate sweep, each read twice: the element tree (part names, sizes, brush
///                identity) and the inked columns of a capture on a lit card. The ink is the only reading that
///                distinguishes "the control laid out an indicator" from "the control painted one" - S1-m
///                measured that a thing can be in the tree, sized, and still print nothing.
///   retemplate : can a template of ours carry the determinate look? Three routes are mounted side by side:
///                a fixed-width part, a TemplateBinding of Value onto Width, and a part named the way the
///                host's own template names its indicator. The question is not whether markup parses (it parses
///                silently and does nothing often enough that every route is read back) but whether anything can
///                make a template's geometry proportional to a range without code.
///   anim       : is there a runtime animation path at all - BeginAnimation with a repeating DoubleAnimation,
///                and a Storyboard in markup - because the indeterminate bar and the ring are pure motion. If
///                only the code path works, the ring is a code-animated own type and the claim is measured here.
/// Modes: census | mount | retemplate | anim | all.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private static string _mode = "all";

    [STAThread]
    private static int Main(string[] arguments)
    {
        _mode = arguments.Length > 0 ? arguments[0].ToLowerInvariant() : "all";
        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            FluentThemeManager.Apply(application);
            Run(application);
        }
        catch (Exception exception)
        {
            Note("BOOT threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        var path = Path.Combine(AppContext.BaseDirectory, $"progress-probe-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {path}");
        return 0;
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(24) };
        var window = new Window { Content = root, Width = 900, Height = 700, Title = "Progress probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();
                Note($"theme dictionaries={FluentThemeManager.DictionaryNames.Count}");
                if (_mode is "all" or "census")
                {
                    Census();
                }

                if (_mode is "all" or "mount")
                {
                    Mount(root);
                }

                if (_mode is "all" or "retemplate")
                {
                    Retemplate(root);
                }

                if (_mode is "all" or "anim")
                {
                    Anim(root);
                }

                if (_mode is "all" or "anim" or "later")
                {
                    Anim2(root);
                    Nest(root);
                }

                if (_mode is "all" or "alias")
                {
                    Alias(application, root);
                }

                if (_mode is "all" or "axis")
                {
                    Axis(root);
                }
            }
            catch (Exception exception)
            {
                Note("PROBE threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            }
            finally
            {
                window.Close();
                application.Shutdown();
            }
        };

        application.Run(window);
    }

    // ---------- A. what the runtime exports for this family ----------

    private static void Census()
    {
        Note("");
        Note("=== A. do the types this family's markup leans on exist here ===");
        foreach (var name in new[]
                 {
                     "ProgressBar", "ProgressRing", "Divider", "InfoBadge", "InfoBadgeValueKind", "RatingControl",
                     "SymbolIcon", "FontIcon", "BitmapIcon", "PathIcon", "IconSource", "Symbol", "SymbolEnum",
                     "ProgressBarTemplateSettings", "ProgressRingTemplateSettings", "RangeBase", "Control",
                     "ArcSegment", "EllipseGeometry", "Path", "Ellipse", "RotateTransform", "TranslateTransform",
                     "Storyboard", "DoubleAnimation", "DoubleAnimationUsingKeyFrames", "ReplicableTimeline",
                     "Duration", "RepeatBehavior", "KeySpline", "PointCollection",
                 })
        {
            var type = TypeByName(name);
            Note($"  {name,-30} {(type is null ? "ABSENT" : Chain(type))}");
        }

        var symbol = TypeByName("Symbol");
        if (symbol is not null && symbol.IsEnum)
        {
            Note($"  Symbol members ({Enum.GetNames(symbol).Length}): {Join(Enum.GetNames(symbol).Take(12))} ...");
        }

        foreach (var (typeName, members) in new[]
                 {
                     ("ProgressBar", new[]
                     {
                         "Minimum", "Maximum", "Value", "IsIndeterminate", "Orientation", "ShowProgress",
                         "TemplateSettings", "Foreground", "Background", "BorderBrush", "BorderThickness",
                         "CornerRadius", "Height", "IsEnabled", "FontFamily", "FontSize",
                     }),
                     ("ProgressRing", new[]
                     {
                         "IsActive", "IsIndeterminate", "TemplateSettings", "Foreground", "Width", "Height",
                     }),
                     ("RatingControl", new[]
                     {
                         "Value", "MaxRating", "PlaceholderValue", "Caption", "ItemInfo", "ReadView",
                     }),
                     ("InfoBadge", new[] { "Value", "IconSource", "Style" }),
                 })
        {
            var type = TypeByName(typeName);
            Note("");
            Note($"  {typeName} surface {(type is null ? "(type absent)" : "(" + Chain(type) + ")")}:");
            if (type is null)
            {
                continue;
            }

            foreach (var member in members)
            {
                var found = Find(type, member);
                Note($"    {member,-26} {(found is null ? "ABSENT" : Describe(found))}");
            }
        }

        Note("");
        Note("  resource rows upstream declares for this family, as this runtime resolves them:");
        foreach (var key in new[]
                 {
                     "ProgressBarBackground", "ProgressBarForeground", "ProgressBarBorderThemeThickness",
                     "ProgressBarHeight", "ProgressBarAnimationDuration", "ProgressBarDeterminateIndicatorTransition",
                     "ProgressBarIndeterminateElement1Stops", "ProgressBarIndeterminateElement2Stops",
                     "ProgressBarIndeterminateDuration", "ProgressBarProgressBarRotation",
                     "ProgressBarRingBackground", "ProgressBarRingForeground", "ProgressBarRingEllipsisVisibility",
                     "ProgressRingBackground", "ProgressRingForeground", "ProgressRingEllipsisVisibility",
                     "ProgressRingRingSize", "DividerStrokeThickness", "DividerStroke", "CardBackgroundFillColorDefault",
                     "SystemAccentColor", "AccentFillColorDefaultBrush", "TextBlockForeground",
                 })
        {
            var value = Resource(key);
            Note($"    {key,-46} {(value is null ? "null" : value.GetType().Name + " = " + Show(value))}");
        }
    }

    // ---------- B. the host's own bar: tree and ink ----------

    private static void Mount(Panel root)
    {
        Note("");
        Note("=== B. the host's own ProgressBar: tree, then ink ===");
        var type = TypeByName("ProgressBar");
        if (type is null)
        {
            Note("  ABSENT - nothing to mount");
            return;
        }

        foreach (var value in new[] { 0d, 25d, 50d, 75d, 100d })
        {
            var bar = New(type, value, indeterminate: false, width: 300);
            var card = Card(bar, 300, 40);
            root.Children.Add(card);
            Pump(8);
            Note($"  value={value:0} size={bar.ActualWidth:0.##}x{bar.ActualHeight:0.##} " +
                 $"style={(Prop(bar, "Style") is null ? "null" : "set")} " +
                 $"template={(Prop(bar, "Template") is null ? "null" : "set")}");
            DescribeTree(bar);
            Profile(card, $"value {value:0}");
            root.Children.Remove(card);
            Pump(2);
        }

        Note("");
        Note("  indeterminate:");
        foreach (var frames in new[] { 2, 40 })
        {
            var bar = New(type, 50, indeterminate: true, width: 300);
            var card = Card(bar, 300, 40);
            root.Children.Add(card);
            Pump(8);
            Profile(card, $"indeterminate after {frames} extra frames");
            Pump(frames);
            Profile(card, $"indeterminate +{frames}");
            root.Children.Remove(card);
            Pump(2);
        }

        Note("");
        Note("  brushes the host reads (identity, not pixels):");
        var sample = New(type, 50, indeterminate: false, width: 300);
        var card2 = Card(sample, 300, 40);
        root.Children.Add(card2);
        Pump(8);
        foreach (var name in new[] { "Foreground", "Background", "BorderBrush", "BorderThickness", "CornerRadius" })
        {
            Note($"    {name,-18} {Show(Prop(sample, name))}");
        }

        Note($"    accent identity      {(ReferenceEquals(Prop(sample, "Foreground"), Resource("AccentFillColorDefaultBrush")) ? "same instance as AccentFillColorDefaultBrush" : "not AccentFillColorDefaultBrush")}");
        root.Children.Remove(card2);
    }

    private static FrameworkElement New(Type type, double value, bool indeterminate, double width)
    {
        var element = (FrameworkElement)Activator.CreateInstance(type)!;
        SetProp(element, "Minimum", 0d);
        SetProp(element, "Maximum", 100d);
        SetProp(element, "Value", value);
        SetProp(element, "IsIndeterminate", indeterminate);
        element.Width = width;
        element.Height = 40;
        element.HorizontalAlignment = HorizontalAlignment.Stretch;
        return element;
    }

    private static Border Card(FrameworkElement child, double width, double height)
    {
        // A lit card, because the host window's own backdrop is black and a translucent row on it reads as no
        // ink at all (adaptation/00 S1-m 6).
        return new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
            Width = width,
            Height = height,
            Child = child,
        };
    }

    private static void DescribeTree(DependencyObject root)
    {
        void Walk(DependencyObject node, int depth)
        {
            if (depth > 7)
            {
                return;
            }

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
            {
                var child = VisualTreeHelper.GetChild(node, index);
                var element = child as FrameworkElement;
                var name = Read(() => child.GetType().GetProperty("Name")?.GetValue(child)) as string;
                var brush = element is Border border
                    ? Show(border.Background)
                    : element is TextBlock text
                        ? Show(text.Foreground)
                        : "";
                Note($"    {new string(' ', depth * 2)}{child.GetType().Name} {Trim(name ?? "")} " +
                     $"{element?.ActualWidth.ToString("0.##") ?? "?"}x{element?.ActualHeight.ToString("0.##") ?? "?"} {brush}");
                Walk(child, depth + 1);
            }
        }

        Walk(root, 0);
    }

    /// <summary>
    /// Counts the ink the card gained over a card painted with nothing in it, and reports where that ink sits.
    /// A column profile is what answers "how wide is the indicator": the bar is a horizontal band, so the
    /// right-most inked column is its end.
    /// </summary>
    private static void Profile(FrameworkElement element, string label)
    {
        var width = (int)Math.Round(element.ActualWidth);
        var height = (int)Math.Round(element.ActualHeight);
        if (width <= 0 || height <= 0)
        {
            Note($"  {label}: no size");
            return;
        }

        var buffer = Capture(element, width, height);
        var ink = 0;
        var minX = width;
        var maxX = -1;
        var minY = height;
        var maxY = -1;
        var columns = new int[width];
        var rows = new int[height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                // Non-white: anything the card did not arrive with.
                if (buffer[offset] > 240 && buffer[offset + 1] > 240 && buffer[offset + 2] > 240)
                {
                    continue;
                }

                ink++;
                columns[x]++;
                rows[y]++;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < 0)
        {
            Note($"  {label}: no ink");
            return;
        }

        var rowProfile = string.Join(",", rows.Select((count, index) => count == 0 ? null : $"{index}:{count}").Where(part => part is not null)!);
        Note($"  {label}: ink={ink} bbox={maxX - minX + 1}x{maxY - minY + 1}@({minX},{minY}) rows=[{Trim(rowProfile)}]");
    }

    private static byte[] Capture(FrameworkElement element, int width, int height)
    {
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(element);
        var buffer = new byte[width * height * 4];
        bitmap.CopyPixels(buffer, width * 4, 0);
        return buffer;
    }

    // ---------- C. can a template of ours carry the geometry? ----------

    private const string ProbeMarkup = """
        <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <Style x:Key="FixedTemplate" TargetType="ProgressBar">
            <Setter Property="Template">
              <ControlTemplate TargetType="ProgressBar">
                <Grid Name="RootGrid" Background="Transparent">
                  <Border Name="Track" Height="4" VerticalAlignment="Center" Background="#FFE0E0E0" />
                  <Border Name="Indicator" Height="4" Width="120" HorizontalAlignment="Left"
                          VerticalAlignment="Center" Background="#FF0000FF" />
                </Grid>
              </ControlTemplate>
            </Setter>
          </Style>
          <Style x:Key="BindingTemplate" TargetType="ProgressBar">
            <Setter Property="Template">
              <ControlTemplate TargetType="ProgressBar">
                <Grid Name="RootGrid" Background="Transparent">
                  <Border Name="Track" Height="4" VerticalAlignment="Center" Background="#FFE0E0E0" />
                  <Border Name="Indicator" Height="4" HorizontalAlignment="Left" VerticalAlignment="Center"
                          Width="{TemplateBinding Value}" Background="#FF00FF00" />
                </Grid>
              </ControlTemplate>
            </Setter>
          </Style>
          <Style x:Key="PartNamedTemplate" TargetType="ProgressBar">
            <Setter Property="Template">
              <ControlTemplate TargetType="ProgressBar">
                <Grid Name="RootGrid" Background="Transparent">
                  <Border Name="PART_Track" Height="4" VerticalAlignment="Center" Background="#FFE0E0E0" />
                  <Border Name="PART_Indicator" Height="4" Width="30" HorizontalAlignment="Left"
                          VerticalAlignment="Center" Background="#FFFF0000" />
                  <Border Name="PART_GlowRect" Height="4" Width="30" HorizontalAlignment="Left"
                          VerticalAlignment="Center" Background="#FFFFFF00" />
                  <Border Name="PART_Decorator" Height="4" Width="30" HorizontalAlignment="Left"
                          VerticalAlignment="Center" Background="#FFFF00FF" />
                </Grid>
              </ControlTemplate>
            </Setter>
          </Style>
        </ResourceDictionary>
        """;

    private static void Retemplate(Panel root)
    {
        Note("");
        Note("=== C. three declarative routes for the indicator's width ===");
        ResourceDictionary? dictionary = null;
        try
        {
            dictionary = (ResourceDictionary)XamlReader.Parse(ProbeMarkup);
            var declared = new List<string>();
            foreach (var key in dictionary.Keys)
            {
                declared.Add(Show(key));
            }

            Note("  markup parsed: " + Join(declared));
        }
        catch (Exception exception)
        {
            Note("  PARSE threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            return;
        }

        var type = TypeByName("ProgressBar");
        if (type is null)
        {
            Note("  ABSENT");
            return;
        }

        foreach (var key in new[] { "FixedTemplate", "BindingTemplate", "PartNamedTemplate" })
        {
            foreach (var value in new[] { 25d, 75d })
            {
                var bar = New(type, value, indeterminate: false, width: 300);
                var card = Card(bar, 300, 40);
                root.Children.Add(card);
                Pump(4);
                var style = dictionary[key] as Style;
                SetProp(bar, "Style", style);
                var applied = Prop(bar, "Template");
                if (applied is ControlTemplate template)
                {
                    try
                    {
                        template.LoadContent();
                    }
                    catch (Exception exception)
                    {
                        Note($"  {key} LoadContent threw {exception.GetType().Name}");
                    }

                    bar.GetType().GetMethod("ApplyTemplate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.Invoke(bar, null);
                }

                Pump(10);
                Note($"  {key,-20} value={value:0} template={(applied is null ? "null (style did not reach)" : "set")} " +
                     $"size={bar.ActualWidth:0.##}x{bar.ActualHeight:0.##}");
                DescribeTree(bar);
                Profile(card, $"    {key} @ {value:0}");
                root.Children.Remove(card);
                Pump(2);
            }
        }
    }

    // ---------- D. is there an animation path? ----------

    private static void Anim(Panel root)
    {
        Note("");
        Note("=== D. animation: repeating DoubleAnimation on a transform, and a markup Storyboard ===");
        var box = new Border
        {
            Width = 20,
            Height = 20,
            Background = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0xFF)),
            RenderTransform = new TranslateTransform(),
        };
        var card = Card(box, 300, 40);
        root.Children.Add(card);
        Pump(6);

        var transform = (TranslateTransform)box.RenderTransform;
        Note($"  before: X={transform.X:0.##}");
        try
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 260,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true,
            };
            box.BeginAnimation(UIElement.RenderTransformProperty, animation);
            Note("  BeginAnimation(RenderTransformProperty, DoubleAnimation Forever) accepted");
        }
        catch (Exception exception)
        {
            Note("  BeginAnimation threw " + exception.GetType().Name + ": " + Trim(exception.Message));
        }

        var samples = new List<string>();
        for (var round = 0; round < 6; round++)
        {
            Pump(6);
            samples.Add($"{transform.X:0.#}");
        }

        Note($"  X over six 6-frame pumps: {Join(samples)} (moving means the clock runs on this runtime)");
        box.BeginAnimation(UIElement.RenderTransformProperty, null);
        root.Children.Remove(card);
        Pump(2);

        // The property a transform sits on is not animatable as a DP of the owner: try the direct route too, so
        // the answer "which DP can a repeating animation attach to" is measured rather than assumed.
        var slide = new Border
        {
            Width = 20,
            Height = 20,
            Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00)),
            Margin = new Thickness(0),
        };
        var card2 = Card(slide, 300, 40);
        root.Children.Add(card2);
        Pump(6);
        try
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 120,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                RepeatBehavior = RepeatBehavior.Forever,
            };
            slide.BeginAnimation(FrameworkElement.MarginProperty, animation);
            Note("  BeginAnimation(MarginProperty, ...) accepted");
        }
        catch (Exception exception)
        {
            Note("  Margin animation threw " + exception.GetType().Name + ": " + Trim(exception.Message));
        }

        Pump(12);
        Note($"  margin after 12 frames: {slide.Margin}");
        Profile(card2, "  margin-animated card");
        root.Children.Remove(card2);
        Pump(2);

        Note("");
        Note("  a Storyboard declared in markup, hydrated and started:");
        const string StoryboardMarkup = """
            <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
              <Storyboard x:Key="Spin">
                <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.Angle)"
                                 From="0" To="360" Duration="0:0:1" RepeatBehavior="Forever" />
              </Storyboard>
            </ResourceDictionary>
            """;
        try
        {
            var dictionary = (ResourceDictionary)XamlReader.Parse(StoryboardMarkup);
            var board = dictionary["Spin"];
            var children = new List<string>();
            if (board is Storyboard outer)
            {
                foreach (var child in outer.Children)
                {
                    children.Add(child.GetType().Name);
                }
            }

            Note($"  parsed as {board?.GetType().Name ?? "null"}; children={Join(children)}");
        }
        catch (Exception exception)
        {
            Note("  Storyboard markup threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        // Does the host's own indeterminate bar animate without being told to? Compared across pumps in B.
        Note("");
        Note("  RotateTransform.Angle animatable? " +
             (Find(typeof(RotateTransform), "Angle") is null ? "no Angle" : "Angle present"));
    }

    // ---------- E. can our own template keep a 4 DIP band, and what else does the host drive? ----------

    private const string NestMarkup = """
        <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <CornerRadius x:Key="ProbeCornerRadius">1.5</CornerRadius>
          <Thickness x:Key="ProbeThickness">0</Thickness>
          <Style x:Key="ExplicitHeight" TargetType="ProgressBar">
            <Setter Property="Template">
              <ControlTemplate TargetType="ProgressBar">
                <Grid Name="RootGrid" Background="Transparent">
                  <Border Name="PART_Track" Height="4" VerticalAlignment="Center" Background="#FFE0E0E0" />
                  <Border Name="PART_Indicator" Height="4" VerticalAlignment="Center" HorizontalAlignment="Left"
                          Background="{TemplateBinding Foreground}" />
                </Grid>
              </ControlTemplate>
            </Setter>
          </Style>
          <Style x:Key="NestedBand" TargetType="ProgressBar">
            <Setter Property="Template">
              <ControlTemplate TargetType="ProgressBar">
                <Grid Name="RootGrid" Background="Transparent">
                  <Grid Height="4" VerticalAlignment="Center">
                    <Border Name="PART_Track" Background="#FFE0E0E0" />
                    <Border Name="PART_Indicator" HorizontalAlignment="Left" Background="{TemplateBinding Foreground}" />
                  </Grid>
                </Grid>
              </ControlTemplate>
            </Setter>
          </Style>
          <Style x:Key="RadiusRead" TargetType="ProgressBar">
            <Setter Property="Template">
              <ControlTemplate TargetType="ProgressBar">
                <Grid Name="RootGrid" Background="Transparent">
                  <Border Name="PART_Track" Height="4" VerticalAlignment="Center" Background="#FFE0E0E0"
                          CornerRadius="{ThemeResource ProbeCornerRadius}" />
                  <Border Name="PART_Indicator" Height="4" VerticalAlignment="Center" HorizontalAlignment="Left"
                          Background="{TemplateBinding Foreground}" CornerRadius="{ThemeResource ProbeCornerRadius}" />
                </Grid>
              </ControlTemplate>
            </Setter>
          </Style>
        </ResourceDictionary>
        """;

    private static void Nest(Panel root)
    {
        Note("");
        Note("=== E. what the host writes onto the parts, and which metric rows this reader parses ===");
        var type = TypeByName("ProgressBar");
        if (type is null)
        {
            Note("  ABSENT");
            return;
        }

        foreach (var member in new[] { "ShowError", "ShowPaused", "Orientation", "TemplateSettings", "Minimum", "Maximum" })
        {
            Note($"  host drives {member,-16} {(Find(type, member) is null ? "ABSENT" : Describe(Find(type, member)!))}");
        }

        ResourceDictionary? dictionary = null;
        try
        {
            dictionary = (ResourceDictionary)XamlReader.Parse(NestMarkup);
        }
        catch (Exception exception)
        {
            Note("  NEST PARSE threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            return;
        }

        foreach (var key in new[] { "ProbeCornerRadius", "ProbeThickness" })
        {
            var value = Read(() => dictionary[key]);
            Note($"  row {key,-18} -> {value?.GetType().Name ?? "null"} = {Show(value)}");
        }

        foreach (var styleKey in new[] { "ExplicitHeight", "NestedBand", "RadiusRead" })
        {
            foreach (var value in new[] { 25d, 100d })
            {
                var bar = New(type, value, indeterminate: false, width: 300);
                var card = Card(bar, 300, 40);
                root.Children.Add(card);
                Pump(4);
                SetProp(bar, "Style", dictionary[styleKey]);
                if (Prop(bar, "Template") is ControlTemplate template)
                {
                    try
                    {
                        template.LoadContent();
                    }
                    catch (Exception exception)
                    {
                        Note($"  {styleKey} LoadContent threw {exception.GetType().Name}");
                    }
                }

                Pump(10);
                var track = FindNamed(bar, "PART_Track");
                var indicator = FindNamed(bar, "PART_Indicator");
                Note($"  {styleKey,-14} value={value:0}");
                Note($"    track     {(track is null ? "null" : Part(track!))}");
                Note($"    indicator {(indicator is null ? "null" : Part(indicator!))}");
                Profile(card, $"    {styleKey} @ {value:0}");
                root.Children.Remove(card);
                Pump(2);
            }
        }

        Note("");
        Note("  vertical orientation, host's own template:");
        var vertical = New(type, 50, indeterminate: false, width: 60);
        SetProp(vertical, "Orientation", Jalium.UI.Controls.Orientation.Vertical);
        var verticalCard = Card(vertical, 60, 200);
        root.Children.Add(verticalCard);
        Pump(10);
        Note($"    size={vertical.ActualWidth:0.##}x{vertical.ActualHeight:0.##} orientation={Show(Prop(vertical, "Orientation"))}");
        DescribeTree(vertical);
        Profile(verticalCard, "    vertical @ 50");
        root.Children.Remove(verticalCard);
    }

    private static string Part(FrameworkElement element)
    {
        var fill = element is Border border ? Show(border.Background) : "";
        return $"{element.ActualWidth:0.##}x{element.ActualHeight:0.##} " +
               $"Height={Show(Prop(element, "Height"))} VerticalAlignment={Show(Prop(element, "VerticalAlignment"))} " +
               $"CornerRadius={Show(Prop(element, "CornerRadius"))} {fill}";
    }

    private static FrameworkElement? FindNamed(DependencyObject root, string name)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is FrameworkElement element && (string?)Read(() => element.GetType().GetProperty("Name")?.GetValue(element)) == name)
            {
                return element;
            }

            if (FindNamed(child, name) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    // ---------- F. which animation route actually moves ----------

    private static void Anim2(Panel root)
    {
        Note("");
        Note("=== F. animation routes, read back as property values ===");

        // The D pass on the previous run animated the WRONG dependency properties: RenderTransform holds a
        // Transform and Margin a Thickness, so a DoubleAnimation on either is a type mismatch - and this runtime
        // accepted both calls without a word, which is itself a reading. The routes below attach to DPs that
        // really are doubles, and each is compared against the keyframe shape the product already ships.
        void Animate(string label, Action<AnimationTimeline?> start, Func<string> read)
        {
            start(new DoubleAnimation
            {
                From = 0,
                To = 180,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                RepeatBehavior = RepeatBehavior.Forever,
            });
            var first = read();
            Pump(12);
            var second = read();
            Pump(12);
            var third = read();
            start(null);
            Note($"  {label}: {first} -> {second} -> {third}");
        }

        var box = new Border
        {
            Width = 20,
            Height = 20,
            Background = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0xFF)),
            RenderTransform = new TranslateTransform(),
        };
        var card = Card(box, 300, 40);
        root.Children.Add(card);
        Pump(6);
        var translate = (TranslateTransform)box.RenderTransform;
        Animate("TranslateTransform.XProperty Forever",
            timeline => translate.BeginAnimation(TranslateTransform.XProperty, timeline),
            () => translate.X.ToString("0.#"));
        Animate("FrameworkElement.WidthProperty Forever",
            timeline => box.BeginAnimation(FrameworkElement.WidthProperty, timeline),
            () => box.Width.ToString("0.#"));

        var spinner = new Border
        {
            Width = 20,
            Height = 20,
            Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00)),
            RenderTransform = new RotateTransform(),
        };
        var spinnerCard = Card(spinner, 300, 40);
        root.Children.Add(spinnerCard);
        Pump(6);
        var rotate = (RotateTransform)spinner.RenderTransform;
        Animate("RotateTransform.AngleProperty Forever",
            timeline => rotate.BeginAnimation(RotateTransform.AngleProperty, timeline),
            () => rotate.Angle.ToString("0.#"));

        // The two routes an indeterminate bar needs: a slide (offset on the host panel) and a fade.
        var slid = new Border
        {
            Width = 60,
            Height = 4,
            Background = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0xFF)),
            Opacity = 1,
        };
        var canvas = new Canvas { Width = 300, Height = 20 };
        Canvas.SetLeft(slid, 0);
        canvas.Children.Add(slid);
        var slideCard = Card(canvas, 300, 40);
        root.Children.Add(slideCard);
        Pump(6);
        Animate("Canvas.LeftProperty Forever",
            timeline => slid.BeginAnimation(Canvas.LeftProperty, timeline),
            () => Canvas.GetLeft(slid).ToString("0.#"));
        Animate("UIElement.OpacityProperty Forever",
            timeline => slid.BeginAnimation(UIElement.OpacityProperty, timeline),
            () => slid.Opacity.ToString("0.##"));
        root.Children.Remove(slideCard);
        Pump(2);

        Note("");
        foreach (var (typeName, member) in new[]
                 {
                     ("Trigger", "EnterActions"), ("Trigger", "ExitActions"), ("Style", "Triggers"),
                     ("Timeline", "RepeatBehavior"), ("StoryBoard", "Begin"),
                 })
        {
            var found = TypeByName(typeName);
            Note($"  {typeName}.{member,-16} {(found is null ? "type absent" : Find(found, member) is null ? "ABSENT" : Describe(Find(found, member)!))}");
        }

        // The positive control: the shape the product's own NavigationIndicatorAnimator uses.
        var keyframed = new Border { Width = 20, Height = 20, Background = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x00)) };
        var keyframeCard = Card(keyframed, 300, 40);
        root.Children.Add(keyframeCard);
        Pump(6);
        Note("  " + Show(Read(() =>
        {
            var animation = new DoubleAnimationUsingKeyFrames { Duration = new Duration(TimeSpan.FromMilliseconds(400)) };
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(180, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400)), new KeySpline(0.1, 0.9, 0.2, 1)));
            keyframed.BeginAnimation(FrameworkElement.WidthProperty, animation);
            var first = keyframed.Width.ToString("0.#");
            Pump(10);
            var second = keyframed.Width.ToString("0.#");
            keyframed.BeginAnimation(FrameworkElement.WidthProperty, null);
            return $"keyframe control Width {first} -> {second}";
        })));
        root.Children.Remove(keyframeCard);
        root.Children.Remove(card);
        root.Children.Remove(spinnerCard);
        Pump(2);

        Note("");
        Note("  a Storyboard in markup, with TargetName rather than an attached-property path:");
        const string BoardMarkup = """
            <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
              <Storyboard x:Key="Spin">
                <DoubleAnimationUsingKeyFrames Storyboard.TargetName="Spin" Storyboard.TargetProperty="Angle"
                                               Duration="0:0:1" RepeatBehavior="Forever">
                  <LinearDoubleKeyFrame KeyTime="0" Value="0" />
                  <LinearDoubleKeyFrame KeyTime="0:0:1" Value="360" />
                </DoubleAnimationUsingKeyFrames>
              </Storyboard>
            </ResourceDictionary>
            """;
        try
        {
            var parsed = (ResourceDictionary)XamlReader.Parse(BoardMarkup);
            var board = (Storyboard?)parsed["Spin"];
            var children = new List<string>();
            if (board is not null)
            {
                foreach (var child in board.Children)
                {
                    children.Add($"{child.GetType().Name} target={Show(Read(() => child.GetType().GetProperty("TargetName")?.GetValue(child)))}");
                }
            }

            Note($"  parsed Storyboard children={Join(children)}");
            if (board is not null)
            {
                var host = new Grid { Width = 40, Height = 40 };
                var rotating = new Border
                {
                    Name = "Spin",
                    Width = 20,
                    Height = 20,
                    Background = new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0xFF)),
                    RenderTransform = new RotateTransform(),
                };
                host.Children.Add(rotating);
                var boardCard = Card(host, 60, 60);
                root.Children.Add(boardCard);
                Pump(6);
                var angle = (RotateTransform)rotating.RenderTransform;
                Note("  " + Show(Read(() =>
                {
                    board.Begin(host);
                    var first = angle.Angle.ToString("0.#");
                    Pump(20);
                    var second = angle.Angle.ToString("0.#");
                    return $"board.Begin(host) then Angle {first} -> {second}";
                })));
                root.Children.Remove(boardCard);
                Pump(2);
            }
        }
        catch (Exception exception)
        {
            Note("  Storyboard markup threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }
    }

    // ---------- G. can a row that names a Color or a CornerRadius land on a Brush property? ----------

    private static void Alias(Application application, Panel root)
    {
        Note("");
        Note("=== G. upstream's alias targets, as this reader hands them over ===");
        // ProgressBar_themeresources.xaml:9 points ProgressBarBackground at ControlStrongStrokeColorDefault, which
        // is a Color row here (keys.md:57), not a brush. Whether a Color reaching a Brush property paints anything is
        // the difference between transcribing upstream's name and having to substitute its brush twin, so it is
        // measured rather than assumed. The rows go into the application dictionary because a {ThemeResource} that
        // cannot resolve reads back as the property's own default and would otherwise look like a negative result.
        const string Markup = """
            <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
              <StaticResource x:Key="ProbeColorAlias" ResourceKey="ControlStrongStrokeColorDefault" />
              <StaticResource x:Key="ProbeBrushAlias" ResourceKey="ControlStrongStrokeColorDefaultBrush" />
              <StaticResource x:Key="ProbeRadiusAlias" ResourceKey="ControlCornerRadius" />
              <Style x:Key="ColorOnBrush" TargetType="Border">
                <Setter Property="Background" Value="{ThemeResource ProbeColorAlias}" />
              </Style>
              <Style x:Key="BrushOnBrush" TargetType="Border">
                <Setter Property="Background" Value="{ThemeResource ProbeBrushAlias}" />
              </Style>
              <Style x:Key="RadiusOnBorder" TargetType="Border">
                <Setter Property="CornerRadius" Value="{ThemeResource ProbeRadiusAlias}" />
              </Style>
            </ResourceDictionary>
            """;

        ResourceDictionary? dictionary = null;
        try
        {
            dictionary = (ResourceDictionary)XamlReader.Parse(Markup);
        }
        catch (Exception exception)
        {
            Note("  PARSE threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            return;
        }

        foreach (var key in new[] { "ProbeColorAlias", "ProbeBrushAlias", "ProbeRadiusAlias" })
        {
            Note($"  row {key,-18} -> {Read(() => dictionary[key])?.GetType().Name ?? "null"}");
            Note($"    into the app dictionary: {Show(Read(() =>
            {
                application.Resources.Add(key, dictionary[key]);
                return "added";
            }))}");
        }

        var panel = new StackPanel { Width = 200 };
        root.Children.Add(panel);
        foreach (var (label, styleKey) in new[]
                 {
                     ("Color target on Background", "ColorOnBrush"),
                     ("Brush target on Background", "BrushOnBrush"),
                     ("CornerRadius row on CornerRadius", "RadiusOnBorder"),
                 })
        {
            var border = new Border { Width = 120, Height = 20 };
            panel.Children.Add(border);
            SetProp(border, "Style", dictionary[styleKey]);
            Pump(8);
            Note($"  {label,-32} -> Background={Show(border.Background)} CornerRadius={Show(border.CornerRadius)}");
            panel.Children.Remove(border);
        }

        root.Children.Remove(panel);
        foreach (var key in new[] { "ProbeColorAlias", "ProbeBrushAlias", "ProbeRadiusAlias" })
        {
            Read(() =>
            {
                application.Resources.Remove(key);
                return null;
            });
        }
    }

    // ---------- H. can one template carry both axes? ----------

    private static void Axis(Panel root)
    {
        Note("");
        Note("=== H. an Orientation trigger that re-sizes the band: does Auto or NaN parse? ===");
        const string Markup = """
            <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
              <Style x:Key="AutoAxis" TargetType="ProgressBar">
                <Setter Property="MinHeight" Value="3" />
                <Setter Property="Template">
                  <ControlTemplate TargetType="ProgressBar">
                    <Grid Name="LayoutRoot">
                      <Grid Name="Band" Height="{TemplateBinding MinHeight}" VerticalAlignment="Center">
                        <Border Name="PART_Track" Height="1" VerticalAlignment="Center" Background="#FFE0E0E0" />
                        <Border Name="PART_Indicator" HorizontalAlignment="Left" Background="{TemplateBinding Foreground}" />
                      </Grid>
                    </Grid>
                    <ControlTemplate.Triggers>
                      <Trigger Property="Orientation" Value="Vertical">
                        <Setter TargetName="Band" Property="Height" Value="Auto" />
                        <Setter TargetName="Band" Property="Width" Value="{TemplateBinding MinHeight}" />
                        <Setter TargetName="Band" Property="VerticalAlignment" Value="Stretch" />
                        <Setter TargetName="Band" Property="HorizontalAlignment" Value="Center" />
                        <Setter TargetName="PART_Track" Property="Height" Value="NaN" />
                        <Setter TargetName="PART_Track" Property="Width" Value="1" />
                        <Setter TargetName="PART_Track" Property="VerticalAlignment" Value="Stretch" />
                        <Setter TargetName="PART_Track" Property="HorizontalAlignment" Value="Center" />
                      </Trigger>
                    </ControlTemplate.Triggers>
                  </ControlTemplate>
                </Setter>
              </Style>
            </ResourceDictionary>
            """;

        var type = TypeByName("ProgressBar");
        ResourceDictionary? dictionary = null;
        try
        {
            dictionary = (ResourceDictionary)XamlReader.Parse(Markup);
        }
        catch (Exception exception)
        {
            Note("  PARSE threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            return;
        }

        foreach (var orientation in new[] { "Horizontal", "Vertical" })
        {
            var bar = New(type!, 50, indeterminate: false, width: 300);
            bar.Height = 40;
            var card = Card(bar, 300, 40);
            root.Children.Add(card);
            Pump(4);
            SetProp(bar, "Style", dictionary["AutoAxis"]);
            SetProp(bar, "Orientation", Enum.Parse(TypeByName("Orientation")!, orientation));
            if (Prop(bar, "Template") is ControlTemplate template)
            {
                Read(() => template.LoadContent());
            }

            Pump(10);
            var band = FindNamed(bar, "Band");
            var indicator = FindNamed(bar, "PART_Indicator");
            Note($"  {orientation,-11} band={(band is null ? "null" : $"{band.ActualWidth:0.##}x{band.ActualHeight:0.##} Height={Show(Prop(band, "Height"))} Width={Show(Prop(band, "Width"))}")} " +
                 $"indicator={(indicator is null ? "null" : $"{indicator.ActualWidth:0.##}x{indicator.ActualHeight:0.##}")}");
            Profile(card, $"    {orientation} @ 50");
            if (orientation == "Vertical")
            {
                // Does a {TemplateBinding} inside a trigger setter actually resolve? MinHeight is the only number
                // the band's cross-axis size reads, so changing it separates "bound" from "frozen at 3".
                SetProp(bar, "MinHeight", 7d);
                Pump(8);
                Note($"    after MinHeight=7: band={(band is null ? "null" : $"{band.ActualWidth:0.##}x{band.ActualHeight:0.##}")}");
            }

            root.Children.Remove(card);
            Pump(2);
        }
    }

    // ---------- shared helpers ----------

    private static MemberInfo? Find(Type type, string name)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        return (MemberInfo?)type.GetProperty(name, flags)
            ?? (MemberInfo?)type.GetEvent(name, flags)
            ?? type.GetMethod(name, flags);
    }

    private static string Describe(MemberInfo member) => member switch
    {
        PropertyInfo property => $"prop {property.PropertyType.Name} ({property.DeclaringType?.Name})",
        EventInfo eventInfo => $"event {eventInfo.EventHandlerType?.Name} ({eventInfo.DeclaringType?.Name})",
        MethodInfo method => $"method ({method.DeclaringType?.Name}){(method.IsPublic ? " public" : " protected/internal")}",
        _ => member.MemberType.ToString(),
    };

    private static List<Assembly> LoadedAssemblies()
    {
        var seen = new Dictionary<string, Assembly>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name ?? "?";
            if (name.StartsWith("Jalium", StringComparison.Ordinal) || name.StartsWith("FluentJalium", StringComparison.Ordinal))
            {
                seen.TryAdd(name, assembly);
            }
        }

        return seen.Values.ToList();
    }

    private static Type? TypeByName(string name) => LoadedAssemblies()
        .SelectMany(SafeTypes)
        .FirstOrDefault(type => type.Name == name && !type.IsNested);

    private static IEnumerable<Type> SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static string Chain(Type type)
    {
        var parts = new List<string>();
        for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            parts.Add(current.Name);
        }

        return string.Join(" < ", parts);
    }

    private static object? Prop(object? target, string name) => target is null
        ? null
        : Read(() => target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(target));

    private static void SetProp(object target, string name, object? value) => Read(() =>
    {
        target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(target, value);
        return null;
    });

    private static object? Read(Func<object?> reader)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            return " threw " + exception.GetType().Name;
        }
    }

    private static object? Resource(string key) => Application.Current?.TryFindResource(key);

    private static string Show(object? value) => value is null ? "null" : Trim(value.ToString() ?? "?");

    private static string Join(IEnumerable<string?> values) => string.Join(", ", values.Where(value => value is not null));

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

    private static int Pump(int frames = 6, int budgetMilliseconds = 1500)
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
        return seen;
    }
}
