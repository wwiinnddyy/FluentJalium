using System.IO;
using System.Xml.Linq;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The Button family, first leg: upstream's own alias layer transcribed, our styles rewired to
/// consume it by upstream's names, and the state mapping checked without needing a mouse
/// (docs/astra/audits/button.md). Hover and press pixels are the input batch's job; what is provable
/// here is that each upstream state has a trigger and that the trigger carries the brush upstream
/// uses for that state.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraButtonTests
{
    private static readonly Color Sentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraButtonTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void The_upstream_button_aliases_resolve_to_the_palette_instances()
    {
        // The point of the alias layer: an app or Gallery page that overrides ControlFillColorDefault
        // moves a button, because ButtonBackground is that same object rather than a copy of it.
        _fixture.Run(() =>
        {
            AssertBrush(FluentThemeManager.GetBrush("ControlFillColorDefaultBrush"), "ButtonBackground");
            AssertBrush(FluentThemeManager.GetBrush("AccentFillColorDefaultBrush"), "AccentButtonBackground");
            AssertBrush(FluentThemeManager.GetBrush("SubtleFillColorSecondaryBrush"), "SubtleButtonBackgroundPointerOver");
            AssertBrush(FluentThemeManager.GetBrush("TextOnAccentFillColorDisabledBrush"), "AccentButtonForegroundDisabled");
            AssertBrush(FluentThemeManager.GetBrush("ControlStrokeColorDefaultBrush"), "ButtonBorderBrush");
            Assert.Equal(new Thickness(1), (Thickness)_fixture.Application.TryFindResource("ButtonBorderThemeThickness")!);
        });
    }

    [Fact]
    public void Every_upstream_button_state_has_a_trigger_carrying_the_upstream_brush()
    {
        // WinUI drives these through VisualStateManager storyboards, which this runtime cannot host
        // (adaptation/00), so the mapping is ControlTemplate/Style triggers. Asserting the setter
        // value is the same object upstream names for that state is what keeps the substitution honest.
        _fixture.Run(() =>
        {
            var style = FluentThemeManager.GetStyle("DefaultButtonStyle");

            AssertTriggerSetter(Setter(style, "IsMouseOver", "Background"), "ButtonBackgroundPointerOver");
            AssertTriggerSetter(Setter(style, "IsMouseOver", "BorderBrush"), "ButtonBorderBrushPointerOver");
            AssertTriggerSetter(Setter(style, "IsPressed", "Background"), "ButtonBackgroundPressed");
            AssertTriggerSetter(Setter(style, "IsPressed", "Foreground"), "ButtonForegroundPressed");
            AssertTriggerSetter(Setter(style, "IsEnabled", "Background"), "ButtonBackgroundDisabled");
            AssertTriggerSetter(Setter(style, "IsEnabled", "Foreground"), "ButtonForegroundDisabled");
        });
    }

    [Fact]
    public void The_button_layout_defaults_are_the_upstream_ones()
    {
        // Read back off a real control: BorderThickness arriving as 1 is the evidence that a
        // non-brush alias (ButtonBorderThemeThickness) resolves through the same layer.
        // HorizontalAlignment=Left is upstream's own setter and it is layout-visible.
        _fixture.Run(() =>
        {
            var button = new Button { Content = "button" };
            PixelHarness.Render(button, 120, 32);

            Assert.Equal(32d, button.MinHeight);
            Assert.Equal(new Thickness(11, 5, 11, 6), button.Padding);
            Assert.Equal(new Thickness(1), button.BorderThickness);
            Assert.Equal(4d, button.CornerRadius.TopLeft);
            Assert.Equal(14d, button.FontSize);
            Assert.Equal(FontWeights.Normal, button.FontWeight);
            Assert.Equal(HorizontalAlignment.Left, button.HorizontalAlignment);
            Assert.Equal(VerticalAlignment.Center, button.VerticalAlignment);
        });
    }

    [Fact]
    public void A_disabled_button_paints_the_upstream_disabled_fill()
    {
        // The one state a test can reach without a pointer: IsEnabled is settable, so the disabled
        // branch of the mapping is a pixel claim, not just a dictionary claim.
        _fixture.Run(() =>
        {
            // The palette key, not the alias: this asserts both halves at once - the disabled trigger
            // really fires, and ButtonBackgroundDisabled is the same object rather than a copy.
            FluentThemeManager.OverrideBrush("ControlFillColorDisabledBrush", Sentinel);
            try
            {
                var enabled = PixelHarness.Render(new Button { Content = "button" }, 120, 32);
                var disabled = PixelHarness.Render(new Button { Content = "button", IsEnabled = false }, 120, 32);

                Assert.Equal(0, enabled.Count(Sentinel));
                Assert.True(disabled.Count(Sentinel) > 1_000,
                            $"disabled fill did not reach the pixels; top={disabled.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ControlFillColorDisabledBrush", null);
            }
        });
    }

    [Fact]
    public void A_swapped_brush_object_reaches_the_transitioning_surface()
    {
        // Every state this library has works by handing the surface a different brush object, and the
        // template surface animates Background. A capture taken while that animation runs shows a
        // blend, which is why the harness re-grabs until two pictures agree - measured with a fixed
        // frame count the same swap read as #E482E4 (docs/astra/audits/button-input-raw.txt). This is
        // the mouse-free half of the hover path: a real pointer move reaches the same brush swap.
        _fixture.Run(() =>
        {
            var button = new Button { Content = "swap", Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0xFF)) };
            var first = PixelHarness.Render(button, 200, 44);
            Assert.True(first.Count(Color.FromRgb(0xFF, 0x00, 0xFF)) > 4_000, $"rest object not painted; top={first.Top(6)}");

            button.Background = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x00));
            var second = PixelHarness.Render(button, 200, 44);
            Assert.True(second.Count(Color.FromRgb(0x00, 0xFF, 0x00)) > 4_000,
                        $"a new brush object did not reach the transitioning surface; top={second.Top(6)}");
        });
    }

    [Fact]
    public void Every_button_family_alias_resolves_to_its_target_instance()
    {
        // One gate for the whole transcription rather than a spot check per row: every alias row in the
        // four button-family dictionaries has to hand back the exact palette object its ResourceKey
        // names. A copy would satisfy a colour comparison and still stop following the theme, so this
        // asserts identity for all 96 rows including the ones no style consumes yet.
        _fixture.Run(() =>
        {
            var checkedRows = 0;
            foreach (var name in new[]
                     {
                         "ThemeResources/Button.jalxaml", "ThemeResources/ToggleButton.jalxaml",
                         "ThemeResources/RepeatButton.jalxaml", "ThemeResources/HyperlinkButton.jalxaml",
                     })
            {
                using var stream = typeof(FluentThemeManager).Assembly.GetManifestResourceStream(name)
                    ?? throw new InvalidOperationException($"Missing Astra dictionary: {name}");
                var root = XDocument.Load(stream);
                foreach (var row in root.Descendants().Where(static element => element.Name.LocalName == "StaticResource"))
                {
                    var alias = row.Attribute(XamlNamespace + "Key")?.Value;
                    var target = row.Attribute("ResourceKey")?.Value;
                    Assert.False(string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(target), $"{name}: incomplete alias row");

                    var palette = FluentThemeManager.GetBrush(target!);
                    var resolved = _fixture.Application.TryFindResource(alias!);
                    Assert.True(ReferenceEquals(palette, resolved),
                                $"{name}: {alias} is not the {target} instance the kernel re-tints (got {resolved?.GetType().Name ?? "null"})");
                    checkedRows++;
                }
            }

            Assert.True(checkedRows >= 70, $"only {checkedRows} alias rows found; a dictionary went missing");
        });
    }

    [Fact]
    public void The_toggle_repeat_and_hyperlink_styles_carry_their_upstream_state_keys()
    {
        // WinUI's CheckedUncheckedStates/IndeterminateStates become triggers here. IsChecked is settable
        // from code, so unlike PointerOver and Pressed this state needs no pointer to reach the pixels
        // (the next test does exactly that) - what this one pins is that each state carries the row
        // upstream names it by.
        _fixture.Run(() =>
        {
            var toggle = FluentThemeManager.GetStyle("DefaultToggleButtonStyle");
            AssertTriggerSetter(Setter(toggle, "IsMouseOver", "Background"), "ToggleButtonBackgroundPointerOver");
            AssertTriggerSetter(Setter(toggle, "IsPressed", "Background"), "ToggleButtonBackgroundPressed");
            AssertTriggerSetter(Setter(toggle, "IsEnabled", "Background"), "ToggleButtonBackgroundDisabled");
            AssertTriggerSetter(Checked(toggle, "IsMouseOver", "Background"), "ToggleButtonBackgroundCheckedPointerOver");
            AssertTriggerSetter(Checked(toggle, "IsPressed", "Background"), "ToggleButtonBackgroundCheckedPressed");
            AssertTriggerSetter(Checked(toggle, "IsEnabled", "Foreground"), "ToggleButtonForegroundCheckedDisabled");
            AssertTriggerSetter(Checked(toggle, "IsEnabled", "BorderBrush"), "ToggleButtonBorderBrushCheckedDisabled");

            var repeat = FluentThemeManager.GetStyle("DefaultRepeatButtonStyle");
            AssertTriggerSetter(Setter(repeat, "IsMouseOver", "Background"), "RepeatButtonBackgroundPointerOver");
            AssertTriggerSetter(Setter(repeat, "IsPressed", "Foreground"), "RepeatButtonForegroundPressed");
            AssertTriggerSetter(Setter(repeat, "IsEnabled", "BorderBrush"), "RepeatButtonBorderBrushDisabled");

            var hyperlink = FluentThemeManager.GetStyle("DefaultHyperlinkButtonStyle");
            AssertTriggerSetter(Setter(hyperlink, "IsMouseOver", "Foreground"), "HyperlinkButtonForegroundPointerOver");
            AssertTriggerSetter(Setter(hyperlink, "IsPressed", "Foreground"), "HyperlinkButtonForegroundPressed");
            AssertTriggerSetter(Setter(hyperlink, "IsEnabled", "Foreground"), "HyperlinkButtonForegroundDisabled");
        });
    }

    [Fact]
    public void A_checked_toggle_and_a_resting_repeat_button_paint_their_upstream_fills()
    {
        // The states the framework can enter without a pointer, asserted where they land. The sentinel
        // goes in by re-tinting the palette object, so a pass also proves the alias rows added for
        // these two controls are not copies.
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", Sentinel);
            try
            {
                var on = PixelHarness.Render(new ToggleButton { Content = "on", Width = 200, Height = 44, IsChecked = true }, 200, 44);
                Assert.True(on.Stable, $"capture never settled: {on.Top(6)}");
                Assert.True(on.Count(Sentinel) > 6_000, $"checked toggle did not paint the accent fill; top={on.Top(6)}");

                var off = PixelHarness.Render(new ToggleButton { Content = "off", Width = 200, Height = 44, IsChecked = false }, 200, 44);
                Assert.Equal(0, off.Count(Sentinel));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }

            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", Sentinel);
            try
            {
                var repeat = PixelHarness.Render(new RepeatButton { Content = "repeat", Width = 200, Height = 44 }, 200, 44);
                Assert.True(repeat.Count(Sentinel) > 6_000, $"repeat button did not paint its own rest fill; top={repeat.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void The_implicit_hyperlink_style_reaches_the_text_element_it_builds()
    {
        // A hyperlink has no opaque surface of its own (its rest fill is transparent), and text is not
        // in the pixel path at all: a text-only capture costs tens of seconds here and writes no
        // pixels (docs/astra/adaptation/06). So the claim is read off the element the template
        // generated inside a shown host, and it is read against the alias resolved now rather than a
        // brush cached before the build.
        _fixture.Run(() =>
        {
            var link = new HyperlinkButton { Content = "link" };
            PixelHarness.Build(link, 200, 44);
            var text = PixelHarness.Descendant<TextBlock>(link);

            Assert.True(text is not null, "no text element was built under the hyperlink template");
            Assert.Same(link.Foreground, text!.Foreground);
            Assert.Same(_fixture.Application.TryFindResource("HyperlinkButtonForeground"), text.Foreground);
            Assert.Equal("link", text.Text);
        });
    }

    [Fact]
    public void The_button_family_is_split_across_two_namespaces()
    {
        // Upstream has all four in Microsoft.UI.Xaml.Controls. Here the base of the family is not, so
        // an app that copies WinUI's using list will not compile against Astra - the 1.0 CLR API
        // inventory has to record the runtime shape, not upstream's.
        _fixture.Run(() =>
        {
            Assert.Equal("Jalium.UI.Controls", typeof(Button).Namespace);
            Assert.Equal("Jalium.UI.Controls.Primitives", typeof(ToggleButton).Namespace);
            Assert.Equal("Jalium.UI.Controls.Primitives", typeof(RepeatButton).Namespace);
            Assert.Equal("Jalium.UI.Controls", typeof(HyperlinkButton).Namespace);
        });
    }

    /// <summary>
    /// The setter a checked-state multi trigger applies, found by its second condition. The parser keeps
    /// a trigger value as the string the markup carried instead of coercing it to the property type, so
    /// matching has to be on the text form; the checked-toggle pixel test shows the runtime still fires
    /// that string against a nullable-bool property.
    /// </summary>
    private static object? Checked(Style style, string secondProperty, string setterProperty)
    {
        foreach (var trigger in style.Triggers.OfType<MultiTrigger>())
        {
            var conditions = trigger.Conditions.OfType<Condition>().ToList();
            if (!conditions.Any(static condition => condition.Property?.Name == "IsChecked" && string.Equals(condition.Value?.ToString(), "True", StringComparison.Ordinal))) continue;
            if (!conditions.Any(condition => condition.Property?.Name == secondProperty)) continue;
            foreach (var setter in trigger.Setters.OfType<Setter>())
            {
                if (setter.Property?.Name == setterProperty) return setter.Value;
            }
        }

        return null;
    }

    /// <summary>The setter a style trigger applies for one property, found by name.</summary>
    private static object? Setter(Style style, string triggerProperty, string setterProperty)
    {
        foreach (var trigger in style.Triggers.OfType<Trigger>())
        {
            if (trigger.Property?.Name != triggerProperty) continue;
            foreach (var setter in trigger.Setters.OfType<Setter>())
            {
                if (setter.Property?.Name == setterProperty) return setter.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Alias keys are not palette keys, so they are looked up the way a control looks them up. A copy
    /// would satisfy a colour comparison and still not follow the theme, so this asserts identity.
    /// </summary>
    private void AssertBrush(Brush expected, string aliasKey) =>
        Assert.Same(expected, _fixture.Application.TryFindResource(aliasKey));

    /// <summary>
    /// A trigger setter written as {ThemeResource X} is stored by this runtime as a lazy reference,
    /// not as the brush - which is why a theme flip repaints a style it already applied. So the
    /// claim checked here is the key the trigger carries; the alias test proves that key resolves to
    /// the palette object.
    /// </summary>
    private static void AssertTriggerSetter(object? value, string aliasKey)
    {
        var resourceKey = value?.GetType().GetProperty("ResourceKey")?.GetValue(value) as string;

        Assert.NotNull(resourceKey);
        Assert.Equal(aliasKey, resourceKey);
    }
}
