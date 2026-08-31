using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Shell;

/// <summary>
/// Frame host for the WinUI item layout.
/// </summary>
/// <remarks>
/// The host stays in C# on purpose. For a XAML-authored page the source generator registers the
/// <c>.jalxaml</c> path as a startup URI as well as emitting <c>InitializeComponent</c>, and
/// <c>FWFrame</c> hosting applies both, so the whole page renders twice. A
/// <see cref="UserControl"/> is applied once, which is why the layout lives in
/// <see cref="GalleryItemView"/>.
/// </remarks>
public sealed class GalleryItemHostPage : Page
{
    private readonly GalleryItemView _view = new();

    public GalleryItemHostPage()
    {
        Content = _view;
    }

    public void ApplyNavigationParameter(object? parameter) => _view.ApplyNavigationParameter(parameter);
}
