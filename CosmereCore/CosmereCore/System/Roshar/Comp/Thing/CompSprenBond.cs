using System.Collections.Generic;
using Cosmere.Core.Comp.Game;
using RimWorld;
using Verse;
using Verse.AI;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Comp.Thing;

public class CompSprenBond : ThingComp {
    private static List<TraitDef>? personalityTraitPool;

    private const int MinAutonomyDurationTicks = 7200;
    private const float AutonomyToggleChance = 0.10f;

    private Verse.Pawn? bondedRadiant;
    private bool dismissed;
    private bool autoDismissed;
    private bool sleepDismissed;
    private bool autonomous = true;
    private int lastDismissSummonTick = -999;
    private int lastAutonomyToggleTick = -999;
    private List<TraitDef> personalityTraits = [];

    public Verse.Pawn? BondedRadiant => bondedRadiant;
    public bool Dismissed => dismissed;
    public bool Autonomous => autonomous;
    public bool CooldownActive => GenTicks.TicksGame - lastDismissSummonTick < 60;
    public IReadOnlyList<TraitDef> PersonalityTraits => personalityTraits;

    public void ToggleAutonomy() {
        autonomous = !autonomous;
    }
    private Verse.Pawn Spren => (Verse.Pawn)parent;

    private static List<TraitDef> GetTraitPool() {
        if (personalityTraitPool != null) return personalityTraitPool;

        personalityTraitPool = [];
        string[] traitNames = ["Kind", "Abrasive", "Nerves", "NaturalMood", "Tough", "Industriousness", "SpeedOffset"];
        for (int i = 0; i < traitNames.Length; i++) {
            TraitDef? trait = DefDatabase<TraitDef>.GetNamedSilentFail(traitNames[i]);
            if (trait != null) personalityTraitPool.Add(trait);
        }

        return personalityTraitPool;
    }

    public void SetupBond(Verse.Pawn radiant, List<TraitDef>? traits = null) {
        bondedRadiant = radiant;
        dismissed = false;
        personalityTraits = traits ?? RollPersonalityTraits();
        ApplyTraitsToStory();
    }

    private void ApplyTraitsToStory() {
        if (Spren.story == null) return;
        for (int i = 0; i < personalityTraits.Count; i++) {
            if (Spren.story.traits.HasTrait(personalityTraits[i])) continue;
            int degree = personalityTraits[i].degreeDatas.Count > 0
                ? personalityTraits[i].degreeDatas[0].degree
                : 0;
            Spren.story.traits.GainTrait(new Trait(personalityTraits[i], degree, true));
        }
    }

    private static List<TraitDef> RollPersonalityTraits() {
        List<TraitDef> pool = GetTraitPool();
        if (pool.Count == 0) return [];

        List<TraitDef> result = [];
        List<TraitDef> available = [..pool];
        int count = Rand.RangeInclusive(1, 2);
        for (int i = 0; i < count && available.Count > 0; i++) {
            int idx = Rand.Range(0, available.Count);
            result.Add(available[idx]);
            available.RemoveAt(idx);
        }

        return result;
    }

    public float GetViolationSeverityMultiplier() {
        float multiplier = 1f;
        for (int i = 0; i < personalityTraits.Count; i++) {
            string defName = personalityTraits[i].defName;
            if (defName == "Kind") multiplier *= 0.8f;
            else if (defName == "Abrasive") multiplier *= 1.2f;
            else if (defName == "Nerves") multiplier *= 0.85f;
        }

        return multiplier;
    }

    public float GetRecoveryMultiplier() {
        float multiplier = 1f;
        for (int i = 0; i < personalityTraits.Count; i++) {
            string defName = personalityTraits[i].defName;
            if (defName == "NaturalMood") multiplier *= 1.1f;
            else if (defName == "Industriousness") multiplier *= 1.1f;
        }

        return multiplier;
    }

    public void Dismiss() {
        if (dismissed || CooldownActive) return;
        dismissed = true;
        lastDismissSummonTick = GenTicks.TicksGame;

        if (Spren.Spawned) {
            Spren.DeSpawn();
        }

        if (!Find.WorldPawns.Contains(Spren)) {
            Find.WorldPawns.PassToWorld(Spren, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);
        }

        Logger.Verbose($"Spren {Spren.NameFullColored} dismissed to Cognitive Realm");
    }

