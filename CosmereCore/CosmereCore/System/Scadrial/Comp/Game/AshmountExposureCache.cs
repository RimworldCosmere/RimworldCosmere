using System.Collections.Generic;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Game;

/// <summary>
///     Per-tile ash multipliers, computed once at world generation. Scribed rather than
///     recomputed because the mounts never move and the sum is over every mount on the planet.
/// </summary>
public class AshmountExposureCache : GameComponent {
    private Dictionary<int, float> exposure = new Dictionary<int, float>();

    public AshmountExposureCache(Verse.Game game) { }

    /// <summary>1 for any tile no mount reaches, which is every tile on a non-Scadrial world.</summary>
    public static float For(int tileId) {
        AshmountExposureCache? cache = Current.Game?.GetComponent<AshmountExposureCache>();
        if (cache == null) return 1f;

        return cache.exposure.TryGetValue(tileId, out float value) ? value : 1f;
    }

    public static void Set(Dictionary<int, float> values) {
        AshmountExposureCache? cache = Current.Game?.GetComponent<AshmountExposureCache>();
        if (cache == null) {
            Core.Logger.Warning("AshmountExposureCache: no component, exposure will read as 1 everywhere.");
            return;
        }

        cache.exposure = values;
    }

    public override void ExposeData() {
        Scribe_Collections.Look(ref exposure, "ashmountExposure", LookMode.Value, LookMode.Value);
        exposure ??= new Dictionary<int, float>();
    }
}
