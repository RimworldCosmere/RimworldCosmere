using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Render;
using LudeonTK;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Dev;

/// <summary>
///     Dev-only live tuner for ashfall. Severity and the emission curve are the two numbers that
///     have to be judged by eye, and rebuilding to change a constant is far too slow a loop.
/// </summary>
public class Dialog_AshTuning : Window {
    public Dialog_AshTuning() {
        doCloseX = true;
        draggable = true;
        preventCameraMotion = false;
        closeOnClickedOutside = false;
        absorbInputAroundWindow = false;
    }

    public override Vector2 InitialSize => new Vector2(420f, 260f);

    public override void DoWindowContents(Rect inRect) {
        AshDepthTracker? tracker = Find.CurrentMap?.GetComponent<AshDepthTracker>();
        if (tracker == null) {
            Widgets.Label(inRect, "No ash tracker on this map.");
            return;
        }

        Listing_Standard list = new Listing_Standard();
        list.Begin(inRect);

        float severity = tracker.Severity;
        list.Label($"Severity: {severity:F2}");
        float newSeverity = list.Slider(severity, 0f, 1f);
        if (!Mathf.Approximately(newSeverity, severity)) tracker.SetSeverityNow(newSeverity);

        list.Label($"Falling at: {tracker.EffectiveSeverity:F2} after sealed vents");

        list.Gap(6f);

        list.Label($"Rate at severity 0: {AshParticles.MinEmissionRate:F0}/s");
        AshParticles.MinEmissionRate = list.Slider(AshParticles.MinEmissionRate, 0f, 200f);

        list.Label($"Rate at severity 1: {AshParticles.MaxEmissionRate:F0}/s");
        AshParticles.MaxEmissionRate = list.Slider(AshParticles.MaxEmissionRate, 50f, 2000f);

        list.Label($"Curve exponent: {AshParticles.EmissionExponent:F2}  (1 = linear, higher = sparser low end)");
        AshParticles.EmissionExponent = list.Slider(AshParticles.EmissionExponent, 0.5f, 3f);

        list.Gap(8f);
        AshParticles? veil = tracker.Veil;
        list.Label(
            veil == null
                ? "Veil: not running"
                : $"Live particles: {veil.LiveCount}    emitting {veil.LastRate:F0}/s"
        );

        list.End();
    }
}

public static class AshTuningDebugActions {
    [DebugAction("Cosmere", "Ash: tuning window", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void OpenTuner() {
        Find.WindowStack.Add(new Dialog_AshTuning());
    }
}
