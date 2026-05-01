using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Types;

public abstract record FontRef {
    public static implicit operator FontRef(Font f) {
        return new Literal(f);
    }

    public static implicit operator FontRef(FontRole r) {
        return new Role(r);
    }

    public sealed record Literal(Font Value) : FontRef;

    public sealed record Role(FontRole RoleValue) : FontRef;
}