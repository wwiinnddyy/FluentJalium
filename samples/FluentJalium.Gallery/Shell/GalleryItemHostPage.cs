using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Shell;

/// <summary>
/// Frame host for the WinUI item layout. Defers Content assignment to navigation time,
/// mirroring <see cref="GalleryHostPage"/>'s lazy pattern.
/// </summary>
public sealed class GalleryItemHostPage : Page
{
    private readonly GalleryItemView _view = new();

    public void ApplyNavigationParameter(object? parameter)
    {
        Content = _view;
        _view.ApplyNavigationParameter(parameter);
    }
}
