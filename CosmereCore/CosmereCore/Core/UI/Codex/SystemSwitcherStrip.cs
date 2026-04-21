using System.Collections.Generic;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public static class SystemSwitcherStrip {
    public static void Draw(Rect rect, Pawn pawn, IReadOnlyList<IInvestitureProvider> providers, CodexState state) {
        if (providers.Count <= 1) return;

        float pillWidth = Mathf.Min(140f, (rect.width - ((providers.Count - 1) * 6f)) / providers.Count);
        float x = rect.x;
        for (int i = 0; i < providers.Count; i++) {
            IInvestitureProvider provider = providers[i];
            ISystemSkin skin = SystemSkinRegistry.For(provider.SystemId);
            Rect pill = new Rect(x, rect.y + 2f, pillWidth, rect.height - 4f);

            bool selected = i == state.SelectedSystemIndex;
            Color bg = selected ? skin.AccentColor : new Color(0.12f, 0.12f, 0.14f, 0.85f);
            Widgets.DrawBoxSolid(pill, bg);
            Widgets.DrawBox(pill);

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, selected ? Color.black : Color.white))
                Widgets.Label(pill, skin.HeaderLabel);

            if (Widgets.ButtonInvisible(pill)) {
                state.SelectedSystemIndex = i;
            }

            x += pillWidth + 6f;
        }
    }
}
