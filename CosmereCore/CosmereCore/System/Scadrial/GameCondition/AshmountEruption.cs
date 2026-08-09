using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Comp.Thing;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.GameCondition;

/// <summary>
///     An Ashmount opening up. Everything here drives a knob that already exists, and the only
///     state it needs is the start tick and duration vanilla already scribes onto a GameCondition.
/// </summary>
public class AshmountEruption : RimWorld.GameCondition {
    public override void Init() {
        base.Init();

        // Snapped rather than eased. A progression beat lands over days because the world itself is
        // thickening; a mountain opening does not ask the player to wait a week to notice.
        SingleMap?.GetComponent<AshDepthTracker>()?.SetSeverityNow(AshEruption.SpikeSeverity(AshPressure.Target));
    }

    public override void GameConditionTick() {
        base.GameConditionTick();

        Verse.Map? map = SingleMap;

        // Duration logs an error every time it is read on a permanent condition. The def refuses
        // to go permanent, but a dev tool can still set it.
        if (map == null || Permanent) return;

        // The Catacendre can land mid-eruption. The start and the end both check the era, and
        // without this the middle keeps throwing metal and shaking ground the mountains no longer own.
        if (!AshEra.CanAccumulate(map)) return;

        AshDepthTracker? tracker = map.GetComponent<AshDepthTracker>();
        if (tracker == null) return;

        int passed = TicksPassed;
        int duration = Duration;

        if (AshEruption.DueOn(passed, duration, AshEruption.ThrowsPerVent) && ThrowFromEveryVent(tracker)) {
            ShakeCamera(map, AshEruption.ThrowShake);
        }

        if (AshEruption.DueOn(passed, duration, AshEruption.TremorCount)) {
            Tremor(map, tracker);
            ShakeCamera(map, AshEruption.TremorShake);
        }
    }

    public override void End() {
        Restore();
        base.End();
    }

    /// <summary>
    ///     Hands the ashfall back to what the arc says it should be rather than to the Final Empire
    ///     constant, which a beat may have raised since. Re-derived rather than remembered, so it is
    ///     right whichever eruption ends last.
    /// </summary>
    private void Restore() {
        Verse.Map? map = SingleMap;
        if (map == null) return;
        if (AnotherEruptionRuns(map)) return;

        map.GetComponent<AshDepthTracker>()
            ?.SetSeverityTarget(AshEra.CanAccumulate(map) ? AshPressure.Target : 0f);
    }

    /// <summary>
    ///     Vanilla refuses a second eruption on a map that already has one, but a dev-forced
    ///     incident goes straight to the worker. Last one out restores, or the first to end cuts
    ///     the other's ashfall short. This runs before base.End, so we are still on the list.
    /// </summary>
    private bool AnotherEruptionRuns(Verse.Map map) {
        List<RimWorld.GameCondition> active = map.gameConditionManager.ActiveConditions;
        for (int i = 0; i < active.Count; i++) {
            if (active[i] != this && active[i] is AshmountEruption) return true;
        }

        return false;
    }

    /// <summary>
    ///     Every vent throws. Reports whether any of them found ground to land on, so the screen only
    ///     knocks when there is a lump on it to explain the knock.
    /// </summary>
    private static bool ThrowFromEveryVent(AshDepthTracker tracker) {
        IReadOnlyList<CompAshVent> vents = tracker.Vents;
        bool landed = false;

        for (int i = 0; i < vents.Count; i++) {
            if (vents[i].ThrowOnce()) landed = true;
        }

        return landed;
    }

    /// <summary>
    ///     One brief knock, once a beat rather than once a vent - six vents would stack six requests
    ///     and clamp to a full jolt. Gated on the map being the one on screen, as vanilla's callers are.
    /// </summary>
    private static void ShakeCamera(Verse.Map map, float magnitude) {
        if (map != Find.CurrentMap) return;

        Find.CameraDriver.shaker.DoShake(magnitude);
    }

    /// <summary>Rattles what stands near a mouth, hardest on the mouth itself.</summary>
    private static void Tremor(Verse.Map map, AshDepthTracker tracker) {
        IReadOnlyList<CompAshVent> vents = tracker.Vents;
        int reach = Mathf.CeilToInt(AshEruption.TremorRadiusCells);

        // One buffer for the whole beat. Six vents at reach 8 is around 1,200 cells to copy.
        List<Verse.Thing> standing = [];

        for (int i = 0; i < vents.Count; i++) {
            IntVec3 mouth = vents[i].parent.Position;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(mouth, reach, true)) {
                if (!cell.InBounds(map)) continue;

                int damage = AshEruption.TremorDamage(
                    mouth.DistanceTo(cell), AshEruption.TremorRadiusCells, AshEruption.TremorPeakDamage
                );

                if (damage > 0) ShakeCell(map, cell, damage, standing);
            }
        }
    }

    /// <summary>
    ///     Natural rock is skipped deliberately. A tremor that chewed through a mountain would be a
    ///     better payday than the metal the vent throws, and free.
    /// </summary>
    private static void ShakeCell(Verse.Map map, IntVec3 cell, int damage, List<Verse.Thing> standing) {
        // Copied, not walked live: killing a shelf hands its overflow to the next cell, which takes
        // several entries off the grid list in one TakeDamage and outruns any cursor into it.
        standing.Clear();
        standing.AddRange(map.thingGrid.ThingsListAtFast(cell));

        for (int i = 0; i < standing.Count; i++) {
            Verse.Thing thing = standing[i];

            // One cell per thing, or a wide building takes the tremor once per square it covers.
            if (thing.Position != cell) continue;
            if (thing is not Building) continue;
            if (!thing.def.useHitPoints) continue;
            if (thing.def.building is { isNaturalRock: true }) continue;

            thing.TakeDamage(new DamageInfo(DamageDefOf.Crush, damage));
        }
    }
}
