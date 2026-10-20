using Cosmere.System.Scadrial.Comp.Map;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Thing;

public class CompProperties_AshMask : CompProperties {
    /// <summary>
    ///     Added to the apparel's score on a map that has vents. Vanilla scoring only understands
    ///     armour and warmth, so without this the mask is worth about as much as a scrap of cloth.
    /// </summary>
    public float scoreOffsetNearVents = 0.5f;

    public CompProperties_AshMask() {
        compClass = typeof(CompAshMask);
    }
}

/// <summary>
///     Makes a pawn want the mask where it is worth wanting. Scored on the map the apparel is on,
///     not the pawn's - the comp is asked with no pawn context, and a mask in a stockpile is being
///     considered by someone standing on that same map anyway.
/// </summary>
public class CompAshMask : ThingComp {
    public CompProperties_AshMask Props => (CompProperties_AshMask)props;

    public override float CompGetSpecialApparelScoreOffset() {
        Verse.Map? map = parent.MapHeld;
        if (map == null) return 0f;

        AshDepthTracker? tracker = map.GetComponent<AshDepthTracker>();
        if (tracker == null || tracker.Vents.Count == 0) return 0f;

        return Props.scoreOffsetNearVents;
    }
}
