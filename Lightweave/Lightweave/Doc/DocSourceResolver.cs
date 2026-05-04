using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Cosmere.Lightweave.Runtime;

namespace Cosmere.Lightweave.Doc;

internal static class DocSourceResolver {
    private static readonly ConcurrentDictionary<string, string?> Cache = new ConcurrentDictionary<string, string?>();

    public static string? ResolveMethodSource(string demoExpr, string file) {
        if (string.IsNullOrEmpty(demoExpr) || string.IsNullOrEmpty(file)) {
            return null;
        }

        string methodName = ExtractMethodName(demoExpr);
        if (string.IsNullOrEmpty(methodName)) {
            return null;
        }

        string key = file + "::" + methodName;
        return Cache.GetOrAdd(key, _ => LoadAndExtract(file, methodName));
    }

    private static string ExtractMethodName(string expr) {
        int paren = expr.IndexOf('(');
        string head = paren < 0 ? expr : expr.Substring(0, paren);
        head = head.Trim();
        int dot = head.LastIndexOf('.');
        return dot < 0 ? head : head.Substring(dot + 1);
    }

    private static string? LoadAndExtract(string file, string methodName) {
        try {
            if (!File.Exists(file)) {
                return null;
            }

            string source = File.ReadAllText(file);
            return ExtractMethod(source, methodName);
        }
        catch (IOException ex) {
            LightweaveLog.Warning($"DocSourceResolver IO failure for {file}: {ex}");
            return null;
        }
        catch (UnauthorizedAccessException ex) {
            LightweaveLog.Warning($"DocSourceResolver access denied for {file}: {ex}");
            return null;
        }
    }

    private static string? ExtractMethod(string source, string methodName) {
        int searchFrom = 0;
        while (searchFrom < source.Length) {
            int matchIdx = source.IndexOf(methodName, searchFrom, StringComparison.Ordinal);
            if (matchIdx < 0) {
                return null;
            }

            searchFrom = matchIdx + methodName.Length;

            if (matchIdx > 0 && IsIdentifierChar(source[matchIdx - 1])) {
                continue;
            }

            int afterEnd = matchIdx + methodName.Length;
            if (afterEnd < source.Length && IsIdentifierChar(source[afterEnd])) {
                continue;
            }

            int parenIdx = afterEnd;
            while (parenIdx < source.Length && (source[parenIdx] == ' ' || source[parenIdx] == '\t')) {
                parenIdx++;
            }

            if (parenIdx >= source.Length || source[parenIdx] != '(') {
                continue;
            }

            int sol = matchIdx;
            while (sol > 0 && source[sol - 1] != '\n') {
                sol--;
            }

            int eol = matchIdx;
            while (eol < source.Length && source[eol] != '\n') {
                eol++;
            }

            string line = source.Substring(sol, eol - sol);
            if (!LooksLikeDeclaration(line)) {
                continue;
            }

            int idx = parenIdx + 1;
            int parenDepth = 1;
            while (idx < source.Length && parenDepth > 0) {
                char c = source[idx];
                if (c == '(') {
                    parenDepth++;
                }
                else if (c == ')') {
                    parenDepth--;
                }

                idx++;
            }

            while (idx < source.Length && source[idx] != '{') {
                idx++;
            }

            if (idx >= source.Length) {
                return null;
            }

            int depth = 0;
            int end = idx;
            while (end < source.Length) {
                char c = source[end];
                if (c == '{') {
                    depth++;
                }
                else if (c == '}') {
                    depth--;
                    if (depth == 0) {
                        end++;
                        break;
                    }
                }

                end++;
            }

            int bodyStart = idx + 1;
            int bodyEnd = end - 1;
            if (bodyEnd <= bodyStart) {
                return string.Empty;
            }

            string body = source.Substring(bodyStart, bodyEnd - bodyStart);
            body = body.Trim('\r', '\n');
            return Dedent(body).TrimEnd();
        }

        return null;
    }

    private static bool IsIdentifierChar(char c) {
        return char.IsLetterOrDigit(c) || c == '_';
    }

    private static bool LooksLikeDeclaration(string line) {
        return line.Contains(" static ")
            || line.TrimStart().StartsWith("static ", StringComparison.Ordinal)
            || line.Contains("public ")
            || line.Contains("private ")
            || line.Contains("protected ")
            || line.Contains("internal ");
    }

    private static string Dedent(string code) {
        string[] lines = code.Split('\n');
        int minIndent = int.MaxValue;
        for (int i = 0; i < lines.Length; i++) {
            string ln = lines[i].TrimEnd('\r');
            if (ln.Trim().Length == 0) {
                continue;
            }

            int indent = 0;
            while (indent < ln.Length && (ln[indent] == ' ' || ln[indent] == '\t')) {
                indent++;
            }

            if (indent < minIndent) {
                minIndent = indent;
            }
        }

        if (minIndent <= 0 || minIndent == int.MaxValue) {
            StringBuilder plain = new StringBuilder();
            for (int i = 0; i < lines.Length; i++) {
                plain.Append(lines[i].TrimEnd('\r'));
                if (i < lines.Length - 1) {
                    plain.Append('\n');
                }
            }

            return plain.ToString();
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < lines.Length; i++) {
            string ln = lines[i].TrimEnd('\r');
            if (ln.Length >= minIndent) {
                bool allWs = true;
                for (int k = 0; k < minIndent; k++) {
                    if (ln[k] != ' ' && ln[k] != '\t') {
                        allWs = false;
                        break;
                    }
                }

                sb.Append(allWs ? ln.Substring(minIndent) : ln);
            }
            else {
                sb.Append(ln);
            }

            if (i < lines.Length - 1) {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }
}
