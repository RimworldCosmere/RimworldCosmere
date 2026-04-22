using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Rendering;

public static class RectSnap
{
    public static Rect Snap(Rect r) => new Rect(
        Mathf.Round(r.x), Mathf.Round(r.y),
        Mathf.Round(r.width), Mathf.Round(r.height));
}
