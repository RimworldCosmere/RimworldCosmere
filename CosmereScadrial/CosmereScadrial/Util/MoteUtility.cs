using Verse;

namespace CosmereScadrial.Util;

public static class MoteUtility {
    public static float GetMoteSize(ThingDef moteDef, float radius, float multiplier) {
        return radius * multiplier * 2f * moteDef.graphicData.drawSize.normalized.magnitude;
    }
}