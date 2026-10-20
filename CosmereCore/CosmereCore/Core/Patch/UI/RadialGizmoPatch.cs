using Concord;
using Cosmere.Core.UI.Radial;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class RadialGizmoPatch : Pawn {
    [Inject(At.Return, nameof(GetGizmos))]
    private void AfterGetGizmos(ControlHandle<IEnumerable<Gizmo>> ch) {
        Pawn self = this;
        ch.ReturnValue = WithRadialGizmo(ch.ReturnValue, self);
    }

    private static IEnumerable<Gizmo> WithRadialGizmo(IEnumerable<Gizmo>? values, Pawn instance) {
        if (values != null) {
            foreach (Gizmo gizmo in values) {
                yield return gizmo;
            }
        }

        if (instance.Faction is not { IsPlayer: true }) yield break;
        if (!RadialController.HasRadialFor(instance)) yield break;

        yield return new Command_OpenRadial(instance);
    }
}
