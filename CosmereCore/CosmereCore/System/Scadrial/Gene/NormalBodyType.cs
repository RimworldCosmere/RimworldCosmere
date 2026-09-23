using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

/// <summary>
///     Keeps a pawn on the ordinary body for its gender.
/// </summary>
/// <remarks>
///     A gene's <c>bodyType</c> field forces one specific body on everybody who carries it, which
///     is right for koloss and wrong for a people who come in both sexes. Terris are lean and
///     tall, never heavyset, so the rule is "the normal one for your gender" rather than any
///     single def.
/// </remarks>
public class NormalBodyType : Verse.Gene {
    public override void PostAdd() {
        base.PostAdd();
        Normalise();
    }

    public override void Notify_NewColony() {
        base.Notify_NewColony();
        Normalise();
    }

    private void Normalise() {
        if (pawn.story == null) return;
        if (pawn.DevelopmentalStage is DevelopmentalStage.Baby or DevelopmentalStage.Newborn) return;

        BodyTypeDef wanted = pawn.gender == Gender.Female
            ? BodyTypeDefOf.Female
            : BodyTypeDefOf.Male;

        if (pawn.story.bodyType == wanted) return;

        pawn.story.bodyType = wanted;
        pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    }
}
