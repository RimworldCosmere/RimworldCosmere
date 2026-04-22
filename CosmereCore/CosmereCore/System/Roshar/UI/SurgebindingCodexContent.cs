using System.Collections.Generic;
using Cosmere.Core.UI.Codex;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Ability;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.UI;

public sealed class SurgebindingCodexContent : ICodexContentProvider {
    private static Vector2 progressionScroll = Vector2.zero;

    public bool HasProgression(Pawn pawn) => GetSurgebinder(pawn) != null;
    public bool ShowsBondsSubtab => true;
    public bool HasBonds(Pawn pawn) => GetSurgebinder(pawn)?.bondedSpren != null;
    public bool HasMemories(Pawn pawn) => false;
    public bool OwnsAbility(RimWorld.Ability ability) => ability is SurgebindingAbility;

    public string? HeaderLabelFor(Pawn pawn) {
        Surgebinder? s = GetSurgebinder(pawn);
        return s?.radiantOrderDef?.LabelCap;
    }

    public void DrawProgression(Pawn pawn, Rect rect) {
        Surgebinder? s = GetSurgebinder(pawn);
        if (s == null) return;

        RadiantOrderDef order = s.radiantOrderDef;
        Color accent = order.color;

        float contentHeight = EstimateProgressionHeight(order, s.currentIdealDisplay, rect.width - 36f);
        Rect viewRect = new Rect(0f, 0f, rect.width - 16f, contentHeight);
        Widgets.BeginScrollView(rect, ref progressionScroll, viewRect);
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

    public void DrawBonds(Pawn pawn, Rect rect) {
        Surgebinder? s = GetSurgebinder(pawn);
        Pawn? spren = s?.bondedSpren;
        if (spren == null) return;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "CC_Codex_Surgebinding_BondedHeader".Translate());

        Rect sprenRow = new Rect(rect.x, rect.y + 34f, rect.width, 28f);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.9f, 0.9f, 0.9f)))
            Widgets.Label(sprenRow, spren.NameFullColored);

        if (Widgets.ButtonInvisible(sprenRow)) {
            CameraJumper.TryJumpAndSelect(spren);
        }
        if (Mouse.IsOver(sprenRow)) {
            Widgets.DrawHighlight(sprenRow);
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
