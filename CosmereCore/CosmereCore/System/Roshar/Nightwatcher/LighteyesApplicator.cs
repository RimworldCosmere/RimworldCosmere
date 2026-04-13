using System.Collections.Generic;
using Cosmere.System.Roshar.Def;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Nightwatcher;

public class LighteyesApplicator : IBoonApplicator {
    public void Apply(Pawn pawn, NightwatcherBoonDef def, Dictionary<string, object>? context = null) {
        if (!Utility.CasteUtility.IsDarkeyes(pawn)) return;
        Utility.CasteUtility.DarkeyesToLighteyes(pawn, "Cosmere_Roshar_Gene_Dahn_Low");
        Logger.Info($"LighteyesApplicator: transitioned {pawn.NameShortColored} to lighteyes");
    }
}
