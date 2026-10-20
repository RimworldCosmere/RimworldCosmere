using Concord;
using Cosmere.System.Scadrial.Threat;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.World;

/// <summary>
///     A coppercloud hides the colony from whoever is sizing it up, so it shrinks the whole threat
///     value. What each invested pawn is worth is priced per colonist in Core, not here.
/// </summary>
[Patch(typeof(StorytellerUtility))]
public static class ScadrialStorytellerUtilityPatch {
    [Inject(
        At.Return,
        nameof(StorytellerUtility.DefaultThreatPointsNow),
        parameterTypes: [typeof(IIncidentTarget)]
    )]
    private static void AfterDefaultThreatPointsNow(IIncidentTarget target, ControlHandle<float> ch) {
        if (target is not Map map) return;

        float hidden = AllomancyUtility.GetCoppercloudStrength(map);
        if (hidden <= 0f) return;

        ch.ReturnValue = ScadrialThreat.Coppercloud(ch.ReturnValue, hidden);
    }
}
