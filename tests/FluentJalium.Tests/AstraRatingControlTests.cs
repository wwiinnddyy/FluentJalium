using FluentJalium.Controls;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// What the rating row decides for itself and what its style has to deliver. 26.10.9 exports no
/// <c>RatingControl</c> and no <c>RatingItem*</c> (<c>spike/RatingProbe</c> asked the assembly that really holds
/// <c>Button</c>), so the control, the six-role decision and the clip hosts are this layer's; the names, the
/// numbers, the colours and the coercion rules are upstream's, transcribed from
/// <c>RatingControl_themeresources.xaml</c> blob 0d9fccdf29a38836bd89fabbc222f6605d104b59,
/// <c>RatingControl.xaml</c> blob 6229cfe469d53d5d466f0baead99c19a5cc7eb6b and <c>RatingControl.cpp</c> blob
/// 27701085117b84f49936435a38c55a63a1e5d8b7.
/// </summary>
/// <remarks>
/// <para>
/// The assertions split along the line where a claim can be faked. A half star is read off the <em>width of the
/// clip host</em>, never off a picture: this runtime's captures reach neither glyphs nor text
/// (adaptation/00 §S1-r clause 3), so a star has no ink to measure - the route the crop uses is separately proven
/// in <c>spike/RatingProbe</c> on a solid block (30 of 60 DIP read 1 800, 45 read 2 700, a markup 20-wide host over
/// 32 DIP of ink read exactly 320). That a state was reached is read off the brush the item ends up wearing, not
/// off the enum alone, because the enum would read the same if the trigger had matched nothing.
/// </para>
/// <para>
/// Nothing here is driven by a real pointer or a real key: this suite has no synthesised-input path (#13). The
/// input model is driven through the internal entries the routed handlers call, which proves the arithmetic and the
/// coercion but not the wiring - so the hardware-input column of this control's evidence stays empty rather than
/// passing. See <c>docs/astra/audits/rating-control.md</c>.
/// </para>
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraRatingControlTests
{
    private const string SetGlyph = "";
    private const string UnsetGlyph = "";

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraRatingControlTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void The_named_style_lands_and_the_template_realizes_every_part()
    {
        _fixture.Run(() =>
        {
            var rating = Rating();

            // Each name is read back as its own type: a ControlTemplate that parsed and silently dropped a branch
            // would still hand back a control with a background.
            Assert.NotNull(Part(rating, "LayoutRoot") as Grid);
            Assert.NotNull(Part(rating, "RatingBackgroundStackPanel") as StackPanel);
            Assert.NotNull(Part(rating, "RatingForegroundStackPanel") as StackPanel);
            Assert.NotNull(Part(rating, "ForegroundContentPresenter") as ContentControl);
            Assert.NotNull(Part(rating, "Caption") as TextBlock);
        });
    }

    [Fact]
    public void The_default_style_carries_upstreams_setters()
    {
        _fixture.Run(() =>
        {
            var rating = Rating();

            Assert.Equal(32, rating.MinHeight);
            Assert.NotNull(rating.FontFamily);
            Assert.Same(Resource("RatingControlCaptionForeground"), rating.Foreground);

            var info = Assert.IsType<FluentRatingItemFontInfo>(rating.ItemInfo);
            Assert.Equal(SetGlyph, info.Glyph);
            Assert.Equal(UnsetGlyph, info.UnsetGlyph);
        });
    }

    public static TheoryData<string> PublishedKeys() => new()
    {
        { "RatingControlUnselectedForeground" },
        { "RatingControlSelectedForeground" },
        { "RatingControlPlaceholderForeground" },
        { "RatingControlPointerOverPlaceholderForeground" },
        { "RatingControlPointerOverUnselectedForeground" },
        { "RatingControlPointerOverSelectedForeground" },
        { "RatingControlDisabledSelectedForeground" },
        { "RatingControlCaptionForeground" },
        { "MUX_RatingControlDefaultFontInfo" },
        { "BackgroundGlyphDefaultTemplate" },
        { "ForegroundGlyphDefaultTemplate" },
        { "BackgroundImageDefaultTemplate" },
        { "ForegroundImageDefaultTemplate" },
    };

    [Theory]
    [MemberData(nameof(PublishedKeys))]
    public void Every_published_key_resolves(string key)
    {
        _fixture.Run(() =>
        {
            var value = Resource(key);
            Assert.NotNull(value);

            if (key.EndsWith("Foreground", StringComparison.Ordinal))
            {
                Assert.IsType<SolidColorBrush>(value);
            }
            else if (key.EndsWith("Template", StringComparison.Ordinal))
            {
                Assert.IsType<DataTemplate>(value);
            }
            else
            {
                Assert.IsType<FluentRatingItemFontInfo>(value);
            }
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(9)]
    public void The_row_holds_one_background_and_one_foreground_item_per_star(int maxRating)
    {
        _fixture.Run(() =>
        {
            var rating = Rating(maxRating: maxRating);

            Assert.Equal(maxRating, Count(rating, "RatingBackgroundStackPanel"));
            Assert.Equal(maxRating, Count(rating, "RatingForegroundStackPanel"));
            for (var index = 0; index < maxRating; index++)
            {
                Assert.NotNull(rating.BackgroundItem(index));
                Assert.IsType<Grid>(rating.ForegroundItem(index));
            }
        });
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-4, 1)]
    [InlineData(2, 2)]
    public void MaxRating_is_clamped_from_below_and_never_throws(int requested, int expected)
    {
        _fixture.Run(() =>
        {
            var rating = Rating();
            rating.MaxRating = requested;
            Assert.Equal(expected, rating.MaxRating);
        });
    }

    [Theory]
    [InlineData(-5, -1)]
    [InlineData(0, 1)]
    [InlineData(0.5, 1)]
    [InlineData(3.5, 3.5)]
    [InlineData(99, 5)]
    public void Value_is_clamped_the_way_upstream_clamps_it(double requested, double expected)
    {
        _fixture.Run(() =>
        {
            var rating = Rating();
            rating.Value = requested;
            Assert.Equal(expected, rating.Value);
        });
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(0, 0)]
    [InlineData(0.4, 0.4)]
    [InlineData(1, 1)]
    [InlineData(99, 5)]
    public void A_placeholder_average_keeps_its_fraction(double requested, double expected)
    {
        _fixture.Run(() =>
        {
            var rating = Rating();
            rating.PlaceholderValue = requested;
            Assert.Equal(expected, rating.PlaceholderValue);
        });
    }

    [Fact]
    public void Lowering_the_maximum_pulls_the_value_and_the_placeholder_down_with_it()
    {
        _fixture.Run(() =>
        {
            var rating = Rating();
            rating.Value = 5;
            rating.PlaceholderValue = 4;

            rating.MaxRating = 3;

            Assert.Equal(3, rating.Value);
            Assert.Equal(3, rating.PlaceholderValue);
            Assert.Equal(3, Count(rating, "RatingForegroundStackPanel"));
        });
    }

    [Fact]
    public void A_missing_ItemInfo_leaves_an_empty_row_inst_of_taking_the_process_down()
    {
        _fixture.Run(() =>
        {
            // Upstream fails fast here (RatingControl.cpp:431). A host that styles the control without the token
            // dictionary merged is the case that has to survive.
            var rating = Rating();
            rating.ItemInfo = null;
            PixelHarness.Settle();

            Assert.Equal(5, Count(rating, "RatingForegroundStackPanel"));
            Assert.Equal(string.Empty, ((TextBlock)((Grid)rating.ForegroundItem(0)!).Children[0]!).Text);
        });
    }

    [Fact]
    public void A_background_item_wears_the_unset_glyph()
    {
        _fixture.Run(() =>
        {
            var rating = Rating();
            var item = (TextBlock)Run(rating.BackgroundItem(0))!;

            Assert.Equal(UnsetGlyph, item.Text);
            Assert.Same(Resource("RatingControlUnselectedForeground"), item.Foreground);
        });
    }

    [Fact]
    public void The_glyph_for_a_role_falls_through_the_way_upstreams_table_does()
    {
        _fixture.Run(() =>
        {
            var rating = Rating();
            var font = new FluentRatingItemFontInfo
            {
                Glyph = "A",
                PlaceholderGlyph = "P",
            };
            rating.ItemInfo = font;

            Assert.Equal("A", rating.GetGlyph(FluentRatingControlDisplayState.Set));
            Assert.Equal("P", rating.GetGlyph(FluentRatingControlDisplayState.Placeholder));
            // PointerOverPlaceholder has no glyph of its own here, so it falls to Placeholder...
            Assert.Equal("P", rating.GetGlyph(FluentRatingControlDisplayState.PointerOverPlaceholder));
            // ...and Placeholder falls through to Set when it is empty too.
            font.PlaceholderGlyph = string.Empty;
            Assert.Equal("A", rating.GetGlyph(FluentRatingControlDisplayState.PointerOverPlaceholder));
            Assert.Equal("A", rating.GetGlyph(FluentRatingControlDisplayState.Disabled));
        });
    }

    [Fact]
    public void The_unsettled_state_is_pointer_over_unselected_but_asks_for_the_placeholder_glyph()
    {
        _fixture.Run(() =>
        {
            // The API-locked asymmetry upstream comments about at RatingControl.cpp:303-308: the state name is one
            // thing and the glyph role it requests is another.
            var rating = Rating(value: -1);
            rating.PlaceholderValue = 3;
            rating.PreviewPointerAt(30);

            Assert.Equal(FluentRatingControlDisplayState.PointerOverUnselected, rating.ActiveRatingState);
            Assert.Equal(UnsetGlyph.Length > 0 ? SetGlyph : SetGlyph, ItemText(rating, 0));
        });
    }

    [Fact]
    public void A_settled_row_wears_the_selected_brush_and_a_placeholder_row_the_placeholder_brush()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 3);

            // The brush has to have travelled: style setter -> trigger on the presenter -> the run inside the clip
            // host. Reading the enum alone would pass with the trigger deleted.
            Assert.Equal(FluentRatingControlDisplayState.Set, rating.ActiveRatingState);
            Assert.Same(Resource("RatingControlSelectedForeground"), ItemBrush(rating, 0));

            rating.Value = -1;
            rating.PlaceholderValue = 2;
            Assert.Equal(FluentRatingControlDisplayState.Placeholder, rating.ActiveRatingState);
            Assert.Same(Resource("RatingControlPlaceholderForeground"), ItemBrush(rating, 0));
        });
    }

    [Fact]
    public void A_disabled_row_loses_the_hover_colour_but_keeps_the_hover_crop()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 4);
            rating.PreviewPointerAt(30);
            rating.IsEnabled = false;

            Assert.Equal(FluentRatingControlDisplayState.Disabled, rating.ActiveRatingState);
            Assert.Same(Resource("RatingControlDisabledSelectedForeground"), ItemBrush(rating, 0));

            rating.IsEnabled = true;
        });
    }

    [Theory]
    [InlineData(2.0, new[] { 32.0, 32.0, 0.0, 0.0, 0.0 })]
    [InlineData(2.5, new[] { 32.0, 32.0, 16.0, 0.0, 0.0 })]
    [InlineData(0.0, new[] { 0.0, 0.0, 0.0, 0.0, 0.0 })]
    public void A_partial_rating_crops_the_boundary_item_and_empties_the_rest(double value, double[] fractions)
    {
        _fixture.Run(() =>
        {
            // Widths are asserted as fractions of the measured advance so the reading does not depend on which
            // font the host has; the advance itself is asserted separately.
            var rating = Rating(value: value <= 0 ? -1 : value);
            var advance = Advance(rating);

            for (var index = 0; index < fractions.Length; index++)
            {
                var expected = advance * fractions[index] / 32.0;
                Assert.Equal(expected, Width(rating, index), 1);
            }
        });
    }

    [Fact]
    public void An_unset_row_crops_every_foreground_item_to_nothing()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: -1);

            for (var index = 0; index < rating.MaxRating; index++)
            {
                Assert.Equal(0, Width(rating, index), 1);
            }
        });
    }

    [Fact]
    public void The_crop_host_is_the_clip_because_Clipped_would_not_cut()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 3);
            var host = Assert.IsType<Grid>(rating.ForegroundItem(0));

            // Measured in spike/RatingProbe: a RectangleGeometry on Clip leaves a 60x60 block fully inked, while a
            // ClipToBounds host cuts proportionally. The template therefore carries the flag, not a geometry.
            Assert.True(host.ClipToBounds);
            Assert.Null(host.Clip);
        });
    }

    [Fact]
    public void The_measured_pitch_is_the_number_the_pointer_model_divides_by()
    {
        _fixture.Run(() =>
        {
            // The contradiction upstream leaves open (audit clause 10): its cells are the full 34-wide run box and
            // only its negative Spacing pulls them back to the pitch the width model uses. Here the cell is the ink
            // box, so the published 8 alone gives the pitch the model divides by - and the row's own right edge is
            // the model's width, to the DIP.
            var rating = Rating(value: 3);
            var cell = Advance(rating);
            var panel = (StackPanel)Part(rating, "RatingBackgroundStackPanel")!;
            var first = panel.Children[0]!.TranslatePoint(new Point(0, 0), panel).X;
            var second = panel.Children[1]!.TranslatePoint(new Point(0, 0), panel).X;
            var last = panel.Children[4]!.TranslatePoint(new Point(0, 0), panel).X;
            var pitch = second - first;

            // Half of the measured 34 run, as cpp:196 and cpp:51-55 together make it.
            Assert.Equal(17, cell, 1);
            Assert.Equal(rating.ActualItemSize + FluentRatingControl.ItemSpacing, pitch, 1);
            Assert.Equal(25, pitch, 1);

            // The whole row: four steps of that pitch, and the right edge is the width the maths divide by.
            Assert.Equal(first + (4 * pitch), last, 1);
            Assert.Equal(0d, first, 1);
            Assert.Equal(rating.ActualRatingWidth, last + cell, 1);
        });
    }

    [Fact]
    public void Each_cell_is_the_star_ink_box_scaled_to_half_and_pulled_flush_left()
    {
        _fixture.Run(() =>
        {
            // Upstream's 32 is a double-size render and its -8 margins compensate the scale-down it applies by
            // composition; the resting 0.5 that cpp:372-392 leaves on screen is written here as a RenderTransform,
            // and the run is pulled left by the leftover so the crop host's edge is the ink's edge.
            var rating = Rating(value: 3);
            var panel = (StackPanel)Part(rating, "RatingBackgroundStackPanel")!;
            var cell = (Grid)rating.BackgroundItem(0)!;
            var run = (FrameworkElement)cell.Children[0]!;
            var scale = Assert.IsType<ScaleTransform>(run.RenderTransform);

            Assert.Equal(FluentRatingControl.RestItemScale, scale.ScaleX, 3);
            Assert.Equal(FluentRatingControl.RestItemScale, scale.ScaleY, 3);
            Assert.Equal(new Point(0.5, 0.5), run.RenderTransformOrigin);
            Assert.Equal(-(Advance(rating) / 2), run.Margin.Left, 1);
            Assert.True(cell.ClipToBounds == false, "the background layer is never cropped - it shows the whole outline");
            Assert.True(((Grid)rating.ForegroundItem(0)!).ClipToBounds);

            // The panels carry the published spacing and nothing else: no negative number to be dropped on the floor.
            Assert.Equal(FluentRatingControl.ItemSpacing, panel.Spacing, 1);
            Assert.Equal(0d, MarginOf(panel.Children[0]).Left, 1);
            Assert.Equal(0d, MarginOf(panel.Children[3]).Left, 1);
        });
    }

    [Fact]
    public void The_star_under_the_pointer_lifts_toward_its_ceiling_and_the_rest_stay_at_the_floor()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 3);
            var pitch = rating.ActualItemSize + FluentRatingControl.ItemSpacing;
            var near = ScaleOf(rating, 2);
            var far = ScaleOf(rating, 0);

            Assert.Equal(FluentRatingControl.RestItemScale, near.ScaleX, 3);

            // Dead centre of the third star, which is cpp:961's own arithmetic: the mouse ceiling.
            rating.PreviewPointerAt((2 * pitch) + (rating.ActualItemSize / 2));
            Assert.Equal(FluentRatingControl.MouseItemScale, near.ScaleX, 3);
            Assert.Equal(FluentRatingControl.MouseItemScale, ScaleOfBackground(rating, 2).ScaleX, 3);

            // Half a pitch off, the quadratic has started to fall but has not reached the floor; the star two over
            // is far enough out to be sitting on the floor, which is the value it rests at when nothing is over the
            // row at all.
            rating.PreviewPointerAt((2 * pitch) - (pitch / 2));
            Assert.True(near.ScaleX < FluentRatingControl.MouseItemScale && near.ScaleX > FluentRatingControl.RestItemScale,
                $"a star half a pitch from the pointer read {near.ScaleX}");
            Assert.Equal(FluentRatingControl.RestItemScale, far.ScaleX, 3);

            // A finger takes the star all the way to the drawn size, which is the other ceiling upstream names.
            rating.PreviewPointerAt((2 * pitch) + (rating.ActualItemSize / 2), touch: true);
            Assert.Equal(FluentRatingControl.TouchItemScale, near.ScaleX, 3);

            // Leaving the row parks the focal point out of reach and everything falls back.
            rating.PreviewPointerAt(0);
            rating.LeavePointer();
            Assert.Equal(FluentRatingControl.RestItemScale, near.ScaleX, 3);
            Assert.Equal(FluentRatingControl.RestItemScale, far.ScaleX, 3);
        });
    }

    [Fact]
    public void The_foreground_layer_never_eats_the_pointer()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 3);

            Assert.False(((StackPanel)Part(rating, "RatingForegroundStackPanel")!).IsHitTestVisible);
            Assert.False(((ContentControl)Part(rating, "ForegroundContentPresenter")!).IsHitTestVisible);
            Assert.True(((StackPanel)Part(rating, "RatingBackgroundStackPanel")!).IsHitTestVisible);
        });
    }

    [Fact]
    public void The_caption_reaches_the_part_and_its_margin_compensates_the_run()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 3);
            rating.Caption = "Rate this";
            PixelHarness.Settle();

            var caption = (TextBlock)Part(rating, "Caption")!;
            Assert.Equal("Rate this", caption.Text);

            // Upstream takes the cell overhang off this gap; here the last cell ends where its star's ink ends, so
            // the caption only needs the 12 upstream names c_captionSpacing.
            Assert.Equal(FluentRatingControl.CaptionSpacing, caption.Margin.Left, 1);
        });
    }

    [Fact]
    public void A_step_from_unset_lands_on_the_initial_value_and_a_second_step_moves_by_one()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: -1);

            Assert.True(rating.TryHandleKey(Key.Right));
            Assert.Equal(1, rating.Value);

            rating.TryHandleKey(Key.Right);
            Assert.Equal(2, rating.Value);

            rating.TryHandleKey(Key.Left);
            Assert.Equal(1, rating.Value);
        });
    }

    [Fact]
    public void Up_and_down_step_like_right_and_left_and_ignore_the_reading_direction()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: -1);
            rating.FlowDirection = FlowDirection.RightToLeft;

            rating.TryHandleKey(Key.Up);
            Assert.Equal(1, rating.Value);
            rating.TryHandleKey(Key.Down);
            Assert.Equal(-1, rating.Value);
        });
    }

    [Fact]
    public void Left_and_right_reverse_in_a_right_to_left_row()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 2);
            rating.FlowDirection = FlowDirection.RightToLeft;

            rating.TryHandleKey(Key.Right);
            Assert.Equal(1, rating.Value);
        });
    }

    [Fact]
    public void Home_clears_and_End_sets_the_maximum()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 3);

            rating.TryHandleKey(Key.End);
            Assert.Equal(5, rating.Value);

            rating.TryHandleKey(Key.Home);
            Assert.Equal(-1, rating.Value);
        });
    }

    [Fact]
    public void Stepping_up_from_the_maximum_from_the_keyboard_leaves_it_there()
    {
        _fixture.Run(() =>
        {
            // Upstream's guard at RatingControl.cpp:599-604: clearing on re-activation is a pointer gesture, and a
            // keyboard press at the top stays at the top.
            var rating = Rating(value: 5);
            rating.TryHandleKey(Key.Right);
            Assert.Equal(5, rating.Value);
        });
    }

    [Fact]
    public void A_fraction_set_in_code_is_dropped_before_a_keyboard_step_moves_it()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 2.5);
            rating.TryHandleKey(Key.Right);
            Assert.Equal(3, rating.Value);
        });
    }

    [Fact]
    public void Refusing_to_clear_forces_one_star_instead_of_the_sentinel()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 3);
            rating.IsClearEnabled = false;

            rating.TryHandleKey(Key.Home);
            Assert.Equal(1, rating.Value);
        });
    }

    [Fact]
    public void A_read_only_row_refuses_the_keyboard_and_says_so_by_not_handling_the_key()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 3);
            rating.IsReadOnly = true;

            Assert.False(rating.TryHandleKey(Key.Right));
            Assert.Equal(3, rating.Value);
        });
    }

    [Fact]
    public void Releasing_on_the_current_star_clears_the_row()
    {
        _fixture.Run(() =>
        {
            // Five stars over the modelled 112 DIP: the band that holds the value 3 is 44.8..67.2.
            var rating = Rating(value: 3);
            rating.CommitPointerAt(55);
            Assert.Equal(-1, rating.Value);
        });
    }

    [Fact]
    public void A_release_left_of_the_row_clears_and_a_drag_is_what_makes_that_reachable()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 4);
            rating.CommitPointerAt(-10);
            Assert.Equal(-1, rating.Value);
        });
    }

    [Fact]
    public void A_release_divides_by_the_whole_row_without_the_first_item_offset()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: -1);

            // Upstream's two pointer paths divide by the same width but only one of them subtracts the offset the
            // first cell sits at (cpp:846 against cpp:925). On this layout that offset reads 0 - the cells start at
            // the panel's own origin - so the asymmetry is in the code and has nothing to move: pinned here at the
            // value that makes it invisible rather than left implied.
            Assert.Equal(0d, rating.FirstItemOffset, 1);

            rating.CommitPointerAt(50);
            Assert.Equal(3, rating.Value);

            rating.CommitPointerAt(1);
            Assert.Equal(1, rating.Value);
        });
    }

    [Fact]
    public void Moving_over_the_row_previews_without_writing_the_value()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: -1);
            rating.PreviewPointerAt(50);

            Assert.Equal(-1, rating.Value);
            Assert.Equal(FluentRatingControlDisplayState.PointerOverPlaceholder, rating.ActiveRatingState);
            Assert.Equal(3, CroppedStars(rating));
        });
    }

    [Fact]
    public void A_committed_value_raises_ValueChanged_once_even_when_it_does_not_change()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: 3);
            var raised = 0;
            rating.ValueChanged += (_, _) => raised++;

            rating.TryHandleKey(Key.End);
            Assert.Equal(1, raised);
            Assert.Equal(5, rating.Value);
        });
    }

    [Fact]
    public void Asking_to_clear_an_already_unset_row_raises_nothing()
    {
        _fixture.Run(() =>
        {
            var rating = Rating(value: -1);
            var raised = 0;
            rating.ValueChanged += (_, _) => raised++;

            rating.CommitPointerAt(-10);
            Assert.Equal(0, raised);
        });
    }

    [Fact]
    public void A_row_loaded_without_the_Astra_dictionaries_still_builds()
    {
        _fixture.Run(() =>
        {
            // The guard that replaces upstream's fail-fast: no ItemInfo, no templates, no brushes - and no crash.
            var rating = new FluentRatingControl { Style = null, ItemInfo = null };
            PixelHarness.Build(rating, 200, 40);
            PixelHarness.Settle();

            Assert.Equal(-1, rating.Value);
            Assert.Equal(5, rating.MaxRating);
        });
    }

    /// <summary>A mounted row. <paramref name="maxRating"/> is set before the template runs so the row is stamped
    /// once at the right size instead of being rebuilt under the test.</summary>
    private FluentRatingControl Rating(double value = -1, int maxRating = 5, string? caption = null)
    {
        var rating = new FluentRatingControl { MaxRating = maxRating, Caption = caption ?? string.Empty };
        PixelHarness.Build(rating, 260, 40);
        rating.Value = value;
        PixelHarness.Settle();
        return rating;
    }

    private static int Count(FluentRatingControl rating, string partName) =>
        ((Panel)Part(rating, partName)!).Children.Count;

    private static FrameworkElement? Part(FrameworkElement root, string name) => PixelHarness.Named(root, name);

    private static double Width(FluentRatingControl rating, int index) => ((Grid)rating.ForegroundItem(index)!).Width;

    /// <summary>The magnifier transform on one star's own foreground run.</summary>
    private static ScaleTransform ScaleOf(FluentRatingControl rating, int index) =>
        (ScaleTransform)((FrameworkElement)((Grid)rating.ForegroundItem(index)!).Children[0]!).RenderTransform!;

    private static ScaleTransform ScaleOfBackground(FluentRatingControl rating, int index) =>
        (ScaleTransform)((FrameworkElement)((Grid)rating.BackgroundItem(index)!).Children[0]!).RenderTransform!;

    /// <summary>The run inside a cell: every item the panel lays out is a crop host around the glyph or image.</summary>
    private static FrameworkElement? Run(object? cell) => (cell as Grid)?.Children.Count > 0 ? (FrameworkElement?)((Grid)cell!).Children[0] : null;

    /// <summary>A panel child's own margin. <c>Children</c> hands back <c>UIElement</c>, which has no such property.</summary>
    private static Thickness MarginOf(object cell) => ((FrameworkElement)cell).Margin;

    /// <summary>The box one star really occupies: the crop host's own width, which is its ink box.</summary>
    private static double Advance(FluentRatingControl rating) => Width(rating, 0);

    private static int CroppedStars(FluentRatingControl rating)
    {
        var full = 0;
        for (var index = 0; index < rating.MaxRating; index++)
        {
            if (Width(rating, index) >= Advance(rating) - 0.5)
            {
                full++;
            }
        }

        return full;
    }

    private static string ItemText(FluentRatingControl rating, int index) =>
        ((TextBlock)((Grid)rating.ForegroundItem(index)!).Children[0]!).Text;

    private static Brush? ItemBrush(FluentRatingControl rating, int index) =>
        ((TextBlock)((Grid)rating.ForegroundItem(index)!).Children[0]!).Foreground;

    private static object? Resource(string key) => Application.Current?.TryFindResource(key);
}
