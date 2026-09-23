using Verse;

namespace Cosmere.Core.Comp.Game;

public static class SpiritWebExtension {
    public static Connection? GetConnection<T>(this T self, ILoadReferenceable target)
        where T : ILoadReferenceable {
        return SpiritWeb.Instance?.TryGetConnection(target, self);
    }

    public static Connection? GetOrCreateConnection<T>(this T self, ILoadReferenceable target)
        where T : ILoadReferenceable {
        // a byline-only pawn (book author) is never placed; keying a connection off it NREs deep in the web.
        if (target == null) return null;

        return SpiritWeb.Instance?.GetOrCreateConnection(target, self);
    }

    public static Connection? SetConnection<T>(this T self, ILoadReferenceable target, float value)
        where T : ILoadReferenceable {
        return SpiritWeb.Instance?.SetConnection(target, self, value);
    }

    public static Connection? AdjustConnection<T>(this T self, ILoadReferenceable target, float delta)
        where T : ILoadReferenceable {
        return SpiritWeb.Instance?.AdjustConnection(target, self, delta);
    }
}
