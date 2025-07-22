using Cosmere.Framework.Comp.Thing;
using Cosmere.Framework.Object;
using RimWorld;
using Verse;

namespace Cosmere.Framework.Thing;

public class ApparelWithStorage : Apparel, IStoreSettingsParent, IThingHolder {
    public Inventory inventory;
    private StorageSettings settings;
    private ApparelStorage? storageCompCache;

    public ApparelWithStorage() {
        inventory = new Inventory(this);
    }

    private ApparelStorage storageComp {
        get {
            if (storageCompCache != null) return storageCompCache;

            storageCompCache = GetComp<ApparelStorage>();
            if (storageCompCache != null) return storageCompCache;

            storageCompCache = new ApparelStorage();
            storageCompCache.Initialize(new ApparelStorageProperties());

            return storageCompCache;
        }
    }

    public StorageSettings GetStoreSettings() {
        return settings;
    }

    public StorageSettings GetParentStoreSettings() {
        return storageComp.fixedStorageSettings ?? StorageSettings.EverStorableFixedSettings();
    }

    public void Notify_SettingsChanged() { }

    public bool StorageTabVisible => true;

    public void GetChildHolders(List<IThingHolder> outChildren) {
        outChildren.Add(inventory);
    }

    public ThingOwner GetDirectlyHeldThings() {
        return inventory.innerContainer;
    }

    public override void PostMake() {
        base.PostMake();
        settings = new StorageSettings(this);
        if (storageComp.defaultStorageSettings != null) {
            settings.CopyFrom(storageComp.defaultStorageSettings);
        }
    }
}