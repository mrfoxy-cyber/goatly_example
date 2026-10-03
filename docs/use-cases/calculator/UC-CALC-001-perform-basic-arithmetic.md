# UC-CALC-001: Perform Basic Arithmetic

**Status:** Implementing  
**Primary actor:** Calculator user  
**Area:** Calculator

## Goal

Perform a requested basic arithmetic operation and receive a predictable result.

## Input

- Two numeric operands
- The requested operation

## Successful flow

1. The user supplies operands and an operation.
2. The calculator validates operation-specific constraints.
3. The calculator returns the arithmetic result.

## Failure cases

| Condition | Result |
|---|---|
| Division uses a zero divisor | The request is rejected |

## Acceptance criteria

### AC-001

Addition returns the arithmetic sum for positive, negative, zero-result, and boundary operands.

### AC-002

Division by zero is rejected instead of returning a result.

