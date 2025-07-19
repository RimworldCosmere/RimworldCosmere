using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Extensions;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

public class ShardBladeProperties : CompProperties {
    public int bladesInExistence;

    public ShardBladeProperties() {
        compClass = typeof(ShardBlade);
    }
}

public class ShardBlade : ThingComp {
    public int graphicId = -1;
    private bool isSpawned;
    public Pawn? swordOwner;
    private new ShardBladeProperties props => (ShardBladeProperties)base.props;

    public override void Initialize(CompProperties props) {
        base.Initialize(props);
        if (graphicId != -1) return;

        graphicId = this.props.bladesInExistence % 4 + 1;
        this.props.bladesInExistence++;
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_References.Look(ref swordOwner, "swordOwner");
        Scribe_Values.Look(ref isSpawned, "isSpawned");
        Scribe_Values.Look(ref graphicId, "graphicId", -1);
    }

    public bool IsBonded(Pawn? pawn) {
        return swordOwner == pawn;
    }

    private void HandleSwordAbility(Pawn pawn, SpawnEquipment? abilityComp) {
        if (abilityComp != null) return;
        pawn.abilities.GainAbility(CosmereRosharDefs.WhtwlSummonShardblade);
        Trait? trait = StormlightUtilities.GetRadiantTrait(pawn);

        if (trait == null) {
            //radiants does not get this ability
            pawn.abilities.GainAbility(CosmereRosharDefs.WhtwlUnbondBlade);
        }
    }

    public void BondWithPawn(Pawn pawn, bool isBladeSpawned) {
        swordOwner = pawn;
        ThingWithComps blade = parent;
        SpawnEquipment? abilityComp =
            pawn.GetAbilityComp<SpawnEquipment>(CosmereRosharDefs.WhtwlSummonShardblade.defName);
        HandleSwordAbility(pawn, abilityComp);
        abilityComp ??= pawn.GetAbilityComp<SpawnEquipment>(
            CosmereRosharDefs.WhtwlSummonShardblade.defName
        );

        abilityComp!.bladeObject = blade;
        isSpawned = isBladeSpawned;
    }


    private static void RemoveRadiantStuff(Pawn pawn) {
        if (pawn?.story?.traits == null) {
            Log.Error("[stormlight mod] Pawn has no traits system!");
            return;
        }

        Trait? trait = StormlightUtilities.GetRadiantTrait(pawn);
        if (trait != null) {
            pawn.story.traits.allTraits.Remove(trait);
            Log.Message($"[stormlight mod] {pawn.Name} broke an oath and lost its Radiant trait.");

            Stormlight comp = pawn.GetComp<Stormlight>();
            if (comp != null) {
                pawn.AllComps.Remove(comp);
            }

            CompGlower glowComp = pawn.GetComp<CompGlower>();
            if (glowComp != null) {
                pawn.AllComps.Remove(glowComp);
            }
        }

        pawn.abilities.RemoveAbility(CosmereRosharDefs.WhtwlSummonShardblade);
        pawn.abilities.RemoveAbility(CosmereRosharDefs.WhtwlUnbondBlade);
    }


    public void SeverBond(Pawn pawn) {
        if (swordOwner != pawn) return;
        RemoveRadiantStuff(pawn);
        swordOwner = null;
    }

    public bool IsBladeSpawned() {
        return isSpawned;
    }

    public void Summon() {
        if (swordOwner == null || isSpawned) {
            return;
        }

        SpawnEquipment? abilityComp =
            swordOwner.GetAbilityComp<SpawnEquipment>(
                CosmereRosharDefs.WhtwlSummonShardblade.defName
            );
        if (abilityComp?.bladeObject == null) return;
        swordOwner.equipment.AddEquipment(abilityComp.bladeObject);
        isSpawned = true;
    }

    public void DismissBlade(Pawn pawn) {
        isSpawned = !pawn.equipment.TryDropEquipment(
            pawn.equipment.Primary,
            out _,
            pawn.Position,
            false
        );
        //dismissal vs dropping is handled by harmony patch in ShardbladePatches.cs
    }
}