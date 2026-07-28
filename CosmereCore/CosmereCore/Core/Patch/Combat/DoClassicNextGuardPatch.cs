using System;
using Concord;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class DoClassicNextGuardPatch : Page_ChooseIdeoPreset {
    [InjectField("classicIdeo")]
    private readonly Ideo classicIdeo = null!;

    [Inject(At.Head, "DoClassic")]
    private Control BeforeDoClassic() {
        List<Faction> factions = Find.FactionManager.AllFactionsListForReading;
        for (int i = 0; i < factions.Count; i++) {
            Faction faction = factions[i];
            if (faction.ideos == null) continue;

            faction.ideos.RemoveAll();
            faction.ideos.SetPrimary(classicIdeo);
        }

        Find.IdeoManager.RemoveUnusedStartingIdeos();
        Find.Scenario.PostIdeoChosen();

        if (next != null) {
            next.prev = Find.Storyteller.def.tutorialMode ? prev : this;
            Find.WindowStack.Add(next);
        }

        Action nextAct = this.nextAct;
        if (nextAct != null) {
            nextAct();
        }

        TutorSystem.Notify_Event("PageClosed");
        TutorSystem.Notify_Event("GoToNextPage");
        Close();

        return Control.Cancel;
    }
}
