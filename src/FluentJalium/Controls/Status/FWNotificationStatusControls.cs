using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

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
/// Event data for snackbar action requests.
/// </summary>
public sealed class FWSnackbarActionEventArgs : EventArgs
{
    public bool Handled { get; set; }
}

/// <summary>
/// FluentJalium Snackbar control for transient in-app messages and lightweight undo actions.
/// </summary>
public class FWSnackbar : ContentControl, IFluentJaliumControl
{
    private Button? _actionButton;
    private Button? _closeButton;

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(object), typeof(FWSnackbar),
            new PropertyMetadata(null, OnSnackbarPropertyChanged));

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(object), typeof(FWSnackbar),
            new PropertyMetadata(null, OnSnackbarPropertyChanged));

    public static readonly DependencyProperty ActionContentProperty =
        DependencyProperty.Register(nameof(ActionContent), typeof(object), typeof(FWSnackbar),
            new PropertyMetadata(null, OnSnackbarPropertyChanged));

    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(nameof(Severity), typeof(ToastSeverity), typeof(FWSnackbar),
            new PropertyMetadata(ToastSeverity.Information, OnSnackbarPropertyChanged), IsValidSeverity);

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(FWSnackbar),
            new PropertyMetadata(false, OnSnackbarPropertyChanged));

    public static readonly DependencyProperty IsClosableProperty =
        DependencyProperty.Register(nameof(IsClosable), typeof(bool), typeof(FWSnackbar),
            new PropertyMetadata(true, OnSnackbarPropertyChanged));

    public static readonly DependencyProperty IsAutoDismissEnabledProperty =
        DependencyProperty.Register(nameof(IsAutoDismissEnabled), typeof(bool), typeof(FWSnackbar),
            new PropertyMetadata(true));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(TimeSpan), typeof(FWSnackbar),
            new PropertyMetadata(TimeSpan.FromSeconds(4)), IsValidDuration);

    public FWSnackbar()
    {
        UseTemplateContentManagement();
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public ToastSeverity Severity
    {
        get => (ToastSeverity)GetValue(SeverityProperty)!;
        set => SetValue(SeverityProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty)!;
        set => SetValue(IsOpenProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty)!;
        set => SetValue(IsClosableProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsAutoDismissEnabled
    {
        get => (bool)GetValue(IsAutoDismissEnabledProperty)!;
        set => SetValue(IsAutoDismissEnabledProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty)!;
        set => SetValue(DurationProperty, value);
    }

    public event EventHandler<FWSnackbarActionEventArgs>? ActionClick;

    public event EventHandler? Closed;

    public void Show()
    {
        IsOpen = true;
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    public bool RequestAction()
    {
        var args = new FWSnackbarActionEventArgs();
        ActionClick?.Invoke(this, args);
        if (!args.Handled)
        {
            Close();
        }

        return args.Handled;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_actionButton != null)
        {
            _actionButton.Click -= OnActionButtonClick;
        }

        if (_closeButton != null)
        {
            _closeButton.Click -= OnCloseButtonClick;
        }

        _actionButton = GetTemplateChild("PART_ActionButton") as Button;
        _closeButton = GetTemplateChild("PART_CloseButton") as Button;

        if (_actionButton != null)
        {
            _actionButton.Click += OnActionButtonClick;
        }

        if (_closeButton != null)
        {
            _closeButton.Click += OnCloseButtonClick;
        }
    }

    private void OnActionButtonClick(object sender, RoutedEventArgs e)
    {
        _ = RequestAction();
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private static void OnSnackbarPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSnackbar snackbar)
        {
            snackbar.InvalidateMeasure();
            snackbar.InvalidateVisual();
        }
    }

    private static bool IsValidSeverity(object? value)
    {
        return value is ToastSeverity severity && Enum.IsDefined(severity);
    }

    private static bool IsValidDuration(object? value)
    {
        return value is TimeSpan duration && duration >= TimeSpan.Zero;
    }
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
