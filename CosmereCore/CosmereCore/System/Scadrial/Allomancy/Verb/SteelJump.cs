using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Extension;
using RimWorld;

namespace Cosmere.System.Scadrial.Allomancy.Verb;

public class SteelJump : Verb_CastAbilityJump {
    private new AllomancyAbility Ability => (AllomancyAbility)verbTracker.directOwner;

    public override float EffectiveRange {
        get {
            int power = SteelJumpRange.PowerFor(Ability.NextStatus?.power, Ability.status.power);

            return SteelJumpRange.For(
                base.EffectiveRange,
                power,
                CasterPawn.GetRawAllomanticPower(Ability.Metal),
                caster.GetStatValue(RimWorld.StatDefOf.Mass)
            );
        }
    }

    protected override bool TryCastShot() {
        bool result = base.TryCastShot();
        Ability.UpdateStatus(BurningStatus.Off);

        return result;
    }
}
