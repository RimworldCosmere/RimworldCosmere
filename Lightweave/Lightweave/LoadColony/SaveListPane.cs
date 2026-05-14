using System;
using System.Collections.Generic;
using System.IO;
using Cosmere.Lightweave.Feedback;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.MainMenu;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Typography.Typography;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.LoadColony;

public static class SaveListPane {
    private static readonly Rem RowHeight = new Rem(5.0f);
    private static readonly Rem StripeWidth = new Rem(0.1875f);

    public static LightweaveNode Create(
        List<SaveFileInfo> files,
        string? selected,
        Action<string> onSelect
    ) {
        return Box.Create(
            children: c => c.Add(ScrollArea.Create(
                content: BuildList(files, selected, onSelect)
            )),
            style: new Style {
                Padding = EdgeInsets.Zero,
                Border = new BorderSpec(Right: new Rem(0.0625f), Color: ThemeSlot.BorderSubtle),
            }
        );
    }

    private static LightweaveNode BuildList(
        List<SaveFileInfo> files,
        string? selected,
        Action<string> onSelect
    ) {
        return Stack.Create(SpacingScale.None, s => {
            if (files == null || files.Count == 0) {
                s.Add(BuildEmptyState());
                return;
            }
            for (int i = 0; i < files.Count; i++) {
                SaveFileInfo file = files[i];
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                bool isSelected = string.Equals(fileName, selected, StringComparison.OrdinalIgnoreCase);
                s.Add(BuildRow(file, fileName, isSelected, () => onSelect(fileName)));
                if (i < files.Count - 1) {
                    s.Add(Divider.Horizontal());
                }
            }
        });
    }

