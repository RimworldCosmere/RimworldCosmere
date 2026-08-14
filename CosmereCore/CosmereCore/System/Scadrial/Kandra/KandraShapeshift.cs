using System.Collections.Generic;
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

    /// <summary>
    ///     Puts a shape on, taking whatever the last one was off first.
    /// </summary>
    /// <remarks>
    ///     Idempotent rather than additive, and that is not a style preference. <see cref="Revert" />
    ///     calls this with the kandra's true body, which <c>KandraForm.From</c> builds without an
    ///     <c>animalKind</c> - so a branch on the incoming form's <c>IsAnimal</c> would never fire
    ///     on the way out, and the animal hediff would survive being human again. The same hole
    ///     opens wearing a human face directly from an animal shape, which goes through
    ///     <see cref="Wear" /> rather than Revert.
    ///     <para>
    ///         Stripping unconditionally at the top means every caller is correct without knowing
    ///         any of this.
    ///     </para>
    /// </remarks>
    private static void ApplyTo(Pawn pawn, KandraForm form) {
        CompKandraForms? forms = pawn.TryGetComp<CompKandraForms>();

        // Always, whatever is being put on next.
        Unshape(pawn, forms);

        if (form.IsAnimal) {
            StowGear(pawn, forms);

            // Before the hediff, never after. The render node reads the worn form from the comp
            // inside its constructor, where its own hediff back-reference is still null.
            forms?.RememberWorkPriorities();
            forms?.SetCurrent(form);

            HediffDef? shape = DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_AnimalShape");
            if (shape != null) pawn.health?.AddHediff(shape);

            pawn.Drawer?.renderer?.SetAllGraphicsDirty();
            PortraitsCache.SetDirty(pawn);
            return;
        }

        // A human face. Guarded, because KandraForm.FromAnimal stores the eaten ANIMAL's gender
        // and leaves the colours at default(Color) - transparent black - so applying these
        // unguarded would change the colonist's gender and paint their skin invisible.
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
        PortraitsCache.SetDirty(pawn);
    }

    /// <summary>Takes off an animal shape, if one is on. Safe when none is.</summary>
    private static void Unshape(Pawn pawn, CompKandraForms? forms) {
        HediffDef? shape = DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_AnimalShape");
        Hediff? worn = shape == null ? null : pawn.health?.hediffSet?.GetFirstHediffOfDef(shape);
        if (worn == null) return;

        pawn.health!.RemoveHediff(worn);
        UnstowGear(pawn, forms);

        // After the hediff is gone, never before: SetPriority logs an error for a work type that
        // is still disabled.
        forms?.RestoreWorkPriorities();
    }

    /// <summary>
    ///     Moves what the kandra was holding and wearing into its own pack.
    /// </summary>
    /// <remarks>
    ///     A wolf cannot hold a rifle, and leaving one equipped would let a raid that walked past
    ///     the dog get shot by it - <c>FloatMenuUtility.GetRangedAttackAction</c> only checks
    ///     <c>WorkTags.Violent</c>, which the shape hediff does not disable.
    /// </remarks>
    private static void StowGear(Pawn pawn, CompKandraForms? forms) {
        if (pawn.inventory == null || forms == null) return;

        List<Verse.Thing> equipped = [];
        List<Verse.Thing> worn = [];

        if (pawn.equipment != null) {
            List<ThingWithComps> weapons = [.. pawn.equipment.AllEquipmentListForReading];
            for (int i = 0; i < weapons.Count; i++) {
                if (!pawn.equipment.TryTransferEquipmentToContainer(weapons[i], pawn.inventory.innerContainer)) {
                    continue;
                }

                equipped.Add(weapons[i]);
            }
        }

        if (pawn.apparel != null) {
            List<Apparel> clothes = [.. pawn.apparel.WornApparel];
            for (int i = 0; i < clothes.Count; i++) {
                if (!pawn.apparel.TryDrop(clothes[i], out Apparel? dropped, pawn.Position, false)) continue;
                if (dropped == null) continue;

                if (dropped.Spawned) dropped.DeSpawn();
                if (pawn.inventory.innerContainer.TryAdd(dropped)) worn.Add(dropped);
            }
        }

        forms.RememberGear(equipped, worn);
    }

    /// <summary>
    ///     Puts back whatever is still in the pack, the way it was carried.
    /// </summary>
    /// <remarks>
    ///     Only what survived. Anything the player made the shape drop stays dropped, because
    ///     being droppable was the point of putting it in the inventory.
    /// </remarks>
    private static void UnstowGear(Pawn pawn, CompKandraForms? forms) {
        if (pawn.inventory == null || forms == null) return;

        List<Verse.Thing> equipped = [.. forms.WasEquipped];
        List<Verse.Thing> worn = [.. forms.WasWorn];

        for (int i = 0; i < equipped.Count; i++) {
            if (!pawn.inventory.innerContainer.Contains(equipped[i])) continue;
            if (equipped[i] is not ThingWithComps weapon) continue;

            pawn.inventory.innerContainer.Remove(weapon);
            pawn.equipment?.AddEquipment(weapon);
        }

        for (int i = 0; i < worn.Count; i++) {
            if (!pawn.inventory.innerContainer.Contains(worn[i])) continue;
            if (worn[i] is not Apparel clothing) continue;

            pawn.inventory.innerContainer.Remove(clothing);
            pawn.apparel?.Wear(clothing, false);
        }

        forms.ForgetGear();
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
