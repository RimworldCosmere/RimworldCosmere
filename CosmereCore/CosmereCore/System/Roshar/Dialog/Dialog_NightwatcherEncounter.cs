using System.Collections.Generic;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Scadrial.Def;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Dialog;

public class Dialog_NightwatcherEncounter : Window {
    private enum Phase {
        Opening,
        BoonSelection,
        CurseReveal,
    }

    private static readonly Color NightwatcherGreen = new Color(0.35f, 0.82f, 0.55f);
    private static readonly Color CurseColor = new Color(0.72f, 0.28f, 0.92f);
    private static readonly Color EffectColor = new Color(0.9f, 0.75f, 0.4f);
    private static readonly Color MetalColor = new Color(0.6f, 0.75f, 0.9f);

    private readonly Verse.Pawn pawn;
    private Phase phase = Phase.Opening;
    private NightwatcherBoonDef? selectedBoon;
    private NightwatcherCurseDef? drawnCurse;
    private Vector2 boonScrollPos;
    private MetallicArtsMetalDef? selectedMetal;

    private List<NightwatcherBoonDef>? filteredBoons;

    private List<NightwatcherBoonDef> FilteredBoons {
        get {
            if (filteredBoons != null) return filteredBoons;
            List<NightwatcherBoonDef> all = DefDatabase<NightwatcherBoonDef>.AllDefsListForReading;
            filteredBoons = [];
            for (int i = 0; i < all.Count; i++) {
                if (all[i].requiresMod != null && !ModsConfig.IsActive(all[i].requiresMod)) continue;
                filteredBoons.Add(all[i]);
            }
            return filteredBoons;
        }
    }

    public override Vector2 InitialSize => new Vector2(720f, 680f);

    public Dialog_NightwatcherEncounter(Verse.Pawn pawn) {
        this.pawn = pawn;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
        doCloseX = false;
    }

    public override void DoWindowContents(Rect inRect) {
        switch (phase) {
            case Phase.Opening:
                DrawOpening(inRect);
                break;
            case Phase.BoonSelection:
                DrawBoonSelection(inRect);
                break;
            case Phase.CurseReveal:
                DrawCurseReveal(inRect);
                break;
        }
    }

    private void DrawOpening(Rect inRect) {
        float y = inRect.y + 20f;

        Text.Font = GameFont.Medium;
        GUI.color = NightwatcherGreen;
        Widgets.Label(new Rect(inRect.x, y, inRect.width, 40f),
            "Cosmere_Roshar_NW_Opening_Title".Translate());
        GUI.color = Color.white;
        y += 48f;

        Text.Font = GameFont.Small;
        GUI.color = new Color(0.85f, 0.85f, 0.85f);
        string openingText = "Cosmere_Roshar_NW_Opening_Text".Translate(pawn.Named("PAWN"));
        float textHeight = Text.CalcHeight(openingText, inRect.width - 20f);
        Widgets.Label(new Rect(inRect.x + 10f, y, inRect.width - 20f, textHeight), openingText);
        GUI.color = Color.white;
        y += textHeight + 20f;

        float btnW = 220f;
        float btnX = inRect.x + (inRect.width - btnW) / 2f;
        if (Widgets.ButtonText(new Rect(btnX, y, btnW, 36f),
                "Cosmere_Roshar_NW_Opening_Ask".Translate())) {
            phase = Phase.BoonSelection;
        }
    }

    private static readonly string[] TierLabels = ["Minor Boons", "Moderate Boons", "Major Boons"];
    private static readonly string[] TierDescs = [
        "Small gifts — a sharpened skill, a shifted temperament. The Nightwatcher gives these freely.",
        "Substantial changes — healed wounds, expanded capacity. These carry a heavier curse.",
        "Transformative power — bonds unlocked, minds awakened. The Old Magic exacts its highest price.",
    ];

