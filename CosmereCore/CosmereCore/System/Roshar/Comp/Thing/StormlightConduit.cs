using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class StormlightConduitProperties : CompProperties {
    public StormlightConduitProperties() {
        compClass = typeof(StormlightConduit);
    }
}

public class StormlightConduit : StormlightNode {
    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
        if (respawningAfterLoad) return;

        List<Verse.Thing> things = parent.Position.GetThingList(parent.Map);
        for (int i = things.Count - 1; i >= 0; i--) {
            Verse.Thing thing = things[i];
            if (thing == parent) continue;
            if (thing.TryGetComp<StormlightConduit>() == null) continue;
            thing.Destroy(DestroyMode.Refund);
        }
    }

    public void PrintForStormlightGrid(SectionLayer layer) {
        if (parent.Graphic is Graphic_LinkedStormlightOverlay overlay) {
            overlay.Print(layer, parent, 0f);
        }
    }
}