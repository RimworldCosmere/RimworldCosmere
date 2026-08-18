using Cosmere.Core.Ability;
using Cosmere.Core.Ability.Autocast;
using Cosmere.Core.UI.Model;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public static class AutocastSubtabRenderer {
    private const float HeaderHeight = 32f;
    private const float RowHeight = 32f;
    private const float GroupGap = 8f;
    private const float Indent = 16f;

    private static readonly Color Muted = new Color(0.7f, 0.7f, 0.7f);
    private static readonly Color Dim = new Color(0.55f, 0.55f, 0.55f);
    private static readonly Color HeaderFill = new Color(1f, 1f, 1f, 0.05f);

    private static Vector2 scroll;

    public static void Draw(Rect rect, Pawn pawn, IInvestitureProvider active) {
        IReadOnlyList<AutocastTarget> targets = active.Codex.AutocastTargets(pawn);
        if (targets.Count == 0) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, Muted))
                Widgets.Label(rect, "CC_Codex_Autocast_NoAbilities".Translate());

            return;
        }

        GameComponent_Autocast store = GameComponent_Autocast.Get();

        float height = 4f;
        for (int i = 0; i < targets.Count; i++) {
            height += GroupHeight(store.RulesFor(pawn, targets[i].Kind, targets[i].Id).Count);
        }

        Rect viewRect = new Rect(0f, 0f, rect.width - 20f, height);
        Widgets.BeginScrollView(rect, ref scroll, viewRect);

        float y = 4f;
        for (int i = 0; i < targets.Count; i++) {
            y = DrawGroup(viewRect.width, y, pawn, store, targets[i]);
        }

        Widgets.EndScrollView();
    }

    private static float GroupHeight(int ruleCount) {
        return HeaderHeight + ruleCount * RowHeight + GroupGap;
    }

    private static float DrawGroup(
        float width,
        float y,
        Pawn pawn,
        GameComponent_Autocast store,
        AutocastTarget target
    ) {
        Rect header = new Rect(0f, y, width, HeaderHeight);
        Widgets.DrawBoxSolid(header, HeaderFill);

        Rect icon = new Rect(header.x + 4f, header.y + 4f, 24f, 24f);
        if (target.Icon != null) GUI.DrawTexture(icon, target.Icon);

        // The add sits on the header rather than under the group, so an empty target is one line rather than two.
        Rect add = new Rect(header.xMax - 26f, header.y + 6f, 20f, 20f);
        Widgets.DrawHighlightIfMouseover(add);
        TooltipHandler.TipRegion(add, "CC_Codex_Autocast_AddRule".Translate());

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(icon.xMax + 6f, header.y, add.x - icon.xMax - 10f, header.height), target.Label);

        if (Widgets.ButtonImage(add, TexButton.Plus)) {
            Find.WindowStack.Add(new AutocastRuleEditorDialog(
                store.AddRule(pawn, target.Kind, target.Id), target.Label, IsToggleableAbility(pawn, target)));
        }

        y += HeaderHeight;

        List<AutocastRule> rules = store.RulesFor(pawn, target.Kind, target.Id);

        // Deferred: removing inside the loop would resize the list being walked.
        AutocastRule? removing = null;
        for (int i = 0; i < rules.Count; i++) {
            if (DrawRule(new Rect(0f, y, width, RowHeight), pawn, rules[i], target, i % 2 == 1)) removing = rules[i];
            y += RowHeight;
        }

        if (removing != null) store.RemoveRule(pawn, removing);

        return y + GroupGap;
    }

    // Returns true when the player asked for this rule to go.
    private static bool DrawRule(Rect row, Pawn pawn, AutocastRule rule, AutocastTarget target, bool striped) {
        if (striped) Widgets.DrawBoxSolid(row, new Color(1f, 1f, 1f, 0.03f));
        Widgets.DrawHighlightIfMouseover(row);

        // Widgets.Checkbox always draws at 24px regardless of the rect size passed in.
        const float checkSize = 24f;
        Rect enabled = new Rect(row.x + Indent, row.y + (row.height - checkSize) / 2f, checkSize, checkSize);
        Widgets.Checkbox(enabled.x, enabled.y, ref rule.Enabled, checkSize);

        Rect remove = new Rect(row.xMax - 26f, row.y + 6f, 20f, 20f);
        Rect edit = new Rect(remove.x - 88f, row.y + 4f, 80f, row.height - 8f);

        Rect summary = new Rect(enabled.xMax + 8f, row.y, edit.x - enabled.xMax - 16f, row.height);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, rule.Enabled ? Color.white : Dim))
            Widgets.Label(summary, AutocastRuleSummary.Describe(rule));

        if (Widgets.ButtonText(edit, "CC_Codex_Autocast_EditButton".Translate())) {
            Find.WindowStack.Add(new AutocastRuleEditorDialog(rule, target.Label, IsToggleableAbility(pawn, target)));
        }

        TooltipHandler.TipRegion(remove, "CC_Codex_Autocast_RemoveRule".Translate());

        return Widgets.ButtonImage(remove, TexButton.Delete);
    }

    // Only a sustained ability can be switched back off, so only those get the release option.
    private static bool IsToggleableAbility(Pawn pawn, AutocastTarget target) {
        if (target.Kind != AutocastRuleKind.Ability || pawn.abilities == null) return false;

        List<RimWorld.Ability> abilities = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i].def.defName == target.Id) {
                return abilities[i] is IToggleableAbility { IsToggleable: true };
            }
        }

        return false;
    }
}
