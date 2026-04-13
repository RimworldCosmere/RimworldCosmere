using Verse;

namespace Cosmere.System.Roshar.Comp.Game;

public class CultivationEntity : GameComponent, ILoadReferenceable {
    public static CultivationEntity? Instance => Current.Game?.GetComponent<CultivationEntity>();

    public CultivationEntity(Verse.Game game) { }

    public string GetUniqueLoadID() => "Cosmere_CultivationEntity";

    public override void ExposeData() {
        base.ExposeData();
    }
}
