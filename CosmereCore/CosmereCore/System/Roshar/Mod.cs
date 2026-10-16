using Cosmere.Core;
using Cosmere.System.Roshar.Settings;
using Verse;

namespace Cosmere.System.Roshar;

public class Mod(ModContentPack content) : CosmereMod<RosharModSettings>(content) {
    public static bool EnableHighstorms => Settings.enableHighstorms;

    public static bool EnableHighstormPushing => Settings.enableHighstormPushing;

    public static bool EnablePawnGlow => Settings.enablePawnGlow;

    public static int HighstormMinIntervalDays => Settings.highstormMinIntervalDays;

    public static int HighstormMaxIntervalDays => Settings.highstormMaxIntervalDays;

    public static bool EnableWeeping => Settings.enableWeeping;

    public static int HighstormDurationTicks => Settings.highstormDurationTicks;
}
