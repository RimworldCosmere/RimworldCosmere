using System;
using Cosmere.Lightweave.Runtime;

namespace Cosmere.Lightweave.Adapter;

/// <summary>
///     Identity registry for adapters whose lifecycle is tied to a game entity (Thing, Pawn, etc.)
///     rather than to a Window's open/close cycle. Use this when the adapter instance can outlive
///     a paint cycle and the same logical UI region (an inspect tab, a gizmo on a particular thing,
///     a float menu option attached to an entity) needs to keep its hook state across draws.
///
///     Lightweave currently has TWO lifecycle paths for adapters - choose the right one when
///     adding a new adapter:
///
///     - **Entity-scoped identity (this registry)**: AsInspectTab, AsGizmo, AsFloatMenuOption.
///       The adapter calls <see cref="GetOrCreate(int, AdapterKind, int)" /> with the owning
///       entity's id (typically <c>thingIDNumber</c>) so that the same Thing painting the same
///       adapter kind reuses the same root id - and thus the same hook state - across paint
///       cycles. Cleanup happens via <see cref="ReleaseAllFor(int)" /> when the entity is
///       destroyed and via <see cref="ClearAll()" /> when the game tears down. Use this when the
///       adapter has no enclosing <see cref="Verse.Window" /> to bound its lifecycle.
///
///     - **Window-scoped identity (per-instance Guid in the adapter)**: AsMainTab, AsChoiceLetter
///       (via its inner <c>LetterWindow : LightweaveWindow</c>). The adapter or its hosting
///       window owns a fresh <c>Guid</c> from construction and calls <c>LightweaveRoot.Release</c>
///       directly in <c>PostClose</c>. Use this when there is no stable entity id (a MainTabWindow
///       is a singleton owned by its <c>MainButtonDef</c>; a ChoiceLetter dialog is per-letter
///       and not tied to a Thing's destruction).
///
///     Do NOT mix the two for the same surface. If you find yourself needing entity-keyed cleanup
///     for a window-scoped adapter, route the adapter through this registry and let
///     <see cref="ReleaseAllFor(int)" /> handle teardown; if you find yourself wanting Window
///     PostClose semantics for an entity-scoped adapter, hold the adapter inside a
///     <see cref="LightweaveWindow" /> rather than calling this registry.
/// </summary>
public static class AdapterStoreRegistry {
    private static readonly Dictionary<Key, Guid> ids = new Dictionary<Key, Guid>();

    public static Guid GetOrCreate(int entityId, AdapterKind kind) {
        return GetOrCreate(entityId, kind, 0);
    }

    public static Guid GetOrCreate(int entityId, AdapterKind kind, int subKey) {
        Key key = new Key(entityId, kind, subKey);
        if (ids.TryGetValue(key, out Guid existing)) {
            return existing;
        }

        Guid created = Guid.NewGuid();
        ids[key] = created;
        return created;
    }

    public static void Release(int entityId, AdapterKind kind) {
        ReleaseWhere(k => k.EntityId == entityId && k.Kind == kind);
    }

    public static void ReleaseAllFor(int entityId) {
        ReleaseWhere(k => k.EntityId == entityId);
    }

    public static void ClearAll() {
        List<Guid> released = new List<Guid>(ids.Values);
        ids.Clear();
        for (int i = 0; i < released.Count; i++) {
            LightweaveRoot.Release(released[i]);
        }
    }

    private static void ReleaseWhere(Predicate<Key> filter) {
        List<Key> removed = new List<Key>();
        foreach (KeyValuePair<Key, Guid> entry in ids) {
            if (filter(entry.Key)) {
                removed.Add(entry.Key);
            }
        }

        for (int i = 0; i < removed.Count; i++) {
            Key key = removed[i];
            Guid guid = ids[key];
            ids.Remove(key);
            LightweaveRoot.Release(guid);
        }
    }

    private readonly struct Key : IEquatable<Key> {
        public readonly int EntityId;
        public readonly AdapterKind Kind;
        public readonly int SubKey;

        public Key(int entityId, AdapterKind kind, int subKey) {
            EntityId = entityId;
            Kind = kind;
            SubKey = subKey;
        }

        public bool Equals(Key other) {
            return EntityId == other.EntityId && Kind == other.Kind && SubKey == other.SubKey;
        }

        public override bool Equals(object? obj) {
            return obj is Key other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hash = EntityId * 397 ^ (int)Kind;
                return hash * 397 ^ SubKey;
            }
        }
    }
}