using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Input;

namespace FluentJalium.Controls;

/// <summary>How much room a PipsPager navigation button keeps when it is not showing.</summary>
/// <remarks>Upstream's <c>PipsPagerButtonVisibility</c>. <c>VisibleOnPointerOver</c> keeps the layout slot and
/// only lifts the opacity, <c>Collapsed</c> gives the slot back - the difference is measurable, so the enum is
/// not decoration.</remarks>
public enum FluentPipsPagerButtonVisibility
{
    /// <summary>The button is shown.</summary>
    Visible,

    /// <summary>The button holds its slot at zero opacity and appears while the pager is hovered or focused.</summary>
    VisibleOnPointerOver,

    /// <summary>The button is removed from layout.</summary>
    Collapsed,
}

/// <summary>Carries nothing, exactly like upstream's empty <c>PipsPagerSelectedIndexChangedEventArgs</c>.</summary>
public sealed class FluentPipsPagerSelectedIndexChangedEventArgs : EventArgs
{
}

/// <summary>
/// A row of page indicators with an optional previous/next pair: WinUI's PipsPager. Each pip is a real
/// <see cref="Button"/>, so invoking one selects that page through the framework's own click path.
/// </summary>
/// <remarks>
/// <para>
/// This is an own type because the runtime has no such control to retemplate: the census
/// (<c>spike/PipsPagerProbe</c>, mode <c>api</c>) reports <c>PipsPager</c> absent from 26.10.9, and so
/// <c>ItemsRepeater</c> and <c>StackLayout</c>, which are what upstream's template builds the pips out of.
/// </para>
/// <para>
/// The pip count is derived, not authored: <see cref="NumberOfPages"/> says how many pips exist and
/// <see cref="MaxVisiblePips"/> says how much of the row the viewport shows. Upstream generates one element per
/// page - there is no flanking and no ellipsis pip, a reading that contradicts the usual description of the
/// control - and limits what you see by clamping the inner <see cref="ScrollViewer"/>'s cross-axis maximum to
/// <c>(k - 1) * defaultPip + selectedPip</c>, with the pip sizes measured off the realized containers
/// (<c>PipsPager.cpp:274-308</c>). Measuring rather than restating the numbers is deliberate: the footprints
/// live in markup here, and the runtime cannot carry an <c>x:Double</c> resource row.
/// </para>
/// <para>
/// Upstream draws a pip as one symbol glyph (<c>U+EA3B</c>) at <c>FontSize</c> 4, or 6 when selected, and that
/// font-size swap is the entire difference between the two states. The glyph is a real hit in this machine's
/// Segoe Fluent Icons cmap and lays out at the matching box, but its ink never reaches an in-process capture
/// (mode <c>pip</c>: two drawn 8x8 references read 64 and 60 inked pixels, every glyph cell reads none) - the
/// same defect that already stops a button's text colour from being asserted. A pip whose only paint is that
/// glyph would therefore be a visual nothing in this library can prove, so the dot is drawn instead, at the
/// diameter the glyph produces: 0.938 em of a 6 or 4 DIP run.
/// </para>
/// </remarks>
public class FluentPipsPager : Control
{
    private const string DefaultStyleResourceKey = "DefaultPipsPagerStyle";
    private const string PipHostPartName = "PipsPagerItemsHost";
    private const string PipScrollPartName = "PipsPagerScrollViewer";
    private const string PreviousButtonPartName = "PreviousPageButton";
    private const string NextButtonPartName = "NextPageButton";

    /// <summary>The tag a chosen pip carries so its template can show the wider dot.</summary>
    private const string SelectedPipTag = "selected";

    /// <summary>
    /// The pip footprints, named after the four upstream rows they stand in for
    /// (PipsPagerHorizontalOrientationButtonWidth/Height and the vertical pair). An <c>x:Double</c> row cannot be
    /// published by this resource reader, and the container is the element whose size both the template and the
    /// viewport clamp have to agree with, so the numbers live here and nowhere else.
    /// </summary>
    private const double HorizontalPipWidth = 12;

    private const double HorizontalPipHeight = 24;
    private const double VerticalPipWidth = 24;
    private const double VerticalPipHeight = 12;

    /// <summary>The shared instance upstream's empty args type only ever produces.</summary>
    private static readonly FluentPipsPagerSelectedIndexChangedEventArgs SelectedIndexChangedArgs = new();

