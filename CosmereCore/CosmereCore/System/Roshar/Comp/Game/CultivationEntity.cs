using Verse;

namespace Cosmere.System.Roshar.Comp.Game;

public class CultivationEntity : GameComponent, ILoadReferenceable {
    public static CultivationEntity? Instance { get; private set; }

    public CultivationEntity(Verse.Game game) {
        Instance = this;
    }

    public string GetUniqueLoadID() => "Cosmere_CultivationEntity";

    public override void ExposeData() {
        base.ExposeData();
    }
}
