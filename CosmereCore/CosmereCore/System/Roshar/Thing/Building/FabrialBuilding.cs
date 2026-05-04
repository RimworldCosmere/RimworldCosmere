using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thing.Building;

public abstract class FabrialBuilding : Verse.Building {
    protected CompFlickable compFlickerable = null!;
    protected CompGlower compGlower = null!;

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        compFlickerable = GetComp<CompFlickable>();
        compGlower = GetComp<CompGlower>();
    }

    protected void ToggleGlow(bool on) {
        if (Map == null) return;
        if (on) Map.glowGrid.RegisterGlower(compGlower);
        else Map.glowGrid.DeRegisterGlower(compGlower);
    }
}
