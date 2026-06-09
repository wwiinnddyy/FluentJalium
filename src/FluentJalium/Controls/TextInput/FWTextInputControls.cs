using System.Diagnostics.CodeAnalysis;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium text input density presets.
/// </summary>
public enum FWTextInputDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium numeric input density presets.
/// </summary>
public enum FWNumberBoxDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium TextBox control.
/// </summary>
public class FWTextBox : TextBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWTextInputDensity), typeof(FWTextBox),
            new PropertyMetadata(FWTextInputDensity.Comfortable, OnDensityChanged));

    public FWTextBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTextInputDensity Density
    {
        get => (FWTextInputDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWTextInputDensity density)
    {
        return density switch
        {
            FWTextInputDensity.Compact => (30.0, new Thickness(8, 4, 8, 5)),
            FWTextInputDensity.Spacious => (40.0, new Thickness(12, 8, 12, 8)),
            _ => (34.0, new Thickness(10, 6, 10, 6))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTextBox textBox && e.NewValue is FWTextInputDensity density)
        {
            ApplyDensity(textBox, density);
        }
    }

    private static void ApplyDensity(FWTextBox textBox, FWTextInputDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        textBox.MinHeight = minHeight;
        textBox.Padding = padding;
    }
}

/// <summary>
/// FluentJalium PasswordBox control.
/// </summary>
public class FWPasswordBox : PasswordBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWTextInputDensity), typeof(FWPasswordBox),
            new PropertyMetadata(FWTextInputDensity.Comfortable, OnDensityChanged));

    public FWPasswordBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTextInputDensity Density
    {
        get => (FWTextInputDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPasswordBox passwordBox && e.NewValue is FWTextInputDensity density)
        {
            ApplyDensity(passwordBox, density);
        }
    }

    private static void ApplyDensity(FWPasswordBox passwordBox, FWTextInputDensity density)
    {
        var (minHeight, padding) = FWTextBox.GetDensityMetrics(density);
        passwordBox.MinHeight = minHeight;
        passwordBox.Padding = padding;
    }
}

/// <summary>
/// FluentJalium NumberBox control.
/// </summary>
public class FWNumberBox : NumberBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNumberBoxDensity), typeof(FWNumberBox),
            new PropertyMetadata(FWNumberBoxDensity.Comfortable, OnDensityChanged));

    public FWNumberBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNumberBoxDensity Density
    {
        get => (FWNumberBoxDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWNumberBox numberBox && e.NewValue is FWNumberBoxDensity density)
        {
            ApplyDensity(numberBox, density);
        }
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWNumberBoxDensity density)
    {
        return density switch
        {
            FWNumberBoxDensity.Compact => (30.0, new Thickness(8, 4, 8, 5)),
            FWNumberBoxDensity.Spacious => (40.0, new Thickness(12, 8, 12, 8)),
            _ => (34.0, new Thickness(10, 6, 10, 6))
        };
    }

    private static void ApplyDensity(FWNumberBox numberBox, FWNumberBoxDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        numberBox.MinHeight = minHeight;
        numberBox.Padding = padding;
    }
}

/// <summary>
/// FluentJalium AutoCompleteBox control.
/// </summary>
public class FWAutoCompleteBox : AutoCompleteBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWTextInputDensity), typeof(FWAutoCompleteBox),
            new PropertyMetadata(FWTextInputDensity.Comfortable, OnDensityChanged));

    public FWAutoCompleteBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTextInputDensity Density
    {
        get => (FWTextInputDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Padding, double MaxDropDownHeight) GetDensityMetrics(FWTextInputDensity density)
    {
        var (minHeight, padding) = FWTextBox.GetDensityMetrics(density);
        var maxDropDownHeight = density switch
        {
            FWTextInputDensity.Compact => 192.0,
            FWTextInputDensity.Spacious => 288.0,
            _ => 224.0
        };

        return (minHeight, padding, maxDropDownHeight);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAutoCompleteBox autoCompleteBox && e.NewValue is FWTextInputDensity density)
        {
            ApplyDensity(autoCompleteBox, density);
        }
    }

    private static void ApplyDensity(FWAutoCompleteBox autoCompleteBox, FWTextInputDensity density)
    {
        var (minHeight, padding, maxDropDownHeight) = GetDensityMetrics(density);
        autoCompleteBox.MinHeight = minHeight;
        autoCompleteBox.Padding = padding;
        autoCompleteBox.MaxDropDownHeight = maxDropDownHeight;
    }
}

