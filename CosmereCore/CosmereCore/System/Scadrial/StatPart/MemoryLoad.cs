using Cosmere.Core;
using Cosmere.System.Scadrial.Extension;
using Cosmere.System.Scadrial.Feruchemy.Memory;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.StatPart;

public class MemoryLoad : RimWorld.StatPart {
    private const float OffsetPerMemory = 0.005f;
    private const float MaxOffset = 0.10f;

    public override void TransformValue(StatRequest req, ref float val) {
        if (!TryGetOffset(req, out float offset, out int _)) return;
        val += offset;
    }

    public override string? ExplanationPart(StatRequest req) {
        if (!TryGetOffset(req, out float offset, out int count)) return null;
        return "CS_StatsReport_FeruchemyMemoryLoad".Translate(
            count.Named("COUNT"),
            offset.ToStringPercent().Named("OFFSET")
        );
    }

    private static bool TryGetOffset(StatRequest req, out float offset, out int count) {
        offset = 0f;
        count = 0;
        if (!req.HasThing || req.Thing is not Pawn pawn) return false;
        if (pawn.genes == null) return false;
        if (!pawn.IsFerring(MetalDefOf.Copper)) return false;

        count = CountActiveMemories(pawn);
        if (count <= 0) return false;

        offset = Mathf.Min(MaxOffset, count * OffsetPerMemory);
        return true;
    }

    private static int CountActiveMemories(Pawn pawn) {
        MemoryThoughtHandler? handler = pawn.needs?.mood?.thoughts?.memories;
        if (handler == null) return 0;

        List<Thought_Memory> list = handler.Memories;
        int count = 0;
        for (int i = 0; i < list.Count; i++) {
            if (list[i] is Thought_Memory_Coppermind) continue;
            if (list[i].MoodOffset() == 0f) continue;
            count++;
        }
        return count;
    }
}
