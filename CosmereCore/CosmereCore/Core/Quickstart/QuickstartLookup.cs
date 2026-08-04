using System;

namespace Cosmere.Core.Quickstart;

/// <summary>
///     Matches a quickstart name given on the command line against the loaded quickstart types.
///     Accepts a plain class name, a namespaced name, or the assembly-qualified name the mod
///     settings store.
/// </summary>
public static class QuickstartLookup {
    public static Type? Resolve(string? name, IReadOnlyList<Type> candidates, out string? error) {
        error = null;
        string wanted = name?.Trim() ?? string.Empty;
        if (wanted.Length == 0) {
            error = "no quickstart name was given";
            return null;
        }

        List<Type> byShortName = [];
        for (int i = 0; i < candidates.Count; i++) {
            Type candidate = candidates[i];
            if (Matches(candidate.AssemblyQualifiedName, wanted) || Matches(candidate.FullName, wanted)) {
                return candidate;
            }

            if (Matches(candidate.Name, wanted)) byShortName.Add(candidate);
        }

        if (byShortName.Count == 1) return byShortName[0];

        error = byShortName.Count > 1
            ? $"'{wanted}' matches several quickstarts ({Join(byShortName)}); use the namespaced name"
            : $"no quickstart is called '{wanted}'. Known quickstarts: {Join(candidates)}";

        return null;
    }

    private static bool Matches(string? candidate, string wanted) {
        return candidate != null && string.Equals(candidate, wanted, StringComparison.OrdinalIgnoreCase);
    }

    private static string Join(IReadOnlyList<Type> types) {
        string[] names = new string[types.Count];
        for (int i = 0; i < types.Count; i++) {
            names[i] = types[i].FullName ?? types[i].Name;
        }

        Array.Sort(names, StringComparer.OrdinalIgnoreCase);
        return string.Join(", ", names);
    }
}