/// <summary>
/// FluentJalium AutoSuggestBox control.
/// </summary>
public class FWAutoSuggestBox : AutoCompleteBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWTextInputDensity), typeof(FWAutoSuggestBox),
            new PropertyMetadata(FWTextInputDensity.Comfortable, OnDensityChanged));

    public static readonly DependencyProperty LastTextChangeReasonProperty =
        DependencyProperty.Register(nameof(LastTextChangeReason), typeof(FWAutoSuggestBoxTextChangeReason), typeof(FWAutoSuggestBox),
            new PropertyMetadata(FWAutoSuggestBoxTextChangeReason.ProgrammaticChange));

    public static readonly RoutedEvent QuerySubmittedEvent =
        EventManager.RegisterRoutedEvent(nameof(QuerySubmitted), RoutingStrategy.Bubble,
            typeof(EventHandler<FWAutoSuggestBoxQuerySubmittedEventArgs>), typeof(FWAutoSuggestBox));

    public static readonly RoutedEvent SuggestionChosenEvent =
        EventManager.RegisterRoutedEvent(nameof(SuggestionChosen), RoutingStrategy.Bubble,
            typeof(EventHandler<FWAutoSuggestBoxSuggestionChosenEventArgs>), typeof(FWAutoSuggestBox));

    public static readonly RoutedEvent AutoSuggestTextChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(AutoSuggestTextChanged), RoutingStrategy.Bubble,
            typeof(EventHandler<FWAutoSuggestBoxTextChangedEventArgs>), typeof(FWAutoSuggestBox));

    private FWAutoSuggestBoxTextChangeReason? _pendingTextChangeReason;

    public FWAutoSuggestBox()
    {
        ApplyDensity(this, Density);
        TextChanged += OnAutoCompleteTextChanged;
        SelectionChanged += OnAutoCompleteSelectionChanged;
    }

    public event EventHandler<FWAutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted
    {
        add => AddHandler(QuerySubmittedEvent, value);
        remove => RemoveHandler(QuerySubmittedEvent, value);
    }

    public event EventHandler<FWAutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen
    {
        add => AddHandler(SuggestionChosenEvent, value);
        remove => RemoveHandler(SuggestionChosenEvent, value);
    }

    public event EventHandler<FWAutoSuggestBoxTextChangedEventArgs> AutoSuggestTextChanged
    {
        add => AddHandler(AutoSuggestTextChangedEvent, value);
        remove => RemoveHandler(AutoSuggestTextChangedEvent, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTextInputDensity Density
    {
        get => (FWTextInputDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public FWAutoSuggestBoxTextChangeReason LastTextChangeReason
    {
        get => (FWAutoSuggestBoxTextChangeReason)GetValue(LastTextChangeReasonProperty)!;
        private set => SetValue(LastTextChangeReasonProperty, value);
    }

    public void SetQueryText(string? text, FWAutoSuggestBoxTextChangeReason reason = FWAutoSuggestBoxTextChangeReason.ProgrammaticChange)
    {
        LastTextChangeReason = reason;
        _pendingTextChangeReason = reason;
        try
        {
            Text = text ?? string.Empty;
        }
        finally
        {
            if (_pendingTextChangeReason == reason)
            {
                _pendingTextChangeReason = null;
            }
        }
    }

    public bool RequestSuggestionChosen(object? suggestion)
    {
        if (suggestion == null)
        {
            return false;
        }

        SelectedItem = suggestion;
        SetQueryText(ResolveSuggestionText(suggestion), FWAutoSuggestBoxTextChangeReason.SuggestionChosen);
        IsDropDownOpen = false;

        RaiseSuggestionChosen(suggestion);
        return true;
    }

    public FWAutoSuggestBoxQuerySubmittedEventArgs RequestQuerySubmitted()
    {
        return RequestQuerySubmitted(SelectedItem);
    }

    public FWAutoSuggestBoxQuerySubmittedEventArgs RequestQuerySubmitted(object? chosenSuggestion)
    {
        var args = new FWAutoSuggestBoxQuerySubmittedEventArgs(QuerySubmittedEvent, this, Text, chosenSuggestion);
        RaiseEvent(args);
        return args;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var submitQuery = e.Key == Key.Enter;

        base.OnKeyDown(e);

        if (submitQuery)
        {
            RequestQuerySubmitted(SelectedItem);
        }
    }

    protected override void InsertText(string textToInsert)
    {
        var previousReason = _pendingTextChangeReason;
        _pendingTextChangeReason = FWAutoSuggestBoxTextChangeReason.UserInput;

        try
        {
            base.InsertText(textToInsert);
        }
        finally
        {
            if (_pendingTextChangeReason == FWAutoSuggestBoxTextChangeReason.UserInput)
            {
                _pendingTextChangeReason = previousReason;
            }
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAutoSuggestBox autoSuggestBox && e.NewValue is FWTextInputDensity density)
        {
            ApplyDensity(autoSuggestBox, density);
        }
    }

    private static void ApplyDensity(FWAutoSuggestBox autoSuggestBox, FWTextInputDensity density)
    {
        var (minHeight, padding, maxDropDownHeight) = FWAutoCompleteBox.GetDensityMetrics(density);
        autoSuggestBox.MinHeight = minHeight;
        autoSuggestBox.Padding = padding;
        autoSuggestBox.MaxDropDownHeight = maxDropDownHeight;
    }

    private void OnAutoCompleteSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedItem = SelectedItem ?? e.AddedItems.FirstOrDefault();
        if (selectedItem == null)
        {
            return;
        }

        RaiseAutoSuggestTextChanged(FWAutoSuggestBoxTextChangeReason.SuggestionChosen);
        RaiseSuggestionChosen(selectedItem);
    }

    private void OnAutoCompleteTextChanged(object? sender, RoutedEventArgs e)
    {
        RaiseAutoSuggestTextChanged(_pendingTextChangeReason ?? FWAutoSuggestBoxTextChangeReason.ProgrammaticChange);
        _pendingTextChangeReason = null;
    }

    private void RaiseAutoSuggestTextChanged(FWAutoSuggestBoxTextChangeReason reason)
    {
        LastTextChangeReason = reason;
        RaiseEvent(new FWAutoSuggestBoxTextChangedEventArgs(AutoSuggestTextChangedEvent, this, Text, reason));
    }

    private void RaiseSuggestionChosen(object selectedItem)
    {
        RaiseEvent(new FWAutoSuggestBoxSuggestionChosenEventArgs(SuggestionChosenEvent, this, selectedItem));
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075:UnrecognizedReflectionPattern",
        Justification = "TextMemberPath mirrors AutoCompleteBox display-path behavior. Consumers that set TextMemberPath are responsible for preserving the referenced property under trimming/AOT.")]
    private string ResolveSuggestionText(object suggestion)
    {
        if (string.IsNullOrWhiteSpace(TextMemberPath))
        {
            return suggestion.ToString() ?? string.Empty;
        }

        var property = suggestion.GetType().GetProperty(TextMemberPath);
        return property?.GetValue(suggestion)?.ToString()
            ?? suggestion.ToString()
            ?? string.Empty;
    }
}

public enum FWAutoSuggestBoxTextChangeReason
{
    ProgrammaticChange,
    UserInput,
    SuggestionChosen
}

public sealed class FWAutoSuggestBoxQuerySubmittedEventArgs : RoutedEventArgs
{
    public FWAutoSuggestBoxQuerySubmittedEventArgs(RoutedEvent routedEvent, object source, string queryText, object? chosenSuggestion)
        : base(routedEvent, source)
    {
        QueryText = queryText;
        ChosenSuggestion = chosenSuggestion;
    }

    public string QueryText { get; }
    public object? ChosenSuggestion { get; }
}

public sealed class FWAutoSuggestBoxTextChangedEventArgs : RoutedEventArgs
{
    public FWAutoSuggestBoxTextChangedEventArgs(
        RoutedEvent routedEvent,
        object source,
        string text,
        FWAutoSuggestBoxTextChangeReason reason)
        : base(routedEvent, source)
    {
        Text = text;
        Reason = reason;
    }

    public string Text { get; }
    public FWAutoSuggestBoxTextChangeReason Reason { get; }
}

public sealed class FWAutoSuggestBoxSuggestionChosenEventArgs : RoutedEventArgs
{
    public FWAutoSuggestBoxSuggestionChosenEventArgs(RoutedEvent routedEvent, object source, object selectedItem)
        : base(routedEvent, source)
    {
        SelectedItem = selectedItem;
    }

    public object SelectedItem { get; }
}

/// <summary>
/// FluentJalium RichTextBox control.
/// </summary>
public class FWRichTextBox : RichTextBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWTextInputDensity), typeof(FWRichTextBox),
            new PropertyMetadata(FWTextInputDensity.Comfortable, OnDensityChanged));

    public FWRichTextBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTextInputDensity Density
    {
        get => (FWTextInputDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWTextInputDensity density)
    {
        return density switch
        {
            FWTextInputDensity.Compact => (80.0, new Thickness(8)),
            FWTextInputDensity.Spacious => (128.0, new Thickness(12)),
            _ => (96.0, new Thickness(10))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRichTextBox richTextBox && e.NewValue is FWTextInputDensity density)
        {
            ApplyDensity(richTextBox, density);
        }
    }

    private static void ApplyDensity(FWRichTextBox richTextBox, FWTextInputDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        richTextBox.MinHeight = minHeight;
        richTextBox.Padding = padding;
    }
}
