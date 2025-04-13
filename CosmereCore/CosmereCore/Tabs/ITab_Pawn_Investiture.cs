using System;
using CosmereFramework;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereCore.Tabs {
    public class ITab_Pawn_Investiture : ITab {
        private const float Padding = 10f;

        public ITab_Pawn_Investiture() {
            Text.Font = GameFont.Small;
            labelKey = "TabInvestiture";
            tutorTag = "Investiture";
            size = new Vector2(600f, 500f); // adjust as needed
        }

        public override bool IsVisible {
            get => SelPawn?.story?.traits?.HasTrait(DefDatabase<TraitDef>.GetNamed("Cosmere_Invested")) == true;
        }

        protected override void FillTab() {
            var pawn = SelPawn;
            var rect = new Rect(Padding, Padding, size.x - Padding * 2, size.y - Padding * 2);
            var listing = new Listing_Standard();
            listing.Begin(rect);

            listing.Label("Investiture Overview");

            // Show BEU count from need
            var investNeed = pawn.needs?.TryGetNeed(DefDatabase<NeedDef>.GetNamed("Cosmere_Investiture"));
            if (investNeed != null) {
                var beus = (int)investNeed.CurLevel;
                listing.Label($"BEUs: {beus:N0}");
            }

            foreach (var drawer in InvestitureTabRegistry.Drawers) {
                try {
                    drawer(SelPawn, listing);
                }
                catch (Exception ex) {
                    Log.Message($"[InvestitureTab] Error in registered drawer: {ex}", LogLevel.Error);
                }
            }


            listing.End();
        }
    }
}