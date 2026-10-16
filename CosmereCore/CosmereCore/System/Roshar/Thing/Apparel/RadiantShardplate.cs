using Cosmere.Core.Ability;
using Cosmere.Core.Def;
using Cosmere.Core.Shader.Properties;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Surgebinding.Ability;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Thing.Apparel;

public class RadiantShardplate : Shardplate {
    private RadiantOrderDef? radiantOrderInt;

    private RadiantOrderDef? RadiantOrder =>
        radiantOrderInt ??= SpawnedParentOrMe is Verse.Pawn pawn ? pawn.GetRadiantOrder() : null;

    private GemDef? Gemstone => RadiantOrder?.gemstone;

    public override void Notify_Unequipped(Verse.Pawn pawn) {
        base.Notify_Unequipped(pawn);
        SurgebindingAbility? ability =
            (SurgebindingAbility)pawn.abilities.GetAbility(
                AbilityDefOf.Cosmere_Roshar_Ability_ToggleShardplate
            );

        ability.UpdateStatus(Active.Off);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        Destroy();
    }

    public override void DrawWornExtras() {
        base.DrawWornExtras();
        if (Gemstone != null) {
            CutoutLUT.Props.glowColor = Gemstone.glowColor ?? Gemstone.color;
        }
    }

    public override List<LUTPaletteMaterial> GetMaterials() {
        if (Gemstone == null) return [];

        LUTPaletteMaterial colorOne = new LUTPaletteMaterial { color = Gemstone.color, Metallic = .7f, Smoothness = .7f };
        LUTPaletteMaterial colorTwo = !Gemstone.colorTwo.HasValue
            ? colorOne
            : new LUTPaletteMaterial { color = Gemstone.colorTwo.Value, Metallic = .7f, Smoothness = .7f };
        LUTPaletteMaterial fallbackThree = Gemstone.colorTwo.HasValue ? colorTwo : colorOne;
        LUTPaletteMaterial colorThree = !Gemstone.glowColor.HasValue
            ? fallbackThree
            : new LUTPaletteMaterial { color = Gemstone.glowColor.Value, Metallic = 0, Smoothness = 1f };

        if (def == ThingDefOf.Cosmere_Roshar_Apparel_RadiantShardhelm) {
            /*
             * First: Visor
             * Second: Cheeks
             * Third: Faceplate
             * Fourth: Top
             */
            return [
                colorThree,
                colorTwo,
                colorOne,
                colorTwo,
            ];
        }

        /*
         * First: Legs
         * Second: Chest
         * Third: Left Pauldron
         * Fourth: Right Pauldron
         */
        return [colorTwo, colorOne, colorTwo, colorTwo];
    }
}
