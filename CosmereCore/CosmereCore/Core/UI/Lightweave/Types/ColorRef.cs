using UnityEngine;
using Cosmere.Core.UI.Lightweave.Tokens;

namespace Cosmere.Core.UI.Lightweave.Types;

public abstract record ColorRef
{
    public sealed record Literal(Color Value) : ColorRef;
    public sealed record Token(ThemeSlot Slot) : ColorRef;

    public static implicit operator ColorRef(Color c) => new Literal(c);
    public static implicit operator ColorRef(ThemeSlot s) => new Token(s);
}
