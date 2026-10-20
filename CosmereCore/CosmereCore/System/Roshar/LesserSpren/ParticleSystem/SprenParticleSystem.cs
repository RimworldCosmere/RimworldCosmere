using Cosmere.System.Roshar.LesserSpren.SprenController;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.ParticleSystem;

public class SprenParticleSystem(SprenType sprenType, int mapID) {
    private readonly BaseSprenController controller = SprenControllerRegistry.GetController(sprenType)!;
    private float? lastEmit;
    public Dictionary<IntVec3, (int, float)> particleCache = [];

    private int MapID { get; } = mapID;

    public UnityEngine.ParticleSystem? ParticleSystem { get; private set; }

    public void Initialize() {
        if (ParticleSystem == null) {
            CreateParticleSystem(MapID);
        }
    }

    private void CreateParticleSystem(int mapID) {
        ParticleSystem =
            Builder.CreateLesserSprenParticleSystem(mapID, controller, controller.DefaultSpawnInformation);

        ParticleSystem.transform.position = Vector3.zero;

        ConfigureForSprenType(ParticleSystem);

        UnityEngine.ParticleSystem.MainModule main = ParticleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.gameObject.SetActive(true);
        ParticleSystem.Play();

        UnityEngine.ParticleSystem.MainModule mainModule = ParticleSystem.main;
        mainModule.simulationSpeed = 1f;

        UpdateParticles();
    }

    private void ConfigureForSprenType(UnityEngine.ParticleSystem ps) {
        UnityEngine.ParticleSystem.MainModule main = ps.main;

        main.startColor = controller.SprenColor;
        main.startSize = main.startSize.constant * controller.SprenSizeMultiplier;

        UnityEngine.ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
    }

    public void UpdateParticles() {
        if (ParticleSystem == null) {
            Log.Error(
                $"[Spren] Cannot update particles - ParticleSystem: {ParticleSystem != null}"
            );
            return;
        }

        if (controller.ValidSpawnInfo.Count == 0) {
            ParticleSystem.Stop();
            return;
        }

        // Disable mesh-based emission since we're doing manual positioning
        UnityEngine.ParticleSystem.ShapeModule shapeModule = ParticleSystem.shape;
        shapeModule.enabled = false;

        UpdateParticleCount();

        EmitParticlesForActiveCells();

        if (!ParticleSystem.isPlaying) {
            ParticleSystem.Play();
        }
    }

    private void UpdateParticleCount() {
        UnityEngine.ParticleSystem.MainModule main = ParticleSystem!.main;
        UnityEngine.ParticleSystem.EmissionModule emission = ParticleSystem.emission;

        int maxParticlesPerCell = controller.MaxParticlesPerCell;
        int expectedParticles = controller.ValidSpawnInfo.Count * maxParticlesPerCell;
        main.maxParticles = Mathf.Max(10, expectedParticles);

        emission.rateOverTime = 0f;
    }

    public bool ShouldReEmitParticles() {
        return lastEmit < (Time.time + ParticleSystem?.main.duration ?? 0);
    }

    public void EmitParticlesForActiveCells() {
        if (controller.ActiveSpawnInfo.Count == 0 || !Mod.Settings.lesserSprenSpawn) return;
        if (Current.CameraDriver.ZoomRootSize > Mod.Settings.lesserSprenMaxZoomSpawn) return;

        List<UnityEngine.ParticleSystem.EmitParams> emitParamsList = [];

        int minParticles = controller.MinParticlesPerCell;
        int maxParticles = controller.MaxParticlesPerCell;

        CellRect rect = Current.CameraDriver.CurrentViewRect.ClipInsideMap(Find.CurrentMap).ExpandedBy(1);

        particleCache.RemoveAll(p => Time.time > p.Value.Item2);
        foreach (SprenSpawnInformation info in controller.ActiveSpawnInfo) {
            QueueCellParticles(info, rect, emitParamsList, minParticles, maxParticles);
        }

        foreach (UnityEngine.ParticleSystem.EmitParams emitParams in emitParamsList) {
            ParticleSystem!.Emit(emitParams, 1);
        }

        lastEmit = Time.time;
    }

    private void QueueCellParticles(
        SprenSpawnInformation info,
        CellRect rect,
        List<UnityEngine.ParticleSystem.EmitParams> emitParamsList,
        int minParticles,
        int maxParticles
    ) {
        IntVec3 position = info.position!.Value;
        if (!position.InBounds(Find.CurrentMap) || !rect.Contains(position)) return;
        int existingParticles = particleCache.TryGetValue(position, (0, 0)).Item1;
        if (existingParticles > minParticles) return;
        int particlesForThisCell = Random.Range(minParticles, maxParticles + 1) - existingParticles;
        if (particlesForThisCell < 0) return;

        for (int i = 0; i < particlesForThisCell; i++) {
            UnityEngine.ParticleSystem.EmitParams emitParams = default(UnityEngine.ParticleSystem.EmitParams);

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

        int alreadyCached = particleCache.TryGetValue(position, out (int, float) cached) ? cached.Item1 : 0;
        particleCache[position] = (alreadyCached + particlesForThisCell,
            ParticleSystem!.main.duration + Time.time);
    }

    public void SetVisible(bool visible) {
        if (ParticleSystem == null) return;
        ParticleSystem.gameObject.SetActive(visible);
    }

    public void Destroy() {
        if (ParticleSystem is not null) Object.Destroy(ParticleSystem.gameObject);
    }
}
