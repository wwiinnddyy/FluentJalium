using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium Button control.
/// </summary>
public class FWButton : Button, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium RepeatButton control.
/// </summary>
public class FWRepeatButton : RepeatButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium HyperlinkButton control.
/// </summary>
public class FWHyperlinkButton : HyperlinkButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TextBox control.
/// </summary>
public class FWTextBox : TextBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium PasswordBox control.
/// </summary>
public class FWPasswordBox : PasswordBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium NumberBox control.
/// </summary>
public class FWNumberBox : NumberBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium AutoCompleteBox control.
/// </summary>
public class FWAutoCompleteBox : AutoCompleteBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium RichTextBox control.
/// </summary>
public class FWRichTextBox : RichTextBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium CheckBox control.
/// </summary>
public class FWCheckBox : CheckBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium RadioButton control.
/// </summary>
public class FWRadioButton : RadioButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ToggleButton control.
/// </summary>
public class FWToggleButton : ToggleButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ToggleSwitch control.
/// </summary>
public class FWToggleSwitch : ToggleSwitch, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Slider control.
/// </summary>
public class FWSlider : Slider, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium RangeSlider control.
/// </summary>
public class FWRangeSlider : RangeSlider, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ProgressBar control.
/// </summary>
public class FWProgressBar : ProgressBar, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ComboBox control.
/// </summary>
public class FWComboBox : ComboBox, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWComboBoxItem();
}

/// <summary>
/// FluentJalium ComboBoxItem control.
/// </summary>
public class FWComboBoxItem : ComboBoxItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ListBox control.
/// </summary>
public class FWListBox : ListBox, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWListBoxItem();
}

/// <summary>
/// FluentJalium ListBoxItem control.
/// </summary>
public class FWListBoxItem : ListBoxItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ListView control.
/// </summary>
public class FWListView : ListView, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWListViewItem();
}

/// <summary>
/// FluentJalium ListViewItem control.
/// </summary>
public class FWListViewItem : ListViewItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TreeView control.
/// </summary>
public class FWTreeView : TreeView, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item)
    {
        return item is TreeViewItem treeViewItem ? treeViewItem : new FWTreeViewItem();
    }
}

/// <summary>
/// FluentJalium TreeViewItem control.
/// </summary>
public class FWTreeViewItem : TreeViewItem, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item)
    {
        return item is TreeViewItem treeViewItem ? treeViewItem : new FWTreeViewItem();
    }
}

/// <summary>
/// FluentJalium DataGrid control.
/// </summary>
public class FWDataGrid : DataGrid, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TreeDataGrid control.
/// </summary>
public class FWTreeDataGrid : TreeDataGrid, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TreeSelector control.
/// </summary>
public class FWTreeSelector : TreeSelector, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item)
    {
        return item is TreeSelectorItem treeSelectorItem ? treeSelectorItem : new FWTreeSelectorItem();
    }
}

/// <summary>
/// FluentJalium TreeSelectorItem control.
/// </summary>
public class FWTreeSelectorItem : TreeSelectorItem, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item)
    {
        return item is TreeSelectorItem treeSelectorItem ? treeSelectorItem : new FWTreeSelectorItem();
    }
}

/// <summary>
/// FluentJalium PropertyGrid control.
/// </summary>
public class FWPropertyGrid : PropertyGrid, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium DiffViewer control.
/// </summary>
public class FWDiffViewer : DiffViewer, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium HexEditor control.
/// </summary>
public class FWHexEditor : HexEditor, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium JsonTreeViewer control.
/// </summary>
public class FWJsonTreeViewer : JsonTreeViewer, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium NavigationView control.
/// </summary>
public class FWNavigationView : NavigationView, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium NavigationViewItem control.
/// </summary>
public class FWNavigationViewItem : NavigationViewItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium NavigationViewItemHeader control.
/// </summary>
public class FWNavigationViewItemHeader : NavigationViewItemHeader, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium NavigationViewItemSeparator control.
/// </summary>
public class FWNavigationViewItemSeparator : NavigationViewItemSeparator, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TabControl control.
/// </summary>
public class FWTabControl : TabControl, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TabItem control.
/// </summary>
public class FWTabItem : TabItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Frame control.
/// </summary>
public class FWFrame : Frame, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Expander control.
/// </summary>
public class FWExpander : Expander, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ToolTip control.
/// </summary>
public class FWToolTip : ToolTip, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ContentDialog control.
/// </summary>
public class FWContentDialog : ContentDialog, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium GroupBox control.
/// </summary>
public class FWGroupBox : GroupBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ScrollViewer control.
/// </summary>
public class FWScrollViewer : ScrollViewer, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium SwipeControl control.
/// </summary>
public class FWSwipeControl : SwipeControl, IFluentJaliumControl
{
    private Pen? _borderPen;
    private Brush? _borderPenBrush;
    private double _borderPenThickness;

