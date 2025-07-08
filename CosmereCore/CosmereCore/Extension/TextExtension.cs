using CosmereCore.Need;
using UnityEngine;

namespace CosmereCore.Extension;

public static class TextExtension {
    public static string ToStringBreathEquivalentUnits(this float breathEquivalentUnits) {
        return Mathf.RoundToInt(breathEquivalentUnits).ToStringBreathEquivalentUnits();
    }

    public static string ToStringBreathEquivalentUnits(this int breathEquivalentUnits) {
        int degree = Investiture.GetDegreeFromBreathEquivalentUnits(breathEquivalentUnits);
        string tier = Investiture.HeighteningLabels[Mathf.Clamp(degree, 0, Investiture.HeighteningLabels.Length - 1)];

        return $"{tier} ({breathEquivalentUnits} BEUs)";
    }

    public static string ToStringBreathEquivalentUnitsRaw(this float breathEquivalentUnits) {
        return Mathf.RoundToInt(breathEquivalentUnits).ToStringBreathEquivalentUnitsRaw();
    }

    public static string ToStringBreathEquivalentUnitsRaw(this int breathEquivalentUnits) {
        return $"{breathEquivalentUnits} BEUs";
    }
}