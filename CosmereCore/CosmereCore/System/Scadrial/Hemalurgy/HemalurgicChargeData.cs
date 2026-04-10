using System;
using System.Collections.Generic;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

public class HemalurgicChargeData : IExposable {
    public HemalurgicStealType stealType;
    public string stolenDefName = "";
    public List<string> stolenDefNames = [];
    public float strength = 1f;
    public float storedInvestiture;
    public int chargedTick;

    public bool isValid => strength > HemalurgicConstants.MinChargeStrength
        && (stealType == HemalurgicStealType.RemoveAllPowers
            || !stolenDefName.NullOrEmpty()
            || stolenDefNames.Count > 0
            || HemalurgicConstants.IsHumanAttribute(stealType)
            || stealType == HemalurgicStealType.Investiture
            || stealType == HemalurgicStealType.ConnectionIdentity);

    public void ExposeData() {
        Scribe_Values.Look(ref stealType, "stealType");
        Scribe_Values.Look(ref stolenDefName, "stolenDefName", "");
        Scribe_Collections.Look(ref stolenDefNames, "stolenDefNames", LookMode.Value);
        Scribe_Values.Look(ref strength, "strength", 1f);
        Scribe_Values.Look(ref storedInvestiture, "storedInvestiture");
        Scribe_Values.Look(ref chargedTick, "chargedTick");
        stolenDefNames ??= [];
    }

    public float GetCurrentStrength(int currentTick, bool inAluminumCase) {
        if (inAluminumCase) return strength;
        if (chargedTick <= 0) return strength;
        int elapsed = currentTick - chargedTick;
        if (elapsed <= 0) return strength;
        float daysElapsed = elapsed / 60000f;
        float decayed = strength * (float)Math.Pow(0.5, daysElapsed / HemalurgicConstants.DecayHalfLifeDays);
        return decayed < HemalurgicConstants.MinChargeStrength ? 0f : decayed;
    }
}
