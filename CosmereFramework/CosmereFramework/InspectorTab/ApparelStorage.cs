using Cosmere.Framework.Thing;
using RimWorld;
using Verse;

namespace Cosmere.Framework.InspectorTab;

public class ApparelStorage : ITab_Storage {
    public ApparelStorage() { }

    public ApparelStorage(string labelKey) {
        this.labelKey = labelKey;
    }

    protected override bool IsPrioritySettingVisible => false;

    private Pawn? selPawn => SelThing as Pawn;

    protected override IStoreSettingsParent? SelStoreSettingsParent {
        get {
            if (SelThing is ApparelWithStorage) return SelThing as ApparelWithStorage;

            return selPawn?.apparel?.WornApparel?.FirstOrDefault(a => a is ApparelWithStorage) as ApparelWithStorage;
        }
    }
}