using Cosmere.Core.Nightwatcher;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Nightwatcher;

public class HealChronicApplicator : IBoonApplicator, INightwatcherEffectDescriber {
    public string? DescribeEffects(NightwatcherApplicationContext? context = null) {
        return "Heals one chronic condition";
    }

    private static readonly HashSet<string> ChronicHediffs = [
        "ChronicPain", "Asthma", "BadBack", "Carcinoma",
        "Frail", "HearingLoss", "Cataract",
        "Cosmere_Roshar_Hediff_NW_CursePassive_ChronicPain",
    ];

    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        List<Verse.Hediff> hediffs = pawn.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++) {
            if (!ChronicHediffs.Contains(hediffs[i].def.defName)) continue;
            pawn.health.RemoveHediff(hediffs[i]);
            Logger.Info($"HealChronicApplicator: removed {hediffs[i].def.defName} from {pawn.NameShortColored}");
            return;
        }
    }
}
