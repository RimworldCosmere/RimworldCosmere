using System.Collections.Generic;
using Cosmere.System.Scadrial.Def;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

public enum HemalurgicStealType {
    // Human attributes (base metals)
    HumanStrength,        // Iron
    HumanSenses,          // Tin
    EmotionalFortitude,   // Zinc
    MentalFortitude,      // Copper

    // Allomantic powers (alloy metals)
    PhysicalAllomancy,    // Steel -> Iron/Steel/Tin/Pewter allomancy
    MentalAllomancy,      // Bronze -> Zinc/Brass/Copper/Bronze allomancy
    TemporalAllomancy,    // Cadmium -> Cadmium/Bendalloy/Gold/Electrum allomancy
    EnhancementAllomancy, // Electrum -> Chromium/Nicrosil/Aluminum/Duralumin allomancy

    // Feruchemical powers (alloy metals)
    PhysicalFeruchemy,    // Pewter -> Iron/Steel/Tin/Pewter feruchemy
    CognitiveFeruchemy,   // Brass -> Zinc/Brass/Copper/Bronze feruchemy
    HybridFeruchemy,      // Gold -> Gold/Electrum/Cadmium/Bendalloy feruchemy
    SpiritualFeruchemy,   // Bendalloy -> Chromium/Nicrosil/Aluminum/Duralumin feruchemy

    // Special
    Investiture,          // Nicrosil
    RemoveAllPowers,      // Aluminum
    ConnectionIdentity,   // Duralumin
    AnyPower,             // Atium (player selects)
    AllAbilities,         // Lerasium (takes everything)
}

public static class HemalurgicConstants {
    public const int DecayIntervalTicks = 2500;
    public const float DecayHalfLifeDays = 1f;
    public const float MinChargeStrength = 0.05f;
    public const float ThinNeedleStrengthMultiplier = 0.7f;
    public const float CorpseChargeMultiplier = 0.8f;
    public const float ExtractionChargeMultiplier = 0.5f;
    public const float InvestitureTheftFraction = 0.25f;
    public const int CorpseFreshnessTickLimit = 2500;

    private static readonly Dictionary<string, HemalurgicStealType> MetalStealMap = new() {
        // Base metals -> Human attributes
        ["Iron"] = HemalurgicStealType.HumanStrength,
        ["Tin"] = HemalurgicStealType.HumanSenses,
        ["Zinc"] = HemalurgicStealType.EmotionalFortitude,
        ["Copper"] = HemalurgicStealType.MentalFortitude,

        // Alloy metals -> Metallic arts powers
        ["Steel"] = HemalurgicStealType.PhysicalAllomancy,
        ["Pewter"] = HemalurgicStealType.PhysicalFeruchemy,
        ["Bronze"] = HemalurgicStealType.MentalAllomancy,
        ["Brass"] = HemalurgicStealType.CognitiveFeruchemy,
        ["Cadmium"] = HemalurgicStealType.TemporalAllomancy,
        ["Gold"] = HemalurgicStealType.HybridFeruchemy,
        ["Bendalloy"] = HemalurgicStealType.SpiritualFeruchemy,
        ["Electrum"] = HemalurgicStealType.EnhancementAllomancy,

        // Higher metals -> Special
        ["Chromium"] = HemalurgicStealType.HumanStrength, // Destiny - not implemented, treat as strength
        ["Nicrosil"] = HemalurgicStealType.Investiture,
        ["Aluminum"] = HemalurgicStealType.RemoveAllPowers,
        ["Duralumin"] = HemalurgicStealType.ConnectionIdentity,

        // God metals
        ["Atium"] = HemalurgicStealType.AnyPower,
        ["Lerasium"] = HemalurgicStealType.AllAbilities,
    };

    private static readonly Dictionary<HemalurgicStealType, string[]> AllomanticGroupMetals = new() {
        [HemalurgicStealType.PhysicalAllomancy] = ["Iron", "Steel", "Tin", "Pewter"],
        [HemalurgicStealType.MentalAllomancy] = ["Zinc", "Brass", "Copper", "Bronze"],
        [HemalurgicStealType.TemporalAllomancy] = ["Cadmium", "Bendalloy", "Gold", "Electrum"],
        [HemalurgicStealType.EnhancementAllomancy] = ["Chromium", "Nicrosil", "Aluminum", "Duralumin"],
    };

