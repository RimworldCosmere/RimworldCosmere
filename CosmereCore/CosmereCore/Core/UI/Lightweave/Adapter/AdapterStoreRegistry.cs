using System;
using System.Collections.Generic;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Adapter;

public static class AdapterStoreRegistry
{
    private readonly struct Key : IEquatable<Key>
    {
        public readonly int EntityId;
        public readonly AdapterKind Kind;

        public Key(int entityId, AdapterKind kind)
        {
            EntityId = entityId;
            Kind = kind;
        }

        public bool Equals(Key other) => EntityId == other.EntityId && Kind == other.Kind;

        public override bool Equals(object? obj) => obj is Key other && Equals(other);

        public override int GetHashCode() => unchecked(EntityId * 397 ^ (int)Kind);
    }

    private static readonly Dictionary<Key, Guid> ids = new Dictionary<Key, Guid>();

    public static Guid Get(int entityId, AdapterKind kind)
    {
        Key key = new Key(entityId, kind);
        if (ids.TryGetValue(key, out Guid existing))
        {
            return existing;
        }
        Guid created = Guid.NewGuid();
        ids[key] = created;
        return created;
    }

    public static void Release(int entityId, AdapterKind kind)
    {
        Key key = new Key(entityId, kind);
        if (ids.TryGetValue(key, out Guid existing))
        {
            ids.Remove(key);
            LightweaveRoot.Release(existing);
        }
    }

    public static void ReleaseAllFor(int entityId)
    {
        List<Key> removed = new List<Key>();
        foreach (KeyValuePair<Key, Guid> entry in ids)
        {
            if (entry.Key.EntityId == entityId)
            {
                removed.Add(entry.Key);
            }
        }
        for (int i = 0; i < removed.Count; i++)
        {
            Key key = removed[i];
            Guid guid = ids[key];
            ids.Remove(key);
            LightweaveRoot.Release(guid);
        }
    }
}
