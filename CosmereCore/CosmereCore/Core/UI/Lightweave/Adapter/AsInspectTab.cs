using System;
using Cosmere.Core.Extension;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Lightweave.Runtime;
using RimWorld;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Adapter;

/// <summary>
/// Adapter that paints a Lightweave tree inside a vanilla inspect tab body.
/// The adapter routes layout and paint through <see cref="LightweaveRoot"/> but does NOT reimplement
/// vanilla <see cref="ITab"/> affordances: tab title bar rendering (drawn externally by
/// <c>InspectTabBase.DoTabGUI</c> and <c>TabDrawer</c> - this adapter does not skip or double-paint
/// it), close-button hit testing (handled by <c>DoTabGUI</c> before <c>FillTab</c> runs), and
/// <c>TutorSystem</c> / UIHighlighter gating. The entity id falls back to <c>0</c> when
/// <c>SelThing</c> is null, which is safe because <c>FillTab</c> only runs when the inspect pane
/// has a valid selection.
/// </summary>
public abstract class AsInspectTab : ITab
{
    protected abstract LightweaveNode Build();

    protected override void FillTab()
    {
        int entityId = SelThing?.thingIDNumber ?? 0;
        Guid id = AdapterStoreRegistry.Get(entityId, AdapterKind.InspectTab);
        Rect rect = new Rect(0f, 20f, size.x, size.y - 20f).ContractedBy(new Padding(8f));
        LightweaveRoot.Render(rect, id, Build);
    }
}
