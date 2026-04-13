using System;
using Cosmere.Core.Ability;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Profile;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Tension;

public class Harden : SurgebindingAbility {
    public static readonly Dictionary<Building, float> HardenedBuildings = new();

    private List<Building> hardenedStructures = [];

    public Harden(Pawn pawn) : base(pawn) { }
    public Harden(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private int maxStructures => gene.currentIdeal switch {
        >= 4 => 5,
        3 => 3,
        2 => 2,
        _ => 1,
    };

    private float hpMultiplier => 1f + gene.currentIdeal * 0.5f;

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + gene.currentIdeal * 0.5f);
    }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        if (target.Thing is not Building building) return false;

        if (hardenedStructures.Contains(building)) {
            RemoveHardening(building);
            if (hardenedStructures.Count == 0) {
                UpdateStatus(Active.Off);
            }

            return base.Activate(target, dest);
        }

        if (hardenedStructures.Count >= maxStructures) return false;

        ApplyHardening(building);

        FleckMaker.Static(building.Position, pawn.Map, FleckDefOf.PsycastAreaEffect);

        return base.Activate(target, dest);
    }

    protected override void OnDisable() {
        base.OnDisable();

        for (int i = hardenedStructures.Count - 1; i >= 0; i--) {
            RemoveHardening(hardenedStructures[i]);
        }
    }

    public override void AbilityTick() {
        base.AbilityTick();
        if (!status.isActive) return;

        for (int i = hardenedStructures.Count - 1; i >= 0; i--) {
            Building? building = hardenedStructures[i];
            if (building == null || building.Destroyed) {
                if (building != null) HardenedBuildings.Remove(building);
                hardenedStructures.RemoveAt(i);
            }
        }

        if (hardenedStructures.Count == 0) {
            UpdateStatus(Active.Off);
        }
    }

    private void ApplyHardening(Building building) {
        int oldMax = building.MaxHitPoints;
        HardenedBuildings[building] = hpMultiplier;
        hardenedStructures.Add(building);
        int newMax = building.MaxHitPoints;
        int hpBonus = newMax - oldMax;
        building.HitPoints += hpBonus;
    }

    private void RemoveHardening(Building building) {
        int oldMax = building.MaxHitPoints;
        HardenedBuildings.Remove(building);
        hardenedStructures.Remove(building);
        int newMax = building.MaxHitPoints;
        building.HitPoints = Math.Min(building.HitPoints, newMax);
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref hardenedStructures, "hardenedStructures", LookMode.Reference);
    }

    [HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
    public static class HardenStateClearer {
        [HarmonyPostfix]
        public static void Postfix() {
            HardenedBuildings.Clear();
        }
    }
}
