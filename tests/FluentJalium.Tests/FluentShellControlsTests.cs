using System.Reflection;
using FluentJalium.Controls;
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

    private static void InvokeTitleBarCommand(TitleBar titleBar, string methodName)
    {
        typeof(TitleBar)
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(titleBar, null);
    }
}
