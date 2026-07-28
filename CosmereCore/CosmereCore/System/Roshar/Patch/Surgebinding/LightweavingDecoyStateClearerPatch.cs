using Concord;
using Cosmere.System.Roshar.Surgebinding.Ability.Illumination;
using Verse.Profile;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[Patch(typeof(MemoryUtility))]
public static class LightweavingDecoyStateClearerPatch {
    [Inject(At.Return, nameof(MemoryUtility.ClearAllMapsAndWorld))]
    private static void AfterClearAllMapsAndWorld() {
        LightweavingDecoyRegistry.Clear();
    }
}