    /// <inheritdoc />
    protected override void OnRender(DrawingContext drawingContext)
    {
        var bounds = new Rect(0, 0, RenderSize.Width, RenderSize.Height);

        if (Background != null)
        {
            drawingContext.DrawRoundedRectangle(Background, null, bounds, CornerRadius);
        }

        base.OnRender(drawingContext);

        if (BorderBrush != null && BorderThickness.Left > 0)
        {
            if (_borderPen == null || _borderPenBrush != BorderBrush || _borderPenThickness != BorderThickness.Left)
            {
                _borderPen = new Pen(BorderBrush, BorderThickness.Left);
                _borderPenBrush = BorderBrush;
                _borderPenThickness = BorderThickness.Left;
            }

            drawingContext.DrawRoundedRectangle(null, _borderPen, bounds, CornerRadius);
        }
    }
}

/// <summary>
/// FluentJalium GridSplitter control.
/// </summary>
public class FWGridSplitter : GridSplitter, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TextBlock control.
/// </summary>
public class FWTextBlock : TextBlock, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium AccessText control.
/// </summary>
public class FWAccessText : AccessText, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Border control.
/// </summary>
public class FWBorder : Border, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ContentControl control.
/// </summary>
public class FWContentControl : ContentControl, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ContentPresenter control.
/// </summary>
public class FWContentPresenter : ContentPresenter, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium StackPanel control.
/// </summary>
public class FWStackPanel : StackPanel, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium WrapPanel control.
/// </summary>
public class FWWrapPanel : WrapPanel, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Grid control.
/// </summary>
public class FWGrid : Grid, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ColorPicker control.
/// </summary>
public class FWColorPicker : ColorPicker, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium InkCanvas control.
/// </summary>
public class FWInkCanvas : InkCanvas, IFluentJaliumControl
{
    /// <summary>
    /// Identifies the BorderBrush dependency property.
    /// </summary>
    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(FWInkCanvas),
            new PropertyMetadata(null, OnSurfacePropertyChanged));

    /// <summary>
    /// Identifies the BorderThickness dependency property.
    /// </summary>
    public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(FWInkCanvas),
            new PropertyMetadata(new Thickness(0), OnSurfacePropertyChanged));

    /// <summary>
    /// Identifies the CornerRadius dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(FWInkCanvas),
            new PropertyMetadata(new CornerRadius(0), OnSurfacePropertyChanged));

    /// <summary>
    /// Gets or sets the brush used to draw the Fluent surface border.
    /// </summary>
    public Brush? BorderBrush
    {
        get => (Brush?)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the Fluent surface border thickness.
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty)!;
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the Fluent surface corner radius.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty)!;
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPostRender(DrawingContext drawingContext)
    {
        base.OnPostRender(drawingContext);
        DrawSurfaceBorder(drawingContext, BorderBrush, BorderThickness, CornerRadius);
    }

    private static void OnSurfacePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWInkCanvas canvas)
        {
            canvas.InvalidateVisual();
        }
    }

    private void DrawSurfaceBorder(DrawingContext drawingContext, Brush? borderBrush, Thickness thickness, CornerRadius cornerRadius)
    {
        if (borderBrush == null || thickness.Left <= 0 || RenderSize.Width <= 0 || RenderSize.Height <= 0)
            return;

        var halfThickness = thickness.Left / 2;
        var bounds = new Rect(
            halfThickness,
            halfThickness,
            Math.Max(0, RenderSize.Width - thickness.Left),
            Math.Max(0, RenderSize.Height - thickness.Left));
        var pen = new Pen(borderBrush, thickness.Left);
        drawingContext.DrawRoundedRectangle(null, pen, bounds, cornerRadius);
    }
}

