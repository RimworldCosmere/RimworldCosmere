using System;
using Concord;
using Cosmere.Core.Thing;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class PermanentFlightForSplinterPatch : Pawn_FlightTracker {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    [InjectField("flyingTicks")]
    private int flyingTicks;

    /// <summary>
    ///     Pawn_FlightTracker.FlightState is a private nested enum, so its type can't be named
    ///     here. object is Concord's escape hatch: boxed on read, unboxed on write, against the real field.
    /// </summary>
    [InjectField("flightState")]
    private object flightState = null!;

    protected PermanentFlightForSplinterPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, nameof(FlightTick))]
    private void AfterFlightTick() {
        if (trackedPawn is Splinter) {
            flyingTicks = 0;

            // FlightState.Flying
            flightState = Enum.ToObject(flightState.GetType(), 1);
        }
    }

    [Inject(At.Head, nameof(ForceLand))]
    private Control BeforeForceLand() {
        return trackedPawn is Splinter ? Control.Cancel : Control.Continue;
    }
}
