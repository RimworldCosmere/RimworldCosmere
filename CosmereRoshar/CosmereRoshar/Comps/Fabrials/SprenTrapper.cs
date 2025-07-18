using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using UnityEngine;
using Verse.AI;
using Verse.Noise;

namespace CosmereRoshar {



    public class Building_SprenTrapper : Building {
        public CompSprenTrapper compTrapper;
        public CompGlower compGlower;
        public Dictionary<string, List<Spren>> gemstonePossibleSprenDict = new Dictionary<string, List<Spren>>() { };
        SectionLayer layerTest = null;


        public override void Destroy(DestroyMode mode = DestroyMode.Vanish) {
            base.Destroy(mode);
            if (compTrapper != null) {
                compTrapper.RemoveGemstone();
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            compTrapper = GetComp<CompSprenTrapper>();
            compGlower = GetComp<CompGlower>();
            gemstonePossibleSprenDict.Add(CosmereResources.ThingDefOf.CutDiamond.defName, new List<Spren> { Spren.Logic, Spren.Light, Spren.Exhaustion });
            gemstonePossibleSprenDict.Add(CosmereResources.ThingDefOf.CutGarnet.defName, new List<Spren> { Spren.Rain, Spren.Exhaustion, Spren.Pain });
            gemstonePossibleSprenDict.Add(CosmereResources.ThingDefOf.CutSapphire.defName, new List<Spren> { Spren.Wind, Spren.Motion, Spren.Cold });
            gemstonePossibleSprenDict.Add(CosmereResources.ThingDefOf.CutRuby.defName, new List<Spren> { Spren.Flame, Spren.Anger });
            gemstonePossibleSprenDict.Add(CosmereResources.ThingDefOf.CutEmerald.defName, new List<Spren> { Spren.Flame, Spren.Life, Spren.Cultivation, Spren.Rain, Spren.Glory });
        }

        public override void TickRare() {
            CaptureLoop();
            toggleGlow();
            TriggerPrint();
            compTrapper.DrainStormlight();
        }

        private void toggleGlow() {
            if (this.Map != null) {
                if (compTrapper.HasGemstone) {
                    if (!compTrapper.sprenCaptured && !compTrapper.insertedGemstone.TryGetComp<CompStormlight>().HasStormlight) { compGlower.GlowColor = ColorInt.FromHdrColor(Color.red); }
                    else if (compTrapper.sprenCaptured) { compGlower.GlowColor = ColorInt.FromHdrColor(Color.green); }
                    else  { compGlower.GlowColor = ColorInt.FromHdrColor(Color.blue); }
                    this.Map.glowGrid.RegisterGlower(compGlower);
                }
                else {
                    this.Map.glowGrid.DeRegisterGlower(compGlower);
                }
            }
        }

        private void CaptureLoop() {
            if (compTrapper.insertedGemstone != null && compTrapper.sprenCaptured == false) {
                List<Spren> sprenList;
                gemstonePossibleSprenDict.TryGetValue(compTrapper.insertedGemstone.def.defName, out sprenList);
                foreach (Spren spren in sprenList) {
                    compTrapper.tryCaptureSpren(spren);
                    compTrapper.checkTrapperState();
                    if (compTrapper.sprenCaptured) break;
                }
            }
            compTrapper.checkTrapperState();
        }

        //Add light system for trapped, empty, e.g.
        public override void Print(SectionLayer layer) {
            layerTest = layer;
            if (compTrapper.HasGemstone) {
                if (!compTrapper.sprenCaptured && !compTrapper.insertedGemstone.TryGetComp<CompStormlight>().HasStormlight) { this.def.graphicData.attachments[0].Graphic.Print(layer, this, 0f); }
                else if (!compTrapper.sprenCaptured) { this.def.graphicData.attachments[1].Graphic.Print(layer, this, 0f); }
                else { this.def.graphicData.attachments[2].Graphic.Print(layer, this, 0f); }
            }
            else { base.Print(layer); }
        }

        public void TriggerPrint() { if (layerTest != null) Print(layerTest); else { Log.Message("Layer was null"); } }

    }



