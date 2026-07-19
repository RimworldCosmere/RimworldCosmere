using Cosmere.Core;
using Cosmere.System.Nalthis.Settings;
using Verse;

namespace Cosmere.System.Nalthis;

public class Mod(ModContentPack content) : CosmereMod<NalthisModSettings>(content) {
    public static bool enableAwakening => Settings.enableAwakening;
}
