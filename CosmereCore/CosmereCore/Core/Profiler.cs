using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Cosmere.Core.Attribute;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Cosmere.Core;

[StaticConstructorOnStartup]
public static class Profiler {
    // Flush every N seconds (wall time) to avoid spam (only when aggregate=true)
    private const int FlushSeconds = 5;
    private static readonly Dictionary<MethodBase, Profile> Attrs = new Dictionary<MethodBase, Profile>();
    private static readonly Dictionary<MethodBase, string> Labels = new Dictionary<MethodBase, string>();
    private static readonly ConcurrentDictionary<string, Agg> ScopeAggs = new ConcurrentDictionary<string, Agg>();

    // Instrumentation stopwatches (per-call)
    private static readonly ConcurrentDictionary<(int threadId, MethodBase method), ConcurrentStack<Stopwatch>>
        InstStacks
            = new ConcurrentDictionary<(int threadId, MethodBase method), ConcurrentStack<Stopwatch>>();

    // Sampling state: track if this invocation was sampled so EndProfiling knows what to do.
    private static readonly ThreadLocal<Stack<SampleToken>> SampleStack =
        new ThreadLocal<Stack<SampleToken>>(() => new Stack<SampleToken>());

    // Aggregation buckets (ticks are long; keep totals big)
    private static readonly ConcurrentDictionary<MethodBase, Agg> Aggs = new ConcurrentDictionary<MethodBase, Agg>();

    private static readonly bool Initialized;

    static Profiler() {
        if (!Mod.debugMode) return;

        try {
            Harmony harmony = new Harmony("Cosmere.Profiler");

            foreach (MethodInfo method in profiledMethods) {
                Profile attr = method.GetCustomAttribute<Profile>()!;
                string label = string.IsNullOrEmpty(attr.Label) ? method.Name : attr.Label!;
                Labels[method] = label;
                Attrs[method] = attr;

                harmony.Patch(
                    method,
                    new HarmonyMethod(typeof(Profiler), nameof(StartProfiling)),
                    new HarmonyMethod(typeof(Profiler), nameof(EndProfiling)),
                    finalizer: new HarmonyMethod(typeof(Profiler), nameof(CleanupProfiling))
                );
            }

            Initialized = true;
        } catch (Exception ex) {
            Logger.Error($"Profiler initialization failed: {ex}");
        }
    }

