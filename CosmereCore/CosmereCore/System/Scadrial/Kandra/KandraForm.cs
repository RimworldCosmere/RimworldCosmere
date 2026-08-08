using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     One body a kandra has eaten and can put back on.
/// </summary>
/// <remarks>
///     Stores what the kandra reproduces, which is everything anyone looks at plus the name they
///     answer to. A kandra wearing this is that person as far as the colony is concerned, and
///     only bronze sees otherwise.
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

    public KandraForm() { }

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
        };
    }

    public string Label => nameShort ?? nameFull ?? "unknown";

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
    }
}
