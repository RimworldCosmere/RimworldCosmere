using Cosmere.System.Roshar.Gene;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Comp.Map;

public class GemheartExpeditionManager(Verse.Map map) : MapComponent(map) {
    private const float BaseHuntChance = 0.4f;
    private const int MinCooldownTicks = 3 * GenDate.TicksPerDay;
    private const int MinExpeditionPawns = 3;
    private const int MaxExpeditionPawns = 6;
    private bool expeditionActive;

    private List<Pawn> expeditionPawns = [];
    private int expeditionReturnTick = -1;
    private int lastHuntTick = -1;

    public bool ExpeditionActive => expeditionActive;

    public bool CanStartHunt => !expeditionActive &&
                                (lastHuntTick < 0 || Find.TickManager.TicksGame - lastHuntTick >= MinCooldownTicks);

    public override void MapComponentTick() {
        if (!expeditionActive) return;
        if (Find.TickManager.TicksGame < expeditionReturnTick) return;

        ResolveExpedition();
    }

    public void QueueHuntAfterHighstorm() {
        if (!CanStartHunt) return;
        if (!Rand.Chance(BaseHuntChance)) return;

        int delayTicks = Rand.Range(1, 4) * GenDate.TicksPerDay;
        int fireTick = Find.TickManager.TicksGame + delayTicks;

        IncidentDef? huntDef = DefDatabase<IncidentDef>.GetNamedSilentFail("Cosmere_Roshar_Incident_GemheartHunt");
        if (huntDef == null) return;

        Find.Storyteller.incidentQueue.Add(huntDef, fireTick, new IncidentParms { target = map, forced = true });
    }

    public bool StartExpedition(List<Pawn> pawns) {
        if (expeditionActive) return false;
        if (pawns.Count < MinExpeditionPawns || pawns.Count > MaxExpeditionPawns) return false;

        expeditionPawns = [.. pawns];
        expeditionActive = true;
        lastHuntTick = Find.TickManager.TicksGame;
        expeditionReturnTick = Find.TickManager.TicksGame + Rand.Range(1, 3) * GenDate.TicksPerDay;

        for (int i = 0; i < expeditionPawns.Count; i++) {
            expeditionPawns[i].mindState.priorityWork.Clear();
            expeditionPawns[i].jobs?.StopAll();
        }

        Logger.Verbose(
            $"[GemheartHunt] Expedition started with {pawns.Count} pawns, returns tick {expeditionReturnTick}"
        );
        return true;
    }

    private void ResolveExpedition() {
        expeditionActive = false;

        List<Pawn> survivors = [];
        for (int i = 0; i < expeditionPawns.Count; i++) {
            if (expeditionPawns[i] != null && !expeditionPawns[i].Dead && !expeditionPawns[i].Destroyed) {
                survivors.Add(expeditionPawns[i]);
            }
        }

        if (survivors.Count == 0) {
            expeditionPawns = [];
            return;
        }

        float power = CalculateExpeditionPower(survivors);
        float difficulty = CalculateDifficulty();
        float ratio = difficulty > 0 ? power / difficulty : 2f;

        Logger.Verbose($"[GemheartHunt] Resolving: power={power:F1}, difficulty={difficulty:F1}, ratio={ratio:F2}");

        if (ratio >= 1.5f) {
            ResolveVictory(survivors);
        }
        else if (ratio >= 1.0f) {
            ResolveHardWon(survivors);
        }
        else if (ratio >= 0.6f) {
            ResolvePyrrhic(survivors);
        }
        else if (ratio >= 0.3f) {
            ResolveFailure(survivors);
        }
        else {
            ResolveDisaster(survivors);
        }

        expeditionPawns = [];
    }

    private static float CalculateExpeditionPower(List<Pawn> pawns) {
        float power = 0f;
        for (int i = 0; i < pawns.Count; i++) {
            Pawn pawn = pawns[i];
            float melee = pawn.skills?.GetSkill(RimWorld.SkillDefOf.Melee)?.Level ?? 0;
            float shooting = pawn.skills?.GetSkill(RimWorld.SkillDefOf.Shooting)?.Level ?? 0;
            power += melee * 3f + shooting * 2f;

            if (pawn.genes != null) {
                Surgebinder? surgebinder = pawn.genes.GetFirstGeneOfType<Surgebinder>();
                if (surgebinder is { Active: true }) {
                    power += surgebinder.CurrentIdealDisplay * 25f;
                }
            }

            if (pawn.equipment?.Primary != null) {
                power += pawn.equipment.Primary.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS) * 2f;
            }
        }

