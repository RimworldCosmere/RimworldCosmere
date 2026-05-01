using System;
using Cosmere.Lightweave.Runtime;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Adapter;

/// <summary>
///     Adapter that paints a Lightweave tree inside a vanilla FloatMenuOption row rect.
///     The adapter routes layout and paint through <see cref="LightweaveRoot" /> but does NOT reimplement
///     vanilla <see cref="FloatMenuOption.DoGUI" /> affordances: default row painting (icon, label, and
///     extraPart composition), <c>mouseoverGuiAction</c> (not called - consumers must handle hover
///     feedback inside their Build closure), hover highlight and selection sound (vanilla draws these;
///     this override skips them entirely), <c>Disabled</c> / <c>disabledReason</c> handling (never applied
///     by this override - consumers must render disabled appearance and gate <c>action</c> themselves),
///     and <c>revalidateClickTarget</c> (still honored if the parent FloatMenu calls back). The <c>subKey</c>
///     parameter disambiguates multiple bespoke options on the same entity in a single frame - use a
///     distinct value per option so each gets a stable, unique Guid from <see cref="AdapterStoreRegistry" />.
/// </summary>
public sealed class AsFloatMenuOption : FloatMenuOption {
    private readonly Func<LightweaveNode> build;
    private readonly int entityId;
    private readonly int subKey;

    public AsFloatMenuOption(int entityId, Action? action, Func<LightweaveNode> build, int subKey = 0)
        : base(string.Empty, action) {
        this.entityId = entityId;
        this.subKey = subKey;
        this.build = build;
    }

    public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu parent) {
        Guid id = AdapterStoreRegistry.GetOrCreate(entityId, AdapterKind.FloatMenu, subKey);
        LightweaveRoot.Render(rect, id, build);

        Event evt = Event.current;
        if (evt == null || evt.type == EventType.Used) {
            return false;
        }

        if (Mouse.IsOver(rect) && evt.type == EventType.MouseUp && evt.button == 0) {
            evt.Use();
            action?.Invoke();
            return true;
        }

        return false;
    }
}