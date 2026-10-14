using FluentJalium.Controls;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The radio-button list: which containers the host generates, whether the column count the app asks for actually
/// reaches the layout, and whether the choice the host reports is the choice the containers show.
/// </summary>
/// <remarks>
/// <para>
/// This is the first control in the library that derives from <c>ItemsControl</c>, and it does so because the
/// pipeline underneath was measured rather than assumed: the container overrides are protected, a derived host
/// realizes containers through its own template, our implicit <c>RadioButton</c> style reaches a generated
/// container, and radios sharing an items host exclude each other with an empty <c>GroupName</c>
/// (docs/astra/adaptation/s1l-itemhost-raw.txt, which also withdraws the older "never derive from ItemsControl"
/// rule in <c>adaptation/09</c>).
/// </para>
/// <para>
/// The geometry tests exist because the route the column count takes is a guess until it is read: an items panel
/// is built by the framework's presenter, so whether it sees the control it serves is not something a type
/// declaration settles. A panel that never found its host would keep drawing a single quiet column and every
/// other test here would still pass, which is the silent failure this file is mostly written against.
/// </para>
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraRadioButtonsTests
{
    /// <summary>The framework's own brand green, which no Astra token produces (adaptation/00 S1-e).</summary>
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraRadioButtonsTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void The_host_generates_real_radio_buttons_carrying_our_template()
    {
        _fixture.Run(() =>
        {
            var host = Host(4, 240, 160);
            var choices = Choices(host);
            Assert.Equal(4, choices.Count);
            Assert.All(choices, choice => Assert.Equal(choice.Content, TextOf(choice)));

            // Style is the wrong read here: the framework resolves a native type's implicit style without ever
            // writing the Style property (s1l section F), so the claim is made on what the container built.
            Assert.All(choices, choice => Assert.NotNull(PixelHarness.Named(choice, "RadioRing")));
            Assert.NotNull(PixelHarness.Descendant<FluentRadioButtonsPanel>(host));
        });
    }

    [Fact]
    public void MaxColumns_reaches_the_layout_and_fills_the_first_column_before_the_second()
    {
        _fixture.Run(() =>
        {
            var host = Host(6, 320, 160, maxColumns: 3);
            var choices = Choices(host);
            Assert.Equal(6, choices.Count);

            var offsets = choices.Select(choice => choice.TranslatePoint(new Point(0, 0), host)).ToList();

            // Six choices over three columns is two rows, and upstream fills down the column: items 0 and 1 share
            // the left column, item 2 opens the next one. A panel that failed to find its host would stack all six
            // at one x and this is where it fails.
            Assert.Equal(offsets[0].X, offsets[1].X, 1);
            Assert.True(offsets[1].Y > offsets[0].Y, $"second row did not drop below the first: {offsets[0]} {offsets[1]}");
            Assert.True(offsets[2].X > offsets[1].X, $"third item did not open a second column: {offsets[1]} {offsets[2]}");
            Assert.Equal(offsets[0].Y, offsets[2].Y, 1);

            // One cell size for the whole grid, at the largest item, with upstream's 7 DIP between columns.
            var width = choices[0].ActualWidth;
            Assert.All(choices, choice => Assert.Equal(width, choice.ActualWidth, 1));
            Assert.Equal(offsets[0].X + width + 7, offsets[2].X, 1);
        });
    }

    [Fact]
    public void One_column_keeps_the_choices_in_a_single_stacked_column()
    {
        _fixture.Run(() =>
        {
            var host = Host(4, 240, 200);
            var offsets = Choices(host).Select(choice => choice.TranslatePoint(new Point(0, 0), host)).ToList();
            Assert.Equal(4, offsets.Count);
            Assert.All(offsets, offset => Assert.Equal(offsets[0].X, offset.X, 1));
            for (var index = 1; index < offsets.Count; index++)
            {
                Assert.True(offsets[index].Y > offsets[index - 1].Y, $"row {index} did not move down");
            }
        });
    }

    [Fact]
    public void Taking_a_choice_clears_the_others_and_reports_it_once()
    {
        _fixture.Run(() =>
        {
            var host = Host(3, 240, 140);
            var choices = Choices(host);
            // Every raise is recorded rather than asserted one at a time, so a second or an out-of-order raise
            // shows up as the whole sequence instead of a bare collection message.
            var raises = new List<string>();
            host.SelectionChanged += (_, arguments) => raises.Add(
                $"added=[{string.Join(",", arguments.AddedItems.OfType<object>())}] " +
                $"removed=[{string.Join(",", arguments.RemovedItems.OfType<object>())}]");

            // This is the state a click leaves behind, not the click itself: no real pointer reaches the control
            // in this suite (docs/astra/audits/radio-buttons.md Known Gaps).
            choices[1].IsChecked = true;
            PixelHarness.Settle(4);

            Assert.Equal(1, host.SelectedIndex);
            Assert.Equal(host.Items[1], host.SelectedItem);
            Assert.Equal(new[] { false, true, false }, choices.Select(choice => choice.IsChecked == true));
            Assert.Equal(["added=[choice 2] removed=[]"], raises);

            choices[2].IsChecked = true;
            PixelHarness.Settle(4);
            Assert.Equal(2, host.SelectedIndex);
            Assert.Equal(
                ["added=[choice 2] removed=[]", "added=[choice 3] removed=[choice 2]"],
                raises);
        });
    }

    [Fact]
    public void A_choice_made_in_code_checks_the_container_that_carries_it()
    {
        _fixture.Run(() =>
        {
            var host = Host(3, 240, 140);
            host.SelectedItem = host.Items[2];
            PixelHarness.Settle(4);

            Assert.Equal(2, host.SelectedIndex);
            var checkedIndex = Choices(host).FindIndex(choice => choice.IsChecked == true);
            Assert.Equal(2, checkedIndex);
            Assert.Same(host.ContainerFromIndex(2), Choices(host)[2]);
        });
    }

    [Fact]
    public void The_ring_and_the_dot_of_a_generated_choice_read_the_palette_instances()
    {
        _fixture.Run(() =>
        {
            var host = Host(2, 240, 120);
            var choice = Choices(host)[0];
            var ring = PixelHarness.Named(choice, "RadioRing") ?? throw new InvalidOperationException("no ring");
            var dot = PixelHarness.Named(choice, "RadioDot") ?? throw new InvalidOperationException("no dot");

            Assert.Same(Resource("RadioButtonOuterEllipseFill"), Assert.IsType<SolidColorBrush>(Assert.IsType<Border>(ring).Background));
            choice.IsChecked = true;
            PixelHarness.Settle(4);
            var fill = Assert.IsType<SolidColorBrush>(Prop(dot, "Fill"));
            Assert.NotEqual(BrandEmerald, fill.Color);
            Assert.Equal(((SolidColorBrush)Resource("RadioButtonCheckGlyphFill")!).Color, fill.Color);
        });
    }

    [Fact]
    public void A_choice_marks_pixels_the_moment_it_is_taken()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);

            // Built through Host and not Build: the two are separate parents and an element that went through one
            // throws when the other takes it (adaptation/06). Host is also the only capture that keeps a
            // translucent fill, which is what the dot's opacity is.
            var host = new FluentRadioButtons
            {
                ItemsSource = new[] { "bus", "tram", "ferry" },
                Width = 240,
                Height = 140,
            };
            // The ring's checked fill, not the dot's own colour: in Dark the dot paints the same white the labels
            // do, so its key goes down when a choice is taken while the accent ring is what actually appears.
            var accent = ((SolidColorBrush)Resource("RadioButtonOuterEllipseCheckedFill")!).Color;
            var key = PixelHarness.PixelKey(accent);

            var before = PixelHarness.Host(host, 240, 140);
            Choices(host)[0].IsChecked = true;
            PixelHarness.Settle(6);
            var after = PixelHarness.Host(host, 240, 140);

            // The dot's own colour is not unique on screen - in Dark it is the same white the labels paint with -
            // so the claim is a delta of that one colour key between rest and chosen, not its presence.
            var atRest = before.Histogram.TryGetValue(key, out var restingCount) ? restingCount : 0;
            var chosen = after.Histogram.TryGetValue(key, out var chosenCount) ? chosenCount : 0;
            Assert.True(chosen - atRest >= 40,
                $"checking a choice added {chosen - atRest} pixels of its own dot colour ({before.Top(4)} / {after.Top(4)})");
            Assert.DoesNotContain(PixelHarness.PixelKey(BrandEmerald), after.Histogram.Keys);
        });
    }

    [Fact]
    public void The_header_reads_its_own_row_and_disappears_without_content()
    {
        _fixture.Run(() =>
        {
            var host = new FluentRadioButtons
            {
                Header = "Transport",
                ItemsSource = new[] { "bus", "tram" },
                Width = 240,
                Height = 140,
            };
            PixelHarness.Build(host, 240, 140);
            var presenter = PixelHarness.Named(host, "HeaderContentPresenter") ?? throw new InvalidOperationException("no header part");
            Assert.Equal("Transport", Assert.IsType<ContentPresenter>(presenter).Content);
            Assert.Equal(Visibility.Visible, presenter.Visibility);
            Assert.Same(Resource("RadioButtonsHeaderForeground"), host.Foreground);
            Assert.Equal(new Thickness(0, 0, 0, 8), (Thickness)Prop(presenter, "Margin")!);

            host.Header = null;
            PixelHarness.Settle(4);
            Assert.Equal(Visibility.Collapsed, presenter.Visibility);
        });
    }

    [Fact]
    public void Disabling_the_list_moves_the_header_to_the_disabled_row()
    {
        _fixture.Run(() =>
        {
            var host = new FluentRadioButtons
            {
                Header = "Transport",
                ItemsSource = new[] { "bus", "tram" },
                Width = 240,
                Height = 140,
            };
            PixelHarness.Build(host, 240, 140);
            Assert.Same(Resource("RadioButtonsHeaderForeground"), host.Foreground);

            // CommonStates/Disabled is the only non-empty state upstream gives this control
            // (RadioButtons.xaml:14-18) and the header's foreground is the only thing it writes.
            host.IsEnabled = false;
            PixelHarness.Settle(6);
            Assert.Same(Resource("RadioButtonsHeaderForegroundDisabled"), host.Foreground);
        });
    }

    [Theory]
    [InlineData("RadioButtonsHeaderForeground")]
    [InlineData("RadioButtonsHeaderForegroundDisabled")]
    [InlineData("RadioButtonsTopHeaderMargin")]
    public void Every_row_the_list_reads_is_published(string key)
    {
        Assert.NotNull(Resource(key));
    }

    [Theory]
    [InlineData("RadioButtonsColumnSpacing")]
    [InlineData("RadioButtonsRowSpacing")]
    [InlineData("RadioButtonsHeaderBackground")]
    [InlineData("RadioButtonsBorderBrush")]
    public void A_row_this_control_does_not_build_is_not_published(string key)
    {
        // The two spacings are x:Double rows upstream, which this reader cannot publish; the other two names do
        // not exist upstream at all. Publishing either kind would put an unbacked row in the token layer.
        Assert.Null(Resource(key));
    }

    private static FluentRadioButtons Host(int count, double width, double height, int maxColumns = 1)
    {
        var host = new FluentRadioButtons
        {
            ItemsSource = Enumerable.Range(1, count).Select(index => $"choice {index}").ToList(),
            MaxColumns = maxColumns,
            Width = width,
            Height = height,
        };
        PixelHarness.Build(host, (int)width, (int)height);
        return host;
    }

    /// <summary>
    /// The containers the host generated, read back through the same public door an app uses. The cross-check
    /// that they really are in the built tree is the <c>Descendant&lt;RadioButton&gt;</c> and the part-name reads
    /// in the first test, because a map only the control keeps would pass on containers that never mounted.
    /// </summary>
    private static List<RadioButton> Choices(FluentRadioButtons host) =>
        Enumerable.Range(0, host.Items.Count).Select(index => host.ContainerFromIndex(index) ??
            throw new InvalidOperationException($"no container for item {index}")).ToList();

    private static string? TextOf(RadioButton choice) =>
        PixelHarness.Descendant<TextBlock>(choice)?.Text;

    private static object? Resource(object key) => Application.Current?.TryFindResource(key);

    private static object? Prop(object? target, string name) => target is null
        ? null
        : target.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.FlattenHierarchy)?.GetValue(target);
}
