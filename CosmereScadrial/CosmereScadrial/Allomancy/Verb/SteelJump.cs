using CosmereScadrial.Allomancy.Ability;
using RimWorld;

namespace CosmereScadrial.Allomancy.Verb;

public class SteelJump : Verb_CastAbilityJump {
    private new AbstractAbility ability => (AbstractAbility)verbTracker.directOwner;

    public override float EffectiveRange {
        get {
            float baseRange = base.EffectiveRange;
            float strengthMultiplier = ability.GetStrength(ability.nextStatus);

            float normalizedMassFactor = caster.GetStatValue(RimWorld.StatDefOf.Mass) / 60;

            return baseRange * strengthMultiplier / normalizedMassFactor;
        }
    }

    protected override bool TryCastShot() {
        bool result = base.TryCastShot();
        ability.UpdateStatus(BurningStatus.Off);

        return result;
    }
}