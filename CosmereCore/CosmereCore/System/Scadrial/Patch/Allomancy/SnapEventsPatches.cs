using Concord;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Patch.Allomancy;

public static class SnapEvents {
    private static readonly PawnRelationDef[] CloseRelations = [
        PawnRelationDefOf.Spouse,
        PawnRelationDefOf.Lover,
        PawnRelationDefOf.Parent,
        PawnRelationDefOf.Child,
        PawnRelationDefOf.Bond,
    ];

    internal static readonly Dictionary<int, int> StarvationCooldowns = new Dictionary<int, int>();
    internal static readonly HashSet<int> TemperatureCooldowns = [];
    internal static readonly HashSet<int> WithdrawalCooldowns = [];

    internal static readonly Dictionary<int, (int startTick, int count, bool triggered)> ColonyDestructionTracker =
        new Dictionary<int, (int startTick, int count, bool triggered)>();

    internal static void Snap(Pawn pawn, int oneInChance, string? cause = null) {
        if (!Rand.Chance(1f / oneInChance)) return;
        SnapUtility.Snap(pawn, cause);
    }

    internal static void SnapFirstKill(Pawn killer) {
        if (killer.records == null) return;
        int kills = (int)killer.records.GetValue(RimWorld.RecordDefOf.KillsHumanlikes);
        if (kills > 1) return;

        Snap(killer, 4, "CS_PawnSnapped_FirstKill");
    }

    internal static bool IsCloseRelation(PawnRelationDef def) {
        for (int i = 0; i < CloseRelations.Length; i++) {
            if (CloseRelations[i] == def) return true;
        }

        return false;
    }
}

[Patch]
public abstract class SnapFromMentalBreakPatch : MentalStateHandler {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected SnapFromMentalBreakPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, nameof(TryStartMentalState))]
    private void AfterTryStartMentalState(ControlHandle<bool> ch) {
        if (ch.ReturnValue) SnapEvents.Snap(trackedPawn, 16);
    }
}

[Patch]
public abstract class SnapHealthTrackerPatch : Pawn_HealthTracker {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected SnapHealthTrackerPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, nameof(PostApplyDamage))]
    private void AfterPostApplyDamage(DamageInfo dinfo) {
        Pawn pawn = trackedPawn;
        if (pawn.Dead) return;

        if (dinfo.Def == DamageDefOf.Crush && dinfo.Instigator == null) {
            SnapEvents.Snap(pawn, 6, "CS_PawnSnapped_RoofCollapse");
            return;
        }

        if (pawn.IsPrisonerOfColony && dinfo.Instigator is Pawn) {
            SnapEvents.Snap(pawn, 12, "CS_PawnSnapped_Prisoner");
            return;
        }

        if (pawn.health.summaryHealth.SummaryHealthPercent > 0.2f) return;
        SnapEvents.Snap(pawn, 16);
    }

    [Inject(At.Return, "MakeDowned")]
    private void AfterMakeDowned(DamageInfo? dinfo) {
        if (dinfo == null) return;
        SnapEvents.Snap(trackedPawn, 16, "CS_PawnSnapped_Downed");
    }

    [Inject(At.Return, nameof(HealthTickInterval))]
    private void AfterHealthTickInterval() {
        Pawn pawn = trackedPawn;
        if (pawn.Dead) return;
        HediffSet? hediffSet = pawn.health?.hediffSet;
        if (hediffSet == null) return;

        Verse.Hediff hypo = hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.Hypothermia);
        if (hypo != null && hypo.Severity >= 0.7f) {
            if (SnapEvents.TemperatureCooldowns.Add(hypo.loadID)) {
                SnapEvents.Snap(pawn, 12, "CS_PawnSnapped_Temperature");
            }

            return;
        }

        Verse.Hediff heat = hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.Heatstroke);
        if (heat != null && heat.Severity >= 0.7f) {
            if (SnapEvents.TemperatureCooldowns.Add(heat.loadID)) {
                SnapEvents.Snap(pawn, 12, "CS_PawnSnapped_Temperature");
            }
        }
    }

    [Inject(
        At.Return,
        nameof(AddHediff),
        parameterTypes: [
            typeof(Verse.Hediff),
            typeof(BodyPartRecord),
            typeof(DamageInfo?),
            typeof(DamageWorker.DamageResult),
        ]
    )]
    private void AfterAddHediff(Verse.Hediff hediff) {
        if (hediff.def != RimWorld.HediffDefOf.MissingBodyPart) return;
        Pawn pawn = hediff.pawn;
        if (pawn == null || pawn.Dead) return;
        SnapEvents.Snap(pawn, 8, "CS_PawnSnapped_LimbLoss");
    }
}

