// using System;
// using System.Collections.Generic;
// using RimWorld;
// using UnityEngine;
// using Verse;
//
// namespace CosmereRoshar.Comp.Thing;
//
// public class StormlightProperties : CompProperties {
//     public float drainRate = 0f;
//     public float maxInvestiture = -1;
//
//     public StormlightProperties() {
//         compClass = typeof(Stormlight);
//     }
//
//     public override IEnumerable<string> ConfigErrors(ThingDef parentDef) {
//         foreach (string configError in base.ConfigErrors(parentDef)) {
//             yield return configError;
//         }
//
//         if (maxInvestiture <= 0) {
//             yield return "maxInvestiture must be greater than 0.";
//         }
//     }
// }

// public class Stormlight : ThingComp {
//     public bool breathInStormlight = false;
//     private float storedInvestiture;
//     public float StoredInvestiture => storedInvestiture;
//     private float maxInvestiture => props.maxInvestiture * qualityMultiplier;
//     private new StormlightProperties props => (StormlightProperties)base.props;
//
//
//
//     public override void PostExposeData() {
//         base.PostExposeData();
//         Scribe_Values.Look(ref storedInvestiture, "storedInvestiture");
//
//         if (breathInStormlight) {
//             // This obviously isn't accurate. Just pseudocode 
//             storedInvestiture += Mathf.Max(props.maxInvestiture, props.drainRate * qualityMultiplier);
//         }
//     }
//
//
//     // This method adds additional text to the inspect pane.
//     public override string CompInspectStringExtra() {
//         return "Stormlight: " + storedInvestiture.ToString("F0") + " / " + maxInvestiture.ToString("F0");
//     }
// }