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

    public override bool IsVisible {
        get {
            Pawn? pawn = SelPawn;
            if (pawn == null) return false;
            IReadOnlyList<IInvestitureProvider> all = InvestitureProviderRegistry.All;
            for (int i = 0; i < all.Count; i++) {
                if (all[i].IsInvested(pawn)) return true;
            }

            return false;
        }
    }

    protected override void FillTab() {
        Pawn? pawn = SelPawn;
        if (pawn == null) return;

        RefreshInvestedProviders(pawn);
        if (investedProviders.Count == 0) return;

        if (state.SelectedSystemIndex >= investedProviders.Count) {
            state.SelectedSystemIndex = 0;
        }

        Rect tabRect = new Rect(0f, 0f, size.x, size.y);
        IInvestitureProvider active = investedProviders[state.SelectedSystemIndex];
        ISystemSkin skin = SystemSkinRegistry.ForOrFallback(active.SystemId);

        Rect header = new Rect(0f, 0f, size.x, CodexChrome.HeaderHeight);
        CodexChrome.DrawHeader(header, ResolveHeaderLabel(pawn, active, skin), skin.AccentColor);

        float y = CodexChrome.HeaderHeight;
        bool hasSwitcher = investedProviders.Count > 1;
        if (hasSwitcher) {
            Rect switcher = new Rect(
                CodexChrome.Gutter,
                y,
                size.x - CodexChrome.Gutter * 2f,
                CodexChrome.SwitcherStripHeight
            );
            SystemSwitcherStrip.Draw(switcher, pawn, state, investedProviders);
            y += CodexChrome.SwitcherStripHeight;
        }

        Rect subtabBar = new Rect(CodexChrome.Gutter, y, size.x - CodexChrome.Gutter * 2f, CodexChrome.SubtabBarHeight);
        SubtabBar.Draw(subtabBar, pawn, state, active, skin.AccentColor);

        Rect divider = new Rect(CodexChrome.Gutter, subtabBar.yMax, subtabBar.width, 1f);
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