[Patch]
public abstract class SnapFromDeathWitnessPatch : Pawn {
    [Inject(At.Return, nameof(Kill))]
    private void AfterKill(DamageInfo? dinfo) {
        Pawn self = this;
        if (self.relations == null) return;
        Map map = self.MapHeld;
        if (map == null) return;

        if (dinfo?.Instigator is Pawn killer && !killer.Dead && killer != self) {
            SnapEvents.SnapFirstKill(killer);
        }

        List<DirectPawnRelation> relations = self.relations.DirectRelations;
        for (int i = 0; i < relations.Count; i++) {
            DirectPawnRelation rel = relations[i];
            if (!SnapEvents.IsCloseRelation(rel.def)) continue;

            Pawn related = rel.otherPawn;
            if (related.Dead || related.Map != map) continue;

            SnapUtility.Snap(related, "CS_PawnSnapped_WitnessedDeath");
        }
    }
}

[Patch]
public abstract class SnapFromStarvationPatch : Need_Food {
    protected SnapFromStarvationPatch(Pawn newPawn) : base(newPawn) { }

    [Inject(At.Return, nameof(NeedInterval))]
    private void AfterNeedInterval() {
        Need_Food self = this;
        if (self.CurLevelPercentage > 0.05f) return;

        Pawn needPawn = pawn;
        int pawnId = needPawn.thingIDNumber;
        int currentDay = GenDate.DaysPassed;

        if (SnapEvents.StarvationCooldowns.TryGetValue(pawnId, out int lastDay) && lastDay == currentDay) return;
        SnapEvents.StarvationCooldowns[pawnId] = currentDay;

        SnapEvents.Snap(needPawn, 16, "CS_PawnSnapped_Starvation");
    }
}

[Patch]
public abstract class SnapFromWithdrawalPatch : Need_Chemical {
    protected SnapFromWithdrawalPatch(Pawn newPawn) : base(newPawn) { }

    [Inject(At.Return, nameof(NeedInterval))]
    private void AfterNeedInterval() {
        Need_Chemical self = this;
        if (self.CurCategory != DrugDesireCategory.Withdrawal) return;
        Pawn needPawn = pawn;
        if (needPawn == null || needPawn.Dead) return;

        Hediff_Addiction addiction = self.AddictionHediff;
        if (addiction == null) return;

        if (SnapEvents.WithdrawalCooldowns.Add(addiction.loadID)) {
            SnapEvents.Snap(needPawn, 14, "CS_PawnSnapped_Withdrawal");
        }
    }
}

[Patch]
public abstract class SnapFromFailedSurgeryPatch : Recipe_Surgery {
    [Inject(At.Return, nameof(CheckSurgeryFail))]
    private void AfterCheckSurgeryFail(Pawn patient, ControlHandle<bool> ch) {
        if (!ch.ReturnValue) return;
        SnapEvents.Snap(patient, 10, "CS_PawnSnapped_FailedSurgery");
    }
}

[Patch]
public abstract class SnapFromOrganHarvestPatch : Recipe_RemoveBodyPart {
    [Inject(At.Return, nameof(ApplyOnPawn))]
    private void AfterApplyOnPawn(Pawn pawn) {
        if (pawn.Dead) return;
        SnapEvents.Snap(pawn, 6, "CS_PawnSnapped_OrganHarvest");
    }
}

