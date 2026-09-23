using UnityEngine;
using Verse;

namespace Cosmere.Core.Shader;

[StaticConstructorOnStartup]
public static class ShaderDatabase {
    public static readonly UnityEngine.Shader CirclePulse = Verse.ShaderDatabase.LoadShader("CirclePulse");
    public static readonly Material CirclePulseMaterial = new Material(CirclePulse);

    public static readonly UnityEngine.Shader CutoutAdvanced = Verse.ShaderDatabase.LoadShader("CutoutAdvanced");
    public static readonly Material CutoutAdvancedMaterial = new Material(CutoutAdvanced);
}
