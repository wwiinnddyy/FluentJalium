using Jalium.UI;
using Jalium.UI.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

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
    public FWSnackbarActionEventArgs(bool commandExecuted)
    {
        CommandExecuted = commandExecuted;
    }

    public bool CommandExecuted { get; }

    public bool Handled { get; set; }
}

/// <summary>
/// Describes why a snackbar closed.
/// </summary>
public enum FWSnackbarCloseReason
{
    None,
    Programmatic,
    Action,
    CloseButton,
    Timeout,
    HostCleared
}

/// <summary>
/// Describes where an <see cref="FWSnackbarHost"/> prefers to place transient notifications.
/// </summary>
public enum FWSnackbarPlacement
{
    Top,
    Bottom
}

/// <summary>
/// FluentJalium Snackbar control for transient in-app messages and lightweight undo actions.
/// </summary>
public class FWSnackbar : ContentControl, IFluentJaliumControl
{
    private Button? _actionButton;
    private Button? _closeButton;
    private CancellationTokenSource? _autoDismissCancellation;
    private TaskCompletionSource<FWSnackbarCloseReason>? _closedTask;

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(object), typeof(FWSnackbar),
            new PropertyMetadata(null, OnSnackbarPropertyChanged));

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(object), typeof(FWSnackbar),
            new PropertyMetadata(null, OnSnackbarPropertyChanged));

    public static readonly DependencyProperty ActionContentProperty =
        DependencyProperty.Register(nameof(ActionContent), typeof(object), typeof(FWSnackbar),
            new PropertyMetadata(null, OnSnackbarPropertyChanged));

    public static readonly DependencyProperty ActionCommandProperty =
        DependencyProperty.Register(nameof(ActionCommand), typeof(ICommand), typeof(FWSnackbar),
            new PropertyMetadata(null, OnActionCommandChanged));

    public static readonly DependencyProperty ActionCommandParameterProperty =
        DependencyProperty.Register(nameof(ActionCommandParameter), typeof(object), typeof(FWSnackbar),
            new PropertyMetadata(null, OnSnackbarPropertyChanged));

    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(nameof(Severity), typeof(ToastSeverity), typeof(FWSnackbar),
            new PropertyMetadata(ToastSeverity.Information, OnSnackbarPropertyChanged), IsValidSeverity);

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(FWSnackbar),
            new PropertyMetadata(false, OnIsOpenChanged));

    public static readonly DependencyProperty IsClosableProperty =
        DependencyProperty.Register(nameof(IsClosable), typeof(bool), typeof(FWSnackbar),
            new PropertyMetadata(true, OnSnackbarPropertyChanged));

    public static readonly DependencyProperty IsAutoDismissEnabledProperty =
        DependencyProperty.Register(nameof(IsAutoDismissEnabled), typeof(bool), typeof(FWSnackbar),
            new PropertyMetadata(true, OnSnackbarPropertyChanged));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(TimeSpan), typeof(FWSnackbar),
            new PropertyMetadata(TimeSpan.FromSeconds(4), OnSnackbarPropertyChanged), IsValidDuration);

    private static readonly DependencyPropertyKey LastCloseReasonPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(LastCloseReason), typeof(FWSnackbarCloseReason), typeof(FWSnackbar),
            new PropertyMetadata(FWSnackbarCloseReason.None));

    public static readonly DependencyProperty LastCloseReasonProperty = LastCloseReasonPropertyKey.DependencyProperty;

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

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public object? ActionCommandParameter
    {
        get => GetValue(ActionCommandParameterProperty);
        set => SetValue(ActionCommandParameterProperty, value);
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

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public FWSnackbarCloseReason LastCloseReason
    {
        get => (FWSnackbarCloseReason)GetValue(LastCloseReasonProperty)!;
    }

    public event EventHandler<FWSnackbarActionEventArgs>? ActionClick;

    public event EventHandler? Opened;

    public event EventHandler? Closed;

    public void Show()
    {
        SetValue(LastCloseReasonPropertyKey.DependencyProperty, FWSnackbarCloseReason.None);
        _closedTask = new TaskCompletionSource<FWSnackbarCloseReason>(TaskCreationOptions.RunContinuationsAsynchronously);
        IsOpen = true;
    }

    public Task ShowAsync()
    {
        Show();
        return _closedTask?.Task ?? Task.CompletedTask;
    }

    public Task<FWSnackbarCloseReason> ShowForResultAsync()
    {
        Show();
        return _closedTask?.Task ?? Task.FromResult(LastCloseReason);
    }

    public void Close(FWSnackbarCloseReason reason = FWSnackbarCloseReason.Programmatic)
    {
        SetValue(LastCloseReasonPropertyKey.DependencyProperty, reason);
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
    }

    public bool RequestAction()
    {
        var commandExecuted = ExecuteActionCommand();
        var args = new FWSnackbarActionEventArgs(commandExecuted);
        ActionClick?.Invoke(this, args);
        if (!args.Handled)
        {
            Close(FWSnackbarCloseReason.Action);
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

        UpdateTemplateState();
    }

    private void OnActionButtonClick(object sender, RoutedEventArgs e)
    {
        _ = RequestAction();
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close(FWSnackbarCloseReason.CloseButton);
    }

    private bool ExecuteActionCommand()
    {
        var command = ActionCommand;
        var parameter = ActionCommandParameter;
        if (command == null || !command.CanExecute(parameter))
        {
            return false;
        }

        command.Execute(parameter);
        return true;
    }

    private void OnActionCanExecuteChanged(object? sender, EventArgs e)
    {
        UpdateTemplateState();
    }

    private void UpdateTemplateState()
    {
        if (_actionButton != null)
        {
            _actionButton.Visibility = ActionContent == null && ActionCommand == null ? Visibility.Collapsed : Visibility.Visible;
            _actionButton.IsEnabled = ActionCommand?.CanExecute(ActionCommandParameter) ?? true;
        }

        if (_closeButton != null)
        {
            _closeButton.Visibility = IsClosable ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void ScheduleAutoDismiss()
    {
        _autoDismissCancellation?.Cancel();
        _autoDismissCancellation?.Dispose();
        _autoDismissCancellation = null;

        if (!IsOpen || !IsAutoDismissEnabled || Duration <= TimeSpan.Zero)
        {
            return;
        }

        var cancellation = new CancellationTokenSource();
        _autoDismissCancellation = cancellation;
        _ = DismissAfterDelayAsync(cancellation.Token);
    }

    private async Task DismissAfterDelayAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(Duration, cancellationToken).ConfigureAwait(false);
            if (!cancellationToken.IsCancellationRequested)
            {
                Close(FWSnackbarCloseReason.Timeout);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void CompleteClose()
    {
        _autoDismissCancellation?.Cancel();
        _autoDismissCancellation?.Dispose();
        _autoDismissCancellation = null;
        Closed?.Invoke(this, EventArgs.Empty);
        _closedTask?.TrySetResult(LastCloseReason);
        _closedTask = null;
    }

    private static void OnActionCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FWSnackbar snackbar)
        {
            return;
        }

        if (e.OldValue is ICommand oldCommand)
        {
            oldCommand.CanExecuteChanged -= snackbar.OnActionCanExecuteChanged;
        }

        if (e.NewValue is ICommand newCommand)
        {
            newCommand.CanExecuteChanged += snackbar.OnActionCanExecuteChanged;
        }

        snackbar.UpdateTemplateState();
        OnSnackbarPropertyChanged(d, e);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSnackbar snackbar)
        {
            OnSnackbarPropertyChanged(d, e);

            if (e.NewValue is true)
            {
                snackbar.Opened?.Invoke(snackbar, EventArgs.Empty);
                snackbar.ScheduleAutoDismiss();
            }
            else if (e.OldValue is true)
            {
                snackbar.CompleteClose();
            }
        }
    }

    private static void OnSnackbarPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSnackbar snackbar)
        {
            snackbar.UpdateTemplateState();
            snackbar.ScheduleAutoDismiss();
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
/// Hosts queued <see cref="FWSnackbar"/> instances and promotes pending messages as visible items close.
/// </summary>
public class FWSnackbarHost : Control, IFluentJaliumControl
{
    private readonly Queue<FWSnackbar> _queue = new();
    private readonly ObservableCollection<FWSnackbar> _snackbars = new();
    private ItemsControl? _itemsControl;

    public static readonly DependencyProperty MaxVisibleSnackbarsProperty =
        DependencyProperty.Register(nameof(MaxVisibleSnackbars), typeof(int), typeof(FWSnackbarHost),
            new PropertyMetadata(1, OnHostLayoutChanged), IsValidMaxVisibleSnackbars);

    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(nameof(Placement), typeof(FWSnackbarPlacement), typeof(FWSnackbarHost),
            new PropertyMetadata(FWSnackbarPlacement.Bottom, OnHostLayoutChanged), IsValidPlacement);

    public FWSnackbarHost()
    {
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public ObservableCollection<FWSnackbar> Snackbars => _snackbars;

    public int PendingCount => _queue.Count;

    public FWSnackbar? CurrentSnackbar => _snackbars.Count > 0 ? _snackbars[0] : null;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public int MaxVisibleSnackbars
    {
        get => (int)GetValue(MaxVisibleSnackbarsProperty)!;
        set => SetValue(MaxVisibleSnackbarsProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSnackbarPlacement Placement
    {
        get => (FWSnackbarPlacement)GetValue(PlacementProperty)!;
        set => SetValue(PlacementProperty, value);
    }

    public FWSnackbar Show(ToastSeverity severity, object title, object? message = null, object? actionContent = null, TimeSpan? duration = null)
    {
        var snackbar = new FWSnackbar
        {
            Severity = severity,
            Title = title,
            Message = message,
            ActionContent = actionContent,
            Duration = duration ?? TimeSpan.FromSeconds(4)
        };

        Enqueue(snackbar);
        return snackbar;
    }

    public FWSnackbar Enqueue(FWSnackbar snackbar)
    {
        ArgumentNullException.ThrowIfNull(snackbar);

        if (_snackbars.Contains(snackbar) || _queue.Contains(snackbar))
        {
            return snackbar;
        }

        snackbar.Closed += OnSnackbarClosed;
        if (_snackbars.Count < MaxVisibleSnackbars)
        {
            OpenSnackbar(snackbar);
        }
        else
        {
            _queue.Enqueue(snackbar);
        }

        return snackbar;
    }

    public bool CloseCurrent()
    {
        var snackbar = CurrentSnackbar;
        if (snackbar == null)
        {
            return false;
        }

        snackbar.Close();
        return true;
    }

    public void Clear()
    {
        while (_queue.Count > 0)
        {
            var pending = _queue.Dequeue();
            pending.Closed -= OnSnackbarClosed;
            pending.Close(FWSnackbarCloseReason.HostCleared);
        }

        foreach (var snackbar in new List<FWSnackbar>(_snackbars))
        {
            snackbar.Close(FWSnackbarCloseReason.HostCleared);
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _itemsControl = GetTemplateChild("PART_ItemsControl") as ItemsControl;
        if (_itemsControl != null)
        {
            _itemsControl.ItemsSource = _snackbars;
        }
    }

    private void OnSnackbarClosed(object? sender, EventArgs e)
    {
        if (sender is not FWSnackbar snackbar)
        {
            return;
        }

        snackbar.Closed -= OnSnackbarClosed;
        _snackbars.Remove(snackbar);
        PromotePendingSnackbars();
    }

    private void OpenSnackbar(FWSnackbar snackbar)
    {
        _snackbars.Add(snackbar);
        snackbar.Show();
        InvalidateMeasure();
        InvalidateVisual();
    }

    private void PromotePendingSnackbars()
    {
        while (_snackbars.Count < MaxVisibleSnackbars && _queue.Count > 0)
        {
            OpenSnackbar(_queue.Dequeue());
        }
    }

    private static void OnHostLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSnackbarHost host)
        {
            host.PromotePendingSnackbars();
            host.InvalidateMeasure();
            host.InvalidateVisual();
        }
    }

    private static bool IsValidMaxVisibleSnackbars(object? value)
    {
        return value is int count && count > 0;
    }

    private static bool IsValidPlacement(object? value)
    {
        return value is FWSnackbarPlacement placement && Enum.IsDefined(placement);
    }
}

/// <summary>
/// Lightweight service wrapper that routes snackbar requests into a configured host.
/// </summary>
public class FWSnackbarService
{
    public FWSnackbarHost? Host { get; private set; }

    public event EventHandler? HostChanged;

    public void SetHost(FWSnackbarHost? host)
    {
        if (ReferenceEquals(Host, host))
        {
            return;
        }

        Host = host;
        HostChanged?.Invoke(this, EventArgs.Empty);
    }

    public FWSnackbar Show(ToastSeverity severity, object title, object? message = null, object? actionContent = null, TimeSpan? duration = null)
    {
        return RequireHost().Show(severity, title, message, actionContent, duration);
    }

    public FWSnackbar Enqueue(FWSnackbar snackbar)
    {
        return RequireHost().Enqueue(snackbar);
    }

    public bool CloseCurrent()
    {
        return RequireHost().CloseCurrent();
    }

    public void Clear()
    {
        RequireHost().Clear();
    }

    private FWSnackbarHost RequireHost()
    {
        return Host ?? throw new InvalidOperationException("Configure a FWSnackbarHost before showing snackbar messages.");
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
