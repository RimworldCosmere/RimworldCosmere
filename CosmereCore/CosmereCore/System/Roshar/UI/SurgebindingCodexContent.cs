using System.Text;
using Cosmere.Core.Ability.Autocast;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.UI.Codex;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Hediff;
using Cosmere.System.Roshar.Surgebinding;
using Cosmere.System.Roshar.Surgebinding.Ability;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.UI;

[StaticConstructorOnStartup]
public sealed class SurgebindingCodexContent : ICodexContentProvider {
    private const float StripWidth = 90f;
    private const float StripEntryHeight = 64f;

    private readonly List<Surgebinder> bondedSurgebindersBuffer = [];

    public bool HasProgression(Pawn pawn) {
        return GetSurgebinder(pawn) != null;
    }

    public bool ShowsMemoriesSubtab => false;

    public bool ShowsBondsSubtab => true;

    public bool HasBonds(Pawn pawn) {
        if (pawn.genes == null) return false;
        List<Verse.Gene> genes = pawn.genes.GenesListForReading;
        for (int i = 0; i < genes.Count; i++) {
            if (genes[i] is Surgebinder s && !s.Overridden && s.bondedSpren != null) return true;
        }

        return false;
    }

    public bool HasMemories(Pawn pawn) {
        return false;
    }

    public bool OwnsAbility(Ability ability) {
        return ability is SurgebindingAbility;
    }

    public string? HeaderLabelFor(Pawn pawn) {
        Surgebinder? s = GetSurgebinder(pawn);
        return s?.radiantOrderDef?.LabelCap;
    }

    public void DrawProgression(Rect rect, Pawn pawn, CodexState state) {
        Surgebinder? s = GetSurgebinder(pawn);
        if (s == null) return;

        RadiantOrderDef order = s.radiantOrderDef;
        Color accent = order.color;

        float contentHeight = EstimateProgressionHeight(order, s.CurrentIdealDisplay, rect.width - 36f);
        Rect viewRect = new Rect(0f, 0f, rect.width - 16f, contentHeight);
        Widgets.BeginScrollView(rect, ref state.ProgressionScroll, viewRect);
        DrawProgressionContent(pawn, viewRect, s, order, accent);
        Widgets.EndScrollView();
    }

    public void DrawBonds(Rect rect, Pawn pawn, CodexState state) {
        CollectBondedSurgebinders(pawn);
        List<Surgebinder> bonds = bondedSurgebindersBuffer;
        if (bonds.Count == 0) return;

        if (state.SelectedSprenIndex >= bonds.Count) state.SelectedSprenIndex = 0;

        Rect stripRect = new Rect(rect.x, rect.y, StripWidth, rect.height);
        Rect detailRect = new Rect(rect.x + StripWidth + 4f, rect.y, rect.width - StripWidth - 4f, rect.height);

        DrawSprenStrip(stripRect, bonds, state);
        DrawBondDetail(detailRect, bonds[state.SelectedSprenIndex], state);
    }

    public void DrawMemories(Rect rect, Pawn pawn, CodexState state) { }

    private static void DrawProgressionContent(
        Pawn pawn,
        Rect rect,
        Surgebinder s,
        RadiantOrderDef order,
        Color accent
    ) {
        float y = rect.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(
                new Rect(rect.x, y, rect.width - 110f, 30f),
                "CC_Codex_Surgebinding_OrderRow".Translate(order.LabelCap.Named("ORDER"))
            );
        }

        Rect infoButton = new Rect(rect.xMax - 100f, y + 3f, 100f, 24f);
        if (Widgets.ButtonText(infoButton, "CC_Codex_Surgebinding_OrderInfo".Translate())) {
            Find.WindowStack.Add(new Dialog_RadiantOrderInfoDialog(pawn, s, RadiantOrderInfoMode.View));
        }

