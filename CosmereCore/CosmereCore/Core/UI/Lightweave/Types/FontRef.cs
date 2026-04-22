using UnityEngine;
using Cosmere.Core.UI.Lightweave.Tokens;

namespace Cosmere.Core.UI.Lightweave.Types;

public abstract record FontRef
{
    public sealed record Literal(Font Value) : FontRef;
    public sealed record Role(FontRole RoleValue) : FontRef;

    public static implicit operator FontRef(Font f) => new Literal(f);
    public static implicit operator FontRef(FontRole r) => new Role(r);
}
