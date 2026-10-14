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

namespace RingProbe;

/// <summary>
/// The readings the ProgressRing slice cannot pick a geometry route without. Upstream builds the ring out of a
/// Lottie asset plus TemplateSettings its C++ writes, and this runtime has no ProgressRing type at all
/// (adaptation/s1o census), so every part of the look has to be re-derived here.
///   surface: which shape types and stroke properties this runtime exports, by their own names and assembly.
///   ink    : does each candidate actually print? A dash arc, an `A` command in path mini-language, an explicit
///            ArcSegment, an EllipseGeometry - parsed AND read back AND measured for ink, because markup that
///            parses silently and paints nothing is already on the record for this framework (S1-m, S1-j).
///   anim   : can a code-built repeating animation drive the dash route, and does a static change of the same
///            property change the picture? The first half decides whether the indeterminate ring is a clock or a
///            render loop; the second half is the control that keeps the first honest.
///   xform  : is a static RotateTransform applied at all? Transforms never tick here (S1-o), and if they also do
///            not render, the ring's start angle has to come from the geometry instead of from a transform.
/// Modes: surface | ink | anim | xform | all.
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

        var path = System.IO.Path.Combine(AppContext.BaseDirectory, $"ring-probe-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {path}");
        return 0;
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(24) };
        var window = new Window { Content = root, Width = 900, Height = 700, Title = "Ring probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();
                Note($"theme dictionaries={FluentThemeManager.DictionaryNames.Count}");
                if (_mode is "all" or "surface")
                {
                    Surface();
                }

                if (_mode is "all" or "ink" or "anim" or "xform")
                {
                    Ink(root);
                }

                if (_mode is "all" or "anim")
                {
                    Anim(root);
                }

                if (_mode is "all" or "xform")
                {
                    Xform(root);
                }

                if (_mode is "all" or "motion")
                {
                    Motion(root);
                }

                if (_mode is "surface2")
                {
                    Surface2();
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

    // ---------- A. what the runtime exports for an arc ----------

    private static void Surface()
    {
        Note("");
        Note("=== A. shape and geometry types this runtime exports ===");
        foreach (var name in new[]
                 {
                     "ProgressRing", "LottiePlayer", "Lottie", "Ellipse", "Path", "Line", "Polyline", "Polygon",
                     "Rectangle", "Shape", "Geometry", "PathGeometry", "StreamGeometry", "PathFigure", "ArcSegment",
                     "EllipseGeometry", "LineSegment", "GeometryGroup", "RotateTransform", "DoubleCollection",
                     "Gauge", "Arc", "Donut", "PieSlice", "Sector", "RingArc", "RingGeometry", "PieRingArc",
                 })
        {
            var type = TypeByName(name);
            Note($"  {name,-18} {(type is null ? "ABSENT" : type.FullName + " [" + type.Assembly.GetName().Name + "] " + Chain(type))}");
        }

        foreach (var name in new[] { "Shape", "Ellipse", "Path" })
        {
            var type = TypeByName(name);
            if (type is null)
            {
                continue;
            }

            Note($"  --- {name}: stroke / dash / angle / data surface ---");
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(property => property.Name))
            {
                var interesting = property.Name.Contains("Stroke", StringComparison.Ordinal) ||
                    property.Name.Contains("Dash", StringComparison.Ordinal) ||
                    property.Name.Contains("Angle", StringComparison.Ordinal) ||
                    property.Name.Contains("Cap", StringComparison.Ordinal) ||
                    property.Name is "Data" or "StartPoint" or "EndPoint" or "Center" or "RadiusX" or "RadiusY" ||
                    property.Name.Contains("Segment", StringComparison.Ordinal);
                if (interesting)
                {
                    Note($"    {property.Name,-26} {property.PropertyType.Name} ({property.DeclaringType?.Name})");
                }
            }
        }

        // Any type whose own name says arc/sector: the framework draws pie charts, so a first-class arc may exist.
        Note("  --- any exported type whose name mentions an arc, a sector or a dash ---");
        foreach (var assembly in LoadedAssemblies())
        {
            foreach (var type in SafeTypes(assembly))
            {
                if (!type.IsPublic || type.IsNested)
                {
                    continue;
                }

                if (type.Name.Contains("Arc", StringComparison.Ordinal) || type.Name.Contains("Sector", StringComparison.Ordinal) ||
                    type.Name.Contains("Dash", StringComparison.Ordinal) || type.Name.Contains("Donut", StringComparison.Ordinal))
                {
                    Note($"    {type.Name,-24} [{assembly.GetName().Name}] {Chain(type)}");
                }
            }
        }
    }

    // ---------- B. does each candidate print? ----------

    private const string Snippets = """
        <StackPanel xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <Grid Name="FullRing" Width="60" Height="60" Background="#FFFFFFFF">
            <Ellipse Width="50" Height="50" Stroke="#FF101010" StrokeThickness="3" HorizontalAlignment="Center" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="DashArc" Width="60" Height="60" Background="#FFFFFFFF">
            <Ellipse Width="50" Height="50" Stroke="#FF101010" StrokeThickness="3"
                     StrokeDashArray="20 200" StrokeDashOffset="0"
                     HorizontalAlignment="Center" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="DashArcMoved" Width="60" Height="60" Background="#FFFFFFFF">
            <Ellipse Width="50" Height="50" Stroke="#FF101010" StrokeThickness="3"
                     StrokeDashArray="20 200" StrokeDashOffset="40"
                     HorizontalAlignment="Center" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="MiniLanguageArc" Width="60" Height="60" Background="#FFFFFFFF">
            <Path Stroke="#FF101010" StrokeThickness="3" Data="M 30,5 A 25,25 0 1 1 29.9,5" />
          </Grid>
          <Grid Name="ExplicitArc" Width="60" Height="60" Background="#FFFFFFFF">
            <Path Stroke="#FF101010" StrokeThickness="3">
              <Path.Data>
                <PathGeometry>
                  <PathFigure StartPoint="30,5" IsClosed="False">
                    <ArcSegment Point="5,30" Size="25,25" />
                  </PathFigure>
                </PathGeometry>
              </Path.Data>
            </Path>
          </Grid>
          <Grid Name="EllipseGeometryArc" Width="60" Height="60" Background="#FFFFFFFF">
            <Path Stroke="#FF101010" StrokeThickness="3">
              <Path.Data>
                <EllipseGeometry Center="30,30" RadiusX="25" RadiusY="25" />
              </Path.Data>
            </Path>
          </Grid>
          <Grid Name="DashCapRound" Width="60" Height="60" Background="#FFFFFFFF">
            <Ellipse Width="50" Height="50" Stroke="#FF101010" StrokeThickness="6"
                     StrokeDashArray="18 200" StrokeDashCap="Round"
                     HorizontalAlignment="Center" VerticalAlignment="Center" />
          </Grid>
        </StackPanel>
        """;

    private static void Ink(Panel root)
    {
        Note("");
        Note("=== B. each candidate geometry: parsed, read back, then measured for ink ===");
        object? parsed;
        try
        {
            parsed = XamlReader.Parse(Snippets);
        }
        catch (Exception exception)
        {
            Note($"  parse threw {exception.GetType().Name}: {Trim(exception.InnerException?.Message ?? exception.Message)}");
            return;
        }

        if (parsed is not FrameworkElement element)
        {
            Note($"  parse produced {parsed?.GetType().Name ?? "null"}, not an element");
            return;
        }

        root.Children.Add(element);
        Pump(8);
        foreach (var name in new[] { "FullRing", "DashArc", "DashArcMoved", "MiniLanguageArc", "ExplicitArc", "EllipseGeometryArc", "DashCapRound" })
        {
            var cell = FindNamed(element, name);
            if (cell is null)
            {
                Note($"  {name,-18} not in tree");
                continue;
            }

            var shape = FirstDescendantOfType(cell, "Ellipse") ?? FirstDescendantOfType(cell, "Path");
            Note($"  {name,-18} size={cell.ActualWidth:0}x{cell.ActualHeight:0} " +
                 $"shape={shape?.GetType().Name ?? "none"} " +
                 $"stroke={Show(Prop(shape, "Stroke") is SolidColorBrush brush ? brush.Color.ToString() : Prop(shape, "Stroke"))} " +
                 $"thickness={Show(Prop(shape, "StrokeThickness"))} " +
                 $"dash={Dash(Prop(shape, "StrokeDashArray"))} " +
                 $"dashOffset={Show(Prop(shape, "StrokeDashOffset"))} " +
                 $"dashCap={Show(Prop(shape, "StrokeDashCap"))} " +
                 $"data={Show(Prop(shape, "Data")?.GetType().Name)}");
            Profile(cell, name);
        }
    }

    private static string Dash(object? value)
    {
        if (value is null)
        {
            return "null";
        }

        if (value is not System.Collections.IEnumerable enumerable || value is string)
        {
            return Show(value);
        }

        var parts = new List<string>();
        foreach (var item in enumerable)
        {
            parts.Add(Show(item));
        }

        return $"[{string.Join(" ", parts)}]";
    }

    // ---------- C. can a code clock drive the dash route, and does the dash route paint? ----------

    private static void Anim(Panel root)
    {
        Note("");
        Note("=== C. animation and static-change readings on the dash route ===");
        var ring = FirstNamedIn(root, "DashArc");
        var ellipse = ring is null ? null : FirstDescendantOfType(ring, "Ellipse");
        if (ellipse is null)
        {
            Note("  no Ellipse to drive; the dash route never mounted");
            return;
        }

        var type = ellipse.GetType();
        void Drive(string label, string dpName, double from, double to)
        {
            var dp = FindDependencyProperty(type, dpName);
            if (dp is null)
            {
                Note($"  {label}: {dpName} ABSENT on {type.Name}");
                return;
            }

            if (ellipse is not UIElement animatable)
            {
                Note($"  {label}: the shape is not a UIElement");
                return;
            }

            try
            {
                animatable.BeginAnimation(dp, new DoubleAnimation
                {
                    From = from,
                    To = to,
                    Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                    RepeatBehavior = RepeatBehavior.Forever,
                });
            }
            catch (Exception exception)
            {
                Note($"  {label}: BeginAnimation threw {exception.GetType().Name}");
                return;
            }

            var first = Show(ReadDp(animatable, dp));
            Pump(12);
            var second = Show(ReadDp(animatable, dp));
            Pump(12);
            var third = Show(ReadDp(animatable, dp));
            animatable.BeginAnimation(dp, null);
            Note($"  {label,-22} {dpName}: {first} -> {second} -> {third}");
        }

        Drive("dash offset", "StrokeDashOffsetProperty", 0, 160);
        Drive("dash array", "StrokeDashArrayProperty", 0, 160);
        Drive("thickness", "StrokeThicknessProperty", 2, 9);
        Drive("opacity", "OpacityProperty", 0.1, 1);
        Drive("width", "WidthProperty", 30, 55);

        // The control that keeps the readings above honest: a plain property write, then a fresh capture.
        foreach (var (offset, label) in new[] { (0.0, "offset 0"), (40.0, "offset 40"), (95.0, "offset 95") })
        {
            SetProp(ellipse, "StrokeDashOffset", offset);
            Pump(6);
            Profile(ring!, $"static {label}");
        }

        Note($"  read back after writes: StrokeDashOffset={Show(Prop(ellipse, "StrokeDashOffset"))}");
    }

    // ---------- D. is a static rotation applied at all? ----------

    private static void Xform(Panel root)
    {
        Note("");
        Note("=== D. static RotateTransform on a shape ===");
        var ring = FirstNamedIn(root, "DashArc");
        var ellipse = ring is null ? null : FirstDescendantOfType(ring, "Ellipse");
        if (ellipse is null)
        {
            Note("  nothing to rotate");
            return;
        }

        SetProp(ellipse, "StrokeDashOffset", 0d);
        Pump(6);
        Profile(ring!, "rotate 0");
        foreach (var angle in new[] { 90d, 180d })
        {
            var transform = new RotateTransform { Angle = angle };
            SetProp(transform, "CenterX", 25d);
            SetProp(transform, "CenterY", 25d);
            SetProp(ellipse, "RenderTransform", transform);
            Pump(6);
            Note($"  transform read back Angle={Show(Prop(Prop(ellipse, "RenderTransform"), "Angle"))} " +
                 $"Center={Show(Prop(Prop(ellipse, "RenderTransform"), "CenterX"))},{Show(Prop(Prop(ellipse, "RenderTransform"), "CenterY"))}");
            Profile(ring, $"rotate {angle:0}");
        }

        // Width-driven rotation of the geometry itself: the mini-language arc has its start point in the data.
        var arcCell = FirstNamedIn(root, "MiniLanguageArc");
        if (arcCell is not null)
        {
            Profile(arcCell, "mini-language arc, re-read");
        }
    }

    // ---------- E. can code repaint an arc, and can either route move? ----------

    private const string ArcMarkup = """
        <Grid xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
              Name="ArcCell" Width="60" Height="60" Background="#FFFFFFFF">
          <Path Name="Arc" Stroke="#FF101010" StrokeThickness="3" Data="M 30,5 A 25,25 0 1 1 29.9,5" />
        </Grid>
        """;

    private const string SegmentMarkup = """
        <Grid xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
              Name="SegmentCell" Width="60" Height="60" Background="#FFFFFFFF">
          <Path Name="SegmentPath" Stroke="#FF101010" StrokeThickness="3">
            <Path.Data>
              <PathGeometry>
                <PathFigure StartPoint="30,5" IsClosed="False">
                  <PathFigure.Segments>
                    <ArcSegment Point="5,30" Size="25,25" SweepDirection="Clockwise" />
                  </PathFigure.Segments>
                </PathFigure>
              </PathGeometry>
            </Path.Data>
          </Path>
        </Grid>
        """;

    private static void Motion(Panel root)
    {
        Note("");
        Note("=== E. repaint and motion routes for an arc of our own making ===");
        var cell = XamlReader.Parse(ArcMarkup) as FrameworkElement;
        if (cell is null)
        {
            Note("  the arc cell did not parse");
            return;
        }

        root.Children.Add(cell);
        Pump(8);
        var arc = FindNamed(cell, "Arc");
        Note($"  mounted: cell={cell.ActualWidth:0}x{cell.ActualHeight:0} arc={arc?.GetType().Name ?? "none"} data={Show(Prop(arc, "Data")?.GetType().Name)}");
        Profile(cell, "E0 markup 359.9-degree arc");

        // E1: is there a code-side parser for mini-language geometry? Without it the sweep angle can only be a
        // per-change string round-trip through the markup reader.
        foreach (var typeName in new[] { "Geometry", "PathGeometry", "StreamGeometry" })
        {
            var type = TypeByName(typeName);
            var parse = type?.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(method => method.Name == "Parse" && method.GetParameters().Length == 1 &&
                    method.GetParameters()[0].ParameterType == typeof(string));
            Note($"  {typeName}.Parse(string): {(parse is null ? "ABSENT" : "present")}");
        }

        // E2: a code-assigned Data - the determinate sweep has to come from somewhere.
        var pathGeometryType = TypeByName("PathGeometry");
        var stringParse = TypeByName("Geometry")?.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(method => method.Name == "Parse" && method.GetParameters().Length == 1 &&
                method.GetParameters()[0].ParameterType == typeof(string));
        if (arc is not null && pathGeometryType is not null && stringParse is not null)
        {
            foreach (var (data, label) in new[]
                     {
                         ("M 30,5 A 25,25 0 0 1 5,30", "90-degree"),
                         ("M 30,5 A 25,25 0 1 1 29.9,5", "359.9-degree"),
                         ("M 30,5 A 25,25 0 0 1 12,48", "135-degree"),
                     })
            {
                var geometry = Read(() => stringParse.Invoke(null, [data]));
                SetProp(arc, "Data", geometry);
                Pump(6);
                Note($"  assigned {label}: data read back as {Show(Prop(arc, "Data")?.GetType().Name)}");
                Profile(cell, $"E2 code Data {label}");
            }
        }

        // E3: the explicit figure route, built in code rather than in markup (B measured the markup one as no ink).
        var figure = BuildFigure();
        if (arc is not null && figure is not null)
        {
            SetProp(arc, "Data", figure);
            Pump(6);
            Profile(cell, "E3 code-built PathFigure+ArcSegment");
        }

        // E4: does a plain per-frame write on an unfrozen RotateTransform move the picture? S1-o only proved that
        // an animation clock refuses to tick on it; a direct DP write is a different route.
        if (arc is not null)
        {
            SetProp(arc, "Data", Read(() => stringParse?.Invoke(null, ["M 30,5 A 25,25 0 0 1 5,30"])));
            Pump(6);
            var rotate = new RotateTransform();
            SetProp(arc, "RenderTransform", rotate);
            var step = 0;
            void OnRendering(object? sender, EventArgs arguments)
            {
                step++;
                SetProp(rotate, "Angle", step * 37d);
            }

            EventHandler handler = OnRendering;
            CompositionTarget.Rendering += handler;
            Pump(10);
            Note($"  after 10 frames Angle read back {Show(Prop(rotate, "Angle"))}");
            Profile(cell, "E4 loop frame 10");
            Pump(20);
            Note($"  after 30 frames Angle read back {Show(Prop(rotate, "Angle"))}");
            Profile(cell, "E4 loop frame 30");
            CompositionTarget.Rendering -= handler;
        }

        // E5: the same arc through the explicit-property markup, to name what B's shorthand missed.
        var segmentCell = XamlReader.Parse(SegmentMarkup) as FrameworkElement;
        if (segmentCell is not null)
        {
            root.Children.Add(segmentCell);
            Pump(8);
            var segmentPath = FindNamed(segmentCell, "SegmentPath");
            var segments = Prop(Prop(segmentPath, "Data"), "Figures");
            Note($"  E5 figures read back: {Show(segments?.GetType().Name)} count={Show(Prop(segments, "Count"))}");
            if (segments is System.Collections.IEnumerable figures)
            {
                foreach (var figureObject in figures)
                {
                    Note($"    figure StartPoint={Show(Prop(figureObject, "StartPoint"))} segments={Show(Prop(Prop(figureObject, "Segments"), "Count"))}");
                }
            }

            Profile(segmentCell, "E5 PathFigure.Segments markup");
        }
    }

    private static object? BuildFigure()
    {
        var geometryType = TypeByName("PathGeometry");
        var figureType = TypeByName("PathFigure");
        var segmentType = TypeByName("ArcSegment");
        var pointType = TypeByName("Point");
        var sizeType = TypeByName("Size");
        if (geometryType is null || figureType is null || segmentType is null || pointType is null)
        {
            Note("  E3 cannot build: a geometry type is absent");
            return null;
        }

        var geometry = Activator.CreateInstance(geometryType);
        var figure = Activator.CreateInstance(figureType);
        var segment = Activator.CreateInstance(segmentType);
        SetProp(figure, "StartPoint", MakePoint(pointType, 30, 5));
        SetProp(segment, "Size", sizeType is null ? (object)"25,25" : MakeSize(sizeType, 25, 25));
        SetProp(segment, "Point", MakePoint(pointType, 5, 30));
        var figures = Prop(geometry, "Figures");
        AddTo(figures, figure);
        AddTo(Prop(figure, "Segments"), segment);
        Note($"  E3 built geometry: figures={Show(Prop(figures, "Count"))} segments={Show(Prop(Prop(figure, "Segments"), "Count"))} " +
             $"segmentPoint={Show(Prop(segment, "Point"))}");
        return geometry;
    }

    private static object? MakePoint(Type type, double x, double y)
    {
        foreach (var argumentType in new[] { typeof(double), typeof(float) })
        {
            var constructor = type.GetConstructor([argumentType, argumentType]);
            if (constructor is not null)
            {
                var value = argumentType == typeof(float)
                    ? constructor.Invoke(new object[] { (float)x, (float)y })
                    : constructor.Invoke(new object[] { x, y });
                Note($"    point ctor {argumentType.Name} -> {Show(value)}");
                return value;
            }
        }

        var instance = Activator.CreateInstance(type)!;
        SetProp(instance, "X", x);
        SetProp(instance, "Y", y);
        Note($"    point via property set -> {Show(instance)}");
        return instance;
    }

    private static object? MakeSize(Type type, double width, double height)
    {
        foreach (var argumentType in new[] { typeof(double), typeof(float) })
        {
            var constructor = type.GetConstructor([argumentType, argumentType]);
            if (constructor is not null)
            {
                return argumentType == typeof(float)
                    ? constructor.Invoke(new object[] { (float)width, (float)height })
                    : constructor.Invoke(new object[] { width, height });
            }
        }

        var instance = Activator.CreateInstance(type)!;
        SetProp(instance, "Width", width);
        SetProp(instance, "Height", height);
        return instance;
    }

    private static void AddTo(object? collection, object item)
    {
        if (collection is null)
        {
            return;
        }

        var add = collection.GetType().GetMethods().FirstOrDefault(method => method.Name == "Add" && method.GetParameters().Length == 1);
        if (add is null)
        {
            Note("    no Add on the collection");
            return;
        }

        Read(() =>
        {
            add.Invoke(collection, [item]);
            return null;
        });
    }

    private static void Surface2()
    {
        Note("");
        Note("=== F. the base type and the geometry surface an own control has to compile against ===");
        foreach (var name in new[] { "RangeBase", "Geometry", "RotateTransform", "Path", "Shape" })
        {
            var type = TypeByName(name);
            if (type is null)
            {
                Note($"  {name}: ABSENT");
                continue;
            }

            Note($"  --- {name} ({(type.IsAbstract ? "abstract" : "concrete")}, {type.Attributes}) ---");
            foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Note($"    ctor({string.Join(", ", constructor.GetParameters().Select(parameter => parameter.ParameterType.Name))}) " +
                     $"{(constructor.IsPublic ? "public" : "protected")}");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                         .Where(method => !method.IsSpecialName)
                         .OrderBy(method => method.Name))
            {
                if (method.Name.StartsWith("get_", StringComparison.Ordinal) || method.Name.StartsWith("set_", StringComparison.Ordinal))
                {
                    continue;
                }

                Note($"    {method.ReturnType.Name,-14} {method.Name}({string.Join(", ", method.GetParameters().Select(parameter => parameter.ParameterType.Name))}) " +
                     $"{(method.IsPublic ? "public" : "protected")}{(method.IsVirtual ? " virtual" : "")}");
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                         .OrderBy(property => property.Name))
            {
                Note($"    prop {property.PropertyType.Name,-18} {property.Name} " +
                     $"{(property.GetMethod?.IsPublic == true ? "public" : "protected")}" +
                     $"{(property.SetMethod is null ? " readonly" : "")}");
            }
        }
    }

    // ---------- shared plumbing ----------

    private static FrameworkElement? FirstNamedIn(DependencyObject root, string name) => FindNamed(root, name);

    private static FrameworkElement? FirstDescendantOfType(DependencyObject root, string typeName)
    {
        foreach (var child in ChildrenOf(root))
        {
            if (child.GetType().Name == typeName && child is FrameworkElement element)
            {
                return element;
            }

            var nested = FirstDescendantOfType(child, typeName);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private static List<DependencyObject> ChildrenOf(DependencyObject root)
    {
        var list = new List<DependencyObject>();

        // The panel route is the honest one for this tree (every cell is a Panel); the VisualTreeHelper route is
        // only a fallback, and which one answered is printed so an empty walk can never read as "no descendants".
        var children = root.GetType().GetProperty("Children", BindingFlags.Public | BindingFlags.Instance)?.GetValue(root);
        if (children is System.Collections.IEnumerable items)
        {
            foreach (var item in items)
            {
                if (item is DependencyObject child)
                {
                    list.Add(child);
                }
            }

            if (list.Count > 0)
            {
                return list;
            }
        }

        var helper = TypeByName("VisualTreeHelper");
        try
        {
            var count = (int)(helper?.GetMethod("GetChildrenCount")?.Invoke(null, [root]) ?? 0);
            var getChild = helper?.GetMethod("GetChild");
            for (var index = 0; index < count; index++)
            {
                if (getChild?.Invoke(null, new object?[] { root, index }) is DependencyObject child)
                {
                    list.Add(child);
                }
            }
        }
        catch (Exception)
        {
            // A cell that cannot enumerate children simply reports no descendants; the ink reading stands alone.
        }

        return list;
    }

    private static object? ReadDp(DependencyObject target, DependencyProperty property)
    {
        var method = typeof(DependencyObject).GetMethod("GetValue",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return method is null ? "no GetValue" : Read(() => method.Invoke(target, [property]));
    }

    private static DependencyProperty? FindDependencyProperty(Type type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            var field = current.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if (field?.GetValue(null) is DependencyProperty property)
            {
                return property;
            }
        }

        return null;
    }

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
        var rows = new int[height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                if (buffer[offset] > 240 && buffer[offset + 1] > 240 && buffer[offset + 2] > 240)
                {
                    continue;
                }

                ink++;
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

    private static FrameworkElement? FindNamed(DependencyObject root, string name)
    {
        foreach (var child in ChildrenOf(root))
        {
            if (child is FrameworkElement element && element.Name == name)
            {
                return element;
            }

            if (FindNamed(child, name) is { } nested)
            {
                return nested;
            }
        }

        return null;
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

    private static string Show(object? value) => value is null ? "null" : Trim(value.ToString() ?? "?");

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

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
