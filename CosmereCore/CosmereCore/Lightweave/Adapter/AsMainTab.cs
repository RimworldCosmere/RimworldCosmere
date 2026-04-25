using System;
using Cosmere.Lightweave.Runtime;
using RimWorld;
using UnityEngine;

namespace Cosmere.Lightweave.Adapter;

/// <summary>
///     Adapter that paints a Lightweave tree inside a vanilla main tab window content area.
///     The adapter overrides <see cref="MainTabWindow.DoWindowContents" /> and routes layout and paint
///     through <see cref="LightweaveRoot" /> but does NOT reimplement vanilla
///     <see cref="MainTabWindow" /> affordances: window chrome (header bar, close button, and tab
///     ribbon drawn by the window system before <c>DoWindowContents</c> runs) and standard
///     <c>Window</c> behaviours (dragging, resize, focus, sound) remain vanilla. Only the content
///     area rect passed to <c>DoWindowContents</c> is Lightweave-rendered. A per-instance
///     <see cref="Guid" /> is used because a <c>MainTabWindow</c> is a singleton owned by its
///     <c>MainButtonDef</c> - there is no entity id to key against. The store is released on
///     <c>PostClose</c> so hook state does not accumulate across open/close cycles.
/// </summary>
public abstract class AsMainTab : MainTabWindow {
    private readonly Guid rootId = Guid.NewGuid();

    protected abstract LightweaveNode Build();

    public override void DoWindowContents(Rect inRect) {
        LightweaveRoot.Render(inRect, rootId, Build);
    }

    public override void PostClose() {
        LightweaveRoot.Release(rootId);
        base.PostClose();
    }
}