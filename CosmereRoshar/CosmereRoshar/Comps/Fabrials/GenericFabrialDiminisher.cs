using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using UnityEngine;
using Verse.AI;
using LudeonTK;
using Verse.Noise;

namespace CosmereRoshar {

    public class Building_Fabrial_Basic_Diminisher : Building {
        public CompBasicFabrialDiminisher compBasicFabrialDiminisher;
        public CompFlickable compFlickerable;
        public CompGlower compGlower;


        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            compBasicFabrialDiminisher = GetComp<CompBasicFabrialDiminisher>();
            compFlickerable = GetComp<CompFlickable>();
            compGlower = GetComp<CompGlower>();
        }
        protected override void Tick() {
            compBasicFabrialDiminisher.checkPower(compFlickerable.SwitchIsOn);
            toggleGlow(compBasicFabrialDiminisher.PowerOn);
            compBasicFabrialDiminisher.usePower();
        }
        private void toggleGlow(bool on) {
            if (this.Map != null) {
                if (on) {
                    var stormlightComp = (compBasicFabrialDiminisher.insertedGemstone as ThingWithComps).GetComp<CompStormlight>();
                    compGlower.GlowRadius = stormlightComp.MaximumGlowRadius;
                    compGlower.GlowColor = stormlightComp.GlowerComp.GlowColor;
                    this.Map.glowGrid.RegisterGlower(compGlower);
                }
                else {
                    this.Map.glowGrid.DeRegisterGlower(compGlower);
                }
            }
        }

