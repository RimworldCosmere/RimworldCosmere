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
        // One entry is furniture. Connection always takes a slot, so a pawn with no Investiture
        // would otherwise get a rail with a single orb and nothing to switch to.
        if (providers.Count == 0) return;

        // No ground of its own: the rail is part of the window, and painting it a
        // different shade made it look like a separate panel bolted on the side.
        Widgets.DrawBoxSolid(new Rect(railRect.xMax - 1f, railRect.y, 1f, railRect.height), DockPalette.BorderSubtle);

        float y = railRect.y + OrbGap;

        // Connection first, and a peer of the systems rather than a tab inside one of them. It
        // is not an investiture system - it is what decides whether you may ever have one.
        y = DrawConnectionOrb(railRect, state, y);

        for (int i = 0; i < providers.Count; i++) {
            IInvestitureProvider provider = providers[i];
            ISystemSkin skin = SystemSkinRegistry.ForOrFallback(provider.SystemId);
            Rect orb = new Rect(railRect.x + (railRect.width - OrbSize) / 2f, y, OrbSize, OrbSize);

            // No box and no ring: the mark alone carries it, lit when chosen and muted otherwise.
            bool selected = !state.ShowingConnection && i == state.SelectedSystemIndex;

            // A per-pawn mark wins over the system sigil, and brings its own colours with it - an
            // order glyph is already painted in the order's colour, so filling it with an accent
            // would multiply the two and muddy it. Tint only the flat system sigils.
            Texture2D? sigil = skin.Sigil;
            Color accent = skin.AccentColor;
            bool selfColoured = false;
            if (provider.Codex is ICodexSystemMark perPawn) {
                Texture2D? pawnSigil = perPawn.SigilFor(pawn, selected);
                if (pawnSigil != null) {
                    sigil = pawnSigil;
                    selfColoured = true;
                }

                accent = perPawn.AccentFor(pawn) ?? accent;
            }

            Color mark = selected ? accent : DockPalette.MutedText;

            if (sigil != null) {
                Color prev = GUI.color;

                // A self-coloured mark already arrives in the right colours for its state, so it is
                // drawn as-is; only the flat system sigils get filled with the accent.
                GUI.color = selfColoured ? Color.white : mark;
                GUI.DrawTexture(orb.ContractedBy(3f), sigil);
                GUI.color = prev;
            } else {
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
                state.ShowingConnection = false;
                RimWorld.SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            }

            y += OrbSize + OrbGap;
        }
    }

    /// <summary>
    ///     The Connection orb. No sigil art exists for it yet, so it draws as a ring - which
    ///     reads well enough as "a tie to something" and is honest about being unfinished.
    /// </summary>
    private static float DrawConnectionOrb(Rect railRect, CodexState state, float y) {
        Rect orb = new Rect(railRect.x + (railRect.width - OrbSize) / 2f, y, OrbSize, OrbSize);
        bool selected = state.ShowingConnection;
        Color mark = selected ? ConnectionPalette.Selected : DockPalette.MutedText;

        // A ring: outer square in the mark colour, inner square punched back out to the panel.
        Widgets.DrawBoxSolid(new Rect(orb.center.x - 9f, orb.center.y - 9f, 18f, 18f), mark);
        Widgets.DrawBoxSolid(new Rect(orb.center.x - 5f, orb.center.y - 5f, 10f, 10f), DockPalette.Panel);

        TooltipHandler.TipRegion(orb, "CC_Codex_Subtab_Connection".Translate());
        Widgets.DrawHighlightIfMouseover(orb);
        MouseoverSounds.DoRegion(orb);

        if (Widgets.ButtonInvisible(orb)) {
            state.ShowingConnection = true;
            RimWorld.SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        }

        return y + OrbSize + OrbGap;
    }
}
