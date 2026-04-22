using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Surface;

public enum SurfaceRole { Primary, Raised, Sunken, Accent }

public static partial class Surface
{
    public static LightweaveNode ByRole(
        SurfaceRole role,
        EdgeInsets? padding = null,
        RadiusSpec? radius = null,
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        ThemeSlot slot = role switch
        {
            SurfaceRole.Primary => ThemeSlot.SurfacePrimary,
            SurfaceRole.Raised  => ThemeSlot.SurfaceRaised,
            SurfaceRole.Sunken  => ThemeSlot.SurfaceSunken,
            SurfaceRole.Accent  => ThemeSlot.SurfaceAccent,
            _ => ThemeSlot.SurfacePrimary,
        };
        return Box(padding, new BackgroundSpec.Solid(slot), null, radius, children, line, file);
    }
}
