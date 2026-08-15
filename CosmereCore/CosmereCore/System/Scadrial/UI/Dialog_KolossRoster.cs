using System.Collections.Generic;
using System.Linq;
using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Comp.Game;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.System.Scadrial.UI;

/// <summary>
///     Everything one Allomancer is holding, and what can be done with it.
/// </summary>
/// <remarks>
///     A float menu was the first attempt and it was wrong for this. Holding is the one Scadrian
///     mechanic where the player owns a small standing army through a second pawn, and a list that
///     vanishes on the first click cannot show what each one is doing, whether it is about to turn,
///     or let two of them be ordered at once.
///     <para>
///         Rows are the vanilla inspector shape - name on the left, state on the right - because
///         that reads faster than anything centred. The accent is Scadrian steel, used only on the
///         held count and the selection bar, so the window sits beside vanilla rather than
///         shouting over it.
///     </para>
/// </remarks>
public class Dialog_KolossRoster : Window {
    private const float Unit = 16f;
    private const float RowHeight = Unit * 2.5f;
    private const float ScrollbarWidth = 20f;

    private static readonly Color Steel = new(0.45f, 0.58f, 0.68f);
    private static readonly Color SelectedRow = new(0.45f, 0.58f, 0.68f, 0.22f);
    private static readonly Color Loose = new(0.72f, 0.31f, 0.28f);
    private static readonly Color Faint = new(0.62f, 0.60f, 0.57f);

    private readonly MetalDef metal;
    private readonly Pawn holder;
    private readonly HashSet<Pawn> picked = [];

    private int lastPicked = -1;
    private Vector2 scroll;

    public Dialog_KolossRoster(Pawn holder, MetalDef metal) {
        this.holder = holder;
        this.metal = metal;

        doCloseX = true;
        closeOnClickedOutside = true;
        absorbInputAroundWindow = true;
        forcePause = false;
    }

    public override Vector2 InitialSize => new(640f, 460f);

    private List<Pawn> Held => KolossRoster.Current?.HeldOnMetal(holder, metal) ?? [];

    public override void DoWindowContents(Rect inRect) {
        List<Pawn> held = Held;

        // Nothing left to hold closes the window rather than leaving an empty table behind, which
        // is what happens the moment the last one is released from inside it.
        if (held.Count == 0) {
            Close();

            return;
        }

        Rect header = inRect.TopPartPixels(Unit * 3f);
        Rect footer = inRect.BottomPartPixels(Unit * 2.5f);
        Rect body = new(
            inRect.x,
            header.yMax + Unit,
            inRect.width,
            inRect.height - header.height - footer.height - (Unit * 2f)
        );

        DrawHeader(header, held);
        DrawRows(body, held);
        DrawFooter(footer, held);
    }

