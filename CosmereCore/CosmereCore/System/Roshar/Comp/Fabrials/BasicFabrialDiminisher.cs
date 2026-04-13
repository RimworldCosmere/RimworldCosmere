using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.Thing.Building;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Fabrials;

public class BasicFabrialDiminisher : BasicFabrial {
    private float tempWhenTurnedOn;

    public CompGlower glowerComp => parent.GetComp<CompGlower>();

    protected override HediffDef PainHediffDef => HediffDefOf.Cosmere_Roshar_Painrial_Diminisher;

    public override void AddGemstone(ThingWithComps gemstone) {
        if (!gemstone.HasComp<SprenContainer>()) return;
        insertedGemstone = gemstone;
        RegisterBuilding();
    }

    public override void RemoveGemstone() {
        if (insertedGemstone == null) return;
        Verse.Thing gemstoneToDrop = insertedGemstone;
        insertedGemstone = null;
        IntVec3 dropPosition = parent.Position;
        dropPosition.z -= 1;
        GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, parent.Map, ThingPlaceMode.Near);
        UnregisterBuilding();
    }

    protected override void SaveExtraData() {
        Scribe_Values.Look(ref tempWhenTurnedOn, "TempWhenTurnedOn");
    }

    public override void CheckPower(bool flickeredOn) {
        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture != null) {
            bool power = investiture.currentInvestiture > 0 && flickeredOn;
            if (!powerOn && power) {
                tempWhenTurnedOn = parent.GetRoom().Temperature;
            }
            powerOn = power;
            return;
        }
        powerOn = false;
    }

    protected override void DoFlameSprenPower() {
        if (!powerOn || parent.IsOutside()) return;
        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture == null) return;
        float gemstoneSize = investiture.maxInvestitureSelf * 3f;
        float currentTemp = parent.GetRoom().Temperature;
        if (currentTemp > tempWhenTurnedOn) {
            GenTemperature.PushHeat(parent.Position, parent.Map, 0f - gemstoneSize);
        }
    }

    protected override void DoColdSprenPower() {
        if (!powerOn || parent.IsOutside()) return;

        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture == null) return;
        float gemstoneSize = investiture.maxInvestitureSelf * 3f;
        float targetTemp = tempWhenTurnedOn;
        float currentTemp = parent.GetRoom().Temperature;
        if (currentTemp < targetTemp) {
            GenTemperature.PushHeat(parent.Position, parent.Map, gemstoneSize);
        }
    }

    protected override void OpenFilterDialog() {
        Find.WindowStack.Add(new SphereFilter<BasicFabrialDiminisher>(this));
    }
}
