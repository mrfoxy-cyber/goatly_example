namespace Goatly.Testing.Metadata;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class UsesTestTechniqueAttribute : Attribute
{
    public UsesTestTechniqueAttribute(
        TestTechnique technique,
        string what,
        string why)
    {
        if (!Enum.IsDefined(technique))
        {
            throw new ArgumentOutOfRangeException(
                nameof(technique),
                technique,
                "A recognized test technique is required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(what);
        ArgumentException.ThrowIfNullOrWhiteSpace(why);

        Technique = technique;
        What = what;
        Why = why;
    }

    public TestTechnique Technique { get; }
    public string What { get; }
    public string Why { get; }
}
