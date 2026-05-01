using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Typography;

public static partial class Typography {
    internal static TextAnchor ResolveAnchor(TextAlign align, Direction dir) {
        return align switch {
            TextAlign.Start => dir == Direction.Ltr ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight,
            TextAlign.End => dir == Direction.Ltr ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft,
            TextAlign.Left => TextAnchor.MiddleLeft,
            TextAlign.Right => TextAnchor.MiddleRight,
            TextAlign.Center => TextAnchor.MiddleCenter,
            TextAlign.Justify => TextAnchor.MiddleCenter,
            _ => TextAnchor.MiddleLeft,
        };
    }
}