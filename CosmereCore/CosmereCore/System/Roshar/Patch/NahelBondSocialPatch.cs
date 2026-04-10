using System.Reflection;
using Cosmere.Core.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch]
public static class NahelBondTooltipPatch {
    private static PawnRelationDef? nahelBondDef;

    static MethodBase TargetMethod() {
        return AccessTools.Method(typeof(SocialCardUtility), "GetPawnRowTooltip");
    }

    static void Postfix(ref string __result, object entry, Pawn selPawnForSocialInfo) {
        nahelBondDef ??= DefDatabase<PawnRelationDef>.GetNamedSilentFail("Cosmere_Roshar_Relation_NahelBond");
        if (nahelBondDef == null) return;

        Traverse entryTraverse = Traverse.Create(entry);
        List<PawnRelationDef> relations = entryTraverse.Field("relations").GetValue<List<PawnRelationDef>>();
        Verse.Pawn otherPawn = entryTraverse.Field("otherPawn").GetValue<Verse.Pawn>();
        if (relations == null || otherPawn == null) return;

        bool hasNahelBond = false;
        for (int i = 0; i < relations.Count; i++) {
            if (relations[i] == nahelBondDef) {
                hasNahelBond = true;
                break;
            }
        }

        if (!hasNahelBond) return;

        float connection = SpiritWeb.Instance.GetConnectionValue(selPawnForSocialInfo, otherPawn);
        int percentage = (int)(connection * 100f);
        string stage = connection switch {
            >= 0.7f => "CRO_BondStage_Healthy".Translate(),
            >= 0.4f => "CRO_BondStage_Strained".Translate(),
            >= 0.15f => "CRO_BondStage_Fractured".Translate(),
            _ => "CRO_BondStage_Breaking".Translate(),
        };

        string bondInfo = "\n\n" + "CRO_NahelBond_SocialTooltip_Header".Translate().Resolve().Colorize(ColoredText.TipSectionTitleColor);
        bondInfo += "\n" + "CRO_NahelBond_SocialTooltip_Strength".Translate(percentage.ToString());
        bondInfo += "\n" + "CRO_NahelBond_SocialTooltip_Stage".Translate(stage);

        CompSprenBond? sprenBond = otherPawn.TryGetComp<CompSprenBond>();
        if (sprenBond == null) {
            sprenBond = selPawnForSocialInfo.TryGetComp<CompSprenBond>();
        }

        if (sprenBond != null && sprenBond.PersonalityTraits.Count > 0) {
            bondInfo += "\n" + "CRO_NahelBond_SocialTooltip_Personality".Translate();
            for (int i = 0; i < sprenBond.PersonalityTraits.Count; i++) {
                bondInfo += "\n  - " + sprenBond.PersonalityTraits[i].LabelCap;
            }
        }

        __result += bondInfo;
    }
}
