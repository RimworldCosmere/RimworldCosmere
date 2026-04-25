using System;

namespace Cosmere.Lightweave.Doc;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class DocParamAttribute : Attribute {
    public string Description { get; }
    public string TypeOverride { get; init; } = "";
    public string DefaultOverride { get; init; } = "";

    public DocParamAttribute(string description) {
        Description = description;
    }
}