/// <summary>
/// FluentJalium InkPresenter control.
/// </summary>
public class FWInkPresenter : InkPresenter, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium MediaElement control.
/// </summary>
public class FWMediaElement : MediaElement, IFluentJaliumControl
{
    /// <summary>
    /// Identifies the Background dependency property.
    /// </summary>
    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(FWMediaElement),
            new PropertyMetadata(null, OnSurfacePropertyChanged));

    /// <summary>
    /// Identifies the BorderBrush dependency property.
    /// </summary>
    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(FWMediaElement),
            new PropertyMetadata(null, OnSurfacePropertyChanged));

    /// <summary>
    /// Identifies the BorderThickness dependency property.
    /// </summary>
    public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(FWMediaElement),
            new PropertyMetadata(new Thickness(0), OnSurfacePropertyChanged));

    /// <summary>
    /// Identifies the CornerRadius dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(FWMediaElement),
            new PropertyMetadata(new CornerRadius(0), OnSurfacePropertyChanged));

    /// <summary>
    /// Gets or sets the Fluent surface background brush.
    /// </summary>
    public Brush? Background
    {
        get => (Brush?)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to draw the Fluent surface border.
    /// </summary>
    public Brush? BorderBrush
    {
        get => (Brush?)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the Fluent surface border thickness.
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty)!;
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the Fluent surface corner radius.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty)!;
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <inheritdoc />
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (Background != null && !HasVideo && !HasAudio && RenderSize.Width > 0 && RenderSize.Height > 0)
        {
            drawingContext.DrawRoundedRectangle(
                Background,
                null,
                new Rect(0, 0, RenderSize.Width, RenderSize.Height),
                CornerRadius);
        }

        if (BorderBrush == null || BorderThickness.Left <= 0 || RenderSize.Width <= 0 || RenderSize.Height <= 0)
            return;

        var halfThickness = BorderThickness.Left / 2;
        var bounds = new Rect(
            halfThickness,
            halfThickness,
            Math.Max(0, RenderSize.Width - BorderThickness.Left),
            Math.Max(0, RenderSize.Height - BorderThickness.Left));
        drawingContext.DrawRoundedRectangle(null, new Pen(BorderBrush, BorderThickness.Left), bounds, CornerRadius);
    }

    private static void OnSurfacePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWMediaElement element)
        {
            element.InvalidateVisual();
        }
    }
}

/// <summary>
/// FluentJalium Image control.
/// </summary>
public class FWImage : Image, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium FontIcon control.
/// </summary>
public class FWFontIcon : FontIcon, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium SymbolIcon control.
/// </summary>
public class FWSymbolIcon : SymbolIcon, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium PathIcon control.
/// </summary>
public class FWPathIcon : PathIcon, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Viewbox control.
/// </summary>
public class FWViewbox : Viewbox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Label control.
/// </summary>
public class FWLabel : Label, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Separator control.
/// </summary>
public class FWSeparator : Separator, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium MenuBar control.
/// </summary>
public class FWMenuBar : MenuBar, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium MenuBarItem control.
/// </summary>
public class FWMenuBarItem : MenuBarItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Menu control.
/// </summary>
public class FWMenu : Menu, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWMenuItem();

    protected override bool IsItemItsOwnContainer(object item) => item is MenuItem or Separator;

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (!ReferenceEquals(element, item) && element is FWMenuItem menuItem && menuItem.Header == null)
        {
            menuItem.Header = item;
        }
    }
}

/// <summary>
/// FluentJalium MenuItem control.
/// </summary>
public class FWMenuItem : MenuItem, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWMenuItem();

    protected override bool IsItemItsOwnContainer(object item) => item is MenuItem or Separator;

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (!ReferenceEquals(element, item) && element is FWMenuItem menuItem && menuItem.Header == null)
        {
            menuItem.Header = item;
        }
    }
}

