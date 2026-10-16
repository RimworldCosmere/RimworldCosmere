using Cosmere.Core.Def;
using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.Core.ShardConnection;

/// <summary>
///     How long each pawn has lived on the save's world.
/// </summary>
/// <remarks>
///     A pawn who was not born to this world grows into it, passing the ancestry floor at about
///     six and a half years and reaching residence's own cap at ten. It is what lets an
///     off-world refugee eventually burn atium without ever having been Scadrian.
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
    /// <returns>The Connection strength residence has earned, 0 to 100. 0 when the pawn is null, when the save has no naturalising world, or when that world's Shards do not include this one.</returns>
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

        return ConnectionMath.ResidenceFrom(TicksFor(pawn));
    }

    /// <summary>Moves a pawn's residence and reports how much actually moved.</summary>
    /// <remarks>Some gameplay moves stored residence rather than only accumulating it, so this runs in both directions.</remarks>
    /// <returns>How many ticks the residence really moved once clamped, which can be less than the delta asked for. 0 when the pawn is null or the delta is 0.</returns>
    public int AdjustTicks(Pawn? pawn, int delta) {
        if (pawn == null || delta == 0) return 0;

        int id = pawn.thingIDNumber;
        int had = ticksByPawn.TryGetValue(id, out int ticks) ? ticks : 0;
        int next = ConnectionMath.ClampResidenceTicks(had, delta);
        ticksByPawn[id] = next;

        return next - had;
    }

    /// <summary>Raw ticks resident, with no shard-ancestry gate. The read half of <see cref="AdjustTicks" />.</summary>
    /// <returns>Ticks the pawn has been resident, seeded from its age the first time it is asked for. 0 when the pawn is null or the save has no naturalising world.</returns>
    public int TicksFor(Pawn? pawn) {
        if (pawn == null) return 0;

        if (ticksByPawn.TryGetValue(pawn.thingIDNumber, out int ticks)) return ticks;

        CosmereWorldDef? world = NaturalisingWorld();
        if (world == null) return 0;

        bool native = world == WorldUtility.WorldForXenotype(pawn.genes?.Xenotype);
        int seeded = ConnectionMath.SeedTicksForAge(pawn.ageTracker.AgeBiologicalTicks, native);
        ticksByPawn[pawn.thingIDNumber] = seeded;

        return seeded;
    }

    public override void GameComponentTick() {
        if (Find.TickManager.TicksGame % TickInterval != 0) return;

        CosmereWorldDef? world = NaturalisingWorld();
        if (world == null) return;

        List<Verse.Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            List<Pawn> pawns = maps[m].mapPawns.FreeColonistsAndPrisonersSpawned;
            for (int i = 0; i < pawns.Count; i++) {
                Pawn pawn = pawns[i];
                int id = pawn.thingIDNumber;
                if (ticksByPawn.TryGetValue(id, out int had)) {
                    ticksByPawn[id] = ConnectionMath.ClampResidenceTicks(had, TickInterval);
                } else {
                    bool native = world == WorldUtility.WorldForXenotype(pawn.genes?.Xenotype);
                    ticksByPawn[id] = ConnectionMath.SeedTicksForAge(pawn.ageTracker.AgeBiologicalTicks, native);
                }
            }
        }
    }

    /// <summary>The world a pawn can naturalise into, or null when this save has none.</summary>
    /// <returns>The save's primary world, or null when there is no primary world or it is the cross-world sentinel.</returns>
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
