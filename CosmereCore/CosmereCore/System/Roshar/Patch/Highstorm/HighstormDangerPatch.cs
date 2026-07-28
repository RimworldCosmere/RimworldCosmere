using Concord;
using Cosmere.System.Roshar.Util;
using Verse;

namespace Cosmere.System.Roshar.Patch.Highstorm;

[Patch(typeof(Region))]
public abstract class HighstormDangerPatch {
    [InjectInstance]
    protected abstract Region Self { get; }

    [Inject(At.Return, nameof(Region.DangerFor))]
    private void AfterDangerFor(Pawn p, ControlHandle<Danger> ch) {
        if (ch.ReturnValue == Danger.Deadly) return;

        Room room = Self.Room;
        if (room == null || !room.PsychologicallyOutdoors) return;

        Map map = Self.Map;
        if (map == null) return;

        if (StormlightUtility.IsHighstormImmune(p)) return;

        List<RimWorld.GameCondition> conditions = map.gameConditionManager.ActiveConditions;
        for (int i = 0; i < conditions.Count; i++) {
            if (conditions[i] is Cosmere.System.Roshar.GameCondition.Highstorm hs && hs.IsDangerousPhase) {
                ch.ReturnValue = Danger.Deadly;
                return;
            }
        }
    }
}
