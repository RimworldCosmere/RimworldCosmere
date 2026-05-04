using System;
using UnityEngine;

namespace Cosmere.Lightweave.Rendering;

public static class GuiStyleCache {
    private const int MaxEntries = 256;

    private static readonly Dictionary<Key, GUIStyle> cache = new Dictionary<Key, GUIStyle>(256);
    private static readonly LinkedList<Key> lru = new LinkedList<Key>();
    private static readonly Dictionary<Key, LinkedListNode<Key>> lruNodes = new Dictionary<Key, LinkedListNode<Key>>();

    public static GUIStyle GetOrCreate(Font font, int pixelSize, FontStyle fontStyle = FontStyle.Normal) {
        Key key = new Key(font, pixelSize, fontStyle);
        if (cache.TryGetValue(key, out GUIStyle style)) {
            Touch(key);
            return style;
        }

        style = new GUIStyle { font = font, fontSize = pixelSize, fontStyle = fontStyle };
        style.normal.textColor = Color.white;
        style.hover.textColor = Color.white;
        style.active.textColor = Color.white;
        style.focused.textColor = Color.white;
        cache[key] = style;
        LinkedListNode<Key> node = lru.AddFirst(key);
        lruNodes[key] = node;
        Evict();
        return style;
    }

    private static void Touch(Key key) {
        if (lruNodes.TryGetValue(key, out LinkedListNode<Key> node)) {
            lru.Remove(node);
            lru.AddFirst(node);
        }
    }

    private static void Evict() {
        while (cache.Count > MaxEntries) {
            LinkedListNode<Key>? tail = lru.Last;
            if (tail == null) {
                break;
            }

            lru.RemoveLast();
            lruNodes.Remove(tail.Value);
            cache.Remove(tail.Value);
        }
    }

    public static void Clear() {
        cache.Clear();
        lru.Clear();
        lruNodes.Clear();
    }

    private readonly struct Key : IEquatable<Key> {
        public readonly int FontId;
        public readonly int PixelSize;
        public readonly FontStyle FontStyle;

        public Key(Font f, int pixelSize, FontStyle fontStyle) {
            FontId = f?.GetInstanceID() ?? 0;
            PixelSize = pixelSize;
            FontStyle = fontStyle;
        }

        public bool Equals(Key o) {
            return FontId == o.FontId && PixelSize == o.PixelSize && FontStyle == o.FontStyle;
        }

        public override bool Equals(object? o) {
            return o is Key k && Equals(k);
        }

        public override int GetHashCode() {
            return (FontId, PixelSize, FontStyle).GetHashCode();
        }
    }
}