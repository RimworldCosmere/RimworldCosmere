using System;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Adapter;

public static class AdapterStoreRegistry {
    private static readonly Dictionary<Key, Guid> ids = new Dictionary<Key, Guid>();

    public static Guid Get(int entityId, AdapterKind kind) {
        return Get(entityId, kind, 0);
    }

    public static Guid Get(int entityId, AdapterKind kind, int subKey) {
        Key key = new Key(entityId, kind, subKey);
        if (ids.TryGetValue(key, out Guid existing)) {
            return existing;
        }

        Guid created = Guid.NewGuid();
        ids[key] = created;
        return created;
    }

    public static void Release(int entityId, AdapterKind kind) {
        List<Key> removed = new List<Key>();
        foreach (KeyValuePair<Key, Guid> entry in ids) {
            if (entry.Key.EntityId == entityId && entry.Key.Kind == kind) {
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

    public static void ReleaseAllFor(int entityId) {
        List<Key> removed = new List<Key>();
        foreach (KeyValuePair<Key, Guid> entry in ids) {
            if (entry.Key.EntityId == entityId) {
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