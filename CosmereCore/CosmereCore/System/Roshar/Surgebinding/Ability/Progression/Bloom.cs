using System;
using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using Cosmere.System.Roshar.Surgebinding.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Progression;

public class Bloom : SurgebindingAbility {
    private const int BaseRadius = 4;
    private const int PlantGrowthIntervalTicks = 60;
    private static readonly ThingDef? AuraMoteDef = ThingDefOf.Cosmere_Roshar_Thing_BloomAura;
    private readonly List<Pawn> pawnsInArea = [];
    private Mote? auraMote;

    public Bloom(Pawn pawn) : base(pawn) { }
    public Bloom(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private float radius => BaseRadius + gene.currentIdeal;

    private HediffDef? hediffToApply => def.hediff;

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + gene.currentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        if (hediffToApply != null) {
            SurgebindingHediffUtility.GetOrAddHediff(pawn, this, hediffToApply);
        }

        if (AuraMoteDef != null) {
            float moteScale = Cosmere.Core.Util.MoteUtility.GetMoteSize(AuraMoteDef, BaseRadius, GetStrength());
            auraMote = MoteMaker.MakeAttachedOverlay(pawn, AuraMoteDef, Vector3.zero, moteScale);
        }
    }

    protected override void OnDisable() {
        base.OnDisable();

        if (auraMote != null && !auraMote.Destroyed) {
            auraMote.Destroy();
        }

        auraMote = null;

        for (int i = pawnsInArea.Count - 1; i >= 0; i--) {
            Pawn targetPawn = pawnsInArea[i];
            if (targetPawn != null && !targetPawn.Dead && hediffToApply != null) {
                SurgebindingHediffUtility.RemoveHediff(targetPawn, this, hediffToApply);
            }
        }

        pawnsInArea.Clear();
    }

    public override void AbilityTick() {
        base.AbilityTick();
        if (!status.isActive) return;

        auraMote?.Maintain();
        if (auraMote != null && AuraMoteDef != null) {
            float moteScale = Cosmere.Core.Util.MoteUtility.GetMoteSize(AuraMoteDef, BaseRadius, GetStrength());
            auraMote.Graphic.drawSize = new Vector2(moteScale, moteScale);
        }

        float currentRadius = radius;

        if (gene.currentIdeal >= 1) {
            UpdateAllyHediffs(currentRadius);
        }

        if (pawn.IsHashIntervalTick(PlantGrowthIntervalTicks)) {
            GrowNearbyPlants(currentRadius);
        }
    }

    private void UpdateAllyHediffs(float currentRadius) {
        for (int i = pawnsInArea.Count - 1; i >= 0; i--) {
            Pawn targetPawn = pawnsInArea[i];
            if (targetPawn == null || targetPawn.Dead ||
                !targetPawn.Position.InHorDistOf(pawn.Position, currentRadius)) {
                if (targetPawn != null && !targetPawn.Dead && hediffToApply != null) {
                    SurgebindingHediffUtility.RemoveHediff(targetPawn, this, hediffToApply);
                }

                pawnsInArea.RemoveAt(i);
            }
        }

        if (!pawn.IsHashIntervalTick(30)) return;

        foreach (Verse.Thing thing in GenRadial.RadialDistinctThingsAround(
                     pawn.Position, pawn.Map, currentRadius, true
                 )) {
            if (thing is not Pawn targetPawn) continue;
            if (targetPawn.Dead) continue;
            if (targetPawn.Faction != pawn.Faction) continue;
            if (hediffToApply == null) continue;

            SurgebindingHediffUtility.GetOrAddHediff(targetPawn, this, hediffToApply);
            pawnsInArea.AddDistinct(targetPawn);
        }
    }

    private void GrowNearbyPlants(float currentRadius) {
        float growthAmount = 0.15f + gene.currentIdeal * 0.05f;

        foreach (Verse.Thing thing in GenRadial.RadialDistinctThingsAround(
                     pawn.Position, pawn.Map, currentRadius, true
                 )) {
            if (thing is not Plant plant) continue;

            plant.Growth = Math.Min(1f, plant.Growth + growthAmount);
        }

        if (gene.currentIdeal >= 4) {
            SpawnNewPlants(currentRadius);
        }
    }

    private void SpawnNewPlants(float currentRadius) {
        int plantsSpawned = 0;
        int maxPlantsPerTick = 2;

        foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.Position, currentRadius, true)) {
            if (plantsSpawned >= maxPlantsPerTick) break;
            if (!cell.InBounds(pawn.Map)) continue;
            if (cell.GetTerrain(pawn.Map).fertility <= 0f) continue;
            if (cell.GetPlant(pawn.Map) != null) continue;
            if (!cell.Standable(pawn.Map)) continue;

            ThingDef plantDef = RimWorld.ThingDefOf.Plant_Grass;
            Plant newPlant = (Plant)ThingMaker.MakeThing(plantDef);
            newPlant.Growth = 0.5f;
            GenSpawn.Spawn(newPlant, cell, pawn.Map);
            plantsSpawned++;
        }
    }
}
