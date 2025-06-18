using System.Linq;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Abilities.Allomancy.Verbs;

public class Coinshot : Verb_Shoot {
    private CoinshotAbility ability => (CoinshotAbility)verbTracker.directOwner;
    public override ThingDef Projectile => CoinThingDefOf.Cosmere_Clip_Projectile;

    public override void WarmupComplete() {
        ability.UpdateStatus(ability.shouldFlare ? BurningStatus.Flaring : BurningStatus.Burning);

        SurgeChargeHediff? surge = AllomancyUtility.GetSurgeBurn(CasterPawn);
        surge?.Burn();
        base.WarmupComplete();
        surge?.PostBurn();
        ability.UpdateStatus(BurningStatus.Off);
    }

    protected override bool TryCastShot() {
        Pawn? pawn = CasterPawn;
        if (pawn?.inventory == null) return false;

        // Find a Clip in inventory
        Thing? clip = pawn.inventory.innerContainer.FirstOrDefault(t => t.def.Equals(CoinThingDefOf.Cosmere_Clip));
        if (clip == null) {
            Messages.Message("No Clips available!", pawn, MessageTypeDefOf.RejectInput, false);
            return false;
        }

        if (!AllomancyUtility.TryBurnMetalForInvestiture(pawn, ability.metal, ability.def.beuPerTick)) return false;
        base.TryCastShot();

        // Consume the clip
        clip.stackCount--;
        if (clip.stackCount <= 0) clip.Destroy();

        return true;
    }

    public override void OrderForceTarget(LocalTargetInfo target) {
        float num = verbProps.EffectiveMinRange(target, CasterPawn);
        if (CasterPawn.Position.DistanceToSquared(target.Cell) < num * (double)num &&
            CasterPawn.Position.AdjacentTo8WayOrInside(target.Cell)) {
            Messages.Message("MessageCantShootInMelee".Translate(), (Thing)CasterPawn, MessageTypeDefOf.RejectInput,
                false);
        } else {
            Job job = ability.GetJob(target, null);
            CasterPawn.jobs.TryTakeOrderedJob(job);
        }
    }
}