using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using System.Security.Cryptography;
using UnityEngine;

namespace CosmereRoshar {
    public class CompStormlight : ThingComp {
        public bool isActivatedOnPawn = false;

        private float m_CurrentStormlight;
        public StormlightProperties Props => (StormlightProperties)props;
        public CompGlower GlowerComp => parent.GetComp<CompGlower>();
        public bool HasStormlight => m_CurrentStormlight > 0f;
        public float Stormlight => m_CurrentStormlight;
        public int StackCount => parent.stackCount;
        public float MaxStormlightPerItem => Props.maxInvestiture;
        public float CurrentMaxStormlight = 0f;
        public bool m_BreathStormlight = false;
        public float drainFactor = 1f;
        
        // Surges
        private bool m_AbrasionActive = false;
        public bool AbrasionActive => m_AbrasionActive;

        // Modifiers
        public float StormlightContainerSize = 1f;
        public float MaximumGlowRadius = 8.0f;

        private bool lightTurnedOn = false;
        private bool thisGlows = false;

        private QualityCategory quality => parent.TryGetComp<CompQuality>()?.Quality ?? QualityCategory.Normal;

        private float qualityMultiplier => quality switch {
            QualityCategory.Awful => .25f,
            QualityCategory.Poor => .5f,
            QualityCategory.Normal => 1,
            QualityCategory.Good => 1.2f,
            QualityCategory.Excellent => 1.6f,
            QualityCategory.Masterwork => 2f,
            QualityCategory.Legendary => 5f,
            _ => throw new ArgumentOutOfRangeException(),
        };

        public void toggleBreathStormlight() {
            m_BreathStormlight = !m_BreathStormlight;
        }

        public void toggleAbrasion() {
            m_AbrasionActive = !m_AbrasionActive;
        }

        public void RemoveAllStormlight() { m_CurrentStormlight = 0f; }

