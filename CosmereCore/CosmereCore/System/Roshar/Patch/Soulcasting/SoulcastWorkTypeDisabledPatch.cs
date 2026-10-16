using Concord;
using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.Soulcasting;

[Patch]
public abstract class SoulcastWorkTypeDisabledPatch : Pawn {
    [Inject(At.Return, nameof(GetDisabledWorkTypes))]
    private void AfterGetDisabledWorkTypes(ControlHandle<List<WorkTypeDef>> ch) {
        WorkTypeDef? soulcastWork = RosharWorkTypeDefOf.Cosmere_Roshar_WorkType_Soulcasting;
        if (soulcastWork == null) return;
        if (ch.ReturnValue.Contains(soulcastWork)) return;

        Pawn self = this;
        if (self.abilities != null) {
            List<Ability> abilities = self.abilities.AllAbilitiesForReading;
            for (int i = 0; i < abilities.Count; i++) {
                if (abilities[i] is Soulcast) return;
            }
        }

        ch.ReturnValue.Add(soulcastWork);
    }
}
