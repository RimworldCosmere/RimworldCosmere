using System.Text;
using Cosmere.Core.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Hediff;
using Cosmere.System.Roshar.Surgebinding;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.UI;

[StaticConstructorOnStartup]
public static class SprenBondUI {
    internal static readonly Texture2D BondBarHealthyTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.3f));

    internal static readonly Texture2D BondBarStrainedTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.9f, 0.8f, 0.2f));

    internal static readonly Texture2D BondBarFracturedTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.9f, 0.5f, 0.1f));

    internal static readonly Texture2D BondBarBreakingTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.9f, 0.2f, 0.1f));

    public static Texture2D GetBondBarTexture(float connection) {
        if (connection >= 0.7f) return BondBarHealthyTex;
        if (connection >= 0.4f) return BondBarStrainedTex;
        if (connection >= 0.15f) return BondBarFracturedTex;
        return BondBarBreakingTex;
    }

    public static string BuildStrengthTooltip(Pawn radiant, float connection) {
        if (connection >= 1f) {
            return "CRO_SprenBond_StrengthFull".Translate();
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("CRO_SprenBond_StrengthTooltip".Translate());

        bool hasStrainedBond = false;
        List<Verse.Hediff> hediffs = radiant.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++) {
            if (hediffs[i] is StrainedBond) {
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
}
