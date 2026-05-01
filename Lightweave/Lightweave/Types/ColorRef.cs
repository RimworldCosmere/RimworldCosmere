using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Types;

public abstract record ColorRef {
    public static implicit operator ColorRef(Color c) {
        return new Literal(c);
    }

    public static implicit operator ColorRef(ThemeSlot s) {
        return new Token(s);
    }

    public sealed record Literal(Color Value) : ColorRef;

    public sealed record Token(ThemeSlot Slot) : ColorRef;
}