using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Theme;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

internal static class InputSurface
{
    public static void Draw(Rect rect, Theme.Theme theme, InteractionState state)
    {
        ThemeSlot borderSlot = state.Focused
            ? ThemeSlot.BorderFocus
            : state.Hovered ? ThemeSlot.BorderHover : ThemeSlot.BorderDefault;
        ThemeSlot surfaceSlot = state.Disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceInput;

        BackgroundSpec bg = new BackgroundSpec.Solid(surfaceSlot);
        BorderSpec border = BorderSpec.All(new Rem(1f / 16f), borderSlot);
        RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));
        PaintBox.Draw(rect, bg, border, radius);
    }
}
