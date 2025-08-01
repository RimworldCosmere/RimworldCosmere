using Cosmere.Core.Thing;
using Verse;

namespace Cosmere.Roshar.Comp.Map;

public class TrueSprenSpawner(Verse.Map map) : MapComponent(map) {
    private int lastSpawn = GenTicks.TicksAbs;
    private SprenForPawn? spawnedSpren;

    public List<Pawn> pawns => Current.Game.CurrentMap.mapPawns.FreeColonistsAndPrisonersSpawned
        // .Where(p => p.genes.GetFirstGeneOfType<Surgebinder>() == null)
        .ToList();

    private int ticksSinceLastSpawn => GenTicks.TicksAbs - lastSpawn;

    private static float baseSpawnChance =>
        1 / Mod.Settings.nahelSprenSpawnAverageIntervalTicks * Current.Game.storyteller.difficulty.threatScale;

    public override void MapComponentTick() {
        if (baseSpawnChance == 0) return;

        base.MapComponentTick();
        foreach (Pawn pawn in pawns) {
            if (ticksSinceLastSpawn < Mod.Settings.nahelSprenSpawnMinIntervalTicks) continue;
            if (spawnedSpren != null) continue;
            if (pawn.IsAsleep()) continue;

            if (!Rand.Chance(baseSpawnChance) && ticksSinceLastSpawn < Mod.Settings.nahelSprenSpawnMaxIntervalTicks) {
                continue;
            }

            SpawnSpren(pawn);
        }
    }

    private void SpawnSpren(Pawn pawn) {
        Splinter? splinter = (Splinter)GenSpawn.Spawn(
            ThingDefOf.Cosmere_Roshar_Race_UnknownTrueSpren,
            CellFinder.RandomSpawnCellForPawnNear(pawn.Position, pawn.Map),
            pawn.Map
        );

        spawnedSpren = new SprenForPawn {
            pawn = pawn,
            spawnedAtTick = GenTicks.TicksGame,
            splinter = splinter,
        };
        lastSpawn = GenTicks.TicksAbs;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref spawnedSpren, "spawnedSpren");
        Scribe_Values.Look(ref lastSpawn, "lastSpawn");
    }

    private struct SprenForPawn : IExposable {
        public Pawn pawn;
        public Splinter splinter;
        public int spawnedAtTick;

        public void ExposeData() {
            Scribe_Values.Look(ref spawnedAtTick, "spawnedAtTick");
            Scribe_References.Look(ref splinter, "splinter");
            Scribe_References.Look(ref pawn, "pawn");
        }
    }
}