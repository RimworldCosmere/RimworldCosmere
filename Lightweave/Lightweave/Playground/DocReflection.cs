using System;
using System.Collections.Generic;
using System.Reflection;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Verse;

namespace Cosmere.Lightweave.Playground;

internal static class DocReflection {
    private const BindingFlags MemberFlags =
        BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static readonly IReadOnlyList<PlaygroundVariant> EmptyVariants = Array.Empty<PlaygroundVariant>();
    private static readonly IReadOnlyList<PlaygroundState> EmptyStates = Array.Empty<PlaygroundState>();

    private static Dictionary<string, Type>? primitiveIndex;
    private static readonly object indexLock = new object();

    internal static Type? GetPrimitiveType(string id) {
        EnsureIndexBuilt();
        return primitiveIndex!.TryGetValue(id, out Type? t) ? t : null;
    }

    internal static (IReadOnlyList<PlaygroundVariant> variants, IReadOnlyList<PlaygroundState> states) BuildSamplesById(
        string id,
        bool forceDisabled
    ) {
        Type? primitive = GetPrimitiveType(id);
        if (primitive == null) {
            return (EmptyVariants, EmptyStates);
        }

        PlaygroundDemoContext ctx = new PlaygroundDemoContext { ForceDisabled = forceDisabled };
        return (BuildVariants(primitive, ctx), BuildStates(primitive, ctx));
    }

    internal static PlaygroundDocs? BuildDocsById(string id) {
        Type? primitive = GetPrimitiveType(id);
        if (primitive == null) {
            return null;
        }

        PlaygroundDocs docs = Build(primitive);
        if (docs.UsageCode == null && docs.Composition == null && docs.ApiReference == null) {
            return null;
        }

        return docs;
    }

    internal static IEnumerable<KeyValuePair<string, Type>> EnumerateRegistered() {
        EnsureIndexBuilt();
        return primitiveIndex!;
    }

