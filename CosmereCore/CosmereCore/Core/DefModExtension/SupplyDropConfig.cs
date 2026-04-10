using Verse;

namespace Cosmere.Core.DefModExtension;

public class SupplyDropItem {
    public string? thing;
    public string? stuff;
    public int count = 1;
}

public class SupplyDropConfig : Verse.DefModExtension {
    public List<SupplyDropItem> items = [];
}
