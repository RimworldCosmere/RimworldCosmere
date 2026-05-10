using Verse;

namespace Cosmere.Lightweave.Settings;

public class LightweaveSettings : ModSettings {
    public int FontScalePercent = 100;
    public bool RedesignMainMenu = true;
    public bool ReduceMotion;
    public bool ParseSaveMetadata = true;
    public bool TranslationToastDismissed;
    public bool DevBuildToastDismissed;

    public float FontScale => FontScalePercent / 100f;

    public override void ExposeData() {
        Scribe_Values.Look(ref FontScalePercent, "fontScalePercent", 100);
        Scribe_Values.Look(ref RedesignMainMenu, "redesignMainMenu", true);
        Scribe_Values.Look(ref ReduceMotion, "reduceMotion");
        Scribe_Values.Look(ref ParseSaveMetadata, "parseSaveMetadata", true);
        Scribe_Values.Look(ref TranslationToastDismissed, "translationToastDismissed");
        Scribe_Values.Look(ref DevBuildToastDismissed, "devBuildToastDismissed");
        base.ExposeData();
    }
}
