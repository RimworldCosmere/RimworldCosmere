using System;
using RimWorld;
using Verse;

namespace Cosmere.Core.Util;

public static class StatUtility {
    public static bool TryGetPawnStat(
        StatRequest req,
        Func<Pawn, float> pawnStatGetter,
        Func<ThingDef, float> pawnDefStatGetter,
        out float stat
    ) {
        return TryGetPawnStatCore(
            req,
            (_, p) => pawnStatGetter(p),
            (_, d) => pawnDefStatGetter(d),
            null,
            out stat
        );
    }

    public static bool TryGetPawnStat(
        StatRequest req,
        StatDef statDef,
        Func<StatDef, Pawn, float> pawnStatGetter,
        Func<StatDef, ThingDef, float> pawnDefStatGetter,
        out float stat
    ) {
        return TryGetPawnStatCore(
            req,
            (s, p) => pawnStatGetter(s!, p),
            (s, d) => pawnDefStatGetter(s!, d),
            statDef,
            out stat
        );
    }

    public static bool TryGetPawnStat(
        StatRequest req,
        StatDef statDef,
        Func<Pawn, float> pawnStatGetter,
        Func<StatDef, ThingDef, float> pawnDefStatGetter,
        out float stat
    ) {
        return TryGetPawnStatCore(
            req,
            (_, p) => pawnStatGetter(p),
            (s, d) => pawnDefStatGetter(s!, d),
            statDef,
            out stat
        );
    }

    public static bool TryGetPawnStat(
        StatRequest req,
        StatDef statDef,
        Func<StatDef, Pawn, float> pawnStatGetter,
        Func<ThingDef, float> pawnDefStatGetter,
        out float stat
    ) {
        return TryGetPawnStatCore(
            req,
            (s, p) => pawnStatGetter(s!, p),
            (_, d) => pawnDefStatGetter(d),
            statDef,
            out stat
        );
    }

    private static bool TryGetPawnStatCore(
        StatRequest req,
        Func<StatDef?, Pawn, float> pawnStatGetter,
        Func<StatDef?, ThingDef, float> pawnDefStatGetter,
        StatDef? statDef,
        out float stat
    ) {
        if (req.HasThing) {
            if (req.Thing is Pawn pawn) {
                stat = pawnStatGetter(statDef, pawn);
                return true;
            }

            if (req.Thing is Corpse corpse) {
                stat = pawnStatGetter(statDef, corpse.InnerPawn);
                return true;
            }
        } else if (req.Def is ThingDef thingDef) {
            if (thingDef.category == ThingCategory.Pawn) {
                stat = pawnDefStatGetter(statDef, thingDef);
                return true;
            }

            if (thingDef.IsCorpse) {
                stat = pawnDefStatGetter(statDef, thingDef.ingestible.sourceDef);
                return true;
            }
        }

        stat = 0f;
        return false;
    }

    public static float NeedLevel<TN>(Pawn pawn) where TN : RimWorld.Need {
        return pawn.needs?.TryGetNeed<TN>()?.CurLevel ?? 0;
    }

    public static float StatBase(StatDef stat, ThingDef def) {
        return def.statBases.FirstOrDefault(x => x.stat.Equals(stat))?.value ?? 0f;
    }

    public static float GearAndInventoryStat(StatDef statDef, Pawn pawn) {
        return GearStat(statDef, pawn) + InventoryStat(statDef, pawn);
    }

    public static float GearStat(StatDef statDef, Pawn pawn) {
        float num = 0f;
        if (pawn.apparel != null) {
            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++) {
                num += wornApparel[i].GetStatValue(statDef, true, 1);
            }
        }

        if (pawn.equipment != null) {
            foreach (ThingWithComps item in pawn.equipment.AllEquipmentListForReading) {
                num += item.GetStatValue(statDef, true, 1);
            }
        }

        return num;
    }

    public static float InventoryStat(StatDef statDef, Pawn pawn) {
        float num = 0f;
        for (int i = 0; i < pawn.inventory.innerContainer.Count; i++) {
            Verse.Thing thing = pawn.inventory.innerContainer[i];
            num += thing.stackCount * thing.GetStatValue(statDef);
        }

        return num;
    }
}