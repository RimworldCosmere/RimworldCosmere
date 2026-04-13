using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.Thing.Building;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Fabrials;

public class BasicFabrialAugmenter : BasicFabrial {
    public CompGlower? glowerComp => parent.TryGetComp<CompGlower>();

    protected override HediffDef PainHediffDef => HediffDefOf.Cosmere_Roshar_Painrial_Augment;

    public override void AddGemstone(ThingWithComps gemstone) {
        if (!gemstone.HasComp<SprenContainer>()) return;
        insertedGemstone = gemstone;
        RegisterBuilding();
    }

    public override void RemoveGemstone() {
        if (insertedGemstone == null) return;

        IntVec3 dropPosition = parent.Position;
        dropPosition.z -= 1;
        GenPlace.TryPlaceThing(insertedGemstone, dropPosition, parent.Map, ThingPlaceMode.Near);
        UnregisterBuilding();
        insertedGemstone = null;
    }

    protected override void DoFlameSprenPower() {
        if (!powerOn || insertedGemstone == null) return;

        InvestitureHolder? investiture = insertedGemstone.TryGetComp<InvestitureHolder>();
        if (investiture == null) return;

        float maxEnergy = investiture.maxInvestitureSelf * 3f;
        float targetTemp = investiture.maxInvestitureSelf;
        if (targetTemp < 20f) targetTemp *= 1.25f;
        float num2 = GenTemperature.ControlTemperatureTempChange(
            parent.Position,
            parent.Map,
            maxEnergy,
            targetTemp
        );
        if (!Mathf.Approximately(num2, 0f)) {
            parent.GetRoom().Temperature += num2;
        }
    }

    protected override void DoColdSprenPower() {
        if (!powerOn || parent.IsOutside()) return;

        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture == null) return;

        float gemstoneSize = investiture.maxInvestitureSelf * 3f;
        float targetTemp = -5f;
        float currentTemp = parent.GetRoom().Temperature;
        if (currentTemp > targetTemp) {
            GenTemperature.PushHeat(parent.Position, parent.Map, 0f - gemstoneSize);
        }
    }

    protected override void OpenFilterDialog() {
        Find.WindowStack.Add(new SphereFilter<BasicFabrialAugmenter>(this));
    }
}
