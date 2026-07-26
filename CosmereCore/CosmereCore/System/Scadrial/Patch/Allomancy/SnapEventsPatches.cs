using HarmonyLib;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Patch.Allomancy;

[HarmonyPatch]
public static class SnapEventsPatch {
    private static readonly AccessTools.FieldRef<MentalStateHandler, Pawn> mshPawn =
        AccessTools.FieldRefAccess<MentalStateHandler, Pawn>("pawn");

    private static readonly AccessTools.FieldRef<Pawn_HealthTracker, Pawn> phtPawn =
        AccessTools.FieldRefAccess<Pawn_HealthTracker, Pawn>("pawn");

    private static readonly AccessTools.FieldRef<Need, Pawn> needPawn =
        AccessTools.FieldRefAccess<Need, Pawn>("pawn");

    private static readonly PawnRelationDef[] CloseRelations = [
        PawnRelationDefOf.Spouse,
        PawnRelationDefOf.Lover,
        PawnRelationDefOf.Parent,
        PawnRelationDefOf.Child,
        PawnRelationDefOf.Bond,
    ];

    private static readonly Dictionary<int, int> StarvationCooldowns = new Dictionary<int, int>();

    private static readonly AccessTools.FieldRef<Pawn_GuestTracker, Pawn> guestPawn =
        AccessTools.FieldRefAccess<Pawn_GuestTracker, Pawn>("pawn");

    private static readonly HashSet<int> TemperatureCooldowns = [];
    private static readonly HashSet<int> WithdrawalCooldowns = [];

