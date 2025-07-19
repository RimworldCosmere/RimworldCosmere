using Verse;

namespace CosmereRoshar.PawnFlyer.Worker;

public class LashUp(PawnFlyerProperties props) : PawnFlyerWorker(props) {
    private const float MaxHeight = 20f;

    // Then override these two methods:
    public override float AdjustedProgress(float baseProgress) {
        // E.g. keep it simple or implement acceleration logic
        return baseProgress;
    }

    public override float GetHeight(float progress) {
        // E.g. a simple parabola from 0..20..0
        float p = 4f * progress * (1f - progress); // peak at p=0.5
        return MaxHeight * p;
    }
}