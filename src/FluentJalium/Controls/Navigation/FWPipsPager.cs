using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium PipsPager control for indicating and navigating through pages.
/// </summary>
public class FWPipsPager : Control, IFluentJaliumControl
{
    private Panel? _pipsPanel;
    private Button? _previousButton;
    private Button? _nextButton;

    public static readonly DependencyProperty NumberOfPagesProperty =
        DependencyProperty.Register(nameof(NumberOfPages), typeof(int), typeof(FWPipsPager),
            new PropertyMetadata(5, OnNumberOfPagesChanged, CoerceNumberOfPages));

    public static readonly DependencyProperty SelectedPageIndexProperty =
        DependencyProperty.Register(nameof(SelectedPageIndex), typeof(int), typeof(FWPipsPager),
            new PropertyMetadata(0, OnSelectedPageIndexChanged, CoerceSelectedPageIndex));

    public static readonly DependencyProperty MaxVisiblePipsProperty =
        DependencyProperty.Register(nameof(MaxVisiblePips), typeof(int), typeof(FWPipsPager),
            new PropertyMetadata(7, OnMaxVisiblePipsChanged));

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(FWPipsPager),
            new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

    public static readonly DependencyProperty PreviousButtonVisibilityProperty =
        DependencyProperty.Register(nameof(PreviousButtonVisibility), typeof(PipsPagerButtonVisibility), typeof(FWPipsPager),
            new PropertyMetadata(PipsPagerButtonVisibility.Visible));

    public static readonly DependencyProperty NextButtonVisibilityProperty =
        DependencyProperty.Register(nameof(NextButtonVisibility), typeof(PipsPagerButtonVisibility), typeof(FWPipsPager),
            new PropertyMetadata(PipsPagerButtonVisibility.Visible));

