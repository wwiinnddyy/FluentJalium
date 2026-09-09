using FluentJalium.Controls;
using Jalium.UI;

namespace FluentJalium.Gallery.Controls;

/// <summary>Shared Jalxaml page frame with Fluent typography, spacing and scrolling.</summary>
public sealed partial class GalleryPageFrame : FWScrollViewer
{
    public GalleryPageFrame()
    {
        InitializeComponent();
    }

    public GalleryPageFrame(string title, string description)
        : this()
    {
        TitlePresenter.Text = title;
        DescriptionPresenter.Text = description;
    }

    public void Add(UIElement content) => Body.Children.Add(content);
}
