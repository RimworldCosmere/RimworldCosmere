using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Game;

/// <summary>
///     Per-tile ash multipliers, computed once at world generation. Scribed rather than
///     recomputed because the mounts never move and the sum is over every mount on the planet.
/// </summary>
public class AshmountExposureCache : GameComponent {
    private Dictionary<int, float> exposure = new Dictionary<int, float>();

    public AshmountExposureCache(Verse.Game game) { }

    /// <summary>
    ///     1 for any tile no mount reaches, and for any layer but the surface - tile ids are only
    ///     unique within a layer, and only the surface is ever populated.
    /// </summary>
    public static float For(PlanetTile tile) {
        if (!tile.Valid || !tile.Layer.IsRootSurface) return 1f;

        AshmountExposureCache? cache = Current.Game?.GetComponent<AshmountExposureCache>();
        if (cache == null) return 1f;

        return cache.exposure.TryGetValue(tile.tileId, out float value) ? value : 1f;
    }

    public static void Set(Dictionary<int, float> values) {
        AshmountExposureCache? cache = Current.Game?.GetComponent<AshmountExposureCache>();
        if (cache == null) {
            Log.Warn("AshmountExposureCache: no component, exposure will read as 1 everywhere.");
            return;
        }

        cache.exposure = values;
    }

    public override void ExposeData() {
        Scribe_Collections.Look(ref exposure, "ashmountExposure", LookMode.Value, LookMode.Value);
        exposure ??= new Dictionary<int, float>();
    }
}
