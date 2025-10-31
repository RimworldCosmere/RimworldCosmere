using Cosmere;
﻿using Cosmere.System.Roshar.LesserSpren.SprenController;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.ParticleSystem;

internal struct ParticleSystemConfig {
    public float particleSizeFactor;

    public FloatRange shapeRandomDirectionAmount;
    public int noiseOctaveCount;
    public float noiseFrequency;
    public float noisePositionAmount;
    public int noiseStrength;
}

[StaticConstructorOnStartup]
public static class Builder {
    private const float ParticleAlpha = 2.5f;

    private static readonly ParticleSystemConfig Config = new ParticleSystemConfig {
        particleSizeFactor = .5f,
        shapeRandomDirectionAmount = new FloatRange(0, 360),
        noiseOctaveCount = 2,
        noiseFrequency = 1f,
        noisePositionAmount = 0.5f,
        noiseStrength = 10,
    };


    /*
    public static UnityEngine.ParticleSystem CreateLesserSprenParticleSystem(int mapID) {
        return CreateLesserSprenParticleSystem(mapID, null, null);
    }*/

    public static UnityEngine.ParticleSystem CreateLesserSprenParticleSystem(
        int mapID,
        BaseSprenController controller,
        SprenSpawnInformation spawnInfo
    ) {
        GameObject spren = new GameObject($"lesser_spren_system_{Mathf.Abs(mapID)}");
        UnityEngine.ParticleSystem particleSys = spren.GetComponent<UnityEngine.ParticleSystem>() ??
                                                 spren.AddComponent<UnityEngine.ParticleSystem>();
        ParticleSystemRenderer renderer = spren.GetComponent<ParticleSystemRenderer>() ??
                                          spren.AddComponent<ParticleSystemRenderer>();

        ConfigureAlpha(renderer);
        ConfigureParticleSystem(particleSys, controller, spawnInfo);
        ConfigureShapeModule(particleSys, spawnInfo);
        ConfigureEmissionModule(particleSys);
        ConfigureNoiseModule(particleSys);
        ConfigureVelocityOverLifetimeModule(particleSys);
        ConfigureSizeOverLifetimeModule(particleSys, Config.particleSizeFactor);
        ConfigureColorOverLifetimeModule(particleSys);
        ConfigureRenderer(particleSys, controller);

        return particleSys;
    }

    private static void ConfigureAlpha(ParticleSystemRenderer renderer) {
        if (!renderer.material.HasProperty("_Color")) return;
        Color currentColor = renderer.material
            .GetColor(Shader.PropertyToID("_Color"));

        // Preserve the RGB values, only modify alpha
        Color newColor = new Color(
            currentColor.r,
            currentColor.g,
            currentColor.b,
            currentColor.a * ParticleAlpha
        );
        renderer.material
            .SetColor(Shader.PropertyToID("_Color"), newColor);
    }


    private static void ConfigureParticleSystem(
        UnityEngine.ParticleSystem particleSys,
        BaseSprenController controller,
        SprenSpawnInformation spawnInfo
    ) {
        UnityEngine.ParticleSystem.MainModule mainModule = particleSys.main;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        mainModule.startSize = 1f;
        mainModule.startLifetime =
            new UnityEngine.ParticleSystem.MinMaxCurve(controller.lifetime.min, controller.lifetime.max);

        mainModule.duration = controller.lifetime.RandomInRange;
        mainModule.loop = true;

        // Use spawn info movement speed if available, otherwise use default range
        float speed = spawnInfo?.movementSpeed ?? Random.Range(0.01f, 20f);
        mainModule.startSpeed = new UnityEngine.ParticleSystem.MinMaxCurve(1f, speed);
    }

    private static void ConfigureShapeModule(
        UnityEngine.ParticleSystem particleSys,
        SprenSpawnInformation spawnInfo
    ) {
        UnityEngine.ParticleSystem.ShapeModule shapeModule = particleSys.shape;
        shapeModule.enabled = true;
        shapeModule.shapeType = ParticleSystemShapeType.Mesh;
        shapeModule.meshShapeType = ParticleSystemMeshShapeType.Vertex;

        // Use spawn info random direction amount if available, otherwise use config default
        float randomDirection = spawnInfo?.randomDirectionAmount ?? Config.shapeRandomDirectionAmount.RandomInRange;
        shapeModule.randomDirectionAmount = randomDirection;
    }

    private static void ConfigureEmissionModule(UnityEngine.ParticleSystem particleSys) {
        UnityEngine.ParticleSystem.EmissionModule emissionModule = particleSys.emission;
        emissionModule.rateOverTime = 0f; // Disable automatic emission since we emit manually
        emissionModule.enabled = true;
    }

    private static void ConfigureNoiseModule(UnityEngine.ParticleSystem particleSys) {
        UnityEngine.ParticleSystem.NoiseModule noiseModule = particleSys.noise;
        noiseModule.enabled = true;
        noiseModule.quality = ParticleSystemNoiseQuality.High;
        noiseModule.octaveCount = Config.noiseOctaveCount;
        noiseModule.frequency = Config.noiseFrequency;
        noiseModule.positionAmount = Config.noisePositionAmount;
        noiseModule.strength = Config.noiseStrength;
        noiseModule.damping = true;
    }

    private static void ConfigureVelocityOverLifetimeModule(UnityEngine.ParticleSystem particleSys) {
        UnityEngine.ParticleSystem.VelocityOverLifetimeModule velocityModule = particleSys.velocityOverLifetime;
        velocityModule.enabled = true;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 1f);
        curve.AddKey(1.0f, 1f);

        velocityModule.speedModifier = new UnityEngine.ParticleSystem.MinMaxCurve(0.05f, curve);
    }

    private static void ConfigureSizeOverLifetimeModule(
        UnityEngine.ParticleSystem particleSys,
        float particleSizeFactor
    ) {
        UnityEngine.ParticleSystem.SizeOverLifetimeModule sizeModule = particleSys.sizeOverLifetime;
        sizeModule.enabled = true;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, .35f * particleSizeFactor);
        curve.AddKey(.5f, .45f * particleSizeFactor);
        curve.AddKey(1f, .35f * particleSizeFactor);

        sizeModule.size = new UnityEngine.ParticleSystem.MinMaxCurve(1.0f, curve);
    }

    private static void ConfigureColorOverLifetimeModule(UnityEngine.ParticleSystem particleSys) {
        UnityEngine.ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = particleSys.colorOverLifetime;
        colorOverLifetimeModule.enabled = true;
        Color color = particleSys.main.startColor.color;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            [
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f),
            ],
            [
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0f, 0.05f),
                new GradientAlphaKey(1f, 0.45f),
                new GradientAlphaKey(1f, 0.55f),
                new GradientAlphaKey(0f, 0.95f),
                new GradientAlphaKey(0f, 1f),
            ]
        );
        colorOverLifetimeModule.color = new UnityEngine.ParticleSystem.MinMaxGradient(gradient);
    }

    private static void ConfigureRenderer(UnityEngine.ParticleSystem particleSys, BaseSprenController controller) {
        ParticleSystemRenderer renderer = particleSys.GetComponent<ParticleSystemRenderer>();
        renderer.material = controller.sprenMaterial;
        renderer.material.SetColor(Shader.PropertyToID("_MainTex"), controller.sprenColor);
        renderer.material.SetTexture(Shader.PropertyToID("_MainTex"), controller.sprenTexture);
    }
}