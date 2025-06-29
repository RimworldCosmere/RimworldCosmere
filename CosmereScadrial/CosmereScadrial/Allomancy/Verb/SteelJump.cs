using CosmereScadrial.Allomancy.Ability;
using RimWorld;

namespace CosmereScadrial.Allomancy.Verb;

public class SteelJump : Verb_CastAbilityJump {
    private AbstractAbility ability => (AbstractAbility)verbTracker.directOwner;

    public override float EffectiveRange => base.EffectiveRange * ability.GetStrength(ability.nextStatus);

    protected override bool TryCastShot() {
        bool result = base.TryCastShot();
        ability.UpdateStatus(BurningStatus.Off);

        return result;
    }
}