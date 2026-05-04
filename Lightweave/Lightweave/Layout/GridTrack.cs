using Cosmere.Lightweave.Types;

namespace Cosmere.Lightweave.Layout;

public abstract record GridTrack {
    public static GridTrack Of(Rem r) {
        return new Fixed(r);
    }

    public static IReadOnlyList<GridTrack> Expand(IReadOnlyList<GridTrack> tracks) {
        List<GridTrack> outList = new List<GridTrack>();
        foreach (GridTrack t in tracks) {
            if (t is Repeat rep) {
                for (int i = 0; i < rep.Count; i++) {
                    outList.Add(rep.Track);
                }
            }
            else {
                outList.Add(t);
            }
        }

        return outList;
    }

    public sealed record Fixed(Rem Size) : GridTrack;

    public sealed record Fr(float Weight) : GridTrack;

    public sealed record Content : GridTrack;

    public sealed record Repeat(int Count, GridTrack Track) : GridTrack;
}