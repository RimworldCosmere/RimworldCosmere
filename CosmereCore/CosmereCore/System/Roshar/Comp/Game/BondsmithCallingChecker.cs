using Cosmere.Core.Util;
using Cosmere.System.Roshar;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.LetterArrive;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Comp.Game;

public class BondsmithCallingChecker : GameComponent {
    private static readonly string[] GodsprenNames = ["Stormfather", "Nightwatcher", "Sibling"];
    private HashSet<string> activeCallings = [];
    private HashSet<string> bondedGodspren = [];

    private int lastCheckTick = -1;
    private Dictionary<int, int> refuseCooldowns = [];

    public BondsmithCallingChecker(Verse.Game game) { }

    public void RecordRefusal(Pawn pawn) {
        refuseCooldowns[pawn.thingIDNumber] = Find.TickManager.TicksGame + 30 * GenDate.TicksPerDay;
    }

    public void RecordBondedGodspren(string sprenName) {
        bondedGodspren.Add(sprenName);
    }

    public void UnregisterGodspren(string sprenName) {
        bondedGodspren.Remove(sprenName);
    }

    public override void GameComponentTick() {
        base.GameComponentTick();

        int ticksGame = Find.TickManager.TicksGame;
        if (lastCheckTick >= 0 && ticksGame - lastCheckTick < GenDate.TicksPerDay) return;
        lastCheckTick = ticksGame;

        RadiantTracker? tracker = Current.Game.GetComponent<RadiantTracker>();
        if (tracker == null || !tracker.CanProgressBondsmith()) return;

        RecalculateActiveCallings();
        RecalculateBondedGodspren();

        List<Verse.Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            Verse.Map map = maps[m];
            if (!map.IsPlayerHome) continue;

            List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++) {
                Pawn pawn = colonists[i];
                if (HasAnyCallingHediff(pawn)) continue;
                if (IsOnRefusalCooldown(pawn)) continue;
                if (IsBondsmith(pawn)) continue;

                for (int s = 0; s < GodsprenNames.Length; s++) {
                    string sprenName = GodsprenNames[s];
                    if (bondedGodspren.Contains(sprenName)) continue;
                    if (activeCallings.Contains(sprenName)) continue;

                    if (!MeetsCallingCriteria(pawn, sprenName, map)) continue;

                    ApplyCalling(pawn, sprenName);
                    return;
                }
            }
        }
    }

    private bool MeetsCallingCriteria(Pawn pawn, string sprenName, Verse.Map map) {
        if (!IsGodsprenAvailable(sprenName)) return false;
        return sprenName switch {
            "Stormfather" => MeetsStormfatherCriteria(pawn, map),
            "Nightwatcher" => MeetsNightwatcherCriteria(pawn),
            "Sibling" => MeetsSiblingCriteria(pawn),
            _ => false,
        };
    }

    private static bool IsGodsprenAvailable(string sprenName) {
        return sprenName switch {
            "Stormfather" => ShardUtility.AreAnyEnabled(ShardDefOf.Honor),
            "Nightwatcher" => ShardUtility.AreAnyEnabled(ShardDefOf.Cultivation),
            "Sibling" => ShardUtility.AreAllEnabled(ShardDefOf.Honor, ShardDefOf.Cultivation),
            _ => false,
        };
    }

    private bool MeetsStormfatherCriteria(Pawn pawn, Verse.Map map) {
        bool isFactionLeader = pawn.Faction?.leader == pawn;
        if (isFactionLeader) {
            float avgOpinion = GetAverageColonyOpinion(pawn, map);
            int friendships = (int)pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_FriendshipsFormed);
            if (avgOpinion >= 40f && friendships >= 5) return true;
        }

        int socialSkill = pawn.skills?.GetSkill(RimWorld.SkillDefOf.Social)?.Level ?? 0;
        int friendshipsFormed = (int)pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_FriendshipsFormed);
        if (socialSkill < 10 || friendshipsFormed < 8) return false;

        int highOpinionCount = CountPawnsWithHighOpinion(pawn, map, 50);
        return highOpinionCount >= 3;
    }

    private bool MeetsNightwatcherCriteria(Pawn pawn) {
        int medSkill = pawn.skills?.GetSkill(RimWorld.SkillDefOf.Medicine)?.Level ?? 0;
        int rescued = (int)pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_PawnsRescued);

        if (medSkill >= 8 && rescued >= 6) return true;

        bool hasCaring = false;
        List<Trait> traits = pawn.story?.traits?.allTraits ?? [];
        for (int i = 0; i < traits.Count; i++) {
            if (traits[i].def == RimWorld.TraitDefOf.Kind || traits[i].def.defName == "Caring") {
                hasCaring = true;
                break;
            }
        }

        bool hasMedPassion = pawn.skills?.GetSkill(RimWorld.SkillDefOf.Medicine)?.passion >= Passion.Minor;
        return hasCaring && hasMedPassion && rescued >= 3;
    }

    private bool MeetsSiblingCriteria(Pawn pawn) {
        int intSkill = pawn.skills?.GetSkill(RimWorld.SkillDefOf.Intellectual)?.Level ?? 0;
        int research = (int)pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_ResearchCompleted);

        if (intSkill >= 10 && research >= 3) return true;

        bool hasIntPassion = pawn.skills?.GetSkill(RimWorld.SkillDefOf.Intellectual)?.passion >= Passion.Minor;
        return hasIntPassion && intSkill >= 8;
    }

    private void ApplyCalling(Pawn pawn, string sprenName) {
        HediffDef? hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(
            $"Cosmere_Roshar_Hediff_BondsmithCalling_{sprenName}"
        );
        if (hediffDef == null) return;

        BondsmithCalling hediff = (BondsmithCalling)HediffMaker.MakeHediff(hediffDef, pawn);
        hediff.sprenName = sprenName;
        pawn.health.AddHediff(hediff);

        activeCallings.Add(sprenName);

        LetterDef? letterDef = DefDatabase<LetterDef>.GetNamedSilentFail("Cosmere_Roshar_Letter_BondsmithCalling");
        if (letterDef == null) return;

        BondsmithCallingLetter letter = (BondsmithCallingLetter)LetterMaker.MakeLetter(
            "CRO_Bondsmith_Calling_Title".Translate(pawn.NameShortColored.Named("PAWN"), sprenName.Named("SPREN")),
            "CRO_Bondsmith_Calling_Text".Translate(
                pawn.NameFullColored.Named("PAWN"),
                sprenName.Named("SPREN")
            ),
            letterDef,
            pawn
        );
        letter.Setup(sprenName);
        Find.LetterStack.ReceiveLetter(letter);
    }

    private void RecalculateActiveCallings() {
        activeCallings.Clear();
        List<Verse.Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            List<Pawn> colonists = maps[m].mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++) {
                BondsmithCalling? calling = GetCallingHediff(colonists[i]);
                if (calling != null) {
                    activeCallings.Add(calling.sprenName);
                }
            }
        }
    }

    private void RecalculateBondedGodspren() {
        bondedGodspren.Clear();
        List<Verse.Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            List<Pawn> colonists = maps[m].mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++) {
                List<Verse.Gene> genes = colonists[i].genes?.GenesListForReading ?? [];
                for (int g = 0; g < genes.Count; g++) {
                    if (genes[g] is Surgebinder surgebinder &&
                        surgebinder.radiantOrderDef == RadiantOrderDefOf.Bondsmith &&
                        !string.IsNullOrEmpty(surgebinder.godsprenName)) {
                        bondedGodspren.Add(surgebinder.godsprenName);
                    }
                }
            }
        }
    }

    private static bool HasAnyCallingHediff(Pawn pawn) {
        return GetCallingHediff(pawn) != null;
    }

    private static BondsmithCalling? GetCallingHediff(Pawn pawn) {
        List<Verse.Hediff> hediffs = pawn.health?.hediffSet?.hediffs ?? [];
        for (int i = 0; i < hediffs.Count; i++) {
            if (hediffs[i] is BondsmithCalling calling) return calling;
        }

        return null;
    }

    private static bool IsBondsmith(Pawn pawn) {
        List<Verse.Gene> genes = pawn.genes?.GenesListForReading ?? [];
        for (int g = 0; g < genes.Count; g++) {
            if (genes[g] is Surgebinder surgebinder && surgebinder.radiantOrderDef == RadiantOrderDefOf.Bondsmith) {
                return true;
            }
        }

        return false;
    }

    private bool IsOnRefusalCooldown(Pawn pawn) {
        if (!refuseCooldowns.TryGetValue(pawn.thingIDNumber, out int cooldownEnd)) return false;
        if (Find.TickManager.TicksGame >= cooldownEnd) {
            refuseCooldowns.Remove(pawn.thingIDNumber);
            return false;
        }

        return true;
    }

    private static float GetAverageColonyOpinion(Pawn pawn, Verse.Map map) {
        List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
        if (colonists.Count <= 1) return 0f;

        float total = 0f;
        int count = 0;
        for (int i = 0; i < colonists.Count; i++) {
            if (colonists[i] == pawn) continue;
            total += colonists[i].relations?.OpinionOf(pawn) ?? 0;
            count++;
        }

        return count > 0 ? total / count : 0f;
    }

    private static int CountPawnsWithHighOpinion(Pawn pawn, Verse.Map map, int threshold) {
        List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
        int count = 0;
        for (int i = 0; i < colonists.Count; i++) {
            if (colonists[i] == pawn) continue;
            if ((colonists[i].relations?.OpinionOf(pawn) ?? 0) >= threshold) count++;
        }

        return count;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref lastCheckTick, "lastCheckTick", -1);
        Scribe_Collections.Look(ref refuseCooldowns, "refuseCooldowns", LookMode.Value, LookMode.Value);
        refuseCooldowns ??= [];
        activeCallings ??= [];
        bondedGodspren ??= [];
    }

    public override void FinalizeInit() {
        base.FinalizeInit();
        RecalculateActiveCallings();
        RecalculateBondedGodspren();
        if (lastCheckTick < 0) {
            lastCheckTick = Find.TickManager.TicksGame;
        }
    }
}
