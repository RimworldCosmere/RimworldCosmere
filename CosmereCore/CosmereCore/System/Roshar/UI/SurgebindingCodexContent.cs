using System.Collections.Generic;
using System.Text;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.UI.Codex;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding;
using Cosmere.System.Roshar.Surgebinding.Ability;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.UI;

public sealed class SurgebindingCodexContent : ICodexContentProvider {
    private static readonly Texture2D BondBarHealthyTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.3f));
    private static readonly Texture2D BondBarStrainedTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.9f, 0.8f, 0.2f));
    private static readonly Texture2D BondBarFracturedTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.9f, 0.5f, 0.1f));
    private static readonly Texture2D BondBarBreakingTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.9f, 0.2f, 0.1f));

    private const float StripWidth = 90f;
    private const float StripEntryHeight = 64f;

    private const float BondDetailHeaderHeight = 35f;
    private const float BondDetailRowHeight = 28f;
    private const float BondDetailTraitHeaderHeight = 26f;
    private const float BondDetailSectionSpacing = 8f;

    private readonly List<Surgebinder> bondedSurgebindersBuffer = [];

    public bool HasProgression(Pawn pawn) => GetSurgebinder(pawn) != null;
    public bool ShowsBondsSubtab => true;
    public bool HasBonds(Pawn pawn) {
        CollectBondedSurgebinders(pawn);
        return bondedSurgebindersBuffer.Count > 0;
    }
    public bool HasMemories(Pawn pawn) => false;
    public bool OwnsAbility(RimWorld.Ability ability) => ability is SurgebindingAbility;

    public string? HeaderLabelFor(Pawn pawn) {
        Surgebinder? s = GetSurgebinder(pawn);
        return s?.radiantOrderDef?.LabelCap;
    }

    public void DrawProgression(Pawn pawn, Rect rect, CodexState state) {
        Surgebinder? s = GetSurgebinder(pawn);
        if (s == null) return;

        RadiantOrderDef order = s.radiantOrderDef;
        Color accent = order.color;

        float contentHeight = EstimateProgressionHeight(order, s.currentIdealDisplay, rect.width - 36f);
        Rect viewRect = new Rect(0f, 0f, rect.width - 16f, contentHeight);
        Widgets.BeginScrollView(rect, ref state.ProgressionScroll, viewRect);
        DrawProgressionContent(pawn, viewRect, s, order, accent);
        Widgets.EndScrollView();
    }

    private static void DrawProgressionContent(Pawn pawn, Rect rect, Surgebinder s, RadiantOrderDef order, Color accent) {
        float y = rect.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(
                new Rect(rect.x, y, rect.width - 110f, 30f),
                "CC_Codex_Surgebinding_OrderRow".Translate(order.LabelCap.Named("ORDER"))
            );

        Rect infoButton = new Rect(rect.xMax - 100f, y + 3f, 100f, 24f);
        if (Widgets.ButtonText(infoButton, "CC_Codex_Surgebinding_OrderInfo".Translate())) {
            Find.WindowStack.Add(new RadiantOrderInfoDialog(pawn, s, RadiantOrderInfoMode.View));
        }
        y += 34f;

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f)))
            Widgets.Label(
                new Rect(rect.x, y, rect.width, 24f),
                "CC_Codex_Surgebinding_IdealsSworn".Translate(s.currentIdealDisplay.Named("CURRENT"))
            );
        y += 30f;

        for (int i = 0; i < 5; i++) {
            int idealNumber = i + 1;
            bool achieved = idealNumber <= s.currentIdealDisplay;
            Color dotColor = achieved ? Color.Lerp(accent, new Color(0.95f, 0.85f, 0.35f), 0.3f) : new Color(0.35f, 0.35f, 0.4f);
            Color headerColor = achieved ? Color.white : new Color(0.6f, 0.6f, 0.65f);

            Rect header = new Rect(rect.x, y, rect.width, 24f);
            Rect dot = new Rect(header.x + 4f, header.y + 5f, 14f, 14f);
            Widgets.DrawBoxSolid(dot, dotColor);

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, headerColor))
                Widgets.Label(
                    new Rect(dot.xMax + 10f, header.y, header.width - dot.xMax - rect.x - 10f, header.height),
                    IdealLabel(order, idealNumber, achieved)
                );
            y += 26f;

            if (achieved && i < order.ideals.Count) {
                string? desc = order.ideals[i].description;
                if (!desc.NullOrEmpty()) {
                    using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, new Color(0.78f, 0.78f, 0.82f))) {
                        float descHeight = Text.CalcHeight(desc, rect.width - 40f);
                        Widgets.Label(new Rect(rect.x + 32f, y, rect.width - 40f, descHeight), desc);
                        y += descHeight + 2f;
                    }
                }
            }

            List<AbilityDef> unlocks = GetAbilitiesUnlockedAt(order, i);
            for (int k = 0; k < unlocks.Count; k++) {
                AbilityDef ability = unlocks[k];
                Color abilityColor = achieved ? new Color(0.88f, 0.88f, 0.9f) : new Color(0.5f, 0.5f, 0.55f);

                Rect abilityRow = new Rect(rect.x + 32f, y, rect.width - 36f, 22f);
                Rect iconRect = new Rect(abilityRow.x, abilityRow.y + 3f, 16f, 16f);
                if (ability.uiIcon != null) {
                    GUI.color = abilityColor;
                    GUI.DrawTexture(iconRect, ability.uiIcon);
                    GUI.color = Color.white;
                }

                using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, abilityColor))
                    Widgets.Label(
                        new Rect(iconRect.xMax + 6f, abilityRow.y, abilityRow.width - iconRect.width - 6f, abilityRow.height),
                        ability.LabelCap
                    );

                y += 22f;
            }

            y += 6f;
        }
    }

    public void DrawBonds(Pawn pawn, Rect rect, CodexState state) {
        CollectBondedSurgebinders(pawn);
        List<Surgebinder> bonds = bondedSurgebindersBuffer;
        if (bonds.Count == 0) return;

        if (state.SelectedSprenIndex >= bonds.Count) state.SelectedSprenIndex = 0;

        Rect stripRect = new Rect(rect.x, rect.y, StripWidth, rect.height);
        Rect detailRect = new Rect(rect.x + StripWidth + 4f, rect.y, rect.width - StripWidth - 4f, rect.height);

        DrawSprenStrip(stripRect, bonds, state);
        DrawBondDetail(detailRect, bonds[state.SelectedSprenIndex], state);
    }

    private static void DrawSprenStrip(Rect rect, List<Surgebinder> bonds, CodexState state) {
        Widgets.DrawBoxSolid(rect, new Color(0.06f, 0.06f, 0.08f, 0.8f));

        for (int i = 0; i < bonds.Count; i++) {
            Surgebinder gene = bonds[i];
            Verse.Pawn spren = gene.bondedSpren!;

            bool selected = state.SelectedSprenIndex == i;
            Color orderColor = gene.radiantOrderDef?.color ?? Color.white;

            Rect entryRect = new Rect(rect.x, rect.y + (i * StripEntryHeight), rect.width, StripEntryHeight);

            if (selected) {
                Widgets.DrawBoxSolid(entryRect, new Color(orderColor.r * 0.25f, orderColor.g * 0.25f, orderColor.b * 0.25f, 0.9f));
                Rect accent = new Rect(entryRect.x, entryRect.y, 3f, entryRect.height);
                Widgets.DrawBoxSolid(accent, orderColor);
            } else if (Mouse.IsOver(entryRect)) {
                Widgets.DrawHighlight(entryRect);
            }

            Rect swatch = new Rect(entryRect.x + 8f, entryRect.y + 8f, 12f, 12f);
            Widgets.DrawBoxSolid(swatch, orderColor);

            Rect nameRect = new Rect(entryRect.x + 6f, swatch.yMax + 4f, entryRect.width - 12f, entryRect.height - swatch.height - 16f);
            using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, selected ? Color.white : new Color(0.8f, 0.8f, 0.8f)))
                Widgets.Label(nameRect, spren.Name?.ToStringShort ?? spren.LabelShortCap);

            if (Widgets.ButtonInvisible(entryRect)) {
                state.SelectedSprenIndex = i;
            }
        }
    }

    private static void DrawBondDetail(Rect rect, Surgebinder gene, CodexState state) {
        Verse.Pawn? spren = gene.bondedSpren;
        if (spren == null) return;

        CompSprenBond? bond = spren.TryGetComp<CompSprenBond>();
        if (bond?.BondedRadiant == null) return;

        Verse.Pawn radiant = bond.BondedRadiant;

        float contentHeight = EstimateBondDetailHeight(bond);
        Rect viewRect = new Rect(0f, 0f, rect.width - 16f, contentHeight);
        Widgets.BeginScrollView(rect, ref state.BondDetailScroll, viewRect);
        DrawBondDetailContent(viewRect, spren, radiant, bond);
        Widgets.EndScrollView();
    }

    private static void DrawBondDetailContent(Rect rect, Verse.Pawn spren, Verse.Pawn radiant, CompSprenBond bond) {
        float y = rect.y;

        using (new TextBlock(GameFont.Medium)) {
            float renameSize = 24f;
            Rect headerRect = new Rect(rect.x, y, rect.width - renameSize - 4f, 30f);
            Widgets.Label(headerRect, spren.NameFullColored);

            Rect renameRect = new Rect(headerRect.xMax + 4f, y + 3f, renameSize, renameSize);
            if (Widgets.ButtonImage(renameRect, TexButton.Rename)) {
                Find.WindowStack.Add(new NameSprenDialog(spren));
            }
            TooltipHandler.TipRegion(renameRect, "CRO_Spren_Rename".Translate());
            y += BondDetailHeaderHeight;
        }

        using (new TextBlock(GameFont.Small)) {
            string bondLabel = "CRO_Spren_BondedSpren".Translate(spren.NameFullColored.Named("SPREN"));
            Rect bondRect = new Rect(rect.x, y, rect.width, 24f);
            Widgets.Label(bondRect, bondLabel);
            if (Widgets.ButtonInvisible(bondRect)) {
                CameraJumper.TryJumpAndSelect(spren);
            }
            if (Mouse.IsOver(bondRect)) {
                Widgets.DrawHighlight(bondRect);
            }
            y += BondDetailRowHeight;

            float connection = SpiritWeb.Instance?.GetConnectionValue(radiant, spren) ?? 0f;
            int percentage = (int)(connection * 100f);
            string stage = connection switch {
                >= 0.7f => "CRO_BondStage_Healthy".Translate(),
                >= 0.4f => "CRO_BondStage_Strained".Translate(),
                >= 0.15f => "CRO_BondStage_Fractured".Translate(),
                _ => "CRO_BondStage_Breaking".Translate(),
            };

            Rect strengthLabelRect = new Rect(rect.x, y, 120f, 24f);
            Widgets.Label(strengthLabelRect, "CRO_SprenBond_Strength".Translate());

            Rect barRect = new Rect(rect.x + 120f, y + 2f, rect.width - 180f, 20f);
            Widgets.FillableBar(barRect, connection, GetBondBarTexture(connection));

            Rect percentRect = new Rect(barRect.xMax + 4f, y, 50f, 24f);
            Widgets.Label(percentRect, $"{percentage}%");

            Rect strengthTooltipRect = new Rect(rect.x, y, rect.width, 24f);
            // distinct hash from ITab_SprenBond (73948201) to prevent tooltip cache collision
            TooltipHandler.TipRegion(strengthTooltipRect, () => BuildStrengthTooltip(radiant, connection), 73948202);
            y += BondDetailRowHeight;

            Rect stageRect = new Rect(rect.x, y, rect.width, 24f);
            Widgets.Label(stageRect, "CRO_SprenBond_Status".Translate(stage));
            y += BondDetailRowHeight;

            if (bond.Dismissed) {
                Rect dismissedRect = new Rect(rect.x, y, rect.width, 24f);
                Widgets.Label(dismissedRect, "CRO_SprenBond_Dismissed".Translate().Colorize(ColorLibrary.RedReadable));
                y += BondDetailRowHeight;
            }

            y += BondDetailSectionSpacing;
            Rect traitHeaderRect = new Rect(rect.x, y, rect.width, 24f);
            Widgets.Label(traitHeaderRect, "CRO_SprenBond_Personality".Translate().Colorize(ColoredText.TipSectionTitleColor));
            y += BondDetailTraitHeaderHeight;

            if (bond.PersonalityTraits.Count == 0) {
                Widgets.Label(new Rect(rect.x + 10f, y, rect.width - 10f, 24f), "CRO_SprenBond_NoTraits".Translate());
            } else {
                for (int i = 0; i < bond.PersonalityTraits.Count; i++) {
                    Rect traitRect = new Rect(rect.x + 10f, y, rect.width - 10f, 24f);
                    TraitDef traitDef = bond.PersonalityTraits[i];
                    string traitLabel = traitDef.degreeDatas.Count > 0
                        ? traitDef.degreeDatas[0].LabelCap
                        : traitDef.LabelCap;
                    Widgets.Label(traitRect, "- " + traitLabel);
                    y += 24f;
                }
            }
        }
    }

    private static float EstimateBondDetailHeight(CompSprenBond bond) {
        float h = BondDetailHeaderHeight + BondDetailRowHeight + BondDetailRowHeight + BondDetailRowHeight + BondDetailSectionSpacing + BondDetailTraitHeaderHeight;
        if (bond.Dismissed) h += BondDetailRowHeight;
        h += bond.PersonalityTraits.Count == 0 ? 24f : bond.PersonalityTraits.Count * 24f;
        return h + 16f;
    }

    private static string BuildStrengthTooltip(Verse.Pawn radiant, float connection) {
        if (connection >= 1f) {
            return "CRO_SprenBond_StrengthFull".Translate();
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("CRO_SprenBond_StrengthTooltip".Translate());

        bool hasStrainedBond = false;
        List<Verse.Hediff> hediffs = radiant.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++) {
            if (hediffs[i] is Cosmere.System.Roshar.Hediff.StrainedBond) {
                hasStrainedBond = true;
                sb.Append("\n  - ").Append("CRO_SprenBond_StrainedBondFactor".Translate());
                break;
            }
        }

        List<LogEntry> logs = Find.PlayLog.AllEntries;
        int violationsShown = 0;
        for (int i = 0; i < logs.Count && violationsShown < 5; i++) {
            if (logs[i] is not BondViolationLogEntry violation) continue;
            if (violation.pawn != radiant) continue;

            sb.Append("\n  - ").Append(violation.reason).Append(" (").Append(violation.severityLabel).Append(')');
            violationsShown++;
        }

        if (!hasStrainedBond && violationsShown == 0) {
            sb.Append("\n  - ").Append("CRO_SprenBond_UnknownFactors".Translate());
        }

        return sb.ToString();
    }

    private static Texture2D GetBondBarTexture(float connection) {
        if (connection >= 0.7f) return BondBarHealthyTex;
        if (connection >= 0.4f) return BondBarStrainedTex;
        if (connection >= 0.15f) return BondBarFracturedTex;
        return BondBarBreakingTex;
    }

    private void CollectBondedSurgebinders(Pawn pawn) {
        bondedSurgebindersBuffer.Clear();
        if (pawn.genes == null) return;
        List<Verse.Gene> genes = pawn.genes.GenesListForReading;
        for (int i = 0; i < genes.Count; i++) {
            if (genes[i] is Surgebinder s && !s.Overridden && s.bondedSpren != null) bondedSurgebindersBuffer.Add(s);
        }
    }

    public void DrawMemories(Pawn pawn, Rect rect) { }

    private static Surgebinder? GetSurgebinder(Pawn pawn) {
        return pawn.genes?.GetFirstGeneOfType<Surgebinder>();
    }

    private static string IdealLabel(RadiantOrderDef order, int ideal, bool achieved) {
        if (achieved && order.ideals != null && ideal - 1 < order.ideals.Count) {
            return order.ideals[ideal - 1].label.CapitalizeFirst();
        }
        return (string)"CC_Codex_Surgebinding_IdealUnsworn".Translate(ideal.Named("IDEAL")).Resolve();
    }

    private static float EstimateProgressionHeight(RadiantOrderDef order, int currentIdealDisplay, float contentWidth) {
        float h = 34f + 30f;
        GameFont prevFont = Text.Font;
        Text.Font = GameFont.Tiny;
        for (int i = 0; i < 5; i++) {
            h += 26f;
            int idealNumber = i + 1;
            bool achieved = idealNumber <= currentIdealDisplay;
            if (achieved && i < order.ideals.Count) {
                string? desc = order.ideals[i].description;
                if (!desc.NullOrEmpty()) {
                    h += Text.CalcHeight(desc, contentWidth) + 2f;
                }
            }
            List<AbilityDef> unlocks = GetAbilitiesUnlockedAt(order, i);
            h += unlocks.Count * 22f;
            h += 6f;
        }
        Text.Font = prevFont;
        return h;
    }

    private static List<AbilityDef> GetAbilitiesUnlockedAt(RadiantOrderDef order, int idealIndex) {
        List<AbilityDef> result = [];
        HashSet<AbilityDef> seen = [];

        if (idealIndex == 0 && order.abilities != null) {
            for (int i = 0; i < order.abilities.Count; i++) {
                if (seen.Add(order.abilities[i])) result.Add(order.abilities[i]);
            }
        }

        if (order.ideals != null && idealIndex < order.ideals.Count) {
            List<AbilityDef>? idealAbilities = order.ideals[idealIndex].abilities;
            if (idealAbilities != null) {
                for (int i = 0; i < idealAbilities.Count; i++) {
                    if (seen.Add(idealAbilities[i])) result.Add(idealAbilities[i]);
                }
            }
        }

        if (order.surges != null) {
            for (int i = 0; i < order.surges.Count; i++) {
                List<AbilityDef>? surgeAbilities = order.surges[i].abilities;
                if (surgeAbilities == null) continue;
                for (int j = 0; j < surgeAbilities.Count; j++) {
                    AbilityDef def = surgeAbilities[j];
                    int minIdeal = def is SurgebindingAbilityDef sd
                        ? sd.GetMinIdealForOrder(order.defName)
                        : 0;
                    if (minIdeal != idealIndex) continue;
                    if (seen.Add(def)) result.Add(def);
                }
            }
        }

        return result;
    }
}
