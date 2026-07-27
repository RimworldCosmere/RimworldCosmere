using Cosmere.Core.Comp.Game;
using Cosmere.Core.Nightwatcher;
using Cosmere.Core.Util;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Dialog;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Nightwatcher;

public static class NightwatcherSystem {
    public static bool IsEligible(Pawn pawn) {
        if (!pawn.IsColonist) return false;
        if (pawn.Dead || pawn.Downed) return false;
        NightwatcherVisit? comp = pawn.TryGetComp<NightwatcherVisit>();
        if (comp == null || comp.HasVisited) return false;
        return ShardDefOf.Cultivation != null &&
               ShardUtility.AreAnyEnabled(ShardDefOf.Cultivation);
    }

    public static void InitiateSeek(Pawn pawn) {
        Find.WindowStack.Add(new Dialog_NightwatcherEncounter(pawn));
    }

    public static void ApplyBoon(Pawn pawn, NightwatcherBoonDef boon, NightwatcherApplicationContext? context = null) {
        StandardBoonApplicator.Instance.Apply(pawn, boon, context);
        boon.Applicator?.Apply(pawn, boon, context);

        SpiritWeb? web = Current.Game.GetComponent<SpiritWeb>();
        CultivationEntity? cultivation = CultivationEntity.Instance;
        if (web != null && cultivation != null) {
            web.AdjustConnection(cultivation, pawn, 0.1f);
        }

        Logger.Info($"NightwatcherSystem: applied boon '{boon.defName}' to {pawn.NameShortColored}");
    }

    public static void ApplyCurse(Pawn pawn, NightwatcherCurseDef curse, NightwatcherApplicationContext? context = null) {
        StandardCurseApplicator.Instance.Apply(pawn, curse, context);
        curse.Applicator?.Apply(pawn, curse, context);

        Logger.Info($"NightwatcherSystem: applied curse '{curse.defName}' to {pawn.NameShortColored}");
    }

    public static NightwatcherCurseDef DrawCurse(NightwatcherBoonDef boon) {
        List<NightwatcherCurseDef> eligible = [];
        List<NightwatcherCurseDef> all = DefDatabase<NightwatcherCurseDef>.AllDefsListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i].minBoonTier <= boon.powerTier) eligible.Add(all[i]);
        }

        if (eligible.Count == 0) {
            Logger.Warning($"NightwatcherSystem: no eligible curses for boon tier {boon.powerTier}");
            return DefDatabase<NightwatcherCurseDef>.AllDefsListForReading[0];
        }

        int tierIndex = boon.powerTier - 1;
        float totalWeight = 0f;
        for (int i = 0; i < eligible.Count; i++) {
            totalWeight += eligible[i].curseWeights[tierIndex];
        }

        float roll = Rand.Value * totalWeight;
        float cumulative = 0f;
        for (int i = 0; i < eligible.Count; i++) {
            cumulative += eligible[i].curseWeights[tierIndex];
            if (roll <= cumulative) return eligible[i];
        }

        return eligible[eligible.Count - 1];
    }
}
