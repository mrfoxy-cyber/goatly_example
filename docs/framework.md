# Framework guide

`Goatly.Testing` complements xUnit; it does not replace the test runner. xUnit
still discovers and executes facts and theories. The framework records why each
use-case test exists and how its cases were selected.

## Traceability chain

```text
Use case
└── acceptance claim
    └── executable test metadata
        └── xUnit result
```

A complete linked test declares:

- `UseCase`: the behavior being supported;
- `Covers`: one or more acceptance claims evidenced by the test;
- `TestType`: unit, contract, integration, or guardrail;
- `Behavior`: the Given/When/Then meaning;
- `UsesTestTechnique`: how cases were selected, including `what` and `why`.

`TestCatalog.Discover` reads that metadata and reports missing, contradictory,
or duplicate declarations. The showcase's `MetadataCatalogTests` acts as a
guardrail over all example test containers.

## Fixtures

`ShoppingCartTests` implements `IClassFixture<ProductCatalogFixture>`. The
fixture creates reusable known products once for the test class, while each
test creates its own cart so mutable state is not shared.

## Decision tables

`PricingTests` defines conditions and expected outcomes as a
`DecisionTable<PricingCase>`. `AsTheoryData` turns every validated row into a
separate xUnit theory case. Duplicate row IDs, missing columns, incomplete rows,
and mismatched decision counts are rejected when the table is constructed.

## Pending human review

Use `BehaviorNeedsVerification` or `TestCoverageNeedsVerification` only when a
test exists but a human has not verified that part of its design. These markers
make unfinished metadata visible without inventing certainty.

