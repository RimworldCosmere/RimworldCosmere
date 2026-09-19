using UnityEngine;
using Verse;
using HairColourRow = (string name, string hex, string labelKey);
using MaterialRow = (string name, string hex, float weight, string labelKey, string group);

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
    private static readonly MaterialRow[] Materials = [
        ("ruby", "e0115f", 1.5f, "CS_Kandra_Material_Ruby", "gem"),
        ("sapphire", "0f52ba", 1.5f, "CS_Kandra_Material_Sapphire", "gem"),
        ("emerald", "50c878", 1.5f, "CS_Kandra_Material_Emerald", "gem"),
        ("topaz", "ffd23f", 1.5f, "CS_Kandra_Material_Topaz", "gem"),
        ("amethyst", "9966cc", 1.5f, "CS_Kandra_Material_Amethyst", "gem"),
        ("diamond", "b9f2ff", 1.5f, "CS_Kandra_Material_Diamond", "gem"),
        ("garnet", "a3123f", 1.5f, "CS_Kandra_Material_Garnet", "gem"),
        ("heliodor", "d1ee1d", 1.5f, "CS_Kandra_Material_Heliodor", "gem"),
        ("zircon", "d8e9f0", 1.5f, "CS_Kandra_Material_Zircon", "gem"),
        ("smokestone", "4a4657", 1.5f, "CS_Kandra_Material_Smokestone", "gem"),
        ("milky quartzite", "dff0f5", 2f, "CS_Kandra_Material_MilkyQuartzite", "stone"),
        ("rose quartzite", "f0a8c8", 2f, "CS_Kandra_Material_RoseQuartzite", "stone"),
        ("smoky quartzite", "9aa3b5", 2f, "CS_Kandra_Material_SmokyQuartzite", "stone"),
        ("pale wood", "d9a441", 2f, "CS_Kandra_Material_PaleWood", "wood"),
        ("walnut", "8a4416", 2f, "CS_Kandra_Material_Walnut", "wood"),
        ("blackwood", "2e1c2a", 2f, "CS_Kandra_Material_Blackwood", "wood"),
        ("marble", "e4eaf5", 0.8f, "CS_Kandra_Material_Marble", "stone"),
        ("obsidian", "171320", 0.8f, "CS_Kandra_Material_Obsidian", "stone"),
    ];

    /// <summary>
    ///     What a kandra can dye the hair on its true body. Nothing rolls these - hair is only
    ///     ever picked - so there is no weight column.
    /// </summary>
    private static readonly HairColourRow[] HairColourTable = [
        ("ash blonde", "d8c9a3", "CS_Kandra_HairColour_AshBlonde"),
        ("wheat", "c7a457", "CS_Kandra_HairColour_Wheat"),
        ("copper", "a4562a", "CS_Kandra_HairColour_Copper"),
        ("auburn", "7a3520", "CS_Kandra_HairColour_Auburn"),
        ("chestnut", "5a3a24", "CS_Kandra_HairColour_Chestnut"),
        ("soot", "2b2622", "CS_Kandra_HairColour_Soot"),
        ("iron grey", "8e8c88", "CS_Kandra_HairColour_IronGrey"),
        ("bone white", "e6e2d8", "CS_Kandra_HairColour_BoneWhite"),
        ("stormlight", "9fd9e0", "CS_Kandra_HairColour_Stormlight"),
        ("oxide", "4e7d6b", "CS_Kandra_HairColour_Oxide"),
        ("wine", "6b2440", "CS_Kandra_HairColour_Wine"),
        ("ink", "1b1b2a", "CS_Kandra_HairColour_Ink"),
    ];

    /// <summary>Every hair colour, in the order a picker should show them.</summary>
    public static readonly IReadOnlyList<HairColourRow> AllHairColours = HairColourTable;

    /// <summary>
    ///     The row a stored hair colour name points at, or null. Same reason as
    ///     <see cref="FindMaterial" />: an older save can name a colour that is gone.
    /// </summary>
    public static HairColourRow? FindHairColour(string? name) {
        foreach (HairColourRow colour in HairColourTable) {
            if (colour.name == name) return colour;
        }

        return null;
    }

    /// <summary>The colour a stored hair colour name draws with. White when nothing matches.</summary>
    public static Color HairColorFor(string? name) {
        HairColourRow? picked = FindHairColour(name);
        return picked != null ? Parse(picked.Value.hex) : Color.white;
    }

    /// <summary>The material groups, in the order a picker should show them.</summary>
    public static readonly (string group, string labelKey)[] MaterialGroups = [
        ("gem", "CS_Kandra_MaterialGroup_Gem"),
        ("stone", "CS_Kandra_MaterialGroup_Stone"),
        ("wood", "CS_Kandra_MaterialGroup_Wood"),
    ];

    /// <summary>Every material, sorted into <see cref="MaterialGroups" /> order.</summary>
    public static readonly IReadOnlyList<MaterialRow> AllMaterials = MaterialGroups
        .SelectMany(group => Materials.Where(material => material.group == group.group))
        .ToArray();

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
    ///     What this kandra's true body is made of. A picked material wins; anything else rolls
    ///     off the pawn id, so an undesigned kandra keeps the same body across reloads.
    /// </summary>
    public static (string name, Color colour) TrueBodyMaterialFor(Pawn pawn) {
        MaterialRow? picked = FindMaterial(pawn.TryGetComp<Kandra.CompKandraForms>()?.TrueBodyMaterial);
        if (picked != null) return (picked.Value.name, Parse(picked.Value.hex));

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

    /// <summary>
    ///     The row a stored material name points at, or null. A save from an older build can
    ///     name a material the table no longer carries, so this never assumes a hit.
    /// </summary>
    public static MaterialRow? FindMaterial(string? name) {
        foreach (MaterialRow material in Materials) {
            if (material.name == name) return material;
        }

        return null;
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
