using Cosmere.Core.Savant;
using Cosmere.Core.UI.Codex;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Savant;
using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Feruchemy.Memory;
using Cosmere.System.Scadrial.Feruchemy.UI;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemyCodexContent : ICodexContentProvider {
    private static readonly Color PositiveMoodColor = new Color(0.45f, 0.85f, 0.45f);
    private static readonly Color NegativeMoodColor = new Color(0.9f, 0.45f, 0.45f);
    private static readonly Color RowStripeColor = new Color(1f, 1f, 1f, 0.03f);
    private static readonly Color CoppermindHeaderColor = new Color(0.85f, 0.7f, 0.45f);
    private static readonly Color SecondaryTextColor = new Color(0.7f, 0.7f, 0.7f);

    public bool HasProgression(Pawn pawn) {
        return CollectFeruchemists(pawn).Count > 0;
    }

    public bool ShowsBondsSubtab => false;

    public bool HasBonds(Pawn pawn) {
        return false;
    }

    public bool OwnsAbility(Ability ability) {
        return false;
    }

    public bool HasMemories(Pawn pawn) {
        List<Feruchemist> fs = CollectFeruchemists(pawn);
        for (int i = 0; i < fs.Count; i++) {
            if (fs[i].metal?.defName == "Copper") return true;
        }

        return false;
    }

    public string? HeaderLabelFor(Pawn pawn) {
        return null;
    }

    public void DrawProgression(Rect rect, Pawn pawn, CodexState state) {
        List<Feruchemist> ferus = CollectFeruchemists(pawn);
        if (ferus.Count == 0) return;

        float y = rect.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(rect.x, y, rect.width, 30f), "CC_Codex_Feruchemy_Progression_Header".Translate());
        y += 34f;

        SkillRecord? skill = pawn.skills?.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower);
        if (skill != null) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f))) {
                Widgets.Label(
                    new Rect(rect.x, y, rect.width, 24f),
                    "CC_Codex_Feruchemy_OverallSkill".Translate(skill.Level.Named("LEVEL"))
                );
            }

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
                ? ScadrialSavantUtility.GetFeruchemicalSavantStage(pawn, metal)
                : 0;
            Rect stageRect = new Rect(swatch.xMax + 146f, row.y, 140f, row.height);
            SavantUI.DrawSavantStage(stageRect, stage, metal.color);

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
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.75f, 0.75f, 0.75f))) {
                Widgets.Label(
                    usageRect,
                    "CC_Codex_Feruchemy_StoredTapped".Translate(
                        storedLabel.Named("STORED"),
                        tappedLabel.Named("TAPPED")
                    )
                );
            }

            y += 28f;
        }
    }

    public void DrawBonds(Rect rect, Pawn pawn, CodexState state) { }

    public void DrawMemories(Rect rect, Pawn pawn, CodexState state) {
        Rect headerRow = new Rect(rect.x, rect.y, rect.width, 30f);
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(
                new Rect(headerRow.x, headerRow.y, headerRow.width - 170f, headerRow.height),
                "CC_Codex_Feruchemy_Copperminds_Header".Translate()
            );
        }

        List<Metalmind> copperminds = CollectCopperminds(pawn);
        List<Thought_Memory> activeMemories = CollectActiveMemories(pawn);

        DrawStoreMemoryButton(
            new Rect(headerRow.xMax - 160f, headerRow.y + 2f, 160f, 26f),
            pawn,
            activeMemories,
            copperminds
        );

        if (copperminds.Count == 0) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, SecondaryTextColor))
                Widgets.Label(
                    new Rect(rect.x, rect.y + 34f, rect.width, 24f),
                    "CC_Codex_Feruchemy_NoCopperminds".Translate()
                );
            return;
        }

        Rect bodyRect = new Rect(rect.x, rect.y + 34f, rect.width, rect.height - 34f);
        float totalHeight = 0f;
        for (int i = 0; i < copperminds.Count; i++) {
            totalHeight += CoppermindBlockHeight(copperminds[i]);
        }

        Rect viewRect = new Rect(0f, 0f, bodyRect.width - 16f, totalHeight);
        Widgets.BeginScrollView(bodyRect, ref state.MemoriesScroll, viewRect);
        float y = 0f;
        for (int i = 0; i < copperminds.Count; i++) {
            y = DrawCoppermindBlock(new Rect(0f, y, viewRect.width, 0f), copperminds[i]);
        }

        Widgets.EndScrollView();
    }

    private static void DrawStoreMemoryButton(
        Rect rect,
        Pawn pawn,
        List<Thought_Memory> memories,
        List<Metalmind> copperminds
    ) {
        bool hasMemories = memories.Count > 0;
        bool hasCopperminds = copperminds.Count > 0;
        bool anyFits = false;
        for (int i = 0; i < memories.Count && !anyFits; i++) {
            float magnitude = Mathf.Abs(memories[i].MoodOffset());
            for (int j = 0; j < copperminds.Count; j++) {
                if (copperminds[j].CanFitMemory(magnitude)) {
                    anyFits = true;
                    break;
                }
            }
        }

        bool enabled = hasMemories && hasCopperminds && anyFits;
        GUI.enabled = enabled;
        if (Widgets.ButtonText(rect, "CC_Codex_Feruchemy_StoreMemory_Button".Translate())) {
            Find.WindowStack.Add(new Dialog_StoreMemory(pawn, memories, copperminds));
        }

        GUI.enabled = true;

        if (!enabled && Mouse.IsOver(rect)) {
            string tooltipKey = !hasCopperminds
                ? "CC_Codex_Feruchemy_StoreMemory_Disabled_NoCopperminds"
                : !hasMemories
                    ? "CC_Codex_Feruchemy_StoreMemory_Disabled_NoMemories"
                    : "CC_Codex_Feruchemy_StoreMemory_Disabled_NoRoom";
            TooltipHandler.TipRegion(rect, tooltipKey.Translate());
        }
    }

    private static List<Thought_Memory> CollectActiveMemories(Pawn pawn) {
        List<Thought_Memory> result = [];
        if (pawn.needs?.mood?.thoughts?.memories == null) return result;
        List<Thought_Memory> all = pawn.needs.mood.thoughts.memories.Memories;
        for (int i = 0; i < all.Count; i++) {
            Thought_Memory memory = all[i];
            if (Mathf.Abs(memory.MoodOffset()) > 0f) result.Add(memory);
        }

        return result;
    }

    private static float CoppermindBlockHeight(Metalmind mind) {
        int entryCount = mind.StoredMemories.Count;
        if (entryCount == 0) entryCount = 1;
        return 28f + entryCount * 24f + 6f;
    }

    private static float DrawCoppermindBlock(Rect rect, Metalmind mind) {
        float y = rect.y;

        Rect header = new Rect(rect.x, y, rect.width, 26f);
        string ownerName = mind.owner != null
            ? mind.owner.LabelShortCap
            : (string)"CC_Codex_Feruchemy_UnclaimedOwner".Translate();
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, CoppermindHeaderColor)) {
            Widgets.Label(
                header,
                "CC_Codex_Feruchemy_CoppermindHeader".Translate(
                    mind.parent.LabelCap.Named("NAME"),
                    ownerName.Named("OWNER"),
                    mind.UsedMemorySpace.ToString("F1").Named("USED"),
                    mind.MaxAmount.ToString("F0").Named("MAX")
                )
            );
        }

        y += 26f;

        Rect divider = new Rect(rect.x, y, rect.width, 1f);
        Widgets.DrawLineHorizontal(divider.x, divider.y, divider.width, new Color(1f, 1f, 1f, 0.1f));
        y += 2f;

        IReadOnlyList<StoredMemory> memories = mind.StoredMemories;
        if (memories.Count == 0) {
            Rect emptyRow = new Rect(rect.x + 12f, y, rect.width - 12f, 22f);
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, SecondaryTextColor))
                Widgets.Label(emptyRow, "CC_Codex_Feruchemy_CoppermindEmpty".Translate());
            y += 24f;
        }
        else {
            for (int i = 0; i < memories.Count; i++) {
                Rect row = new Rect(rect.x, y, rect.width, 22f);
                if (i % 2 == 0) Widgets.DrawBoxSolid(row, RowStripeColor);

                StoredMemory memory = memories[i];
                Color moodColor = memory.isPositive ? PositiveMoodColor : NegativeMoodColor;

                Rect labelRect = new Rect(row.x + 12f, row.y, row.width * 0.55f - 12f, row.height);
                using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, moodColor))
                    Widgets.Label(labelRect, memory.labelCap);

                string storedBy = memory.owner != null
                    ? memory.owner.LabelShortCap
                    : (string)"CC_Codex_Feruchemy_UnknownStoredBy".Translate();
                Rect attributionRect = new Rect(labelRect.xMax, row.y, row.width - labelRect.width - 12f, row.height);
                using (new TextBlock(GameFont.Small, TextAnchor.MiddleRight, SecondaryTextColor))
                    Widgets.Label(
                        attributionRect,
                        "CC_Codex_Feruchemy_MemoryStoredBy".Translate(storedBy.Named("STOREDBY"))
                    );

                y += 22f;
            }
        }

        y += 6f;
        return y;
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
            if (mind != null && mind.Metal?.defName == "Copper") {
                result.Add(mind);
            }
        }

        return result;
    }
}