# TestUse: UC-CART-001 Add Products to Cart

**Status:** Ready  
**Use case:** [UC-CART-001: Add Products to Cart](../../../use-cases/cart/UC-CART-001-add-products-to-cart.md)

## Purpose

Define fixture-backed evidence for known and unknown product lookups without
sharing mutable cart state between tests.

## Acceptance-criteria coverage

| Claim | What is selected | Type | Technique | Why the technique fits | Status |
|---|---|---|---|---|---|
| [AC-001](../../../use-cases/cart/UC-CART-001-add-products-to-cart.md#ac-001) | Multiple known products and a repeated quantity | Unit | Expert judgment | The representative case exercises fixture lookup, multiplication, and summation | Implemented |
| [AC-002](../../../use-cases/cart/UC-CART-001-add-products-to-cart.md#ac-002) | The unknown-SKU partition | Unit | Equivalence partitioning | Every SKU absent from the fixture follows the same failure path | Implemented |

