using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Controls;

/// <summary>
/// One gallery example card: the live sample, optional output and options panes, and a
/// collapsible source-code view. Mirrors WinUI Gallery's <c>ControlExample</c>.
/// </summary>
public sealed partial class ControlExample : UserControl
{
    private string? _xamlCode;
    private string? _cSharpCode;

    public ControlExample()
    {
        InitializeComponent();

        SourceSelector.SelectionChanged += (_, _) => ShowSelectedSource();
    }

    public string? HeaderText
    {
        get => HeaderTextPresenter.Text;
        set => HeaderTextPresenter.Text = value;
    }

    public UIElement? Example
    {
        get => ExampleHost.Child;
        set => ExampleHost.Child = value;
    }

    public object? Output
    {
        get => OutputContentHost.Content;
        set
        {
            OutputContentHost.Content = value;
            OutputHost.Visibility = value is null ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public UIElement? Options
    {
        get => OptionsHost.Child;
        set
        {
            OptionsHost.Child = value;
            OptionsHost.Visibility = value is null ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public string? XamlCode
    {
        get => _xamlCode;
        set
        {
            _xamlCode = value;
            ShowSelectedSource();
        }
    }

    public string? CSharpCode
    {
        get => _cSharpCode;
        set
        {
            _cSharpCode = value;
            ShowSelectedSource();
        }
    }

    private void ShowSelectedSource() =>
        SourceCodePresenter.Text = SourceSelector.SelectedIndex == 1 ? _cSharpCode : _xamlCode;
}
