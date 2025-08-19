using Cosmere.Roshar.LesserSpren.SprenControllers;
using UnityEngine;
using Logger = Cosmere.Foundation.Logger;

namespace Cosmere.Roshar.LesserSpren.ParticleSystem;

public class SprenParticleSystem(SprenType sprenType, int mapID) {
    private readonly BaseSprenController controller = SprenControllerRegistry.GetController(sprenType)!;

    private int mapID { get; } = mapID;

    public UnityEngine.ParticleSystem? particleSystem { get; private set; }

    public void Initialize() {
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
        return controller.defaultSpawnInformation;
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

    public void UpdateParticles() {
        if (particleSystem == null) {
            Logger.Error(
                $"[Spren] Cannot update particles - ParticleSystem: {particleSystem != null}"
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
    }

    public bool ShouldReEmitParticles() {
        if (particleSystem == null || controller.validSpawnInfo.Count == 0) return false;

        // Re-emit if particle count drops below minimum expected
        return particleSystem.particleCount < controller.activeSpawnInfo.Count * controller.minParticlesPerCell;
    }

    public void Destroy() {
        if (particleSystem is not null) Object.Destroy(particleSystem.gameObject);
    }
}