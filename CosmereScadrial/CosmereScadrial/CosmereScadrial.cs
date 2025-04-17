using CosmereCore.Tabs;
using CosmereScadrial.Inspector;
using Verse;

namespace CosmereScadrial {
    public class CosmereScadrial : Mod {
        public CosmereScadrial(ModContentPack content) : base(content) {
            InvestitureTabRegistry.Register(InvestitureTabScadrial.Draw);
        }
    }
}