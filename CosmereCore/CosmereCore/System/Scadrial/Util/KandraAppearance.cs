using UnityEngine;
using Verse;
using EyeColourRow = (string name, string hex, string labelKey);
using EyeLightRow = (string name, float strength, string labelKey);
using HairColourRow = (string name, string hex, string labelKey);
using IrisRow = (string name, float scale, string labelKey);
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

    /// <summary>
    ///     What a kandra can light its eyes with. Nothing rolls these either - eyes are only ever
    ///     picked - so there is no weight column.
    /// </summary>
    private static readonly EyeColourRow[] EyeColourTable = [
        ("carnelian", "b04a2f", "CS_Kandra_EyeColour_Carnelian"),
        ("amber", "d99a1c", "CS_Kandra_EyeColour_Amber"),
        ("citrine", "e8c54a", "CS_Kandra_EyeColour_Citrine"),
        ("peridot", "9bbf3f", "CS_Kandra_EyeColour_Peridot"),
        ("malachite", "0f8a6a", "CS_Kandra_EyeColour_Malachite"),
        ("turquoise", "30a3b8", "CS_Kandra_EyeColour_Turquoise"),
        ("lapis", "1f4fa8", "CS_Kandra_EyeColour_Lapis"),
        ("iolite", "5a4fb8", "CS_Kandra_EyeColour_Iolite"),
        ("sugilite", "8a4fa0", "CS_Kandra_EyeColour_Sugilite"),
        ("moonstone", "dfe6ee", "CS_Kandra_EyeColour_Moonstone"),
        ("hematite", "6e6f76", "CS_Kandra_EyeColour_Hematite"),
        ("onyx", "161418", "CS_Kandra_EyeColour_Onyx"),
    ];

    /// <summary>Every eye colour, in the order a picker should show them.</summary>
    public static readonly IReadOnlyList<EyeColourRow> AllEyeColours = EyeColourTable;

    /// <summary>What a picked second eye colour says when the player wants both eyes to match.</summary>
    public const string EyeColourNone = "none";

    /// <summary>
    ///     The row a stored eye colour name points at, or null. Same reason as
    ///     <see cref="FindHairColour" />: an older save can name a colour that is gone.
    /// </summary>
    public static EyeColourRow? FindEyeColour(string? name) {
        foreach (EyeColourRow colour in EyeColourTable) {
            if (colour.name == name) return colour;
        }

        return null;
    }

    /// <summary>The colour a stored eye colour name draws with. White when nothing matches.</summary>
    public static Color EyeColorFor(string? name) {
        EyeColourRow? picked = FindEyeColour(name);
        return picked != null ? Parse(picked.Value.hex) : Color.white;
    }

    private const string DefaultIris = "standard";

    /// <summary>How wide the lit part of the eye is drawn, as a multiple of the socket.</summary>
    private static readonly IrisRow[] IrisSizes = [
        ("small", 0.58f, "CS_Kandra_Iris_Small"),
        ("standard", 0.80f, "CS_Kandra_Iris_Standard"),
        ("wide", 1.00f, "CS_Kandra_Iris_Wide"),
    ];

    /// <summary>Every iris size, in the order a picker should show them.</summary>
    public static readonly IReadOnlyList<IrisRow> AllIrisSizes = IrisSizes;

    /// <summary>The row a stored iris size name points at, or null.</summary>
    public static IrisRow? FindIrisSize(string? name) {
        foreach (IrisRow iris in IrisSizes) {
            if (iris.name == name) return iris;
        }

        return null;
    }

    /// <summary>
    ///     The drawn scale for a stored iris name. A name the table no longer carries falls back
    ///     to the standard size, so an old save draws a normal eye rather than none at all.
    /// </summary>
    public static float IrisScaleFor(string? name) {
        return (FindIrisSize(name) ?? FindIrisSize(DefaultIris) ?? IrisSizes[0]).scale;
    }

    private const string DefaultEyeLight = "steady";

    /// <summary>What a stored light name says when the player put the light out.</summary>
    public const string EyeLightOff = "off";

    /// <summary>
    ///     How hard the eyes glow, as a multiple of the iris colour's brightness. Not alpha - the
    ///     eye art is fully opaque, so anything above 1 would clamp to nothing and 0.5 would sit on
    ///     the Cutout alpha test and flicker. Must stay in (0, 2]: under 1 the colour darkens toward
    ///     black, over 1 it bleaches toward white by the remainder.
    /// </summary>
    private static readonly EyeLightRow[] EyeLights = [
        ("dim", 1.15f, "CS_Kandra_EyeLight_Dim"),
        ("steady", 1.4f, "CS_Kandra_EyeLight_Steady"),
        ("burning", 1.75f, "CS_Kandra_EyeLight_Burning"),
    ];

    /// <summary>Every light strength, in the order a picker should show them.</summary>
    public static readonly IReadOnlyList<EyeLightRow> AllEyeLights = EyeLights;

    /// <summary>The row a stored light strength name points at, or null.</summary>
    public static EyeLightRow? FindEyeLight(string? name) {
        foreach (EyeLightRow light in EyeLights) {
            if (light.name == name) return light;
        }

        return null;
    }

    /// <summary>
    ///     The brightness multiple for a stored light name. Undesigned and off both give 1, no
    ///     lift; a name the table no longer carries falls back to steady.
    /// </summary>
    public static float EyeLightStrengthFor(string? name) {
        if (name is null or EyeLightOff) return 1f;

        return (FindEyeLight(name) ?? FindEyeLight(DefaultEyeLight) ?? EyeLights[0]).strength;
    }

    /// <summary>An unlit iris draws its stone colour exactly, so the palette is what a player sees.</summary>
    private const float UnlitDim = 1f;

    /// <summary>How much every channel gains per point of strength above 1.</summary>
    private const float LightLift = 0.35f;

    /// <summary>
    ///     The colour the iris draws with. Unlit is the stone itself. Lighting it adds a flat
    ///     amount to every channel, which clamps on a pale stone - the bloom node carries the rest
    ///     of the separation, so the iris does not have to.
    /// </summary>
    public static Color EyeDrawColorFor(string? colourName, string? lightName) {
        Color stone = EyeColorFor(colourName);
        float lift = (EyeLightStrengthFor(lightName) - 1f) * LightLift;

        return new Color(
            Mathf.Clamp01((stone.r * UnlitDim) + lift),
            Mathf.Clamp01((stone.g * UnlitDim) + lift),
            Mathf.Clamp01((stone.b * UnlitDim) + lift),
            stone.a
        );
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