    public void Summon(Verse.Map map, IntVec3 nearPosition) {
        if (!dismissed || CooldownActive) return;
        if (Spren == null || Spren.Destroyed) return;
        dismissed = false;
        lastDismissSummonTick = GenTicks.TicksGame;

        if (Find.WorldPawns.Contains(Spren)) {
            Find.WorldPawns.RemovePawn(Spren);
        }

        IntVec3 cell = CellFinder.RandomSpawnCellForPawnNear(nearPosition, map, 2);
        GenSpawn.Spawn(Spren, cell, map);

        Logger.Verbose($"Spren {Spren.NameFullColored} summoned from Cognitive Realm");
    }

    public override void CompTick() {
        base.CompTick();
        if (bondedRadiant == null) return;

        if (dismissed) {
            if (autoDismissed && bondedRadiant.Spawned && bondedRadiant.Map != null && !Spren.Spawned) {
                autoDismissed = false;
                Summon(bondedRadiant.Map, bondedRadiant.Position);
            } else if (sleepDismissed && bondedRadiant.Spawned && bondedRadiant.Map != null && !bondedRadiant.IsAsleep()) {
                sleepDismissed = false;
                Summon(bondedRadiant.Map, bondedRadiant.Position);
            } else if (autonomous && bondedRadiant.Spawned && bondedRadiant.Map != null && !bondedRadiant.IsAsleep()) {
                TickAutonomous();
            }
            return;
        }

        if (!Spren.Spawned) return;

        if (bondedRadiant.Map == null || bondedRadiant.Map != Spren.Map) {
            autoDismissed = true;
            Dismiss();
            return;
        }

        if (bondedRadiant.IsAsleep()) {
            sleepDismissed = true;
            Dismiss();
            return;
        }

        if (Spren.IsHashIntervalTick(120)) {
            TickFollow();
            TickRecreation();
        }

        if (autonomous) {
            TickAutonomous();
        }
    }

    private void TickFollow() {
        if (bondedRadiant!.Map != Spren.Map) return;
        if (Spren.Drafted) return;

        float dist = Spren.Position.DistanceTo(bondedRadiant.Position);
        if (dist <= 3f) return;

        if (Spren.CurJobDef == RimWorld.JobDefOf.FollowClose) return;

        Verse.AI.Job followJob = JobMaker.MakeJob(RimWorld.JobDefOf.FollowClose, bondedRadiant);
        followJob.followRadius = 3f;
        followJob.locomotionUrgency = dist > 10f ? LocomotionUrgency.Sprint : LocomotionUrgency.Walk;
        followJob.expiryInterval = 600;
        followJob.checkOverrideOnExpire = true;
        Spren.jobs?.StartJob(followJob, JobCondition.InterruptOptional);
    }

    private void TickRecreation() {
        if (bondedRadiant?.needs?.joy == null) return;
        if (bondedRadiant.Map != Spren.Map) return;

        float dist = Spren.Position.DistanceTo(bondedRadiant.Position);
        if (dist > 5f) return;

        bondedRadiant.needs.joy.GainJoy(0.0002f, JoyKindDefOf.Social);

        if (Rand.MTBEventOccurs(2f, GenDate.TicksPerDay, 120f)) {
            Spren.interactions?.TryInteractWith(bondedRadiant, InteractionDefOf.Chitchat);
        }
    }

    private void TickAutonomous() {
        if (!Spren.IsHashIntervalTick(GenTicks.TickLongInterval)) return;
        if (GenTicks.TicksGame - lastAutonomyToggleTick < MinAutonomyDurationTicks) return;
        if (!Rand.Chance(AutonomyToggleChance)) return;

        lastAutonomyToggleTick = GenTicks.TicksGame;
        if (dismissed) {
            if (bondedRadiant!.Spawned && bondedRadiant.Map != null) {
                Summon(bondedRadiant.Map, bondedRadiant.Position);
            }
        } else {
            Dismiss();
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_References.Look(ref bondedRadiant, "bondedRadiant");
        Scribe_Values.Look(ref dismissed, "dismissed");
        Scribe_Values.Look(ref autoDismissed, "autoDismissed");
        Scribe_Values.Look(ref sleepDismissed, "sleepDismissed");
        Scribe_Values.Look(ref autonomous, "autonomous", true);
        Scribe_Values.Look(ref lastAutonomyToggleTick, "lastAutonomyToggleTick", -999);
        Scribe_Values.Look(ref lastDismissSummonTick, "lastDismissSummonTick", -999);
        Scribe_Collections.Look(ref personalityTraits, "personalityTraits", LookMode.Def);
        personalityTraits ??= [];
    }
}
