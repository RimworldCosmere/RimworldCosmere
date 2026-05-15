using Cosmere.Lightweave.Theme;
using Verse;

namespace Cosmere.Lightweave.ModsConfig;

internal enum ModKind {
    Core,
    Expansion,
    Library,
    CommunityMod,
}

internal static class ModKindResolver {
    public static ModKind Resolve(ModMetaData mod) {
        if (mod.IsCoreMod) {
            return ModKind.Core;
        }
        if (mod.Official) {
            return ModKind.Expansion;
        }
        if (IsLibrary(mod)) {
            return ModKind.Library;
        }
        return ModKind.CommunityMod;
    }

    public static string LabelKey(ModKind kind) {
        return kind switch {
            ModKind.Core => "CL_ModsConfig_Source_Core",
            ModKind.Expansion => "CL_ModsConfig_Source_Expansion",
            ModKind.Library => "CL_ModsConfig_Source_Library",
            _ => "CL_ModsConfig_Source_Local",
        };
    }

    public static ThemeSlot Tone(ModKind kind) {
        return kind switch {
            ModKind.Core => ThemeSlot.SurfaceAccent,
            ModKind.Expansion => ThemeSlot.SurfaceAccent,
            ModKind.Library => ThemeSlot.StatusInfo,
            _ => ThemeSlot.TextMuted,
        };
    }

    public static bool IsLocked(ModMetaData mod) {
        return mod.IsCoreMod || mod.Official;
    }

    private static bool IsLibrary(ModMetaData mod) {
        string? packageId = mod.PackageId?.ToLowerInvariant();
        if (string.IsNullOrEmpty(packageId)) {
            return false;
        }
        return packageId == "brrainz.harmony"
            || packageId == "unlimitedhugs.hugslib"
            || packageId == "krkr.rocketman"
            || packageId == "owlchemist.performanceoptimizer"
            || packageId.EndsWith(".harmony")
            || packageId.EndsWith(".hugslib");
    }
}
