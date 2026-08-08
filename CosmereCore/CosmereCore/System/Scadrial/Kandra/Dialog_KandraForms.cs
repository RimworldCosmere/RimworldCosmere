using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     The bodies a kandra has eaten, laid out as faces rather than a list of names.
/// </summary>
/// <remarks>
///     A kandra picks a form by remembering a person, so the card leads with the portrait and
///     puts the name under it, the way the colonist bar does. Faction and ideoligion sit on the
///     card because "which of the four Ministry obligators was that" is the question a player
///     actually has.
/// </remarks>
public class Dialog_KandraForms : Window {
    private const float CardWidth = 104f;
    private const float CardHeight = 132f;
    private const float Gap = 8f;
    private const int Columns = 7;

    private static readonly Vector2 PortraitSize = new Vector2(CardWidth - 16f, 84f);
    private static readonly Color CardBack = new Color(0.16f, 0.17f, 0.19f);
    private static readonly Color CardHover = new Color(0.24f, 0.26f, 0.29f);
    private static readonly Color Accent = new Color(0.62f, 0.66f, 0.72f);

    /// <summary>Parchment, the colour vanilla already uses for a person's name.</summary>
    private static readonly Color NameTint = new Color(0.90f, 0.84f, 0.67f);

    /// <summary>Muted violet. Distinct from faction and ideoligion, which bring their own.</summary>
    private static readonly Color XenotypeTint = new Color(0.71f, 0.64f, 0.82f);

    private readonly CompKandraForms forms;
    private readonly global::System.Action<int> choose;

    private Vector2 scroll;

    public Dialog_KandraForms(CompKandraForms forms, global::System.Action<int> choose) {
        this.forms = forms;
        this.choose = choose;

        doCloseX = true;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
    }

    public override Vector2 InitialSize =>
        new Vector2((Columns * (CardWidth + Gap)) + Gap + 36f, 520f);

    public override void DoWindowContents(Rect inRect) {
        Rect header = inRect.TopPartPixels(36f);
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft)) {
            Widgets.Label(header, "CS_Kandra_PickForm".Translate());
        }

        Rect count = header;
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleRight, Accent)) {
            Widgets.Label(count, "CS_Kandra_FormCount".Translate(forms.Known.Count.Named("COUNT")));
        }

        Rect body = inRect.BottomPartPixels(inRect.height - 44f);

        if (forms.Known.Count == 0) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, Accent)) {
                Widgets.Label(body, "CS_Kandra_NoFormsYet".Translate());
            }

            return;
        }

        DrawGrid(body);
    }

    private void DrawGrid(Rect body) {
        IReadOnlyList<KandraForm> known = forms.Known;
        int rows = Mathf.CeilToInt(known.Count / (float)Columns);

        Rect view = new Rect(0f, 0f, body.width - 20f, (rows * (CardHeight + Gap)) + Gap);
        Widgets.BeginScrollView(body, ref scroll, view);

        for (int i = 0; i < known.Count; i++) {
            int column = i % Columns;
            int row = i / Columns;

            Rect card = new Rect(
                Gap + (column * (CardWidth + Gap)),
                Gap + (row * (CardHeight + Gap)),
                CardWidth,
                CardHeight
            );

            // Culling matters here. A kandra centuries old can have a great many faces.
            if (card.yMax < scroll.y || card.y > scroll.y + body.height) continue;

            DrawCard(card, known[i], i);
        }

        Widgets.EndScrollView();
    }

    private void DrawCard(Rect card, KandraForm form, int index) {
        bool hover = Mouse.IsOver(card);
        Widgets.DrawBoxSolid(card, hover ? CardHover : CardBack);

        Rect portrait = new Rect(
            card.x + ((card.width - PortraitSize.x) / 2f),
            card.y + 6f,
            PortraitSize.x,
            PortraitSize.y
        );

        Pawn? pawn = form.PortraitPawn;
        if (pawn != null) {
            GUI.DrawTexture(
                portrait,
                PortraitsCache.Get(pawn, PortraitSize, Rot4.South, default, 1.2f)
            );
        } else {
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, Accent)) {
                Widgets.Label(portrait, "?");
            }
        }

        Rect label = new Rect(card.x + 4f, portrait.yMax + 2f, card.width - 8f, 22f);
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter)) {
            Widgets.Label(label, form.Label);
        }

        DrawBadges(card, form);

        Widgets.DrawHighlightIfMouseover(card);
        TooltipHandler.TipRegion(card, () => Tooltip(form), card.GetHashCode());
        Verse.Sound.MouseoverSounds.DoRegion(card);

        if (Widgets.ButtonInvisible(card)) {
            choose(index);
            Close();
        }
    }

    /// <summary>Faction on the left, ideoligion on the right, both along the bottom edge.</summary>
    private static void DrawBadges(Rect card, KandraForm form) {
        float y = card.yMax - 22f;

        if (form.faction?.FactionIcon != null) {
            Rect icon = new Rect(card.x + 6f, y, 18f, 18f);
            GUI.color = form.faction.DefaultColor;
            GUI.DrawTexture(icon, form.faction.FactionIcon);
            GUI.color = Color.white;
        }

        if (form.ideo?.Icon != null) {
            Rect icon = new Rect(card.xMax - 24f, y, 18f, 18f);
            GUI.color = form.ideo.Color;
            GUI.DrawTexture(icon, form.ideo.Icon);
            GUI.color = Color.white;
        }
    }

    /// <summary>
    ///     Four lines that all look alike are four lines nobody reads. Each carries its own
    ///     colour so the eye can go straight to the one it wants, and the faction and ideoligion
    ///     match the icons on the card.
    /// </summary>
    private static string Tooltip(KandraForm form) {
        global::System.Text.StringBuilder text = new global::System.Text.StringBuilder();

        text.AppendLine(Tinted(form.nameFull ?? form.Label, NameTint));

        if (form.faction != null) text.AppendLine(Tinted(form.faction.LabelCap, form.faction.DefaultColor));
        if (form.ideo != null) text.AppendLine(Tinted(form.ideo.name, form.ideo.Color));
        if (form.xenotype != null) text.AppendLine(Tinted(form.xenotype.LabelCap, XenotypeTint));

        text.AppendLine();
        text.Append("CS_Kandra_PickFormTip".Translate().Resolve());
        return text.ToString();
    }

    private static string Tinted(string body, Color colour) {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(colour) + ">" + body + "</color>";
    }
}
