using System;
using System.Collections.Generic;
using System.IO;
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
    private static readonly Rem RowHeight = new Rem(4.25f);
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
                Background = BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
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
        LightweaveNode node = NodeBuilder.New("SaveListRow:" + fileName);
        node.PreferredHeight = RowHeight.ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            InteractionState state = InteractionState.Resolve(rect, null, false);

            ThemeSlot bgSlot = isSelected
                ? ThemeSlot.SurfaceRaised
                : (state.Hovered ? ThemeSlot.SurfaceRaised : ThemeSlot.SurfaceSunken);
            PaintBox.Draw(rect, BackgroundSpec.Of(bgSlot), null, null);

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

            Font bodyFont = theme.GetFont(FontRole.BodyBold);
            int titlePx = Mathf.RoundToInt(new Rem(0.95f).ToFontPx());
            GUIStyle titleStyle = GuiStyleCache.GetOrCreate(bodyFont, titlePx, FontStyle.Bold);
            titleStyle.alignment = TextAnchor.UpperLeft;

            Color saved = GUI.color;
            GUI.color = theme.GetColor(isSelected ? ThemeSlot.TextPrimary : ThemeSlot.TextPrimary);
            Rect titleRect = new Rect(content.x, content.y, content.width, titlePx + 4f);
            GUI.Label(RectSnap.Snap(titleRect), display, titleStyle);

            Font tinyFont = theme.GetFont(FontRole.Body);
            int detailPx = Mathf.RoundToInt(new Rem(0.7f).ToFontPx());
            GUIStyle detailStyle = GuiStyleCache.GetOrCreate(tinyFont, detailPx, FontStyle.Normal);
            detailStyle.alignment = TextAnchor.UpperLeft;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            Rect detailRect = new Rect(content.x, titleRect.yMax + 2f, content.width, detailPx + 4f);
            GUI.Label(RectSnap.Snap(detailRect), detail, detailStyle);

            if (status.ModMatch == SaveStatusInspector.ModMatchKind.Mismatch) {
                int count = status.MissingModNames.Count;
                if (count > 0) {
                    string warn = "CL_LoadColony_Status_ModsMissing".Translate(count.Named("COUNT"));
                    GUI.color = theme.GetColor(ThemeSlot.StatusWarning);
                    Rect warnRect = new Rect(content.x, detailRect.yMax + 2f, content.width, detailPx + 4f);
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
        string left = status.Sidecar != null && status.Sidecar.DaysSurvived > 0
            ? "CL_LoadColony_DayShort".Translate(status.Sidecar.DaysSurvived.Named("DAY"))
            : "—";
        string right = SaveMetadata.FormatRelative(status.LastWriteTime);
        return left + "  ·  " + right;
    }
}
