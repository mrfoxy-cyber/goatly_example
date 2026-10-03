namespace Goatly.Testing.Model;

public sealed record TestDefinition(
    string Id,
    string Name,
    string UseCaseId,
    TestType Type,
    TestBehavior Behavior,
    TestCoverageStrategy CoverageStrategy,
    IReadOnlyList<TestClaimReference> Covers,
    Type DeclaringType,
    string MethodName);