    public class CompSprenTrapper : ThingComp, IGemstoneHandler, IFilterableComp {
        public CompProperties_SprenTrapper Props => (CompProperties_SprenTrapper)props;
        public Thing insertedGemstone = null;
        public bool HasGemstone => insertedGemstone != null;
        public bool sprenCaptured = false;

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


        private float stormlightThresholdForRefuel = 0f;
        public float StormlightThresholdForRefuel {
            get { return stormlightThresholdForRefuel; }
            set { stormlightThresholdForRefuel = value; }
        }





        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
        }


        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
            Scribe_Values.Look(ref sprenCaptured, "sprenCaptured");
            Scribe_Collections.Look(ref sizeFilterList, "sizeFilterList", LookMode.Value);
            Scribe_Collections.Look(ref filterList, "filterList", LookMode.Def);
        }
        public void tryCaptureSpren(Spren targetSpren) {

            if (insertedGemstone == null || targetSpren == Spren.None || sprenCaptured == true) return;
            switch (targetSpren) {
                case Spren.Flame:
                    tryCaptureFlameSpren();
                    break;
                case Spren.Cold:
                    tryCaptureColdSpren();
                    break;
                case Spren.Pain:
                    tryCapturePainSpren();
                    break;
                case Spren.Life:
                    tryCaptureLifeSpren();
                    break;
                case Spren.Logic:
                    tryCaptureLogicSpren();
                    break;
                default:
                    break;
            }
        }
        private void displayCaptureMessage(Spren spren) {
            (parent as Building_SprenTrapper).TriggerPrint();
            Messages.Message($"One of your traps captured a {spren.ToStringSafe()}spren!", parent, MessageTypeDefOf.PositiveEvent);
        }

        private float getProbability(CompStormlight stormlightComp, float probabilityFactor, float baseMaxNormalizedValue) {

            float baseProbability = StormlightUtilities.SprenBaseCaptureProbability(stormlightComp.Stormlight, 0f, stormlightComp.CurrentMaxStormlight);
            float probability = StormlightUtilities.Normalize(baseProbability, 0f, 100f, 0f, baseMaxNormalizedValue);
            return probability * probabilityFactor;
        }

        private void tryCaptureFlameSpren() {

            if (StormlightUtilities.IsAnyFireNearby(parent as Building, 5f)) {
                var stormlightcomp = insertedGemstone.TryGetComp<CompStormlight>();
                if (stormlightcomp != null) {
                    float probabilityFactor = (float)StormlightUtilities.GetNumberOfFiresNearby(parent as Building, 3f);
                    float probability = getProbability(stormlightcomp, probabilityFactor, 0.1f);

                    if (Rand.Chance(probability)) {
                        insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Flame;
                        stormlightcomp.RemoveAllStormlight();
                        displayCaptureMessage(Spren.Flame);
                    }
                }
            }

        }

        private void tryCaptureColdSpren() {
            float averageSuroundingTemperature = StormlightUtilities.GetAverageSuroundingTemperature(parent as Building, 5f);
            if (averageSuroundingTemperature < 0f) {
                var stormlightcomp = insertedGemstone.TryGetComp<CompStormlight>();
                if (stormlightcomp != null) {
                    float probabilityFactor = Mathf.Abs(averageSuroundingTemperature);
                    float probability = getProbability(stormlightcomp, probabilityFactor, 0.01f);

                    if (Rand.Chance(probability)) {
                        insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Cold;
                        displayCaptureMessage(Spren.Cold);
                        stormlightcomp.RemoveAllStormlight();
                    }
                }
            }
        }



