using Cosmere.Foundation;
using Cosmere.Scadrial.Settings;
using Verse;

namespace Cosmere.Scadrial;

public class Mod(ModContentPack content) : CosmereMod<ScadrialModSettings>(content) {
    public static MistsFrequency mistsFrequency => Settings.mistsFrequency;
}