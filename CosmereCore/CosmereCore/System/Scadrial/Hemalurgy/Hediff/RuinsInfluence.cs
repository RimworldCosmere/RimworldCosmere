using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.Hediff;

/// <summary>
///     How far into somebody Ruin has got, counted in spikes.
/// </summary>
/// <remarks>
///     Under four spikes it only whispers, which is a mood penalty and nothing more. At four it
///     can move a person, and that arrives as a compulsion the colony is told about before it
///     happens. The warning is the whole design: a spiked pawn should be a liability you can plan
///     around rather than a slot machine that occasionally eats an afternoon.
/// </remarks>
public class RuinsInfluence : HediffWithComps {
    /// <summary>Fewest spikes at which Ruin can move somebody rather than just talk to them.</summary>
    public const int ControlThreshold = 4;

    private int spikeCount;

    /// <summary>What Ruin has settled on, and the tick it acts. Null when nothing is coming.</summary>
    private string? pendingKey;
    private int pendingTick = -1;

    public bool Compelled => pendingTick > 0;

    public void UpdateSpikeCount(int count) {
        spikeCount = count;
        Severity = count * 0.2f;

        // Dropping below the threshold calls off whatever was coming. Pulling a spike in time is
        // meant to be an answer.
        if (spikeCount < ControlThreshold) Clear();
    }

    public override void Tick() {
        base.Tick();

        if (pendingTick > 0) {
            if (Find.TickManager.TicksGame < pendingTick) return;

            Act();
            return;
        }

        if (!pawn.IsHashIntervalTick(2500)) return;
        if (spikeCount < ControlThreshold) return;
        if (!RuinCompulsions.CanBeMoved(pawn)) return;

        // Rises with every spike past the threshold.
        if (!Rand.Chance((spikeCount - (ControlThreshold - 1)) * 0.02f)) return;

        Decide();
    }

    private void Decide() {
        RuinCompulsion? compulsion = RuinCompulsions.Choose(spikeCount);
        if (compulsion == null) return;

        pendingKey = compulsion.key;
        pendingTick = Find.TickManager.TicksGame + RuinCompulsions.WarningTicks;

        RuinCompulsions.Warn(pawn, compulsion);
    }

    private void Act() {
        string? key = pendingKey;
        Clear();

        if (key == null) return;

        // Anybody stopped in the meantime is no use to it. Arresting, downing or drugging the
        // pawn is the intended answer to the warning.
        if (!RuinCompulsions.CanBeMoved(pawn)) return;

        RuinCompulsion? compulsion = RuinCompulsions.All.Find(c => c.key == key);
        if (compulsion == null) return;

        RuinCompulsions.Fire(pawn, compulsion);
    }

    private void Clear() {
        pendingKey = null;
        pendingTick = -1;
    }

    public override string DebugString() {
        return base.DebugString()
               + $"\nspikes: {spikeCount}"
               + (Compelled ? $"\ncompelled: {pendingKey} at tick {pendingTick}" : "\ncompelled: no");
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref spikeCount, "spikeCount");
        Scribe_Values.Look(ref pendingKey, "pendingKey");
        Scribe_Values.Look(ref pendingTick, "pendingTick", -1);
    }
}
