using Cosmere.Core.Ability;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Savant;
using RimWorld;
using UnityEngine;
using Verse;
using static Cosmere.Core.Mod;

namespace Cosmere.System.Scadrial.Allomancy.Ability;

public class TimeAbility : AllomancyAbility {
    private const int BaseRadius = 3;

    /// <summary>
    ///     A bubble stays nailed to the ground - holding one still is the hard part. At this
    ///     savant stage, 30 days of burning makes it second nature, so the bubble moves with the pawn.
    /// </summary>
    private const int SavantStageForMobileBubble = 3;

    private readonly List<Pawn> pawnsInBubble = [];
    private Mote? bubble;
    private Mote? bubbleWithDistortion;
    private IntVec3 centre = IntVec3.Invalid;

    public TimeAbility(Pawn pawn) : base(pawn) { }

    public TimeAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private ThingDef MoteDef => Metal.Equals(MetallicArtsMetalDefOf.Cadmium)
        ? ThingDefOf.Cosmere_Scadrial_Thing_TimeBubbleCadmium
        : ThingDefOf.Cosmere_Scadrial_Thing_TimeBubbleBendalloy;

    private ThingDef WarpMoteDef { get; } = ThingDefOf.Cosmere_Scadrial_Thing_TimeBubbleWarp;

    private float MoteScale => MoteUtility.GetMoteSize(MoteDef, BaseRadius, GetStrength());

    private HediffDef HediffToApply => Metal == MetallicArtsMetalDefOf.Cadmium
        ? HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleCadmium
        : HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleBendalloy;

    public bool AnchorsToSelf =>
        ScadrialSavantUtility.GetAllomanticSavantStage(pawn, Metal) >= SavantStageForMobileBubble;

    protected override void OnEnable() {
        base.OnEnable();

        centre = pawn.Position;
        bubble = MoteMaker.MakeStaticMote(centre, pawn.MapHeld, MoteDef, MoteScale);
        bubbleWithDistortion = MoteMaker.MakeStaticMote(centre, pawn.MapHeld, WarpMoteDef, MoteScale * 0.8f);

        bubble.Maintain();
        bubbleWithDistortion.Maintain();
    }

    protected override void OnDisable() {
        base.OnDisable();
        centre = IntVec3.Invalid;
        if (bubble != null && !bubble.Destroyed) bubble.Destroy();
        if (bubbleWithDistortion != null && !bubbleWithDistortion.Destroyed) bubbleWithDistortion.Destroy();
    }

    public override void AbilityTick() {
        base.AbilityTick();
        if (!AtLeastBurning) return;

        bubble?.Maintain();
        bubbleWithDistortion?.Maintain();
        if (bubble == null || bubbleWithDistortion == null) return;

        // mote keeps the cell it spawned in - only its draw position moves. radius tests below read `centre`.
        if (AnchorsToSelf && pawn.Spawned) {
            centre = pawn.Position;
            bubble.exactPosition = pawn.DrawPos;
            bubbleWithDistortion.exactPosition = pawn.DrawPos;
        }

        bubble.Scale = MoteScale;
        bubbleWithDistortion.Scale = MoteScale * 0.8f;

        float radius = BaseRadius * GetStrength();

        if (DebugMode) {
            GenDraw.DrawCircleOutline(bubble.DrawPos, radius, Metal.SolidLineColor);
        }

        for (int i = pawnsInBubble.Count - 1; i >= 0; i--) {
            Pawn pawnInBubble = pawnsInBubble[i];
            if (!pawnInBubble.Position.InHorDistOf(centre, radius)) {
                pawnInBubble.RemoveHediff(this, HediffToApply);
            }
        }

        if (!pawn.Spawned || pawn.MapHeld != bubble.Map) {
            UpdateStatus(Active.Off);
            return;
        }

        // Walking out of your own bubble ends it - unless you are the one carrying it.
        if (!AnchorsToSelf && !pawn.Position.InHorDistOf(centre, radius)) {
            UpdateStatus(Active.Off);
            return;
        }

        if (!pawn.IsHashIntervalTick(30)) return;

        foreach (Pawn? targetPawn in GenRadial.RadialDistinctThingsAround(centre, bubble.Map, radius, true)
                     .OfType<Pawn>()) {
            targetPawn.GetOrAddHediff(this, HediffToApply);
            pawnsInBubble.AddDistinct(targetPawn);
        }
    }
}
