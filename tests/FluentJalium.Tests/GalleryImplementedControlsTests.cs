using System.Reflection;
using FluentJalium.Gallery.Models;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// The catalog's headline count has to come from the assembly, not from flattening keyword tokens
/// out of each page's metadata.
/// </summary>
public sealed class GalleryImplementedControlsTests
{
    [Fact]
    public void All_ShouldListConcreteFluentControlsOnceEach()
    {
        var all = GalleryImplementedControls.All;

        Assert.NotEmpty(all);
        Assert.All(all, control => Assert.StartsWith("FW", control.Name, StringComparison.Ordinal));
        Assert.Equal(all.Length, all.Select(control => control.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.All(all, control => Assert.False(string.IsNullOrWhiteSpace(control.Namespace)));
    }

    [Fact]
    public void All_ShouldNotCountAbstractOrNonControlTypes()
    {
        Assert.False(GalleryImplementedControls.IsImplemented(nameof(Object)));
        Assert.False(GalleryImplementedControls.IsImplemented("FWButtonAutomationPeer"));
        Assert.True(GalleryImplementedControls.IsImplemented("FWButton"));
    }

    [Fact]
    public void Count_ShouldMatchReflectionOverTheMarkerInterface()
    {
        var marker = typeof(FluentJalium.Controls.IFluentJaliumControl);

        var expected = marker.Assembly.GetExportedTypes()
            .Count(type => type.IsClass && !type.IsAbstract && marker.IsAssignableFrom(type));

        Assert.Equal(expected, GalleryImplementedControls.Count);
    }
}
