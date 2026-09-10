using FluentJalium.Controls;
using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Pages;

public sealed partial class MenuSample : UserControl
{
    public MenuSample()
    {
        InitializeComponent();

        var file = new FWMenuBarItem { Title = "File" };
        file.Items.Add(new FWMenuItem { Header = "New project" });
        file.Items.Add(new FWMenuItem { Header = "Open" });
        file.Items.Add(new FWMenuItem { Header = "Save" });

        var view = new FWMenuBarItem { Title = "View" };
        view.Items.Add(new FWMenuItem { Header = "Appearance" });
        view.Items.Add(new FWMenuItem { Header = "Command palette" });

        MenuBar.Items.Add(file);
        MenuBar.Items.Add(view);
        MenuBar.UpdateItems();
    }
}
