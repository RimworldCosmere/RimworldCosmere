using System;
using Cosmere.Lightweave.Tokens;
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

    // Role-aware overload: when Bold is requested, swap to the bold variant of the
    // role (Body -> BodyBold) and use FontStyle.Normal on the GUIStyle so we never
    // fall back to Unity's faux-bold (which draws shifted/jagged text).
    public static GUIStyle GetOrCreate(Theme.Theme theme, FontRole role, int pixelSize, FontStyle fontStyle = FontStyle.Normal) {
        if (fontStyle == FontStyle.Bold) {
            FontRole boldRole = role switch {
                FontRole.Body => FontRole.BodyBold,
                _ => role,
            };
            return GetOrCreate(theme.GetFont(boldRole), pixelSize, FontStyle.Normal);
        }
        return GetOrCreate(theme.GetFont(role), pixelSize, fontStyle);
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