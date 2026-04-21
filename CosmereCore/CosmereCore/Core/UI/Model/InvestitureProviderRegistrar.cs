using Cosmere.System.Nalthis.UI;
using Cosmere.System.Roshar.UI;
using Cosmere.System.Scadrial.UI;
using Verse;

namespace Cosmere.Core.UI.Model;

[StaticConstructorOnStartup]
public static class InvestitureProviderRegistrar {
    static InvestitureProviderRegistrar() {
        PawnInvestitureProviders.Register(new AllomancyInvestitureProvider());
        PawnInvestitureProviders.Register(new FeruchemyInvestitureProvider());
        PawnInvestitureProviders.Register(new SurgebindingInvestitureProvider());
        PawnInvestitureProviders.Register(new AwakeningInvestitureProvider());
    }
}
