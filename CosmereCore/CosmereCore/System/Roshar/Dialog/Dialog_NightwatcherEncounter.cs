using Cosmere.Core.Nightwatcher;
using Cosmere.System.Roshar.Comp.Hediff;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Nightwatcher;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Dialog;

public class Dialog_NightwatcherEncounter : Window {
    private static readonly Color NightwatcherGreen = new Color(0.35f, 0.82f, 0.55f);
    private static readonly Color CurseColor = new Color(0.72f, 0.28f, 0.92f);
    private static readonly Color EffectColor = new Color(0.9f, 0.75f, 0.4f);
    private static readonly Color MetalColor = new Color(0.6f, 0.75f, 0.9f);

    private static string GetTierLabel(int tier) => tier switch {
        1 => "Cosmere_Roshar_NW_Tier1_Label".Translate(),
        2 => "Cosmere_Roshar_NW_Tier2_Label".Translate(),
        _ => "Cosmere_Roshar_NW_Tier3_Label".Translate(),
    };

    private static string GetTierDesc(int tier) => tier switch {
        1 => "Cosmere_Roshar_NW_Tier1_Desc".Translate(),
        2 => "Cosmere_Roshar_NW_Tier2_Desc".Translate(),
        _ => "Cosmere_Roshar_NW_Tier3_Desc".Translate(),
    };

    private readonly Pawn pawn;
    private Vector2 boonScrollPos;
    private NightwatcherCurseDef? drawnCurse;

    private List<NightwatcherBoonDef>? filteredBoons;
    private Phase phase = Phase.Opening;
    private NightwatcherBoonDef? selectedBoon;
    private string? selectedChoiceKey;

