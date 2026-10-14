using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Markup;
using Jalium.UI.Interop;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace RightGapProbe;

/// <summary>
/// The "right gap" question, made measurable. The reading from the running Gallery is that several controls
/// carry a wider empty band on their right than the inset their content starts at on the left. This lays the
/// suspect controls out once, all at the same width, on a shown window, prints each one's own geometry, and
/// holds for an outside PrintWindow capture - so a gap becomes a number twice over, once from the tree and
/// once from the frame.
///   --open closed   : every control at rest.
///   --open suggest  : the AutoCompleteBox with its suggestion list dropped, the pass that showed the hole.
///   --open combo    : the combo box dropped.
///   --open listheight : 24 labels through the suggestion list at four ceilings, the combo, the flyout.
///   --quick         : hold two seconds instead of forty-five, for a run that only reads the tree.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private static string Mode = "closed";

    [STAThread]
    private static int Main(string[] arguments)
    {
        for (var index = 0; index < arguments.Length; index++)
        {
            if (arguments[index] == "--open" && index + 1 < arguments.Length)
            {
                Mode = arguments[index + 1];
            }
        }

        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            FluentThemeManager.Apply(application);
            Run(Mode, arguments);
        }
        catch (Exception exception)
        {
            Note("BOOT threw " + exception.GetType().Name + ": " + (exception.InnerException?.Message ?? exception.Message));
        }

        var path = Path.Combine(AppContext.BaseDirectory, $"right-gap-{Mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, full log: {path}");
        return 0;
    }

    private static void Run(string mode, string[] arguments)
    {
        if (mode == "types")
        {
            // "Add a custom control only for a demonstrated behavior gap" needs the inventory first: which
            // types does the runtime actually own? Written out of the loaded assemblies rather than from
            // memory, so the claim "there is no dialog here" is a reading and not a guess.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                         .Where(static candidate => candidate.GetName().Name?.StartsWith("Jalium", StringComparison.Ordinal) == true)
                         .OrderBy(static candidate => candidate.GetName().Name, StringComparer.Ordinal))
            {
                Type[] types;
                try
                {
                    types = assembly.GetExportedTypes();
                }
                catch (Exception exception)
                {
                    Note($"== {assembly.GetName().Name}: unreadable ({exception.GetType().Name})");
                    continue;
                }

                Note($"== {assembly.GetName().Name}: {types.Length} exported types");
                foreach (var type in types.OrderBy(static candidate => candidate.FullName, StringComparer.Ordinal))
                {
                    Note("  " + type.FullName);
                }
            }

            return;
        }

        if (mode == "reflect")
        {
            // The last question the right-gap batch left open is whether the runtime gives a popup's surface
            // any width lever at all. Before calling the hole framework-owned, ask the type what it owns.
            foreach (var type in new[] { typeof(AutoCompleteBox), typeof(Popup), typeof(ComboBox), typeof(NumberBox), typeof(ContentDialog) })
            {
                Note($"== {type.FullName} : width / popup levers");
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .OrderBy(static property => property.Name, StringComparer.Ordinal))
                {
                    if (property.Name.Contains("Width", StringComparison.Ordinal)
                        || property.Name.Contains("Height", StringComparison.Ordinal)
                        || property.Name.Contains("Popup", StringComparison.Ordinal)
                        || property.Name.Contains("DropDown", StringComparison.Ordinal)
                        || property.Name.Contains("Template", StringComparison.Ordinal)
                        || property.Name.Contains("Button", StringComparison.Ordinal)
                        || property.Name.Contains("Title", StringComparison.Ordinal)
                        || property.Name.Contains("Container", StringComparison.Ordinal)
                        || property.Name.Contains("Item", StringComparison.Ordinal)
                        || property.Name.Contains("Placement", StringComparison.Ordinal))
                    {
                        Note($"  {property.PropertyType.Name} {property.Name}"
                             + $" get={property.CanRead} set={property.CanWrite} declaredBy={property.DeclaringType?.Name}");
                    }
                }
            }

            return;
        }

        if (mode == "corner")
        {
            // The second half of the corner complaint. Two faults look identical on screen: the radius token
            // a surface wears is wrong, or the surface rounds its own fill but never clips the square child
            // drawn on top of it - in which case every popup whose content reaches the corner shows a square
            // notch no matter what radius the style asks for. A known third-colour backdrop separates the
            // three answers: blue = clipped, white = rounded fill only, red = the child overflows the corner.
            foreach ((string label, int radius, bool square) in new[]
                     {
                         ("radius 12, square child", 12, true),
                         ("radius 12, no child", 12, false),
                         ("radius 0, square child (control)", 0, true),
                     })
            {
                var surface = new Border
                {
                    Width = 60,
                    Height = 60,
                    CornerRadius = new CornerRadius(radius),
                    Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
                    Child = square
                        ? new Border { Width = 60, Height = 60, Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00)) }
                        : null,
                };
                var backdrop = new Border
                {
                    Width = 60,
                    Height = 60,
                    Background = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0xFF)),
                    Child = surface,
                };

                var shot = new Window { Content = backdrop, Width = 240, Height = 240 };
                shot.Show();
                Pump(20, 1500);
                Note($"== {label}: surface {surface.ActualWidth}x{surface.ActualHeight} Clip={Property(surface, "Clip")}");
                foreach ((int x, int y) in new[] { (0, 0), (1, 1), (2, 2), (4, 4), (12, 0), (30, 1), (30, 30) })
                {
                    Note($"    ({x},{y}) {PixelAt(backdrop, x, y)}");
                }

                shot.Close();
                Pump(5, 800);
            }

            return;
        }

        if (mode == "dialog")
        {
            // The plan said "ContentDialog (own type)"; the inventory (S0-v) says the runtime ships one. Before
            // either claim is written down, ask the mounted control the three questions this repo decides every
            // control batch by: is there a template to displace, which part names does the control's own code
            // read, and which palette rows does the framework's template consume. All through reflection, so the
            // probe keeps compiling against a member list nobody has verified.
            var dialogType = typeof(ContentDialog);
            Note("== declared members");
            foreach (var property in dialogType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(candidate => candidate.DeclaringType == dialogType)
                         .OrderBy(static candidate => candidate.Name, StringComparer.Ordinal))
            {
                Note($"  prop {property.PropertyType.Name} {property.Name} set={property.CanWrite}");
            }

            foreach (var method in dialogType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                         .Where(candidate => candidate.DeclaringType == dialogType)
                         .OrderBy(static candidate => candidate.Name, StringComparer.Ordinal))
            {
                Note($"  method {method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(static p => p.ParameterType.Name))})");
            }

            foreach (var dialogEvent in dialogType.GetEvents(BindingFlags.Public | BindingFlags.Instance)
                         .Where(candidate => candidate.DeclaringType == dialogType)
                         .OrderBy(static candidate => candidate.Name, StringComparer.Ordinal))
            {
                Note($"  event {dialogEvent.EventHandlerType?.Name} {dialogEvent.Name}");
            }

            var fresh = new ContentDialog();
            Note($"-- fresh: Style={(fresh.Style is null ? "null" : "set")} Template={(fresh.Template is null ? "null" : "set")}");

            var shell = new Window
            {
                Content = new ContentPresenter(),
                Width = 700,
                Height = 600,
                Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
            };
            shell.Show();
            Pump(20, 1500);

            var mounted = new ContentDialog
            {
                Title = "Publish this draft?",
                Content = "Once it is public, readers see the version as of now.",
                PrimaryButtonText = "Publish",
                SecondaryButtonText = "Save draft",
                CloseButtonText = "Cancel",
            };
            Note($"-- mounted (our theme applied): Style={(mounted.Style is null ? "null" : "set")} Template={(mounted.Template is null ? "null" : "set")}");

            // The framework builds a default template at mount, not at construction, so the read that decides
            // "re-template or own type" has to happen after the control is in a shown tree.
            ((ContentPresenter)shell.Content!).Content = mounted;
            shell.UpdateLayout();
            Pump(20, 1500);
            Note($"-- in tree: Style={(mounted.Style is null ? "null" : "set")} Template={(mounted.Template is null ? "null" : "set")} " +
                 $"size={mounted.ActualWidth}x{mounted.ActualHeight}");
            Walk(mounted, 0);

            // VisualTreeXaml is public on ControlTemplate here, so the framework's own template can be read
            // rather than guessed - that is where the part names and the {ThemeResource} rows it consumes live.
            Note($"-- template reads {mounted.Template?.GetType().FullName ?? "null"}");
            var visualTree = mounted.Template is null ? null : typeof(ControlTemplate).GetProperty("VisualTreeXaml")?.GetValue(mounted.Template);
            Note(visualTree is null ? "-- VisualTreeXaml: unreadable" : "-- VisualTreeXaml:\n" + visualTree);

            Hold(2);
            shell.Close();
            return;
        }

        if (mode == "shown")
        {
            // Every ContentDialog reading taken so far was taken on a merely mounted control, which measured:
            // Visibility=Collapsed and 0x0. That is not a dialog, and a claim about a card's size box, a
            // generated text element or a click handler cannot be settled on it. This opens one for real.
            var shell = new Window
            {
                Content = new Grid(),
                Width = 900,
                Height = 700,
                Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
            };
            shell.Show();
            Pump(20, 1500);

            var dialog = new ContentDialog
            {
                Title = "Publish this draft?",
                Content = "Once it is public, readers see the version as of now.",
                PrimaryButtonText = "Publish",
                SecondaryButtonText = "Save draft",
                CloseButtonText = "Cancel",
            };
            Note("-- A: detached, before ShowAsync (no tree to read)");

            var opened = 0;
            var closed = 0;
            var clicks = 0;
            dialog.Opened += (_, _) => opened++;
            dialog.Closed += (_, _) => closed++;
            dialog.PrimaryButtonClick += (_, _) => clicks++;
            var operation = dialog.ShowAsync();
            Pump(40, 2500);
            Note($"-- B: ShowAsync on a detached dialog (completed={operation.IsCompleted} fault={operation.Exception?.Message ?? "none"})");
            DumpDialog(dialog, shell);
            Note("   tree:");
            Walk(dialog, 1);

            if (ByName(dialog, "PART_PrimaryButton") is Button primary)
            {
                Note($"   primary: content={primary.Content} enabled={primary.IsEnabled} style={primary.Style?.GetType().Name}");
                ((Jalium.UI.Automation.Provider.IInvokeProvider)new Jalium.UI.Automation.Peers.ButtonAutomationPeer(primary)).Invoke();
                Pump(40, 2500);
                Note($"-- C: after invoking the primary button. opened={opened} closed={closed} clicks={clicks} " +
                     $"completed={operation.IsCompleted} result={TaskResult(operation)} vis={dialog.Visibility}");
                DumpDialog(dialog, shell);
            }
            else
            {
                Note($"-- C: no PART_PrimaryButton in the built tree (opened={opened} closed={closed})");
            }

            var capped = new ContentDialog
            {
                Title = "Cap check",
                Content = "body",
                CloseButtonText = "Cancel",
                MaxWidth = 548,
                MinWidth = 320,
            };
            var second = capped.ShowAsync();
            Pump(40, 2500);
            Note($"-- D: a dialog whose OWN MaxWidth is 548 (completed={second.IsCompleted})");
            DumpDialog(capped, shell);
            capped.Hide();
            Pump(10, 800);
            Note("-- E: after Hide()");
            DumpDialog(capped, shell);

            shell.Close();
            Pump(4, 400);
            return;
        }

        if (mode == "listheight")
        {
            // "The suggestion dropdown is obviously small next to a combo box or a flyout." Three surfaces, the
            // same 24 short labels, the same 260 DIP width, so the answer is a ratio and not an impression.
            // The response curve matters more than one reading: if the surface height moves when only
            // MaxDropDownHeight moves, that property is the lever and no markup literal can substitute for it.
            WalkDepth = 3;
            var labels = Enumerable.Range(0, 24).Select(static index => $"Item {index:00}").ToList();
            var stack = new StackPanel { Margin = new Thickness(16) };
            var shot = new Window
            {
                Content = stack,
                Width = 420,
                Height = 680,
                Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
            };

            var list = new AutoCompleteBox { Width = 260 };
            list.ItemsSource = labels;
            stack.Children.Add(list);
            shot.Show();
            Pump(20, 1500);
            shot.Activate();
            list.Focus();
            // ItemFilter on this runtime is Func<string, object, bool>: TEXT FIRST, item second - reversed from
            // WinUI's predicate. An earlier pass of this probe named the parameters the other way around, so
            // every candidate failed the filter and the popup stayed shut; the readings taken after that looked
            // like a capped list and were an empty one.
            list.ItemFilter = static (text, item) => text is { Length: > 0 } prefix
                && item?.ToString()?.Contains(prefix, StringComparison.OrdinalIgnoreCase) == true;
            list.Text = "I";
            Pump(40, 2500);
            if (ValueOf(list, "IsDropDownOpen") is false)
            {
                // Setting Text programmatically may not be an edit as far as the control is concerned. Say which
                // door was used instead of reporting a closed popup as a sizing answer.
                typeof(AutoCompleteBox).GetProperty("IsDropDownOpen")?.SetValue(list, true);
                Pump(40, 2500);
                Note("   (opened by IsDropDownOpen, not by the text change)");
            }

            Note($"== A: AutoCompleteBox, 24 matching rows, text={ValueOf(list, "Text")}, MaxDropDownHeight as our style leaves it = {ValueOf(list, "MaxDropDownHeight")} ==");
            DumpSuggest(list);

            typeof(AutoCompleteBox).GetProperty("MaxDropDownHeight")?.SetValue(list, 374d);
            Pump(40, 2500);
            Note($"== B: same box, MaxDropDownHeight set to 374 (upstream's AutoSuggestListMaxHeight) = {ValueOf(list, "MaxDropDownHeight")} ==");
            DumpSuggest(list);

            typeof(AutoCompleteBox).GetProperty("MaxDropDownHeight")?.SetValue(list, 120d);
            Pump(40, 2500);
            Note("== C: same box, MaxDropDownHeight set to 120, the second point of the curve ==");
            DumpSuggest(list);

            typeof(AutoCompleteBox).GetProperty("MaxDropDownHeight")?.SetValue(list, 374d);
            Pump(40, 2500);

            // A screen clamp and a property cap read the same at one point, so the curve gets a fourth sample
            // above any plausible surface: if 700 stops where 374 stops, the limiter is not the property.
            Note($"   screen: {GetSystemMetrics(0)}x{GetSystemMetrics(1)} px, work area {GetSystemMetrics(17)}x{GetSystemMetrics(30)} px"
                 + $" | window top={Property(shot, "Top")} left={Property(shot, "Left")} size={SizeOf(shot)} box={SizeOf(list)}");
            typeof(AutoCompleteBox).GetProperty("MaxDropDownHeight")?.SetValue(list, 700d);
            Pump(40, 2500);
            Note("== C2: same box, MaxDropDownHeight set to 700, above what the 24 rows need ==");
            DumpSuggest(list);

            typeof(AutoCompleteBox).GetProperty("MaxDropDownHeight")?.SetValue(list, 374d);
            Pump(40, 2500);
            Note("== C3: back to 374 ==");
            DumpSuggest(list);

            var drop = new ComboBox { Width = 260 };
            foreach (var label in labels)
            {
                drop.Items.Add(label);
            }

            stack.Children.Add(drop);
            Pump(10, 800);
            drop.Focus();
            drop.IsDropDownOpen = true;
            Pump(6, 600);
            Note($"== D: ComboBox, same 24 rows, open={drop.IsDropDownOpen}, MaxDropDownHeight={ValueOf(drop, "MaxDropDownHeight")} ==");
            DumpSuggest(drop);

            var flyout = new MenuFlyout();
            MenuFlyoutItem? first = null;
            foreach (var label in labels)
            {
                first ??= new MenuFlyoutItem { Text = label };
                flyout.Items.Add(new MenuFlyoutItem { Text = label });
            }

            var anchor = new Button { Content = "anchor", Width = 260 };
            stack.Children.Add(anchor);
            Pump(10, 800);
            anchor.Focus();
            try
            {
                flyout.ShowAt(anchor);
            }
            catch (Exception exception)
            {
                Note("  ShowAt threw " + exception.Message);
            }

            Pump(40, 2500);
            Note($"== E: MenuFlyout, same 24 rows, open={flyout.IsOpen} ==");
            Report("flyout row 0", first!);
            foreach (var item in flyout.Items.OfType<MenuFlyoutItem>().Take(3))
            {
                Note($"   row \"{item.Text}\" {item.ActualWidth:0.#}x{item.ActualHeight:0.#} padding={Property(item, "Padding")} radius={Property(item, "CornerRadius")}");
            }

            Note($"   flyout rows realized={flyout.Items.OfType<MenuFlyoutItem>().Count(static item => item.ActualHeight > 0)}"
                 + $" summed={flyout.Items.OfType<MenuFlyoutItem>().Sum(static item => item.ActualHeight):0.#}");

            shot.Close();
            Pump(4, 400);
            return;
        }

        if (mode == "clamp")
        {
            // The test host is a 420x300 window, and there the same 24-row list stopped at 41.78 DIP - one row.
            // If the surface is capped by the room left below the box rather than by its own ceiling, then
            // where a page happens to put the control decides how many suggestions fit, and that is the
            // "obviously small" the Gallery shows. Two boxes, same window, same 24 items, one near the top and
            // one near the bottom, is the whole question.
            var labels = Enumerable.Range(0, 24).Select(static index => $"Item {index:00}").ToList();
            var canvas = new Canvas();
            var high = new AutoCompleteBox { Width = 260 };
            var low = new AutoCompleteBox { Width = 260 };
            high.ItemsSource = labels;
            low.ItemsSource = labels;
            Canvas.SetTop(high, 20);
            Canvas.SetLeft(high, 20);
            Canvas.SetTop(low, 560);
            Canvas.SetLeft(low, 20);
            canvas.Children.Add(high);
            canvas.Children.Add(low);
            var shot = new Window
            {
                Content = canvas,
                Width = 420,
                Height = 700,
                Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
            };
            shot.Show();
            Pump(20, 1500);
            shot.Activate();

            high.Focus();
            high.Text = "I";
            Pump(40, 2500);
            Note($"== box at y=20 of a 700 DIP window, 24 matching rows ==");
            DumpSuggest(high);
            high.IsDropDownOpen = false;
            high.Text = string.Empty;
            Pump(10, 800);

            low.Focus();
            low.Text = "I";
            Pump(40, 2500);
            Note($"== same control at y=560 of the same window, same 24 rows ==");
            DumpSuggest(low);
            shot.Close();
            Pump(4, 400);
            return;
        }

        if (mode == "itemstyle")
        {
            // The suggestion rows measure 35.8 DIP while the ComboBoxItem style this library ships is
            // MinHeight=32 with 11,5,11,7 padding. Either the style never reaches a container the framework
            // generates for a data-bound item, or 32 is not what our own style produces. Declared and generated
            // containers of the same type, in the same window, answer that with one comparison - and the answer
            // is not about this control: every data-bound list in the product rides on it.
            WalkDepth = 4;
            var names = new[] { "Balanced", "Focused", "Relaxed" };
            var declared = new ComboBox { Width = 260 };
            foreach (var name in names)
            {
                declared.Items.Add(new ComboBoxItem { Content = name });
            }

            var generated = new ComboBox { Width = 260, Margin = new Thickness(0, 40, 0, 0) };
            foreach (var name in names)
            {
                generated.Items.Add(name);
            }

            declared.SelectedIndex = 0;
            generated.SelectedIndex = 0;
            var stack = new StackPanel { Margin = new Thickness(16) };
            stack.Children.Add(declared);
            stack.Children.Add(generated);
            var shot = new Window
            {
                Content = stack,
                Width = 420,
                Height = 680,
                Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
            };
            shot.Show();
            Pump(20, 1500);

            void DumpRows(string label, DependencyObject root)
            {
                var rows = new List<FrameworkElement>();
                CollectRows(root, rows, 0);
                Note($"== {label}: rows={rows.Count} " + string.Join(" | ", rows.Select(static row =>
                    $"{row.GetType().Name} {row.ActualWidth:0.#}x{row.ActualHeight:0.#}"
                    + $" minH={row.MinHeight} localMinH={Local(row, "MinHeight")} pad={Property(row, "Padding")}"
                    + $" r={Property(row, "CornerRadius")} font={Property(row, "FontSize")} content={ValueOf(row, "Content")}")));
                if (rows.Count > 0)
                {
                    Note("   first row subtree:");
                    WalkSizes(rows[0], 1);
                }
            }

            shot.Activate();
            declared.Focus();
            declared.IsDropDownOpen = true;
            Pump(40, 2500);
            DumpRows("declared ComboBoxItems (markup)", declared);
            DumpRows("declared, from the popup child", (ValueOf(FindDescendant(declared, "PART_Popup"), "Child") as DependencyObject)!);
            declared.IsDropDownOpen = false;
            Pump(10, 800);

            generated.Focus();
            generated.IsDropDownOpen = true;
            Pump(40, 2500);
            DumpRows("generated containers (strings)", generated);
            DumpRows("generated, from the popup child", (ValueOf(FindDescendant(generated, "PART_Popup"), "Child") as DependencyObject)!);
            generated.IsDropDownOpen = false;
            Pump(10, 800);

            var list = new AutoCompleteBox { Width = 260, Margin = new Thickness(0, 40, 0, 0) };
            list.ItemsSource = names;
            stack.Children.Add(list);
            Pump(10, 800);
            shot.Activate();
            list.Focus();
            list.Text = "B";
            Pump(40, 2500);
            DumpRows("suggestion rows", (ValueOf(FindDescendant(list, "PART_Popup"), "Child") as DependencyObject)!);
            DumpSuggest(list);

            // Two ways the rows could still be reached, and they lead to different fixes: hand the control
            // containers the theme can style on its own, or put the style on the row after the fact. Both are
            // read here because the product needs to know which one it can ask a caller to do.
            var ours = FluentThemeManager.GetStyle("DefaultComboBoxItemStyle");
            var generated2 = new List<FrameworkElement>();
            CollectRows((ValueOf(FindDescendant(list, "PART_Popup"), "Child") as DependencyObject)!, generated2, 0);
            if (generated2.Count > 0)
            {
                var row = generated2[0];
                Note($"   before repair: minH={row.MinHeight} style={row.Style?.GetType().Name ?? "null"}");
                row.Style = ours;
                row.UpdateLayout();
                Pump(20, 1200);
                Note($"   after Style=ours: minH={row.MinHeight} size={SizeOf(row)} r={Property(row, "CornerRadius")}"
                     + $" pill={FindDescendant(row, "Pill")?.GetType().Name ?? "no PART named Pill"}");
                CollectRows((ValueOf(FindDescendant(list, "PART_Popup"), "Child") as DependencyObject)!, generated2, 0);
            }

            list.Text = string.Empty;
            Pump(10, 800);
            var boxed = new AutoCompleteBox { Width = 260, Margin = new Thickness(0, 8, 0, 0) };
            boxed.ItemsSource = names.Select(static name => (object)new ComboBoxItem { Content = name }).ToList();
            stack.Children.Add(boxed);
            Pump(10, 800);
            shot.Activate();
            boxed.Focus();
            boxed.Text = "B";
            Pump(40, 2500);
            DumpRows("suggestion rows from declared containers", (ValueOf(FindDescendant(boxed, "PART_Popup"), "Child") as DependencyObject)!);

            // The captured frame carries two marks the row template has no part for: a ring at the first row's
            // left and a "(" at the second's, the second one outside the card's own edge. Naming them needs an
            // offset, not a type, so every painted element under the popup's root is printed with where it
            // landed in the window.
            list.Text = "B";
            Pump(40, 2500);
            var surface = ValueOf(FindDescendant(list, "PART_Popup"), "Child") as DependencyObject;
            DependencyObject? root = surface;
            for (DependencyObject? node = surface; node is not null; node = VisualTreeHelper.GetParent(node))
            {
                if (node.GetType().Name != "PopupRoot")
                {
                    continue;
                }

                root = node;
                break;
            }

            Note($"== every painted element under {root?.GetType().Name ?? "null"}, offsets in DIP vs the window ==");
            WalkOffsets(root!, shot, 1);
            shot.Close();
            Pump(4, 400);
            return;
        }

        if (mode == "suggest10")
        {
            // The Gallery's own fruit box, reproduced name for name: ten items, the control's default filter,
            // MaxWidth 440 in a stretching card. The layout readings already say the surface is 374 tall, so
            // the only thing left to settle is what reaches the frame - this holds with the list open for
            // spike/VisualQA/capture-pid-windows.ps1 to printwindow.
            var fruits = new[]
            {
                "Apple", "Apricot", "Banana", "Blueberry", "Cherry", "Grapefruit", "Lemon", "Mango", "Orange", "Peach",
            };
            var field = new AutoCompleteBox
            {
                MaxWidth = 440,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                PlaceholderText = "Start typing a fruit",
            };
            field.ItemsSource = fruits;
            var picker = new ComboBox
            {
                MaxWidth = 440,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 24, 0, 0),
            };
            foreach (var fruit in fruits)
            {
                picker.Items.Add(fruit);
            }

            picker.SelectedIndex = 0;

            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x2D)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x39, 0x39, 0x39)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                MaxWidth = 480,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = "Fruit" },
                        field,
                        new TextBlock { Text = "Choices", Margin = new Thickness(0, 24, 0, 0) },
                        picker,
                    },
                },
            };

            var holder = new Window
            {
                Content = new StackPanel { Margin = new Thickness(24), Children = { card } },
                Width = 700,
                Height = 760,
                Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
            };
            holder.Show();
            Pump(20, 1500);
            holder.Activate();
            field.Focus();
            field.Text = arguments.FirstOrDefault(static argument => argument.StartsWith("text=", StringComparison.Ordinal))?[5..] ?? "a";
            Pump(40, 2500);
            Note($"== Gallery-shaped fruit box, text=\"{field.Text}\" == box={SizeOf(field)}");
            DumpSuggest(field);
            Console.Out.WriteLine("open");
            Console.Out.Flush();
            Note("holding for a capture");
            Hold(arguments.Contains("--fast") ? 6 : 25);

            field.Text = string.Empty;
            Pump(10, 800);
            picker.Focus();
            picker.IsDropDownOpen = true;
            Pump(40, 2500);
            Note($"== same card, the picker dropped instead, items={picker.Items.Count} ==");
            DumpSuggest(picker);
            Console.Out.WriteLine("picker-open");
            Console.Out.Flush();
            Note("holding for the picker capture");
            Hold(arguments.Contains("--fast") ? 6 : 25);
            holder.Close();
            Pump(4, 400);
            return;
        }

        var panel = new StackPanel { Margin = new Thickness(24), Spacing = 10 };
        var window = new Window
        {
            Content = panel,
            Width = 760,
            Height = 900,
            Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
        };

        var combo = new ComboBox { Width = 260 };
        foreach (var name in new[] { "Compact", "Comfortable", "Cozy" })
        {
            combo.Items.Add(name);
        }

        combo.SelectedIndex = 0;

        var suggest = new AutoCompleteBox { Width = 260, Text = "Ap" };
        suggest.ItemsSource = new List<string> { "Apple", "Apricot", "Avocado" };

        var number = new NumberBox { Width = 260, Value = 42, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline };
        var split = new SplitButton { Width = 260, Content = "Split primary" };
        var dropDown = new FluentDropDownButton { Width = 260, Content = "Drop-down" };
        var expander = new Expander { Width = 260, Header = "Weekly digest", Content = "the body" };
        var navItem = new FluentNavigationItem { Content = "Teaching tip and dialogs", Width = 260 };

        Add(panel, "TextBox", new TextBox { Width = 260, Text = "Astra" });
        Add(panel, "PasswordBox", new PasswordBox { Width = 260, Password = "secret" });
        Add(panel, "ComboBox", combo);
        Add(panel, "NumberBox", number);
        Add(panel, "AutoCompleteBox", suggest);
        Add(panel, "SplitButton", split);
        Add(panel, "FluentDropDownButton", dropDown);
        Add(panel, "Expander", expander);
        Add(panel, "Nav item (long label)", navItem);

        window.Show();
        Pump(20, 1500);

        if (mode == "suggest")
        {
            // The suggestion list opens on its own once the text is edited after the box is on screen, and the
            // combo has to stay out of the way: focus moves close it again.
            window.Activate();
            suggest.Focus();
            // "Apr" filters down to one row, which cannot show a clipped list. A contains-filter on "a" realizes
            // most of the ten fruits, so the question "does the surface grow with its rows or stop at the 32 DIP
            // the framework stamped on its own popup root" is answered by the row count against the height.
            // Text first, item second - see the note in listheight mode.
            suggest.ItemFilter = static (text, item) => text is { Length: > 0 } prefix
                && item?.ToString()?.Contains(prefix, StringComparison.OrdinalIgnoreCase) == true;
            suggest.Text = "a";
            Pump(40, 2500);
            Note("-- suggestion list open, contains filter on 'a' --");
            foreach (var candidate in new[] { "MaxDropDownHeight", "DropDownHeight", "ItemCount", "FilteredItems", "ItemsSource", "ItemFilter", "Text" })
            {
                var value = ValueOf(suggest, candidate);
                Note($"   suggest.{candidate} = " + (value is System.Collections.ICollection collection ? $"count={collection.Count}" : value?.ToString() ?? "no such property"));
            }

            FindNamed(window, 0);
            DumpPopup(suggest, "suggest");
            DumpPopup(combo, "combo (closed, for the stamped size)");
        }
        else if (mode == "combo")
        {
            window.Activate();
            combo.Focus();
            combo.IsDropDownOpen = true;
            Pump(40, 2500);
            Note($"-- combo box open={combo.IsDropDownOpen} --");
            FindNamed(window, 0);
            DumpPopup(combo, "combo");
            DumpPopup(suggest, "suggest (closed, for the stamped size)");
        }
        else if (mode == "flyout")
        {
            // A MenuFlyout is the one popup family this runtime actually puts on screen, so it is the only
            // open surface whose left/right inset can be read off pixels instead of off a property.
            var rename = new MenuFlyoutItem { Text = "Rename", KeyboardAcceleratorTextOverride = "F2" };
            var archive = new MenuFlyoutItem { Text = "Archive" };
            var receipts = new ToggleMenuFlyoutItem { Text = "Show read receipts", IsChecked = true };
            var sub = new MenuFlyoutSubItem { Text = "Share with" };
            sub.Items.Add(new MenuFlyoutItem { Text = "nested" });
            var delete = new MenuFlyoutItem { Text = "Delete", IsEnabled = false };
            var flyout = new MenuFlyout();
            flyout.Items.Add(rename);
            flyout.Items.Add(archive);
            flyout.Items.Add(receipts);
            flyout.Items.Add(new MenuFlyoutSeparator());
            flyout.Items.Add(sub);
            flyout.Items.Add(delete);
            var anchor = new Button { Content = "anchor", Width = 260 };
            panel.Children.Add(anchor);
            Pump(10, 800);
            window.Activate();
            anchor.Focus();
            try
            {
                flyout.ShowAt(anchor);
            }
            catch (Exception exception)
            {
                Note("  ShowAt threw " + exception.Message);
            }

            // IsOpen is read at four points because the question is not just "is the presenter there" but
            // "when is it there": a ShowAt that light-dismisses before the read makes a "not found" answer
            // mean nothing, which is exactly how the previous pass of this mode read inconclusive.
            void SampleSurface(string stage)
            {
                Note($"-- surface sample {stage}: open={flyout.IsOpen}");

                // "Not found under the window" has two causes and they are different answers: the framework may
                // not use that type, or the popup may live above a root the window walk never reaches. Climbing
                // from a row the flyout did realize settles which, because that chain is the popup's own ancestry.
                var chain = new List<DependencyObject>();
                for (DependencyObject? node = rename; node is not null; node = VisualTreeHelper.GetParent(node))
                {
                    chain.Add(node);
                }

                Note($"   {stage}: row climb {chain.Count} up = " + string.Join(" > ", chain.Select(static node =>
                    node is FrameworkElement element && element.Name.Length > 0
                        ? $"{node.GetType().Name}\"{element.Name}\""
                        : node.GetType().Name)));
                if (chain.Count > 1)
                {
                    Walk(chain[^1], 1);
                }

                var presenter = FindByType(window, "MenuFlyoutPresenter", 0);
                if (presenter is null)
                {
                    Note($"   {stage}: no MenuFlyoutPresenter instance under the window");
                    return;
                }

                Note($"   {stage}: presenter {presenter.GetType().FullName} {presenter.ActualWidth:0.#}x{presenter.ActualHeight:0.#}"
                    + $" bg={Property(presenter, "Background")} radius={Property(presenter, "CornerRadius")}"
                    + $" padding={Property(presenter, "Padding")} border={Property(presenter, "BorderThickness")}"
                    + $" style={Property(presenter, "Style")} template={Property(presenter, "Template")?.GetType().Name ?? "null"}");
                Walk(presenter, 1);
            }

            SampleSurface("immediately");
            Pump(3, 200);
            SampleSurface("after 3");
            Pump(40, 2500);
            SampleSurface("after 40");
            Note($"-- flyout open={flyout.IsOpen} --");
            FindNamed(window, 0);
            Note("== flyout rows, read back in DIP ==");
            foreach (var (label, row) in new (string, Control)[]
                     {
                         ("Rename+F2", rename), ("Archive", archive), ("Show read receipts", receipts),
                         ("Share with", sub), ("Delete", delete),
                     })
            {
                Report(label, row);
            }
        }

        foreach (var (label, control) in new (string, Control)[]
                 {
                     ("ComboBox", combo), ("NumberBox", number), ("AutoCompleteBox", suggest),
                     ("SplitButton", split), ("DropDownButton", dropDown), ("Expander", expander),
                     ("Nav item", navItem),
                 })
        {
            Report(label, control);
        }

        Note("holding for an outside capture");
        Hold(arguments.Contains("--quick") ? 2 : 45);
        window.Close();
        Pump(4, 400);
    }

    private static void Add(Panel panel, string label, Control control)
    {
        panel.Children.Add(new TextBlock { Text = label, FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(0x9A, 0x9A, 0x9A)) });
        panel.Children.Add(control);
    }

    /// <summary>The four readings a dialog batch needs, in one line each, on whatever state it is in.</summary>
    private static void DumpDialog(ContentDialog dialog, Window shell)
    {
        var parent = VisualTreeHelper.GetParent(dialog);
        Note($"   dialog vis={dialog.Visibility} actual={dialog.ActualWidth:0.#}x{dialog.ActualHeight:0.#} " +
             $"parent={parent?.GetType().Name ?? "none"} inWindow={BelongsTo(dialog, shell, 0)}");
        if (parent is not null)
        {
            // The host the framework puts a shown dialog into paints behind it, so a smoke layer inside our own
            // template can be a second dim pass nobody asked for. That is the double-image defect class, so it
            // gets read rather than assumed.
            Note($"   host {parent.GetType().Name}: bg={Property(parent, "Background")} opacity={Property(parent, "Opacity")} " +
                 $"children={VisualTreeHelper.GetChildrenCount(parent)} radius={Property(parent, "CornerRadius")}");
        }

        Note($"   dialog box localMax={dialog.ReadLocalValue(FrameworkElement.MaxWidthProperty)} max={dialog.MaxWidth}");

        var card = ByName(dialog, "PART_DialogCard");
        if (card is null)
        {
            Note("   card: PART_DialogCard not in the built tree");
            return;
        }

        Note($"   card {card.ActualWidth:0.#}x{card.ActualHeight:0.#} min={card.MinWidth} max={card.MaxWidth} " +
             $"minH={card.MinHeight} maxH={card.MaxHeight} localMax={card.ReadLocalValue(FrameworkElement.MaxWidthProperty)} " +
             $"localMin={card.ReadLocalValue(FrameworkElement.MinWidthProperty)} radius={Property(card, "CornerRadius")} align={card.HorizontalAlignment}");

        if (ByName(dialog, "DialogSurface") is { } surface)
        {
            Note($"   surface {surface.ActualWidth:0.#}x{surface.ActualHeight:0.#} min={surface.MinWidth} max={surface.MaxWidth} " +
                 $"minH={surface.MinHeight} maxH={surface.MaxHeight} localMax={surface.ReadLocalValue(FrameworkElement.MaxWidthProperty)} " +
                 $"radius={Property(surface, "CornerRadius")} bg={Property(surface, "Background")} border={Property(surface, "BorderThickness")}");
        }
        else
        {
            Note("   surface: DialogSurface not in the built tree");
        }

        var title = ByName(dialog, "PART_TitleHost");
        var text = title is null ? null : FirstText(title, 0);
        Note($"   title host={title?.GetType().Name ?? "null"} hostSize={(title is null ? "-" : Property(title, "FontSize"))} " +
             $"hostWeight={(title is null ? "-" : Property(title, "FontWeight"))} " +
             (text is null
                 ? "text=none"
                 : $"text=\"{text.Text}\" size={text.FontSize} weight={text.FontWeight} localSize={text.ReadLocalValue(TextBlock.FontSizeProperty)} " +
                   $"localWeight={text.ReadLocalValue(TextBlock.FontWeightProperty)} style={(text.Style is null ? "null" : "set")} wrapping={text.TextWrapping}"));
    }

    private static FrameworkElement? ByName(DependencyObject node, string name)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            if (VisualTreeHelper.GetChild(node, index) is not FrameworkElement child)
            {
                continue;
            }

            if (child.Name == name)
            {
                return child;
            }

            if (ByName(child, name) is { } deeper)
            {
                return deeper;
            }
        }

        return null;
    }

    private static TextBlock? FirstText(DependencyObject node, int depth)
    {
        if (depth > 8)
        {
            return null;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if (child is TextBlock text)
            {
                return text;
            }

            if (child is not null && FirstText(child, depth + 1) is { } deeper)
            {
                return deeper;
            }
        }

        return null;
    }

    private static bool BelongsTo(DependencyObject node, Window shell, int depth)
    {
        if (depth > 20)
        {
            return false;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(shell); index++)
        {
            var child = VisualTreeHelper.GetChild(shell, index);
            if (ReferenceEquals(child, node) || (child is not null && BelongsTo(child, shell, depth + 1)))
            {
                return true;
            }
        }

        return ReferenceEquals(node, shell);
    }

    private static string TaskResult(Task task) =>
        task.IsCompletedSuccessfully ? task.GetType().GetProperty("Result")?.GetValue(task)?.ToString() ?? "-" : "not completed";

    /// <summary>How deep {@link WalkSizes} descends. Popup surfaces repeat their scrollbar subtree five times
    /// over, so a run that only needs the surface gets a shallower walk.</summary>
    private static int WalkDepth = 6;

    private static readonly string[] Interesting =
    [
        "PART_Popup", "PART_PopupBorder", "SuggestionsContainer", "PART_DropDownBorder",
        "PART_DropDownScrollViewer", "PART_DropDownItemsHost", "PopupContentRoot", "UpDownPopup",
    ];

    /// <summary>
    /// Popup children are re-parented into the host window's overlay layer, so the only way to read what the
    /// framework did to their width is to walk down from the window rather than from the control.
    /// </summary>
    private static void FindNamed(DependencyObject node, int depth)
    {
        if (depth > 14)
        {
            return;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if (child is FrameworkElement { Name: { Length: > 0 } } element && Interesting.Contains(element.Name))
            {
                var width = DependencyProperty.FromName(child.GetType(), "Width");
                var minWidth = DependencyProperty.FromName(child.GetType(), "MinWidth");
                var min = minWidth is null ? "?" : element.GetValue(minWidth);
                var localMin = minWidth is null ? "?" : element.ReadLocalValue(minWidth);
                var wide = width is null ? "?" : element.GetValue(width);
                var localWide = width is null ? "?" : element.ReadLocalValue(width);
                Note($"popup part {child.GetType().Name} \"{element.Name}\" {element.ActualWidth:0.#}x{element.ActualHeight:0.#}"
                     + $" min={min} localMin={localMin} width={wide} localWidth={localWide}");
                Console.Out.WriteLine(element.Name);
                Console.Out.Flush();
            }

            FindNamed(child, depth + 1);
        }
    }

    private static FrameworkElement? FindByType(DependencyObject node, string typeName, int depth)
    {
        if (depth > 14)
        {
            return null;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if (child is FrameworkElement { ActualWidth: > 0 } element
                && element.GetType().Name.Contains(typeName, StringComparison.Ordinal))
            {
                return element;
            }

            var nested = FindByType(child, typeName, depth + 1);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private static void Report(string label, Control control)
    {
        Note($"== {label} {control.GetType().Name} actual={control.ActualWidth:0.#}x{control.ActualHeight:0.#} "
             + $"padding={Property(control, "Padding")} margin={control.Margin}");
        Walk(control, 1);
    }

    /// <summary>
    /// The suggestion list read the way its ceiling is set: the surface, its scroll host, and the rows the
    /// framework realized under it. Row count against surface height is what separates "the list is short
    /// because the control caps it" from "the list is short because it has few rows".
    /// </summary>
    private static void DumpSuggest(DependencyObject suggest)
    {
        var popup = FindDescendant(suggest, "PART_Popup");
        if (popup is null)
        {
            Note("   no PART_Popup under the control");
            return;
        }

        // Once the popup opens, its surface is re-parented above the control, so a walk from the control finds
        // nothing. The rows live under the Popup's own child, and the size that actually reaches the screen
        // lives at the top of that chain - which is the only place a "the layout box says 374" reading can be
        // told apart from a "10 rows are visible" one.
        var surface = ValueOf(popup, "Child") as DependencyObject;
        if (surface is null)
        {
            Note("   the Popup has no child");
            return;
        }

        var rows = new List<FrameworkElement>();
        CollectRows(surface, rows, 0);
        Note($"   rows: realized={rows.Count} summed={rows.Sum(static row => row.ActualHeight):0.#}"
             + $" first=" + (rows.Count == 0 ? "none"
                 : $"{rows[0].GetType().Name} {rows[0].ActualWidth:0.#}x{rows[0].ActualHeight:0.#}" +
                   $" padding={Property(rows[0], "Padding")} radius={Property(rows[0], "CornerRadius")}" +
                   $" margin={rows[0].Margin} font={Property(rows[0], "FontSize")} style={rows[0].Style?.GetType().Name ?? "null"}"));
        for (DependencyObject? node = surface; node is not null; node = VisualTreeHelper.GetParent(node))
        {
            if (node is not FrameworkElement element)
            {
                Note($"     up: {node.GetType().Name} (not a FrameworkElement)");
                continue;
            }

            Note($"     up: {node.GetType().Name} \"{element.Name}\" {SizeOf(element)}"
                 + $" minH={Local(element, "MinHeight")} maxH={Local(element, "MaxHeight")} h={Local(element, "Height")}"
                 + $" vis={element.Visibility} top={Property(element, "Top")} left={Property(element, "Left")}"
                 + $" bg={Property(element, "Background")}");
        }

        DumpPopup(suggest, "suggest");
    }

    /// <summary>Item containers the framework realizes under the drop-down host, wherever it parked them.</summary>
    private static void CollectRows(DependencyObject node, List<FrameworkElement> rows, int depth)
    {
        if (depth > 9)
        {
            return;
        }

        if (node.GetType().Name.EndsWith("Item", StringComparison.Ordinal) && node is FrameworkElement element && element.ActualHeight > 0)
        {
            rows.Add(element);
            return;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            CollectRows(VisualTreeHelper.GetChild(node, index), rows, depth + 1);
        }
    }

    private static void DumpPopup(DependencyObject owner, string label)
    {
        Note($"== popup surface dump: {label} ==");
        var popup = FindDescendant(owner, "PART_Popup") ?? FindDescendantByTypeName(owner, "Popup");
        if (popup is null)
        {
            Note("   no Popup anywhere under the control's own tree");
            return;
        }

        Note($"   {popup.GetType().Name} \"{(popup as FrameworkElement)?.Name}\" open={Property(popup, "IsOpen")}"
             + $" actual={SizeOf(popup)} width={Local(popup, "Width")} minWidth={Local(popup, "MinWidth")}"
             + $" maxWidth={Local(popup, "MaxWidth")} height={Local(popup, "Height")}"
             + $" placement={Property(popup, "Placement")} child={ValueOf(popup, "Child")?.GetType().Name ?? "null"}");

        if (ValueOf(popup, "Child") is not DependencyObject child)
        {
            Note("   the Popup has no child, so nothing inside it can carry a size");
            return;
        }

        WalkSizes(child, 1);
    }

    /// <summary>
    /// Painted elements with where they actually landed. A mark that has no part in the template can only be
    /// found by its offset, so this prints one line per element with a size and its position relative to the
    /// anchor, read through the same TransformToVisual the renderer uses.
    /// </summary>
    private static void WalkOffsets(DependencyObject node, DependencyObject anchor, int depth)
    {
        if (depth > 9)
        {
            return;
        }

        if (node is FrameworkElement element && element.ActualHeight > 0)
        {
            Note($"  {new string(' ', depth * 2)}{element.GetType().Name} \"{element.Name}\" {SizeOf(element)} at {OffsetOf(element, anchor)}"
                 + $" localPad={Local(element, "Padding")} localMinH={Local(element, "MinHeight")}"
                 + $" localMargin={Local(element, "Margin")} text={ValueOf(element, "Text") ?? ValueOf(element, "Content") ?? "-"}"
                 + $" font={Property(element, "FontFamily")}");
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            WalkOffsets(VisualTreeHelper.GetChild(node, index), anchor, depth + 1);
        }
    }

    private static string OffsetOf(DependencyObject element, DependencyObject anchor)
    {
        try
        {
            var method = element.GetType().GetMethod("TransformToVisual", [typeof(DependencyObject)]);
            var transform = method?.Invoke(element, [anchor]);
            var matrix = transform?.GetType().GetProperty("Value")?.GetValue(transform);
            var x = matrix?.GetType().GetProperty("OffsetX")?.GetValue(matrix);
            var y = matrix?.GetType().GetProperty("OffsetY")?.GetValue(matrix);
            return x is null || y is null ? "unreadable" : $"{Convert.ToDouble(x):0.#},{Convert.ToDouble(y):0.#}";
        }
        catch (Exception exception)
        {
            return "threw " + exception.GetType().Name;
        }
    }

    private static void WalkSizes(DependencyObject node, int depth)
    {
        if (depth > WalkDepth)
        {
            return;
        }

        var indent = new string(' ', depth * 2);
        if (node is FrameworkElement element)
        {
            Note($"  {indent}{element.GetType().Name} \"{element.Name}\" {SizeOf(element)}"
                 + $" w={Local(element, "Width")} minW={Local(element, "MinWidth")} maxW={Local(element, "MaxWidth")}"
                 + $" h={Local(element, "Height")} maxH={Local(element, "MaxHeight")}"
                 + $" margin={element.Margin} padding={Property(element, "Padding")}"
                 + $" radius={Property(element, "CornerRadius")} align={element.HorizontalAlignment}");
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            WalkSizes(VisualTreeHelper.GetChild(node, index), depth + 1);
        }
    }

    private static string SizeOf(DependencyObject node) => node is FrameworkElement element
        ? $"{element.ActualWidth:0.#}x{element.ActualHeight:0.#}"
        : "n/a";

    private static object? ValueOf(DependencyObject node, string propertyName) =>
        node.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(node);

    private static string Local(DependencyObject node, string propertyName)
    {
        for (var type = node.GetType(); type is not null; type = type.BaseType)
        {
            var field = type.GetField(propertyName + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (field?.GetValue(null) is DependencyProperty property)
            {
                var value = node.ReadLocalValue(property);
                return value == DependencyProperty.UnsetValue ? "-" : value switch
                {
                    double number => number.ToString("0.#"),
                    GridLength length => length.ToString(),
                    null => "null",
                    _ => value.ToString() ?? "null",
                };
            }
        }

        return "?no such dp";
    }

    private static DependencyObject? FindDescendant(DependencyObject node, string name)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if ((child as FrameworkElement)?.Name == name)
            {
                return child;
            }

            if (FindDescendant(child, name) is { } match)
            {
                return match;
            }
        }

        return null;
    }

    private static DependencyObject? FindDescendantByTypeName(DependencyObject node, string typeName)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if (child.GetType().Name == typeName)
            {
                return child;
            }

            if (FindDescendantByTypeName(child, typeName) is { } match)
            {
                return match;
            }
        }

        return null;
    }

    private static void Walk(DependencyObject node, int depth)
    {
        if (depth > 5)
        {
            return;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if (child is FrameworkElement element)
            {
                var name = element.Name;
                var type = child.GetType().Name;
                if (name.Length > 0 || type.Contains("Presenter", StringComparison.Ordinal) || type.Contains("Path", StringComparison.Ordinal)
                    || type.Contains("Toggle", StringComparison.Ordinal) || type.Contains("Thumb", StringComparison.Ordinal))
                {
                    var indent = new string(' ', depth * 2);
                    Note($"  {indent}{type} \"{name}\" {element.ActualWidth:0.#}x{element.ActualHeight:0.#}"
                         + $" margin={element.Margin} padding={Property(element, "Padding")}"
                         + $" radius={Property(element, "CornerRadius")} align={element.HorizontalAlignment}");
                }
            }

            Walk(child, depth + 1);
        }
    }

    private static string Property(object target, string name)
    {
        var value = target.GetType().GetProperty(name)?.GetValue(target);
        return value?.ToString() ?? "-";
    }

    /// <summary>
    /// One pixel of a re-rasterized subtree, in #RRGGBB. Legitimate for a claim about the drawing pipeline
    /// (does a rounded surface clip its child) because the rasterizer rebuilds from current property values;
    /// it says nothing about a frame that already reached the compositor.
    /// </summary>
    private static string PixelAt(Visual target, int x, int y)
    {
        var host = (FrameworkElement)target;
        var width = (int)Math.Ceiling(host.ActualWidth);
        var height = (int)Math.Ceiling(host.ActualHeight);
        if (width <= 0 || height <= 0) return "unlaid";
        if (x >= width || y >= height) return "outside";

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(target);
        var buffer = new byte[width * height * 4];
        bitmap.CopyPixels(buffer, width * 4, 0);
        var offset = (y * width + x) * 4;
        return $"#{buffer[offset + 2]:X2}{buffer[offset + 1]:X2}{buffer[offset]:X2}";
    }

    private static void Hold(int seconds)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var deadline = DateTime.UtcNow.AddSeconds(seconds);
        void OnRendering(object? sender, EventArgs arguments)
        {
            if (DateTime.UtcNow > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var timer = new System.Threading.Timer(
            _ => dispatcher.InvokeAsync(() => frame.Continue = false),
            null,
            TimeSpan.FromSeconds(seconds + 1),
            Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }

    private static int Pump(int frames, int budgetMilliseconds)
    {
        var count = 0;
        var frame = new DispatcherFrame();
        var start = DateTime.UtcNow;
        void OnRendering(object? sender, EventArgs arguments)
        {
            count++;
            if (count >= frames || (DateTime.UtcNow - start).TotalMilliseconds > budgetMilliseconds) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return count;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);

    private static void Note(string line) => Lines.Add(line);
}
