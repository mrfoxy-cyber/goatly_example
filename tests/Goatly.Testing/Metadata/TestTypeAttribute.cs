namespace Goatly.Testing.Metadata;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class TestTypeAttribute : Attribute
{
    public TestTypeAttribute(TestType type) => Type = type;

    public TestType Type { get; }
}
