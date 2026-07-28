using Concord;
using Cosmere.Core.Ability;
using Cosmere.Core.Util;
using Cosmere.System.Roshar.Surgebinding.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Adhesion;

public class WindsprenShield : SurgebindingAbility {
    private const int BaseRadius = 3;
    public static readonly HashSet<Pawn> ShieldedPawns = [];
    private static ThingDef? _auraMoteDef;

    private static ThingDef? AuraMoteDef => _auraMoteDef ??= ThingDefOf.Cosmere_Roshar_Thing_WindsprenShieldAura;

    private readonly List<Pawn> pawnsInArea = [];
    private Mote? auraMote;

    public WindsprenShield(Pawn pawn) : base(pawn) { }

    public WindsprenShield(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private float radius => BaseRadius + Gene.CurrentIdeal;

    private HediffDef hediffToApply => def.hediff!;

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + Gene.CurrentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        ShieldedPawns.Add(pawn);
        SurgebindingHediffUtility.GetOrAddHediff(pawn, this, hediffToApply);

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
        ShieldedPawns.Remove(pawn);

        if (auraMote != null && !auraMote.Destroyed) {
            auraMote.Destroy();
        }

        auraMote = null;

        for (int i = pawnsInArea.Count - 1; i >= 0; i--) {
            Pawn targetPawn = pawnsInArea[i];
            if (targetPawn != null && !targetPawn.Dead) {
                SurgebindingHediffUtility.RemoveHediff(targetPawn, this, hediffToApply);
            }
        }

        pawnsInArea.Clear();
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

        if (pawn.IsHashIntervalTick(15)) {
            IntVec3 randomCell = pawn.Position + GenRadial.RadialPattern[Rand.Range(1, (int)(radius * radius))];
            if (randomCell.InBounds(pawn.Map)) {
                FleckMaker.ThrowDustPuffThick(
                    randomCell.ToVector3Shifted(),
                    pawn.Map,
                    0.5f,
                    new Color(0.7f, 0.85f, 1f, 0.4f)
                );
            }
        }

        float currentRadius = radius;

        for (int i = pawnsInArea.Count - 1; i >= 0; i--) {
            Pawn targetPawn = pawnsInArea[i];
            if (targetPawn == null ||
                targetPawn.Dead ||
                !targetPawn.Position.InHorDistOf(pawn.Position, currentRadius)) {
                if (targetPawn != null && !targetPawn.Dead) {
                    SurgebindingHediffUtility.RemoveHediff(targetPawn, this, hediffToApply);
                }

                pawnsInArea.RemoveAt(i);
            }
        }

        if (!pawn.IsHashIntervalTick(30)) return;

        foreach (Verse.Thing thing in GenRadial.RadialDistinctThingsAround(
                     pawn.Position,
                     pawn.Map,
                     currentRadius,
                     true
                 )) {
            if (thing is not Pawn targetPawn) continue;
            if (targetPawn.Dead) continue;
            if (targetPawn.Faction != pawn.Faction) continue;

            SurgebindingHediffUtility.GetOrAddHediff(targetPawn, this, hediffToApply);
            pawnsInArea.AddDistinct(targetPawn);
        }
    }

    [Patch(typeof(MemoryUtility))]
    public static class WindsprenShieldStateClearer {
        [Inject(At.Return, nameof(MemoryUtility.ClearAllMapsAndWorld))]
        private static void AfterClearAllMapsAndWorld() {
            ShieldedPawns.Clear();
        }
    }
}
