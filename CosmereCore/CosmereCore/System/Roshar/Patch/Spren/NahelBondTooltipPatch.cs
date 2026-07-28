using System;
using System.Reflection;
using Concord;
using Cosmere.Core.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.Spren;

[Patch(typeof(SocialCardUtility))]
public static class NahelBondTooltipPatch {
    private static PawnRelationDef? nahelBondDef;

    // CachedSocialTabEntry is a private nested type, so its fields stay behind reflection.
    private static readonly Type? EntryType =
        typeof(SocialCardUtility).GetNestedType("CachedSocialTabEntry", BindingFlags.NonPublic);

    private static readonly FieldInfo? RelationsField =
        EntryType?.GetField("relations", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    private static readonly FieldInfo? OtherPawnField =
        EntryType?.GetField("otherPawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    [Inject(At.Return, "GetPawnRowTooltip")]
    private static void AfterGetPawnRowTooltip(
        object entry,
        Pawn selPawnForSocialInfo,
        ControlHandle<string> ch
    ) {
        nahelBondDef ??= DefDatabase<PawnRelationDef>.GetNamedSilentFail("Cosmere_Roshar_Relation_NahelBond");
        if (nahelBondDef == null) return;

        List<PawnRelationDef>? relations = RelationsField?.GetValue(entry) as List<PawnRelationDef>;
        Pawn? otherPawn = OtherPawnField?.GetValue(entry) as Pawn;
        if (relations == null || otherPawn == null) return;

        bool hasNahelBond = false;
        for (int i = 0; i < relations.Count; i++) {
            if (relations[i] == nahelBondDef) {
                hasNahelBond = true;
                break;
            }
        }

        if (!hasNahelBond) return;

        SpiritWeb? spiritWeb = SpiritWeb.Instance;
        if (spiritWeb == null) return;

        float connection = spiritWeb.GetConnectionValue(selPawnForSocialInfo, otherPawn);
        int percentage = (int)(connection * 100f);
        string stage = connection switch {
            >= 0.7f => "CRO_BondStage_Healthy".Translate(),
            >= 0.4f => "CRO_BondStage_Strained".Translate(),
            >= 0.15f => "CRO_BondStage_Fractured".Translate(),
            _ => "CRO_BondStage_Breaking".Translate(),
        };

        string bondInfo = "\n\n" +
                          "CRO_NahelBond_SocialTooltip_Header".Translate()
                              .Resolve()
                              .Colorize(ColoredText.TipSectionTitleColor);
        bondInfo += "\n" + "CRO_NahelBond_SocialTooltip_Strength".Translate(percentage.ToString());
        bondInfo += "\n" + "CRO_NahelBond_SocialTooltip_Stage".Translate(stage);

        SprenBond? sprenBond = otherPawn.TryGetComp<SprenBond>();
        if (sprenBond == null) {
            sprenBond = selPawnForSocialInfo.TryGetComp<SprenBond>();
        }

        if (sprenBond != null && sprenBond.PersonalityTraits.Count > 0) {
            bondInfo += "\n" + "CRO_NahelBond_SocialTooltip_Personality".Translate();
            for (int i = 0; i < sprenBond.PersonalityTraits.Count; i++) {
                bondInfo += "\n  - " + sprenBond.PersonalityTraits[i].LabelCap;
            }
        }

        ch.ReturnValue += bondInfo;
    }
}
