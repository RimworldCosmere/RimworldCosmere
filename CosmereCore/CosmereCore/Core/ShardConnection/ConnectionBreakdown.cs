namespace Cosmere.Core.ShardConnection;

public readonly struct ConnectionBreakdown {
    public ConnectionBreakdown(
        int ancestry,
        int residence,
        int investiture,
        int earned,
        int held,
        int harmony,
        int total
    ) {
        Ancestry = ancestry;
        Residence = residence;
        Investiture = investiture;
        Earned = earned;
        Held = held;
        Harmony = harmony;
        Total = total;
    }

    public int Ancestry { get; }

    public int Residence { get; }

    public int Investiture { get; }

    public int Earned { get; }

    public int Held { get; }

    public int Harmony { get; }

    public int Total { get; }
}
