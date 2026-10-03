# Goatly testing framework showcase

This repository is a focused, runnable showcase of the testing framework from
`P:\Goatly\Goatly.Backend\tests\Goatly.Testing`.

The complete framework is kept under `tests/Goatly.Testing`. It provides:

- use-case and acceptance-claim links;
- Given/When/Then behavior metadata;
- unit, contract, integration, and guardrail test types;
- explicit test-technique selection with required `what` and `why` reasoning;
- decision tables that feed xUnit theories;
- reflection-based catalog discovery and validation;
- explicit markers for metadata that still needs human verification.

The repository also retains the framework's own tests and adds small examples
for ordinary facts, theories, class fixtures, boundary-value analysis,
equivalence partitioning, and decision tables.

## Run everything

Requires the .NET 10 SDK.

```powershell
dotnet test Goatly.Testing.Showcase.slnx
```

## Repository map

```text
src/Showcase.Examples/             Small code under test
tests/Goatly.Testing/              The complete Goatly test framework
tests/Goatly.Testing.Tests/        The framework's original self-tests
tests/Showcase.Examples.Tests/     Basic fixture and metadata examples
docs/framework.md                  How the pieces fit together
```

Start with `PricingTests.cs` for the decision-table example and
`ShoppingCartTests.cs` for an xUnit class fixture example.

