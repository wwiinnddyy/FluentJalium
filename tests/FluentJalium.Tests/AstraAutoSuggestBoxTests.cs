using System.Reflection;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// AutoSuggestBox batch. There is no AutoSuggestBox on this runtime: the host substitution is the native
/// <see cref="AutoCompleteBox"/>, which the framework gives a code-built template at mount time (its own
/// colours, none of them ours), so the style below has to displace that template and honour the part names
/// the control's own dropdown code looks up (docs/astra/audits/autosuggestbox.md §0).
/// Every state cell is read back the way the Slider, ComboBox and NumberBox batches established: a named
/// condition plus the value the built tree ends up holding, never a markup string.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraAutoSuggestBoxTests
{
    private static readonly Color SurfaceSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color AccentSentinel = Color.FromRgb(0x00, 0xFF, 0x80);
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraAutoSuggestBoxTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    /// <summary>The two suggestion-list alias rows, by upstream name and upstream target.</summary>
    [Theory]
    [InlineData("AutoSuggestBoxSuggestionsListBackground", "FlyoutPresenterBackground")]
    [InlineData("AutoSuggestBoxSuggestionsListBorderBrush", "SurfaceStrokeColorFlyoutBrush")]
    public void A_suggestions_list_alias_resolves_to_its_target(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    [Fact]
    public void The_suggestions_list_metric_rows_are_upstreams_including_the_name_swap()
    {
        _fixture.Run(() =>
        {
            Assert.Equal(new Thickness(1), Res("AutoSuggestListBorderThemeThickness"));
            // Upstream's own swap, kept because it is what the pixels are made of: the row named Margin is the
            // container's Padding and the row named Padding is the list's Margin.
            Assert.Equal(new Thickness(0, 2, 0, 2), Res("AutoSuggestListMargin"));
            Assert.Equal(new Thickness(-1, 0, -1, 0), Res("AutoSuggestListPadding"));
        });
    }

    /// <summary>
    /// Three upstream rows are x:Double and this reader cannot parse one at all, so their values have to be
    /// proven present as literals where they are consumed - a {ThemeResource} on a key that does not exist
    /// fails silently.
    /// </summary>
    [Fact]
    public void The_x_double_rows_stay_literals_because_this_reader_cannot_parse_them()
    {
        _fixture.Run(() =>
        {
            Assert.Null(Res("AutoSuggestBoxIconFontSize"));
            Assert.Null(Res("AutoSuggestListMaxHeight"));
            Assert.Null(Res("AutoSuggestListBorderOpacity"));

            var box = Mounted();
            Assert.Equal(32d, box.MinHeight);
            Assert.Equal(64d, box.MinWidth);
            Open(box);
            Assert.Equal(374d, (PopupPart("SuggestionsContainer") as Border)!.MaxHeight);
        });
    }

    /// <summary>
    /// A row only earns a transcription when the host type has something to land it on. These upstream keys
    /// place a header, a delete button, a QueryIcon or a light-dismiss overlay, and AutoCompleteBox has none
    /// of those parts, so declaring them would fail the key-consumption gate. Four of them (the two
    /// LeftHeader rows, the list border opacity, the item margin) have no consumer in upstream's own markup
    /// either - they are inert names, not a surface we are missing.
    /// </summary>
    [Fact]
    public void Rows_with_no_consumer_on_the_host_type_stay_undeclared()
    {
        _fixture.Run(() =>
        {
            foreach (var key in new[]
                     {
                         "AutoSuggestBoxTopHeaderMargin", "AutoSuggestBoxInnerButtonMargin",
                         "AutoSuggestBoxDeleteButtonMargin", "AutoSuggestBoxQueryButtonPadding",
                         "AutoSuggestBoxLeftButtonMargin", "AutoSuggestBoxRightButtonMargin",
                         "AutoSuggestBoxLeftHeaderMargin", "AutoSuggestBoxLeftHeaderMaxWidth",
                         "AutoSuggestBoxLightDismissOverlayBackground", "AutoSuggestListViewItemMargin",
                         "AutoSuggestListBorderOpacity", "HelperButtonThemePadding",
                         "AutoSuggestBackgroundThemeBrush",
                     })
            {
                Assert.Null(Res(key));
            }
        });
    }

    /// <summary>
    /// Downgrades the <c>adaptation/00 S0-g</c> note from a syntax ban to a measurement: the ComboBox batch
    /// recorded "an empty-string condition never matches", but in the production shape (dictionary -> style
    /// -> setter -> inline template) here a cell keyed on <c>Value=""</c> does fire while <c>Text</c> is the
    /// empty string, and stops firing one character in. Why the two readings differ is not measured here -
    /// only that the empty string is not inert as a rule.
    /// </summary>
    [Fact]
    public void An_empty_string_condition_matches_a_string_property_that_is_actually_empty()
    {
        _fixture.Run(() =>
        {
            const string markup = """
                <ResourceDictionary xmlns='http://schemas.jalium.ui/2024'
                                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                  <Style x:Key='ProbeAutoCompleteBoxStyle' TargetType='AutoCompleteBox'>
                    <Setter Property='Template'>
                      <ControlTemplate TargetType='AutoCompleteBox'>
                        <Grid>
                          <Grid Name='PART_ContentHost' />
                          <TextBlock Name='ProbePlaceholder' Text='{TemplateBinding PlaceholderText}'
                                     Visibility='Collapsed' />
                        </Grid>
                        <ControlTemplate.Triggers>
                          <Trigger Property='Text' Value=''>
                            <Setter TargetName='ProbePlaceholder' Property='Visibility' Value='Visible' />
                          </Trigger>
                        </ControlTemplate.Triggers>
                      </ControlTemplate>
                    </Setter>
                  </Style>
                </ResourceDictionary>
                """;
            var dictionary = (ResourceDictionary)XamlReader.Parse(markup)!;
            var box = new AutoCompleteBox
            {
                PlaceholderText = "Pick a fruit",
                Style = (Style)dictionary["ProbeAutoCompleteBoxStyle"]!,
            };
            PixelHarness.Build(box, 260, 32);

            Assert.Equal(string.Empty, box.Text);
            var placeholder = (FrameworkElement)Part(box, "ProbePlaceholder");
            Assert.Equal(Visibility.Visible, placeholder.Visibility);

            box.Text = "b";
            PixelHarness.Settle();
            Assert.Equal(Visibility.Collapsed, placeholder.Visibility);
        });
    }

    /// <summary>Every style-level row the text half reads, by property and upstream key.</summary>
    [Theory]
    [InlineData("Foreground", "TextControlForeground")]
    [InlineData("Background", "TextControlBackground")]
    [InlineData("BorderBrush", "TextControlBorderBrush")]
    [InlineData("SelectionBrush", "TextControlSelectionHighlightColor")]
    [InlineData("CaretBrush", "TextControlForeground")]
    [InlineData("BorderThickness", "TextControlBorderThemeThickness")]
    [InlineData("CornerRadius", "ControlCornerRadius")]
    [InlineData("Padding", "TextControlThemePadding")]
    public void A_style_setter_names_its_upstream_row(string property, string key)
    {
        _fixture.Run(() => Assert.Equal(key, KeyOf(Setter(Style(), property))));
    }

    [Fact]
    public void The_template_carries_exactly_the_three_state_cells_upstream_has()
    {
        _fixture.Run(() => Assert.Equal(
            ["IsMouseOver=True", "IsKeyboardFocusWithin=True", "IsEnabled=False"],
            Cells(Template()).Select(static cell => cell.Condition).ToArray()));
    }

    [Fact]
    public void The_pointer_over_cell_writes_the_pointer_over_rows()
    {
        _fixture.Run(() => AssertCell(Cells(Template()), "IsMouseOver=True",
            ("OuterBorder", "Background", "TextControlBackgroundPointerOver"),
            ("OuterBorder", "BorderBrush", "TextControlBorderBrushPointerOver"),
            ("self", "Foreground", "TextControlForegroundPointerOver")));
    }

    [Fact]
    public void The_focused_cell_writes_the_focused_rows_and_the_thicker_edge()
    {
        _fixture.Run(() => AssertCell(Cells(Template()), "IsKeyboardFocusWithin=True",
            ("OuterBorder", "Background", "TextControlBackgroundFocused"),
            ("OuterBorder", "BorderThickness", "TextControlBorderThemeThicknessFocused"),
            ("BottomEdge", "Background", "TextControlBorderBrushFocused"),
            ("self", "Foreground", "TextControlForegroundFocused")));
    }

    [Fact]
    public void The_disabled_cell_writes_the_disabled_rows_and_the_disabled_caret()
    {
        _fixture.Run(() => AssertCell(Cells(Template()), "IsEnabled=False",
            ("OuterBorder", "Background", "TextControlBackgroundDisabled"),
            ("OuterBorder", "BorderBrush", "TextControlBorderBrushDisabled"),
            ("BottomEdge", "Background", "TextControlBorderBrushDisabled"),
            ("self", "Foreground", "TextControlForegroundDisabled"),
            ("self", "CaretBrush", "TextControlForegroundDisabled")));
    }

    /// <summary>
    /// The framework builds a template for AutoCompleteBox when the control is mounted, and it reads its own
    /// colours out of it (measured: #FF2C2C2E / #FF48484A, none of them a palette instance). Ours has to be
    /// the one that survives, with the four part names the control's dropdown code looks up.
    /// </summary>
    [Fact]
    public void Our_template_displaces_the_frameworks_and_keeps_its_part_names()
    {
        _fixture.Run(() =>
        {
            var box = Mounted();
            Assert.Same(Template(), box.Template);
            Assert.NotNull(Part(box, "OuterBorder"));
            Assert.NotNull(Part(box, "PART_ContentHost"));
            Assert.NotNull(Part(box, "PART_Popup"));
            Open(box);
            Assert.NotNull(PopupPart("PART_DropDownItemsHost"));
            Assert.IsType<Grid>(Part(box, "PART_ContentHost"));
            Assert.IsType<StackPanel>(PopupPart("PART_DropDownItemsHost"));
        });
    }

    /// <summary>
    /// Framework contract, measured in the NumberBox batch and re-measured here: the text engine is grafted
    /// into PART_ContentHost only when that part is a panel.
    /// </summary>
    [Fact]
    public void The_framework_grafts_its_text_engine_into_our_content_host()
    {
        _fixture.Run(() =>
        {
            var host = Part(Mounted(), "PART_ContentHost");
            Assert.Equal("TextBoxContentHost", FirstChild(host).GetType().Name);
        });
    }

    [Fact]
    public void The_suggestions_container_wears_the_two_rows_and_the_corner_token()
    {
        _fixture.Run(() =>
        {
            var box = Mounted();
            Open(box);
            var container = (Border)PopupPart("SuggestionsContainer");
            Assert.Same(Res("AutoSuggestBoxSuggestionsListBackground"), container.Background);
            Assert.Same(Res("AutoSuggestBoxSuggestionsListBorderBrush"), container.BorderBrush);
            Assert.Equal(new Thickness(1), container.BorderThickness);
            Assert.Equal(new Thickness(0, 2, 0, 2), container.Padding);
            Assert.Equal(new CornerRadius(8), container.CornerRadius);
            Assert.Equal(new Thickness(-1, 0, -1, 0),
                ((FrameworkElement)PopupPart("PART_DropDownScrollViewer")).Margin);
        });
    }

    /// <summary>
    /// The right-hand hole the running Gallery showed: the runtime sizes the popup's own window to the control
    /// (260 here) while the surface inside it only ever asked for its content's width (148 measured on screen),
    /// so the dropdown painted a card and left the rest of its window unpainted. The surface now carries the
    /// control's width as a floor, which is what upstream's list does.
    /// </summary>
    [Fact]
    public void The_suggestions_surface_spans_the_box_instead_of_its_longest_row()
    {
        _fixture.Run(() =>
        {
            var box = Mounted();
            Open(box);
            var container = (Border)PopupPart("SuggestionsContainer");
            Assert.Multiple(
                () => Assert.Equal(box.ActualWidth, container.MinWidth),
                () => Assert.Equal(box.ActualWidth, container.ActualWidth));
        });
    }

    /// <summary>
    /// Typing filters and opens, and the containers land in the panel our template provides. This is the
    /// read-back that proves the popup part names are the contract: with no child on PART_Popup the
    /// framework opens an empty 20 DIP root, and with an ItemsControl of that name it never populates.
    /// </summary>
    [Fact]
    public void Typing_filters_into_the_items_host_our_template_provides()
    {
        _fixture.Run(() =>
        {
            var box = Mounted();
            box.ItemsSource = new[] { "Apple", "Banana", "Cherry", "Blueberry" };
            box.Text = "ba";
            Assert.True(box.IsDropDownOpen, "the dropdown did not open on a filtering edit");
            Assert.Equal("Banana", Assert.Single(box.FilteredItems));

            var host = PopupPart("PART_DropDownItemsHost");
            Assert.True(VisualTreeHelper.GetChildrenCount(host) >= 1,
                $"the framework generated no container under PART_DropDownItemsHost ({host.GetType().Name})");
            Assert.Equal("ComboBoxItem", FirstChild(host).GetType().Name);
        });
    }

    /// <summary>
    /// The framework owns the popup's width and writes it locally onto our part, the same contract measured
    /// for ComboBox. Nothing in the style can set it, so the only honest assertion is the read-back.
    /// </summary>
    [Fact]
    public void The_framework_sizes_our_popup_to_the_control()
    {
        _fixture.Run(() =>
        {
            var box = Mounted();
            var popup = Part(box, "PART_Popup");
            var width = DependencyProperty.FromName(popup.GetType(), "Width")
                ?? throw new InvalidOperationException("Popup has no Width.");
            Assert.NotEqual(DependencyProperty.UnsetValue, popup.ReadLocalValue(width));
            Assert.Equal(box.Width, Convert.ToDouble(popup.GetValue(width)));
        });
    }

    /// <summary>
    /// Item containers come out of the framework's dropdown code: its text token reaches them (our implicit
    /// ComboBoxItem style is what paints their Foreground) but their fill is a framework local value, which
    /// outranks any style setter - the same bill the TextBox, ComboBox and NumberBox batches recorded, now
    /// measured on a seventh and eighth property.
    /// </summary>
    [Fact]
    public void An_item_takes_our_text_row_but_keeps_the_frameworks_own_fill()
    {
        _fixture.Run(() =>
        {
            var box = Mounted();
            box.ItemsSource = new[] { "Apple", "Banana", "Cherry" };
            box.Text = "a";
            var host = PopupPart("PART_DropDownItemsHost");
            var item = (Control)FirstChild(host);

            Assert.Same(Res("TextFillColorPrimaryBrush"), item.Foreground);
            var background = DependencyProperty.FromName(item.GetType(), "Background")!;
            Assert.NotEqual(DependencyProperty.UnsetValue, item.ReadLocalValue(background));
            Assert.NotSame(Res("ComboBoxItemBackground"), item.Background);
        });
    }

    /// <summary>
    /// Framework ownership again, control level: a disabled AutoCompleteBox reports the framework's own
    /// disabled grey, not our row. Pinning the loss instead of dropping the claim.
    /// </summary>
    [Fact]
    public void A_disabled_box_keeps_the_frameworks_own_disabled_text_colour()
    {
        _fixture.Run(() =>
        {
            var box = Mounted();
            var disabled = (Brush)Res("TextControlForegroundDisabled")!;
            box.IsEnabled = false;
            Assert.NotSame(disabled, box.Foreground);
            box.IsEnabled = true;
            Assert.Same(Res("TextControlForeground"), box.Foreground);
        });
    }

    /// <summary>
    /// The behaviour this substitution loses, written down as an assertion so it cannot be forgotten: with
    /// the framework's own template, selecting a suggestion writes the completed text into the box; with
    /// ours it does not, even focused and even with IsTextCompletionEnabled on. Upstream calls this
    /// UpdateTextOnSelect, and no resource row can bring it back - it needs code.
    /// </summary>
    [Fact]
    public void Selecting_a_suggestion_does_not_write_the_text_under_a_replaced_template()
    {
        _fixture.Run(() =>
        {
            var box = Mounted();
            box.ItemsSource = new[] { "Apple", "Banana", "Blueberry" };
            box.Focus();
            box.Text = "bl";
            box.IsTextCompletionEnabled = true;
            Assert.Single(box.FilteredItems);
            box.SelectedItem = box.FilteredItems[0];
            Assert.Equal("bl", box.Text);
        });
    }

    [Fact]
    public void A_resting_box_paints_its_rows_and_nothing_invented()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", SurfaceSentinel);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", AccentSentinel);
            try
            {
                var sample = PixelHarness.Render(new AutoCompleteBox { Width = 260 }, 260, 32);
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
    /// The two suggestions-list rows are the only surface this batch adds that no other control reads, so the
    /// claim goes as far as the pixels: under a sentinel on the palette brush the flyout chain ends at, the
    /// open list paints it. <c>Chrome</c> is the capture path that photographs a visual where it already
    /// sits, which is what a popup the framework re-parented into the overlay layer needs.
    /// Measured here: this crop is an ancestor of the item containers, and those carry a framework-local
    /// gradient of their own (see the item test above), so at this crop the picture never comes back
    /// bit-identical between rounds - neither <c>Stable</c> nor the "brand green never appears" gate is
    /// assertible. The honest claim is the sentinel's presence and size.
    /// </summary>
    [Fact]
    public void The_open_suggestion_surface_reaches_the_pixels()
    {
        _fixture.Run(() =>
        {
            var box = Mounted();
            Open(box);
            var container = PopupPart("SuggestionsContainer");
            // The alias layer forwards instances, so the sentinel has to go on the palette row the chain
            // ends at (AutoSuggestBoxSuggestionsListBackground -> FlyoutPresenterBackground -> this).
            FluentThemeManager.OverrideBrush("AcrylicInAppFillColorDefaultBrush", SurfaceSentinel);
            try
            {
                PixelHarness.Settle();
                var sample = PixelHarness.Chrome(container);
                Assert.True(sample.Count(SurfaceSentinel) > 200,
                    $"the suggestions-list surface row did not reach the pixels; top={sample.Top(8)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AcrylicInAppFillColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void The_closed_box_surface_changes_between_light_and_dark()
    {
        _fixture.Run(() =>
        {
            var light = PixelHarness.Render(new AutoCompleteBox { Width = 260 }, 260, 32);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            try
            {
                var dark = PixelHarness.Render(new AutoCompleteBox { Width = 260 }, 260, 32);
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
    /// A cell that writes a literal colour is invisible to the alias layer, so an app overriding a row sees
    /// nothing. Every state setter here has to carry a resource key.
    /// </summary>
    [Fact]
    public void Every_cell_names_a_row_and_never_a_palette_brush()
    {
        _fixture.Run(() =>
        {
            foreach (var cell in Cells(Template()))
            {
                foreach (var setter in cell.Setters)
                {
                    Assert.NotNull(KeyOf(setter));
                }
            }
        });
    }

    private object? Res(string key) => _fixture.Application.TryFindResource(key);

    private AutoCompleteBox Mounted()
    {
        var box = new AutoCompleteBox { Width = 260 };
        PixelHarness.Build(box, 260, 32);
        return box;
    }

    /// <summary>
    /// Opening the dropdown is what makes the framework build and graft the popup child, so any read of a
    /// popup part has to happen after this.
    /// </summary>
    private static void Open(AutoCompleteBox box)
    {
        box.ItemsSource = new[] { "Apple", "Banana", "Cherry" };
        box.Text = "b";
        PixelHarness.Settle();
    }

    /// <summary>
    /// Popup parts are only reachable from the host window: the framework re-parents the popup child into
    /// the window's overlay layer when it opens, the same way the ComboBox batch had to read its dropdown
    /// off the host.
    /// </summary>
    private static FrameworkElement PopupPart(string name) =>
        Part(PixelHarness.HostWindow(), name);

    private static Style Style() => FluentThemeManager.GetStyle("DefaultAutoCompleteBoxStyle");

    private static ControlTemplate Template() =>
        (Style().Setters.Cast<object>().OfType<Setter>()
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

    /// <summary>
    /// The framework's generated child (a grafted text host, an item container). A missing one is a failure
    /// worth naming rather than a null to dereference.
    /// </summary>
    private static DependencyObject FirstChild(DependencyObject parent) =>
        VisualTreeHelper.GetChild(parent, 0) ?? throw new InvalidOperationException(
            $"{parent.GetType().Name} has no visual child.");

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

    private sealed record Cell(string Condition, Setter[] Setters);
}
