using Concord;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Patch;

[Patch(typeof(CharacterCardUtility))]
public static class CharacterCardUtilitySizePatch {
    [Inject(At.Return, nameof(CharacterCardUtility.PawnCardSize))]
    private static void AfterPawnCardSize(Pawn pawn, ControlHandle<Vector2> ch) {
        ch.ReturnValue = GetViewSize(pawn, ch.ReturnValue);
    }

    private static Vector2 GetViewSize(Pawn p, Vector2 result) {
        const float rowHeight = 27f;

        if (p.skills?.skills?.Count == null) return result;

        int hiddenCount = SkillVisibilityPatch.GetHiddenSkillCount(p);
        int visibleExtra = p.skills.skills.Count - 12 - hiddenCount;

        result.y += visibleExtra * rowHeight;

        return result;
    }
}
