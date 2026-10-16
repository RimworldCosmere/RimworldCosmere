using System;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Extension;
using Cosmere.Core.Gene;
using Cosmere.Core.Investiture;
using Cosmere.Core.Savant;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.LetterArrive;
using Cosmere.System.Roshar.Savant;
using Cosmere.System.Roshar.Surgebinding;
using Cosmere.System.Roshar.Surgebinding.Ability;
using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;
using Cosmere.System.Roshar.Util;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using static Cosmere.System.Roshar.RadiantOrderDefOf;
using RadiantOrder = Cosmere.System.Roshar.DefModExtension.RadiantOrder;

namespace Cosmere.System.Roshar.Gene;

public class Surgebinder : Invested {
    /// <summary>Placeholder name for the radiant order in keyed strings.</summary>
    private const string OrderArg = "ORDER";

    private static readonly List<int> SkillRequirements = [0, 4, 8, 14, 18];
    private static readonly List<int> IdealCooldownDays = [0, 0, 3, 7, 15];
    private static readonly HediffDef LifelightDef = HediffDefOf.Cosmere_Roshar_Hediff_NW_BoonPassive_Lifelight;

    private int artPiecesCreated;
    private int artPiecesExcellent;
    private int artPiecesGood;
    private int artPiecesLegendary;

    public Pawn? bondedSpren;

    private Dictionary<string, int> cachedSavantStages = [];

    private int CurrentIdealInt;
    public string godsprenName = string.Empty;

    private int griefRecoveryCount;

    private int lastCreativeOutputTick = -1;
    private int lastIdealChangeTick = -1;
    private int lastSkillGainTick = -1;

    private int lastZoneComplianceCheckTick = -1;
    private bool lostCloseRelationship;
    private int mentalBreaksSurvived;
    private bool pendingOath;
    private bool recoveredFromMajorHediff;
    private Dictionary<string, float> savantDecayOffsets = [];
    private int traumaEventCount;
    private bool wasDownedInCombat;
    private bool wasImprisoned;
    private bool zoneViolatedToday;

    public int CurrentIdeal {
        get => CurrentIdealInt;
        set {
            CurrentIdealInt = Math.Clamp(value, 0, 4);
            OnIdealChange();
        }
    }

    public int CurrentIdealDisplay => CurrentIdeal + 1;

    public RadiantOrder RadiantOrder =>
        def.GetModExtension<RadiantOrder>() ??
        throw new InvalidOperationException($"Surgebinder def '{def.defName}' is missing RadiantOrder mod extension");

    public RadiantOrderDef RadiantOrderDef => RadiantOrder.order;

    protected override Color BarColor => RadiantOrderDef.color.SaturationChanged(1f);

    protected override Color BarHighlightColor => RadiantOrderDef.color.SaturationChanged(2f);

