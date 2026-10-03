using Goatly.Testing.DecisionTables;
using Showcase.Examples;

namespace Showcase.Examples.Tests;

public sealed record PricingCase(
    decimal Subtotal,
    CustomerTier Tier,
    bool HasValidCoupon,
    decimal Expected);

public sealed class PricingTests
{
    private static readonly DecisionTable<PricingCase> PricingTable = new(
        "Tier and coupon discounts",
        "an order subtotal of 100",
        ["Premium customer", "Valid coupon"],
        [
            new("standard", "No discount", ["No", "No"], "price is calculated", "the subtotal is unchanged", new(100m, CustomerTier.Standard, false, 100m)),
            new("premium", "Premium discount", ["Yes", "No"], "price is calculated", "ten percent is removed", new(100m, CustomerTier.Premium, false, 90m)),
            new("coupon", "Coupon discount", ["No", "Yes"], "price is calculated", "five percent is removed", new(100m, CustomerTier.Standard, true, 95m)),
            new("combined", "Combined discounts", ["Yes", "Yes"], "price is calculated", "fifteen percent is removed", new(100m, CustomerTier.Premium, true, 85m))
        ]);

    public static IEnumerable<object[]> PricingCases => PricingTable.AsTheoryData();

    [Theory]
    [MemberData(nameof(PricingCases))]
    [UseCase(typeof(Criteria.UC_PRICE_001))]
    [Covers(typeof(Criteria.UC_PRICE_001.AC_001))]
    [TestType(TestType.Unit)]
    [Behavior(
        "an order with a customer tier and coupon state",
        "its final price is calculated",
        "the applicable discount combination is applied")]
    [UsesTestTechnique(
        TestTechnique.DecisionTable,
        what: "All four combinations of premium membership and valid-coupon state.",
        why: "Both independent conditions affect the discount outcome.")]
    public void Pricing_follows_the_decision_table(DecisionTableRow<PricingCase> row)
    {
        var input = row.Input;

        var actual = OrderPricing.Calculate(input.Subtotal, input.Tier, input.HasValidCoupon);

        Assert.Equal(input.Expected, actual);
    }
}

