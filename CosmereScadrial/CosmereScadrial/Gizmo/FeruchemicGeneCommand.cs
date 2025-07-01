using System.Collections.Generic;
using CosmereScadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Gizmo;

[StaticConstructorOnStartup]
public class FeruchemicGeneCommand(
    Gene_Resource gene,
    List<IGeneResourceDrain> drainGenes,
    Color barColor,
    Color barHighlightColor
) : CosmereGeneCommand(gene, drainGenes, barColor, barHighlightColor) {
    protected override float abilityIconSize => Height / 2f;
    protected override float baseWidth => GetWidthForAbilityCount(3);
    private new Feruchemist gene => (Feruchemist)base.gene;

    protected override int IncrementDivisor => 100 / 5;
    protected override FloatRange DragRange => new FloatRange(0.0f, 1f);

    protected override Texture2D GetIcon() {
        return metal.feruchemy!.invertedIcon;
    }

    protected override void DrawBottomBar(Rect bottomBarRect, ref bool mouseOverElement) {
        base.DrawBottomBar(bottomBarRect, ref mouseOverElement);

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter)) Widgets.Label(bottomBarRect, BarLabel);

        TooltipHandler.TipRegionByKey(bottomBarRect, "CS_SetTapStoreRate", pawn.Named("PAWN"),
            metal.Named("METAL"));
        if (Mouse.IsOver(bottomBarRect)) {
            mouseOverElement = true;
        }
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms) {
        GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
        bool mouseOver = result.State.Equals(GizmoState.Mouseover);

        if (Widgets.ButtonInvisible(iconRect)) {
            mouseOver = true;
            gene.Reset();
            gene.ResetMax();
        }

        return new GizmoResult(mouseOver ? GizmoState.Mouseover : GizmoState.Clear, result.InteractEvent);
    }
}