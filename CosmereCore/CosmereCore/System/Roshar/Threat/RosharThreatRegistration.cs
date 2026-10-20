using Cosmere.Core.Threat;
using Verse;

namespace Cosmere.System.Roshar.Threat;

[StaticConstructorOnStartup]
public static class RosharThreatRegistration {
    static RosharThreatRegistration() {
        ThreatContributorRegistry.Register(new RosharThreatContributor());
    }
}
