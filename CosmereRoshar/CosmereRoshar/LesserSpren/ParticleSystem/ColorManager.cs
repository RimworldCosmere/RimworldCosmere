using UnityEngine;
using Logger = Cosmere.Foundation.Logger;

namespace Cosmere.Roshar.LesserSpren.ParticleSystem;

public static class ColorManager {
    public static void SetParticleAlpha(UnityEngine.ParticleSystem particleSys, float alphaFactor) {
        Renderer particleRenderer = particleSys.GetComponent<Renderer>();
        if (particleRenderer != null && particleRenderer.material.HasProperty("_Color")) {
            Color currentColor = particleRenderer.material
                .GetColor(Shader.PropertyToID("_Color"));

            // Preserve the RGB values, only modify alpha
            Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, currentColor.a * alphaFactor);
            particleRenderer.material
                .SetColor(Shader.PropertyToID("_Color"), newColor);
        } else {
            Logger.Warning("No _Color property found on particle material!");
        }
    }
}