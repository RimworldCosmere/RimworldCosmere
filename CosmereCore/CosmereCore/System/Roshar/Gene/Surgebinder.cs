using System;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Gene;
using Cosmere.Core.Savant;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.DefModExtension;
using Cosmere.System.Roshar.LetterArrive;
using Cosmere.System.Roshar.Surgebinding;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Gene;

public class Surgebinder : Invested {
    private static readonly List<int> SkillRequirements = [0, 4, 8, 14, 18];
    private static readonly List<int> IdealCooldownDays = [0, 0, 3, 7, 15];

    private int currentIdealInt;
    private int lastIdealChangeTick = -1;
    private bool pendingOath;

    public Verse.Pawn? bondedSpren;
    public string godsprenName = "";

    private int griefRecoveryCount;
    private bool wasDownedInCombat;

    private int artPiecesCreated;
    private int artPiecesGood;
    private int artPiecesExcellent;
    private int artPiecesLegendary;
    private int mentalBreaksSurvived;
    private bool lostCloseRelationship;
    private bool recoveredFromMajorHediff;
    private bool wasImprisoned;
    private int traumaEventCount;

    private int lastZoneComplianceCheckTick = -1;
    private bool zoneViolatedToday;

    private int lastCreativeOutputTick = -1;
    private int lastSkillGainTick = -1;

    private Dictionary<string, int> cachedSavantStages = new();
    private Dictionary<string, float> savantDecayOffsets = new();

    public int currentIdeal {
        get => currentIdealInt;
        set {
            currentIdealInt = Math.Clamp(value, 0, 4);
            OnIdealChange();
        }
    }

    public int currentIdealDisplay => currentIdeal + 1;

