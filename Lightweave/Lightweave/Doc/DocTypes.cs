namespace Cosmere.Lightweave.Doc;

public sealed record ApiParam(string Name, string Type, string DefaultValue, string Description);

public sealed record ApiGroup(string MethodName, IReadOnlyList<ApiParam> Parameters);

public sealed record CompositionLine(int Indent, string Text);

public sealed record PlaygroundDocs(
    string? UsageCode = null,
    IReadOnlyList<CompositionLine>? Composition = null,
    IReadOnlyList<ApiGroup>? ApiReference = null,
    bool ShowRtl = false
);
