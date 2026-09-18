using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
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
