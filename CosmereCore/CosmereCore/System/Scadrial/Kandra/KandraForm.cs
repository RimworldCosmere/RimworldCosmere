using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     One body a kandra has eaten and can put back on.
/// </summary>
/// <remarks>
///     Stores what the kandra reproduces, which is everything anyone looks at plus the name they
///     answer to, and who that person belonged to. A kandra wearing this is that person as far as
///     the colony is concerned, and only bronze sees otherwise.
/// </remarks>
public class KandraForm : IExposable {
    public BodyTypeDef? bodyType;
    public Gender gender;
    public HairDef? hair;
    public Color hairColour;
    public HeadTypeDef? headType;

    /// <summary>The name the colony will use. The whole point of wearing a face.</summary>
    public string? nameFull;

    public string? nameShort;
    public Color skinColour;
    public XenotypeDef? xenotype;

    /// <summary>Who they belonged to. Shown on the card so you can tell two guards apart.</summary>
    public FactionDef? faction;

    /// <summary>What they believed. Held by reference, so it goes null if the ideo is gone.</summary>
    public Ideo? ideo;

    /// <summary>
    ///     Set when this form is an animal rather than a person.
    /// </summary>
    /// <remarks>
    ///     A human shape is worn by editing the kandra in place. An animal one cannot be: a
    ///     pawn's race is its ThingDef and does not change, so wearing it means putting the
    ///     kandra aside and standing a pawn of this kind up in its place.
    /// </remarks>
    public PawnKindDef? animalKind;

    public bool IsAnimal => animalKind != null;

    /// <summary>
    ///     A pawn that exists only to be drawn.
    /// </summary>
    /// <remarks>
    ///     Rendering a portrait needs a Pawn, and the one this form came from was eaten. Rather
    ///     than serialise a whole Pawn into every save, this builds a throwaway one from the
    ///     stored appearance the first time the picker asks for it, and lets it go on load.
    /// </remarks>
    private Pawn? portrait;

    public KandraForm() { }

    /// <summary>An animal shape, remembered by its kind rather than by its face.</summary>
    public static KandraForm FromAnimal(Pawn source, PawnKindDef wornAs) {
        return new KandraForm {
            nameFull = source.kindDef.label,
            nameShort = source.kindDef.label,
            gender = source.gender,
            animalKind = wornAs,
        };
    }

    public static KandraForm From(Pawn source) {
        return new KandraForm {
            nameFull = source.Name?.ToStringFull,
            nameShort = source.Name?.ToStringShort,
            bodyType = source.story?.bodyType,
            headType = source.story?.headType,
            hair = source.story?.hairDef,
            hairColour = source.story?.HairColor ?? Color.white,
            skinColour = source.story?.SkinColor ?? Color.white,
            gender = source.gender,
            xenotype = source.genes?.Xenotype,
            faction = source.Faction?.def,
            ideo = source.Ideo,
        };
    }

    public string Label => nameShort ?? nameFull ?? "unknown";

    /// <summary>Builds the stand-in on first use. Null if generation failed for any reason.</summary>
    public Pawn? PortraitPawn {
        get {
            if (portrait != null) return portrait;

            if (animalKind != null) {
                try {
                    portrait = PawnGenerator.GeneratePawn(
                        new PawnGenerationRequest(animalKind, forceGenerateNewPawn: true)
                    );
                } catch (global::System.Exception) {
                    return null;
                }

                return portrait;
            }

            try {
                portrait = PawnGenerator.GeneratePawn(
                    new PawnGenerationRequest(
                        RimWorld.PawnKindDefOf.Colonist,
                        null,
                        PawnGenerationContext.NonPlayer,
                        forceGenerateNewPawn: true,
                        canGeneratePawnRelations: false,
                        fixedGender: gender,
                        forcedXenotype: xenotype
                    )
                );
            } catch (global::System.Exception) {
                return null;
            }

            if (portrait.story != null) {
                if (bodyType != null) portrait.story.bodyType = bodyType;
                if (headType != null) portrait.story.headType = headType;
                portrait.story.hairDef = hair ?? portrait.story.hairDef;
                portrait.story.HairColor = hairColour;
                portrait.story.skinColorOverride = skinColour;
            }

            portrait.Drawer?.renderer?.SetAllGraphicsDirty();
            return portrait;
        }
    }

    public void ExposeData() {
        Scribe_Values.Look(ref nameFull, "nameFull");
        Scribe_Values.Look(ref nameShort, "nameShort");
        Scribe_Defs.Look(ref bodyType, "bodyType");
        Scribe_Defs.Look(ref headType, "headType");
        Scribe_Defs.Look(ref hair, "hair");
        Scribe_Values.Look(ref hairColour, "hairColour");
        Scribe_Values.Look(ref skinColour, "skinColour");
        Scribe_Values.Look(ref gender, "gender");
        Scribe_Defs.Look(ref xenotype, "xenotype");
        Scribe_Defs.Look(ref faction, "faction");
        Scribe_References.Look(ref ideo, "ideo");
        Scribe_Defs.Look(ref animalKind, "animalKind");
    }
}
