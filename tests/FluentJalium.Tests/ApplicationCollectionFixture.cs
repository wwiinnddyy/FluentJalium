using Jalium.UI.Markup;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace FluentJalium.Tests;

[CollectionDefinition("Application")]
public sealed class ApplicationCollection : ICollectionFixture<ApplicationCollectionFixture>
{
}

public sealed class ApplicationCollectionFixture
{
    public ApplicationCollectionFixture()
    {
        ThemeLoader.Initialize();
    }
}