    public static readonly RoutedEvent SelectedIndexChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectedIndexChanged), RoutingStrategy.Bubble,
            typeof(EventHandler<SelectedIndexChangedEventArgs>), typeof(FWPipsPager));

    /// <summary>
    /// Initializes a new instance of the <see cref="FWPipsPager"/> class.
    /// </summary>
    public FWPipsPager()
    {
    }

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public int NumberOfPages
    {
        get => (int)GetValue(NumberOfPagesProperty)!;
        set => SetValue(NumberOfPagesProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected page index (0-based).
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public int SelectedPageIndex
    {
        get => (int)GetValue(SelectedPageIndexProperty)!;
        set => SetValue(SelectedPageIndexProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of pips to display at once.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public int MaxVisiblePips
    {
        get => (int)GetValue(MaxVisiblePipsProperty)!;
        set => SetValue(MaxVisiblePipsProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation of the pips.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty)!;
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the visibility mode for the previous button.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public PipsPagerButtonVisibility PreviousButtonVisibility
    {
        get => (PipsPagerButtonVisibility)GetValue(PreviousButtonVisibilityProperty)!;
        set => SetValue(PreviousButtonVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the visibility mode for the next button.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public PipsPagerButtonVisibility NextButtonVisibility
    {
        get => (PipsPagerButtonVisibility)GetValue(NextButtonVisibilityProperty)!;
        set => SetValue(NextButtonVisibilityProperty, value);
    }

    /// <summary>
    /// Occurs when the selected page index changes.
    /// </summary>
    public event EventHandler<SelectedIndexChangedEventArgs> SelectedIndexChanged
    {
        add => AddHandler(SelectedIndexChangedEvent, value);
        remove => RemoveHandler(SelectedIndexChangedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_previousButton != null)
            _previousButton.Click -= OnPreviousButtonClick;
        if (_nextButton != null)
            _nextButton.Click -= OnNextButtonClick;

        _pipsPanel = GetTemplateChild("PART_PipsPanel") as Panel;
        _previousButton = GetTemplateChild("PART_PreviousButton") as Button;
        _nextButton = GetTemplateChild("PART_NextButton") as Button;

        if (_previousButton != null)
            _previousButton.Click += OnPreviousButtonClick;
        if (_nextButton != null)
            _nextButton.Click += OnNextButtonClick;

        UpdatePips();
        UpdateNavigationButtons();
    }

    private static object CoerceNumberOfPages(DependencyObject d, object baseValue)
    {
        int value = (int)baseValue;
        return Math.Max(0, value);
    }

    private static object CoerceSelectedPageIndex(DependencyObject d, object baseValue)
    {
        var pager = (FWPipsPager)d;
        int value = (int)baseValue;
        return Math.Max(0, Math.Min(value, pager.NumberOfPages - 1));
    }

    private static void OnNumberOfPagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPipsPager pager)
        {
            pager.CoerceValue(SelectedPageIndexProperty);
            pager.UpdatePips();
            pager.UpdateNavigationButtons();
        }
    }

    private static void OnSelectedPageIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPipsPager pager)
        {
            pager.UpdatePips();
            pager.UpdateNavigationButtons();

            var args = new SelectedIndexChangedEventArgs(SelectedIndexChangedEvent, pager)
            {
                OldIndex = (int)e.OldValue,
                NewIndex = (int)e.NewValue
            };
            pager.RaiseEvent(args);
        }
    }

    private static void OnMaxVisiblePipsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPipsPager pager)
        {
            pager.UpdatePips();
        }
    }

    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPipsPager pager)
        {
            pager.UpdatePips();
        }
    }

    private void UpdatePips()
    {
        if (_pipsPanel == null)
            return;

        _pipsPanel.Children.Clear();

        int visibleCount = Math.Min(NumberOfPages, MaxVisiblePips);
        int startIndex = CalculateStartIndex();

        for (int i = 0; i < visibleCount; i++)
        {
            int pageIndex = startIndex + i;
            var pip = CreatePip(pageIndex);
            _pipsPanel.Children.Add(pip);
        }
    }

    private int CalculateStartIndex()
    {
        if (NumberOfPages <= MaxVisiblePips)
            return 0;

        int halfVisible = MaxVisiblePips / 2;
        int startIndex = SelectedPageIndex - halfVisible;

        if (startIndex < 0)
            startIndex = 0;
        else if (startIndex + MaxVisiblePips > NumberOfPages)
            startIndex = NumberOfPages - MaxVisiblePips;

        return startIndex;
    }

    private FrameworkElement CreatePip(int pageIndex)
    {
        var button = new Button
        {
            Width = 8,
            Height = 8,
            Margin = new Thickness(4, 0, 4, 0),
            Tag = pageIndex
        };

        if (pageIndex == SelectedPageIndex)
        {
            button.SetValue(FrameworkElement.NameProperty, "SelectedPip");
        }
        else
        {
            button.SetValue(FrameworkElement.NameProperty, "NormalPip");
        }

        button.Click += OnPipClick;

        return button;
    }

    private void OnPipClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int pageIndex)
        {
            SelectedPageIndex = pageIndex;
        }
    }

    private void OnPreviousButtonClick(object sender, RoutedEventArgs e)
    {
        if (SelectedPageIndex > 0)
        {
            SelectedPageIndex--;
        }
    }

    private void OnNextButtonClick(object sender, RoutedEventArgs e)
    {
        if (SelectedPageIndex < NumberOfPages - 1)
        {
            SelectedPageIndex++;
        }
    }

    private void UpdateNavigationButtons()
    {
        if (_previousButton != null)
        {
            _previousButton.IsEnabled = SelectedPageIndex > 0;
            UpdateButtonVisibility(_previousButton, PreviousButtonVisibility, SelectedPageIndex > 0);
        }

        if (_nextButton != null)
        {
            _nextButton.IsEnabled = SelectedPageIndex < NumberOfPages - 1;
            UpdateButtonVisibility(_nextButton, NextButtonVisibility, SelectedPageIndex < NumberOfPages - 1);
        }
    }

    private void UpdateButtonVisibility(FrameworkElement button, PipsPagerButtonVisibility mode, bool canNavigate)
    {
        button.Visibility = mode switch
        {
            PipsPagerButtonVisibility.Collapsed => Visibility.Collapsed,
            PipsPagerButtonVisibility.VisibleOnPointerOver => canNavigate ? Visibility.Visible : Visibility.Collapsed,
            _ => Visibility.Visible
        };
    }
}

/// <summary>
/// Defines button visibility modes for PipsPager navigation buttons.
/// </summary>
public enum PipsPagerButtonVisibility
{
    /// <summary>
    /// Button is always visible.
    /// </summary>
    Visible,

    /// <summary>
    /// Button is visible only when pointer is over the control.
    /// </summary>
    VisibleOnPointerOver,

    /// <summary>
    /// Button is collapsed and never visible.
    /// </summary>
    Collapsed
}

/// <summary>
/// Event arguments for selected index changed event.
/// </summary>
public class SelectedIndexChangedEventArgs : RoutedEventArgs
{
    public SelectedIndexChangedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    /// <summary>
    /// Gets the previous selected index.
    /// </summary>
    public int OldIndex { get; init; }

    /// <summary>
    /// Gets the new selected index.
    /// </summary>
    public int NewIndex { get; init; }
}
