using Concord;
using Cosmere.System.Roshar.Surgebinding.Ability.Abrasion;
using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.Soulcasting;

[Patch]
public abstract class SoulcastOverlayPatch : MapInterface {
    [Inject(At.Return, nameof(MapInterfaceUpdate))]
    private void AfterMapInterfaceUpdate() {
        Map? map = Find.CurrentMap;
        if (map != null) {
            SoulcastOverlay.Draw(map);
            FrictionTrapOverlay.Draw();
        }
    }
}

[Patch(typeof(DesignationManager))]
public abstract class DesignationRemovedPatch {
    [InjectInstance]
    protected abstract DesignationManager Self { get; }

    [Inject(At.Return, nameof(DesignationManager.RemoveDesignation))]
    private void AfterRemoveDesignation(Designation des) {
        if (des.def == Designator_Soulcast.DesignationDef) {
            SoulcastOverlay.Remove(des.target.Cell, Self.map);
        }
    }
}
