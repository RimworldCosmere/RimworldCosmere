using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Comp.Hediff;
using Cosmere.System.Scadrial.Def;
using Verse;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     Where the copperclouds on a map are, and whether one of them is hiding something.
/// </summary>
/// <remarks>
///     Copper hides Allomantic pulses, so anything standing in a cloud stronger than the burn
///     reaching for it simply is not there: no Seeker line, no rioting, no soothing, no seizure.
///     <para>
///         The emitter is covered by their own cloud. That matters more than it sounds - a Smoker
///         never receives the CopperCloud hediff, because the aura that hands it out skips itself,
///         so a check written against that hediff would leave the one pawn burning copper as the
///         only pawn it did not protect.
///     </para>
/// </remarks>
public static class Coppercloud {
    /// <summary>Rebuilt once a tick at most; every aura scanning in that tick reuses it.</summary>
    /// <remarks>Keyed by id rather than by Map, so an abandoned map is not held alive by this.</remarks>
    private static readonly Dictionary<int, Snapshot> cache = [];

    /// <summary>
    ///     Whether this map has any cloud at all, so a scan can skip the rest of the work.
    /// </summary>
    public static bool AnyOn(Map? map) {
        return map != null && CloudsOn(map).Count > 0;
    }

    /// <summary>
    ///     What the clouds covering this thing are worth together, or zero if none reach it.
    /// </summary>
    public static float StrengthOver(Verse.Thing? thing) {
        Map? map = thing?.Map;
        if (map == null) return 0f;

        List<Cloud> clouds = CloudsOn(map);
        if (clouds.Count == 0) return 0f;

        List<float> covering = [];
        IntVec3 position = thing!.Position;
        for (int i = 0; i < clouds.Count; i++) {
            if (position.InHorDistOf(clouds[i].center, clouds[i].radius)) covering.Add(clouds[i].strength);
        }

        return DiminishingStack.Combine(covering);
    }

    /// <summary>
    ///     How hard this pawn is burning that metal right now, or zero if they are not.
    /// </summary>
    /// <remarks>
    ///     Both halves of the contest are read through here, so a Smoker and a Seeker are always
    ///     measured on the same scale.
    /// </remarks>
    public static float BurnStrengthOf(Pawn? pawn, MetallicArtsMetalDef? metal) {
        if (pawn?.abilities == null || metal == null) return 0f;

        float strongest = 0f;
        List<RimWorld.Ability> abilities = pawn.abilities.abilities;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is not AllomancyAbility burn) continue;
            if (burn.def.metal != metal || !burn.atLeastBurning) continue;

            float strength = burn.GetStrength();
            if (strength > strongest) strongest = strength;
        }

        return strongest;
    }

    /// <summary>
    ///     Whether a cloud on the target is enough to hide it from this burn.
    /// </summary>
    public static bool Hides(Pawn? seeker, Verse.Thing? target, MetallicArtsMetalDef? metal) {
        if (seeker == null || target == null) return false;

        return Hides(target, BurnStrengthOf(seeker, metal));
    }

    /// <summary>
    ///     The same question with the burn already measured, for scans that test many things
    ///     against one burn.
    /// </summary>
    public static bool Hides(Verse.Thing? target, float senseStrength) {
        return CoppercloudContest.Blocks(StrengthOver(target), senseStrength);
    }

    private static List<Cloud> CloudsOn(Map map) {
        int tick = Find.TickManager.TicksGame;
        if (cache.TryGetValue(map.uniqueID, out Snapshot cached) && cached.tick == tick) return cached.clouds;

        List<Cloud> clouds = [];
        IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
        for (int i = 0; i < pawns.Count; i++) {
            Pawn smoker = pawns[i];
            Verse.Hediff? aura = smoker.health?.hediffSet?.GetFirstHediffOfDef(
                HediffDefOf.Cosmere_Scadrial_Hediff_CopperAura
            );
            if (aura == null) continue;

            float strength = BurnStrengthOf(smoker, MetallicArtsMetalDefOf.Copper);
            if (strength <= 0f) continue;

            clouds.Add(new Cloud(smoker.Position, ReachOf(aura), strength));
        }

        cache[map.uniqueID] = new Snapshot(tick, clouds);

        return clouds;
    }

    /// <summary>
    ///     How far the cloud actually carries, read off the comp that hands the hediff out rather
    ///     than restated here. The two drifting apart is what left the old check reading a radius
    ///     the game never applied.
    /// </summary>
    private static float ReachOf(Verse.Hediff aura) {
        return aura.TryGetComp(out AllomancyAuraHediffGiver? giver) && giver != null ? giver.Reach : 0f;
    }

    private readonly struct Cloud(IntVec3 center, float radius, float strength) {
        public readonly IntVec3 center = center;
        public readonly float radius = radius;
        public readonly float strength = strength;
    }

    private readonly struct Snapshot(int tick, List<Cloud> clouds) {
        public readonly int tick = tick;
        public readonly List<Cloud> clouds = clouds;
    }
}