    /// <summary>Identifies the <see cref="NumberOfPages"/> dependency property.</summary>
    public static readonly DependencyProperty NumberOfPagesProperty = DependencyProperty.Register(
        nameof(NumberOfPages), typeof(int), typeof(FluentPipsPager),
        new PropertyMetadata(-1, OnNumberOfPagesChanged));

    /// <summary>Identifies the <see cref="SelectedPageIndex"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedPageIndexProperty = DependencyProperty.Register(
        nameof(SelectedPageIndex), typeof(int), typeof(FluentPipsPager),
        new PropertyMetadata(0, OnSelectedPageIndexChanged));

    /// <summary>Identifies the <see cref="MaxVisiblePips"/> dependency property.</summary>
    public static readonly DependencyProperty MaxVisiblePipsProperty = DependencyProperty.Register(
        nameof(MaxVisiblePips), typeof(int), typeof(FluentPipsPager),
        new PropertyMetadata(5, OnMaxVisiblePipsChanged));

    /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation), typeof(Orientation), typeof(FluentPipsPager),
        new PropertyMetadata(Jalium.UI.Controls.Orientation.Horizontal, OnOrientationChanged));

    /// <summary>Identifies the <see cref="PreviousButtonVisibility"/> dependency property.</summary>
    public static readonly DependencyProperty PreviousButtonVisibilityProperty = DependencyProperty.Register(
        nameof(PreviousButtonVisibility), typeof(FluentPipsPagerButtonVisibility), typeof(FluentPipsPager),
        new PropertyMetadata(FluentPipsPagerButtonVisibility.Collapsed));

    /// <summary>Identifies the <see cref="NextButtonVisibility"/> dependency property.</summary>
    public static readonly DependencyProperty NextButtonVisibilityProperty = DependencyProperty.Register(
        nameof(NextButtonVisibility), typeof(FluentPipsPagerButtonVisibility), typeof(FluentPipsPager),
        new PropertyMetadata(FluentPipsPagerButtonVisibility.Collapsed));

    /// <summary>Identifies the <see cref="PreviousButtonStyle"/> dependency property.</summary>
    public static readonly DependencyProperty PreviousButtonStyleProperty = DependencyProperty.Register(
        nameof(PreviousButtonStyle), typeof(Style), typeof(FluentPipsPager), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="NextButtonStyle"/> dependency property.</summary>
    public static readonly DependencyProperty NextButtonStyleProperty = DependencyProperty.Register(
        nameof(NextButtonStyle), typeof(Style), typeof(FluentPipsPager), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="SelectedPipStyle"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedPipStyleProperty = DependencyProperty.Register(
        nameof(SelectedPipStyle), typeof(Style), typeof(FluentPipsPager),
        new PropertyMetadata(null, OnPipStyleChanged));

    /// <summary>Identifies the <see cref="NormalPipStyle"/> dependency property.</summary>
    public static readonly DependencyProperty NormalPipStyleProperty = DependencyProperty.Register(
        nameof(NormalPipStyle), typeof(Style), typeof(FluentPipsPager),
        new PropertyMetadata(null, OnPipStyleChanged));

    private readonly List<Button> _pips = [];

    private Panel? _pipHost;
    private ScrollViewer? _pipScroll;
    private Button? _previousButton;
    private Button? _nextButton;
    private Style? _appliedDefaultStyle;

    /// <summary>Creates a pager and applies its named style when one is available.</summary>
    public FluentPipsPager()
    {
        KeyDown += OnKeyDownHandler;
        ApplyDefaultStyle();
        Loaded += (_, _) => ApplyDefaultStyle();
    }

    /// <summary>Raised after <see cref="SelectedPageIndex"/> settles on a new value.</summary>
    public event EventHandler<FluentPipsPagerSelectedIndexChangedEventArgs>? SelectedIndexChanged;

    /// <summary>
    /// Gets or sets how many pages exist. <c>-1</c> means unbounded: pips grow as the selection advances and
    /// never shrink, which is upstream's default rather than an accident.
    /// </summary>
    public int NumberOfPages
    {
        get => (int)GetValue(NumberOfPagesProperty)!;
        set => SetValue(NumberOfPagesProperty, value);
    }

    /// <summary>Gets or sets the selected page, clamped into <see cref="NumberOfPages"/>.</summary>
    public int SelectedPageIndex
    {
        get => (int)GetValue(SelectedPageIndexProperty)!;
        set => SetValue(SelectedPageIndexProperty, value);
    }

    /// <summary>Gets or sets how many pips the viewport shows before the rest are clipped.</summary>
    public int MaxVisiblePips
    {
        get => (int)GetValue(MaxVisiblePipsProperty)!;
        set => SetValue(MaxVisiblePipsProperty, value);
    }

    /// <summary>Gets or sets the axis the row runs along.</summary>
    public Jalium.UI.Controls.Orientation Orientation
    {
        get => (Jalium.UI.Controls.Orientation)GetValue(OrientationProperty)!;
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>Gets or sets how the previous button takes its room.</summary>
    public FluentPipsPagerButtonVisibility PreviousButtonVisibility
    {
        get => (FluentPipsPagerButtonVisibility)GetValue(PreviousButtonVisibilityProperty)!;
        set => SetValue(PreviousButtonVisibilityProperty, value);
    }

    /// <summary>Gets or sets how the next button takes its room.</summary>
    public FluentPipsPagerButtonVisibility NextButtonVisibility
    {
        get => (FluentPipsPagerButtonVisibility)GetValue(NextButtonVisibilityProperty)!;
        set => SetValue(NextButtonVisibilityProperty, value);
    }

    /// <summary>Gets or sets the style handed to the previous button.</summary>
    public Style? PreviousButtonStyle
    {
        get => (Style?)GetValue(PreviousButtonStyleProperty);
        set => SetValue(PreviousButtonStyleProperty, value);
    }

    /// <summary>Gets or sets the style handed to the next button.</summary>
    public Style? NextButtonStyle
    {
        get => (Style?)GetValue(NextButtonStyleProperty);
        set => SetValue(NextButtonStyleProperty, value);
    }

    /// <summary>Gets or sets the style handed to the selected pip.</summary>
    public Style? SelectedPipStyle
    {
        get => (Style?)GetValue(SelectedPipStyleProperty);
        set => SetValue(SelectedPipStyleProperty, value);
    }

    /// <summary>Gets or sets the style handed to every other pip.</summary>
    public Style? NormalPipStyle
    {
        get => (Style?)GetValue(NormalPipStyleProperty);
        set => SetValue(NormalPipStyleProperty, value);
    }

    /// <summary>Gets the pip at <paramref name="index"/>, or <c>null</c> when no such pip is realized.</summary>
    /// <remarks>Upstream reaches the same element through <c>ItemsRepeater.GetElement</c>; the repeater is the
    /// one piece of its machinery this runtime does not have, so the list is ours.</remarks>
    public Button? PipFromIndex(int index) => index >= 0 && index < _pips.Count ? _pips[index] : null;

    private static void OnNumberOfPagesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var pager = (FluentPipsPager)sender;
        pager.RebuildPips();
        pager.UpdateSizeOfSet();
        pager.UpdateNavigationButtons();
    }

    private static void OnMaxVisiblePipsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var pager = (FluentPipsPager)sender;
        if (pager.NumberOfPages < 0)
        {
            pager.RebuildPips();
        }

        pager.UpdateViewport();
        pager.UpdateNavigationButtons();
    }

    private static void OnOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var pager = (FluentPipsPager)sender;
        pager.ApplyPipFootprints();
        pager.UpdateViewport();
    }

    private static void OnPipStyleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((FluentPipsPager)sender).ApplyPipStyles();

    private static void OnSelectedPageIndexChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var pager = (FluentPipsPager)sender;
        var requested = (int)args.NewValue!;
        var clamped = pager.Clamp(requested);
        if (clamped != requested)
        {
            // Upstream rewrites the property and lets the rewrite raise the event, so an out-of-range write
            // produces exactly one event carrying the clamped index. The rewrite does reach this callback even
            // when it lands on the value that is already set - measured, because the first version of this
            // branch raised twice on a second out-of-range write.
            pager.SetValue(SelectedPageIndexProperty, clamped);
            return;
        }

        pager.ApplySelection();
        if (requested != (int)args.OldValue!)
        {
            pager.SelectedIndexChanged?.Invoke(pager, SelectedIndexChangedArgs);
        }
    }

    private int Clamp(int index)
    {
        var pages = NumberOfPages;
        if (index > pages - 1 && pages > 0)
        {
            return pages - 1;
        }

        return index < 0 ? 0 : index;
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _pipHost = GetTemplateChild(PipHostPartName) as Panel;
        _pipScroll = GetTemplateChild(PipScrollPartName) as ScrollViewer;
        _previousButton = GetTemplateChild(PreviousButtonPartName) as Button;
        _nextButton = GetTemplateChild(NextButtonPartName) as Button;

        if (_previousButton is not null)
        {
            _previousButton.Click -= OnPreviousButtonClicked;
            _previousButton.Click += OnPreviousButtonClicked;
            AutomationProperties.SetName(_previousButton, "Previous Page");
        }

        if (_nextButton is not null)
        {
            _nextButton.Click -= OnNextButtonClicked;
            _nextButton.Click += OnNextButtonClicked;
            AutomationProperties.SetName(_nextButton, "Next Page");
        }

        _pips.Clear();
        RebuildPips();
        UpdateSizeOfSet();
        UpdateNavigationButtons();
        AutomationProperties.SetName(this, "Pager");
    }

    private void RebuildPips()
    {
        if (_pipHost is null)
        {
            return;
        }

        // Anything already generated belongs to the previous template: a re-templated pager must not keep
        // buttons whose handlers point at the old selection path.
        foreach (var pip in _pips)
        {
            pip.Click -= OnPipClicked;
        }

        _pipHost.Children.Clear();

        // The grow-only rule below reads the count the row had a moment ago, so it is taken before the list is
        // emptied: measured as 5 pips after the selection walked back from page 8, because PipCount was asking a
        // list that had already been cleared.
        var previous = _pips.Count;
        _pips.Clear();

        for (var index = 0; index < PipCount(previous); index++)
        {
            AddPip(index);
        }

        ApplyPipStyles();
        ApplyPipFootprints();
        UpdateViewport();
    }

    private int PipCount(int previous)
    {
        var pages = NumberOfPages;
        if (pages == 0)
        {
            return 0;
        }

        if (pages < 0)
        {
            // Grow-only: an unbounded row never hands back a pip it has already shown (PipsPager.cpp:318-332), so
            // walking the selection back to page 2 leaves the eight pips it took to get there in place.
            return Math.Max(previous, Math.Max(SelectedPageIndex + 1, Math.Max(0, MaxVisiblePips)));
        }

        return pages;
    }

    private void AddPip(int index)
    {
        var pip = new Button();
        pip.Click += OnPipClicked;
        AutomationProperties.SetName(pip, $"Page {index + 1}");
        pip.SetValue(AutomationProperties.PositionInSetProperty, index + 1);
        ApplyPipFootprint(pip);
        _pips.Add(pip);
        _pipHost!.Children.Add(pip);
    }

    private void ApplyPipFootprints()
    {
        foreach (var pip in _pips)
        {
            ApplyPipFootprint(pip);
        }
    }

    private void ApplyPipFootprint(Button pip)
    {
        var horizontal = Orientation == Jalium.UI.Controls.Orientation.Horizontal;
        pip.Width = horizontal ? HorizontalPipWidth : VerticalPipWidth;
        pip.Height = horizontal ? HorizontalPipHeight : VerticalPipHeight;
    }

    private void OnPipClicked(object sender, RoutedEventArgs args)
    {
        var index = _pips.IndexOf((Button)sender);
        if (index >= 0)
        {
            SelectedPageIndex = index;
        }
    }

    private void ApplyPipStyles()
    {
        for (var index = 0; index < _pips.Count; index++)
        {
            var chosen = index == SelectedPageIndex;
            var pip = _pips[index];
            pip.Style = chosen ? SelectedPipStyle : NormalPipStyle;

            // The style carries the brushes; the dot that shows is switched by a trigger on this tag, because a
            // style setter cannot reach a template part here.
            pip.Tag = chosen ? SelectedPipTag : null;
        }
    }

    private void ApplySelection()
    {
        if (NumberOfPages < 0)
        {
            // An unbounded row grows a pip for each page the selection walks into (PipsPager.cpp:318-332), so a
            // selection change is a regeneration, not just a restyle.
            RebuildPips();
            return;
        }

        ApplyPipStyles();
        UpdateNavigationButtons();
        UpdateViewport();
    }

    private void UpdateSizeOfSet()
    {
        var pages = NumberOfPages;
        var size = pages < 0 ? -1 : pages;
        foreach (var pip in _pips)
        {
            pip.SetValue(AutomationProperties.SizeOfSetProperty, size);
        }
    }

    /// <summary>
    /// Clamps the pip viewport to <c>(k - 1) * default + selected</c> along the running axis, with the footprint
    /// the control itself wrote onto each container. Measuring the realized containers instead - which is what
    /// upstream does (<c>PipsPager.cpp:144-159</c>) - was tried first and starves: the clamp applies to the very
    /// arrangement the measure reads back, so a row once clamped to one pip kept reporting a 12 DIP host however
    /// many pips followed, and the second pip never appeared. Upstream escapes that only because a repeater measures
    /// its elements outside the scroller that hosts them.
    /// </summary>
    private void UpdateViewport()
    {
        if (_pipScroll is null || _pips.Count == 0)
        {
            return;
        }

        var horizontal = Orientation == Jalium.UI.Controls.Orientation.Horizontal;

        // Every pip takes the same slot, selected or not: upstream's selected pip is the same footprint with a
        // wider dot inside it.
        var footprint = horizontal ? HorizontalPipWidth : VerticalPipHeight;

        var shown = Math.Max(0, Math.Min(Math.Max(0, MaxVisiblePips), _pips.Count));
        var extent = shown == 0 ? 0d : footprint * shown;
        // Writing the maximum from a layout callback can re-enter layout, so only a real change is written.
        var along = horizontal ? extent : double.PositiveInfinity;
        var across = horizontal ? double.PositiveInfinity : extent;
        if (Math.Abs(_pipScroll.MaxWidth - along) > 0.01 || Math.Abs(_pipScroll.MaxHeight - across) > 0.01)
        {
            _pipScroll.MaxWidth = along;
            _pipScroll.MaxHeight = across;
        }
    }

    private void UpdateNavigationButtons()
    {
        var pages = NumberOfPages;
        var index = SelectedPageIndex;
        SetNavigationEnabled(_previousButton, !IsEdge(index, first: true) && pages != 0 && MaxVisiblePips > 0);
        SetNavigationEnabled(_nextButton, !IsEdge(index, first: false) && pages != 0 && MaxVisiblePips > 0);
    }

    private bool IsEdge(int index, bool first) =>
        first ? index == 0 : NumberOfPages > 0 && index == NumberOfPages - 1;

    private static void SetNavigationEnabled(Button? button, bool enabled)
    {
        if (button is not null)
        {
            button.IsEnabled = enabled;
        }
    }

    private void OnPreviousButtonClicked(object sender, RoutedEventArgs args)
    {
        var pages = NumberOfPages;
        if (pages == 0 || pages == 1)
        {
            return;
        }

        SelectedPageIndex = Math.Max(0, SelectedPageIndex - 1);
    }

    private void OnNextButtonClicked(object sender, RoutedEventArgs args)
    {
        var pages = NumberOfPages;
        if (pages == 0 || pages == 1)
        {
            return;
        }

        SelectedPageIndex = pages > 0
            ? Math.Min(SelectedPageIndex + 1, pages - 1)
            : SelectedPageIndex + 1;
    }

    /// <summary>
    /// Moves focus to the neighbouring pip. Arrow keys never change the selection: upstream's handler only
    /// walks focus, and a pip is selected by activating it, so Enter and Space belong to the button.
    /// </summary>
    private void OnKeyDownHandler(object sender, KeyEventArgs args)
    {
        var forward = args.Key is Key.Right or Key.Down;
        var backward = args.Key is Key.Left or Key.Up;
        if (!forward && !backward)
        {
            return;
        }

        var target = SelectedPageIndex + (forward ? 1 : -1);
        if (PipFromIndex(target) is { } pip)
        {
            pip.Focus();
            args.Handled = true;
        }
    }

    private void ApplyDefaultStyle()
    {
        // Same named-style fallback as FluentDropDownButton: an implicit derived-control style is not
        // guaranteed to resolve, and an application-assigned style - including an explicit null - is never
        // replaced.
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
