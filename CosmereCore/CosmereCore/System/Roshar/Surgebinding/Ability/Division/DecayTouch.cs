using System;
using Cosmere.Core.Ability;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Division;

public class DecayTouch : SurgebindingAbility {
    private const int DamageIntervalTicks = 60;

    public DecayTouch(Pawn pawn) : base(pawn) { }
    public DecayTouch(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private float pawnDamage => 3f + gene.currentIdeal * 2f;

    private float structureDamage => 5f + gene.currentIdeal * 5f;

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + gene.currentIdeal * 0.5f);
    }

    public override void AbilityTick() {
        base.AbilityTick();
        if (!status.isActive) return;
        if (!localTarget.HasValue) return;
        if (!pawn.IsHashIntervalTick(DamageIntervalTicks)) return;

        Verse.Thing? target = localTarget.Value.Thing;
        if (target == null || target.Destroyed) {
            UpdateStatus(Active.Off);
            return;
        }

        if (!pawn.Position.AdjacentTo8WayOrInside(target.Position)) {
            UpdateStatus(Active.Off);
            return;
        }

        if (target is Pawn targetPawn) {
            DamageInfo dinfo = new DamageInfo(
                DamageDefOf.Burn,
                pawnDamage,
                instigator: pawn
            );
            targetPawn.TakeDamage(dinfo);
        } else {
            float dmg = structureDamage;
            target.HitPoints = Math.Max(0, target.HitPoints - (int)dmg);
            if (target.HitPoints <= 0) {
                target.Destroy(DestroyMode.KillFinalize);
                UpdateStatus(Active.Off);
            }
        }
    }
}