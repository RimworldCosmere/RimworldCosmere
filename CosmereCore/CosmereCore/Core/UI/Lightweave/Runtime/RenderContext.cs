using System.Collections.Generic;
using UnityEngine;
using Cosmere.Core.UI.Skin;
using Cosmere.Core.UI.Lightweave.Theme;
using Cosmere.Core.UI.Lightweave.Types;

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
    public List<global::System.Action> DeferredOverlays = new List<global::System.Action>();
    public HookStore Hooks = null!;

    private static readonly global::System.Threading.ThreadLocal<RenderContext?> current = new global::System.Threading.ThreadLocal<RenderContext?>();
    public static RenderContext Current => current.Value ?? throw new global::System.InvalidOperationException("No RenderContext active");
    public static RenderContext? CurrentOrNull => current.Value;
    public static void Push(RenderContext ctx) => current.Value = ctx;
    public static void Clear() => current.Value = null;

    public Theme.Theme Theme => ThemeStack.Count == 0 ? throw new global::System.InvalidOperationException("No theme in stack") : ThemeStack.Peek();
    public Direction Direction => DirectionStack.Count == 0 ? Direction.Ltr : DirectionStack.Peek();
    public ISystemSkin? Skin => SkinStack.Count == 0 ? null : SkinStack.Peek();
}
