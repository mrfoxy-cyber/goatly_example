namespace Goatly.Testing.Metadata;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class BehaviorAttribute : Attribute
{
    public BehaviorAttribute(string given, string when, string then)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(given);
        ArgumentException.ThrowIfNullOrWhiteSpace(when);
        ArgumentException.ThrowIfNullOrWhiteSpace(then);

        Given = given;
        When = when;
        Then = then;
    }

    public string Given { get; }
    public string When { get; }
    public string Then { get; }
}
