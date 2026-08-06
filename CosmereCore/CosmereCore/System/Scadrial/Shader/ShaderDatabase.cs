using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Shader;

[StaticConstructorOnStartup]
public static class ShaderDatabase {
    public static readonly UnityEngine.Shader AshGround = Verse.ShaderDatabase.LoadShader("AshGround");
    public static readonly Material AshGroundMaterial = new Material(AshGround);
}
