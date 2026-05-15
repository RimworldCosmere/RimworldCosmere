using System;
using System.Collections.Generic;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Steam;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.ModsConfig;

public static class ModListPane {
    private static readonly Rem RowHeight = new Rem(2.6f);
    private static readonly Rem HeaderHeight = new Rem(2.0f);
    private static readonly Rem PaddingX = new Rem(1.0f);
    private static readonly Rem StripeWidth = new Rem(0.15f);


    private static string? dragPressedPackageId;
    private static float dragPressedY;
    private static bool dragActive;
    private static string? dragHoverPackageId;
    private static int dragHoverInsertOffset;

    private static readonly Rem ColOrder = new Rem(2.5f);
    private static readonly Rem ColCheck = new Rem(1.75f);
    private static readonly Rem ColAuthor = new Rem(8.75f);
    private static readonly Rem ColVersion = new Rem(5.625f);
    private static readonly Rem ColStatus = new Rem(5.625f);

    public static LightweaveNode Create(
        List<ModMetaData> mods,
        string? selected,
        Action<string> onSelect,
        RimWorld.Page_ModsConfig page,
        ModsTab tab,
        string query,
        Action<string> onQueryChange
    ) {
        return Box.Create(
            children: c => c.Add(Stack.Create(SpacingScale.None, s => {
                s.Add(BuildSearchBar(query, onQueryChange, mods.Count));
                s.Add(BuildColumnHeader());
                s.AddFlex(ScrollArea.Create(content: BuildList(mods, selected, onSelect, query)));
            })),
            style: new Style {
                Padding = EdgeInsets.Zero,
                Border = new BorderSpec(Right: new Rem(1f / 16f), Color: ThemeSlot.BorderSubtle),
            }
        );
    }

    private static LightweaveNode BuildSearchBar(string query, Action<string> onQueryChange, int matchCount) {
        bool hasQuery = !string.IsNullOrEmpty(query);
        bool showWorkshop = SteamManager.Initialized;
        return Box.Create(
            children: c => c.Add(HStack.Create(SpacingScale.Sm, h => {
                h.AddFlex(SearchField.Create(
                    value: query,
                    onChange: onQueryChange,
                    placeholder: "CL_ModsConfig_Search_Placeholder".Translate(),
                    variant: SearchFieldVariant.Borderless
                ));
                if (hasQuery) {
                    string matchText = (matchCount == 1
                        ? "CL_ModsConfig_Search_MatchCount".Translate(matchCount.Named("COUNT"))
                        : "CL_ModsConfig_Search_MatchCountPlural".Translate(matchCount.Named("COUNT"))).Resolve();
                    h.AddHug(Text.Create(
                        matchText,
                        style: new Style {
                            FontFamily = FontRole.Mono,
                            FontSize = new Rem(0.65f),
                            LetterSpacing = Tracking.Of(0.14f),
                            TextColor = ThemeSlot.TextMuted,
                        }
                    ));
                }
                if (showWorkshop) {
                    h.AddHug(Button.Create(
                        label: ((string)"CL_ModsConfig_Workshop_Button".Translate()).ToUpperInvariant(),
                        onClick: () => SteamUtility.OpenSteamWorkshopPage(),
                        variant: ButtonVariant.Secondary
                    ));
                }
            })),
            style: new Style {
                Padding = new EdgeInsets(Top: SpacingScale.Sm, Right: SpacingScale.Md, Bottom: SpacingScale.Sm, Left: SpacingScale.Md),
                Border = new BorderSpec(Bottom: new Rem(1f / 16f), Color: ThemeSlot.BorderSubtle),
            }
        );
    }

