using CosmereFramework.Settings;
using UnityEngine;
using Verse;

namespace CosmereRoshar.Settings;


public class RosharModSettings : CosmereModSettings {
     public bool enableHighstormPushing;
     public bool enablePawnGlow;
     public bool enableHighstormDamage;
     public bool devOptionAutofillSpheres;
     public float bondChanceMultiplier;
     public string bondChanceMultiplierBuffer;
     public override string Name => "Roshar";
     public override void ExposeData() {
         Scribe_Values.Look(ref enableHighstormPushing, "enableHighstormPushing", true); 
         Scribe_Values.Look(ref enablePawnGlow, "enablePawnGlow", false); 
         Scribe_Values.Look(ref devOptionAutofillSpheres, "devOptionAutofillSpheres", false); 
         Scribe_Values.Look(ref enableHighstormDamage, "enableHighstormDamage", true);  
         Scribe_Values.Look(ref bondChanceMultiplier, "bondChanceMultiplier", 1);
     }
     public override void DoTabContents(Rect inRect, Listing_Standard mainListing) {
         Listing_Standard listingStandard = new Listing_Standard();
         listingStandard.Begin(inRect);
         listingStandard.CheckboxLabeled("Highstorms will move items and pawns", ref enableHighstormPushing, "expensive stuff");
         listingStandard.CheckboxLabeled("Highstorms will damage visitors and animals", ref enableHighstormDamage, "turn off if you dont want all your wildlife to die, and visitors to hate you");
         listingStandard.CheckboxLabeled("Pawns will glow when infused with stormlight", ref enablePawnGlow, "currently broken");
         listingStandard.CheckboxLabeled("DEV OPTION: enable dev features", ref devOptionAutofillSpheres, "Will enable various dev features, like filling spheres");
         listingStandard.TextFieldNumericLabeled<float>("bond chance multiplier", ref bondChanceMultiplier, ref bondChanceMultiplierBuffer, 1, 10000);
         listingStandard.End();
     }
 }

