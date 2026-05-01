using System;

namespace Cosmere.Lightweave.Doc;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = false,
    Inherited = false)]
public sealed class DocAttribute : Attribute {
    public string Id { get; init; } = "";
    public string Summary { get; init; } = "";
    public string WhenToUse { get; init; } = "";
    public string Category { get; init; } = "";
    public string SourcePath { get; init; } = "";
    public float PreferredVariantHeight { get; init; }
    public bool ShowRtl { get; init; } = true;
    public bool Slot { get; init; }
    public string ParentSlot { get; init; } = "";
}
