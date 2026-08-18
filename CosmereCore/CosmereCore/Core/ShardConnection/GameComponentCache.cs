using Verse;

namespace Cosmere.Core.ShardConnection;

/// <summary>
///     The running game's component of one type, held on to until the game itself changes.
/// </summary>
/// <remarks>
///     GetComponent walks the component list, and the Connection read path runs it twice per read
///     on every frame a panel is open. A new or loaded game is a new Game, so identity invalidates.
/// </remarks>
public static class GameComponentCache<T>
    where T : Verse.GameComponent {
    private static Verse.Game? owner;

    private static T? component;

    /// <summary>Null outside a running game, and never caches a null against a live one.</summary>
    public static T? Get() {
        Verse.Game? current = Current.Game;
        if (current == null) return null;

        if (component == null || !ReferenceEquals(current, owner)) {
            owner = current;
            component = current.GetComponent<T>();
        }

        return component;
    }
}
