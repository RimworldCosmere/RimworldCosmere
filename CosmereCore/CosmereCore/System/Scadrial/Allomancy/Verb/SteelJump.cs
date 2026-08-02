using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Extension;
using RimWorld;

namespace Cosmere.System.Scadrial.Allomancy.Verb;

public class SteelJump : Verb_CastAbilityJump {
    private new AllomancyAbility ability => (AllomancyAbility)verbTracker.directOwner;

    public override float EffectiveRange {
        get {
            int power = (ability.nextStatus ?? ability.status).power;

            return SteelJumpRange.For(
                base.EffectiveRange,
                power,
                CasterPawn.GetRawAllomanticPower(ability.metal),
                caster.GetStatValue(RimWorld.StatDefOf.Mass)
            );
        }
    }

    protected override bool TryCastShot() {
        bool result = base.TryCastShot();
        ability.UpdateStatus(BurningStatus.Off);

        return result;
    }
}
