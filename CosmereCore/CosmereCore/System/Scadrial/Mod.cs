using Cosmere;
﻿using Cosmere;
using Cosmere.System.Scadrial.Settings;
using Verse;

namespace Cosmere.System.Scadrial;

public class Mod(ModContentPack content) : CosmereMod<ScadrialModSettings>(content) {
    public static MistsFrequency mistsFrequency => Settings.mistsFrequency;
}