    public RadiantOrder radiantOrder => def.GetModExtension<RadiantOrder>();
    public RadiantOrderDef radiantOrderDef => radiantOrder.order;
    protected override Color BarColor => radiantOrderDef.color.SaturationChanged(1f);
    protected override Color BarHighlightColor => radiantOrderDef.color.SaturationChanged(2f);
    private SkillRecord skill => pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower);
    private PawnTracker tracker => pawn.TryGetComp<PawnTracker>();
    public override List<AbilityDef> abilities => radiantOrderDef.GetAbilities(currentIdeal).ToList();

    public override string ResourceLabel {
        get {
            HediffDef lifelightDef = HediffDefOf.Cosmere_Roshar_Hediff_NW_BoonPassive_Lifelight;
            if (lifelightDef != null && pawn.health?.hediffSet?.HasHediff(lifelightDef) == true)
                return "lifelight";
            return def.resourceLabel;
        }
    }

    public override string investitureLabel {
        get {
            HediffDef lifelightDef = HediffDefOf.Cosmere_Roshar_Hediff_NW_BoonPassive_Lifelight;
            if (lifelightDef != null && pawn.health?.hediffSet?.HasHediff(lifelightDef) == true)
                return "Lifelight";
            return "Stormlight";
        }
    }

    public override float maxInvestitureLevel {
        get {
            if (currentIdeal >= 0 && currentIdeal < radiantOrderDef.ideals.Count) {
                int stormlightMax = radiantOrderDef.ideals[currentIdeal].stormlightMax;
                if (stormlightMax > 0) return stormlightMax;
            }
            return 1f;
        }
    }

    public override float Max => investiture.MaxLevel;

    public override float Value {
        get => investiture.CurLevel;
        set => investiture.CurLevel = value;
    }

    public int GriefRecoveryCount => griefRecoveryCount;
    public bool WasDownedInCombat { get => wasDownedInCombat; set => wasDownedInCombat = value; }

    public int ArtPiecesCreated => artPiecesCreated;
    public int ArtPiecesGood => artPiecesGood;
    public int ArtPiecesExcellent => artPiecesExcellent;
    public int ArtPiecesLegendary => artPiecesLegendary;
    public int MentalBreaksSurvived => mentalBreaksSurvived;
    public bool LostCloseRelationship => lostCloseRelationship;
    public bool RecoveredFromMajorHediff => recoveredFromMajorHediff;
    public bool WasImprisoned => wasImprisoned;
    public int TraumaEventCount => traumaEventCount;

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
        if (currentIdealInt <= 0) return;

        int previousIdeal = currentIdealInt;
        currentIdealInt = Math.Max(0, currentIdealInt - 1);

        ILoadReferenceable? bondTarget = GetBondTarget();
        if (bondTarget != null) {
            SpiritWeb.Instance?.SetConnection(pawn, bondTarget, 0.6f);
        }

        OnIdealChange();

        string orderLabel = radiantOrderDef.LabelCap;
        Find.LetterStack.ReceiveLetter(
            "CRO_IdealRegressed_Title".Translate(pawn.NameShortColored.Named("PAWN")),
            "CRO_IdealRegressed_Text".Translate(
                pawn.NameFullColored.Named("PAWN"),
                orderLabel.Named("ORDER"),
                (previousIdeal + 1).Named("OLDIDEAL"),
                (currentIdealInt + 1).Named("NEWIDEAL")
            ),
            RimWorld.LetterDefOf.NegativeEvent,
            pawn
        );

        Logger.Important($"RegressIdeal: {pawn.NameShortColored} regressed from ideal {previousIdeal + 1} to {currentIdealInt + 1}");
    }

    public void CatastrophicBondDeath() {
        int idealBeforeDeath = currentIdeal;
        bool isBondsmith = radiantOrderDef == RadiantOrderDefOf.Bondsmith;
        Logger.Important($"CatastrophicBondDeath: {pawn.NameShortColored}, ideal={idealBeforeDeath}, bondsmith={isBondsmith}");

        Comp.Game.RadiantTracker tracker = Current.Game.GetComponent<Comp.Game.RadiantTracker>();
        tracker?.RecordBrokenBond(pawn, radiantOrderDef.defName);

        if (isBondsmith) {
            tracker?.UnregisterBondsmith();
            Comp.Game.BondsmithCallingChecker? checker = Current.Game.GetComponent<Comp.Game.BondsmithCallingChecker>();
            if (!string.IsNullOrEmpty(godsprenName)) {
                checker?.UnregisterGodspren(godsprenName);
            }
        }

        Verse.Hediff? strainedBond = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Roshar_Hediff_StrainedBond);
        if (strainedBond != null) {
            pawn.health!.RemoveHediff(strainedBond);
        }

        Thought_Memory brokenBondThought = ThoughtMaker.MakeThought(ThoughtDefOf.Cosmere_Roshar_Thought_BrokenBond, 0);
        brokenBondThought.permanent = true;
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(brokenBondThought);

        bool droppedBlade = false;
        string? sprenName = bondedSpren?.Name?.ToStringShort;

        if (bondedSpren != null && !isBondsmith) {
            if (idealBeforeDeath >= 2 && pawn.Map != null) {
                ThingDef bladeDef = ThingDefOf.Cosmere_Roshar_MeleeWeapon_DeadShardblade;
                if (bladeDef != null) {
                    Verse.Thing deadBlade = ThingMaker.MakeThing(bladeDef, radiantOrderDef.gemstone?.Item);
                    if (sprenName != null) {
                        deadBlade.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Normal, ArtGenerationContext.Colony);
                    }
                    bool placed = GenPlace.TryPlaceThing(deadBlade, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    droppedBlade = placed;
                }
            }

            if (!bondedSpren.Dead && !bondedSpren.Destroyed) {
                IntVec3 deathPos = bondedSpren.PositionHeld;
                Verse.Map? deathMap = bondedSpren.MapHeld;
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
                if (!bondedSpren.Destroyed) {
                    bondedSpren.Destroy();
                }
            }

            bondedSpren = null;
        }

        string bodyKey = isBondsmith ? "CRO_BondDeath_Bondsmith_Text" : "CRO_BondDeath_Text";
        string bodyText = bodyKey.Translate(
            pawn.NameFullColored.Named("PAWN"),
            radiantOrderDef.LabelCap.Named("ORDER"),
            godsprenName.Named("SPREN")
        );

        if (droppedBlade) {
            bodyText += "\n\n" + "CRO_DeadBlade_Text".Translate(
                pawn.NameFullColored.Named("PAWN"),
                radiantOrderDef.LabelCap.Named("ORDER")
            );
        }

        Find.LetterStack.ReceiveLetter(
            "CRO_BondDeath_Title".Translate(pawn.NameShortColored.Named("PAWN")),
            bodyText,
            RimWorld.LetterDefOf.Death,
            pawn
        );

        godsprenName = "";
        pawn.genes?.RemoveGene(this);
    }

    public void OnWitnessedDeath(Pawn deceased) { }

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
        pawn.story.TryAddTrait(radiantOrder.trait, currentIdealInt);
        UpdateAbilities();
        EnableSoulcastWorkType();

        int idealIndex = currentIdealInt - 1;
        if (idealIndex >= 0 && idealIndex < radiantOrderDef.ideals.Count) {
            int newMax = radiantOrderDef.ideals[idealIndex].stormlightMax;
            if (newMax > 0) {
                investitureHolder.maxInvestitureSelf = newMax;
                investiture.CurLevel = newMax;
            }
        }

        investitureHolder.drainRate = GetDrainRate();

        if (godsprenName == "Sibling") {
            ApplyUrithuruBlessing();
        }

        TryTransitionEyeColor();
    }

    private void TryTransitionEyeColor() {
        if (currentIdealInt < 1) return;
        if (!Utility.CasteUtility.IsDarkeyes(pawn)) return;

        Utility.CasteUtility.DarkeyesToLighteyes(pawn);

        Find.LetterStack.ReceiveLetter(
            "Eyes of Light",
            $"As Stormlight courses through {pawn.NameShortColored}'s veins, their eyes lighten. {pawn.gender.GetPronoun().CapitalizeFirst()} is darkeyes no longer.",
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
        existing.Severity = currentIdealInt / 5f;
    }

    private float GetDrainRate() {
        if (currentIdeal >= 4) return 0f;

        float totalDrainTimeInSeconds = (10 * 60).TicksToSeconds();

        float baseRate = 1f / totalDrainTimeInSeconds;

        float idealMultiplier = 1f - currentIdeal / 4f;

        return baseRate * idealMultiplier;
    }

    public void NotifyZoneViolation() {
        zoneViolatedToday = true;
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (sources.Count > 0 && pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) {
            DrainStormlightForAbilities();
        }

        TryLevelUp(delta);
        TrySkillUp(delta);
        TrackZoneCompliance(delta);
        TrackGraveProximity(delta);
        CheckOathViolations(delta);
        ManageBondHediff(delta);
        CheckSurgebindingSavantProgression(delta);
    }

    private void ManageBondHediff(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;

        ILoadReferenceable? bondTarget = GetBondTarget();
        if (bondTarget == null) return;

        float connection = SpiritWeb.Instance?.GetConnectionValue(pawn, bondTarget) ?? 0f;

        Verse.Hediff? existing = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Roshar_Hediff_StrainedBond);

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
                Cosmere.Core.Investiture.DrainSource source = sources[i];
                RimWorld.Ability? ability = pawn.abilities?.GetAbility(source.Def);
                if (ability is Surgebinding.Ability.SurgebindingAbility surgeAbility) {
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
    ///     leveled up, and on their way to higher ideals
    /// </summary>
    /// <param name="delta"></param>
    private void TrySkillUp(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (currentIdeal >= 2) return;

        skill.Learn(5, true, true);
    }

    private void TryLevelUp(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (currentIdeal >= 4) return;
        if (pendingOath) return;

        int nextIdeal = currentIdeal + 1;
        if (skill.Level < SkillRequirements[nextIdeal]) return;
        if (!IsCooldownElapsed(nextIdeal)) return;
        if (!radiantOrderDef.idealChecker.IsSatisfied(pawn, this, nextIdeal)) return;

        pendingOath = true;
        SendOathNotification(nextIdeal);
    }

    public void SpeakOath() {
        if (currentIdeal >= 4) return;

        int nextIdeal = currentIdeal + 1;
        if (!radiantOrderDef.idealChecker.IsSatisfied(pawn, this, nextIdeal)) return;

        pendingOath = false;
        currentIdeal = nextIdeal;

        radiantOrderDef.idealChecker.Satisfy(pawn, this, currentIdeal);
    }

    private bool IsCooldownElapsed(int nextIdeal) {
        if (lastIdealChangeTick < 0) return true;
        int cooldownTicks = IdealCooldownDays[nextIdeal] * GenDate.TicksPerDay;
        return Find.TickManager.TicksGame - lastIdealChangeTick >= cooldownTicks;
    }

    private void SendOathNotification(int nextIdeal) {
        Ideal ideal = radiantOrderDef.ideals[nextIdeal];
        string oathText = ideal.quotes.Count > 0 ? ideal.quotes[0] : "";
        bool isTruth = radiantOrderDef == RadiantOrderDefOf.Lightweaver;
        string idealLabel = isTruth ? "Truth" : "Ideal";

        string title = "CRO_SpeakOath_Title".Translate(
            pawn.NameShortColored.Named("PAWN"),
            radiantOrderDef.LabelCap.Named("ORDER"),
            ideal.label.Named("IDEAL")
        );

        string text = "CRO_SpeakOath_Text".Translate(
            pawn.NameFullColored.Named("PAWN"),
            radiantOrderDef.LabelCap.Named("ORDER"),
            ideal.label.Named("IDEAL"),
            oathText.Named("OATH"),
            idealLabel.Named("IDEALLABEL")
        );

        SpeakOath letter = (SpeakOath)LetterMaker.MakeLetter(
            title, text, LetterDefOf.Cosmere_Roshar_Letter_SpeakOath, pawn
        );
        letter.Setup(pawn, nextIdeal);
        Find.LetterStack.ReceiveLetter(letter);
    }

    private void TrackZoneCompliance(int delta) {
        if (radiantOrderDef.defName != "Skybreaker") return;

        if (!zoneViolatedToday && pawn.Map != null && pawn.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap != null) {
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
        if (radiantOrderDef.defName != "Edgedancer") return;
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

    private void CheckOathViolations(int delta) {
        if (!pawn.IsHashIntervalTick(GenDate.TicksPerDay, delta)) return;

        string orderName = radiantOrderDef.defName;
        int currentTick = Find.TickManager.TicksGame;

        if (orderName == "Stoneward") {
            CheckStonewardCaravanDuringSiege();
        }

        if (pawn.Map == null) return;

        switch (orderName) {
            case "Windrunner":
                CheckWindrunnerIdleWhileDownedAllies();
                break;
            case "Edgedancer":
                CheckEdgedancerPrisonerNeglect();
                break;
            case "Truthwatcher":
                CheckTruthwatcherMedicalDisabled();
                break;
            case "Lightweaver":
                CheckLightweaverStagnation(currentTick);
                break;
            case "Elsecaller":
                CheckElsecallerStagnation(currentTick);
                break;
            case "Stoneward":
                CheckStonewardUndraftedDuringRaid();
                break;
        }
    }

    private void CheckWindrunnerIdleWhileDownedAllies() {
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

        bool isIdle = pawn.CurJobDef == RimWorld.JobDefOf.Wait_Wander
                      || pawn.CurJobDef == RimWorld.JobDefOf.GotoWander
                      || pawn.CurJobDef == RimWorld.JobDefOf.Wait;
        if (!isIdle) return;

        ViolationUtility.ApplyViolation(pawn, 0.1f, "idling while allies are downed");
    }

    private void CheckEdgedancerPrisonerNeglect() {
        List<Pawn> prisoners = pawn.Map.mapPawns.PrisonersOfColonySpawned;
        for (int i = 0; i < prisoners.Count; i++) {
            Pawn prisoner = prisoners[i];
            if (prisoner.Dead) continue;

            bool starving = prisoner.needs?.food != null && prisoner.needs.food.Starving;
            bool hasBleeding = prisoner.health?.hediffSet?.HasTendableHediff(false) ?? false;

            if (starving || hasBleeding) {
                ViolationUtility.ApplyViolation(pawn, 0.1f, "neglecting prisoner welfare");
                return;
            }
        }
    }

    private void CheckTruthwatcherMedicalDisabled() {
        if (pawn.workSettings == null) return;

        bool doctorDisabled = pawn.workSettings.GetPriority(RimWorld.WorkTypeDefOf.Doctor) == 0;
        if (doctorDisabled) {
            ViolationUtility.ApplyViolation(pawn, 0.1f, "disabling medical work");
        }
    }

    private void CheckLightweaverStagnation(int currentTick) {
        int stagnationThresholdTicks = 30 * GenDate.TicksPerDay;

        if (lastCreativeOutputTick < 0) {
            lastCreativeOutputTick = currentTick;
            return;
        }

        if (currentTick - lastCreativeOutputTick >= stagnationThresholdTicks) {
            ViolationUtility.ApplyViolation(pawn, 0.3f, "creative stagnation");
            lastCreativeOutputTick = currentTick;
        }
    }

    private void CheckElsecallerStagnation(int currentTick) {
        int stagnationThresholdTicks = 30 * GenDate.TicksPerDay;

        if (lastSkillGainTick < 0) {
            lastSkillGainTick = currentTick;
            return;
        }

        if (currentTick - lastSkillGainTick >= stagnationThresholdTicks) {
            ViolationUtility.ApplyViolation(pawn, 0.1f, "intellectual stagnation");
            lastSkillGainTick = currentTick;
        }
    }

    private void CheckStonewardUndraftedDuringRaid() {
        if (pawn.Drafted) return;

        if (!IsRaidActiveOnMap(pawn.Map)) return;

        ViolationUtility.ApplyViolation(pawn, 0.1f, "refusing to fight during a raid");
    }

    private void CheckStonewardCaravanDuringSiege() {
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

        List<Verse.AI.Group.Lord> lords = map.lordManager.lords;
        for (int i = 0; i < lords.Count; i++) {
            if (lords[i].faction == null) continue;
            if (!lords[i].faction.HostileTo(Faction.OfPlayer)) continue;
            if (lords[i].LordJob is not RimWorld.LordJob_AssaultColony and not RimWorld.LordJob_AssaultThings) continue;

            return true;
        }

        return false;
    }

    public override void PostAdd() {
        skill.Level = 0;
        base.PostAdd();
        OnIdealChange();
        Logger.Verbose($"{pawn.NameFullColored} has become a {radiantOrderDef.LabelCap}");
    }

    private void EnableSoulcastWorkType() {
        if (pawn.workSettings == null || !pawn.workSettings.EverWork) return;
        if (pawn.abilities == null) return;

        List<RimWorld.Ability> abilities = pawn.abilities.AllAbilitiesForReading;
        bool hasSoulcast = false;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is Surgebinding.Ability.Transformation.Soulcast) {
                hasSoulcast = true;
                break;
            }
        }

        if (!hasSoulcast) return;

        pawn.Notify_DisabledWorkTypesChanged();

        WorkTypeDef? soulcastWork = DefDatabase<WorkTypeDef>.GetNamedSilentFail("Cosmere_Roshar_WorkType_Soulcasting");
        if (soulcastWork == null) return;
        if (pawn.WorkTypeIsDisabled(soulcastWork)) return;
        if (pawn.workSettings.GetPriority(soulcastWork) > 0) return;
        pawn.workSettings.SetPriority(soulcastWork, 3);
    }

    public override void PostRemove() {
        skill.Level = 0;
        foreach (AbilityDef unlockedAbilityDef in radiantOrderDef.GetAbilities(currentIdealInt)) {
            pawn.abilities.RemoveAbility(unlockedAbilityDef);
        }

        pawn.story.TryRemoveTrait(radiantOrder.trait);
    }

    internal bool PendingOath => pendingOath;
    internal int LastIdealChangeTick => lastIdealChangeTick;

    internal void DebugTriggerOath() {
        if (currentIdeal >= 4) return;
        pendingOath = true;
        SendOathNotification(currentIdeal + 1);
    }

    internal void DebugSpeakOathNow() {
        if (currentIdeal >= 4) return;

        int nextIdeal = currentIdeal + 1;
        if (radiantOrderDef.idealChecker.HasIncompatibleTrait(pawn, nextIdeal)) {
            string traitName = radiantOrderDef.idealChecker.GetIncompatibleTraitName(pawn) ?? "unknown";
            Messages.Message(
                $"{pawn.NameShortColored} has an incompatible trait ({traitName}) for the next Ideal",
                pawn,
                MessageTypeDefOf.RejectInput
            );
            return;
        }

        pendingOath = false;
        currentIdeal = nextIdeal;
        radiantOrderDef.idealChecker.Satisfy(pawn, this, currentIdeal);
    }

    internal void DebugResetCooldown() {
        lastIdealChangeTick = -1;
    }

    internal void UpdateAbilities() {
        foreach (AbilityDef unlockedAbilityDef in radiantOrderDef.GetAbilities(currentIdealInt)) {
            pawn.abilities.GainAbility(unlockedAbilityDef);
        }

        if (radiantOrderDef == RadiantOrderDefOf.Bondsmith && !string.IsNullOrEmpty(godsprenName) && currentIdealInt >= 2) {
            AddBondsmithSprenAbility();
        }
    }

    private void AddBondsmithSprenAbility() {
        RimWorld.AbilityDef? abilityDef = godsprenName switch {
            "Stormfather" => DefDatabase<RimWorld.AbilityDef>.GetNamedSilentFail("Cosmere_Roshar_Ability_HonorsPerpendicularity"),
            "Nightwatcher" => DefDatabase<RimWorld.AbilityDef>.GetNamedSilentFail("Cosmere_Roshar_Ability_CultivationsPerpendicularity"),
            "Sibling" => DefDatabase<RimWorld.AbilityDef>.GetNamedSilentFail("Cosmere_Roshar_Ability_SiblingBlessing"),
            _ => null,
        };
        if (abilityDef != null) pawn.abilities.GainAbility(abilityDef);

        if (godsprenName == "Sibling") {
            RimWorld.AbilityDef? reinforceDef = DefDatabase<RimWorld.AbilityDef>.GetNamedSilentFail("Cosmere_Roshar_Ability_ReinforceStructure");
            if (reinforceDef != null) pawn.abilities.GainAbility(reinforceDef);
        }
    }

    private void CheckSurgebindingSavantProgression(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;

        List<Def.SurgeDef> surges = radiantOrderDef.surges;
        for (int i = 0; i < surges.Count; i++) {
            Def.SurgeDef surge = surges[i];
            CheckSurgeSavant(surge);
        }
    }

    private void CheckSurgeSavant(Def.SurgeDef surge) {
        string surgeName = surge.defName;
        cachedSavantStages.TryGetValue(surgeName, out int previousStage);
        savantDecayOffsets.TryGetValue(surgeName, out float decayOffset);

        RecordDef? recordDef = DefDatabase<RecordDef>.GetNamedSilentFail("Cosmere_Roshar_Record_TimeSpentUsing_" + surgeName);
        if (recordDef == null) return;

        float ticks = pawn.records.GetValue(recordDef) - decayOffset;
        int newStage = SavantUtility.GetSurgebindingStage(ticks);

        if (newStage == 1) {
            bool surgeActive = false;
            for (int i = 0; i < sources.Count; i++) {
                List<RimWorld.AbilityDef> surgeAbilities = surge.abilities;
                for (int j = 0; j < surgeAbilities.Count; j++) {
                    if (sources[i].Def == surgeAbilities[j]) {
                        surgeActive = true;
                        break;
                    }
                }
                if (surgeActive) break;
            }

            if (!surgeActive) {
                float decayAmount = ticks * SavantUtility.Stage1DecayPerDayFraction / GenDate.TicksPerDay * GenTicks.TickLongInterval;
                savantDecayOffsets[surgeName] = decayOffset + decayAmount;
                ticks -= decayAmount;
                newStage = SavantUtility.GetSurgebindingStage(ticks);
            }
        }

        cachedSavantStages[surgeName] = newStage;

        HediffDef? savantHediffDef = SavantUtility.GetSurgeSavantHediffDef(surgeName);
        if (newStage > 0 && savantHediffDef != null) {
            Verse.Hediff? existing = pawn.health.hediffSet.GetFirstHediffOfDef(savantHediffDef);
            if (existing == null) {
                Verse.Hediff hediff = HediffMaker.MakeHediff(savantHediffDef, pawn);
                hediff.Severity = SavantUtility.SeverityForStage(newStage);
                pawn.health.AddHediff(hediff);
            } else {
                existing.Severity = SavantUtility.SeverityForStage(newStage);
            }
        } else if (newStage == 0 && savantHediffDef != null) {
            Verse.Hediff? existing = pawn.health.hediffSet.GetFirstHediffOfDef(savantHediffDef);
            if (existing != null) pawn.health.RemoveHediff(existing);
        }

        HediffDef? permanentHediffDef = SavantUtility.GetSurgePermanentHediffDef(surgeName);
        if (newStage >= 3 && previousStage < 3 && permanentHediffDef != null) {
            if (!pawn.health.hediffSet.HasHediff(permanentHediffDef)) {
                pawn.health.AddHediff(HediffMaker.MakeHediff(permanentHediffDef, pawn));
            }
        }

        if (newStage > previousStage && newStage > 0) {
            SavantUtility.SendSavantLetter(pawn, newStage, "Surgebinding", surge.LabelCap);
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref currentIdealInt, "currentIdeal");
        Scribe_Values.Look(ref lastIdealChangeTick, "lastIdealChangeTick", -1);
        Scribe_Values.Look(ref pendingOath, "pendingOath");
        Scribe_References.Look(ref bondedSpren, "bondedSpren");
        Scribe_Values.Look(ref godsprenName, "godsprenName", "");
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
        cachedSavantStages ??= new();
        Scribe_Collections.Look(ref savantDecayOffsets, "savantDecayOffsets", LookMode.Value, LookMode.Value);
        savantDecayOffsets ??= new();
    }

    private List<Roshar.Gizmo.SurgeGizmo>? cachedSurgeGizmos;

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        foreach (Verse.Gizmo gizmo in base.GetGizmos()) {
            yield return gizmo;
        }

        if (!pawn.Spawned) yield break;

        if (cachedSurgeGizmos == null) {
            cachedSurgeGizmos = [];
            foreach (Def.SurgeDef surge in radiantOrderDef.surges) {
                cachedSurgeGizmos.Add(new Roshar.Gizmo.SurgeGizmo(this, surge));
            }
        }

        for (int i = 0; i < cachedSurgeGizmos.Count; i++) {
            yield return cachedSurgeGizmos[i];
        }
    }
}