        power += pawns.Count * 5f;
        return power;
    }

    private float CalculateDifficulty() {
        int daysPassed = GenDate.DaysPassed;
        float wealth = map.wealthWatcher.WealthTotal;
        return 100f + daysPassed * 0.5f + wealth / 10000f;
    }

    private void ResolveVictory(List<Pawn> pawns) {
        SpawnGemheart();
        ApplyMinorInjuries(pawns, 1);
        ApplyMoodThought(pawns, "Cosmere_Roshar_Thought_GemheartVictory");

        Find.LetterStack.ReceiveLetter(
            "Gemheart Claimed",
            "The expedition returns triumphant. The chasmfiend was dead in its chrysalis, the gemheart extracted cleanly. Your soldiers carry it back with pride - a prize worth the march across the plateaus.",
            RimWorld.LetterDefOf.PositiveEvent
        );
    }

    private void ResolveHardWon(List<Pawn> pawns) {
        SpawnGemheart();
        ApplyModerateInjuries(pawns, Rand.Range(1, 3));
        ApplyMoodThought(pawns, "Cosmere_Roshar_Thought_GemheartVictory");

        Find.LetterStack.ReceiveLetter(
            "Gemheart Secured",
            "The chrysalis was not as dead as the scouts believed. The chasmfiend stirred as your soldiers approached, and the fight that followed left its mark. But the gemheart is yours.",
            RimWorld.LetterDefOf.PositiveEvent
        );
    }

    private void ResolvePyrrhic(List<Pawn> pawns) {
        SpawnGemheart();
        ApplySeriousInjuries(pawns, Rand.Range(1, 3));
        ApplyMoodThought(pawns, "Cosmere_Roshar_Thought_GemheartVictory");

        Find.LetterStack.ReceiveLetter(
            "Costly Victory",
            "The chasmfiend fought. Your soldiers paid the price. The gemheart was torn free at great cost - carried back by those still able to walk. The wounded will need time to recover.",
            RimWorld.LetterDefOf.NeutralEvent
        );
    }

    private void ResolveFailure(List<Pawn> pawns) {
        ApplyModerateInjuries(pawns, Rand.Range(1, 3));
        ApplyMoodThought(pawns, "Cosmere_Roshar_Thought_GemheartFailure");

        Find.LetterStack.ReceiveLetter(
            "Expedition Failed",
            "The chasmfiend was alive. Fully alive. Your soldiers fought to reach the chrysalis but were driven back across the plateaus. They return empty-handed, bloodied, and fortunate to return at all.",
            RimWorld.LetterDefOf.NegativeEvent
        );
    }

    private void ResolveDisaster(List<Pawn> pawns) {
        ApplySeriousInjuries(pawns, pawns.Count);
        ApplyMoodThought(pawns, "Cosmere_Roshar_Thought_GemheartFailure");

        Find.LetterStack.ReceiveLetter(
            "Disaster on the Plains",
            "The expedition was a catastrophe. The chasmfiend was enormous, far larger than any your scouts have seen. Your soldiers barely escaped the plateau alive. No gemheart. Only wounds and the memory of those terrible legs crashing down.",
            RimWorld.LetterDefOf.NegativeEvent
        );
    }

    private void SpawnGemheart() {
        ThingDef? gemheartDef = ThingDefOf.Cosmere_Roshar_Thing_Gemheart;
        if (gemheartDef == null) {
            Logger.Warning("[GemheartHunt] Gemheart ThingDef not found");
            return;
        }

        Verse.Thing gemheart = ThingMaker.MakeThing(gemheartDef);
        gemheart.stackCount = 1;
        IntVec3 dropSpot = DropCellFinder.TradeDropSpot(map);
        GenPlace.TryPlaceThing(gemheart, dropSpot, map, ThingPlaceMode.Near);
    }

    private static void ApplyMinorInjuries(List<Pawn> pawns, int count) {
        for (int i = 0; i < Mathf.Min(count, pawns.Count); i++) {
            Pawn pawn = pawns[Rand.Range(0, pawns.Count)];
            DamageInfo damage = new DamageInfo(DamageDefOf.Blunt, Rand.Range(3f, 8f));
            pawn.TakeDamage(damage);
        }
    }

    private static void ApplyModerateInjuries(List<Pawn> pawns, int count) {
        for (int i = 0; i < Mathf.Min(count, pawns.Count); i++) {
            Pawn pawn = pawns[Rand.Range(0, pawns.Count)];
            DamageInfo damage = new DamageInfo(DamageDefOf.Cut, Rand.Range(8f, 18f));
            pawn.TakeDamage(damage);
        }
    }

    private static void ApplySeriousInjuries(List<Pawn> pawns, int count) {
        for (int i = 0; i < Mathf.Min(count, pawns.Count); i++) {
            Pawn pawn = pawns[Rand.Range(0, pawns.Count)];
            DamageInfo damage = new DamageInfo(DamageDefOf.Cut, Rand.Range(18f, 35f));
            pawn.TakeDamage(damage);
        }
    }

    private static void ApplyMoodThought(List<Pawn> pawns, string thoughtDefName) {
        ThoughtDef? thought = DefDatabase<ThoughtDef>.GetNamedSilentFail(thoughtDefName);
        if (thought == null) return;

        for (int i = 0; i < pawns.Count; i++) {
            pawns[i].needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
        }
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref expeditionReturnTick, "expeditionReturnTick", -1);
        Scribe_Values.Look(ref lastHuntTick, "lastHuntTick", -1);
        Scribe_Values.Look(ref expeditionActive, "expeditionActive");
        Scribe_Collections.Look(ref expeditionPawns, "expeditionPawns", LookMode.Reference);
        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            expeditionPawns ??= [];
            expeditionPawns.RemoveAll(p => p == null);
        }
    }
}