    private static LightweaveNode BuildRow(SaveFileInfo file, string fileName, bool isSelected, Action onClick) {
        bool isAuto = IsAutosave(fileName);
        LightweaveNode node = NodeBuilder.New("SaveListRow:" + fileName);
        node.PreferredHeight = RowHeight.ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            InteractionState state = InteractionState.Resolve(rect, null, false);

            if (isSelected) {
                PaintBox.Draw(rect, BackgroundSpec.Of(new Color(1f, 1f, 1f, 0.06f)), null, null);
            }
            else if (state.Hovered) {
                PaintBox.Draw(rect, BackgroundSpec.Of(new Color(1f, 1f, 1f, 0.03f)), null, null);
            }

            if (isSelected) {
                Rect stripe = new Rect(rect.x, rect.y, StripeWidth.ToPixels(), rect.height);
                PaintBox.Draw(stripe, BackgroundSpec.Of(ThemeSlot.SurfaceAccent), null, null);
            }

            float padX = SpacingScale.Md.ToPixels();
            float padY = SpacingScale.Sm.ToPixels();
            Rect content = new Rect(rect.x + padX, rect.y + padY, rect.width - padX * 2f, rect.height - padY * 2f);

            SaveStatusInspector.SaveStatus status = SaveStatusInspector.Inspect(file);
            string display = status.DisplayName;
            string detail = ResolveDetail(status);

            float chipReserve = 0f;
            if (isAuto) {
                LightweaveNode autoChip = Tag.Create(
                    "CL_LoadColony_AutoChip".Translate(),
                    textColor: ThemeSlot.TextMuted,
                    borderColor: ThemeSlot.BorderSubtle
                );
                float chipW = autoChip.MeasureWidth?.Invoke() ?? new Rem(3f).ToPixels();
                float chipH = autoChip.PreferredHeight ?? new Rem(1.25f).ToPixels();
                Rect chipRect = new Rect(
                    content.xMax - chipW,
                    content.y + (content.height - chipH) * 0.5f,
                    chipW,
                    chipH
                );
                autoChip.Paint?.Invoke(chipRect, () => { });
                chipReserve = chipW + SpacingScale.Sm.ToPixels();
            }

            float labelWidth = Mathf.Max(0f, content.width - chipReserve);

            Font titleFont = theme.GetFont(FontRole.Display);
            int titlePx = Mathf.RoundToInt(new Rem(1.05f).ToFontPx());
            GUIStyle titleStyle = GuiStyleCache.GetOrCreate(titleFont, titlePx, FontStyle.Normal);
            titleStyle.alignment = TextAnchor.UpperLeft;
            titleStyle.clipping = TextClipping.Clip;

            Color saved = GUI.color;
            float titleCursor = content.x;
            if (isSelected) {
                GUIContent starGc = new GUIContent("★");
                float starW = titleStyle.CalcSize(starGc).x;
                GUI.color = theme.GetColor(ThemeSlot.SurfaceAccent);
                Rect starRect = new Rect(titleCursor, content.y, starW + 2f, titlePx + 6f);
                GUI.Label(RectSnap.Snap(starRect), "★", titleStyle);
                titleCursor += starW + new Rem(0.4f).ToPixels();
            }
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
            Rect titleRect = new Rect(titleCursor, content.y, Mathf.Max(0f, content.xMax - chipReserve - titleCursor), titlePx + 6f);
            GUI.Label(RectSnap.Snap(titleRect), display, titleStyle);

            Font metaFont = theme.GetFont(FontRole.Body);
            int detailPx = Mathf.RoundToInt(new Rem(0.7f).ToFontPx());
            GUIStyle detailStyle = GuiStyleCache.GetOrCreate(metaFont, detailPx, FontStyle.Normal);
            detailStyle.alignment = TextAnchor.UpperLeft;
            detailStyle.clipping = TextClipping.Clip;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            Rect detailRect = new Rect(content.x, titleRect.yMax + 2f, labelWidth, detailPx + 4f);
            GUI.Label(RectSnap.Snap(detailRect), detail, detailStyle);

            if (status.ModMatch == SaveStatusInspector.ModMatchKind.Mismatch) {
                int count = status.MissingModNames.Count;
                if (count > 0) {
                    string warn = "CL_LoadColony_Status_ModsMissing".Translate(count.Named("COUNT"));
                    GUI.color = theme.GetColor(ThemeSlot.StatusWarning);
                    Rect warnRect = new Rect(content.x, detailRect.yMax + 2f, labelWidth, detailPx + 4f);
                    GUI.Label(RectSnap.Snap(warnRect), warn, detailStyle);
                }
            }
            GUI.color = saved;

            InteractionFeedback.Apply(rect, true, true);

            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                onClick?.Invoke();
                e.Use();
            }
        };
        return node;
    }

    private static bool IsAutosave(string fileName) {
        return !string.IsNullOrEmpty(fileName)
            && fileName.StartsWith("Autosave", StringComparison.OrdinalIgnoreCase);
    }

    private static LightweaveNode BuildEmptyState() {
        return Container.Create(
            child: Stack.Create(SpacingScale.Xs, s => {
                s.Add(Eyebrow.Create("CL_LoadColony_Empty_Eyebrow".Translate()));
                s.Add(Text.Create(
                    "CL_LoadColony_Empty_Body".Translate(),
                    wrap: true,
                    style: new Style { TextColor = ThemeSlot.TextSecondary }
                ));
            }),
            style: new Style {
                Padding = EdgeInsets.All(SpacingScale.Lg),
            }
        );
    }

    private static string ResolveDetail(SaveStatusInspector.SaveStatus status) {
        SaveSidecarData? sc = status.Sidecar;
        List<string> parts = new List<string>(3);
        if (sc != null) {
            if (sc.DaysSurvived > 0) {
                parts.Add("CL_LoadColony_DayShort".Translate(sc.DaysSurvived.Named("DAY")).Resolve());
            }
            else if (!string.IsNullOrEmpty(sc.Quadrum) && sc.InGameYear > 0) {
                parts.Add(sc.Quadrum + " " + sc.InGameYear);
            }
            if (sc.ColonistCount > 0) {
                parts.Add("CL_LoadColony_ColonistsShort".Translate(sc.ColonistCount.Named("COUNT")).Resolve());
            }
        }
        parts.Add(SaveMetadata.FormatRelative(status.LastWriteTime));
        return string.Join("  ·  ", parts);
    }
}
