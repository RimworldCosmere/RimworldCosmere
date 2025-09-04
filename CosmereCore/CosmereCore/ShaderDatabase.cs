using UnityEngine;
using Verse;

namespace Cosmere.Core;

[StaticConstructorOnStartup]
public static class ShaderDatabase {
    public static readonly UnityEngine.Shader CirclePulse = Verse.ShaderDatabase.LoadShader("CirclePulse");
    public static readonly Material CirclePulseMaterial = new Material(CirclePulse);

    public static readonly UnityEngine.Shader CutoutLUT = Verse.ShaderDatabase.LoadShader("CutoutLUT");
    public static readonly Material CutoutLUTMaterial = new Material(CutoutLUT);
}