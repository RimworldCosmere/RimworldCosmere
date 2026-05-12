using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.MainMenu;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using RimWorld;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Typography.Typography;
using Display = Cosmere.Lightweave.Typography.Display;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.LoadColony;

public static class SaveDetailPane {
    public static LightweaveNode Create(
        SaveFileInfo? file,
        SaveStatusInspector.SaveStatus? status,
        Action onClose,
        Action onAfterDelete
    ) {
        if (file == null || status == null) {
            return BuildEmpty();
        }

        return Box.Create(
            children: c => c.Add(Stack.Create(SpacingScale.Md, s => {
                s.AddFlex(ScrollArea.Create(
                    content: BuildBody(status),
                    showScrollbar: true
                ));
                s.Add(BuildActionBar(file, onClose, onAfterDelete));
            })),
            style: new Style {
                Padding = EdgeInsets.All(SpacingScale.Lg),
                Background = BackgroundSpec.Of(ThemeSlot.SurfacePrimary),
            }
        );
    }

    private static LightweaveNode BuildBody(SaveStatusInspector.SaveStatus status) {
        return Stack.Create(SpacingScale.Lg, s => {
            s.Add(HStack.Create(SpacingScale.Lg, h => {
                h.Add(BuildPreview(status), new Rem(13.75f).ToPixels());
                h.AddFlex(Stack.Create(SpacingScale.Xs, t => {
                    t.Add(BuildStatusEyebrow(status));
                    t.Add(Display.Create(status.DisplayName, style: new Style { TextAlign = TextAlign.Start }, level: 2));
                    t.Add(Text.Create(BuildSubtitle(status), style: new Style { TextColor = ThemeSlot.TextMuted }));
                    t.Add(Text.Create(BuildStory(status), style: new Style { TextColor = ThemeSlot.TextSecondary }));
                }));
            }));
            s.Add(KeyValueTable.Create(BuildStatRows(status), KeyValueOrientation.Horizontal));
            s.Add(BuildConditionsSection(status));
            s.Add(BuildBadgeRow(status));
        });
    }

    private static string BuildStory(SaveStatusInspector.SaveStatus status) {
        if (status.Sidecar == null) {
            return string.Empty;
        }
        List<string> parts = new List<string>();
        if (!string.IsNullOrEmpty(status.Sidecar.ColonyName)) {
            parts.Add(status.Sidecar.ColonyName + ".");
        }
        if (status.Sidecar.ColonistCount > 0) {
            parts.Add(status.Sidecar.ColonistCount + (status.Sidecar.ColonistCount == 1 ? " colonist" : " colonists") + ".");
        }
        if (status.Sidecar.AnimalCount > 0) {
            parts.Add(status.Sidecar.AnimalCount + (status.Sidecar.AnimalCount == 1 ? " animal" : " animals") + ".");
        }
        if (status.Sidecar.Permadeath) {
            parts.Add("Permadeath.");
        }
        if (status.Sidecar.MoodAveragePercent > 0) {
            parts.Add("Mood " + status.Sidecar.MoodAveragePercent + "%.");
        }
        return string.Join(" ", parts);
    }

    private static LightweaveNode BuildConditionsSection(SaveStatusInspector.SaveStatus status) {
        return Stack.Create(SpacingScale.Sm, s => {
            s.Add(Eyebrow.Create(
                "CL_LoadColony_Conditions".Translate(),
                style: new Style { TextColor = ThemeSlot.TextMuted, TextAlign = TextAlign.Start, LetterSpacing = Tracking.Widest }
            ));
            s.Add(HStack.Create(SpacingScale.Sm, h => {
                if (status.Sidecar == null) {
                    h.Add(BuildPill("CL_LoadColony_Conditions_None".Translate(), ThemeSlot.TextMuted), new Rem(11f).ToPixels());
                    return;
                }
                if (!string.IsNullOrEmpty(status.Sidecar.ActiveThreat)) {
                    h.Add(BuildPill(status.Sidecar.ActiveThreat, ThemeSlot.StatusDanger), new Rem(11f).ToPixels());
                }
                if (status.Sidecar.ThreatScale > 1.5f) {
                    h.Add(BuildPill("Heightened threat", ThemeSlot.StatusWarning), new Rem(9f).ToPixels());
                }
                if (status.Sidecar.Permadeath) {
                    h.Add(BuildPill("Permadeath", ThemeSlot.StatusDanger), new Rem(7f).ToPixels());
                }
                if (string.IsNullOrEmpty(status.Sidecar.ActiveThreat) && status.Sidecar.ThreatScale <= 1.5f && !status.Sidecar.Permadeath) {
                    h.Add(BuildPill("CL_LoadColony_Conditions_None".Translate(), ThemeSlot.TextMuted), new Rem(11f).ToPixels());
                }
            }));
        });
    }