    private void DrawBoonSelection(Rect inRect) {
        float y = inRect.y + 12f;

        Text.Font = GameFont.Medium;
        GUI.color = NightwatcherGreen;
        Widgets.Label(new Rect(inRect.x, y, inRect.width, 36f),
            "Cosmere_Roshar_NW_Boon_Title".Translate());
        GUI.color = Color.white;
        y += 44f;

        float listHeight = inRect.height - 56f - (y - inRect.y);
        Rect listRect = new Rect(inRect.x, y, inRect.width, listHeight);
        float contentWidth = listRect.width - 16f;

        List<NightwatcherBoonDef> boons = FilteredBoons;
        Text.Font = GameFont.Tiny;
        float totalHeight = 0f;
        int lastTier = 0;
        for (int i = 0; i < boons.Count; i++) {
            if (boons[i].powerTier != lastTier) {
                lastTier = boons[i].powerTier;
                float tierDescH = Text.CalcHeight(TierDescs[lastTier - 1], contentWidth - 32f);
                totalHeight += 28f + tierDescH + 8f;
            }
            string resolvedDesc = boons[i].description.Formatted(pawn.Named("PAWN"));
            float descHeight = Text.CalcHeight(resolvedDesc, contentWidth - 24f);
            string effects = GetBoonEffects(boons[i], selectedBoon == boons[i] ? selectedMetal : null);
            float effectsHeight = effects.Length > 0 ? Text.CalcHeight(effects, contentWidth - 24f) + 4f : 0f;
            totalHeight += 32f + descHeight + effectsHeight + 12f;
        }

        Rect viewRect = new Rect(0f, 0f, contentWidth, totalHeight);
        Widgets.BeginScrollView(listRect, ref boonScrollPos, viewRect);

        float rowY = 0f;
        lastTier = 0;
        for (int i = 0; i < boons.Count; i++) {
            NightwatcherBoonDef boon = boons[i];

            if (boon.powerTier != lastTier) {
                lastTier = boon.powerTier;
                Text.Font = GameFont.Small;
                GUI.color = new Color(0.7f, 0.7f, 0.7f);
                Widgets.Label(new Rect(8f, rowY + 8f, viewRect.width - 16f, 22f),
                    TierLabels[lastTier - 1]);
                Text.Font = GameFont.Tiny;
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                float tierDescH = Text.CalcHeight(TierDescs[lastTier - 1], viewRect.width - 32f);
                Widgets.Label(new Rect(16f, rowY + 26f, viewRect.width - 32f, tierDescH),
                    TierDescs[lastTier - 1]);
                GUI.color = Color.white;
                rowY += 28f + tierDescH + 8f;
            }

            Text.Font = GameFont.Tiny;
            string boonDesc = boon.description.Formatted(pawn.Named("PAWN"));
            float descHeight = Text.CalcHeight(boonDesc, viewRect.width - 24f);
            string effectsText = GetBoonEffects(boon, selectedBoon == boon ? selectedMetal : null);
            float effectsHeight = effectsText.Length > 0 ? Text.CalcHeight(effectsText, viewRect.width - 24f) + 4f : 0f;
            float rowHeight = 32f + descHeight + effectsHeight + 12f;
            Rect rowRect = new Rect(0f, rowY, viewRect.width, rowHeight);

            bool isSelected = selectedBoon == boon;
            if (isSelected) {
                Widgets.DrawBoxSolid(rowRect, new Color(0.2f, 0.45f, 0.3f, 0.4f));
            } else if (Mouse.IsOver(rowRect)) {
                Widgets.DrawBoxSolid(rowRect, new Color(0.3f, 0.3f, 0.3f, 0.3f));
            }

            Text.Font = GameFont.Small;
            GUI.color = NightwatcherGreen;
            Widgets.Label(new Rect(12f, rowRect.y + 4f, viewRect.width - 24f, 24f),
                boon.LabelCap);

            Text.Font = GameFont.Tiny;
            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            Widgets.Label(new Rect(12f, rowRect.y + 28f, viewRect.width - 24f, descHeight),
                boonDesc);

            if (effectsText.Length > 0) {
                GUI.color = EffectColor;
                Widgets.Label(new Rect(12f, rowRect.y + 28f + descHeight + 4f, viewRect.width - 24f, effectsHeight),
                    effectsText);
            }

            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            if (Widgets.ButtonInvisible(rowRect)) {
                if (boon.metalSelectionType != null) {
                    ShowMetalSelectionMenu(boon);
                } else {
                    selectedBoon = boon;
                    selectedMetal = null;
                }
            }

            Widgets.DrawLineHorizontal(4f, rowRect.yMax - 1f, rowRect.width - 8f);
            rowY += rowHeight;
        }
        Widgets.EndScrollView();

        y = listRect.yMax + 8f;

        bool canConfirm = selectedBoon != null
            && (selectedBoon.metalSelectionType == null || selectedMetal != null);
        if (canConfirm) {
            float btnW = 180f;
            float btnX = inRect.x + (inRect.width - btnW) / 2f;
            if (Widgets.ButtonText(new Rect(btnX, y, btnW, 36f),
                    "Cosmere_Roshar_NW_Boon_Confirm".Translate())) {
                Dictionary<string, object>? context = null;
                if (selectedMetal != null) {
                    context = new Dictionary<string, object> { ["SelectedMetal"] = selectedMetal };
                }
                NightwatcherSystem.ApplyBoon(pawn, selectedBoon!, context);
                drawnCurse = NightwatcherSystem.DrawCurse(selectedBoon!);
                phase = Phase.CurseReveal;
            }
        }
    }