        // Called after loading or on spawn
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref m_AbrasionActive, "abrasionActive", false);
            Scribe_Values.Look(ref m_CurrentStormlight, "currentStormlight", 0f);
            Scribe_Values.Look(ref m_BreathStormlight, CosmereRosharDefs.whtwl_BreathStormlight.defName, false);
            Scribe_Values.Look(ref isActivatedOnPawn, "isActivatedOnPawn", false);
            Scribe_Values.Look(ref CurrentMaxStormlight, "CurrentMaxStormlight", 0f);
            Scribe_Values.Look(ref StormlightContainerSize, "StormlightContainerSize", 1f);
            // Scribe_Values.Look(ref quality, "StormlightContainerQuality", 1f);
            Scribe_Values.Look(ref MaximumGlowRadius, "MaximumGlowRadius", 1f);
            Scribe_Values.Look(ref drainFactor, "drainFactor", 1f);
        }

        private void adjustMaximumStormlight() {
            CurrentMaxStormlight = (MaxStormlightPerItem * StormlightContainerSize);
            CurrentMaxStormlight *= Math.Max(StackCount,1);
        }

      

        
        public override void CompTickInterval(int delta) {
            if (isActivatedOnPawn == false && this.parent is Pawn _pawn) {
                return;
            }
            if (!GenTicks.IsTickIntervalDelta(GenTicks.TicksPerRealSecond, delta)) return;
                base.CompTickInterval(delta);
                drainStormLight();
                if (parent is Pawn pawn && pawn.RaceProps.Humanlike) {
                    handleRadiantStuff(pawn);
                }
                adjustMaximumStormlight();
            handleGlow();
        }


        // This method adds additional text to the inspect pane.
        public override string CompInspectStringExtra() {
            if (isActivatedOnPawn == false && this.parent is Pawn _pawn) {
                return "";
            }
            return "Stormlight: " + m_CurrentStormlight.ToString("F0") + " / " + CurrentMaxStormlight.ToString("F0") + "\ntime remaining: " + getTimeRemaining();
        }
        public string getTimeRemaining() {
            
            float stormlightPerHour = (50.0f * this.GetDrainRate(this.drainFactor));
            if (Mathf.Approximately(stormlightPerHour, 0f)) {
                return "∞";
            }
            int hoursLeft = (int)(this.Stormlight / stormlightPerHour);
            int daysLeft = 0;
            if (hoursLeft > 24) {
                daysLeft = hoursLeft / 24;
            }
            hoursLeft %= 24;
            return daysLeft.ToString() + "d " + hoursLeft.ToString() + "h";
        }
        
        private void toggleGlow(bool on) {
            if (parent.Map != null) {
                if (on && thisGlows == false) {
                    parent.Map.glowGrid.DeRegisterGlower(GlowerComp);
                    parent.Map.glowGrid.RegisterGlower(GlowerComp);
                    GlowerComp.GlowRadius = MaximumGlowRadius;
                    thisGlows = true;
                }
                else if (!on && thisGlows) {
                    parent.Map.glowGrid.DeRegisterGlower(GlowerComp);
                    thisGlows = false;
                }
            }
        }
        public void handleGlow() {
            if (GlowerComp != null) {
                if (parent is Pawn pawn && pawn.Spawned) {
                    if (!CosmereRoshar.EnablePawnGlow) {
                        GlowerComp.Props.glowRadius = 0;
                        GlowerComp.Props.overlightRadius = 0;
                        parent.Map.glowGrid.DeRegisterGlower(GlowerComp);
                        toggleGlow(false);
                        return;
                    }
                }

                bool glowEnabled = m_CurrentStormlight > 0f;
                if (glowEnabled) {
                    GlowerComp.Props.glowRadius = MaximumGlowRadius;
                }
                toggleGlow(glowEnabled);
            }
        }

        public void calculateMaximumGlowRadius(int quality, int size) {
            // Normalize size (1, 5, 20) to range 1-10
            float normalizedSize = (size - 1f) / (20.0f - 1f) * 9f + 1f;

            MaximumGlowRadius = (float)Math.Round(normalizedSize, 2) + (quality / 5f);
            handleGlow();
        }

        private void radiantHeal(Pawn pawn) {

            // HEAL MISSING PARTS
            var missingParts = pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().OrderByDescending(h => h.Severity).ToList();
            foreach (var injury in missingParts) {
                float cost = 250f;  // More severe wounds cost more stormlight
                if (m_CurrentStormlight < cost)
                    break;
                pawn.health.hediffSet.hediffs.Remove(injury);
                m_CurrentStormlight -= cost;
                RadiantUtility.GiveRadiantXP(pawn, 20f);
            }


            // HEAL ADDICTIONS
            var addictions = pawn.health.hediffSet.hediffs.OfType<Hediff_Addiction>().OrderByDescending(h => h.Severity).ToList();
            foreach (var addiction in addictions) {
                float cost = 1000f;
                if (m_CurrentStormlight < cost)
                    break;
                pawn.health.hediffSet.hediffs.Remove(addiction);
                m_CurrentStormlight -= cost;
                RadiantUtility.GiveRadiantXP(pawn, 12f);
            }

            // HEAL INJURIES
            var injuries = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().OrderByDescending(h => h.Severity).ToList();
            foreach (var injury in injuries) {
                float cost = injury.Severity * 3f;  // More severe wounds cost more stormlight
                if (m_CurrentStormlight < cost)
                    break;

                injury.Heal(10.0f);
                m_CurrentStormlight -= cost;
                RadiantUtility.GiveRadiantXP(pawn, 0.5f);
            }
        }



        private void radiantAbsorbStormlight(Pawn pawn) {
            if (pawn == null || !pawn.Spawned || !pawn.RaceProps.Humanlike)
                return;

            float absorbAmount = 25f; // How much Stormlight is drawn per tick
            float maxDrawDistance = 3.0f; // Range to absorb from nearby spheres

            if ((MaxStormlightPerItem - Stormlight) < absorbAmount)
                absorbAmount = (MaxStormlightPerItem - Stormlight);

            if (m_BreathStormlight == false)
                return;

            if (infuseStormlight(0)) return; //check if full.

            //  Absorb Stormlight from pouch
            CompSpherePouch pouchComp = pawn.apparel?.WornApparel?.Find(a => a.GetComp<CompSpherePouch>() != null)?.GetComp<CompSpherePouch>();
            if (pouchComp != null && pouchComp.GetTotalStoredStormlight() > 0) {
                float drawn = pouchComp.DrawStormlight(absorbAmount);
                RadiantUtility.GiveRadiantXP(pawn, 0.1f);
                if (infuseStormlight(drawn)) return;
            }


            //  Absorb Stormlight from inventory
            if (pawn.inventory != null) {
                foreach (Thing item in pawn.inventory.innerContainer) {
                    ThingWithComps thingWithComps = item as ThingWithComps;
                    if (thingWithComps == null) continue; // Skip items without comps

                    CompStormlight sphereComp = thingWithComps.GetComp<CompStormlight>();
                    if (sphereComp != null && sphereComp.Stormlight > 0) {
                        float drawn = Math.Min(absorbAmount, sphereComp.Stormlight);
                        sphereComp.infuseStormlight(-drawn); // Remove from sphere
                        RadiantUtility.GiveRadiantXP(pawn, 0.1f);
                        if (infuseStormlight(drawn)) return;
                    }
                }
            }

            //  Absorb Stormlight from nearby spheres
            List<Thing> nearbyThings = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Everything);
            foreach (Thing thing in nearbyThings) {
                if (thing.def.defName.StartsWith("whtwl_Sphere_") && !thing.Position.Roofed(thing.Map)) {
                    var stormlightComp = thing.TryGetComp<CompStormlight>();
                    if (stormlightComp != null) {
                        float drawnLight = stormlightComp.drawStormlight(absorbAmount);
                        RadiantUtility.GiveRadiantXP(pawn, 0.1f);
                        if (infuseStormlight(drawnLight)) return;
                    }
                }
                else if ((thing.def.defName.StartsWith("Cut") || thing.def.defName.StartsWith("Raw")) && !thing.Position.Roofed(thing.Map)) {
                    var stormlightComp = thing.TryGetComp<CompStormlight>();
                    if (stormlightComp != null) {
                        float drawnLight = stormlightComp.drawStormlight(absorbAmount);
                        RadiantUtility.GiveRadiantXP(pawn, 0.1f);
                        if (infuseStormlight(drawnLight)) return;
                    }
                }
                else if (thing.def == CosmereRosharDefs.whtwl_Apparel_SpherePouch && !thing.Position.Roofed(thing.Map)) {
                    var pouch = thing.TryGetComp<CompSpherePouch>();
                    if (pouch != null && pouch.GetTotalStoredStormlight() > 0f) {
                        absorbAmount = Math.Min(absorbAmount, pouch.GetTotalStoredStormlight());
                        float drawnLight = pouch.DrawStormlight(absorbAmount);
                        RadiantUtility.GiveRadiantXP(pawn, 0.1f);
                        if (infuseStormlight(drawnLight)) return;
                    }
                }
                else if (thing.def == CosmereRosharDefs.whtwl_SphereLamp_Wall && !thing.Position.Roofed(thing.Map)) {
                    var lamp = thing.TryGetComp<StormlightLamps>();
                    if (lamp != null && lamp.m_CurrentStormlight > 0f) {
                        float drawnLight = lamp.DrawStormlight(absorbAmount);
                        RadiantUtility.GiveRadiantXP(pawn, 0.1f);
                        if (infuseStormlight(drawnLight)) return;
                    }
                }
            }
        }



        private void handleRadiantStuff(Pawn pawn) {
            radiantHeal(pawn);
            radiantAbsorbStormlight(pawn);
            handleSurges(pawn);
        }



        private void handleSurges(Pawn pawn) {
            if (m_CurrentStormlight <= 0f) {
                m_AbrasionActive = false;
            }
            if (m_AbrasionActive) {
                if (pawn != null && pawn.health.hediffSet.GetFirstHediffOfDef(CosmereRosharDefs.whtwl_surge_abrasion) == null) {
                    pawn.health.AddHediff(CosmereRosharDefs.whtwl_surge_abrasion);
                    drainFactor += 500f;
                }
            }
            else {
                if (pawn != null && pawn.health.hediffSet.GetFirstHediffOfDef(CosmereRosharDefs.whtwl_surge_abrasion) is Hediff hediff) {
                    pawn.health.RemoveHediff(hediff);
                }
                drainFactor = 1f;
            }
        }


        public float GetDrainRate(float factor) {
            return (Props.drainRate / qualityMultiplier) * factor;
        }

        public void drainStormLight() {
            if (m_CurrentStormlight > 0) {
                m_CurrentStormlight -= GetDrainRate(drainFactor);
                if (m_CurrentStormlight < 0)
                    m_CurrentStormlight = 0;
            }
        }

        public float drawStormlight(float amount) {
            float drawnAmount = amount;
            if (m_CurrentStormlight > 0) {
                m_CurrentStormlight -= amount;
                if (m_CurrentStormlight < 0) {
                    drawnAmount += m_CurrentStormlight;
                    m_CurrentStormlight = 0;
                }
            }
            else { drawnAmount = 0f; }
            return drawnAmount;
        }

        // Infuse from code when highstorm is active
        public bool infuseStormlight(float amount) {
            if (CurrentMaxStormlight <= 0f) { adjustMaximumStormlight(); }
            m_CurrentStormlight += amount;
            if (m_CurrentStormlight >= CurrentMaxStormlight) {
                m_CurrentStormlight = CurrentMaxStormlight;
                return true;
            }
            return false;
        }
    }
}

namespace CosmereRoshar {
    public class StormlightProperties : CompProperties {
        public float maxInvestiture;
        public float drainRate;

        public StormlightProperties() {
            this.compClass = typeof(CompStormlight);
        }
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef) {
            foreach (string configError in base.ConfigErrors(parentDef)) {
                yield return configError;
            }

            if (maxInvestiture <= 0) {
                yield return "maxInvestiture must be greater than 0.";
            }
        }
    }
}
