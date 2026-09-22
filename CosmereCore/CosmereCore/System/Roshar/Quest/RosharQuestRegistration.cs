using Cosmere.Core.Quest;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Quest;

[StaticConstructorOnStartup]
public static class RosharQuestRegistration {
    static RosharQuestRegistration() {
        QuestComponentRegistry.RegisterBondedOrderProbe(ProbeSurgebinder);
    }

    private static KeyValuePair<string, int>? ProbeSurgebinder(Pawn pawn) {
        Surgebinder? gene = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        RadiantOrderDef? order = gene?.radiantOrderDef;
        if (order == null) return null;

        // Display, not CurrentIdeal: a first-ideal Radiant is 0, and minIdeal cannot go below 1.
        return new KeyValuePair<string, int>(order.defName, gene!.CurrentIdealDisplay);
    }
}
