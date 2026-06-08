using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Media;
using Jalium.UI.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PopupPlacementMode = Jalium.UI.Controls.Primitives.PlacementMode;

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
/// Event data raised before a snackbar closes.
/// </summary>
public sealed class FWSnackbarClosingEventArgs : EventArgs
{
    public FWSnackbarClosingEventArgs(FWSnackbarCloseReason reason)
    {
        Reason = reason;
    }

    public FWSnackbarCloseReason Reason { get; }

    public bool Cancel { get; set; }
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
/// Describes the transition phase requested by <see cref="FWSnackbarHost"/>.
/// </summary>
public enum FWSnackbarTransitionKind
{
    Show,
    Close
}

/// <summary>
/// Describes the visual presenter phase currently applied to a <see cref="FWSnackbar"/>.
/// </summary>
public enum FWSnackbarPresenterState
{
    Idle,
    Entering,
    Visible,
    Exiting
}

/// <summary>
/// Describes why a snackbar host queue changed.
/// </summary>
public enum FWSnackbarHostQueueChangeReason
{
    Queued,
    Shown,
    Closed,
    Cleared,
    LayoutChanged
}

/// <summary>
/// Snapshot of snackbar host layout, queue, and motion state.
/// </summary>
public readonly record struct FWSnackbarHostDiagnostics(
    int VisibleCount,
    int PendingCount,
    int MaxVisibleSnackbars,
    FWSnackbarPlacement Placement,
    VerticalAlignment VerticalAlignment,
    HorizontalAlignment HorizontalAlignment,
    double Spacing,
    bool IsTransitionEnabled,
    FWContentTransitionProfile TransitionProfile,
    TimeSpan SnackbarTransitionDuration,
    double TransitionOffset)
{
    public bool HasCurrentSnackbar => VisibleCount > 0;

    public bool HasPendingSnackbars => PendingCount > 0;
}

/// <summary>
/// Event data raised when a snackbar host requests an entrance or exit transition.
/// </summary>
public sealed class FWSnackbarTransitionRequestedEventArgs : EventArgs
{
    public FWSnackbarTransitionRequestedEventArgs(
        FWSnackbar snackbar,
        FWSnackbarTransitionKind kind,
        FWSnackbarHostDiagnostics diagnostics)
    {
        Snackbar = snackbar;
        Kind = kind;
        Diagnostics = diagnostics;
        PresenterDiagnostics = snackbar.GetPresenterDiagnostics();
    }

    public FWSnackbar Snackbar { get; }

    public FWSnackbarTransitionKind Kind { get; }

    public FWSnackbarHostDiagnostics Diagnostics { get; }

    public FWSnackbarPresenterDiagnostics PresenterDiagnostics { get; }
}

/// <summary>
/// Event data raised when a snackbar host queue or layout state changes.
/// </summary>
public sealed class FWSnackbarHostQueueChangedEventArgs : EventArgs
{
    public FWSnackbarHostQueueChangedEventArgs(
        FWSnackbarHostQueueChangeReason reason,
        FWSnackbar? snackbar,
        FWSnackbarHostDiagnostics diagnostics)
    {
        Reason = reason;
        Snackbar = snackbar;
        Diagnostics = diagnostics;
    }

    public FWSnackbarHostQueueChangeReason Reason { get; }

    public FWSnackbar? Snackbar { get; }

    public FWSnackbarHostDiagnostics Diagnostics { get; }
}

/// <summary>
/// Snapshot of snackbar presenter motion state.
/// </summary>
public readonly record struct FWSnackbarPresenterDiagnostics(
    FWSnackbarPresenterState PresenterState,
    bool HasPresenter,
    double PresenterOpacity,
    double PresenterOffset,
    TimeSpan TransitionDuration,
    FWSnackbarPlacement Placement,
    FWSnackbarTransitionKind? LastTransitionKind);

/// <summary>
/// FluentJalium Snackbar control for transient in-app messages and lightweight undo actions.
/// </summary>
public class FWSnackbar : ContentControl, IFluentJaliumControl
{
    private Button? _actionButton;
    private Button? _closeButton;
    private FrameworkElement? _presenterRoot;
    private TranslateTransform? _presenterTransform;
    private CancellationTokenSource? _autoDismissCancellation;
    private TaskCompletionSource<FWSnackbarCloseReason>? _closedTask;
    private DispatcherTimer? _presenterTransitionTimer;
    private readonly Stopwatch _presenterTransitionStopwatch = new();
    private bool _manualAutoDismissPause;
    private bool _pointerAutoDismissPause;
    private bool _focusAutoDismissPause;
    private bool _isCompletingPresenterClose;
    private bool _completeCloseWhenPresenterTransitionCompletes;
    private FWSnackbarTransitionKind? _pendingPresenterTransitionKind;
    private FWSnackbarPlacement _pendingPresenterTransitionPlacement;
    private TimeSpan _pendingPresenterTransitionDuration;
    private FWSnackbarTransitionKind? _lastTransitionKind;
    private double _presenterStartOpacity = 1.0;
    private double _presenterTargetOpacity = 1.0;
    private double _presenterStartOffset;
    private double _presenterTargetOffset;
    private double _pendingPresenterTransitionOffset;

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