    private static LightweaveNode BuildColumnHeader() {
        LightweaveNode node = NodeBuilder.New("ModListHeader");
        node.PreferredHeight = HeaderHeight.ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;

            PaintBox.Draw(
                rect,
                null,
                new BorderSpec(Bottom: new Rem(1f / 16f), Color: ThemeSlot.BorderSubtle),
                null
            );

            ColumnRects cols = ComputeColumns(rect);

            Font font = theme.GetFont(FontRole.Mono);
            int px = Mathf.RoundToInt(new Rem(0.8f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, px);

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.MetadataLabel);

            style.alignment = TextAnchor.MiddleLeft;
            GUI.Label(RectSnap.Snap(cols.Order), "CL_ModsConfig_Col_Order".Translate(), style);
            GUI.Label(RectSnap.Snap(cols.Name), ((string)"CL_ModsConfig_Col_Name".Translate()).ToUpperInvariant(), style);
            GUI.Label(RectSnap.Snap(cols.Author), ((string)"CL_ModsConfig_Col_Author".Translate()).ToUpperInvariant(), style);
            GUI.Label(RectSnap.Snap(cols.Version), ((string)"CL_ModsConfig_Col_Version".Translate()).ToUpperInvariant(), style);

            style.alignment = TextAnchor.MiddleRight;
            GUI.Label(RectSnap.Snap(cols.Status), ((string)"CL_ModsConfig_Col_Status".Translate()).ToUpperInvariant(), style);

            GUI.color = saved;
        };
        return node;
    }

    private static LightweaveNode BuildList(
        List<ModMetaData> mods,
        string? selected,
        Action<string> onSelect,
        string query
    ) {
        return Stack.Create(SpacingScale.None, s => {
            if (mods == null || mods.Count == 0) {
                if (!string.IsNullOrEmpty(query)) {
                    s.Add(BuildSearchEmptyState(query));
                }
                else {
                    s.Add(BuildEmptyState());
                }
                return;
            }
            for (int i = 0; i < mods.Count; i++) {
                ModMetaData mod = mods[i];
                bool isSelected = string.Equals(mod.PackageId, selected, StringComparison.OrdinalIgnoreCase);
                int loadOrder = i + 1;
                bool zebra = (i % 2) == 1;
                s.Add(BuildRow(mod, loadOrder, isSelected, zebra, () => onSelect(mod.PackageId)));
            }
        });
    }

    private static LightweaveNode BuildRow(ModMetaData mod, int loadOrder, bool isSelected, bool zebra, Action onClick) {
        LightweaveNode node = NodeBuilder.New("ModListRow:" + mod.PackageId);
        node.PreferredHeight = RowHeight.ToPixels();
        bool draggable = mod.Active && !ModKindResolver.IsLocked(mod);
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            InteractionState state = InteractionState.Resolve(rect, null, false);
            bool isDragSource = draggable && dragActive && string.Equals(dragPressedPackageId, mod.PackageId, StringComparison.OrdinalIgnoreCase);

            if (isSelected) {
                PaintBox.Draw(rect, BackgroundSpec.Of(ThemeSlot.SurfaceRaised), null, null);
            }
            else if (zebra) {
                PaintBox.Draw(rect, BackgroundSpec.Of(ThemeSlot.SurfaceTranslucent), null, null);
            }

            if (state.Hovered && !dragActive) {
                Color hoverWash = new Color(20f / 255f, 16f / 255f, 11f / 255f, 0.35f);
                PaintBox.Draw(rect, BackgroundSpec.Of(hoverWash), null, null);
            }

            PaintBox.Draw(
                rect,
                null,
                new BorderSpec(Bottom: new Rem(1f / 16f), Color: ThemeSlot.BorderSubtle),
                null
            );

            if (isSelected) {
                Rect stripe = new Rect(rect.x, rect.y, StripeWidth.ToPixels(), rect.height);
                PaintBox.Draw(stripe, BackgroundSpec.Of(ThemeSlot.SurfaceAccent), null, null);
            }

            ColumnRects cols = ComputeColumns(rect);

            Color saved = GUI.color;
            if (isDragSource) {
                GUI.color = new Color(saved.r, saved.g, saved.b, saved.a * 0.45f);
            }

            DrawOrder(cols.Order, loadOrder, theme);
            DrawCheckbox(cols.Check, mod, theme);
            DrawName(cols.Name, mod, theme);
            DrawAuthor(cols.Author, mod, theme);
            DrawVersion(cols.Version, mod, theme);
            DrawStatus(cols.Status, mod, theme);

            GUI.color = saved;

            Event e = Event.current;

            if (draggable && dragActive && !isDragSource && rect.Contains(e.mousePosition)) {
                dragHoverPackageId = mod.PackageId;
                dragHoverInsertOffset = e.mousePosition.y < rect.y + rect.height / 2f ? 0 : 1;
                float indicatorY = dragHoverInsertOffset == 0 ? rect.y : rect.yMax - 2f;
                Rect indicator = new Rect(rect.x + StripeWidth.ToPixels(), indicatorY, rect.width - StripeWidth.ToPixels(), 2f);
                PaintBox.Draw(indicator, BackgroundSpec.Of(ThemeSlot.SurfaceAccent), null, null);
            }

            if (draggable && e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition)
                && !cols.Check.Contains(e.mousePosition)) {
                dragPressedPackageId = mod.PackageId;
                dragPressedY = e.mousePosition.y;
                dragActive = false;
                dragHoverPackageId = null;
            }

            if (draggable && e.type == EventType.MouseDrag && !dragActive
                && string.Equals(dragPressedPackageId, mod.PackageId, StringComparison.OrdinalIgnoreCase)
                && Mathf.Abs(e.mousePosition.y - dragPressedY) > 4f) {
                dragActive = true;
            }

            InteractionFeedback.Apply(rect, true, true);

            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                if (dragActive && dragPressedPackageId != null) {
                    string fromPkg = dragPressedPackageId;
                    string toPkg = dragHoverPackageId ?? mod.PackageId;
                    int insertOffset = dragHoverPackageId != null ? dragHoverInsertOffset
                        : (e.mousePosition.y < rect.y + rect.height / 2f ? 0 : 1);
                    TryReorderByPackageId(fromPkg, toPkg, insertOffset);
                    ResetDragState();
                    e.Use();
                }
                else {
                    ResetDragState();
                    onClick?.Invoke();
                    e.Use();
                }
            }
        };
        return node;
    }

    private static void ResetDragState() {
        dragPressedPackageId = null;
        dragActive = false;
        dragHoverPackageId = null;
        dragHoverInsertOffset = 0;
    }

    private static void TryReorderByPackageId(string fromPkg, string toPkg, int insertOffset) {
        if (string.Equals(fromPkg, toPkg, StringComparison.OrdinalIgnoreCase)) return;
        List<ModMetaData> active = Verse.ModsConfig.ActiveModsInLoadOrder.ToList();
        int fromIdx = active.FindIndex(m => string.Equals(m.PackageId, fromPkg, StringComparison.OrdinalIgnoreCase));
        int toIdx = active.FindIndex(m => string.Equals(m.PackageId, toPkg, StringComparison.OrdinalIgnoreCase));
        if (fromIdx < 0 || toIdx < 0 || fromIdx == toIdx) return;
        int finalPos;
        if (toIdx > fromIdx) {
            finalPos = insertOffset == 0 ? toIdx - 1 : toIdx;
        }
        else {
            finalPos = insertOffset == 0 ? toIdx : toIdx + 1;
        }
        if (finalPos == fromIdx) return;
        int newIndex = finalPos > fromIdx ? finalPos + 1 : finalPos;
        ForceReorderActive(fromIdx, newIndex);
    }

    private static void ForceReorderActive(int modIndex, int newIndex) {
        System.Reflection.BindingFlags privStatic = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
        System.Reflection.FieldInfo? dataField = typeof(Verse.ModsConfig).GetField("data", privStatic);
        object? data = dataField?.GetValue(null);
        if (data == null) return;
        System.Reflection.FieldInfo? activeModsField = data.GetType().GetField("activeMods", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (activeModsField?.GetValue(data) is not List<string> activeMods) return;
        if (modIndex < 0 || modIndex >= activeMods.Count) return;
        int clamped = Mathf.Clamp(newIndex, 0, activeMods.Count);
        string packageId = activeMods[modIndex];
        activeMods.Insert(clamped, packageId);
        activeMods.RemoveAt(modIndex < clamped ? modIndex : modIndex + 1);
        System.Reflection.FieldInfo? dirtyField = typeof(Verse.ModsConfig).GetField("activeModsInLoadOrderCachedDirty", privStatic);
        dirtyField?.SetValue(null, true);
    }

    private static void DrawOrder(Rect r, int order, Theme.Theme theme) {
        Font font = theme.GetFont(FontRole.Mono);
        int px = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, px);
        style.alignment = TextAnchor.MiddleLeft;
        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextMuted);
        GUI.Label(RectSnap.Snap(r), order.ToString("D2"), style);
        GUI.color = saved;
    }

    private static void DrawCheckbox(Rect r, ModMetaData mod, Theme.Theme theme) {
        float boxSize = new Rem(1.0f).ToPixels();
        Rect box = new Rect(
            r.x,
            r.y + (r.height - boxSize) / 2f,
            boxSize,
            boxSize
        );
        bool toggleLocked = ModKindResolver.IsLocked(mod);
        bool hovered = !toggleLocked && Mouse.IsOver(box);
        Checkbox.DrawBox(box, mod.Active, toggleLocked, hovered);
        Event e = Event.current;
        if (!toggleLocked && e.type == EventType.MouseUp && e.button == 0 && box.Contains(e.mousePosition)) {
            Verse.ModsConfig.SetActive(mod, !mod.Active);
            e.Use();
        }
    }

    private static void DrawName(Rect r, ModMetaData mod, Theme.Theme theme) {
        Font font = theme.GetFont(FontRole.Body);
        int px = Mathf.RoundToInt(new Rem(1.0f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, px);
        style.alignment = TextAnchor.MiddleLeft;
        style.clipping = TextClipping.Clip;

        ModKind kind = ModKindResolver.Resolve(mod);
        bool showDlc = kind == ModKind.Core || kind == ModKind.Expansion;
        bool showLib = kind == ModKind.Library;
        bool locked = ModKindResolver.IsLocked(mod);

        float tagGap = new Rem(0.375f).ToPixels();
        float tagH = new Rem(1.25f).ToPixels();
        float tagPad = new Rem(0.375f).ToPixels();

        string name = mod.Name ?? mod.PackageId ?? string.Empty;
        Vector2 nameSize = style.CalcSize(new GUIContent(name));
        float maxNameW = r.width - (showDlc || showLib ? new Rem(2.5f).ToPixels() : 0f) - (locked ? new Rem(1.0f).ToPixels() : 0f);
        float nameW = Mathf.Min(nameSize.x, maxNameW);

        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
        Rect nameRect = new Rect(r.x, r.y, nameW, r.height);
        GUI.Label(RectSnap.Snap(nameRect), name, style);
        GUI.color = saved;

        float cursorX = r.x + nameW + tagGap;
        float tagY = r.y + (r.height - tagH) / 2f;

        if (showDlc) {
            string tagText = (string)"CL_ModsConfig_Tag_Dlc".Translate();
            cursorX = DrawTagChip(cursorX, tagY, tagH, tagPad, tagText, ThemeSlot.SurfaceAccent, theme);
            cursorX += tagGap;
        }
        else if (showLib) {
            string tagText = (string)"CL_ModsConfig_Tag_Lib".Translate();
            cursorX = DrawTagChip(cursorX, tagY, tagH, tagPad, tagText, ThemeSlot.StatusSuccess, theme);
            cursorX += tagGap;
        }

        if (locked) {
            Font glyphFont = theme.GetFont(FontRole.Body);
            int glyphPx = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
            GUIStyle glyphStyle = GuiStyleCache.GetOrCreate(glyphFont, glyphPx);
            glyphStyle.alignment = TextAnchor.MiddleLeft;
            Color savedG = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            Rect glyphRect = new Rect(cursorX, r.y, new Rem(1.0f).ToPixels(), r.height);
            GUI.Label(RectSnap.Snap(glyphRect), "🔒", glyphStyle);
            GUI.color = savedG;
        }
    }

    private static float DrawTagChip(float x, float y, float h, float padX, string text, ThemeSlot toneSlot, Theme.Theme theme) {
        Font font = theme.GetFont(FontRole.Mono);
        int px = Mathf.RoundToInt(new Rem(0.7f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, px);
        style.alignment = TextAnchor.MiddleCenter;
        Vector2 size = style.CalcSize(new GUIContent(text));
        float w = size.x + padX * 2f;
        Rect chip = new Rect(x, y, w, h);
        PaintBox.Draw(chip, null, BorderSpec.All(new Rem(1f / 16f), toneSlot), null);
        Color saved = GUI.color;
        GUI.color = theme.GetColor(toneSlot);
        GUI.Label(RectSnap.Snap(chip), text, style);
        GUI.color = saved;
        return x + w;
    }

    private static void DrawAuthor(Rect r, ModMetaData mod, Theme.Theme theme) {
        string author = mod.AuthorsString ?? string.Empty;
        if (string.IsNullOrEmpty(author)) {
            return;
        }
        Font font = theme.GetFont(FontRole.Mono);
        int px = Mathf.RoundToInt(new Rem(0.85f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, px);
        style.alignment = TextAnchor.MiddleLeft;
        style.clipping = TextClipping.Clip;
        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextSecondary);
        GUI.Label(RectSnap.Snap(r), author, style);
        GUI.color = saved;
    }

    private static void DrawVersion(Rect r, ModMetaData mod, Theme.Theme theme) {
        string version = string.IsNullOrEmpty(mod.ModVersion) ? "—" : mod.ModVersion;
        Font font = theme.GetFont(FontRole.Mono);
        int px = Mathf.RoundToInt(new Rem(0.85f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, px);
        style.alignment = TextAnchor.MiddleLeft;
        style.clipping = TextClipping.Clip;
        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextMuted);
        GUI.Label(RectSnap.Snap(r), version, style);
        GUI.color = saved;
    }

    private static void DrawStatus(Rect r, ModMetaData mod, Theme.Theme theme) {
        string text;
        ThemeSlot slot;
        if (!mod.Active) {
            text = (string)"CL_ModsConfig_Row_Status_Disabled".Translate();
            slot = ThemeSlot.TextMuted;
        }
        else {
            int conflicts = ModConflicts.CountFor(mod);
            if (conflicts > 0) {
                text = "CL_ModsConfig_Row_Status_Conflict".Translate(conflicts.Named("COUNT")).Resolve();
                slot = ThemeSlot.StatusDanger;
            }
            else {
                text = (string)"CL_ModsConfig_Row_Status_Ok".Translate();
                slot = ThemeSlot.StatusSuccess;
            }
        }
        Font font = theme.GetFont(FontRole.Mono);
        int px = Mathf.RoundToInt(new Rem(0.85f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, px);
        style.alignment = TextAnchor.MiddleRight;
        Color saved = GUI.color;
        GUI.color = theme.GetColor(slot);
        GUI.Label(RectSnap.Snap(r), text, style);
        GUI.color = saved;
    }

    private static ColumnRects ComputeColumns(Rect rect) {
        float padX = PaddingX.ToPixels();
        float colOrderW = ColOrder.ToPixels();
        float colCheckW = ColCheck.ToPixels();
        float colAuthorW = ColAuthor.ToPixels();
        float colVersionW = ColVersion.ToPixels();
        float colStatusW = ColStatus.ToPixels();
        float gap = new Rem(0.5f).ToPixels();

        float x = rect.x + padX;
        Rect order = new Rect(x, rect.y, colOrderW, rect.height);
        x += colOrderW + gap;
        Rect check = new Rect(x, rect.y, colCheckW, rect.height);
        x += colCheckW + gap;

        float right = rect.xMax - padX;
        Rect status = new Rect(right - colStatusW, rect.y, colStatusW, rect.height);
        Rect version = new Rect(status.x - gap - colVersionW, rect.y, colVersionW, rect.height);
        Rect author = new Rect(version.x - gap - colAuthorW, rect.y, colAuthorW, rect.height);

        float nameW = Mathf.Max(0f, author.x - gap - x);
        Rect name = new Rect(x, rect.y, nameW, rect.height);

        return new ColumnRects(order, check, name, author, version, status);
    }

    private static LightweaveNode BuildEmptyState() {
        return Container.Create(
            child: Stack.Create(SpacingScale.Xs, s => {
                s.Add(Eyebrow.Create("CL_ModsConfig_Empty_Eyebrow".Translate()));
                s.Add(Text.Create(
                    "CL_ModsConfig_Empty_Body".Translate(),
                    wrap: true,
                    style: new Style { TextColor = ThemeSlot.TextSecondary }
                ));
            }),
            style: new Style {
                Padding = EdgeInsets.All(SpacingScale.Lg),
            }
        );
    }


    private static LightweaveNode BuildSearchEmptyState(string query) {
        return Container.Create(
            child: Stack.Create(SpacingScale.Xs, s => {
                s.Add(Eyebrow.Create("CL_ModsConfig_Search_EmptyEyebrow".Translate()));
                s.Add(Text.Create(
                    "CL_ModsConfig_Search_NoMatch".Translate(query.Named("QUERY")),
                    wrap: true,
                    style: new Style { TextColor = ThemeSlot.TextSecondary }
                ));
            }),
            style: new Style {
                Padding = EdgeInsets.All(SpacingScale.Lg),
            }
        );
    }

    private readonly struct ColumnRects {
        public readonly Rect Order;
        public readonly Rect Check;
        public readonly Rect Name;
        public readonly Rect Author;
        public readonly Rect Version;
        public readonly Rect Status;

        public ColumnRects(Rect order, Rect check, Rect name, Rect author, Rect version, Rect status) {
            Order = order;
            Check = check;
            Name = name;
            Author = author;
            Version = version;
            Status = status;
        }
    }
}
