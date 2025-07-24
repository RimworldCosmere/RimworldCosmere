using Cosmere.Framework.Comp.Thing;
using Cosmere.Framework.Object;
using RimWorld;
using Verse;

namespace Cosmere.Framework.Thing;

public class ApparelWithStorage : Apparel, IThingHolder, IHaulDestination, IThingHolderTickable {
    public ThingOwner<Verse.Thing> innerContainer;
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

    public bool StorageTabVisible => true;
    public bool HaulDestinationEnabled => StorageTabVisible;

    public new Map Map => base.Map ?? ((Pawn?)holdingOwner?.Owner.ParentHolder)?.Map!;

    public StorageSettings GetStoreSettings() {
        return settings;
    }

    public StorageSettings GetParentStoreSettings() {
        return storageComp.fixedStorageSettings ?? StorageSettings.EverStorableFixedSettings();
    }

    public void Notify_SettingsChanged() { }

    public bool Accepts(Verse.Thing t) {
        int currentInventoryRemaining = storageComp.maxItems - innerContainer.TotalStackCount;

        return currentInventoryRemaining > 0 && GetStoreSettings().AllowedToAccept(t);
    }

    public void GetChildHolders(List<IThingHolder> outChildren) {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public ThingOwner GetDirectlyHeldThings() {
        return innerContainer;
    }

    public bool ShouldTickContents => true;


    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this, storageComp.maxItems);
    }

    public override void PostMake() {
        base.PostMake();
        innerContainer = new ThingOwnerWithCapacity<Verse.Thing>(this, storageComp.maxItems);
        settings = new StorageSettings(this);
        if (storageComp.defaultStorageSettings != null) {
            settings.CopyFrom(storageComp.defaultStorageSettings);
        }
    }

    protected override void Tick() {
        base.Tick();
        //innerContainer.DoTick();
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