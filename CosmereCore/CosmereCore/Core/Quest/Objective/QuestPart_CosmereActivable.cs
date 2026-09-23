using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Base for Cosmere quest parts that poll for a condition rather than waiting on a
///     signal. Vanilla's QuestPart_Filter family is signal-driven because it is built for
///     QuestNode trees; CosmereQuestBuilder assembles parts directly and needs pollers.
///     Modelled on RimWorld.QuestPart_Delay, which is the vanilla ticking reference.
/// </summary>
public abstract class QuestPart_CosmereActivable : QuestPartActivable {
    /// <summary>
    ///     Restricts this part to one branch of a preceding ChoiceObjective. On any other
    ///     branch it passes straight through on its first poll, so the stage after it starts
    ///     without the player having to satisfy an objective their choice bypassed.
    /// </summary>
    public string? afterChoice;

    /// <summary>How often to evaluate. 60 ticks is one in-game second.</summary>
    public int checkIntervalTicks = 60;

    /// <summary>Raised instead of completing when the objective becomes impossible.</summary>
    public string? failSignal;

    public override void QuestPartTick() {
        base.QuestPartTick();
        if (State != QuestPartState.Enabled) return;

        // Checked on first poll, not Enable: completing there would re-enter the signal manager mid-walk.
        if (!QuestBranch.Matches(afterChoice, quest)) {
            Log.Debug($"{GetType().Name}: skipped, quest took a branch other than '{afterChoice}'.");
            OnSkipped();
            Complete();
            return;
        }

        // int has no IsHashIntervalTick overload - RimWorld defines it only on Map/Thing/WorldObject/Faction.
        if (checkIntervalTicks > 0 && Find.TickManager.TicksGame % checkIntervalTicks != 0) return;

        if (IsSatisfied()) OnSatisfied();
    }

    /// <summary>Tidy-up for a part the player's branch bypassed. Runs before it completes.</summary>
    protected virtual void OnSkipped() { }

    /// <summary>True once this objective is done. Called on the check interval, not every tick.</summary>
    protected abstract bool IsSatisfied();

    protected virtual void OnSatisfied() {
        Complete();
    }

    /// <summary>Raise the fail signal and stop polling.</summary>
    protected void Fail() {
        Log.Warn($"{GetType().Name} failed the quest (state was {State}).");

        string? signal = failSignal;
        if (signal != null && signal.Length > 0) Find.SignalManager.SendSignal(new Signal(signal));

        Disable();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref afterChoice, "afterChoice");
        Scribe_Values.Look(ref checkIntervalTicks, "checkIntervalTicks", 60);
        Scribe_Values.Look(ref failSignal, "failSignal");
    }
}
