using Showcase.Examples;

namespace Showcase.Examples.Tests;

public sealed class ShoppingCartTests(ProductCatalogFixture fixture)
    : IClassFixture<ProductCatalogFixture>
{
    [Fact]
    [UseCase(typeof(Criteria.UC_CART_001))]
    [Covers(typeof(Criteria.UC_CART_001.AC_001))]
    [TestType(TestType.Unit)]
    [Behavior(
        "a catalog fixture with known products and a new cart",
        "several product quantities are added",
        "the cart total uses the fixture prices")]
    [UsesTestTechnique(
        TestTechnique.ExpertJudgment,
        what: "A cart containing multiple products and a repeated quantity.",
        why: "This representative case exercises lookup, quantity multiplication, and summation together.")]
    public void Known_products_are_totaled_using_fixture_data()
    {
        var cart = new ShoppingCart(fixture.Catalog);

        cart.Add("apple", 2);
        cart.Add("bread", 1);

        Assert.Equal(6.00m, cart.Total);
    }

    [Fact]
    [UseCase(typeof(Criteria.UC_CART_001))]
    [Covers(typeof(Criteria.UC_CART_001.AC_002))]
    [TestType(TestType.Unit)]
    [Behavior(
        "a catalog fixture without the requested product",
        "that product is added to a new cart",
        "the unknown SKU is rejected")]
    [UsesTestTechnique(
        TestTechnique.EquivalencePartitioning,
        what: "The unknown-SKU input partition.",
        why: "All SKUs absent from the fixture catalog follow the same failure path.")]
    public void Unknown_products_are_rejected()
    {
        var cart = new ShoppingCart(fixture.Catalog);

        Assert.Throws<KeyNotFoundException>(() => cart.Add("missing", 1));
    }
}

