using Cosmere.Core.Quest;
using Cosmere.Core.Quest.Reward;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Extension;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using Verse;

namespace Cosmere.System.Roshar.Quest;

/// <summary>
///     Bonds the subject to a Radiant order. Naming a godspren makes it a Bondsmith bond, the
///     branch Dialog_ChooseRadiantOrder used to own before the capstone took it over.
/// </summary>
public class GrantOrderReward : QuestReward {
    public string? godspren;
    public string? order;

    public override void Give(QuestBuildContext ctx) {
        Pawn? pawn = ctx.subject;
        if (pawn?.genes == null) {
            throw new QuestBuildFailure("GrantOrderReward requires a pawn-scoped quest.");
        }

        RadiantOrderDef? orderDef = DefDatabase<RadiantOrderDef>.GetNamedSilentFail(order);
        if (orderDef == null) {
            throw new QuestBuildFailure($"GrantOrderReward names an unknown order '{order}'.");
        }

        bool isGodspren = !string.IsNullOrEmpty(godspren);
        Surgebinder? surgebinder = pawn.genes.TryAddRadiantOrder(
            orderDef.GetSurgebindingGene(),
            sprenName: isGodspren ? godspren : null,
            showNamingDialog: !isGodspren
        );

        if (surgebinder == null || !isGodspren) return;

        surgebinder.godsprenName = godspren!;
        ClearCalling(pawn);

        Current.Game?.GetComponent<RadiantTracker>()?.RegisterBondsmith();
        Current.Game?.GetComponent<BondsmithCallingChecker>()?.RecordBondedGodspren(godspren!);
    }

    public override string Describe() {
        return "CRO_Quest_Reward_RadiantBond".Translate().Resolve();
    }

    public override string? ConfigError() {
        return string.IsNullOrEmpty(order) ? "GrantOrderReward has no order." : null;
    }

    /// <summary>The calling is answered once the bond lands, so it stops pulling the pawn east.</summary>
    private static void ClearCalling(Pawn pawn) {
        List<Verse.Hediff> hediffs = pawn.health?.hediffSet?.hediffs ?? [];
        for (int i = 0; i < hediffs.Count; i++) {
            if (hediffs[i] is not BondsmithCalling calling) continue;

            pawn.health!.RemoveHediff(calling);
            return;
        }
    }
}
