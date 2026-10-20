using System.Linq;
using System.Reflection;

namespace Cosmere.Core.Framework;

/// <summary>
///     Version and build time, read off the assembly. The release build passes the tag in as
///     InformationalVersion, and the csproj stamps BuildTime as assembly metadata.
/// </summary>
public static class BuildInfo {
    public static readonly string Revision = Attribute<AssemblyInformationalVersionAttribute>()?.ConstructorArguments[0].Value as string ?? "unknown";

    public static readonly string BuildTime =
        typeof(BuildInfo).Assembly.GetCustomAttributesData()
            .Where(a => a.AttributeType == typeof(AssemblyMetadataAttribute))
            .FirstOrDefault(a => a.ConstructorArguments[0].Value as string == "BuildTime")
            ?.ConstructorArguments[1].Value as string
        ?? "unknown";

    // CustomAttributeData, not GetCustomAttribute: that would load every attribute type on the assembly.
    private static CustomAttributeData? Attribute<T>() {
        return typeof(BuildInfo).Assembly.GetCustomAttributesData().FirstOrDefault(a => a.AttributeType == typeof(T));
    }
}
