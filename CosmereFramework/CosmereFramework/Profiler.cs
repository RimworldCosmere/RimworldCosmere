using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CosmereFramework.Attribute;
using HarmonyLib;
using Verse;

namespace CosmereFramework;

[StaticConstructorOnStartup]
public static class Profiler {
    private static readonly Dictionary<MethodBase, string> Labels = new Dictionary<MethodBase, string>();
    private static readonly Dictionary<MethodBase, Stopwatch> Stopwatches = new Dictionary<MethodBase, Stopwatch>();

    static Profiler() {
        Harmony harmony = new Harmony("Cryptiklemur.Cosmere.Framework.Profiler");
        List<MethodInfo> methods = ProfiledMethods.ToList();
        foreach (MethodInfo method in methods) {
            Profile? attr = method.GetCustomAttribute<Profile>();
            Logger.Verbose("Profiling Method: " + method.DeclaringType!.FullName + "." + method.Name);
            try {
                string methodName = (string.IsNullOrEmpty(attr.label) ? method.Name : attr.label) ??
                                    "Anonymous Method";

                harmony.Patch(
                    method,
                    new HarmonyMethod(typeof(Profiler), nameof(StartProfiling)),
                    new HarmonyMethod(typeof(Profiler), nameof(EndProfiling)),
                    finalizer: new HarmonyMethod(typeof(Profiler), nameof(CleanupProfiling))
                );

                Labels[method] = methodName;
            } catch (Exception ex) {
                Log.Warning($"[Profile] Failed to patch {method.DeclaringType?.FullName}.{method.Name}: {ex}");
            }
        }
    }

    private static IEnumerable<MethodInfo> ProfiledMethods =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic)
            .SelectMany(x => x.GetTypes())
            .SelectMany(type => type.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static
                )
            )
            .Where(m =>
                !m.IsAbstract &&
                !m.ContainsGenericParameters &&
                m.HasAttribute<Profile>() &&
                m.IsDeclaredMember()
            )
            .Distinct(new MethodComparer());

    public static void StartProfiling(MethodBase __originalMethod) {
        Stopwatch sw = Stopwatch.StartNew();
        Stopwatches[__originalMethod] = sw;
    }

    public static void EndProfiling(MethodBase __originalMethod) {
        if (Stopwatches.TryGetValue(__originalMethod, out Stopwatch? sw)) {
            sw.Stop();
            string label = Labels.TryGetValue(__originalMethod, out string? l) ? l : __originalMethod.Name;
            Logger.Profile(label, sw.ElapsedTicks);
        }
    }

    public static void CleanupProfiling(MethodBase __originalMethod) {
        Stopwatches.Remove(__originalMethod);
    }
}

internal class MethodComparer : IEqualityComparer<MethodInfo> {
    public bool Equals(MethodInfo? x, MethodInfo? y) {
        if (ReferenceEquals(x, y)) {
            return true;
        }

        if (x is null) {
            return false;
        }

        if (y is null) {
            return false;
        }

        if (x.GetType() != y.GetType()) {
            return false;
        }

        return x.Name == y.Name && Equals(x.DeclaringType, y.DeclaringType) && x.ReturnType.Equals(y.ReturnType);
    }

    public int GetHashCode(MethodInfo obj) {
        return HashCode.Combine(obj.Name, obj.DeclaringType, obj.ReturnType);
    }
}