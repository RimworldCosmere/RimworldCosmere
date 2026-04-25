using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Runtime;

namespace Cosmere.Lightweave.Doc;

public sealed record DocSample {
    public LightweaveNode Demo { get; init; }
    public string Code { get; init; }

    public DocSample(
        LightweaveNode demo,
        bool useFullSource = false,
        [CallerArgumentExpression("demo")] string demoExpr = "",
        [CallerFilePath] string file = ""
    ) {
        Demo = demo;
        Code = useFullSource
            ? DocSourceResolver.ResolveMethodSource(demoExpr, file) ?? demoExpr
            : demoExpr;
    }
}
