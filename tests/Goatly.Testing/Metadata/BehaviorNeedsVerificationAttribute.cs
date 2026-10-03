namespace Goatly.Testing.Metadata;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class BehaviorNeedsVerificationAttribute : Attribute
{
    public BehaviorNeedsVerificationAttribute(
        string reason = "The test behavior has not been verified yet.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Reason = reason;
    }

    public string Reason { get; }
}
