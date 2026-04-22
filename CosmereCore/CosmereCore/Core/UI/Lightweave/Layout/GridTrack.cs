using System.Collections.Generic;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Layout;

public abstract record GridTrack
{
    public sealed record Fixed(Rem Size) : GridTrack;
    public sealed record Fr(float Weight) : GridTrack;
    public sealed record Content : GridTrack;
    public sealed record Repeat(int Count, GridTrack Track) : GridTrack;

    public static GridTrack Of(Rem r) => new Fixed(r);

    public static IReadOnlyList<GridTrack> Expand(IReadOnlyList<GridTrack> tracks)
    {
        List<GridTrack> outList = new List<GridTrack>();
        foreach (GridTrack t in tracks)
        {
            if (t is Repeat rep)
            {
                for (int i = 0; i < rep.Count; i++)
                {
                    outList.Add(rep.Track);
                }
            }
            else
            {
                outList.Add(t);
            }
        }
        return outList;
    }
}
