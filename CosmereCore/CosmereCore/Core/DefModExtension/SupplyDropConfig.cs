namespace Cosmere.Core.DefModExtension;

public class SupplyDropItem {
    public int count = 1;
    public string? stuff;
    public string? thing;
}

public class SupplyDropConfig : Verse.DefModExtension {
    public List<SupplyDropItem> items = [];
}