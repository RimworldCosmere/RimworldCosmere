using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Tab;

public class ITab_Investiture : ITab {
    private readonly List<IInvestitureProvider> investedProviders = [];
    private readonly CodexState state = new CodexState();

    public ITab_Investiture() {
        labelKey = "CC_Codex_Tab";
        size = new Vector2(560f, 520f);
    }

    /// <summary>
    ///     Open for any colonist or prisoner, Invested or not. Connection is chrome above the
    ///     system switcher and everyone has one - a pawn with none at all is exactly who the
    ///     reading is most worth having for.
    /// </summary>
    public override bool IsVisible {
        get {
            Pawn? pawn = SelPawn;
            if (pawn == null) return false;
            if (!pawn.RaceProps.Humanlike) return false;

            return pawn.IsColonist || pawn.IsSlaveOfColony || pawn.IsPrisonerOfColony;
        }
    }

    protected override void FillTab() {
        Pawn? pawn = SelPawn;
        if (pawn == null) return;

        RefreshInvestedProviders(pawn);
        if (investedProviders.Count == 0) {
            state.ShowingConnection = true;
            DrawConnectionOnly(pawn);
            return;
        }

        if (state.SelectedSystemIndex >= investedProviders.Count) {
            state.SelectedSystemIndex = 0;
        }

        Rect tabRect = new Rect(0f, 0f, size.x, size.y);
        IInvestitureProvider active = investedProviders[state.SelectedSystemIndex];
        ISystemSkin skin = SystemSkinRegistry.ForOrFallback(active.SystemId);

        Rect header = new Rect(0f, 0f, size.x, CodexChrome.HeaderHeight);
        string headerLabel = state.ShowingConnection
            ? "CC_Codex_Subtab_Connection".Translate().Resolve()
            : ResolveHeaderLabel(pawn, active, skin);
        Color headerAccent = state.ShowingConnection ? ConnectionPalette.Selected : skin.AccentColor;
        CodexChrome.DrawHeader(header, headerLabel, headerAccent);

        // Connection always has a slot, so the rail is shown whenever any system exists.
        bool hasSwitcher = investedProviders.Count > 0;
        float contentX = hasSwitcher ? CodexChrome.RailWidth : 0f;

        if (hasSwitcher) {
            Rect rail = new Rect(0f, CodexChrome.HeaderHeight, CodexChrome.RailWidth, size.y - CodexChrome.HeaderHeight);
            SystemSwitcherStrip.Draw(rail, pawn, state, investedProviders);
        }

        if (state.ShowingConnection) {
            Rect connectionDivider = new Rect(contentX, CodexChrome.HeaderHeight, size.x - contentX, 1f);
            CodexChrome.DrawDivider(connectionDivider, headerAccent);

            ConnectionSubtab.Draw(
                new Rect(
                    contentX + CodexChrome.Gutter,
                    connectionDivider.yMax + CodexChrome.Gutter,
                    size.x - contentX - CodexChrome.Gutter * 2f,
                    size.y - connectionDivider.yMax - CodexChrome.Gutter * 2f
                ),
                pawn,
                state
            );
            return;
        }

        // Runs to both frame edges: a gap before the first tab and after the last
        // made the bar look inset from the window it belongs to.
        Rect subtabBar = new Rect(
            contentX,
            CodexChrome.HeaderHeight,
            size.x - contentX,
            CodexChrome.SubtabBarHeight
        );
        SubtabBar.Draw(subtabBar, pawn, state, active, skin.AccentColor);

        Rect divider = new Rect(subtabBar.x, subtabBar.yMax, subtabBar.width, 1f);
        CodexChrome.DrawDivider(divider, skin.AccentColor);

        Rect body = CodexChrome.BodyRect(tabRect, hasSwitcher);
        DrawSubtabBody(body, pawn, active);
    }

    private void DrawSubtabBody(Rect rect, Pawn pawn, IInvestitureProvider active) {
        if (state.Subtab == CodexSubtab.Autocast) {
            AutocastSubtabRenderer.Draw(rect, pawn, active);
            return;
        }

        CodexSubtabRenderer.Draw(rect, pawn, state, active, state.Subtab);
    }

    /// <summary>
    ///     The whole tab for a pawn with no Investiture at all: header, and Connection.
    /// </summary>
    /// <remarks>
    ///     No system switcher and no subtab bar, because there is exactly one thing to look at
    ///     and a one-tab bar is furniture. The fallback skin supplies the accent - Connection
    ///     belongs to no shardworld, so borrowing one world's colour here would be a lie.
    /// </remarks>
    private void DrawConnectionOnly(Pawn pawn) {
        ISystemSkin skin = SystemSkinRegistry.ForOrFallback(string.Empty);

        Rect header = new Rect(0f, 0f, size.x, CodexChrome.HeaderHeight);
        CodexChrome.DrawHeader(header, "CC_Codex_Subtab_Connection".Translate(), skin.AccentColor);

        Rect divider = new Rect(0f, header.yMax, size.x, 1f);
        CodexChrome.DrawDivider(divider, skin.AccentColor);

        Rect body = new Rect(
            CodexChrome.Gutter,
            divider.yMax + CodexChrome.Gutter,
            size.x - CodexChrome.Gutter * 2f,
            size.y - divider.yMax - CodexChrome.Gutter * 2f
        );
        ConnectionSubtab.Draw(body, pawn, state);
    }

    private static string ResolveHeaderLabel(Pawn pawn, IInvestitureProvider provider, ISystemSkin skin) {
        string? custom = provider.Codex.HeaderLabelFor(pawn);
        if (!custom.NullOrEmpty()) return custom!;

        return skin.HeaderLabel;
    }

    private void RefreshInvestedProviders(Pawn pawn) {
        investedProviders.Clear();
        IReadOnlyList<IInvestitureProvider> all = InvestitureProviderRegistry.All;
        for (int i = 0; i < all.Count; i++) {
            if (all[i].IsInvested(pawn)) investedProviders.Add(all[i]);
        }
    }
}
