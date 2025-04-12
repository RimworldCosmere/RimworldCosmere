using System.Collections.Generic;
using System.Linq;
using CosmereCore.Tabs;
using CosmereFramework.Utils;
using CosmereScadrial.Registry;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Inspector {
    public class InvestitureTab_Scadrial {
        private static readonly Dictionary<Color, Texture2D> _colorCache = new Dictionary<Color, Texture2D>();

        private static Texture2D GetColorTexture(Color color) {
            if (_colorCache.TryGetValue(color, out var tex)) return tex;

            tex = SolidColorMaterials.NewSolidColorTexture(color);
            _colorCache[color] = tex;

            return tex;
        }

        public static void Draw(Pawn pawn, Listing_Standard listing) {
            var comp = pawn.AllComps.FirstOrDefault(c => c.GetType().Name == "CompScadrialInvestiture");
            if (comp == null) return;

            var reserves = comp.GetType().GetField("metalReserves")?.GetValue(comp) as Dictionary<string, float>;
            if (reserves == null || reserves.Count == 0) return;

            if (!InvestitureTabUI.DrawCollapsibleHeader("Allomantic Reserves", "Scadrial_Allomancy", listing)) {
                return;
            }

            var tableRect = listing.GetRect(4 * 26f + 32f); // room for headers + rows
            DrawAllomanticMetalTable(pawn, tableRect, listing);
            listing.Gap(10f);
        }

        private static void DrawAllomanticMetalTable(Pawn pawn, Rect outerRect, Listing_Standard listing) {
            var comp = pawn.AllComps.FirstOrDefault(c => c.GetType().Name == "CompScadrialInvestiture");
            if (comp?.GetType().GetField("metalReserves")?.GetValue(comp) is not Dictionary<string, float> reserves) return;

            var groups = new[] { "Physical", "Mental", "Enhancement", "Temporal" };
            var axes = new[] { "External", "Internal" };
            var polarities = new[] { "Pushing", "Pulling" };

            var cellWidth = outerRect.width / (groups.Length + 1); // +1 for row labels
            const float cellHeight = 26f; // Room for bar

            UIUtil.WithFont(GameFont.Small, () => {
                // Draw column headers
                for (var col = 0; col < groups.Length; col++) {
                    var headerRect = new Rect(outerRect.x + cellWidth * (col + 1), outerRect.y, cellWidth, cellHeight);
                    UIUtil.WithAnchor(TextAnchor.MiddleCenter, () => Widgets.Label(headerRect, groups[col]));
                }

                // Draw rows
                for (var row = 0; row < axes.Length * polarities.Length; row++) {
                    var axis = axes[row / 2];
                    var polarity = polarities[row % 2];

                    // Draw row header
                    var rowHeaderRect = new Rect(outerRect.x, outerRect.y + cellHeight * (row + 1),
                        cellWidth, cellHeight);
                    UIUtil.WithAnchor(TextAnchor.MiddleCenter, () => Widgets.Label(rowHeaderRect, polarity));

                    for (var col = 0; col < groups.Length; col++) {
                        var group = groups[col];

                        var metal = MetalRegistry.Metals.Values.FirstOrDefault(m =>
                            m.Allomancy.Group.EqualsIgnoreCase(group)
                            && m.Allomancy.Axis.EqualsIgnoreCase(axis)
                            && m.Allomancy.Polarity.EqualsIgnoreCase(polarity));

                        var cellRect = new Rect(
                            outerRect.x + cellWidth * (col + 1),
                            outerRect.y + cellHeight * (row + 1),
                            cellWidth,
                            cellHeight
                        );

                        if (metal != null) {
                            var value = reserves.TryGetValue(metal.Name, out var amt) ? amt : 0f;

                            // Prep bar space in Listing
                            listing.Gap(2f);
                            var max = metal.MaxAmount;
                            var fillPct = Mathf.Clamp01(value / max);
                            var label = metal.Name.CapitalizeFirst();
                            var tooltip =
                                $"{label}\n\n{metal.Allomancy.Description}\n\n{value:N0} / {max:N0}";

                            // Faint cell border
                            Widgets.DrawBoxSolid(cellRect, new Color(0.15f, 0.15f, 0.15f)); // background
                            Widgets.DrawBox(cellRect); // faint outline

                            var barRect = cellRect.ContractedBy(2f);
                            // Fillable bar
                            Widgets.FillableBar(barRect, fillPct, GetColorTexture(metal.Color));

                            // Label
                            UIUtil.DrawContrastLabel(barRect, label, metal.Color);

                            TooltipHandler.TipRegion(cellRect, tooltip);
                        }
                        else {
                            Widgets.DrawBoxSolid(cellRect, new Color(0.1f, 0.1f, 0.1f, 0.5f)); // Empty cell
                        }
                    }
                }
            });
        }
    }
}