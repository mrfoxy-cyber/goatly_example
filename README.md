# Goatly testing framework showcase

This repository is a focused, runnable showcase of the use-case-driven testing
system from `P:\Goatly`. It demonstrates the complete path from a business
UseCase to executable evidence—not only the final xUnit test.

```text
Business UseCase Markdown
        │ defines numbered acceptance criteria
        ▼
TestUse Markdown
        │ translates each criterion into test type,
        │ technique, selected cases, and reasoning
        ▼
Generated Criteria.UC_...AC_... types
        │ create build-checked links
        ▼
xUnit tests with Goatly.Testing metadata
        │ are discovered and collected
        ▼
Use-case catalogue and test results
```

The pricing example is the clearest end-to-end walkthrough:

1. The [business UseCase](docs/use-cases/pricing/UC-PRICE-001-calculate-order-price.md)
   states the pricing rules and `AC-001`.
2. The matching [TestUse](docs/testing/use-cases/pricing/UC-PRICE-001-calculate-order-price.md)
   translates `AC-001` into a decision-table test design and explains why.
3. The source generator creates `Criteria.UC_PRICE_001.AC_001` from the documents.
4. [PricingTests](tests/Showcase.Examples.Tests/PricingTests.cs) links to that
   generated type through `UseCase` and `Covers` attributes.
5. The catalogue reports the criterion as mapped and points to the exact test.

The complete runtime framework is kept under `tests/Goatly.Testing`. It provides:

- use-case and acceptance-claim links;
- Given/When/Then behavior metadata;
- unit, contract, integration, and guardrail test types;
- explicit test-technique selection with required `what` and `why` reasoning;
- decision tables that feed xUnit theories;
- reflection-based catalog discovery and validation;
- explicit markers for metadata that still needs human verification.

The copied use-case tooling under `tools/` parses and pairs the Markdown,
validates criterion parity, generates typed references, tracks reviewed
revisions, finds test links, and reports missing coverage. The repository also
retains the framework's own tests and adds small examples for ordinary facts,
theories, class fixtures, boundary-value analysis, equivalence partitioning,
and decision tables.

## Run everything

Requires the .NET 10 SDK.

```powershell
dotnet test Goatly.Testing.Showcase.slnx
```

Validate the document-to-test mapping:

```powershell
.\goatly check
.\goatly usecase UC-PRICE-001
.\goatly criterion UC-PRICE-001/AC-001
```

The last command displays the business criterion, the corresponding TestUse
row, and `Pricing_follows_the_decision_table` as its executable evidence.

## How a change is protected

The generated type is the connection:

```csharp
[UseCase(typeof(Criteria.UC_PRICE_001))]
[Covers(typeof(Criteria.UC_PRICE_001.AC_001))]
```

If `AC-001` is removed or renamed in the UseCase, the generated member
disappears and the test no longer compiles. If its wording changes, the revision
lock reports it as changed until the mapped tests are reviewed and the revision
is acknowledged. If a required criterion has no `Covers` link, `goatly check`
reports it as unmapped.

## Repository map

```text
src/Showcase.Examples/             Small code under test
tests/Goatly.Testing/              The complete Goatly test framework
tests/Goatly.Testing.Tests/        The framework's original self-tests
tests/Showcase.Examples.Tests/     Basic fixture and metadata examples
docs/use-cases/                    Business UseCases
docs/testing/use-cases/            TestUses translating them into evidence
tools/Goatly.UseCases.*            Parser, generator, and catalogue CLI
docs/framework.md                  Detailed framework guide
```

Start with `PricingTests.cs` for the decision-table example and
`ShoppingCartTests.cs` for an xUnit class fixture example.

