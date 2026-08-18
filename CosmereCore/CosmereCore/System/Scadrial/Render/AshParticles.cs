using Cosmere.System.Scadrial.Util;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Render;

/// <summary>
///     Falling ash as real particles, not a panning texture.
///     <para>
///     The previous implementation scrolled a tiled sheet. That can never look right: every flake
///     moves on one shared vector, the tile period is visible, and the quad is locked either to
///     the viewport (drags when you pan) or to the ground (scales when you zoom). Individual
///     particles fix all three at once.
///     </para>
/// </summary>
[StaticConstructorOnStartup]
public class AshParticles {
    /// <summary>Particles per second at severity 1. Mutable so the dev tuner can drive it live.</summary>
    public static float MaxEmissionRate = 1035f;

    /// <summary>Particles per second at severity 0.</summary>
    public static float MinEmissionRate = 15f;

    /// <summary>Curve between the two. Above 1 keeps the low end sparse; 1 is linear.</summary>
    public static float EmissionExponent = 2f;

    /// <summary>Live count, for the dev tuner readout.</summary>
    public int LiveCount => system == null ? 0 : system.particleCount;

    /// <summary>Rate actually used last frame, for the dev tuner readout.</summary>
    public float LastRate { get; private set; }

    /// <summary>Height above the terrain the field lives at, between weather and the map.</summary>
    private static readonly float Altitude = AltitudeLayer.Weather.AltitudeFor();

    private static readonly Material FlakeMat =
        MaterialPool.MatFrom("Weather/AshFlake", Verse.ShaderDatabase.Transparent);

    private readonly UnityEngine.ParticleSystem system;
    private readonly GameObject host;

    private float pendingEmit;

    public AshParticles(int mapId) {
        host = new GameObject($"cosmere_ash_particles_{Mathf.Abs(mapId)}");

        // Unity tears this down on scene load, leaving a dangling ref that NREs on host.transform each frame
        Object.DontDestroyOnLoad(host);
        system = host.AddComponent<UnityEngine.ParticleSystem>();
        ParticleSystemRenderer renderer = host.GetComponent<ParticleSystemRenderer>();

        Configure();
        ConfigureRenderer(renderer);
        system.Play();
    }

    /// <summary>
    ///     Repositions the emitter over the current view and retunes it for wind and severity.
    ///     Called every frame. The emitter box follows the camera but simulationSpace is World,
    ///     so particles already in flight stay put and you pan *through* them.
    /// </summary>
    /// <summary>False once Unity has torn the host down; the owner must rebuild.</summary>
    public bool Alive => host != null && system != null;

    public void Update(Verse.Map map, float severity) {
        if (!Alive) return;

        CameraDriver camera = Find.CameraDriver;
        CellRect view = camera.CurrentViewRect;

        float width = view.Width + 12f;
        float height = view.Height + 12f;

        host.transform.position = new Vector3(view.CenterCell.x, Altitude, view.CenterCell.z);

        UnityEngine.ParticleSystem.ShapeModule shape = system.shape;
        shape.scale = new Vector3(width, 1f, height);

        // emission tracks severity and screen area so zooming out doesnt thin the fall to nothing
        float area = width * height / 2500f;
        float rate = Mathf.Lerp(MinEmissionRate, MaxEmissionRate, Mathf.Pow(severity, EmissionExponent)) *
                     Mathf.Clamp(area, 0.4f, 4f);

        // fall stays downward-dominant: WindSpeed is unsigned, so lean comes from a drifting signed heading instead
        float wind = map.windManager.WindSpeed;
        float heading = Mathf.Sin(Find.TickManager.TicksGame / 4200f) +
                        0.35f * Mathf.Sin(Find.TickManager.TicksGame / 1150f);

        float fallMin = -2.4f - severity * 3.0f;
        float fallMax = -4.6f - severity * 6.0f;

        // one shared, slowly drifting slant reads as snow; per-particle random sideways motion reads as swirling
        float lean = Mathf.Clamp(heading, -1f, 1f) * (0.22f + wind * 0.30f) * Mathf.Abs(fallMin);

        // velocityOverLifetime silently dropped Z here (measured); setting velocity per-emit is deterministic instead

        // RimWorld never ticks a system it didnt make; rateOverTime is a no-op. emit by hand (see Builder.cs)
        LastRate = rate;

        float dt = Mathf.Min(Time.deltaTime, 0.1f);
        pendingEmit += rate * dt;
        int burst = (int)pendingEmit;
        pendingEmit -= burst;

        for (int i = 0; i < burst; i++) {
            float fall = Rand.Range(fallMax, fallMin);
            UnityEngine.ParticleSystem.EmitParams p = new UnityEngine.ParticleSystem.EmitParams {
                position = new Vector3(
                    view.CenterCell.x + Rand.Range(-width, width) * 0.5f,
                    Altitude + Rand.Range(-0.6f, 0.6f),
                    view.CenterCell.z + Rand.Range(-height, height) * 0.5f
                ),
                velocity = new Vector3(lean * Rand.Range(0.85f, 1.15f), 0f, fall),
                startSize = Rand.Range(0.11f, 0.34f),
                startLifetime = Rand.Range(3.5f, 9f),
                startColor = new Color32(189, 181, 171, (byte)Rand.Range(180, 255)),
                applyShapeToPosition = false,
            };
            system.Emit(p, 1);
        }

        // no Simulate() call: it pauses the system and wipes particles before they travel; velocity is baked in at emit
        if (!system.isPlaying) system.Play();
    }

