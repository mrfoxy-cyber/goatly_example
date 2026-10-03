namespace Goatly.Testing.Metadata;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class CoversAttribute : Attribute
{
    public CoversAttribute(string claimId, string claim)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(claimId);
        ArgumentException.ThrowIfNullOrWhiteSpace(claim);
        ClaimId = claimId;
        Claim = claim;
    }

    public CoversAttribute(Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);
        UseCaseId = GeneratedDefinition.ReadRequiredConstant(definitionType, "UseCaseId");
        ClaimId = GeneratedDefinition.ReadRequiredConstant(definitionType, "Id");
        Claim = GeneratedDefinition.ReadRequiredConstant(definitionType, "Text");
        DefinitionType = definitionType;
    }

    public string? UseCaseId { get; }
    public string ClaimId { get; }
    public string Claim { get; }
    public Type? DefinitionType { get; }
}
