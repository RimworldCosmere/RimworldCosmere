using System.Threading;
using System.Threading.Tasks;
using Cosmere.Roshar.Map;
using Cosmere.Roshar.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Comp.Map;

public class LesserSprenSpawner : MapComponent {
    private const float ParticleAlpha = 2.5f;
    private const float MaxParticlesMultiplier = 0.5f;
    private const float EmissionRateBase = 0.25f;
    private const float EmissionRatePower = 2.7f;
    private readonly int mapID;
    private bool allColumnsValidated;

    private CellValidator cellValidator;
    public Task fetchTask;
    private MeshManager meshManager;
    private bool particlesSpawned;
    private UnityEngine.ParticleSystem particleSystem;
    private Mesh spawnAreaMesh;
    public List<IntVec3> validEmissionCells;

    public LesserSprenSpawner(Verse.Map map) : base(map) {
        mapID = map.GetHashCode();
        LongEventHandler.ExecuteWhenFinished(InitializeMapSystems);
    }

    public override void MapComponentTick() {
        base.MapComponentTick();

        if (particlesSpawned) {
            StateHandler.SetParticleSystemState(particleSystem, true);
            particlesSpawned = true;
        }

        if (!fetchTask.IsCompleted) return;

        //if (allColumnsValidated) return;
        //allColumnsValidated = meshManager.ValidateCells();
        //if (!allColumnsValidated) return;
        //meshManager.FinalValidCells = GenRadial.RadialCellsAround(UI.MouseCell(), 5, true).ToList();
        meshManager.ConstructMesh(spawnAreaMesh);
        validEmissionCells = meshManager.FinalValidCells;
        UnityEngine.ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
        shapeModule.mesh = spawnAreaMesh;
        UpdateParticleSystemParameters();
        FetchAllCells();
    }

    public void FetchAllCells() {
        fetchTask = Task.Run(() => {
                Thread.Sleep(30000);
                meshManager.ValidateCells();
            }
        );
    }

    private void UpdateParticleSystemParameters() {
        UnityEngine.ParticleSystem.MainModule main = particleSystem.main;
        UnityEngine.ParticleSystem.EmissionModule emission = particleSystem.emission;

        main.maxParticles = Mathf.FloorToInt(validEmissionCells.Count * MaxParticlesMultiplier);

        int validCellsCount = validEmissionCells.Count;
        float rawEmissionRate = validCellsCount * Mathf.Pow(EmissionRateBase, EmissionRatePower);
        float finalEmissionRate = Mathf.Max(4, Mathf.FloorToInt(rawEmissionRate));

        emission.rateOverTime = finalEmissionRate;
    }


    public override void MapRemoved() {
        base.MapRemoved();
        StateHandler.DestroyParticleSystem(particleSystem);
    }

    private void InitializeMapSystems() {
        if (particleSystem is not null) return;
        spawnAreaMesh = new Mesh();
        cellValidator = new CellValidator(map);
        meshManager = new MeshManager(map, cellValidator.IsCellValidForParticleEmissionMesh);
        particleSystem = Builder.CreateLesserSprenParticleSystem(mapID);
        particleSystem.transform.position = Vector3.zero;

        ColorManager.GetBaseColorGradient(particleSystem);
        ColorManager.SetParticleAlpha(particleSystem, ParticleAlpha);
        StateHandler.RestoreParticleSystemState(particleSystem);
        FetchAllCells();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref particlesSpawned, "particlesSpawned");
    }
}