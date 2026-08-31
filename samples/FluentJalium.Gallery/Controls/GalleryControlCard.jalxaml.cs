using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;

namespace FluentJalium.Gallery.Controls;

/// <summary>
/// Catalog entry card, sized and coloured like WinUI Gallery's <c>ControlItemTemplate</c>.
/// </summary>
public sealed partial class GalleryControlCard : UserControl
{
    public GalleryControlCard()
    {
        InitializeComponent();
    }

    public event RoutedEventHandler? Activated
    {
        add => Root.Click += value;
        remove => Root.Click -= value;
    }

    public string Title
    {
        get => TitlePresenter.Text;
        set => TitlePresenter.Text = value;
    }

    public string Subtitle
    {
        get => SubtitlePresenter.Text;
        set => SubtitlePresenter.Text = value;
    }

    public string Glyph
    {
        get => GlyphPresenter.Text;
        set => GlyphPresenter.Text = value;
    }

    public void Invoke() => Root.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
}
