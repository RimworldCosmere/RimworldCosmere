using System;

namespace Cosmere.Lightweave.Doc;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class DocOverrideAttribute : Attribute {
    public string Description { get; }
    public string TypeOverride { get; init; } = "";
    public string DefaultOverride { get; init; } = "";

    public DocOverrideAttribute(string description) {
        Description = description;
    }
}
