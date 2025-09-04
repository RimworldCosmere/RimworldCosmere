using Cosmere.Core.Shader.Properties;
using Cosmere.Roshar.Def;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Thing.Weapon;

public class RadiantShardblade : Shardblade {
    public override void Notify_Unequipped(Verse.Pawn pawn) {
        base.Notify_Unequipped(pawn);
        Surgebinding.Ability.Shardblade? ability =
            (Surgebinding.Ability.Shardblade)pawn.abilities.GetAbility(
                AbilityDefOf.Cosmere_Roshar_Ability_ToggleShardblade
            );

        ability.UpdateStatus(false);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        Destroy();
    }


    protected override LUTPaletteMaterial[] GetBladeMaterials() {
        if (SpawnedParentOrMe is not Verse.Pawn pawn) return [];

        RadiantOrderDef? order = pawn.GetRadiantOrder();
        if (order == null) return [];

        Color colorOne = order.gemstone.color;
        Color colorTwo = order.gemstone.colorTwo ?? colorOne;
        Color colorThree = order.gemstone.glowColor ?? colorTwo;

        return [
            new LUTPaletteMaterial(colorOne, .3, 0.7),
            new LUTPaletteMaterial(colorTwo, .8, .85),
            new LUTPaletteMaterial(colorThree, 0, 1),
            new LUTPaletteMaterial(def.GetColorForStuff(Resources.ThingDefOf.Gold), 1, 1),
            new LUTPaletteMaterial(def.GetColorForStuff(RimWorld.ThingDefOf.WoodLog), 0, 0),
            new LUTPaletteMaterial(def.GetColorForStuff(Resources.ThingDefOf.Steel), 1, .4),
        ];
    }
}