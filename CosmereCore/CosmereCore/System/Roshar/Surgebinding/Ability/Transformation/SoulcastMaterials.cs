using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Transformation;

public enum SoulcastCategory {
    DroppedItem,
    StuffedThing,
    Plant,
    Mineable,
    Fire,
    Pawn,
    Corpse,
    SteamGeyser,
    Terrain,
}

public enum SoulcastMode {
    ConvertDrop,
    ChangeStuff,
    Wall,
    Terraform,
    Geyser,
    Sculpture,
    Destroy,
}

public static class SoulcastMaterials {
    private static List<ThingDef>? cachedStoneBlocks;
    private static List<ThingDef>? cachedStoneChunks;

    public static SoulcastCategory Categorize(LocalTargetInfo target, Map map) {
        if (target.HasThing && target.Thing != null && !target.Thing.Destroyed) {
            Verse.Thing thing = target.Thing;

            if (thing is Fire) return SoulcastCategory.Fire;
            if (thing is Pawn) return SoulcastCategory.Pawn;
            if (thing is Corpse) return SoulcastCategory.Corpse;
            if (thing.def == RimWorld.ThingDefOf.SteamGeyser) return SoulcastCategory.SteamGeyser;
            if (thing.def.mineable) return SoulcastCategory.Mineable;
            if (thing.def.plant != null) return SoulcastCategory.Plant;
            if (thing.def.MadeFromStuff && thing.Stuff != null && !IsDroppedItem(thing))
                return SoulcastCategory.StuffedThing;
            return SoulcastCategory.DroppedItem;
        }

        return SoulcastCategory.Terrain;
    }

    public static bool IsDroppedItem(Verse.Thing thing) {
        if (thing.def.category != ThingCategory.Item) return false;
        if (thing.def.IsApparel || thing.def.IsWeapon) return false;
        return true;
    }

    public static List<ThingDef> GetStoneBlocks() {
        if (cachedStoneBlocks != null) return cachedStoneBlocks;
        cachedStoneBlocks = [
            DefDatabase<ThingDef>.GetNamed("BlocksGranite"),
            DefDatabase<ThingDef>.GetNamed("BlocksMarble"),
            DefDatabase<ThingDef>.GetNamed("BlocksLimestone"),
            DefDatabase<ThingDef>.GetNamed("BlocksSandstone"),
            DefDatabase<ThingDef>.GetNamed("BlocksSlate"),
        ];
        return cachedStoneBlocks;
    }

    public static List<ThingDef> GetStoneChunks() {
        if (cachedStoneChunks != null) return cachedStoneChunks;
        cachedStoneChunks = [];
        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading) {
            if (def.IsWithinCategory(ThingCategoryDefOf.StoneChunks) ||
                def.IsWithinCategory(ThingCategoryDefOf.Chunks)) {
                cachedStoneChunks.Add(def);
            }
        }

        return cachedStoneChunks;
    }

    public static List<ThingDef> GetAvailableDropOutputs(int ideal) {
        List<ThingDef> materials = [
            RimWorld.ThingDefOf.WoodLog,
            ..GetStoneBlocks(),
        ];

        if (ideal >= 2) {
            materials.Add(RimWorld.ThingDefOf.Steel);
            AddIfExists(materials, "Silver");
            AddIfExists(materials, "Gold");
        }

        if (ideal >= 3) {
            materials.Add(RimWorld.ThingDefOf.Plasteel);
            AddIfExists(materials, "Uranium");
            AddIfExists(materials, "Jade");
        }

        AddIfExists(materials, "RawPotatoes");
        AddIfExists(materials, "RawCorn");
        AddIfExists(materials, "RawRice");
        AddIfExists(materials, "RawBerries");
        AddIfExists(materials, "Cloth");

        return materials;
    }

    public static List<ThingDef> GetAvailableStuffs(int ideal) {
        List<ThingDef> stuffs = [..GetStoneBlocks()];

        if (ideal >= 2) {
            stuffs.Add(RimWorld.ThingDefOf.Steel);
            AddIfExists(stuffs, "Silver");
            AddIfExists(stuffs, "Gold");
        }

        if (ideal >= 3) {
            stuffs.Add(RimWorld.ThingDefOf.Plasteel);
            AddIfExists(stuffs, "Uranium");
            AddIfExists(stuffs, "Jade");
        }

        stuffs.Add(RimWorld.ThingDefOf.WoodLog);

        return stuffs;
    }

    public static ThingDef? GetBlocksForChunk(ThingDef chunkDef) {
        string chunkName = chunkDef.defName;
        if (chunkName.StartsWith("Chunk")) {
            string stoneName = chunkName.Substring(5);
            return DefDatabase<ThingDef>.GetNamedSilentFail("Blocks" + stoneName);
        }

        return null;
    }

    public static List<TerrainDef> GetTerrainOptions() {
        return [
            TerrainDefOf.Soil,
            TerrainDefOf.SoilRich,
            TerrainDefOf.Sand,
            TerrainDefOf.Gravel,
            TerrainDefOf.Mud,
            DefDatabase<TerrainDef>.GetNamed("MarshyTerrain"),
            TerrainDefOf.WaterShallow,
            TerrainDefOf.WaterDeep,
            TerrainDefOf.Ice,
            TerrainDefOf.Concrete,
        ];
    }

    public static List<ThingDef> GetAvailableMineables() {
        List<ThingDef> mineables = [];
        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading) {
            if (def.mineable && def.building?.isNaturalRock == true) {
                mineables.Add(def);
            }
        }

        return mineables;
    }

    public static ThingDef? GetMineableForMaterial(ThingDef material) {
        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading) {
            if (!def.mineable) continue;
            if (def.building?.mineableThing == material) return def;
        }

        if (material.IsStuff && material.defName.StartsWith("Blocks")) {
            string stoneName = material.defName.Substring(6);
            return DefDatabase<ThingDef>.GetNamedSilentFail(stoneName);
        }

        return null;
    }

    public static float GetMaterialCost(ThingDef mat) {
        string defName = mat.defName;
        if (defName == "WoodLog") return 1f;
        if (defName.StartsWith("Blocks")) return 1f;
        if (defName.StartsWith("Chunk")) return 0.5f;
        if (defName == "Steel") return 2f;
        if (defName == "Silver") return 2.5f;
        if (defName == "Gold") return 3f;
        if (defName == "Plasteel") return 4f;
        if (defName == "Uranium") return 4f;
        if (defName == "Jade") return 3f;
        if (defName == "Cloth") return 1f;
        if (defName.StartsWith("Raw")) return 1.5f;
        return 2f;
    }

    public static float GetTerrainCost(TerrainDef terrain) {
        if (terrain == TerrainDefOf.Soil || terrain == TerrainDefOf.Sand || terrain == TerrainDefOf.Gravel) return 1f;
        if (terrain == TerrainDefOf.SoilRich) return 1.5f;
        if (terrain == TerrainDefOf.Mud) return 1f;
        if (terrain.defName == "MarshyTerrain") return 1.5f;
        if (terrain == TerrainDefOf.WaterShallow) return 3f;
        if (terrain == TerrainDefOf.WaterDeep) return 5f;
        if (terrain == TerrainDefOf.Ice) return 2f;
        if (terrain == TerrainDefOf.Concrete) return 2.5f;
        return 2f;
    }

    private static void AddIfExists(List<ThingDef> list, string defName) {
        ThingDef? def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
        if (def != null) list.Add(def);
    }
}