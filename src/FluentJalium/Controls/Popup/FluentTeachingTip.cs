using System.Windows.Input;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// Where a <see cref="FluentTeachingTip"/> sits in relation to its target. WinUI spells eighteen of these;
/// the runtime's own <c>Popup.Placement</c> has none of the per-edge or per-corner values, so this carries the
/// six a Relative popup plus two offsets can place exactly (spike/TeachingTipProbe, docs/astra/adaptation/00 S1-b).
/// </summary>
public enum FluentTeachingTipPlacementMode
{
    /// <summary>Top with a target, Bottom without it - the resolution WinUI's own layout code makes.</summary>
    Auto,
    /// <summary>Above the target, tail pointing down at it.</summary>
    Top,
    /// <summary>Below the target, tail pointing up at it.</summary>
    Bottom,
    /// <summary>Left of the target, tail pointing right at it.</summary>
    Left,
    /// <summary>Right of the target, tail pointing left at it.</summary>
    Right,
    /// <summary>Centred on the target, no tail.</summary>
    Center,
}

/// <summary>Whether the tip draws the polygon that points at its target.</summary>
public enum FluentTeachingTipTailVisibility
{
    /// <summary>Show it whenever there is a target to point at.</summary>
    Auto,
    /// <summary>Show it; a tip with no target still hides it, which is upstream's own ordering.</summary>
    Visible,
    /// <summary>Never show it.</summary>
    Collapsed,
}

/// <summary>
/// A pointed callout anchored to another element: WinUI's TeachingTip shape, with its card, tail and buttons
/// painted by a JALXAML template instead of by the control.
/// </summary>
/// <remarks>
/// <para>
/// This is an own type because the runtime has nothing to re-template: 26.10.9 exports no TeachingTip at all
/// (docs/astra/adaptation/s0v-runtime-type-inventory-raw.txt), and <see cref="ContentControl"/> is the base
/// WinUI itself derives from. Two base-class facts carry over from the InfoBar batch: a
/// <see cref="ContentControl"/> has to opt into template content management before a template builds a tree,
/// and its default style is applied by name from the constructor because an implicit derived-control style is
/// not guaranteed to resolve (Controls/Layout/FluentInfoBar.cs:42-97).
/// </para>
/// <para>
/// What the type owns is the part a template cannot reach on this runtime. Markup
/// <c>VisualStateManager</c> throws here, so where WinUI's C++ writes a placement state, a border thickness and
/// popup offsets, this control writes <see cref="EffectivePlacement"/> - which is what the template's own
/// triggers read - and the popup's offsets. The offsets are arithmetic rather than heuristic because the tail
/// bands make the popup box exactly eight DIP larger than the card on every side (upstream's
/// 8,10,*,10,8 grid), so putting that box's tail edge on the target's edge is a size sum, not a guess. The
/// anchor it assumes is measured: <c>Placement=Relative</c> lands the popup child on the target's top-left
/// corner and each offset is an additive DIP - 0,0 read back -0.29,-0.29 and 50,50 read back 50,50.
/// </para>
/// <para>
/// The type measures to nothing without help, which is why there is no <c>MeasureOverride</c> here: a
/// <see cref="Popup"/> that roots a template contributes 0x0 to its host whether it is closed or showing a card
/// (spike/TeachingTipProbe mode room, run against both variants of that override - the sibling under a stacked
/// tip landed at the same y=20 closed, open and open-with-a-target either way). Upstream's tip takes no space
/// for a different reason - its card is built outside the control's tree in code.
/// </para>
/// <para>
/// Deliberately not here, each for a reason recorded in docs/astra/audits/teachingtip.md: hero content, an
/// icon, the header close button, light dismiss, the entrance animation and a theme shadow.
/// </para>
/// </remarks>
public class FluentTeachingTip : ContentControl
{
    private const string DefaultStyleResourceKey = "DefaultFluentTeachingTipStyle";

    /// <summary>The eight DIP band the 5x5 tail grid adds outside the card on each side.</summary>
    private const double TailBand = 8;

    private Style? _appliedDefaultStyle;
    private Popup? _popup;
    private Button? _actionButton;
    private Button? _closeButton;