        public override void Print(SectionLayer layer) {
            base.Print(layer);
            if (compBasicFabrialDiminisher.HasGemstone) {
                if (compBasicFabrialDiminisher.insertedGemstone.def ==      CosmereResources.ThingDefOf.CutRuby) { this.def.graphicData.attachments[0].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialDiminisher.insertedGemstone.def == CosmereResources.ThingDefOf.CutDiamond) { this.def.graphicData.attachments[1].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialDiminisher.insertedGemstone.def == CosmereResources.ThingDefOf.CutGarnet) { this.def.graphicData.attachments[2].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialDiminisher.insertedGemstone.def == CosmereResources.ThingDefOf.CutEmerald) { this.def.graphicData.attachments[3].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialDiminisher.insertedGemstone.def == CosmereResources.ThingDefOf.CutSapphire) { this.def.graphicData.attachments[4].Graphic.Print(layer, this, 0f); }
            }
        }
    }

    public class CompBasicFabrialDiminisher : ThingComp, IGemstoneHandler, IFilterableComp {
        public CompProperties_BasicFabrialDiminisher Props => (CompProperties_BasicFabrialDiminisher)props;
        public CompGlower GlowerComp => parent.GetComp<CompGlower>();
        public Thing insertedGemstone = null;
        public bool PowerOn = false;
        public bool HasGemstone => insertedGemstone != null;
        private float TempWhenTurnedOn = 0f;

        private List<ThingDef> filterList = new List<ThingDef>();
        public List<ThingDef> FilterList => filterList;
        private List<ThingDef> allowedGems = new List<ThingDef>() {
            CosmereResources.ThingDefOf.CutDiamond,
            CosmereResources.ThingDefOf.CutGarnet,
            CosmereResources.ThingDefOf.CutRuby,
            CosmereResources.ThingDefOf.CutSapphire,
            CosmereResources.ThingDefOf.CutEmerald
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
                CultivationSprenPatch.RegisterBuilding(this.parent as Building_Fabrial_Basic_Diminisher);
            }
        }

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
            Scribe_Values.Look(ref PowerOn, "PowerOn");
            Scribe_Values.Look(ref TempWhenTurnedOn, "TempWhenTurnedOn", 0f);
            Scribe_Collections.Look(ref sizeFilterList, "sizeFilterList", LookMode.Value);
            Scribe_Collections.Look(ref filterList, "filterList", LookMode.Def);
        }

        public void checkPower(bool flickeredOn) {
            if (insertedGemstone != null) {
                var stormlightComp = (insertedGemstone as ThingWithComps).GetComp<CompStormlight>();
                if (stormlightComp != null) {
                    bool power = stormlightComp.HasStormlight && flickeredOn;
                    if (!PowerOn && power) {
                        TempWhenTurnedOn = parent.GetRoom().Temperature;
                    }
                    PowerOn = power;
                    return;
                }
            }
            PowerOn = false;
        }

        public Spren CurrentSpren {
            get {
                if (HasGemstone) {
                    return insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren;
                }
                return Spren.None;
            }
        }

        public void usePower() {
            if (PowerOn && insertedGemstone != null && insertedGemstone.TryGetComp<CompStormlight>() is CompStormlight stormlightComp) {
                stormlightComp.drainFactor = 2f;
                stormlightComp.CompTick();
                switch (insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren) {
                    case Spren.Flame:
                        doFlameSprenPower();
                        break;
                    case Spren.Cold:
                        doColdSprenPower();
                        break;
                    case Spren.Pain:
                        doPainSprenPower();
                        break;
                    case Spren.Logic:        //diamond
                        doLogicSprenPower();
                        break;
                    case Spren.Life:  //emerald
                        //Handled by patch
                        break;
                    default:
                        break;

                }
            }
        }


        private void doFlameSprenPower() {
            if (PowerOn && parent.IsOutside() == false) { 
                int gemstoneSize = insertedGemstone.TryGetComp<CompCutGemstone>().gemstoneSize * 3; //3, 15, 60
                float targetTemp = TempWhenTurnedOn;
                float currentTemp = parent.GetRoom().Temperature;
                if (currentTemp > TempWhenTurnedOn) {
                    GenTemperature.PushHeat(parent.Position, parent.Map, (0f - gemstoneSize));
                }
            }
        }

        private void doColdSprenPower() {
            if (PowerOn && parent.IsOutside() == false) {
                int gemstoneSize = insertedGemstone.TryGetComp<CompCutGemstone>().gemstoneSize * 3; //3, 15, 60
                float targetTemp = TempWhenTurnedOn;
                float currentTemp = parent.GetRoom().Temperature;
                if (currentTemp < targetTemp) {
                    GenTemperature.PushHeat(parent.Position, parent.Map, gemstoneSize);
                }
            }
        }

        private void doPainSprenPower() {
            if (PowerOn) {
                IntVec3 position = parent.Position;
                Map map = parent.Map;
                var cells = GenRadial.RadialCellsAround(position, 5f, true);
                foreach (IntVec3 cell in cells) {
                    Pawn pawn = cell.GetFirstPawn(map);
                    if (pawn != null && pawn.health.hediffSet.GetFirstHediffOfDef(CosmereRosharDefs.whtwl_painrial_diminisher) == null && pawn.Position.InHorDistOf(position, 5f)) {
                        pawn.health.AddHediff(CosmereRosharDefs.whtwl_painrial_diminisher);
                    }
                }
            }
        }

        private void doLogicSprenPower() {
            if (PowerOn) {
                IntVec3 position = parent.Position;
                Map map = parent.Map;
                var cells = GenRadial.RadialCellsAround(position, 5f, true);
                foreach (IntVec3 cell in cells) {
                    foreach (Thing thing in cell.GetThingList(map)) {
                        Pawn pawn = cell.GetFirstPawn(map);
                        if (pawn != null && pawn.health.hediffSet.GetFirstHediffOfDef(CosmereRosharDefs.whtwl_logirial_diminisher) == null && pawn.Position.InHorDistOf(position, 5f)) {
                            pawn.health.AddHediff(CosmereRosharDefs.whtwl_logirial_diminisher);
                        }
                    }
                }
            }
        }
        public void AddGemstone(ThingWithComps gemstone) {
            var gemstoneComp = gemstone.GetComp<CompCutGemstone>();
            if (gemstoneComp != null) {
                insertedGemstone = gemstoneComp.parent;
                CultivationSprenPatch.RegisterBuilding(this.parent as Building_Fabrial_Basic_Diminisher);
            }
        }

        public void RemoveGemstone() {
            if (insertedGemstone != null) {
                var gemstoneToDrop = insertedGemstone;
                insertedGemstone = null;
                IntVec3 dropPosition = parent.Position;
                dropPosition.z -= 1;
                GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, parent.Map, ThingPlaceMode.Near);
                CultivationSprenPatch.UnregisterBuilding(this.parent as Building_Fabrial_Basic_Diminisher);
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
            float stormlightPerHour = (50.0f * stormlightComp.GetDrainRate(2f));
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
                && (thing.TryGetComp<CompCutGemstone>().HasSprenInside)
                && (thing.TryGetComp<CompStormlight>().HasStormlight)
                && FilterList.Contains(thing.def)
                && SizeFilterList.Contains(thing.TryGetComp<CompCutGemstone>()?.GetGemSize() ?? GemSize.None)
                ), 500f);

            //var cutGemstone = GenClosest.ClosestThing_Global(
            //       selPawn.Position,
            //       selPawn.Map.listerThings.AllThings.Where(
            //           thing => (StormlightUtilities.isThingCutGemstone(thing)) && (thing.TryGetComp<CompCutGemstone>().HasSprenInside && thing.TryGetComp<CompStormlight>().HasStormlight)), 500f);

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
                //icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
                icon = TexCommand.SelectShelf,
                action = () => {
                    Find.WindowStack.Add(new Dialog_SphereFilter<CompBasicFabrialDiminisher>(this));
                }
            };
        }
    }

    public class CompProperties_BasicFabrialDiminisher : CompProperties {
        public CompProperties_BasicFabrialDiminisher() {
            this.compClass = typeof(CompBasicFabrialDiminisher);
        }
    }
}
