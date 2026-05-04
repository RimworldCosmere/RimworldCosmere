using RimWorld;
using UnityEngine;
using System.Text;
using Cosmere.Core.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Hediff;
using Cosmere.System.Roshar.Surgebinding;
using Verse;

namespace Cosmere.System.Roshar.UI;

[StaticConstructorOnStartup]
public static class SprenBondDetailRenderer {
    public const float HeaderHeight = 30f;
    public const float HeaderAdvance = 35f;
    public const float RenameSize = 24f;
    public const float RenameGap = 4f;
    public const float RenameYOffset = 3f;
    public const float RowHeight = 24f;
    public const float RowAdvance = 28f;
    public const float SectionGap = 8f;
    public const float TraitHeaderAdvance = 26f;
    public const float StrengthLabelWidth = 120f;
    public const float StrengthBarYOffset = 2f;
    public const float StrengthBarHeight = 20f;
    public const float StrengthBarRightPad = 180f;
    public const float StrengthPercentGap = 4f;
    public const float StrengthPercentWidth = 50f;
    public const float TraitIndent = 10f;

    public static float EstimateHeight(SprenBond bond) {
        float h = HeaderAdvance +
                  RowAdvance +
                  RowAdvance +
                  RowAdvance +
                  SectionGap +
                  TraitHeaderAdvance;
        if (bond.Dismissed) h += RowAdvance;
        h += bond.PersonalityTraits.Count == 0 ? RowHeight : bond.PersonalityTraits.Count * RowHeight;
        return h;
    }

    public static void Render(
        Rect rect,
        Pawn spren,
        Pawn radiant,
        SprenBond bond,
        bool isSprenSide,
        int tooltipHashSalt
    ) {
        float y = rect.y;

        using (new TextBlock(GameFont.Medium)) {
            Rect headerRect = new Rect(rect.x, y, rect.width - RenameSize - RenameGap, HeaderHeight);
            Widgets.Label(headerRect, spren.NameFullColored);

            Rect renameRect = new Rect(headerRect.xMax + RenameGap, y + RenameYOffset, RenameSize, RenameSize);
            if (Widgets.ButtonImage(renameRect, TexButton.Rename)) {
                Find.WindowStack.Add(new Dialog_NameSprenDialog(spren));
            }

            TooltipHandler.TipRegion(renameRect, "CRO_Spren_Rename".Translate());
            y += HeaderAdvance;
        }

        using (new TextBlock(GameFont.Small)) {
            Pawn otherPawn = isSprenSide ? radiant : spren;
            string bondLabel = isSprenSide
                ? "CRO_SprenBond_BondedTo".Translate(radiant.NameFullColored.Named("PAWN"))
                : "CRO_Spren_BondedSpren".Translate(spren.NameFullColored.Named("SPREN"));
            Rect bondRect = new Rect(rect.x, y, rect.width, RowHeight);
            Widgets.Label(bondRect, bondLabel);
            if (Widgets.ButtonInvisible(bondRect)) {
                CameraJumper.TryJumpAndSelect(otherPawn);
            }

            if (Mouse.IsOver(bondRect)) {
                Widgets.DrawHighlight(bondRect);
            }

            y += RowAdvance;

            float connection = SpiritWeb.Instance?.GetConnectionValue(radiant, spren) ?? 0f;
            int percentage = (int)(connection * 100f);
            string stage = connection switch {
                >= 0.7f => "CRO_BondStage_Healthy".Translate(),
                >= 0.4f => "CRO_BondStage_Strained".Translate(),
                >= 0.15f => "CRO_BondStage_Fractured".Translate(),
                _ => "CRO_BondStage_Breaking".Translate(),
            };

            Rect strengthLabelRect = new Rect(rect.x, y, StrengthLabelWidth, RowHeight);
            Widgets.Label(strengthLabelRect, "CRO_SprenBond_Strength".Translate());

            Rect barRect = new Rect(
                rect.x + StrengthLabelWidth,
                y + StrengthBarYOffset,
                rect.width - StrengthBarRightPad,
                StrengthBarHeight
            );
            Widgets.FillableBar(barRect, connection, SprenBondUI.GetBondBarTexture(connection));

            Rect percentRect = new Rect(barRect.xMax + StrengthPercentGap, y, StrengthPercentWidth, RowHeight);
            Widgets.Label(percentRect, $"{percentage}%");

            Rect strengthTooltipRect = new Rect(rect.x, y, rect.width, RowHeight);
            TooltipHandler.TipRegion(strengthTooltipRect, () => SprenBondUI.BuildStrengthTooltip(radiant, connection), tooltipHashSalt);
            y += RowAdvance;

            Rect stageRect = new Rect(rect.x, y, rect.width, RowHeight);
            Widgets.Label(stageRect, "CRO_SprenBond_Status".Translate(stage));
            y += RowAdvance;

            if (bond.Dismissed) {
                Rect dismissedRect = new Rect(rect.x, y, rect.width, RowHeight);
                Widgets.Label(dismissedRect, "CRO_SprenBond_Dismissed".Translate().Colorize(ColorLibrary.RedReadable));
                y += RowAdvance;
            }

            y += SectionGap;
            Rect traitHeaderRect = new Rect(rect.x, y, rect.width, RowHeight);
            Widgets.Label(
                traitHeaderRect,
                "CRO_SprenBond_Personality".Translate().Colorize(ColoredText.TipSectionTitleColor)
            );
            y += TraitHeaderAdvance;

            if (bond.PersonalityTraits.Count == 0) {
                Widgets.Label(
                    new Rect(rect.x + TraitIndent, y, rect.width - TraitIndent, RowHeight),
                    "CRO_SprenBond_NoTraits".Translate()
                );
            }
            else {
                for (int i = 0; i < bond.PersonalityTraits.Count; i++) {
                    Rect traitRect = new Rect(rect.x + TraitIndent, y, rect.width - TraitIndent, RowHeight);
                    TraitDef traitDef = bond.PersonalityTraits[i];
                    string traitLabel = traitDef.degreeDatas.Count > 0
                        ? traitDef.degreeDatas[0].LabelCap
                        : traitDef.LabelCap;
                    Widgets.Label(traitRect, "- " + traitLabel);
                    y += RowHeight;
                }
            }
        }
    }

}
