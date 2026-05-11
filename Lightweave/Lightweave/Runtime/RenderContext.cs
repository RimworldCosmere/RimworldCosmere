using System;
using System.Threading;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Runtime.Internal;
using Cosmere.Lightweave.Theme;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Runtime;

public sealed class RenderContext {
    private static readonly ThreadLocal<RenderContext?> current = new ThreadLocal<RenderContext?>();

    private readonly List<HotkeyBinding> pendingHotkeys = new List<HotkeyBinding>();
    private readonly Stack<int> pathHashStack = new Stack<int>();
    public Stack<object> ContextValues = new Stack<object>();
    public Stack<Direction> DirectionStack = new Stack<Direction>();
    public Breakpoint Breakpoint = Breakpoint.Xs;
    public int? FocusedNodeId;
    public readonly HookStore Hooks;
    public int? HoveredNodeId;
    public int ParentPathHash;
    public Vector2 PointerPos;
    public Rect RootRect;
    public Stack<Theme.Theme> ThemeStack = new Stack<Theme.Theme>();
    public Stack<Rect> PositioningAncestorStack = new Stack<Rect>();
    public bool ForceDisabled;
    internal OverlayQueue PendingOverlays { get; } = new OverlayQueue();
    public string? FocusedControlName { get; internal set; }
    public Guid RootId { get; internal set; }

    public RenderContext(HookStore hooks) {
        Hooks = hooks ?? throw new ArgumentNullException(nameof(hooks));
    }

    public static RenderContext Current =>
        current.Value ?? throw new InvalidOperationException("No RenderContext active");

    public static RenderContext? CurrentOrNull => current.Value;

    public Theme.Theme Theme => ThemeStack.Count == 0
        ? throw new InvalidOperationException("No theme in stack")
        : ThemeStack.Peek();

    public Direction Direction => DirectionStack.Count == 0 ? Direction.Ltr : DirectionStack.Peek();

    public static void Push(RenderContext ctx) {
        current.Value = ctx;
    }

    public static void Clear() {
        current.Value = null;
    }

    public void PushPathSalt(int salt) {
        pathHashStack.Push(ParentPathHash);
        ParentPathHash = unchecked(ParentPathHash * 31 + salt);
    }

    public void PopPathSalt() {
        if (pathHashStack.Count == 0) {
            throw new InvalidOperationException("PopPathSalt without matching PushPathSalt");
        }

        ParentPathHash = pathHashStack.Pop();
    }

    public void RegisterHotkey(HotkeyBinding binding) {
        pendingHotkeys.Add(binding);
    }

    public void FlushHotkeys() {
        Event e = Event.current;
        if (e == null || e.type != EventType.KeyDown) {
            pendingHotkeys.Clear();
            return;
        }

        for (int i = 0; i < pendingHotkeys.Count; i++) {
            HotkeyBinding binding = pendingHotkeys[i];
            if (e.keyCode != binding.Code) {
                continue;
            }

            bool ctrlMatch = (binding.Modifiers & KeyModifiers.Control) == 0 || e.control || e.command;
            bool shiftMatch = (binding.Modifiers & KeyModifiers.Shift) == 0 || e.shift;
            bool altMatch = (binding.Modifiers & KeyModifiers.Alt) == 0 || e.alt;

            if (ctrlMatch && shiftMatch && altMatch) {
                binding.Handler.Invoke();
                e.Use();
                break;
            }
        }

        pendingHotkeys.Clear();
    }
}