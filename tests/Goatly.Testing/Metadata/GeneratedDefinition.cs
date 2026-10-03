using System.Reflection;

namespace Goatly.Testing.Metadata;

internal static class GeneratedDefinition
{
    public static string ReadRequiredConstant(Type definitionType, string fieldName)
    {
        var field = definitionType.GetField(
            fieldName,
            BindingFlags.Public | BindingFlags.Static);
        var value = field?.GetRawConstantValue() as string;

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"{definitionType.FullName} is not a generated Goatly definition with a {fieldName} constant.",
                nameof(definitionType));
        }

        return value;
    }
}
