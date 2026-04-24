using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialAnchor {
    public static Vector2 PawnScreenCenter(Pawn pawn) {
        Vector3 world = pawn.DrawPos;
        Vector3 screen = Find.Camera.WorldToScreenPoint(world);
        return new Vector2(screen.x, Verse.UI.screenHeight - screen.y);
    }
}