    private static IEnumerable<MethodInfo> profiledMethods =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && a.FullName.StartsWith("Cosmere"))
            .SelectMany(a => SafeGetTypes(a))
            .SelectMany(t => SafeGetMethods(t))
            .Where(m =>
                !m.IsAbstract &&
                !m.ContainsGenericParameters &&
                m.HasAttribute<Profile>() &&
                m.IsDeclaredMember()
            )
            .Distinct(new MethodComparer());

    private static IEnumerable<Type> SafeGetTypes(Assembly a) {
        try {
            return a.GetTypes();
        } catch (Exception ex) {
            Logger.Verbose($"Failed to get types from assembly {a.FullName}: {ex.Message}");
            return [];
        }
    }

    private static IEnumerable<MethodInfo> SafeGetMethods(Type t) {
        try {
            return t.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static
            );
        } catch (Exception ex) {
            Logger.Verbose($"Failed to get methods from type {t.FullName}: {ex.Message}");
            return [];
        }
    }

    public static void StartProfiling(MethodBase __originalMethod) {
        if (!Attrs.TryGetValue(__originalMethod, out Profile? attr)) return;

        if (attr.Mode == ProfileMode.Instrumentation) {
            (int threadId, MethodBase method) key = (Environment.CurrentManagedThreadId, __originalMethod);
            ConcurrentStack<Stopwatch>? stack = InstStacks.GetOrAdd(key, _ => new ConcurrentStack<Stopwatch>());
            Stopwatch sw = Stopwatch.StartNew();
            stack.Push(sw);
            return;
        }

        // Sampling
        if (attr.Mode == ProfileMode.Sampling) {
            bool sampled = Rand.Value < Mathf.Clamp01(attr.SampleProbability <= 0f ? 0.01f : attr.SampleProbability);
            Stopwatch? sw = null;
            if (sampled) {
                sw = Stopwatch.StartNew();
            }

            SampleStack.Value!.Push(new SampleToken(sampled, sw));
        }
    }

    public static void EndProfiling(MethodBase __originalMethod) {
        if (!Attrs.TryGetValue(__originalMethod, out Profile? attr)) return;
        string label = Labels.TryGetValue(__originalMethod, out string? l) ? l : __originalMethod.Name;

        if (attr.Mode == ProfileMode.Instrumentation) {
            (int threadId, MethodBase method) key = (Environment.CurrentManagedThreadId, __originalMethod);
            if (!InstStacks.TryGetValue(key, out ConcurrentStack<Stopwatch>? stack) || stack.Count == 0) return;

            if (!stack.TryPop(out Stopwatch sw)) return;
            sw.Stop();

            if (attr.Aggregate) {
                Agg? agg = Aggs.GetOrAdd(__originalMethod, _ => new Agg(label, attr.Category, attr.Mode));
                agg.Add(sw.ElapsedTicks, 1, 1.0f);
            } else {
                Logger.Profile(label, sw.ElapsedTicks);
            }

            return;
        }

        // Sampling
        if (attr.Mode == ProfileMode.Sampling) {
            Stack<SampleToken>? stack = SampleStack.Value!;
            if (stack.Count == 0) return;
            SampleToken token = stack.Pop();

            if (token.sampled && token.sw != null) {
                token.sw.Stop();

                // Scale by 1/p to get unbiased estimate
                float p = Mathf.Clamp(attr.SampleProbability <= 0f ? 0.01f : attr.SampleProbability, 0.000001f, 1f);
                double weight = 1.0 / p;

                if (attr.Aggregate) {
                    Agg? agg = Aggs.GetOrAdd(__originalMethod, _ => new Agg(label, attr.Category, attr.Mode));
                    agg.Add(token.sw.ElapsedTicks, 1, weight);
                } else {
                    long estTicks = (long)(token.sw.ElapsedTicks * weight);
                    Logger.Profile($"[SAMP] {label}", estTicks);
                }
            }
        }
    }

    // Finalizer — always runs, even on exceptions. Clean up per-call state.
    public static void CleanupProfiling(MethodBase __originalMethod) {
        // For instrumentation, if an exception bypassed EndProfiling, pop & drop
        (int threadId, MethodBase method) key = (Environment.CurrentManagedThreadId, __originalMethod);
        if (InstStacks.TryGetValue(key, out ConcurrentStack<Stopwatch>? stack) && stack.Count > 0) {
            stack.TryPop(out _);

            // we could log as "faulted" if desired; for now just discard
        }

        // For sampling, unwind if needed (in case EndProfiling wasn’t hit)
        Stack<SampleToken>? sstack = SampleStack.Value!;
        if (sstack.Count > 0) {
            SampleToken token = sstack.Pop();

            // discard on exception path
        }
    }

    private static void Flush() {
        // Existing method aggregates
        foreach (KeyValuePair<MethodBase, Agg> kvp in Aggs) {
            Agg? agg = kvp.Value;
            if (!agg.TryConsume(out Agg.Snapshot s)) continue;
            EmitSnapshot(s);
        }

        // Scope aggregates
        foreach (KeyValuePair<string, Agg> kvp in ScopeAggs) {
            Agg? agg = kvp.Value;
            if (!agg.TryConsume(out Agg.Snapshot s)) continue;
            EmitSnapshot(s);
        }
    }

    private static void EmitSnapshot(Agg.Snapshot snapshot) {
        double tickToMs = 1000.0 / Stopwatch.Frequency;
        double totalMs = snapshot.totalTicks * tickToMs;
        double meanMs = snapshot.totalTicks / Math.Max(1.0, snapshot.samples) * tickToMs;

        string mode = snapshot.mode == ProfileMode.Sampling ? "SAMP" : "INST";
        string cat = string.IsNullOrEmpty(snapshot.category) ? string.Empty : $" [{snapshot.category}]";
        Logger.Verbose(
            $"{mode}{cat} {snapshot.label}: calls={snapshot.calls}, samples={snapshot.samples}, mean≈{meanMs:F3}ms, total≈{totalMs:F1}ms, min≈{snapshot.minTicks * tickToMs:F3}ms, max≈{snapshot.maxTicks * tickToMs:F3}ms"
        );
    }

    /// <summary>
    ///     Begin a disposable profiling scope. Use with: using var _ = Profiler.Scope("Label", ...).
    /// </summary>
    /// <returns></returns>
    public static ScopeTimer Scope(
        string label,
        ProfileMode mode = ProfileMode.Instrumentation,
        float sampleProbability = 0.01f,
        string? category = null,
        bool aggregate = true
    ) {
        if (!Initialized) return default;
        return new ScopeTimer(label, mode, sampleProbability, category, aggregate);
    }

