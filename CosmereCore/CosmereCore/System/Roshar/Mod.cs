using Cosmere.Core;
using Cosmere.System.Roshar.Settings;
using Verse;

namespace Cosmere.System.Roshar;

public class Mod(ModContentPack content) : CosmereMod<RosharModSettings>(content) {
    public static bool enableHighstorms => Settings.enableHighstorms;

    public static bool enableHighstormPushing => Settings.enableHighstormPushing;

    public static bool enablePawnGlow => Settings.enablePawnGlow;

    public static int highstormMinIntervalDays => Settings.highstormMinIntervalDays;

    public static int highstormMaxIntervalDays => Settings.highstormMaxIntervalDays;

    public static bool enableWeeping => Settings.enableWeeping;

    public static int highstormDurationTicks => Settings.highstormDurationTicks;
}