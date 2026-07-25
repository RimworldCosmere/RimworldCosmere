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
    /// Below this a tick would move nothing worth counting, so it waits instead.
    private const float MinimumCharge = 0.001f;

    private const float ChargeAtSkillFloor = 15f;
    private const float ChargeAtSkillCeiling = 25f;
    private const float SkillFloor = 10f;
    private const float SkillCeiling = 20f;

    /// Held rather than stopped: the reserve has run dry but the pawn is still
    /// set to compound, and will pick up again the moment a vial restocks them.
    public bool Paused { get; private set; }

    public Compound() { }

    public Compound(HediffDef hediffDef, Pawn pawn, IAbility<Allomancer, IHediff<Allomancer>> ability) : base(
        hediffDef,
        pawn,
        ability
    ) { }

    /// Reserve burned per real second. Chosen so an unpractised Compounder fills
    /// a seventy-five unit metalmind in about an in-game hour; skill raises the
    /// yield rather than the burn, so practice fills faster off the same metal.
    protected const float MetalPerSecond = 0.12f;

    public float MetalDrainPerSecond => MetalPerSecond * DialFraction;

    /// Scaled by how far the dial is pushed, so the player sets the pace rather
    /// than compounding being one speed you either take or leave.
    public virtual float StorePerSecond => MetalPerSecond * DialFraction * ChargePerMetalUnit;

    private float DialFraction =>
        pawn.genes?.GetFeruchemicGeneForMetal(metal) is { } gene ? gene.CompoundFraction : 1f;

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

        CompoundResult result = TickLogic(allomancer, feruchemist);
        if (result == CompoundResult.Paused) {
            if (!Paused) {
                Paused = true;
                Messages.Message(
                    "CS_Feruchemy_CompoundOutOfMetal".Translate(pawn.Named("PAWN"), metal.Named("METAL")),
                    pawn,
                    MessageTypeDefOf.NeutralEvent,
                    false
                );
            }

            return;
        }

        Paused = false;
        if (result == CompoundResult.Continue) return;

        // Stopping without a word looked like the feature was broken, when the
        // metalminds had simply filled.
        Messages.Message(
            "CS_Feruchemy_CompoundNoRoom".Translate(
                pawn.Named("PAWN"),
                metal.Named("METAL")
            ),
            pawn,
            MessageTypeDefOf.NeutralEvent,
            false
        );
        End();
    }

    protected enum CompoundResult {
        Continue,
        NoRoom,
        Paused,
    }

    protected virtual CompoundResult TickLogic(Allomancer allomancer, Feruchemist feruchemist) {
        // Clamped to the room available before anything is burned, so reserve is
        // never spent on charge that has nowhere to go.
        float room = feruchemist.CompoundedFreeSpace;
        if (room <= 0f) return CompoundResult.NoRoom;

        float seconds = GenTicks.TickRareInterval / (float)GenTicks.TicksPerRealSecond;
        float charge = Mathf.Min(StorePerSecond * seconds, room);

        // Spend what the reserve can actually cover rather than refusing a whole
        // tick for want of a fraction of one.
        float perUnit = ChargePerMetalUnit;
        charge = Mathf.Min(charge, allomancer.Value * perUnit);
        if (charge <= MinimumCharge) return CompoundResult.Paused;

        // Re-derived from the clamped charge so a partial fill only costs what it stored.
        float metalUnits = Mathf.Min(charge / perUnit, allomancer.Value);
        float beu = metalUnits * ScadrialMetallurgyConstants.BreathEquivalentUnitsPerMetalUnit;
        if (!allomancer.TryBurnMetalForInvestiture(beu)) return CompoundResult.Paused;

        return feruchemist.AddCompoundedToStore(charge) ? CompoundResult.Continue : CompoundResult.NoRoom;
    }

    private void End() {
        ability.UpdateStatus(BurningStatus.Off);
    }
}
