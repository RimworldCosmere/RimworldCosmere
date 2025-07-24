using Cosmere.Framework;
using Cosmere.Scadrial.Settings;
using Verse;

namespace Cosmere.Scadrial;

public class Mod(ModContentPack content) : CosmereMod<ScadrialModSettings>(content, "Scadrial") {
    public static MistsFrequency mistsFrequency => Settings.mistsFrequency;
}