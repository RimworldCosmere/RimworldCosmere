using System;
using System.Collections.Generic;
using System.Linq;
using CosmereRoshar.Comps.Apparel;
using CosmereRoshar.Comps.Furniture;
using CosmereRoshar.Need;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereRoshar.Comp.Thing {
    public class StormlightProperties : CompProperties {
        public float drainRate;
        public float maxInvestiture;

        public StormlightProperties() {
            compClass = typeof(Stormlight);
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
    
    public class Stormlight : ThingComp {
        public float currentMaxStormlight;
        public float drainFactor = 1f;
        public bool isActivatedOnPawn;

        private bool lightTurnedOn = false;

        // Surges
        private bool abrasionActiveInt;
        private bool breathStormlightInt;
        public bool breathStormlight => breathStormlight;

        private float currentStormlightInt;
        public float currentStormlight => currentStormlightInt;
        public float maximumGlowRadius = 8.0f;

        // Modifiers
        public float stormlightContainerSize = 1f;
        private bool thisGlows;

        private new StormlightProperties props => (StormlightProperties)base.props;
        public CompGlower? glowerComp => parent.TryGetComp<CompGlower>();
        public bool hasStormlight => currentStormlightInt > 0f;

        private int stackCount => parent.stackCount;
        public float maxStormlightPerItem => props.maxInvestiture;
        public bool abrasionActive => abrasionActiveInt;

        private Pawn? pawn => parent as Pawn;
        
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

        public void ToggleBreathStormlight() {
            breathStormlightInt = !breathStormlightInt;
        }

        public void ToggleAbrasion() {
            abrasionActiveInt = !abrasionActiveInt;
        }

        public void RemoveAllStormlight() {
            currentStormlightInt = 0f;
        }

        // Called after loading or on spawn
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref abrasionActiveInt, "abrasionActiveInt");
            Scribe_Values.Look(ref currentStormlightInt, "currentStormlightInt");
            Scribe_Values.Look(ref breathStormlightInt, CosmereRosharDefs.WhtwlBreathStormlight.defName);
            Scribe_Values.Look(ref isActivatedOnPawn, "isActivatedOnPawn");
            Scribe_Values.Look(ref currentMaxStormlight, "CurrentMaxStormlight");
            Scribe_Values.Look(ref stormlightContainerSize, "StormlightContainerSize", 1f);
            // Scribe_Values.Look(ref quality, "StormlightContainerQuality", 1f);
            Scribe_Values.Look(ref maximumGlowRadius, "MaximumGlowRadius", 1f);
            Scribe_Values.Look(ref drainFactor, "drainFactor", 1f);
        }

        private void AdjustMaximumStormlight() {
            currentMaxStormlight = maxStormlightPerItem * stormlightContainerSize;
            currentMaxStormlight *= Math.Max(stackCount, 1);
        }


        public override void CompTickInterval(int delta) {
            if (isActivatedOnPawn == false && pawn is not null) {
                return;
            }

            if (!GenTicks.IsTickIntervalDelta(GenTicks.TicksPerRealSecond, delta)) return;
            base.CompTickInterval(delta);
            DrainStormLight();
            if (pawn is not null && pawn.RaceProps.Humanlike) {
                HandleRadiantStuff();
            }

            AdjustMaximumStormlight();
            HandleGlow();
        }


        private void HandleRadiantStuff() {
            RadiantHeal();
            RadiantAbsorbStormlight();
            HandleSurges();
        }


        // This method adds additional text to the inspect pane.
        public override string CompInspectStringExtra() {
            if (isActivatedOnPawn == false && parent is Pawn pawn) {
                return "";
            }

            return "Stormlight: " +
                   currentStormlightInt.ToString("F0") +
                   " / " +
                   currentMaxStormlight.ToString("F0") +
                   "\ntime remaining: " +
                   GetTimeRemaining();
        }

        private string GetTimeRemaining() {
            float stormlightPerHour = 50.0f * GetDrainRate(drainFactor);
            if (Mathf.Approximately(stormlightPerHour, 0f)) {
                return "∞";
            }

            int hoursLeft = (int)(currentStormlightInt / stormlightPerHour);
            int daysLeft = 0;
            if (hoursLeft > 24) {
                daysLeft = hoursLeft / 24;
            }

            hoursLeft %= 24;
            return daysLeft + "d " + hoursLeft + "h";
        }

        private void ToggleGlow(bool on) {
            if (parent.Map == null) return;
            
            if (on && thisGlows == false) {
                parent.Map.glowGrid.DeRegisterGlower(glowerComp);
                parent.Map.glowGrid.RegisterGlower(glowerComp);
                glowerComp.GlowRadius = maximumGlowRadius;
                thisGlows = true;
            } else if (!on && thisGlows) {
                parent.Map.glowGrid.DeRegisterGlower(glowerComp);
                thisGlows = false;
            }
        }

        private void HandleGlow() {
            if (glowerComp == null) return;
            
            if (pawn?.Spawned ?? false) {
                if (!CosmereRoshar.enablePawnGlow) {
                    glowerComp.Props.glowRadius = 0;
                    glowerComp.Props.overlightRadius = 0;
                    parent.Map.glowGrid.DeRegisterGlower(glowerComp);
                    ToggleGlow(false);
                    return;
                }
            }

            bool glowEnabled = currentStormlightInt > 0f;
            if (glowEnabled) {
                glowerComp.Props.glowRadius = maximumGlowRadius;
            }

            ToggleGlow(glowEnabled);
        }

        public void CalculateMaximumGlowRadius(int quality, int size) {
            // Normalize size (1, 5, 20) to range 1-10
            float normalizedSize = (size - 1f) / (20.0f - 1f) * 9f + 1f;

            maximumGlowRadius = (float)Math.Round(normalizedSize, 2) + quality / 5f;
            HandleGlow();
        }

        private void RadiantHeal() {
            if (pawn is null) return;
            
            // HEAL MISSING PARTS
            List<Hediff_MissingPart> missingParts = pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>()
                .OrderByDescending(h => h.Severity)
                .ToList();
            foreach (Hediff_MissingPart? injury in missingParts) {
                const float cost = 250f; // More severe wounds cost more stormlight
                if (currentStormlightInt < cost) {
                    break;
                }

                pawn.health.hediffSet.hediffs.Remove(injury);
                currentStormlightInt -= cost;
                RadiantUtility.GiveRadiantXp(pawn, 20f);
            }


            // HEAL ADDICTIONS
            List<Hediff_Addiction> addictions = pawn.health.hediffSet.hediffs.OfType<Hediff_Addiction>()
                .OrderByDescending(h => h.Severity)
                .ToList();
            foreach (Hediff_Addiction? addiction in addictions) {
                const float cost = 1000f;
                if (currentStormlightInt < cost) {
                    break;
                }

                pawn.health.hediffSet.hediffs.Remove(addiction);
                currentStormlightInt -= cost;
                RadiantUtility.GiveRadiantXp(pawn, 12f);
            }

            // HEAL INJURIES
            List<Hediff_Injury> injuries = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                .OrderByDescending(h => h.Severity)
                .ToList();
            foreach (Hediff_Injury? injury in injuries) {
                float cost = injury.Severity * 3f; // More severe wounds cost more stormlight
                if (currentStormlightInt < cost) {
                    break;
                }

                injury.Heal(10.0f);
                currentStormlightInt -= cost;
                RadiantUtility.GiveRadiantXp(pawn, 0.5f);
            }
        }

        private void RadiantAbsorbStormlight() {
            if (pawn?.Spawned ?? false) return;
            if (!pawn!.RaceProps.Humanlike) return;
            if (breathStormlight == false) return;
            if (InfuseStormlight(0)) return; //check if full.

            float absorbAmount = 25f; // How much Stormlight is drawn per tick
            float maxDrawDistance = 3.0f; // Range to absorb from nearby spheres

            if (maxStormlightPerItem - currentStormlightInt < absorbAmount) {
                absorbAmount = maxStormlightPerItem - currentStormlightInt;
            }

            //  Absorb Stormlight from pouch
            CompSpherePouch pouchComp = pawn.apparel?.WornApparel?.Find(a => a.GetComp<CompSpherePouch>() != null)
                ?.GetComp<CompSpherePouch>();
            if (pouchComp != null && pouchComp.GetTotalStoredStormlight() > 0) {
                float drawn = pouchComp.DrawStormlight(absorbAmount);
                RadiantUtility.GiveRadiantXp(pawn, 0.1f);
                if (InfuseStormlight(drawn)) return;
            }


            //  Absorb Stormlight from inventory
            if (pawn.inventory != null) {
                foreach (Verse.Thing item in pawn.inventory.innerContainer) {
                    ThingWithComps? thingWithComps = item as ThingWithComps;

                    Stormlight? sphere = thingWithComps?.TryGetComp<Stormlight>();
                    if (sphere is not { currentStormlightInt: > 0 }) continue;
                    float drawn = Math.Min(absorbAmount, sphere.currentStormlightInt);
                    sphere.InfuseStormlight(-drawn); // Remove from sphere
                    RadiantUtility.GiveRadiantXp(pawn, 0.1f);
                    if (InfuseStormlight(drawn)) return;
                }
            }

            //  Absorb Stormlight from nearby spheres
            List<Verse.Thing> nearbyThings = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Everything).Where(t => t.Position.Roofed(t.Map)).ToList();
            foreach (Verse.Thing thing in nearbyThings) {
                float drawnLight;
                if (thing.TryGetComp(out Stormlight stormlight)) {
                    drawnLight = stormlight.DrawStormlight(absorbAmount);
                } else if (thing.TryGetComp(out CompSpherePouch pouch)) {
                    drawnLight = pouch.DrawStormlight(absorbAmount);
                } else if (thing.TryGetComp(out StormlightLamps lamp)) {
                    drawnLight = lamp.DrawStormlight(absorbAmount);
                } else {
                    return;
                }
                
                RadiantUtility.GiveRadiantXp(pawn, 0.1f);
                if (InfuseStormlight(drawnLight)) return;
            }
        }


        private void HandleSurges() {
            if (pawn == null) return;
            if (currentStormlightInt <= 0f) {
                abrasionActiveInt = false;
            }

            if (abrasionActiveInt) {
                if (pawn.health.hediffSet.GetFirstHediffOfDef(CosmereRosharDefs.WhtwlSurgeAbrasion) == null) {
                    pawn.health.AddHediff(CosmereRosharDefs.WhtwlSurgeAbrasion);
                    drainFactor += 500f;
                }
            } else {
                if (pawn.health.hediffSet.GetFirstHediffOfDef(CosmereRosharDefs.WhtwlSurgeAbrasion) is { } hediff) {
                    pawn.health.RemoveHediff(hediff);
                }

                drainFactor = 1f;
            }
        }


        public float GetDrainRate(float factor) {
            return props.drainRate / qualityMultiplier * factor;
        }

        public void DrainStormLight() {
            if (!(currentStormlightInt > 0)) return;
            
            currentStormlightInt -= GetDrainRate(drainFactor);
            if (currentStormlightInt < 0) {
                currentStormlightInt = 0;
            }
        }

        public float DrawStormlight(float amount) {
            float drawnAmount = amount;
            if (currentStormlightInt <= 0) return 0f;

            currentStormlightInt -= amount;
            if (currentStormlightInt > 0) return drawnAmount;

            drawnAmount += currentStormlightInt;
            currentStormlightInt = 0;

            return drawnAmount;
        }

        // Infuse from code when highstorm is active
        public bool InfuseStormlight(float amount) {
            if (currentMaxStormlight <= 0f) {
                AdjustMaximumStormlight();
            }

            currentStormlightInt += amount;
            if (currentStormlightInt < currentMaxStormlight) {
                return false;
            }

            currentStormlightInt = currentMaxStormlight;
            return true;
        }
    }
}