/// <summary>
/// FluentJalium ContextMenu control.
/// </summary>
public class FWContextMenu : ContextMenu, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWMenuItem();

    protected override bool IsItemItsOwnContainer(object item) => item is MenuItem or Separator;

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (!ReferenceEquals(element, item) && element is FWMenuItem menuItem && menuItem.Header == null)
        {
            menuItem.Header = item;
        }
    }
}

/// <summary>
/// FluentJalium MenuFlyoutItem control.
/// </summary>
public class FWMenuFlyoutItem : MenuFlyoutItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ToggleMenuFlyoutItem control.
/// </summary>
public class FWToggleMenuFlyoutItem : ToggleMenuFlyoutItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium MenuFlyoutSeparator control.
/// </summary>
public class FWMenuFlyoutSeparator : MenuFlyoutSeparator, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium DatePicker control.
/// </summary>
public class FWDatePicker : DatePicker, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TimePicker control.
/// </summary>
public class FWTimePicker : TimePicker, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Calendar control.
/// </summary>
public class FWCalendar : Calendar, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium InfoBar control.
/// </summary>
public class FWInfoBar : InfoBar, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium toast notification item control.
/// </summary>
public class FWToastNotificationItem : ToastNotificationItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium toast notification host control.
/// </summary>
public class FWToastNotificationHost : ToastNotificationHost, IFluentJaliumControl
{
    /// <summary>
    /// Shows a FluentJalium toast notification with the specified severity, title, and message.
    /// </summary>
    public new FWToastNotificationItem Show(ToastSeverity severity, string title, string? message = null, TimeSpan? duration = null)
    {
        var toast = new FWToastNotificationItem
        {
            Severity = severity,
            Title = title,
            Message = message,
            IsAutoDismissEnabled = true,
            Duration = duration ?? TimeSpan.FromSeconds(5)
        };

        ShowToast(toast);
        return toast;
    }

    public new FWToastNotificationItem ShowInformation(string title, string? message = null, TimeSpan? duration = null)
        => Show(ToastSeverity.Information, title, message, duration);

    public new FWToastNotificationItem ShowSuccess(string title, string? message = null, TimeSpan? duration = null)
        => Show(ToastSeverity.Success, title, message, duration);

    public new FWToastNotificationItem ShowWarning(string title, string? message = null, TimeSpan? duration = null)
        => Show(ToastSeverity.Warning, title, message, duration);

    public new FWToastNotificationItem ShowError(string title, string? message = null, TimeSpan? duration = null)
        => Show(ToastSeverity.Error, title, message, duration);
}

/// <summary>
/// FluentJalium StatusBar control.
/// </summary>
public class FWStatusBar : StatusBar, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWStatusBarItem();
}

/// <summary>
/// FluentJalium StatusBarItem control.
/// </summary>
public class FWStatusBarItem : Jalium.UI.Controls.StatusBarItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium SplitButton control.
/// </summary>
public class FWSplitButton : SplitButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium CommandBar control.
/// </summary>
public class FWCommandBar : CommandBar, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium AppBarButton control.
/// </summary>
public class FWAppBarButton : AppBarButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium AppBarToggleButton control.
/// </summary>
public class FWAppBarToggleButton : AppBarToggleButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium AppBarSeparator control.
/// </summary>
public class FWAppBarSeparator : AppBarSeparator, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ToolBar control.
/// </summary>
public class FWToolBar : Jalium.UI.Controls.ToolBar, IFluentJaliumControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FWToolBar"/> class.
    /// </summary>
    public FWToolBar()
    {
        var template = new ItemsPanelTemplate();
        template.SetVisualTree(() => new StackPanel { Orientation = Orientation.Horizontal });
        template.Seal();
        ItemsPanel = template;
    }
}

/// <summary>
/// FluentJalium ToolBarTray control.
/// </summary>
public class FWToolBarTray : Jalium.UI.Controls.ToolBarTray, IFluentJaliumControl
{
}
