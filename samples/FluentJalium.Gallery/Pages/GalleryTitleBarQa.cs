using FluentJalium.Controls;

namespace FluentJalium.Gallery.Pages;

internal readonly record struct GalleryTitleBarVisualQaSnapshot(
    string Title,
    bool IsShowIcon,
    bool IsShowTitle,
    bool IsMaximized,
    bool ShowMinimizeButton,
    bool ShowMaximizeButton,
    bool ShowCloseButton,
    bool HasLeftWindowCommands,
    bool HasRightWindowCommands,
    double Width,
    double Height);

internal static class GalleryTitleBarQa
{
    public static GalleryTitleBarVisualQaSnapshot CreateTitleBarVisualQaSnapshot(FWTitleBar titleBar)
    {
        ArgumentNullException.ThrowIfNull(titleBar);

        return new GalleryTitleBarVisualQaSnapshot(
            titleBar.Title ?? string.Empty,
            titleBar.IsShowIcon,
            titleBar.IsShowTitle,
            titleBar.IsMaximized,
            titleBar.ShowMinimizeButton,
            titleBar.ShowMaximizeButton,
            titleBar.ShowCloseButton,
            titleBar.LeftWindowCommands != null,
            titleBar.RightWindowCommands != null,
            titleBar.Width,
            titleBar.Height);
    }

    public static string FormatTitleBarVisualQa(string action, GalleryTitleBarVisualQaSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(action);

        return $"{action}. TitleBar QA: title {snapshot.Title}. Icon {FormatOnOff(snapshot.IsShowIcon)}. Title text {FormatOnOff(snapshot.IsShowTitle)}. Maximized {FormatOnOff(snapshot.IsMaximized)}. Buttons min/max/close {FormatOnOff(snapshot.ShowMinimizeButton)}/{FormatOnOff(snapshot.ShowMaximizeButton)}/{FormatOnOff(snapshot.ShowCloseButton)}. Commands left/right {FormatOnOff(snapshot.HasLeftWindowCommands)}/{FormatOnOff(snapshot.HasRightWindowCommands)}. Size {snapshot.Width:0}x{snapshot.Height:0}.";
    }

    private static string FormatOnOff(bool value) => value ? "on" : "off";
}