    private static LightweaveNode BuildPreview(SaveStatusInspector.SaveStatus status) {
        LightweaveNode node = NodeBuilder.New("SavePreview");
        node.PreferredHeight = new Rem(8.75f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            PaintBox.Draw(
                rect,
                BackgroundSpec.Of(ThemeSlot.MapPreviewTint),
                BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
                RadiusSpec.All(RadiusScale.Sm)
            );

            DrawDiagonalHatch(rect, theme);

            DrawRadialOverlay(rect, theme);

            DrawPreviewLabel(rect, theme);
        };
        return node;
    }

    private static void DrawDiagonalHatch(Rect rect, Theme.Theme theme) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }
        Color saved = GUI.color;
        Color hatch = theme.GetColor(ThemeSlot.SurfaceAccent);
        hatch.a = 0.05f;
        GUI.color = hatch;
        float step = new Rem(1f).ToPixels();
        float thickness = Mathf.Max(1f, new Rem(0.0625f).ToPixels());
        Matrix4x4 prev = GUI.matrix;
        GUI.BeginClip(rect);
        Rect inner = new Rect(0f, 0f, rect.width, rect.height);
        for (float offset = -inner.height; offset < inner.width + inner.height; offset += step) {
            float x0 = offset;
            float y0 = 0f;
            float x1 = offset + inner.height;
            float y1 = inner.height;
            DrawLine(x0, y0, x1, y1, thickness);
        }
        GUI.EndClip();
        GUI.matrix = prev;
        GUI.color = saved;
    }

    private static void DrawLine(float x0, float y0, float x1, float y1, float thickness) {
        float dx = x1 - x0;
        float dy = y1 - y0;
        float len = Mathf.Sqrt(dx * dx + dy * dy);
        if (len < 0.001f) return;
        Matrix4x4 prev = GUI.matrix;
        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        Vector2 pivot = new Vector2(x0, y0);
        GUIUtility.RotateAroundPivot(angle, pivot);
        GUI.DrawTexture(RectSnap.Snap(new Rect(x0, y0, len, thickness)), BaseContent.WhiteTex);
        GUI.matrix = prev;
    }

    private static void DrawRadialOverlay(Rect rect, Theme.Theme theme) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }
        Color saved = GUI.color;
        Color core = theme.GetColor(ThemeSlot.SurfaceAccent);
        core.a = 0.10f;
        GUI.color = core;
        float diam = Mathf.Min(rect.width, rect.height) * 0.7f;
        Rect glow = new Rect(rect.x + rect.width * 0.3f - diam * 0.5f, rect.y + rect.height * 0.3f - diam * 0.5f, diam, diam);
        GUI.DrawTexture(RectSnap.Snap(glow), BaseContent.WhiteTex);
        GUI.color = saved;
    }

    private static void DrawPreviewLabel(Rect rect, Theme.Theme theme) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }
        Font font = theme.GetFont(FontRole.Mono);
        int pixelSize = Mathf.RoundToInt(new Rem(0.625f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Normal);
        style.alignment = TextAnchor.LowerLeft;
        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextMuted);
        float padX = SpacingScale.Sm.ToPixels();
        float padY = SpacingScale.Xs.ToPixels();
        Rect labelRect = new Rect(rect.x + padX, rect.y + padY, rect.width - padX * 2f, rect.height - padY * 2f);
        GUI.Label(RectSnap.Snap(labelRect), "CL_LoadColony_PreviewLabel".Translate(), style);
        GUI.color = saved;
    }

    private static LightweaveNode BuildStatusEyebrow(SaveStatusInspector.SaveStatus status) {
        string label;
        ThemeSlot tone;
        if (status.Compatibility == SaveStatusInspector.SaveCompatibility.Incompatible) {
            label = "CL_LoadColony_Status_Incompatible".Translate();
            tone = ThemeSlot.StatusDanger;
        }
        else if (status.ModMatch == SaveStatusInspector.ModMatchKind.Mismatch) {
            label = "CL_LoadColony_Status_ModsChanged".Translate();
            tone = ThemeSlot.StatusWarning;
        }
        else if (status.Compatibility == SaveStatusInspector.SaveCompatibility.Match) {
            label = "CL_LoadColony_Status_Ready".Translate();
            tone = ThemeSlot.StatusSuccess;
        }
        else {
            label = "CL_LoadColony_Status_Differs".Translate();
            tone = ThemeSlot.StatusWarning;
        }
        return Eyebrow.Create(label, style: new Style { TextColor = tone });
    }

    private static string BuildSubtitle(SaveStatusInspector.SaveStatus status) {
        List<string> parts = new List<string>();
        if (status.Sidecar != null) {
            if (status.Sidecar.DaysSurvived > 0) {
                parts.Add("CL_LoadColony_DayShort".Translate(status.Sidecar.DaysSurvived.Named("DAY")).Resolve());
            }
            if (!string.IsNullOrEmpty(status.Sidecar.Quadrum)) {
                parts.Add(status.Sidecar.Quadrum);
            }
            if (status.Sidecar.InGameYear > 0) {
                parts.Add(status.Sidecar.InGameYear.ToString(CultureInfo.InvariantCulture));
            }
        }
        parts.Add(SaveMetadata.FormatRelative(status.LastWriteTime));
        return string.Join("  ·  ", parts);
    }

    private static List<KeyValueRow> BuildStatRows(SaveStatusInspector.SaveStatus status) {
        return new List<KeyValueRow> {
            new KeyValueRow(
                "CL_LoadColony_Stat_Biome".Translate(),
                status.Sidecar?.Biome ?? "—"
            ),
            new KeyValueRow(
                "CL_LoadColony_Stat_Climate".Translate(),
                status.Sidecar?.Climate ?? "—"
            ),
            new KeyValueRow(
                "CL_LoadColony_Stat_Colonists".Translate(),
                status.Sidecar != null ? status.Sidecar.ColonistCount.ToString(CultureInfo.InvariantCulture) : "—"
            ),
            new KeyValueRow(
                "CL_LoadColony_Stat_Wealth".Translate(),
                FormatWealth(status.Sidecar?.Wealth)
            ),
        };
    }

    private static string FormatWealth(float? wealth) {
        if (!wealth.HasValue || wealth.Value <= 0f) {
            return "—";
        }
        return "$" + wealth.Value.ToString("N0", CultureInfo.InvariantCulture);
    }

    private static LightweaveNode BuildBadgeRow(SaveStatusInspector.SaveStatus status) {
        return HStack.Create(SpacingScale.Sm, h => {
            h.Add(BuildPill(BuildVersionLabel(status), VersionTone(status)), VersionWidth(status));
            h.Add(BuildPill(BuildModLabel(status), ModTone(status)), ModWidth(status));
        });
    }

    private static float VersionWidth(SaveStatusInspector.SaveStatus status) {
        return new Rem(BuildVersionLabel(status).Length * 0.55f + 1.5f).ToPixels();
    }

    private static float ModWidth(SaveStatusInspector.SaveStatus status) {
        return new Rem(BuildModLabel(status).Length * 0.55f + 1.5f).ToPixels();
    }

    private static string BuildVersionLabel(SaveStatusInspector.SaveStatus status) {
        if (status.Compatibility == SaveStatusInspector.SaveCompatibility.Match) {
            return "CL_LoadColony_Pill_VersionMatch".Translate();
        }
        if (string.IsNullOrEmpty(status.GameVersion)) {
            return "CL_LoadColony_Pill_VersionUnknown".Translate();
        }
        return "CL_LoadColony_Pill_VersionShort".Translate(status.GameVersion.Named("VER")).Resolve();
    }

    private static ThemeSlot VersionTone(SaveStatusInspector.SaveStatus status) {
        return status.Compatibility switch {
            SaveStatusInspector.SaveCompatibility.Match => ThemeSlot.StatusSuccess,
            SaveStatusInspector.SaveCompatibility.DifferentBuild => ThemeSlot.StatusWarning,
            SaveStatusInspector.SaveCompatibility.DifferentVersion => ThemeSlot.StatusWarning,
            SaveStatusInspector.SaveCompatibility.Incompatible => ThemeSlot.StatusDanger,
            _ => ThemeSlot.TextMuted,
        };
    }

    private static string BuildModLabel(SaveStatusInspector.SaveStatus status) {
        if (status.ModMatch == SaveStatusInspector.ModMatchKind.Match) {
            return "CL_LoadColony_Pill_ModsReady".Translate();
        }
        int missing = status.MissingModNames.Count;
        return "CL_LoadColony_Pill_ModsChanged".Translate(missing.Named("COUNT")).Resolve();
    }

    private static ThemeSlot ModTone(SaveStatusInspector.SaveStatus status) {
        return status.ModMatch switch {
            SaveStatusInspector.ModMatchKind.Match => ThemeSlot.StatusSuccess,
            SaveStatusInspector.ModMatchKind.Mismatch => ThemeSlot.StatusWarning,
            _ => ThemeSlot.TextMuted,
        };
    }

    private static LightweaveNode BuildPill(string label, ThemeSlot tone) {
        LightweaveNode node = NodeBuilder.New("Pill:" + label);
        node.PreferredHeight = new Rem(1.6f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            PaintBox.Draw(
                rect,
                BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
                RadiusSpec.All(RadiusScale.Full)
            );

            float dotSize = new Rem(0.5f).ToPixels();
            float padX = SpacingScale.Sm.ToPixels();
            Rect dotRect = new Rect(
                rect.x + padX,
                rect.y + (rect.height - dotSize) * 0.5f,
                dotSize,
                dotSize
            );
            PaintBox.Draw(
                dotRect,
                BackgroundSpec.Of(tone),
                null,
                RadiusSpec.All(RadiusScale.Full)
            );

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.6875f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleLeft;

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextSecondary);
            Rect labelRect = new Rect(
                dotRect.xMax + SpacingScale.Xs.ToPixels(),
                rect.y,
                rect.xMax - (dotRect.xMax + SpacingScale.Xs.ToPixels()) - padX,
                rect.height
            );
            GUI.Label(RectSnap.Snap(labelRect), label.ToUpperInvariant(), style);
            GUI.color = saved;
        };
        return node;
    }

    private static LightweaveNode BuildActionBar(SaveFileInfo file, Action onClose, Action onAfterDelete) {
        string fileName = Path.GetFileNameWithoutExtension(file.FileName);
        return HStack.Create(SpacingScale.Sm, h => {
            h.AddFlex(Button.Create(
                label: "CL_LoadColony_Load".Translate(),
                onClick: () => {
                    onClose?.Invoke();
                    GameDataSaveLoader.CheckVersionAndLoadGame(fileName);
                },
                variant: ButtonVariant.Primary,
                style: new Style { Width = Length.Stretch }
            ));
            h.Add(Button.Create(
                label: "CL_LoadColony_Rename".Translate(),
                onClick: () => RenameSave(file, onAfterDelete),
                variant: ButtonVariant.Ghost
            ), new Rem(7f).ToPixels());
            h.Add(Button.Create(
                label: "CL_LoadColony_Duplicate".Translate(),
                onClick: () => DuplicateSave(file, onAfterDelete),
                variant: ButtonVariant.Ghost
            ), new Rem(7f).ToPixels());
            h.Add(Button.Create(
                label: "CL_LoadColony_Delete".Translate(),
                onClick: () => ConfirmDelete(file, onAfterDelete),
                variant: ButtonVariant.Danger
            ), new Rem(7f).ToPixels());
        });
    }

    private static void RenameSave(SaveFileInfo file, Action onAfter) {
        string current = Path.GetFileNameWithoutExtension(file.FileName);
        Find.WindowStack.Add(new Dialog_RenameSaveFile(file, current, onAfter));
    }

    private static void DuplicateSave(SaveFileInfo file, Action onAfter) {
        try {
            string dir = file.FileInfo.DirectoryName ?? string.Empty;
            string baseName = Path.GetFileNameWithoutExtension(file.FileName);
            string ext = file.FileInfo.Extension;
            string candidate = Path.Combine(dir, baseName + "_copy" + ext);
            int n = 2;
            while (File.Exists(candidate)) {
                candidate = Path.Combine(dir, baseName + "_copy" + n + ext);
                n++;
            }
            File.Copy(file.FileInfo.FullName, candidate);
            onAfter?.Invoke();
        }
        catch (System.Exception ex) {
            Runtime.LightweaveLog.Warning($"Duplicate save failed: {ex.Message}");
        }
    }

    private class Dialog_RenameSaveFile : Window {
        private readonly SaveFileInfo file;
        private readonly Action onAfter;
        private string newName;

        public Dialog_RenameSaveFile(SaveFileInfo file, string currentName, Action onAfter) {
            this.file = file;
            this.newName = currentName;
            this.onAfter = onAfter;
            doCloseX = true;
            doCloseButton = false;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnAccept = true;
            closeOnCancel = true;
        }

        public override Vector2 InitialSize => new Vector2(400f, 160f);

        public override void DoWindowContents(Rect inRect) {
            Verse.Text.Font = GameFont.Small;
            Widgets.Label(RectSnap.SnapText(new Rect(inRect.x, inRect.y, inRect.width, 24f)), "CL_LoadColony_Rename".Translate());
            newName = Widgets.TextField(new Rect(inRect.x, inRect.y + 32f, inRect.width, 28f), newName ?? string.Empty);
            if (Widgets.ButtonText(new Rect(inRect.xMax - 120f, inRect.yMax - 32f, 120f, 28f), "OK")) {
                Apply();
                Close();
            }
        }

        private void Apply() {
            if (string.IsNullOrWhiteSpace(newName)) return;
            try {
                string dir = file.FileInfo.DirectoryName ?? string.Empty;
                string ext = file.FileInfo.Extension;
                string newPath = Path.Combine(dir, newName + ext);
                if (!File.Exists(newPath)) {
                    file.FileInfo.MoveTo(newPath);
                    onAfter?.Invoke();
                }
            }
            catch (System.Exception ex) {
                Runtime.LightweaveLog.Warning($"Rename save failed: {ex.Message}");
            }
        }
    }


    private static void ConfirmDelete(SaveFileInfo file, Action onAfterDelete) {
        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
            "ConfirmDelete".Translate(file.FileInfo.Name),
            () => {
                try {
                    file.FileInfo.Delete();
                    SaveSidecar.Delete(file.FileInfo.FullName);
                    SaveStatusInspector.Invalidate(file.FileInfo.FullName);
                    onAfterDelete?.Invoke();
                }
                catch (Exception ex) {
                    Log.Error("Lightweave delete failed: " + ex);
                }
            },
            destructive: true
        ));
    }

    private static LightweaveNode BuildEmpty() {
        return Container.Create(
            child: Stack.Create(SpacingScale.Sm, s => {
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
}
