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
        node.apparel.GetComp<Comp.Thing.CutoutAdvanced>()
            .UpdateMaterialPropertyBlock(
                block,
                node.PrimaryGraphic,
                material
            );
    }

    public override void EditMaterial(PawnRenderNode node, PawnDrawParms parms, ref Material material) {
        if (parms.DrawNow) {
            bool test = true;
        }

        material.shader = ShaderDatabase.CutoutAdvanced;
    }
}