using UnityEngine;
using Verse;
using Logger = Cosmere.Foundation.Logger;

namespace Cosmere.Roshar.ParticleSystem.LesserSpren;

public static class ColorManager {
    public static readonly Color GreenEmission = new Color(0.4f, 1.0f, 0.4f);
    public static readonly Color YellowEmission = new Color(1.0f, 1.0f, 0.4f);
    public static readonly Color OrangeEmission = new Color(1.0f, 0.6f, 0.2f);
    public static readonly Color RedEmission = new Color(1.0f, 0.4f, 0.4f);
    public static readonly Color BlueEmission = new Color(0.6f, 0.8f, 1.0f);
    public static readonly Color PurpleEmission = new Color(0.8f, 0.6f, 1.0f);

    private static readonly Gradient ColorGradient = new Gradient {
        colorKeys = [
            new GradientColorKey(GreenEmission, 0.5f),
            new GradientColorKey(YellowEmission, 0.8f),
            new GradientColorKey(OrangeEmission, 0.9f),
            new GradientColorKey(RedEmission, 0.95f),
            new GradientColorKey(BlueEmission, 0.975f),
            new GradientColorKey(PurpleEmission, 1f),
        ],
    };

    public static void GetBaseColorGradient(UnityEngine.ParticleSystem particleSys) {
        UnityEngine.ParticleSystem.MainModule mainModule = particleSys.main;
        UnityEngine.ParticleSystem.MinMaxGradient gradientColor =
            new UnityEngine.ParticleSystem.MinMaxGradient(ColorGradient);
        mainModule.startColor = gradientColor;
    }

    public static void SetParticleAlpha(UnityEngine.ParticleSystem particleSys, float alphaFactor) {
        Renderer particleRenderer = particleSys.GetComponent<Renderer>();
        if (particleRenderer != null && particleRenderer.material.HasProperty("_Color")) {
            Color currentColor = particleRenderer.material
                .GetColor(Shader.PropertyToID("_Color"));
            currentColor.a *= alphaFactor;
            particleRenderer.material
                .SetColor(Shader.PropertyToID("_Color"), currentColor);
        } else {
            Logger.Warning("No _Color property found on particle material!");
        }
    }

    public static Color RandomWeightedColor() {
        (Color color, float weight)[] weightedColors = [
            (GreenEmission, 3f),
            (YellowEmission, 3f),
            (OrangeEmission, 2f),
            (RedEmission, 1.5f),
            (BlueEmission, 1f),
            (PurpleEmission, 0.5f),
        ];

        float totalWeight = weightedColors.Sum(pair => pair.weight);
        float random = Rand.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach ((Color color, float weight) pair in weightedColors) {
            cumulative += pair.weight;
            if (random <= cumulative) {
                return pair.color;
            }
        }

        return YellowEmission;
    }

    private static bool Approx(Color a, Color b, float tolerance = 0.01f) {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    public static string GetColorName(Color color) {
        return color switch {
            _ when Approx(color, GreenEmission) => "Green",
            _ when Approx(color, YellowEmission) => "Yellow",
            _ when Approx(color, OrangeEmission) => "Orange",
            _ when Approx(color, RedEmission) => "Red",
            _ when Approx(color, BlueEmission) => "Blue",
            _ when Approx(color, PurpleEmission) => "Purple",
            _ => "Unknown",
        };
    }
}