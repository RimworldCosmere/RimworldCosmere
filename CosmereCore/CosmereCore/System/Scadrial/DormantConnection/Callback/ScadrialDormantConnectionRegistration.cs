using Cosmere.Core.DefModExtension;
using Verse;

namespace Cosmere.System.Scadrial.DormantConnection.Callback;

[StaticConstructorOnStartup]
internal static class ScadrialDormantConnectionRegistration {
    static ScadrialDormantConnectionRegistration() {
        DormantConnectionCallbackRegistry.Register(new Snapped());
    }
}
