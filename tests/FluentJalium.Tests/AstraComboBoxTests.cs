using System.Reflection;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// ComboBox, the fourth control of the selection batch. Upstream gives it 103 rows per theme scope - the
/// widest surface in the batch - and Jalium gives it eleven properties, no header, no IsPressed, and a
/// framework that paints four things itself: the editable box's visibility, the selection text, the popup
/// width and the chevron's own geometry (docs/astra/audits/combobox.md). What the template does own is
/// colour, and every cell of it is checked the way the Slider batch established: the condition resolved to
/// a real property, and the value the control ends up holding is the palette object the row names.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraComboBoxTests
{
    private static readonly Color SurfaceSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color AccentSentinel = Color.FromRgb(0x00, 0xFF, 0x80);
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraComboBoxTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    /// <summary>
    /// Row-by-row transcription promise for all 58 alias brushes. Upstream's own target stands in the right
    /// column except on the rows the audit marks as value deviations.
    /// </summary>
    [Theory]
    [InlineData("ComboBoxItemForeground", "TextFillColorPrimaryBrush")]
    [InlineData("ComboBoxItemForegroundPressed", "TextFillColorSecondaryBrush")]
    [InlineData("ComboBoxItemForegroundPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("ComboBoxItemForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("ComboBoxItemForegroundSelected", "TextFillColorPrimaryBrush")]
    [InlineData("ComboBoxItemForegroundSelectedPressed", "TextFillColorSecondaryBrush")]
    [InlineData("ComboBoxItemForegroundSelectedPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("ComboBoxItemForegroundSelectedDisabled", "TextFillColorDisabledBrush")]
    [InlineData("ComboBoxItemBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("ComboBoxItemBackgroundPressed", "SubtleFillColorTertiaryBrush")]
    [InlineData("ComboBoxItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("ComboBoxItemBackgroundDisabled", "SubtleFillColorDisabledBrush")]
    [InlineData("ComboBoxItemBackgroundSelected", "SubtleFillColorSecondaryBrush")]
    [InlineData("ComboBoxItemBackgroundSelectedPressed", "SubtleFillColorSecondaryBrush")]
    [InlineData("ComboBoxItemBackgroundSelectedPointerOver", "SubtleFillColorTertiaryBrush")]
    [InlineData("ComboBoxItemBackgroundSelectedDisabled", "SubtleFillColorSecondaryBrush")]
    [InlineData("ComboBoxItemBorderBrush", "SubtleFillColorTransparentBrush")]
    [InlineData("ComboBoxItemBorderBrushPressed", "SubtleFillColorTertiaryBrush")]
    [InlineData("ComboBoxItemBorderBrushPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("ComboBoxItemBorderBrushDisabled", "SubtleFillColorDisabledBrush")]
    [InlineData("ComboBoxItemBorderBrushSelected", "SubtleFillColorSecondaryBrush")]
    [InlineData("ComboBoxItemBorderBrushSelectedPressed", "SubtleFillColorSecondaryBrush")]
    [InlineData("ComboBoxItemBorderBrushSelectedPointerOver", "SubtleFillColorTertiaryBrush")]
    [InlineData("ComboBoxItemBorderBrushSelectedDisabled", "SubtleFillColorSecondaryBrush")]
    [InlineData("ComboBoxBackground", "ControlFillColorDefaultBrush")]
    [InlineData("ComboBoxBackgroundPointerOver", "ControlFillColorSecondaryBrush")]
    [InlineData("ComboBoxBackgroundPressed", "ControlFillColorTertiaryBrush")]
    [InlineData("ComboBoxBackgroundDisabled", "ControlFillColorDisabledBrush")]
    [InlineData("ComboBoxBackgroundFocused", "ControlFillColorDefaultBrush")]
    [InlineData("ComboBoxBackgroundBorderBrushFocused", "FocusStrokeColorOuterBrush")]
    [InlineData("ComboBoxForeground", "TextFillColorPrimaryBrush")]
    [InlineData("ComboBoxForegroundPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("ComboBoxForegroundPressed", "TextFillColorSecondaryBrush")]
    [InlineData("ComboBoxForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("ComboBoxForegroundFocused", "TextFillColorPrimaryBrush")]
    [InlineData("ComboBoxForegroundFocusedPressed", "TextFillColorPrimaryBrush")]
    [InlineData("ComboBoxPlaceHolderForeground", "TextFillColorSecondaryBrush")]
    [InlineData("ComboBoxPlaceHolderForegroundPointerOver", "TextFillColorSecondaryBrush")]
    [InlineData("ComboBoxPlaceHolderForegroundPressed", "TextFillColorTertiaryBrush")]
    [InlineData("ComboBoxPlaceHolderForegroundFocused", "TextFillColorSecondaryBrush")]
    [InlineData("ComboBoxPlaceHolderForegroundFocusedPressed", "TextFillColorSecondaryBrush")]
    [InlineData("ComboBoxBorderBrush", "ControlStrokeColorDefaultBrush")]
    [InlineData("ComboBoxBorderBrushPointerOver", "ControlStrokeColorDefaultBrush")]
    [InlineData("ComboBoxBorderBrushPressed", "ControlStrokeColorDefaultBrush")]
    [InlineData("ComboBoxBorderBrushDisabled", "ControlStrokeColorDefaultBrush")]
    [InlineData("ComboBoxDropDownGlyphForeground", "TextFillColorSecondaryBrush")]
    [InlineData("ComboBoxDropDownGlyphForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("ComboBoxDropDownGlyphForegroundFocused", "TextFillColorSecondaryBrush")]
    [InlineData("ComboBoxDropDownGlyphForegroundFocusedPressed", "TextFillColorSecondaryBrush")]
    [InlineData("ComboBoxDropDownForeground", "TextFillColorPrimaryBrush")]
    [InlineData("ComboBoxDropDownBackground", "AcrylicInAppFillColorDefaultBrush")]
    [InlineData("ComboBoxDropDownBorderBrush", "SurfaceStrokeColorFlyoutBrush")]
    [InlineData("ComboBoxDropDownBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("ComboBoxDropDownBackgroundPointerPressed", "SubtleFillColorTertiaryBrush")]
    [InlineData("ComboBoxFocusedDropDownBackgroundPointerOver", "ControlFillColorTertiaryBrush")]
    [InlineData("ComboBoxFocusedDropDownBackgroundPointerPressed", "ControlAltFillColorQuarternaryBrush")]
    [InlineData("ComboBoxEditableDropDownGlyphForeground", "TextFillColorSecondaryBrush")]
    [InlineData("ComboBoxItemPillFillBrush", "AccentFillColorDefaultBrush")]
    public void An_upstream_combo_row_hands_back_the_palette_instance_it_aliases(string alias, string paletteKey)
    {
        _fixture.Run(() => Assert.Same(FluentThemeManager.GetBrush(paletteKey), Res(alias)));
    }

    /// <summary>
    /// The metric rows, including the two things this dictionary corrected: ComboBoxPadding is upstream's
    /// own key (an earlier note called it invented, wrongly) but it carried the Button's value, and a
    /// combo's padding has zero on the right because the 38-DIP chevron column is a sibling, not padding.
    /// </summary>
    [Fact]
    public void The_combo_metric_rows_are_upstreams()
    {
        _fixture.Run(() => Assert.Multiple(
            () => Assert.Equal(new Thickness(12, 5, 0, 7), Res("ComboBoxPadding")),
            () => Assert.Equal(new Thickness(11, 5, 38, 6), Res("ComboBoxEditableTextPadding")),
            () => Assert.Equal(new Thickness(11, 5, 11, 7), Res("ComboBoxItemThemePadding")),
            () => Assert.Equal(new Thickness(1), Res("ComboBoxBorderThemeThickness")),
            () => Assert.Equal(new Thickness(1), Res("ComboBoxDropdownBorderThickness")),
            () => Assert.Equal(new Thickness(2), Res("ComboBoxBackgroundBorderThicknessFocused")),
            () => Assert.Equal(new Thickness(0, 4), Res("ComboBoxDropdownContentMargin")),
            () => Assert.Equal(new Thickness(0), Res("ComboBoxDropdownBorderPadding")),
            () => Assert.Equal(new CornerRadius(7), Res("ComboBoxHiglightBorderCornerRadius")),
            () => Assert.Equal(new CornerRadius(4), Res("ComboBoxDropDownButtonBackgroundCornerRadius")),
            () => Assert.Equal(new CornerRadius(3), Res("ComboBoxItemCornerRadius")),
            () => Assert.Equal(new CornerRadius(1.5), Res("ComboBoxItemPillCornerRadius"))));
    }

    /// <summary>
    /// What is left out is a claim about the runtime, so each omission is pinned to a reading: no header to
    /// paint, no light-dismiss brush slot on the Popup, no IsPressed on the item. The dead row is pinned by
    /// counting the reference file itself.
    /// </summary>
    [Fact]
    public void The_rows_left_out_are_left_out_because_no_state_can_reach_them()
    {
        _fixture.Run(() =>
        {
            static bool Has(Type type, string property) =>
                type.GetProperty(property, BindingFlags.Public | BindingFlags.Instance) is not null;

            Assert.False(Has(typeof(ComboBox), "Header"));
            Assert.False(Has(typeof(ComboBoxItem), "IsPressed"));
            Assert.True(Has(typeof(ComboBoxItem), "IsHighlighted"));
            Assert.True(Has(typeof(ComboBox), "PlaceholderText"));
            Assert.True(Has(typeof(ComboBox), "IsEditable"));

            // The runtime's Popup can light-dismiss but carries no overlay brush, so the upstream row that
            // paints that overlay has nowhere to be set.
            Assert.True(Has(typeof(Popup), "IsLightDismissEnabled"));
            Assert.False(Has(typeof(Popup), "LightDismissOverlayBackground"));

            // ComboBoxBackgroundBorderBrushUnfocused is read by nothing - not even upstream: three
            // declarations and no other occurrence in the whole 814-line reference file.
            const string reference = @"C:\git\Jalium\microsoft-ui-xaml\controls\dev\ComboBox\ComboBox_themeresources.xaml";
            if (File.Exists(reference))
            {
                Assert.Equal(3, File.ReadAllText(reference).Split("ComboBoxBackgroundBorderBrushUnfocused").Length - 1);
            }
        });
    }

    /// <summary>
    /// The state map, read off the hydrated templates: every cell's condition with its resolved property,
    /// in document order, because order is what decides which cell wins. The keyed-template defect left
    /// these properties null, and a null condition is an inert cell no pixel test would notice.
    /// </summary>
    [Fact]
    public void Both_combo_templates_carry_one_cell_per_upstream_state()
    {
        _fixture.Run(() =>
        {
            var combo = Cells(ComboTemplate());
            Assert.Equal(
                new[]
                {
                    "SelectedIndex=-1", "IsMouseOver=True", "SelectedIndex=-1+IsMouseOver=True", "IsEditable=True+IsMouseOver=True",
                    "IsMouseCaptureWithin=True", "SelectedIndex=-1+IsMouseCaptureWithin=True",
                    "IsEditable=True+IsMouseCaptureWithin=True", "IsKeyboardFocused=True",
                    "SelectedIndex=-1+IsKeyboardFocused=True", "IsKeyboardFocused=True+IsMouseOver=True",
                    "IsKeyboardFocused=True+IsMouseCaptureWithin=True",
                    "SelectedIndex=-1+IsKeyboardFocused=True+IsMouseCaptureWithin=True", "IsEditable=True",
                    "IsEnabled=False",                 },
                combo.Select(static cell => cell.Condition));

            AssertCell(combo, "SelectedIndex=-1", ("PART_SelectionPresenter", "Foreground", "ComboBoxPlaceHolderForeground"));
            AssertCell(combo, "IsMouseOver=True",
                ("PART_MainBorder", "Background", "ComboBoxBackgroundPointerOver"),
                ("PART_MainBorder", "BorderBrush", "ComboBoxBorderBrushPointerOver"),
                ("self", "Foreground", "ComboBoxForegroundPointerOver"));
            AssertCell(combo, "SelectedIndex=-1+IsMouseOver=True",
                ("PART_SelectionPresenter", "Foreground", "ComboBoxPlaceHolderForegroundPointerOver"));
            AssertCell(combo, "IsEditable=True+IsMouseOver=True",
                ("DropDownOverlay", "Background", "ComboBoxDropDownBackgroundPointerOver"));
            AssertCell(combo, "IsMouseCaptureWithin=True",
                ("PART_MainBorder", "Background", "ComboBoxBackgroundPressed"),
                ("PART_MainBorder", "BorderBrush", "ComboBoxBorderBrushPressed"),
                ("self", "Foreground", "ComboBoxForegroundPressed"));
            AssertCell(combo, "SelectedIndex=-1+IsMouseCaptureWithin=True",
                ("PART_SelectionPresenter", "Foreground", "ComboBoxPlaceHolderForegroundPressed"));
            AssertCell(combo, "IsEditable=True+IsMouseCaptureWithin=True",
                ("DropDownOverlay", "Background", "ComboBoxDropDownBackgroundPointerPressed"));
            AssertCell(combo, "IsKeyboardFocused=True",
                ("HighlightBackground", "Opacity", null),
                ("self", "Foreground", "ComboBoxForegroundFocused"),
                ("PART_ToggleButton", "Foreground", "ComboBoxDropDownGlyphForegroundFocused"));
            AssertCell(combo, "SelectedIndex=-1+IsKeyboardFocused=True",
                ("PART_SelectionPresenter", "Foreground", "ComboBoxPlaceHolderForegroundFocused"));
            AssertCell(combo, "IsKeyboardFocused=True+IsMouseOver=True",
                ("DropDownOverlay", "Background", "ComboBoxFocusedDropDownBackgroundPointerOver"));
            AssertCell(combo, "IsKeyboardFocused=True+IsMouseCaptureWithin=True",
                ("self", "Foreground", "ComboBoxForegroundFocusedPressed"),
                ("PART_ToggleButton", "Foreground", "ComboBoxDropDownGlyphForegroundFocusedPressed"),
                ("DropDownOverlay", "Background", "ComboBoxFocusedDropDownBackgroundPointerPressed"));
            AssertCell(combo, "SelectedIndex=-1+IsKeyboardFocused=True+IsMouseCaptureWithin=True",
                ("PART_SelectionPresenter", "Foreground", "ComboBoxPlaceHolderForegroundFocusedPressed"));
            AssertCell(combo, "IsEditable=True",
                ("DropDownOverlay", "Visibility", null),
                ("PART_ToggleButton", "Foreground", "ComboBoxEditableDropDownGlyphForeground"));
            AssertCell(combo, "IsEnabled=False",
                ("PART_MainBorder", "Background", "ComboBoxBackgroundDisabled"),
                ("PART_MainBorder", "BorderBrush", "ComboBoxBorderBrushDisabled"),
                ("self", "Foreground", "ComboBoxForegroundDisabled"),
                ("PART_ToggleButton", "Foreground", "ComboBoxDropDownGlyphForegroundDisabled"),
                ("HighlightBackground", "Opacity", null));

            // There is no disabled-placeholder cell: see An_editable_combo_loads_the_chip_and_a_disabled_one_beats_every_cell.
            Assert.DoesNotContain(combo, candidate => candidate.Condition.Contains("IsEnabled=False+SelectedIndex"));

            var item = Cells(ItemTemplate());
            Assert.Equal(
                new[]
                {
                    "IsMouseOver=True", "IsHighlighted=True", "IsMouseCaptureWithin=True", "IsSelected=True",
                    "IsSelected=True+IsMouseOver=True", "IsSelected=True+IsHighlighted=True",
                    "IsSelected=True+IsMouseCaptureWithin=True", "IsEnabled=False",
                    "IsEnabled=False+IsSelected=True",
                },
                item.Select(static cell => cell.Condition));

            AssertCell(item, "IsMouseOver=True",
                ("LayoutRoot", "Background", "ComboBoxItemBackgroundPointerOver"),
                ("LayoutRoot", "BorderBrush", "ComboBoxItemBorderBrushPointerOver"),
                ("ContentPresenter", "Foreground", "ComboBoxItemForegroundPointerOver"));
            AssertCell(item, "IsHighlighted=True",
                ("LayoutRoot", "Background", "ComboBoxItemBackgroundPointerOver"),
                ("ContentPresenter", "Foreground", "ComboBoxItemForegroundPointerOver"));
            AssertCell(item, "IsMouseCaptureWithin=True",
                ("LayoutRoot", "Background", "ComboBoxItemBackgroundPressed"),
                ("LayoutRoot", "BorderBrush", "ComboBoxItemBorderBrushPressed"),
                ("ContentPresenter", "Foreground", "ComboBoxItemForegroundPressed"));
            AssertCell(item, "IsSelected=True",
                ("LayoutRoot", "Background", "ComboBoxItemBackgroundSelected"),
                ("LayoutRoot", "BorderBrush", "ComboBoxItemBorderBrushSelected"),
                ("ContentPresenter", "Foreground", "ComboBoxItemForegroundSelected"),
                ("Pill", "Opacity", null));
            AssertCell(item, "IsSelected=True+IsMouseOver=True",
                ("LayoutRoot", "Background", "ComboBoxItemBackgroundSelectedPointerOver"),
                ("ContentPresenter", "Foreground", "ComboBoxItemForegroundSelectedPointerOver"));
            AssertCell(item, "IsSelected=True+IsHighlighted=True",
                ("LayoutRoot", "Background", "ComboBoxItemBackgroundSelectedPointerOver"),
                ("ContentPresenter", "Foreground", "ComboBoxItemForegroundSelectedPointerOver"));
            AssertCell(item, "IsSelected=True+IsMouseCaptureWithin=True",
                ("LayoutRoot", "Background", "ComboBoxItemBackgroundSelectedPressed"),
                ("ContentPresenter", "Foreground", "ComboBoxItemForegroundSelectedPressed"));
            AssertCell(item, "IsEnabled=False",
                ("LayoutRoot", "Background", "ComboBoxItemBackgroundDisabled"),
                ("LayoutRoot", "BorderBrush", "ComboBoxItemBorderBrushDisabled"),
                ("ContentPresenter", "Foreground", "ComboBoxItemForegroundDisabled"));
            AssertCell(item, "IsEnabled=False+IsSelected=True",
                ("LayoutRoot", "Background", "ComboBoxItemBackgroundSelectedDisabled"),
                ("LayoutRoot", "BorderBrush", "ComboBoxItemBorderBrushSelectedDisabled"),
                ("ContentPresenter", "Foreground", "ComboBoxItemForegroundSelectedDisabled"));
        });
    }

    /// <summary>
    /// The style rows read twice: the setter names the key, and the control the implicit style produced
    /// holds the palette instance behind it. The part group is the interesting half - a ContentPresenter
    /// with no local Foreground inherits the control's, which is what leaves the placeholder cells able to
    /// beat it, and the chevron colour rides the toggle's Foreground so a style setter (not a local value)
    /// has to supply the resting row.
    /// </summary>
    [Fact]
    public void The_combo_style_reads_upstreams_surface_rows()
    {
        _fixture.Run(() =>
        {
            var style = FluentThemeManager.GetStyle("DefaultComboBoxStyle");
            Assert.Multiple(
                () => Assert.Equal("ComboBoxPadding", KeyOf(Setter(style, "Padding"))),
                () => Assert.Equal("ComboBoxBackground", KeyOf(Setter(style, "Background"))),
                () => Assert.Equal("ComboBoxForeground", KeyOf(Setter(style, "Foreground"))),
                () => Assert.Equal("ComboBoxBorderBrush", KeyOf(Setter(style, "BorderBrush"))),
                () => Assert.Equal("ComboBoxBorderThemeThickness", KeyOf(Setter(style, "BorderThickness"))),
                () => Assert.Equal("ControlCornerRadius", KeyOf(Setter(style, "CornerRadius"))),
                () => Assert.Equal(504d, Convert.ToDouble(Setter(style, "MaxDropDownHeight").Value!)));

            var combo = Mount(new ComboBox { ItemsSource = new[] { "one", "two" }, SelectedIndex = 1 });
            Assert.Multiple(
                () => Assert.Same(Res("ComboBoxBackground"), combo.Background),
                () => Assert.Same(Res("ComboBoxForeground"), combo.Foreground),
                () => Assert.Same(Res("ComboBoxBorderBrush"), combo.BorderBrush),
                () => Assert.Equal(new Thickness(12, 5, 0, 7), combo.Padding),
                () => Assert.Equal(new Thickness(1), combo.BorderThickness),
                () => Assert.Equal(new CornerRadius(4), combo.CornerRadius),
                () => Assert.Equal(504d, combo.MaxDropDownHeight),
                () => Assert.Equal(32d, combo.MinHeight),
                () => Assert.Same(Res("ComboBoxBackground"), Get(Part(combo, "PART_MainBorder"), "Background")),
                () => Assert.Same(Res("ComboBoxDropDownGlyphForeground"), Get(Part(combo, "PART_ToggleButton"), "Foreground")),
                () => Assert.Same(Res("ComboBoxForeground"),
                    Part(combo, "PART_SelectionPresenter").GetValue(CellProperty(ComboTemplate(), "SelectedIndex=-1", "PART_SelectionPresenter", "Foreground"))),
                () => Assert.Equal(new Thickness(11, 5, 38, 6), Get(Part(combo, "PART_EditableTextBox"), "Padding")),
                () => Assert.Equal(new CornerRadius(7), Get(Part(combo, "HighlightBackground"), "CornerRadius")),
                () => Assert.Equal(new Thickness(2), Get(Part(combo, "HighlightBackground"), "BorderThickness")),
                () => Assert.Same(Res("ComboBoxBackgroundFocused"), Get(Part(combo, "HighlightBackground"), "Background")),
                () => Assert.Same(Res("ComboBoxBackgroundBorderBrushFocused"), Get(Part(combo, "HighlightBackground"), "BorderBrush")),
                () => Assert.Equal(new CornerRadius(4), Get(Part(combo, "DropDownOverlay"), "CornerRadius")));
        });
    }

    /// <summary>
    /// The four things the framework owns, read back so a later template edit that fights them fails here
    /// instead of showing up as a subtle visual regression somewhere else.
    /// </summary>
    [Fact]
    public void The_framework_keeps_what_it_took_ownership_of()
    {
        _fixture.Run(() =>
        {
            var combo = Mount(new ComboBox { ItemsSource = new[] { "one", "two", "three" }, SelectedIndex = 1 });
            var edit = (TextBox)Part(combo, "PART_EditableTextBox");
            var presenter = Part(combo, "PART_SelectionPresenter");
            var toggle = (ToggleButton)Part(combo, "PART_ToggleButton");
            var glyph = Part(toggle, "DropDownGlyph");
            var resting = Data(glyph);

            Assert.Multiple(
                () => Assert.Equal(Visibility.Collapsed, edit.Visibility),
                () => Assert.Equal(Visibility.Visible, presenter.Visibility),
                () => Assert.False(combo.IsDropDownOpen),
                () => Assert.False(toggle.IsChecked.GetValueOrDefault()));

            combo.IsEditable = true;
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.Equal(Visibility.Visible, edit.Visibility),
                () => Assert.Equal(Visibility.Collapsed, presenter.Visibility),
                () => Assert.Equal("two", edit.Text));

            combo.IsEditable = false;
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.Equal(Visibility.Collapsed, edit.Visibility),
                () => Assert.Equal(Visibility.Visible, presenter.Visibility));

            combo.IsDropDownOpen = true;
            PixelHarness.Settle(30);
            Assert.Multiple(
                () => Assert.True(toggle.IsChecked.GetValueOrDefault(), "the toggle did not follow IsDropDownOpen"),
                () => Assert.NotEqual(resting, Data(glyph)),
                () => Assert.Equal(220d, ((Popup)Part(combo, "PART_Popup")).Width));

            combo.IsDropDownOpen = false;
            PixelHarness.Settle(30);
            Assert.Equal(resting, Data(glyph));
        });
    }

    /// <summary>
    /// The dropdown's own rows, read where the dropdown actually lives. Measured here: a closed combo has
    /// no PART_PopupBorder in its tree at all - the runtime moves the popup's child into the window overlay
    /// while it is open - so the claim about the flyout surface has to be taken from the host window, not
    /// from the control.
    /// </summary>
    [Fact]
    public void An_open_combo_grafts_its_dropdown_into_the_overlay_and_wears_its_rows()
    {
        _fixture.Run(() =>
        {
            var combo = Mount(new ComboBox { ItemsSource = new[] { "one", "two", "three" }, SelectedIndex = 1 });
            Assert.Null(PixelHarness.Named(combo, "PART_PopupBorder"));

            combo.IsDropDownOpen = true;
            PixelHarness.Settle(30);
            var overlay = PixelHarness.HostWindow();
            var popupBorder = PixelHarness.Named(overlay, "PART_PopupBorder")
                ?? throw new InvalidOperationException("The open dropdown never reached the overlay layer.");
            var scroller = PixelHarness.Named(overlay, "PART_ScrollViewer")
                ?? throw new InvalidOperationException("The dropdown has no scroll viewer to inherit.");

            Assert.Multiple(
                () => Assert.Same(Res("ComboBoxDropDownBackground"), Get(popupBorder, "Background")),
                () => Assert.Same(Res("ComboBoxDropDownBorderBrush"), Get(popupBorder, "BorderBrush")),
                () => Assert.Equal(new CornerRadius(8), Get(popupBorder, "CornerRadius")),
                () => Assert.Equal(new Thickness(1), Get(popupBorder, "BorderThickness")),
                () => Assert.Equal(new Thickness(0), Get(popupBorder, "Padding")),
                () => Assert.Same(Res("ComboBoxDropDownForeground"), Get(scroller, "Foreground")),
                () => Assert.Equal(504d, Get(scroller, "MaxHeight")),
                () => Assert.Equal(new Thickness(0, -0.5, 0, -1), Get(popupBorder, "Margin")),
                // The hole the running Gallery showed: the runtime sizes the popup's own window to the combo
                // but measures the surface inside it against infinity, so a short item list painted a narrow
                // card and left the rest of that window unpainted.
                () => Assert.Equal(combo.ActualWidth, popupBorder.MinWidth),
                () => Assert.Equal(combo.ActualWidth, popupBorder.ActualWidth));

            var presenter = PixelHarness.Descendant<ItemsPresenter>(popupBorder)
                ?? throw new InvalidOperationException("The dropdown has no items presenter to place.");
            var item = PixelHarness.Descendant<ComboBoxItem>(popupBorder)
                ?? throw new InvalidOperationException("The open dropdown carries no generated item.");
            Assert.Multiple(
                () => Assert.Equal(new Thickness(0, 4), Get(presenter, "Margin")),
                () => Assert.Same(Res("ComboBoxItemBackground"), Get(Part(item, "LayoutRoot"), "Background")),
                () => Assert.Equal(new Thickness(11, 5, 11, 7), item.Padding),
                // Upstream insets the whole row from the dropdown edge (ComboBox_themeresources.xaml:614,
                // Margin="5,2,5,2" on the template root), so the highlight stops 5 short of the border and the
                // 11 padding lives inside that. Read off the arranged surface, not off the markup.
                () => Assert.Equal(Math.Round(item.ActualWidth - 10), Math.Round(Part(item, "LayoutRoot").ActualWidth)),
                () => Assert.Equal(Math.Round(item.ActualHeight - 4), Math.Round(Part(item, "LayoutRoot").ActualHeight)));
        });
    }

    /// <summary>
    /// The placeholder cell, proven by behaviour rather than markup. The probe found that the framework
    /// paints the placeholder TextBlock it injects with the control's own foreground, so the only thing
    /// standing between "secondary grey" and "primary black" is whether a template cell can beat that
    /// inheritance - and whether it lets go again once something is selected.
    /// </summary>
    [Fact]
    public void An_unselected_combo_takes_the_placeholder_row_and_a_selected_one_lets_go()
    {
        _fixture.Run(() =>
        {
            var foreground = CellProperty(ComboTemplate(), "SelectedIndex=-1", "PART_SelectionPresenter", "Foreground");
            var combo = Mount(new ComboBox { ItemsSource = new[] { "one", "two" }, PlaceholderText = "Pick one" });
            var presenter = Part(combo, "PART_SelectionPresenter");

            // Attribution for the claim below: the condition's operand really is an unset selection, and
            // nothing has written a local value onto the presenter that a template cell could not beat.
            Assert.Multiple(
                () => Assert.Equal(-1, combo.SelectedIndex),
                () => Assert.Equal(string.Empty, combo.Text),
                // The framework renders the placeholder by putting the string into SelectionBoxItem, which
                // is why "no selection" cannot be conditioned on that property, and why an empty-string
                // condition had to be dropped: Value="" leaves a cell inert on this runtime.
                () => Assert.Equal("Pick one", combo.SelectionBoxItem),
                () => Assert.Equal(DependencyProperty.UnsetValue, presenter.ReadLocalValue(foreground)));

            Assert.Same(Res("ComboBoxPlaceHolderForeground"), presenter.GetValue(foreground));

            combo.SelectedIndex = 1;
            PixelHarness.Settle(20);
            Assert.Same(Res("ComboBoxForeground"), presenter.GetValue(foreground));

            combo.SelectedIndex = -1;
            PixelHarness.Settle(20);
            Assert.Same(Res("ComboBoxPlaceHolderForeground"), presenter.GetValue(foreground));
        });
    }

    /// <summary>
    /// The press and focus cells, driven through the properties they condition on. Capture is taken on the
    /// child the press would land on, so the cell fires through IsMouseCaptureWithin - the closest
    /// reachable stand-in for upstream's Pressed, and a real state transition rather than a read of the
    /// setter list.
    /// </summary>
    [Fact]
    public void A_pressed_and_focused_combo_reach_their_rows()
    {
        _fixture.Run(() =>
        {
            var combo = Mount(new ComboBox { ItemsSource = new[] { "one", "two" }, SelectedIndex = 1 });
            var surface = Part(combo, "PART_MainBorder");
            var toggle = Part(combo, "PART_ToggleButton");
            var halo = Part(combo, "HighlightBackground");

            Assert.Equal(0d, (double)Get(halo, "Opacity"));
            Assert.True(((UIElement)toggle).CaptureMouse());
            PixelHarness.Settle(20);
            Assert.True(combo.IsMouseCaptureWithin);
            Assert.Same(Res("ComboBoxBackgroundPressed"), Get(surface, "Background"));
            Assert.Same(Res("ComboBoxForegroundPressed"), combo.Foreground);
            ((UIElement)toggle).ReleaseMouseCapture();
            PixelHarness.Settle(20);
            Assert.Same(Res("ComboBoxBackground"), Get(surface, "Background"));

            Assert.True(combo.Focus(), "Focus() refused the combo in the host window.");
            PixelHarness.Settle(20);
            Assert.True(combo.IsKeyboardFocused);
            Assert.Equal(1d, (double)Get(halo, "Opacity"));
            Assert.Same(Res("ComboBoxForegroundFocused"), combo.Foreground);
            Assert.Same(Res("ComboBoxDropDownGlyphForegroundFocused"), Get(toggle, "Foreground"));
        });
    }

    /// <summary>
    /// The editable chip and its glyph row, plus the disabled cell that has to beat every one of them.
    /// </summary>
    [Fact]
    public void An_editable_combo_loads_the_chip_and_a_disabled_one_beats_every_cell()
    {
        _fixture.Run(() =>
        {
            var combo = Mount(new ComboBox { ItemsSource = new[] { "one", "two" }, SelectedIndex = 1 });
            var chip = Part(combo, "DropDownOverlay");
            var surface = Part(combo, "PART_MainBorder");
            var toggle = Part(combo, "PART_ToggleButton");
            Assert.Equal(Visibility.Collapsed, Get(chip, "Visibility"));

            combo.IsEditable = true;
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.Equal(Visibility.Visible, Get(chip, "Visibility")),
                () => Assert.Same(Res("ComboBoxEditableDropDownGlyphForeground"), Get(toggle, "Foreground")));

            combo.IsEnabled = false;
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.Same(Res("ComboBoxBackgroundDisabled"), Get(surface, "Background")),
                () => Assert.Same(Res("ComboBoxBorderBrushDisabled"), Get(surface, "BorderBrush")),
                () => Assert.Same(Res("ComboBoxDropDownGlyphForegroundDisabled"), Get(toggle, "Foreground")),
                () => Assert.Equal(0d, (double)Get(Part(combo, "HighlightBackground"), "Opacity")));

            // The third instance of a bill this theme keeps being handed: disabling makes the framework set
            // its OWN Foreground on the control (#FFAEAEB2, a local value), which outranks both the style
            // setter and the disabled cell, so ComboBoxForegroundDisabled reaches the parts that read it
            // directly and not the text of a mounted combo. Same shape as the mounted TextBox.Foreground
            // and the resting PasswordBox.Background (audits/textbox-passwordbox.md).
            Assert.NotSame(Res("ComboBoxForegroundDisabled"), combo.Foreground);
            var presenterForeground = CellProperty(ComboTemplate(), "SelectedIndex=-1", "PART_SelectionPresenter", "Foreground");

            // The dropped row, measured rather than assumed: the presenter carries no local value of its
            // own, so what it shows while disabled is the control's framework-written grey reaching it by
            // inheritance - and that inheritance outranks nothing, while our cell would have to beat the
            // control's local value to matter. ComboBoxPlaceHolderForegroundDisabled has no lever.
            var unselected = Mount(new ComboBox { ItemsSource = new[] { "one", "two" } });
            unselected.IsEnabled = false;
            PixelHarness.Settle(20);
            Assert.Equal(DependencyProperty.UnsetValue,
                Part(unselected, "PART_SelectionPresenter").ReadLocalValue(presenterForeground));
            Assert.NotSame(Res("ComboBoxForegroundDisabled"),
                Part(unselected, "PART_SelectionPresenter").GetValue(presenterForeground));
        });
    }

    /// <summary>
    /// An item's pill is the only accent in a combo's tree, so this is where the accent row is checked as a
    /// state rather than as a brush: hidden at rest, up when the item is the selection, and the selected
    /// disabled pair still winning when the whole list is switched off.
    /// </summary>
    [Fact]
    public void An_item_shows_the_pill_only_when_it_is_selected()
    {
        _fixture.Run(() =>
        {
            var itemStyle = FluentThemeManager.GetStyle("DefaultComboBoxItemStyle");
            Assert.Multiple(
                () => Assert.Equal("ComboBoxItemForeground", KeyOf(Setter(itemStyle, "Foreground"))),
                () => Assert.Equal("ComboBoxItemBackground", KeyOf(Setter(itemStyle, "Background"))),
                () => Assert.Equal("ComboBoxItemBorderBrush", KeyOf(Setter(itemStyle, "BorderBrush"))),
                () => Assert.Equal("ComboBoxItemThemePadding", KeyOf(Setter(itemStyle, "Padding"))),
                () => Assert.Equal("ComboBoxItemCornerRadius", KeyOf(Setter(itemStyle, "CornerRadius"))));

            var item = new ComboBoxItem { Content = "two" };
            item.Width = 200;
            item.Height = 32;
            PixelHarness.Build(item, 200, 32);
            var pill = Part(item, "Pill");
            var surface = Part(item, "LayoutRoot");
            var foreground = CellProperty(ItemTemplate(), "IsMouseOver=True", "ContentPresenter", "Foreground");
            var text = Part(item, "ContentPresenter");
            Assert.Multiple(
                () => Assert.Equal(0d, (double)Get(pill, "Opacity")),
                () => Assert.Same(Res("ComboBoxItemBackground"), Get(surface, "Background")),
                () => Assert.Same(Res("ComboBoxItemForeground"), text.GetValue(foreground)),
                () => Assert.Same(Res("ComboBoxItemPillFillBrush"), Get(pill, "Background")),
                () => Assert.Equal(3d, Convert.ToDouble(Get(pill, "Width"))),
                () => Assert.Equal(16d, Convert.ToDouble(Get(pill, "Height"))));

            item.IsSelected = true;
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.Equal(1d, (double)Get(pill, "Opacity")),
                () => Assert.Same(Res("ComboBoxItemBackgroundSelected"), Get(surface, "Background")),
                () => Assert.Equal(new Thickness(11, 5, 11, 7), item.Padding),
                () => Assert.Equal(new CornerRadius(3), item.CornerRadius));

            item.IsEnabled = false;
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.Same(Res("ComboBoxItemBackgroundSelectedDisabled"), Get(surface, "Background")),
                () => Assert.Same(Res("ComboBoxItemForegroundSelectedDisabled"), text.GetValue(foreground)));
        });
    }

    /// <summary>
    /// Pixels, resting, with two palette rows dyed: the surface row has to reach the capture and the brand
    /// green this repository shipped before Astra must not appear at any count.
    /// </summary>
    [Fact]
    public void A_resting_combo_paints_its_rows_and_nothing_invented()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", SurfaceSentinel);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", AccentSentinel);
            try
            {
                var sample = PixelHarness.Render(
                    new ComboBox { ItemsSource = new[] { "one", "two" }, SelectedIndex = 1 }, 220, 32);
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");
                Assert.True(sample.Count(SurfaceSentinel) > 3_000,
                    $"surface row did not reach the pixels; top={sample.Top(8)}");
                Assert.Equal(0, sample.Count(BrandEmerald));
                Assert.Equal(0, sample.Count(AccentSentinel));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", null);
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    /// <summary>
    /// The same scene under Light and Dark has to differ, which is the part of the claim the alias layer
    /// exists for: a row that froze at parse time would give two identical pictures.
    /// </summary>
    [Fact]
    public void The_combo_surface_changes_between_light_and_dark()
    {
        _fixture.Run(() =>
        {
            var light = PixelHarness.Render(
                new ComboBox { ItemsSource = new[] { "one", "two" }, SelectedIndex = 1 }, 220, 32);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            try
            {
                var dark = PixelHarness.Render(
                    new ComboBox { ItemsSource = new[] { "one", "two" }, SelectedIndex = 1 }, 220, 32);
                Assert.True(light.Stable && dark.Stable, $"light={light.Top(4)} dark={dark.Top(4)}");
                Assert.NotEqual(light.Top(4), dark.Top(4));
                Assert.True(dark.DistinctColors > 1, $"dark capture came back flat: {dark.Top(6)}");
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    /// <summary>
    /// The shape this batch was about: every value a combo cell writes is one of the transcribed rows, or a
    /// literal for a property that has no row at all (an opacity, a visibility). The previous template wrote
    /// palette brushes and the flyout rows directly, which is exactly how a theme override of
    /// ComboBoxBackground can be named correctly and still be ignored.
    /// </summary>
    [Fact]
    public void Every_combo_cell_names_a_row_and_never_a_palette_brush()
    {
        _fixture.Run(() =>
        {
            var offenders = new List<string>();
            foreach (var cell in Cells(ComboTemplate()).Concat(Cells(ItemTemplate())))
            {
                foreach (var setter in cell.Setters)
                {
                    var key = KeyOf(setter);
                    var property = setter.Property?.Name ?? setter.PropertyName ?? "unknown";
                    if (key is null)
                    {
                        if (property is not ("Opacity" or "Visibility")) offenders.Add($"{cell.Condition}: {property}={setter.Value}");
                    }
                    else if (!key.StartsWith("ComboBox", StringComparison.Ordinal))
                    {
                        offenders.Add($"{cell.Condition}: {property}->{key}");
                    }
                }
            }

            Assert.Empty(offenders);
        });
    }

    // ---- helpers -----------------------------------------------------------------------------------

    /// <summary>One hydrated cell: its condition as written in markup, and the setters it carries.</summary>
    private sealed record Cell(string Condition, Setter[] Setters);

    private object? Res(string key) => _fixture.Application.TryFindResource(key);

    private ComboBox Mount(ComboBox combo)
    {
        combo.Width = 220;
        combo.Height = 32;
        PixelHarness.Build(combo, 220, 32);
        return combo;
    }

    private ControlTemplate ComboTemplate() => Template("DefaultComboBoxStyle");

    private ControlTemplate ItemTemplate() => Template("DefaultComboBoxItemStyle");

    private static ControlTemplate Template(string key) =>
        (FluentThemeManager.GetStyle(key).Setters.Cast<object>().OfType<Setter>()
            .First(static setter => (setter.Property?.Name ?? setter.PropertyName) == "Template").Value as ControlTemplate)!;

    private static List<Cell> Cells(ControlTemplate template) =>
        template.Triggers.Cast<object>().Select(static trigger => trigger switch
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

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name} in the built tree.");

    private static object Get(FrameworkElement part, string property)
    {
        var clr = part.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
        if (clr is not null) return clr.GetValue(part) ?? throw new InvalidOperationException($"{part.GetType().Name}.{property} is null.");
        var dp = DependencyProperty.FromName(part.GetType(), property)
            ?? throw new InvalidOperationException($"{part.GetType().Name} has no {property}.");
        return part.GetValue(dp) ?? throw new InvalidOperationException($"{part.GetType().Name}.{property} is null.");
    }

    /// <summary>
    /// The foreground a cell writes on a ContentPresenter is the attached text property, which has no CLR
    /// member to read, so the read-back takes the dependency property off the hydrated cell itself: the
    /// assertion then proves the part carries the very property the cell names.
    /// </summary>
    private static DependencyProperty CellProperty(ControlTemplate template, string condition, string part, string property)
    {
        var cell = Cells(template).Find(candidate => candidate.Condition == condition)
            ?? throw new InvalidOperationException($"No cell for {condition}.");
        var setter = cell.Setters.FirstOrDefault(candidate =>
                (candidate.TargetName ?? "self") == part && (candidate.Property?.Name ?? candidate.PropertyName) == property)
            ?? throw new InvalidOperationException($"{condition} carries no {part}.{property} setter.");
        return setter.Property!;
    }

    private static string Data(DependencyObject glyph) => Get((FrameworkElement)glyph, "Data")?.ToString() ?? "null";

    private static Setter Setter(Style style, string property) => style.Setters.Cast<object>().OfType<Setter>()
        .First(setter => (setter.Property?.Name ?? setter.PropertyName) == property);

    private static string? KeyOf(Setter setter) =>
        setter.Value?.GetType().GetProperty("ResourceKey")?.GetValue(setter.Value) as string;

    private static void AssertCell(List<Cell> cells, string condition, params (string Part, string Property, string? Key)[] wanted)
    {
        var cell = cells.Find(candidate => candidate.Condition == condition)
            ?? throw new InvalidOperationException($"No cell for {condition}; the template carries " +
                string.Join(" | ", cells.Select(static candidate => candidate.Condition)));

        foreach (var (part, name, key) in wanted)
        {
            var setter = cell.Setters.FirstOrDefault(candidate =>
                    (candidate.TargetName ?? "self") == part && (candidate.Property?.Name ?? candidate.PropertyName) == name)
                ?? throw new InvalidOperationException($"{condition}: no {part}.{name} setter; the cell carries " +
                    string.Join(", ", cell.Setters.Select(static candidate =>
                        $"{candidate.TargetName ?? "self"}.{candidate.Property?.Name ?? candidate.PropertyName}")));
            if (key is not null)
            {
                Assert.Equal(key, KeyOf(setter));
            }
        }
    }
}
