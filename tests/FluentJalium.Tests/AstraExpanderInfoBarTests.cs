using FluentJalium.Controls;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Ellipse = Jalium.UI.Shapes.Ellipse;
using ShapePath = Jalium.UI.Shapes.Path;

namespace FluentJalium.Tests;

/// <summary>
/// The first stage-4 pair. The Expander is native and only needs its skin, but its skin is a contract:
/// the control reads three parts by name and does real work with them (spike/ExpanderInfoBarProbe measures
/// that it installs its left-button toggle on PART_HeaderBorder, writes PART_ContentBorder's visibility,
/// and rotates PART_Chevron - and that a renamed set of parts does none of that while building, mounting and
/// painting identically). The InfoBar is native too but ships no template at all, and 26.10.9's instance
/// never opts into template content management, so <see cref="FluentInfoBar" /> exists for exactly that call.
/// <para>
/// Both halves of the naming contract are asserted, and the pixels are asserted by overriding a palette
/// token and reading it back, never by a screenshot. The framework's own painted colours are refused by
/// name: #2C2C2E for the expander chrome, #2D2D37 for the info bar's hardcoded severity fill.
/// </para>
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraExpanderInfoBarTests
{
    private static readonly Color HeaderSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color ContentSentinel = Color.FromRgb(0x00, 0x80, 0xFF);
    private static readonly Color ErrorSentinel = Color.FromRgb(0xFF, 0x22, 0x00);
    private static readonly Color AttentionSentinel = Color.FromRgb(0xFF, 0xD6, 0x0A);
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    /// <summary>The grey 26.10.9's own expander chrome paints when no style of ours reaches it.</summary>
    private static readonly Color FrameworkChrome = Color.FromRgb(0x2C, 0x2C, 0x2E);

    /// <summary>The severity fill the InfoBar draws itself when it is still painting itself.</summary>
    private static readonly Color FrameworkInfoBarFill = Color.FromRgb(0x2D, 0x2D, 0x37);

    /// <summary>
    /// The informational icon the same OnRender draws: a literal #0A84FF circle, read out of a capture of an
    /// untemplated bar in spike/InfoBarGhostProbe pass 1 (310 pixels of it). It is not a palette row, so it
    /// is a stable witness that the base class painted over a template that hid the icon.
    /// </summary>
    private static readonly Color FrameworkSeverityIconBlue = Color.FromRgb(0x0A, 0x84, 0xFF);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraExpanderInfoBarTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    // ---------- rows ----------

    [Theory]
    [InlineData("ExpanderHeaderBackground", "CardBackgroundFillColorDefaultBrush")]
    [InlineData("ExpanderHeaderForeground", "TextFillColorPrimaryBrush")]
    [InlineData("ExpanderHeaderDisabledForeground", "TextFillColorDisabledBrush")]
    [InlineData("ExpanderHeaderBorderBrush", "CardStrokeColorDefaultBrush")]
    [InlineData("ExpanderChevronBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("ExpanderChevronPointerOverBackground", "SubtleFillColorSecondaryBrush")]
    [InlineData("ExpanderChevronForeground", "TextFillColorPrimaryBrush")]
    [InlineData("ExpanderContentBackground", "CardBackgroundFillColorSecondaryBrush")]
    [InlineData("ExpanderContentBorderBrush", "CardStrokeColorDefaultBrush")]
    public void An_expander_alias_resolves_to_the_object_upstream_names(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    [Theory]
    [InlineData("InfoBarErrorSeverityBackgroundBrush", "SystemFillColorCriticalBackgroundBrush")]
    [InlineData("InfoBarWarningSeverityBackgroundBrush", "SystemFillColorCautionBackgroundBrush")]
    [InlineData("InfoBarSuccessSeverityBackgroundBrush", "SystemFillColorSuccessBackgroundBrush")]
    [InlineData("InfoBarInformationalSeverityBackgroundBrush", "SystemFillColorAttentionBackgroundBrush")]
    [InlineData("InfoBarErrorSeverityIconBackground", "SystemFillColorCriticalBrush")]
    [InlineData("InfoBarWarningSeverityIconBackground", "SystemFillColorCautionBrush")]
    [InlineData("InfoBarSuccessSeverityIconBackground", "SystemFillColorSuccessBrush")]
    [InlineData("InfoBarInformationalSeverityIconBackground", "SystemFillColorAttentionBrush")]
    [InlineData("InfoBarErrorSeverityIconForeground", "TextFillColorInverseBrush")]
    [InlineData("InfoBarInformationalSeverityIconForeground", "TextFillColorInverseBrush")]
    [InlineData("InfoBarTitleForeground", "TextFillColorPrimaryBrush")]
    [InlineData("InfoBarMessageForeground", "TextFillColorPrimaryBrush")]
    [InlineData("InfoBarBorderBrush", "CardStrokeColorDefaultBrush")]
    public void An_info_bar_alias_resolves_to_the_object_upstream_names(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    /// <summary>
    /// The deferral, pinned. Upstream also publishes ExpanderContentUpBorderThickness and the header
    /// alignment rows, five InfoBar icon-glyph strings, the InfoBar close-button size rows and the
    /// horizontal-orientation margins that belong to its InfoBarPanel; none of them has a consumer here
    /// (ExpandDirection is not honoured, the chevron is a Path rather than a glyph, and this template always
    /// stacks), and a published key is a promise that overriding it reaches pixels.
    /// </summary>
    [Theory]
    [InlineData("ExpanderContentUpBorderThickness")]
    [InlineData("ExpanderHeaderForegroundPressed")]
    [InlineData("ExpanderHeaderBorderPressedBrush")]
    [InlineData("ExpanderChevronPressedBackground")]
    [InlineData("ExpanderChevronPressedForeground")]
    [InlineData("ExpanderChevronBorderPressedBrush")]
    [InlineData("ExpanderChevronDownGlyph")]
    [InlineData("ExpanderChevronUpGlyph")]
    [InlineData("ExpanderMinHeight")]
    [InlineData("ExpanderHeaderHorizontalContentAlignment")]
    [InlineData("InfoBarHyperlinkButtonForeground")]
    [InlineData("InfoBarHyperlinkButtonMargin")]
    [InlineData("InfoBarTitleHorizontalOrientationMargin")]
    [InlineData("InfoBarPanelHorizontalOrientationPadding")]
    [InlineData("InfoBarIconBackgroundGlyph")]
    [InlineData("InfoBarMinHeight")]
    [InlineData("InfoBarCloseButtonStyle")]
    public void A_row_with_no_consumer_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(TryRes(key)));
    }

    // ---------- the expander's part-name contract ----------

    [Fact]
    public void The_expander_keeps_the_three_names_it_reads()
    {
        _fixture.Run(() =>
        {
            var expander = Mount(new Expander { Header = "header", Content = "body" }, 320, 96);

            // The control casts what it finds: the header and the content to FrameworkElement and the chevron
            // to Shapes.Path specifically, so a TextBlock glyph in that slot would be silently ignored.
            Assert.IsType<Border>(Part(expander, "PART_HeaderBorder"));
            Assert.IsType<Border>(Part(expander, "PART_ContentBorder"));
            Assert.IsType<ShapePath>(Part(expander, "PART_Chevron"));
        });
    }

    [Fact]
    public void The_header_is_a_border_because_the_control_itself_handles_the_click()
    {
        // A ToggleButton in the header slot answers both its own click and the control's handler, which is
        // how the split button's command came to run twice. The control focuses and toggles itself, and its
        // key handler is installed on the Expander rather than on a child, so focus stays on the control.
        _fixture.Run(() =>
        {
            var expander = Mount(new Expander { Header = "header", Content = "body" }, 320, 96);
            var header = Part(expander, "PART_HeaderBorder");

            Assert.IsNotAssignableFrom<ButtonBase>(header);
            Assert.True(expander.Focusable);
        });
    }

    [Fact]
    public void A_left_button_down_on_the_header_expands_once_and_a_second_one_collapses()
    {
        // RaiseEvent is public on this runtime, so the control's own MouseDown handler runs - the same
        // handler a physical click reaches - with no OS input and no cursor on anybody's desktop.
        _fixture.Run(() =>
        {
            var expander = Mount(new Expander { Header = "header", Content = "body" }, 320, 140);
            var events = 0;
            expander.Expanded += (_, _) => events++;
            expander.Collapsed += (_, _) => events++;

            RaiseMouseDown(Part(expander, "PART_HeaderBorder"));
            PixelHarness.Settle(40);
            Assert.True(expander.IsExpanded, "the control's header handler did not expand it.");
            Assert.True(Shown(Part(expander, "PART_ContentBorder")));

            RaiseMouseDown(Part(expander, "PART_HeaderBorder"));
            PixelHarness.Settle(40);
            Assert.False(expander.IsExpanded, "a second click on the header did not collapse it.");
            Assert.Equal(2, events);
        });
    }

    [Fact]
    public void A_disabled_expander_ignores_the_header_click()
    {
        _fixture.Run(() =>
        {
            var expander = Mount(new Expander { Header = "header", Content = "body", IsEnabled = false }, 320, 140);

            RaiseMouseDown(Part(expander, "PART_HeaderBorder"));
            PixelHarness.Settle(20);

            Assert.False(expander.IsExpanded, "a disabled expander answered a header click.");
        });
    }

    [Fact]
    public void Expanding_rotates_the_named_chevron_and_shows_the_named_content()
    {
        _fixture.Run(() =>
        {
            var expander = Mount(new Expander { Header = "header", Content = "body" }, 320, 140);
            var chevron = (ShapePath)Part(expander, "PART_Chevron");
            Assert.Null(chevron.RenderTransform);

            expander.IsExpanded = true;
            PixelHarness.Settle(60);

            Assert.True(Shown(Part(expander, "PART_ContentBorder")));
            var rotation = Assert.IsType<RotateTransform>(chevron.RenderTransform);
            Assert.Equal(90d, rotation.Angle, 1);
        });
    }

    /// <summary>
    /// The negative half of the contract. A template whose parts carry other names builds, mounts, lays out
    /// and paints - and then never expands, because the control found nothing to wire. Nothing in a structure
    /// or pixel reading of that template would say so, which is why this case exists.
    /// </summary>
    [Fact]
    public void A_renamed_header_does_not_expand()
    {
        _fixture.Run(() =>
        {
            var dictionary = (ResourceDictionary)XamlReader.Parse(RenamedMarkup)!;
            var expander = new Expander
            {
                Header = "header",
                Content = "body",
                Template = (ControlTemplate)dictionary["RenamedExpanderTemplate"]!,
            };
            Mount(expander, 320, 140);

            var header = Part(expander, "HeaderBorderZ");
            Assert.Null(PixelHarness.Named(expander, "PART_HeaderBorder"));
            Assert.Null(PixelHarness.Named(expander, "PART_ContentBorder"));
            Assert.Null(PixelHarness.Named(expander, "PART_Chevron"));

            RaiseMouseDown(header);
            PixelHarness.Settle(40);

            Assert.False(expander.IsExpanded, "renaming PART_HeaderBorder should not have made the click work.");
            Assert.False(Shown(Part(expander, "ContentBorderZ")), "the renamed content part was still driven.");
            Assert.Null(((ShapePath)Part(expander, "ChevronZ")).RenderTransform);
        });
    }

    [Fact]
    public void Every_expander_state_names_a_row_and_watches_the_header()
    {
        _fixture.Run(() =>
        {
            var template = Template(FluentThemeManager.GetStyle("ExpanderStyle"));
            var cells = TemplateCells(template);
            var published = new[]
            {
                "ExpanderHeaderForegroundPointerOver", "ExpanderHeaderDisabledForeground",
                "ExpanderHeaderBorderPointerOverBrush", "ExpanderHeaderDisabledBorderBrush",
                "ExpanderChevronPointerOverForeground", "ExpanderChevronPointerOverBackground",
                "ExpanderChevronBorderPointerOverBrush",
            };

            foreach (var condition in new[] { "IsMouseOver=True", "IsEnabled=False" })
            {
                // More than one cell can share a condition - IsEnabled=False also hides the focus ring - so
                // the claim is about the values the state writes in total, not about the first cell found.
                var matching = cells.FindAll(candidate => candidate.Condition == condition);
                Assert.NotEmpty(matching);
                var setters = matching.SelectMany(static cell => cell.Setters).ToArray();
                Assert.True(setters.Length >= 4, $"{condition} writes {setters.Length} values.");
                foreach (var key in setters.Select(ResourceKey).Where(static key => key is not null).Select(static key => key!))
                {
                    Assert.Contains(key, published);
                }
            }

            // The disabled state colours with the two disabled rows, which no other state may name.
            var disabled = cells.FindAll(candidate => candidate.Condition == "IsEnabled=False")
                .SelectMany(static cell => cell.Setters).Select(ResourceKey).Where(static key => key is not null);
            Assert.Contains("ExpanderHeaderDisabledForeground", disabled);
            Assert.Contains("ExpanderHeaderDisabledBorderBrush", disabled);

            // Hover and press belong to the header, not to the whole control: watching the control would
            // light the header while the pointer sits over the body.
            foreach (var name in new[] { "IsMouseOver" })
            {
                var trigger = template.Triggers.Cast<object>().OfType<Trigger>()
                    .First(trigger => trigger.Property?.Name == name);
                Assert.Equal("PART_HeaderBorder", trigger.SourceName);
            }
        });
    }

    // ---------- expander pixels ----------

    [Fact]
    public void The_expander_takes_our_style_rather_than_the_framework_chrome()
    {
        _fixture.Run(() =>
        {
            var expander = Mount(new Expander { Header = "header", Content = "body" }, 320, 140);
            var ours = FluentThemeManager.GetStyle("ExpanderStyle");

            // An implicit style never lands in FrameworkElement.Style on this runtime, so the template
            // object is the readable proof that our style is the effective one.
            Assert.Null(expander.Style);
            Assert.Same(Template(ours), expander.Template);

            var rest = PixelHarness.Render(expander, 320, 140);
            Assert.True(rest.PaintedPixels > 0, $"capture is empty: {rest.Top(6)}");
            Assert.Equal(0, rest.Count(FrameworkChrome));
            Assert.Equal(0, rest.Count(BrandEmerald));
        });
    }

    [Fact]
    public void The_header_and_the_content_paint_their_own_card_tokens()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("CardBackgroundFillColorDefaultBrush", HeaderSentinel);
            FluentThemeManager.OverrideBrush("CardBackgroundFillColorSecondaryBrush", ContentSentinel);
            try
            {
                var expander = new Expander { Header = "header", Content = "body", IsExpanded = true };
                PixelHarness.Build(expander, 320, 140);
                PixelHarness.Settle(60);

                var rest = PixelHarness.Render(expander, 320, 140);
                Assert.True(rest.Stable, $"capture never settled: {rest.Top(6)}");
                Assert.True(rest.Count(HeaderSentinel) > 3_000,
                    $"the header token did not reach pixels; top={rest.Top(6)}");
                Assert.True(rest.Count(ContentSentinel) > 1_000,
                    $"the content token did not reach pixels; top={rest.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("CardBackgroundFillColorDefaultBrush", null);
                FluentThemeManager.OverrideBrush("CardBackgroundFillColorSecondaryBrush", null);
            }
        });
    }

    [Fact]
    public void The_expander_follows_the_theme()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var light = PixelHarness.Render(new Expander { Header = "header", Content = "body" }, 320, 140);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = PixelHarness.Render(new Expander { Header = "header", Content = "body" }, 320, 140);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.True(light.PaintedPixels > 0, $"light capture is empty: {light.Top(6)}");
            Assert.True(dark.PaintedPixels > 0, $"dark capture is empty: {dark.Top(6)}");
            Assert.NotEqual(light.Top(2), dark.Top(2));
            Assert.Equal(0, light.Count(BrandEmerald));
        });
    }

    // ---------- the info bar's own type ----------

    /// <summary>
    /// Why the type exists, as an assertion: assigning the same template to the native control builds no
    /// tree at all, because 26.10.9's InfoBar never calls the protected template opt-in, while the subclass
    /// gets the whole skin. The failure mode is invisible to a colour test - an untemplated info bar simply
    /// paints its own hard-coded severity fill.
    /// </summary>
    [Fact]
    public void The_native_info_bar_ignores_a_template_and_ours_does_not()
    {
        _fixture.Run(() =>
        {
            var ours = FluentThemeManager.GetStyle("DefaultFluentInfoBarStyle");
            var native = new InfoBar { Title = "title", Message = "message", Template = Template(ours) };
            PixelHarness.Build(native, 360, 60);
            Assert.Null(PixelHarness.Named(native, "RootBorder"));
            Assert.Null(PixelHarness.Named(native, "PART_CloseButton"));

            var bar = Mount(new FluentInfoBar { Title = "title", Message = "message" }, 360, 60);
            Assert.IsType<Border>(Part(bar, "RootBorder"));
            Assert.IsType<Button>(Part(bar, "PART_CloseButton"));
        });
    }

    [Fact]
    public void The_close_button_raises_the_event_and_collapses_the_bar()
    {
        // Only one name buys this: the base class listens to a Button called PART_CloseButton and nothing
        // else, and it raises CloseButtonClick and sets IsOpen=false itself.
        _fixture.Run(() =>
        {
            var bar = Mount(new FluentInfoBar { Title = "title", Message = "message" }, 360, 60);
            var clicks = 0;
            var closed = 0;
            bar.CloseButtonClick += (_, _) => clicks++;
            bar.Closed += (_, _) => closed++;

            Invoke((Button)Part(bar, "PART_CloseButton"));
            PixelHarness.Settle(20);

            Assert.Equal(1, clicks);
            Assert.False(bar.IsOpen);
            Assert.Equal(1, closed);

            // Nothing but this cell collapses a templated bar: measured, the control only drops its own
            // measure and keeps the skin on screen at MinHeight.
            var root = Part(bar, "RootBorder");
            Assert.False(Shown(root), "IsOpen=false left the bar visible: the style has to collapse it, the control does not.");
        });
    }

    [Fact]
    public void A_templated_bar_leaves_the_base_class_nothing_to_paint()
    {
        // The part name is the whole switch. InfoBar.OnApplyTemplate looks RootBorder up by name - an IL
        // string literal, spike/InfoBarGhostProbe pass 2 - and its OnRender steps aside only once it has
        // found one. With upstream's own name (ContentRoot) on that part the base kept drawing its own
        // icon, title, message and close mark on top of our template: the ghost in
        // spike/VisualQA/out/surfaces.png, and 178 pixels of its icon blue in a capture of a bar whose
        // icon was hidden. After the rename the same capture holds none, and a styled bar measures
        // byte-identically to one whose OnRender is overridden away (pass 3: 7 colours, 936 ink pixels).
        _fixture.Run(() =>
        {
            var bar = Mount(new FluentInfoBar
            {
                Title = "Weekly digest ready",
                Message = "Nine new items are waiting.",
                Width = 360,
                IsIconVisible = false,
                IsClosable = false,
            }, 360, 60);

            Assert.IsType<Border>(Part(bar, "RootBorder"));

            var capture = PixelHarness.Render(bar, 360, 60);
            Assert.True(capture.PaintedPixels > 0, $"capture is empty: {capture.Top(6)}");
            Assert.Equal(0, capture.Count(FrameworkSeverityIconBlue));
        });
    }

    [Fact]
    public void A_wrapped_message_grows_the_bar_instead_of_painting_below_it()
    {
        // The base class measures its own literal single-line layout, not the tree it now hosts: a bar whose
        // message wraps measured 69.8 outside against 107.1 inside, so the second line painted below the
        // surface (spike/TextWrapProbe pass 1 case B). FluentInfoBar.MeasureOverride takes the larger of the
        // two, which is what a template-driven control gets for free.
        _fixture.Run(() =>
        {
            var bar = new FluentInfoBar
            {
                Title = "Weekly digest ready",
                Message = "Nine new items are waiting. Closing this bar raises CloseButtonClick, which is the base class's own event.",
                Width = 300,
                IsIconVisible = false,
                IsClosable = false,
            };

            // A vertical panel, because that is the shape that exposes it: the page hands a bar an infinite
            // height and takes whatever it asks for. Hosted at a fixed height the defect is arranged away.
            var host = new StackPanel { Orientation = Orientation.Vertical };
            host.Children.Add(bar);
            Mount(host, 320, 240);

            var root = Part(bar, "RootBorder");
            var message = (TextBlock)Part(bar, "Message");

            Assert.True(message.DesiredSize.Height > 30, $"the message did not wrap: {message.DesiredSize}.");
            Assert.True(bar.DesiredSize.Height + 0.5 >= root.DesiredSize.Height,
                $"the bar measures {bar.DesiredSize.Height} tall but its own template asks for {root.DesiredSize.Height}.");
            Assert.True(bar.DesiredSize.Height > 48,
                $"a two-line bar still measures only MinHeight: {bar.DesiredSize.Height}.");
        });
    }

    [Fact]
    public void Each_severity_names_the_rows_and_the_glyph_upstream_uses()
    {
        _fixture.Run(() =>
        {
            var cells = TemplateCells(Template(FluentThemeManager.GetStyle("DefaultFluentInfoBarStyle")));

            foreach (var (severity, background, icon) in new[]
                     {
                         ("Error", "InfoBarErrorSeverityBackgroundBrush", "InfoBarErrorSeverityIconBackground"),
                         ("Warning", "InfoBarWarningSeverityBackgroundBrush", "InfoBarWarningSeverityIconBackground"),
                         ("Success", "InfoBarSuccessSeverityBackgroundBrush", "InfoBarSuccessSeverityIconBackground"),
                     })
            {
                var cell = FindCell(cells, $"Severity={severity}");
                var keys = cell.Setters.Select(ResourceKey).Where(static key => key is not null).Select(static key => key!).ToList();
                Assert.Contains(background, keys);
                Assert.Contains(icon, keys);
                Assert.Contains($"InfoBar{severity}SeverityIconForeground", keys);
                Assert.Contains(cell.Setters, static setter => setter.Property?.Name == "Text" || setter.PropertyName == "Text");
            }

            // Informational is the resting state: upstream's Informational VisualState is empty too.
            Assert.DoesNotContain(cells, static cell => cell.Condition == "Severity=Informational");
        });
    }

    [Fact]
    public void The_icon_and_the_close_button_have_a_cell_each()
    {
        _fixture.Run(() =>
        {
            var cells = TemplateCells(Template(FluentThemeManager.GetStyle("DefaultFluentInfoBarStyle")));
            Assert.Equal("Visible", Form(FindCell(cells, "IsIconVisible=True").Setters.Single().Value));
            Assert.Equal("Collapsed", Form(FindCell(cells, "IsClosable=False").Setters.Single().Value));
            Assert.Equal("Collapsed", Form(FindCell(cells, "IsOpen=False").Setters.Single().Value));
        });
    }

    [Fact]
    public void Title_and_message_read_upstreams_foreground_rows()
    {
        _fixture.Run(() =>
        {
            var bar = Mount(new FluentInfoBar { Title = "title", Message = "message" }, 360, 60);

            // Both rows alias the same palette instance upstream, so this asserts the pair, not two colours.
            Assert.Same(Res("TextFillColorPrimaryBrush"), ((TextBlock)Part(bar, "Title")).Foreground);
            Assert.Same(Res("TextFillColorPrimaryBrush"), ((TextBlock)Part(bar, "Message")).Foreground);
            Assert.Equal("title", ((TextBlock)Part(bar, "Title")).Text);
            Assert.Equal("message", ((TextBlock)Part(bar, "Message")).Text);
        });
    }

    [Fact]
    public void The_error_severity_paints_the_critical_token_and_not_the_frameworks_fill()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("SystemFillColorCriticalBackgroundBrush", ErrorSentinel);
            try
            {
                var bar = new FluentInfoBar
                {
                    Title = "title",
                    Message = "message",
                    Severity = InfoBarSeverity.Error,
                };
                PixelHarness.Build(bar, 360, 60);
                PixelHarness.Settle(30);

                var capture = PixelHarness.Render(bar, 360, 60);
                Assert.True(capture.Stable, $"capture never settled: {capture.Top(6)}");
                Assert.True(capture.Count(ErrorSentinel) > 3_000,
                    $"the severity token did not reach pixels; top={capture.Top(6)}");
                Assert.Equal(0, capture.Count(FrameworkInfoBarFill));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("SystemFillColorCriticalBackgroundBrush", null);
            }
        });
    }

    [Fact]
    public void The_informational_icon_disc_take_is_the_attention_token()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("SystemFillColorAttentionBrush", AttentionSentinel);
            try
            {
                var bar = new FluentInfoBar { Title = "title", Message = "message" };
                PixelHarness.Build(bar, 360, 60);
                PixelHarness.Settle(30);

                var icon = (Ellipse)PixelHarness.Named(bar, "IconBackground")!;
                Assert.Same(Res("SystemFillColorAttentionBrush"), icon.Fill);
                var capture = PixelHarness.Render(bar, 360, 60);
                Assert.True(capture.Count(AttentionSentinel) > 100,
                    $"the 16x16 icon disc did not reach pixels; top={capture.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("SystemFillColorAttentionBrush", null);
            }
        });
    }

    [Fact]
    public void The_info_bar_takes_our_style_and_follows_the_theme()
    {
        _fixture.Run(() =>
        {
            var bar = Mount(new FluentInfoBar { Title = "title", Message = "message" }, 360, 60);
            Assert.Same(Template(FluentThemeManager.GetStyle("DefaultFluentInfoBarStyle")), bar.Template);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var light = PixelHarness.Render(new FluentInfoBar { Title = "title", Message = "message" }, 360, 60);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = PixelHarness.Render(new FluentInfoBar { Title = "title", Message = "message" }, 360, 60);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.True(light.PaintedPixels > 0, $"light capture is empty: {light.Top(6)}");
            Assert.True(dark.PaintedPixels > 0, $"dark capture is empty: {dark.Top(6)}");
            Assert.NotEqual(light.Top(2), dark.Top(2));
            Assert.Equal(0, light.Count(BrandEmerald));
            Assert.Equal(0, light.Count(FrameworkInfoBarFill));
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

    private static bool Shown(FrameworkElement element) => element.Visibility == Visibility.Visible;

    private static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();

    /// <summary>
    /// Drives the handler a physical click would reach, through the public routed-event door. This is not
    /// hardware input: the pointer never moves, so hover and press *pixels* stay owed (task 13).
    /// </summary>
    private static void RaiseMouseDown(FrameworkElement element)
    {
        var arguments = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseDownEvent,
            Source = element,
        };
        element.RaiseEvent(arguments);
    }

    private static object? TryRes(string key) => Application.Current!.TryFindResource(key);

    private static Brush Res(string key) => (Brush)Application.Current!.TryFindResource(key)!;

    private static ControlTemplate Template(Style style) =>
        (style.Setters.Cast<object>().OfType<Setter>()
            .First(static setter => (setter.Property?.Name ?? setter.PropertyName) == "Template").Value as ControlTemplate)!;

    private static List<Cell> TemplateCells(ControlTemplate template) => template.Triggers.Cast<object>().Select(static trigger => trigger switch
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
        ?? throw new InvalidOperationException($"No cell for {condition}; the template carries " +
            string.Join(" | ", cells.Select(static candidate => candidate.Condition)));

    private static string Form(object? value) => value?.ToString() ?? "null";

    private static string? ResourceKey(Setter setter) =>
        setter.Value?.GetType().GetProperty("ResourceKey")?.GetValue(setter.Value) as string;

    private sealed record Cell(string Condition, Setter[] Setters);

    /// <summary>The same template with every name the control reads renamed.</summary>
    private const string RenamedMarkup = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='RenamedExpanderTemplate' TargetType='Expander'>
            <Grid>
              <Grid.RowDefinitions>
                <RowDefinition Height='Auto' />
                <RowDefinition Height='Auto' />
              </Grid.RowDefinitions>
              <Border x:Name='HeaderBorderZ' Grid.Row='0' Background='#FFFF00FF' MinHeight='48'>
                <Grid>
                  <Grid.ColumnDefinitions>
                    <ColumnDefinition Width='*' />
                    <ColumnDefinition Width='Auto' />
                  </Grid.ColumnDefinitions>
                  <ContentPresenter x:Name='HeaderContentZ' Content='{TemplateBinding Header}' />
                  <Path x:Name='ChevronZ' Grid.Column='1' Width='12' Height='12' Stretch='Uniform'
                        Data='M 1.5,0 L 6.5,5 L 1.5,10' Stroke='White' StrokeThickness='1.25' />
                </Grid>
              </Border>
              <Border x:Name='ContentBorderZ' Grid.Row='1' Background='#FF00FF00' MinHeight='40' Visibility='Collapsed'>
                <ContentPresenter Content='{TemplateBinding Content}' />
              </Border>
            </Grid>
          </ControlTemplate>
        </ResourceDictionary>
        """;
}
