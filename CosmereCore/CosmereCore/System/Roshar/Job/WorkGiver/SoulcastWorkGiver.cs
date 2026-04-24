using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;
using RimWorld;
using Verse;
using Verse.AI;
using SoulcastAbility = Cosmere.System.Roshar.Surgebinding.Ability.Transformation.Soulcast;

namespace Cosmere.System.Roshar.Job.WorkGiver;

public class SoulcastWorkGiver : WorkGiver_Scanner {
    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn) {
        if (FindSoulcastAbility(pawn) == null) yield break;

        foreach (Designation designation in pawn.Map.designationManager.SpawnedDesignationsOfDef(
                     Designator_Soulcast.DesignationDef
                 )) {
            yield return designation.target.Cell;
        }
    }

    public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false) {
        if (pawn.Map.designationManager.DesignationAt(c, Designator_Soulcast.DesignationDef) == null) return false;
        if (!SoulcastOverlay.TryGetData(c, out _, out _, out _)) return false;
        return FindSoulcastAbility(pawn) != null;
    }

    public override Verse.AI.Job? JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false) {
        if (!SoulcastOverlay.TryGetData(cell, out _, out _, out _)) return null;

        SoulcastAbility? ability = FindSoulcastAbility(pawn);
        if (ability == null) return null;

        JobDef soulcastJobDef = JobDefOf.Cosmere_Roshar_Job_Soulcast;
        Verse.AI.Job job = JobMaker.MakeJob(soulcastJobDef, cell);
        job.ability = ability;
        return job;
    }

    private static SoulcastAbility? FindSoulcastAbility(Pawn pawn) {
        if (pawn.abilities == null) return null;
        List<Ability> abilities = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is SoulcastAbility soulcast) return soulcast;
        }

        return null;
    }
}