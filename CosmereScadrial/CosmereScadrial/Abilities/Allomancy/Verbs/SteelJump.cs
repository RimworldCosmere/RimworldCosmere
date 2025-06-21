using RimWorld;
using UnityEngine;

namespace CosmereScadrial.Abilities.Allomancy.Verbs;

public class SteelJump : Verb_CastAbilityJump {
    private AbstractAbility ability => (AbstractAbility)verbTracker.directOwner;

    public override float EffectiveRange => base.EffectiveRange *
                                            ability.GetStrength(Event.current.control
                                                ? BurningStatus.Flaring
                                                : BurningStatus.Burning);

    protected override bool TryCastShot() {
        bool result = base.TryCastShot();
        ability.UpdateStatus(BurningStatus.Off);

        return result;
    }
}