using Cosmere.Framework;
using Cosmere.Resources.Settings;
using Verse;

namespace Cosmere.Resources;

public class Mod(ModContentPack content) : CosmereMod<ResourcesModSettings>(content, "Resources");