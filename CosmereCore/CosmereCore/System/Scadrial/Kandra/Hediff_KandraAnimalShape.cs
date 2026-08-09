using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     Makes a borrowed body move, eat and bite like the animal it copies.
/// </summary>
/// <remarks>
///     The generated shape races got all of this free from <c>statBases</c> and <c>tools</c> on
///     their own ThingDef. With one pawn there is no second def, so the numbers have to come from
///     somewhere else, and a hediff is the only per-pawn thing that reaches stats and capacities.
///     <para>
///         The stage is built at runtime from the worn animal rather than written in XML, because
///         there are 117 animals and one hediff. <see cref="HediffStage" /> has no
///         <c>IExposable</c>, so nothing about it is saved and it must be rebuilt after load - miss
///         that and a shaped kandra quietly loads with human stats and nothing appears in the log.
///     </para>
/// </remarks>
public class Hediff_KandraAnimalShape : HediffWithComps {
    private HediffStage? built;
    private PawnKindDef? builtFor;

    /// <summary>
    ///     The animal's numbers, cached per instance.
    /// </summary>
    /// <remarks>
    ///     Hit once per stat lookup, so it must never allocate on the common path. The cache is
    ///     keyed on the worn animal so changing shape rebuilds it and nothing else does.
    /// </remarks>
    public override HediffStage? CurStage {
        get {
            PawnKindDef? worn = KandraShapeGraphicUtility.WornKind(pawn);
            if (worn == null) return base.CurStage;

            if (built != null && builtFor == worn) return built;

            builtFor = worn;
            built = Build(worn);

            return built;
        }
    }

    public override void PostMake() {
        base.PostMake();
        built = null;
        builtFor = null;
    }

    /// <summary>Nothing about the stage is saved, so it is thrown away and rebuilt on demand.</summary>
    public override void ExposeData() {
        base.ExposeData();

        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            built = null;
            builtFor = null;
        }
    }

    /// <summary>
    ///     Copies the parts of an animal a hediff can reach.
    /// </summary>
    /// <remarks>
    ///     What it cannot reach: body size, health scale and the drawn footprint, all of which are
    ///     <c>ageTracker.CurLifeStage.X * RaceProps.Y</c> with no hook. A timber wolf is 0.85
    ///     against a human 1.0, so the gap is noise.
    /// </remarks>
    private HediffStage Build(PawnKindDef worn) {
        HediffStage baseline = def.stages is { Count: > 0 } ? def.stages[0] : new HediffStage();
        ThingDef animal = worn.race;

        HediffStage stage = new HediffStage {
            // The player-facing consequences of being a beast, which stay whatever the animal is.
            disabledWorkTags = baseline.disabledWorkTags,
            capMods = baseline.capMods,

            // Not optional. DynamicPawnRenderNodeSetup_Hediffs skips any hediff whose Visible is
            // false, so hiding this from the health tab also deletes the render node and the
            // pawn draws as nothing at all.
            becomeVisible = true,
            statFactors = [],
            statOffsets = [],
        };

        float speed = animal.GetStatValueAbstract(RimWorld.StatDefOf.MoveSpeed);
        float human = RimWorld.ThingDefOf.Human.GetStatValueAbstract(RimWorld.StatDefOf.MoveSpeed);
        if (speed > 0f && human > 0f) {
            stage.statFactors.Add(new StatModifier { stat = RimWorld.StatDefOf.MoveSpeed, value = speed / human });
        }

        // A wolf shrugging off a blizzard is the most visible thing about wearing one.
        Offset(stage, animal, RimWorld.StatDefOf.ComfyTemperatureMin);
        Offset(stage, animal, RimWorld.StatDefOf.ComfyTemperatureMax);

        float hunger = animal.race?.baseHungerRate ?? 1f;
        float humanHunger = RimWorld.ThingDefOf.Human.race?.baseHungerRate ?? 1f;
        if (hunger > 0f && humanHunger > 0f) stage.hungerRateFactor = hunger / humanHunger;

        return stage;
    }

    /// <summary>Carries a temperature comfort band across as a difference from the human one.</summary>
    private static void Offset(HediffStage stage, ThingDef animal, StatDef stat) {
        float theirs = animal.GetStatValueAbstract(stat);
        float ours = RimWorld.ThingDefOf.Human.GetStatValueAbstract(stat);
        if (Mathf.Approximately(theirs, ours)) return;

        stage.statOffsets.Add(new StatModifier { stat = stat, value = theirs - ours });
    }
}
