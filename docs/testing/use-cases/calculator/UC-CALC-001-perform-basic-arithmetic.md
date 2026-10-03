# TestUse: UC-CALC-001 Perform Basic Arithmetic

**Status:** Ready  
**Use case:** [UC-CALC-001: Perform Basic Arithmetic](../../../use-cases/calculator/UC-CALC-001-perform-basic-arithmetic.md)

## Purpose

Translate the arithmetic behavior into small executable examples while making
the selected input partitions explicit.

## Acceptance-criteria coverage

| Claim | What is selected | Type | Technique | Why the technique fits | Status |
|---|---|---|---|---|---|
| [AC-001](../../../use-cases/calculator/UC-CALC-001-perform-basic-arithmetic.md#ac-001) | Positive, negative, zero-result, and maximum integer operands | Unit | Boundary value analysis | Sign changes and integer boundaries are the meaningful risks | Implemented |
| [AC-002](../../../use-cases/calculator/UC-CALC-001-perform-basic-arithmetic.md#ac-002) | The invalid zero-divisor partition | Unit | Equivalence partitioning | Every zero divisor follows the same rejection behavior | Implemented |

