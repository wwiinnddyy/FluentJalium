using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Data;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// The text colour a state row is supposed to send to a label, read back off the label.
///
/// These cells were written against the upstream template shape, where the label of a CheckBox, a RadioButton or a
/// list row is a <c>ContentPresenter</c> a VisualState can paint. On this runtime <c>ContentPresenter</c> declares
/// no <c>Foreground</c> member at all (spike/TreeViewProbe mode K, docs/astra/adaptation/00 S1-e 8), so a setter
/// aimed at one has nothing to write: the dictionary loads, the build stays green, the cell never fires, and the
/// label sits in its resting colour whatever the control does. Fifty of those cells shipped across six families,
/// and the sweep found eighteen more of the same shape writing <c>BorderBrush</c> onto a <c>Grid</c>.
/// <c>AstraGateTests.State_cells_name_properties_the_template_parts_actually_have</c> is the structural gate that
/// now reads them all; this file is the effect side, because the gate proves the target exists while only a
/// read-back proves the colour arrives.
///
/// The route that works is the control or container itself: <c>Foreground</c> is inherited and the <c>TextBlock</c>
/// this runtime generates for string content picks it up - the measurement that made <c>Styles/TreeViews.jalxaml</c>
/// the first style written this way, and the shape the ComboBox already used for its own rows. Forty-three cells
/// were re-pointed at their templated parent, six MenuFlyoutItem cells went away because that style already writes
/// the same rows on the item and the icon is painted by inheritance, and eighteen CheckBox / RadioButton cells went
/// away because the Grid they name has no border to paint.
///
/// What the read-back then had to admit: on a disabled control this runtime stamps the generated text with a
/// Foreground of its own, as a local value on that element, and a local value outranks every cell - so the disabled
/// label colour is not ours to paint. Four facts below pin that reading instead of the row, and the two states this
/// file proves end to end are the ComboBox placeholder (Secondary against Primary, correct on both sides of the
/// selection for the first time) and the menu icon.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraForegroundRoutingTests : IDisposable
{
    /// <summary>
    /// What this runtime paints on the generated text of a disabled control: measured as a local value on the
    /// text element itself (#FFAEAEB2, with the control reading the same colour and no local value of its own),
    /// against <c>TextFillColorDisabledBrush</c> = #5C000000, which is what the published rows ask for. A local
    /// value outranks every cell this reader has, so the disabled label colour is the framework's, not ours, for
    /// as long as it stamps it. The four disabled facts below pin that reading rather than the token, and each one
    /// is the place to look if a later release stops stamping.
    /// </summary>
    private static readonly Color FrameworkDisabledText = Color.FromRgb(0xAE, 0xAE, 0xB2);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraForegroundRoutingTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    public void Dispose()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", null);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
        });
    }

    [Fact]
    public void A_contentPresenter_has_no_foreground_and_a_grid_no_border_to_write_cells_on()
    {
        // Why every fact below reads the generated text rather than the named part, and why the CheckBox and
        // RadioButton root rows were deleted instead of re-pointed. If a later Jalium release gives either type the
        // member, this fact says so and the cells can go back to upstream's shape.
        Assert.Null(typeof(ContentPresenter).GetProperty("Foreground"));
        Assert.Null(typeof(Grid).GetProperty("BorderBrush"));
        Assert.NotNull(typeof(TextBlock).GetProperty("Foreground"));
        Assert.NotNull(typeof(ContentControl).GetProperty("Foreground"));
    }

    [Fact]
    public void A_check_box_label_takes_the_resting_row_and_the_framework_owns_the_disabled_one()
    {
        _fixture.Run(() =>
        {
            var box = Mount(new CheckBox { Content = "Subscribe", IsChecked = false });
            var label = Text(box);
            AssertEqualColour("CheckBoxForegroundUnchecked", label);

            box.IsEnabled = false;
            PixelHarness.Settle(30);
            // Our row is #5C000000; the text reads the framework's stamp because the stamp is a local value on it.
            Assert.Equal(FrameworkDisabledText, ((SolidColorBrush)label.Foreground!).Color);
        });
    }

    [Fact]
    public void A_radio_button_label_takes_the_resting_row_and_the_framework_owns_the_disabled_one()
    {
        _fixture.Run(() =>
        {
            var button = Mount(new RadioButton { Content = "Weekly", IsChecked = true });
            var label = Text(button);
            AssertEqualColour("RadioButtonForeground", label);

            button.IsEnabled = false;
            PixelHarness.Settle(30);
            Assert.Equal(FrameworkDisabledText, ((SolidColorBrush)label.Foreground!).Color);
        });
    }

    [Fact]
    public void A_list_box_row_label_takes_the_resting_row_and_the_framework_owns_the_disabled_one()
    {
        _fixture.Run(() =>
        {
            var list = Mount(new ListBox { Width = 220, Height = 120 });
            list.Items.Add("Quarterly review");
            PixelHarness.Settle(30);
            var row = PixelHarness.Descendant<ListBoxItem>(list)
                ?? throw new InvalidOperationException("the list realised no container, so there is no row to read");
            var label = Text(row);
            AssertEqualColour("ListBoxItemForeground", label);

            row.IsEnabled = false;
            PixelHarness.Settle(30);
            Assert.Equal(FrameworkDisabledText, ((SolidColorBrush)label.Foreground!).Color);
        });
    }

    [Fact]
    public void A_disabled_combo_box_row_sends_its_label_row_into_the_label()
    {
        // The dropdown is closed again before the asserts, the same way the ComboBox suite does it: an open
        // dropdown leaves its grafted surface in the shared host window for the next test to trip over.
        _fixture.Run(() =>
        {
            var combo = Mount(new ComboBox { Width = 220, Height = 32 });
            combo.Items.Add("Monday");
            combo.IsDropDownOpen = true;
            PixelHarness.Settle(30);
            var popup = PixelHarness.Named(PixelHarness.HostWindow(), "PART_PopupBorder")
                ?? throw new InvalidOperationException("the open dropdown never reached the overlay layer");
            var row = PixelHarness.Descendant<ComboBoxItem>(popup)!;
            var label = Text(row);
            var before = ((SolidColorBrush)label.Foreground!).Color;

            row.IsEnabled = false;
            PixelHarness.Settle(30);
            var after = ((SolidColorBrush)label.Foreground!).Color;
            combo.IsDropDownOpen = false;
            PixelHarness.Settle(20);

            Assert.Multiple(
                () => Assert.Equal(ToColour("ComboBoxItemForeground"), before),
                () => Assert.Equal(FrameworkDisabledText, after));
        });
    }

    [Fact]
    public void An_unselected_combo_box_shows_the_placeholder_row_and_a_selected_one_its_own()
    {
        // Upstream keeps the placeholder a different brush from the selection (Secondary against Primary), so an
        // empty box is the one ComboBox state this file can tell apart without a pointer - and before this sweep it
        // was the one visible defect the sixty-eight cells left behind: the box painted its placeholder in the
        // selection colour. The text element is read again after the selection because the presenter regenerates
        // it; the placeholder's element is gone by then, and reading it would only prove the old value stuck.
        _fixture.Run(() =>
        {
            var combo = Mount(new ComboBox { Width = 220, Height = 32, PlaceholderText = "Pick a day" });
            combo.Items.Add("Monday");
            PixelHarness.Settle(30);
            var placeholder = PixelHarness.Descendant<TextBlock>(combo)
                ?? throw new InvalidOperationException("the empty box painted no text at all, so it has no placeholder to colour");
            Assert.Equal(ToColour("ComboBoxPlaceHolderForeground"), ((SolidColorBrush)placeholder.Foreground!).Color);

            combo.SelectedIndex = 0;
            PixelHarness.Settle(30);
            var selection = PixelHarness.Descendant<TextBlock>(combo)!;
            Assert.Equal(ToColour("ComboBoxForeground"), ((SolidColorBrush)selection.Foreground!).Color);
        });
    }

    [Fact]
    public void A_disabled_number_box_sends_the_disabled_row_into_its_header()
    {
        // The header needs a colour of its own, so the control-level route is not available for it: writing the
        // NumberBox's Foreground would recolour the editor too. The row carrier is therefore a ContentControl,
        // which does have a Foreground for the cell to aim at. Both ends are read: the carrier says the cell
        // fired, the text says the value reached the glyph.
        _fixture.Run(() =>
        {
            var box = Mount(new NumberBox { Header = "Quantity", Width = 200, Height = 62 });
            var carrier = (Control)(PixelHarness.Named(box, "HeaderContentPresenter")
                ?? throw new InvalidOperationException("the NumberBox template put no header carrier on it"));
            var header = HeaderText(box);
            var restCarrier = ColourOf(carrier.Foreground);
            var restText = ColourOf(header.Foreground);

            box.IsEnabled = false;
            PixelHarness.Settle(30);

            Assert.Multiple(
                // The resting header row is withheld (ThemeResources/TextBox.jalxaml says why), so the colour the
                // carrier and its glyph hold is the primary text token the box passes down by inheritance.
                () => Assert.Equal(ToColour("TextFillColorPrimaryBrush"), restCarrier),
                () => Assert.Equal(ToColour("TextFillColorPrimaryBrush"), restText));
            // Where the disabled header row stops: the cell now fires and lands on the header carrier (measured),
            // and it stops there - the glyph the carrier generates keeps the colour it inherited when it was built
            // and does not re-take the carrier's. Recorded as a Known Gap rather than claimed as a pixel.
            Assert.Equal(ToColour("TextControlHeaderForegroundDisabled"), ColourOf(carrier.Foreground));
            Assert.Equal(ToColour("TextFillColorPrimaryBrush"), ColourOf(HeaderText(box).Foreground));
        });
    }

    [Fact]
    public void A_disabled_menu_item_carries_its_row_into_the_icon_as_well_as_the_text()
    {
        // MenuFlyoutItem already wrote its state rows on the item itself, so text and icon both reach pixels by
        // inheritance and the six IconContent cells this sweep deleted were duplicates. This fact is the guard that
        // the deletion left the icon painted.
        _fixture.Run(() =>
        {
            var item = Mount(new MenuFlyoutItem { Text = "Rename", Icon = new TextBlock { Text = "*" } });
            var icon = PixelHarness.Named(item, "IconContent")
                ?? throw new InvalidOperationException("the item template put no IconContent on the row");
            var glyph = Text(icon);
            AssertEqualColour("MenuFlyoutItemForeground", glyph);

            item.IsEnabled = false;
            PixelHarness.Settle(30);
            AssertEqualColour("MenuFlyoutItemForegroundDisabled", glyph);
        });
    }

    // ---------- the owners the audit left unread ----------


    /// <summary>
    /// The six template owners whose foreground rows the audit had no subject row for
    /// (docs/astra/audits/foreground.md 7.1). Each leg names the element the shipped row actually points at, which
    /// is not one element for all of them: the flyout item family and the two grid containers carry the row on
    /// themselves and paint their label from that value, while the app bar toggle button's label is a named template
    /// part. Every one of these tokens is a transcribed alias row over the same palette brush.
    ///
    /// Two sentinel instruments were tried first and both are unavailable here, which is the finding worth keeping.
    /// (1) <c>OverrideBrush</c> cannot aim at an alias key at all - <c>GetBrush</c> reads the generated palette only,
    /// so it throws <c>KeyNotFoundException</c> for <c>DataGridRowForeground</c> and the other five. (2) Overriding
    /// the *palette* brush the alias points at leaves the carrier on the light colour, and so does flipping the
    /// variant after the element is mounted: measured, six for six, #E4000000 stays #E4000000 through both. That is
    /// the shown-window broadcast rule - a mounted-but-not-shown element keeps the brushes it resolved when it was
    /// built. So the falsifiable instrument is the one below: mount **under** the variant being read, and take the
    /// two variants as two legs. Withholding one shipped row is what proves the reading means something: with the
    /// <c>MenuBarItem</c> row removed the two legs go red and the item falls back to the ink it inherits from the
    /// harness host (#FF1D1D1F light, #FFF5F5F7 dark) - not to the property default, which is why the equality
    /// against the token is the assertion doing the work and the black check is only a sanity floor.
    /// </summary>
    [Theory]
    [InlineData("appbar-toggle", "AppBarToggleButtonForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Light)]
    [InlineData("appbar-toggle", "AppBarToggleButtonForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Dark)]
    [InlineData("data-grid-cell", "DataGridRowForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Light)]
    [InlineData("data-grid-cell", "DataGridRowForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Dark)]
    [InlineData("data-grid-header", "DataGridColumnHeaderForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Light)]
    [InlineData("data-grid-header", "DataGridColumnHeaderForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Dark)]
    [InlineData("menu-bar-item", "MenuBarItemForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Light)]
    [InlineData("menu-bar-item", "MenuBarItemForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Dark)]
    [InlineData("menu-flyout-sub-item", "MenuFlyoutSubItemForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Light)]
    [InlineData("menu-flyout-sub-item", "MenuFlyoutSubItemForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Dark)]
    [InlineData("toggle-menu-flyout-item", "MenuFlyoutItemForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Light)]
    [InlineData("toggle-menu-flyout-item", "MenuFlyoutItemForeground", "TextFillColorPrimaryBrush", FluentThemeVariant.Dark)]
    public void An_owners_resting_foreground_row_lands_on_the_carrier_that_paints_its_text(string owner, string key, string aliases, FluentThemeVariant variant)
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(variant);
            try
            {
                var carrier = CarrierOf(owner);
                var landed = ForegroundOf(carrier);
                var palette = Assert.IsType<SolidColorBrush>(FluentThemeManager.GetBrush(aliases));

                Assert.Multiple(
                    // The token must still be the palette brush itself: a transcribed alias row, not a copied colour.
                    () => Assert.True(ReferenceEquals(palette, Application.Current!.TryFindResource(key)),
                        $"{key} does not resolve to the {aliases} instance it is transcribed as aliasing."),
                    () => Assert.Equal(ToColour(aliases), landed),
                    () => Assert.NotEqual(Colors.Black, landed));
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    /// <summary>
    /// The checked label row of an <c>AppBarToggleButton</c>: the one state of these seven owners that is reachable
    /// without a pointer, and the one that a same-colour reading could never tell apart, because the resting and
    /// checked tokens are different colours by design.
    /// </summary>
    [Fact]
    public void The_checked_row_of_an_app_bar_toggle_button_moves_its_label()
    {
        _fixture.Run(() =>
        {
            var button = Mount(new AppBarToggleButton { Label = "Like", IsChecked = false });
            var label = LabelText(button);
            var resting = ForegroundOf(label);

            button.IsChecked = true;
            PixelHarness.Settle(20);

            Assert.Multiple(
                () => Assert.Equal(ToColour("AppBarToggleButtonForeground"), resting),
                () => Assert.Equal(ToColour("AppBarToggleButtonForegroundChecked"), ForegroundOf(label)));
        });
    }

    /// <summary>
    /// Where a disabled grid header's row stops: the template writes it on the header itself, so the header takes it,
    /// and the text the header's content presenter generates keeps what it inherited when it was built - the same
    /// stop the NumberBox header fact above measures. Pinned as a reading rather than written as a claim about pixels.
    /// </summary>
    [Fact]
    public void A_disabled_column_header_takes_its_row_on_the_carrier_the_template_names()
    {
        _fixture.Run(() =>
        {
            var header = (DataGridColumnHeader)CarrierOf("data-grid-header");
            var resting = ForegroundOf(header);

            header.IsEnabled = false;
            PixelHarness.Settle(20);

            Assert.Multiple(
                () => Assert.Equal(ToColour("DataGridColumnHeaderForeground"), resting),
                () => Assert.Equal(ToColour("DataGridColumnHeaderForegroundDisabled"), ForegroundOf(header)));
        });
    }

    // ---------- helpers ----------

    /// <summary>Mounts one of the seven owners and hands back the element a foreground row of its style writes.</summary>
    private static FrameworkElement CarrierOf(string owner) => owner switch
    {
        "appbar-toggle" => LabelText(Mount(new AppBarToggleButton { Label = "Like", IsChecked = false })),
        "data-grid-cell" => GridCell(),
        "data-grid-header" => GridHeader(),
        "menu-bar-item" => MenuBarItemOf(),
        "menu-flyout-sub-item" => Mount(new MenuFlyoutSubItem { Text = "sub" }),
        "toggle-menu-flyout-item" => Mount(new ToggleMenuFlyoutItem { Text = "toggle" }),
        _ => throw new InvalidOperationException($"Unknown owner '{owner}' - extend this switch, do not skip the owner."),
    };

    private static FrameworkElement LabelText(AppBarToggleButton button) =>
        PixelHarness.Named(button, "LabelText") ?? throw new InvalidOperationException("the app bar template put no LabelText on it");

    private static FrameworkElement GridCell() =>
        PixelHarness.Descendant<DataGridCell>(MountedGrid()) ?? throw new InvalidOperationException("the grid realised no cell");

    private static FrameworkElement GridHeader() =>
        PixelHarness.Descendant<DataGridColumnHeader>(MountedGrid()) ?? throw new InvalidOperationException("the grid realised no header");

    private static FrameworkElement MenuBarItemOf()
    {
        var bar = new MenuBar { Width = 240, Height = 34 };
        bar.Items.Add(new MenuBarItem { Title = "view" });
        PixelHarness.Build(bar, 240, 34);
        PixelHarness.Settle(30);
        return PixelHarness.Descendant<MenuBarItem>(bar) ?? throw new InvalidOperationException("the menu bar realised no item");
    }

    /// <summary>A freshly mounted one-column grid. Not cached: a header a later fact disables must not leak into it.</summary>
    private static DataGrid MountedGrid()
    {
        var grid = new DataGrid { Width = 420, Height = 160, AutoGenerateColumns = false };
        grid.Columns.Add(new DataGridTextColumn { Header = "Alpha", Width = 160, Binding = new Binding("Name") });
        grid.ItemsSource = new List<Row> { new("Row 1", 1) };
        PixelHarness.Build(grid, 440, 200);
        PixelHarness.Settle(60);
        return grid;
    }

    private sealed record Row(string Name, int Value);

    /// <summary>The foreground a carrier holds, read off the carrier itself - the three spellings of this DP are one object.</summary>
    private static Color ForegroundOf(FrameworkElement carrier) => carrier switch
    {
        Control control => ColourOf(control.Foreground),
        TextBlock text => ColourOf(text.Foreground),
        _ => throw new InvalidOperationException($"{carrier.GetType().Name} carries no Foreground to read."),
    };

    private static T Mount<T>(T control) where T : FrameworkElement
    {
        var width = control is ListBox or ComboBox ? 220 : 320;
        var height = control is NumberBox or ComboBox ? 62 : control is MenuFlyoutItem ? 38 : 120;
        PixelHarness.Build(control, width, height);
        PixelHarness.Settle(30);
        return control;
    }

    /// <summary>The text this runtime generated for the subject's own label.</summary>
    private static TextBlock Text(Visual root) =>
        PixelHarness.Descendant<TextBlock>(root) ?? throw new InvalidOperationException("no generated text on the subject");

    private static TextBlock HeaderText(Visual root)
    {
        var carrier = PixelHarness.Named(root, "HeaderContentPresenter")
            ?? throw new InvalidOperationException("the NumberBox template put no header carrier on it");
        return PixelHarness.Descendant<TextBlock>(carrier)
            ?? throw new InvalidOperationException("the NumberBox header painted no text");
    }

    private static Color ToColour(string key) => ((SolidColorBrush)Application.Current!.TryFindResource(key)!).Color;

    private static Color ColourOf(Brush? brush) => ((SolidColorBrush)brush!).Color;

    private static void AssertEqualColour(string key, TextBlock text) => Assert.Equal(ToColour(key), ((SolidColorBrush)text.Foreground!).Color);
}