    private static void EnsureIndexBuilt() {
        if (primitiveIndex != null) {
            return;
        }

        lock (indexLock) {
            if (primitiveIndex != null) {
                return;
            }

            Dictionary<string, Type> map = new Dictionary<string, Type>(StringComparer.Ordinal);
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < asms.Length; a++) {
                Type[] types;
                try {
                    types = asms[a].GetTypes();
                } catch (ReflectionTypeLoadException ex) {
                    types = ex.Types ?? Array.Empty<Type>();
                }

                for (int t = 0; t < types.Length; t++) {
                    Type type = types[t];
                    if (type == null) {
                        continue;
                    }

                    DocAttribute? doc = type.GetCustomAttribute<DocAttribute>(false);
                    if (doc == null || string.IsNullOrEmpty(doc.Id)) {
                        continue;
                    }

                    map[doc.Id] = type;
                }
            }

            primitiveIndex = map;
        }
    }

    internal static PlaygroundDocs Build(Type primitive) {
        DocAttribute? root = primitive.GetCustomAttribute<DocAttribute>();
        if (root == null) {
            return new PlaygroundDocs();
        }

        IReadOnlyList<CompositionLine> composition = BuildComposition(primitive);
        IReadOnlyList<ApiParam> api = BuildApi(primitive);
        string? usage = BuildUsageCode(primitive);

        return new PlaygroundDocs(
            UsageCode: usage,
            Composition: composition.Count > 0 ? composition : null,
            ApiReference: api.Count > 0 ? api : null,
            ShowRtl: root.ShowRtl
        );
    }

    internal static IReadOnlyList<PlaygroundVariant> BuildVariants(Type primitive, PlaygroundDemoContext ctx) {
        return BuildSamples<DocVariantAttribute, PlaygroundVariant>(
            primitive,
            ctx,
            (attr, sample) => new PlaygroundVariant(attr.LabelKey, sample.Demo, sample.Code),
            attr => attr.Order
        );
    }

    internal static IReadOnlyList<PlaygroundState> BuildStates(Type primitive, PlaygroundDemoContext ctx) {
        return BuildSamples<DocStateAttribute, PlaygroundState>(
            primitive,
            ctx,
            (attr, sample) => new PlaygroundState(attr.LabelKey, sample.Demo, sample.Code),
            attr => attr.Order
        );
    }

    private static IReadOnlyList<TItem> BuildSamples<TAttr, TItem>(
        Type primitive,
        PlaygroundDemoContext ctx,
        Func<TAttr, DocSample, TItem> map,
        Func<TAttr, int> orderOf
    ) where TAttr : Attribute {
        List<(TAttr attr, MemberInfo member)> sources = new List<(TAttr, MemberInfo)>();
        MethodInfo[] methods = primitive.GetMethods(MemberFlags);
        for (int i = 0; i < methods.Length; i++) {
            TAttr? attr = methods[i].GetCustomAttribute<TAttr>();
            if (attr == null) {
                continue;
            }

            sources.Add((attr, methods[i]));
        }

        FieldInfo[] fields = primitive.GetFields(MemberFlags);
        for (int i = 0; i < fields.Length; i++) {
            TAttr? attr = fields[i].GetCustomAttribute<TAttr>();
            if (attr == null) {
                continue;
            }

            sources.Add((attr, fields[i]));
        }

        PropertyInfo[] props = primitive.GetProperties(MemberFlags);
        for (int i = 0; i < props.Length; i++) {
            TAttr? attr = props[i].GetCustomAttribute<TAttr>();
            if (attr == null) {
                continue;
            }

            sources.Add((attr, props[i]));
        }

        if (sources.Count == 0) {
            return Array.Empty<TItem>();
        }

        sources.Sort((a, b) => orderOf(a.attr).CompareTo(orderOf(b.attr)));

        List<TItem> result = new List<TItem>(sources.Count);
        using (PlaygroundDemoContext.Push(ctx)) {
            for (int i = 0; i < sources.Count; i++) {
                DocSample? sample = InvokeForSample(sources[i].member);
                if (sample == null) {
                    continue;
                }

                result.Add(map(sources[i].attr, sample));
            }
        }

        return result;
    }

    private static DocSample? InvokeForSample(MemberInfo member) {
        try {
            object? value;
            if (member is MethodInfo m) {
                value = m.Invoke(null, null);
            } else if (member is FieldInfo f) {
                value = f.GetValue(null);
            } else if (member is PropertyInfo p) {
                value = p.GetValue(null);
            } else {
                return null;
            }

            return value as DocSample;
        } catch (TargetInvocationException ex) {
            Log.Error($"[Lightweave] DocSample provider {member.DeclaringType?.Name}.{member.Name} threw: {ex.InnerException ?? ex}");
            return null;
        } catch (Exception ex) {
            Log.Error($"[Lightweave] Failed to read DocSample from {member.DeclaringType?.Name}.{member.Name}: {ex}");
            return null;
        }
    }

    private static IReadOnlyList<CompositionLine> BuildComposition(Type primitive) {
        string rootName = primitive.Name;
        List<CompositionLine> lines = new List<CompositionLine> {
            new CompositionLine(0, rootName),
        };

        Dictionary<string, List<MethodInfo>> children = new Dictionary<string, List<MethodInfo>>(StringComparer.Ordinal);
        bool hasAnySlot = false;
        MethodInfo[] methods = primitive.GetMethods(MemberFlags);
        for (int i = 0; i < methods.Length; i++) {
            DocAttribute? d = methods[i].GetCustomAttribute<DocAttribute>();
            if (d == null || !d.Slot) continue;
            hasAnySlot = true;
            string parent = d.ParentSlot ?? "";
            if (!children.TryGetValue(parent, out List<MethodInfo>? list)) {
                list = new List<MethodInfo>();
                children[parent] = list;
            }
            list.Add(methods[i]);
        }

        if (!hasAnySlot) {
            return Array.Empty<CompositionLine>();
        }

        AppendChildren(rootName, "", 1, children, lines);
        return lines;
    }

    private static void AppendChildren(
        string rootName,
        string parentName,
        int indent,
        Dictionary<string, List<MethodInfo>> children,
        List<CompositionLine> lines
    ) {
        if (!children.TryGetValue(parentName, out List<MethodInfo>? kids)) return;
        for (int i = 0; i < kids.Count; i++) {
            string name = kids[i].Name;
            lines.Add(new CompositionLine(indent, $"{rootName}.{name}"));
            AppendChildren(rootName, name, indent + 1, children, lines);
        }
    }

    private static IReadOnlyList<ApiParam> BuildApi(Type primitive) {
        List<ApiParam> api = new List<ApiParam>();
        HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
        MethodInfo[] methods = primitive.GetMethods(MemberFlags);
        for (int m = 0; m < methods.Length; m++) {
            MethodInfo method = methods[m];
            DocAttribute? d = method.GetCustomAttribute<DocAttribute>();
            if (d != null && d.Slot) continue;

            ParameterInfo[] parms = method.GetParameters();
            for (int p = 0; p < parms.Length; p++) {
                ParameterInfo pi = parms[p];
                DocParamAttribute? pa = pi.GetCustomAttribute<DocParamAttribute>();
                if (pa == null) continue;
                string paramName = pi.Name ?? "";
                if (seen.Contains(paramName)) continue;
                seen.Add(paramName);

                string typeStr = pa.TypeOverride.Length > 0 ? pa.TypeOverride : FormatType(pi);
                string defaultStr;
                if (pa.DefaultOverride.Length > 0) {
                    defaultStr = pa.DefaultOverride;
                } else if (pi.HasDefaultValue) {
                    defaultStr = FormatDefault(pi.DefaultValue);
                } else {
                    defaultStr = "-";
                }

                api.Add(new ApiParam(paramName, typeStr, defaultStr, pa.Description));
            }
        }

        return api;
    }

    private static string FormatType(ParameterInfo pi) {
        Type t = pi.ParameterType;
        if (t.IsArray) {
            string inner = t.GetElementType()?.Name ?? "object";
            return $"{inner}[]";
        }

        return t.Name;
    }

    private static string FormatDefault(object? v) {
        if (v == null) return "null";
        if (v is string s) return $"\"{s}\"";
        if (v is bool b) return b ? "true" : "false";
        return v.ToString() ?? "";
    }

    private static string? BuildUsageCode(Type primitive) {
        MethodInfo[] methods = primitive.GetMethods(MemberFlags);
        for (int i = 0; i < methods.Length; i++) {
            if (methods[i].GetCustomAttribute<DocUsageAttribute>() == null) continue;
            try {
                object? result = methods[i].Invoke(null, null);
                if (result is DocSample sample) return sample.Code;
            } catch (TargetInvocationException ex) {
                Log.Error($"[Lightweave] DocUsage provider {primitive.Name}.{methods[i].Name} threw: {ex.InnerException ?? ex}");
            } catch (Exception ex) {
                Log.Error($"[Lightweave] Failed to read DocUsage from {primitive.Name}.{methods[i].Name}: {ex}");
            }
        }

        FieldInfo[] fields = primitive.GetFields(MemberFlags);
        for (int i = 0; i < fields.Length; i++) {
            if (fields[i].GetCustomAttribute<DocUsageAttribute>() == null) continue;
            try {
                object? value = fields[i].GetValue(null);
                if (value is DocSample sample) return sample.Code;
            } catch (Exception ex) {
                Log.Error($"[Lightweave] Failed to read DocUsage field {primitive.Name}.{fields[i].Name}: {ex}");
            }
        }

        PropertyInfo[] props = primitive.GetProperties(MemberFlags);
        for (int i = 0; i < props.Length; i++) {
            if (props[i].GetCustomAttribute<DocUsageAttribute>() == null) continue;
            object? value = props[i].GetValue(null);
            if (value is DocSample sample) return sample.Code;
        }

        return null;
    }
}
