# TestUse: UC-PRICE-001 Calculate Order Price

**Status:** Ready  
**Use case:** [UC-PRICE-001: Calculate Order Price](../../../use-cases/pricing/UC-PRICE-001-calculate-order-price.md)

## Purpose

Translate the two independent pricing conditions into one complete executable
decision table.

## Acceptance-criteria coverage

| Claim | What is selected | Type | Technique | Why the technique fits | Status |
|---|---|---|---|---|---|
| [AC-001](../../../use-cases/pricing/UC-PRICE-001-calculate-order-price.md#ac-001) | All four combinations of premium membership and valid-coupon state | Unit | Decision table | Both independent conditions affect the discount outcome | Implemented |

## Executable decision table

| Row | Premium customer | Valid coupon | Expected price for 100 |
|---|---|---|---|
| standard | No | No | 100 |
| premium | Yes | No | 90 |
| coupon | No | Yes | 95 |
| combined | Yes | Yes | 85 |

The same four rows appear in `PricingTests.PricingTable`; the Markdown explains
the evidence design and the C# table executes it.

