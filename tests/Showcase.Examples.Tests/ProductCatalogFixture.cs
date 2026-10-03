using Showcase.Examples;

namespace Showcase.Examples.Tests;

public sealed class ProductCatalogFixture
{
    public ProductCatalogFixture()
    {
        Catalog = new ProductCatalog(
        [
            new Product("apple", "Apple", 1.50m),
            new Product("bread", "Bread", 3.00m)
        ]);
    }

    public ProductCatalog Catalog { get; }
}

