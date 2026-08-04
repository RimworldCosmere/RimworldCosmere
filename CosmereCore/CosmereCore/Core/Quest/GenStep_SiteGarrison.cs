using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest;

/// <summary>
///     Posts a faction's fighters on the map's RectOfInterest. GenStep_Outpost, the vanilla
///     way to garrison a site, is a base builder - it walls a settlement into one of four
///     rects beside the rect of interest, so the ground the quest is about ends up empty.
///     This puts the defenders on that ground instead.
/// </summary>
public class GenStep_SiteGarrison : GenStep {
    /// <summary>Used when the site part carries no threat points of its own.</summary>
    public float defaultPoints = 300f;

    public float defendRadius = 16f;

    /// <summary>How far from the rect's centre a defender may be placed.</summary>
    public int spawnRadius = 12;

    public override int SeedPart => 1276643318;

    public override void Generate(Verse.Map map, GenStepParams parms) {
        Faction? faction = map.ParentFaction;
        if (faction == null || faction.IsPlayer) faction = Find.FactionManager.RandomEnemyFaction();

        if (faction == null) {
            Logger.Warning("GenStep_SiteGarrison: no enemy faction in this world. Site left unguarded.");
            return;
        }

        // GenStep_PreciousLump sets this to the seam's bounds in ScatterAt, so this GenStep's
        // def must be ordered after it.
        if (!MapGenerator.TryGetVar("RectOfInterest", out CellRect rect)) {
            Logger.Warning("GenStep_SiteGarrison: no RectOfInterest. Falling back to map centre.");
            rect = CellRect.CenteredOn(map.Center, 4);
        }

        IntVec3 center = AnchorNear(rect, map);
        float points = parms.sitePart != null ? parms.sitePart.parms.threatPoints : defaultPoints;
        if (points <= 0f) points = defaultPoints;

        PawnGroupMakerParms groupParms = new PawnGroupMakerParms {
            groupKind = PawnGroupKindDefOf.Settlement,
            tile = map.Tile,
            faction = faction,
            points = points,
            inhabitants = true,
        };

        List<Pawn> defenders = new List<Pawn>();
        foreach (Pawn pawn in PawnGroupMakerUtility.GeneratePawns(groupParms, true)) {
            IntVec3 cell = CellFinder.RandomSpawnCellForPawnNear(center, map, spawnRadius);
            GenSpawn.Spawn(pawn, cell, map);
            defenders.Add(pawn);
        }

        if (defenders.Count == 0) {
            Logger.Warning($"GenStep_SiteGarrison: {faction.Name} generated no pawns for {points} points.");
            return;
        }

        LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(center, null, defendRadius), map, defenders);
        Logger.Verbose($"GenStep_SiteGarrison: {defenders.Count} {faction.Name} pawns holding {center}.");
    }

    /// <summary>
    ///     A standable cell beside the rect. RectOfInterest is the ore body itself, so its
    ///     centre is solid rock - spawning on it walls the defenders into the seam.
    /// </summary>
    private IntVec3 AnchorNear(CellRect rect, Verse.Map map) {
        IntVec3 center = rect.CenterCell;
        if (center.Standable(map)) return center;

        bool Standable(IntVec3 cell) {
            return cell.Standable(map) && map.reachability.CanReachMapEdge(cell, TraverseParms.For(TraverseMode.PassDoors));
        }

        if (CellFinder.TryFindRandomCellNear(center, map, spawnRadius * 2, Standable, out IntVec3 found, -1)) {
            return found;
        }

        Logger.Warning("GenStep_SiteGarrison: nothing standable beside the seam. Using the map centre.");
        return CellFinder.RandomNotEdgeCell(20, map);
    }
}
