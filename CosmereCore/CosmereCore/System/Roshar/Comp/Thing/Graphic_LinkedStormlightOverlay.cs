using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class Graphic_LinkedStormlightOverlay : Graphic_Linked {
    public Graphic_LinkedStormlightOverlay() { }

    public Graphic_LinkedStormlightOverlay(Graphic subGraphic) : base(subGraphic) { }

    public override bool ShouldLinkWith(IntVec3 c, Verse.Thing parent) {
        if (!c.InBounds(parent.Map)) {
            return false;
        }

        List<Verse.Thing> things = parent.Map.thingGrid.ThingsListAtFast(c);
        for (int i = 0; i < things.Count; i++) {
            if (things[i].TryGetComp<StormlightConduit>() != null) {
                return true;
            }
        }

        return false;
    }

    public override void Print(SectionLayer layer, Verse.Thing parent, float extraRotation) {
        foreach (IntVec3 item in parent.OccupiedRect()) {
            Vector3 center = item.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay);
            Printer_Plane.PrintPlane(
                layer,
                center,
                new Vector2(1f, 1f),
                LinkedDrawMatFrom(parent, item),
                extraRotation
            );
        }
    }
}
