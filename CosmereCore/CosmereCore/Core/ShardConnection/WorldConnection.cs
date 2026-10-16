using Cosmere.Core.Def;

namespace Cosmere.Core.ShardConnection;

public readonly struct WorldConnection {
    public WorldConnection(CosmereWorldDef world, int ancestry, int residence, int investiture, int earned) {
        World = world;
        Ancestry = ancestry;
        Residence = residence;
        Investiture = investiture;
        Earned = earned;
    }

    public CosmereWorldDef World { get; }

    public int Ancestry { get; }

    public int Residence { get; }

    public int Investiture { get; }

    public int Earned { get; }

    public int Strength => ConnectionMath.ComposeWorld(Ancestry, Residence, Investiture, Earned);
}