    private SkillRecord Skill => pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower);

    public override List<AbilityDef> Abilities => [.. RadiantOrderDef.GetAbilities(CurrentIdeal)];

    public override string ResourceLabel {
        get {
            if (LifelightDef != null && pawn.health?.hediffSet?.HasHediff(LifelightDef) == true) {
                return "lifelight";
            }

            return def.resourceLabel;
        }
    }

    public override string InvestitureLabel {
        get {
            if (LifelightDef != null && pawn.health?.hediffSet?.HasHediff(LifelightDef) == true) {
                return "Lifelight";
            }

            return "Stormlight";
        }
    }

    /// <summary>
    ///     Gets the Stormlight ceiling shared by every Nahel bond on this pawn. The most advanced
    ///     order sets it, so a second bond never shrinks it.
    /// </summary>
    /// <remarks>
    ///     Reading only this gene would let gene-list order decide the capacity.
    /// </remarks>
    public override float MaxInvestitureLevel {
        get {
            float ceiling = 0f;

            List<Verse.Gene>? all = pawn.genes?.GenesListForReading;
            for (int i = 0; all != null && i < all.Count; i++) {
                if (all[i] is not Surgebinder bond || bond.Overridden) continue;

                List<Ideal> ideals = bond.RadiantOrderDef.ideals;
                if (bond.CurrentIdeal >= ideals.Count) continue;

                int stormlightMax = ideals[bond.CurrentIdeal].stormlightMax;
                if (stormlightMax > ceiling) ceiling = stormlightMax;
            }

            return ceiling > 0f ? ceiling : 1f;
        }
    }

    public override float Max => Investiture.MaxLevel;

    public override float Value {
        get => Investiture.CurLevel;
        set => Investiture.CurLevel = value;
    }

    public int GriefRecoveryCount => griefRecoveryCount;

    public bool WasDownedInCombat {
        get => wasDownedInCombat;
        set => wasDownedInCombat = value;
    }

    public int ArtPiecesCreated => artPiecesCreated;

    public int ArtPiecesGood => artPiecesGood;

    public int ArtPiecesExcellent => artPiecesExcellent;

    public int ArtPiecesLegendary => artPiecesLegendary;

    public int MentalBreaksSurvived => mentalBreaksSurvived;

    public bool LostCloseRelationship => lostCloseRelationship;

    public bool RecoveredFromMajorHediff => recoveredFromMajorHediff;

    public bool WasImprisoned => wasImprisoned;

    public int TraumaEventCount => traumaEventCount;

    internal bool PendingOath => pendingOath;

    internal int LastIdealChangeTick => lastIdealChangeTick;

    public ILoadReferenceable? GetBondTarget() {
        return bondedSpren;
    }

    public void NotifyCreativeOutput() {
        lastCreativeOutputTick = Find.TickManager.TicksGame;
    }

    public void NotifySkillGain() {
        lastSkillGainTick = Find.TickManager.TicksGame;
    }

    public void RegressIdeal() {
        if (CurrentIdealInt <= 0) return;

        int previousIdeal = CurrentIdealInt;
        CurrentIdealInt = Math.Max(0, CurrentIdealInt - 1);

        ILoadReferenceable? bondTarget = GetBondTarget();
        if (bondTarget != null) {
            SpiritWeb.Instance?.SetConnection(pawn, bondTarget, 0.6f);
        }

        OnIdealChange();

        string orderLabel = RadiantOrderDef.LabelCap;
        Find.LetterStack.ReceiveLetter(
            "CRO_IdealRegressed_Title".Translate(pawn.NameShortColored.Named("PAWN")),
            "CRO_IdealRegressed_Text".Translate(
                pawn.NameFullColored.Named("PAWN"),
                orderLabel.Named(OrderArg),
                (previousIdeal + 1).Named("OLDIDEAL"),
                (CurrentIdealInt + 1).Named("NEWIDEAL")
            ),
            RimWorld.LetterDefOf.NegativeEvent,
            pawn
        );

        Log.Info(
            $"RegressIdeal: {pawn.NameShortColored} regressed from ideal {previousIdeal + 1} to {CurrentIdealInt + 1}"
        );
    }

    public void CatastrophicBondDeath() {
        int idealBeforeDeath = CurrentIdeal;
        bool isBondsmith = RadiantOrderDef == RadiantOrderDefOf.Bondsmith;
        Log.Info(
            $"CatastrophicBondDeath: {pawn.NameShortColored}, ideal={idealBeforeDeath}, bondsmith={isBondsmith}"
        );
        DeregisterFromTrackers(isBondsmith);
        ApplyBondDeathEffects();
        bool droppedBlade = DropDeadBladeAndKillSpren(isBondsmith, idealBeforeDeath);
        SendBondDeathLetter(isBondsmith, droppedBlade);
        godsprenName = string.Empty;
        pawn.genes?.RemoveGene(this);
    }

    private void DeregisterFromTrackers(bool isBondsmith) {
        RadiantTracker tracker = Current.Game.GetComponent<RadiantTracker>();
        tracker?.RecordBrokenBond(pawn, RadiantOrderDef.defName);
        if (!isBondsmith) return;
        tracker?.UnregisterBondsmith();
        BondsmithCallingChecker? checker = Current.Game.GetComponent<BondsmithCallingChecker>();
        if (!string.IsNullOrEmpty(godsprenName)) {
            checker?.UnregisterGodspren(godsprenName);
        }
    }

    private void ApplyBondDeathEffects() {
        Verse.Hediff? strainedBond =
            pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Roshar_Hediff_StrainedBond);
        if (strainedBond != null) {
            pawn.health!.RemoveHediff(strainedBond);
        }

        Thought_Memory brokenBondThought = ThoughtMaker.MakeThought(ThoughtDefOf.Cosmere_Roshar_Thought_BrokenBond, 0);
        brokenBondThought.permanent = true;
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(brokenBondThought);
    }

    private bool DropDeadBladeAndKillSpren(bool isBondsmith, int idealBeforeDeath) {
        if (bondedSpren == null || isBondsmith) return false;

        bool droppedBlade = TryDropDeadBlade(idealBeforeDeath);
        if (!bondedSpren.Dead && !bondedSpren.Destroyed) {
            KillBondedSpren();
        }

        bondedSpren = null;
        return droppedBlade;
    }

    private bool TryDropDeadBlade(int idealBeforeDeath) {
        if (idealBeforeDeath < 2 || pawn.Map == null) return false;

        ThingDef bladeDef = ThingDefOf.Cosmere_Roshar_MeleeWeapon_DeadShardblade;
        if (bladeDef == null) return false;

        Verse.Thing deadBlade = ThingMaker.MakeThing(bladeDef, RadiantOrderDef.gemstone?.Item);
        string? sprenName = bondedSpren!.Name?.ToStringShort;
        if (sprenName != null) {
            deadBlade.TryGetComp<CompQuality>()
                ?.SetQuality(QualityCategory.Normal, ArtGenerationContext.Colony);
        }

        return GenPlace.TryPlaceThing(deadBlade, pawn.Position, pawn.Map, ThingPlaceMode.Near);
    }

    private void KillBondedSpren() {
        IntVec3 deathPos = bondedSpren!.PositionHeld;
        Map? deathMap = bondedSpren.MapHeld;
        bondedSpren.Kill(null);
        if (deathMap != null) {
            List<Verse.Thing> atCell = deathPos.GetThingList(deathMap);
            for (int i = atCell.Count - 1; i >= 0; i--) {
                if (atCell[i] is Corpse corpse && corpse.InnerPawn == bondedSpren) {
                    corpse.Destroy();
                    break;
                }
            }
        }

        if (!bondedSpren.Destroyed) bondedSpren.Destroy();
    }

    private void SendBondDeathLetter(bool isBondsmith, bool droppedBlade) {
        string bodyKey = isBondsmith ? "CRO_BondDeath_Bondsmith_Text" : "CRO_BondDeath_Text";
        string bodyText = bodyKey.Translate(
            pawn.NameFullColored.Named("PAWN"),
            RadiantOrderDef.LabelCap.Named(OrderArg),
            godsprenName.Named("SPREN")
        );
        if (droppedBlade) {
            bodyText += "\n\n" +
                        "CRO_DeadBlade_Text".Translate(
                            pawn.NameFullColored.Named("PAWN"),
                            RadiantOrderDef.LabelCap.Named(OrderArg)
                        );
        }

        Find.LetterStack.ReceiveLetter(
            "CRO_BondDeath_Title".Translate(pawn.NameShortColored.Named("PAWN")),
            bodyText,
            RimWorld.LetterDefOf.Death,
            pawn
        );
    }

    public void OnWitnessedDeath(Pawn deceased) {
        // hook the death-witness patch always calls. no order reacts to a witnessed death yet.
    }

    public void OnGriefRecovered() {
        griefRecoveryCount++;
    }

    public void OnArtCreated(QualityCategory quality) {
        artPiecesCreated++;
        if (quality >= QualityCategory.Good) artPiecesGood++;
        if (quality >= QualityCategory.Excellent) artPiecesExcellent++;
        if (quality >= QualityCategory.Legendary) artPiecesLegendary++;
    }

    public void OnMentalBreakSurvived() {
        mentalBreaksSurvived++;
        traumaEventCount++;
    }

    public void OnRelationshipLost() {
        lostCloseRelationship = true;
        traumaEventCount++;
    }

    public void OnMajorHediffRecovery() {
        recoveredFromMajorHediff = true;
        traumaEventCount++;
    }

    public void OnImprisoned() {
        wasImprisoned = true;
        traumaEventCount++;
    }

    private void OnIdealChange() {
        lastIdealChangeTick = Find.TickManager.TicksGame;
        pawn.story.EnsureTrait(RadiantOrder.trait, CurrentIdealInt);
        UpdateAbilities();
        EnableSoulcastWorkType();

        int idealIndex = CurrentIdealInt - 1;
        if (idealIndex >= 0 && idealIndex < RadiantOrderDef.ideals.Count) {
            int newMax = RadiantOrderDef.ideals[idealIndex].stormlightMax;
            if (newMax > 0) {
                InvestitureHolder.maxInvestitureSelf = newMax;
                Investiture.CurLevel = newMax;
            }
        }

        InvestitureHolder.drainRate = GetDrainRate();

        if (godsprenName == "Sibling") {
            ApplyUrithuruBlessing();
        }

        TryTransitionEyeColor();
    }

    private void TryTransitionEyeColor() {
        if (CurrentIdealInt < 1) return;
        if (!CasteUtility.IsDarkeyes(pawn)) return;

        CasteUtility.DarkeyesToLighteyes(pawn);

        Find.LetterStack.ReceiveLetter(
            "CRO_EyesOfLight_Title".Translate(),
            "CRO_EyesOfLight_Text".Translate(pawn.Named("PAWN")),
            RimWorld.LetterDefOf.PositiveEvent,
            pawn
        );
    }

    private void ApplyUrithuruBlessing() {
        HediffDef blessingDef = HediffDefOf.Cosmere_Roshar_Hediff_UrithuruBlessing;
        Verse.Hediff? existing = pawn.health.hediffSet.GetFirstHediffOfDef(blessingDef);
        if (existing == null) {
            existing = HediffMaker.MakeHediff(blessingDef, pawn);
            pawn.health.AddHediff(existing);
        }

        existing.Severity = CurrentIdealInt / 5f;
    }

    private float GetDrainRate() {
        if (CurrentIdeal >= 4) return 0f;

        float totalDrainTimeInSeconds = (10 * 60).TicksToSeconds();

        float baseRate = 1f / totalDrainTimeInSeconds;

        float idealMultiplier = 1f - CurrentIdeal / 4f;

        return baseRate * idealMultiplier;
    }

    public void NotifyZoneViolation() {
        zoneViolatedToday = true;
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (sources.Count > 0 && pawn.IsHashIntervalTick(UpkeepTicks, delta)) {
            DrainStormlightForAbilities();
        }

        TryLevelUp(delta);
        TrySkillUp(delta);
        TrackZoneCompliance(delta);
        TrackGraveProximity(delta);
        EnforceOathViolations(delta);
        SyncStrainedBondHediff(delta);
        UpdateSurgebindingSavantProgression(delta);
    }

    private void SyncStrainedBondHediff(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;

        ILoadReferenceable? bondTarget = GetBondTarget();
        if (bondTarget == null) return;

        float connection = SpiritWeb.Instance?.GetConnectionValue(pawn, bondTarget) ?? 0f;

        Verse.Hediff? existing =
            pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Roshar_Hediff_StrainedBond);

        if (connection < 1.0f && existing == null) {
            Verse.Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Cosmere_Roshar_Hediff_StrainedBond, pawn);
            hediff.Severity = 1f - connection;
            pawn.health?.AddHediff(hediff);
        } else if (connection >= 1.0f && existing != null) {
            pawn.health?.RemoveHediff(existing);
        }
    }

    private void DrainStormlightForAbilities() {
        float totalDrain = 0f;
        for (int i = 0; i < sources.Count; i++) {
            totalDrain += sources[i].Rate;
        }

        if (totalDrain <= 0) return;

        if (!CanLowerReserve(totalDrain)) {
            for (int i = sources.Count - 1; i >= 0; i--) {
                DrainSource source = sources[i];
                Ability? ability = pawn.abilities?.GetAbility(source.Def);
                if (ability is SurgebindingAbility surgeAbility) {
                    surgeAbility.UpdateStatus(Core.Ability.Active.Off);
                }
            }

            sources.Clear();
            return;
        }

        RemoveFromReserve(totalDrain);
    }

    /// <summary>
    ///     The pawn slowly just levels up their Surgebinding skill from talking with their
    ///     spren. 5 xp every 2000 ticks is pretty slow, but will eventually get pawns
    ///     leveled up, and on their way to higher ideals.
    /// </summary>
    private void TrySkillUp(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (CurrentIdeal >= 2) return;

        Skill.Learn(5, true, true);
    }

    private void TryLevelUp(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (CurrentIdeal >= 4) return;
        if (pendingOath) return;

        int nextIdeal = CurrentIdeal + 1;
        if (Skill.Level < SkillRequirements[nextIdeal]) return;
        if (!IsCooldownElapsed(nextIdeal)) return;
        if (!RadiantOrderDef.idealChecker.IsSatisfied(pawn, this, nextIdeal)) return;

        pendingOath = true;
        SendOathNotification(nextIdeal);
    }

    public void SpeakOath() {
        if (CurrentIdeal >= 4) return;

        int nextIdeal = CurrentIdeal + 1;
        if (!RadiantOrderDef.idealChecker.IsSatisfied(pawn, this, nextIdeal)) return;

        pendingOath = false;
        CurrentIdeal = nextIdeal;

        RadiantOrderDef.idealChecker.ConsummateOath(pawn, this, CurrentIdeal);
    }

    private bool IsCooldownElapsed(int nextIdeal) {
        if (lastIdealChangeTick < 0) return true;
        int cooldownTicks = IdealCooldownDays[nextIdeal] * GenDate.TicksPerDay;
        return Find.TickManager.TicksGame - lastIdealChangeTick >= cooldownTicks;
    }

    private void SendOathNotification(int nextIdeal) {
        Ideal ideal = RadiantOrderDef.ideals[nextIdeal];
        string oathText = ideal.quotes.Count > 0 ? ideal.quotes[0] : string.Empty;
        bool isTruth = RadiantOrderDef == RadiantOrderDefOf.Lightweaver;
        string idealLabel = isTruth ? "Truth" : "Ideal";

        string title = "CRO_SpeakOath_Title".Translate(
            pawn.NameShortColored.Named("PAWN"),
            RadiantOrderDef.LabelCap.Named(OrderArg),
            ideal.label.Named("IDEAL")
        );

        string text = "CRO_SpeakOath_Text".Translate(
            pawn.NameFullColored.Named("PAWN"),
            RadiantOrderDef.LabelCap.Named(OrderArg),
            ideal.label.Named("IDEAL"),
            oathText.Named("OATH"),
            idealLabel.Named("IDEALLABEL")
        );

        SpeakOath letter = (SpeakOath)LetterMaker.MakeLetter(
            title,
            text,
            LetterDefOf.Cosmere_Roshar_Letter_SpeakOath,
            pawn
        );
        letter.Setup(pawn, nextIdeal);
        Find.LetterStack.ReceiveLetter(letter);
    }

    private void TrackZoneCompliance(int delta) {
        if (RadiantOrderDef != Skybreaker) return;

        if (!zoneViolatedToday &&
            pawn.Map != null &&
            pawn.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap != null) {
            Area allowedArea = pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap;
            if (!allowedArea[pawn.Position]) {
                zoneViolatedToday = true;
            }
        }

        if (!pawn.IsHashIntervalTick(GenDate.TicksPerDay, delta)) return;

        if (!zoneViolatedToday) {
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ZoneComplianceDays, 1);
        } else {
            ViolationUtility.ApplyViolation(pawn, 0.1f, "leaving assigned zone");
        }

        zoneViolatedToday = false;
        lastZoneComplianceCheckTick = Find.TickManager.TicksGame;
    }

    private void TrackGraveProximity(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (RadiantOrderDef != Edgedancer) return;
        if (pawn.Map == null) return;

        List<Building> buildings = pawn.Map.listerBuildings.allBuildingsColonist;
        for (int i = 0; i < buildings.Count; i++) {
            if (buildings[i] is not Building_Grave) continue;
            if (pawn.Position.InHorDistOf(buildings[i].Position, 3f)) {
                pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_GraveVisits, 1);
                return;
            }
        }
    }

    private void EnforceOathViolations(int delta) {
        if (!pawn.IsHashIntervalTick(GenDate.TicksPerDay, delta)) return;

        int currentTick = Find.TickManager.TicksGame;

        if (RadiantOrderDef == Stoneward) {
            EnforceStonewardCaravanDuringSiege();
            if (pawn.Map != null) EnforceStonewardDraftedDuringRaid();
            return;
        }

        if (pawn.Map == null) return;

        if (RadiantOrderDef == Windrunner) EnforceWindrunnerIdleWhileDownedAllies();
        else if (RadiantOrderDef == Edgedancer) EnforceEdgedancerPrisonerNeglect();
        else if (RadiantOrderDef == Truthwatcher) EnforceTruthwatcherMedical();
        else if (RadiantOrderDef == Lightweaver) EnforceLightweaverStagnation(currentTick);
        else if (RadiantOrderDef == Elsecaller) EnforceElsecallerStagnation(currentTick);
    }

    private void EnforceWindrunnerIdleWhileDownedAllies() {
        if (pawn.Drafted) return;
        if (pawn.CurJobDef == RimWorld.JobDefOf.Rescue || pawn.CurJobDef == RimWorld.JobDefOf.TendPatient) return;

        bool hasDownedAlly = false;
        List<Pawn> mapPawns = pawn.Map.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < mapPawns.Count; i++) {
            if (mapPawns[i] == pawn) continue;
            if (!mapPawns[i].Downed) continue;

            hasDownedAlly = true;
            break;
        }

        if (!hasDownedAlly) return;

        bool isIdle = pawn.CurJobDef == RimWorld.JobDefOf.Wait_Wander ||
                      pawn.CurJobDef == RimWorld.JobDefOf.GotoWander ||
                      pawn.CurJobDef == RimWorld.JobDefOf.Wait;
        if (!isIdle) return;

        ViolationUtility.ApplyViolation(pawn, 0.1f, "idling while allies are downed");
    }

    private void EnforceEdgedancerPrisonerNeglect() {
        List<Pawn> prisoners = pawn.Map.mapPawns.PrisonersOfColonySpawned;
        for (int i = 0; i < prisoners.Count; i++) {
            Pawn prisoner = prisoners[i];
            if (prisoner.Dead) continue;

            bool starving = prisoner.needs?.food != null && prisoner.needs.food.Starving;
            bool hasBleeding = prisoner.health?.hediffSet?.HasTendableHediff() ?? false;

            if (starving || hasBleeding) {
                ViolationUtility.ApplyViolation(pawn, 0.1f, "neglecting prisoner welfare");
                return;
            }
        }
    }

    private void EnforceTruthwatcherMedical() {
        if (pawn.workSettings == null) return;

        bool doctorDisabled = pawn.workSettings.GetPriority(WorkTypeDefOf.Doctor) == 0;
        if (doctorDisabled) {
            ViolationUtility.ApplyViolation(pawn, 0.1f, "disabling medical work");
        }
    }

    private void EnforceLightweaverStagnation(int currentTick) {
        EnforceStagnation(ref lastCreativeOutputTick, currentTick, 0.3f, "creative stagnation");
    }

    private void EnforceElsecallerStagnation(int currentTick) {
        EnforceStagnation(ref lastSkillGainTick, currentTick, 0.1f, "intellectual stagnation");
    }

    private void EnforceStagnation(ref int lastTick, int currentTick, float violationWeight, string reason) {
        int stagnationThresholdTicks = 30 * GenDate.TicksPerDay;
        if (lastTick < 0) {
            lastTick = currentTick;
            return;
        }

        if (currentTick - lastTick >= stagnationThresholdTicks) {
            ViolationUtility.ApplyViolation(pawn, violationWeight, reason);
            lastTick = currentTick;
        }
    }

    private void EnforceStonewardDraftedDuringRaid() {
        if (CurrentIdeal < 2) return;

        if (!IsRaidActiveOnMap(pawn.Map)) return;

        bool isEngagedInCombat = pawn.Drafted ||
                                pawn.mindState?.meleeThreat != null ||
                                (pawn.CurJob?.def?.defName.Contains("Attack") ?? false) ||
                                pawn.stances.curStance is Stance_Busy;

        if (isEngagedInCombat) return;

        ViolationUtility.ApplyViolation(pawn, 0.1f, "refusing to fight during a raid");
    }

    private void EnforceStonewardCaravanDuringSiege() {
        if (pawn.GetCaravan() == null) return;

        List<Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++) {
            if (!IsRaidActiveOnMap(maps[i])) continue;

            ViolationUtility.ApplyViolation(pawn, 0.6f, "fleeing to caravan during a siege");
            return;
        }
    }

    private static bool IsRaidActiveOnMap(Map map) {
        if (map == null) return false;

        List<Lord> lords = map.lordManager.lords;
        for (int i = 0; i < lords.Count; i++) {
            if (lords[i].faction == null) continue;
            if (!lords[i].faction.HostileTo(Faction.OfPlayer)) continue;
            if (lords[i].LordJob is not LordJob_AssaultColony and not LordJob_AssaultThings) continue;

            return true;
        }

        return false;
    }

    public override void PostAdd() {
        Skill.Level = 0;
        base.PostAdd();
        OnIdealChange();
        Log.Debug($"{pawn.NameFullColored} has become a {RadiantOrderDef.LabelCap}");
    }

    private void EnableSoulcastWorkType() {
        if (pawn.workSettings == null || !pawn.workSettings.EverWork) return;
        if (pawn.abilities == null) return;

        List<Ability> abilities = pawn.abilities.AllAbilitiesForReading;
        bool hasSoulcast = false;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is Soulcast) {
                hasSoulcast = true;
                break;
            }
        }

        if (!hasSoulcast) return;

        pawn.Notify_DisabledWorkTypesChanged();

        WorkTypeDef? soulcastWork = RosharWorkTypeDefOf.Cosmere_Roshar_WorkType_Soulcasting;
        if (soulcastWork == null) return;
        if (pawn.WorkTypeIsDisabled(soulcastWork)) return;
        if (pawn.workSettings.GetPriority(soulcastWork) > 0) return;
        pawn.workSettings.SetPriority(soulcastWork, 3);
    }

    public override void PostRemove() {
        Skill.Level = 0;
        foreach (AbilityDef unlockedAbilityDef in RadiantOrderDef.GetAbilities(CurrentIdealInt)) {
            pawn.abilities.RemoveAbility(unlockedAbilityDef);
        }

        pawn.story.RemoveTrait(RadiantOrder.trait);
    }

    internal void DebugTriggerOath() {
        if (CurrentIdeal >= 4) {
            RejectIdealAdvance();
            return;
        }

        pendingOath = true;
        SendOathNotification(CurrentIdeal + 1);
    }

    internal void DebugSpeakOathNow() {
        // a silent no-op is unfixable from outside; every skip path here logs why it declined
        if (CurrentIdeal >= 4) {
            RejectIdealAdvance();
            return;
        }

        int nextIdeal = CurrentIdeal + 1;
        if (RadiantOrderDef.idealChecker.HasIncompatibleTrait(pawn, nextIdeal)) {
            string traitName = RadiantOrderDef.idealChecker.GetIncompatibleTraitName(pawn) ?? "unknown";
            Messages.Message(
                $"{pawn.NameShortColored} has an incompatible trait ({traitName}) for the next Ideal",
                pawn,
                MessageTypeDefOf.RejectInput
            );
            return;
        }

        pendingOath = false;
        CurrentIdeal = nextIdeal;
        RadiantOrderDef.idealChecker.ConsummateOath(pawn, this, CurrentIdeal);

        List<AbilityDef> granted = [.. RadiantOrderDef.GetAbilities(CurrentIdealInt)];
        Log.Info(
            $"{pawn.LabelShort}: {RadiantOrderDef.defName} ideal -> {CurrentIdealDisplay}, " +
            $"abilities now [{string.Join(", ", granted.Select(a => a.defName))}]"
        );
        Messages.Message(
            $"{pawn.NameShortColored} spoke the Ideal ({CurrentIdealDisplay}) - {granted.Count} ability(s)",
            pawn,
            MessageTypeDefOf.PositiveEvent
        );
    }

    private void RejectIdealAdvance() {
        Messages.Message(
            $"{pawn.NameShortColored} is already at the highest Ideal ({CurrentIdealDisplay})",
            pawn,
            MessageTypeDefOf.RejectInput
        );
    }

    internal void DebugResetCooldown() {
        lastIdealChangeTick = -1;
    }

    internal void UpdateAbilities() {
        foreach (AbilityDef unlockedAbilityDef in RadiantOrderDef.GetAbilities(CurrentIdealInt)) {
            pawn.abilities.GainAbility(unlockedAbilityDef);
        }

        if (RadiantOrderDef == RadiantOrderDefOf.Bondsmith &&
            !string.IsNullOrEmpty(godsprenName) &&
            CurrentIdealInt >= 2) {
            AddBondsmithSprenAbility();
        }
    }

    private void AddBondsmithSprenAbility() {
        AbilityDef? abilityDef = godsprenName switch {
            "Stormfather" => AbilityDefOf.Cosmere_Roshar_Ability_HonorsPerpendicularity,
            "Nightwatcher" => AbilityDefOf.Cosmere_Roshar_Ability_CultivationsPerpendicularity,
            "Sibling" => AbilityDefOf.Cosmere_Roshar_Ability_SiblingBlessing,
            _ => null,
        };
        if (abilityDef != null) pawn.abilities.GainAbility(abilityDef);

        if (godsprenName == "Sibling" && AbilityDefOf.Cosmere_Roshar_Ability_ReinforceStructure != null) {
            pawn.abilities.GainAbility(AbilityDefOf.Cosmere_Roshar_Ability_ReinforceStructure);
        }
    }

    private void UpdateSurgebindingSavantProgression(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;

        List<SurgeDef> surges = RadiantOrderDef.surges;
        for (int i = 0; i < surges.Count; i++) {
            SurgeDef surge = surges[i];
            UpdateSurgeSavant(surge);
        }
    }

    private void UpdateSurgeSavant(SurgeDef surge) {
        string surgeName = surge.defName;
        cachedSavantStages.TryGetValue(surgeName, out int previousStage);
        savantDecayOffsets.TryGetValue(surgeName, out float decayOffset);

        RecordDef? recordDef = surge.timeSpentRecord;
        if (recordDef == null) return;

        float ticks = pawn.records.GetValue(recordDef) - decayOffset;
        int newStage = SurgebindingSavantUtility.GetStage(ticks);

        if (newStage == 1 && !IsSurgeActive(surge)) {
            float decayAmount = ticks *
                                SavantUtility.Stage1DecayPerDayFraction /
                                GenDate.TicksPerDay *
                                GenTicks.TickLongInterval;
            savantDecayOffsets[surgeName] = decayOffset + decayAmount;
            ticks -= decayAmount;
            newStage = SurgebindingSavantUtility.GetStage(ticks);
        }

        cachedSavantStages[surgeName] = newStage;

        HediffDef? savantHediffDef = SurgebindingSavantUtility.GetSavantHediffDef(surgeName);
        HediffDef? permanentHediffDef = SurgebindingSavantUtility.GetPermanentHediffDef(surgeName);
        SavantUtility.UpdateSavantHediffState(pawn, previousStage, newStage, savantHediffDef, permanentHediffDef);

        if (newStage > previousStage && newStage > 0) {
            SavantUtility.SendSavantLetter(pawn, newStage, "Surgebinding", surge.LabelCap);
        }
    }

    private bool IsSurgeActive(SurgeDef surge) {
        List<AbilityDef> surgeAbilities = surge.abilities;
        for (int i = 0; i < sources.Count; i++) {
            for (int j = 0; j < surgeAbilities.Count; j++) {
                if (sources[i].Def == surgeAbilities[j]) return true;
            }
        }

        return false;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref CurrentIdealInt, "CurrentIdeal");
        Scribe_Values.Look(ref lastIdealChangeTick, "lastIdealChangeTick", -1);
        Scribe_Values.Look(ref pendingOath, "pendingOath");
        Scribe_References.Look(ref bondedSpren, "bondedSpren");
        Scribe_Values.Look(ref godsprenName, "godsprenName", string.Empty);
        Scribe_Values.Look(ref griefRecoveryCount, "griefRecoveryCount");
        Scribe_Values.Look(ref wasDownedInCombat, "wasDownedInCombat");
        Scribe_Values.Look(ref artPiecesCreated, "artPiecesCreated");
        Scribe_Values.Look(ref artPiecesGood, "artPiecesGood");
        Scribe_Values.Look(ref artPiecesExcellent, "artPiecesExcellent");
        Scribe_Values.Look(ref artPiecesLegendary, "artPiecesLegendary");
        Scribe_Values.Look(ref mentalBreaksSurvived, "mentalBreaksSurvived");
        Scribe_Values.Look(ref lostCloseRelationship, "lostCloseRelationship");
        Scribe_Values.Look(ref recoveredFromMajorHediff, "recoveredFromMajorHediff");
        Scribe_Values.Look(ref wasImprisoned, "wasImprisoned");
        Scribe_Values.Look(ref traumaEventCount, "traumaEventCount");
        Scribe_Values.Look(ref lastZoneComplianceCheckTick, "lastZoneComplianceCheckTick", -1);
        Scribe_Values.Look(ref zoneViolatedToday, "zoneViolatedToday");
        Scribe_Values.Look(ref lastCreativeOutputTick, "lastCreativeOutputTick", -1);
        Scribe_Values.Look(ref lastSkillGainTick, "lastSkillGainTick", -1);
        Scribe_Collections.Look(ref cachedSavantStages, "cachedSavantStages", LookMode.Value, LookMode.Value);
        cachedSavantStages ??= [];
        Scribe_Collections.Look(ref savantDecayOffsets, "savantDecayOffsets", LookMode.Value, LookMode.Value);
        savantDecayOffsets ??= [];
    }

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        if (!pawn.Spawned) yield break;

        foreach (Verse.Gizmo gizmo in GetShardEquipmentGizmos()) {
            yield return gizmo;
        }
    }

    private IEnumerable<Verse.Gizmo> GetShardEquipmentGizmos() {
        if (pawn.abilities == null) yield break;

        List<Ability> abilities = pawn.abilities.abilities;
        for (int i = 0; i < abilities.Count; i++) {
            Ability ability = abilities[i];
            if (ability.def != AbilityDefOf.Cosmere_Roshar_Ability_ToggleShardblade
                && ability.def != AbilityDefOf.Cosmere_Roshar_Ability_ToggleShardplate) {
                continue;
            }

            if (ability is SurgebindingAbility sa && !sa.GizmosVisible()) continue;

            foreach (Verse.Gizmo gizmo in ability.GetGizmos()) {
                yield return gizmo;
            }
        }
    }
}
