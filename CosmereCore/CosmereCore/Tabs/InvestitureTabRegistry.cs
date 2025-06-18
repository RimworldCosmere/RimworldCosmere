using System.Collections.Generic;
using Verse;

namespace CosmereCore.Tabs;

public static class InvestitureTabRegistry {
    public delegate void InvestitureTabDrawer(Pawn pawn, Listing_Standard listing);

    public static readonly List<InvestitureTabDrawer> drawers = new List<InvestitureTabDrawer>();

    public static void Register(InvestitureTabDrawer drawer) {
        if (!drawers.Contains(drawer)) {
            drawers.Add(drawer);
        }
    }
}