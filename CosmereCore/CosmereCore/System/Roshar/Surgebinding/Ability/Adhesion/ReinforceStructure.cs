using Cosmere.Core.Ability;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Adhesion;

public class ReinforceStructure : SurgebindingAbility {
    public ReinforceStructure(Pawn pawn) : base(pawn) { }
    public ReinforceStructure(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        if (!base.Activate(target, dest)) return false;

        if (pawn.Map == null) return true;

        List<Building> buildings = pawn.Map.listerBuildings.allBuildingsColonist;
        for (int i = 0; i < buildings.Count; i++) {
            Building building = buildings[i];
            if (building.Faction != pawn.Faction) continue;
            building.HitPoints = building.MaxHitPoints;
        }

        FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, 6f);
        return true;
    }
}
