using System.Collections.Generic;
using Cosmere.System.Scadrial.Grid;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Comp.Map;

/// <summary>
///     Owns what is on the ground. The weather decides when ash falls; this decides how much has
///     piled up and never forgets it when the weather clears.
/// </summary>
public class AshDepthTracker : MapComponent {
    private const int Stripes = 64;
    private const float UnitMm = AshGrid.UnitMm;

    /// <summary>A stripe comes round once every 64 ticks, so 937.5 times a day.</summary>
    private const float SweepsPerDay = GenDate.TicksPerDay / (float)Stripes;

    /// <summary>Millimetres a day at severity 1 on a tile no Ashmount reaches.</summary>
    public const float BaseRateMmPerDay = 300f;

    private float exposureMultiplier = 1f;

    /// <summary>How much worse this tile is than open ground, 1 to 3. Set once at FinalizeInit.</summary>
    public float ExposureMultiplier => exposureMultiplier;

    /// <summary>What the sweep actually deposits at severity 1 on this map.</summary>
    public float FullRateMmPerDay => BaseRateMmPerDay * exposureMultiplier;

    /// <summary>Rain heavier than this washes ash off unroofed ground. Vanilla's filth threshold.</summary>
    private const float RainWashThreshold = 0.4f;

    private const int RainWashMmPerSweep = 20;

    private static MapMeshFlagDef? ashFlag;
    private static TerrainDef? ashTerrain;

    private float[] stripeAccrual = new float[Stripes];
    private float[] drainAccrual = new float[Stripes];

    private Render.AshParticles? veil;

    public Render.AshParticles? Veil => veil;

    private AshGrid grid;
    private AshTerrainMemory terrainMemory;
    private AshSettleClock settleClock;
    private AshBuriedCells buried;
    private float severity;
    private float severityTarget;

    private readonly List<Comp.Thing.CompAshVent> vents = new List<Comp.Thing.CompAshVent>();

    public AshDepthTracker(Verse.Map map) : base(map) {
        grid = new AshGrid(map);
        terrainMemory = new AshTerrainMemory(map);
        settleClock = new AshSettleClock(map);
        buried = new AshBuriedCells(map.cellIndices.NumGridCells);
    }

    public AshGrid Grid => grid;

    /// <summary>Cells holding enough ash to swallow what is on them. Kept current by the sweep.</summary>
    public AshBuriedCells Buried => buried;

    /// <summary>What the deep cells were before the ash, so the Catacendre can put them back.</summary>
    public AshTerrainMemory TerrainMemory => terrainMemory;

    public bool HasAnyAsh => Grid.Any;

    public float Severity => severity;

    private static MapMeshFlagDef AshFlag =>
        ashFlag ??= DefDatabase<MapMeshFlagDef>.GetNamed("Cosmere_Scadrial_MapMeshFlag_Ash");

    private static TerrainDef AshTerrain =>
        ashTerrain ??= DefDatabase<TerrainDef>.GetNamed("Cosmere_Scadrial_Terrain_Ash");

    /// <summary>Severity the Final Empire sits at before any progression beat touches it.</summary>
    public const float BaselineSeverity = AshPressure.Default;

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

        exposureMultiplier = Comp.Game.AshmountExposureCache.For(map.Tile);
        if (exposureMultiplier > 1.01f) {
            Logger.Important($"Ash: this tile sits at {exposureMultiplier:0.00}x for Ashmount exposure.");
        }

        // Ash falls in the Final Empire whether or not a progression beat has fired yet, and it
        // was already falling before the colony landed - so the arc's current pressure applies at
        // once rather than easing up from clean air over a week.
        if (severityTarget > 0f || !AshEra.CanAccumulate(map)) return;

