using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Settings;

[StaticConstructorOnStartup]
public static class SettingsTextures {
    public static readonly Texture2D CoreSigil = ContentFinder<Texture2D>.Get("UI/Icons/Cosmere");
}
