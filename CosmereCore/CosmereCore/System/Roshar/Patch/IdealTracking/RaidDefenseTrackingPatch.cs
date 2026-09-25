using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class RaidDefenseTrackingPatch : Lord {
    [Inject(At.Head, nameof(Cleanup))]
    private void BeforeCleanup() {
        if (Map == null) return;
        if (faction == null || !faction.HostileTo(Faction.OfPlayer)) return;

        LordJob lordJob = LordJob;
        if (lordJob is not LordJob_AssaultColony and not LordJob_AssaultThings) return;

        List<Pawn> colonists = Map.mapPawns.FreeColonistsSpawned;
        CountColonistStates(colonists, out int downedCount, out int standingCount);

        for (int i = 0; i < colonists.Count; i++) {
            CreditRaidDefense(colonists[i], standingCount, downedCount);
        }
    }

    private static void CountColonistStates(List<Pawn> colonists, out int downedCount, out int standingCount) {
        downedCount = 0;
        standingCount = 0;

        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            if (colonist.Downed) {
                downedCount++;
            } else if (!colonist.Dead) {
                standingCount++;
            }
        }
    }

    private static void CreditRaidDefense(Pawn colonist, int standingCount, int downedCount) {
        if (colonist.Dead || colonist.Downed) return;

        Surgebinder? surgebinder = colonist.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        if (colonist.Drafted ||
            colonist.CurJobDef == RimWorld.JobDefOf.AttackMelee ||
            colonist.CurJobDef == RimWorld.JobDefOf.AttackStatic) {
            colonist.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_RaidsDefended, 1);
        }

        if (standingCount <= 2 && downedCount >= 2) {
            colonist.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_LastStanding, 1);
        }
    }
}
