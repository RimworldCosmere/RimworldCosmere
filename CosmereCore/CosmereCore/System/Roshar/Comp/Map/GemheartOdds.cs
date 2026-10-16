namespace Cosmere.System.Roshar.Comp.Map;

public enum GemheartOutcome {
    Disaster,
    Failure,
    Pyrrhic,
    HardWon,
    Victory,
}

// pure so the dialog can show the player the same number the resolver will use
public static class GemheartOdds {
    public static float PawnPower(int melee, int shooting, int radiantIdeal, float weaponDps) {
        return melee * 3f + shooting * 2f + radiantIdeal * 25f + weaponDps * 2f;
    }

    public static float PartyPower(float pawnPowerSum, int pawnCount) {
        return pawnPowerSum + pawnCount * 5f;
    }

    public static float Difficulty(int daysPassed, float wealth) {
        return 100f + daysPassed * 0.5f + wealth / 10000f;
    }

    public static float Ratio(float power, float difficulty) {
        return difficulty > 0f ? power / difficulty : 2f;
    }

    public static GemheartOutcome Classify(float ratio) {
        if (ratio >= 1.5f) return GemheartOutcome.Victory;
        if (ratio >= 1.0f) return GemheartOutcome.HardWon;
        if (ratio >= 0.6f) return GemheartOutcome.Pyrrhic;
        if (ratio >= 0.3f) return GemheartOutcome.Failure;

        return GemheartOutcome.Disaster;
    }
}
