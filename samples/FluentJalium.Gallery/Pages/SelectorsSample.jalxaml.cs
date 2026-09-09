using FluentJalium.Controls;
using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Pages;

public sealed partial class SelectorsSample : UserControl
{
    public SelectorsSample()
    {
        InitializeComponent();

        var buttons = new FWTreeSelectorItem { Header = "Buttons", IsExpanded = true };
        buttons.Items.Add(new FWTreeSelectorItem { Header = "Button" });
        buttons.Items.Add(new FWTreeSelectorItem { Header = "SplitButton" });
        var navigation = new FWTreeSelectorItem { Header = "Navigation", IsExpanded = true };
        navigation.Items.Add(new FWTreeSelectorItem { Header = "TabView" });
        navigation.Items.Add(new FWTreeSelectorItem { Header = "BreadcrumbBar" });
        TreeSelector.Items.Add(buttons);
        TreeSelector.Items.Add(navigation);
        PropertyGrid.SelectedObject = new SelectorProfile();
    }

    private sealed class SelectorProfile
    {
        public string Density { get; set; } = "Comfortable";
        public bool ShowFocus { get; set; } = true;
        public string Theme { get; set; } = "System";
    }
}
