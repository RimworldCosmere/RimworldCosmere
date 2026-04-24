using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;
using Cosmere.Core.UI.Skin;

namespace Cosmere.Core.UI.Lightweave.Hooks;

public static class Hooks {
    public static StateHandle<T> UseState<T>(
        T initial,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        HookKey key = Key(line, file);
        HookSlot slot = RenderContext.Current.Hooks.Acquire(key);
        slot.Value ??= initial;
        return new StateHandle<T>(slot);
    }

    public static RefHandle<T> UseRef<T>(
        T initial,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        HookKey key = Key(line, file);
        HookSlot slot = RenderContext.Current.Hooks.Acquire(key);
        slot.Value ??= initial;
        return new RefHandle<T>(slot);
    }

    public static T UseMemo<T>(
        Func<T> compute,
        object[] deps,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        HookKey key = Key(line, file);
        HookSlot slot = RenderContext.Current.Hooks.Acquire(key);
        MemoBox<T> box = (MemoBox<T>)(slot.Value ??= new MemoBox<T>());
        if (box.Deps == null || !DepsEqual(box.Deps, deps)) {
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

    public static ISystemSkin? UseSystemSkin() {
        return RenderContext.Current.Skin;
    }

    public static Direction UseDirection() {
        return RenderContext.Current.Direction;
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

    public sealed class StateHandle<T> {
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

    public sealed class RefHandle<T> {
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
        public object[]? Deps;
        public T Value = default!;
    }

    private sealed class EffectBox {
        public Action? Cleanup;
        public object[]? Deps;
    }
}