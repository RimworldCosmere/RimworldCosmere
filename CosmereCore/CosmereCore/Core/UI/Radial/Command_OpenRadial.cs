using RimWorld;
using Verse;

namespace Cosmere.Core.UI.Radial;

public class Command_OpenRadial : Command_Action {
    public Command_OpenRadial(Pawn pawn) {
        defaultLabel = "CC_Radial_Gizmo_Label".Translate();
        defaultDesc = "CC_Radial_Gizmo_Desc".Translate(
            RadialKeyBindingDefOf.Cosmere_Keybind_RadialOpen.MainKeyLabel.Named("KEY")
        );
        icon = RadialWedgeTex.GizmoIcon();
        action = () => RadialController.OpenForPawn(pawn);
    }
}
