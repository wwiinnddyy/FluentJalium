using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Gallery.Pages;
using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentShellControlsTests
{
    [Fact]
    public void FWTitleBar_ShouldTrackWindowChromeStateAndRaiseCommands()
    {
        var leftCommands = new FWStackPanel();
        var rightCommands = new FWStackPanel();
        var titleBar = new FWTitleBar
        {
            Title = "FluentJalium",
            IsMaximized = true,
            ShowMinimizeButton = false,
            ShowMaximizeButton = true,
            ShowCloseButton = true,
            IsShowIcon = false,
            IsShowTitle = true,
            LeftWindowCommands = leftCommands,
            RightWindowCommands = rightCommands,
            Height = 34,
            FontSize = 13
        };
        var minimized = 0;
        var maximizeRestored = 0;
        var closed = 0;

        titleBar.MinimizeClicked += (_, _) => minimized++;
        titleBar.MaximizeRestoreClicked += (_, _) => maximizeRestored++;
        titleBar.CloseClicked += (_, _) => closed++;

        InvokeTitleBarCommand(titleBar, "RaiseMinimizeClicked");
        InvokeTitleBarCommand(titleBar, "RaiseMaximizeRestoreClicked");
        InvokeTitleBarCommand(titleBar, "RaiseCloseClicked");

        Assert.Equal("FluentJalium", titleBar.Title);
        Assert.True(titleBar.IsMaximized);
        Assert.False(titleBar.ShowMinimizeButton);
        Assert.True(titleBar.ShowMaximizeButton);
        Assert.True(titleBar.ShowCloseButton);
        Assert.False(titleBar.IsShowIcon);
        Assert.True(titleBar.IsShowTitle);
        Assert.Same(leftCommands, titleBar.LeftWindowCommands);
        Assert.Same(rightCommands, titleBar.RightWindowCommands);
        Assert.Equal(34, titleBar.Height);
        Assert.Equal(13, titleBar.FontSize);
        Assert.Equal(1, minimized);
        Assert.Equal(1, maximizeRestored);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWTitleBarButton_ShouldExposeKindGlyphAndWindowButtonSizing()
    {
        var button = new FWTitleBarButton
        {
            Kind = TitleBarButtonKind.Restore,
            GlyphSize = 11,
            Width = 48,
            Height = 34
        };

        button.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        Assert.Equal(TitleBarButtonKind.Restore, button.Kind);
        Assert.Equal(11, button.GlyphSize);
        Assert.Equal(48, button.Width);
        Assert.Equal(34, button.Height);
        Assert.Equal(48, button.DesiredSize.Width);
        Assert.Equal(34, button.DesiredSize.Height);
        Assert.False(button.Focusable);
    }

    [Fact]
    public void GalleryNavigationPage_ShouldFormatTitleBarVisualQaSnapshot()
    {
        var leftCommands = new FWStackPanel();
        var rightCommands = new FWStackPanel();
        var titleBar = new FWTitleBar
        {
            Width = 500,
            Height = 36,
            Title = "FluentJalium Gallery",
            IsShowIcon = false,
            IsShowTitle = true,
            IsMaximized = true,
            ShowMinimizeButton = true,
            ShowMaximizeButton = false,
            ShowCloseButton = true,
            LeftWindowCommands = leftCommands,
            RightWindowCommands = rightCommands
        };

        var snapshot = GalleryNavigationPage.CreateTitleBarVisualQaSnapshot(titleBar);
        var text = GalleryNavigationPage.FormatTitleBarVisualQa("TitleBar preview ready", snapshot);

        Assert.Equal("FluentJalium Gallery", snapshot.Title);
        Assert.False(snapshot.IsShowIcon);
        Assert.True(snapshot.IsShowTitle);
        Assert.True(snapshot.IsMaximized);
        Assert.True(snapshot.ShowMinimizeButton);
        Assert.False(snapshot.ShowMaximizeButton);
        Assert.True(snapshot.ShowCloseButton);
        Assert.True(snapshot.HasLeftWindowCommands);
        Assert.True(snapshot.HasRightWindowCommands);
        Assert.Equal(500, snapshot.Width);
        Assert.Equal(36, snapshot.Height);
        Assert.Contains("TitleBar preview ready. TitleBar QA", text);
        Assert.Contains("title FluentJalium Gallery", text);
        Assert.Contains("Icon off", text);
        Assert.Contains("Title text on", text);
        Assert.Contains("Maximized on", text);
        Assert.Contains("Buttons min/max/close on/off/on", text);
        Assert.Contains("Commands left/right on/on", text);
        Assert.Contains("Size 500x36", text);
    }

    private static void InvokeTitleBarCommand(TitleBar titleBar, string methodName)
    {
        typeof(TitleBar)
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(titleBar, null);
    }
}