    private void DrawCurseReveal(Rect inRect) {
        float y = inRect.y + 20f;

        Text.Font = GameFont.Medium;
        GUI.color = CurseColor;
        Widgets.Label(new Rect(inRect.x, y, inRect.width, 40f),
            "Cosmere_Roshar_NW_Curse_Title".Translate());
        GUI.color = Color.white;
        y += 48f;

        Text.Font = GameFont.Small;
        GUI.color = new Color(0.85f, 0.85f, 0.85f);
        string revealText = "Cosmere_Roshar_NW_Curse_Text".Translate(
            pawn.Named("PAWN"), selectedBoon!.Named("BOON"));
        float textHeight = Text.CalcHeight(revealText, inRect.width - 20f);
        Widgets.Label(new Rect(inRect.x + 10f, y, inRect.width - 20f, textHeight), revealText);
        GUI.color = Color.white;
        y += textHeight + 16f;

        if (drawnCurse != null) {
            Text.Font = GameFont.Medium;
            GUI.color = CurseColor;
            Widgets.Label(new Rect(inRect.x + 10f, y, inRect.width - 20f, 32f),
                drawnCurse.LabelCap);
            y += 34f;

            Text.Font = GameFont.Small;
            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            string curseDesc = drawnCurse.description.Formatted(pawn.Named("PAWN"));
            float curseDescH = Text.CalcHeight(curseDesc, inRect.width - 40f);
            Widgets.Label(new Rect(inRect.x + 20f, y, inRect.width - 40f, curseDescH),
                curseDesc);
            y += curseDescH + 6f;

            string curseEffects = GetCurseEffects(drawnCurse);
            if (curseEffects.Length > 0) {
                Text.Font = GameFont.Tiny;
                GUI.color = EffectColor;
                float effectsH = Text.CalcHeight(curseEffects, inRect.width - 40f);
                Widgets.Label(new Rect(inRect.x + 20f, y, inRect.width - 40f, effectsH),
                    curseEffects);
                y += effectsH + 8f;
            }

            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            y += 8f;
        }

        float btnW = 160f;
        float btnX = inRect.x + (inRect.width - btnW) / 2f;
        if (Widgets.ButtonText(new Rect(btnX, y, btnW, 36f),
                "Cosmere_Roshar_NW_Curse_Accept".Translate())) {
            if (drawnCurse != null) {
                NightwatcherSystem.ApplyCurse(pawn, drawnCurse);
            }

            CompNightwatcher? comp = pawn.TryGetComp<CompNightwatcher>();
            comp?.MarkVisited();

            SendResultLetter();
            Close();
        }
    }