    /// <summary>Identifies the <see cref="IsOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
        nameof(IsOpen), typeof(bool), typeof(FluentTeachingTip),
        new PropertyMetadata(false, OnIsOpenChanged));

    /// <summary>Identifies the <see cref="Title"/> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(FluentTeachingTip), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="Subtitle"/> dependency property.</summary>
    public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
        nameof(Subtitle), typeof(string), typeof(FluentTeachingTip), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="Target"/> dependency property.</summary>
    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
        nameof(Target), typeof(FrameworkElement), typeof(FluentTeachingTip),
        new PropertyMetadata(null, OnTargetChanged));

    /// <summary>Identifies the <see cref="TailVisibility"/> dependency property.</summary>
    public static readonly DependencyProperty TailVisibilityProperty = DependencyProperty.Register(
        nameof(TailVisibility), typeof(FluentTeachingTipTailVisibility), typeof(FluentTeachingTip),
        new PropertyMetadata(FluentTeachingTipTailVisibility.Auto));

    /// <summary>Identifies the <see cref="PreferredPlacement"/> dependency property.</summary>
    public static readonly DependencyProperty PreferredPlacementProperty = DependencyProperty.Register(
        nameof(PreferredPlacement), typeof(FluentTeachingTipPlacementMode), typeof(FluentTeachingTip),
        new PropertyMetadata(FluentTeachingTipPlacementMode.Auto, OnPreferredPlacementChanged));

    /// <summary>Identifies the <see cref="EffectivePlacement"/> dependency property.</summary>
    public static readonly DependencyProperty EffectivePlacementProperty = DependencyProperty.Register(
        nameof(EffectivePlacement), typeof(FluentTeachingTipPlacementMode), typeof(FluentTeachingTip),
        new PropertyMetadata(FluentTeachingTipPlacementMode.Top));

    /// <summary>Identifies the <see cref="PlacementMargin"/> dependency property.</summary>
    public static readonly DependencyProperty PlacementMarginProperty = DependencyProperty.Register(
        nameof(PlacementMargin), typeof(Thickness), typeof(FluentTeachingTip),
        new PropertyMetadata(new Thickness(), OnPlacementMarginChanged));

    /// <summary>Identifies the <see cref="ActionButtonContent"/> dependency property.</summary>
    public static readonly DependencyProperty ActionButtonContentProperty = DependencyProperty.Register(
        nameof(ActionButtonContent), typeof(object), typeof(FluentTeachingTip), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="ActionButtonStyle"/> dependency property.</summary>
    public static readonly DependencyProperty ActionButtonStyleProperty = DependencyProperty.Register(
        nameof(ActionButtonStyle), typeof(Style), typeof(FluentTeachingTip), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="ActionButtonCommand"/> dependency property.</summary>
    public static readonly DependencyProperty ActionButtonCommandProperty = DependencyProperty.Register(
        nameof(ActionButtonCommand), typeof(ICommand), typeof(FluentTeachingTip), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="ActionButtonCommandParameter"/> dependency property.</summary>
    public static readonly DependencyProperty ActionButtonCommandParameterProperty = DependencyProperty.Register(
        nameof(ActionButtonCommandParameter), typeof(object), typeof(FluentTeachingTip), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="CloseButtonContent"/> dependency property.</summary>
    public static readonly DependencyProperty CloseButtonContentProperty = DependencyProperty.Register(
        nameof(CloseButtonContent), typeof(object), typeof(FluentTeachingTip), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="CloseButtonStyle"/> dependency property.</summary>
    public static readonly DependencyProperty CloseButtonStyleProperty = DependencyProperty.Register(
        nameof(CloseButtonStyle), typeof(Style), typeof(FluentTeachingTip), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="CloseButtonCommand"/> dependency property.</summary>
    public static readonly DependencyProperty CloseButtonCommandProperty = DependencyProperty.Register(
        nameof(CloseButtonCommand), typeof(ICommand), typeof(FluentTeachingTip), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="CloseButtonCommandParameter"/> dependency property.</summary>
    public static readonly DependencyProperty CloseButtonCommandParameterProperty = DependencyProperty.Register(
        nameof(CloseButtonCommandParameter), typeof(object), typeof(FluentTeachingTip), new PropertyMetadata(null));

    /// <summary>Creates a teaching tip, switches on template content management and applies its named style.</summary>
    public FluentTeachingTip()
    {
        UseTemplateContentManagement();
        ApplyDefaultStyle();
        Loaded += (_, _) =>
        {
            ApplyDefaultStyle();
            UpdatePlacement();
        };
    }

    /// <summary>Gets or sets whether the tip is shown.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty)!;
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Gets or sets the bold first line of the card.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the second line under the title.</summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Gets or sets the element the tip points at. With none, the tip sits where it was placed and draws no tail.</summary>
    public FrameworkElement? Target
    {
        get => (FrameworkElement?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>Gets or sets whether the tail is drawn.</summary>
    public FluentTeachingTipTailVisibility TailVisibility
    {
        get => (FluentTeachingTipTailVisibility)GetValue(TailVisibilityProperty)!;
        set => SetValue(TailVisibilityProperty, value);
    }

    /// <summary>Gets or sets the side to place the card on.</summary>
    public FluentTeachingTipPlacementMode PreferredPlacement
    {
        get => (FluentTeachingTipPlacementMode)GetValue(PreferredPlacementProperty)!;
        set => SetValue(PreferredPlacementProperty, value);
    }

    /// <summary>
    /// Gets the side the card is actually on: <see cref="PreferredPlacement"/> with Auto resolved and, when the
    /// preferred side would push the card out of the window, the first fallback that fits. The template's tail
    /// triggers read this rather than the preference, because a tail on the wrong edge is worse than a tip that
    /// moved - and because markup states are the only lever this runtime gives a template.
    /// </summary>
    public FluentTeachingTipPlacementMode EffectivePlacement
    {
        get => (FluentTeachingTipPlacementMode)GetValue(EffectivePlacementProperty)!;
        private set => SetCurrentValue(EffectivePlacementProperty, value);
    }

    /// <summary>Gets or sets extra space kept between the card's tail and the target.</summary>
    public Thickness PlacementMargin
    {
        get => (Thickness)GetValue(PlacementMarginProperty)!;
        set => SetValue(PlacementMarginProperty, value);
    }

    /// <summary>Gets or sets the action button's content. With none, the button is collapsed.</summary>
    public object? ActionButtonContent
    {
        get => GetValue(ActionButtonContentProperty);
        set => SetValue(ActionButtonContentProperty, value);
    }

    /// <summary>Gets or sets the style of the action button.</summary>
    public Style? ActionButtonStyle
    {
        get => (Style?)GetValue(ActionButtonStyleProperty);
        set => SetValue(ActionButtonStyleProperty, value);
    }

    /// <summary>Gets or sets the command the action button invokes.</summary>
    public ICommand? ActionButtonCommand
    {
        get => (ICommand?)GetValue(ActionButtonCommandProperty);
        set => SetValue(ActionButtonCommandProperty, value);
    }

    /// <summary>Gets or sets the parameter passed to <see cref="ActionButtonCommand"/>.</summary>
    public object? ActionButtonCommandParameter
    {
        get => GetValue(ActionButtonCommandParameterProperty);
        set => SetValue(ActionButtonCommandParameterProperty, value);
    }

    /// <summary>Gets or sets the close button's content. With none, the button is collapsed.</summary>
    public object? CloseButtonContent
    {
        get => GetValue(CloseButtonContentProperty);
        set => SetValue(CloseButtonContentProperty, value);
    }

    /// <summary>Gets or sets the style of the close button.</summary>
    public Style? CloseButtonStyle
    {
        get => (Style?)GetValue(CloseButtonStyleProperty);
        set => SetValue(CloseButtonStyleProperty, value);
    }

    /// <summary>Gets or sets the command the close button invokes.</summary>
    public ICommand? CloseButtonCommand
    {
        get => (ICommand?)GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    /// <summary>Gets or sets the parameter passed to <see cref="CloseButtonCommand"/>.</summary>
    public object? CloseButtonCommandParameter
    {
        get => GetValue(CloseButtonCommandParameterProperty);
        set => SetValue(CloseButtonCommandParameterProperty, value);
    }

    /// <summary>Raised after the tip is opened.</summary>
    public event EventHandler? Opened;

    /// <summary>Raised after the tip is closed.</summary>
    public event EventHandler? Closed;

    /// <summary>Raised when the action button is invoked.</summary>
    public event EventHandler? ActionButtonClick;

    /// <summary>Raised when the close button is invoked, before the tip closes.</summary>
    public event EventHandler? CloseButtonClick;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        ReleaseParts();
        _popup = GetTemplateChild("PART_Popup") as Popup;
        _actionButton = GetTemplateChild("PART_ActionButton") as Button;
        _closeButton = GetTemplateChild("PART_CloseButton") as Button;
        if (_actionButton is not null)
        {
            _actionButton.Click += OnActionButtonClick;
        }

        if (_closeButton is not null)
        {
            _closeButton.Click += OnCloseButtonClick;
        }

        ApplyOpenState();
    }

    /// <inheritdoc/>
    protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
    {
        ReleaseParts();
        base.OnTemplateChanged(oldTemplate, newTemplate);
    }

    private static void OnIsOpenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var tip = (FluentTeachingTip)sender;
        tip.ApplyOpenState();
        if ((bool)args.NewValue!)
        {
            tip.Opened?.Invoke(tip, EventArgs.Empty);
        }
        else
        {
            tip.Closed?.Invoke(tip, EventArgs.Empty);
        }
    }

    private static void OnTargetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var tip = (FluentTeachingTip)sender;
        if (args.OldValue is FrameworkElement previous)
        {
            previous.LayoutUpdated -= tip.OnTargetLayoutUpdated;
        }

        if (args.NewValue is FrameworkElement current)
        {
            current.LayoutUpdated += tip.OnTargetLayoutUpdated;
        }

        tip.UpdatePlacement();
    }

    private static void OnPreferredPlacementChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((FluentTeachingTip)sender).UpdatePlacement();

    private static void OnPlacementMarginChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((FluentTeachingTip)sender).UpdatePlacement();

    private void OnTargetLayoutUpdated(object? sender, EventArgs args)
    {
        if (IsOpen)
        {
            UpdatePlacement();
        }
    }

    private void OnActionButtonClick(object sender, RoutedEventArgs args) => ActionButtonClick?.Invoke(this, EventArgs.Empty);

    private void OnCloseButtonClick(object sender, RoutedEventArgs args)
    {
        CloseButtonClick?.Invoke(this, EventArgs.Empty);
        SetCurrentValue(IsOpenProperty, false);
    }

    private void ApplyOpenState()
    {
        if (_popup is null)
        {
            return;
        }

        _popup.PlacementTarget = Target;
        _popup.IsOpen = IsOpen;
        if (IsOpen)
        {
            UpdatePlacement();
        }
    }

    private void ReleaseParts()
    {
        if (_actionButton is not null)
        {
            _actionButton.Click -= OnActionButtonClick;
        }

        if (_closeButton is not null)
        {
            _closeButton.Click -= OnCloseButtonClick;
        }

        _popup = null;
        _actionButton = null;
        _closeButton = null;
    }

    /// <summary>
    /// Resolves Auto the way ModernWpf's port of WinUI's layout does it: a tip with something to point at goes
    /// above it, a tip without one goes below.
    /// </summary>
    private FluentTeachingTipPlacementMode ResolvePreferredPlacement() => PreferredPlacement switch
    {
        FluentTeachingTipPlacementMode.Auto => Target is null ? FluentTeachingTipPlacementMode.Bottom : FluentTeachingTipPlacementMode.Top,
        _ => PreferredPlacement,
    };

    private static FluentTeachingTipPlacementMode[] GetPlacementFallbacks(FluentTeachingTipPlacementMode placement) => placement switch
    {
        FluentTeachingTipPlacementMode.Top => [FluentTeachingTipPlacementMode.Top, FluentTeachingTipPlacementMode.Bottom, FluentTeachingTipPlacementMode.Right, FluentTeachingTipPlacementMode.Left],
        FluentTeachingTipPlacementMode.Left => [FluentTeachingTipPlacementMode.Left, FluentTeachingTipPlacementMode.Right, FluentTeachingTipPlacementMode.Bottom, FluentTeachingTipPlacementMode.Top],
        FluentTeachingTipPlacementMode.Right => [FluentTeachingTipPlacementMode.Right, FluentTeachingTipPlacementMode.Left, FluentTeachingTipPlacementMode.Bottom, FluentTeachingTipPlacementMode.Top],
        FluentTeachingTipPlacementMode.Bottom => [FluentTeachingTipPlacementMode.Bottom, FluentTeachingTipPlacementMode.Top, FluentTeachingTipPlacementMode.Right, FluentTeachingTipPlacementMode.Left],
        _ => [placement],
    };

    /// <summary>
    /// Measures the card the template built, then puts its tail edge on the target and keeps the whole box
    /// inside the window where a fallback side can. Both the anchor the arithmetic assumes and the additive
    /// offsets are measured, not assumed (docs/astra/adaptation/00 S1-b).
    /// </summary>
    private void UpdatePlacement()
    {
        if (_popup is null || _popup.Child is not FrameworkElement container)
        {
            return;
        }

        container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var size = container.DesiredSize;
        var target = Target;
        if (target is null)
        {
            EffectivePlacement = ResolvePreferredPlacement();
            _popup.HorizontalOffset = 0;
            _popup.VerticalOffset = 0;
            return;
        }

        var margin = PlacementMargin;
        var targetWidth = target.ActualWidth;
        var targetHeight = target.ActualHeight;
        var window = FindWindow();
        var bounds = window is not null ? new Size(window.ActualWidth, window.ActualHeight) : Size.Empty;
        var at = window is not null ? target.TranslatePoint(new Point(), window) : new Point();

        var chosen = ResolvePreferredPlacement();
        foreach (var candidate in GetPlacementFallbacks(chosen))
        {
            var (x, y) = OffsetFor(candidate, targetWidth, targetHeight, size, margin);
            if (at.X + x >= 0 && at.Y + y >= 0 && at.X + x + size.Width <= bounds.Width && at.Y + y + size.Height <= bounds.Height)
            {
                chosen = candidate;
                break;
            }
        }

        EffectivePlacement = chosen;
        var (offsetX, offsetY) = OffsetFor(chosen, targetWidth, targetHeight, size, margin);
        _popup.HorizontalOffset = offsetX;
        _popup.VerticalOffset = offsetY;
    }

    private static (double X, double Y) OffsetFor(
        FluentTeachingTipPlacementMode placement,
        double targetWidth,
        double targetHeight,
        Size container,
        Thickness margin)
    {
        // The container is the card plus a tail band on each side, so centring it on the target also centres
        // the tail - which is what every one of upstream's placement states aligns to.
        var centredX = (targetWidth - container.Width) / 2;
        var centredY = (targetHeight - container.Height) / 2;
        return placement switch
        {
            FluentTeachingTipPlacementMode.Top => (centredX, -container.Height - margin.Bottom),
            FluentTeachingTipPlacementMode.Bottom => (centredX, targetHeight + margin.Top),
            FluentTeachingTipPlacementMode.Left => (-container.Width - margin.Right, centredY),
            FluentTeachingTipPlacementMode.Right => (targetWidth + margin.Left, centredY),
            _ => (centredX, centredY),
        };
    }

    /// <summary>
    /// The window the fit check keeps the card inside, found through <see cref="FrameworkElement.Parent"/> rather
    /// than the render tree: AGENTS.md bans tree walks over render parents in product code, and the logical chain
    /// reaches the window just as well for a mounted tip -
    /// <c>FluentTeachingTip &lt; Grid &lt; StackPanel &lt; Window</c> (spike/TeachingTipProbe mode parent).
    /// </summary>
    private Window? FindWindow()
    {
        FrameworkElement? node = this;
        for (var steps = 0; steps < 40 && node is not null; steps++)
        {
            if (node is Window window)
            {
                return window;
            }

            node = node.Parent as FrameworkElement;
        }

        return null;
    }

    private void ApplyDefaultStyle()
    {
        // Same constructor fallback as FluentInfoBar: an implicit derived-control style is not guaranteed to
        // resolve, and an application-assigned style - including an explicit null - is never replaced.
        if ((Style is not null || _appliedDefaultStyle is not null) &&
            !ReferenceEquals(Style, _appliedDefaultStyle))
        {
            return;
        }

        if (_appliedDefaultStyle is null &&
            !ReferenceEquals(ReadLocalValue(StyleProperty), DependencyProperty.UnsetValue))
        {
            return;
        }

        if (TryFindResource(DefaultStyleResourceKey) is not Style style)
        {
            return;
        }

        _appliedDefaultStyle = style;
        SetCurrentValue(StyleProperty, style);
    }
}
