using Cosmere.System.Scadrial.Grid;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Map;

/// <summary>
///     Owns what is on the ground. The weather decides when ash falls; this decides how much has
///     piled up and never forgets it when the weather clears.
/// </summary>
public class AshDepthTracker : MapComponent {
    private const int Stripes = 64;
    private const float UnitMm = 10f;

    /// <summary>Millimetres a day at severity 1. Progression moves severity, not this.</summary>
    public const float FullRateMmPerDay = 300f;

    /// <summary>Rain heavier than this washes ash off unroofed ground. Vanilla's filth threshold.</summary>
    private const float RainWashThreshold = 0.4f;

    private const int RainWashMmPerSweep = 20;

    private static MapMeshFlagDef? ashFlag;

    private readonly float[] stripeAccrual = new float[Stripes];

    private Render.AshParticles? veil;

    public Render.AshParticles? Veil => veil;

    private AshGrid grid;
    private float severity;
    private float severityTarget;

    public AshDepthTracker(Verse.Map map) : base(map) {
        grid = new AshGrid(map);
    }

    public AshGrid Grid => grid;

    public bool HasAnyAsh => Grid.Any;

    public float Severity => severity;

    private static MapMeshFlagDef AshFlag =>
        ashFlag ??= DefDatabase<MapMeshFlagDef>.GetNamed("Cosmere_Scadrial_MapMeshFlag_Ash");

    /// <summary>Severity the Final Empire sits at before any progression beat touches it.</summary>
    public const float BaselineSeverity = 0.15f;

    /// <summary>Progression writes the target; the tracker walks towards it over days.</summary>
    public void SetSeverityTarget(float target) {
        severityTarget = Mathf.Clamp01(target);
    }

    /// <summary>Dev shortcut. Progression must never use this - the whole point is the slow ramp.</summary>
    public void SetSeverityNow(float value) {
        severityTarget = Mathf.Clamp01(value);
        severity = severityTarget;
    }

    public override void FinalizeInit() {
        base.FinalizeInit();

        // Ash falls in the Final Empire whether or not a progression beat has fired yet, and it
        // was already falling before the colony landed - so the baseline applies at once rather
        // than easing up from clean air over a week.
        if (severityTarget <= 0f && AshEra.CanAccumulate(map)) {
            severityTarget = BaselineSeverity;
            if (severity <= 0f) severity = BaselineSeverity;
        }
    }

    /// <summary>The Catacendre. Stop the fall and let what is down drain rather than cutting it.</summary>
    public void BeginDrain() {
        severityTarget = 0f;
    }

    public override void MapComponentTick() {
        int tick = Find.TickManager.TicksGame;

        severity = AshDepthMath.EaseSeverity(severity, severityTarget, 0.12f, 1f / GenDate.TicksPerDay);

        if (!AshEra.CanAccumulate(map)) {
            if (severityTarget != 0f) severityTarget = 0f;
            if (Grid.Any) DrainStripe(tick % Stripes);

            return;
        }

        AccumulateStripe(tick % Stripes);
    }

    /// <summary>Rain carries ash off exactly as it washes vanilla filth away.</summary>
    private bool RainWashing => map.weatherManager.RainRate >= RainWashThreshold;

    public override void MapComponentUpdate() {
        if (map != Find.CurrentMap) return;

        UnityEngine.Shader.SetGlobalFloat(Scadrial.Shader.AshShaderProperties.AshSeverity, severity);

        if (severity <= 0.01f || !AshEra.ShouldRender(map)) return;

        if (veil is not { Alive: true }) veil = new Render.AshParticles(map.uniqueID);
        veil.Update(map, severity);
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref severity, "ashSeverity");
        Scribe_Values.Look(ref severityTarget, "ashSeverityTarget");
        Scribe_Deep.Look(ref grid, "ashGrid", map);

        if (Scribe.mode == LoadSaveMode.PostLoadInit) grid ??= new AshGrid(map);
    }

    /// <summary>
    ///     One stripe of cells per tick, so the whole map turns over every 64 ticks and no tick
    ///     ever walks 62,500 cells. Deposition is uniform, so the fractional millimetres are
    ///     banked per stripe until they add up to a whole unit.
    /// </summary>
    private void AccumulateStripe(int stripe) {
        if (RainWashing) {
            WashStripe(stripe);
            return;
        }

        float perSweepMm = AshDepthMath.FallRateMmPerHour(severity, FullRateMmPerDay) *
                           (Stripes / (float)GenDate.TicksPerHour);

        stripeAccrual[stripe] += perSweepMm;
        if (stripeAccrual[stripe] < UnitMm) return;

        int deposit = (int)(stripeAccrual[stripe] / UnitMm) * (int)UnitMm;
        stripeAccrual[stripe] -= deposit;

        bool changed = false;
        CellIndices indices = map.cellIndices;
        int count = indices.NumGridCells;

        for (int i = stripe; i < count; i += Stripes) {
            IntVec3 cell = indices.IndexToCell(i);
            if (!Grid.CanHaveAsh(cell)) continue;
            if (Grid.AddDepthMm(i, deposit) > 0) changed = true;
        }

        if (changed) NotifyAshChanged();
    }

    /// <summary>Ash moved, so the mesh and the dev overlay both need rebuilding.</summary>
    public void NotifyAshChanged() {
        map.mapDrawer.WholeMapChanged(AshFlag);
        map.GetComponent<Dev.AshOverlayDrawer>()?.SetDirty();
    }

    /// <summary>Unroofed cells only - rain cannot reach what is under a roof.</summary>
    private void WashStripe(int stripe) {
        bool changed = false;
        CellIndices indices = map.cellIndices;
        int count = indices.NumGridCells;

        for (int i = stripe; i < count; i += Stripes) {
            if (map.roofGrid.Roofed(indices.IndexToCell(i))) continue;
            if (Grid.RemoveDepthMm(i, RainWashMmPerSweep) > 0) changed = true;
        }

        if (changed) NotifyAshChanged();
    }

    /// <summary>After the Catacendre the ash goes, but over days rather than between frames.</summary>
    private void DrainStripe(int stripe) {
        bool changed = false;
        CellIndices indices = map.cellIndices;
        int count = indices.NumGridCells;

        for (int i = stripe; i < count; i += Stripes) {
            if (Grid.RemoveDepthMm(i, 30) > 0) changed = true;
        }

        if (changed) NotifyAshChanged();
    }
}
