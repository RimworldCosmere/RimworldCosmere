using Cosmere.Core.Util;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.System.Roshar.Comp.Game;

public class NightwatcherValleyPlacer : GameComponent {
    public NightwatcherValleyPlacer(Verse.Game game) { }

    public override void StartedNewGame() {
        PlaceValley();
    }

    public override void LoadedGame() {
        PlaceValley();
    }

    private void PlaceValley() {
        if (!FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Nightwatcher)) return;

        WorldObjectDef? def = DefDatabase<WorldObjectDef>.GetNamedSilentFail("Cosmere_Roshar_NightwatcherValley");
        if (def == null) return;

        List<RimWorld.Planet.WorldObject> existing = Find.WorldObjects.AllWorldObjects;
        for (int i = 0; i < existing.Count; i++) {
            if (existing[i].def == def) return;
        }

        if (!TileFinder.TryFindNewSiteTile(out PlanetTile tile, 5, 50)) return;

        RimWorld.Planet.WorldObject valley = WorldObjectMaker.MakeWorldObject(def);
        valley.Tile = tile;
        Find.WorldObjects.Add(valley);
    }
}
