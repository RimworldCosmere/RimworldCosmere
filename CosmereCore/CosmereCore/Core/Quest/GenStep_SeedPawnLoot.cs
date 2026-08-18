using System.Collections.Generic;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest;

/// <summary>
///     Puts a quest's payout into the pockets of the pawns guarding it, so the player takes it
///     off the bodies instead of receiving it by letter. Ordered after whatever GenStep spawns
///     those pawns.
/// </summary>
public class GenStep_SeedPawnLoot : GenStep {
    public int countPerCarrier = 1;
    public int maxCarriers = 4;
    public int minCarriers = 2;
    public ThingDef? thingDef;

    public override int SeedPart => 823461907;

    public override void Generate(Verse.Map map, GenStepParams parms) {
        ThingDef? loot = thingDef;
        if (loot == null) {
            Logger.Error("GenStep_SeedPawnLoot has no thingDef.");
            return;
        }

        Faction player = Faction.OfPlayer;
        List<Pawn> candidates = new List<Pawn>();
        IReadOnlyList<Pawn> spawned = map.mapPawns.AllPawnsSpawned;
        for (int i = 0; i < spawned.Count; i++) {
            Pawn pawn = spawned[i];
            if (!pawn.RaceProps.Humanlike) continue;
            if (pawn.Dead || pawn.inventory == null) continue;
            if (pawn.Faction == null || !pawn.Faction.HostileTo(player)) continue;

            candidates.Add(pawn);
        }

        if (candidates.Count == 0) {
            Logger.Warning($"GenStep_SeedPawnLoot: no hostile pawns to carry {loot.defName}.");
            return;
        }

        int wanted = Rand.RangeInclusive(minCarriers, maxCarriers);
        int carriers = wanted < candidates.Count ? wanted : candidates.Count;

        // Fisher-Yates over the prefix only: picks distinct carriers without a full shuffle or a contains-check loop.
        for (int i = 0; i < carriers; i++) {
            int swap = Rand.Range(i, candidates.Count);
            (candidates[i], candidates[swap]) = (candidates[swap], candidates[i]);

            Verse.Thing stack = ThingMaker.MakeThing(loot);
            stack.stackCount = countPerCarrier;
            candidates[i].inventory.innerContainer.TryAdd(stack);
        }

        Logger.Verbose(
            $"GenStep_SeedPawnLoot: {carriers} pawn(s) carrying {countPerCarrier} {loot.defName} each."
        );
    }
}
