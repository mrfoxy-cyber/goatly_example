namespace Showcase.Examples;

public sealed record Product(string Sku, string Name, decimal Price);

public sealed class ProductCatalog(IEnumerable<Product> products)
{
    private readonly IReadOnlyDictionary<string, Product> _products = products
        .ToDictionary(product => product.Sku, StringComparer.OrdinalIgnoreCase);

    public Product Find(string sku) =>
        _products.TryGetValue(sku, out var product)
            ? product
            : throw new KeyNotFoundException($"Unknown product '{sku}'.");
}

public sealed class ShoppingCart(ProductCatalog catalog)
{
    private readonly List<(Product Product, int Quantity)> _lines = [];

    public void Add(string sku, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        _lines.Add((catalog.Find(sku), quantity));
    }

    public decimal Total => _lines.Sum(line => line.Product.Price * line.Quantity);
}

