using Verse;

namespace Cosmere.System.Roshar.Comp.Game;

public class CultivationEntity : GameComponent, ILoadReferenceable {
    public CultivationEntity(Verse.Game game) { }

    public static CultivationEntity? Instance => Current.Game?.GetComponent<CultivationEntity>();

    public string GetUniqueLoadID() {
        return "Cosmere_CultivationEntity";
    }

    public override void ExposeData() {
        base.ExposeData();
    }
}
