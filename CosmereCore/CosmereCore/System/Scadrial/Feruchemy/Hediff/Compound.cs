using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class Compound : AllomanticHediff {
    private const float ChargeAtSkillFloor = 15f;
    private const float ChargeAtSkillCeiling = 25f;
    private const float SkillFloor = 10f;
    private const float SkillCeiling = 20f;

    public Compound() { }

    public Compound(HediffDef hediffDef, Pawn pawn, IAbility<Allomancer, IHediff<Allomancer>> ability) : base(
        hediffDef,
        pawn,
        ability
    ) { }

    /// Charge poured into the metalmind per real second.
    protected const float ChargePerSecond = 0.1f;

    public virtual float StorePerSecond => ChargePerSecond;

    /// Reserve spent per real second. Skill does not make compounding faster, it
    /// makes it cheaper - the same charge costs less swallowed metal.
    public float MetalDrainPerSecond => ChargePerSecond / ChargePerMetalUnit;

    /// A Compounder practised in both arts wrings more out of the same swallowed
    /// metal, so yield rises with the average of the two skills.
    private float ChargePerMetalUnit {
        get {
            if (pawn.skills == null) return ChargeAtSkillFloor;

            float allomancy = pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower).Level;
            float feruchemy = pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower).Level;
            float t = Mathf.Clamp01(((allomancy + feruchemy) / 2f - SkillFloor) / (SkillCeiling - SkillFloor));

            return Mathf.Lerp(ChargeAtSkillFloor, ChargeAtSkillCeiling, t);
        }
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        Allomancer? allomancer = pawn.genes?.GetAllomanticGeneForMetal(metal);
        Feruchemist? feruchemist = pawn.genes?.GetFeruchemicGeneForMetal(metal);

        if (allomancer == null || feruchemist == null || pawn.DeadOrDowned) {
            End();
            return;
        }

        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;

        if (!TickLogic(allomancer, feruchemist)) End();
    }

    protected virtual bool TickLogic(Allomancer allomancer, Feruchemist feruchemist) {
        // Clamped to the room available before anything is burned, so reserve is
        // never spent on charge that has nowhere to go.
        float seconds = GenTicks.TickRareInterval / (float)GenTicks.TicksPerRealSecond;
        float charge = Mathf.Min(ChargePerSecond * seconds, feruchemist.CompoundedFreeSpace);

        if (charge <= 0f) return false;

        // Re-derived from the clamped charge so a partial fill only costs what it stored.
        float metalUnits = charge / ChargePerMetalUnit;
        float beu = metalUnits * ScadrialMetallurgyConstants.BreathEquivalentUnitsPerMetalUnit;
        if (!allomancer.TryBurnMetalForInvestiture(beu)) return false;

        return feruchemist.AddCompoundedToStore(charge);
    }

    private void End() {
        ability.UpdateStatus(BurningStatus.Off);
    }
}
