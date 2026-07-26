using Cosmere.System.Roshar.LesserSpren.SprenController;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.LesserSpren.ParticleSystem;

public class SprenParticleSystem(SprenType sprenType, int mapID) {
    private readonly BaseSprenController controller = SprenControllerRegistry.GetController(sprenType)!;
    private float? lastEmit;
    public Dictionary<IntVec3, (int, float)> particleCache = [];

    private int mapID { get; } = mapID;

    public UnityEngine.ParticleSystem? particleSystem { get; private set; }

    public void Initialize() {
        if (particleSystem == null) {
            CreateParticleSystem(mapID);
        }
    }

    private void CreateParticleSystem(int mapID) {
        particleSystem =
            Builder.CreateLesserSprenParticleSystem(mapID, controller, controller.defaultSpawnInformation!);

        particleSystem.transform.position = Vector3.zero;

        ConfigureForSprenType(particleSystem);

        UnityEngine.ParticleSystem.MainModule main = particleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        particleSystem.gameObject.SetActive(true);
        particleSystem.Play();

        UnityEngine.ParticleSystem.MainModule mainModule = particleSystem.main;
        mainModule.simulationSpeed = 1f;

        UpdateParticles();
    }

    private void ConfigureForSprenType(UnityEngine.ParticleSystem ps) {
        UnityEngine.ParticleSystem.MainModule main = ps.main;

        main.startColor = controller.sprenColor;
        main.startSize = main.startSize.constant * controller.sprenSizeMultiplier;

        UnityEngine.ParticleSystem.EmissionModule emission = ps.emission;
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

    public bool ShouldReEmitParticles() {
        return lastEmit < (Time.time + particleSystem?.main.duration ?? 0);
    }

    public void EmitParticlesForActiveCells() {
        if (controller.activeSpawnInfo.Count == 0 || !Mod.Settings.lesserSprenSpawn) return;
        if (Current.CameraDriver.ZoomRootSize > Mod.Settings.lesserSprenMaxZoomSpawn) return;

        List<UnityEngine.ParticleSystem.EmitParams> emitParamsList = [];

        int minParticles = controller.minParticlesPerCell;
        int maxParticles = controller.maxParticlesPerCell;

        CellRect rect = Current.CameraDriver.CurrentViewRect.ClipInsideMap(Find.CurrentMap).ExpandedBy(1);

        particleCache.RemoveAll(p => Time.time > p.Value.Item2);
        foreach (SprenSpawnInformation info in controller.activeSpawnInfo) {
            IntVec3 position = info.position!.Value;
            if (!position.InBounds(Find.CurrentMap) || !rect.Contains(position)) continue;
            int existingParticles = particleCache.TryGetValue(position, (0, 0)).Item1;
            if (existingParticles > minParticles) continue;
            int particlesForThisCell = Random.Range(minParticles, maxParticles + 1) - existingParticles;
            if (particlesForThisCell < 0) continue;

            for (int i = 0; i < particlesForThisCell; i++) {
                UnityEngine.ParticleSystem.EmitParams emitParams = new UnityEngine.ParticleSystem.EmitParams();

                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    0f,
                    Random.Range(-0.5f, 0.5f)
                );
                emitParams.position = position.ToVector3Shifted() + randomOffset;

                emitParams.velocity = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0.1f, 0.3f),
                    Random.Range(-0.5f, 0.5f)
                );

                emitParamsList.Add(emitParams);
            }

            if (particleCache.ContainsKey(position)) {
                particleCache[position] = (particleCache[position].Item1 + particlesForThisCell,
                    particleSystem!.main.duration + Time.time);
            }
            else {
                particleCache[position] = (particlesForThisCell, particleSystem!.main.duration + Time.time);
            }
        }

        foreach (UnityEngine.ParticleSystem.EmitParams emitParams in emitParamsList) {
            particleSystem!.Emit(emitParams, 1);
        }

        lastEmit = Time.time;
    }

    public void SetVisible(bool visible) {
        if (particleSystem == null) return;
        particleSystem.gameObject.SetActive(visible);
    }

    public void Destroy() {
        if (particleSystem is not null) Object.Destroy(particleSystem.gameObject);
    }
}