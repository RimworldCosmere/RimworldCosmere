using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection;
using RimWorld.QuestGen;
using System.Linq;
using System;

namespace CosmereRoshar {


    public class Graphic_ShardBladeVariants : Graphic_Collection {
        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo) {
            return GraphicDatabase.Get<Graphic_ShardBladeVariants>(path, newShader, drawSize, newColor, newColorTwo, data);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null) {
            return MatSingleFor(thing);
        }

        public override Material MatSingleFor(Thing thing) {
            int id = StormlightUtilities.GetGraphicId(thing);
            return subGraphics[id].MatSingle;
        }


        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation) {
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
        public CompProperties_Shardblade Props => props as CompProperties_Shardblade;
        public Pawn swordOwner = null;
        public int graphicId = -1;
        private bool isSpawned = false;


        public override void Initialize(CompProperties props) {
            this.props = props;
            if (graphicId == -1) {
                graphicId = (Props.bladesInExistence % 4) + 1;
                Props.bladesInExistence++;
            }
        }
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_References.Look(ref swordOwner, "swordOwner", saveDestroyedThings: false);
            Scribe_Values.Look(ref isSpawned, "isSpawned", false);
            Scribe_Values.Look(ref graphicId, "graphicId", -1);
        }

        public bool isBonded(Pawn pawn) {
            return swordOwner == pawn;
        }

        private void handleSwordAbility(Pawn pawn, CompAbilityEffect_SpawnEquipment abilityComp) {
            if (abilityComp == null) {
                pawn.abilities.GainAbility(CosmereRosharDefs.whtwl_SummonShardblade);
                Trait trait = StormlightUtilities.GetRadiantTrait(pawn);

                if (trait == null) { //radiants does not get this ability
                    pawn.abilities.GainAbility(CosmereRosharDefs.whtwl_UnbondBlade);
                }
            }
        }

        public void bondWithPawn(Pawn pawn, bool isBladeSpawned) {
            swordOwner = pawn;
            ThingWithComps blade = this.parent as ThingWithComps;
            CompAbilityEffect_SpawnEquipment abilityComp = pawn.GetAbilityComp<CompAbilityEffect_SpawnEquipment>(CosmereRosharDefs.whtwl_SummonShardblade.defName);
            handleSwordAbility(pawn, abilityComp);
            if (abilityComp == null) {
                abilityComp = pawn.GetAbilityComp<CompAbilityEffect_SpawnEquipment>(CosmereRosharDefs.whtwl_SummonShardblade.defName);
            }
            abilityComp.bladeObject = blade;
            isSpawned = isBladeSpawned;
        }


        private static void removeRadiantStuff(Pawn pawn) {
            if (pawn?.story?.traits == null) {
                Log.Error("[stormlight mod] Pawn has no traits system!");
                return;
            }
            Trait trait = StormlightUtilities.GetRadiantTrait(pawn);
            if (trait != null) {
                pawn.story.traits.allTraits.Remove(trait);
                Log.Message($"[stormlight mod] {pawn.Name} broke an oath and lost its Radiant trait.");

                CompStormlight comp = pawn.GetComp<CompStormlight>();
                if (comp != null) {
                    pawn.AllComps.Remove(comp);
                }

                CompGlower glowComp = pawn.GetComp<CompGlower>();
                if (glowComp != null) {
                    pawn.AllComps.Remove(glowComp);
                }
            }
            pawn.abilities.RemoveAbility(CosmereRosharDefs.whtwl_SummonShardblade);
            pawn.abilities.RemoveAbility(CosmereRosharDefs.whtwl_UnbondBlade);
        }


        public void severBond(Pawn pawn) {
            if (swordOwner == pawn) {
                removeRadiantStuff(pawn);
                swordOwner = null;
            }
        }
        public bool isBladeSpawned() {
            return isSpawned;
        }
        public void summon() {
            if (swordOwner == null || isSpawned == true) {
                return;
            }
            CompAbilityEffect_SpawnEquipment abilityComp = swordOwner.GetAbilityComp<CompAbilityEffect_SpawnEquipment>(CosmereRosharDefs.whtwl_SummonShardblade.defName);
            if (abilityComp != null && abilityComp.bladeObject != null) {
                swordOwner.equipment.AddEquipment(abilityComp.bladeObject);
                isSpawned = true;
            }
        }

        public void dismissBlade(Pawn pawn) {
            ThingWithComps droppedWeapon;
            bool success = pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out droppedWeapon, pawn.Position, forbid: false);
            isSpawned = false;
            //dismissal vs dropping is handled by harmony patch in ShardbladePatches.cs
        }

    }
}
namespace CosmereRoshar {
    public class CompProperties_Shardblade : CompProperties {
        public int bladesInExistence = 0;
        public CompProperties_Shardblade() {
            this.compClass = typeof(CompShardblade);
        }
    }
}

