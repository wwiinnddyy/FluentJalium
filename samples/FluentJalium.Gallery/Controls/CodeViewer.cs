using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI;

namespace FluentJalium.Gallery.Controls;

/// <summary>
/// Control for displaying code with copy functionality.
/// Note: Full syntax highlighting requires RichTextBox or custom rendering in Jalium.UI.
/// This version displays code in monospace font with copy button.
/// </summary>
public sealed class CodeViewer : Control
{
    public static readonly DependencyProperty CodeProperty = DependencyProperty.Register(
        nameof(Code),
        typeof(string),
        typeof(CodeViewer),
        new PropertyMetadata(string.Empty, OnCodeChanged));

    private ScrollViewer? _scrollViewer;
    private Button? _copyButton;

    public string Code
    {
        get => (string)GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public CodeViewer()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        var grid = new Grid();

        _scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        grid.Children.Add(_scrollViewer);

        _copyButton = new Button
        {
            Content = "📋",
            Width = 32,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 8, 8, 0),
            ToolTip = "Copy code"
        };
        _copyButton.Click += OnCopyClick;
        grid.Children.Add(_copyButton);

        UpdateHighlightedCode();
    }

    private static void OnCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CodeViewer viewer)
        {
            viewer.UpdateHighlightedCode();
        }
    }

    private void UpdateHighlightedCode()
    {
        if (_scrollViewer == null || string.IsNullOrWhiteSpace(Code))
            return;

        var textBlock = new TextBlock
        {
            Text = Code,
            FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
            FontSize = 13,
            TextWrapping = TextWrapping.NoWrap,
            Foreground = new SolidColorBrush(Color.FromRgb(212, 212, 212))
        };
        _scrollViewer.Content = textBlock;
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(Code);

            // Show feedback
            if (_copyButton != null)
            {
                var originalContent = _copyButton.Content;
                _copyButton.Content = "✓";
                _copyButton.IsEnabled = false;

                // Reset after 2 seconds
                Task.Delay(2000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        _copyButton.Content = originalContent;
                        _copyButton.IsEnabled = true;
                    });
                });
            }
        }
        catch
        {
            // Clipboard operation failed
        }
    }
}
