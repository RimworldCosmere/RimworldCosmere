using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Render;

public class PawnRenderNode_SprenBody : PawnRenderNode {
    private static readonly Dictionary<string, int> VariantCountCache = [];

    public PawnRenderNode_SprenBody(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree) { }

    public override Graphic? GraphicFor(Pawn pawn) {
        PawnKindDef kindDef = pawn.kindDef;
        if (kindDef.lifeStages == null || kindDef.lifeStages.Count == 0) return null;

        GraphicData graphicData = kindDef.lifeStages[0].bodyGraphicData;
        if (graphicData == null) return null;

        string basePath = graphicData.texPath;
        string gender = pawn.gender == Gender.Female ? "female" : "male";
        int variantCount = GetVariantCount(basePath, gender);

        if (variantCount == 0) {
            Graphic fallback = graphicData.Graphic;
            if (fallback == null) return null;
            return fallback.GetColoredVersion(fallback.Shader, fallback.Color, fallback.ColorTwo);
        }

        int variant = pawn.thingIDNumber % variantCount + 1;
        string variantPath = $"{basePath}{variant}_{gender}";

        return GraphicDatabase.Get<Graphic_Multi>(
            variantPath,
            Verse.ShaderDatabase.Cutout,
            graphicData.drawSize,
            Color.white
        );
    }

    public override GraphicMeshSet? MeshSetFor(Pawn pawn) {
        Graphic? graphic = GraphicFor(pawn);
        if (graphic == null) return null;
        return MeshPool.GetMeshSetForSize(graphic.drawSize.x, graphic.drawSize.y);
    }

    private static int GetVariantCount(string basePath, string gender) {
        string cacheKey = $"{basePath}_{gender}";
        if (VariantCountCache.TryGetValue(cacheKey, out int count)) return count;

        count = 0;
        for (int i = 1; i <= 20; i++) {
            if (ContentFinder<Texture2D>.Get($"{basePath}{i}_{gender}_south", false) != null) {
                count = i;
            } else {
                break;
            }
        }

        VariantCountCache[cacheKey] = count;
        return count;
    }
}
