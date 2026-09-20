using Cosmere.Core.UI;
using Cosmere.System.Scadrial.Util;
using UnityEngine;
using Verse;
using Verse.Sound;
using EyeColourRow = (string name, string hex, string labelKey);
using EyeLightRow = (string name, float strength, string labelKey);
using IrisRow = (string name, float scale, string labelKey);

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     Everything about a kandra's eyes that is not their colour: how much of the socket the iris
///     fills, whether the two eyes match, and whether they carry light.
/// </summary>
public partial class Dialog_KandraForms {
    /// <summary>An iris row is taller than a chip row because the swatch has to show a size.</summary>
    private const float IrisRowHeight = 30f;

    private const float IrisChipSize = 24f;
    private const float CheckRowHeight = 24f;

    private const float IrisCanvasSize = 256f;

    /// <summary>
    ///     How much of the canvas the chip shows, centred on the left iris. The painted mark is six
    ///     canvas pixels across at Small and eight at Standard, so the whole canvas in a 24px box puts
    ///     the two within a fifth of a pixel of each other. This window blows them up to nine and twelve.
    /// </summary>
    private const float IrisChipWindow = 16f;

    private const float IrisMarkX = 114f;
    private const float IrisMarkY = 141f;

    /// <summary>The window above in UV, where row 0 is the bottom of the texture.</summary>
    private static readonly Rect IrisChipCoords = new Rect(
        (IrisMarkX - (IrisChipWindow / 2f)) / IrisCanvasSize,
        1f - ((IrisMarkY + (IrisChipWindow / 2f)) / IrisCanvasSize),
        IrisChipWindow / IrisCanvasSize,
        IrisChipWindow / IrisCanvasSize
    );

    private Vector2 eyeOptionScroll;

    /// <summary>Iris, odd eyes and the light, stacked down the Eyes tab's right column.</summary>
    private void DrawEyeOptions(Rect inner) {
        float gap = Spacing.Get(0.5f);
        float width = ColumnContentWidth(inner);
        float column = (width - (gap * (SwatchColumns - 1))) / SwatchColumns;

        IReadOnlyList<IrisRow> irises = KandraAppearance.AllIrisSizes;
        IReadOnlyList<EyeColourRow> stones = KandraAppearance.AllEyeColours;
        IReadOnlyList<EyeLightRow> lights = KandraAppearance.AllEyeLights;

        // Only a name the table still carries counts as on. Both sentinels and null read as off.
        bool odd = KandraAppearance.FindEyeColour(designEyeColourTwo) != null;
        bool lit = KandraAppearance.FindEyeLight(designEyeLight) != null;

        TaggedString oddNote = "CS_Kandra_OddEyesDesc".Translate();
        TaggedString lightNote = "CS_Kandra_EyeLightDesc".Translate();
        float oddNoteHeight;
        float lightNoteHeight;

        using (new TextBlock(GameFont.Tiny)) {
            oddNoteHeight = Text.CalcHeight(oddNote, width);
            lightNoteHeight = Text.CalcHeight(lightNote, width);
        }

        // Two of the three sections come and go, so the height is measured rather than assumed.
        float content = (SectionHeaderHeight * 3f)
                        + (irises.Count * IrisRowHeight)
                        + (CheckRowHeight * 2f)
                        + oddNoteHeight
                        + lightNoteHeight
                        + (gap * 2f)
                        + (odd ? Mathf.CeilToInt(stones.Count / (float)SwatchColumns) * SwatchRowHeight : 0f)
                        + (lit ? lights.Count * IrisRowHeight : 0f);

        Widgets.BeginScrollView(inner, ref eyeOptionScroll, new Rect(0f, 0f, width, content));

        float y = DrawSectionHeader(0f, width, "CS_Kandra_IrisHeader");

        foreach (IrisRow iris in irises) {
            Rect row = new Rect(0f, y, width, IrisRowHeight);

            if (DrawEyeRow(row, iris.labelKey.Translate(), iris.name == designIrisSize, iris.name, designEyeLight)) {
                designIrisSize = iris.name;
            }

            y += IrisRowHeight;
        }

        y += gap;
        y = DrawSectionHeader(y, width, "CS_Kandra_OddEyesHeader");

        bool wantsOdd = odd;
        DrawCheck(new Rect(0f, y, width, CheckRowHeight), "CS_Kandra_OddEyesToggle", ref wantsOdd);
        y = DrawNote(y + CheckRowHeight, width, oddNote, oddNoteHeight);

        // The sentinel, never null: null means the player changed nothing, so it cannot turn this off.
        if (wantsOdd != odd) {
            designEyeColourTwo = wantsOdd ? FirstColourApartFrom(designEyeColour) : KandraAppearance.EyeColourNone;
        }

        if (odd) {
            for (int i = 0; i < stones.Count; i++) {
                (string name, string hex, string labelKey) = stones[i];

                Rect row = new Rect(
                    (i % SwatchColumns) * (column + gap),
                    y + ((i / SwatchColumns) * SwatchRowHeight),
                    column,
                    SwatchRowHeight - 2f
                );

                if (DrawSwatch(row, hex, labelKey.Translate(), name == designEyeColourTwo)) designEyeColourTwo = name;
            }

            y += Mathf.CeilToInt(stones.Count / (float)SwatchColumns) * SwatchRowHeight;
        }

        y += gap;
        y = DrawSectionHeader(y, width, "CS_Kandra_EyeLightHeader");

        bool wantsLight = lit;
        DrawCheck(new Rect(0f, y, width, CheckRowHeight), "CS_Kandra_EyeLightToggle", ref wantsLight);
        y = DrawNote(y + CheckRowHeight, width, lightNote, lightNoteHeight);

        // steady, not the table's first row: dim sits barely above unlit and reads as a light that failed.
        if (wantsLight != lit) {
            designEyeLight = wantsLight ? KandraAppearance.DefaultEyeLight : KandraAppearance.EyeLightOff;
        }

        if (lit) {
            for (int i = 0; i < lights.Count; i++) {
                (string name, float _, string labelKey) = lights[i];
                Rect row = new Rect(0f, y + (i * IrisRowHeight), width, IrisRowHeight);

                // A flat chip cannot tell two lights apart, so the chip draws the eye itself.
                if (DrawEyeRow(row, labelKey.Translate(), name == designEyeLight, designIrisSize, name)) {
                    designEyeLight = name;
                }
            }
        }

        Widgets.EndScrollView();
    }