#pragma warning disable CS9113
    private sealed class ProfilerFlushComponent(Game _) : GameComponent {
        public override void GameComponentUpdate() {
            if ((int)Time.time % FlushSeconds == 0) Flush();
        }
#pragma warning restore CS9113
    }

    private readonly struct SampleToken(bool sampled, Stopwatch? sw) {
        public bool sampled { get; } = sampled;

        public Stopwatch? sw { get; } = sw;
    }

    private sealed class Agg(string label, string? category, ProfileMode mode) {
        private long calls; // total invocations (estimated == actual call count only in instrumentation)
        private long maxTicks = long.MinValue;

        private long minTicks = long.MaxValue;

        private long pending; // simple dirty flag
        private long samples; // number of measured samples (== calls in instrumentation; ≈ p*calls in sampling)
        private long totalTicks; // scaled (for sampling) or raw (instrumentation)

        public void Add(long ticksMeasured, long addedCalls, double weight) {
            // weight==1 for instrumentation; weight==1/p for sampling
            long scaled = (long)Math.Round(ticksMeasured * weight);

            Interlocked.Add(ref totalTicks, scaled);
            Interlocked.Add(ref calls, addedCalls);
            Interlocked.Increment(ref samples);
            Interlocked.Exchange(ref pending, 1);

            // atomic min
            long current;
            while (true) {
                current = Volatile.Read(ref minTicks);
                long newVal = scaled < current ? scaled : current;
                if (newVal == current) break;
                if (Interlocked.CompareExchange(ref minTicks, newVal, current) == current) break;
            }

            // atomic max
            while (true) {
                current = Volatile.Read(ref maxTicks);
                long newVal = scaled > current ? scaled : current;
                if (newVal == current) break;
                if (Interlocked.CompareExchange(ref maxTicks, newVal, current) == current) break;
            }
        }

        public bool TryConsume(out Snapshot snap) {
            if (Interlocked.Exchange(ref pending, 0) == 0) {
                snap = default;
                return false;
            }

            snap = new Snapshot {
                label = label,
                category = category,
                mode = mode,
                totalTicks = Interlocked.Read(ref totalTicks),
                minTicks = Volatile.Read(ref minTicks) == long.MaxValue ? 0 : Volatile.Read(ref minTicks),
                maxTicks = Volatile.Read(ref maxTicks) == long.MinValue ? 0 : Volatile.Read(ref maxTicks),
                calls = Interlocked.Read(ref calls),
                samples = Interlocked.Read(ref samples),
            };
            return true;
        }

        public struct Snapshot {
            public string label;
            public string? category;
            public ProfileMode mode;
            public long totalTicks;
            public long minTicks;
            public long maxTicks;
            public long calls;
            public long samples;
        }
    }

    public readonly struct ScopeTimer : IDisposable {
        private readonly string label;
        private readonly string? category;
        private readonly ProfileMode mode;
        private readonly bool aggregate;
        private readonly float p; // sampling probability (0,1]
        private readonly bool sampled;
        private readonly Stopwatch? sw;

        internal ScopeTimer(string label, ProfileMode mode, float p, string? category, bool aggregate) {
            this.label = string.IsNullOrEmpty(label) ? "Scope" : label;
            this.category = category;
            this.mode = mode;
            this.aggregate = aggregate;
            this.p = Mathf.Clamp(p <= 0f ? 0.01f : p, 0.000001f, 1f);

            if (mode == ProfileMode.Instrumentation) {
                sampled = true; // always measure in instrumentation mode
                sw = Stopwatch.StartNew();
            } else {
                sampled = Rand.Value < this.p; // Bernoulli
                sw = sampled ? Stopwatch.StartNew() : null;
            }
        }

        public void Dispose() {
            if (!sampled || sw == null) return;

            sw.Stop();
            ProfileMode localMode = mode;
            double weight = localMode == ProfileMode.Sampling ? 1.0 / p : 1.0;
            long scaledTicks = (long)Math.Round(sw.ElapsedTicks * weight);

            if (aggregate) {
                string? localLabel = label;
                string? localCategory = category;

                Agg? agg = ScopeAggs.GetOrAdd(label, _ => new Agg(localLabel, localCategory, localMode));
                agg.Add(scaledTicks, 1, 1.0); // already scaled above
            } else {
                double ms = sw.ElapsedTicks * (1000.0 / Stopwatch.Frequency) * weight;
                string modeString = mode == ProfileMode.Sampling ? "SAMP" : "INST";
                string cat = string.IsNullOrEmpty(category) ? string.Empty : $" [{category}]";
                Logger.Verbose($"{modeString}{cat} {label}: {ms:F3}ms");
            }
        }
    }
}

internal class MethodComparer : IEqualityComparer<MethodInfo> {
    public bool Equals(MethodInfo? x, MethodInfo? y) {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.GetType() != y.GetType()) return false;

        // Include parameter types so overloads are distinct
        if (x.Name != y.Name) return false;
        if (x.DeclaringType != y.DeclaringType) return false;
        if (x.ReturnType != y.ReturnType) return false;

        ParameterInfo[] xp = x.GetParameters();
        ParameterInfo[] yp = y.GetParameters();
        if (xp.Length != yp.Length) return false;

        for (int i = 0; i < xp.Length; i++) {
            if (xp[i].ParameterType != yp[i].ParameterType) return false;
        }

        return true;
    }

    public int GetHashCode(MethodInfo obj) {
        int h = HashCode.Combine(obj.Name, obj.DeclaringType, obj.ReturnType);
        ParameterInfo[] parameters = obj.GetParameters();
        for (int i = 0; i < parameters.Length; i++) {
            h = HashCode.Combine(h, parameters[i].ParameterType);
        }

        return h;
    }
}
