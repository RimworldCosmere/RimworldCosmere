using Concord;
using Cosmere.Core.Def;
using Cosmere.Core.Util;
using RimWorld.Planet;

namespace Cosmere.Core.Patch;

/// <summary>
///     Guarantees the save's world is committed before any world is generated.
/// </summary>
/// <remarks>
///     A WorldGenStep that reads the world during generation - Scadrial's Ashmounts, for one -
///     runs inside GenerateWorld, so the world has to be set before it is called. The colony
///     creation page and the Cosmere quickstart both set it explicitly at the right point, but
///     RimWorld's own Root_Play.SetupForQuickTestPlay builds the Game and generates the world in
///     one method with nothing in between to hook. Guarding the generator itself covers that and
///     any other caller, present or future.
/// </remarks>
[Patch(typeof(WorldGenerator))]
public static class WorldBeforeGenerationPatch {
    [Inject(At.Head, nameof(WorldGenerator.GenerateWorld))]
    private static void BeforeGenerateWorld() {
        if (WorldUtility.Primary != null) return;

        // Important, not Info: Info is filtered out of the shipped log level, and a world
        // seeded this late is exactly what someone debugging a wrong-world save needs to see.
        CosmereWorldDef? seeded = WorldUtility.SeedFromScenario();
        if (seeded == null) Logger.Warning("World was unset at generation and no world could be inferred.");
    }
}
