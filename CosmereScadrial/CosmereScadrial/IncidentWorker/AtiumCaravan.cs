using System.Collections.Generic;
using System.Linq;
using CosmereResources;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace CosmereScadrial.IncidentWorker;

public class AtiumCaravan : RimWorld.IncidentWorker {
    protected override bool CanFireNowSub(IncidentParms parms) {
        return base.CanFireNowSub(parms) && Find.CurrentMap != null;
    }

    protected override bool TryExecuteWorker(IncidentParms parms) {
        Map map = (Map)parms.target;

        // Find spawn and exit points
        if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 entryPoint, map, CellFinder.EdgeRoadChance_Neutral)) {
            return false;
        }

        IntVec3 exitPoint = CellFinder.RandomEdgeCell(map);

        // @TODO Create new faction
        Faction faction = Find.FactionManager.OfAncientsHostile;
        PawnGroupMakerParms groupParms = new PawnGroupMakerParms {
            groupKind = PawnGroupKindDefOf.Combat,
            tile = map.Tile,
            faction = faction,
            points = 300f,
            raidStrategy = RaidStrategyDefOf.ImmediateAttack,
        };

        List<Pawn> attackers = PawnGroupMakerUtility.GeneratePawns(groupParms).ToList();

        if (attackers.Count == 0) {
            return false;
        }

        // Give a few pawns Atium
        foreach (Pawn? p in attackers.Take(2)) // First two
        {
            Verse.Thing atium = ThingMaker.MakeThing(CosmereResources.ThingDefOf.Atium);
            atium.stackCount = Rand.RangeInclusive(1, 3);
            p.inventory?.TryAddItemNotForSale(atium);
        }

        // Spawn and start raid
        foreach (Pawn? pawn in attackers) {
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(entryPoint, map, 10), map);
        }

        LordJob lordJob = new LordJob_AssaultColony(faction, false, canSteal: false);
        LordMaker.MakeNewLord(faction, lordJob, map, attackers);

        Find.LetterStack.ReceiveLetter(
            "Atium Caravan",
            "CS_MetalCaravanLetter".Translate(MetalDefOf.Atium.coloredLabel.Named("METAL")).Resolve(),
            LetterDefOf.PositiveEvent,
            attackers[0]
        );

        return true;
    }
}