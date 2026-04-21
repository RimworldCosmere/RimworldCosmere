using System.Collections.Generic;
using Cosmere.Core.UI.Codex;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Gene;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemyCodexContent : ICodexContentProvider {
    public bool HasProgression(Pawn pawn) => CollectFeruchemists(pawn).Count > 0;

    public bool HasBonded(Pawn pawn) => false;

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

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "CC_Codex_Feruchemy_Progression_Header".Translate());

        float y = rect.y + 34f;
        for (int i = 0; i < ferus.Count; i++) {
            Feruchemist f = ferus[i];
            Rect row = new Rect(rect.x, y, rect.width, 26f);
            if (i % 2 == 0) Widgets.DrawBoxSolid(row, new Color(1f, 1f, 1f, 0.03f));

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white))
                Widgets.Label(new Rect(row.x + 4f, row.y, 140f, row.height), f.metal.LabelCap);

            List<IMetalmindSource> mms = f.metalminds;
            float totalMax = 0f;
            float totalValue = 0f;
            for (int j = 0; j < mms.Count; j++) {
                totalMax += mms[j].maxAmount;
                totalValue += mms[j].storedAmount;
            }

            float fill = totalMax > 0f ? totalValue / totalMax : 0f;
            Rect bar = new Rect(row.x + 150f, row.y + 7f, row.width - 260f, 12f);
            Widgets.DrawBoxSolid(bar, new Color(0.1f, 0.1f, 0.12f));
            Rect fillRect = new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(fill), bar.height);
            Widgets.DrawBoxSolid(fillRect, new Color(0.85f, 0.75f, 0.3f));

            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, new Color(0.75f, 0.75f, 0.75f)))
                Widgets.Label(
                    new Rect(bar.xMax + 6f, row.y, 100f, row.height),
                    "CC_Codex_Feruchemy_Target".Translate(f.targetValue.ToString("F0").Named("VALUE"))
                );

            y += 28f;
        }
    }

    public void DrawBonded(Pawn pawn, Rect rect) { }

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
