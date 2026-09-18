using System.Collections.Generic;
using Cosmere.Core.Listing;
using Cosmere.Core.UI;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     The body a kandra actually is, beside the ones it has eaten. The true body is not a peer
///     of the borrowed shapes, so it takes the rail rather than a card in the grid.
/// </summary>
[StaticConstructorOnStartup]
public class Dialog_KandraForms : Core.Window.BaseWindow {
    private static readonly Texture2D MaleTrueBody =
        ContentFinder<Texture2D>.Get("Things/Pawn/Humanlike/Bodies/Kandra_Male_south");

    private static readonly Texture2D FemaleTrueBody =
        ContentFinder<Texture2D>.Get("Things/Pawn/Humanlike/Bodies/Kandra_Female_south");

    private static readonly Texture2D MaleTrueHead =
        ContentFinder<Texture2D>.Get("Things/Pawn/Humanlike/Heads/Kandra_Male_south");

    private static readonly Texture2D FemaleTrueHead =
        ContentFinder<Texture2D>.Get("Things/Pawn/Humanlike/Heads/Kandra_Female_south");

    /// <summary>The humanlike body mesh is 1.5 world units, which is what headOffset is measured in.</summary>
    private const float BodyMeshUnits = 1.5f;

    private const float RailWidth = 232f;
    private const float CardWidth = 118f;
    private const float CardHeight = 132f;
    private const float CardGap = 12f;
    private const float ScrollbarWidth = 12f;
    private const int Columns = 4;

    private const float ButtonHeight = 32f;
    private const float RailPad = 12f;
    private const float WellHeight = 150f;

    private static readonly Vector2 CardPortrait = new Vector2(108f, 84f);
    private static readonly Vector2 RailPortrait = new Vector2(200f, WellHeight);

    private readonly global::System.Action<int> choose;
    private readonly CompKandraForms forms;

    private KandraForm? confirming;
    private bool picking;
    private Vector2 scroll;

    public Dialog_KandraForms(CompKandraForms forms, global::System.Action<int> choose) {
        this.forms = forms;
        this.choose = choose;

        doCloseX = false;
        forcePause = true;
        absorbInputAroundWindow = true;
    }

    protected override Vector2 initialWindowSize => new Vector2(Spacing.Get(50), Spacing.Get(32.5));

    protected override float headerHeight => Spacing.Get(3.5);

    private Pawn? Pawn => forms.parent as Pawn;

    protected override TaggedString GetTitle() {
        return (picking ? "CS_Kandra_ChooseTrue" : "CS_Kandra_PickForm").Translate();
    }

    protected override TaggedString? GetSubtitle() {
        return CountLine();
    }

    /// <summary>Nothing flows here, so the listing body stays empty and DrawBody lays out Rects.</summary>
    protected override void DrawBodyContent(FoundationListing listing) { }

    /// <summary>
    ///     Returns the rect's own height so the window's scroll view never engages. The grid runs
    ///     its own scroll, and a scroll view inside a scroll view eats the wheel.
    /// </summary>
    protected override float DrawBody(Rect rect) {
        Rect body = rect.ContractedBy(Spacing.Get(0.75f));
        Rect rail = new Rect(body.x, body.y, RailWidth, body.height);
        Rect right = new Rect(
            rail.xMax + Spacing.Get(),
            body.y,
            body.width - RailWidth - Spacing.Get(),
            body.height
        );

        if (confirming != null) {
            DrawConfirm(rail, confirming);
        } else {
            DrawRail(rail);
        }

        if (forms.Known.Count == 0) {
            DrawEmpty(right);

            return rect.height;
        }

        if (picking) right = DrawPickBar(right);

        DrawGrid(right);

        return rect.height;
    }

