using FluentJalium.Controls;
using FluentJalium.Gallery.Pages;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Automation;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Controls;

public sealed partial class GalleryLinkCard : UserControl
{
    private GalleryEntry? _entry;

    public GalleryLinkCard()
    {
        InitializeComponent();
    }

    public GalleryLinkCard(GalleryEntry entry, Action<GalleryEntry> navigate)
        : this()
    {
        _entry = entry;
        IconPresenter.Glyph = GalleryIcon.Create(entry.Icon).Glyph;
        TitlePresenter.Text = entry.Title;
        DescriptionPresenter.Text = entry.Description;
        AutomationProperties.SetName(this, entry.Title);
        RootButton.Click += (_, _) => navigate(entry);
    }
}
