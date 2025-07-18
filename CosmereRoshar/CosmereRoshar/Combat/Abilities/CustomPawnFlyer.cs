using Verse;

namespace CosmereRoshar;

public class PawnFlyerWorker_LashUp : PawnFlyerWorker {
    public static float maxHeight = 20f;

    public PawnFlyerWorker_LashUp(PawnFlyerProperties props) : base(props) { }

    // Then override these two methods:
    public override float AdjustedProgress(float baseProgress) {
        // E.g. keep it simple or implement acceleration logic
        return baseProgress;
    }

    public override float GetHeight(float progress) {
        // E.g. a simple parabola from 0..20..0
        float p = 4f * progress * (1f - progress); // peak at p=0.5
        return maxHeight * p;
    }
}

public class PawnFlyerWorker_WindRunnerFlight : PawnFlyerWorker {
    public PawnFlyerWorker_WindRunnerFlight(PawnFlyerProperties props) : base(props) { }

    // Then override these two methods:
    public override float AdjustedProgress(float baseProgress) {
        // E.g. keep it simple or implement acceleration logic
        return baseProgress;
    }

    public override float GetHeight(float progress) {
        // E.g. a simple parabola from 0..20..0
        float maxHeight = 5f;
        float p = 4f * progress * (1f - progress); // peak at p=0.5
        return maxHeight * p;
    }
}