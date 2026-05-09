using System;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Adapter;

/// <summary>
///     Adapter that paints a Lightweave tree inside a vanilla inspect tab body.
///     The adapter routes layout and paint through <see cref="LightweaveRoot" /> but does NOT reimplement
///     vanilla <see cref="ITab" /> affordances: tab title bar rendering (drawn externally by
///     <c>InspectTabBase.DoTabGUI</c> and <c>TabDrawer</c> - this adapter does not skip or double-paint
///     it), close-button hit testing (handled by <c>DoTabGUI</c> before <c>FillTab</c> runs), and
///     <c>TutorSystem</c> / UIHighlighter gating. The entity id falls back to <c>0</c> when
///     <c>SelThing</c> is null, which is safe because <c>FillTab</c> only runs when the inspect pane
///     has a valid selection.
/// </summary>
public abstract class AsInspectTab : ITab {
    protected abstract LightweaveNode Build();

    protected override void FillTab() {
        int entityId = SelThing?.thingIDNumber ?? 0;
        Guid id = AdapterStoreRegistry.GetOrCreate(entityId, AdapterKind.InspectTab);
        float titleBarPx = TitleBarHeight;
        float insetPx = SpacingScale.Xs.ToPixels();
        Rect rect = new Rect(0f, titleBarPx, size.x, size.y - titleBarPx).ContractedBy(insetPx);
        LightweaveRoot.Render(rect, id, Build);
    }

    private const float TitleBarHeight = 20f;
}