    public void Stop() {
        if (host != null) Object.Destroy(host);
    }

    private void Configure() {
        UnityEngine.ParticleSystem.MainModule main = system.main;

        // world space is the point: particles anchor in the world, so panning moves you through the field, not with it
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.startLifetime = new UnityEngine.ParticleSystem.MinMaxCurve(3.5f, 11f);

        // zero on purpose: startSpeed fires along the emitters forward axis and would fight velocityOverLifetime
        main.startSpeed = 0f;

        // Several size bands rather than one. Small flakes read as distant, large as near.
        main.startSize = new UnityEngine.ParticleSystem.MinMaxCurve(0.11f, 0.34f);
        main.startRotation = new UnityEngine.ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new Color(0.74f, 0.71f, 0.67f, 0.92f);
        main.maxParticles = 3000;
        main.gravityModifier = 0f;

        // RimWorld loads paused (Time.timeScale 0), which freezes particles; vanilla weather keeps animating though
        main.useUnscaledTime = true;
        main.playOnAwake = true;

        UnityEngine.ParticleSystem.EmissionModule emission = system.emission;
        emission.enabled = true;
        emission.rateOverTime = MinEmissionRate;

        UnityEngine.ParticleSystem.ShapeModule shape = system.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.randomDirectionAmount = 0.12f;

        // noise must stay weak vs the fall rate or particles swirl in place; low frequency keeps it a wander
        UnityEngine.ParticleSystem.NoiseModule noise = system.noise;
        noise.enabled = true;
        noise.quality = ParticleSystemNoiseQuality.Medium;
        noise.octaveCount = 2;
        noise.frequency = 0.09f;
        noise.scrollSpeed = 0.18f;
        noise.damping = true;

        // Z pinned to zero: noise on the fall axis occasionally beats gravity at the slow end, so ash would rise
        noise.separateAxes = true;
        noise.strengthX = new UnityEngine.ParticleSystem.MinMaxCurve(0.02f, 0.09f);
        noise.strengthY = 0f;
        noise.strengthZ = 0f;

        UnityEngine.ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;

        // Fade in and out so flakes do not pop at the edges of their lifetime.
        UnityEngine.ParticleSystem.ColorOverLifetimeModule fade = system.colorOverLifetime;
        fade.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            [new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f)],
            [
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.15f),
                new GradientAlphaKey(1f, 0.8f),
                new GradientAlphaKey(0f, 1f),
            ]
        );
        fade.color = new UnityEngine.ParticleSystem.MinMaxGradient(gradient);
    }

    private static void ConfigureRenderer(ParticleSystemRenderer renderer) {
        renderer.material = FlakeMat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sortingFudge = -1f;
    }
}
