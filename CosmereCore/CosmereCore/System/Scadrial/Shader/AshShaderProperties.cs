using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Shader;

/// <summary>Cached shader property ids, so nothing string-hashes inside a draw call.</summary>
[StaticConstructorOnStartup]
public static class AshShaderProperties {
    public static readonly int AshSeverity = UnityEngine.Shader.PropertyToID("_AshSeverity");
    public static readonly int AshColor = UnityEngine.Shader.PropertyToID("_AshColor");
    public static readonly int AshColorDeep = UnityEngine.Shader.PropertyToID("_AshColorDeep");
    public static readonly int Gates = UnityEngine.Shader.PropertyToID("_Gates");
    public static readonly int WindDir = UnityEngine.Shader.PropertyToID("_WindDir");
}
