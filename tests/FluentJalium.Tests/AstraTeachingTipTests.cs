using System.Windows.Input;
using FluentJalium.Controls;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using Jalium.UI.Shapes;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// TeachingTip (stage 4, fifth slice). The runtime ships no such control
/// (docs/astra/adaptation/s0v-runtime-type-inventory-raw.txt), so this one is ours, and the two questions this
/// class exists to answer are the ones a native re-template never has: does a Popup inside a template build and
/// realize at all, and is the placement arithmetic the type does landing where the tail says it is.
/// <para>
/// Everything is read off an opened tip. A tip whose popup never opened has no realized card - probe mode popup
/// saw the child appear only after IsOpen went true - so the helper opens the subject and searches from the host
/// window rather than from the control, because the card lives under OverlayLayer &gt; PopupRoot, not under the
/// tip's own subtree.
/// </para>
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraTeachingTipTests : IDisposable
{
    private readonly List<FluentTeachingTip> _opened = [];
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraTeachingTipTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    /// <summary>An open tip is a popup on the shared host window, so every fact has to close its own.</summary>
    public void Dispose()
    {
        _fixture.Run(() =>
        {
            foreach (var tip in _opened.Where(static opened => opened.IsOpen))
            {
                tip.IsOpen = false;
            }

            _opened.Clear();
            PixelHarness.Settle();
        });
    }

    /// <summary>
    /// The five brush rows alias palette instances rather than copies, so a theme flip follows them and an app
    /// that overrides the palette moves the tip with it.
    /// </summary>
    [Theory]
    [InlineData("TeachingTipBackgroundBrush", "SolidBackgroundFillColorTertiaryBrush")]
    [InlineData("TeachingTipBorderBrush", "SurfaceStrokeColorDefaultBrush")]
    [InlineData("TeachingTipForegroundBrush", "TextFillColorPrimaryBrush")]
    [InlineData("TeachingTipTitleForegroundBrush", "TextFillColorPrimaryBrush")]
    [InlineData("TeachingTipSubtitleForegroundBrush", "TextFillColorPrimaryBrush")]
    public void An_alias_row_hands_back_the_palette_instance_it_aliases(string alias, string paletteKey)
    {
        _fixture.Run(() => Assert.Same(Res(paletteKey), Res(alias)));
    }

    /// <summary>
    /// The sixteen geometry rows at upstream's values. The pairing this batch had to read rather than infer is
    /// that a row named for a placement clears the edge the tail sits on, not the edge it is named after: Top
    /// means "the card is above the target", so its tail hangs from the bottom and 1,1,1,0 is right.
    /// </summary>
    [Theory]
    [InlineData("TeachingTipButtonPanelMargin", "0,12,0,0")]
    [InlineData("TeachingTipRightButtonMargin", "4,12,0,0")]
    [InlineData("TeachingTipLeftButtonMargin", "0,12,4,0")]
    [InlineData("TeachingTipMainContentPresentMargin", "0,12,0,0")]
    [InlineData("TeachingTipMainContentAbsentMargin", "0,0,0,0")]
    [InlineData("TeachingTipTitleStackPanelMarginWithFooterCloseButton", "0,0,0,0")]
    [InlineData("TeachingTipContentBorderThicknessTop", "1,1,1,0")]
    [InlineData("TeachingTipContentBorderThicknessBottom", "1,0,1,1")]
    [InlineData("TeachingTipContentBorderThicknessLeft", "1,1,0,1")]
    [InlineData("TeachingTipContentBorderThicknessRight", "0,1,1,1")]
    [InlineData("TeachingTipContentBorderThicknessUntargeted", "1,1,1,1")]
    [InlineData("TeachingTipTailPolygonMarginTop", "0,-1,0,0")]
    [InlineData("TeachingTipTailPolygonMarginBottom", "0,0,0,-1")]
    [InlineData("TeachingTipTailPolygonMarginLeft", "-1,0,0,0")]
    [InlineData("TeachingTipTailPolygonMarginRight", "0,0,-1,0")]
    [InlineData("TeachingTipContentMargin", "12,12,12,12")]
    public void A_geometry_row_carries_upstreams_value(string key, string thickness)
    {
        _fixture.Run(() => Assert.Equal(thickness, Application.Current!.TryFindResource(key)!.ToString()));
    }

    /// <summary>
    /// Twenty-nine of upstream's fifty TeachingTip row names are not published, and this names every one of them
    /// so a deferral cannot quietly become a hole in the dictionary. The GridLength pair is the only one with a
    /// measurement behind it rather than a decision: a {ThemeResource} on a ColumnDefinition.Width is dropped in
    /// silence by this reader (see the next two facts), so those two rows would have no consumer even in a full
    /// port. The eight x:Double rows are not merely unpublishable, they are unloadable - see
    /// docs/astra/adaptation/s1b-teachingtip-host-raw.txt section B.
    /// </summary>
    [Theory]
    [InlineData("TeachingTipTransientBackground")]
    [InlineData("TeachingTipTopHighlightBrush")]
    [InlineData("TeachingTipTopHighlightOffsetForBorder")]
    [InlineData("TeachingTipAlternateCloseButtonBackground")]
    [InlineData("TeachingTipAlternateCloseButtonBackgroundPointerOver")]
    [InlineData("TeachingTipAlternateCloseButtonBackgroundPressed")]
    [InlineData("TeachingTipAlternateCloseButtonBackgroundDisabled")]
    [InlineData("TeachingTipAlternateCloseButtonForeground")]
    [InlineData("TeachingTipAlternateCloseButtonForegroundPointerOver")]
    [InlineData("TeachingTipAlternateCloseButtonForegroundPressed")]
    [InlineData("TeachingTipAlternateCloseButtonForegroundDisabled")]
    [InlineData("TeachingTipAlternateCloseButtonBorderBrush")]
    [InlineData("TeachingTipAlternateCloseButtonBorderBrushPointerOver")]
    [InlineData("TeachingTipAlternateCloseButtonBorderBrushPressed")]
    [InlineData("TeachingTipAlternateCloseButtonBorderBrushDisabled")]
    [InlineData("TeachingTipAlternateCloseButtonBorderThickness")]
    [InlineData("TeachingTipAlternateCloseButtonSize")]
    [InlineData("TeachingTipAlternateCloseButtonGlyphSize")]
    [InlineData("TeachingTipIconPresenterMarginWithIcon")]
    [InlineData("TeachingTipIconPresenterMarginWithoutIcon")]
    [InlineData("TeachingTipTitleStackPanelMarginWithHeaderCloseButton")]
    [InlineData("TeachingTipTailShortSideLength")]
    [InlineData("TeachingTipTailMargin")]
    [InlineData("TeachingTipMinHeight")]
    [InlineData("TeachingTipMaxHeight")]
    [InlineData("TeachingTipMinWidth")]
    [InlineData("TeachingTipMaxWidth")]
    [InlineData("TeachingTipTopHighlightHeight")]
    [InlineData("TeachingTipBorderThickness")]
    public void A_row_with_no_consumer_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(Application.Current!.TryFindResource(key)));
    }

    /// <summary>The five tail bands of our own template, read back where the lookup above could not reach.</summary>
    [Fact]
    public void The_tail_grid_bands_are_upstreams_eight_ten_star_ten_eight()
    {
        _fixture.Run(() =>
        {
            var tip = Opened();
            var bands = ColumnWidths((Panel)VisualTreeHelper.GetParent(Card(tip))!);
            Assert.Equal("8 | 10 | * | 10 | 8", string.Join(" | ", bands));
        });
    }

    /// <summary>The part names the template is written against - the same list upstream's states address.</summary>
    [Fact]
    public void An_opened_tip_has_built_upstreams_part_tree()
    {
        _fixture.Run(() =>
        {
            var tip = Opened();
            foreach (var name in new[] { "TailOcclusionGrid", "ContentRootGrid", "ContentRootSurface", "TitlesStackPanel", "TitleTextBlock", "SubtitleTextBlock", "MainContentPresenter", "ButtonPanel", "PART_ActionButton", "PART_CloseButton", "TailPolygon" })
            {
                Assert.NotNull(Find(name));
            }

            Assert.NotNull(Container());
            Assert.NotNull(tip.Template);
            Assert.IsType<Popup>(OpenedPopup());
        });
    }

    /// <summary>
    /// The card's own geometry, read off the element that now holds it. Before the attribute sweep these four rows
    /// sat on <c>ContentRootGrid</c>: a Grid has no CornerRadius, BorderBrush or BorderThickness member, so the
    /// tip drew square and edgeless while every row still counted as consumed (spike/AttributeSweep,
    /// docs/astra/adaptation/00 S1-g). The layout element keeps its name; the painting moved to a Border under it.
    /// The last row reads the property <c>Grid</c> declares - <c>Control.BackgroundProperty</c> is a different
    /// dependency property on a type this element is not, and a first draft of this assertion read that one and
    /// passed on the broken markup (same class as S1-g item 8).
    /// </summary>
    [Fact]
    public void The_card_paints_on_a_border_that_can_hold_its_radius()
    {
        _fixture.Run(() =>
        {
            var tip = Opened();
            var surface = (Border)Find("ContentRootSurface")!;
            Assert.Multiple(
                () => Assert.Equal((CornerRadius)Application.Current!.TryFindResource("OverlayCornerRadius")!, surface.CornerRadius),
                () => Assert.Same(Application.Current!.TryFindResource("TeachingTipBackgroundBrush"), surface.Background),
                () => Assert.Equal(tip.BorderThickness, surface.BorderThickness),
                () => Assert.Null(((Grid)Find("ContentRootGrid")!).Background));
        });
    }

    /// <summary>
    /// A tip takes no room where it is placed, and still realizes its card. Gated rather than assumed: the first
    /// draft of this control carried a MeasureOverride that returned nothing on the strength of a probe line
    /// reading "320x168", which turned out to be the realized container inside the popup window and not the
    /// layout the tip claimed. Running the same question against both builds of that override
    /// (spike/TeachingTipProbe mode room) put the rule under a stacked tip at y=20 closed, open and open with a
    /// target either way, so the override is gone and this fact is what keeps the template's root a Popup.
    /// </summary>
    [Fact]
    public void A_tip_holds_no_layout_room_and_still_realizes_its_card()
    {
        _fixture.Run(() =>
        {
            var closed = Anchored();
            Assert.True(closed.DesiredSize.Width < 1 && closed.DesiredSize.Height < 1,
                $"a closed tip measured {closed.DesiredSize:0.##}");

            // DesiredSize is not enough on its own, because a stretched element's ActualWidth is the slot its
            // panel gave it rather than the room it took from a neighbour - a tip inside a Grid cell reads
            // 820x620 there with a zero desired size. A stack is what the override used to move.
            var above = new Border { Height = 20 };
            var tip = new FluentTeachingTip
            {
                Title = "Three shortcuts you already know",
                Subtitle = "Hold Ctrl and press a key.",
                Content = "The editor keeps a list of them under Help, which is long enough to measure 168 tall.",
            };
            var below = new Border { Height = 20 };
            var stack = new StackPanel();
            stack.Children.Add(above);
            stack.Children.Add(tip);
            stack.Children.Add(below);
            _current = tip;
            PixelHarness.Build(stack, 820, 620);
            PixelHarness.Settle();
            tip.IsOpen = true;
            PixelHarness.Settle();
            _opened.Add(tip);

            var at = below.TranslatePoint(new Point(), stack).Y;
            Assert.Multiple(
                () => Assert.True(at is >= 19 and <= 21, $"the tip pushed the rule under it down to {at:0.##}"),
                () => Assert.True(Card(tip).ActualWidth >= 300 && Card(tip).ActualHeight >= 100,
                    $"the card realized {Card(tip).ActualWidth:0.##}x{Card(tip).ActualHeight:0.##}"));
        });
    }

    /// <summary>
    /// Each side of the card is a different tail: the row, column, points and alignment upstream's own
    /// PlacementStates write, read back off the polygon the built tree holds.
    /// </summary>
    [Theory]
    [InlineData(FluentTeachingTipPlacementMode.Top, "4", "2", "0,0 10,10 20,0", "1,1,1,0")]
    [InlineData(FluentTeachingTipPlacementMode.Bottom, "0", "2", "0,10 10,0 20,10", "1,0,1,1")]
    [InlineData(FluentTeachingTipPlacementMode.Left, "2", "4", "0,0 10,10 0,20", "1,1,0,1")]
    [InlineData(FluentTeachingTipPlacementMode.Right, "2", "0", "10,0 0,10 10,20", "0,1,1,1")]
    [InlineData(FluentTeachingTipPlacementMode.Center, "4", "2", "0,0 10,10 20,0", "1,1,1,0")]
    public void A_placement_state_moves_the_tail_and_opens_the_edge_it_sits_on(
        FluentTeachingTipPlacementMode placement,
        string row,
        string column,
        string points,
        string borderThickness)
    {
        _fixture.Run(() =>
        {
            var tip = Opened(placement: placement);
            var tail = (Polygon)Find("TailPolygon")!;
            Assert.Multiple(
                () => Assert.Equal(placement, tip.EffectivePlacement),
                () => Assert.Equal(row, Grid.GetRow(tail).ToString()),
                () => Assert.Equal(column, Grid.GetColumn(tail).ToString()),
                () => Assert.Equal(points, tail.Points!.ToString()),
                () => Assert.Equal(borderThickness, tip.BorderThickness.ToString()));
        });
    }

    /// <summary>
    /// A tip with nothing to point at draws no tail and keeps all four edges, whatever its placement says. This
    /// is the one cell that has to outrank the placement cells, and the ordering is what makes it do so.
    /// </summary>
    [Fact]
    public void An_untargeted_tip_has_no_tail_and_a_closed_border()
    {
        _fixture.Run(() =>
        {
            var tip = Opened(target: false);
            Assert.Multiple(
                () => Assert.Equal(Visibility.Collapsed, ((Polygon)Find("TailPolygon")!).Visibility),
                () => Assert.Equal("1,1,1,1", tip.BorderThickness.ToString()));
        });
    }

    /// <summary>TailVisibility=Collapsed is a preference the placement cannot overrule.</summary>
    [Fact]
    public void A_tip_can_be_asked_to_draw_no_tail_and_a_target_cannot_argue()
    {
        _fixture.Run(() =>
        {
            var tip = Opened(tail: FluentTeachingTipTailVisibility.Collapsed);
            Assert.Equal(Visibility.Collapsed, ((Polygon)Find("TailPolygon")!).Visibility);
        });
    }

    /// <summary>The two text lines and the body, each collapsed or re-margined the way upstream's groups do.</summary>
    [Fact]
    public void Absent_title_subtitle_and_content_take_their_state_with_them()
    {
        _fixture.Run(() =>
        {
            var full = Opened();
            Assert.Multiple(
                () => Assert.Equal(Visibility.Visible, ((TextBlock)Find("TitleTextBlock")!).Visibility),
                () => Assert.Equal(Visibility.Visible, ((TextBlock)Find("SubtitleTextBlock")!).Visibility),
                () => Assert.Equal("0,12,0,0", ((FrameworkElement)Find("MainContentPresenter")!).Margin.ToString()));

            var bare = Opened(title: null, subtitle: "", content: null);
            Assert.Multiple(
                () => Assert.Equal(Visibility.Collapsed, ((TextBlock)Find("TitleTextBlock")!).Visibility),
                () => Assert.Equal(Visibility.Collapsed, ((TextBlock)Find("SubtitleTextBlock")!).Visibility),
                () => Assert.Equal("0,0,0,0", ((FrameworkElement)Find("MainContentPresenter")!).Margin.ToString()));
        });
    }

    /// <summary>
    /// ButtonsStates over the AllVisible resting markup: a missing button collapses, and the survivor takes the
    /// panel's own margin and both columns, which is what upstream writes onto it.
    /// </summary>
    [Fact]
    public void A_lone_button_takes_the_row_the_pair_would_have_shared()
    {
        _fixture.Run(() =>
        {
            var pair = Opened();
            Assert.Multiple(
                () => Assert.Equal(Visibility.Visible, ((Button)Find("PART_ActionButton")!).Visibility),
                () => Assert.Equal("0,12,4,0", ((Button)Find("PART_ActionButton")!).Margin.ToString()),
                () => Assert.Equal("4,12,0,0", ((Button)Find("PART_CloseButton")!).Margin.ToString()));

            var actionOnly = Opened(close: null);
            Assert.Multiple(
                () => Assert.Equal(Visibility.Collapsed, ((Button)Find("PART_CloseButton")!).Visibility),
                () => Assert.Equal(2, Grid.GetColumnSpan((Button)Find("PART_ActionButton")!)),
                () => Assert.Equal("0,12,0,0", ((Button)Find("PART_ActionButton")!).Margin.ToString()));

            var closeOnly = Opened(action: null);
            Assert.Multiple(
                () => Assert.Equal(Visibility.Collapsed, ((Button)Find("PART_ActionButton")!).Visibility),
                () => Assert.Equal(2, Grid.GetColumnSpan((Button)Find("PART_CloseButton")!)),
                () => Assert.Equal("0,12,0,0", ((Button)Find("PART_CloseButton")!).Margin.ToString()));

            Opened(action: null, close: null);
            Assert.Equal(Visibility.Collapsed, ((FrameworkElement)Find("ButtonPanel")!).Visibility);
        });
    }

    /// <summary>
    /// The close button's whole contract in one read-back: it raises, it closes, and the action button does not.
    /// Activation goes through the automation peer because that is the path mouse, touch and keyboard share.
    /// </summary>
    [Fact]
    public void The_close_button_closes_and_the_action_button_only_reports()
    {
        _fixture.Run(() =>
        {
            var tip = Opened();
            var actions = 0;
            var closes = 0;
            tip.ActionButtonClick += (_, _) => actions++;
            tip.CloseButtonClick += (_, _) => closes++;

            Invoke((Button)Find("PART_ActionButton")!);
            PixelHarness.Settle();
            Assert.Multiple(
                () => Assert.Equal(1, actions),
                () => Assert.Equal(0, closes),
                () => Assert.True(tip.IsOpen, "the action button closed a tip it was never meant to close"));

            Invoke((Button)Find("PART_CloseButton")!);
            PixelHarness.Settle();
            Assert.Multiple(
                () => Assert.Equal(1, closes),
                () => Assert.False(tip.IsOpen));
        });
    }

    /// <summary>Both buttons carry their command, so a tip can act without an event handler.</summary>
    [Fact]
    public void A_tip_button_invokes_the_command_its_property_named()
    {
        _fixture.Run(() =>
        {
            var invoked = 0;
            var tip = Opened(action: "Keep");
            tip.ActionButtonCommand = new DelegateCommand(() => invoked++);
            PixelHarness.Settle();

            Invoke((Button)Find("PART_ActionButton")!);
            PixelHarness.Settle();
            Assert.Equal(1, invoked);
        });
    }

    /// <summary>
    /// Opening and closing are properties, not only side effects of a button: the popup follows IsOpen, and the
    /// two events fire once each across the pair.
    /// </summary>
    [Fact]
    public void The_popup_opens_and_closes_with_the_property_and_says_so_once_each_way()
    {
        _fixture.Run(() =>
        {
            var tip = Anchored();
            var opened = 0;
            var closed = 0;
            tip.Opened += (_, _) => opened++;
            tip.Closed += (_, _) => closed++;
            Assert.False(OpenedPopup()!.IsOpen);

            tip.IsOpen = true;
            PixelHarness.Settle();
            Assert.True(OpenedPopup()!.IsOpen);

            tip.IsOpen = false;
            PixelHarness.Settle();
            Assert.Multiple(
                () => Assert.Equal(1, opened),
                () => Assert.Equal(1, closed),
                () => Assert.False(OpenedPopup()!.IsOpen));
        });
    }

    /// <summary>
    /// Where the card actually lands, asserted as the two invariants a tip has to keep rather than as a
    /// re-derivation of the arithmetic: the container's tail edge sits exactly on the anchor's near edge - which
    /// is what the 8 DIP bands make the offsets able to do - and the card is centred across it. The offsets are
    /// read off the popup the control wrote them on; the realized card's own size is the second half of the
    /// equation, so a wrong measurement, a wrong anchor or a cell that never applied all show up here.
    /// </summary>
    [Theory]
    [InlineData(FluentTeachingTipPlacementMode.Top)]
    [InlineData(FluentTeachingTipPlacementMode.Bottom)]
    [InlineData(FluentTeachingTipPlacementMode.Left)]
    [InlineData(FluentTeachingTipPlacementMode.Right)]
    public void The_cards_tail_edge_sits_on_the_anchors_edge(FluentTeachingTipPlacementMode placement)
    {
        _fixture.Run(() =>
        {
            var tip = Opened(placement: placement);
            var popup = OpenedPopup()!;
            var container = Container();

            // The anchor's own box is 100x40 from the origin the offsets are written against, so its near edge is
            // 0 or 100/40 and its centre is 50 or 20. Which side of the container has to meet it follows from the
            // placement, because the tail is on the far side from the card.
            var (tail, cross, anchorEdge, anchorCentre) = placement switch
            {
                FluentTeachingTipPlacementMode.Top => (popup.VerticalOffset + container.ActualHeight, popup.HorizontalOffset + container.ActualWidth / 2, 0d, 50d),
                FluentTeachingTipPlacementMode.Bottom => (popup.VerticalOffset, popup.HorizontalOffset + container.ActualWidth / 2, 40d, 50d),
                FluentTeachingTipPlacementMode.Left => (popup.HorizontalOffset + container.ActualWidth, popup.VerticalOffset + container.ActualHeight / 2, 0d, 20d),
                _ => (popup.HorizontalOffset, popup.VerticalOffset + container.ActualHeight / 2, 100d, 20d),
            };
            Assert.Multiple(
                () => Assert.Equal(placement, tip.EffectivePlacement),
                () => Assert.True(Math.Abs(tail - anchorEdge) < 1.5,
                    $"a card placed {placement} put its tail edge {tail:0.##} DIP from the anchors origin, expected {anchorEdge:0.#} (container {container.ActualWidth:0.##}x{container.ActualHeight:0.##})"),
                () => Assert.True(Math.Abs(cross - anchorCentre) < 1.5,
                    $"a card placed {placement} is not centred across the anchor: {cross:0.##} against {anchorCentre:0.#}"));
        });
    }

    /// <summary>
    /// The preferred side loses to a side that fits: an anchor 60 DIP from the top has no room above it for a
    /// 168 DIP card, so the tip goes below - and the tail moves with it, which is the point of writing
    /// EffectivePlacement instead of letting the template read the preference.
    /// </summary>
    [Fact]
    public void A_side_with_no_room_for_the_card_loses_to_one_that_has()
    {
        _fixture.Run(() =>
        {
            var tip = Opened(placement: FluentTeachingTipPlacementMode.Top);
            Assert.Equal(FluentTeachingTipPlacementMode.Top, tip.EffectivePlacement);

            var cramped = Opened(placement: FluentTeachingTipPlacementMode.Top, anchorY: 60);
            Assert.Multiple(
                () => Assert.Equal(FluentTeachingTipPlacementMode.Bottom, cramped.EffectivePlacement),
                () => Assert.True(Math.Abs(OpenedPopup()!.VerticalOffset - 40) < 1.5,
                    $"the flipped card did not move to the anchor's bottom edge: {OpenedPopup()!.VerticalOffset:0.##}"));
        });
    }

    /// <summary>
    /// Auto with a target resolves above it, which is the resolution ModernWpf's port of WinUI's layout makes.
    /// Center is the placement that keeps the tail on the bottom edge and drops the card onto the target, so it is
    /// the one case where both axes are centred instead of one being flush.
    /// </summary>
    [Fact]
    public void Auto_goes_above_a_target_and_center_sits_on_it()
    {
        _fixture.Run(() =>
        {
            var auto = Opened(placement: FluentTeachingTipPlacementMode.Auto);
            Assert.Equal(FluentTeachingTipPlacementMode.Top, auto.EffectivePlacement);

            var center = Opened(placement: FluentTeachingTipPlacementMode.Center);
            var popup = OpenedPopup()!;
            var container = Container();
            Assert.Multiple(
                () => Assert.Equal(FluentTeachingTipPlacementMode.Center, center.EffectivePlacement),
                () => Assert.True(Math.Abs(popup.HorizontalOffset + container.ActualWidth / 2 - 50) < 1.5,
                    $"centered across at {popup.HorizontalOffset + container.ActualWidth / 2:0.##}"),
                () => Assert.True(Math.Abs(popup.VerticalOffset + container.ActualHeight / 2 - 20) < 1.5,
                    $"centered down at {popup.VerticalOffset + container.ActualHeight / 2:0.##}"));
        });
    }

    /// <summary>
    /// The card is capped the way upstream caps it, and the cap is on the box the popup hosts - so the 8 DIP tail
    /// bands sit outside the 336 the caller asked for and the horizontal offsets have to account for them.
    /// </summary>
    [Fact]
    public void The_card_keeps_upstreams_size_box()
    {
        _fixture.Run(() =>
        {
            var tip = Opened();
            var container = Container();
            var card = Card(tip);

            // Upstream caps the box the popup hosts (TeachingTip.xaml:297), not the control, so the control's own
            // MinWidth/MaxWidth are untouched by our style and the numbers have to be read where they sit.
            Assert.Multiple(
                () => Assert.Equal("320", ((FrameworkElement)Find("TailOcclusionGrid")!).MinWidth.ToString()),
                () => Assert.Equal("336", ((FrameworkElement)Find("TailOcclusionGrid")!).MaxWidth.ToString()),
                () => Assert.True(container.ActualWidth <= 336 + 16 && container.ActualWidth >= 320,
                    $"the container measured {container.ActualWidth:0.##} against a 320..352 box"),
                () => Assert.True(Math.Abs(container.ActualWidth - card.ActualWidth - 16) < 1.5,
                    $"card {card.ActualWidth:0.##} inside container {container.ActualWidth:0.##}: the bands are not 8 on each side"),
                () => Assert.True(Math.Abs(container.ActualHeight - card.ActualHeight - 16) < 1.5,
                    $"card {card.ActualHeight:0.##} inside container {container.ActualHeight:0.##}"));
        });
    }

    /// <summary>
    /// The surface row has to reach the card's pixels. A tip paints outside the control that owns it, so a row
    /// that resolves in the dictionary but not on the surface would be an alias nobody sees; the sentinel goes on
    /// the palette row before the tip resolves, because a control that read its brushes first keeps them.
    /// </summary>
    [Fact]
    public void The_card_paints_the_row_its_alias_hands_back()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("SolidBackgroundFillColorTertiaryBrush", Color.FromRgb(0xFF, 0x00, 0xFF));
            try
            {
                var card = Card(Opened(content: null, action: null, close: null, title: "Sentinel", subtitle: null));
                if (card.ActualWidth <= 0 || card.ActualHeight <= 0)
                {
                    throw new InvalidOperationException(
                        $"the realized card has no layout to sample ({card.ActualWidth}x{card.ActualHeight}); " +
                        "that is the host limit recorded in docs/astra/audits/menu-flyout.md 4c, and this claim stays unproven.");
                }

                var centre = PixelHarness.Hex(PixelHarness.PixelAt(card, (int)(card.ActualWidth / 2), (int)(card.ActualHeight / 2)));
                var edge = PixelHarness.Hex(PixelHarness.PixelAt(card, (int)(card.ActualWidth / 2), 0));
                Assert.Multiple(
                    () => Assert.Equal("#FF00FF", centre, StringComparer.OrdinalIgnoreCase),
                    () => Assert.DoesNotContain("207245", centre + edge, StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("SolidBackgroundFillColorTertiaryBrush", null);
            }
        });
    }

    // ---------- helpers ----------

    private FluentTeachingTip Opened(
        FluentTeachingTipPlacementMode placement = FluentTeachingTipPlacementMode.Top,
        bool target = true,
        FluentTeachingTipTailVisibility? tail = null,
        string? title = "Three shortcuts you already know",
        string? subtitle = "Hold Ctrl and press a key.",
        string? content = "The editor keeps a list of them under Help.",
        string? action = "Got it",
        string? close = "Dismiss",
        double anchorY = 300)
    {
        var tip = Anchored(target, anchorY: anchorY);
        tip.PreferredPlacement = placement;
        if (tail is not null)
        {
            tip.TailVisibility = tail.Value;
        }

        tip.Title = title;
        tip.Subtitle = subtitle;
        tip.Content = content;
        tip.ActionButtonContent = action;
        tip.CloseButtonContent = close;
        PixelHarness.Settle();
        tip.IsOpen = true;
        PixelHarness.Settle();
        _opened.Add(tip);
        Assert.True(tip.IsOpen);
        return tip;
    }

    /// <summary>
    /// Places a closed tip next to a 100x40 anchor at a known spot, and remembers it as the subject the part
    /// lookups below read. Any tip an earlier call in the same fact opened is shut first: a name is only unique
    /// per open tip, and a popup's content outlives the surface that hosted it.
    /// </summary>
    private FluentTeachingTip Anchored(bool target = true, double anchorX = 300, double anchorY = 300)
    {
        foreach (var previous in _opened.Where(static opened => opened.IsOpen))
        {
            previous.IsOpen = false;
        }

        var anchor = new Border
        {
            Width = 100,
            Height = 40,
            Background = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(anchorX, anchorY, 0, 0),
        };
        var tip = new FluentTeachingTip();
        if (target)
        {
            tip.Target = anchor;
        }

        _current = tip;
        var surface = new Grid();
        surface.Children.Add(anchor);
        surface.Children.Add(tip);
        PixelHarness.Build(surface, 820, 620);
        PixelHarness.Settle();
        return tip;
    }

    private FrameworkElement Card(FluentTeachingTip tip) =>
        Find("ContentRootGrid") ?? throw new InvalidOperationException("The opened tip built no card.");

    /// <summary>
    /// The tip the last helper placed, which is what every part lookup below is anchored to. Walking from the
    /// host window instead reaches the PART_Popup of whichever tip some earlier fact left open, because a popup
    /// with a placement target realizes its content in its own top-level PopupWindow
    /// (Border &lt; PopupRoot &lt; PopupWindow, spike/TeachingTipProbe mode tip) and that window is not a child of
    /// this one. The popup element itself is found by walking down from the control.
    /// </summary>
    private FluentTeachingTip? _current;

    private Popup? OpenedPopup() => PixelHarness.Named(_current!, "PART_Popup") as Popup;

    /// <summary>
    /// A part below the card's own root. The lookup starts at the popup's child rather than at the host window:
    /// a Popup with a placement target realizes its content in its own top-level PopupWindow
    /// (Border &lt; PopupRoot &lt; PopupWindow, spike/TeachingTipProbe mode tip), which is not a child of the host,
    /// and a window walk would instead reach whichever earlier fact left one open. The child is the Container
    /// itself - a name lookup only ever tests children, so that one part is read as the child, not by name.
    /// </summary>
    private FrameworkElement? Find(string name) =>
        OpenedPopup()?.Child is DependencyObject child ? PixelHarness.Named(child, name) : null;

    private FrameworkElement Container() =>
        (FrameworkElement)(OpenedPopup()?.Child ?? throw new InvalidOperationException("The opened popup hosts no card."));

    private static Panel FirstChildGrid(DependencyObject root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is Panel { Visibility: Visibility.Visible } panel && ColumnWidths(panel).Count > 0)
            {
                return panel;
            }

            if (child is not null && VisualTreeHelper.GetChildrenCount(child) > 0)
            {
                try
                {
                    return FirstChildGrid(child);
                }
                catch (InvalidOperationException)
                {
                    // Keep looking: a template has several panels and only one of them has the columns.
                }
            }
        }

        throw new InvalidOperationException("No panel with column definitions was built under this subject.");
    }

    private static List<string> ColumnWidths(Panel panel)
    {
        // Grid exposes its definitions; a Decorator or StackPanel does not, which is how the caller knows to move on.
        var definitions = panel.GetType().GetProperty("ColumnDefinitions")?.GetValue(panel) as System.Collections.IEnumerable;
        return definitions?.Cast<object>()
                   .Select(definition => definition.GetType().GetProperty("Width")?.GetValue(definition)?.ToString() ?? "?")
                   .ToList() ?? [];
    }

    private static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();

    private static Brush Res(string key) => (Brush)Application.Current!.TryFindResource(key)!;

    private sealed class DelegateCommand(Action action) : ICommand
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