        severityTarget = AshPressure.Target;
        if (severity <= 0f) severity = severityTarget;
    }

    /// <summary>The Catacendre. Stop the fall and let what is down drain rather than cutting it.</summary>
    public void BeginDrain() {
        severityTarget = 0f;
    }

    /// <summary>Vents announce themselves rather than the sweep scanning for them each tick.</summary>
    public void RegisterVent(Comp.Thing.CompAshVent vent) {
        if (!vents.Contains(vent)) vents.Add(vent);
    }

    public void DeregisterVent(Comp.Thing.CompAshVent vent) {
        vents.Remove(vent);
    }

    public override void MapComponentTick() {
        int stripe = Find.TickManager.TicksGame % Stripes;

        severity = AshDepthMath.EaseSeverity(severity, severityTarget, 0.12f, 1f / GenDate.TicksPerDay);

        if (!AshEra.CanAccumulate(map)) {
            if (severityTarget != 0f) severityTarget = 0f;
            if (Grid.Any) DrainStripe(stripe);
        } else {
            AccumulateStripe(stripe);

            // Stripe 0 only: a vent's plume is a bounded write, so it runs once per sweep cycle.
            if (stripe == 0 && vents.Count > 0) {
                float cycleDays = Stripes / (float)GenDate.TicksPerDay;
                bool changed = false;
                for (int i = 0; i < vents.Count; i++) {
                    if (vents[i].ContributeToGrid(Grid, cycleDays)) changed = true;
                }

                if (changed) NotifyAshChanged();
            }
        }

        // Outside the era branch on purpose: the terrain has to unwind off the draining grid, not
        // off the era flip, or the Catacendre hands metres of ash back in a single frame.
        SweepTerrain(stripe);
    }

    /// <summary>Rain carries ash off exactly as it washes vanilla filth away.</summary>
    private bool RainWashing => map.weatherManager.RainRate >= RainWashThreshold;

    public override void MapComponentUpdate() {
        if (map != Find.CurrentMap) return;

        UnityEngine.Shader.SetGlobalFloat(Scadrial.Shader.AshShaderProperties.AshSeverity, severity);

        // CanAccumulate, not ShouldRender: nothing new falls after the Catacendre. The ground
        // layers keep drawing off ShouldRender until the grid empties, but the sky stops feeding.
        if (severity <= 0.01f || !AshEra.CanAccumulate(map)) {
            // Flakes already in the air finish falling. Only drop the emitter once the ground has
            // drained too, so the end of the ashfall reads as stopping and not as a cut.
            if (veil != null && !AshEra.ShouldRender(map)) {
                veil.Stop();
                veil = null;
            }

            return;
        }

        if (veil is not { Alive: true }) veil = new Render.AshParticles(map.uniqueID);
        veil.Update(map, severity);
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref severity, "ashSeverity");
        Scribe_Values.Look(ref severityTarget, "ashSeverityTarget");
        Scribe_Deep.Look(ref grid, "ashGrid", map);
        Scribe_Deep.Look(ref terrainMemory, "ashTerrainMemory", map);
        Scribe_Deep.Look(ref settleClock, "ashSettleClock", map);
        ExposeAccrual(ref stripeAccrual, "ashStripeAccrual");
        ExposeAccrual(ref drainAccrual, "ashDrainAccrual");
        ExposeBuried();

        if (Scribe.mode != LoadSaveMode.PostLoadInit) return;

        grid ??= new AshGrid(map);
        terrainMemory ??= new AshTerrainMemory(map);
        settleClock ??= new AshSettleClock(map);
    }

    /// <summary>
    ///     Banked sub-unit millimetres, through the list round trip and length guard the vent uses.
    ///     Leaving them unsaved costs every stripe just under a whole unit on each load.
    /// </summary>
    private static void ExposeAccrual(ref float[] accrual, string label) {
        List<float>? banked = null;
        if (Scribe.mode == LoadSaveMode.Saving) banked = [..accrual];

        Scribe_Collections.Look(ref banked, label, LookMode.Value);

        if (Scribe.mode != LoadSaveMode.LoadingVars) return;

        accrual = AshPlume.RestoreBank(banked, Stripes) ?? new float[Stripes];
    }

    /// <summary>
    ///     Bit-packed, so a fully buried 250x250 map costs under 8 KB before the deflate. Unsaved,
    ///     every load would hand back a stockpile the ash had already swallowed.
    /// </summary>
    private void ExposeBuried() {
        int count = map.cellIndices.NumGridCells;
        bool[]? saved = Scribe.mode == LoadSaveMode.Saving ? buried.Raw : null;

        DataExposeUtility.LookBoolArray(ref saved, count, "ashBuriedCells");

        if (Scribe.mode != LoadSaveMode.LoadingVars) return;

        buried = AshBuriedCells.Restore(saved, count);
    }

    /// <summary>
    ///     Turns deep cells into ash terrain and hands them back as they thin out. Runs every
    ///     sweep rather than only on a deposit, so a cell a pawn just shovelled reverts without
    ///     waiting on the next millimetre to fall.
    /// </summary>
    private void SweepTerrain(int stripe) {
        SweepTerrain(stripe, AshDepthMath.TerrainChangesPerSweep);
    }

    /// <summary>Dev shortcut. Settles every stripe at once instead of waiting out the dwell.</summary>
    public int RunTerrainSweepNow() {
        settleClock.ExpireAll();

        for (int stripe = 0; stripe < Stripes; stripe++) {
            SweepTerrain(stripe, int.MaxValue, true);
        }

        return terrainMemory.SwappedCount;
    }

    private void SweepTerrain(int stripe, int budget, bool ignoreDwell = false) {
        if (!Grid.Any && terrainMemory.SwappedCount == 0 && !buried.Any) return;

        CellIndices indices = map.cellIndices;
        int count = indices.NumGridCells;
        int today = GenDate.DaysPassed;
        bool flipped = false;

        for (int i = stripe; i < count; i += Stripes) {
            int mm = Grid.GetDepthMm(i);
            if (buried.Set(i, AshDepthMath.IsBuried(mm, buried.IsBuried(i)))) flipped = true;

            // The budget rations terrain swaps only. Burial has to finish the stripe.
            if (budget <= 0) continue;

            AshTerrainAction action = AshDepthMath.NextTerrainAction(mm, terrainMemory.IsSwapped(i));

            if (action == AshTerrainAction.Leave) {
                settleClock.Cancel(i);
                continue;
            }

            // Ash does not become ground the day it gets deep enough, and it does not stop being
            // ground the day it thins. Each cell holds for its own seven to thirty days either
            // way, so the map turns over in patches rather than as a wave.
            bool settling = action == AshTerrainAction.Swap;
            if (!ignoreDwell && !settleClock.IsDue(i, today, settling)) continue;

            bool changed = settling ? SwapToAsh(i) : RestoreUnderAsh(i);
            if (!changed) continue;

            settleClock.Cancel(i);
            budget--;
        }

        // The depth write dirtied the mesh up to 64 ticks before this stripe turned it into a flip.
        if (flipped) NotifyAshChanged();
    }

    /// <summary>Only natural ground goes under. A floor the colony laid stays theirs.</summary>
    private bool SwapToAsh(int index) {
        TerrainDef current = map.terrainGrid.TerrainAt(index);
        if (current == AshTerrain) return false;

        // temporary comes first: TerrainAt hands back the temp layer, and recording that as the
        // original would send the restore through SetTempTerrain years later.
        if (current.temporary || !current.natural || current.IsWater) return false;
        if (current.passability == Traversability.Impassable) return false;

        terrainMemory.Remember(index, current);
        map.terrainGrid.SetTerrain(map.cellIndices.IndexToCell(index), AshTerrain);
        return true;
    }

    /// <summary>
    ///     Puts the cell back only if it is still the ash we put there. Anything else means the
    ///     player built over it while it was buried, and that is theirs to keep.
    /// </summary>
    private bool RestoreUnderAsh(int index) {
        TerrainDef? original = terrainMemory.Take(index);
        if (original == null) return false;
        if (map.terrainGrid.TerrainAt(index) != AshTerrain) return false;

        map.terrainGrid.SetTerrain(map.cellIndices.IndexToCell(index), original);
        return true;
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
        // A cell is only visited once every 64 ticks, so the per-sweep loss is well under one
        // unit. Bank the fractions per stripe the way deposition does, or every drain rounds to
        // zero and nothing ever clears.
        drainAccrual[stripe] += AshDepthMath.DrainMmPerSweep(AshGrid.MaxDepthMm, SweepsPerDay);
        if (drainAccrual[stripe] < UnitMm) return;

        int remove = (int)(drainAccrual[stripe] / UnitMm) * (int)UnitMm;
        drainAccrual[stripe] -= remove;

        bool changed = false;
        CellIndices indices = map.cellIndices;
        int count = indices.NumGridCells;

        for (int i = stripe; i < count; i += Stripes) {
            if (Grid.RemoveDepthMm(i, remove) > 0) changed = true;
        }

        if (changed) NotifyAshChanged();
    }
}
