using Concord;
using Verse;

namespace Cosmere.System.Roshar.Patch.Spren;

[Patch]
public abstract class SprenFactionPatch : Pawn {
    [Inject(At.Head, nameof(SetFaction))]
    private Control BeforeSetFaction() {
        Pawn self = this;
        if (self is Cosmere.System.Roshar.Thing.Pawn.Animal.Spren && self.Faction != null) {
            return Control.Cancel;
        }

        return Control.Continue;
    }
}