        private void tryCapturePainSpren() {
            float sumOfPain = StormlightUtilities.GetSuroundingPain(parent as Building, 5f);
            if (sumOfPain > 0f) {
                var stormlightcomp = insertedGemstone.TryGetComp<CompStormlight>();
                if (stormlightcomp != null) {
                    float probabilityFactor = sumOfPain * 5;
                    float probability = getProbability(stormlightcomp, probabilityFactor, 0.01f);

                    if (Rand.Chance(probability)) {
                        insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Pain;
                        displayCaptureMessage(Spren.Pain);
                        stormlightcomp.RemoveAllStormlight();
                    }
                }
            }
        }

        private void tryCaptureLifeSpren() {
            float sumOfPlants = StormlightUtilities.GetSuroundingPlants(parent as Building, 5f);
            if (sumOfPlants > 0) {
                var stormlightcomp = insertedGemstone.TryGetComp<CompStormlight>();
                if (stormlightcomp != null) {
                    float probabilityFactor = sumOfPlants;
                    float probability = getProbability(stormlightcomp, probabilityFactor, 0.01f);

                    if (Rand.Chance(probability)) {
                        insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Life;
                        displayCaptureMessage(Spren.Life);
                        stormlightcomp.RemoveAllStormlight();
                    }
                }
            }
        }

        private void tryCaptureLogicSpren() {
            bool researchDoneNearby = StormlightUtilities.ResearchBeingDoneNearby(parent as Building, 5f);
            if (researchDoneNearby) {
                var stormlightcomp = insertedGemstone.TryGetComp<CompStormlight>();
                if (stormlightcomp != null) {
                    float probabilityFactor = 2f;
                    float probability = getProbability(stormlightcomp, probabilityFactor, 0.0005f);
                    if (Rand.Chance(probability)) {
                        insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Logic;
                        displayCaptureMessage(Spren.Logic);
                        stormlightcomp.RemoveAllStormlight();
                    }
                }
            }
        }


        public void checkTrapperState() {
            if (insertedGemstone != null && insertedGemstone.TryGetComp<CompCutGemstone>() is CompCutGemstone compCutGemstone) {
                if (compCutGemstone.capturedSpren != Spren.None) {
                    sprenCaptured = true;
                    return;
                }
            }
            sprenCaptured = false;
        }

        public void DrainStormlight() {
            if (insertedGemstone != null) {
                var stormlightcomp = insertedGemstone.TryGetComp<CompStormlight>();
                if (stormlightcomp != null) {
                    stormlightcomp.drainStormLight(); 
                    stormlightcomp.drainStormLight(); 
                    stormlightcomp.drainStormLight(); 
                    stormlightcomp.drainStormLight(); 
                    stormlightcomp.drainStormLight(); 
                }
            }
        }
        public void InstallCage(ThingWithComps cage) {
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
                var map = parent.Map;

                if (map == null) {
                    map = Find.CurrentMap;
                }
                if (map != null) {
                    GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, map, ThingPlaceMode.Near);
                }
            }
        }



        public override string CompInspectStringExtra() {
            string gemName = "No gem in fabrial.";

            if (insertedGemstone != null) {
                ThingWithComps gemstone = insertedGemstone as ThingWithComps;
                gemName = gemstone.GetComp<CompCutGemstone>().GetFullLabel + "(" + gemstone.GetComp<CompStormlight>().Stormlight.ToString("F0") + ")";
            }
            return gemName;
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {

            var cutGemstone = GenClosest.ClosestThing_Global(
                   selPawn.Position,
                   selPawn.Map.listerThings.AllThings.Where(thing => (StormlightUtilities.isThingCutGemstone(thing))
                   && (thing.TryGetComp<CompCutGemstone>().HasSprenInside == false)
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
                defaultDesc = "Click to choose which gems are allowed in this trap.",
                //icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
                icon = TexCommand.SelectShelf,
                action = () => {
                    Find.WindowStack.Add(new Dialog_SphereFilter<CompSprenTrapper>(this));
                }
            };
        }
    }

    public class CompProperties_SprenTrapper : CompProperties {
        public CompProperties_SprenTrapper() {
            this.compClass = typeof(CompSprenTrapper);
        }
    }

}