    private void ShowMetalSelectionMenu(NightwatcherBoonDef boon) {
        List<MetallicArtsMetalDef> allMetals = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading;
        List<FloatMenuOption> options = [];
        for (int i = 0; i < allMetals.Count; i++) {
            MetallicArtsMetalDef metal = allMetals[i];
            if (boon.metalSelectionType == "allomancy" && metal.allomancy?.userName == null) continue;
            if (boon.metalSelectionType == "feruchemy" && metal.feruchemy == null) continue;

            string userName = boon.metalSelectionType == "allomancy"
                ? metal.allomancy!.userName!
                : metal.feruchemy!.userName ?? metal.LabelCap;
            string label = $"{userName} ({metal.LabelCap})";
            options.Add(new FloatMenuOption(label, () => {
                selectedBoon = boon;
                selectedMetal = metal;
            }));
        }
        Find.WindowStack.Add(new FloatMenu(options));
    }

    private static string GetBoonEffects(NightwatcherBoonDef boon, MetallicArtsMetalDef? metal = null) {
        List<string> effects = [];

        for (int i = 0; i < boon.skillBoosts.Count; i++) {
            BoonSkillBoost boost = boon.skillBoosts[i];
            effects.Add($"+{boost.levels} {boost.skill.LabelCap}");
        }

        if (boon.grantTrait != null)
            effects.Add($"Gains: {boon.grantTrait.DataAtDegree(boon.grantTraitDegree).label.CapitalizeFirst()}");

        if (boon.removeTrait != null)
            effects.Add($"Removes: {boon.removeTrait.degreeDatas[0].label.CapitalizeFirst()}");

        if (boon.removeHediff != null)
            effects.Add($"Cures: {boon.removeHediff.LabelCap}");

        if (boon.investitureBonus > 0f)
            effects.Add($"+{boon.investitureBonus:0} Investiture capacity");

        if (boon.surgebindingConnectionBoost > 0f)
            effects.Add($"+{boon.surgebindingConnectionBoost:0.#} Cultivation connection");

        if (boon.psylinkBoost)
            effects.Add("+1 Psylink level");

        if (boon.hediff != null) {
            AppendHediffEffects(boon.hediff, effects);
            if (boon.hediff.HasComp(typeof(Comp.Hediff.HediffComp_AgelessBody)))
                effects.Add("Biological immortality");
        }

        if (boon.applicatorClass?.Name == "HealChronicApplicator")
            effects.Add("Heals one chronic condition");
        else if (boon.applicatorClass?.Name == "GriefReliefApplicator")
            effects.Add("Clears all negative memories");
        else if (boon.applicatorClass?.Name == "MistbornApplicator")
            effects.Add("Grants all Allomantic powers");
        else if (boon.applicatorClass?.Name == "MistingApplicator")
            effects.Add(metal != null ? $"Grants Allomancy: {metal.allomancy?.userName} ({metal.LabelCap})" : "Grants one Allomantic power (choose metal)");
        else if (boon.applicatorClass?.Name == "FullFeruchemistApplicator")
            effects.Add("Grants all Feruchemical powers");
        else if (boon.applicatorClass?.Name == "FerringApplicator")
            effects.Add(metal != null ? $"Grants Feruchemy: {metal.feruchemy?.userName ?? metal.LabelCap} ({metal.LabelCap})" : "Grants one Feruchemical power (choose metal)");

        return effects.Count > 0 ? string.Join("  |  ", effects) : "";
    }

