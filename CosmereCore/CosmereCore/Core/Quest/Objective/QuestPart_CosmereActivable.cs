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
    /// <summary>How often to evaluate. 60 ticks is one in-game second.</summary>
    public int checkIntervalTicks = 60;

    /// <summary>Raised instead of completing when the objective becomes impossible.</summary>
    public string? failSignal;

    public override void QuestPartTick() {
        base.QuestPartTick();
        if (State != QuestPartState.Enabled) return;

        // int.IsHashIntervalTick(int) does not exist - RimWorld only defines the
        // Map/Thing/WorldObject/Faction overloads. A non-positive interval means "check every
        // tick" instead of throwing on a modulo by zero.
        if (checkIntervalTicks > 0 && Find.TickManager.TicksGame % checkIntervalTicks != 0) return;

        if (IsSatisfied()) OnSatisfied();
    }

    /// <summary>True once this objective is done. Called on the check interval, not every tick.</summary>
    protected abstract bool IsSatisfied();

    protected virtual void OnSatisfied() {
        Complete();
    }

    /// <summary>Raise the fail signal and stop polling.</summary>
    protected void Fail() {
        string? signal = failSignal;
        if (signal != null && signal.Length > 0) Find.SignalManager.SendSignal(new Signal(signal));

        Disable();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref checkIntervalTicks, "checkIntervalTicks", 60);
        Scribe_Values.Look(ref failSignal, "failSignal");
    }
}
