using Verse;

namespace Cosmere.Core.Comp.Game;

public static class SpiritWebExtensions {
    public static Connection? GetConnection<T>(this T self, ILoadReferenceable target) where T : ILoadReferenceable {
        return SpiritWeb.Instance?.GetOrCreateConnection(target, self);
    }

    public static Connection? InitializeConnection<T>(this T self, ILoadReferenceable target) where T : ILoadReferenceable {
        return SpiritWeb.Instance?.InitializeConnection(target, self);
    }

    public static Connection? SetConnection<T>(this T self, ILoadReferenceable target, float value) where T : ILoadReferenceable {
        return SpiritWeb.Instance?.SetConnection(target, self, value);
    }

    public static Connection? AdjustConnection<T>(this T self, ILoadReferenceable target, float delta) where T : ILoadReferenceable {
        return SpiritWeb.Instance?.AdjustConnection(target, self, delta);
    }
}
