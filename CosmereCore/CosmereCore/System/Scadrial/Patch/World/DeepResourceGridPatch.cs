using Concord;
using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Patch.World;

[Patch(typeof(DeepResourceGrid))]
public abstract class DeepResourceGridPatch {
    [InjectField("map")]
    private readonly Map gridMap = null!;

    [Inject(At.Return, nameof(DeepResourceGrid.AnyActiveDeepScannersOnMap))]
    private void AfterAnyActiveDeepScannersOnMap(ControlHandle<bool> ch) {
        if (ch.ReturnValue) return;
        if (gridMap == null) return;

        ch.ReturnValue = AllomancyUtility.HasActiveBronzeSeeker(gridMap);
    }
}
