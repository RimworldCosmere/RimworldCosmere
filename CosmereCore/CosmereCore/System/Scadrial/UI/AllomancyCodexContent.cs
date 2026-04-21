using System.Collections.Generic;
using Cosmere.Core.Ability;
using Cosmere.Core.UI.Codex;
using Cosmere.System.Scadrial.Allomancy.Ability;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class AllomancyCodexContent : ICodexContentProvider {
    public bool HasProgression(Pawn pawn) => CollectAbilities(pawn).Count > 0;
    public bool HasBonded(Pawn pawn) => false;
    public bool HasMemories(Pawn pawn) => false;

    public void DrawProgression(Pawn pawn, Rect rect) {
        List<AllomancyAbility> abilities = CollectAbilities(pawn);
        if (abilities.Count == 0) return;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "Allomantic flare mastery");

        float y = rect.y + 34f;
        for (int i = 0; i < abilities.Count; i++) {
            AllomancyAbility a = abilities[i];
            Rect row = new Rect(rect.x, y, rect.width, 24f);
            if (i % 2 == 0) Widgets.DrawBoxSolid(row, new Color(1f, 1f, 1f, 0.03f));

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white))
                Widgets.Label(new Rect(row.x + 4f, row.y, 180f, row.height), a.metal.LabelCap);

            Status s = a.status;
            string statusText;
            Color statusColor;
            if (s.isPoweredUp) {
                statusText = "Flaring";
                statusColor = new Color(1f, 0.4f, 0.3f);
            } else if (s.isActive) {
                statusText = "Burning";
                statusColor = new Color(1f, 0.8f, 0.3f);
            } else {
                statusText = "Off";
                statusColor = new Color(0.6f, 0.6f, 0.6f);
            }

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, statusColor))
                Widgets.Label(new Rect(row.x + 190f, row.y, 120f, row.height), statusText);

            int flareSeconds = (int)(a.flareDuration / 60f);
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.75f, 0.75f, 0.75f)))
                Widgets.Label(new Rect(row.x + 320f, row.y, row.width - 320f, row.height), $"flared {flareSeconds}s this session");

            y += 26f;
        }
    }

    public void DrawBonded(Pawn pawn, Rect rect) { }
    public void DrawMemories(Pawn pawn, Rect rect) { }

    private static List<AllomancyAbility> CollectAbilities(Pawn pawn) {
        List<AllomancyAbility> result = [];
        if (pawn.abilities == null) return result;
        List<RimWorld.Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is AllomancyAbility a) result.Add(a);
        }
        return result;
    }
}
