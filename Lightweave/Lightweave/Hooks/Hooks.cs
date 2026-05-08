using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Theme;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;

namespace Cosmere.Lightweave.Hooks;

/// <summary>
/// State and effect hooks for Lightweave components.
/// </summary>
/// <remarks>
/// <para>
/// All <c>Use*</c> hooks must be invoked from inside a component's
/// <see cref="LightweaveNode.Paint"/> callback - never from Create-scope (the body
/// of a <c>Component.Create()</c> method that runs once at tree construction time
/// rather than once per frame).
/// </para>
/// <para>
/// Calling a hook from Create-scope binds the slot to the parent's path hash
/// instead of the component's own, which produces stale state across re-renders
/// and silently breaks identity for sibling instances. The contract is not
/// enforced by the type system today; treat the rule as load-bearing when
/// authoring components and reviewing changes.
/// </para>
/// <para>
/// If you need state to survive across frames at construction time (e.g. an id),
/// compute it from <see cref="RenderContext.Current"/> inside <c>Paint</c> and
/// pass the closed-over value into child nodes.
/// </para>
/// <para>
/// To disambiguate sibling hook calls in the same Paint method, override the
/// <c>file</c> caller-attribute argument with a suffixed string (e.g. <c>file: file + "#expanded"</c>).
/// This is the established convention for any component that calls a Use* hook
/// in Paint and needs to keep multiple slots distinct.
/// </para>
/// </remarks>
public static class Hooks {
    public static StateHandle<T> UseState<T>(
        [DocParam("Initial value used the first time this hook slot is acquired.")]
        T initial,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        HookKey hookKey = Key(line, file);
        HookSlot slot = RenderContext.Current.Hooks.Acquire(hookKey);
        slot.Value ??= initial;
        return new StateHandle<T>(slot);
    }

    public static RefHandle<T> UseRef<T>(
        [DocParam("Initial reference value seeded the first render.")]
        T initial,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        HookKey hookKey = Key(line, file);
        HookSlot slot = RenderContext.Current.Hooks.Acquire(hookKey);
        slot.Value ??= initial;
        return new RefHandle<T>(slot);
    }

    public static T UseMemo<T>(
        [DocParam("Factory invoked when deps change. Result is cached between renders.")]
        Func<T> compute,
        [DocParam("Dependency array. Recomputes when any element fails Equals().")]
        object[] deps,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        HookKey key = Key(line, file);
        HookSlot slot = RenderContext.Current.Hooks.Acquire(key);
        if (slot.Value is not MemoBox<T> box) {
            box = new MemoBox<T>(compute(), (object[])deps.Clone());
            slot.Value = box;
            return box.Value;
        }

        if (!DepsEqual(box.Deps, deps)) {
            box.Value = compute();
            box.Deps = (object[])deps.Clone();
        }

        return box.Value;
    }

    public static void UseEffect(
        Func<Action?> effect,
        object[] deps,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        HookKey key = Key(line, file);
        HookSlot slot = RenderContext.Current.Hooks.Acquire(key);
        EffectBox box = (EffectBox)(slot.Value ??= new EffectBox());
        if (box.Deps == null || !DepsEqual(box.Deps, deps)) {
            box.Cleanup?.Invoke();
            box.Cleanup = effect();
            box.Deps = (object[])deps.Clone();
            slot.Cleanup = () => box.Cleanup?.Invoke();
        }
    }

    public static T UseContext<T>() where T : class {
        foreach (object v in RenderContext.Current.ContextValues) {
            if (v is T t) {
                return t;
            }
        }

        throw new InvalidOperationException($"No {typeof(T).Name} context in scope");
    }

    public static Theme.Theme UseTheme() {
        return RenderContext.Current.Theme;
    }

    public static Direction UseDirection() {
        return RenderContext.Current.Direction;
    }


    public static Breakpoint UseBreakpoint() {
        return RenderContext.Current.Breakpoint;
    }

    private static HookKey Key(int line, string file) {
        int callSiteId = unchecked(file.GetHashCode() * 31 + line);
        int parentHash = RenderContext.Current.ParentPathHash;
        return new HookKey(parentHash, callSiteId, null);
    }

    private static bool DepsEqual(object[] a, object[] b) {
        if (a.Length != b.Length) {
            return false;
        }

        for (int i = 0; i < a.Length; i++) {
            if (!Equals(a[i], b[i])) {
                return false;
            }
        }

        return true;
    }

    public readonly struct StateHandle<T> {
        private readonly HookSlot slot;

        public StateHandle(HookSlot slot) {
            this.slot = slot;
        }

        public T Value {
            get => (T)slot.Value!;
            set => slot.Value = value;
        }

        public void Set(T v) {
            slot.Value = v;
        }
    }

    public readonly struct RefHandle<T> {
        private readonly HookSlot slot;

        public RefHandle(HookSlot slot) {
            this.slot = slot;
        }

        public T Current {
            get => (T)slot.Value!;
            set => slot.Value = value;
        }
    }

    private sealed class MemoBox<T> {
        public T Value;
        public object[] Deps;

        public MemoBox(T value, object[] deps) {
            Value = value;
            Deps = deps;
        }
    }

    private sealed class EffectBox {
        public Action? Cleanup;
        public object[]? Deps;
    }
}