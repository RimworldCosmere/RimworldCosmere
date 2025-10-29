using Cosmere.Core.Shader.Properties;
using Cosmere.Def;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Surgebinding.Ability;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Thing.Apparel;

public class RadiantShardplate : Shardplate {
    private float colorTime = 0f;
    private RadiantOrderDef? radiantOrderInt;
    private Color[] storedColors = new Color[4];

    private RadiantOrderDef? radiantOrder {
        get { return radiantOrderInt ??= SpawnedParentOrMe is Verse.Pawn pawn ? pawn.GetRadiantOrder() : null; }
    }

    private GemDef? gemstone => radiantOrder?.gemstone;

    public override void Notify_Unequipped(Verse.Pawn pawn) {
        base.Notify_Unequipped(pawn);
        SurgebindingAbility? ability =
            (SurgebindingAbility)pawn.abilities.GetAbility(
                AbilityDefOf.Cosmere_Roshar_Ability_ToggleShardplate
            );

        ability.UpdateStatus(false);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        Destroy();
    }

    public override void DrawWornExtras() {
        base.DrawWornExtras();
        if (gemstone != null) {
            cutoutLUT.props.glowColor = gemstone.glowColor ?? gemstone.color;
        }
    }

    public override List<LUTPaletteMaterial> GetMaterials() {
        if (gemstone == null) return [];

        LUTPaletteMaterial colorOne = new LUTPaletteMaterial
            { color = gemstone.color, metallic = .7f, smoothness = .7f };
        LUTPaletteMaterial colorTwo = !gemstone.colorTwo.HasValue
            ? colorOne
            : new LUTPaletteMaterial { color = gemstone.colorTwo.Value, metallic = .7f, smoothness = .7f };
        LUTPaletteMaterial colorThree = !gemstone.glowColor.HasValue
            ? gemstone.colorTwo.HasValue ? colorTwo : colorOne
            : new LUTPaletteMaterial { color = gemstone.glowColor.Value, metallic = 0, smoothness = 1f };


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