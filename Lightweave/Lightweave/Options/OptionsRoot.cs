using System;
using System.Collections.Generic;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Options.Tabs;
using Cosmere.Lightweave.Overlay;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using RimWorld;
using UnityEngine;
using Verse;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;

namespace Cosmere.Lightweave.Options;

public static class OptionsRoot {
    public static LightweaveNode Build(Dialog_Options dialog, Action onClose) {
        Hooks.Hooks.StateHandle<OptionsTab> tab = Hooks.Hooks.UseState(OptionsTab.General);

        LightweaveNode card = Box.Create(
            children: c => c.Add(Stack.Create(SpacingScale.None, root => {
                root.Add(DialogHeader.Create(
                    title: "CL_Options_Title".Translate(),
                    trailingActionLabel: "CL_Options_ResetCategory".Translate(),
                    onTrailingAction: null,
                    onClose: () => {
                        Prefs.Save();
                        onClose?.Invoke();
                    },
                    drawDivider: true
                ));
                root.AddFlex(HStack.Create(SpacingScale.None, h => {
                    h.Add(BuildSidebar(tab), new Rem(14f).ToPixels());
                    h.AddFlex(BuildContent(tab.Value, dialog));
                }));
            }))
        );

        return Dialog.Create(
            content: () => card,
            cardBackground: BackgroundSpec.Blur(new Color(0f, 0f, 0f, 0.95f), 10f)
        );
    }

    private static LightweaveNode BuildSidebar(Hooks.Hooks.StateHandle<OptionsTab> tab) {
        return Box.Create(
            children: c => c.Add(Stack.Create(SpacingScale.None, s => {
                AppendTab(s, tab, OptionsTab.General, "CL_Options_Tab_General");
                AppendTab(s, tab, OptionsTab.Gameplay, "CL_Options_Tab_Gameplay");
                AppendTab(s, tab, OptionsTab.Graphics, "CL_Options_Tab_Graphics");
                AppendTab(s, tab, OptionsTab.Audio, "CL_Options_Tab_Audio");
                AppendTab(s, tab, OptionsTab.Controls, "CL_Options_Tab_Controls");
                AppendTab(s, tab, OptionsTab.Interface, "CL_Options_Tab_Interface");
                AppendTab(s, tab, OptionsTab.Developer, "CL_Options_Tab_Developer");
                AppendTab(s, tab, OptionsTab.ModSettings, "CL_Options_Tab_ModSettings");
            })),
            style: new Style {
                Padding = new EdgeInsets(Top: SpacingScale.None, Bottom: SpacingScale.Md, Left: SpacingScale.None, Right: SpacingScale.None),
                Border = new BorderSpec(Right: new Rem(1f / 16f), Color: ThemeSlot.BorderSubtle),
            }
        );
    }

    private static void AppendTab(
        StackBuilder s,
        Hooks.Hooks.StateHandle<OptionsTab> selection,
        OptionsTab tab,
        string labelKey
    ) {
        bool isActive = selection.Value == tab;
        string label = labelKey.Translate();
        LightweaveNode node = NodeBuilder.New("OptionsTab:" + tab);
        node.PreferredHeight = new Rem(2.8f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            InteractionState state = InteractionState.Resolve(rect, null, false);

            if (isActive) {
                Color warmBg = new Color(0.157f, 0.125f, 0.086f, 0.4f);
                PaintBox.Draw(rect, BackgroundSpec.Of(warmBg), null, null);
            }
            else if (state.Hovered) {
                Color hoverBg = new Color(0.157f, 0.125f, 0.086f, 0.2f);
                PaintBox.Draw(rect, BackgroundSpec.Of(hoverBg), null, null);
            }

            if (isActive) {
                float stripeW = new Rem(0.1875f).ToPixels();
                Rect stripe = new Rect(rect.x, rect.y, stripeW, rect.height);
                PaintBox.Draw(stripe, BackgroundSpec.Of(ThemeSlot.SurfaceAccent), null, null);
            }

            Font font = theme.GetFont(isActive ? FontRole.BodyBold : FontRole.Body);
            int px = Mathf.RoundToInt(new Rem(0.95f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Normal);
            style.alignment = TextAnchor.MiddleLeft;

            Color saved = GUI.color;
            GUI.color = theme.GetColor(isActive ? ThemeSlot.TextPrimary : ThemeSlot.TextSecondary);
            float padX = SpacingScale.Md.ToPixels();
            Rect labelRect = new Rect(rect.x + padX, rect.y, rect.width - padX * 2f, rect.height);
            GUI.Label(RectSnap.Snap(labelRect), label, style);
            GUI.color = saved;

            InteractionFeedback.Apply(rect, true, true);

            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                selection.Set(tab);
                e.Use();
            }
        };
        s.Add(node);
    }

    private static LightweaveNode BuildContent(OptionsTab tab, Dialog_Options dialog) {
        LightweaveNode body = tab switch {
            OptionsTab.General => GeneralTab.Build(),
            OptionsTab.Graphics => GraphicsTab.Build(),
            OptionsTab.Audio => AudioTab.Build(),
            OptionsTab.Gameplay => GameplayTab.Build(),
            OptionsTab.Interface => InterfaceTab.Build(),
            OptionsTab.Controls => ControlsTab.Build(),
            OptionsTab.Developer => DeveloperTab.Build(),
            OptionsTab.ModSettings => ModSettingsTab.Build(),
            _ => GeneralTab.Build(),
        };

        LightweaveNode paddedBody = Box.Create(
            children: c => c.Add(body),
            style: new Style {
                Padding = new EdgeInsets(Right: new Rem(0.75f)),
            }
        );

        return Box.Create(
            children: c => c.Add(ScrollArea.Create(content: paddedBody, showScrollbar: true)),
            style: new Style {
                Padding = EdgeInsets.All(SpacingScale.Lg),
            }
        );
    }
}
