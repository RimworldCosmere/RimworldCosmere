using System.Collections.Generic;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Rendering;

public static class GuiStyleCache
{
    private const int MaxEntries = 256;

    private readonly struct Key : global::System.IEquatable<Key>
    {
        public readonly int FontId;
        public readonly int PixelSize;
        public readonly FontStyle Weight;

        public Key(Font f, int pixelSize, FontStyle w)
        {
            FontId = f?.GetInstanceID() ?? 0;
            PixelSize = pixelSize;
            Weight = w;
        }

        public bool Equals(Key o) => FontId == o.FontId && PixelSize == o.PixelSize && Weight == o.Weight;
        public override bool Equals(object? o) => o is Key k && Equals(k);
        public override int GetHashCode() => (FontId, PixelSize, Weight).GetHashCode();
    }

    private static readonly Dictionary<Key, GUIStyle> cache = new Dictionary<Key, GUIStyle>(256);
    private static readonly LinkedList<Key> lru = new LinkedList<Key>();
    private static readonly Dictionary<Key, LinkedListNode<Key>> lruNodes = new Dictionary<Key, LinkedListNode<Key>>();

    public static GUIStyle Get(Font font, int pixelSize, FontStyle weight = FontStyle.Normal)
    {
        Key key = new Key(font, pixelSize, weight);
        if (cache.TryGetValue(key, out GUIStyle style))
        {
            Touch(key);
            return style;
        }
        style = new GUIStyle { font = font, fontSize = pixelSize, fontStyle = weight };
        cache[key] = style;
        LinkedListNode<Key> node = lru.AddFirst(key);
        lruNodes[key] = node;
        Evict();
        return style;
    }

    private static void Touch(Key key)
    {
        if (lruNodes.TryGetValue(key, out LinkedListNode<Key> node))
        {
            lru.Remove(node);
            lru.AddFirst(node);
        }
    }

    private static void Evict()
    {
        while (cache.Count > MaxEntries)
        {
            LinkedListNode<Key>? tail = lru.Last;
            if (tail == null)
            {
                break;
            }
            lru.RemoveLast();
            lruNodes.Remove(tail.Value);
            cache.Remove(tail.Value);
        }
    }

    public static void Clear()
    {
        cache.Clear();
        lru.Clear();
        lruNodes.Clear();
    }
}
