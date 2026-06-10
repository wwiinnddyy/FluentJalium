using Jalium.UI.Controls;
using FWSnackbarHost = FluentJalium.Controls.FWSnackbarHost;
using ToastSeverity = Jalium.UI.Controls.ToastSeverity;

namespace FluentJalium.Gallery.Controls;

internal static class GalleryFeedback
{
    private static FWSnackbarHost? _host;

    public static void SetHost(FWSnackbarHost host)
    {
        _host = host;
    }

    public static void Copied(string what)
    {
        _host?.Show(ToastSeverity.Success, $"Copied: {what}");
    }

    public static void Info(string message)
    {
        _host?.Show(ToastSeverity.Information, message);
    }
}
