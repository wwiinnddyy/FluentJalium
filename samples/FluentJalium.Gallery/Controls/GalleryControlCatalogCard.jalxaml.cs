using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Controls;

public sealed partial class GalleryControlCatalogCard : UserControl
{
    public GalleryControlCatalogCard()
    {
        InitializeComponent();
    }

    public GalleryControlCatalogCard(Type controlType, string category)
        : this()
    {
        NamePresenter.Text = controlType.Name;
        BasePresenter.Text = controlType.BaseType?.Name ?? "Control";
        CategoryPresenter.Text = category;
    }
}
