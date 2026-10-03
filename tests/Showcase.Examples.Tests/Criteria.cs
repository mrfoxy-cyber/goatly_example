namespace Showcase.Examples.Tests;

public static class Criteria
{
    public static class UC_CALC_001
    {
        public const string Id = "UC-CALC-001";

        public static class AC_001
        {
            public const string UseCaseId = UC_CALC_001.Id;
            public const string Id = "AC-001";
            public const string Text = "Basic arithmetic returns the expected result.";
        }

        public static class AC_002
        {
            public const string UseCaseId = UC_CALC_001.Id;
            public const string Id = "AC-002";
            public const string Text = "Division by zero is rejected.";
        }
    }

    public static class UC_PRICE_001
    {
        public const string Id = "UC-PRICE-001";

        public static class AC_001
        {
            public const string UseCaseId = UC_PRICE_001.Id;
            public const string Id = "AC-001";
            public const string Text = "Tier and coupon discounts produce the correct price.";
        }
    }

    public static class UC_CART_001
    {
        public const string Id = "UC-CART-001";

        public static class AC_001
        {
            public const string UseCaseId = UC_CART_001.Id;
            public const string Id = "AC-001";
            public const string Text = "Known products can be added and totaled.";
        }

        public static class AC_002
        {
            public const string UseCaseId = UC_CART_001.Id;
            public const string Id = "AC-002";
            public const string Text = "Unknown products are rejected.";
        }
    }
}

