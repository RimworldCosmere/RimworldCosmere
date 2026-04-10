using Cosmere.Core.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Tab;

public class ITab_SprenBond : ITab {
    private CompSprenBond? SprenBond {
        get {
            CompSprenBond? direct = SelPawn?.TryGetComp<CompSprenBond>();
            if (direct != null) return direct;

            Surgebinder? surgebinder = SelPawn?.genes?.GetFirstGeneOfType<Surgebinder>();
            return surgebinder?.bondedSpren?.TryGetComp<CompSprenBond>();
        }
    }

    private bool IsSprenSide => SelPawn?.TryGetComp<CompSprenBond>() != null;

    public override bool IsVisible => SprenBond?.BondedRadiant != null;

    public ITab_SprenBond() {
        labelKey = "CRO_Spren_Tab";
        size = new Vector2(400f, 320f);
    }

    protected override void FillTab() {
        CompSprenBond? bond = SprenBond;
        if (bond?.BondedRadiant == null) return;

        Verse.Pawn spren = (Verse.Pawn)bond.parent;
        Verse.Pawn radiant = bond.BondedRadiant;

        Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(17f);
        float y = rect.y;

        using (new TextBlock(GameFont.Medium)) {
            float renameSize = 24f;
            Rect headerRect = new Rect(rect.x, y, rect.width - renameSize - 4f, 30f);
            Widgets.Label(headerRect, spren.NameFullColored);

            Rect renameRect = new Rect(headerRect.xMax + 4f, y + 3f, renameSize, renameSize);
            if (Widgets.ButtonImage(renameRect, TexButton.Rename)) {
                Find.WindowStack.Add(new Dialog.NameSprenDialog(spren));
            }
            TooltipHandler.TipRegion(renameRect, "CRO_Spren_Rename".Translate());
            y += 35f;
        }

        using (new TextBlock(GameFont.Small)) {
            Verse.Pawn otherPawn = IsSprenSide ? radiant : spren;
            string bondLabel = IsSprenSide
                ? "CRO_SprenBond_BondedTo".Translate(radiant.NameFullColored.Named("PAWN"))
                : "CRO_Spren_BondedSpren".Translate(spren.NameFullColored.Named("SPREN"));
            Rect bondRect = new Rect(rect.x, y, rect.width, 24f);
            Widgets.Label(bondRect, bondLabel);
            if (Widgets.ButtonInvisible(bondRect)) {
                CameraJumper.TryJumpAndSelect(otherPawn);
            }
            if (Mouse.IsOver(bondRect)) {
                Widgets.DrawHighlight(bondRect);
            }
            y += 28f;

            float connection = SpiritWeb.Instance.GetConnectionValue(radiant, spren);
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
            Widgets.FillableBar(barRect, connection, SolidColorMaterials.NewSolidColorTexture(GetBondColor(connection)));

            Rect percentRect = new Rect(barRect.xMax + 4f, y, 50f, 24f);
            Widgets.Label(percentRect, $"{percentage}%");

            Rect strengthTooltipRect = new Rect(rect.x, y, rect.width, 24f);
            TooltipHandler.TipRegion(strengthTooltipRect, () => BuildStrengthTooltip(radiant, connection), 73948201);
            y += 28f;

            Rect stageRect = new Rect(rect.x, y, rect.width, 24f);
            Widgets.Label(stageRect, "CRO_SprenBond_Status".Translate(stage));
            y += 28f;

            if (bond.Dismissed) {
                Rect dismissedRect = new Rect(rect.x, y, rect.width, 24f);
                Widgets.Label(dismissedRect, "CRO_SprenBond_Dismissed".Translate().Colorize(ColorLibrary.RedReadable));
                y += 28f;
            }

            y += 8f;
            Rect traitHeaderRect = new Rect(rect.x, y, rect.width, 24f);
            Widgets.Label(traitHeaderRect, "CRO_SprenBond_Personality".Translate().Colorize(ColoredText.TipSectionTitleColor));
            y += 26f;

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

    private static string BuildStrengthTooltip(Verse.Pawn radiant, float connection) {
        if (connection >= 1f) {
            return "CRO_SprenBond_StrengthFull".Translate();
        }

        string tip = "CRO_SprenBond_StrengthTooltip".Translate();

        bool hasStrainedBond = false;
        List<Verse.Hediff> hediffs = radiant.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++) {
            if (hediffs[i] is Roshar.Hediff.StrainedBond strained) {
                hasStrainedBond = true;
                tip += $"\n  - Strained bond (recovering)";
                break;
            }
        }

        List<LogEntry> logs = Find.PlayLog.AllEntries;
        int violationsShown = 0;
        for (int i = 0; i < logs.Count && violationsShown < 5; i++) {
            if (logs[i] is not Surgebinding.BondViolationLogEntry violation) continue;
            if (violation.pawn != radiant) continue;

            tip += $"\n  - {violation.reason} ({violation.severityLabel})";
            violationsShown++;
        }

        if (!hasStrainedBond && violationsShown == 0) {
            tip += "\n  - Unknown factors";
        }

        return tip;
    }

    private static Color GetBondColor(float connection) {
        if (connection >= 0.7f) return new Color(0.2f, 0.8f, 0.3f);
        if (connection >= 0.4f) return new Color(0.9f, 0.8f, 0.2f);
        if (connection >= 0.15f) return new Color(0.9f, 0.5f, 0.1f);
        return new Color(0.9f, 0.2f, 0.1f);
    }
}
