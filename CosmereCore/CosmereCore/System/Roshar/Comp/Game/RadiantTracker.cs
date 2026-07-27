using Cosmere.System.Roshar;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Ability.Transportation;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Comp.Game;

public class RadiantTracker : GameComponent {
    private int activeBondsmithCount;
    private Dictionary<int, BrokenBondRecord> brokenBonds = new Dictionary<int, BrokenBondRecord>();
    private int lastStabilityCheckTick = -1;

    public RadiantTracker(Verse.Game game) { }

    public int ActiveBondsmithCount => activeBondsmithCount;

    public bool CanProgressBondsmith() {
        return activeBondsmithCount < 3;
    }

    public void RegisterBondsmith() {
        activeBondsmithCount++;
    }

    public void UnregisterBondsmith() {
        activeBondsmithCount--;
        if (activeBondsmithCount < 0) activeBondsmithCount = 0;
    }

    public void RecalculateBondsmithCount() {
        activeBondsmithCount = 0;
        List<Verse.Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            List<Pawn> colonists = maps[m].mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++) {
                List<Verse.Gene> genes = colonists[i].genes?.GenesListForReading ?? [];
                for (int g = 0; g < genes.Count; g++) {
                    if (genes[g] is not Surgebinder surgebinder) continue;
                    if (surgebinder.radiantOrderDef != RadiantOrderDefOf.Bondsmith) continue;
                    if (surgebinder.CurrentIdeal >= 1) activeBondsmithCount++;
                }
            }
        }
    }

    public override void GameComponentTick() {
        base.GameComponentTick();

        int ticksGame = Find.TickManager.TicksGame;
        if (lastStabilityCheckTick >= 0 && ticksGame - lastStabilityCheckTick < GenDate.TicksPerDay) return;
        lastStabilityCheckTick = ticksGame;

        TrackColonyStability();
    }

    private void TrackColonyStability() {
        List<Verse.Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            Verse.Map map = maps[m];
            if (!map.IsPlayerHome) continue;

            List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
            if (colonists.Count == 0) continue;

            float totalMood = 0f;
            int moodCount = 0;
            for (int i = 0; i < colonists.Count; i++) {
                Need_Mood? mood = colonists[i].needs?.mood;
                if (mood == null) continue;
                totalMood += mood.CurLevelPercentage;
                moodCount++;
            }

            if (moodCount == 0) continue;
            float averageMood = totalMood / moodCount;
            if (averageMood <= 0.6f) continue;

            for (int i = 0; i < colonists.Count; i++) {
                Surgebinder? surgebinder = colonists[i].genes?.GetFirstGeneOfType<Surgebinder>();
                if (surgebinder == null) continue;
                if (surgebinder.radiantOrderDef != RadiantOrderDefOf.Bondsmith) continue;

                colonists[i].records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ColonyStabilityDays, 1);
            }
        }
    }

    public void RecordBrokenBond(Pawn pawn, string orderDefName) {
        brokenBonds[pawn.thingIDNumber] = new BrokenBondRecord {
            orderDefName = orderDefName,
            breakTick = Find.TickManager.TicksGame,
        };
    }

    public bool CanRebond(Pawn pawn, string orderDefName) {
        if (!brokenBonds.TryGetValue(pawn.thingIDNumber, out BrokenBondRecord record)) return true;

        if (record.orderDefName == orderDefName) return false;

        int ticksSinceBreak = Find.TickManager.TicksGame - record.breakTick;
        return ticksSinceBreak >= 60 * GenDate.TicksPerDay;
    }

    public bool HasAnyAvailableOrder(Pawn pawn) {
        if (!brokenBonds.TryGetValue(pawn.thingIDNumber, out BrokenBondRecord record)) return true;

        int ticksSinceBreak = Find.TickManager.TicksGame - record.breakTick;
        return ticksSinceBreak >= 60 * GenDate.TicksPerDay;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref activeBondsmithCount, "activeBondsmithCount");
        Scribe_Values.Look(ref lastStabilityCheckTick, "lastStabilityCheckTick", -1);
        Scribe_Collections.Look(ref brokenBonds, "brokenBonds", LookMode.Value, LookMode.Deep);
        brokenBonds ??= new Dictionary<int, BrokenBondRecord>();
    }

    public override void FinalizeInit() {
        base.FinalizeInit();
        Portal.activePortals.Clear();
        RecalculateBondsmithCount();
    }
}
