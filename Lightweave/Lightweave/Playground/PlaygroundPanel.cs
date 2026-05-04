using System;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Verse;

namespace Cosmere.Lightweave.Playground;

public sealed record PlaygroundVariant(
    string LabelKey,
    LightweaveNode Demo,
    [CallerArgumentExpression("Demo")] string Code = ""
);

public sealed record PlaygroundState(
    string LabelKey,
    LightweaveNode Demo,
    [CallerArgumentExpression("Demo")] string Code = ""
);

public sealed record PlaygroundPanelResult(LightweaveNode Body, IReadOnlyList<TocEntry> TocEntries);

public static class PlaygroundPanel {
    private const string ExamplesAnchor = "examples";
    private const string StatesAnchor = "states";
    private const string UsageAnchor = "usage";
    private const string CompositionAnchor = "composition";
    private const string RtlAnchor = "rtl";
    private const string ApiAnchor = "api-reference";
    private const string SourceAnchor = "source";

    public static PlaygroundPanelResult Create(
        string titleKey,
        string whatKey,
        string whenKey,
        IReadOnlyList<PlaygroundVariant>? variants,
        IReadOnlyList<PlaygroundState>? states,
        string sourcePath,
        DocContext ctx,
        LightweaveNode? breadcrumb = null,
        float? variantMinHeight = null,
        PlaygroundDocs? docs = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        bool hasVariants = variants != null && variants.Count > 0;
        bool hasStates = states != null && states.Count > 0;
        bool hasUsage = !string.IsNullOrEmpty(docs?.UsageCode);
        bool hasComposition = docs?.Composition != null && docs.Composition.Count > 0;
        bool hasRtl = docs?.ShowRtl == true && hasVariants;
        bool hasApi = docs?.ApiReference != null && docs.ApiReference.Count > 0;

        List<TocEntry> tocEntries = new List<TocEntry>();

        LightweaveNode title = Typography.Typography.Heading.Create(
            1,
            (string)titleKey.Translate(),
            ThemeSlot.TextPrimary
        );

        LightweaveNode whatText = Typography.Typography.Text.Create(
            (string)whatKey.Translate(),
            FontRole.Body,
            new Rem(1f),
            ThemeSlot.TextMuted,
            wrap: true
        );

        LightweaveNode whenText = Typography.Typography.Text.Create(
            (string)whenKey.Translate(),
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextMuted,
            wrap: true
        );

        LightweaveNode bodyStack = Layout.Stack.Create(
            SpacingScale.Xl,
            s => {
                LightweaveNode intro = Layout.Stack.Create(
                    SpacingScale.Sm,
                    inner => {
                        if (breadcrumb != null) {
                            inner.Add(breadcrumb);
                        }

                        inner.Add(title);
                        inner.Add(whatText);
                        inner.Add(whenText);
                    }
                );

                s.Add(intro);
                s.Add(Layout.Divider.Horizontal());

                if (hasVariants) {
                    tocEntries.Add(new TocEntry(ExamplesAnchor, (string)"CC_Playground_Panel_Examples".Translate(), 2));
                    s.Add(BuildExamplesSection(variants!, ctx, tocEntries, variantMinHeight));
                }

                if (hasStates) {
                    tocEntries.Add(new TocEntry(StatesAnchor, (string)"CC_Playground_Panel_States".Translate(), 2));
                    s.Add(BuildStatesSection(states!, ctx, tocEntries, variantMinHeight));
                }

                if (hasUsage) {
                    tocEntries.Add(new TocEntry(UsageAnchor, (string)"CC_Playground_Panel_Usage".Translate(), 2));
                    s.Add(BuildUsageSection(NormalizeCode(docs!.UsageCode) ?? docs.UsageCode!, ctx));
                }

                if (hasComposition) {
                    tocEntries.Add(new TocEntry(CompositionAnchor, (string)"CC_Playground_Panel_Composition".Translate(), 2));
                    s.Add(BuildCompositionSection(docs!.Composition!, ctx));
                }

                if (hasRtl) {
                    tocEntries.Add(new TocEntry(RtlAnchor, (string)"CC_Playground_Panel_Rtl".Translate(), 2));
                    s.Add(BuildRtlSection(variants!, ctx, variantMinHeight));
                }

                if (hasApi) {
                    tocEntries.Add(new TocEntry(ApiAnchor, (string)"CC_Playground_Panel_Api".Translate(), 2));
                    s.Add(BuildApiSection(docs!.ApiReference!, ctx));
                }

                tocEntries.Add(new TocEntry(SourceAnchor, (string)"CC_Playground_Panel_Source".Translate(), 2));
                s.Add(BuildSourceSection(sourcePath, ctx));
            }
        );

        return new PlaygroundPanelResult(bodyStack, tocEntries);
    }

