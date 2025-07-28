using Cosmere.Core.Ability;
using Cosmere.Scadrial.Utility;
using RimWorld;
using Verse;
using static Cosmere.Framework.Mod;

namespace Cosmere.Scadrial.Allomancy.Ability;

public class TimeAbility(Pawn pawn, AbilityDef def) : AllomancyAbility(pawn, def) {
    private const int BaseRadius = 3;
    private readonly List<Pawn> pawnsInBubble = [];
    private Mote? bubble;
    private Mote? bubbleWithDistortion;

    private ThingDef moteDef => metal.Equals(MetallicArtsMetalDefOf.Cadmium)
        ? ThingDefOf.Cosmere_Scadrial_Thing_TimeBubbleCadmium
        : ThingDefOf.Cosmere_Scadrial_Thing_TimeBubbleBendalloy;

    private ThingDef warpMoteDef { get; } = ThingDefOf.Cosmere_Scadrial_Thing_TimeBubbleWarp;

    private float moteScale => MoteUtility.GetMoteSize(moteDef, BaseRadius, GetStrength());

    private HediffDef hediffToApply => metal.defName switch {
        "Cadmium" => HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleCadmium,
        "Bendalloy" => HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleBendalloy,
        _ => null!,
    };

    protected override void OnEnable() {
        base.OnEnable();

        bubble = MoteMaker.MakeStaticMote(pawn.Position, pawn.MapHeld, moteDef, moteScale);
        bubbleWithDistortion = MoteMaker.MakeStaticMote(pawn.Position, pawn.MapHeld, warpMoteDef, moteScale * 0.8f);

        bubble.Maintain();
        bubbleWithDistortion.Maintain();
    }

    protected override void OnDisable() {
        base.OnDisable();
        if (!bubble?.Destroyed ?? false) bubble?.Destroy();
        if (!bubbleWithDistortion?.Destroyed ?? false) bubbleWithDistortion?.Destroy();
    }


    public override void AbilityTick() {
        base.AbilityTick();
        if (!atLeastBurning) return;

        bubble?.Maintain();
        bubbleWithDistortion?.Maintain();
        if (bubble == null || bubbleWithDistortion == null) return;

        bubble.Scale = moteScale;
        bubbleWithDistortion.Scale = moteScale * 0.8f;


        float radius = BaseRadius * GetStrength();

        if (debugMode) {
            GenDraw.DrawCircleOutline(bubble.DrawPos, radius, metal.solidLineColor);
        }

        foreach (Pawn? pawnInBubble in pawnsInBubble.Where(otherPawn =>
                     !otherPawn.Position.InHorDistOf(bubble.Position, radius)
                 )) {
            pawnInBubble.RemoveHediff(this, hediffToApply);
        }

        if (!pawn.Position.InHorDistOf(bubble.Position, radius)) {
            UpdateStatus(Active.Off);
            return;
        }

        if (!pawn.IsHashIntervalTick(30)) return;

        foreach (Pawn? targetPawn in GenRadial.RadialDistinctThingsAround(bubble.Position, bubble.Map, radius, true)
                     .OfType<Pawn>()) {
            targetPawn.GetOrAddHediff(this, hediffToApply);
            pawnsInBubble.AddDistinct(targetPawn);
        }
    }
}