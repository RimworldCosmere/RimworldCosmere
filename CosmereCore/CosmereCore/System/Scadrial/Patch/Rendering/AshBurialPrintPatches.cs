using Concord;
using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Grid;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Rendering;

/// <summary>
///     Ash deep enough to bury a cell keeps what lies on it out of the things mesh, the way deep
///     snow drops anything under hideAtSnowOrSandDepth. Nothing drawn above a print can erase it,
///     so the print has to not happen.
/// </summary>
[Patch]
public abstract class AshBurialPrintPatch : SectionLayer_ThingsGeneral {
    protected AshBurialPrintPatch(Section section) : base(section) { }

    /// <summary>
    ///     Runs for every printed thing in a section, so the category test comes first: a wall or
    ///     a pawn leaves before anything touches the map, and a clean map costs one lookup.
    /// </summary>
    [Inject(At.Head, nameof(TakePrintFrom))]
    private Control BeforeTakePrintFrom(Verse.Thing t) {
        ThingCategory category = t.def.category;
        if (category != ThingCategory.Item && category != ThingCategory.Plant) return Control.Continue;

        AshDepthTracker? tracker = Map.GetComponent<AshDepthTracker>();
        if (tracker == null) return Control.Continue;

        AshBuriedCells buried = tracker.Buried;
        if (!buried.Any) return Control.Continue;

        return buried.IsBuried(Map.cellIndices.CellToIndex(t.Position)) ? Control.Cancel : Control.Continue;
    }
}
