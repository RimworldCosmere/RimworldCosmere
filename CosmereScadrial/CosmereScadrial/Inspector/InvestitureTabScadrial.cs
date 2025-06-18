using System.Collections.Generic;
using CosmereFramework.Utils;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using LudeonTK;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Inspector;

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
        if (colorCache.TryGetValue(color, out Texture2D? tex)) return tex;

        tex = SolidColorMaterials.NewSolidColorTexture(color);
        colorCache[color] = tex;

        return tex;
    }

    public static void Draw(Pawn pawn, Listing_Standard listing) {
        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Metalborn)) return;

        if (!UIUtil.DrawCollapsibleHeader("Allomantic Reserves", "Scadrial_Allomancy", listing)) {
            return;
        }

        Rect tableRect = listing.GetRect(tableHeight, tableWidthPct); // room for headers + rows
        DrawAllomanticMetalTable(pawn, tableRect, listing);
        listing.Gap(10f);
    }

    private static void DrawAllomanticMetalTable(Pawn pawn, Rect outerRect, Listing_Standard listing) {
        List<MetallicArtsMetalDef>? metals = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading;
        MetalReserves? comp = pawn.GetComp<MetalReserves>();
        MetalBurning? burning = pawn.GetComp<MetalBurning>();

        AllomancyGroup[] groups = new[]
            { AllomancyGroup.Physical, AllomancyGroup.Mental, AllomancyGroup.Enhancement, AllomancyGroup.Temporal };
        AllomancyAxis[] axes = new[] { AllomancyAxis.External, AllomancyAxis.Internal };
        AllomancyPolarity[] polarities = new[] { AllomancyPolarity.Pulling, AllomancyPolarity.Pushing };

        float cellWidth = outerRect.width / (groups.Length + 1); // +1 for row labels

        UIUtil.WithFont(GameFont.Small, () => {
            // Draw column headers
            for (int col = 0; col < groups.Length; col++) {
                Rect headerRect = new Rect(outerRect.x + cellWidth * (col + 1), outerRect.y, cellWidth, cellHeight);
                UIUtil.WithAnchor(TextAnchor.MiddleCenter, () => Widgets.Label(headerRect, groups[col].ToString()));
            }

            // Draw rows
            int key = 0;
            for (int row = 0; row < axes.Length * polarities.Length; row++) {
                AllomancyAxis axis = axes[row / 2];
                AllomancyPolarity polarity = polarities[row % 2];

                // Draw row header
                Rect rowHeaderRect = new Rect(outerRect.x, outerRect.y + cellHeight * (row + 1),
                    cellWidth, cellHeight);
                UIUtil.WithAnchor(TextAnchor.MiddleCenter, () => Widgets.Label(rowHeaderRect, polarity.ToString()));

                for (int col = 0; col < groups.Length; col++) {
                    AllomancyGroup group = groups[col];

                    MetallicArtsMetalDef? metal = metals.FirstOrDefault(m =>
                        m.allomancy != null &&
                        m.allomancy.group.Equals(group)
                        && m.allomancy.axis.Equals(axis)
                        && m.allomancy.polarity.Equals(polarity));

                    Rect cellRect = new Rect(
                        outerRect.x + cellWidth * (col + 1),
                        outerRect.y + cellHeight * (row + 1),
                        cellWidth,
                        cellHeight
                    );

                    if (metal != null) {
                        float value = comp.TryGetReserve(metal, out float amt) ? amt : 0f;
                        float rate = burning.GetTotalBurnRate(metal) * GenTicks.TicksPerRealSecond;

                        // Prep bar space in Listing
                        listing.Gap(2f);
                        float fillPct = Mathf.Clamp01(value / MetalReserves.MAX_AMOUNT);
                        string tooltip =
                            $"{metal.label.CapitalizeFirst()}\n\n{metal.allomancy.description}\n\n{value:N0} / {MetalReserves.MAX_AMOUNT:N0}";
                        if (burning.IsBurning(metal)) {
                            tooltip += $" ({rate:0.000}/sec)";
                        }

                        Widgets.DrawBoxSolidWithOutline(cellRect, new Color(0.66f, 0.66f, 0.66f),
                            rate > 0 ? Color.green : new Color(0.1f, 0.1f, 0.1f));

                        Rect barRect = cellRect.ContractedBy(2f);
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