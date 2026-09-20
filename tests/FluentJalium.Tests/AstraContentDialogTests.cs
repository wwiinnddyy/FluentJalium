using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// ContentDialog. The plan called for an own type; the runtime inventory (adaptation/00 S0-v) shows
/// <see cref="ContentDialog"/> shipping natively with a complete member surface and a code-built template,
/// so AGENTS.md's rule sends this down the re-template path instead.
/// <para>
/// Every claim here is read off a dialog that is actually open. That is a correction, not a detail: a merely
/// mounted ContentDialog measures Visibility=Collapsed and 0x0 (measured, spike/RightGapProbe shown mode), so a
/// test that places one in a window reads a tree that is laid out by nothing and paints nothing - which is how
/// the first draft of this file produced a card with no local size values, a title with no style, no pixels and
/// no click. The control also refuses to open a dialog that is already attached ("Popup-hosted ContentDialog
/// must not already be attached to the visual tree"), so <see cref="Shown"/> has to keep its promise and open
/// the detached one into the harness window's own overlay host.
/// </para>
/// <para>
/// Upstream drives the dialog from six VisualStateManager groups, which markup cannot express here, so each
/// state is a cell and the cells are checked by reading the control's behaviour back, not by walking key names.
/// </para>
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraContentDialogTests : IDisposable
{
    private static readonly Color SurfaceSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    private readonly List<ContentDialog> _opened = [];
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraContentDialogTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    /// <summary>
    /// A shown dialog lives in the shared host window's overlay layer, so leaving one open would dim every later
    /// full-window capture in the suite. xunit builds one instance per fact, so this runs after each one.
    /// </summary>
    public void Dispose()
    {
        _fixture.Run(() =>
        {
            foreach (var dialog in _opened.Where(static opened => opened.Visibility == Visibility.Visible))
            {
                dialog.Hide();
            }

            _opened.Clear();
            PixelHarness.Settle();
        });
    }

    /// <summary>The six alias rows, by upstream name and upstream target.</summary>
    [Theory]
    [InlineData("ContentDialogForeground", "TextFillColorPrimaryBrush")]
    [InlineData("ContentDialogBackground", "SolidBackgroundFillColorBaseBrush")]
    [InlineData("ContentDialogSmokeFill", "SmokeFillColorDefaultBrush")]
    [InlineData("ContentDialogTopOverlay", "LayerFillColorAltBrush")]
    [InlineData("ContentDialogBorderBrush", "SurfaceStrokeColorDefaultBrush")]
    [InlineData("ContentDialogSeparatorBorderBrush", "CardStrokeColorDefaultBrush")]
    public void An_alias_row_resolves_to_its_target(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    /// <summary>The four numeric rows that do parse here, at upstream's values.</summary>
    [Theory]
    [InlineData("ContentDialogBorderWidth", "1,1,1,1")]
    [InlineData("ContentDialogTitleMargin", "0,0,0,12")]
    [InlineData("ContentDialogPadding", "24,24,24,24")]
    [InlineData("ContentDialogSeparatorThickness", "0,0,0,1")]
    public void A_metric_row_carries_upstreams_value(string key, string thickness)
    {
        _fixture.Run(() => Assert.Equal(thickness, Application.Current!.TryFindResource(key)!.ToString()));
    }

    /// <summary>
    /// The five rows upstream has and this dictionary does not publish: an x:Double or GridLength row cannot be
    /// parsed by this reader (adaptation/00), so the value rides as a literal at its one consumer, and publishing
    /// the name would promise a lever that does not exist.
    /// </summary>
    [Theory]
    [InlineData("ContentDialogMinWidth")]
    [InlineData("ContentDialogMaxWidth")]
    [InlineData("ContentDialogMinHeight")]
    [InlineData("ContentDialogMaxHeight")]
    [InlineData("ContentDialogButtonSpacing")]
    public void A_row_this_reader_cannot_parse_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(Application.Current!.TryFindResource(key)));
    }

    [Fact]
    public void Our_template_displaces_the_frameworks_and_keeps_its_part_names()
    {
        _fixture.Run(() =>
        {
            var dialog = Shown();
            Assert.Same(Template(), dialog.Template);
            Assert.All(new[]
            {
                "PART_Root",
                "PART_Overlay",
                "PART_DialogCard",
                "DialogSurface",
                "PART_ContentScrollViewer",
                "PART_TitleHost",
                "PART_ContentPresenter",
                "PART_ButtonArea",
                "PART_ButtonPanel",
                "PART_PrimaryButton",
                "PART_SecondaryButton",
                "PART_CloseButton",
            }, name => Assert.NotNull(Part(dialog, name)));

            // The names are the control's contract, so a part that exists but has the wrong type is a break.
            Assert.IsType<ContentControl>(Part(dialog, "PART_TitleHost"));
            Assert.IsType<Button>(Part(dialog, "PART_PrimaryButton"));
        });
    }

    [Fact]
    public void The_surface_wears_the_rows_a_dialog_surface_needs()
    {
        _fixture.Run(() =>
        {
            var dialog = Shown();
            var surface = (Border)Part(dialog, "DialogSurface");
            var card = (Border)Part(dialog, "PART_DialogCard");

            Assert.Multiple(
                () => Assert.Same(Res("ContentDialogBackground"), surface.Background),
                () => Assert.Same(Res("ContentDialogBorderBrush"), surface.BorderBrush),
                () => Assert.Equal(new Thickness(1), surface.BorderThickness),
                () => Assert.Equal(new CornerRadius(8), surface.CornerRadius),
                () => Assert.Equal(320d, surface.MinWidth),
                () => Assert.Equal(548d, surface.MaxWidth),
                () => Assert.Equal(184d, surface.MinHeight),
                () => Assert.Equal(756d, surface.MaxHeight),
                // The wrapper is the framework's gutter, and it is bare: no fill, no stroke, no radius of its
                // own, so the one rounded surface in the card is the one that carries the content.
                () => Assert.Null(card.Background),
                () => Assert.Equal(new Thickness(24), card.Margin),
                () => Assert.Equal(new CornerRadius(0), card.CornerRadius),
                () => Assert.Same(Res("ContentDialogSmokeFill"), ((Border)Part(dialog, "PART_Overlay")).Background));
        });
    }

    /// <summary>
    /// Who owns the card's width, measured rather than assumed. The control maps its own size box onto whatever
    /// is named PART_DialogCard and caps it at the host width less the 24 DIP gutter: a dialog with no local
    /// MaxWidth gets 838.3 on a 886.3 host, and an app that sets MaxWidth="548" sees that number arrive on the
    /// card unchanged. Upstream's 548 default is therefore not something a literal on that element can hold,
    /// which is why the cap lives one layer in, on DialogSurface, where the control has no name to write through.
    /// </summary>
    [Fact]
    public void The_control_owns_the_card_box_and_the_surface_keeps_upstreams_cap()
    {
        _fixture.Run(() =>
        {
            var dialog = Shown();
            var card = (Border)Part(dialog, "PART_DialogCard");
            var surface = (Border)Part(dialog, "DialogSurface");
            var gutter = dialog.ActualWidth - card.Margin.Left - card.Margin.Right;

            Assert.Multiple(
                () => Assert.NotEqual(DependencyProperty.UnsetValue, card.ReadLocalValue(FrameworkElement.MaxWidthProperty)),
                () => Assert.Equal(gutter, card.MaxWidth, 1),
                () => Assert.Equal(548d, surface.MaxWidth),
                () => Assert.Equal(DependencyProperty.UnsetValue, surface.ReadLocalValue(FrameworkElement.MaxWidthProperty)),
                () => Assert.True(surface.ActualWidth <= 548d, $"the surface measured {surface.ActualWidth} past its cap"));
        });
    }

    [Fact]
    public void The_app_can_widen_the_card_up_to_the_gutter()
    {
        _fixture.Run(() =>
        {
            var dialog = Shown(maxWidth: 420);
            var card = (Border)Part(dialog, "PART_DialogCard");

            Assert.Equal(420d, card.MaxWidth);
            Assert.Equal(420d, Assert.IsType<double>(card.ReadLocalValue(FrameworkElement.MaxWidthProperty)));
        });
    }

    [Fact]
    public void The_title_and_the_body_read_their_two_rows()
    {
        _fixture.Run(() =>
        {
            var dialog = Shown();
            var title = (ContentControl)Part(dialog, "PART_TitleHost");
            var body = (ContentPresenter)Part(dialog, "PART_ContentPresenter");
            var strip = (Border)Part(dialog, "TitleStrip");
            var buttonArea = (Border)Part(dialog, "PART_ButtonArea");

            // The title's 20 / SemiBold are local values on a TextBlock this dictionary builds, reached through
            // TitleTemplate. They cannot sit on the presenter instead: a ContentPresenter has no FontSize, and a
            // FontSize set on the ContentControl host is dropped on the floor by the text element the framework
            // generates for it (measured: weight=SemiBold survived, size stayed 14).
            var titleText = PixelHarness.Descendant<TextBlock>(title)
                ?? throw new InvalidOperationException("the title template built no text element.");

            Assert.Multiple(
                () => Assert.Equal("Publish this draft?", title.Content),
                () => Assert.Equal(new Thickness(0, 0, 0, 12), title.Margin),
                () => Assert.Equal(20d, titleText.FontSize),
                () => Assert.Equal(FontWeights.SemiBold, titleText.FontWeight),
                () => Assert.Equal("Once it is public, readers see it as of now.", body.Content),
                () => Assert.Same(Res("ContentDialogTopOverlay"), strip.Background),
                () => Assert.Equal(new Thickness(24), strip.Padding),
                () => Assert.Same(Res("ContentDialogSeparatorBorderBrush"), strip.BorderBrush),
                () => Assert.Equal(new Thickness(0, 0, 0, 1), strip.BorderThickness),
                () => Assert.Same(Res("ContentDialogBackground"), buttonArea.Background),
                () => Assert.Equal(new Thickness(24), buttonArea.Padding));
        });
    }

    [Fact]
    public void The_command_row_splits_the_width_the_way_upstream_does_when_all_three_show()
    {
        _fixture.Run(() =>
        {
            var dialog = Shown();
            var panel = (Grid)Part(dialog, "PART_ButtonPanel");
            var widths = panel.ColumnDefinitions.Select(static column => column.Width.ToString()).ToArray();

            Assert.Multiple(
                () => Assert.Equal(new[] { "*", "8", "*", "8", "*" }, widths),
                () => Assert.Equal(0, Grid.GetColumn(Part(dialog, "PART_PrimaryButton"))),
                () => Assert.Equal(2, Grid.GetColumn(Part(dialog, "PART_SecondaryButton"))),
                () => Assert.Equal(4, Grid.GetColumn(Part(dialog, "PART_CloseButton"))),
                // Half-width buttons, not thirds: with three texts set the two star columns carry the row.
                () => Assert.Equal(
                    ((Button)Part(dialog, "PART_PrimaryButton")).ActualWidth,
                    ((Button)Part(dialog, "PART_CloseButton")).ActualWidth,
                    1));
        });
    }

    /// <summary>
    /// The command row for every combination of button texts, as the columns the surviving buttons actually
    /// occupy. Upstream reaches these arrangements by resizing its columns from a VisualState; a cell cannot
    /// write a named ColumnDefinition on this runtime (measured in both value shapes), so this template keeps
    /// the five columns fixed and moves the buttons instead. The result is upstream's where it can be: three
    /// buttons in thirds, every shorter row adjacent with the unused third at an end rather than a hole in the
    /// middle, and one button on the right - and the residual difference is in the audit's Known Gaps, not here.
    /// </summary>
    [Theory]
    [InlineData("P", "S", "C", "0:2:4")]
    [InlineData(null, "S", "C", "2:4")]
    [InlineData("P", null, "C", "2:4")]
    [InlineData("P", "S", null, "0:2")]
    [InlineData(null, null, "C", "4")]
    [InlineData(null, "S", null, "4")]
    [InlineData("P", null, null, "4")]
    [InlineData(null, null, null, "")]
    public void The_command_row_holds_one_arrangement_per_text_combination(
        string? primary, string? secondary, string? close, string columns)
    {
        _fixture.Run(() =>
        {
            var dialog = Shown(primary: primary, secondary: secondary, close: close);
            var visible = new[] { "PART_PrimaryButton", "PART_SecondaryButton", "PART_CloseButton" }
                .Select(name => (Button)Part(dialog, name))
                .Where(static button => button.Visibility == Visibility.Visible)
                .ToArray();
            var area = (Border)Part(dialog, "PART_ButtonArea");

            Assert.Multiple(
                () => Assert.Equal(columns, string.Join(":", visible.Select(static button => Grid.GetColumn(button)))),
                () => Assert.Equal(columns.Length == 0 ? Visibility.Collapsed : Visibility.Visible, area.Visibility),
                // Every survivor sits in one star column of the same grid, so equal widths are the check that
                // no arrangement leaves one button wider than its neighbour.
                () => Assert.All(visible, button => Assert.Equal(
                    visible[0].ActualWidth,
                    button.ActualWidth,
                    1)));
        });
    }

    /// <summary>
    /// The default button's accent, and the shape the cell has to take to reach it. Written onto the button it
    /// never lands - the button's Style is a local value, because the template binds it to the control's
    /// *ButtonStyle - so the cell writes that property on the dialog instead, which the binding then carries.
    /// Claimed in both directions: the button changes, the other two do not, and setting DefaultButton back to
    /// None restores the resting style.
    /// </summary>
    [Fact]
    public void The_default_button_takes_the_accent_style_and_gives_it_back()
    {
        _fixture.Run(() =>
        {
            var dialog = Shown();
            var primary = (Button)Part(dialog, "PART_PrimaryButton");
            var secondary = (Button)Part(dialog, "PART_SecondaryButton");
            var rest = primary.Style;
            var accent = AccentStyleFromCell();

            dialog.DefaultButton = ContentDialogButton.Primary;
            PixelHarness.Settle();
            Assert.Multiple(
                () => Assert.NotSame(rest, primary.Style),
                () => Assert.Same(accent, dialog.PrimaryButtonStyle),
                () => Assert.Same(rest, secondary.Style));

            dialog.DefaultButton = ContentDialogButton.None;
            PixelHarness.Settle();
            Assert.Same(rest, primary.Style);
        });
    }

    [Fact]
    public void FullSizeDesired_stretches_the_card_and_the_enabled_flags_reach_the_buttons()
    {
        _fixture.Run(() =>
        {
            var dialog = Shown();
            var card = (Border)Part(dialog, "PART_DialogCard");
            var surface = (Border)Part(dialog, "DialogSurface");
            var restHeight = surface.ActualHeight;

            dialog.FullSizeDesired = true;
            dialog.IsPrimaryButtonEnabled = false;
            PixelHarness.Settle();

            Assert.Multiple(
                () => Assert.Equal(VerticalAlignment.Stretch, card.VerticalAlignment),
                () => Assert.Equal(VerticalAlignment.Stretch, surface.VerticalAlignment),
                () => Assert.True(surface.ActualHeight > restHeight,
                    $"full size did not stretch the card: {restHeight} -> {surface.ActualHeight}"),
                () => Assert.False(((Button)Part(dialog, "PART_PrimaryButton")).IsEnabled),
                () => Assert.True(((Button)Part(dialog, "PART_SecondaryButton")).IsEnabled));
        });
    }

    [Fact]
    public void The_template_carries_the_ten_cells_upstream_has_states_for()
    {
        // Reading the style needs the UI thread, because FluentThemeManager refuses to hand a style to any other.
        _fixture.Run(() => Assert.Equal(
            new[]
            {
                "FullSizeDesired=True",
                "DefaultButton=Primary",
                "DefaultButton=Secondary",
                "DefaultButton=Close",
                "PrimaryButtonText=null",
                "SecondaryButtonText=null",
                "CloseButtonText=null",
                "PrimaryButtonText=null+CloseButtonText=null",
                "SecondaryButtonText=null+CloseButtonText=null",
                "PrimaryButtonText=null+SecondaryButtonText=null+CloseButtonText=null",
            },
            TemplateCells().Select(static cell => cell.Condition).ToArray()));
    }

    /// <summary>
    /// The behaviour half of the exit, and the one that a merely mounted dialog could not show: a button built by
    /// our template still drives the control's own event, closes the dialog and completes the operation with the
    /// result WinUI would report. Kept in one fact because splitting it would let a template that answers clicks
    /// but never closes the dialog pass.
    /// </summary>
    [Fact]
    public void A_button_of_our_template_reports_through_the_controls_own_events_and_ends_the_operation()
    {
        _fixture.Run(() =>
        {
            var dialog = new ContentDialog
            {
                Title = "Publish this draft?",
                Content = "Once it is public, readers see it as of now.",
                PrimaryButtonText = "Publish",
                SecondaryButtonText = "Save draft",
                CloseButtonText = "Cancel",
            };
            var opened = 0;
            var closed = 0;
            var clicks = 0;
            dialog.Opened += (_, _) => opened++;
            dialog.Closed += (_, _) => closed++;
            dialog.PrimaryButtonClick += (_, _) => clicks++;

            var operation = dialog.ShowAsync();
            PixelHarness.Settle();
            Assert.Equal(1, opened);

            Invoke((Button)Part(dialog, "PART_PrimaryButton"));
            PixelHarness.Settle();

            Assert.Multiple(
                () => Assert.Equal(1, clicks),
                () => Assert.Equal(1, closed),
                () => Assert.True(operation.IsCompleted, "the show operation never completed"),
                () => Assert.Equal(Visibility.Collapsed, dialog.Visibility));
        });
    }

    [Fact]
    public void A_dialog_already_in_the_tree_refuses_to_open()
    {
        _fixture.Run(() =>
        {
            var dialog = new ContentDialog { Title = "t", Content = "body", CloseButtonText = "Cancel" };
            PixelHarness.Build(dialog, 548, 220);

            // Statement body, not an expression: the refusal is synchronous, so the operation is never awaited,
            // and the async-looking overload would send the analyzer down the ThrowsAsync path this is not.
            var error = Record.Exception(() =>
            {
                _ = dialog.ShowAsync();
            });

            Assert.IsType<InvalidOperationException>(error);
            Assert.Contains("must not already be attached", error!.Message, StringComparison.Ordinal);
        });
    }

    /// <summary>
    /// A disabled button must not answer the same activation that drives an enabled one. Whether the peer throws
    /// or stays silent is the runtime's choice - ToggleButton's peer throws - so this records which it did and
    /// claims only the part that matters: no click reaches the control either way.
    /// </summary>
    [Fact]
    public void A_disabled_button_of_our_template_raises_no_click()
    {
        _fixture.Run(() =>
        {
            var dialog = Shown();
            var primary = (Button)Part(dialog, "PART_PrimaryButton");
            var clicks = 0;
            dialog.PrimaryButtonClick += (_, _) => clicks++;
            primary.IsEnabled = false;
            PixelHarness.Settle();

            var error = Record.Exception(() => Invoke(primary));
            PixelHarness.Settle();

            Assert.Multiple(
                () => Assert.Equal(0, clicks),
                () => Assert.True(error is null or InvalidOperationException,
                    $"activation failed the wrong way: {error?.GetType().Name} {error?.Message}"));
        });
    }

    [Fact]
    public void The_surface_row_reaches_the_card_pixels_and_nothing_invented_with_it()
    {
        _fixture.Run(() =>
        {
            // The sentinel goes on the palette row before the dialog resolves, and the dialog is built after it,
            // because a control that resolved its brushes first keeps them.
            FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", SurfaceSentinel);
            try
            {
                var dialog = Shown();
                var surface = (Border)Part(dialog, "DialogSurface");
                var sample = PixelHarness.Chrome(surface);

                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");
                // The crop is the whole card, and the top-overlay strip owns the larger part of it, so the
                // surface row is measured on the band it does own rather than on a share of the total.
                Assert.True(sample.Count(SurfaceSentinel) > 5_000,
                    $"the dialog's surface row did not reach the pixels; top={sample.Top(8)}");
                Assert.Equal(0, sample.Count(BrandEmerald));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", null);
            }
        });
    }

    /// <summary>
    /// One dialog per theme, not one dialog captured twice: the theme flip re-resolves {ThemeResource} for
    /// brushes that have not been looked up yet, and this control's surfaces resolved theirs on the way in.
    /// The captures are not claimed to be bit-identical between rounds - a card that holds text and sits in a
    /// popup the framework may still be moving does not always settle, so the claim is the palette difference
    /// and the painted area, and the instability is in the audit's Known Gaps.
    /// </summary>
    [Fact]
    public void A_shown_dialog_looks_different_in_light_and_in_dark()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var lightDialog = Shown();
            var light = PixelHarness.Chrome((Border)Part(lightDialog, "DialogSurface"));
            lightDialog.Hide();
            PixelHarness.Settle();

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            try
            {
                var darkDialog = Shown();
                var dark = PixelHarness.Chrome((Border)Part(darkDialog, "DialogSurface"));
                darkDialog.Hide();

                Assert.Multiple(
                    () => Assert.True(light.PaintedPixels > 20_000, $"the light card barely painted: {light.Top(3)}"),
                    () => Assert.True(dark.PaintedPixels > 20_000, $"the dark card barely painted: {dark.Top(3)}"),
                    () => Assert.NotEqual(light.Top(3), dark.Top(3)));
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    /// <summary>
    /// The corner question the last batch left open for this control (audits/corner-radius.md §6): a Border here
    /// rounds its own fill and never clips its child, so anything that fills to the card's edge paints a square
    /// corner over the arc. (1,1) is 9.9 DIP from the arc's centre at (8,8) - outside the card - and it is not on
    /// the antialiasing ramp, so it answers the question directly. Both halves were real defects, measured before
    /// and after: the title strip read #FFFFFF - its own brush - at (1,1), (2,1) and (1,2) until it carried
    /// CornerRadius 8,8,0,0, and the command row read #F3F3F3 at (1,h-2) until it carried 0,0,8,8. That second
    /// one is the sneaky shape: the command row paints the card's own fill, so the nub is invisible against the
    /// surface and only shows outside the arc - which means the pixel beside it (2,h-2) sits on the ramp both
    /// before and after and is NOT a discriminator; the (1,·) probes are. Six probes, three radii.
    /// </summary>
    [Fact]
    public void The_two_inner_surfaces_leave_the_cards_round_corners_alone()
    {
        _fixture.Run(() =>
        {
            var dialog = Shown();
            var surface = (Border)Part(dialog, "DialogSurface");
            var strip = (Border)Part(dialog, "TitleStrip");
            var area = (Border)Part(dialog, "PART_ButtonArea");
            var height = (int)surface.ActualHeight;
            var right = (int)surface.ActualWidth - 2;
            var stripKey = PixelHarness.PixelKey(((SolidColorBrush)Res("ContentDialogTopOverlay")!).Color);
            var corner = PixelHarness.PixelAt(surface, 1, 1);
            var beside = PixelHarness.PixelAt(surface, 2, 1);
            var topRight = PixelHarness.PixelAt(surface, right, 1);
            var bottom = PixelHarness.PixelAt(surface, 1, height - 2);
            var bottomBeside = PixelHarness.PixelAt(surface, 2, height - 2);
            var bottomRight = PixelHarness.PixelAt(surface, right, height - 2);
            var fillKey = PixelHarness.PixelKey(((SolidColorBrush)Res("ContentDialogBackground")!).Color);

            Assert.Multiple(
                () => Assert.Equal(new CornerRadius(8), surface.CornerRadius),
                () => Assert.Equal(new CornerRadius(8, 8, 0, 0), strip.CornerRadius),
                () => Assert.Equal(new CornerRadius(0, 0, 8, 8), area.CornerRadius),
                () => Assert.True(corner != stripKey,
                    $"the strip reaches the card's corner: (1,1) reads {PixelHarness.Hex(corner)}, the strip's own brush"),
                () => Assert.True(beside != stripKey,
                    $"the strip reaches the card's corner: (2,1) reads {PixelHarness.Hex(beside)}"),
                () => Assert.True(topRight != stripKey,
                    $"the strip reaches the card's corner: ({right},1) reads {PixelHarness.Hex(topRight)}"),
                // The command row paints the card's own fill, so a square corner there is the same colour as the
                // surface and only shows against the smoke outside the arc - still a square nub on a round card.
                () => Assert.True(bottom != fillKey,
                    $"the command row reaches the card's corner: (1,{height - 2}) reads {PixelHarness.Hex(bottom)}"),
                () => Assert.True(bottomBeside != fillKey,
                    $"the command row reaches the card's corner: (2,{height - 2}) reads {PixelHarness.Hex(bottomBeside)}"),
                () => Assert.True(bottomRight != fillKey,
                    $"the command row reaches the card's corner: ({right},{height - 2}) reads {PixelHarness.Hex(bottomRight)}"));
        });
    }

    /// <summary>
    /// Opens a dialog for real. The control hosts it in the window's own overlay layer, so nothing here places
    /// it; callers that mutate it must still close it, and a fact that clicks a button does not need to.
    /// </summary>
    private ContentDialog Shown(
        string? primary = "Publish",
        string? secondary = "Save draft",
        string? close = "Cancel",
        double? maxWidth = null)
    {
        PixelHarness.HostWindow();
        var dialog = new ContentDialog
        {
            Title = "Publish this draft?",
            Content = "Once it is public, readers see it as of now.",
        };
        // A null text is load-bearing: the command row's cells key on PrimaryButtonText={x:Null}
        // (Styles/ContentDialog.jalxaml), which is how a button leaves the row. The CLR properties are annotated
        // non-null, so these nulls go straight to the dependency properties they write.
        dialog.SetValue(ContentDialog.PrimaryButtonTextProperty, primary);
        dialog.SetValue(ContentDialog.SecondaryButtonTextProperty, secondary);
        dialog.SetValue(ContentDialog.CloseButtonTextProperty, close);
        if (maxWidth is not null)
        {
            dialog.MaxWidth = maxWidth.Value;
        }

        dialog.ShowAsync();
        PixelHarness.Settle();
        Assert.Equal(Visibility.Visible, dialog.Visibility);
        _opened.Add(dialog);
        return dialog;
    }

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name} in the built tree.");

    private static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();

    private static Brush Res(string key) => (Brush)Application.Current!.TryFindResource(key)!;

    private static Style Style() => FluentThemeManager.GetStyle("DefaultContentDialogStyle");

    private static ControlTemplate Template() =>
        (Style().Setters.Cast<object>().OfType<Setter>()
            .First(static setter => (setter.Property?.Name ?? setter.PropertyName) == "Template").Value as ControlTemplate)!;

    /// <summary>
    /// The Style instance the DefaultButton cells actually carry. Not the object GetStyle("AccentButtonStyle")
    /// returns - the reader seals each dictionary copy separately, so only the cell's own instance is a
    /// meaningful identity claim here.
    /// </summary>
    private static Style AccentStyleFromCell() =>
        TemplateCells().First(cell => cell.Condition == "DefaultButton=Primary")
            .Setters.First().Value as Style ?? throw new InvalidOperationException("The DefaultButton cell carries no style.");

    private static List<Cell> TemplateCells() => Template().Triggers.Cast<object>().Select(static trigger => trigger switch
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

    private static string Form(object? value) => value?.ToString() ?? "null";

    private sealed record Cell(string Condition, Setter[] Setters);
}
