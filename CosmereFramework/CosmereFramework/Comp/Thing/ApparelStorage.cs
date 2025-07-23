using RimWorld;
using Verse;

namespace Cosmere.Framework.Comp.Thing;

public class ApparelStorageProperties : CompProperties {
    public StorageSettings? defaultStorageSettings;
    public StorageSettings? fixedStorageSettings;
    public int maxItems = -1;

    [MustTranslate]
    public string tabName;

    public ApparelStorageProperties() {
        compClass = typeof(ApparelStorage);
    }

    public override void ResolveReferences(ThingDef parentDef) {
        base.ResolveReferences(parentDef);
        defaultStorageSettings?.filter.ResolveReferences();
        fixedStorageSettings?.filter.ResolveReferences();
    }
}

public class ApparelStorage : ThingComp {
    public int maxItems => props.maxItems;
    public string tabName => props.tabName;
    private new ApparelStorageProperties props => (ApparelStorageProperties)base.props;
    public StorageSettings? defaultStorageSettings => props.defaultStorageSettings;
    public StorageSettings? fixedStorageSettings => props.fixedStorageSettings;
}