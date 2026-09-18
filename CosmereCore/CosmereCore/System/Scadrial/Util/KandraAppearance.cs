using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     Which textures and colour a kandra draws with while it is in its own shape.
/// </summary>
/// <remarks>
///     Not a BodyTypeDef and not a HeadTypeDef. Both would need every piece of apparel and every
///     hair to declare a matching texture, and a kandra out of its disguise wears neither. The
///     paths are swapped at render time instead, the same way koloss bodies are.
/// </remarks>
public static class KandraAppearance {
    private const string BodyDir = "Things/Pawn/Humanlike/Bodies/";
    private const string HeadDir = "Things/Pawn/Humanlike/Heads/";

    /// <summary>
    ///     What a kandra built its own body out of. Every entry sits outside the human skin
    ///     gamut - hue 18-24 at up to 0.55 saturation - or it reads as a person wearing nothing.
    /// </summary>
    private static readonly (string name, string hex, float weight)[] Materials = [
        ("ruby", "e0115f", 1.5f),
        ("sapphire", "0f52ba", 1.5f),
        ("emerald", "50c878", 1.5f),
        ("topaz", "ffd23f", 1.5f),
        ("amethyst", "9966cc", 1.5f),
        ("diamond", "b9f2ff", 1.5f),
        ("garnet", "a3123f", 1.5f),
        ("heliodor", "d1ee1d", 1.5f),
        ("zircon", "d8e9f0", 1.5f),
        ("smokestone", "4a4657", 1.5f),
        ("milky quartzite", "dff0f5", 2f),
        ("rose quartzite", "f0a8c8", 2f),
        ("smoky quartzite", "9aa3b5", 2f),
        ("pale wood", "d9a441", 2f),
        ("walnut", "8a4416", 2f),
        ("blackwood", "2e1c2a", 2f),
        ("marble", "e4eaf5", 0.8f),
        ("obsidian", "171320", 0.8f),
    ];

    /// <summary>The body texture for a formless kandra, or null when it is wearing a face.</summary>
    public static string? BodyGraphicPathFor(Pawn? pawn) {
        return IsFormless(pawn) ? BodyDir + Suffix(pawn!) : null;
    }

    /// <summary>The head texture for a formless kandra, or null when it is wearing a face.</summary>
    public static string? HeadGraphicPathFor(Pawn? pawn) {
        return IsFormless(pawn) ? HeadDir + Suffix(pawn!) : null;
    }

    /// <summary>
    ///     Whether the crafted true body is showing. Runs per node per frame for every pawn, so
    ///     the gene lookup goes first and nothing that is not a kandra reaches the comp scan.
    /// </summary>
    public static bool IsFormless(Pawn? pawn) {
        if (pawn?.genes?.HasActiveGene(GeneDefOf.Cosmere_Scadrial_Gene_TrueBody) != true) return false;

        Kandra.CompKandraForms? forms = pawn.TryGetComp<Kandra.CompKandraForms>();
        if (forms == null) return false;

        // The flag rides the form, so wearing the crafted body reads as grey the same as being it.
        return forms.Current != null ? forms.Current.crafted : forms.TrueBody?.crafted == true;
    }

    /// <summary>
    ///     Keeps the animal-shape hediff matching what the pawn is showing. A kandra whose true
    ///     body is an animal wears no disguise, so nothing else would ever put the hediff on.
    /// </summary>
    public static void SyncAnimalShape(Pawn? pawn) {
        if (pawn?.health == null) return;

        bool animal = Kandra.KandraShapeGraphicUtility.WornKind(pawn) != null;
        Verse.Hediff? showing = pawn.health.hediffSet?.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Scadrial_Hediff_AnimalShape
        );

        if (animal == (showing != null)) return;

        if (animal) {
            pawn.health.AddHediff(HediffDefOf.Cosmere_Scadrial_Hediff_AnimalShape);
        } else {
            pawn.health.RemoveHediff(showing);
        }

        pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    }

    /// <summary>
    ///     Puts the formless hediff where the disguise says it belongs. Rendering does not wait
    ///     on this - the hediff only carries the social penalty for being seen as a thing.
    /// </summary>
    public static void SyncFormless(Pawn? pawn) {
        Verse.Hediff? showing = pawn?.health?.hediffSet?.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Scadrial_Hediff_Formless
        );

        bool formless = IsFormless(pawn);
        if (formless == (showing != null)) return;

        if (formless) {
            pawn!.health?.AddHediff(HediffDefOf.Cosmere_Scadrial_Hediff_Formless);
        } else {
            pawn!.health?.RemoveHediff(showing);
        }
    }

    /// <summary>
    ///     What this kandra's true body is made of. Keyed off the pawn id, so it is the same body
    ///     every time it drops a disguise and survives a reload without a saved field.
    /// </summary>
    public static (string name, Color colour) TrueBodyMaterialFor(Pawn pawn) {
        Rand.PushState(pawn.thingIDNumber);
        try {
            float total = 0f;
            for (int i = 0; i < Materials.Length; i++) total += Materials[i].weight;

            float roll = Rand.Range(0f, total);
            for (int i = 0; i < Materials.Length; i++) {
                roll -= Materials[i].weight;
                if (roll <= 0f) return (Materials[i].name, Parse(Materials[i].hex));
            }

            return (Materials[0].name, Parse(Materials[0].hex));
        } finally {
            Rand.PopState();
        }
    }

    /// <summary>The colour half of <see cref="TrueBodyMaterialFor" />.</summary>
    public static Color TrueBodyColorFor(Pawn pawn) {
        return TrueBodyMaterialFor(pawn).colour;
    }

    private static Color Parse(string hex) {
        return ColorUtility.TryParseHtmlString("#" + hex, out Color color) ? color : Color.white;
    }

    /// <summary>Two sets of art. Gender None falls in with the male one.</summary>
    private static string Suffix(Pawn pawn) {
        return pawn.gender == Gender.Female ? "Kandra_Female" : "Kandra_Male";
    }
}
