using RimWorld;

namespace Cosmere.Core;

[DefOf]
public static class SitePartDefOf {
    public static SitePartDef Cosmere_SitePart_Persistent = null!;

    static SitePartDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(SitePartDefOf));
    }
}
