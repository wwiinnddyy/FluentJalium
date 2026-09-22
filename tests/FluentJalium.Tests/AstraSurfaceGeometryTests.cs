using FluentJalium.Controls;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The effect side of the attribute sweep (docs/astra/adaptation/00 S1-g): four surfaces were square or unpadded
/// at runtime because the rows that describe them were written onto elements whose type has no such member -
/// a <c>CornerRadius</c> on a <c>Grid</c>, a <c>Padding</c> on a <c>Grid</c> or a <c>StackPanel</c>, an alignment on
/// a <c>ContentPresenter</c>. The markup still parsed, the build stayed green and the resource-key gate still read
/// every row as consumed, so nothing but a read-back off the realised element could tell.
///
/// Each fact here reads the row off the element that now carries it, and one reads the shape that was left alone:
/// the check box and radio labels lost a <c>TextWrapping</c> attribute nothing could read, and the text element the
/// presenter generates defaults to wrapping on its own, which is what the last pair below pins.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraSurfaceGeometryTests : IDisposable
{
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraSurfaceGeometryTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    public void Dispose() => GC.SuppressFinalize(this);

    [Fact]
    public void The_info_bar_insets_its_content_with_the_rows_that_now_have_a_painter()
    {
        _fixture.Run(() =>
        {
            var bar = Mount(new FluentInfoBar { Title = "title", Message = "message" }, 360, 60);
            var root = (Border)Part(bar, "RootBorder");
            var panel = (Border)Part(bar, "PanelSurface");
            Assert.Multiple(
                () => Assert.Equal(Thickness("InfoBarContentRootPadding"), root.Padding),
                () => Assert.Equal(48d, root.MinHeight, 0.01),
                () => Assert.Equal(Thickness("InfoBarPanelVerticalOrientationPadding"), panel.Padding),
                () => Assert.IsType<StackPanel>(Part(bar, "Panel")));
        });
    }

    [Fact]
    public void The_spinner_popup_surfaces_on_a_border_that_can_hold_its_radius()
    {
        _fixture.Run(() =>
        {
            var box = Mount(new NumberBox { Value = 3, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact }, 220, 32);
            var popup = (Popup)Part(box, "UpDownPopup");
            // The cell writes this property on Compact + focus; writing it directly reaches the same state without
            // a pointer, and the child only realises once the popup is open.
            popup.IsOpen = true;
            // Paired on purpose (spike/PopupLadderProbe S9's lesson, re-measured here as mutation MB): the
            // popup's Child keeps its name, its 8/1 and both brushes after the popup is closed, so the reads
            // below alone would say only that the surface exists somewhere.
            Assert.True(popup.IsOpen, "the write did not open the popup, so the surface below may be a stale graft.");
            // The child is there when the write returns (spike/PopupLadderProbe S13 reads PopupContentRoot with
            // 8/1 and both brushes at rung 0 through all twenty frames). The pump that used to sit here was
            // waiting for nothing this claim needs.
            // The surface is the popup's own child, so the name has to be read off it rather than looked up
            // below it - a name lookup only ever tests children (the same trap AstraTeachingTipTests documents).
            var surface = (Border)(popup.Child ?? throw new InvalidOperationException("the popup realized no child"));
            Assert.Multiple(
                () => Assert.Equal("PopupContentRoot", surface.Name),
                () => Assert.Equal(CornerRadius("OverlayCornerRadius"), surface.CornerRadius),
                () => Assert.Equal(Thickness("NumberBoxPopupBorderThickness"), surface.BorderThickness),
                () => Assert.Same(Res("NumberBoxPopupBackground"), surface.Background),
                () => Assert.Same(Res("NumberBoxPopupBorderBrush"), surface.BorderBrush));
            popup.IsOpen = false;
            // This pump stays: it is not an assertion, it is letting the framework finish tearing the surface out
            // of the shared host before the next test walks the overlay.
            PixelHarness.Settle(20);
        });
    }

    [Fact]
    public void A_menu_bar_item_paints_its_fill_once_so_the_corners_stay_round()
    {
        // The old shape painted the same brush twice: the root Grid filled the item square underneath the Border
        // that rounds it. At rest that is invisible - MenuBarItemBackground aliases SubtleFillColorTransparentBrush,
        // so the square layer paints nothing - and it only bites when an app sets a fill of its own, which is the
        // case pinned second here. The read has to name the property Grid actually declares: this runtime's
        // Control.BackgroundProperty is a different dependency property on a type this Grid is not, and reading
        // that one returned null while the element held #00FFFFFF (spike/AttributeSweep/menuitem-fill-first.txt).
        _fixture.Run(() =>
        {
            var bar = new MenuBar();
            bar.Items.Add(new MenuBarItem { Title = "view" });
            Mount(bar, 320, 40);
            var item = (MenuBarItem)bar.Items[0];
            var root = (Grid)Part(item, "ContentRoot");
            var surface = (Border)Part(item, "Background");
            Assert.Multiple(
                () => Assert.Null(root.Background),
                () => Assert.Same(Res("MenuBarItemBackground"), surface.Background),
                () => Assert.Equal(CornerRadius("ControlCornerRadius"), surface.CornerRadius));

            var fill = new SolidColorBrush(Color.FromRgb(0x11, 0x22, 0x33));
            item.Background = fill;
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.Null(root.Background),
                () => Assert.Same(fill, surface.Background));
        });
    }

    [Fact]
    public void The_expander_hands_its_content_alignment_to_the_presenter()
    {
        _fixture.Run(() =>
        {
            var expander = Mount(new Expander
            {
                Header = "head",
                Content = "body",
                IsExpanded = true,
                HorizontalContentAlignment = HorizontalAlignment.Right,
            }, 320, 120);
            var border = Part(expander, "PART_ContentBorder");
            var presenter = PixelHarness.Descendant<ContentPresenter>(border)!;
            Assert.Equal(HorizontalAlignment.Right, presenter.HorizontalAlignment);

            expander.HorizontalContentAlignment = HorizontalAlignment.Center;
            PixelHarness.Settle(20);
            Assert.Equal(HorizontalAlignment.Center,
                ((ContentPresenter)PixelHarness.Descendant<ContentPresenter>(Part(expander, "PART_ContentBorder"))!).HorizontalAlignment);
        });
    }

    [Fact]
    public void A_check_box_label_and_a_radio_label_still_wrap_without_the_attribute()
    {
        // spike/AttributeSweep removed two TextWrapping="Wrap" attributes a ContentPresenter cannot hold. This
        // runtime's generated text element wraps by default - the measurement Styles/Navigation.jalxaml already
        // cites for the opposite direction - so the labels kept the behaviour and lost the dead markup.
        _fixture.Run(() =>
        {
            var box = Mount(new CheckBox { Content = "a label long enough to need a second line", Width = 120 }, 120, 40);
            var dot = Mount(new RadioButton { Content = "a label long enough to need a second line", Width = 120 }, 120, 40);
            var text = PixelHarness.Descendant<TextBlock>(box)!;
            var other = PixelHarness.Descendant<TextBlock>(dot)!;
            Assert.Multiple(
                () => Assert.Equal(TextWrapping.Wrap, text.TextWrapping),
                () => Assert.Equal(TextWrapping.Wrap, other.TextWrapping));
        });
    }

    // ---------- helpers ----------

    private static T Mount<T>(T element, int width, int height) where T : FrameworkElement
    {
        PixelHarness.Build(element, width, height);
        PixelHarness.Settle(20);
        return element;
    }

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name}.");

    private static Brush Res(string key) => (Brush)Application.Current!.TryFindResource(key)!;

    private static Thickness Thickness(string key) => (Thickness)Application.Current!.TryFindResource(key)!;

    private static CornerRadius CornerRadius(string key) => (CornerRadius)Application.Current!.TryFindResource(key)!;
}
