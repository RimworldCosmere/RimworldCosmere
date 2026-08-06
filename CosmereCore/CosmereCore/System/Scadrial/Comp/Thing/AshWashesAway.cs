using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Thing;

public class AshWashesAwayProperties : CompProperties {
    /// <summary>Rain heavier than this carries ash off. Matches the vanilla filth threshold.</summary>
    public float rainRateThreshold = 0.4f;

    /// <summary>Stack lost per rare tick of exposed rain.</summary>
    public int washPerRareTick = 2;

    public AshWashesAwayProperties() {
        compClass = typeof(AshWashesAway);
    }
}

/// <summary>
///     Ash swept into a pile is still ash. Left outside in the rain it goes the way the filth it
///     came from would have, rather than sitting in a stockpile forever.
/// </summary>
public class AshWashesAway : ThingComp {
    private AshWashesAwayProperties Props => (AshWashesAwayProperties)props;

    public override void CompTickRare() {
        Verse.Map? map = parent.MapHeld;
        if (map == null) return;
        if (map.weatherManager.RainRate < Props.rainRateThreshold) return;
        if (parent.PositionHeld.Roofed(map)) return;

        int washed = Mathf.Min(Props.washPerRareTick, parent.stackCount);
        parent.stackCount -= washed;
        if (parent.stackCount <= 0) parent.Destroy();
    }
}
