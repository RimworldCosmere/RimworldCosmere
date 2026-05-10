using System;

namespace Cosmere.Lightweave.Doc;

public sealed class DocAttribute : Attribute {
    public string Id { get; init; } = "";
    public string Summary { get; init; } = "";
    public string WhenToUse { get; init; } = "";
    public string Category { get; init; } = "";
    public DocKind Kind { get; init; } = DocKind.Primitive;
    public string SourcePath { get; init; } = "";
    public float PreferredVariantHeight { get; init; }
    public bool ShowRtl { get; init; } = false;
    public bool Slot { get; init; }
    public string ParentSlot { get; init; } = "";
    public Type? Target { get; init; }
    public string TargetMember { get; init; } = "";
}
