using Concord;
using Cosmere.Core.BetaHub;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class RootUpdatePumpPatch : Root {
    [Inject(At.Return, nameof(Update))]
    private void AfterUpdate() {
        BetaHubRequestPump.Pump();
    }
}
