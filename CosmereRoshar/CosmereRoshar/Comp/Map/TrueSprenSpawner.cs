using Cosmere.Core.Thing;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Comp.Map;

public class TrueSprenSpawner(Verse.Map map) : MapComponent(map) {
    private List<SprenForPawn> spawnedSpren = [];
    private List<SpawnInfo> spawnInfo = [];

    public List<Pawn> pawns => Current.Game.CurrentMap.mapPawns.FreeColonistsAndPrisonersSpawned
        // .Where(p => p.genes.GetFirstGeneOfType<Surgebinder>() == null)
        .ToList();

    private static float baseSpawnChance =>
        1 / Mod.Settings.nahelSprenSpawnAverageIntervalTicks * Current.Game.storyteller.difficulty.threatScale;

    public override void MapComponentTick() {
        if (baseSpawnChance == 0) return;

        base.MapComponentTick();
        TrySpawnSpren();
        TryDespawnSpren();
    }

    private SpawnInfo GetSpawnInfo(Pawn pawn) {
        foreach (SpawnInfo s in spawnInfo) {
            if (s.pawn.Equals(pawn)) return s;
        }

        SpawnInfo info = new SpawnInfo { pawn = pawn, lastSpawn = GenTicks.TicksGame };
        spawnInfo.Add(info);

        return info;
    }

    private void TrySpawnSpren() {
        foreach (Pawn pawn in pawns) {
            SpawnInfo spawnInfoForPawn = GetSpawnInfo(pawn);
            if (spawnInfoForPawn.ticksSinceLastSpawn < Mod.Settings.nahelSprenSpawnMinIntervalTicks) continue;
            if (spawnedSpren.Any(sp => sp.pawn.Equals(pawn))) continue;
            if (pawn.IsAsleep()) continue;

            if (!Rand.Chance(baseSpawnChance) &&
                spawnInfoForPawn.ticksSinceLastSpawn < Mod.Settings.nahelSprenSpawnMaxIntervalTicks) {
                continue;
            }

            SpawnSpren(pawn, spawnInfoForPawn);
        }
    }

    private void TryDespawnSpren() {
        foreach (SprenForPawn data in
                 spawnedSpren.ToList().Where(data => !data.spren.Spawned || data.spren.Destroyed)) {
            spawnedSpren.Remove(data);
        }
    }

    private void SpawnSpren(Pawn pawn, SpawnInfo spawnInfoForPawn) {
        PawnKindDef? kind = PawnKindDefOf.Cosmere_Roshar_Race_UnknownTrueSpren;

        Splinter? splinter = (Splinter)PawnGenerator.GeneratePawn(
            kind,
            pawn.Faction,
            Find.CurrentMap.Tile
        );
        GenSpawn.Spawn(
            splinter,
            CellFinder.RandomSpawnCellForPawnNear(pawn.Position, pawn.Map),
            pawn.Map
        );
        splinter.training.Train(TrainableDefOf.Obedience, pawn, true);
        splinter.playerSettings = new Pawn_PlayerSettings(splinter)
            { Master = pawn, followDrafted = true, followFieldwork = true };
        splinter.mindState.canFleeIndividual = false;
        splinter.mindState.exitMapAfterTick =
            GenTicks.TicksGame + GenTicks.SecondsToTicks(Rand.RangeInclusive(60 * 5, 60 * 15));
        Find.Selector.Select(splinter);

        spawnedSpren.Add(
            new SprenForPawn {
                pawn = pawn,
                spawnedAtTick = GenTicks.TicksGame,
                spren = splinter,
            }
        );

        spawnInfoForPawn.lastSpawn = GenTicks.TicksGame;
        Find.TickManager.Pause();
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Collections.Look(ref spawnedSpren, "spawnedSpren");
        Scribe_Collections.Look(ref spawnInfo, "spawnInfo");
    }

    private struct SprenForPawn : IExposable {
        public Pawn pawn;
        public Splinter spren;
        public int spawnedAtTick;

        public void ExposeData() {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref spren, "splinter");
            Scribe_Values.Look(ref spawnedAtTick, "spawnedAtTick");
        }
    }

    private struct SpawnInfo : IExposable {
        public Pawn pawn;
        public int lastSpawn;
        public int ticksSinceLastSpawn => GenTicks.TicksGame - lastSpawn;

        public void ExposeData() {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref lastSpawn, "lastSpawn");
        }
    }
}