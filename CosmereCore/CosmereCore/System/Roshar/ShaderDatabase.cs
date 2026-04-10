using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar;

[StaticConstructorOnStartup]
public static class ShaderDatabase {
    public static readonly Shader CrystallineFacet = Verse.ShaderDatabase.LoadShader("CrystallineFacet");
    public static readonly Material CrystallineFacetMaterial = new Material(CrystallineFacet);

    public static readonly Shader FlowingParticleStream = Verse.ShaderDatabase.LoadShader("FlowingParticleStream");
    public static readonly Material FlowingParticleStreamMaterial = new Material(FlowingParticleStream);
}