    /// <summary>A row previewing the eye it would pick: one iris, at a size, under a light.</summary>
    private bool DrawEyeRow(Rect row, TaggedString label, bool selected, string? irisSize, string? light) {
        if (selected) {
            Widgets.DrawHighlightSelected(row);
        } else {
            Widgets.DrawHighlightIfMouseover(row);
        }

        Rect socket = new Rect(row.x + 4f, row.y + ((row.height - IrisChipSize) / 2f), IrisChipSize, IrisChipSize);
        Widgets.DrawBoxSolid(socket, BodyColor);

        // Every size shares a canvas, so cropping all of them to one window keeps the sizes to scale.
        Color previous = GUI.color;
        DrawIris(socket, IrisLeft(designGender == Gender.Female, irisSize), designEyeColour, light, IrisChipCoords);
        GUI.color = previous;

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, selected ? BorderColor : BodyTextColor)) {
            Rect text = new Rect(socket.xMax + 6f, row.y, row.width - IrisChipSize - 14f, row.height);
            Widgets.Label(text, label);
        }

        MouseoverSounds.DoRegion(row);

        return Widgets.ButtonInvisible(row);
    }

    /// <summary>One iris the way a pawn draws it: the cutout, then the bloom node over the same art.</summary>
    /// <remarks>Without the bloom a pale stone clamps to white and every light looks the same.</remarks>
    private static void DrawIris(Rect rect, Texture2D art, string? stone, string? light, Rect texCoords = default) {
        GUI.color = KandraAppearance.EyeDrawColorFor(stone, light);

        if (texCoords == default) {
            GUI.DrawTexture(rect, art, ScaleMode.ScaleToFit);
        } else {
            GUI.DrawTextureWithTexCoords(rect, art, texCoords);
        }

        // The glow node carries the strength in alpha rather than the channels, so nothing clamps.
        Color glow = KandraAppearance.EyeDrawColorFor(stone, light);
        glow.a = KandraAppearance.EyeLightStrengthFor(light) - 1f;
        if (glow.a <= 0f) return;

        GUI.color = Color.white;
        GenUI.DrawTextureWithMaterial(
            rect,
            art,
            MaterialPool.MatFrom(art, Verse.ShaderDatabase.MoteGlow, glow),
            texCoords
        );
    }

    /// <summary>A section label in the window's own gold, and where the rows under it start.</summary>
    private static float DrawSectionHeader(float y, float width, string labelKey) {
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, BorderColor)) {
            Widgets.Label(new Rect(0f, y, width, 18f), labelKey.Translate());
        }

        return y + SectionHeaderHeight;
    }

    /// <summary>The line under a checkbox saying what ticking it actually does.</summary>
    private static float DrawNote(float y, float width, TaggedString text, float height) {
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, BodyTextColor)) {
            Widgets.Label(new Rect(0f, y, width, height), text);
        }

        return y + height;
    }

    /// <summary>A vanilla checkbox that answers the mouse the way the rest of the designer does.</summary>
    private static void DrawCheck(Rect row, string labelKey, ref bool on) {
        Widgets.DrawHighlightIfMouseover(row);
        MouseoverSounds.DoRegion(row);

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, BodyTextColor)) {
            Widgets.CheckboxLabeled(row, labelKey.Translate(), ref on);
        }
    }

    /// <summary>What the second eye starts as, so turning odd eyes on is never a colour that matches.</summary>
    private static string FirstColourApartFrom(string? colour) {
        foreach ((string name, string _, string _) in KandraAppearance.AllEyeColours) {
            if (name != colour) return name;
        }

        return KandraAppearance.EyeColourNone;
    }
}
