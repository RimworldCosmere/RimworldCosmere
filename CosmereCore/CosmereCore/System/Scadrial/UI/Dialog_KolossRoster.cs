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
    private const float RowHeight = Unit * 3.5f;
    private const float ScrollbarWidth = 20f;
    private const float IconSize = Unit * 1.5f;

    private static readonly Color Steel = new(0.45f, 0.58f, 0.68f);
    private static readonly Color SelectedRow = new(0.45f, 0.58f, 0.68f, 0.22f);
    private static readonly Color Loose = new(0.72f, 0.31f, 0.28f);
    private static readonly Color Faint = new(0.62f, 0.60f, 0.57f);
    private static readonly Color PortraitBack = new(0.16f, 0.17f, 0.19f);
    private static readonly Color PortraitEdge = new(0.35f, 0.42f, 0.48f);

    /// <summary>
    ///     Vanilla's release-to-wild command. Letting a koloss go is not deleting it - it walks
    ///     off and becomes a problem - and the trash icon said the opposite.
    /// </summary>
    private static readonly Texture2D LetGo =
        ContentFinder<Texture2D>.Get("UI/Commands/ReleaseAnimals", false) ?? TexButton.Delete;

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
        Rect buttons = row.RightPartPixels(Unit * 8f);
        Rect text = new(row.x, row.y, row.width - buttons.width - Unit, row.height);

        Rect portrait = text.LeftPartPixels(RowHeight);
        DrawPortrait(portrait, koloss);

        Rect name = new(portrait.xMax + (Unit / 2f), text.y, text.width - portrait.width - (Unit / 2f), text.height);
        bool loose = koloss.InMentalState;

        using (new TextBlock(GameFont.Small, TextAnchor.LowerLeft)) {
            Widgets.Label(name.TopHalf(), koloss.LabelShortCap);
        }

        // What it is doing right now, which is the question the window exists to answer. A koloss
        // that says "Hauling steel" is working; one that says it is loose is a problem.
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft)) {
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

    /// <summary>
    ///     The colonist bar's shape - a dark plate with a lit edge, and the portrait sitting whole
    ///     inside it rather than cropped to a square.
    /// </summary>
    private static void DrawPortrait(Rect rect, Pawn koloss) {
        Widgets.DrawBoxSolid(rect, PortraitBack);

        GUI.color = PortraitEdge;
        Widgets.DrawBox(rect);
        GUI.color = Color.white;

        // A portrait rather than a thing icon. ThingIcon squares the pawn off at the shoulders,
        // which on something drawn at 1.75 times a person clips its head.
        Rect inside = rect.ContractedBy(1f);
        GUI.DrawTexture(
            inside,
            PortraitsCache.Get(koloss, inside.size, Rot4.South, default, 1.1f),
            ScaleMode.ScaleToFit
        );
    }

    private void DrawRowButtons(Rect rect, Pawn koloss) {
        float third = rect.width / 3f;
        float inset = (third - IconSize) / 2f;
        float top = rect.y + ((rect.height - IconSize) / 2f);

        Rect info = new(rect.x + inset, top, IconSize, IconSize);
        if (Draw(info, TexButton.Info, "CS_KolossRoster_InfoTip".Translate(), true)) {
            Find.WindowStack.Add(new Dialog_InfoCard(koloss));
        }

        Rect draft = new(rect.x + third + inset, top, IconSize, IconSize);
        bool drafted = koloss.drafter?.Drafted == true;
        bool canDraft = koloss.drafter != null && koloss.IsColonistPlayerControlled;

        // The reason matters more than a greyed button. A koloss in bloodlust is not yours to send
        // anywhere, and saying so is the difference between a rule and a bug.
        TaggedString draftTip = canDraft
            ? drafted ? "CS_KolossRoster_Undraft".Translate() : "CS_KolossRoster_DraftTip".Translate()
            : "CS_KolossRoster_CannotDraft".Translate();

        if (Draw(draft, TexCommand.Draft, draftTip, canDraft) && canDraft) {
            koloss.drafter!.Drafted = !drafted;
        }

        Rect release = new(rect.x + (third * 2f) + inset, top, IconSize, IconSize);
        if (!Draw(release, LetGo, "CS_KolossRoster_LetTip".Translate(), true)) return;

        foreach (Pawn one in picked.Contains(koloss) ? picked.ToList() : [koloss]) {
            KolossControl.Release(one);
        }

        picked.Clear();
    }

    private static bool Draw(Rect rect, Texture2D icon, TaggedString tip, bool live) {
        TooltipHandler.TipRegion(rect, tip);

        if (live) {
            Widgets.DrawHighlightIfMouseover(rect);
            MouseoverSounds.DoRegion(rect);
        }

        GUI.color = live ? Color.white : Faint;
        GUI.DrawTexture(rect, icon);
        GUI.color = Color.white;

        return live && Widgets.ButtonInvisible(rect);
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

        string job = koloss.jobs?.curDriver?.GetReport()?.CapitalizeFirst() ?? string.Empty;
        if (job.NullOrEmpty()) job = "CS_KolossRoster_StateIdle".Translate();

        return koloss.drafter?.Drafted == true
            ? "CS_KolossRoster_StateDrafted".Translate(job.Named("JOB")).Resolve()
            : job;
    }
}
