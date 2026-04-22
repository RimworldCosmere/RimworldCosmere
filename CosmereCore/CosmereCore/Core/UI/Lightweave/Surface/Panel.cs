using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Layout;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Surface;

public static partial class Surface
{
    public static LightweaveNode Panel(
        LightweaveNode title,
        LightweaveNode body,
        LightweaveNode? footer = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        EdgeInsets pad = EdgeInsets.All(SpacingScale.Md);
        return Box(pad, new BackgroundSpec.Solid(ThemeSlot.SurfacePrimary), null, RadiusSpec.All(new Rem(0.5f)),
            c =>
            {
                c.Add(title);
                c.Add(Layout.Layout.Divider.Horizontal());
                c.Add(body);
                if (footer != null)
                {
                    c.Add(Layout.Layout.Divider.Horizontal());
                    c.Add(footer);
                }
            }, line, file);
    }
}
