using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CosmereCore.Util;

public static class InvestitureStatUtility {
    public static bool TryGetPawnInvestitureStat(
        StatRequest req,
        Func<Pawn, float> pawnStatGetter,
        Func<ThingDef, float> pawnDefStatGetter,
        out float stat
    ) {
        if (req.HasThing) {
            if (req.Thing is Pawn arg) {
                stat = pawnStatGetter(arg);
                return true;
            }

            if (req.Thing is Corpse corpse) {
                stat = pawnStatGetter(corpse.InnerPawn);
                return true;
            }
        } else if (req.Def is ThingDef thingDef) {
            if (thingDef.category == ThingCategory.Pawn) {
                stat = pawnDefStatGetter(thingDef);
                return true;
            }

            if (thingDef.IsCorpse) {
                stat = pawnDefStatGetter(thingDef.ingestible.sourceDef);
                return true;
            }
        }

        stat = 0f;
        return false;
    }

    public static float GearAndInventoryInvestiture(Pawn pawn) {
        return GearInvestiture(pawn) + InventoryInvestiture(pawn);
    }

    public static float GearInvestiture(Pawn pawn) {
        float num = 0f;
        if (pawn.apparel != null) {
            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++) {
                num += wornApparel[i].GetStatValue(StatDefOf.Cosmere_Investiture, true, 1);
            }
        }

        if (pawn.equipment != null) {
            foreach (ThingWithComps item in pawn.equipment.AllEquipmentListForReading) {
                num += item.GetStatValue(StatDefOf.Cosmere_Investiture, true, 1);
            }
        }

        return num;
    }

    public static float InventoryInvestiture(Pawn pawn) {
        float num = 0f;
        for (int i = 0; i < pawn.inventory.innerContainer.Count; i++) {
            Thing thing = pawn.inventory.innerContainer[i];
            num += thing.stackCount * thing.GetStatValue(StatDefOf.Cosmere_Investiture);
        }

        return num;
    }
}