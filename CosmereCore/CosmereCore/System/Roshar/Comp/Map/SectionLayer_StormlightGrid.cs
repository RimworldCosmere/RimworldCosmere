using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Comp.Map;

public class SectionLayer_StormlightGrid : SectionLayer {
    public SectionLayer_StormlightGrid(Section section) : base(section) {
        relevantChangeTypes = DefDatabase<MapMeshFlagDef>.GetNamed("Cosmere_Roshar_StormlightGrid");
    }

    public override void DrawLayer() {
        if (StormlightOverlayDrawHandler.ShouldDraw) {
            base.DrawLayer();
        }
    }

    public override void Regenerate() {
        ClearSubMeshes(MeshParts.All);

        foreach (IntVec3 cell in section.CellRect) {
            List<Verse.Thing> things = Map.thingGrid.ThingsListAt(cell);
            for (int i = 0; i < things.Count; i++) {
                Verse.Thing thing = things[i];
                if (thing.Position.x != cell.x || thing.Position.z != cell.z) continue;
                if (thing.Faction != null && thing.Faction != Faction.OfPlayer) continue;

                StormlightConduit? conduit = thing.TryGetComp<StormlightConduit>();
                conduit?.PrintForStormlightGrid(this);
            }
        }

        FinalizeMesh(MeshParts.All);
    }
}
