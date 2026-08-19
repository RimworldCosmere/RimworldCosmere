using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Def;
using Cosmere.Core.Framework;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.Core.ShardConnection;

public static class WorldConnectionUtility {
    public static CosmereWorldDef? HomeworldFor(Pawn? pawn) {
        return WorldUtility.WorldForXenotype(pawn?.genes?.Xenotype);
    }

    public static CosmereWorldDef? CurrentWorld {
        get {
            CosmereWorldDef? world = WorldUtility.Primary;
            return world is { crossWorld: false } ? world : null;
        }
    }

    public static WorldConnection For(Pawn pawn, CosmereWorldDef world) {
        CosmereWorldDef? home = HomeworldFor(pawn);
        CosmereWorldDef? current = CurrentWorld;
        int ancestry = home == world ? ConnectionMath.AncestryFloor : 0;
        int residence = current == world
            ? ConnectionMath.ResidenceFrom(GameComponentCache<ResidenceTracker>.Get()?.TicksFor(pawn) ?? 0)
            : 0;
        int investiture = InvestitureFor(pawn, world);
        int earned = Earned(pawn, world);

        return new WorldConnection(world, ancestry, residence, investiture, earned);
    }

    public static List<WorldConnection> ActiveFor(Pawn? pawn) {
        List<WorldConnection> entries = [];
        if (pawn == null) return entries;

        CosmereWorldDef? home = HomeworldFor(pawn);
        CosmereWorldDef? current = CurrentWorld;

        if (home != null) entries.Add(For(pawn, home));
        if (current != null && current != home) entries.Add(For(pawn, current));

        List<CosmereWorldDef> worlds = WorldUtility.All;
        for (int i = 0; i < worlds.Count; i++) {
            CosmereWorldDef world = worlds[i];
            if (world.crossWorld || world == home || world == current) continue;

            WorldConnection connection = For(pawn, world);
            if (connection.Strength > 0) entries.Add(connection);
        }

        return entries;
    }

    public static List<WorldConnection> AllFor(Pawn? pawn) {
        List<WorldConnection> entries = [];
        if (pawn == null) return entries;

        List<CosmereWorldDef> worlds = WorldUtility.All;
        for (int i = 0; i < worlds.Count; i++) {
            if (!worlds[i].crossWorld) entries.Add(For(pawn, worlds[i]));
        }

        return entries;
    }

    public static int Earned(Pawn? pawn, CosmereWorldDef? world) {
        if (pawn == null || world == null) return 0;

        return pawn.TryGetComp<PawnConnectionStats>()?.EarnedFor(world) ?? 0;
    }

    public static void Grant(Pawn? pawn, CosmereWorldDef? world, int amount) {
        if (pawn == null || world == null || amount == 0) return;

        pawn.TryGetComp<PawnConnectionStats>()?.Grant(world, amount);
    }

    private static int InvestitureFor(Pawn pawn, CosmereWorldDef world) {
        int best = 0;
        for (int i = 0; i < world.nativeShards.Count; i++) {
            int strength = ConnectionInvestitureRegistry.StrengthFor(pawn, world.nativeShards[i]);
            if (strength > best) best = strength;
        }

        return best;
    }
}
