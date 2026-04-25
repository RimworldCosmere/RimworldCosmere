using System;

namespace Cosmere.Lightweave.Doc;

[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = false,
    Inherited = false)]
public sealed class DocVariantAttribute : Attribute {
    public string LabelKey { get; }
    public int Order { get; init; }

    public DocVariantAttribute(string labelKey) {
        LabelKey = labelKey;
    }
}
