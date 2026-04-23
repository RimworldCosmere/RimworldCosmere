using System;
using Cosmere.Core.UI.Lightweave.Runtime;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Adapter;

/// <summary>
/// Adapter that paints a Lightweave tree inside a vanilla FloatMenuOption row rect.
/// The adapter routes layout and paint through <see cref="LightweaveRoot"/> but does NOT reimplement
/// vanilla <see cref="FloatMenuOption.DoGUI"/> affordances: default row painting (icon, label, and
/// extraPart composition), <c>mouseoverGuiAction</c> (not called - consumers must handle hover
/// feedback inside their Build closure), hover highlight and selection sound (vanilla draws these;
/// this override skips them entirely), and <c>revalidateClickTarget</c> (still honored if the parent
/// FloatMenu calls back, but disabled state must be handled by the caller). The <c>subKey</c>
/// parameter disambiguates multiple bespoke options on the same entity in a single frame - use a
/// distinct value per option so each gets a stable, unique Guid from <see cref="AdapterStoreRegistry"/>.
/// </summary>
public sealed class AsFloatMenuOption : FloatMenuOption
{
    private readonly int entityId;
    private readonly int subKey;
    private readonly Func<LightweaveNode> build;

    public AsFloatMenuOption(int entityId, Action? action, Func<LightweaveNode> build, int subKey = 0)
        : base(string.Empty, action)
    {
        this.entityId = entityId;
        this.subKey = subKey;
        this.build = build;
    }

    public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu parent)
    {
        Guid id = AdapterStoreRegistry.Get(entityId, AdapterKind.FloatMenu, subKey);
        LightweaveRoot.Render(rect, id, build);

        Event evt = Event.current;
        if (evt == null || evt.type == EventType.Used)
        {
            return false;
        }

        if (Mouse.IsOver(rect) && evt.type == EventType.MouseUp && evt.button == 0)
        {
            action?.Invoke();
            evt.Use();
            return true;
        }

        return false;
    }
}
