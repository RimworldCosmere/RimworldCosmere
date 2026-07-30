using System.Collections.Generic;
using Cosmere.Core.UI;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Presents a branch choice when its stage begins, then completes with the chosen key as
///     a signal argument so later parts can read it. Fires once on Enable rather than
///     polling, so it extends QuestPartActivable directly rather than the polling base.
/// </summary>
public class QuestPart_CosmereChoice : QuestPartActivable {
    public string? chosenKey;
    public List<QuestChoiceOption>? options;

    protected override void Enable(SignalArgs receivedArgs) {
        base.Enable(receivedArgs);
        Find.WindowStack.Add(new Dialog_QuestChoice(this));
    }

    /// <summary>
    ///     Called by the dialog once the player picks an option. Applies the cost, then
    ///     completes. Returns false without completing if the option costs silver the player
    ///     cannot pay - the dialog stays open so the player can choose differently.
    /// </summary>
    public bool Choose(QuestChoiceOption option) {
        if (option.silverCost > 0 && !QuestSilver.TryCharge(option.silverCost)) {
            Messages.Message("CC_Quest_Choice_InsufficientSilver".Translate(), MessageTypeDefOf.RejectInput, false);
            return false;
        }

        chosenKey = option.key;

        if (option.goodwillOnTarget != 0 && quest != null) ApplyGoodwill(option.goodwillOnTarget);

        Complete(chosenKey.Named("CHOICE"));
        return true;
    }

    private void ApplyGoodwill(int delta) {
        foreach (Faction faction in quest.InvolvedFactions) {
            if (faction == null || faction.IsPlayer) continue;
            faction.TryAffectGoodwillWith(Faction.OfPlayer, delta, true, true);
            return;
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref chosenKey, "chosenKey");
        Scribe_Collections.Look(ref options, "options", LookMode.Deep);
    }
}
