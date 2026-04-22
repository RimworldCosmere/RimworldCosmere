using System.Collections.Generic;
using Cosmere.Core.Savant;
using Cosmere.Core.UI.Codex;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemyCodexContent : ICodexContentProvider {
    public bool HasProgression(Pawn pawn) => CollectFeruchemists(pawn).Count > 0;

    public bool ShowsBondsSubtab => false;
    public bool HasBonds(Pawn pawn) => false;
    public bool OwnsAbility(RimWorld.Ability ability) => false;

    public bool HasMemories(Pawn pawn) {
        List<Feruchemist> fs = CollectFeruchemists(pawn);
        for (int i = 0; i < fs.Count; i++) {
            if (fs[i].metal?.defName == "Copper") return true;
        }
        return false;
    }

    public string? HeaderLabelFor(Pawn pawn) => null;

    public void DrawProgression(Pawn pawn, Rect rect) {
        List<Feruchemist> ferus = CollectFeruchemists(pawn);
        if (ferus.Count == 0) return;

        float y = rect.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(rect.x, y, rect.width, 30f), "CC_Codex_Feruchemy_Progression_Header".Translate());
        y += 34f;

        SkillRecord? skill = pawn.skills?.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower);
        if (skill != null) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f)))
                Widgets.Label(
                    new Rect(rect.x, y, rect.width, 24f),
                    "CC_Codex_Feruchemy_OverallSkill".Translate(skill.Level.Named("LEVEL"))
                );
            y += 28f;
        }

        for (int i = 0; i < ferus.Count; i++) {
            Feruchemist f = ferus[i];
            MetallicArtsMetalDef metal = f.metal;

            Rect row = new Rect(rect.x, y, rect.width, 26f);
            if (i % 2 == 0) Widgets.DrawBoxSolid(row, new Color(1f, 1f, 1f, 0.03f));

            Rect swatch = new Rect(row.x + 4f, row.y + 8f, 10f, 10f);
            Widgets.DrawBoxSolid(swatch, metal.color);

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white))
                Widgets.Label(new Rect(swatch.xMax + 8f, row.y, 130f, row.height), metal.LabelCap);

            int stage = SavantUtility.CanBeSavant(metal)
                ? SavantUtility.GetFeruchemicalSavantStage(pawn, metal)
                : 0;
            Rect stageRect = new Rect(swatch.xMax + 146f, row.y, 140f, row.height);
            DrawSavantStage(stageRect, stage, metal.color);

            int storingTicks = 0;
            int tappingTicks = 0;
            if (pawn.records != null) {
                storingTicks = (int)pawn.records.GetValue(RecordDefOf.GetTimeSpentStoringForMetal(metal));
                tappingTicks = (int)pawn.records.GetValue(RecordDefOf.GetTimeSpentTappingForMetal(metal));
            }

            string storedLabel = storingTicks > 0
                ? storingTicks.ToStringTicksToPeriod(false, true, false)
                : (string)"CC_Codex_Feruchemy_Never".Translate();
            string tappedLabel = tappingTicks > 0
                ? tappingTicks.ToStringTicksToPeriod(false, true, false)
                : (string)"CC_Codex_Feruchemy_Never".Translate();

            Rect usageRect = new Rect(stageRect.xMax + 8f, row.y, row.xMax - stageRect.xMax - 12f, row.height);
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.75f, 0.75f, 0.75f)))
                Widgets.Label(
                    usageRect,
                    "CC_Codex_Feruchemy_StoredTapped".Translate(
                        storedLabel.Named("STORED"),
                        tappedLabel.Named("TAPPED")
                    )
                );

            y += 28f;
        }
    }

    public void DrawBonds(Pawn pawn, Rect rect, CodexState state) { }

    public void DrawMemories(Pawn pawn, Rect rect) {
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "CC_Codex_Feruchemy_Copperminds_Header".Translate());

        List<Metalmind> copperminds = CollectCopperminds(pawn);
        if (copperminds.Count == 0) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.7f, 0.7f, 0.7f)))
                Widgets.Label(new Rect(rect.x, rect.y + 34f, rect.width, 24f), "CC_Codex_Feruchemy_NoCopperminds".Translate());
            return;
        }

        float y = rect.y + 34f;
        for (int i = 0; i < copperminds.Count; i++) {
            Metalmind mind = copperminds[i];
            Rect row = new Rect(rect.x, y, rect.width, 24f);
            if (i % 2 == 0) Widgets.DrawBoxSolid(row, new Color(1f, 1f, 1f, 0.03f));

            string ownerName = mind.owner != null
                ? mind.owner.LabelShortCap
                : (string)"CC_Codex_Feruchemy_UnclaimedOwner".Translate();

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white))
                Widgets.Label(
                    new Rect(row.x + 4f, row.y, row.width - 8f, row.height),
                    "CC_Codex_Feruchemy_CoppermindRow".Translate(
                        mind.storedAmount.ToString("F0").Named("AMOUNT"),
                        ownerName.Named("OWNER")
                    )
                );

            y += 26f;
        }
    }

    private static void DrawSavantStage(Rect rect, int stage, Color accent) {
        string label;
        Color color;
        switch (stage) {
            case 1:
                label = (string)"CC_Codex_Savant_Stage1".Translate();
                color = Color.Lerp(accent, Color.white, 0.35f);
                break;
            case 2:
                label = (string)"CC_Codex_Savant_Stage2".Translate();
                color = accent;
                break;
            case 3:
                label = (string)"CC_Codex_Savant_Stage3".Translate();
                color = Color.Lerp(accent, new Color(1f, 0.9f, 0.4f), 0.5f);
                break;
            default:
                label = (string)"CC_Codex_Savant_Stage0".Translate();
                color = new Color(0.55f, 0.55f, 0.55f);
                break;
        }
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, color))
            Widgets.Label(rect, label);
    }

    private static List<Feruchemist> CollectFeruchemists(Pawn pawn) {
        List<Feruchemist> result = [];
        if (pawn.genes == null) return result;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Feruchemist f && !f.Overridden) result.Add(f);
        }
        return result;
    }

    private static List<Metalmind> CollectCopperminds(Pawn pawn) {
        List<Metalmind> result = [];
        if (pawn.inventory?.innerContainer == null) return result;
        List<Verse.Thing> items = pawn.inventory.innerContainer.InnerListForReading;
        for (int i = 0; i < items.Count; i++) {
            Metalmind? mind = (items[i] as ThingWithComps)?.TryGetComp<Metalmind>();
            if (mind != null && mind.metal?.defName == "Copper") {
                result.Add(mind);
            }
        }
        return result;
    }
}
