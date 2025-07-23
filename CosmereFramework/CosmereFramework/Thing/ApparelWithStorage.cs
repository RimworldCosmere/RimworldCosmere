using Cosmere.Framework.Comp.Thing;
using Cosmere.Framework.Object;
using RimWorld;
using Verse;

namespace Cosmere.Framework.Thing;

public class ApparelWithStorage : Apparel, IThingHolder, IHaulDestination {
    public Inventory inventory;
    private StorageSettings settings;
    private ApparelStorage? storageCompCache;

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

    public new Map Map => base.Map ?? ((Pawn?)holdingOwner?.Owner.ParentHolder)?.Map!;

    public StorageSettings GetStoreSettings() {
        return settings;
    }

    public StorageSettings GetParentStoreSettings() {
        return storageComp.fixedStorageSettings ?? StorageSettings.EverStorableFixedSettings();
    }

    public void Notify_SettingsChanged() { }

    public bool StorageTabVisible => true;
    public bool HaulDestinationEnabled => StorageTabVisible;

    public bool Accepts(Verse.Thing t) {
        int currentInventoryRemaining = storageComp.maxItems - inventory.innerContainer.TotalStackCount;

        return currentInventoryRemaining > 0 && GetStoreSettings().AllowedToAccept(t);
    } /*

    public int SpaceRemainingFor(ThingDef stuff) {
        return storageComp.maxItems - inventory.innerContainer.Count;
    }*/

    public void GetChildHolders(List<IThingHolder> outChildren) {
        outChildren.Add(inventory);
    }

    public ThingOwner GetDirectlyHeldThings() {
        return inventory.innerContainer;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref inventory, "inventory", this, storageComp.maxItems);
    }

    public override void PostMake() {
        base.PostMake();
        inventory = new Inventory(this, storageComp.maxItems);
        settings = new StorageSettings(this);
        if (storageComp.defaultStorageSettings != null) {
            settings.CopyFrom(storageComp.defaultStorageSettings);
        }
    }

    public override void Notify_RecipeProduced(Pawn pawn) {
        base.Notify_RecipeProduced(pawn);
        factionInt = pawn.Faction;
    }

    public override void Notify_Unequipped(Pawn pawn) {
        base.Notify_Unequipped(pawn);
        pawn.def.inspectorTabsResolved.RemoveWhere(x => x is InspectorTab.ApparelStorage);
    }

    public override void Notify_DebugSpawned() {
        base.Notify_DebugSpawned();
        factionInt = Faction.OfPlayer;
    }

    public override void Notify_Equipped(Pawn pawn) {
        base.Notify_Equipped(pawn);
        factionInt = pawn.Faction;
        pawn.def.inspectorTabsResolved.AddUnique(new InspectorTab.ApparelStorage(storageComp.tabName));
        if (!Map.haulDestinationManager.AllHaulDestinations.Contains(this)) {
            Map.haulDestinationManager.AddHaulDestination(this);
        }
    }
}