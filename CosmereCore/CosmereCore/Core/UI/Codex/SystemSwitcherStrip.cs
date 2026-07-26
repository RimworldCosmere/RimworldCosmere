using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Codex;

public static class SystemSwitcherStrip {
    private const float OrbSize = 36f;
    private const float OrbGap = 8f;

    public static void Draw(Rect railRect, Pawn pawn, CodexState state, IReadOnlyList<IInvestitureProvider> providers) {
        if (providers.Count <= 1) return;

        // No ground of its own: the rail is part of the window, and painting it a
        // different shade made it look like a separate panel bolted on the side.
        Widgets.DrawBoxSolid(new Rect(railRect.xMax - 1f, railRect.y, 1f, railRect.height), DockPalette.BorderSubtle);

        float y = railRect.y + OrbGap;
        for (int i = 0; i < providers.Count; i++) {
            IInvestitureProvider provider = providers[i];
            ISystemSkin skin = SystemSkinRegistry.ForOrFallback(provider.SystemId);
            Rect orb = new Rect(railRect.x + (railRect.width - OrbSize) / 2f, y, OrbSize, OrbSize);

            // No box and no ring: the mark alone carries it, lit in the system's
            // own colour when chosen and muted otherwise.
            bool selected = i == state.SelectedSystemIndex;
            Color mark = selected ? skin.AccentColor : DockPalette.MutedText;

            Texture2D? sigil = skin.Sigil;
            if (sigil != null) {
                Color prev = GUI.color;
                GUI.color = mark;
                GUI.DrawTexture(orb.ContractedBy(3f), sigil);
                GUI.color = prev;
            }
            else {
                Rect dot = new Rect(orb.center.x - 4f, orb.center.y - 4f, 8f, 8f);
                Widgets.DrawBoxSolid(dot, mark);
            }

            string label = skin.HeaderLabel;
            string? custom = provider.Codex.HeaderLabelFor(pawn);
            if (!custom.NullOrEmpty()) label = custom!;
            TooltipHandler.TipRegion(orb, label);
            Widgets.DrawHighlightIfMouseover(orb);
            MouseoverSounds.DoRegion(orb);

            if (Widgets.ButtonInvisible(orb)) {
                state.SelectedSystemIndex = i;
                RimWorld.SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            }

            y += OrbSize + OrbGap;
        }
    }
}