using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     What a kandra is allowed to become.
/// </summary>
/// <remarks>
///     This used to be <c>KandraShapeGenerator.Eligible</c>, deciding which animals got a
///     generated race. With no generated races left it is still the only thing standing between a
///     kandra and eating a mechanoid, and it is what tells an eaten animal apart from an eaten
///     person - so it outlived the generator.
/// </remarks>
public static class KandraShapeEligibility {
    /// <summary>
    ///     Above this the shape is cropped when the map is zoomed out.
    /// </summary>
    /// <remarks>
    ///     A shaped kandra stays humanlike, so past <c>ZoomRootSize &gt; 18</c>
    ///     <c>PawnRenderer.ParallelGetPreRenderResults</c> blits it from the pawn texture atlas,
    ///     whose frame covers exactly two world units. Real animals never take that path. Rather
    ///     than ship a thrumbo that loses its head at half zoom, the big ones are simply not on
    ///     the menu.
    /// </remarks>
    public const float MaxDrawSize = 2f;

    public static bool Wearable(PawnKindDef? kind) {
        if (kind?.race?.race == null) return false;
        if (!kind.race.race.Animal) return false;
        if (kind.race.race.IsMechanoid) return false;
        if (kind.lifeStages == null || kind.lifeStages.Count == 0) return false;

        GraphicData? picture = kind.lifeStages[^1].bodyGraphicData;
        if (picture == null) return false;

        return picture.drawSize.x <= MaxDrawSize && picture.drawSize.y <= MaxDrawSize;
    }
}
