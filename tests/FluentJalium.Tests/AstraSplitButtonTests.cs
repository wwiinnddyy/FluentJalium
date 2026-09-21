using FluentJalium.Controls;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Markup;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The last two members of the button family: the native <see cref="SplitButton" /> restyled from
/// upstream's rows, and <see cref="FluentDropDownButton" />, which exists because 26.10.9 ships no
/// DropDownButton at all.
/// <para>
/// Upstream drives fourteen states from internal flags (PrimaryPointerOver, SecondaryPressed, FlyoutOpen,
/// the Checked family) that this runtime does not expose: the control declares only Command,
/// CommandParameter and Flyout, and <c>FlyoutBase.IsOpen</c> is a get-only CLR property rather than a
/// dependency property, so no cell or binding can read it (spike/SplitButtonProbe sections B3, F and J).
/// The adaptation moves each half's fill onto the half itself, which keeps the per-half states independent
/// - what those flags were for - and the mapping below says which upstream state each cell stands for
/// (docs/astra/audits/splitbutton.md).
/// </para>
/// <para>
/// What the framework does require is tested rather than assumed: the two halves have to be named
/// PrimaryButton and SecondaryButton, and the negative half of that is tested too, because a renamed part
/// builds, mounts and paints and only fails to open the flyout.
/// </para>
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraSplitButtonTests
{
    private static readonly Color SurfaceSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color DividerSentinel = Color.FromRgb(0x00, 0x80, 0xFF);
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    /// <summary>The grey 26.10.9's own split button chrome paints when no style of ours reaches it.</summary>
    private static readonly Color FrameworkChrome = Color.FromRgb(0x2C, 0x2C, 0x2E);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraSplitButtonTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Theory]
    [InlineData("SplitButtonBackground", "ControlFillColorDefaultBrush")]
    [InlineData("SplitButtonBackgroundPointerOver", "ControlFillColorSecondaryBrush")]
    [InlineData("SplitButtonBackgroundPressed", "ControlFillColorTertiaryBrush")]
    [InlineData("SplitButtonBackgroundDisabled", "ControlFillColorDisabledBrush")]
    [InlineData("SplitButtonForegroundSecondary", "TextFillColorSecondaryBrush")]
    [InlineData("SplitButtonForegroundSecondaryPressed", "TextFillColorTertiaryBrush")]
    // Rest and hover carry upstream's elevation name, which this palette cannot retint as a gradient, so
    // they land on the same instance the button family uses; pressed is upstream's own token. Which cell
    // names which row is the cell table's business - these three rows have no distinguishable pixels here.
    [InlineData("SplitButtonBorderBrush", "ControlStrokeColorDefaultBrush")]
    [InlineData("SplitButtonBorderBrushPointerOver", "ControlStrokeColorDefaultBrush")]
    [InlineData("SplitButtonBorderBrushPressed", "ControlStrokeColorDefaultBrush")]
    [InlineData("SplitButtonBorderBrushDivider", "ControlStrokeColorDefaultBrush")]
    [InlineData("DropDownButtonForegroundSecondary", "TextFillColorSecondaryBrush")]
    [InlineData("DropDownButtonForegroundSecondaryPointerOver", "TextFillColorTertiaryBrush")]
    [InlineData("DropDownButtonForegroundSecondaryPressed", "TextFillColorTertiaryBrush")]
    public void An_alias_resolves_to_the_object_upstream_names(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    /// <summary>
    /// The deferral, pinned. Upstream publishes thirteen *Checked* rows for SplitButton and they belong to
    /// ToggleSplitButton, which this runtime does not ship, so nothing could read them; publishing them
    /// would promise that overriding them reaches pixels when no state can ask for them. They return with
    /// that control; eight of the thirteen are named here, and this test fails the day someone ships a row
    /// with no consumer.
    /// </summary>
    [Theory]
    [InlineData("SplitButtonBackgroundChecked")]
    [InlineData("SplitButtonBackgroundCheckedPointerOver")]
    [InlineData("SplitButtonBackgroundCheckedPressed")]
    [InlineData("SplitButtonBackgroundCheckedDisabled")]
    [InlineData("SplitButtonForegroundChecked")]
    [InlineData("SplitButtonForegroundCheckedDisabled")]
    [InlineData("SplitButtonBorderBrushChecked")]
    [InlineData("SplitButtonBorderBrushCheckedDivider")]
    [InlineData("SplitButtonInAppBarUnfocusedPointerOver")]
    public void A_row_with_no_consumer_is_not_published(string key)
    {
        _fixture.Run(() => Assert.False(_fixture.Application.TryFindResource(key) is not null,
            $"{key} is published but no state on this runtime can reach it."));
    }

    [Fact]
    public void The_split_style_reads_its_own_rows_rather_than_the_buttons()
    {
        _fixture.Run(() =>
        {
            var style = FluentThemeManager.GetStyle("SplitButtonStyle");
            Assert.Equal("SplitButtonBackground", SetterKey(style, "Background"));
            Assert.Equal("SplitButtonForeground", SetterKey(style, "Foreground"));
            Assert.Equal("SplitButtonBorderBrush", SetterKey(style, "BorderBrush"));
            Assert.Equal("SplitButtonBorderThemeThickness", SetterKey(style, "BorderThickness"));
            Assert.Equal("SplitButtonPadding", SetterKey(style, "Padding"));
            Assert.Equal(new Thickness(1), Res("SplitButtonBorderThemeThickness"));
            Assert.Equal(new Thickness(11, 6, 11, 7), Res("SplitButtonPadding"));

            var split = Mount(new SplitButton { Content = "split" }, 220, 36);
            Assert.Equal(new Thickness(11, 6, 11, 7), split.Padding);
            Assert.Same(Res("ControlFillColorDefaultBrush"), split.Background);
        });
    }

    /// <summary>
    /// DoD step 4 as a table. Upstream's flags are not readable here, so each half's own hover and press
    /// stand in for PrimaryPointerOver / PrimaryPressed / SecondaryPointerOver / SecondaryPressed, and the
    /// same three rows sit in each cell as upstream writes for that state.
    /// </summary>
    [Theory]
    [InlineData("SplitButtonPrimaryButtonStyle", new[] { "IsMouseOver=True", "IsPressed=True", "IsEnabled=False" })]
    [InlineData("SplitButtonSecondaryButtonStyle", new[] { "IsMouseOver=True", "IsPressed=True", "IsEnabled=False" })]
    public void Each_half_carries_one_cell_per_reachable_state(string key, string[] expected)
    {
        List<Cell> cells = [];
        _fixture.Run(() => cells = Cells(FluentThemeManager.GetStyle(key)));
        Assert.Equal(expected, cells.Select(static cell => cell.Condition));
    }

    [Theory]
    [InlineData("SplitButtonPrimaryButtonStyle", "IsMouseOver=True", "Background", "SplitButtonBackgroundPointerOver")]
    [InlineData("SplitButtonPrimaryButtonStyle", "IsMouseOver=True", "Foreground", "SplitButtonForegroundPointerOver")]
    [InlineData("SplitButtonPrimaryButtonStyle", "IsMouseOver=True", "BorderBrush", "SplitButtonBorderBrushPointerOver")]
    [InlineData("SplitButtonPrimaryButtonStyle", "IsPressed=True", "Background", "SplitButtonBackgroundPressed")]
    [InlineData("SplitButtonPrimaryButtonStyle", "IsPressed=True", "Foreground", "SplitButtonForegroundPressed")]
    [InlineData("SplitButtonPrimaryButtonStyle", "IsPressed=True", "BorderBrush", "SplitButtonBorderBrushPressed")]
    [InlineData("SplitButtonPrimaryButtonStyle", "IsEnabled=False", "Background", "SplitButtonBackgroundDisabled")]
    [InlineData("SplitButtonPrimaryButtonStyle", "IsEnabled=False", "Foreground", "SplitButtonForegroundDisabled")]
    [InlineData("SplitButtonPrimaryButtonStyle", "IsEnabled=False", "BorderBrush", "SplitButtonBorderBrushDisabled")]
    [InlineData("SplitButtonSecondaryButtonStyle", "IsMouseOver=True", "Background", "SplitButtonBackgroundPointerOver")]
    [InlineData("SplitButtonSecondaryButtonStyle", "IsPressed=True", "Background", "SplitButtonBackgroundPressed")]
    [InlineData("SplitButtonSecondaryButtonStyle", "IsEnabled=False", "Background", "SplitButtonBackgroundDisabled")]
    public void A_half_cell_names_a_split_row(string key, string condition, string property, string row)
    {
        _fixture.Run(() =>
        {
            var cell = FindCell(FluentThemeManager.GetStyle(key), condition);
            var setter = cell.Setters.FirstOrDefault(candidate =>
                (candidate.Property?.Name ?? candidate.PropertyName) == property)
                ?? throw new InvalidOperationException($"{key} {condition} writes no {property}.");
            Assert.Equal(row, ResourceKey(setter));
        });
    }

    /// <summary>
    /// The chevron is the secondary half's only content and, being a part, a style cell cannot reach it, so
    /// its three state rows live in the template. Upstream writes the primary's hover foreground onto the
    /// chevron when the secondary is hovered and its own secondary-pressed row on press.
    /// </summary>
    [Theory]
    [InlineData("IsMouseOver=True", "SplitButtonForegroundPointerOver")]
    [InlineData("IsPressed=True", "SplitButtonForegroundSecondaryPressed")]
    [InlineData("IsEnabled=False", "SplitButtonForegroundDisabled")]
    public void The_secondary_chevron_has_a_cell_for_every_state_that_colors_it(string condition, string row)
    {
        _fixture.Run(() =>
        {
            var template = Template(FluentThemeManager.GetStyle("SplitButtonSecondaryButtonStyle"));
            // The disabled condition appears twice in this template (once for the ring, once for the chevron),
            // so the lookup is over every cell that matches rather than the first one.
            var setter = TemplateCells(template).Where(candidate => candidate.Condition == condition)
                .SelectMany(candidate => candidate.Setters)
                .FirstOrDefault(candidate =>
                    (candidate.TargetName ?? "self") == "Chevron"
                    && (candidate.Property?.Name ?? candidate.PropertyName) == "Stroke")
                ?? throw new InvalidOperationException($"{condition} writes no Chevron.Stroke; the cells carry " +
                    string.Join(", ", TemplateCells(template).Where(candidate => candidate.Condition == condition)
                        .SelectMany(static candidate => candidate.Setters)
                        .Select(static candidate => $"{candidate.TargetName}.{candidate.Property?.Name}")));
            Assert.Equal(row, ResourceKey(setter));
        });
    }

    /// <summary>
    /// The part-name contract, in both directions. Only a template whose halves are named PrimaryButton and
    /// SecondaryButton gets the framework's wiring: invoking the secondary opens the flyout and invoking the
    /// primary raises Click. A renamed half builds, mounts, paints and silently does nothing, which is the
    /// kind of break a style-only test would never see.
    /// </summary>
    [Fact]
    public void The_named_halves_drive_the_flyout_and_the_click_and_the_renamed_ones_do_not()
    {
        _fixture.Run(() =>
        {
            var dictionary = (ResourceDictionary)XamlReader.Parse(RenamedMarkup)!;
            var named = Mount(new SplitButton { Content = "split", Flyout = Flyout() }, 220, 36);
            var renamed = new SplitButton
            {
                Content = "renamed",
                Template = (ControlTemplate)dictionary["RenamedSplitTemplate"]!,
                Flyout = Flyout(),
            };
            PixelHarness.Build(renamed, 220, 36);
            PixelHarness.Settle(20);

            var namedClicks = 0;
            named.Click += (_, _) => namedClicks++;
            var renamedClicks = 0;
            renamed.Click += (_, _) => renamedClicks++;

            Invoke((Button)Part(named, "SecondaryButton"));
            Assert.True(named.Flyout!.IsOpen, "invoking the named secondary did not open the flyout.");
            Assert.Equal(0, namedClicks);

            Invoke((Button)Part(renamed, "SecondaryButtonZ"));
            Assert.False(renamed.Flyout!.IsOpen, "a renamed secondary opened the flyout, so the contract is not the name.");

            Invoke((Button)Part(named, "PrimaryButton"));
            Assert.Equal(1, namedClicks);

            Invoke((Button)Part(renamed, "PrimaryButtonZ"));
            Assert.Equal(0, renamedClicks);
        });
    }

    [Fact]
    public void The_primary_half_carries_the_content_and_the_command()
    {
        _fixture.Run(() =>
        {
            var executed = 0;
            var split = Mount(new SplitButton { Content = "split" }, 220, 36);
            split.Command = new CountUpCommand(() => executed++);

            var primary = (Button)Part(split, "PrimaryButton");
            Assert.Equal("split", primary.Content);
            Assert.Same(split.GetValue(Control.ForegroundProperty), primary.GetValue(Control.ForegroundProperty));

            Invoke(primary);

            // Exactly once. Upstream binds Command onto the primary half as well as raising Click, which is
            // correct there because WinUI's SplitButton does not execute the command itself; 26.10.9 does,
            // so carrying the binding here ran the command twice (measured, then removed from the template).
            Assert.Equal(1, executed);
            Assert.Null(primary.Command);
        });
    }

    [Fact]
    public void The_divider_is_the_row_upstream_names_for_it()
    {
        _fixture.Run(() =>
        {
            var split = Mount(new SplitButton { Content = "split" }, 220, 36);
            var divider = (Border)Part(split, "Divider");
            Assert.Same(Res("SplitButtonBorderBrushDivider"), divider.Background);

            FluentThemeManager.OverrideBrush("ControlStrokeColorDefaultBrush", DividerSentinel);
            try
            {
                Assert.Equal(DividerSentinel, ((SolidColorBrush)divider.Background!).Color);
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ControlStrokeColorDefaultBrush", null);
            }
        });
    }

    /// <summary>
    /// Focus lands on the control and not on either half, so the ring is the root's. Without this the whole
    /// split button would be the only member of the family that shows nothing on the keyboard.
    /// </summary>
    [Fact]
    public void The_shared_focus_ring_reaches_a_split_button()
    {
        _fixture.Run(() =>
        {
            var split = Mount(new SplitButton { Content = "split" }, 220, 36);
            var ring = RootRing(split);
            Assert.Equal(0d, ring.Opacity);

            Assert.True(split.Focus());
            PixelHarness.Settle(20);
            Assert.True(split.IsKeyboardFocused);
            Assert.Equal(1d, ring.Opacity);
        });
    }

    [Fact]
    public void The_split_button_takes_our_style_rather_than_the_framework_chrome()
    {
        // Before this batch the mounted control reported no style at all and built the framework's own
        // template: #2C2C2E surfaces and an emerald focus border (probe sections C and G). Both readings are
        // refused here, in the tree and in the raster.
        _fixture.Run(() =>
        {
            var split = Mount(new SplitButton { Content = "split" }, 220, 36);
            var ours = FluentThemeManager.GetStyle("SplitButtonStyle");

            // An implicit style never lands in FrameworkElement.Style on this runtime - Button, ToggleButton,
            // HyperlinkButton and SplitButton all report Style==null while painting our tokens (probe
            // section K) - so the template object is the readable proof that our style is the effective one.
            Assert.Null(split.Style);
            Assert.Same(Template(ours), split.Template);
            Assert.Same(Res("ControlFillColorDefaultBrush"), split.Background);
            Assert.Same(Res("ControlFillColorDefaultBrush"), ((Button)Part(split, "PrimaryButton")).Background);

            var rest = PixelHarness.Render(split, 220, 36);
            Assert.True(rest.PaintedPixels > 0, $"capture is empty: {rest.Top(6)}");
            Assert.Equal(0, rest.Count(FrameworkChrome));
            Assert.Equal(0, rest.Count(BrandEmerald));
        });
    }

    /// <summary>
    /// The halves fill their columns rather than sizing to their content. The shared layout style aligns a
    /// button Left / Center, and a left-aligned half inside a star column leaves the rest of the control
    /// unpainted - measured here as 53x35 for a 220-wide control, with the missing 131 pixels of width
    /// showing up as an empty capture.
    /// </summary>
    [Fact]
    public void Both_halves_stretch_into_their_columns()
    {
        _fixture.Run(() =>
        {
            var split = Mount(new SplitButton { Content = "split" }, 220, 36);
            var primary = (Button)Part(split, "PrimaryButton");
            var secondary = (Button)Part(split, "SecondaryButton");

            Assert.Equal(36d, primary.ActualHeight, 3);
            Assert.Equal(36d, secondary.ActualHeight, 3);
            Assert.Equal(35d, secondary.ActualWidth, 3);
            Assert.True(primary.ActualWidth > 180,
                $"the primary half took {primary.ActualWidth} of the ~184 pixels left by a 35-pixel secondary.");
        });
    }

    [Fact]
    public void The_resting_split_button_paints_the_control_fill_token()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", SurfaceSentinel);
            try
            {
                var rest = PixelHarness.Render(new SplitButton { Content = "split", Width = 220, Height = 36 }, 220, 36);
                Assert.True(rest.Stable, $"capture never settled: {rest.Top(6)}");

                // Both halves, not one: the primary takes the remaining 184 columns of pixels and the
                // secondary the fixed 35, so a rest fill that reaches only one of them is a half that
                // stopped stretching.
                Assert.True(rest.Count(SurfaceSentinel) > 5_000,
                    $"rest fill did not reach both halves; top={rest.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void The_split_button_follows_the_theme()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var (lightInk, light) = PixelHarness.AssertSurfaceLands(
                new SplitButton { Content = "split", Width = 220, Height = 36 }, PixelHarness.Self,
                PixelHarness.LightPage, 220, 36, 2_000, "the split button's own surface");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var (darkInk, dark) = PixelHarness.AssertSurfaceLands(
                new SplitButton { Content = "split", Width = 220, Height = 36 }, PixelHarness.Self,
                PixelHarness.DarkPage, 220, 36, 2_000, "the split button's own surface");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.NotEqual(lightInk, darkInk);
            Assert.Equal(0, light.Count(BrandEmerald));
            // The framework's own dark chrome is the other thing that must not survive the retemplate.
            Assert.Equal(0, dark.Count(FrameworkChrome));
        });
    }

    [Fact]
    public void Activating_the_drop_down_opens_its_flyout_and_a_second_activation_closes_it()
    {
        _fixture.Run(() =>
        {
            var dropDown = Mount(new FluentDropDownButton { Content = "more", Flyout = Flyout() }, 160, 32);
            Assert.False(dropDown.IsExpanded);

            Invoke(dropDown);
            Assert.True(dropDown.IsExpanded, "activating the button did not expand it.");
            Assert.True(dropDown.Flyout!.IsOpen, "IsExpanded said open while the flyout stayed closed.");

            Invoke(dropDown);
            Assert.False(dropDown.IsExpanded, "the second activation did not collapse it.");
            Assert.False(dropDown.Flyout!.IsOpen);
        });
    }

    [Fact]
    public void Closing_the_flyout_from_anywhere_collapses_the_button()
    {
        _fixture.Run(() =>
        {
            var dropDown = Mount(new FluentDropDownButton { Content = "more", Flyout = Flyout() }, 160, 32);
            dropDown.IsExpanded = true;
            Assert.True(dropDown.Flyout!.IsOpen);

            dropDown.IsExpanded = false;
            Assert.False(dropDown.Flyout!.IsOpen, "clearing IsExpanded left the flyout up.");

            dropDown.IsExpanded = true;
            dropDown.Flyout!.Hide();
            Assert.False(dropDown.IsExpanded, "hiding the flyout did not report back to IsExpanded.");
        });
    }

    [Fact]
    public void A_drop_down_with_no_flyout_stays_closed_and_a_disabled_one_refuses()
    {
        _fixture.Run(() =>
        {
            var empty = Mount(new FluentDropDownButton { Content = "more" }, 160, 32);
            empty.IsExpanded = true;
            Assert.False(empty.IsExpanded, "an expanded state was claimed with nothing to show.");

            var dropDown = Mount(new FluentDropDownButton { Content = "more", Flyout = Flyout() }, 160, 32);
            dropDown.IsEnabled = false;
            Assert.Throws<InvalidOperationException>(() => Invoke(dropDown));
            Assert.False(dropDown.IsExpanded);
        });
    }

    [Fact]
    public void The_drop_down_paints_the_button_rows_and_names_its_own_chevron_rows()
    {
        _fixture.Run(() =>
        {
            var style = FluentThemeManager.GetStyle("DefaultDropDownButtonStyle");
            Assert.Equal("ButtonBackground", SetterKey(style, "Background"));
            Assert.Equal("ButtonBorderThemeThickness", SetterKey(style, "BorderThickness"));

            var hover = FindCell(TemplateCells(Template(style)), "IsMouseOver=True");
            var chevron = hover.Setters.Single(static candidate =>
                (candidate.TargetName ?? "self") == "Chevron");
            Assert.Equal("DropDownButtonForegroundSecondaryPointerOver", ResourceKey(chevron));

            var surface = FindCell(Cells(style), "IsMouseOver=True");
            Assert.Equal("ButtonBackgroundPointerOver", ResourceKey(surface.Setters.Single(static candidate =>
                (candidate.Property?.Name ?? candidate.PropertyName) == "Background")));
        });
    }

    [Fact]
    public void The_drop_down_button_shows_no_brand_emerald_and_follows_the_theme()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var (lightInk, light) = PixelHarness.AssertSurfaceLands(
                new FluentDropDownButton { Content = "more", Width = 160, Height = 32 }, PixelHarness.Self,
                PixelHarness.LightPage, 160, 32, 2_000, "the drop down button's own surface");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var (darkInk, dark) = PixelHarness.AssertSurfaceLands(
                new FluentDropDownButton { Content = "more", Width = 160, Height = 32 }, PixelHarness.Self,
                PixelHarness.DarkPage, 160, 32, 2_000, "the drop down button's own surface");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.NotEqual(lightInk, darkInk);
            Assert.Equal(0, light.Count(BrandEmerald));
            Assert.Equal(0, dark.Count(BrandEmerald));
        });
    }

    /// <summary>
    /// A template with the same geometry and the wrong part names, used only to prove the name is what the
    /// framework wires on. It is a probe rather than a fixture because nothing ships it.
    /// </summary>
    private const string RenamedMarkup = """
        <ResourceDictionary xmlns='http://schemas.jalium.ui/2024'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='RenamedSplitTemplate' TargetType='SplitButton'>
            <Grid>
              <Grid.ColumnDefinitions>
                <ColumnDefinition Width='*' />
                <ColumnDefinition Width='1' />
                <ColumnDefinition Width='35' />
              </Grid.ColumnDefinitions>
              <Button x:Name='PrimaryButtonZ' Grid.Column='0' Content='{TemplateBinding Content}' />
              <Border x:Name='DividerZ' Grid.Column='1' Background='Orange' />
              <Button x:Name='SecondaryButtonZ' Grid.Column='2' Content='v' />
            </Grid>
          </ControlTemplate>
        </ResourceDictionary>
        """;

    private object? Res(string key) => _fixture.Application.TryFindResource(key);

    private FluentDropDownButton Mount(FluentDropDownButton dropDown, double width, double height)
    {
        PixelHarness.Build(dropDown, (int)width, (int)height);
        return dropDown;
    }

    private SplitButton Mount(SplitButton split, double width, double height)
    {
        PixelHarness.Build(split, (int)width, (int)height);
        return split;
    }

    private static MenuFlyout Flyout()
    {
        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuFlyoutItem { Text = "one" });
        flyout.Items.Add(new MenuFlyoutItem { Text = "two" });
        return flyout;
    }

    /// <summary>
    /// The framework's own activation entry point, the one a keyboard or a screen reader reaches, so no test
    /// in this file needs a synthetic pointer and none of them touches the user's desktop.
    /// </summary>
    private static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();

    /// <summary>
    /// The root's own ring. Both halves come from the shared layout style and each carries a part named
    /// FocusOutline, so the first hit by name is the primary half's - which never focuses, because upstream
    /// keeps the focus on the control. This walks the template root's children instead.
    /// </summary>
    private static Border RootRing(SplitButton split)
    {
        if (VisualTreeHelper.GetChild(split, 0) is not Panel root)
        {
            throw new InvalidOperationException("The split button's template root is not a panel.");
        }

        foreach (var child in root.Children)
        {
            if (child is Border { Name: "FocusOutline" } border)
            {
                return border;
            }
        }

        throw new InvalidOperationException("The split button's template root carries no FocusOutline.");
    }

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name} in the built tree.");

    private static ControlTemplate Template(Style style) =>
        (style.Setters.Cast<object>().OfType<Setter>()
            .First(static setter => (setter.Property?.Name ?? setter.PropertyName) == "Template").Value as ControlTemplate)!;

    private static List<Cell> Cells(Style style) => ToCells(style.Triggers.Cast<object>());

    private static List<Cell> TemplateCells(ControlTemplate template) => ToCells(template.Triggers.Cast<object>());

    private static List<Cell> ToCells(IEnumerable<object> triggers) => triggers.Select(static trigger => trigger switch
    {
        Trigger single => new Cell(
            $"{single.Property?.Name ?? "UNRESOLVED"}={Form(single.Value)}",
            single.Setters.Cast<object>().OfType<Setter>().ToArray()),
        MultiTrigger multi => new Cell(
            string.Join("+", multi.Conditions.Cast<object>().OfType<Condition>()
                .Select(condition => $"{condition.Property?.Name ?? "UNRESOLVED"}={Form(condition.Value)}")),
            multi.Setters.Cast<object>().OfType<Setter>().ToArray()),
        _ => new Cell(trigger.GetType().Name, []),
    }).ToList();

    private static Cell FindCell(List<Cell> cells, string condition) =>
        cells.Find(candidate => candidate.Condition == condition)
        ?? throw new InvalidOperationException($"No cell for {condition}; the style carries " +
            string.Join(" | ", cells.Select(static candidate => candidate.Condition)));

    private static Cell FindCell(Style style, string condition) => FindCell(Cells(style), condition);

    private static string Form(object? value) => value?.ToString() ?? "null";

    private static string? ResourceKey(Setter setter) =>
        setter.Value?.GetType().GetProperty("ResourceKey")?.GetValue(setter.Value) as string;

    private static string? SetterKey(Style style, string property) =>
        style.Setters.Cast<object>().OfType<Setter>()
            .FirstOrDefault(setter => (setter.Property?.Name ?? setter.PropertyName) == property) is { } setter
            ? ResourceKey(setter)
            : null;

    private sealed record Cell(string Condition, Setter[] Setters);

    /// <summary>The smallest command that answers "did the primary half carry Command through?".</summary>
    private sealed class CountUpCommand(Action action) : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => action();
    }
}
