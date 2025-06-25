using Verse;

namespace CosmereScadrial.Util;

public static class MoteUtility {
    public static float GetMoteSize(ThingDef moteDef, float radius, float multiplier) {
        float desiredRadius = radius * multiplier; // includes flare doubling if needed
        float drawSize = moteDef.graphicData.drawSize.magnitude; // assume square; use magnitude if not
        float scale = desiredRadius * 2f * drawSize; // times 2 since radius = diameter / 2

        return scale + multiplier * drawSize;
    }
}