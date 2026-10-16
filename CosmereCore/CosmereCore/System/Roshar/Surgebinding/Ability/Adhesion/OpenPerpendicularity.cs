using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Util;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Adhesion;

public class OpenPerpendicularity : SurgebindingAbility {
    private const int BaseRadius = 8;
    private const int RefillIntervalTicks = 30;
    private const float HealAmount = 2f;
    private readonly List<Pawn> alliesInArea = [];

    private Mote? auraMote;

    private ThingDef? auraMoteDef;

    public OpenPerpendicularity(Pawn pawn) : base(pawn) { }

    public OpenPerpendicularity(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    protected virtual string AuraMoteDefName => "Cosmere_Roshar_Thing_PerpendicularityAura";

    private ThingDef? AuraMoteDef => auraMoteDef ??= DefDatabase<ThingDef>.GetNamedSilentFail(AuraMoteDefName);

    private float Radius => BaseRadius + Gene.CurrentIdeal * 2;

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + Gene.CurrentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        if (Def.hediff != null) {
            SurgebindingHediffUtility.GetOrAddHediff(pawn, this, Def.hediff);
        }

        if (AuraMoteDef != null) {
            float moteScale = MoteUtility.GetMoteSize(
                AuraMoteDef,
                BaseRadius,
                GetStrength()
            );
            auraMote = MoteMaker.MakeAttachedOverlay(pawn, AuraMoteDef, Vector3.zero, moteScale);
        }
    }

    protected override void OnDisable() {
        base.OnDisable();

        if (auraMote != null && !auraMote.Destroyed) {
            auraMote.Destroy();
        }

        auraMote = null;

        for (int i = alliesInArea.Count - 1; i >= 0; i--) {
            Pawn ally = alliesInArea[i];
            if (ally != null && !ally.Dead && Def.hediff != null) {
                SurgebindingHediffUtility.RemoveHediff(ally, this, Def.hediff);
            }
        }

        alliesInArea.Clear();
    }

    public override void AbilityTick() {
        base.AbilityTick();
        if (!status.IsActive) return;

        auraMote?.Maintain();
        if (auraMote != null) {
            float moteScale = MoteUtility.GetMoteSize(
                AuraMoteDef!,
                BaseRadius,
                GetStrength()
            );
            auraMote.Graphic.drawSize = new Vector2(moteScale, moteScale);
        }

        if (!pawn.IsHashIntervalTick(RefillIntervalTicks)) return;

        float currentRadius = Radius;

        foreach (Verse.Thing thing in GenRadial.RadialDistinctThingsAround(
                     pawn.Position,
                     pawn.Map,
                     currentRadius,
                     true
                 )) {
            ApplyAuraTo(thing);
        }

        CleanupAllies(currentRadius);
    }

    private void ApplyAuraTo(Verse.Thing thing) {
        if (thing is not Pawn ally) {
            InvestitureHolder? holder = thing.TryGetComp<InvestitureHolder>();
            holder?.FillInvestiture();
            return;
        }

        if (ally.Dead) return;
        if (ally.Faction != pawn.Faction) return;

        RefillRadiantStormlight(ally);
        HealAlly(ally);

        if (Def.hediff != null && ally != pawn) {
            SurgebindingHediffUtility.GetOrAddHediff(ally, this, Def.hediff);
            alliesInArea.AddDistinct(ally);
        }
    }

    protected virtual void RefillRadiantStormlight(Pawn ally) {
        Surgebinder? surgebinder = ally.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;
        if (surgebinder.Value >= surgebinder.Max) return;

        surgebinder.SetReserve(surgebinder.Max);
    }

    protected virtual void HealAlly(Pawn ally) {
        List<Verse.Hediff> hediffs = ally.health.hediffSet.hediffs;
        for (int i = hediffs.Count - 1; i >= 0; i--) {
            Verse.Hediff hediff = hediffs[i];
            if (hediff is Hediff_Injury injury) {
                injury.Heal(HealAmount * (1 + Gene.CurrentIdeal * 0.5f));
                return;
            }
        }
    }

    private void CleanupAllies(float currentRadius) {
        for (int i = alliesInArea.Count - 1; i >= 0; i--) {
            Pawn ally = alliesInArea[i];
            if (ally == null || ally.Dead || !ally.Position.InHorDistOf(pawn.Position, currentRadius)) {
                if (ally != null && !ally.Dead && Def.hediff != null) {
                    SurgebindingHediffUtility.RemoveHediff(ally, this, Def.hediff);
                }

                alliesInArea.RemoveAt(i);
            }
        }
    }
}
