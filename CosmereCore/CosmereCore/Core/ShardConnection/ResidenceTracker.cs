using Cosmere.Core.Def;
using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.Core.ShardConnection;

/// <summary>
///     How long each pawn has lived on the save's world.
/// </summary>
/// <remarks>
///     A pawn who was not born to this world grows into it, reaching the ancestry floor at about
///     a year. It is what lets an off-world refugee eventually burn atium without ever having
///     been Scadrian.
///     <para>
///         Only a real shardworld naturalises anyone. The cross-world sentinel is nobody's home -
///         living on a planet no Shard ever settled teaches you nothing, so a Crashlanded colony
///         stays at nothing unless it finds lerasium or a spike.
///     </para>
/// </remarks>
public class ResidenceTracker : Verse.GameComponent {
    /// <summary>Hourly. Residence is measured in seasons, so a finer tick buys nothing.</summary>
    private const int TickInterval = 2500;

    private Dictionary<int, int> ticksByPawn = [];

    public ResidenceTracker(Verse.Game game) { }

    public override void ExposeData() {
        base.ExposeData();

        // Pawns who left the story stop mattering, and their entries would accumulate forever.
        if (Scribe.mode == LoadSaveMode.Saving) Prune();

        Scribe_Collections.Look(ref ticksByPawn, "residenceTicks", LookMode.Value, LookMode.Value);
        ticksByPawn ??= [];
    }

    /// <summary>What this pawn has earned by living here, on the 0-100 Connection scale.</summary>
    public int StrengthFor(Pawn? pawn, ShardDef shard) {
        if (pawn == null) return 0;

        CosmereWorldDef? world = NaturalisingWorld();
        if (world == null) return 0;

        // Living on Scadrial grows a tie to Scadrial's Shards. It teaches you nothing of Honor.
        List<ShardDef> local = WorldUtility.AncestryShards(world);
        bool belongs = false;
        for (int i = 0; i < local.Count; i++) {
            if (local[i] == shard) {
                belongs = true;
                break;
            }
        }

        if (!belongs) return 0;

        return ConnectionMath.ResidenceFrom(
            ticksByPawn.TryGetValue(pawn.thingIDNumber, out int ticks) ? ticks : 0
        );
    }

    public override void GameComponentTick() {
        if (Find.TickManager.TicksGame % TickInterval != 0) return;
        if (NaturalisingWorld() == null) return;

        List<Verse.Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            List<Pawn> pawns = maps[m].mapPawns.FreeColonistsAndPrisonersSpawned;
            for (int i = 0; i < pawns.Count; i++) {
                int id = pawns[i].thingIDNumber;
                ticksByPawn[id] = (ticksByPawn.TryGetValue(id, out int had) ? had : 0) + TickInterval;
            }
        }
    }

    /// <summary>The world a pawn can naturalise into, or null when this save has none.</summary>
    private static CosmereWorldDef? NaturalisingWorld() {
        CosmereWorldDef? world = WorldUtility.Primary;

        return world is { crossWorld: false } ? world : null;
    }

    private void Prune() {
        List<int> gone = [];
        foreach (KeyValuePair<int, int> pair in ticksByPawn) {
            if (Find.Maps.Any(map => map.mapPawns.AllPawns.Any(p => p.thingIDNumber == pair.Key))) continue;
            gone.Add(pair.Key);
        }

        for (int i = 0; i < gone.Count; i++) ticksByPawn.Remove(gone[i]);
    }
}
