using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout
{
    public static class Divider
    {
        public static LightweaveNode Horizontal(
            Rem? thickness = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = "")
        {
            float t = (thickness ?? new Rem(1f / 16f)).ToPixels();
            LightweaveNode n = NodeBuilder.New("Divider.Horizontal", line, file);
            n.Paint = (rect, _) =>
            {
                Rect bar = new Rect(rect.x, rect.y + (rect.height - t) / 2f, rect.width, t);
                PaintBox.Draw(bar, new BackgroundSpec.Solid(ThemeSlot.BorderSubtle), null, null);
            };
            return n;
        }

        public static LightweaveNode Vertical(
            Rem? thickness = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = "")
        {
            float t = (thickness ?? new Rem(1f / 16f)).ToPixels();
            LightweaveNode n = NodeBuilder.New("Divider.Vertical", line, file);
            n.Paint = (rect, _) =>
            {
                Rect bar = new Rect(rect.x + (rect.width - t) / 2f, rect.y, t, rect.height);
                PaintBox.Draw(bar, new BackgroundSpec.Solid(ThemeSlot.BorderSubtle), null, null);
            };
            return n;
        }
    }
}
