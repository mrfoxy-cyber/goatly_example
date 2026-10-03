namespace Showcase.Examples;

public enum CustomerTier
{
    Standard,
    Premium
}

public static class OrderPricing
{
    public static decimal Calculate(decimal subtotal, CustomerTier tier, bool hasValidCoupon)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(subtotal);

        var discount = tier == CustomerTier.Premium ? 0.10m : 0m;
        if (hasValidCoupon)
        {
            discount += 0.05m;
        }

        return decimal.Round(subtotal * (1 - discount), 2, MidpointRounding.AwayFromZero);
    }
}

