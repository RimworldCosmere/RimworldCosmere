using Cosmere.Core.Nightwatcher;
using Cosmere.System.Roshar.Util;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Nightwatcher;

public class LighteyesApplicator : IBoonApplicator {
    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        if (!CasteUtility.IsDarkeyes(pawn)) return;
        CasteUtility.DarkeyesToLighteyes(pawn, "Cosmere_Roshar_Gene_Dahn_Low");
        Logger.Info($"LighteyesApplicator: transitioned {pawn.NameShortColored} to lighteyes");
    }
}
