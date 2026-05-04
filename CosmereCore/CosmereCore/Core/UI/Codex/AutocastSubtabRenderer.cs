using Cosmere.Core.Ability.Autocast;
using Cosmere.Core.UI.Model;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public static class AutocastSubtabRenderer {
    private static Vector2 scroll;
    private static readonly List<RimWorld.Ability> filtered = [];

    public static void Draw(Rect rect, Pawn pawn, IInvestitureProvider active) {
        if (pawn.abilities == null) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f)))
                Widgets.Label(rect, "CC_Codex_Autocast_NoAbilities".Translate());
            return;
        }

        ICodexContentProvider owner = active.Codex;
        List<RimWorld.Ability> all = pawn.abilities.AllAbilitiesForReading;
        filtered.Clear();
        for (int i = 0; i < all.Count; i++) {
            if (owner.OwnsAbility(all[i])) filtered.Add(all[i]);
        }

        if (filtered.Count == 0) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f)))
                Widgets.Label(rect, "CC_Codex_Autocast_NoAbilities".Translate());
            return;
        }

        GameComponent_Autocast store = GameComponent_Autocast.Get();
        float rowHeight = 34f;

        Rect viewRect = new Rect(0f, 0f, rect.width - 16f, filtered.Count * rowHeight + 8f);
        Widgets.BeginScrollView(rect, ref scroll, viewRect);

        float y = 4f;
        for (int i = 0; i < filtered.Count; i++) {
            RimWorld.Ability ability = filtered[i];
            AutocastRule rule = store.GetOrCreateRule(pawn, ability.def.defName);
            Rect row = new Rect(0f, y, viewRect.width, rowHeight - 2f);

            if (i % 2 == 0) Widgets.DrawBoxSolid(row, new Color(1f, 1f, 1f, 0.03f));

            Rect enabled = new Rect(row.x + 4f, row.y + 8f, 16f, 16f);
            Widgets.Checkbox(enabled.x, enabled.y, ref rule.Enabled);

            Rect iconRect = new Rect(row.x + 26f, row.y + 4f, 24f, 24f);
            if (ability.def.uiIcon != null) GUI.DrawTexture(iconRect, ability.def.uiIcon);

            Rect labelRect = new Rect(row.x + 56f, row.y, 220f, row.height);
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white))
                Widgets.Label(labelRect, ability.def.LabelCap);

            Rect metaRect = new Rect(labelRect.xMax + 4f, row.y, 140f, row.height);
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, new Color(0.7f, 0.7f, 0.7f)))
                Widgets.Label(
                    metaRect,
                    "CC_Codex_Autocast_RowMeta".Translate(
                        rule.Triggers.Count.Named("TRIGGERS"),
                        rule.FireCount.Named("COUNT")
                    )
                );

            Rect editButton = new Rect(row.xMax - 88f, row.y + 4f, 80f, row.height - 8f);
            if (Widgets.ButtonText(editButton, "CC_Codex_Autocast_EditButton".Translate())) {
                Find.WindowStack.Add(new AutocastRuleEditorDialog(rule, ability.def.LabelCap));
            }

            y += rowHeight;
        }

        Widgets.EndScrollView();
    }
}