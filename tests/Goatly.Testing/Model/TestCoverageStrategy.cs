namespace Goatly.Testing.Model;

public abstract record TestCoverageStrategy
{
    private TestCoverageStrategy()
    {
    }

    public sealed record HumanVerified(
        IReadOnlyList<TestTechniqueSelection> Selections) : TestCoverageStrategy;

    public sealed record NeedsVerification(string Reason) : TestCoverageStrategy;
}
