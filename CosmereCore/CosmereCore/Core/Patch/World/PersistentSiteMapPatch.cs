using System.Collections.Generic;
using Concord;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Patch.World;

/// <summary>
///     Keeps a marked site's map alive across departures. Vanilla tears a site map down the
///     moment the last colonist leaves and regenerates it from scratch on the next visit, which
///     restores every ore vein the player just spent days mining out.
///     <para>
///         Patches CheckRemoveMapNow rather than ShouldRemoveMapNow: the latter reports through
///         an out parameter, while this is a plain void with no arguments, so cancelling it is
///         unambiguous. The quest that owns the site destroys it explicitly when it ends, so the
///         map is not leaked.
///     </para>
/// </summary>
[Patch]
public abstract class PersistentSiteMapPatch : MapParent {
    [Inject(At.Head, nameof(CheckRemoveMapNow))]
    private Control BeforeCheckRemoveMapNow() {
        // Cast through object: the patch class is a compile-time stand-in for MapParent, so the
        // compiler will not accept a direct pattern match against a subclass of it.
        if ((object)this is not Site site) return Control.Continue;

        List<SitePart> parts = site.parts;
        if (parts == null) return Control.Continue;

        for (int i = 0; i < parts.Count; i++) {
            if (parts[i].def == Cosmere.Core.SitePartDefOf.Cosmere_SitePart_Persistent) {
                return Control.Cancel;
            }
        }

        return Control.Continue;
    }
}
