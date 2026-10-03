# Framework guide

The showcase has two connected parts:

- `Goatly.UseCases` turns business UseCases into validated, typed test links.
- `Goatly.Testing` records executable-test behavior and selection reasoning.

Both complement xUnit; they do not replace the test runner. xUnit still
discovers and executes facts and theories.

## From UseCase to TestUse

A business UseCase is the authoritative description of behavior. Its numbered
acceptance criteria describe what must be true without prescribing a test
implementation.

A matching TestUse translates every criterion into an evidence plan. Its table
states:

- what cases, risks, boundaries, or states are selected;
- which test type will provide the evidence;
- which test-design technique selects the cases;
- why that technique fits;
- whether the evidence is implemented, planned, deferred, or manual.

The parser requires a non-draft UseCase and TestUse to form a pair and requires
their criterion IDs to match exactly.

## Generated typed links

When a test project sets `EnableGoatlyUseCases`, the source generator reads both
document sets and creates types such as:

```csharp
Criteria.UC_PRICE_001
Criteria.UC_PRICE_001.AC_001
```

Tests refer to those types rather than repeating IDs and criterion text as
strings. `UseCaseAttribute` reads the generated UseCase ID. `CoversAttribute`
reads the generated UseCase ID, criterion ID, and criterion text.

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

`TestCatalog.Discover` reads the runtime metadata and reports missing,
contradictory, or duplicate declarations. The showcase's
`MetadataCatalogTests` acts as a guardrail over all example test containers.

The use-case CLI performs the cross-file collection. It scans typed `Covers`
attributes and joins them back to the parsed UseCase and TestUse catalogue:

```powershell
.\goatly criterion UC-PRICE-001/AC-001
```

That output is the navigable chain from requirement to plan to executable test.

## Revision and missing-coverage checks

`docs/testing/criteria.lock` stores semantic fingerprints for reviewed UseCases
and criteria. Formatting-only edits are ignored, while meaningful wording
changes require the linked tests to be reviewed and the revision acknowledged.

```powershell
.\goatly changed
.\goatly missing
.\goatly check
```

`changed` finds reviewed definitions that changed. `missing` finds required
criteria without a typed executable link. `check` combines document pairing,
criterion parity, revision, and missing-link validation.

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

