using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace AppBarProbe;

/// <summary>
/// Throwaway probe for the stage-4 command-bar batch, pass 5 - re-measuring the three things the shipped
/// headers now claim on pass 4's authority, because pass 4's own readings turn out not to carry them.
/// Pass 4 wrote a literal Background on every template part it triggered on, and every cell it then drove
/// read back the resting colour: one reading, equally consistent with "cells do not fire" and with "a local
/// value on a part beats a cell", and pass 4 cannot tell them apart. The shipping templates bind their parts
/// instead of setting them and their cells do fire (AstraAppBarTests), so the second answer is the likely one
/// - which would leave pass 4 having established nothing about IsCompact, LabelPosition or MultiTrigger.
///   A: do the SHIPPING cells fire, state by state, on the real styles this batch publishes?
///   B: does a cell beat a local value on the part it names? (the pass-4 artifact, made explicit)
///   C: can a compound state be expressed - does MultiTrigger apply, not just parse?
///   D: which LabelPosition members exist, and which of them change anything?
///   E: VisualStateManager - does it parse, does a template carrying it build into a shown window, and is
///      there any code that could drive a state? (the "no visual-state manager" claim, as a measurement)
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "appbar-probe5.txt");

    private static Application _application = null!;
    private static bool _ran;

    private static readonly Color Rest = Color.FromRgb(0x11, 0x22, 0x33);
    private static readonly Color Disabled = Color.FromRgb(0x00, 0xFF, 0x00);
    private static readonly Color Checked = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color Compound = Color.FromRgb(0xFF, 0x80, 0x00);
    private static readonly Color OnState = Color.FromRgb(0x00, 0xFF, 0xFF);

    [STAThread]
    private static int Main()
    {
        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            _application = application;
            FluentThemeManager.Apply(application);
            Run(application);
        }
        catch (Exception exception)
        {
            Note("BOOT threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        File.WriteAllText(LogPath, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, full log: {LogPath}");
        return 0;
    }

    private static void Run(Application application)
    {
        var dictionary = (ResourceDictionary)XamlReader.Parse(Markup)!;
        application.Resources.MergedDictionaries.Add(dictionary);

        // The shipping styles: no Template of ours on these three, so the implicit Astra style is what runs.
        var button = new AppBarButton { Label = "save", Height = 40 };
        var toggle = new AppBarToggleButton { Label = "bold", Height = 40 };
        var separator = new AppBarSeparator { Height = 40 };

        // B/C/D run on probe templates whose only job is to answer one mechanism question each.
        var local = new AppBarButton { Label = "local", Height = 40, Template = (ControlTemplate)dictionary["LocalTemplate"]! };
        var bound = new AppBarButton { Label = "bound", Height = 40, Template = (ControlTemplate)dictionary["BoundTemplate"]! };
        var multi = new AppBarToggleButton { Label = "multi", Height = 40, Template = (ControlTemplate)dictionary["MultiTemplate"]! };
        var labels = new AppBarButton { Label = "labels", Height = 40, Template = (ControlTemplate)dictionary["LabelTemplate"]! };

        var root = new StackPanel { Margin = new Thickness(24) };
        root.Children.Add(button);
        root.Children.Add(toggle);
        root.Children.Add(separator);
        root.Children.Add(local);
        root.Children.Add(bound);
        root.Children.Add(multi);
        root.Children.Add(labels);
        var window = new Window { Content = root, Width = 900, Height = 700, Title = "AppBar probe 5" };

        window.Loaded += (_, _) =>
        {
            if (_ran)
            {
                return;
            }

            _ran = true;
            try
            {
                Pump(12);

                Note("=== rows the shipping aliases resolve to, for reading the answers against ===");
                foreach (var name in new[]
                         {
                             "AppBarButtonBackground", "AppBarButtonBackgroundPointerOver", "AppBarButtonBackgroundPressed",
                             "AppBarButtonBackgroundDisabled", "AppBarToggleButtonBackgroundChecked",
                             "AppBarButtonInnerBorderMargin", "AppBarButtonInnerBorderCompactMargin",
                         })
                {
                    var value = Res(name);
                    Note($"  {name} = " + (value is SolidColorBrush solid ? Hex(solid.Color)
                        : value is Thickness thickness ? $"{thickness.Left},{thickness.Top},{thickness.Right},{thickness.Bottom}"
                        : value?.GetType().Name ?? "absent"));
                }

                Note(string.Empty);
                Note("=== A. do the SHIPPING cells fire? (real Astra template, part read by name) ===");
                Note($"  style={(button.Style is null ? "null" : "set")} tmpl={(button.Template is null ? "null" : "set")} root part={(Part(button, "Root") is Border ? "found" : "missing")}");
                Note($"  rest: root={Brush(RootBrush(button))} label={(Shown(LabelOf(button)))}");
                Raise(button, UIElement.MouseDownEvent);
                Pump(8);
                Note($"  mouseDown: isPressed={button.IsPressed} root={Brush(RootBrush(button))} (expect the pressed row)");
                Raise(button, UIElement.MouseUpEvent);
                button.ReleaseMouseCapture();
                Pump(8);
                Note($"  mouseUp: isPressed={button.IsPressed} root={Brush(RootBrush(button))} (expect back to rest)");
                button.IsEnabled = false;
                Pump(8);
                Note($"  IsEnabled=false: root={Brush(RootBrush(button))} (expect the disabled row)");
                button.IsEnabled = true;
                Pump(8);
                button.IsCompact = true;
                Pump(8);
                Note($"  IsCompact=true: label={(Shown(LabelOf(button)))} root={Brush(RootBrush(button))} size={button.ActualWidth:0.##}x{button.ActualHeight:0.##}");
                button.IsCompact = false;
                Pump(8);
                Note($"  compact back: label={(Shown(LabelOf(button)))}");
                Note($"  toggle rest: root={Brush(RootBrush(toggle))}");
                toggle.IsChecked = true;
                Pump(8);
                Note($"  toggle IsChecked=true: root={Brush(RootBrush(toggle))} (expect the accent checked row)");
                toggle.IsChecked = null;
                Pump(8);
                Note($"  separator: RootGrid={(Part(separator, "RootGrid")?.GetType().Name ?? "missing")} SeparatorRectangle={(Part(separator, "SeparatorRectangle")?.GetType().Name ?? "missing")} fg={Brush(((Control)separator).Foreground)}");
                Note("  hover: " + SetHover(button, true));
                Pump(8);
                Note($"  hover on: root={Brush(RootBrush(button))} (expect the pointer-over row)");
                Note("  hover off: " + SetHover(button, false));
                Pump(8);

                Note(string.Empty);
                Note("=== B. does a cell beat a local value on the part it names? ===");
                local.IsEnabled = false;
                bound.IsEnabled = false;
                Pump(8);
                Note($"  literal-Background template: root={Brush(RootBrush(local))} -> cell {(Same(RootBrush(local), Disabled) ? "FIRED" : Same(RootBrush(local), Rest) ? "LOST to the local value" : "wrote something else")}");
                Note($"  TemplateBinding-Background template: root={Brush(RootBrush(bound))} -> cell {(Same(RootBrush(bound), Disabled) ? "FIRED" : Same(RootBrush(bound), Rest) ? "did not fire" : "wrote something else")}");
                local.IsEnabled = true;
                bound.IsEnabled = true;
                Pump(8);
                Note($"  back to rest: local={Brush(RootBrush(local))} bound={Brush(RootBrush(bound))}");

                Note(string.Empty);
                Note("=== C. MultiTrigger: single condition, then both ===");
                Note($"  rest: root={Brush(RootBrush(multi))}");
                multi.IsChecked = true;
                Pump(8);
                Note($"  IsChecked only: root={Brush(RootBrush(multi))} (expect {Hex(Checked)})");
                multi.IsEnabled = false;
                Pump(8);
                Note($"  IsChecked + IsEnabled=false: root={Brush(RootBrush(multi))} (expect {Hex(Compound)} if MultiTrigger applies, " +
                     $"{Hex(Disabled)} if the later single cell wins, {Hex(Checked)} if MultiTrigger is inert)");
                multi.IsChecked = null;
                multi.IsEnabled = true;
                Pump(8);

                Note(string.Empty);
                Note("=== D. LabelPosition: what exists, and what does anything ===");
                var enumType = typeof(AppBarButton).GetProperty("LabelPosition")?.PropertyType!;
                Note($"  type={enumType.Name} members={string.Join("/", Enum.GetNames(enumType))}");
                Note($"  baseline (Default): root={Brush(RootBrush(labels))} label={(Shown(LabelOf(labels)))} size={labels.ActualWidth:0.##}x{labels.ActualHeight:0.##}");
                foreach (var name in Enum.GetNames(enumType))
                {
                    var result = Try(() => labels.LabelPosition = (CommandBarLabelPosition)Enum.Parse(enumType, name));
                    Pump(8);
                    Note($"  {name}: set={result} cell={Brush(RootBrush(labels))} label={(Shown(LabelOf(labels)))} size={labels.ActualWidth:0.##}x{labels.ActualHeight:0.##}");
                }

                Note(string.Empty);
                Note("=== E. VisualStateManager ===");
                var managerType = Type.GetType("Jalium.UI.Controls.VisualStateManager, Jalium.UI.Controls")
                    ?? AppDomain.CurrentDomain.GetAssemblies().Select(static assembly => assembly.GetType("Jalium.UI.Controls.VisualStateManager"))
                        .FirstOrDefault(static type => type is not null);
                Note($"  type: {managerType?.FullName ?? "absent from Jalium.UI.Controls"}")
                    ;
                if (managerType is not null)
                {
                    Note("  public static GoToState* methods: " + string.Join(", ", managerType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(static info => info.Name.Contains("GoToState", StringComparison.Ordinal))
                        .Select(static info => info.Name + string.Join("/", info.GetParameters().Select(static parameter => parameter.ParameterType.Name)))));
                }

                Note("  dictionary parse (a template may be lazy, so this alone is not the answer): " + Try(() => XamlReader.Parse(VisualStateMarkup)));
                var live = new AppBarButton { Label = "vsm", Height = 40, Template = (ControlTemplate)dictionary["VsmTemplate"]! };
                Note("  template held: " + Try(() =>
                {
                    root.Children.Add(live);
                    live.UpdateLayout();
                }));
                Pump(10);
                Note($"  in a shown window: built={(Part(live, "Root") is Border ? "yes" : "no")} root={Brush(RootBrush(live))} size={live.ActualWidth:0.##}x{live.ActualHeight:0.##}");
                Note("  GoToState(Disabled): " + Try(() =>
                {
                    var method = managerType?.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(static info => info.Name.Contains("GoToState", StringComparison.Ordinal));
                    if (method is null)
                    {
                        throw new MissingMethodException("no public GoToState* on VisualStateManager");
                    }

                    method.Invoke(null, [live, "Disabled", true]);
                }));
                Pump(8);
                Note($"  after the call: root={Brush(RootBrush(live))}");
            }
            catch (Exception exception)
            {
                Note("LOADED threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            }
            finally
            {
                Note(string.Empty);
                Note("  closing the window: " + Try(() => window.Close()));
            }
        };

        try
        {
            window.Show();
            application.Run(window);
        }
        catch (Exception exception)
        {
            Note("RUN threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }
    }

    /// <summary>
    /// Every probe template here sets its part through TemplateBinding except LocalTemplate, which repeats the
    /// pass-4 shape on purpose: a literal on the part plus a cell naming that part. The two answers together
    /// are the rule, and neither one alone is.
    /// </summary>
    private const string Markup = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='LocalTemplate' TargetType='AppBarButton'>
            <Border x:Name='Root' Background='#FF112233' MinHeight='40' Width='68'>
              <TextBlock x:Name='LabelText' Text='{TemplateBinding Label}' FontSize='12' />
            </Border>
            <ControlTemplate.Triggers>
              <Trigger Property='IsEnabled' Value='False'>
                <Setter TargetName='Root' Property='Background' Value='#FF00FF00' />
              </Trigger>
            </ControlTemplate.Triggers>
          </ControlTemplate>
          <ControlTemplate x:Key='BoundTemplate' TargetType='AppBarButton'>
            <Border x:Name='Root' Background='{TemplateBinding Background}' MinHeight='40' Width='68'>
              <TextBlock x:Name='LabelText' Text='{TemplateBinding Label}' FontSize='12' />
            </Border>
            <ControlTemplate.Triggers>
              <Trigger Property='IsEnabled' Value='False'>
                <Setter TargetName='Root' Property='Background' Value='#FF00FF00' />
              </Trigger>
            </ControlTemplate.Triggers>
          </ControlTemplate>
          <ControlTemplate x:Key='MultiTemplate' TargetType='AppBarToggleButton'>
            <Border x:Name='Root' Background='{TemplateBinding Background}' MinHeight='40' Width='68'>
              <TextBlock x:Name='LabelText' Text='{TemplateBinding Label}' FontSize='12' />
            </Border>
            <ControlTemplate.Triggers>
              <Trigger Property='IsChecked' Value='True'>
                <Setter TargetName='Root' Property='Background' Value='#FFFF00FF' />
              </Trigger>
              <Trigger Property='IsEnabled' Value='False'>
                <Setter TargetName='Root' Property='Background' Value='#FF00FF00' />
              </Trigger>
              <MultiTrigger>
                <MultiTrigger.Conditions>
                  <Condition Property='IsChecked' Value='True' />
                  <Condition Property='IsEnabled' Value='False' />
                </MultiTrigger.Conditions>
                <Setter TargetName='Root' Property='Background' Value='#FFFF8000' />
              </MultiTrigger>
            </ControlTemplate.Triggers>
          </ControlTemplate>
          <ControlTemplate x:Key='LabelTemplate' TargetType='AppBarButton'>
            <Border x:Name='Root' Background='{TemplateBinding Background}' MinHeight='40' Width='68'>
              <TextBlock x:Name='LabelText' Text='{TemplateBinding Label}' FontSize='12' />
            </Border>
            <ControlTemplate.Triggers>
              <Trigger Property='LabelPosition' Value='Left'>
                <Setter TargetName='Root' Property='Background' Value='#FF00FFFF' />
              </Trigger>
              <Trigger Property='LabelPosition' Value='Collapsed'>
                <Setter TargetName='Root' Property='Background' Value='#FF00FFFF' />
              </Trigger>
              <Trigger Property='LabelPosition' Value='Right'>
                <Setter TargetName='Root' Property='Background' Value='#FF00FFFF' />
              </Trigger>
              <Trigger Property='LabelPosition' Value='Hidden'>
                <Setter TargetName='Root' Property='Background' Value='#FF00FFFF' />
              </Trigger>
            </ControlTemplate.Triggers>
          </ControlTemplate>
          <ControlTemplate x:Key='VsmTemplate' TargetType='AppBarButton'>
            <Grid>
              <VisualStateManager.VisualStateGroups>
                <VisualStateGroup x:Name='CommonStates'>
                  <VisualState x:Name='Normal' />
                  <VisualState x:Name='Disabled'>
                    <Storyboard>
                      <ObjectAnimationUsingKeyFrames Storyboard.TargetName='Root' Storyboard.TargetProperty='Background'>
                        <DiscreteObjectKeyFrame KeyTime='0'>
                          <DiscreteObjectKeyFrame.Value>
                            <SolidColorBrush Color='#FF00FF00' />
                          </DiscreteObjectKeyFrame.Value>
                        </DiscreteObjectKeyFrame>
                      </ObjectAnimationUsingKeyFrames>
                    </Storyboard>
                  </VisualState>
                </VisualStateGroup>
              </VisualStateManager.VisualStateGroups>
              <Border x:Name='Root' Background='#FF112233' MinHeight='40' Width='68'>
                <TextBlock x:Name='LabelText' Text='{TemplateBinding Label}' FontSize='12' />
              </Border>
            </Grid>
          </ControlTemplate>
        </ResourceDictionary>
        """;

    private const string VisualStateMarkup = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='T' TargetType='AppBarButton'>
            <Grid>
              <VisualStateManager.VisualStateGroups>
                <VisualStateGroup x:Name='CommonStates'>
                  <VisualState x:Name='Normal' />
                </VisualStateGroup>
              </VisualStateManager.VisualStateGroups>
              <Border x:Name='Root' />
            </Grid>
          </ControlTemplate>
        </ResourceDictionary>
        """;

    /// <summary>IsMouseOver has no public setter; whether a readonly key exists at all is itself the answer.</summary>
    private static string SetHover(Control element, bool value) => Try(() =>
    {
        var field = typeof(UIElement).GetField("IsMouseOverPropertyKey", BindingFlags.Public | BindingFlags.Static);
        if (field?.GetValue(null) is not DependencyProperty property)
        {
            throw new MissingFieldException("UIElement.IsMouseOverPropertyKey");
        }

        element.SetValue(property, value);
    });

    private static Brush? RootBrush(FrameworkElement element) => (Part(element, "Root") as Border)?.Background;

    private static FrameworkElement? LabelOf(FrameworkElement element) => Part(element, "LabelText");

    private static string Shown(FrameworkElement? element) => element is null ? "no part" : element.IsVisible ? "visible" : "collapsed";

    private static bool Same(Brush? brush, Color color) => brush is SolidColorBrush solid
        && solid.Color.R == color.R && solid.Color.G == color.G && solid.Color.B == color.B;

    private static string Hex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    private static FrameworkElement? Part(DependencyObject? root, string name)
    {
        if (root is not Visual visual)
        {
            return null;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
        {
            var child = VisualTreeHelper.GetChild(visual, index);
            if (child is FrameworkElement element && element.Name == name)
            {
                return element;
            }

            if (Part(child, name) is { } match)
            {
                return match;
            }
        }

        return null;
    }

    private static object? Res(string key)
    {
        try
        {
            return _application.TryFindResource(key);
        }
        catch (Exception exception)
        {
            return "threw " + exception.GetType().Name;
        }
    }

    private static void Raise(Control element, RoutedEvent routedEvent)
    {
        var arguments = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = routedEvent,
            Source = element,
        };
        element.RaiseEvent(arguments);
    }

    private static string Try(Action action)
    {
        try
        {
            action();
            return "ok";
        }
        catch (Exception exception)
        {
            return exception.GetType().Name + ": " + Trim(exception.Message);
        }
    }

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

    private static string Brush(Brush? brush) =>
        brush is SolidColorBrush solid ? Hex(solid.Color)
        : brush is null ? "-"
        : brush.GetType().Name;

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