    private static readonly Dictionary<HemalurgicStealType, string[]> FeruchemicGroupMetals = new() {
        [HemalurgicStealType.PhysicalFeruchemy] = ["Iron", "Steel", "Tin", "Pewter"],
        [HemalurgicStealType.CognitiveFeruchemy] = ["Zinc", "Brass", "Copper", "Bronze"],
        [HemalurgicStealType.HybridFeruchemy] = ["Gold", "Electrum", "Cadmium", "Bendalloy"],
        [HemalurgicStealType.SpiritualFeruchemy] = ["Chromium", "Nicrosil", "Aluminum", "Duralumin"],
    };

    public static HemalurgicStealType GetStealType(MetallicArtsMetalDef metal) {
        if (MetalStealMap.TryGetValue(metal.defName, out HemalurgicStealType stealType)) {
            return stealType;
        }
        return HemalurgicStealType.HumanStrength;
    }

    public static string[] GetAllomanticGroupMetals(HemalurgicStealType type) {
        if (AllomanticGroupMetals.TryGetValue(type, out string[]? metals)) {
            return metals;
        }
        return [];
    }

    public static string[] GetFeruchemicGroupMetals(HemalurgicStealType type) {
        if (FeruchemicGroupMetals.TryGetValue(type, out string[]? metals)) {
            return metals;
        }
        return [];
    }

    public static bool IsHumanAttribute(HemalurgicStealType type) {
        return type is HemalurgicStealType.HumanStrength
            or HemalurgicStealType.HumanSenses
            or HemalurgicStealType.EmotionalFortitude
            or HemalurgicStealType.MentalFortitude;
    }

    public static bool IsAllomanticSteal(HemalurgicStealType type) {
        return type is HemalurgicStealType.PhysicalAllomancy
            or HemalurgicStealType.MentalAllomancy
            or HemalurgicStealType.TemporalAllomancy
            or HemalurgicStealType.EnhancementAllomancy;
    }

    public static bool IsFeruchemicSteal(HemalurgicStealType type) {
        return type is HemalurgicStealType.PhysicalFeruchemy
            or HemalurgicStealType.CognitiveFeruchemy
            or HemalurgicStealType.HybridFeruchemy
            or HemalurgicStealType.SpiritualFeruchemy;
    }

    public static bool RequiresSelection(HemalurgicStealType type) {
        return IsAllomanticSteal(type)
            || IsFeruchemicSteal(type)
            || type == HemalurgicStealType.AnyPower;
    }

    public static string GetStealTypeLabel(HemalurgicStealType type) {
        return type switch {
            HemalurgicStealType.HumanStrength => "CS_Hemalurgy_StealStrength".Translate(),
            HemalurgicStealType.HumanSenses => "CS_Hemalurgy_StealSenses".Translate(),
            HemalurgicStealType.EmotionalFortitude => "CS_Hemalurgy_StealEmotional".Translate(),
            HemalurgicStealType.MentalFortitude => "CS_Hemalurgy_StealMental".Translate(),
            HemalurgicStealType.PhysicalAllomancy => "CS_Hemalurgy_StealPhysAllomancy".Translate(),
            HemalurgicStealType.MentalAllomancy => "CS_Hemalurgy_StealMentAllomancy".Translate(),
            HemalurgicStealType.TemporalAllomancy => "CS_Hemalurgy_StealTempAllomancy".Translate(),
            HemalurgicStealType.EnhancementAllomancy => "CS_Hemalurgy_StealEnhAllomancy".Translate(),
            HemalurgicStealType.PhysicalFeruchemy => "CS_Hemalurgy_StealPhysFeruchemy".Translate(),
            HemalurgicStealType.CognitiveFeruchemy => "CS_Hemalurgy_StealCogFeruchemy".Translate(),
            HemalurgicStealType.HybridFeruchemy => "CS_Hemalurgy_StealHybFeruchemy".Translate(),
            HemalurgicStealType.SpiritualFeruchemy => "CS_Hemalurgy_StealSprFeruchemy".Translate(),
            HemalurgicStealType.Investiture => "CS_Hemalurgy_StealInvestiture".Translate(),
            HemalurgicStealType.RemoveAllPowers => "CS_Hemalurgy_StealRemoveAll".Translate(),
            HemalurgicStealType.ConnectionIdentity => "CS_Hemalurgy_StealConnection".Translate(),
            HemalurgicStealType.AnyPower => "CS_Hemalurgy_StealAny".Translate(),
            HemalurgicStealType.AllAbilities => "CS_Hemalurgy_StealAll".Translate(),
            _ => type.ToString(),
        };
    }
}
