using System.Text;
using Jalium.UI.Controls;
using Jalium.UI.Documents;
using Jalium.UI.Media;
using Jalium.UI;

namespace FluentJalium.Gallery.Controls;

/// <summary>
/// Simple C# syntax highlighter for Gallery code samples.
/// Provides basic keyword, string, comment, and number highlighting.
/// </summary>
public static class SimpleCSharpHighlighter
{
    private static readonly string[] Keywords = new[]
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this",
        "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
        "using", "var", "virtual", "void", "volatile", "while"
    };

    private static readonly string[] Types = new[]
    {
        "String", "Int32", "Double", "Boolean", "Object", "UIElement", "Control", "Button",
        "TextBlock", "StackPanel", "Grid", "Border", "ContentControl"
    };

    /// <summary>
    /// Highlights C# code and returns a TextBlock with colored inlines.
    /// </summary>
    public static TextBlock Highlight(string code)
    {
        var textBlock = new TextBlock
        {
            FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
            FontSize = 13,
            TextWrapping = TextWrapping.NoWrap,
            Foreground = new SolidColorBrush(Color.FromRgb(212, 212, 212)) // #D4D4D4
        };

        var lines = code.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                textBlock.Inlines.Add(new LineBreak());
            }

            HighlightLine(textBlock.Inlines, lines[i]);
        }

        return textBlock;
    }

    private static void HighlightLine(InlineCollection inlines, string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return;
        }

        // Check for single-line comment
        var commentIndex = line.IndexOf("//");
        if (commentIndex >= 0)
        {
            if (commentIndex > 0)
            {
                HighlightCode(inlines, line.Substring(0, commentIndex));
            }
            AddColoredText(inlines, line.Substring(commentIndex), Color.FromRgb(106, 153, 85)); // #6A9955 Green
            return;
        }

        HighlightCode(inlines, line);
    }

    private static void HighlightCode(InlineCollection inlines, string code)
    {
        var tokens = TokenizeCode(code);
        foreach (var (text, type) in tokens)
        {
            switch (type)
            {
                case TokenType.Keyword:
                    AddColoredText(inlines, text, Color.FromRgb(86, 156, 214)); // #569CD6 Blue
                    break;
                case TokenType.Type:
                    AddColoredText(inlines, text, Color.FromRgb(78, 201, 176)); // #4EC9B0 Cyan
                    break;
                case TokenType.String:
                    AddColoredText(inlines, text, Color.FromRgb(206, 145, 120)); // #CE9178 Orange
                    break;
                case TokenType.Number:
                    AddColoredText(inlines, text, Color.FromRgb(181, 206, 168)); // #B5CEA8 Light Green
                    break;
                default:
                    AddColoredText(inlines, text, Color.FromRgb(212, 212, 212)); // #D4D4D4 Default
                    break;
            }
        }
    }

    private static List<(string text, TokenType type)> TokenizeCode(string code)
    {
        var tokens = new List<(string, TokenType)>();
        var current = new StringBuilder();
        var inString = false;
        char stringChar = '\0';

        for (int i = 0; i < code.Length; i++)
        {
            char c = code[i];

            // Handle strings
            if (inString)
            {
                current.Append(c);
                if (c == stringChar && (i == 0 || code[i - 1] != '\\'))
                {
                    tokens.Add((current.ToString(), TokenType.String));
                    current.Clear();
                    inString = false;
                }
                continue;
            }

            if (c == '"' || c == '\'')
            {
                if (current.Length > 0)
                {
                    ClassifyAndAdd(tokens, current.ToString());
                    current.Clear();
                }
                inString = true;
                stringChar = c;
                current.Append(c);
                continue;
            }

            // Handle word boundaries
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                current.Append(c);
            }
            else
            {
                if (current.Length > 0)
                {
                    ClassifyAndAdd(tokens, current.ToString());
                    current.Clear();
                }
                tokens.Add((c.ToString(), TokenType.Default));
            }
        }

        if (current.Length > 0)
        {
            if (inString)
            {
                tokens.Add((current.ToString(), TokenType.String));
            }
            else
            {
                ClassifyAndAdd(tokens, current.ToString());
            }
        }

        return tokens;
    }

    private static void ClassifyAndAdd(List<(string, TokenType)> tokens, string text)
    {
        if (Keywords.Contains(text))
        {
            tokens.Add((text, TokenType.Keyword));
        }
        else if (Types.Contains(text))
        {
            tokens.Add((text, TokenType.Type));
        }
        else if (double.TryParse(text, out _))
        {
            tokens.Add((text, TokenType.Number));
        }
        else
        {
            tokens.Add((text, TokenType.Default));
        }
    }

    private static void AddColoredText(InlineCollection inlines, string text, Color color)
    {
        inlines.Add(new Run
        {
            Text = text,
            Foreground = new SolidColorBrush(color)
        });
    }

    private enum TokenType
    {
        Default,
        Keyword,
        Type,
        String,
        Number
    }
}

/// <summary>
/// Control for displaying syntax-highlighted code with copy functionality.
/// </summary>
public sealed class CodeViewer : Control
{
    public static readonly DependencyProperty CodeProperty = DependencyProperty.Register(
        nameof(Code),
        typeof(string),
        typeof(CodeViewer),
        new PropertyMetadata(string.Empty, OnCodeChanged));

    public static readonly DependencyProperty ShowLineNumbersProperty = DependencyProperty.Register(
        nameof(ShowLineNumbers),
        typeof(bool),
        typeof(CodeViewer),
        new PropertyMetadata(false));

    private ScrollViewer? _scrollViewer;
    private Button? _copyButton;

    public string Code
    {
        get => (string)GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public bool ShowLineNumbers
    {
        get => (bool)GetValue(ShowLineNumbersProperty);
        set => SetValue(ShowLineNumbersProperty, value);
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

        Content = grid;
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

        var highlightedText = SimpleCSharpHighlighter.Highlight(Code);
        _scrollViewer.Content = highlightedText;
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
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };
                timer.Tick += (s, args) =>
                {
                    _copyButton.Content = originalContent;
                    _copyButton.IsEnabled = true;
                    timer.Stop();
                };
                timer.Start();
            }
        }
        catch
        {
            // Clipboard operation failed
        }
    }
}
