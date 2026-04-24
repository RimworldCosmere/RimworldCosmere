using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Map;

public class Designator_DeconstructStormlightConduit : Designator_Deconstruct {
    public Designator_DeconstructStormlightConduit() {
        defaultLabel = "Deconstruct stormlight conduit";
        defaultDesc = "Deconstruct stormlight conduits only, leaving other buildings intact.";
        icon = ContentFinder<Texture2D>.Get("UI/Designators/DeconstructConduit");
        soundDragSustain = SoundDefOf.Designate_DragStandard;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        useMouseIcon = true;
        soundSucceeded = SoundDefOf.Designate_Deconstruct;
        hotKey = null;
    }

    public override AcceptanceReport CanDesignateThing(Verse.Thing t) {
        if (t.TryGetComp<StormlightConduit>() == null) {
            return false;
        }

        return base.CanDesignateThing(t);
    }

    public override void SelectedUpdate() {
        base.SelectedUpdate();
        StormlightOverlayDrawHandler.DrawThisFrame();
    }
}