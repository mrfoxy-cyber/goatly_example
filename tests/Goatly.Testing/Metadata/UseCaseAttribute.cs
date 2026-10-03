namespace Goatly.Testing.Metadata;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class UseCaseAttribute : Attribute
{
    public UseCaseAttribute(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
    }

    public UseCaseAttribute(Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);
        Id = GeneratedDefinition.ReadRequiredConstant(definitionType, "Id");
        DefinitionType = definitionType;
    }

    public string Id { get; }
    public Type? DefinitionType { get; }
}
