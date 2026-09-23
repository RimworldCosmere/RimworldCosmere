using System.Collections.Generic;
using System.Reflection;
using Concord;
using Cosmere.System.Scadrial.Kandra;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Kandra;

/// <summary>
///     Gives a shaped kandra an animal-shaped outline when the map is zoomed out.
/// </summary>
/// <remarks>
///     <c>PawnRenderer.RenderPawnAt</c> hard-selects
///     <c>pawn.RaceProps.Humanlike ? CurLifeStage.silhouetteGraphicData : ...</c>, so a kandra that
///     keeps its own race gets a person's outline whatever it is drawn as.
///     <para>
///         Two seams. <c>SilhouetteGraphic</c> is what <c>DrawSilhouette</c> measures for scale.
///         The material comes from <c>GetCachedSilhouetteData</c>, whose cache is keyed on
///         (ThingDef, LifeStageDef, graphicIndex, gender, rotStage) - every one of which a shaped
///         kandra shares with an ordinary colonist. Evicting that entry would paint a wolf over the
///         whole colony, so the shaped case is served from its own cache and vanilla's is left
///         alone.
///     </para>
/// </remarks>
public static class KandraSilhouette {
    /// <summary>
    ///     keyed on gender too: DataFor picks the female graphic when present, so keying on kind alone let the
    ///     first kandra wearing a gendered animal set the outline for every other; vanillas cache key does this too.
    /// </summary>
    private static readonly Dictionary<(PawnKindDef, Gender), Graphic> outlines = [];

    /// <summary>
    ///     Reaches the renderer's pawn, which is private and readonly.
    /// </summary>
    /// <remarks>
    ///     The getter being patched takes no arguments and the field has no accessor, so there is
    ///     no other way in. Resolved once at type load rather than per draw.
    /// </remarks>
    private static readonly FieldInfo? pawnField =
        typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

    public static Pawn? PawnOf(PawnRenderer renderer) {
        return pawnField?.GetValue(renderer) as Pawn;
    }

    /// <summary>The animal's graphic, or null when this pawn is not wearing one.</summary>
    public static Graphic? For(Pawn? pawn) {
        PawnKindDef? kind = KandraShapeGraphicUtility.WornKind(pawn);
        if (kind == null) return null;

        (PawnKindDef, Gender) key = (kind, pawn!.gender);
        if (outlines.TryGetValue(key, out Graphic? cached)) return cached;

        GraphicData? data = KandraShapeGraphicUtility.DataFor(pawn);
        if (data == null || string.IsNullOrEmpty(data.texPath)) return null;

        outlines[key] = data.Graphic;

        return outlines[key];
    }

    public static Material MaterialFor(Graphic animal, bool west) {
        Graphic outline = animal.GetColoredVersion(ShaderDatabase.Silhouette, Color.white, Color.white);

        return west ? outline.MatWest : outline.MatEast;
    }
}

/// <summary>Measures the outline against the animal rather than the person.</summary>
[Patch]
public abstract class KandraSilhouetteGraphicPatch : PawnRenderer {
    protected KandraSilhouetteGraphicPatch(Pawn pawn)
        : base(pawn) { }

    [Inject(At.Return, nameof(SilhouetteGraphic))]
    private void AfterSilhouetteGraphic(ControlHandle<Graphic> ch) {
        Graphic? animal = KandraSilhouette.For(KandraSilhouette.PawnOf(this));
        if (animal == null) return;

        ch.ReturnValue = animal;
    }
}

/// <summary>Serves the shaped case from its own cache so vanilla's stays clean.</summary>
[Patch(typeof(SilhouetteUtility))]
public static class KandraSilhouetteDataPatch {
    [Inject(At.Head, "GetCachedSilhouetteData")]
    private static Control BeforeGetCachedSilhouetteData(
        Verse.Thing thing,
        ControlHandle<(Mesh mesh, Material material)> ch
    ) {
        if (thing is not Pawn pawn) return Control.Continue;

        Graphic? animal = KandraSilhouette.For(pawn);
        if (animal == null) return Control.Continue;

        bool west = pawn.Rotation == Rot4.West;
        ch.ReturnValue = (
            west ? MeshPool.GridPlaneFlip(Vector2.one) : MeshPool.GridPlane(Vector2.one),
            KandraSilhouette.MaterialFor(animal, west)
        );

        return Control.Cancel;
    }
}
