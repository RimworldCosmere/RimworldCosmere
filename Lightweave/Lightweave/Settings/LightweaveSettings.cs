using Verse;

namespace Cosmere.Lightweave.Settings;

public class LightweaveSettings : ModSettings {
    public int FontScalePercent = 100;

    public float FontScale => FontScalePercent / 100f;

    public override void ExposeData() {
        Scribe_Values.Look(ref FontScalePercent, "fontScalePercent", 100);
        base.ExposeData();
    }
}