    public static readonly DependencyProperty IsAutoDismissPausedOnPointerOverEnabledProperty =
        DependencyProperty.Register(nameof(IsAutoDismissPausedOnPointerOverEnabled), typeof(bool), typeof(FWSnackbar),
            new PropertyMetadata(true, OnAutoDismissPausePropertyChanged));

    public static readonly DependencyProperty IsAutoDismissPausedOnFocusEnabledProperty =
        DependencyProperty.Register(nameof(IsAutoDismissPausedOnFocusEnabled), typeof(bool), typeof(FWSnackbar),
            new PropertyMetadata(true, OnAutoDismissPausePropertyChanged));

    private static readonly DependencyPropertyKey LastCloseReasonPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(LastCloseReason), typeof(FWSnackbarCloseReason), typeof(FWSnackbar),
            new PropertyMetadata(FWSnackbarCloseReason.None));

    public static readonly DependencyProperty LastCloseReasonProperty = LastCloseReasonPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsAutoDismissPausedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsAutoDismissPaused), typeof(bool), typeof(FWSnackbar),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsAutoDismissPausedProperty = IsAutoDismissPausedPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey PresenterStatePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(PresenterState), typeof(FWSnackbarPresenterState), typeof(FWSnackbar),
            new PropertyMetadata(FWSnackbarPresenterState.Idle, OnPresenterStateChanged));

    public static readonly DependencyProperty PresenterStateProperty = PresenterStatePropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey PresenterOpacityPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(PresenterOpacity), typeof(double), typeof(FWSnackbar),
            new PropertyMetadata(1.0, OnPresenterVisualStateChanged));

    public static readonly DependencyProperty PresenterOpacityProperty = PresenterOpacityPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey PresenterOffsetPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(PresenterOffset), typeof(double), typeof(FWSnackbar),
            new PropertyMetadata(0.0, OnPresenterVisualStateChanged));

    public static readonly DependencyProperty PresenterOffsetProperty = PresenterOffsetPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey PresenterTransitionDurationPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(PresenterTransitionDuration), typeof(TimeSpan), typeof(FWSnackbar),
            new PropertyMetadata(TimeSpan.Zero));

    public static readonly DependencyProperty PresenterTransitionDurationProperty = PresenterTransitionDurationPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey PresenterPlacementPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(PresenterPlacement), typeof(FWSnackbarPlacement), typeof(FWSnackbar),
            new PropertyMetadata(FWSnackbarPlacement.Bottom, OnPresenterVisualStateChanged));

    public static readonly DependencyProperty PresenterPlacementProperty = PresenterPlacementPropertyKey.DependencyProperty;

    public FWSnackbar()
    {
        UseTemplateContentManagement();
        AddHandler(MouseEnterEvent, new MouseEventHandler(OnMouseEnterHandler));
        AddHandler(MouseLeaveEvent, new MouseEventHandler(OnMouseLeaveHandler));
        AddHandler(GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocusHandler));
        AddHandler(LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocusHandler));
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

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsAutoDismissPausedOnPointerOverEnabled
    {
        get => (bool)GetValue(IsAutoDismissPausedOnPointerOverEnabledProperty)!;
        set => SetValue(IsAutoDismissPausedOnPointerOverEnabledProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsAutoDismissPausedOnFocusEnabled
    {
        get => (bool)GetValue(IsAutoDismissPausedOnFocusEnabledProperty)!;
        set => SetValue(IsAutoDismissPausedOnFocusEnabledProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public FWSnackbarCloseReason LastCloseReason
    {
        get => (FWSnackbarCloseReason)GetValue(LastCloseReasonProperty)!;
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsAutoDismissPaused
    {
        get => (bool)GetValue(IsAutoDismissPausedProperty)!;
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public FWSnackbarPresenterState PresenterState => (FWSnackbarPresenterState)GetValue(PresenterStateProperty)!;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public double PresenterOpacity => (double)GetValue(PresenterOpacityProperty)!;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public double PresenterOffset => (double)GetValue(PresenterOffsetProperty)!;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public TimeSpan PresenterTransitionDuration => (TimeSpan)GetValue(PresenterTransitionDurationProperty)!;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSnackbarPlacement PresenterPlacement => (FWSnackbarPlacement)GetValue(PresenterPlacementProperty)!;

    public event EventHandler<FWSnackbarActionEventArgs>? ActionClick;

    public event EventHandler? Opened;

    public event EventHandler<FWSnackbarClosingEventArgs>? Closing;

    public event EventHandler? Closed;

    public void Show()
    {
        SetValue(LastCloseReasonPropertyKey.DependencyProperty, FWSnackbarCloseReason.None);
        EnsureClosedTask();
        IsOpen = true;
    }

    public Task ShowAsync()
    {
        Show();
        return EnsureClosedTask();
    }

    public Task<FWSnackbarCloseReason> ShowForResultAsync()
    {
        Show();
        return EnsureClosedTask();
    }

    public Task<FWSnackbarCloseReason> WaitForCloseAsync()
    {
        return EnsureClosedTask();
    }

    public void PauseAutoDismiss()
    {
        _manualAutoDismissPause = true;
        UpdateAutoDismissPauseState();
    }

    public void ResumeAutoDismiss()
    {
        _manualAutoDismissPause = false;
        UpdateAutoDismissPauseState();
    }

    public void Close(FWSnackbarCloseReason reason = FWSnackbarCloseReason.Programmatic)
    {
        _ = RequestClose(reason);
    }

    public bool RequestClose(FWSnackbarCloseReason reason = FWSnackbarCloseReason.Programmatic)
    {
        return CloseCore(reason, suppressClosing: false);
    }

    internal bool CloseFromHost(FWSnackbarCloseReason reason)
    {
        return CloseCore(reason, suppressClosing: true);
    }

    private bool CloseCore(FWSnackbarCloseReason reason, bool suppressClosing)
    {
        if (!IsOpen)
        {
            SetValue(LastCloseReasonPropertyKey.DependencyProperty, reason);
            CompleteCloseTask(reason);
            return true;
        }

        if (!suppressClosing)
        {
            var args = new FWSnackbarClosingEventArgs(reason);
            Closing?.Invoke(this, args);
            if (args.Cancel)
            {
                return false;
            }
        }

        SetValue(LastCloseReasonPropertyKey.DependencyProperty, reason);
        IsOpen = false;
        return true;
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

        _presenterRoot = GetTemplateChild("PART_PresenterRoot") as FrameworkElement;
        _actionButton = GetTemplateChild("PART_ActionButton") as Button;
        _closeButton = GetTemplateChild("PART_CloseButton") as Button;

        EnsurePresenterTransform();
        if (!TryStartPendingPresenterTransition())
        {
            UpdatePresenterVisualState();
        }

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

    public FWSnackbarPresenterDiagnostics GetPresenterDiagnostics()
    {
        return new FWSnackbarPresenterDiagnostics(
            PresenterState,
            _presenterRoot is not null,
            PresenterOpacity,
            PresenterOffset,
            PresenterTransitionDuration,
            PresenterPlacement,
            _lastTransitionKind);
    }

    internal void BeginPresenterTransition(
        FWSnackbarTransitionKind kind,
        FWSnackbarPlacement placement,
        double transitionOffset,
        TimeSpan transitionDuration)
    {
        _lastTransitionKind = kind;
        SetValue(PresenterTransitionDurationPropertyKey.DependencyProperty, transitionDuration);
        SetValue(PresenterPlacementPropertyKey.DependencyProperty, placement);

        if (kind == FWSnackbarTransitionKind.Show)
        {
            CancelPresenterCloseTransition();
        }

        if (_presenterRoot is null)
        {
            if (kind == FWSnackbarTransitionKind.Show)
            {
                if (transitionDuration > TimeSpan.Zero && transitionOffset > 0.0)
                {
                    QueuePresenterTransition(kind, placement, transitionOffset, transitionDuration);
                    var signedOffset = ResolveSignedPresenterOffset(placement, transitionOffset);
                    SetPresenterVisual(FWSnackbarPresenterState.Entering, opacity: 0.0, offset: signedOffset);
                }
                else
                {
                    ClearPendingPresenterTransition();
                    SetPresenterVisual(FWSnackbarPresenterState.Visible, opacity: 1.0, offset: 0.0);
                }
            }
            else
            {
                ClearPendingPresenterTransition();
                SetPresenterVisual(FWSnackbarPresenterState.Idle, opacity: 1.0, offset: 0.0);
            }

            return;
        }

        ClearPendingPresenterTransition();
        StartResolvedPresenterTransition(kind, placement, transitionOffset, transitionDuration);
    }

    private void StartResolvedPresenterTransition(
        FWSnackbarTransitionKind kind,
        FWSnackbarPlacement placement,
        double transitionOffset,
        TimeSpan transitionDuration)
    {
        if (transitionDuration <= TimeSpan.Zero || transitionOffset <= 0.0)
        {
            if (kind == FWSnackbarTransitionKind.Show)
            {
                SetPresenterVisual(FWSnackbarPresenterState.Visible, opacity: 1.0, offset: 0.0);
            }
            else
            {
                CompletePresenterClose();
            }

            return;
        }

        var signedOffset = ResolveSignedPresenterOffset(placement, transitionOffset);
        if (kind == FWSnackbarTransitionKind.Show)
        {
            SetPresenterVisual(FWSnackbarPresenterState.Entering, opacity: 0.0, offset: signedOffset);
            StartPresenterTransition(FWSnackbarPresenterState.Visible, 1.0, 0.0, transitionDuration, completeCloseWhenFinished: false);
        }
        else
        {
            SetValue(PresenterStatePropertyKey.DependencyProperty, FWSnackbarPresenterState.Exiting);
            StartPresenterTransition(FWSnackbarPresenterState.Exiting, 0.0, signedOffset, transitionDuration, completeCloseWhenFinished: true);
        }
    }

    private void QueuePresenterTransition(
        FWSnackbarTransitionKind kind,
        FWSnackbarPlacement placement,
        double transitionOffset,
        TimeSpan transitionDuration)
    {
        _pendingPresenterTransitionKind = kind;
        _pendingPresenterTransitionPlacement = placement;
        _pendingPresenterTransitionOffset = transitionOffset;
        _pendingPresenterTransitionDuration = transitionDuration;
    }

    private bool TryStartPendingPresenterTransition()
    {
        if (_presenterRoot is null || _pendingPresenterTransitionKind is not { } kind)
        {
            return false;
        }

        var placement = _pendingPresenterTransitionPlacement;
        var transitionOffset = _pendingPresenterTransitionOffset;
        var transitionDuration = _pendingPresenterTransitionDuration;
        ClearPendingPresenterTransition();
        StartResolvedPresenterTransition(kind, placement, transitionOffset, transitionDuration);
        return true;
    }

    private void ClearPendingPresenterTransition()
    {
        _pendingPresenterTransitionKind = null;
        _pendingPresenterTransitionPlacement = FWSnackbarPlacement.Bottom;
        _pendingPresenterTransitionOffset = 0.0;
        _pendingPresenterTransitionDuration = TimeSpan.Zero;
    }

    private void OnActionButtonClick(object sender, RoutedEventArgs e)
    {
        _ = RequestAction();
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close(FWSnackbarCloseReason.CloseButton);
    }

    private void OnMouseEnterHandler(object sender, MouseEventArgs e)
    {
        if (IsAutoDismissPausedOnPointerOverEnabled)
        {
            _pointerAutoDismissPause = true;
            UpdateAutoDismissPauseState();
        }
    }

    private void OnMouseLeaveHandler(object sender, MouseEventArgs e)
    {
        if (_pointerAutoDismissPause)
        {
            _pointerAutoDismissPause = false;
            UpdateAutoDismissPauseState();
        }
    }

    private void OnGotKeyboardFocusHandler(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (IsAutoDismissPausedOnFocusEnabled)
        {
            _focusAutoDismissPause = true;
            UpdateAutoDismissPauseState();
        }
    }

    private void OnLostKeyboardFocusHandler(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (_focusAutoDismissPause)
        {
            _focusAutoDismissPause = false;
            UpdateAutoDismissPauseState();
        }
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

        if (!IsOpen || !IsAutoDismissEnabled || IsAutoDismissPaused || Duration <= TimeSpan.Zero)
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

    private void UpdateAutoDismissPauseState()
    {
        var isPaused = _manualAutoDismissPause
            || (IsAutoDismissPausedOnPointerOverEnabled && _pointerAutoDismissPause)
            || (IsAutoDismissPausedOnFocusEnabled && _focusAutoDismissPause);

        if (IsAutoDismissPaused != isPaused)
        {
            SetValue(IsAutoDismissPausedPropertyKey.DependencyProperty, isPaused);
        }

        ScheduleAutoDismiss();
    }

    private Task<FWSnackbarCloseReason> EnsureClosedTask()
    {
        if (!IsOpen && _closedTask == null && LastCloseReason != FWSnackbarCloseReason.None)
        {
            return Task.FromResult(LastCloseReason);
        }

        _closedTask ??= new TaskCompletionSource<FWSnackbarCloseReason>(TaskCreationOptions.RunContinuationsAsynchronously);
        return _closedTask.Task;
    }

    private void CompleteCloseTask(FWSnackbarCloseReason reason)
    {
        _closedTask?.TrySetResult(reason);
        _closedTask = null;
    }

    private void CompleteClose()
    {
        if (!_isCompletingPresenterClose && PresenterState == FWSnackbarPresenterState.Exiting)
        {
            return;
        }

        _autoDismissCancellation?.Cancel();
        _autoDismissCancellation?.Dispose();
        _autoDismissCancellation = null;
        _pointerAutoDismissPause = false;
        _focusAutoDismissPause = false;
        UpdateAutoDismissPauseState();
        ClearPendingPresenterTransition();
        CancelPresenterCloseTransition();
        StopPresenterTransitionTimer();
        if (!_isCompletingPresenterClose)
        {
            SetPresenterVisual(FWSnackbarPresenterState.Idle, opacity: 1.0, offset: 0.0);
        }

        Closed?.Invoke(this, EventArgs.Empty);
        CompleteCloseTask(LastCloseReason);
    }

    private void StartPresenterTransition(
        FWSnackbarPresenterState completedState,
        double targetOpacity,
        double targetOffset,
        TimeSpan transitionDuration,
        bool completeCloseWhenFinished)
    {
        CancelPresenterCloseTransition();
        StopPresenterTransitionTimer();

        _presenterStartOpacity = PresenterOpacity;
        _presenterStartOffset = PresenterOffset;
        _presenterTargetOpacity = Math.Clamp(targetOpacity, 0.0, 1.0);
        _presenterTargetOffset = targetOffset;
        _completeCloseWhenPresenterTransitionCompletes = completeCloseWhenFinished;

        if (transitionDuration <= TimeSpan.Zero)
        {
            CompletePresenterTransition(completedState);
            return;
        }

        SetValue(PresenterTransitionDurationPropertyKey.DependencyProperty, transitionDuration);
        _presenterTransitionStopwatch.Restart();
        _presenterTransitionTimer = new DispatcherTimer
        {
            Interval = CompositionTarget.FrameInterval
        };
        _presenterTransitionTimer.Tick += (_, _) => OnPresenterTransitionTick(completedState);
        _presenterTransitionTimer.Start();
    }

    private void OnPresenterTransitionTick(FWSnackbarPresenterState completedState)
    {
        var durationMs = Math.Max(1.0, PresenterTransitionDuration.TotalMilliseconds);
        var elapsed = _presenterTransitionStopwatch.Elapsed.TotalMilliseconds;
        var t = Math.Clamp(elapsed / durationMs, 0.0, 1.0);
        var eased = completedState == FWSnackbarPresenterState.Visible ? EaseOutCubic(t) : EaseInCubic(t);

        SetValue(PresenterOpacityPropertyKey.DependencyProperty, Lerp(_presenterStartOpacity, _presenterTargetOpacity, eased));
        SetValue(PresenterOffsetPropertyKey.DependencyProperty, Lerp(_presenterStartOffset, _presenterTargetOffset, eased));
        UpdatePresenterVisualState();

        if (t >= 1.0)
        {
            CompletePresenterTransition(completedState);
        }
    }

    private void CompletePresenterTransition(FWSnackbarPresenterState completedState)
    {
        StopPresenterTransitionTimer();
        SetPresenterVisual(completedState, _presenterTargetOpacity, _presenterTargetOffset);

        if (_completeCloseWhenPresenterTransitionCompletes)
        {
            _completeCloseWhenPresenterTransitionCompletes = false;
            CompletePresenterClose();
        }
    }

    private void StopPresenterTransitionTimer()
    {
        if (_presenterTransitionTimer is not null)
        {
            _presenterTransitionTimer.Stop();
            _presenterTransitionTimer = null;
        }

        _presenterTransitionStopwatch.Stop();
    }

    private void CompletePresenterClose()
    {
        if (!IsOpen && !_isCompletingPresenterClose)
        {
            _isCompletingPresenterClose = true;
            try
            {
                CompleteClose();
            }
            finally
            {
                _isCompletingPresenterClose = false;
            }
        }
    }

    private void CancelPresenterCloseTransition()
    {
        _completeCloseWhenPresenterTransitionCompletes = false;
    }

    private void SetPresenterVisual(FWSnackbarPresenterState state, double opacity, double offset)
    {
        SetValue(PresenterStatePropertyKey.DependencyProperty, state);
        SetValue(PresenterOpacityPropertyKey.DependencyProperty, Math.Clamp(opacity, 0.0, 1.0));
        SetValue(PresenterOffsetPropertyKey.DependencyProperty, offset);
        UpdatePresenterVisualState();
    }

    private void EnsurePresenterTransform()
    {
        if (_presenterRoot is null)
        {
            return;
        }

        _presenterTransform = _presenterRoot.RenderTransform as TranslateTransform;
        if (_presenterTransform is null)
        {
            _presenterTransform = new TranslateTransform();
            _presenterRoot.RenderTransform = _presenterTransform;
        }
    }

    private void UpdatePresenterVisualState()
    {
        if (_presenterRoot is null)
        {
            return;
        }

        EnsurePresenterTransform();
        _presenterRoot.Opacity = PresenterOpacity;

        if (_presenterTransform is not null)
        {
            _presenterTransform.X = 0.0;
            _presenterTransform.Y = PresenterOffset;
        }

        _presenterRoot.InvalidateVisual();
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

    private static void OnAutoDismissPausePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FWSnackbar snackbar)
        {
            return;
        }

        if (!snackbar.IsAutoDismissPausedOnPointerOverEnabled)
        {
            snackbar._pointerAutoDismissPause = false;
        }

        if (!snackbar.IsAutoDismissPausedOnFocusEnabled)
        {
            snackbar._focusAutoDismissPause = false;
        }

        snackbar.UpdateAutoDismissPauseState();
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

    private static void OnPresenterStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSnackbar snackbar)
        {
            snackbar.InvalidateVisual();
        }
    }

    private static void OnPresenterVisualStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSnackbar snackbar)
        {
            snackbar.UpdatePresenterVisualState();
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

    private static double Lerp(double start, double end, double progress)
    {
        return start + (end - start) * progress;
    }

    private static double ResolveSignedPresenterOffset(FWSnackbarPlacement placement, double transitionOffset)
    {
        return placement == FWSnackbarPlacement.Top ? -transitionOffset : transitionOffset;
    }

    private static double EaseOutCubic(double progress)
    {
        return 1.0 - Math.Pow(1.0 - progress, 3.0);
    }

    private static double EaseInCubic(double progress)
    {
        return progress * progress * progress;
    }
}

/// <summary>
/// Hosts queued <see cref="FWSnackbar"/> instances and promotes pending messages as visible items close.
/// </summary>
public class FWSnackbarHost : Control, IFluentJaliumControl
{
    private const double DefaultSpacing = 8.0;
    private static readonly TimeSpan s_defaultTransitionDuration = TimeSpan.FromMilliseconds(320);
    private readonly Queue<FWSnackbar> _queue = new();
    private readonly ObservableCollection<FWSnackbar> _snackbars = new();
    private ItemsControl? _itemsControl;
    private bool _applyingTransitionProfile;

    public static readonly DependencyProperty MaxVisibleSnackbarsProperty =
        DependencyProperty.Register(nameof(MaxVisibleSnackbars), typeof(int), typeof(FWSnackbarHost),
            new PropertyMetadata(1, OnHostLayoutChanged), IsValidMaxVisibleSnackbars);

    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(nameof(Placement), typeof(FWSnackbarPlacement), typeof(FWSnackbarHost),
            new PropertyMetadata(FWSnackbarPlacement.Bottom, OnHostLayoutChanged), IsValidPlacement);

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(FWSnackbarHost),
            new PropertyMetadata(DefaultSpacing, OnHostLayoutChanged), IsValidSpacing);

    public static readonly DependencyProperty IsTransitionEnabledProperty =
        DependencyProperty.Register(nameof(IsTransitionEnabled), typeof(bool), typeof(FWSnackbarHost),
            new PropertyMetadata(true));

    public static readonly DependencyProperty TransitionProfileProperty =
        DependencyProperty.Register(nameof(TransitionProfile), typeof(FWContentTransitionProfile), typeof(FWSnackbarHost),
            new PropertyMetadata(FWContentTransitionProfile.Entrance, OnTransitionProfileChanged), IsValidTransitionProfile);

    public static readonly DependencyProperty SnackbarTransitionDurationProperty =
        DependencyProperty.Register(nameof(SnackbarTransitionDuration), typeof(TimeSpan), typeof(FWSnackbarHost),
            new PropertyMetadata(s_defaultTransitionDuration), IsValidTransitionDuration);

    public static readonly DependencyProperty TransitionOffsetProperty =
        DependencyProperty.Register(nameof(TransitionOffset), typeof(double), typeof(FWSnackbarHost),
            new PropertyMetadata(16.0), IsValidTransitionOffset);

    public FWSnackbarHost()
    {
        ApplyTransitionProfileState(FWContentTransitionProfile.Entrance);
        ApplyPlacementState();
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

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty)!;
        set => SetValue(SpacingProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsTransitionEnabled
    {
        get => (bool)GetValue(IsTransitionEnabledProperty)!;
        set => SetValue(IsTransitionEnabledProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public FWContentTransitionProfile TransitionProfile
    {
        get => (FWContentTransitionProfile)GetValue(TransitionProfileProperty)!;
        set => SetValue(TransitionProfileProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public TimeSpan SnackbarTransitionDuration
    {
        get => (TimeSpan)GetValue(SnackbarTransitionDurationProperty)!;
        set => SetValue(SnackbarTransitionDurationProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public double TransitionOffset
    {
        get => (double)GetValue(TransitionOffsetProperty)!;
        set => SetValue(TransitionOffsetProperty, value);
    }

    public event EventHandler<FWSnackbarTransitionRequestedEventArgs>? TransitionRequested;

    public event EventHandler<FWSnackbarHostQueueChangedEventArgs>? QueueChanged;

    public FWSnackbarHostDiagnostics GetDiagnostics()
    {
        return new FWSnackbarHostDiagnostics(
            _snackbars.Count,
            _queue.Count,
            MaxVisibleSnackbars,
            Placement,
            VerticalContentAlignment,
            HorizontalContentAlignment,
            Spacing,
            IsTransitionEnabled,
            TransitionProfile,
            SnackbarTransitionDuration,
            TransitionOffset);
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

    public Task<FWSnackbarCloseReason> ShowForResultAsync(ToastSeverity severity, object title, object? message = null, object? actionContent = null, TimeSpan? duration = null)
    {
        var snackbar = Show(severity, title, message, actionContent, duration);
        return snackbar.WaitForCloseAsync();
    }

    public FWSnackbar Enqueue(FWSnackbar snackbar)
    {
        ArgumentNullException.ThrowIfNull(snackbar);

        if (_snackbars.Contains(snackbar) || _queue.Contains(snackbar))
        {
            return snackbar;
        }

        snackbar.Closed += OnSnackbarClosed;
        snackbar.Closing += OnSnackbarClosing;
        if (_snackbars.Count < MaxVisibleSnackbars)
        {
            OpenSnackbar(snackbar);
        }
        else
        {
            _queue.Enqueue(snackbar);
            RaiseQueueChanged(FWSnackbarHostQueueChangeReason.Queued, snackbar);
        }

        return snackbar;
    }

    public Task<FWSnackbarCloseReason> EnqueueForResultAsync(FWSnackbar snackbar)
    {
        return Enqueue(snackbar).WaitForCloseAsync();
    }

    public bool CloseCurrent()
    {
        var snackbar = CurrentSnackbar;
        if (snackbar == null)
        {
            return false;
        }

        return snackbar.RequestClose();
    }

    public void Clear()
    {
        while (_queue.Count > 0)
        {
            var pending = _queue.Dequeue();
            pending.Closed -= OnSnackbarClosed;
            pending.Closing -= OnSnackbarClosing;
            pending.CloseFromHost(FWSnackbarCloseReason.HostCleared);
            RaiseQueueChanged(FWSnackbarHostQueueChangeReason.Cleared, pending);
        }

        foreach (var snackbar in new List<FWSnackbar>(_snackbars))
        {
            RequestTransition(snackbar, FWSnackbarTransitionKind.Close);
            snackbar.CloseFromHost(FWSnackbarCloseReason.HostCleared);
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _itemsControl = GetTemplateChild("PART_ItemsControl") as ItemsControl;
        if (_itemsControl != null)
        {
            _itemsControl.ItemsSource = _snackbars;
            ApplyPlacementState();
        }
    }

    private void OnSnackbarClosed(object? sender, EventArgs e)
    {
        if (sender is not FWSnackbar snackbar)
        {
            return;
        }

        snackbar.Closed -= OnSnackbarClosed;
        snackbar.Closing -= OnSnackbarClosing;
        _snackbars.Remove(snackbar);
        RaiseQueueChanged(FWSnackbarHostQueueChangeReason.Closed, snackbar);
        PromotePendingSnackbars();
    }

    private void OnSnackbarClosing(object? sender, FWSnackbarClosingEventArgs e)
    {
        if (sender is FWSnackbar snackbar && !e.Cancel)
        {
            RequestTransition(snackbar, FWSnackbarTransitionKind.Close);
        }
    }

    private void OpenSnackbar(FWSnackbar snackbar)
    {
        _snackbars.Add(snackbar);
        RequestTransition(snackbar, FWSnackbarTransitionKind.Show);
        snackbar.Show();
        RaiseQueueChanged(FWSnackbarHostQueueChangeReason.Shown, snackbar);
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

    private void ApplyPlacementState()
    {
        var verticalAlignment = Placement == FWSnackbarPlacement.Bottom
            ? VerticalAlignment.Bottom
            : VerticalAlignment.Top;

        VerticalContentAlignment = verticalAlignment;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;

        if (_itemsControl == null)
        {
            return;
        }

        _itemsControl.VerticalAlignment = verticalAlignment;
        _itemsControl.HorizontalAlignment = HorizontalContentAlignment;
        _itemsControl.ItemsPanel = CreateItemsPanelTemplate(Spacing);
        _itemsControl.InvalidateMeasure();
        _itemsControl.InvalidateVisual();
    }

    private void RequestTransition(FWSnackbar snackbar, FWSnackbarTransitionKind kind)
    {
        if (!IsTransitionEnabled)
        {
            return;
        }

        snackbar.BeginPresenterTransition(kind, Placement, TransitionOffset, SnackbarTransitionDuration);
        TransitionRequested?.Invoke(this, new FWSnackbarTransitionRequestedEventArgs(snackbar, kind, GetDiagnostics()));
    }

    private void RaiseQueueChanged(FWSnackbarHostQueueChangeReason reason, FWSnackbar? snackbar)
    {
        QueueChanged?.Invoke(this, new FWSnackbarHostQueueChangedEventArgs(reason, snackbar, GetDiagnostics()));
    }

    private void ApplyTransitionProfileState(FWContentTransitionProfile profile)
    {
        _applyingTransitionProfile = true;
        try
        {
            var duration = FWContentTransitionRecipe.Create(profile).Duration;
            SetCurrentValue(SnackbarTransitionDurationProperty, duration.HasTimeSpan ? duration.TimeSpan : s_defaultTransitionDuration);
        }
        finally
        {
            _applyingTransitionProfile = false;
        }
    }

    private static ItemsPanelTemplate CreateItemsPanelTemplate(double spacing)
    {
        var template = new ItemsPanelTemplate();
        template.SetVisualTree(() => new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = spacing
        });
        template.Seal();
        return template;
    }

    private static void OnHostLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSnackbarHost host)
        {
            host.ApplyPlacementState();
            host.PromotePendingSnackbars();
            host.RaiseQueueChanged(FWSnackbarHostQueueChangeReason.LayoutChanged, null);
            host.InvalidateMeasure();
            host.InvalidateVisual();
        }
    }

    private static void OnTransitionProfileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSnackbarHost host &&
            !host._applyingTransitionProfile &&
            e.NewValue is FWContentTransitionProfile profile)
        {
            host.ApplyTransitionProfileState(profile);
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

    private static bool IsValidSpacing(object? value)
    {
        return value is double spacing && spacing >= 0 && !double.IsNaN(spacing) && !double.IsInfinity(spacing);
    }

    private static bool IsValidTransitionProfile(object? value)
    {
        return value is FWContentTransitionProfile profile && Enum.IsDefined(profile);
    }

    private static bool IsValidTransitionDuration(object? value)
    {
        return value is TimeSpan duration && duration >= TimeSpan.Zero;
    }

    private static bool IsValidTransitionOffset(object? value)
    {
        return value is double offset && offset >= 0 && !double.IsNaN(offset) && !double.IsInfinity(offset);
    }
}

/// <summary>
/// Snackbar host that presents queued snackbars through an overlay popup while reusing host queue semantics.
/// </summary>
public class FWSnackbarOverlayHost : FWSnackbarHost
{
    private static readonly DependencyPropertyKey IsOverlayOpenPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsOverlayOpen), typeof(bool), typeof(FWSnackbarOverlayHost),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsOverlayOpenProperty = IsOverlayOpenPropertyKey.DependencyProperty;

    public static readonly DependencyProperty OverlayTargetProperty =
        DependencyProperty.Register(nameof(OverlayTarget), typeof(UIElement), typeof(FWSnackbarOverlayHost),
            new PropertyMetadata(null));

    public static readonly DependencyProperty OverlayPlacementProperty =
        DependencyProperty.Register(nameof(OverlayPlacement), typeof(PopupPlacementMode), typeof(FWSnackbarOverlayHost),
            new PropertyMetadata(PopupPlacementMode.Bottom), IsValidOverlayPlacement);

    public static readonly DependencyProperty IsOverlayAutoOpenEnabledProperty =
        DependencyProperty.Register(nameof(IsOverlayAutoOpenEnabled), typeof(bool), typeof(FWSnackbarOverlayHost),
            new PropertyMetadata(true, OnOverlayAutoOpenChanged));

    public FWSnackbarOverlayHost()
    {
        QueueChanged += OnOverlayQueueChanged;
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsOverlayOpen => (bool)GetValue(IsOverlayOpenProperty)!;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public UIElement? OverlayTarget
    {
        get => (UIElement?)GetValue(OverlayTargetProperty);
        set => SetValue(OverlayTargetProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public PopupPlacementMode OverlayPlacement
    {
        get => (PopupPlacementMode)GetValue(OverlayPlacementProperty)!;
        set => SetValue(OverlayPlacementProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsOverlayAutoOpenEnabled
    {
        get => (bool)GetValue(IsOverlayAutoOpenEnabledProperty)!;
        set => SetValue(IsOverlayAutoOpenEnabledProperty, value);
    }

    public event EventHandler? OverlayOpened;

    public event EventHandler? OverlayClosed;

    public bool OpenOverlay()
    {
        if (IsOverlayOpen)
        {
            return false;
        }

        SetValue(IsOverlayOpenPropertyKey.DependencyProperty, true);
        OverlayOpened?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool CloseOverlay()
    {
        if (!IsOverlayOpen)
        {
            return false;
        }

        SetValue(IsOverlayOpenPropertyKey.DependencyProperty, false);
        OverlayClosed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private void OnOverlayQueueChanged(object? sender, FWSnackbarHostQueueChangedEventArgs e)
    {
        UpdateOverlayState(e.Diagnostics);
    }

    private void UpdateOverlayState(FWSnackbarHostDiagnostics diagnostics)
    {
        if (!IsOverlayAutoOpenEnabled)
        {
            return;
        }

        if (diagnostics.VisibleCount > 0 || diagnostics.PendingCount > 0)
        {
            OpenOverlay();
        }
        else
        {
            CloseOverlay();
        }
    }

    private static void OnOverlayAutoOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSnackbarOverlayHost host && e.NewValue is true)
        {
            host.UpdateOverlayState(host.GetDiagnostics());
        }
    }

    private static bool IsValidOverlayPlacement(object? value)
    {
        return value is PopupPlacementMode placement && Enum.IsDefined(placement);
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

    public Task<FWSnackbarCloseReason> ShowForResultAsync(ToastSeverity severity, object title, object? message = null, object? actionContent = null, TimeSpan? duration = null)
    {
        return RequireHost().ShowForResultAsync(severity, title, message, actionContent, duration);
    }

    public FWSnackbar Enqueue(FWSnackbar snackbar)
    {
        return RequireHost().Enqueue(snackbar);
    }

    public Task<FWSnackbarCloseReason> EnqueueForResultAsync(FWSnackbar snackbar)
    {
        return RequireHost().EnqueueForResultAsync(snackbar);
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
