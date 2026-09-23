using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Shader;

[StaticConstructorOnStartup]
public static class ShaderDatabase {
    public static readonly UnityEngine.Shader CrystallineFacet = Verse.ShaderDatabase.LoadShader("CrystallineFacet");
    public static readonly Material CrystallineFacetMaterial = new Material(CrystallineFacet);

    public static readonly UnityEngine.Shader FlowingParticleStream = Verse.ShaderDatabase.LoadShader("FlowingParticleStream");
    public static readonly Material FlowingParticleStreamMaterial = new Material(FlowingParticleStream);
}
