using Goatly.Testing.Catalog;

namespace Showcase.Examples.Tests;

public sealed class MetadataCatalogTests
{
    [Fact]
    public void Every_linked_example_has_complete_framework_metadata()
    {
        var catalog = TestCatalog.Discover(
            typeof(CalculatorTests),
            typeof(PricingTests),
            typeof(ShoppingCartTests));

        Assert.True(catalog.IsValid, string.Join(Environment.NewLine, catalog.Errors));
        Assert.Equal(5, catalog.Tests.Count);
        Assert.All(catalog.Tests, test => Assert.NotEmpty(test.Covers));
    }
}