        y += 34f;

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f))) {
            Widgets.Label(
                new Rect(rect.x, y, rect.width, 24f),
                "CC_Codex_Surgebinding_IdealsSworn".Translate(s.CurrentIdealDisplay.Named("CURRENT"))
            );
        }

        y += 30f;

        for (int i = 0; i < 5; i++) {
            int idealNumber = i + 1;
            bool achieved = idealNumber <= s.CurrentIdealDisplay;
            Color dotColor = achieved
                ? Color.Lerp(accent, new Color(0.95f, 0.85f, 0.35f), 0.3f)
                : new Color(0.35f, 0.35f, 0.4f);
            Color headerColor = achieved ? Color.white : new Color(0.6f, 0.6f, 0.65f);

            Rect header = new Rect(rect.x, y, rect.width, 24f);
            Rect dot = new Rect(header.x + 4f, header.y + 5f, 14f, 14f);
            Widgets.DrawBoxSolid(dot, dotColor);

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, headerColor)) {
                Widgets.Label(
                    new Rect(dot.xMax + 10f, header.y, header.width - dot.xMax - rect.x - 10f, header.height),
                    IdealLabel(order, idealNumber, achieved)
                );
            }

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

                using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, abilityColor)) {
                    Widgets.Label(
                        new Rect(
                            iconRect.xMax + 6f,
                            abilityRow.y,
                            abilityRow.width - iconRect.width - 6f,
                            abilityRow.height
                        ),
                        ability.LabelCap
                    );
                }

                y += 22f;
            }

            y += 6f;
        }
    }

    private static void DrawSprenStrip(Rect rect, List<Surgebinder> bonds, CodexState state) {
        Widgets.DrawBoxSolid(rect, new Color(0.06f, 0.06f, 0.08f, 0.8f));

        for (int i = 0; i < bonds.Count; i++) {
            Surgebinder gene = bonds[i];
            Pawn spren = gene.bondedSpren!;

            bool selected = state.SelectedSprenIndex == i;
            Color orderColor = gene.radiantOrderDef?.color ?? Color.white;

            Rect entryRect = new Rect(rect.x, rect.y + i * StripEntryHeight, rect.width, StripEntryHeight);

            if (selected) {
                Widgets.DrawBoxSolid(
                    entryRect,
                    new Color(orderColor.r * 0.25f, orderColor.g * 0.25f, orderColor.b * 0.25f, 0.9f)
                );
                Rect accent = new Rect(entryRect.x, entryRect.y, 3f, entryRect.height);
                Widgets.DrawBoxSolid(accent, orderColor);
            } else if (Mouse.IsOver(entryRect)) {
                Widgets.DrawHighlight(entryRect);
            }

            Rect swatch = new Rect(entryRect.x + 8f, entryRect.y + 8f, 12f, 12f);
            Widgets.DrawBoxSolid(swatch, orderColor);

            Rect nameRect = new Rect(
                entryRect.x + 6f,
                swatch.yMax + 4f,
                entryRect.width - 12f,
                entryRect.height - swatch.height - 16f
            );
            using (new TextBlock(
                       GameFont.Tiny,
                       TextAnchor.UpperLeft,
                       selected ? Color.white : new Color(0.8f, 0.8f, 0.8f)
                   ))
                Widgets.Label(nameRect, spren.Name?.ToStringShort ?? spren.LabelShortCap);

            if (Widgets.ButtonInvisible(entryRect)) {
                state.SelectedSprenIndex = i;
            }
        }
    }

    private static void DrawBondDetail(Rect rect, Surgebinder gene, CodexState state) {
        Pawn? spren = gene.bondedSpren;
        if (spren == null) return;

        SprenBond? bond = spren.TryGetComp<SprenBond>();
        if (bond?.BondedRadiant == null) return;

        Pawn radiant = bond.BondedRadiant;

        float contentHeight = SprenBondDetailRenderer.EstimateHeight(bond) + 16f;
        Rect viewRect = new Rect(0f, 0f, rect.width - 16f, contentHeight);
        Widgets.BeginScrollView(rect, ref state.BondDetailScroll, viewRect);
        SprenBondDetailRenderer.Render(viewRect, spren, radiant, bond, false, 73948202);
        Widgets.EndScrollView();
    }

    private void CollectBondedSurgebinders(Pawn pawn) {
        bondedSurgebindersBuffer.Clear();
        if (pawn.genes == null) return;
        List<Verse.Gene> genes = pawn.genes.GenesListForReading;
        for (int i = 0; i < genes.Count; i++) {
            if (genes[i] is Surgebinder s && !s.Overridden && s.bondedSpren != null) bondedSurgebindersBuffer.Add(s);
        }
    }

    private static Surgebinder? GetSurgebinder(Pawn pawn) {
        return pawn.genes?.GetFirstGeneOfType<Surgebinder>();
    }

    private static string IdealLabel(RadiantOrderDef order, int ideal, bool achieved) {
        if (achieved && order.ideals != null && ideal - 1 < order.ideals.Count) {
            return order.ideals[ideal - 1].label.CapitalizeFirst();
        }

        return (string)"CC_Codex_Surgebinding_IdealUnsworn".Translate(ideal.Named("IDEAL")).Resolve();
    }

    private static float EstimateProgressionHeight(RadiantOrderDef order, int CurrentIdealDisplay, float contentWidth) {
        float h = 34f + 30f;
        GameFont prevFont = Text.Font;
        Text.Font = GameFont.Tiny;
        for (int i = 0; i < 5; i++) {
            h += 26f;
            int idealNumber = i + 1;
            bool achieved = idealNumber <= CurrentIdealDisplay;
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

    public IReadOnlyList<AutocastTarget> AutocastTargets(Pawn pawn) {
        List<AutocastTarget> targets = [];
        List<RimWorld.Ability> all = pawn.abilities?.AllAbilitiesForReading ?? [];
        for (int i = 0; i < all.Count; i++) {
            if (!OwnsAbility(all[i])) continue;

            targets.Add(
                new AutocastTarget(
                    AutocastRuleKind.Ability,
                    all[i].def.defName,
                    all[i].def.LabelCap,
                    all[i].def.uiIcon
                )
            );
        }

        return targets;
    }
}