    private static string GetCurseEffects(NightwatcherCurseDef curse) {
        List<string> effects = [];

        if (curse.forceTrait != null) {
            TraitDegreeData? degreeData = curse.forceTrait.degreeDatas
                .FirstOrDefault(d => d.degree == curse.forceTraitDegree)
                ?? curse.forceTrait.degreeDatas.FirstOrDefault();
            if (degreeData != null)
                effects.Add($"Gains: {degreeData.label.CapitalizeFirst()}");
        }

        if (curse.stripTrait != null && curse.stripTrait.degreeDatas.Count > 0)
            effects.Add($"Loses: {curse.stripTrait.degreeDatas[0].label.CapitalizeFirst()}");

        if (curse.penaltySkill != null)
            effects.Add($"-{curse.penaltySkillLevels} {curse.penaltySkill.LabelCap}");

        if (curse.hediff != null && !AppendHediffEffects(curse.hediff, effects))
            effects.Add(curse.hediff.LabelCap);

        if (curse.applicatorClass?.Name == "MemoryLossApplicator")
            effects.Add("All skills reset to 4");
        else if (curse.applicatorClass?.Name == "NarcolepsyApplicator")
            effects.Add("Random narcoleptic episodes");

        if (curse.cultivationEvolutionDays > 0) {
            float years = curse.cultivationEvolutionDays / 365f;
            effects.Add($"Evolves after {years:0.#} years");
        }

        return effects.Count > 0 ? string.Join("  |  ", effects) : "";
    }

    private static string GetHediffLabel(HediffDef hediff) {
        if (hediff.stages != null && hediff.stages.Count > 0 && hediff.stages[0].label != null)
            return hediff.stages[0].label.CapitalizeFirst();
        return hediff.LabelCap;
    }

    private static bool AppendHediffEffects(HediffDef hediff, List<string> effects) {
        if (hediff.stages == null || hediff.stages.Count == 0) return false;
        HediffStage stage = hediff.stages[0];
        int startCount = effects.Count;

        if (stage.statOffsets != null) {
            for (int i = 0; i < stage.statOffsets.Count; i++) {
                StatModifier mod = stage.statOffsets[i];
                string sign = mod.value >= 0 ? "+" : "";
                effects.Add($"{sign}{mod.value:0.##} {mod.stat.LabelCap}");
            }
        }

        if (stage.statFactors != null) {
            for (int i = 0; i < stage.statFactors.Count; i++) {
                StatModifier mod = stage.statFactors[i];
                float pct = (mod.value - 1f) * 100f;
                string sign = pct >= 0 ? "+" : "";
                effects.Add($"{sign}{pct:0.#}% {mod.stat.LabelCap}");
            }
        }

        if (stage.capMods != null) {
            for (int i = 0; i < stage.capMods.Count; i++) {
                PawnCapacityModifier cap = stage.capMods[i];
                if (cap.offset != 0f) {
                    string sign = cap.offset >= 0 ? "+" : "";
                    effects.Add($"{sign}{cap.offset * 100f:0.#}% {cap.capacity.LabelCap}");
                }
            }
        }

        if (stage.painOffset != 0f) {
            string sign = stage.painOffset >= 0 ? "+" : "";
            effects.Add($"{sign}{stage.painOffset:0.##} Pain");
        }

        return effects.Count > startCount;
    }

    private void SendResultLetter() {
        string boonLabel = selectedBoon?.LabelCap ?? "";
        string curseLabel = drawnCurse?.LabelCap ?? "";

        Find.LetterStack.ReceiveLetter(
            "Cosmere_Roshar_NW_Letter_Title".Translate(pawn.Named("PAWN")),
            "Cosmere_Roshar_NW_Letter_Text".Translate(
                pawn.Named("PAWN"), boonLabel.Named("BOON"), curseLabel.Named("CURSE")),
            RimWorld.LetterDefOf.PositiveEvent,
            pawn
        );
    }
}