    private void DrawHeader(Rect rect, List<Pawn> held) {
        using (new TextBlock(GameFont.Medium)) {
            Widgets.Label(rect.TopPartPixels(Unit * 2f), "CS_KolossRoster_Title".Translate());
        }

        Rect count = rect.BottomPartPixels(Unit);
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft)) {
            GUI.color = Faint;
            Widgets.Label(
                count,
                "CS_KolossRoster_Subtitle".Translate(
                    holder.LabelShortCap.Named("HOLDER"),
                    metal.LabelCap.Named("METAL")
                )
            );

            GUI.color = Steel;
            using (new TextBlock(TextAnchor.MiddleRight)) {
                Widgets.Label(
                    count,
                    "CS_KolossRoster_Label".Translate(
                        held.Count.Named("COUNT"),
                        KolossRoster.CapacityOf(holder).Named("CAPACITY")
                    )
                );
            }

            GUI.color = Color.white;
        }
    }

    private void DrawRows(Rect rect, List<Pawn> held) {
        Widgets.DrawMenuSection(rect);
        Rect inner = rect.ContractedBy(Unit / 2f);
        Rect view = new(0f, 0f, inner.width - ScrollbarWidth, held.Count * RowHeight);

        Widgets.BeginScrollView(inner, ref scroll, view);

        for (int i = 0; i < held.Count; i++) {
            DrawRow(new Rect(0f, i * RowHeight, view.width, RowHeight), held[i], i);
        }

        Widgets.EndScrollView();
    }

    private void DrawRow(Rect rect, Pawn koloss, int index) {
        if (picked.Contains(koloss)) Widgets.DrawBoxSolid(rect, SelectedRow);

        Rect row = rect.ContractedBy(Unit / 4f);
        Rect buttons = row.RightPartPixels(Unit * 9f);
        Rect text = new(row.x, row.y, row.width - buttons.width - Unit, row.height);

        Rect portrait = text.LeftPartPixels(RowHeight);
        Widgets.ThingIcon(portrait, koloss);

        Rect name = new(portrait.xMax + (Unit / 2f), text.y, text.width - portrait.width - (Unit / 2f), text.height);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft)) {
            Widgets.Label(name.TopHalf(), koloss.LabelShortCap);
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft)) {
            bool loose = koloss.InMentalState;
            GUI.color = loose ? Loose : Faint;
            Widgets.Label(name.BottomHalf(), StateOf(koloss, loose));
            GUI.color = Color.white;
        }

        DrawRowButtons(buttons, koloss);

        // Selection is the whole row minus the buttons, so a click anywhere on the name picks it
        // up without stealing the presses meant for the actions.
        Widgets.DrawHighlightIfMouseover(text);
        MouseoverSounds.DoRegion(text);
        TooltipHandler.TipRegion(text, "CS_KolossRoster_RowTip".Translate());

        if (Widgets.ButtonInvisible(text)) Pick(koloss, index);
    }

    private void DrawRowButtons(Rect rect, Pawn koloss) {
        float third = rect.width / 3f;

        Rect info = new(rect.x, rect.y, third, rect.height);
        if (Draw(info, "CS_KolossRoster_Info".Translate(), "CS_KolossRoster_InfoTip".Translate())) {
            Find.WindowStack.Add(new Dialog_InfoCard(koloss));
        }

        Rect draft = new(info.xMax, rect.y, third, rect.height);
        bool drafted = koloss.drafter?.Drafted == true;
        bool canDraft = koloss.drafter != null && koloss.IsColonistPlayerControlled;
        TaggedString draftLabel = drafted
            ? "CS_KolossRoster_Undraft".Translate()
            : "CS_KolossRoster_Draft".Translate();

        if (canDraft) {
            if (Draw(draft, draftLabel, "CS_KolossRoster_DraftTip".Translate())) {
                koloss.drafter!.Drafted = !drafted;
            }
        } else {
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter)) {
                GUI.color = Faint;
                Widgets.Label(draft, draftLabel);
                GUI.color = Color.white;
            }

            // The reason matters more than the greyed button. A koloss in bloodlust is not yours.
            TooltipHandler.TipRegion(draft, "CS_KolossRoster_CannotDraft".Translate());
        }

        Rect release = new(draft.xMax, rect.y, third, rect.height);
        if (Draw(release, "CS_KolossRoster_Let".Translate(), "CS_KolossRoster_LetTip".Translate())) {
            foreach (Pawn one in picked.Contains(koloss) ? picked.ToList() : [koloss]) {
                KolossControl.Release(one);
            }

            picked.Clear();
        }
    }

    private static bool Draw(Rect rect, TaggedString label, TaggedString tip) {
        Widgets.DrawHighlightIfMouseover(rect);
        MouseoverSounds.DoRegion(rect);
        TooltipHandler.TipRegion(rect, tip);

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter)) {
            Widgets.Label(rect, label);
        }

        return Widgets.ButtonInvisible(rect);
    }

    private void DrawFooter(Rect rect, List<Pawn> held) {
        Rect jump = rect.LeftPartPixels(Unit * 10f);
        if (Widgets.ButtonText(jump, "CS_KolossRoster_Jump".Translate())) {
            Select(picked.Count > 0 ? picked.ToList() : held);
            Close();
        }

        Rect letAll = rect.RightPartPixels(Unit * 10f);
        if (!Widgets.ButtonText(letAll, "CS_KolossRoster_LetAll".Translate())) return;

        foreach (Pawn one in held) KolossControl.Release(one);

        picked.Clear();
    }

    /// <summary>
    ///     Shift extends from the last row touched; plain click starts over. Same as the colonist
    ///     bar, because that is where a player already learned this.
    /// </summary>
    private void Pick(Pawn koloss, int index) {
        List<Pawn> held = Held;

        if (Event.current.shift && lastPicked >= 0 && lastPicked < held.Count) {
            int from = Mathf.Min(lastPicked, index);
            int to = Mathf.Max(lastPicked, index);
            for (int i = from; i <= to && i < held.Count; i++) picked.Add(held[i]);
        } else if (Event.current.control) {
            if (!picked.Add(koloss)) picked.Remove(koloss);
        } else {
            picked.Clear();
            picked.Add(koloss);
        }

        lastPicked = index;
        Select(picked.ToList());
    }

    private static void Select(List<Pawn> pawns) {
        if (pawns.Count == 0) return;

        Find.Selector?.ClearSelection();
        for (int i = 0; i < pawns.Count; i++) {
            if (pawns[i].Spawned) Find.Selector?.Select(pawns[i], false);
        }
    }

    private static string StateOf(Pawn koloss, bool loose) {
        if (loose) return "CS_KolossRoster_StateLoose".Translate();

        return koloss.drafter?.Drafted == true
            ? "CS_KolossRoster_StateDrafted".Translate()
            : "CS_KolossRoster_StateHeld".Translate(koloss.jobs?.curDriver?.GetReport() ?? string.Empty).Resolve();
    }
}
