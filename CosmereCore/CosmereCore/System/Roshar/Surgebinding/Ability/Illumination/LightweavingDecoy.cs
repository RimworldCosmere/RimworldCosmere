using System;
using RimWorld;
using Verse;
using DecoyHediff = Cosmere.System.Roshar.Surgebinding.Hediff.Illumination.LightweavingDecoy;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Illumination;

public class LightweavingDecoy : SurgebindingAbility {
    public LightweavingDecoy(Pawn pawn) : base(pawn) { }
    public LightweavingDecoy(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private int decoyCount => Math.Clamp(Gene.CurrentIdeal, 1, 4);

    protected override void OnEnable() {
        base.OnEnable();
        SpawnDecoys();
    }

    protected override void OnDisable() {
        DestroyAllDecoys();
        base.OnDisable();
    }

    private void SpawnDecoys() {
        if (!pawn.Spawned || pawn.Map == null) return;

        int count = decoyCount;
        for (int i = 0; i < count; i++) {
            IntVec3 spawnCell = CellFinder.RandomSpawnCellForPawnNear(pawn.Position, pawn.Map, 3);
            if (!spawnCell.IsValid) continue;

            Pawn decoy = LightweavingDecoyFactory.Spawn(pawn, spawnCell, pawn.Map);
            LightweavingDecoyRegistry.Add(decoy);
            FleckMaker.Static(spawnCell, pawn.Map, FleckDefOf.PsycastAreaEffect);
        }
    }

    private void DestroyAllDecoys() {
        List<Pawn> all = LightweavingDecoyRegistry.All;
        for (int i = all.Count - 1; i >= 0; i--) {
            Pawn decoy = all[i];
            DecoyHediff? hediff = decoy.health?.hediffSet?.GetFirstHediffOfDef(DecoyHediff.Def) as DecoyHediff;
            if (hediff?.caster != pawn) continue;

            if (decoy.Spawned) {
                FleckMaker.Static(decoy.Position, decoy.Map, FleckDefOf.PsycastAreaEffect);
                decoy.DeSpawn();
            }

            if (!decoy.Destroyed) {
                decoy.Discard(true);
            }

            LightweavingDecoyRegistry.RemoveAt(i);
        }
    }
}
