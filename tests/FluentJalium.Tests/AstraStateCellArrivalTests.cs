using FluentJalium.Controls;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// The other half of <c>docs/astra/audits/foreground.md</c> 7.6: a state cell's value landing on the element its
/// style writes it to. The census that scoped this (<c>spike/StateCellCensus/census-2026-09-23.txt</c>) counts 176
/// <c>Foreground</c> setters inside triggers over <c>Styles/*.jalxaml</c>, 91 of them reachable without a pointer -
/// and 13 that have neither a pointer-free route nor a test naming their key, which is the worklist these six came
/// from (audits/foreground.md 7.6e enumerates all 13 and what happened to each).
///
/// Each fact here was paired with two mutations of its own row - take the row away, and point it at a brush
/// nothing else supplies - and the pair says more than the row alone can. A row whose brush equals what the
/// control reads anyway (#87's default-ink finding) cannot be found by deletion, so the re-pointing is the ruler
/// here; and where the re-pointing lands too, which writer wins is decided by where the cell lives: a disabled
/// ink row in a <c>Style.Triggers</c> beats the framework's own write, while the same row in a
/// <c>ControlTemplate.Triggers</c> loses to it. Both halves are recorded in audits/foreground.md 7.6b.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraStateCellArrivalTests : IDisposable
{
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraStateCellArrivalTests(AstraThemeRuntimeFixture fixture)
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
        _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Light));
    }

    [Fact]
    public void A_disabled_subtle_button_takes_the_disabled_row_its_style_cell_names()
    {
        // SubtleButtonStyle is a keyed style, so the cell only fires on a control that was given that style - and
        // the resting row forwards the same brush an unwritten control would carry anyway (the #87 reading), so
        // this fact is only falsifiable against the disabled row.
        _fixture.Run(() =>
        {
            var button = Mount(new Button { Content = "Subtle", Style = Keyed("SubtleButtonStyle") });
            AssertRow(button, "SubtleButtonForeground");

            button.IsEnabled = false;
            PixelHarness.Settle(30);
            AssertRow(button, "SubtleButtonForegroundDisabled");
        });
    }

    [Fact]
    public void A_disabled_check_box_takes_the_disabled_ink_the_framework_writes_not_our_cell()
    {
        // The control's own ink, not the label's: AstraForegroundRoutingTests already reads the generated label of a
        // disabled check box, and that one is a framework local value our cells cannot beat.
        //
        // Which writer supplies this one is the opposite answer from the two cells below, and the mutations say so:
        // pointing the check box's disabled template cell at white leaves the reading alone, deleting it does too,
        // and only moving the name `TextDisabled` moves the ink (audits/foreground.md 7.6b). So the claim is about
        // the route, and the brush is the palette one that name forwards - checking against the alias would follow
        // the mutation and certify nothing.
        _fixture.Run(() =>
        {
            var box = Mount(new CheckBox { Content = "Subscribe", IsChecked = false });
            AssertRow(box, "CheckBoxForegroundUnchecked");

            box.IsEnabled = false;
            PixelHarness.Settle(30);
            var ink = Resource("TextFillColorDisabledBrush");
            Assert.True(
                ReferenceEquals(ink, box.Foreground),
                $"CheckBox reads {Hex(box.Foreground)}, not the {Hex(ink)} the framework writes from the name " +
                "TextDisabled.");
        });
    }

    [Fact]
    public void A_checked_check_box_reads_the_ink_its_template_cell_names()
    {
        // The resting row forwards the same brush an unwritten label carries (#87), so only the checked half of
        // this pair can be reddened - and it can be reddened by re-pointing its cell, which is the reading that
        // separates "this cell writes it" from "the default ink happens to match".
        _fixture.Run(() =>
        {
            var box = Mount(new CheckBox { Content = "Subscribe", IsChecked = false });
            AssertRow(box, "CheckBoxForegroundUnchecked");

            box.IsChecked = true;
            PixelHarness.Settle(30);
            AssertRow(box, "CheckBoxForegroundChecked");
        });
    }

    [Fact]
    public void A_disabled_repeat_button_takes_the_disabled_row_its_style_cell_names()
    {
        _fixture.Run(() =>
        {
            var button = Mount(new RepeatButton { Content = "Step" });
            AssertRow(button, "RepeatButtonForeground");

            button.IsEnabled = false;
            PixelHarness.Settle(30);
            AssertRow(button, "RepeatButtonForegroundDisabled");
        });
    }

    [Fact]
    public void A_selected_tab_takes_the_selected_row_and_leaves_the_rest_one_alone()
    {
        // The one row of this batch whose absence is visible without a mutation: a resting tab forwards the secondary
        // ink and the selected row forwards the primary one, so the two cells disagree (#88's three indistinguishable
        // rows could only be probed by re-pointing). Both halves are asserted because a fact that only reads the
        // selected tab would also hold if the row were written for every tab.
        _fixture.Run(() =>
        {
            var view = MountTabs();
            AssertRow(view.TabItems[0], "TabViewItemHeaderForeground");

            view.SelectedIndex = 0;
            PixelHarness.Settle(30);
            AssertRow(view.TabItems[0], "TabViewItemHeaderForegroundSelected");
            AssertRow(view.TabItems[1], "TabViewItemHeaderForeground");
        });
    }

    [Fact]
    public void A_selected_tab_icon_takes_the_selected_row_written_for_its_part()
    {
        // The other half of the same markup line: `<Setter TargetName="IconHost" …>`, which writes a part rather than
        // the control. Whether this one can be falsified at all is the question the paired deletion mutant answers -
        // the part may simply inherit the primary ink the row above already put on the control.
        _fixture.Run(() =>
        {
            var view = MountTabs();
            var first = view.TabItems[0];
            first.Icon = new TextBlock { Text = "#" };
            PixelHarness.Settle(30);
            AssertRow(IconHost(first), "TabViewItemIconForeground");

            view.SelectedIndex = 0;
            PixelHarness.Settle(30);
            AssertRow(IconHost(first), "TabViewItemIconForegroundSelected");
        });
    }

    private static ContentControl IconHost(FluentTabViewItem item) =>
        (ContentControl)(PixelHarness.Named(item, "IconHost")
            ?? throw new InvalidOperationException("No part named IconHost."));

    [Fact]
    public void A_disabled_tab_icon_takes_the_disabled_row_written_for_its_part()
    {
        _fixture.Run(() =>
        {
            var view = MountTabs();
            var first = view.TabItems[0];
            first.Icon = new TextBlock { Text = "#" };
            PixelHarness.Settle(30);
            AssertRow(IconHost(first), "TabViewItemIconForeground");

            first.IsEnabled = false;
            PixelHarness.Settle(30);
            AssertRow(IconHost(first), "TabViewItemIconForegroundDisabled");
        });
    }

    [Fact]
    public void A_disabled_tab_close_button_takes_the_disabled_row_written_for_its_part()
    {
        // Which of the two rows writing this property is the writer is answered by the paired mutants, not by this
        // fact passing: taking the tab item template's row away reddens it, while taking away the close button's own
        // style row (`TabViewButtonForegroundDisabled`, TabView.jalxaml:71) leaves it green - so the style row never
        // lands and only the template row is claimed here. audits/foreground.md 7.6g records that asymmetry.
        _fixture.Run(() =>
        {
            var view = MountTabs();
            var first = view.TabItems[0];
            var close = (Button)(PixelHarness.Named(first, "CloseButton")
                ?? throw new InvalidOperationException("No part named CloseButton."));
            AssertRow(close, "TabViewItemHeaderCloseButtonForeground");

            first.IsEnabled = false;
            PixelHarness.Settle(30);
            AssertRow(close, "TabViewItemHeaderDisabledCloseButtonForeground");
        });
    }

    /// <summary>
    /// What the cell must move: the control's own <c>Foreground</c>, because both cells here are written without a
    /// <c>TargetName</c> and the label is generated by the presenter, which follows the control (#87 measured both
    /// halves of that). Reading the label instead would answer a different question - a disabled control's generated
    /// text carries a framework local value that outranks our cells.
    /// </summary>
    private static void AssertRow(Control carrier, string rowKey)
    {
        var row = Resource(rowKey);
        Assert.True(
            ReferenceEquals(row, carrier.Foreground),
            $"{carrier.GetType().Name} reads {Hex(carrier.Foreground)}, not the {Hex(row)} that " +
            $"{rowKey} carries - so the cell naming that row is not what writes this control.");
    }

    private static T Mount<T>(T control) where T : Control
    {
        PixelHarness.Build(control, 200, 40);
        PixelHarness.Settle(30);
        return control;
    }

    /// <summary>
    /// Three tabs, none of them selected: the row under test is a state cell, so the fact has to start on the far
    /// side of it and drive the state over.
    /// </summary>
    private static FluentTabView MountTabs()
    {
        var view = new FluentTabView();
        for (var index = 0; index < 3; index++)
        {
            view.TabItems.Add(new FluentTabViewItem
            {
                Header = $"tab {index + 1}",
                Content = new TextBlock { Text = $"body {index + 1}" },
                Icon = new TextBlock { Text = "#" },
            });
        }

        PixelHarness.Build(view, 420, 160);
        PixelHarness.Settle(30);
        return view;
    }

    private static Style Keyed(string key) =>
        Application.Current!.TryFindResource(key) as Style
            ?? throw new InvalidOperationException($"{key} resolved to no style.");

    private static Brush Resource(string key) =>
        Application.Current!.TryFindResource(key) as Brush
            ?? throw new InvalidOperationException($"{key} resolved to no brush.");

    private static string Hex(Brush? brush) =>
        brush is SolidColorBrush solid ? PixelHarness.Hex(solid.Color) : brush?.GetType().Name ?? "<null>";
}
