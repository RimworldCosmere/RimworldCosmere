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

    // Fitts: every target on these rows sits at a frame edge, so none of them goes below this.
    private const float TargetSize = 24f;

    private static readonly Color Muted = new Color(0.7f, 0.7f, 0.7f);
    private static readonly Color Dim = new Color(0.55f, 0.55f, 0.55f);
    private static readonly Color HeaderFill = new Color(1f, 1f, 1f, 0.05f);

    public static void Draw(Rect rect, Pawn pawn, CodexState state, IInvestitureProvider active) {
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
        Widgets.BeginScrollView(rect, ref state.AutocastScroll, viewRect);

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
        Rect add = new Rect(header.xMax - 28f, header.y + (HeaderHeight - TargetSize) / 2f, TargetSize, TargetSize);
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

        for (int i = 0; i < rules.Count; i++) {
            DrawRule(new Rect(0f, y, width, RowHeight), pawn, store, rules[i], target);
            y += RowHeight;
        }

        return y + GroupGap;
    }

    private static void DrawRule(
        Rect row,
        Pawn pawn,
        GameComponent_Autocast store,
        AutocastRule rule,
        AutocastTarget target
    ) {
        Widgets.DrawHighlightIfMouseover(row);

        // Widgets.Checkbox always draws at 24px regardless of the rect size passed in.
        Rect enabled = new Rect(row.x + Indent, row.y + (row.height - TargetSize) / 2f, TargetSize, TargetSize);
        TooltipHandler.TipRegion(enabled, "CC_Codex_Autocast_EnabledTip".Translate());
        Widgets.Checkbox(enabled.x, enabled.y, ref rule.Enabled, TargetSize);

        float controlY = row.y + (row.height - TargetSize) / 2f;
        Rect remove = new Rect(row.xMax - 28f, controlY, TargetSize, TargetSize);
        Rect edit = new Rect(remove.x - 88f, controlY, 80f, TargetSize);

        Rect summary = new Rect(enabled.xMax + 8f, row.y, edit.x - enabled.xMax - 16f, row.height);
        string described = AutocastRuleSummary.Describe(rule);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, rule.Enabled ? Color.white : Dim))
            Widgets.Label(summary, described);

        if (Widgets.ButtonText(edit, "CC_Codex_Autocast_EditButton".Translate())) {
            Find.WindowStack.Add(new AutocastRuleEditorDialog(rule, target.Label, IsToggleableAbility(pawn, target)));
        }

        TooltipHandler.TipRegion(remove, "CC_Codex_Autocast_RemoveRule".Translate());

        // The confirm runs from its own window, so removing there never resizes the list being walked here.
        if (Widgets.ButtonImage(remove, TexButton.Delete)) {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "CC_Codex_Autocast_RemoveConfirm".Translate(described.Named("RULE")),
                () => store.RemoveRule(pawn, rule),
                true
            ));
        }
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
