using System.Collections.Generic;
using CosmereFramework.Utils;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using LudeonTK;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Inspector {
    public class InvestitureTabScadrial {
        private static readonly Dictionary<Color, Texture2D> colorCache = new Dictionary<Color, Texture2D>();
#if DEBUG
        [TweakValue("AAA_CosmereScadrial", 10f, 300f)]
#endif
        private static readonly float tableHeight = 4 * 26f + 32f;
#if DEBUG
        [TweakValue("AAA_CosmereScadrial", 0f, 10f)]
#endif
        private static readonly float tableWidthPct = 1f;
#if DEBUG
        [TweakValue("AAA_CosmereScadrial", 10f)]
#endif
        private static readonly float cellHeight = 36f;

        private static Texture2D GetColorTexture(Color color) {
            if (colorCache.TryGetValue(color, out var tex)) return tex;

            tex = SolidColorMaterials.NewSolidColorTexture(color);
            colorCache[color] = tex;

            return tex;
        }

        public static void Draw(Pawn pawn, Listing_Standard listing) {
            if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Metalborn)) return;

            if (!UIUtil.DrawCollapsibleHeader("Allomantic Reserves", "Scadrial_Allomancy", listing)) {
                return;
            }

            var tableRect = listing.GetRect(tableHeight, tableWidthPct); // room for headers + rows
            DrawAllomanticMetalTable(pawn, tableRect, listing);
            listing.Gap(10f);
        }

        private static void DrawAllomanticMetalTable(Pawn pawn, Rect outerRect, Listing_Standard listing) {
            var metals = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading;
            var comp = pawn.GetComp<MetalReserves>();
            var burning = pawn.GetComp<MetalBurning>();

            var groups = new[] { AllomancyGroup.Physical, AllomancyGroup.Mental, AllomancyGroup.Enhancement, AllomancyGroup.Temporal };
            var axes = new[] { AllomancyAxis.External, AllomancyAxis.Internal };
            var polarities = new[] { AllomancyPolarity.Pulling, AllomancyPolarity.Pushing };

            var cellWidth = outerRect.width / (groups.Length + 1); // +1 for row labels

            UIUtil.WithFont(GameFont.Small, () => {
                // Draw column headers
                for (var col = 0; col < groups.Length; col++) {
                    var headerRect = new Rect(outerRect.x + cellWidth * (col + 1), outerRect.y, cellWidth, cellHeight);
                    UIUtil.WithAnchor(TextAnchor.MiddleCenter, () => Widgets.Label(headerRect, groups[col].ToString()));
                }

                // Draw rows
                var key = 0;
                for (var row = 0; row < axes.Length * polarities.Length; row++) {
                    var axis = axes[row / 2];
                    var polarity = polarities[row % 2];

                    // Draw row header
                    var rowHeaderRect = new Rect(outerRect.x, outerRect.y + cellHeight * (row + 1),
                        cellWidth, cellHeight);
                    UIUtil.WithAnchor(TextAnchor.MiddleCenter, () => Widgets.Label(rowHeaderRect, polarity.ToString()));

                    for (var col = 0; col < groups.Length; col++) {
                        var group = groups[col];

                        var metal = metals.FirstOrDefault(m =>
                            m.allomancy != null &&
                            m.allomancy.group.Equals(group)
                            && m.allomancy.axis.Equals(axis)
                            && m.allomancy.polarity.Equals(polarity));

                        var cellRect = new Rect(
                            outerRect.x + cellWidth * (col + 1),
                            outerRect.y + cellHeight * (row + 1),
                            cellWidth,
                            cellHeight
                        );

                        if (metal != null) {
                            var value = comp.TryGetReserve(metal, out var amt) ? amt : 0f;
                            var rate = burning.GetTotalBurnRate(metal) * GenTicks.TicksPerRealSecond;

                            // Prep bar space in Listing
                            listing.Gap(2f);
                            var fillPct = Mathf.Clamp01(value / MetalReserves.MAX_AMOUNT);
                            var tooltip = $"{metal.label.CapitalizeFirst()}\n\n{metal.allomancy.description}\n\n{value:N0} / {MetalReserves.MAX_AMOUNT:N0}";
                            if (burning.IsBurning(metal)) {
                                tooltip += $" ({rate:0.000}/sec)";
                            }

                            Widgets.DrawBoxSolidWithOutline(cellRect, new Color(0.66f, 0.66f, 0.66f), rate > 0 ? Color.green : new Color(0.1f, 0.1f, 0.1f));

                            var barRect = cellRect.ContractedBy(2f);
                            Widgets.FillableBar(barRect, fillPct, GetColorTexture(metal.color));
                            UIUtil.DrawContrastLabel(barRect, metal.label.CapitalizeFirst(), metal.color);
                            TooltipHandler.TipRegion(cellRect, () => tooltip, key++);
                        } else {
                            Widgets.DrawBoxSolid(cellRect, new Color(0.1f, 0.1f, 0.1f, 0.5f)); // Empty cell
                        }
                    }
                }
            });
        }
    }
}