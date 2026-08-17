using Cosmere.Core.UI.Dock;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI.Feruchemy;

public sealed class FeruchemyDialWidget {
    private const float IdleTarget = 50f;

    private string? draggingDial;

    // Compounding is only offered on an implanted metalmind, because burning one
    // destroys it and a worn band is not what the pawn is setting alight. The
    // toggle parks the other pool so only one is ever moving.
    internal float DrawCompoundToggle(Rect inner, float y, Pawn pawn, Feruchemist gene, float buttonHeight) {
        // The internal group is every implant, so compounding reaches it just as it
        // reaches a single one.
        bool eligible = gene.TargetIsInternalOnly;
        AcceptanceReport report = CompoundingAccess.Gate(pawn, gene);

        // Losing the implant or the gate mid-compound parks the pool rather than
        // leaving it draining behind a control the player can no longer see.
        if (gene.compounding && (!eligible || !report.Accepted)) {
            gene.compounding = false;
            gene.compoundedTargetValue = IdleTarget;
        }

        if (!eligible) return y - 6f;

        bool on = gene.compounding;
        Rect rect = new Rect(inner.x, y, inner.width, buttonHeight);

        TooltipHandler.TipRegion(
            rect,
            report.Accepted
                ? "CC_Dock_Feruchemy_CompoundTip".Translate()
                : "CC_Dock_Feruchemy_CompoundBlocked".Translate(
                    (report.Reason.NullOrEmpty()
                        ? "CC_Dock_Feruchemy_CompoundUnavailable".Translate().Resolve()
                        : report.Reason).Named("REASON")
                )
        );

        string label = on
            ? "CC_Dock_Feruchemy_StopCompound".Translate()
            : "CC_Dock_Feruchemy_Compound".Translate();
        if (!DockButton.Draw(
                rect,
                label,
                FeruchemyPalette.CompoundTint,
                kind: on ? DockButtonKind.Active : DockButtonKind.Primary,
                enabled: report.Accepted && eligible
            )) {
            return rect.yMax;
        }

        // Carry whatever the dial was set to across, and no further. The rate is the
        // player's to choose, so an idle dial stays idle rather than being pegged
        // somewhere on their behalf.
        if (on) {
            gene.compounding = false;
            gene.targetValue = gene.compoundedTargetValue;
            gene.compoundedTargetValue = IdleTarget;
        } else {
            gene.compounding = true;
            gene.compoundedTargetValue = gene.targetValue;
            gene.targetValue = IdleTarget;
        }

        Event.current?.Use();

        return rect.yMax;
    }

    private static void DrawCompoundedDialBacking(Rect rect, Feruchemist gene, FeruchemyCapacity capacity) {
        Widgets.DrawBoxSolid(rect, new Color(0.047f, 0.043f, 0.035f));

        Color blocked = new Color(0.098f, 0.090f, 0.075f);
        if (!capacity.CanTapCompounded) {
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, rect.width / 2f, rect.height), blocked);
        }

        if (!capacity.CanStoreCompounded) {
            Widgets.DrawBoxSolid(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height), blocked);
        }

        float delta = gene.compoundedTargetValue - IdleTarget;
        if (delta < 0f) {
            float width = rect.width / 2f * Mathf.Clamp01(-delta / IdleTarget);
            Widgets.DrawBoxSolid(new Rect(rect.center.x - width, rect.y, width, rect.height), FeruchemyPalette.TapFill);
        } else if (delta > 0f) {
            float width = rect.width / 2f * Mathf.Clamp01(delta / IdleTarget);
            Widgets.DrawBoxSolid(new Rect(rect.center.x, rect.y, width, rect.height), FeruchemyPalette.CompoundTint);
        }

        Widgets.DrawBoxSolid(
            new Rect(rect.center.x, rect.y - 1f, 1f, rect.height + 2f),
            new Color(0.353f, 0.322f, 0.271f)
        );
    }

    // Hand-drawn so the dial keeps the section's chrome. The vanilla slider
    // brings its own tan gradient, which fights everything around it.
    internal void DrawDial(Rect rect, string metalId, Feruchemist gene, FeruchemyCapacity capacity, bool compounded) {
        if (compounded) DrawCompoundedDialBacking(rect, gene, capacity);
        else DrawDialBacking(rect, gene, capacity);

        float value = compounded ? gene.compoundedTargetValue : gene.targetValue;
        float handleX = rect.x + rect.width * (Mathf.Clamp(value, 0f, 100f) / 100f);
        Widgets.DrawBoxSolid(
            new Rect(handleX - 1.5f, rect.y - 2f, 3f, rect.height + 4f),
            compounded ? new Color(0.898f, 0.812f, 0.588f) : new Color(0.816f, 0.851f, 0.871f)
        );

        HandleDialDrag(rect, metalId, gene, capacity, compounded);
    }

    // Shared by both dials. Compounded runs tap-only, so its reachable span stops
    // at the idle point rather than continuing into the store half.
    private void HandleDialDrag(Rect rect, string dialId, Feruchemist gene, FeruchemyCapacity capacity, bool compounded) {
        float min = (compounded ? capacity.CanTapCompounded : capacity.CanTap || capacity.CanTapCompounded)
            ? 0f
            : IdleTarget;
        float max = (compounded ? capacity.CanStoreCompounded : capacity.CanStore) ? 100f : IdleTarget;

        Event? e = Event.current;
        if (e == null) return;

        if (e.type == EventType.MouseDown && Mouse.IsOver(rect)) {
            draggingDial = dialId;
            e.Use();
        }

        if (draggingDial != dialId) return;

        // MouseDrag only reaches a control that claimed the hot control, which a
        // hand-drawn dial never does, so follow the button state directly.
        if (!Input.GetMouseButton(0)) {
            draggingDial = null;
            return;
        }

        float snapped = gene.SnapTarget(Mathf.Clamp((e.mousePosition.x - rect.x) / rect.width * 100f, min, max));
        if (compounded) gene.compoundedTargetValue = snapped;
        else gene.targetValue = snapped;
    }

    private static void DrawDialBacking(Rect rect, Feruchemist gene, FeruchemyCapacity capacity) {
        Widgets.DrawBoxSolid(rect, new Color(0.047f, 0.043f, 0.035f));

        Color blocked = new Color(0.098f, 0.090f, 0.075f);
        if (!capacity.CanTap) {
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, rect.width / 2f, rect.height), blocked);
        }

        if (!capacity.CanStore) {
            Widgets.DrawBoxSolid(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height), blocked);
        }

        float delta = gene.targetValue - IdleTarget;
        if (delta < 0f) {
            float width = rect.width / 2f * Mathf.Clamp01(-delta / IdleTarget);
            Widgets.DrawBoxSolid(new Rect(rect.center.x - width, rect.y, width, rect.height), FeruchemyPalette.TapFill);
        } else if (delta > 0f) {
            float width = rect.width / 2f * Mathf.Clamp01(delta / IdleTarget);
            Widgets.DrawBoxSolid(new Rect(rect.center.x, rect.y, width, rect.height), FeruchemyPalette.StoreFill);
        }

        Widgets.DrawBoxSolid(
            new Rect(rect.center.x, rect.y - 1f, 1f, rect.height + 2f),
            new Color(0.353f, 0.322f, 0.271f)
        );
    }
}
