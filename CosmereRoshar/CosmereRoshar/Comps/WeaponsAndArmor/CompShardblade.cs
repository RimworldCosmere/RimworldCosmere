using System.Linq;
using CosmereRoshar.Combat.Abilities.Implementations;
using CosmereRoshar.Comp;
using CosmereRoshar.Comp.Thing;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereRoshar.Comps.WeaponsAndArmor;

public class GraphicShardBladeVariants : Graphic_Collection {
    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo) {
        return GraphicDatabase.Get<GraphicShardBladeVariants>(
            path,
            newShader,
            drawSize,
            newColor,
            newColorTwo,
            data
        );
    }

    public override Material MatAt(Rot4 rot, Thing thing = null) {
        return MatSingleFor(thing);
    }

    public override Material MatSingleFor(Thing thing) {
        int id = StormlightUtilities.GetGraphicId(thing);
        return subGraphics[id].MatSingle;
    }


    public override void DrawWorker(
        Vector3 loc,
        Rot4 rot,
        ThingDef thingDef,
        Thing thing,
        float extraRotation
    ) {
        int id = StormlightUtilities.GetGraphicId(thing);
        Graphic graphic = subGraphics[id];
        graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
    }

    public override string ToString() {
        return "StackCount(path=" + path + ", count=" + subGraphics.Length + ")";
    }
}

public static class AbilityExtensions {
    public static T GetAbilityComp<T>(this Pawn pawn, string abilityDefName) where T : CompAbilityEffect {
        if (pawn.abilities == null) return null;

        Ability ability = pawn.abilities.GetAbility(DefDatabase<AbilityDef>.GetNamed(abilityDefName));
        return ability?.comps?.OfType<T>().FirstOrDefault();
    }
}

public class CompShardblade : ThingComp {
    public int graphicId = -1;
    private bool isSpawned;
    public Pawn swordOwner;
    public CompPropertiesShardblade props => base.props as CompPropertiesShardblade;


    public override void Initialize(CompProperties props) {
        base.props = props;
        if (graphicId == -1) {
            graphicId = this.props.bladesInExistence % 4 + 1;
            this.props.bladesInExistence++;
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_References.Look(ref swordOwner, "swordOwner");
        Scribe_Values.Look(ref isSpawned, "isSpawned");
        Scribe_Values.Look(ref graphicId, "graphicId", -1);
    }

    public bool IsBonded(Pawn pawn) {
        return swordOwner == pawn;
    }

    private void HandleSwordAbility(Pawn pawn, CompAbilityEffectSpawnEquipment abilityComp) {
        if (abilityComp == null) {
            pawn.abilities.GainAbility(CosmereRosharDefs.WhtwlSummonShardblade);
            Trait trait = StormlightUtilities.GetRadiantTrait(pawn);

            if (trait == null) {
                //radiants does not get this ability
                pawn.abilities.GainAbility(CosmereRosharDefs.WhtwlUnbondBlade);
            }
        }
    }

    public void BondWithPawn(Pawn pawn, bool isBladeSpawned) {
        swordOwner = pawn;
        ThingWithComps blade = parent;
        CompAbilityEffectSpawnEquipment abilityComp =
            pawn.GetAbilityComp<CompAbilityEffectSpawnEquipment>(CosmereRosharDefs.WhtwlSummonShardblade.defName);
        HandleSwordAbility(pawn, abilityComp);
        if (abilityComp == null) {
            abilityComp =
                pawn.GetAbilityComp<CompAbilityEffectSpawnEquipment>(
                    CosmereRosharDefs.WhtwlSummonShardblade.defName
                );
        }

        abilityComp.bladeObject = blade;
        isSpawned = isBladeSpawned;
    }


    private static void RemoveRadiantStuff(Pawn pawn) {
        if (pawn?.story?.traits == null) {
            Log.Error("[stormlight mod] Pawn has no traits system!");
            return;
        }

        Trait trait = StormlightUtilities.GetRadiantTrait(pawn);
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
        if (swordOwner == pawn) {
            RemoveRadiantStuff(pawn);
            swordOwner = null;
        }
    }

    public bool IsBladeSpawned() {
        return isSpawned;
    }

    public void Summon() {
        if (swordOwner == null || isSpawned) {
            return;
        }

        CompAbilityEffectSpawnEquipment abilityComp =
            swordOwner.GetAbilityComp<CompAbilityEffectSpawnEquipment>(
                CosmereRosharDefs.WhtwlSummonShardblade.defName
            );
        if (abilityComp != null && abilityComp.bladeObject != null) {
            swordOwner.equipment.AddEquipment(abilityComp.bladeObject);
            isSpawned = true;
        }
    }

    public void DismissBlade(Pawn pawn) {
        ThingWithComps droppedWeapon;
        bool success = pawn.equipment.TryDropEquipment(
            pawn.equipment.Primary,
            out droppedWeapon,
            pawn.Position,
            false
        );
        isSpawned = false;
        //dismissal vs dropping is handled by harmony patch in ShardbladePatches.cs
    }
}

public class CompPropertiesShardblade : CompProperties {
    public int bladesInExistence;

    public CompPropertiesShardblade() {
        compClass = typeof(CompShardblade);
    }
}