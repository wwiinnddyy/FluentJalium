using FluentJalium.Gallery.Models;
using Jalium.UI;
using Jalium.UI.Controls;
using FWInfoBar = FluentJalium.Controls.FWInfoBar;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;

namespace FluentJalium.Gallery.Pages;

/// <summary>
/// Preview shell for catalog pages that mirror WinUI Gallery structure but have no
/// FW sample coverage yet (Fundamentals, Accessibility, Navigation). Phase 6 replaces
/// each shell with migrated samples.
/// </summary>
internal static class GalleryPlaceholderPage
{
    public static UIElement Create(GalleryPageInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Children =
            {
                new FWInfoBar
                {
                    Title = info.Title,
                    Message = info.Description,
                    Severity = InfoBarSeverity.Informational,
                    IsOpen = true,
                    IsClosable = false
                },
                new FWTextBlock
                {
                    Text = info.Description,
                    TextWrapping = TextWrapping.Wrap
                }
            }
        };
    }
}