    private Rect DrawPickBar(Rect right) {
        Rect bar = right.TopPartPixels(Spacing.Get(1.75f));

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, BorderColor)) {
            Widgets.Label(bar, "CS_Kandra_PickTrueHint".Translate());
        }

        if (Widgets.ButtonText(bar.RightPartPixels(88f), "CS_Kandra_Cancel".Translate())) {
            picking = false;
            confirming = null;
        }

        return new Rect(right.x, bar.yMax + Spacing.Get(0.5f), right.width, right.height - Spacing.Get(2.25f));
    }

    private TaggedString CountLine() {
        if (picking) return "CS_Kandra_ToChooseFrom".Translate(forms.Known.Count.Named("COUNT"));

        string? worn = forms.WornName;

        return worn.NullOrEmpty()
            ? "CS_Kandra_FormCount".Translate(forms.Known.Count.Named("COUNT"))
            : "CS_Kandra_CountWearing".Translate(forms.Known.Count.Named("COUNT"), worn!.Named("FORM"));
    }

    /// <summary>
    ///     A vanilla menu section is what makes the true body read as its own region, so nothing
    ///     here needs a coloured frame or a fill this window invented.
    /// </summary>
    private void DrawRail(Rect rail) {
        Widgets.DrawMenuSection(rail);

        float x = rail.x + RailPad;
        float width = rail.width - (RailPad * 2f);
        float y = rail.y + RailPad;

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor)) {
            Widgets.Label(new Rect(x, y, width, 20f), "CS_Kandra_TrueFormHeader".Translate());
        }

        y += 22f;
        DrawPortrait(new Rect(x, y, width, WellHeight), forms.TrueBody, RailPortrait, 1.12f);
        y += WellHeight + 8f;

        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, headerTextColor)) {
            Widgets.Label(new Rect(x, y, width, 22f), Pawn?.LabelShortCap ?? string.Empty);
        }

        y += 24f;
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor)) {
            Widgets.Label(new Rect(x, y, width, 18f), KindLine());
            DrawMaterial(new Rect(x, y + 18f, width, 18f));
        }

        DrawRailButtons(new Rect(x, rail.yMax - RailPad - (ButtonHeight * 3f) - 16f, width, ButtonHeight));
    }

    private void DrawRailButtons(Rect first) {
        bool wearing = forms.IsWearingSomeoneElse;

        // Spec pending. Inert with the real reason rather than shipped as a button that lies.
        Reasoned(
            first,
            "CS_Kandra_EditTrue".Translate(),
            false,
            forms.TrueBodyCrafted
                ? "CS_Kandra_EditTrue_Unbuilt".Translate()
                : "CS_Kandra_EditTrue_NotCrafted".Translate()
        );

        // Allowed while wearing somebody. The body underneath is what changes, not the disguise.
        Rect change = new Rect(first.x, first.y + ButtonHeight + 8f, first.width, first.height);
        bool canChange = forms.Known.Count > 0;

        if (Reasoned(
                change,
                "CS_Kandra_ChangeTrue".Translate(),
                canChange,
                canChange ? null : "CS_Kandra_ChangeTrue_Nothing".Translate()
            )) {
            picking = true;
        }

        if (!wearing) return;

        // The one action in this state that changes what the colony sees. Nothing else is a CTA.
        Rect revert = new Rect(first.x, change.yMax + 8f, first.width, first.height);
        if (!CTAButtonText(revert, "CS_Kandra_Revert".Translate())) return;

        // Queued as a job rather than applied here. Rearranging a body takes ten seconds.
        choose(JobDriver.KandraChangeShape.RevertIndex);
        Close();
    }

    /// <summary>A button that says why it cannot be used, rather than one that is merely grey.</summary>
    private static bool Reasoned(Rect rect, string label, bool enabled, string? reason) {
        bool clicked = Widgets.ButtonText(rect, label, active: enabled);
        if (!enabled && !reason.NullOrEmpty()) TooltipHandler.TipRegion(rect, reason);

        return clicked;
    }

    private string KindLine() {
        if (forms.TrueBodyCrafted) return "CS_Kandra_TrueKind_Crafted".Translate();

        return "CS_Kandra_TrueKind_Adopted".Translate(
            (forms.TrueBody?.Label ?? string.Empty).Named("FORM")
        );
    }

    /// <summary>The material only exists on a crafted body. An adopted one is bone.</summary>
    private void DrawMaterial(Rect rect) {
        Pawn? pawn = Pawn;
        if (pawn == null) return;

        if (!forms.TrueBodyCrafted) {
            Widgets.Label(rect, "CS_Kandra_NoMaterial".Translate());

            return;
        }

        (string name, Color colour) = KandraAppearance.TrueBodyMaterialFor(pawn);

        Rect swatch = new Rect(rect.x, rect.y + 4f, 10f, 10f);
        Widgets.DrawBoxSolid(swatch, colour);
        Widgets.Label(new Rect(swatch.xMax + 6f, rect.y, rect.width - 16f, rect.height), name);
    }

    private void DrawGrid(Rect region) {
        IReadOnlyList<KandraForm> known = forms.Known;
        int rows = Mathf.CeilToInt(known.Count / (float)Columns);

        Rect view = new Rect(0f, 0f, region.width - ScrollbarWidth, rows * (CardHeight + CardGap));
        Widgets.BeginScrollView(region, ref scroll, view);

        for (int i = 0; i < known.Count; i++) {
            Rect card = new Rect(
                (i % Columns) * (CardWidth + CardGap),
                (i / Columns) * (CardHeight + CardGap),
                CardWidth,
                CardHeight
            );

            // Culling matters here. A kandra centuries old can have a great many faces.
            if (card.yMax < scroll.y || card.y > scroll.y + region.height) continue;

            DrawCard(card, known[i], i);
        }

        Widgets.EndScrollView();
    }

    private void DrawCard(Rect card, KandraForm form, int index) {
        Widgets.DrawMenuSection(card);
        DrawPortrait(new Rect(card.x + 5f, card.y + 6f, CardPortrait.x, CardPortrait.y), form, CardPortrait, 1.2f);

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, bodyTextColor)) {
            Widgets.Label(new Rect(card.x + 4f, card.y + 93f, card.width - 8f, 26f), form.Label);
        }

        DrawBadges(card, form);

        // Inert behind the confirmation, or a stray click wears a form and drops the question.
        if (confirming != null) return;

        Widgets.DrawHighlightIfMouseover(card);
        TooltipHandler.TipRegion(card, () => Tooltip(form), card.GetHashCode());
        Verse.Sound.MouseoverSounds.DoRegion(card);

        if (!Widgets.ButtonInvisible(card)) return;

        if (picking) {
            confirming = form;
            picking = false;

            return;
        }

        choose(index);
        Close();
    }

    /// <summary>
    ///     The body it gives up beside the one it takes. Drawn in the rail rather than over the
    ///     window, because a kandra dialog never stacks a second one.
    /// </summary>
    private void DrawConfirm(Rect rail, KandraForm form) {
        Widgets.DrawMenuSection(rail);

        float x = rail.x + RailPad;
        float width = rail.width - (RailPad * 2f);
        float y = rail.y + RailPad;

        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, headerTextColor)) {
            Rect title = new Rect(x, y, width, Text.CalcHeight("CS_Kandra_AdoptTitle".Translate(), width));
            Widgets.Label(title, "CS_Kandra_AdoptTitle".Translate());
            y = title.yMax + 8f;
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor)) {
            Rect blurb = new Rect(x, y, width, Text.CalcHeight("CS_Kandra_AdoptBody".Translate(), width));
            Widgets.Label(blurb, "CS_Kandra_AdoptBody".Translate());
            y = blurb.yMax + 12f;
        }

        Rect was = new Rect(x, y, 98f, 96f);
        Rect now = new Rect(x + width - 98f, y, 98f, 96f);
        DrawPortrait(was, forms.TrueBody, new Vector2(98f, 96f), 1.05f);
        DrawPortrait(now, form, new Vector2(98f, 96f), 1.05f);

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, bodyTextColor)) {
            Widgets.Label(new Rect(was.x, was.yMax + 4f, was.width, 28f), CurrentTrueLabel());
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, BorderColor)) {
            Widgets.Label(new Rect(now.x, now.yMax + 4f, now.width, 28f), form.Label);
        }

        Rect keep = new Rect(x, rail.yMax - RailPad - (ButtonHeight * 2f) - 8f, width, ButtonHeight);
        if (Widgets.ButtonText(keep, "CS_Kandra_AdoptKeep".Translate())) confirming = null;

        Rect take = new Rect(x, keep.yMax + 8f, width, ButtonHeight);
        if (!CTAButtonText(take, "CS_Kandra_AdoptTake".Translate())) return;

        forms.AdoptTrueBody(form);
        confirming = null;
    }

    private string CurrentTrueLabel() {
        if (!forms.TrueBodyCrafted) return forms.TrueBody?.Label ?? string.Empty;

        Pawn? pawn = Pawn;

        return pawn == null ? string.Empty : KandraAppearance.TrueBodyMaterialFor(pawn).name;
    }

    private void DrawEmpty(Rect region) {
        using (new TextBlock(GameFont.Small, TextAnchor.UpperCenter, headerTextColor)) {
            Widgets.Label(new Rect(region.x, region.y + 150f, region.width, 24f), "CS_Kandra_NoFormsYet".Translate());
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, bodyTextColor)) {
            Widgets.Label(
                new Rect(region.x + 56f, region.y + 178f, region.width - 112f, 60f),
                "CS_Kandra_NoFormsYetHint".Translate()
            );
        }
    }

    private void DrawPortrait(Rect well, KandraForm? form, Vector2 size, float zoom) {
        Widgets.DrawBoxSolid(well, Widgets.WindowBGFillColor);

        // A generated stand-in arrives dressed and human, which is the one thing this is not.
        if (form is { crafted: true }) {
            DrawTrueBody(well, form, size);

            return;
        }

        Pawn? pawn = form?.PortraitPawn;
        if (pawn == null) {
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, bodyTextColor)) {
                Widgets.Label(well, "CS_Kandra_NoPortrait".Translate());
            }

            return;
        }

        Rect inner = new Rect(
            well.x + ((well.width - size.x) / 2f),
            well.y + ((well.height - size.y) / 2f),
            size.x,
            size.y
        );
        GUI.DrawTexture(inner, PortraitsCache.Get(pawn, size, Rot4.South, default, zoom));
    }

    /// <summary>
    ///     The crafted body drawn from its own textures, in the material it is made of. Body and
    ///     head are separate files, and the head rides where this pawn's body type puts one.
    /// </summary>
    private void DrawTrueBody(Rect well, KandraForm form, Vector2 size) {
        Pawn? pawn = Pawn;
        if (pawn == null) return;

        bool female = pawn.gender == Gender.Female;
        float side = Mathf.Min(size.x, size.y);
        Rect body = new Rect(
            well.x + ((well.width - side) / 2f),
            well.y + ((well.height - side) / 2f),
            side,
            side
        );

        BodyTypeDef? shape = form.bodyType ?? pawn.story?.bodyType;
        float lift = shape == null ? 0f : side * (shape.headOffset.y / BodyMeshUnits);
        Rect head = new Rect(body.x, body.y - lift, side, side);

        Color previous = GUI.color;
        GUI.color = KandraAppearance.TrueBodyColorFor(pawn);
        GUI.DrawTexture(body, female ? FemaleTrueBody : MaleTrueBody, ScaleMode.ScaleToFit);
        GUI.DrawTexture(head, female ? FemaleTrueHead : MaleTrueHead, ScaleMode.ScaleToFit);

        // A kandra grows its own hair, so it keeps its own colour rather than the body material.
        Texture? mane = HairTexture(form.hair ?? pawn.story?.hairDef);
        if (mane != null) {
            GUI.color = form.hairColour;
            GUI.DrawTexture(head, mane, ScaleMode.ScaleToFit);
        }

        GUI.color = previous;
    }

    /// <summary>
    ///     The south-facing hair sheet, through GraphicDatabase so nothing hits ContentFinder on a
    ///     draw call. Null for a bald head or a path that will not load.
    /// </summary>
    private static Texture? HairTexture(HairDef? hair) {
        if (hair == null || hair.texPath.NullOrEmpty()) return null;

        try {
            return GraphicDatabase.Get<Graphic_Multi>(
                hair.texPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                Color.white
            ).MatSouth.mainTexture;
        } catch (global::System.Exception) {
            return null;
        }
    }

    /// <summary>Faction on the left, ideoligion on the right, both along the bottom edge.</summary>
    private static void DrawBadges(Rect card, KandraForm form) {
        float y = card.yMax - 19f;
        Color previous = GUI.color;

        if (form.faction?.FactionIcon != null) {
            GUI.color = form.faction.DefaultColor;
            GUI.DrawTexture(new Rect(card.x + 5f, y, 14f, 14f), form.faction.FactionIcon);
        }

        if (form.ideo?.Icon != null) {
            GUI.color = form.ideo.Color;
            GUI.DrawTexture(new Rect(card.xMax - 19f, y, 14f, 14f), form.ideo.Icon);
        }

        GUI.color = previous;
    }

    /// <summary>
    ///     Four lines that all look alike are four lines nobody reads. Each carries its own colour
    ///     so the eye can go straight to the one it wants.
    /// </summary>
    private string Tooltip(KandraForm form) {
        global::System.Text.StringBuilder text = new global::System.Text.StringBuilder();

        text.AppendLine(Tinted(form.nameFull ?? form.Label, BorderColor));

        if (form.faction != null) text.AppendLine(Tinted(form.faction.LabelCap, form.faction.DefaultColor));
        if (form.ideo != null) text.AppendLine(Tinted(form.ideo.name, form.ideo.Color));
        if (form.xenotype != null) text.AppendLine(Tinted(form.xenotype.LabelCap, BodyTextColor));

        text.AppendLine();
        text.Append((picking ? "CS_Kandra_PickTrueTip" : "CS_Kandra_PickFormTip").Translate().Resolve());

        return text.ToString();
    }

    private static string Tinted(string body, Color colour) {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(colour) + ">" + body + "</color>";
    }
}
