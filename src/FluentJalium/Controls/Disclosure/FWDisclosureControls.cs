using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using System.Threading;
using System.Threading.Tasks;
using ICommand = System.Windows.Input.ICommand;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium density presets for disclosure, flyout, and dialog surfaces.
/// </summary>
public enum FWDisclosureDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium Expander control.
/// </summary>
public class FWExpander : Expander, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDisclosureDensity), typeof(FWExpander),
            new PropertyMetadata(FWDisclosureDensity.Comfortable, OnDensityChanged));

    public FWExpander()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDisclosureDensity Density
    {
        get => (FWDisclosureDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (Thickness Padding, double MinHeight) GetExpanderMetrics(FWDisclosureDensity density)
    {
        return density switch
        {
            FWDisclosureDensity.Compact => (new Thickness(10), 36.0),
            FWDisclosureDensity.Spacious => (new Thickness(18), 48.0),
            _ => (new Thickness(14), 40.0)
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWExpander expander && e.NewValue is FWDisclosureDensity density)
        {
            ApplyDensity(expander, density);
        }
    }

    private static void ApplyDensity(FWExpander expander, FWDisclosureDensity density)
    {
        var (padding, minHeight) = GetExpanderMetrics(density);
        expander.Padding = padding;
        expander.MinHeight = minHeight;
    }
}

/// <summary>
/// FluentJalium ToolTip control.
/// </summary>
public class FWToolTip : ToolTip, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDisclosureDensity), typeof(FWToolTip),
            new PropertyMetadata(FWDisclosureDensity.Comfortable, OnDensityChanged));

    public FWToolTip()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDisclosureDensity Density
    {
        get => (FWDisclosureDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (Thickness Padding, double MinHeight) GetToolTipMetrics(FWDisclosureDensity density)
    {
        return density switch
        {
            FWDisclosureDensity.Compact => (new Thickness(6, 3, 6, 3), 24.0),
            FWDisclosureDensity.Spacious => (new Thickness(10, 7, 10, 7), 32.0),
            _ => (new Thickness(8, 5, 8, 5), 28.0)
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWToolTip toolTip && e.NewValue is FWDisclosureDensity density)
        {
            ApplyDensity(toolTip, density);
        }
    }

    private static void ApplyDensity(FWToolTip toolTip, FWDisclosureDensity density)
    {
        var (padding, minHeight) = GetToolTipMetrics(density);
        toolTip.Padding = padding;
        toolTip.MinHeight = minHeight;
    }
}

/// <summary>
/// FluentJalium ContentDialog control.
/// </summary>
public class FWContentDialog : ContentDialog, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDisclosureDensity), typeof(FWContentDialog),
            new PropertyMetadata(FWDisclosureDensity.Comfortable, OnDensityChanged));

    public FWContentDialog()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDisclosureDensity Density
    {
        get => (FWDisclosureDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (Thickness Padding, double MinWidth, double MaxWidth) GetDialogMetrics(FWDisclosureDensity density)
    {
        return density switch
        {
            FWDisclosureDensity.Compact => (new Thickness(20), 300.0, 520.0),
            FWDisclosureDensity.Spacious => (new Thickness(28), 340.0, 600.0),
            _ => (new Thickness(24), 320.0, 548.0)
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWContentDialog dialog && e.NewValue is FWDisclosureDensity density)
        {
            ApplyDensity(dialog, density);
        }
    }

    private static void ApplyDensity(FWContentDialog dialog, FWDisclosureDensity density)
    {
        var (padding, minWidth, maxWidth) = GetDialogMetrics(density);
        dialog.Padding = padding;
        dialog.MinWidth = minWidth;
        dialog.MaxWidth = maxWidth;
    }
}

/// <summary>
/// Describes the button that should receive default emphasis in <see cref="FWTaskDialog"/>.
/// </summary>
public enum FWTaskDialogButton
{
    None,
    Primary,
    Secondary,
    Close
}

/// <summary>
/// Describes the latest result chosen from <see cref="FWTaskDialog"/>.
/// </summary>
public enum FWTaskDialogResult
{
    None,
    Primary,
    Secondary,
    Close
}

/// <summary>
/// Event data for task dialog button requests.
/// </summary>
public sealed class FWTaskDialogButtonClickEventArgs : EventArgs
{
    public FWTaskDialogButtonClickEventArgs(FWTaskDialogResult result)
    {
        Result = result;
    }

    public FWTaskDialogResult Result { get; }

    public bool CommandExecuted { get; internal set; }

    public bool Cancel { get; set; }
}

/// <summary>
/// Event data raised before an <see cref="FWTaskDialog"/> closes.
/// </summary>
public sealed class FWTaskDialogClosingEventArgs : EventArgs
{
    public FWTaskDialogClosingEventArgs(FWTaskDialogResult result)
    {
        Result = result;
    }

    public FWTaskDialogResult Result { get; }

    public bool Cancel { get; set; }
}

/// <summary>
/// Event data raised after an <see cref="FWTaskDialog"/> closes.
/// </summary>
public sealed class FWTaskDialogClosedEventArgs : EventArgs
{
    public FWTaskDialogClosedEventArgs(FWTaskDialogResult result)
    {
        Result = result;
    }

    public FWTaskDialogResult Result { get; }
}

/// <summary>
/// FluentJalium TaskDialog control for command confirmation and rich status prompts.
/// </summary>
public class FWTaskDialog : ContentControl, IFluentJaliumControl
{
    private Button? _primaryButton;
    private Button? _secondaryButton;
    private Button? _closeButton;
    private TaskCompletionSource<FWTaskDialogResult>? _showTask;
    private CancellationTokenRegistration _showCancellationRegistration;

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(object), typeof(FWTaskDialog),
            new PropertyMetadata(null, OnDialogPropertyChanged));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(object), typeof(FWTaskDialog),
            new PropertyMetadata(null, OnDialogPropertyChanged));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(FWTaskDialog),
            new PropertyMetadata(null, OnDialogPropertyChanged));

    public static readonly DependencyProperty PrimaryButtonTextProperty =
        DependencyProperty.Register(nameof(PrimaryButtonText), typeof(string), typeof(FWTaskDialog),
            new PropertyMetadata(string.Empty, OnDialogPropertyChanged));

    public static readonly DependencyProperty SecondaryButtonTextProperty =
        DependencyProperty.Register(nameof(SecondaryButtonText), typeof(string), typeof(FWTaskDialog),
            new PropertyMetadata(string.Empty, OnDialogPropertyChanged));

    public static readonly DependencyProperty CloseButtonTextProperty =
        DependencyProperty.Register(nameof(CloseButtonText), typeof(string), typeof(FWTaskDialog),
            new PropertyMetadata("Close", OnDialogPropertyChanged));

    public static readonly DependencyProperty DefaultButtonProperty =
        DependencyProperty.Register(nameof(DefaultButton), typeof(FWTaskDialogButton), typeof(FWTaskDialog),
            new PropertyMetadata(FWTaskDialogButton.Primary), IsValidButton);

    public static readonly DependencyProperty CancelButtonProperty =
        DependencyProperty.Register(nameof(CancelButton), typeof(FWTaskDialogButton), typeof(FWTaskDialog),
            new PropertyMetadata(FWTaskDialogButton.Close), IsValidButton);

    public static readonly DependencyProperty PrimaryButtonCommandProperty =
        DependencyProperty.Register(nameof(PrimaryButtonCommand), typeof(ICommand), typeof(FWTaskDialog),
            new PropertyMetadata(null, OnButtonCommandChanged));

    public static readonly DependencyProperty PrimaryButtonCommandParameterProperty =
        DependencyProperty.Register(nameof(PrimaryButtonCommandParameter), typeof(object), typeof(FWTaskDialog),
            new PropertyMetadata(null, OnButtonCommandParameterChanged));

    public static readonly DependencyProperty SecondaryButtonCommandProperty =
        DependencyProperty.Register(nameof(SecondaryButtonCommand), typeof(ICommand), typeof(FWTaskDialog),
            new PropertyMetadata(null, OnButtonCommandChanged));

    public static readonly DependencyProperty SecondaryButtonCommandParameterProperty =
        DependencyProperty.Register(nameof(SecondaryButtonCommandParameter), typeof(object), typeof(FWTaskDialog),
            new PropertyMetadata(null, OnButtonCommandParameterChanged));

    public static readonly DependencyProperty CloseButtonCommandProperty =
        DependencyProperty.Register(nameof(CloseButtonCommand), typeof(ICommand), typeof(FWTaskDialog),
            new PropertyMetadata(null, OnButtonCommandChanged));

    public static readonly DependencyProperty CloseButtonCommandParameterProperty =
        DependencyProperty.Register(nameof(CloseButtonCommandParameter), typeof(object), typeof(FWTaskDialog),
            new PropertyMetadata(null, OnButtonCommandParameterChanged));

    public static readonly DependencyProperty ResultProperty =
        DependencyProperty.Register(nameof(Result), typeof(FWTaskDialogResult), typeof(FWTaskDialog),
            new PropertyMetadata(FWTaskDialogResult.None), IsValidResult);

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(FWTaskDialog),
            new PropertyMetadata(false, OnIsOpenChanged));

    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDisclosureDensity), typeof(FWTaskDialog),
            new PropertyMetadata(FWDisclosureDensity.Comfortable, OnDensityChanged));

    public FWTaskDialog()
    {
        UseTemplateContentManagement();
        ApplyDensity(this, Density);
        Focusable = true;
        AddHandler(KeyDownEvent, new KeyEventHandler(OnKeyDownHandler));
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string PrimaryButtonText
    {
        get => (string)(GetValue(PrimaryButtonTextProperty) ?? string.Empty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string SecondaryButtonText
    {
        get => (string)(GetValue(SecondaryButtonTextProperty) ?? string.Empty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string CloseButtonText
    {
        get => (string)(GetValue(CloseButtonTextProperty) ?? string.Empty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public FWTaskDialogButton DefaultButton
    {
        get => (FWTaskDialogButton)GetValue(DefaultButtonProperty)!;
        set => SetValue(DefaultButtonProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public FWTaskDialogButton CancelButton
    {
        get => (FWTaskDialogButton)GetValue(CancelButtonProperty)!;
        set => SetValue(CancelButtonProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public ICommand? PrimaryButtonCommand
    {
        get => (ICommand?)GetValue(PrimaryButtonCommandProperty);
        set => SetValue(PrimaryButtonCommandProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public object? PrimaryButtonCommandParameter
    {
        get => GetValue(PrimaryButtonCommandParameterProperty);
        set => SetValue(PrimaryButtonCommandParameterProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public ICommand? SecondaryButtonCommand
    {
        get => (ICommand?)GetValue(SecondaryButtonCommandProperty);
        set => SetValue(SecondaryButtonCommandProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public object? SecondaryButtonCommandParameter
    {
        get => GetValue(SecondaryButtonCommandParameterProperty);
        set => SetValue(SecondaryButtonCommandParameterProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public ICommand? CloseButtonCommand
    {
        get => (ICommand?)GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public object? CloseButtonCommandParameter
    {
        get => GetValue(CloseButtonCommandParameterProperty);
        set => SetValue(CloseButtonCommandParameterProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public FWTaskDialogResult Result
    {
        get => (FWTaskDialogResult)GetValue(ResultProperty)!;
        private set => SetValue(ResultProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty)!;
        set => SetValue(IsOpenProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDisclosureDensity Density
    {
        get => (FWDisclosureDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    public event EventHandler<FWTaskDialogButtonClickEventArgs>? PrimaryButtonClick;

    public event EventHandler<FWTaskDialogButtonClickEventArgs>? SecondaryButtonClick;

    public event EventHandler<FWTaskDialogButtonClickEventArgs>? CloseButtonClick;

    public event EventHandler? Opening;

    public event EventHandler? Opened;

    public event EventHandler<FWTaskDialogClosingEventArgs>? Closing;

    public event EventHandler<FWTaskDialogClosedEventArgs>? Closed;

    public void Open()
    {
        Result = FWTaskDialogResult.None;
        IsOpen = true;
    }

    public bool Close(FWTaskDialogResult result = FWTaskDialogResult.Close)
    {
        if (!IsOpen)
        {
            Result = result;
            CompleteShowTask(result);
            return true;
        }

        var args = new FWTaskDialogClosingEventArgs(result);
        Closing?.Invoke(this, args);
        if (args.Cancel)
        {
            return false;
        }

        Result = result;
        IsOpen = false;
        return true;
    }

    public Task<FWTaskDialogResult> ShowAsync(CancellationToken cancellationToken = default)
    {
        if (IsOpen && _showTask != null)
        {
            return _showTask.Task;
        }

        _showTask = new TaskCompletionSource<FWTaskDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        _showCancellationRegistration.Dispose();
        if (cancellationToken.CanBeCanceled)
        {
            _showCancellationRegistration = cancellationToken.Register(static state =>
            {
                ((FWTaskDialog)state!).Close(FWTaskDialogResult.Close);
            }, this);
        }

        Open();
        return _showTask.Task;
    }

    public bool RequestPrimaryButtonClick()
    {
        return RequestButton(FWTaskDialogResult.Primary, PrimaryButtonClick);
    }

    public bool RequestSecondaryButtonClick()
    {
        return RequestButton(FWTaskDialogResult.Secondary, SecondaryButtonClick);
    }

    public bool RequestCloseButtonClick()
    {
        return RequestButton(FWTaskDialogResult.Close, CloseButtonClick);
    }

    public bool RequestDefaultButtonClick()
    {
        return RequestButton(ToResult(DefaultButton), HandlerFor(DefaultButton));
    }

    public bool RequestCancelButtonClick()
    {
        return RequestButton(ToResult(CancelButton), HandlerFor(CancelButton));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_primaryButton != null)
        {
            _primaryButton.Click -= OnPrimaryButtonClick;
        }

        if (_secondaryButton != null)
        {
            _secondaryButton.Click -= OnSecondaryButtonClick;
        }

        if (_closeButton != null)
        {
            _closeButton.Click -= OnCloseButtonClick;
        }

        _primaryButton = GetTemplateChild("PART_PrimaryButton") as Button;
        _secondaryButton = GetTemplateChild("PART_SecondaryButton") as Button;
        _closeButton = GetTemplateChild("PART_CloseButton") as Button;

        if (_primaryButton != null)
        {
            _primaryButton.Click += OnPrimaryButtonClick;
        }

        if (_secondaryButton != null)
        {
            _secondaryButton.Click += OnSecondaryButtonClick;
        }

        if (_closeButton != null)
        {
            _closeButton.Click += OnCloseButtonClick;
        }

        UpdateButtonState();
        FocusDefaultButton();
    }

    private bool RequestButton(FWTaskDialogResult result, EventHandler<FWTaskDialogButtonClickEventArgs>? handler)
    {
        if (result == FWTaskDialogResult.None)
        {
            return false;
        }

        var button = ToButton(result);
        if (!CanExecuteButtonCommand(button))
        {
            return false;
        }

        var commandExecuted = ExecuteButtonCommand(button);
        var args = new FWTaskDialogButtonClickEventArgs(result);
        args.CommandExecuted = commandExecuted;
        handler?.Invoke(this, args);
        if (args.Cancel)
        {
            return false;
        }

        return Close(result);
    }

    private void OnPrimaryButtonClick(object sender, RoutedEventArgs e)
    {
        _ = RequestPrimaryButtonClick();
    }

    private void OnSecondaryButtonClick(object sender, RoutedEventArgs e)
    {
        _ = RequestSecondaryButtonClick();
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        _ = RequestCloseButtonClick();
    }

    private void OnKeyDownHandler(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen)
        {
            _ = RequestCancelButtonClick();
            e.Handled = true;
        }
    }

    private static void OnDialogPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTaskDialog dialog)
        {
            dialog.UpdateButtonState();
            dialog.InvalidateMeasure();
            dialog.InvalidateVisual();
        }
    }

    private static void OnButtonCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FWTaskDialog dialog)
        {
            return;
        }

        if (e.OldValue is ICommand oldCommand)
        {
            oldCommand.CanExecuteChanged -= dialog.OnButtonCanExecuteChanged;
        }

        if (e.NewValue is ICommand newCommand)
        {
            newCommand.CanExecuteChanged += dialog.OnButtonCanExecuteChanged;
        }

        dialog.UpdateButtonState();
    }

    private static void OnButtonCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTaskDialog dialog)
        {
            dialog.UpdateButtonState();
        }
    }

    private void OnButtonCanExecuteChanged(object? sender, EventArgs e)
    {
        UpdateButtonState();
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FWTaskDialog dialog)
        {
            return;
        }

        OnDialogPropertyChanged(d, e);

        if (e.NewValue is true)
        {
            dialog.Opening?.Invoke(dialog, EventArgs.Empty);
            dialog.Opened?.Invoke(dialog, EventArgs.Empty);
            dialog.FocusDefaultButton();
        }
        else if (e.OldValue is true)
        {
            dialog.Closed?.Invoke(dialog, new FWTaskDialogClosedEventArgs(dialog.Result));
            dialog.CompleteShowTask(dialog.Result);
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTaskDialog dialog && e.NewValue is FWDisclosureDensity density)
        {
            ApplyDensity(dialog, density);
        }
    }

    private static void ApplyDensity(FWTaskDialog dialog, FWDisclosureDensity density)
    {
        var (padding, minWidth, maxWidth) = FWContentDialog.GetDialogMetrics(density);
        dialog.Padding = padding;
        dialog.MinWidth = minWidth;
        dialog.MaxWidth = maxWidth;
    }

    private static bool IsValidButton(object? value)
    {
        return value is FWTaskDialogButton button && Enum.IsDefined(button);
    }

    private static bool IsValidResult(object? value)
    {
        return value is FWTaskDialogResult result && Enum.IsDefined(result);
    }

    private EventHandler<FWTaskDialogButtonClickEventArgs>? HandlerFor(FWTaskDialogButton button)
    {
        return button switch
        {
            FWTaskDialogButton.Primary => PrimaryButtonClick,
            FWTaskDialogButton.Secondary => SecondaryButtonClick,
            FWTaskDialogButton.Close => CloseButtonClick,
            _ => null
        };
    }

    private ICommand? CommandFor(FWTaskDialogButton button)
    {
        return button switch
        {
            FWTaskDialogButton.Primary => PrimaryButtonCommand,
            FWTaskDialogButton.Secondary => SecondaryButtonCommand,
            FWTaskDialogButton.Close => CloseButtonCommand,
            _ => null
        };
    }

    private object? CommandParameterFor(FWTaskDialogButton button)
    {
        return button switch
        {
            FWTaskDialogButton.Primary => PrimaryButtonCommandParameter,
            FWTaskDialogButton.Secondary => SecondaryButtonCommandParameter,
            FWTaskDialogButton.Close => CloseButtonCommandParameter,
            _ => null
        };
    }

    private bool CanExecuteButtonCommand(FWTaskDialogButton button)
    {
        var command = CommandFor(button);
        var parameter = CommandParameterFor(button);
        if (command == null)
        {
            return true;
        }

        return command.CanExecute(parameter);
    }

    private bool ExecuteButtonCommand(FWTaskDialogButton button)
    {
        var command = CommandFor(button);
        var parameter = CommandParameterFor(button);
        if (command == null)
        {
            return false;
        }

        if (!command.CanExecute(parameter))
        {
            return false;
        }

        command.Execute(parameter);
        return true;
    }

    private void UpdateButtonState()
    {
        UpdateButtonState(_primaryButton, PrimaryButtonText, FWTaskDialogButton.Primary);
        UpdateButtonState(_secondaryButton, SecondaryButtonText, FWTaskDialogButton.Secondary);
        UpdateButtonState(_closeButton, CloseButtonText, FWTaskDialogButton.Close);
    }

    private void UpdateButtonState(Button? button, string text, FWTaskDialogButton taskDialogButton)
    {
        if (button == null)
        {
            return;
        }

        button.Visibility = string.IsNullOrWhiteSpace(text) ? Visibility.Collapsed : Visibility.Visible;
        button.IsEnabled = CanExecuteButtonCommand(taskDialogButton);
    }

    private void FocusDefaultButton()
    {
        if (!IsOpen)
        {
            return;
        }

        var button = DefaultButton switch
        {
            FWTaskDialogButton.Primary => _primaryButton,
            FWTaskDialogButton.Secondary => _secondaryButton,
            FWTaskDialogButton.Close => _closeButton,
            _ => null
        };

        if (button != null && button.Visibility == Visibility.Visible && button.IsEnabled)
        {
            button.Focus();
        }
    }

    private static FWTaskDialogResult ToResult(FWTaskDialogButton button)
    {
        return button switch
        {
            FWTaskDialogButton.Primary => FWTaskDialogResult.Primary,
            FWTaskDialogButton.Secondary => FWTaskDialogResult.Secondary,
            FWTaskDialogButton.Close => FWTaskDialogResult.Close,
            _ => FWTaskDialogResult.None
        };
    }

    private static FWTaskDialogButton ToButton(FWTaskDialogResult result)
    {
        return result switch
        {
            FWTaskDialogResult.Primary => FWTaskDialogButton.Primary,
            FWTaskDialogResult.Secondary => FWTaskDialogButton.Secondary,
            FWTaskDialogResult.Close => FWTaskDialogButton.Close,
            _ => FWTaskDialogButton.None
        };
    }

    private void CompleteShowTask(FWTaskDialogResult result)
    {
        _showCancellationRegistration.Dispose();
        _showTask?.TrySetResult(result);
        _showTask = null;
    }
}

/// <summary>
/// FluentJalium GroupBox control.
/// </summary>
public class FWGroupBox : GroupBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDisclosureDensity), typeof(FWGroupBox),
            new PropertyMetadata(FWDisclosureDensity.Comfortable, OnDensityChanged));

    public FWGroupBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDisclosureDensity Density
    {
        get => (FWDisclosureDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (Thickness Padding, double MinHeight) GetGroupBoxMetrics(FWDisclosureDensity density)
    {
        return density switch
        {
            FWDisclosureDensity.Compact => (new Thickness(10, 12, 10, 10), 48.0),
            FWDisclosureDensity.Spacious => (new Thickness(18, 20, 18, 18), 72.0),
            _ => (new Thickness(14, 16, 14, 14), 56.0)
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWGroupBox groupBox && e.NewValue is FWDisclosureDensity density)
        {
            ApplyDensity(groupBox, density);
        }
    }

    private static void ApplyDensity(FWGroupBox groupBox, FWDisclosureDensity density)
    {
        var (padding, minHeight) = GetGroupBoxMetrics(density);
        groupBox.Padding = padding;
        groupBox.MinHeight = minHeight;
    }
}
