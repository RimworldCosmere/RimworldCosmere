using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Surface;

public static partial class Surface
{
    public static LightweaveNode Card(
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        EdgeInsets pad = EdgeInsets.All(SpacingScale.Md);
        BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
        RadiusSpec r = RadiusSpec.All(new Rem(0.5f));
        BorderSpec b = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
        return Box(pad, bg, b, r, children, line, file);
    }
}