    private static readonly Dictionary<int, (int startTick, int count, bool triggered)> ColonyDestructionTracker =
        new Dictionary<int, (int startTick, int count, bool triggered)>();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
    public static void SnapFromMentalBreak(MentalStateHandler __instance, bool __result) {
        if (__result) Snap(mshPawn(__instance), 16);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.PostApplyDamage))]
    public static void SnapFromInjury(Pawn_HealthTracker __instance, DamageInfo dinfo) {
        Pawn pawn = phtPawn(__instance);
        if (pawn.Dead) return;

        if (dinfo.Def == DamageDefOf.Crush && dinfo.Instigator == null) {
            Snap(pawn, 6, "CS_PawnSnapped_RoofCollapse");
            return;
        }

        if (pawn.IsPrisonerOfColony && dinfo.Instigator is Pawn) {
            Snap(pawn, 12, "CS_PawnSnapped_Prisoner");
            return;
        }

        if (pawn.health.summaryHealth.SummaryHealthPercent > 0.2f) return;
        Snap(pawn, 16);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static void SnapFromDeathWitness(Pawn __instance, DamageInfo? dinfo) {
        if (__instance.relations == null) return;
        Map map = __instance.MapHeld;
        if (map == null) return;

        if (dinfo?.Instigator is Pawn killer && !killer.Dead && killer != __instance) {
            SnapFirstKill(killer);
        }

        List<DirectPawnRelation> relations = __instance.relations.DirectRelations;
        for (int i = 0; i < relations.Count; i++) {
            DirectPawnRelation rel = relations[i];
            if (!IsCloseRelation(rel.def)) continue;

            Pawn related = rel.otherPawn;
            if (related.Dead || related.Map != map) continue;

            SnapUtility.Snap(related, "CS_PawnSnapped_WitnessedDeath");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    public static void SnapFromDowned(Pawn_HealthTracker __instance, DamageInfo? dinfo) {
        if (dinfo == null) return;
        Pawn pawn = phtPawn(__instance);
        Snap(pawn, 16, "CS_PawnSnapped_Downed");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Need_Food), nameof(Need_Food.NeedInterval))]
    public static void SnapFromStarvation(Need_Food __instance) {
        if (__instance.CurLevelPercentage > 0.05f) return;

        Pawn pawn = needPawn(__instance);
        int pawnId = pawn.thingIDNumber;
        int currentDay = GenDate.DaysPassed;

        if (StarvationCooldowns.TryGetValue(pawnId, out int lastDay) && lastDay == currentDay) return;
        StarvationCooldowns[pawnId] = currentDay;

        Snap(pawn, 16, "CS_PawnSnapped_Starvation");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.HealthTickInterval))]
    public static void SnapFromTemperature(Pawn_HealthTracker __instance) {
        Pawn pawn = phtPawn(__instance);
        if (pawn.Dead) return;
        HediffSet? hediffSet = pawn.health?.hediffSet;
        if (hediffSet == null) return;

        Hediff hypo = hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.Hypothermia);
        if (hypo != null && hypo.Severity >= 0.7f) {
            if (TemperatureCooldowns.Add(hypo.loadID)) {
                Snap(pawn, 12, "CS_PawnSnapped_Temperature");
            }

            return;
        }

        Hediff heat = hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.Heatstroke);
        if (heat != null && heat.Severity >= 0.7f) {
            if (TemperatureCooldowns.Add(heat.loadID)) {
                Snap(pawn, 12, "CS_PawnSnapped_Temperature");
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Need_Chemical), nameof(Need_Chemical.NeedInterval))]
    public static void SnapFromWithdrawal(Need_Chemical __instance) {
        if (__instance.CurCategory != DrugDesireCategory.Withdrawal) return;
        Pawn pawn = needPawn(__instance);
        if (pawn == null || pawn.Dead) return;

        Hediff_Addiction addiction = __instance.AddictionHediff;
        if (addiction == null) return;

        if (WithdrawalCooldowns.Add(addiction.loadID)) {
            Snap(pawn, 14, "CS_PawnSnapped_Withdrawal");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(
        typeof(Pawn_HealthTracker),
        "AddHediff",
        typeof(Hediff),
        typeof(BodyPartRecord),
        typeof(DamageInfo?),
        typeof(DamageWorker.DamageResult)
    )]
    public static void SnapFromLimbLoss(Hediff hediff) {
        if (hediff.def != RimWorld.HediffDefOf.MissingBodyPart) return;
        Pawn pawn = hediff.pawn;
        if (pawn == null || pawn.Dead) return;
        Snap(pawn, 8, "CS_PawnSnapped_LimbLoss");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Recipe_Surgery), "CheckSurgeryFail")]
    public static void SnapFromFailedSurgery(bool __result, Pawn patient) {
        if (!__result) return;
        Snap(patient, 10, "CS_PawnSnapped_FailedSurgery");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Recipe_RemoveBodyPart), nameof(Recipe_RemoveBodyPart.ApplyOnPawn))]
    public static void SnapFromOrganHarvest(Pawn pawn) {
        if (pawn.Dead) return;
        Snap(pawn, 6, "CS_PawnSnapped_OrganHarvest");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MentalState_SocialFighting), nameof(MentalState_SocialFighting.PostEnd))]
    public static void SnapFromSocialFight(MentalState_SocialFighting __instance) {
        Pawn pawn = __instance.pawn;
        if (pawn == null || pawn.Dead) return;
        Snap(pawn, 10, "CS_PawnSnapped_SocialFight");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(InteractionWorker_Breakup), nameof(InteractionWorker_Breakup.Interacted))]
    public static void SnapFromBreakup(Pawn initiator, Pawn recipient) {
        if (recipient is { Dead: false }) Snap(recipient, 12, "CS_PawnSnapped_Breakup");
        if (initiator is { Dead: false }) Snap(initiator, 12, "CS_PawnSnapped_Breakup");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
    public static void SnapFromArrest(Pawn_GuestTracker __instance, GuestStatus guestStatus) {
        if (guestStatus != GuestStatus.Prisoner) return;
        Pawn pawn = guestPawn(__instance);
        if (pawn == null || pawn.Dead) return;
        if (!pawn.IsFreeColonist) return;
        Snap(pawn, 6, "CS_PawnSnapped_Arrest");
    }

    private static void SnapFirstKill(Pawn killer) {
        if (killer.records == null) return;
        int kills = (int)killer.records.GetValue(RimWorld.RecordDefOf.KillsHumanlikes);
        if (kills > 1) return;

        Snap(killer, 4, "CS_PawnSnapped_FirstKill");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WeatherEvent_LightningStrike), nameof(WeatherEvent_LightningStrike.DoStrike))]
    public static void SnapFromLightning(IntVec3 strikeLoc, Map map) {
        if (map == null) return;
        List<Verse.Thing> things = map.thingGrid.ThingsListAt(strikeLoc);
        for (int i = 0; i < things.Count; i++) {
            if (things[i] is Pawn pawn && !pawn.Dead) {
                Snap(pawn, 4, "CS_PawnSnapped_Lightning");
            }
        }

        foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(strikeLoc, Rot4.North, IntVec2.One)) {
            if (!cell.InBounds(map)) continue;
            List<Verse.Thing> adjacent = map.thingGrid.ThingsListAt(cell);
            for (int i = 0; i < adjacent.Count; i++) {
                if (adjacent[i] is Pawn pawn && !pawn.Dead) {
                    Snap(pawn, 4, "CS_PawnSnapped_Lightning");
                }
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Building), nameof(Building.Destroy))]
    public static void SnapFromColonyDestruction(Building __instance, DestroyMode mode) {
        if (mode != DestroyMode.KillFinalize) return;
        if (__instance.Faction == null || !__instance.Faction.IsPlayer) return;
        Map map = __instance.Map;
        if (map == null) return;

        int mapId = map.uniqueID;
        int currentTick = Find.TickManager.TicksGame;

        if (ColonyDestructionTracker.TryGetValue(mapId, out (int startTick, int count, bool triggered) data)) {
            if (currentTick - data.startTick > 2500) {
                ColonyDestructionTracker[mapId] = (currentTick, 1, false);
            }
            else {
                int newCount = data.count + 1;
                if (newCount >= 3 && !data.triggered) {
                    ColonyDestructionTracker[mapId] = (data.startTick, newCount, true);
                    List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
                    for (int i = 0; i < colonists.Count; i++) {
                        Snap(colonists[i], 10, "CS_PawnSnapped_ColonyDestruction");
                    }
                }
                else {
                    ColonyDestructionTracker[mapId] = (data.startTick, newCount, data.triggered);
                }
            }
        }
        else {
            ColonyDestructionTracker[mapId] = (currentTick, 1, false);
        }
    }

    private static void Snap(Pawn pawn, int oneInChance, string? cause = null) {
        if (!Rand.Chance(1f / oneInChance)) return;
        SnapUtility.Snap(pawn, cause);
    }

    private static bool IsCloseRelation(PawnRelationDef def) {
        for (int i = 0; i < CloseRelations.Length; i++) {
            if (CloseRelations[i] == def) return true;
        }

        return false;
    }
}