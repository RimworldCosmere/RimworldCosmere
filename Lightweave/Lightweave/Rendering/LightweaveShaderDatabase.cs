using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Rendering;

[StaticConstructorOnStartup]
public static class LightweaveShaderDatabase {
    public static readonly Shader Blur = Verse.ShaderDatabase.LoadShader("Blur");
    public static readonly Material BlurMaterial = new Material(Blur);
}
