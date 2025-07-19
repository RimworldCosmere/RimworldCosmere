using UnityEngine;
using Verse;

namespace CosmereRoshar.Graphic;

public class ShardBladeVariants : Graphic_Collection {
    public override Verse.Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo) {
        return GraphicDatabase.Get<ShardBladeVariants>(
            path,
            newShader,
            drawSize,
            newColor,
            newColorTwo,
            data
        );
    }

    public override Material MatAt(Rot4 rot, Verse.Thing thing = null) {
        return MatSingleFor(thing);
    }

    public override Material MatSingleFor(Verse.Thing thing) {
        int id = StormlightUtilities.GetGraphicId(thing as ThingWithComps);
        return subGraphics[id].MatSingle;
    }


    public override void DrawWorker(
        Vector3 loc,
        Rot4 rot,
        ThingDef thingDef,
        Verse.Thing thing,
        float extraRotation
    ) {
        int id = StormlightUtilities.GetGraphicId(thing as ThingWithComps);
        Verse.Graphic graphic = subGraphics[id];
        graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
    }

    public override string ToString() {
        return "StackCount(path=" + path + ", count=" + subGraphics.Length + ")";
    }
}