[Patch]
public abstract class SnapFromSocialFightPatch : MentalState_SocialFighting {
    [Inject(At.Return, nameof(PostEnd))]
    private void AfterPostEnd() {
        Pawn statePawn = pawn;
        if (statePawn == null || statePawn.Dead) return;
        SnapEvents.Snap(statePawn, 10, "CS_PawnSnapped_SocialFight");
    }
}

[Patch]
public abstract class SnapFromBreakupPatch : InteractionWorker_Breakup {
    [Inject(At.Return, nameof(Interacted))]
    private void AfterInteracted(Pawn initiator, Pawn recipient) {
        if (recipient is { Dead: false }) SnapEvents.Snap(recipient, 12, "CS_PawnSnapped_Breakup");
        if (initiator is { Dead: false }) SnapEvents.Snap(initiator, 12, "CS_PawnSnapped_Breakup");
    }
}

[Patch]
public abstract class SnapFromArrestPatch : Pawn_GuestTracker {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected SnapFromArrestPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Head, nameof(SetGuestStatus))]
    private void BeforeSetGuestStatus(GuestStatus guestStatus) {
        if (guestStatus != GuestStatus.Prisoner) return;
        Pawn pawn = trackedPawn;
        if (pawn == null || pawn.Dead) return;
        if (!pawn.IsFreeColonist) return;
        SnapEvents.Snap(pawn, 6, "CS_PawnSnapped_Arrest");
    }
}

[Patch]
public abstract class SnapFromLightningPatch : WeatherEvent_LightningStrike {
    protected SnapFromLightningPatch(Map map) : base(map) { }

    [Inject(At.Return, nameof(DoStrike))]
    private static void AfterDoStrike(IntVec3 strikeLoc, Map map) {
        if (map == null) return;
        List<Verse.Thing> things = map.thingGrid.ThingsListAt(strikeLoc);
        for (int i = 0; i < things.Count; i++) {
            if (things[i] is Pawn pawn && !pawn.Dead) {
                SnapEvents.Snap(pawn, 4, "CS_PawnSnapped_Lightning");
            }
        }

        foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(strikeLoc, Rot4.North, IntVec2.One)) {
            if (!cell.InBounds(map)) continue;
            List<Verse.Thing> adjacent = map.thingGrid.ThingsListAt(cell);
            for (int i = 0; i < adjacent.Count; i++) {
                if (adjacent[i] is Pawn pawn && !pawn.Dead) {
                    SnapEvents.Snap(pawn, 4, "CS_PawnSnapped_Lightning");
                }
            }
        }
    }
}

[Patch]
public abstract class SnapFromColonyDestructionPatch : Building {
    [Inject(At.Head, nameof(Destroy))]
    private void BeforeDestroy(DestroyMode mode) {
        Building self = this;
        if (mode != DestroyMode.KillFinalize) return;
        if (self.Faction == null || !self.Faction.IsPlayer) return;
        Map map = self.Map;
        if (map == null) return;

        int mapId = map.uniqueID;
        int currentTick = Find.TickManager.TicksGame;

        if (SnapEvents.ColonyDestructionTracker.TryGetValue(
                mapId,
                out (int startTick, int count, bool triggered) data
            )) {
            if (currentTick - data.startTick > 2500) {
                SnapEvents.ColonyDestructionTracker[mapId] = (currentTick, 1, false);
            } else {
                int newCount = data.count + 1;
                if (newCount >= 3 && !data.triggered) {
                    SnapEvents.ColonyDestructionTracker[mapId] = (data.startTick, newCount, true);
                    List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
                    for (int i = 0; i < colonists.Count; i++) {
                        SnapEvents.Snap(colonists[i], 10, "CS_PawnSnapped_ColonyDestruction");
                    }
                } else {
                    SnapEvents.ColonyDestructionTracker[mapId] = (data.startTick, newCount, data.triggered);
                }
            }
        } else {
            SnapEvents.ColonyDestructionTracker[mapId] = (currentTick, 1, false);
        }
    }
}
