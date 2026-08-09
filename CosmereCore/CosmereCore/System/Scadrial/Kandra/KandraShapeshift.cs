using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     Puts a body on, or takes it off.
/// </summary>
/// <remarks>
///     Everything a colonist looks at moves across: shape, face, hair, colouring, and the name.
///     Someone who knew the dead sees them walk back in. Only bronze reads what is underneath,
///     which is the one thing a kandra cannot change about itself.
/// </remarks>
public static class KandraShapeshift {
    /// <summary>Ten seconds at normal speed. Long enough to matter, short enough to use.</summary>
    public const int TicksToChange = 600;

    public static void Wear(Pawn kandra, KandraForm form) {
        CompKandraForms? forms = kandra.TryGetComp<CompKandraForms>();
        if (forms == null) return;

        forms.RememberTrueBody();
        ApplyTo(kandra, form);
        forms.SetCurrent(form);

        Messages.Message(
            "CS_KandraTookForm".Translate(form.Label.Named("FORM")),
            kandra,
            MessageTypeDefOf.SilentInput,
            false
        );
    }

    /// <summary>Back to whatever the kandra actually is.</summary>
    public static void Revert(Pawn kandra) {
        CompKandraForms? forms = kandra.TryGetComp<CompKandraForms>();
        KandraForm? own = forms?.TrueBody;
        if (forms == null || own == null) return;

        ApplyTo(kandra, own);
        forms.SetCurrent(null);
    }

    private static void ApplyTo(Pawn pawn, KandraForm form) {
        if (pawn.story != null) {
            if (form.bodyType != null) pawn.story.bodyType = form.bodyType;
            if (form.headType != null) pawn.story.headType = form.headType;
            if (form.hair != null) pawn.story.hairDef = form.hair;
            pawn.story.HairColor = form.hairColour;
            pawn.story.skinColorOverride = form.skinColour;
        }

        pawn.gender = form.gender;

        // The xenotype is captured on the form and deliberately not applied. A kandra wearing a
        // Mistborn's body looks like her; it does not make them Mistborn. Shapeshifting is a
        // disguise, not a way to farm powers off corpses.

        // The pawn keeps its own Name. Overwriting it looked right until a kandra died wearing
        // somebody's face and the corpse kept their name for good. The worn name lives on the
        // comp and is added at the places that display it.
        pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    }

    /// <summary>
    ///     Builds a face nobody has ever worn.
    /// </summary>
    /// <remarks>
    ///     The reward for practice. A kandra that has eaten enough bodies stops needing a
    ///     template and can put together a person who never existed, which is exactly the thing
    ///     that makes an old kandra impossible to search for.
    /// </remarks>
    public static void WearInvented(Pawn kandra) {
        CompKandraForms? forms = kandra.TryGetComp<CompKandraForms>();
        if (forms == null || !forms.CanFreeForm) return;

        forms.RememberTrueBody();

        KandraForm invented = new KandraForm {
            // A face nobody has worn needs a name nobody answers to. Any person namer will do;
            // the kandra is inventing, not impersonating.
            nameFull = PawnBioAndNameGenerator.GeneratePawnName(kandra, NameStyle.Full).ToStringShort,
            bodyType = kandra.story?.bodyType,
            headType = kandra.story?.headType,
            hair = kandra.story?.hairDef,
            hairColour = kandra.story?.HairColor ?? Color.white,
            skinColour = kandra.story?.SkinColor ?? Color.white,
            gender = kandra.gender,
        };
        invented.nameShort = invented.nameFull;

        forms.Remember(invented);
        Wear(kandra, invented);
    }
}
