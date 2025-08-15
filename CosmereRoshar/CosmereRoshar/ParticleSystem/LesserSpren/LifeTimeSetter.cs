using UnityEngine;

namespace Cosmere.Roshar.ParticleSystem.LesserSpren;

public static class LifeTimeSetter {
    public static AnimationCurve GetMinLifetimeCurve() {
        AnimationCurve minCurve = new AnimationCurve();
        minCurve.AddKey(0.75f, 1.75f);
        minCurve.AddKey(0.90f, 2.25f);
        minCurve.AddKey(1f, 2.25f);

        return minCurve;
    }

    public static AnimationCurve GetMaxLifetimeCurve() {
        AnimationCurve minCurve = new AnimationCurve();
        minCurve.AddKey(0.75f, 12.25f);
        minCurve.AddKey(0.90f, 12.65f);
        minCurve.AddKey(1f, 13.2f);

        return minCurve;
    }
}