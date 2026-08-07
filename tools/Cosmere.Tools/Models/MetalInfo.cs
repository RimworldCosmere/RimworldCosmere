using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cosmere.Tools.Models;

public enum AllomancyGroup {
    None,
    Physical,
    Mental,
    Enhancement,
    Temporal,
}

public enum FeruchemyGroup {
    None,
    Physical,
    Cognitive,
    Spiritual,
    Hybrid,
}

public enum Axis {
    None,
    Internal,
    External,
}

public enum Polarity {
    None,
    Pushing,
    Pulling,
}

public class MetalInfo {
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? DefName { get; set; }

    public ColorInfo Color { get; set; } = new();

    public ColorInfo? ColorTwo { get; set; }

    public bool GodMetal { get; set; }

    /// <summary>The Shards this metal is made of, with what burning it grants to each.</summary>
    public List<ShardGrantInfo>? Shards { get; set; }

    public bool Stackable { get; set; } = true;

    public float DrawSize { get; set; } = 1f;

    public float Beauty { get; set; }

    public float MarketValue { get; set; }

    public int MaxAmount { get; set; } = 100;

    public Dictionary<string, string[]> GenesToGrant { get; set; } = new();

    public AllomancyInfo? Allomancy { get; set; }

    public FeruchemyInfo? Feruchemy { get; set; }

    public BuildableInfo? Buildable { get; set; }

    public MiningInfo? Mining { get; set; }

    public AlloyInfo? Alloy { get; set; }
}

public class AllomancyInfo {
    public string? UserName { get; set; }

    public string Description { get; set; } = string.Empty;

    public AllomancyGroup Group { get; set; } = AllomancyGroup.None;

    public Axis Axis { get; set; } = Axis.None;

    public Polarity Polarity { get; set; } = Polarity.None;

    public List<string> Abilities { get; set; } = new();
}

public class FeruchemyInfo {
    public string? UserName { get; set; }

    public string Description { get; set; } = string.Empty;

    public FeruchemyGroup Group { get; set; } = FeruchemyGroup.None;

    public List<string> Abilities { get; set; } = new();

    public string? Attribute { get; set; }

    public bool CustomHediffClass { get; set; }

    public FeruchemyAbilityInfo? Store { get; set; }

    public FeruchemyAbilityInfo? Tap { get; set; }

    public float? StoreRateMultiplier { get; set; }

    public float? TapRateMultiplier { get; set; }
}

public class FeruchemyAbilityInfo {
    public string Description { get; set; } = string.Empty;

    public int Stages { get; set; } = 20;

    public bool MultiplyBySeverity { get; set; }

    public Dictionary<string, CapacityMod> CapacityMods { get; set; } = new();

    public Dictionary<string, float>? StatOffsets { get; set; }

    public Dictionary<string, float>? StatFactors { get; set; }

    public float? HungerRateFactor { get; set; }
}

public class CapacityMod {
    public float? Factor { get; set; }

    public float? Offset { get; set; }
}

public class BuildableInfo {
    public float Commonality { get; set; }

    public DefenseInfo? Defense { get; set; }

    public OffenseInfo? Offense { get; set; }

    public Dictionary<string, float>? StuffStatFactors { get; set; }
}

public class DefenseInfo {
    public float? Sharp { get; set; }

    public float? Blunt { get; set; }

    public float? Heat { get; set; }

    public float? ColdInsulation { get; set; }

    public float? HeatInsulation { get; set; }
}

public class OffenseInfo {
    public float? Sharp { get; set; }

    public float? Blunt { get; set; }

    public float? ArmorPenetration { get; set; }

    public float? Cooldown { get; set; }
}

public class MiningInfo {
    public string? Description { get; set; }

    public int HitPoints { get; set; }

    public int Yield { get; set; }

    public float Commonality { get; set; }

    public int[] SizeRange { get; set; } = new int[2];
}

public class AlloyInfo {
    public List<AlloyIngredient> Ingredients { get; set; } = new();

    [JsonConverter(typeof(StringOrListConverter))]
    public List<string>? Stuff { get; set; }

    public int? StuffCount { get; set; }

    public AlloyProduct Product { get; set; } = new();

    public string Type { get; set; } = "simple";
}

public class AlloyIngredient {
    public List<string>? Items { get; set; }

    public List<string>? Stuffs { get; set; }

    public int Count { get; set; }
}

public class AlloyProduct {
    public string Item { get; set; } = string.Empty;

    public int Count { get; set; }
}

/// <summary>One Shard a metal is made of, and the Connection burning it grants there.</summary>
public class ShardGrantInfo {
    public string Shard { get; set; } = string.Empty;

    public int Grant { get; set; }
}
