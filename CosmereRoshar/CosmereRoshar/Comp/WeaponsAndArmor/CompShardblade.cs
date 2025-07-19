using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereRoshar.Comp.WeaponsAndArmor;

public class GraphicShardBladeVariants : Graphic_Collection {
    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo) {
        return GraphicDatabase.Get<GraphicShardBladeVariants>(
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
        Graphic graphic = subGraphics[id];
        graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
    }

    public override string ToString() {
        return "StackCount(path=" + path + ", count=" + subGraphics.Length + ")";
    }
}

public static class AbilityExtensions {
    public static T? GetAbilityComp<T>(this Pawn pawn, string abilityDefName) where T : CompAbilityEffect {
        if (pawn.abilities == null) return null;

        Ability ability = pawn.abilities.GetAbility(DefDatabase<AbilityDef>.GetNamed(abilityDefName));
        return ability?.comps?.OfType<T>().FirstOrDefault();
    }
}