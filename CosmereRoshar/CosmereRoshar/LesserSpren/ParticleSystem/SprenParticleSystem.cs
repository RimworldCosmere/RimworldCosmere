using Cosmere.Roshar.LesserSpren.SprenControllers;
using UnityEngine;
using Verse;
using Logger = Cosmere.Foundation.Logger;

namespace Cosmere.Roshar.LesserSpren.ParticleSystem;

public class SprenParticleSystem(SprenType sprenType, int mapID) {
    private const float UpdateInterval = 5f; // Update dynamic spren every 5 seconds
    private const float ParticleEmissionInterval = 5f; // Re-emit particles every 5 seconds (real-time)

    private readonly BaseSprenController controller = SprenControllerRegistry.GetController(sprenType)!;

    private int mapID { get; } = mapID;

    private SprenType sprenType { get; } = sprenType;
    public UnityEngine.ParticleSystem? particleSystem { get; private set; }
    private Mesh? spawnAreaMesh { get; set; }
    private bool isStatic { get; } = sprenType.IsNatureSpren();
    public float lastUpdateTime { get; set; }

    private float lastParticleEmissionTime { get; set; }

    public void InitializeOnMainThread() {
        if (spawnAreaMesh == null) {
            spawnAreaMesh = new Mesh { name = $"SprenMesh_{sprenType}" };
            spawnAreaMesh.MarkDynamic(); // Mark as frequently updated
        }

        if (particleSystem == null) {
            CreateParticleSystem(mapID);
        }
    }

    private void CreateParticleSystem(int mapID) {
        // Get representative spawn information for particle system configuration
        SprenSpawnInformation representativeSpawnInfo = GetRepresentativeSpawnInfo()!;
        particleSystem =
            Builder.CreateLesserSprenParticleSystem(mapID, controller, representativeSpawnInfo);

        // Ensure particle system is at world origin
        particleSystem.transform.position = Vector3.zero;

        // Customize particle system based on spren type
        ConfigureForSprenType(particleSystem);

        // Ensure it's in world space
        UnityEngine.ParticleSystem.MainModule main = particleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
    }

    private SprenSpawnInformation? GetRepresentativeSpawnInfo() {
        // Get a default spawn info from the controller to configure the particle system
        // We use an invalid position since this is just for getting movement parameters
        return controller.GetSprenSpawnInformation(IntVec3.Invalid, null);
    }

    private void ConfigureForSprenType(UnityEngine.ParticleSystem ps) {
        UnityEngine.ParticleSystem.MainModule main = ps.main;

        // Set colors from controller
        main.startColor = controller.sprenColor;

        // Configure size from controller
        main.startSize = main.startSize.constant * controller.sprenSizeMultiplier;

        // Configure emission rate from controller
        UnityEngine.ParticleSystem.EmissionModule emission = ps.emission;
        //emission.rateOverTime = emission.rateOverTime.constant * controller.emissionRateMultiplier;
        emission.rateOverTime = 0f;
    }

    public void UpdateMesh(MeshManager meshManager) {
        if (particleSystem == null || spawnAreaMesh == null) {
            Logger.Error(
                $"[Spren] Cannot update mesh - ParticleSystem: {particleSystem != null}, Mesh: {spawnAreaMesh != null}"
            );
            return;
        }

        if (controller.validSpawnInfo.Count == 0) {
            particleSystem.Stop();
            return;
        }

        // Disable mesh-based emission since we're doing manual positioning
        UnityEngine.ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
        shapeModule.enabled = false;

        UpdateParticleCount();

        EmitParticlesForActiveCells();
        lastParticleEmissionTime = Time.time;

        // Ensure the particle system is playing
        if (!particleSystem.isPlaying) {
            particleSystem.Play();
        }
    }

    private void UpdateParticleCount() {
        UnityEngine.ParticleSystem.MainModule main = particleSystem!.main;
        UnityEngine.ParticleSystem.EmissionModule emission = particleSystem.emission;

        int maxParticlesPerCell = controller.maxParticlesPerCell;
        int expectedParticles = controller.validSpawnInfo.Count * maxParticlesPerCell;
        main.maxParticles = Mathf.Max(10, expectedParticles);

        emission.rateOverTime = 0f;
    }

    public void EmitParticlesForActiveCells() {
        if (controller.activeSpawnInfo.Count == 0) return;


        List<UnityEngine.ParticleSystem.EmitParams> emitParamsList = [];

        int minParticles = controller.minParticlesPerCell;
        int maxParticles = controller.maxParticlesPerCell;

        foreach (SprenSpawnInformation info in controller.activeSpawnInfo) {
            int particlesForThisCell = Random.Range(minParticles, maxParticles + 1);

            for (int i = 0; i < particlesForThisCell; i++) {
                UnityEngine.ParticleSystem.EmitParams emitParams = new UnityEngine.ParticleSystem.EmitParams();

                Vector3 cellCenter = info.position!.Value.ToVector3Shifted();
                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    0f,
                    Random.Range(-0.5f, 0.5f)
                );
                emitParams.position = cellCenter + randomOffset;

                emitParams.velocity = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0.1f, 0.3f),
                    Random.Range(-0.5f, 0.5f)
                );

                emitParamsList.Add(emitParams);
            }
        }

        foreach (UnityEngine.ParticleSystem.EmitParams emitParams in emitParamsList) {
            particleSystem!.Emit(emitParams, 1);
        }

        lastParticleEmissionTime = Time.time;
    }

    public bool ShouldUpdateDynamic() {
        return !isStatic && Time.time - lastUpdateTime > UpdateInterval;
    }

    public bool ShouldReEmitParticles() {
        if (particleSystem == null || controller.validSpawnInfo.Count == 0) return false;

        // Re-emit if particle count drops below minimum expected
        return particleSystem.particleCount < controller.minParticlesPerCell;
    }

    public void Destroy() {
        StateHandler.DestroyParticleSystem(particleSystem);
        if (spawnAreaMesh != null) {
            Object.Destroy(spawnAreaMesh);
        }
    }
}