    private static LightweaveNode BuildExamplesSection(
        IReadOnlyList<PlaygroundVariant> variants,
        DocContext ctx,
        List<TocEntry> tocEntries,
        float? variantMinHeight
    ) {
        LightweaveNode heading = Typography.Typography.Heading.Create(
            2,
            (string)"CC_Playground_Panel_Examples".Translate(),
            ThemeSlot.TextPrimary
        );

        LightweaveNode body = Layout.Stack.Create(
            SpacingScale.Lg,
            s => {
                for (int i = 0; i < variants.Count; i++) {
                    PlaygroundVariant v = variants[i];
                    string anchor = ExamplesAnchor + "-" + SlugifyLabel(v.LabelKey);
                    string label = (string)v.LabelKey.Translate();
                    tocEntries.Add(new TocEntry(anchor, label, 3));
                    s.Add(BuildExampleItem(anchor, label, v.Demo, ctx, variantMinHeight, NormalizeCode(v.Code)));
                }
            }
        );

        return Doc.Doc.Section(ExamplesAnchor, heading, body, ctx);
    }

    private static LightweaveNode BuildStatesSection(
        IReadOnlyList<PlaygroundState> states,
        DocContext ctx,
        List<TocEntry> tocEntries,
        float? variantMinHeight
    ) {
        LightweaveNode heading = Typography.Typography.Heading.Create(
            2,
            (string)"CC_Playground_Panel_States".Translate(),
            ThemeSlot.TextPrimary
        );

        LightweaveNode body = Layout.Stack.Create(
            SpacingScale.Lg,
            s => {
                for (int i = 0; i < states.Count; i++) {
                    PlaygroundState st = states[i];
                    string anchor = StatesAnchor + "-" + SlugifyLabel(st.LabelKey);
                    string label = (string)st.LabelKey.Translate();
                    tocEntries.Add(new TocEntry(anchor, label, 3));
                    s.Add(BuildExampleItem(anchor, label, st.Demo, ctx, variantMinHeight, NormalizeCode(st.Code)));
                }
            }
        );

        return Doc.Doc.Section(StatesAnchor, heading, body, ctx);
    }

    private static string? NormalizeCode(string? raw) {
        if (string.IsNullOrWhiteSpace(raw)) {
            return null;
        }

        string[] lines = raw!.Replace("\r\n", "\n").Split('\n');
        int startIdx = LeadingSpaces(lines[0]) == 0 ? 1 : 0;

        int minIndent = int.MaxValue;
        for (int i = startIdx; i < lines.Length; i++) {
            if (string.IsNullOrWhiteSpace(lines[i])) {
                continue;
            }

            int indent = LeadingSpaces(lines[i]);
            if (indent < minIndent) {
                minIndent = indent;
            }
        }

        if (minIndent != int.MaxValue && minIndent > 0) {
            int dedent = minIndent;
            if (startIdx == 1) {
                const int indentUnit = 4;
                dedent = Math.Max(0, minIndent - indentUnit);
            }

            if (dedent > 0) {
                for (int i = startIdx; i < lines.Length; i++) {
                    if (lines[i].Length >= dedent) {
                        lines[i] = lines[i].Substring(dedent);
                    }
                }
            }
        }

        string dedented = string.Join("\n", lines).Trim('\n');
        return StripQualifiedNames(dedented);
    }

    private static string StripQualifiedNames(string code) {
        if (string.IsNullOrEmpty(code)) {
            return code;
        }

        SortedSet<string> usedModules = new SortedSet<string>(StringComparer.Ordinal);
        StringBuilder body = new StringBuilder(code.Length);

        int i = 0;
        int n = code.Length;
        while (i < n) {
            int wordStart = i;
            while (i < n && (char.IsLetterOrDigit(code[i]) || code[i] == '_')) {
                i++;
            }

            if (i > wordStart) {
                string ident = code.Substring(wordStart, i - wordStart);

                if (i + 1 + ident.Length + 1 <= n
                    && code[i] == '.'
                    && string.CompareOrdinal(code, i + 1, ident, 0, ident.Length) == 0
                    && i + 1 + ident.Length < n
                    && code[i + 1 + ident.Length] == '.') {
                    usedModules.Add(ident);
                    i += 1 + ident.Length + 1;
                    continue;
                }

                body.Append(ident);
                continue;
            }

            body.Append(code[i]);
            i++;
        }

        if (usedModules.Count == 0) {
            return body.ToString();
        }

        StringBuilder header = new StringBuilder();
        header.Append("using Cosmere.Lightweave;\n\n");
        header.Append(body);
        return header.ToString();
    }

    private static int LeadingSpaces(string s) {
        int n = 0;
        while (n < s.Length && s[n] == ' ') {
            n++;
        }

        return n;
    }

    private static LightweaveNode BuildExampleItem(
        string anchorId,
        string label,
        LightweaveNode demo,
        DocContext ctx,
        float? variantMinHeight,
        string? code
    ) {
        LightweaveNode heading = Typography.Typography.Heading.Create(
            3,
            label,
            ThemeSlot.TextPrimary
        );

        LightweaveNode body = code != null
            ? BuildPreviewWithCodeFrame(demo, variantMinHeight, code, anchorId)
            : BuildPreviewFrame(demo, variantMinHeight);

        return Doc.Doc.Section(anchorId, heading, body, ctx);
    }

