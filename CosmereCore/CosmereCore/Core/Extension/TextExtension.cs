using UnityEngine;

namespace Cosmere.Core.Extension;

public static class TextExtension {
    public static string ToStringBreathEquivalentUnits(this float breathEquivalentUnits) {
        return Mathf.RoundToInt(breathEquivalentUnits).ToStringBreathEquivalentUnits();
    }

    public static string ToStringBreathEquivalentUnits(this int breathEquivalentUnits) {
        int degree = Need.Investiture.GetDegreeFromBreathEquivalentUnits(breathEquivalentUnits);
        string tier = Need.Investiture.HeighteningLabels[Mathf.Clamp(
            degree,
            0,
            Need.Investiture.HeighteningLabels.Length - 1
        )];

        return $"{tier} ({breathEquivalentUnits} BEUs)";
    }

    public static string ToStringBreathEquivalentUnitsRaw(this float breathEquivalentUnits) {
        return Mathf.RoundToInt(breathEquivalentUnits).ToStringBreathEquivalentUnitsRaw();
    }

    public static string ToStringBreathEquivalentUnitsRaw(this int breathEquivalentUnits) {
        return $"{breathEquivalentUnits} BEUs";
    }
}
