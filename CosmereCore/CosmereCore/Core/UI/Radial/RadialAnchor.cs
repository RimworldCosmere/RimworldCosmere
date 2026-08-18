using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialAnchor {
    public static Vector2 PawnScreenCenter(Pawn pawn) {
        Vector3 world = pawn.DrawPos;
        Vector3 screen = Find.Camera.WorldToScreenPoint(world);
        return new Vector2(screen.x, Verse.UI.screenHeight - screen.y);
    }

    public static Vector2 Resolve(Pawn pawn, bool preferPawn = false) {
        // gizmo click leaves the cursor on the command bar; following it would open the wheel off in a corner.
        Vector2 anchor = !preferPawn && Mod.GetModSettings<Cosmere.Core.Settings.CoreModSettings>().radialAnchorMouse
            ? Verse.UI.MousePositionOnUIInverted
            : PawnScreenCenter(pawn);
        const float half = RadialLayout.AbilityRingOuter + 20f;
        anchor.x = Mathf.Clamp(anchor.x, half, Verse.UI.screenWidth - half);
        anchor.y = Mathf.Clamp(anchor.y, half, Verse.UI.screenHeight - half);
        return anchor;
    }
}
