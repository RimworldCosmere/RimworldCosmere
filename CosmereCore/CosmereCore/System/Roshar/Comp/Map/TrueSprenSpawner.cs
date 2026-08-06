using Cosmere.Core.Def;
using Cosmere.Core.Util;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Thing.Pawn.Animal;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Comp.Map;

public class TrueSprenSpawner(Verse.Map map) : MapComponent(map) {
    private List<SprenForPawn> pawnSprens = [];
    private List<SpawnInfo> spawnInfo = [];

    public List<Pawn> pawns {
        get {
            Verse.Map? currentMap = map;
            if (currentMap?.mapPawns == null) return [];
            List<Pawn> result = [];
            List<Pawn> colonists = currentMap.mapPawns.FreeColonistsAndPrisonersSpawned;
            for (int i = 0; i < colonists.Count; i++) {
                Pawn p = colonists[i];
                if (p.genes?.GetFirstGeneOfType<Surgebinder>() == null && !p.HasComp<ChooseRadiantOrder>()) {
                    result.Add(p);
                }
            }

            return result;
        }
    }

    private static float baseSpawnChance =>
        1 / Mod.Settings.nahelSprenSpawnAverageIntervalTicks * Current.Game.storyteller.difficulty.threatScale;

    public override void MapComponentTick() {
        // Spren are Honor's and Cultivation's. Nothing on a Scadrial map should be
        // growing them, and the particle systems are expensive to keep warm besides.
        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Honor, ShardDefOf.Cultivation)) return;
        if (baseSpawnChance == 0) return;

        base.MapComponentTick();
        SpawnSpren();
    }

    private SpawnInfo GetSpawnInfo(Pawn pawn) {
        foreach (SpawnInfo s in spawnInfo) {
            if (s.pawn.Equals(pawn)) return s;
        }

        SpawnInfo info = new SpawnInfo { pawn = pawn, lastSpawn = GenTicks.TicksGame };
        spawnInfo.Add(info);

        return info;
    }

    private void SpawnSpren() {
        foreach (Pawn pawn in pawns) {
            SpawnInfo spawnInfoForPawn = GetSpawnInfo(pawn);
            if (spawnInfoForPawn.ticksSinceLastSpawn < Mod.Settings.nahelSprenSpawnMinIntervalTicks) continue;
            bool hasSpawnedSpren = false;
            for (int i = 0; i < pawnSprens.Count; i++) {
                if (pawnSprens[i].pawn.Equals(pawn) && pawnSprens[i].spren.Spawned) {
                    hasSpawnedSpren = true;
                    break;
                }
            }

            if (hasSpawnedSpren) continue;
            if (pawn.IsAsleep()) continue;

            if (!Rand.Chance(baseSpawnChance) &&
                spawnInfoForPawn.ticksSinceLastSpawn < Mod.Settings.nahelSprenSpawnMaxIntervalTicks) {
                continue;
            }

            SpawnSpren(pawn, spawnInfoForPawn);
        }
    }

    public void DespawnSpren(Pawn pawn, TrueSpren spren) {
        pawnSprens.RemoveWhere(d => d.pawn.Equals(pawn) && d.spren.Equals(spren));
    }

    public void DestroySprenForPawn(Pawn pawn) {
        for (int i = pawnSprens.Count - 1; i >= 0; i--) {
            if (!pawnSprens[i].pawn.Equals(pawn)) continue;

            TrueSpren spren = pawnSprens[i].spren;
            pawnSprens.RemoveAt(i);
            if (spren is { Spawned: true, Destroyed: false }) {
                spren.Destroy();
            }
        }
    }

    private void SpawnSpren(Pawn pawn, SpawnInfo spawnInfoForPawn) {
        PawnKindDef? kind = PawnKindDefOf.Cosmere_Roshar_Race_UnknownTrueSpren;

        TrueSpren spren;
        SprenForPawn? sprenForPawn = null;
        for (int i = 0; i < pawnSprens.Count; i++) {
            if (pawnSprens[i].pawn.Equals(pawn)) {
                sprenForPawn = pawnSprens[i];
                break;
            }
        }

        if (sprenForPawn == null) {
            spren = (TrueSpren)PawnGenerator.GeneratePawn(
                kind,
                pawn.Faction,
                Find.CurrentMap.Tile
            );
            sprenForPawn = new SprenForPawn {
                pawn = pawn,
                spren = spren,
            };
            pawnSprens.Add(sprenForPawn);
        } else {
            spren = sprenForPawn.spren;
        }

        if (!spren.Spawned) {
            spren.ForceSetStateToUnspawned();
            spren.HitPoints = spren.MaxHitPoints;
            GenSpawn.Spawn(
                spren,
                CellFinder.RandomSpawnCellForPawnNear(pawn.Position, pawn.Map),
                pawn.Map
            );
            spren.SetFactionDirect(pawn.Faction);
            spren.training.Train(TrainableDefOf.Obedience, pawn, true);
            spren.playerSettings = new Pawn_PlayerSettings(spren) { Master = pawn, followDrafted = true, followFieldwork = true };
            spren.mindState.canFleeIndividual = false;
        }

        spren.mindState.exitMapAfterTick =
            GenTicks.TicksGame + GenTicks.SecondsToTicks(Rand.RangeInclusive(60 * 5, 60 * 15));
        Find.Selector.Select(spren);

        spawnInfoForPawn.lastSpawn = spren.spawnedTick;
        CameraJumper.TryJumpAndSelect(spren);
        Find.TickManager.Pause();
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Collections.Look(ref pawnSprens, "spawnedSpren", LookMode.Deep);
        Scribe_Collections.Look(ref spawnInfo, "spawnInfo", LookMode.Deep);
    }

    private class SprenForPawn : IExposable {
        public Pawn pawn = null!;
        public TrueSpren spren = null!;

        public void ExposeData() {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref spren, "spren");
        }
    }

    private class SpawnInfo : IExposable {
        public int lastSpawn;
        public Pawn pawn = null!;

        public int ticksSinceLastSpawn => GenTicks.TicksGame - lastSpawn;

        public void ExposeData() {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref lastSpawn, "lastSpawn");
        }
    }
}
