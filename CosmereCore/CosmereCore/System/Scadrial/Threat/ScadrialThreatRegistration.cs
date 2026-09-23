using Cosmere.Core.Threat;
using Verse;

namespace Cosmere.System.Scadrial.Threat;

[StaticConstructorOnStartup]
public static class ScadrialThreatRegistration {
    static ScadrialThreatRegistration() {
        ThreatContributorRegistry.Register(new ScadrialThreatContributor());
    }
}
