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

    // A bubble is nailed to the ground because holding one still is the hard part.
    // Someone who has burned this metal for thirty days no longer has to think about
    // it, and carries the bubble with them.
    private const int SavantStageForMobileBubble = 3;

    private readonly List<Pawn> pawnsInBubble = [];
    private Mote? bubble;
    private Mote? bubbleWithDistortion;
    private IntVec3 centre = IntVec3.Invalid;

    public TimeAbility(Pawn pawn) : base(pawn) { }

    public TimeAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private ThingDef moteDef => metal.Equals(MetallicArtsMetalDefOf.Cadmium)
        ? ThingDefOf.Cosmere_Scadrial_Thing_TimeBubbleCadmium
        : ThingDefOf.Cosmere_Scadrial_Thing_TimeBubbleBendalloy;

    private ThingDef warpMoteDef { get; } = ThingDefOf.Cosmere_Scadrial_Thing_TimeBubbleWarp;

    private float moteScale => MoteUtility.GetMoteSize(moteDef, BaseRadius, GetStrength());

    private HediffDef hediffToApply => metal == MetallicArtsMetalDefOf.Cadmium
        ? HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleCadmium
        : HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleBendalloy;

    public bool AnchorsToSelf =>
        ScadrialSavantUtility.GetAllomanticSavantStage(pawn, metal) >= SavantStageForMobileBubble;

    protected override void OnEnable() {
        base.OnEnable();

        centre = pawn.Position;
        bubble = MoteMaker.MakeStaticMote(centre, pawn.MapHeld, moteDef, moteScale);
        bubbleWithDistortion = MoteMaker.MakeStaticMote(centre, pawn.MapHeld, warpMoteDef, moteScale * 0.8f);

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
        if (!atLeastBurning) return;

        bubble?.Maintain();
        bubbleWithDistortion?.Maintain();
        if (bubble == null || bubbleWithDistortion == null) return;

        // The mote keeps the cell it spawned in; only what it draws at moves. Every
        // radius test below reads `centre`, so the mote's own cell never matters.
        if (AnchorsToSelf && pawn.Spawned) {
            centre = pawn.Position;
            bubble.exactPosition = pawn.DrawPos;
            bubbleWithDistortion.exactPosition = pawn.DrawPos;
        }

        bubble.Scale = moteScale;
        bubbleWithDistortion.Scale = moteScale * 0.8f;

        float radius = BaseRadius * GetStrength();

        if (debugMode) {
            GenDraw.DrawCircleOutline(bubble.DrawPos, radius, metal.solidLineColor);
        }

        for (int i = pawnsInBubble.Count - 1; i >= 0; i--) {
            Pawn pawnInBubble = pawnsInBubble[i];
            if (!pawnInBubble.Position.InHorDistOf(centre, radius)) {
                pawnInBubble.RemoveHediff(this, hediffToApply);
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
            targetPawn.GetOrAddHediff(this, hediffToApply);
            pawnsInBubble.AddDistinct(targetPawn);
        }
    }
}
