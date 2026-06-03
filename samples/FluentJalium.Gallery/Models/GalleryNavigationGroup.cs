using FluentJalium.Icon;

namespace FluentJalium.Gallery.Models;

internal static class GalleryNavigationGroup
{
    public const string Home = "Home";
    public const string ControlSurfaces = "Control surfaces";
    public const string Input = "Input";
    public const string LayoutAndMedia = "Layout and media";
    public const string CollectionsAndData = "Collections and data";
    public const string MaterialsAndMotion = "Materials and motion";
    public const string AppStructure = "App structure";
    public const string Diagnostics = "Diagnostics";

    public static readonly string[] Order =
    [
        ControlSurfaces,
        Input,
        LayoutAndMedia,
        CollectionsAndData,
        MaterialsAndMotion,
        AppStructure
    ];

    public static FluentIconRegular GetIcon(string groupName)
    {
        return groupName switch
        {
            ControlSurfaces => FluentIconRegular.ControlButton24,
            Input => FluentIconRegular.Textbox24,
            LayoutAndMedia => FluentIconRegular.LayoutColumnTwo24,
            CollectionsAndData => FluentIconRegular.Table24,
            MaterialsAndMotion => FluentIconRegular.SlideTransition24,
            AppStructure => FluentIconRegular.Navigation24,
            Diagnostics => FluentIconRegular.DataUsage24,
            _ => FluentIconRegular.Home24
        };
    }
}
