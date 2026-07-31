using System.Collections.Generic;
using Cosmere.Core.Quest;
using Cosmere.Core.UI;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Quest;

/// <summary>
///     The once-per-campaign choice for the Lerasium bead recovered in The First Bead. Modal:
///     the reward cannot resolve until the player picks a branch. Themed with a muted gold
///     accent, distinct from Dialog_QuestChoice's steel-blue, to mark the weight of the moment.
/// </summary>
public class Dialog_LerasiumChoice : Verse.Window {
    public const string MistbornAllyFlag = "Cosmere_Scadrial_Quest_LerasiumGivenAway";

    private const float WindowWidth = 480f;
    private const float AccentBarHeight = 5f;
    private const float DividerHeight = 1f;

    private static readonly Color AccentColor = new Color(0.72f, 0.58f, 0.24f);
    private static readonly Color RowColor = new Color(0.18f, 0.15f, 0.1f, 0.65f);
    private static readonly Color DividerColor = new Color(0.4f, 0.34f, 0.22f);
    private static readonly Color BlurbColor = new Color(0.78f, 0.75f, 0.68f);

    // Blocked-row wash: the accent colour itself at a fraction of its alpha, same technique as
    // Dialog_QuestChoice's BlockedRowColor.
    private static readonly Color BlockedRowColor = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.15f);

    private readonly QuestBuildContext ctx;

    public Dialog_LerasiumChoice(QuestBuildContext ctx) {
        this.ctx = ctx;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
        doCloseX = false;
        draggable = false;
    }

    // See Dialog_QuestChoice: overriding Margin to 0 leaves Spacing.Get() as the only
    // contraction in play, so CalcHeight's content width matches DoWindowContents.
    protected override float Margin => 0f;

    public override Vector2 InitialSize => new Vector2(WindowWidth, CalcHeight());

    public override void DoWindowContents(Rect inRect) {
        Rect body = inRect.ContractedBy(Spacing.Get());
        float y = body.y;

        using (new Verse.TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Rect title = new Rect(
                body.x,
                y,
                body.width,
                Text.CalcHeight("CS_Quest_Lerasium_Title".Translate(), body.width)
            );
            Widgets.Label(title, "CS_Quest_Lerasium_Title".Translate());
            y = title.yMax + Spacing.Get(0.25f);
        }

        Widgets.DrawBoxSolid(new Rect(body.x, y, body.width, AccentBarHeight), AccentColor);
        y += AccentBarHeight + Spacing.Get();

        using (new Verse.TextBlock(GameFont.Small, TextAnchor.UpperLeft, BlurbColor)) {
            Rect blurb = new Rect(
                body.x,
                y,
                body.width,
                Text.CalcHeight("CS_Quest_Lerasium_Blurb".Translate(), body.width)
            );
            Widgets.Label(blurb, "CS_Quest_Lerasium_Blurb".Translate());
            y = blurb.yMax + Spacing.Get();
        }

        bool noFreeColonists = FreeColonistCount() == 0;

        Rect drinkRow = new Rect(body.x, y, body.width, RowHeight("CS_Quest_Lerasium_Drink", body.width));
        if (DrawRow(
                drinkRow,
                "CS_Quest_Lerasium_Drink",
                "CS_Quest_Lerasium_Drink_Tip",
                noFreeColonists,
                "CS_Quest_Lerasium_Drink_Blocked"
            )) {
            OpenDrinkMenu();
            Close();
            return;
        }

        y = drinkRow.yMax + Spacing.Get(0.5f);
        Widgets.DrawBoxSolid(new Rect(body.x, y, body.width, DividerHeight), DividerColor);
        y += DividerHeight + Spacing.Get(0.5f);

        Rect giveRow = new Rect(body.x, y, body.width, RowHeight("CS_Quest_Lerasium_Give", body.width));
        if (DrawRow(giveRow, "CS_Quest_Lerasium_Give", "CS_Quest_Lerasium_Give_Tip", false, null)) {
            if (GiveAway()) Close();
            return;
        }

        y = giveRow.yMax + Spacing.Get(0.5f);
        Widgets.DrawBoxSolid(new Rect(body.x, y, body.width, DividerHeight), DividerColor);
        y += DividerHeight + Spacing.Get(0.5f);

        Rect hideRow = new Rect(body.x, y, body.width, RowHeight("CS_Quest_Lerasium_Hide", body.width));
        if (DrawRow(hideRow, "CS_Quest_Lerasium_Hide", "CS_Quest_Lerasium_Hide_Tip", false, null)) {
            HideBead();
            Close();
        }
    }

    private static bool DrawRow(Rect row, string labelKey, string tipKey, bool blocked, string? blockedReasonKey) {
        Widgets.DrawBoxSolid(row, blocked ? BlockedRowColor : RowColor);
        Widgets.DrawHighlightIfMouseover(row);

        string tip = tipKey.Translate().Resolve();
        if (blocked && blockedReasonKey != null) {
            tip = blockedReasonKey.Translate().Resolve() + "\n\n" + tip;
        }

        TooltipHandler.TipRegion(row, tip);
        MouseoverSounds.DoRegion(row);

        Rect inner = row.ContractedBy(Spacing.Get(0.5f));
        using (new Verse.TextBlock(GameFont.Small, TextAnchor.MiddleLeft, blocked ? BlurbColor : Color.white)) {
            Widgets.Label(inner, labelKey.Translate());
        }

        // ButtonInvisible's own doMouseoverSound is skipped - MouseoverSounds.DoRegion above
        // already covers the row every hovered frame, not just on the click frame.
        return !blocked && Widgets.ButtonInvisible(row, false);
    }

    private float CalcHeight() {
        float contentWidth = WindowWidth - Spacing.Get() * 2;

        float height = Spacing.Get() * 2;
        height += Text.CalcHeight("CS_Quest_Lerasium_Title".Translate(), contentWidth);
        height += Spacing.Get(0.25f) + AccentBarHeight + Spacing.Get();
        height += Text.CalcHeight("CS_Quest_Lerasium_Blurb".Translate(), contentWidth);
        height += Spacing.Get();

        height += RowHeight("CS_Quest_Lerasium_Drink", contentWidth) + Spacing.Get(0.5f);
        height += DividerHeight + Spacing.Get(0.5f);
        height += RowHeight("CS_Quest_Lerasium_Give", contentWidth) + Spacing.Get(0.5f);
        height += DividerHeight + Spacing.Get(0.5f);
        height += RowHeight("CS_Quest_Lerasium_Hide", contentWidth);

        return height;
    }

    private static float RowHeight(string labelKey, float rowWidth) {
        float labelWidth = rowWidth - Spacing.Get();
        float textHeight = Text.CalcHeight(labelKey.Translate(), labelWidth);
        return Mathf.Max(textHeight + Spacing.Get(), Spacing.Get(3f));
    }

    private int FreeColonistCount() {
        Verse.Map? map = ctx.map;
        return map?.mapPawns.FreeColonistsSpawnedCount ?? 0;
    }

    private void OpenDrinkMenu() {
        Verse.Map? map = ctx.map;
        if (map == null) return;

        List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
        if (colonists.Count == 0) return;

        List<FloatMenuOption> options = new List<FloatMenuOption>(colonists.Count);
        for (int i = 0; i < colonists.Count; i++) {
            Pawn pawn = colonists[i];
            options.Add(
                new FloatMenuOption(pawn.LabelShortCap, () => {
                    if (!TryConsumeBead()) {
                        Messages.Message("CS_Quest_Lerasium_BeadMissing".Translate(), MessageTypeDefOf.RejectInput, false);
                        return;
                    }

                    GeneUtility.AddMistborn(pawn, false, true, "drank Lerasium");
                })
            );
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private bool GiveAway() {
        FactionDef? giverDef = ctx.def?.giverFaction;
        if (giverDef == null) {
            Logger.Error($"LerasiumChoiceReward on {ctx.def?.defName}: no giverFaction to make an ally.");
            return false;
        }

        Faction? faction = Find.FactionManager.FirstFactionOfDef(giverDef);
        if (faction == null) return false;

        if (!TryConsumeBead()) {
            Messages.Message("CS_Quest_Lerasium_BeadMissing".Translate(), MessageTypeDefOf.RejectInput, false);
            return false;
        }

        faction.TryAffectGoodwillWith(Faction.OfPlayer, 200);
        QuestFlagStore.SetFlag(MistbornAllyFlag);
        return true;
    }

    // The bead CarryHomeObjective confirmed in storage stays exactly where it is - "hide it"
    // means leave it alone, not place a second one. No physical thing needs to move.
    private static void HideBead() {
        Messages.Message("CS_Quest_Lerasium_Hide_Confirm".Translate(), MessageTypeDefOf.NeutralEvent, false);
    }

    // Shared by Drink and Give - the only two branches that spend the bead. CarryHomeObjective
    // already confirmed one was in storage before this dialog opened, but a player can move or
    // destroy things in the time it takes to resolve the choice, so this checks again rather
    // than assuming it is still there.
    private static bool TryConsumeBead() {
        List<Verse.Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++) {
            Verse.Map map = maps[i];
            if (!map.IsPlayerHome) continue;

            List<Verse.Thing> stacks = map.listerThings.ThingsOfDef(Core.ThingDefOf.Lerasium);
            if (stacks.Count == 0) continue;

            stacks[0].SplitOff(1).Destroy();
            return true;
        }

        return false;
    }
}
