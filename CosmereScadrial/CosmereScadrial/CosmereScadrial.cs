using CosmereCore.Tabs;
using CosmereScadrial.Inspector;
using RimWorld;
using Verse;

namespace CosmereScadrial {
    public class CosmereScadrial : Mod {
        public CosmereScadrial(ModContentPack content) : base(content) {
            InvestitureTabRegistry.Register(InvestitureTab_Scadrial.Draw);
            DefOfHelper.EnsureInitializedInCtor(typeof(CosmereJobDefOf));
        }
    }
}