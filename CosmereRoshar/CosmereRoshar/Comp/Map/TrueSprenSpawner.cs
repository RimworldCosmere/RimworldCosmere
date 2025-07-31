using Cosmere.Core.Thing;
using Verse;

namespace Cosmere.Roshar.Comp.Map;

public class TrueSprenSpawner(Verse.Map map) : MapComponent(map) {
    private readonly int lastSpawn = GenTicks.TicksAbs;
    private List<SprenForPawn> spawnedSpren = [];

    public List<Pawn> pawns => Current.Game.CurrentMap.mapPawns.FreeColonistsAndPrisonersSpawned
        // .Where(p => p.genes.GetFirstGeneOfType<Surgebinder>() == null)
        .ToList();

    private static float baseSpawnChance =>
        Mod.Settings.baseNahelSprenSpawnChance * Current.Game.storyteller.difficulty.threatScale;

    public override void MapComponentTick() {
        if (baseSpawnChance == 0) return;

        base.MapComponentTick();
        foreach (Pawn pawn in pawns) {
            if (GenTicks.TicksAbs - lastSpawn < Mod.Settings.nahelSprenSpawnMinIntervalTicks) continue;
            if (spawnedSpren.Any(s => s.pawn.Equals(pawn))) continue;
            float chance = baseSpawnChance / spawnedSpren.Count;

            if (!Rand.Chance(chance) && GenTicks.TicksAbs - lastSpawn < Mod.Settings.nahelSprenSpawnMaxIntervalTicks) {
                continue;
            }

            SpawnSpren(pawn);
        }
    }

    private void SpawnSpren(Pawn pawn) {
        spawnedSpren.Add(
            new SprenForPawn {
                pawn = pawn,
                spawnedAtTick = GenTicks.TicksGame,
                splinter = (Splinter)ThingMaker.MakeThing(
                    Core.ThingDefOf.Cosmere_Core_Race_Splinter
                ),
            }
        );
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref spawnedSpren, "spawnedSpren");
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