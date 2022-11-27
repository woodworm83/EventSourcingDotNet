using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.Serialization.Json;

internal static class TypeExtensions
{
    public static bool HasEncryptedProperties(this Type type)
        => type.GetProperties().Any(HasEncryptedAttribute);

    public static bool HasEncryptedAttribute(this JsonProperty jsonProperty, Type type)
    {
        return jsonProperty.UnderlyingName is { } propertyName
               && type.GetProperty(propertyName) is { } property 
               && HasEncryptedAttribute(property);
    }

    private static bool HasEncryptedAttribute(MemberInfo property) 
        => property.CustomAttributes.Any(x => x.AttributeType == typeof(EncryptedAttribute));
}