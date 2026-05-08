using System;
using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Types;

public abstract record ColorRef {
    private static readonly Token[] TokenByThemeSlot = BuildTokenCache();

    private static Token[] BuildTokenCache() {
        Array values = Enum.GetValues(typeof(ThemeSlot));
        int max = 0;
        foreach (object v in values) {
            int idx = (int)v;
            if (idx > max) {
                max = idx;
            }
        }

        Token[] cache = new Token[max + 1];
        foreach (object v in values) {
            int idx = (int)v;
            cache[idx] = new Token((ThemeSlot)v);
        }

        return cache;
    }

    public static implicit operator ColorRef(Color c) {
        return new Literal(c);
    }

    public static implicit operator ColorRef(ThemeSlot s) {
        return TokenByThemeSlot[(int)s];
    }

    public sealed record Literal(Color Value) : ColorRef;

    public sealed record Token(ThemeSlot Slot) : ColorRef;
}