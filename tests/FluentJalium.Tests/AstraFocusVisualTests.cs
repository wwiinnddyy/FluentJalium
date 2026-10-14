using System.IO;
using System.Xml.Linq;
using FluentJalium.Controls;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The keyboard focus ring, and the one input path that is allowed to draw it.
///
/// Every ring this library shipped was a double Border inside the control template, resting at Opacity 0 and
/// raised by <c>Trigger Property="IsKeyboardFocused"</c>. A pointer click takes keyboard focus on 26.10.9 too,
/// so that condition held on both input paths and clicking a control drew the ring WinUI reserves for keyboard
/// navigation - the defect as reported. WinUI gates on FocusState and paints only for Keyboard
/// (microsoft-ui-xaml @19e3bdc3 UIElement.cpp:6437-6490, focusmgr.cpp:2704-2752). This runtime's equivalent is
/// public: FocusVisualManager.ShowFocusCues, which a mouse press clears and Tab / the arrows / Home / End /
/// PageUp / PageDown set, materialising FrameworkElement.FocusVisualStyle through FocusVisualAdorner in the
/// window's adorner layer. So each ring moved out of its template and into that property, in
/// Styles/FocusVisuals.jalxaml.
///
/// What these tests hold, and what they do not: the setter arriving on a mounted control, the geometry of the
/// ring that arrives, the ink it prints, and the framework gate reading off on the pointer path with no adorner
/// raised while keyboard focus is nevertheless held. Turning the gate on needs a key event, and this runtime
/// publishes no constructor for one that a test can call (spike/FocusCueProbe reaches the framework's internal
/// one), so the keyboard-on half is probe-measured, not asserted here - see docs/astra/audits/focus-visual.md.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraFocusVisualTests
{
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraFocusVisualTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    /// <summary>
    /// The twelve sites that used to own a ring Border inside their template now hand it over as
    /// FocusVisualStyle, and the value that arrives on a mounted control is the same Style object the
    /// dictionary publishes - so the ring the framework draws is ours, token-for-token, and the
    /// {ThemeResource} lookup really resolved. The two Slider rows are the interesting pair: the vertical one
    /// arrives through a Style trigger rather than a setter.
    /// </summary>
    [Theory]
    [InlineData("DefaultButtonStyle", "FocusVisualRingStyle")]
    [InlineData("SplitButtonSecondaryButtonStyle", "FocusVisualRingStyle")]
    [InlineData("SplitButtonStyle", "FocusVisualRingStyle")]
    [InlineData("DefaultDropDownButtonStyle", "FocusVisualRingStyle")]
    [InlineData("FluentToggleSwitchStyle", "FocusVisualRingStyle")]
    [InlineData("ExpanderStyle", "FocusVisualRingStyle")]
    [InlineData("FluentNavigationItemStyle", "FocusVisualRingStyle")]
    [InlineData("FluentNavigationPaneToggleButtonStyle", "FocusVisualRingStyle")]
    [InlineData("DefaultCheckBoxStyle", "FocusVisualCheckStyle")]
    [InlineData("DefaultRadioButtonStyle", "FocusVisualCheckStyle")]
    [InlineData("DefaultSliderStyle", "FocusVisualSliderStyle")]
    [InlineData("DefaultSliderStyle/vertical", "FocusVisualSliderVerticalStyle")]
    public void A_family_that_own_a_ring_hands_it_to_the_focus_visual(string styleKey, string ringKey)
    {
        _fixture.Run(() =>
        {
            var control = Mount(styleKey);
            Assert.NotNull(control.FocusVisualStyle);
            Assert.Same(FluentThemeManager.GetStyle(ringKey), control.FocusVisualStyle);
        });
    }

    /// <summary>
    /// The invariant that keeps the reported defect from coming back: no style in the library draws anything
    /// from IsKeyboardFocused any more. The scan keys on the word the rings were named with - ComboBox and
    /// TextBox keep IsKeyboardFocused cells on purpose, because upstream writes those too (its FocusStates row
    /// fires for pointer focus as well) and they move a surface or a border brush, not a focus rectangle.
    /// </summary>
    [Fact]
    public void No_style_in_the_library_draws_a_ring_from_keyboard_focus()
    {
        var root = RepositoryRoot();
        var offenders = new List<string>();
        var files = 0;
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src", "FluentJalium", "Styles"), "*.jalxaml", SearchOption.AllDirectories))
        {
            files++;
            var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            var document = XDocument.Load(file);
            foreach (var element in document.Descendants())
            {
                var name = element.Attributes().FirstOrDefault(static attribute => attribute.Name.LocalName == "Name")?.Value;
                if (element.Name.LocalName == "Border" && name?.Contains("Focus", StringComparison.Ordinal) == true)
                {
                    offenders.Add($"{relative}: a template part named {name}");
                }

                if (element.Name.LocalName != "Trigger"
                    || element.Attributes().FirstOrDefault(static attribute => attribute.Name.LocalName == "Property")?.Value != "IsKeyboardFocused")
                {
                    continue;
                }

                foreach (var setter in element.Descendants().Where(static child => child.Name.LocalName == "Setter"))
                {
                    var property = setter.Attributes().FirstOrDefault(static attribute => attribute.Name.LocalName == "Property")?.Value;
                    var targetName = setter.Attributes().FirstOrDefault(static attribute => attribute.Name.LocalName == "TargetName")?.Value;
                    if (property == "Opacity" && targetName?.Contains("Focus", StringComparison.Ordinal) == true)
                    {
                        offenders.Add($"{relative}: IsKeyboardFocused raises {targetName}.Opacity");
                    }
                }
            }
        }

        Assert.True(files >= 20, $"only {files} style dictionaries were scanned; the gate has gone vacuous.");
        offenders.Sort(StringComparer.Ordinal);
        Assert.False(offenders.Count > 0, "A keyboard-focus cell can draw a ring again:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    /// <summary>
    /// The hand-over has to carry the same pixels the templates used to: 2px outer and 1px inner stroke, the
    /// 4/2 radii, the palette instances as the strokes, and each family's own offset. Those offsets moved from
    /// the ring Border in the control template into this template, because the adorner is arranged at the
    /// adorned element's bounds and this runtime publishes no FocusVisualMargin.
    /// </summary>
    [Theory]
    [InlineData("FocusVisualRingStyle", 1, 1, 1, 1)]
    [InlineData("FocusVisualCheckStyle", -2, 1, -2, 1)]
    [InlineData("FocusVisualSliderStyle", 0, 2, 0, 2)]
    [InlineData("FocusVisualSliderVerticalStyle", 2, 0, 2, 0)]
    public void The_ring_a_family_hands_over_keeps_the_geometry_its_template_used(string ringKey, double left, double top, double right, double bottom)
    {
        _fixture.Run(() =>
        {
            var ring = new Control { Style = FluentThemeManager.GetStyle(ringKey) };
            PixelHarness.Build(ring, 120, 32);
            var outer = FirstBorder(ring) ?? throw new InvalidOperationException($"{ringKey} built no Border.");
            var inner = InnerBorder(outer);

            Assert.Equal(new Thickness(2), outer.BorderThickness);
            Assert.Equal(new CornerRadius(4), outer.CornerRadius);
            Assert.Equal(new Thickness(left, top, right, bottom), outer.Margin);
            Assert.Same(FluentThemeManager.GetBrush("FocusStrokeColorOuterBrush"), outer.BorderBrush);
            Assert.NotNull(inner);
            Assert.Equal(new Thickness(1), inner!.BorderThickness);
            Assert.Equal(new CornerRadius(2), inner.CornerRadius);
            Assert.Same(FluentThemeManager.GetBrush("FocusStrokeColorInnerBrush"), inner.BorderBrush);
        });
    }

    /// <summary>
    /// The defect, read on the path that caused it: the pointer press leaves the button holding keyboard focus
    /// - the very condition the old cells keyed on - while the framework's own gate stays off and its adorner is
    /// never raised. Both halves are needed: keyboard focus alone does not prove the ring is gone, and an empty
    /// tree alone does not prove the condition was satisfied.
    /// </summary>
    [Fact]
    public void A_pointer_press_keeps_keyboard_focus_without_raising_the_ring()
    {
        _fixture.Run(() =>
        {
            var button = new Button { Content = "ring" };
            button.Style = FluentThemeManager.GetStyle("DefaultButtonStyle");
            PixelHarness.Build(button, 120, 32);
            Assert.True(button.Focus(), "Focus() refused the button in the host window.");
            PixelHarness.Settle(20);
            Assert.True(button.IsKeyboardFocused);
            Assert.Equal(0, Adorners());

            button.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = UIElement.PreviewMouseDownEvent,
            });
            PixelHarness.Settle(20);

            Assert.True(button.IsKeyboardFocused, "The pointer press was expected to keep keyboard focus, as it did before the fix.");
            Assert.False(FocusVisualManager.ShowFocusCues, "A pointer press left the framework's focus-cue gate on.");
            Assert.Equal(0, Adorners());
        });
    }

    /// <summary>
    /// Two ink readings, so the negative one cannot pass by being blind. The ring style really paints both
    /// strokes over an opaque plate, and a button resting at the same size paints neither ring colour where a
    /// ring would sit. The negative leg keys on the outer stroke only: the inner one is #B3FFFFFF, which over
    /// the light plate lands on #FCFCFC - measured inside a resting button's own fill, where it coloured 1486
    /// of the 3840 pixels. Only the dark outer stroke is attributable to a ring.
    /// </summary>
    [Fact]
    public void The_ring_paints_and_a_resting_button_does_not()
    {
        _fixture.Run(() =>
        {
            var outerInk = PixelHarness.Over(PixelHarness.LightPage, ((SolidColorBrush)FluentThemeManager.GetBrush("FocusStrokeColorOuterBrush")!).Color);
            var innerInk = PixelHarness.Over(PixelHarness.LightPage, ((SolidColorBrush)FluentThemeManager.GetBrush("FocusStrokeColorInnerBrush")!).Color);

            var ring = new Control { Style = FluentThemeManager.GetStyle("FocusVisualRingStyle") };
            var painted = PixelHarness.Render(PixelHarness.Backdrop(ring, PixelHarness.LightPage), 120, 32);

            var button = new Button { Style = FluentThemeManager.GetStyle("DefaultButtonStyle") };
            var resting = PixelHarness.Render(PixelHarness.Backdrop(button, PixelHarness.LightPage), 120, 32);

            Assert.Multiple(
                () => Assert.True(painted.Count(outerInk) > 0, $"the outer stroke printed nothing: {painted.Top(6)}"),
                () => Assert.True(painted.Count(innerInk) > 0, $"the inner stroke printed nothing: {painted.Top(6)}"),
                () => Assert.Equal(0, resting.Count(outerInk)));
        });
    }

    private Control Mount(string styleKey)
    {
        var vertical = styleKey.EndsWith("/vertical", StringComparison.Ordinal);
        var key = vertical ? styleKey[..^"/vertical".Length] : styleKey;
        Control control = key switch
        {
            "DefaultButtonStyle" => new Button { Content = "b" },
            "SplitButtonSecondaryButtonStyle" => new Button { Content = "s" },
            "SplitButtonStyle" => new SplitButton { Content = "s" },
            "DefaultDropDownButtonStyle" => new FluentDropDownButton { Content = "d" },
            "FluentToggleSwitchStyle" => new FluentToggleSwitch { Content = "t" },
            "ExpanderStyle" => new Expander { Header = "h" },
            "FluentNavigationItemStyle" => new FluentNavigationItem { Content = "n" },
            "FluentNavigationPaneToggleButtonStyle" => new Button { Content = "p" },
            "DefaultCheckBoxStyle" => new CheckBox { Content = "c" },
            "DefaultRadioButtonStyle" => new RadioButton { Content = "r" },
            "DefaultSliderStyle" => new Slider { Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal },
            _ => throw new ArgumentOutOfRangeException(nameof(styleKey), styleKey, "unknown style key"),
        };

        control.Style = FluentThemeManager.GetStyle(key);
        PixelHarness.Build(control, 200, 44);
        return control;
    }

    private static int Adorners()
    {
        var count = 0;
        void Walk(DependencyObject node)
        {
            if (node.GetType().FullName?.Contains("FocusVisual", StringComparison.Ordinal) == true)
            {
                count++;
            }

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
            {
                var child = VisualTreeHelper.GetChild(node, index);
                if (child is not null)
                {
                    Walk(child);
                }
            }
        }

        Walk(PixelHarness.HostWindow());
        return count;
    }

    private static Border? FirstBorder(DependencyObject root)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is null)
            {
                continue;
            }

            if (child is Border border)
            {
                return border;
            }

            if (FirstBorder(child) is { } deeper)
            {
                return deeper;
            }
        }

        return null;
    }

    private static Border? InnerBorder(Border outer)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(outer); index++)
        {
            if (VisualTreeHelper.GetChild(outer, index) is Border border)
            {
                return border;
            }
        }

        return FirstBorder(outer);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FluentJalium.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("No repository root above " + AppContext.BaseDirectory);
    }
}
