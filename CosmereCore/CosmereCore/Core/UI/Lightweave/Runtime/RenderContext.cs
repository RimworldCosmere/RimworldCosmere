using System;
using System.Collections.Generic;
using UnityEngine;
using Cosmere.Core.UI.Skin;
using Cosmere.Core.UI.Lightweave.Runtime.Internal;
using Cosmere.Core.UI.Lightweave.Theme;
using Cosmere.Core.UI.Lightweave.Types;
using Cosmere.Core.UI.Lightweave.Hooks;

namespace Cosmere.Core.UI.Lightweave.Runtime;

public sealed class RenderContext
{
    public int ParentPathHash;
    public Stack<Theme.Theme> ThemeStack = new Stack<Theme.Theme>();
    public Stack<ISystemSkin?> SkinStack = new Stack<ISystemSkin?>();
    public Stack<Direction> DirectionStack = new Stack<Direction>();
    public Stack<object> ContextValues = new Stack<object>();
    public Vector2 PointerPos;
    public int? HoveredNodeId;
    public int? FocusedNodeId;
    internal OverlayQueue PendingOverlays { get; } = new OverlayQueue();
    public string? FocusedControlName { get; internal set; }
    public HookStore Hooks = null!;
    public Guid RootId { get; internal set; }

    private readonly List<HotkeyBinding> pendingHotkeys = new List<HotkeyBinding>();

    private static readonly global::System.Threading.ThreadLocal<RenderContext?> current = new global::System.Threading.ThreadLocal<RenderContext?>();
    public static RenderContext Current => current.Value ?? throw new global::System.InvalidOperationException("No RenderContext active");
    public static RenderContext? CurrentOrNull => current.Value;
    public static void Push(RenderContext ctx) => current.Value = ctx;
    public static void Clear() => current.Value = null;

    public Theme.Theme Theme => ThemeStack.Count == 0 ? throw new global::System.InvalidOperationException("No theme in stack") : ThemeStack.Peek();
    public Direction Direction => DirectionStack.Count == 0 ? Direction.Ltr : DirectionStack.Peek();
    public ISystemSkin? Skin => SkinStack.Count == 0 ? null : SkinStack.Peek();

    public void RegisterHotkey(HotkeyBinding binding)
    {
        pendingHotkeys.Add(binding);
    }

    public void FlushHotkeys()
    {
        Event e = Event.current;
        if (e == null || e.type != EventType.KeyDown)
        {
            pendingHotkeys.Clear();
            return;
        }

        for (int i = 0; i < pendingHotkeys.Count; i++)
        {
            HotkeyBinding binding = pendingHotkeys[i];
            if (e.keyCode != binding.Code)
            {
                continue;
            }

            bool ctrlMatch = (binding.Modifiers & KeyModifiers.Control) == 0 || (e.control || e.command);
            bool shiftMatch = (binding.Modifiers & KeyModifiers.Shift) == 0 || e.shift;
            bool altMatch = (binding.Modifiers & KeyModifiers.Alt) == 0 || e.alt;

            if (ctrlMatch && shiftMatch && altMatch)
            {
                binding.Handler.Invoke();
                e.Use();
                break;
            }
        }

        pendingHotkeys.Clear();
    }
}
