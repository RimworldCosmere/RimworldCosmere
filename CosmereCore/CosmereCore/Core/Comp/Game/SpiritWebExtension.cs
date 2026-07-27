using Verse;

namespace Cosmere.Core.Comp.Game;

public static class SpiritWebExtension {
    public static Connection? GetConnection<T>(this T self, ILoadReferenceable target)
        where T : ILoadReferenceable {
        return SpiritWeb.Instance?.TryGetConnection(target, self);
    }

    public static Connection? GetOrCreateConnection<T>(this T self, ILoadReferenceable target)
        where T : ILoadReferenceable {
        // A pawn generated for a book's author byline is never placed, so its tile
        // and layer come back null. Keying a connection off that dereferences null
        // deep inside the web, which surfaced as a pawn generation failure.
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
