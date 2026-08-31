using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using FluentJalium.Gallery.Shell;
using Jalium.UI;
using Jalium.UI.Controls;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// WinUI Gallery's ItemPage fixes the header at margin 36,24,36,0 and the content root at
/// padding 36,0,36,36 with the example column capped to 1028 wide.
/// </summary>
[Collection("Application")]
public sealed class GalleryHostPageLayoutTests
{
    [Fact]
    public void PageContent_ShouldFollowTheWinUiItemPageMetrics()
    {
        ResetApplicationState();
        var app = new Application();
        Jalium.UI.Controls.Themes.ThemeManager.Initialize(app);
        FluentJalium.Controls.Themes.FluentThemeManager.Apply(app);

        var owner = new Window();
        var pages = new GalleryCatalogService().CreatePages(owner, _ => { }, _ => { }, _ => { });
        var page = pages[0];

        var host = new GalleryHostPage();
        host.ApplyNavigationParameter(page);

        var elements = Walk((DependencyObject)host.Content!).ToList();

        Assert.Contains(elements.OfType<FrameworkElement>(), element => element.Margin == new Thickness(36, 24, 36, 0));
        Assert.Contains(elements.OfType<FrameworkElement>(), element => element.MaxWidth == 1028d);
        Assert.DoesNotContain(elements.OfType<Panel>(), panel => panel.Margin == new Thickness(36));

        ResetApplicationState();
    }

    private static void ResetApplicationState()
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        typeof(Application).GetField("_current", flags)?.SetValue(null, null);
        typeof(Jalium.UI.Controls.Themes.ThemeManager).GetMethod("Reset", flags)?.Invoke(null, null);
        typeof(FluentJalium.Controls.Themes.FluentThemeManager).GetMethod("Reset", flags)?.Invoke(null, null);
    }

    private static IEnumerable<DependencyObject> Walk(DependencyObject node)
    {
        yield return node;

        switch (node)
        {
            case FrameworkElement element:
                if (element is Panel panel)
                {
                    foreach (UIElement child in panel.Children)
                    {
                        foreach (var descendant in Walk(child))
                        {
                            yield return descendant;
                        }
                    }
                }
                else if (element is ContentControl contentControl && contentControl.Content is DependencyObject content)
                {
                    foreach (var descendant in Walk(content))
                    {
                        yield return descendant;
                    }
                }
                else if (element is Decorator decorator && decorator.Child is DependencyObject childElement)
                {
                    foreach (var descendant in Walk(childElement))
                    {
                        yield return descendant;
                    }
                }
                break;
        }
    }
}
