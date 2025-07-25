using Cosmere.Framework.InspectorTab;
using Cosmere.Framework.Object;
using RimWorld;
using Verse;

namespace Cosmere.Framework.Comp.Thing;

public class InnerStorageProperties : CompProperties {
    public StorageSettings? defaultStorageSettings;
    public StorageSettings? fixedStorageSettings;
    public int maxItems = -1;

    [MustTranslate]
    public string tabName;

    public InnerStorageProperties() {
        compClass = typeof(InnerStorage);
    }

    public override void ResolveReferences(ThingDef parentDef) {
        base.ResolveReferences(parentDef);
        defaultStorageSettings?.filter.ResolveReferences();
        fixedStorageSettings?.filter.ResolveReferences();
    }
}

public class InnerStorage : ThingComp, IHaulDestination, IThingHolderTickable {
    public ThingOwner<Verse.Thing> innerContainer;
    private StorageSettings settings;
    private new InnerStorageProperties props => (InnerStorageProperties)base.props;

    public Verse.Map Map => parent.Map ?? ((Pawn?)parent.holdingOwner?.Owner.ParentHolder)?.Map!;
    public bool StorageTabVisible => true;
    public bool HaulDestinationEnabled => StorageTabVisible;

    public StorageSettings GetStoreSettings() {
        return settings;
    }

    public StorageSettings GetParentStoreSettings() {
        return props.fixedStorageSettings ?? StorageSettings.EverStorableFixedSettings();
    }

    public void Notify_SettingsChanged() { }

    public bool Accepts(Verse.Thing t) {
        int currentInventoryRemaining = props.maxItems - innerContainer.TotalStackCount;

        return currentInventoryRemaining > 0 && GetStoreSettings().AllowedToAccept(t);
    }

    public IntVec3 Position { get; }
    public bool ShouldTickContents => true;

    public void GetChildHolders(List<IThingHolder> outChildren) {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public ThingOwner GetDirectlyHeldThings() {
        return innerContainer;
    }

    public override void Initialize(CompProperties originalProps) {
        base.Initialize(originalProps);
        innerContainer = new ThingOwnerWithCapacity<Verse.Thing>(this, props.maxItems);
        settings = new StorageSettings(this);
        if (props.defaultStorageSettings != null) {
            settings.CopyFrom(props.defaultStorageSettings);
        }
    }

    public override void PostExposeData() {
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this, props.maxItems);
    }

    public override void Notify_RecipeProduced(Pawn pawn) {
        base.Notify_RecipeProduced(pawn);
        parent.SetFactionDirect(pawn.Faction);
    }

    public override void Notify_Unequipped(Pawn pawn) {
        base.Notify_Unequipped(pawn);
        pawn.def.inspectorTabsResolved.RemoveWhere(x => x is StorageWithInventory);
    }

    public override void Notify_Equipped(Pawn pawn) {
        base.Notify_Equipped(pawn);
        parent.SetFactionDirect(pawn.Faction);
        pawn.def.inspectorTabsResolved.AddUnique(new StorageWithInventory(props.tabName));
        if (!Map.haulDestinationManager.AllHaulDestinations.Contains(this)) {
            Map.haulDestinationManager.AddHaulDestination(this);
        }
    }
}