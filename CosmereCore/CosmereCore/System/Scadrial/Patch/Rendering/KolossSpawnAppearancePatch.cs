using Concord;
using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Rendering;

/// <summary>
///     Settles a koloss's look on arrival. PawnGenerator picks hair, beard and tattoos after the
///     gene's PostAdd, and the tick that would undo that never runs while the game is paused.
/// </summary>
[Patch]
public abstract class KolossSpawnAppearancePatch : Pawn {
    [Inject(At.Return, nameof(SpawnSetup))]
    private void AfterSpawnSetup(Map map, bool respawningAfterLoad) {
        KolossAppearance.Refresh(this);
    }
}
