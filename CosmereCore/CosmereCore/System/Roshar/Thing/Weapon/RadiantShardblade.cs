using Cosmere.Core.Ability;
using Cosmere.Core.Shader.Properties;
using Cosmere.System.Roshar.Def;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Thing.Weapon;

public class RadiantShardblade : Shardblade {
    public override void Notify_Unequipped(Verse.Pawn pawn) {
        base.Notify_Unequipped(pawn);
        Surgebinding.Ability.Shardblade? ability =
            (Surgebinding.Ability.Shardblade)pawn.abilities.GetAbility(
                AbilityDefOf.Cosmere_Roshar_Ability_ToggleShardblade
            );

        ability.UpdateStatus(Active.Off);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        Destroy();
    }

    public override List<LUTPaletteMaterial> GetMaterials() {
        if (SpawnedParentOrMe is not Verse.Pawn pawn) return [];

        RadiantOrderDef? order = pawn.GetRadiantOrder();
        if (order == null) return [];

        Color colorOne = order.gemstone.color;
        Color colorTwo = order.gemstone.colorTwo ?? colorOne;
        Color colorThree = order.gemstone.glowColor ?? colorTwo;

        return [
            new LUTPaletteMaterial { color = colorOne, Metallic = .3f, Smoothness = .7f },
            new LUTPaletteMaterial { color = colorTwo, Metallic = .8f, Smoothness = .85f },
            new LUTPaletteMaterial { color = colorThree, Smoothness = 1 },
            new LUTPaletteMaterial
                { color = def.GetColorForStuff(Core.ThingDefOf.Gold), Metallic = 1, Smoothness = 1 },
            new LUTPaletteMaterial
                { color = def.GetColorForStuff(RimWorld.ThingDefOf.WoodLog) },
            new LUTPaletteMaterial
                { color = def.GetColorForStuff(Core.ThingDefOf.Steel), Metallic = .5f, Smoothness = 1 },
        ];
    }
}