    private static LightweaveNode BuildPreviewFrame(LightweaveNode demo, float? minHeight) {
        LightweaveNode content = minHeight.HasValue
            ? Layout.Stack.Create(new Rem(0f), s => s.Add(demo, minHeight.Value))
            : demo;

        return Layout.Box.Create(
            EdgeInsets.All(SpacingScale.Lg),
            new BackgroundSpec.Solid(ThemeSlot.SurfacePrimary),
            BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
            RadiusSpec.All(new Rem(0.5f)),
            c => c.Add(content)
        );
    }

    private static LightweaveNode BuildPreviewWithCodeFrame(
        LightweaveNode demo,
        float? minHeight,
        string code,
        string anchorId
    ) {
        LightweaveNode content = minHeight.HasValue
            ? Layout.Stack.Create(new Rem(0f), s => s.Add(demo, minHeight.Value))
            : demo;

        LightweaveNode previewSection = Layout.Box.Create(
            EdgeInsets.All(SpacingScale.Lg),
            null,
            null,
            null,
            c => c.Add(content)
        );

        LightweaveNode codeSection = Doc.Doc.CodeBlock(
            code,
            flat: true,
            key: anchorId,
            backgroundRadius: RadiusSpec.Bottom(new Rem(0.4375f))
        );

        return Layout.Box.Create(
            EdgeInsets.All(new Rem(1f / 16f)),
            new BackgroundSpec.Solid(ThemeSlot.SurfacePrimary),
            BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
            RadiusSpec.All(new Rem(0.5f)),
            c => {
                c.Add(previewSection);
                c.Add(Layout.Divider.Horizontal());
                c.Add(codeSection);
            }
        );
    }

    private static LightweaveNode BuildUsageSection(string usageCode, DocContext ctx) {
        LightweaveNode heading = Typography.Typography.Heading.Create(
            2,
            (string)"CC_Playground_Panel_Usage".Translate(),
            ThemeSlot.TextPrimary
        );

        return Doc.Doc.Section(UsageAnchor, heading, Doc.Doc.CodeBlock(usageCode), ctx);
    }

    private static LightweaveNode BuildCompositionSection(
        IReadOnlyList<CompositionLine> lines,
        DocContext ctx
    ) {
        LightweaveNode heading = Typography.Typography.Heading.Create(
            2,
            (string)"CC_Playground_Panel_Composition".Translate(),
            ThemeSlot.TextPrimary
        );

        return Doc.Doc.Section(CompositionAnchor, heading, Doc.Doc.CompositionTree(lines), ctx);
    }

    private static LightweaveNode BuildRtlSection(
        IReadOnlyList<PlaygroundVariant> variants,
        DocContext ctx,
        float? variantMinHeight
    ) {
        LightweaveNode heading = Typography.Typography.Heading.Create(
            2,
            (string)"CC_Playground_Panel_Rtl".Translate(),
            ThemeSlot.TextPrimary
        );

        PlaygroundVariant first = variants[0];
        LightweaveNode mirrored = Doc.Doc.DirectionScope(Direction.Rtl, first.Demo);
        LightweaveNode body = BuildPreviewFrame(mirrored, variantMinHeight);

        return Doc.Doc.Section(RtlAnchor, heading, body, ctx);
    }

    private static LightweaveNode BuildApiSection(
        IReadOnlyList<ApiParam> parameters,
        DocContext ctx
    ) {
        LightweaveNode heading = Typography.Typography.Heading.Create(
            2,
            (string)"CC_Playground_Panel_Api".Translate(),
            ThemeSlot.TextPrimary
        );

        return Doc.Doc.Section(ApiAnchor, heading, Doc.Doc.ApiTable(parameters), ctx);
    }

    private static LightweaveNode BuildSourceSection(string sourcePath, DocContext ctx) {
        LightweaveNode heading = Typography.Typography.Heading.Create(
            2,
            (string)"CC_Playground_Panel_Source".Translate(),
            ThemeSlot.TextPrimary
        );

        string fileLabel = (string)"CC_Playground_Panel_SourceFile".Translate();
        LightweaveNode fileRow = Typography.Typography.Text.Create(
            fileLabel + " " + sourcePath,
            FontRole.Mono,
            new Rem(0.75f),
            ThemeSlot.TextMuted
        );

        LightweaveNode body = Layout.Stack.Create(
            SpacingScale.Xs,
            s => {
                s.Add(fileRow);
                s.Add(SourceLink.Create(sourcePath));
                s.Add(SourceLink.GithubLink(sourcePath));
            }
        );
        return Doc.Doc.Section(SourceAnchor, heading, body, ctx);
    }

    private static string SlugifyLabel(string labelKey) {
        string result = labelKey.Replace("CC_Playground_Label_", "").ToLowerInvariant();
        char[] chars = new char[result.Length];
        for (int i = 0; i < result.Length; i++) {
            char c = result[i];
            chars[i] = char.IsLetterOrDigit(c) ? c : '-';
        }

        return new string(chars);
    }
}
