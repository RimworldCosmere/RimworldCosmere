using UnityEngine;
using Verse;

namespace Cosmere.Core.Gene.PawnRender.SubWorker;

public class CutoutAdvanced : PawnRenderSubWorker {
    public override void EditMaterialPropertyBlock(
        PawnRenderNode node,
        Material material,
        PawnDrawParms parms,
        ref MaterialPropertyBlock block
    ) {
        Comp.Thing.CutoutAdvanced? comp = node.apparel?.GetComp<Comp.Thing.CutoutAdvanced>();
        comp?.UpdateMaterialPropertyBlock(block, node.PrimaryGraphic, material);
    }

    public override void EditMaterial(PawnRenderNode node, PawnDrawParms parms, ref Material material) {
        if (material == null) return;
        material.shader = ShaderDatabase.CutoutAdvanced;
    }
}