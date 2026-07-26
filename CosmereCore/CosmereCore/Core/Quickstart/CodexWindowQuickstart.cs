using Cosmere.Core.Tab;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quickstart;

public class CodexWindowQuickstart : CosmereQuickstart {
    public override TaggedString description =>
        "Cosmere All-Stars with the Investiture codex tab opened for verification";

    public override void PostLoaded() {
        base.PostLoaded();
        InspectPaneUtility.OpenTab(typeof(ITab_Investiture));
    }
}
