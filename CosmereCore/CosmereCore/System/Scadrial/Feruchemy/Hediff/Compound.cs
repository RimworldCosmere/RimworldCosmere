using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Gene;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class Compound : AllomanticHediff {
    /// Compounding is meant to dwarf what the dial can move by hand, so the yield
    /// sits well above MaxTransferPerSecond. Scales with each ability's own cost
    /// and strength, so the per-second figure differs by metal.
    protected const float CompoundYield = 60f;

    public Compound() { }

    public Compound(HediffDef hediffDef, Pawn pawn, IAbility<Allomancer, IHediff<Allomancer>> ability) : base(
        hediffDef,
        pawn,
        ability
    ) { }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        Allomancer? allomancer = pawn.genes?.GetAllomanticGeneForMetal(metal);
        Feruchemist? feruchemist = pawn.genes?.GetFeruchemicGeneForMetal(metal);

        if (allomancer == null || feruchemist == null || pawn.DeadOrDowned) {
            End();
            return;
        }

        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;

        float strength = ability.GetStrength(BurningStatus.Burning);
        float metalToBurn = ability.def.beuPerTick * GenTicks.TickRareInterval * strength;
        if (!TickLogic(feruchemist, delta, metalToBurn)) {
            End();
        }
    }

    /// What compounding pours into the metalmind per real second, so the dock can
    /// report it alongside the dial's own contribution.
    public virtual float StorePerSecond =>
        ability.def.beuPerTick * ability.GetStrength(BurningStatus.Burning) * CompoundYield * GenTicks.TicksPerRealSecond;

    protected virtual bool TickLogic(Feruchemist feruchemist, int delta, float metalToBurn) {
        return feruchemist.AddToStore(metalToBurn * CompoundYield);
    }

    private void End() {
        ability.UpdateStatus(BurningStatus.Off);
    }
}