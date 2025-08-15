using UnityEngine;
using Verse;

namespace Cosmere.Roshar.ParticleSystem.LesserSpren;

internal struct ParticleSystemConfig {
    public float particleSizeFactor;
    public float sunGlowThreshold;

    public FloatRange shapeRandomDirectionAmount;
    public int noiseOctaveCount;
    public float noiseFrequency;
    public float noisePositionAmount;
    public int noiseStrength;
}

[StaticConstructorOnStartup]
public static class Builder {
    private static readonly ParticleSystemConfig Config = new ParticleSystemConfig {
        particleSizeFactor = 0.25f, // Reduced by half from 0.5f
        sunGlowThreshold = 0.3f,
        shapeRandomDirectionAmount = new FloatRange(0, 360),
        noiseOctaveCount = 2,
        noiseFrequency = 1.5f,
        noisePositionAmount = 0.5f,
        noiseStrength = 15,
    };

    private static readonly Texture2D LesserSpren = ContentFinder<Texture2D>.Get("Things/Pawn/Animal/LesserSpren");

    public static UnityEngine.ParticleSystem CreateLesserSprenParticleSystem(int mapID) {
        GameObject fireflies = new GameObject($"firefly_system_{Mathf.Abs(mapID)}");
        UnityEngine.ParticleSystem particleSys = fireflies.GetComponent<UnityEngine.ParticleSystem>() ??
                                                 fireflies.AddComponent<UnityEngine.ParticleSystem>();
        ParticleSystemRenderer renderer = fireflies.GetComponent<ParticleSystemRenderer>() ??
                                          fireflies.AddComponent<ParticleSystemRenderer>();

        ConfigureParticleSystem(particleSys);
        ConfigureShapeModule(particleSys);
        ConfigureEmissionModule(particleSys);
        ConfigureNoiseModule(particleSys);
        ConfigureVelocityOverLifetimeModule(particleSys);
        ConfigureSizeOverLifetimeModule(particleSys, Config.particleSizeFactor);
        ConfigureColorOverLifetimeModule(particleSys);
        Material material = new Material(ShaderDatabase.TransparentPostLight);
        // ConfigureTrailModule(particleSys, material);
        ConfigureRenderer(particleSys, material, LesserSpren);

        return particleSys;
    }

    private static void ConfigureParticleSystem(UnityEngine.ParticleSystem particleSys) {
        UnityEngine.ParticleSystem.MainModule mainModule = particleSys.main;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        mainModule.loop = true;
        mainModule.duration = Rand.Value;
        mainModule.startSize = 1f;
        mainModule.startLifetime = new UnityEngine.ParticleSystem.MinMaxCurve(
            1,
            LifeTimeSetter.GetMinLifetimeCurve(),
            LifeTimeSetter.GetMaxLifetimeCurve()
        );
        mainModule.startSpeed = new UnityEngine.ParticleSystem.MinMaxCurve(1f, Random.Range(0.01f, 20f));
    }

    private static void ConfigureShapeModule(UnityEngine.ParticleSystem particleSys) {
        UnityEngine.ParticleSystem.ShapeModule shapeModule = particleSys.shape;
        shapeModule.enabled = true;
        shapeModule.shapeType = ParticleSystemShapeType.Mesh;
        shapeModule.meshShapeType = ParticleSystemMeshShapeType.Vertex;
        shapeModule.randomDirectionAmount = Config.shapeRandomDirectionAmount.RandomInRange;
    }

    private static void ConfigureEmissionModule(UnityEngine.ParticleSystem particleSys) {
        UnityEngine.ParticleSystem.EmissionModule emissionModule = particleSys.emission;
        emissionModule.rateOverTime = emissionModule.rateOverTime
            with {
                mode = ParticleSystemCurveMode.Constant,
            };
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
        curve.AddKey(0f, 1f * particleSizeFactor);
        curve.AddKey(1f, 1f * particleSizeFactor);

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

    private static void ConfigureRenderer(
        UnityEngine.ParticleSystem particleSys,
        Material material,
        Texture2D fireflyTexture
    ) {
        ParticleSystemRenderer renderer = particleSys.GetComponent<ParticleSystemRenderer>();
        renderer.material = material;
        material.SetTexture(Shader.PropertyToID("_MainTex"), fireflyTexture);
        particleSys.Stop();
    }

    // FOR DEBUGGING
    private static void ConfigureTrailModule(UnityEngine.ParticleSystem particleSys, Material material) {
        UnityEngine.ParticleSystem.TrailModule trailModule = particleSys.trails;
        trailModule.enabled = true;
        trailModule.mode = ParticleSystemTrailMode.PerParticle;
        trailModule.dieWithParticles = true;
        ParticleSystemRenderer pSR = particleSys.GetComponent<ParticleSystemRenderer>();
        pSR.trailMaterial = material;
        pSR.trailMaterial.SetColor(Shader.PropertyToID("_Color"), ColorManager.PurpleEmission);
    }
}