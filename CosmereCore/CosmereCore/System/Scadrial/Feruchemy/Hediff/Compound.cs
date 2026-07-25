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

    /// What compounding pours into the metalmind per real second, for the dock
    /// readout. Shares its arithmetic with the tick so the two cannot drift.
    public virtual float StorePerSecond =>
        MetalPerRareTick * ChargePerMetalUnit * GenTicks.TicksPerRealSecond / GenTicks.TickRareInterval;

    /// Holding the ability active already drains the reserve at the burning rate
    /// through the usual pipeline, so compounding contributes the difference and
    /// the reserve empties at exactly the rate flaring would empty it.
    private float MetalPerRareTick =>
        ability.def.beuPerTick
        * (BurningStatus.Flaring.power - BurningStatus.Burning.power)
        / ScadrialMetallurgyConstants.BreathEquivalentUnitsPerMetalUnit;

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
            Core.Logger.Verbose(
                $"Compound end: allomancer={allomancer != null} feruchemist={feruchemist != null} deadOrDowned={pawn.DeadOrDowned} metal={metal?.defName}"
            );
            End();
            return;
        }

        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;

        if (!TickLogic(allomancer, feruchemist)) End();
    }

    protected virtual bool TickLogic(Allomancer allomancer, Feruchemist feruchemist) {
        // Clamped to the room available before anything is burned, so reserve is
        // never spent on charge that has nowhere to go.
        float free = feruchemist.CompoundedFreeSpace;
        float charge = Mathf.Min(MetalPerRareTick * ChargePerMetalUnit, free);
        Core.Logger.Verbose(
            $"Compound tick {metal?.defName}: perSec={StorePerSecond:F4} free={free:F2} charge={charge:F4} reserve={allomancer.Value:F4}/{allomancer.Max:F3}"
        );

        if (charge <= 0f) {
            Core.Logger.Verbose("Compound stop: nothing to store");
            return false;
        }

        // Re-derived from the clamped charge so a partial fill only costs what it stored.
        float metalUnits = charge / ChargePerMetalUnit;
        float beu = metalUnits * ScadrialMetallurgyConstants.BreathEquivalentUnitsPerMetalUnit;
        if (!allomancer.TryBurnMetalForInvestiture(beu)) {
            Core.Logger.Verbose($"Compound stop: reserve cannot pay {metalUnits:F3} units");
            return false;
        }

        bool stored = feruchemist.AddCompoundedToStore(charge);
        Core.Logger.Verbose($"Compound stored={stored} amount={charge:F2}");
        return stored;
    }

    private void End() {
        ability.UpdateStatus(BurningStatus.Off);
    }
}
