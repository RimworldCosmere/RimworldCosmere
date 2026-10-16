using Concord;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class SteelMinistryDeityGenderPatch : IdeoFoundation_Deity {
    private const string SteelMinistryMemeDefName = "Cosmere_Structure_SteelMinistry";

    [Inject(At.Return, "FillDeity")]
    private void AfterFillDeity(Deity deity) {
        Ideo? ideo = this.ideo;
        if (ideo?.StructureMeme == null) return;
        if (ideo.StructureMeme.defName != SteelMinistryMemeDefName) return;

        deity.gender = Gender.Male;
    }
}
