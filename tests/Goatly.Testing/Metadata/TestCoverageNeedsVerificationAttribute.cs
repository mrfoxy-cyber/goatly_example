namespace Goatly.Testing.Metadata;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class TestCoverageNeedsVerificationAttribute : Attribute
{
    public TestCoverageNeedsVerificationAttribute(
        string reason = "The test-technique selections and their what-and-why reasoning have not been verified yet.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Reason = reason;
    }

    public string Reason { get; }
}
