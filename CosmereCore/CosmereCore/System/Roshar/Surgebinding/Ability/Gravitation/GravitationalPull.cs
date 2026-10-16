using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Gravitation;

public class GravitationalPull : SurgebindingAbility {
    private const int BaseRadius = 3;
    private readonly List<Pawn> pawnsInArea = [];

    public GravitationalPull(Pawn pawn) : base(pawn) { }

    public GravitationalPull(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private float Radius => BaseRadius + Gene.CurrentIdeal;

    private HediffDef HediffToApply => Def.hediff!;

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + Gene.CurrentIdeal * 0.5f);
    }

    protected override void OnDisable() {
        base.OnDisable();

        for (int i = pawnsInArea.Count - 1; i >= 0; i--) {
            Pawn targetPawn = pawnsInArea[i];
            if (targetPawn != null && !targetPawn.Dead) {
                SurgebindingHediffUtility.RemoveHediff(targetPawn, this, HediffToApply);
            }
        }

        pawnsInArea.Clear();
    }

    public override void AbilityTick() {
        base.AbilityTick();
        if (!status.IsActive) return;

        float currentRadius = Radius;

        PruneDepartedPawns(currentRadius);

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

            SurgebindingHediffUtility.GetOrAddHediff(targetPawn, this, HediffToApply);
            pawnsInArea.AddDistinct(targetPawn);
        }
    }

    private void PruneDepartedPawns(float currentRadius) {
        for (int i = pawnsInArea.Count - 1; i >= 0; i--) {
            Pawn targetPawn = pawnsInArea[i];
            if (targetPawn == null ||
                targetPawn.Dead ||
                !targetPawn.Position.InHorDistOf(pawn.Position, currentRadius)) {
                if (targetPawn != null && !targetPawn.Dead) {
                    SurgebindingHediffUtility.RemoveHediff(targetPawn, this, HediffToApply);
                }

                pawnsInArea.RemoveAt(i);
            }
        }
    }
}
