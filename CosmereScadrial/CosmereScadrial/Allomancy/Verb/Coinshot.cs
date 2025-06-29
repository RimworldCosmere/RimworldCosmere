using System.Linq;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Allomancy.Hediff;
using CosmereScadrial.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Allomancy.Verb;

public class Coinshot : Verb_Shoot {
    private CoinshotAbility ability => (CoinshotAbility)verbTracker.directOwner;
    public override ThingDef Projectile => ThingDefOf.Cosmere_Scadrial_Thing_ClipProjectile;

    public override void WarmupComplete() {
        ability.UpdateStatus(ability.nextStatus!.Value);

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
        Verse.Thing? clip =
            pawn.inventory.innerContainer.FirstOrDefault(t => t.def.Equals(ThingDefOf.Cosmere_Scadrial_Thing_Clip));
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
        Job job = ability.GetJob(target, null);
        CasterPawn.jobs.TryTakeOrderedJob(job);
    }
}