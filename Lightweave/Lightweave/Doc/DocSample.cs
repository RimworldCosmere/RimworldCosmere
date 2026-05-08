using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Runtime;

namespace Cosmere.Lightweave.Doc;

public sealed record DocSample {
    public Func<LightweaveNode> Build { get; init; }
    public string Code { get; init; }

    public DocSample(
        Func<LightweaveNode> build,
        bool useFullSource = false,
        Type? companion = null,
        [CallerArgumentExpression("build")] string buildExpr = "",
        [CallerFilePath] string file = ""
    ) {
        Build = build;
        string normalized = StripLambdaWrapper(buildExpr);
        string raw = useFullSource
            ? DocSourceResolver.ResolveMethodSource(normalized, file) ?? normalized
            : normalized;
        string qualified = QualifyLeadingMethodCall(raw, file);
        Code = AppendCompanion(qualified, companion, file);
    }

    public DocSample(
        LightweaveNode demo,
        bool useFullSource = false,
        Type? companion = null,
        [CallerArgumentExpression("demo")] string demoExpr = "",
        [CallerFilePath] string file = ""
    ) {
        Build = () => demo;
        string raw = useFullSource
            ? DocSourceResolver.ResolveMethodSource(demoExpr, file) ?? demoExpr
            : demoExpr;
        string qualified = QualifyLeadingMethodCall(raw, file);
        Code = AppendCompanion(qualified, companion, file);
    }

    public LightweaveNode Demo => Build();

    private static string StripLambdaWrapper(string expr) {
        if (string.IsNullOrEmpty(expr)) {
            return expr;
        }

        string trimmed = expr.TrimStart();
        if (!trimmed.StartsWith("() =>")) {
            return expr;
        }

        string body = trimmed.Substring(5).TrimStart();
        if (body.StartsWith("{") && body.EndsWith("}")) {
            body = body.Substring(1, body.Length - 2);
            string[] lines = body.Replace("\r\n", "\n").Split('\n');
            int minIndent = int.MaxValue;
            foreach (string line in lines) {
                if (string.IsNullOrWhiteSpace(line)) {
                    continue;
                }

                int indent = 0;
                while (indent < line.Length && line[indent] == ' ') {
                    indent++;
                }

                if (indent < minIndent) {
                    minIndent = indent;
                }
            }

            if (minIndent != int.MaxValue && minIndent > 0) {
                for (int i = 0; i < lines.Length; i++) {
                    if (lines[i].Length >= minIndent) {
                        lines[i] = lines[i].Substring(minIndent);
                    }
                }
            }

            body = string.Join("\n", lines).Trim('\n');
        }

        return body;
    }


    private static string QualifyLeadingMethodCall(string code, string file) {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(file)) {
            return code;
        }

        string className = System.IO.Path.GetFileNameWithoutExtension(file);
        if (string.IsNullOrEmpty(className)) {
            return code;
        }

        int i = 0;
        int n = code.Length;
        while (i < n && char.IsWhiteSpace(code[i])) {
            i++;
        }

        int identStart = i;
        while (i < n && (char.IsLetterOrDigit(code[i]) || code[i] == '_')) {
            i++;
        }

        if (i == identStart) {
            return code;
        }

        int identEnd = i;
        while (i < n && char.IsWhiteSpace(code[i])) {
            i++;
        }

        if (i >= n || code[i] != '(') {
            return code;
        }

        string ident = code.Substring(identStart, identEnd - identStart);
        if (ident == className) {
            return code;
        }

        return code.Substring(0, identStart) + className + "." + code.Substring(identStart);
    }


    private static string AppendCompanion(string code, Type? companion, string file) {
        if (companion == null) {
            return code;
        }

        string? companionSrc = DocSourceResolver.ResolveTypeSource(file, companion.Name);
        if (string.IsNullOrEmpty(companionSrc)) {
            return code;
        }

        return code + "\n\n" + companionSrc;
    }
}
