# UC-CART-001: Add Products to Cart

**Status:** Implementing  
**Primary actor:** Shopper  
**Area:** Cart

## Goal

Add known products to a cart and calculate their total from catalogue prices.

## Successful flow

1. The shopper selects a known product and a positive quantity.
2. The cart looks up the product in the catalogue.
3. The cart adds the line and includes it in the total.

## Acceptance criteria

### AC-001

Known products can be added in positive quantities and the cart total uses their catalogue prices.

### AC-002

An unknown product is rejected and is not added to the cart.

