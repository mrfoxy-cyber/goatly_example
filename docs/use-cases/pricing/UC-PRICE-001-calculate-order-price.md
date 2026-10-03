# UC-PRICE-001: Calculate Order Price

**Status:** Implementing  
**Primary actor:** Customer  
**Area:** Pricing

## Goal

Calculate an order price using the customer's tier and coupon eligibility.

## Business rules

- Standard customers receive no tier discount.
- Premium customers receive a ten-percent discount.
- A valid coupon adds a five-percent discount.
- Tier and coupon discounts may be combined.

## Acceptance criteria

### AC-001

Every combination of standard or premium tier and absent or valid coupon returns the corresponding final price.