    public Dialog_NightwatcherEncounter(Pawn pawn) {
        this.pawn = pawn;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
        doCloseX = false;
    }

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
        Widgets.Label(
            new Rect(inRect.x, y, inRect.width, 40f),
            "Cosmere_Roshar_NW_Opening_Title".Translate()
        );
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
        if (Widgets.ButtonText(
                new Rect(btnX, y, btnW, 36f),
                "Cosmere_Roshar_NW_Opening_Ask".Translate()
            )) {
            phase = Phase.BoonSelection;
        }
    }

    private struct BoonRowLayout {
        public bool isNewTier;
        public int tier;
        public float tierDescHeight;
        public string descText;
        public float descHeight;
        public string effectsText;
        public float effectsHeight;
        public float rowHeight;
    }

    private readonly List<BoonRowLayout> rowLayouts = [];

    private void DrawBoonSelection(Rect inRect) {
        float y = inRect.y + 12f;

        Text.Font = GameFont.Medium;
        GUI.color = NightwatcherGreen;
        Widgets.Label(
            new Rect(inRect.x, y, inRect.width, 36f),
            "Cosmere_Roshar_NW_Boon_Title".Translate()
        );
        GUI.color = Color.white;
        y += 44f;

        float listHeight = inRect.height - 56f - (y - inRect.y);
        Rect listRect = new Rect(inRect.x, y, inRect.width, listHeight);
        float contentWidth = listRect.width - 16f;

        List<NightwatcherBoonDef> boons = FilteredBoons;
        Text.Font = GameFont.Tiny;

        rowLayouts.Clear();
        float totalHeight = 0f;
        int lastTier = 0;
        for (int i = 0; i < boons.Count; i++) {
            NightwatcherBoonDef boon = boons[i];
            BoonRowLayout layout = default;
            if (boon.powerTier != lastTier) {
                lastTier = boon.powerTier;
                layout.isNewTier = true;
                layout.tier = lastTier;
                layout.tierDescHeight = Text.CalcHeight(GetTierDesc(lastTier), contentWidth - 32f);
                totalHeight += 28f + layout.tierDescHeight + 8f;
            }

            layout.descText = boon.description.Formatted(pawn.Named("PAWN"));
            layout.descHeight = Text.CalcHeight(layout.descText, contentWidth - 24f);
            layout.effectsText = GetBoonEffects(boon, selectedBoon == boon ? selectedChoiceKey : null);
            layout.effectsHeight = layout.effectsText.Length > 0
                ? Text.CalcHeight(layout.effectsText, contentWidth - 24f) + 4f
                : 0f;
            layout.rowHeight = 32f + layout.descHeight + layout.effectsHeight + 12f;
            totalHeight += layout.rowHeight;
            rowLayouts.Add(layout);
        }

        Rect viewRect = new Rect(0f, 0f, contentWidth, totalHeight);
        Widgets.BeginScrollView(listRect, ref boonScrollPos, viewRect);

        float rowY = 0f;
        for (int i = 0; i < boons.Count; i++) {
            NightwatcherBoonDef boon = boons[i];
            BoonRowLayout layout = rowLayouts[i];

            if (layout.isNewTier) {
                Text.Font = GameFont.Small;
                GUI.color = new Color(0.7f, 0.7f, 0.7f);
                Widgets.Label(
                    new Rect(8f, rowY + 8f, viewRect.width - 16f, 22f),
                    GetTierLabel(layout.tier)
                );
                Text.Font = GameFont.Tiny;
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                Widgets.Label(
                    new Rect(16f, rowY + 26f, viewRect.width - 32f, layout.tierDescHeight),
                    GetTierDesc(layout.tier)
                );
                GUI.color = Color.white;
                rowY += 28f + layout.tierDescHeight + 8f;
            }

            Text.Font = GameFont.Tiny;
            string boonDesc = layout.descText;
            float descHeight = layout.descHeight;
            string effectsText = layout.effectsText;
            float effectsHeight = layout.effectsHeight;
            float rowHeight = layout.rowHeight;
            Rect rowRect = new Rect(0f, rowY, viewRect.width, rowHeight);

            bool isSelected = selectedBoon == boon;
            if (isSelected) {
                Widgets.DrawBoxSolid(rowRect, new Color(0.2f, 0.45f, 0.3f, 0.4f));
            } else if (Mouse.IsOver(rowRect)) {
                Widgets.DrawBoxSolid(rowRect, new Color(0.3f, 0.3f, 0.3f, 0.3f));
            }

            Text.Font = GameFont.Small;
            GUI.color = NightwatcherGreen;
            Widgets.Label(
                new Rect(12f, rowRect.y + 4f, viewRect.width - 24f, 24f),
                boon.LabelCap
            );

            Text.Font = GameFont.Tiny;
            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            Widgets.Label(
                new Rect(12f, rowRect.y + 28f, viewRect.width - 24f, descHeight),
                boonDesc
            );

            if (effectsText.Length > 0) {
                GUI.color = EffectColor;
                Widgets.Label(
                    new Rect(12f, rowRect.y + 28f + descHeight + 4f, viewRect.width - 24f, effectsHeight),
                    effectsText
                );
            }

            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            if (Widgets.ButtonInvisible(rowRect)) {
                if (boon.Applicator is INightwatcherChoiceProvider) {
                    ShowChoiceMenu(boon);
                } else {
                    selectedBoon = boon;
                    selectedChoiceKey = null;
                }
            }

            Widgets.DrawLineHorizontal(4f, rowRect.yMax - 1f, rowRect.width - 8f);
            rowY += rowHeight;
        }

        Widgets.EndScrollView();

        y = listRect.yMax + 8f;

        bool needsChoice = selectedBoon?.Applicator is INightwatcherChoiceProvider;
        bool canConfirm = selectedBoon != null && (!needsChoice || selectedChoiceKey != null);
        if (canConfirm) {
            float btnW = 180f;
            float btnX = inRect.x + (inRect.width - btnW) / 2f;
            if (Widgets.ButtonText(
                    new Rect(btnX, y, btnW, 36f),
                    "Cosmere_Roshar_NW_Boon_Confirm".Translate()
                )) {
                NightwatcherApplicationContext? context = selectedChoiceKey != null
                    ? new NightwatcherApplicationContext(selectedChoiceKey)
                    : null;
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
        Widgets.Label(
            new Rect(inRect.x, y, inRect.width, 40f),
            "Cosmere_Roshar_NW_Curse_Title".Translate()
        );
        GUI.color = Color.white;
        y += 48f;

        Text.Font = GameFont.Small;
        GUI.color = new Color(0.85f, 0.85f, 0.85f);
        string revealText = "Cosmere_Roshar_NW_Curse_Text".Translate(
            pawn.Named("PAWN"),
            selectedBoon!.Named("BOON")
        );
        float textHeight = Text.CalcHeight(revealText, inRect.width - 20f);
        Widgets.Label(new Rect(inRect.x + 10f, y, inRect.width - 20f, textHeight), revealText);
        GUI.color = Color.white;
        y += textHeight + 16f;

        if (drawnCurse != null) {
            Text.Font = GameFont.Medium;
            GUI.color = CurseColor;
            Widgets.Label(
                new Rect(inRect.x + 10f, y, inRect.width - 20f, 32f),
                drawnCurse.LabelCap
            );
            y += 34f;

            Text.Font = GameFont.Small;
            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            string curseDesc = drawnCurse.description.Formatted(pawn.Named("PAWN"));
            float curseDescH = Text.CalcHeight(curseDesc, inRect.width - 40f);
            Widgets.Label(
                new Rect(inRect.x + 20f, y, inRect.width - 40f, curseDescH),
                curseDesc
            );
            y += curseDescH + 6f;

            string curseEffects = GetCurseEffects(drawnCurse);
            if (curseEffects.Length > 0) {
                Text.Font = GameFont.Tiny;
                GUI.color = EffectColor;
                float effectsH = Text.CalcHeight(curseEffects, inRect.width - 40f);
                Widgets.Label(
                    new Rect(inRect.x + 20f, y, inRect.width - 40f, effectsH),
                    curseEffects
                );
                y += effectsH + 8f;
            }

            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            y += 8f;
        }

        float btnW = 160f;
        float btnX = inRect.x + (inRect.width - btnW) / 2f;
        if (Widgets.ButtonText(
                new Rect(btnX, y, btnW, 36f),
                "Cosmere_Roshar_NW_Curse_Accept".Translate()
            )) {
            if (drawnCurse != null) {
                NightwatcherSystem.ApplyCurse(pawn, drawnCurse);
            }

            NightwatcherVisit? comp = pawn.TryGetComp<NightwatcherVisit>();
            comp?.MarkVisited();

            SendResultLetter();
            Close();
        }
    }

    private void ShowChoiceMenu(NightwatcherBoonDef boon) {
        if (boon.Applicator is not INightwatcherChoiceProvider provider) return;
        List<FloatMenuOption> options = [];
        foreach (NightwatcherChoice choice in provider.GetChoices(boon)) {
            NightwatcherChoice captured = choice;
            options.Add(
                new FloatMenuOption(
                    captured.Label,
                    () => {
                        selectedBoon = boon;
                        selectedChoiceKey = captured.Key;
                    }
                )
            );
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private static string GetBoonEffects(NightwatcherBoonDef boon, string? choiceKey = null) {
        List<string> effects = [];

        for (int i = 0; i < boon.skillBoosts.Count; i++) {
            BoonSkillBoost boost = boon.skillBoosts[i];
            effects.Add("CRO_NW_Effect_SkillBoost".Translate(
                boost.levels.Named("LEVELS"),
                boost.skill.LabelCap.Named("SKILL")
            ));
        }

        if (boon.grantTrait != null) {
            string label = boon.grantTrait.DataAtDegree(boon.grantTraitDegree).label.CapitalizeFirst();
            effects.Add("CRO_NW_Effect_Gains".Translate(label.Named("LABEL")));
        }

        if (boon.removeTrait != null) {
            string label = boon.removeTrait.degreeDatas[0].label.CapitalizeFirst();
            effects.Add("CRO_NW_Effect_Removes".Translate(label.Named("LABEL")));
        }

        if (boon.removeHediff != null) {
            effects.Add("CRO_NW_Effect_Cures".Translate(boon.removeHediff.LabelCap.Named("LABEL")));
        }

        if (boon.investitureBonus > 0f) {
            effects.Add("CRO_NW_Effect_InvestitureCapacity".Translate(
                ((int)boon.investitureBonus).Named("AMOUNT")
            ));
        }

        if (boon.surgebindingConnectionBoost > 0f) {
            effects.Add("CRO_NW_Effect_CultivationConnection".Translate(
                boon.surgebindingConnectionBoost.Named("AMOUNT")
            ));
        }

        if (boon.psylinkBoost) {
            effects.Add("CRO_NW_Effect_PsylinkLevel".Translate());
        }

        if (boon.hediff != null) {
            AppendHediffEffects(boon.hediff, effects);
            if (boon.hediff.HasComp(typeof(AgelessBody))) {
                effects.Add("CRO_NW_Effect_BiologicalImmortality".Translate());
            }
        }

        if (boon.Applicator is INightwatcherEffectDescriber describer) {
            NightwatcherApplicationContext? ctx = choiceKey != null
                ? new NightwatcherApplicationContext(choiceKey)
                : null;
            string? extra = describer.DescribeEffects(ctx);
            if (!string.IsNullOrEmpty(extra)) effects.Add(extra!);
        }

        return effects.Count > 0 ? string.Join("  |  ", effects) : string.Empty;
    }

    private static string GetCurseEffects(NightwatcherCurseDef curse) {
        List<string> effects = [];

        if (curse.forceTrait != null) {
            TraitDegreeData? degreeData = curse.forceTrait.degreeDatas
                                              .FirstOrDefault(d => d.degree == curse.forceTraitDegree) ??
                                          curse.forceTrait.degreeDatas.FirstOrDefault();
            if (degreeData != null) {
                effects.Add("CRO_NW_Effect_Gains".Translate(degreeData.label.CapitalizeFirst().Named("LABEL")));
            }
        }

        if (curse.stripTrait != null && curse.stripTrait.degreeDatas.Count > 0) {
            string label = curse.stripTrait.degreeDatas[0].label.CapitalizeFirst();
            effects.Add("CRO_NW_Effect_Loses".Translate(label.Named("LABEL")));
        }

        if (curse.penaltySkill != null) {
            effects.Add("CRO_NW_Effect_SkillPenalty".Translate(
                curse.penaltySkillLevels.Named("LEVELS"),
                curse.penaltySkill.LabelCap.Named("SKILL")
            ));
        }

        if (curse.hediff != null && !AppendHediffEffects(curse.hediff, effects)) {
            effects.Add(curse.hediff.LabelCap);
        }

        if (curse.Applicator is INightwatcherEffectDescriber describer) {
            string? extra = describer.DescribeEffects();
            if (!string.IsNullOrEmpty(extra)) effects.Add(extra!);
        }

        if (curse.cultivationEvolutionDays > 0) {
            float years = curse.cultivationEvolutionDays / 365f;
            effects.Add("CRO_NW_Effect_EvolvesAfterYears".Translate(years.Named("YEARS")));
        }

        return effects.Count > 0 ? string.Join("  |  ", effects) : string.Empty;
    }

    private static string GetHediffLabel(HediffDef hediff) {
        if (hediff.stages != null && hediff.stages.Count > 0 && hediff.stages[0].label != null) {
            return hediff.stages[0].label.CapitalizeFirst();
        }

        return hediff.LabelCap;
    }

    private static bool AppendHediffEffects(HediffDef hediff, List<string> effects) {
        if (hediff.stages == null || hediff.stages.Count == 0) return false;
        HediffStage stage = hediff.stages[0];
        int startCount = effects.Count;

        if (stage.statOffsets != null) {
            for (int i = 0; i < stage.statOffsets.Count; i++) {
                StatModifier mod = stage.statOffsets[i];
                string sign = mod.value >= 0 ? "+" : string.Empty;
                effects.Add("CRO_NW_Effect_StatOffset".Translate(
                    sign.Named("SIGN"),
                    mod.value.Named("VALUE"),
                    mod.stat.LabelCap.Named("STAT")
                ));
            }
        }

        if (stage.statFactors != null) {
            for (int i = 0; i < stage.statFactors.Count; i++) {
                StatModifier mod = stage.statFactors[i];
                float pct = (mod.value - 1f) * 100f;
                string sign = pct >= 0 ? "+" : string.Empty;
                effects.Add("CRO_NW_Effect_StatFactor".Translate(
                    sign.Named("SIGN"),
                    pct.Named("VALUE"),
                    mod.stat.LabelCap.Named("STAT")
                ));
            }
        }

        if (stage.capMods != null) {
            for (int i = 0; i < stage.capMods.Count; i++) {
                PawnCapacityModifier cap = stage.capMods[i];
                if (cap.offset != 0f) {
                    string sign = cap.offset >= 0 ? "+" : string.Empty;
                    effects.Add("CRO_NW_Effect_CapacityOffset".Translate(
                        sign.Named("SIGN"),
                        (cap.offset * 100f).Named("VALUE"),
                        cap.capacity.LabelCap.Named("CAPACITY")
                    ));
                }
            }
        }

        if (stage.painOffset != 0f) {
            string sign = stage.painOffset >= 0 ? "+" : string.Empty;
            effects.Add("CRO_NW_Effect_PainOffset".Translate(
                sign.Named("SIGN"),
                stage.painOffset.Named("VALUE")
            ));
        }

        return effects.Count > startCount;
    }

    private void SendResultLetter() {
        string boonLabel = selectedBoon?.LabelCap ?? string.Empty;
        string curseLabel = drawnCurse?.LabelCap ?? string.Empty;

        Find.LetterStack.ReceiveLetter(
            "Cosmere_Roshar_NW_Letter_Title".Translate(pawn.Named("PAWN")),
            "Cosmere_Roshar_NW_Letter_Text".Translate(
                pawn.Named("PAWN"),
                boonLabel.Named("BOON"),
                curseLabel.Named("CURSE")
            ),
            RimWorld.LetterDefOf.PositiveEvent,
            pawn
        );
    }

    private enum Phase {
        Opening,
        BoonSelection,
        CurseReveal,
    }
}
