using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;

namespace FluentJalium.Gallery.Controls;

/// <summary>
/// Home page quick-link tile, sized and spaced like WinUI Gallery's <c>Tile</c>.
/// </summary>
public sealed partial class GalleryTile : UserControl
{
    public GalleryTile()
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

    public string Caption
    {
        get => CaptionPresenter.Text;
        set => CaptionPresenter.Text = value;
    }

    public string Glyph
    {
        get => GlyphPresenter.Text;
        set => GlyphPresenter.Text = value;
    }

    public void Invoke() => Root.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
}
