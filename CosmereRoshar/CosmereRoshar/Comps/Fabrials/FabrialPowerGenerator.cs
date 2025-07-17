using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using UnityEngine;
using Verse.AI;
using LudeonTK;
using System.Diagnostics.Eventing.Reader;

namespace CosmereRoshar {

    public class Building_Fabrial_Power_Generator : Building {
        public CompFabrialPowerGenerator compFabrialPowerGenerator;
        public CompFlickable compFlickerable;
        public CompGlower compGlower;
        public CompPowerPlant compPowerPlant;


        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            compFabrialPowerGenerator = GetComp<CompFabrialPowerGenerator>();
            compFlickerable = GetComp<CompFlickable>();
            compGlower = GetComp<CompGlower>();
            compPowerPlant = GetComp<CompPowerPlant>();
        }
        protected override void Tick() {
            compFabrialPowerGenerator.checkPower(compFlickerable.SwitchIsOn);
            if (!compFabrialPowerGenerator.PowerOn) { compPowerPlant.PowerOutput = 0f; }
            else { compPowerPlant.PowerOutput =  1000f; } //maybe add quality of stone affects power output later
            toggleGlow(compFabrialPowerGenerator.PowerOn);
            compFabrialPowerGenerator.usePower(); 
        }
        private void toggleGlow(bool on) {
            if (this.Map != null) {
                if (on) {
                    var stormlightComp = (compFabrialPowerGenerator.insertedGemstone as ThingWithComps).GetComp<CompStormlight>();
                    compGlower.GlowRadius = stormlightComp.MaximumGlowRadius;
                    compGlower.GlowColor = stormlightComp.GlowerComp.GlowColor;
                    this.Map.glowGrid.RegisterGlower(compGlower);
                }
                else {
                    this.Map.glowGrid.DeRegisterGlower(compGlower);
                }
            }
        }
    }

    public class CompFabrialPowerGenerator : ThingComp, IGemstoneHandler, IFilterableComp {
        public CompProperties_FabrialPowerGenerator Props => (CompProperties_FabrialPowerGenerator)props;
        public CompGlower GlowerComp => parent.GetComp<CompGlower>();
        public Thing insertedGemstone = null;
        public bool PowerOn = false;
        public bool HasGemstone => insertedGemstone != null;

        private List<ThingDef> filterList = new List<ThingDef>();
        public List<ThingDef> FilterList => filterList;
        private List<ThingDef> allowedGems = new List<ThingDef>() {
            CosmereRosharDefs.whtwl_CutDiamond,
            CosmereRosharDefs.whtwl_CutGarnet,
            CosmereRosharDefs.whtwl_CutRuby,
            CosmereRosharDefs.whtwl_CutSapphire,
            CosmereRosharDefs.whtwl_CutEmerald
        };
        public List<ThingDef> AllowedSpheres => this.allowedGems;

        private List<GemSize> sizeFilterList = new List<GemSize>()
        {
            GemSize.Chip,
            GemSize.Mark,
            GemSize.Broam
        };
        public List<GemSize> SizeFilterList => sizeFilterList;

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            if (insertedGemstone != null) {
            }
        }

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
            Scribe_Values.Look(ref PowerOn, "PowerOn");
            Scribe_Collections.Look(ref sizeFilterList, "sizeFilterList", LookMode.Value);
            Scribe_Collections.Look(ref filterList, "filterList", LookMode.Def);
        }

        public void checkPower(bool flickeredOn) {
            if (insertedGemstone != null) {
                var stormlightComp = (insertedGemstone as ThingWithComps).GetComp<CompStormlight>();
                if (stormlightComp != null) {
                    PowerOn = stormlightComp.HasStormlight && flickeredOn;
                    return;
                }
            }
            PowerOn = false;
        }


        public void usePower() {
            if (insertedGemstone != null && insertedGemstone.TryGetComp<CompStormlight>() is CompStormlight stormlightComp) {
                if (PowerOn) { stormlightComp.drainFactor = 4f; }
                else { stormlightComp.drainFactor = 1f; }
                stormlightComp.CompTick();
            }
        }

        public void AddGemstone(ThingWithComps gemstone) {
            var gemstoneComp = gemstone.GetComp<CompCutGemstone>();
            if (gemstoneComp != null) {
                insertedGemstone = gemstoneComp.parent;
            }
        }

        public void RemoveGemstone() {
            if (insertedGemstone != null) {
                var gemstoneToDrop = insertedGemstone;
                insertedGemstone = null;
                IntVec3 dropPosition = parent.Position;
                dropPosition.z -= 1;
                GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, parent.Map, ThingPlaceMode.Near);
            }
        }
        public override string CompInspectStringExtra() {
            string gemName = "No gem in fabrial.";

            if (insertedGemstone != null) {
                ThingWithComps gemstone = insertedGemstone as ThingWithComps;
                gemName = "Spren: " + gemstone.GetComp<CompCutGemstone>().capturedSpren.ToString() + "\nStormlight: " + gemstone.GetComp<CompStormlight>().Stormlight.ToString("F0") + "\ntime remaining: " + getTimeRemaining();
            }
            return gemName;
        }

        public string getTimeRemaining() {
            var stormlightComp = insertedGemstone.TryGetComp<CompStormlight>();
            float stormlightPerHour = (50.0f * stormlightComp.GetDrainRate(stormlightComp.drainFactor));
            if (Mathf.Approximately(stormlightPerHour, 0f)) {
                return "∞";
            }
            int hoursLeft = (int)(stormlightComp.Stormlight / stormlightPerHour);
            int daysLeft = 0;
            if (hoursLeft > 24) {
                daysLeft = hoursLeft / 24;
            }
            hoursLeft %= 24;
            return daysLeft.ToString() + "d " + hoursLeft.ToString() + "h";
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {

            var cutGemstone = GenClosest.ClosestThing_Global(
                  selPawn.Position,
                  selPawn.Map.listerThings.AllThings.Where(thing => (StormlightUtilities.isThingCutGemstone(thing))
                  && (thing.TryGetComp<CompStormlight>().HasStormlight)
                  && FilterList.Contains(thing.def)
                  && SizeFilterList.Contains(thing.TryGetComp<CompCutGemstone>()?.GetGemSize() ?? GemSize.None)
                  ), 500f);


            Action replaceGemAction = null;
            string replaceGemText = "No suitable gem available";
            if (cutGemstone != null) {

                replaceGemAction = () => {
                    Job job = JobMaker.MakeJob(CosmereRosharDefs.whtwl_RefuelFabrial, parent, cutGemstone);
                    if (job.TryMakePreToilReservations(selPawn, errorOnFailed: true)) {
                        selPawn.jobs.TryTakeOrderedJob(job);
                    }
                };
                replaceGemText = $"Replace with {cutGemstone.Label}";
            }
            yield return new FloatMenuOption(replaceGemText, replaceGemAction);


            Action removeGemAction = null;
            string removeGemText = "Remove Gemstone";
            if (insertedGemstone != null) {

                removeGemAction = () => {
                    Job job = JobMaker.MakeJob(CosmereRosharDefs.whtwl_RemoveFromFabrial, parent);
                    if (job.TryMakePreToilReservations(selPawn, errorOnFailed: true)) {
                        selPawn.jobs.TryTakeOrderedJob(job);
                    }
                };
            }
            yield return new FloatMenuOption(removeGemText, removeGemAction);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra()) {
                yield return gizmo;
            }

            yield return new Command_Action {
                defaultLabel = "Set Gem Filters",
                defaultDesc = "Click to choose which gems are allowed in this fabrial.",
                icon = TexCommand.SelectShelf,
                action = () => {
                    Find.WindowStack.Add(new Dialog_SphereFilter<CompFabrialPowerGenerator>(this));
                }
            };
        }

    }

    public class CompProperties_FabrialPowerGenerator : CompProperties {
        public CompProperties_FabrialPowerGenerator() {
            this.compClass = typeof(CompFabrialPowerGenerator);
        }
    }
}
