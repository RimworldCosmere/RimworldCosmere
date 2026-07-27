using System;
using Cosmere.Core.Ability;
using Cosmere.Core.Util;
using Cosmere.System.Roshar.Surgebinding.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Division;

public class EntropicAura : SurgebindingAbility {
    private const int BaseRadius = 2;
    private const int DamageIntervalTicks = 120;
    private const float BaseDamage = 3f;

    private static ThingDef? _entropicAuraDef;

    private static ThingDef? AuraMoteDef => _entropicAuraDef ??= ThingDefOf.Cosmere_Roshar_Thing_EntropicAura;

    private Mote? auraMote;

    public EntropicAura(Pawn pawn) : base(pawn) { }

    public EntropicAura(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private float radius => BaseRadius + Gene.CurrentIdeal;

    private float damage => BaseDamage + Gene.CurrentIdeal * 1.5f;

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + Gene.CurrentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        SurgebindingHediffUtility.GetOrAddHediff(pawn, this, def.hediff);
        if (AuraMoteDef != null) {
            float moteScale = MoteUtility.GetMoteSize(AuraMoteDef, BaseRadius, GetStrength());
            auraMote = MoteMaker.MakeAttachedOverlay(pawn, AuraMoteDef, Vector3.zero, moteScale);
        }
    }

    protected override void OnDisable() {
        base.OnDisable();
        SurgebindingHediffUtility.RemoveHediff(pawn, this, def.hediff);
        if (auraMote != null && !auraMote.Destroyed) {
            auraMote.Destroy();
        }

        auraMote = null;
    }

    public override void AbilityTick() {
        base.AbilityTick();
        auraMote?.Maintain();
        if (auraMote != null) {
            float moteScale = MoteUtility.GetMoteSize(AuraMoteDef!, BaseRadius, GetStrength());
            auraMote.Graphic.drawSize = new Vector2(moteScale, moteScale);
        }

        if (!status.IsActive) return;
        if (!pawn.IsHashIntervalTick(DamageIntervalTicks)) return;

        float currentRadius = radius;
        float currentDamage = damage;
        float structureDamage = currentDamage * 0.5f;

        foreach (Verse.Thing thing in GenRadial.RadialDistinctThingsAround(
                     pawn.Position,
                     pawn.Map,
                     currentRadius,
                     true
                 )) {
            if (thing == pawn) continue;
            if (thing.Faction == pawn.Faction) continue;

            if (thing is Pawn targetPawn) {
                if (targetPawn.Dead) continue;

                DamageInfo dinfo = new DamageInfo(
                    DamageDefOf.Burn,
                    currentDamage,
                    instigator: pawn
                );
                targetPawn.TakeDamage(dinfo);

                if (Gene.CurrentIdeal >= 3) {
                    DegradeEquipment(targetPawn);
                }
            } else if (thing is Building && thing.def.useHitPoints) {
                int dmg = Math.Max(1, (int)structureDamage);
                thing.HitPoints -= dmg;
                if (thing.HitPoints <= 0) {
                    thing.Destroy(DestroyMode.KillFinalize);
                }
            }
        }
    }

    private void DegradeEquipment(Pawn targetPawn) {
        if (targetPawn.apparel == null) return;

        float degradeAmount = Gene.CurrentIdeal >= 4 ? 2f : 1f;
        List<Apparel> wornApparel = targetPawn.apparel.WornApparel;
        for (int i = 0; i < wornApparel.Count; i++) {
            Apparel apparel = wornApparel[i];
            if (apparel.HitPoints > 0) {
                apparel.HitPoints = Math.Max(0, apparel.HitPoints - (int)degradeAmount);
            }
        }
    }
}
