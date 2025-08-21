using Cosmere.Foundation.Object;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Foundation.Comp.Thing;

public class InnerStorageProperties : CompProperties {
    public StorageSettings? defaultStorageSettings;
    public bool dropItemsWhenDeconstructed = true;
    public bool dropItemsWhenDestroyed = false;
    public StorageSettings? fixedStorageSettings;
    public int maxItems = -1;
    public bool oneStackOnly = false;

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

public class InnerStorage : ThingComp, IHaulDestination, IThingHolderTickable, IHaulEnroute,
    IThingHolderEvents<Verse.Thing> {
    public ThingOwnerWithCapacity<Verse.Thing> innerContainer;
    private StorageSettings? settings;

    public new InnerStorageProperties props => (InnerStorageProperties)base.props;

    public Pawn? ParentPawn => ParentThing as Pawn;

    public ThingWithComps? ParentThing => parent is Apparel apparel ? apparel.Wearer : parent;

    public Verse.Map? Map => parent.MapHeld;

    public bool StorageTabVisible => true;

    public bool HaulDestinationEnabled => StorageTabVisible;

    public StorageSettings GetStoreSettings() {
        return settings!;
    }

    public StorageSettings GetParentStoreSettings() {
        return props.fixedStorageSettings ?? StorageSettings.EverStorableFixedSettings();
    }

    public void Notify_SettingsChanged() { }

    public bool Accepts(Verse.Thing t) {
        int currentInventoryRemaining = props.maxItems - innerContainer!.TotalStackCount;

        return innerContainer!.CanAcceptAnyOf(t) &&
               currentInventoryRemaining > 0 &&
               GetStoreSettings().AllowedToAccept(t);
    }

    public IntVec3 Position =>
        parent is not Apparel apparel || apparel.Wearer is not { } wearer
            ? parent.SpawnedParentOrMe.Position
            : wearer.SpawnedParentOrMe.Position;

    public int SpaceRemainingFor(ThingDef _) {
        return props.maxItems - innerContainer!.TotalStackCount;
    }

    public string GetUniqueLoadID() {
        return parent.GetUniqueLoadID();
    }

    public void Notify_ItemAdded(Verse.Thing item) {
        parent.BroadcastCompSignal("Cosmere_InnerStorage_Added");
        parent.BroadcastCompSignal("Cosmere_InnerStorage_Changed");
    }

    public void Notify_ItemRemoved(Verse.Thing item) {
        parent.BroadcastCompSignal("Cosmere_InnerStorage_Removed");
        parent.BroadcastCompSignal("Cosmere_InnerStorage_Changed");
    }

    public bool ShouldTickContents => true;

    public void GetChildHolders(List<IThingHolder> outChildren) {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public ThingOwner GetDirectlyHeldThings() {
        return innerContainer!;
    }

    public override void CompTick() {
        innerContainer?.DoTick();
    }

    public override void PostDestroy(DestroyMode mode, Verse.Map? previousMap) {
        base.PostDestroy(mode, previousMap);
        previousMap?.haulDestinationManager?.RemoveHaulDestination(this);
        if (mode == DestroyMode.Deconstruct && props.dropItemsWhenDeconstructed) {
            foreach (Verse.Thing thing in innerContainer.ToList()) {
                innerContainer!.TryDrop(
                    thing,
                    ParentThing!.Position,
                    previousMap,
                    ThingPlaceMode.Near,
                    thing.stackCount,
                    out _
                );
            }
        }

        innerContainer.ClearAndDestroyContents(mode);
    }

    public override void Initialize(CompProperties originalProps) {
        base.Initialize(originalProps);
        parent.DoTick();
        // Ticker type HAS to be normal for the parent
        parent.def.tickerType = TickerType.Normal;
        innerContainer = new ThingOwnerWithCapacity<Verse.Thing>(this, props.maxItems, props.oneStackOnly);

        settings = new StorageSettings(this);
        if (props.defaultStorageSettings != null) {
            settings.CopyFrom(props.defaultStorageSettings);
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        AddHaulDestination();
    }

    public override void PostExposeData() {
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this, props.maxItems, props.oneStackOnly);
        Scribe_Deep.Look(ref settings, "storageSettings", this);
    }

    public override void Notify_RecipeProduced(Pawn pawn) {
        base.Notify_RecipeProduced(pawn);
        if (parent.def.CanHaveFaction) parent.SetFactionDirect(pawn.Faction);
    }


    public override void Notify_Equipped(Pawn pawn) {
        base.Notify_Equipped(pawn);
        if (parent.def.CanHaveFaction) parent.SetFactionDirect(pawn.Faction);

        AddHaulDestination();
    }


    public int GetSpaceRemainingWithEnroute(ThingDef stuff, Pawn? excludeEnrouteFor = null) {
        if (Map == null) return 0;

        int enroute1 = Map.enrouteManager.GetEnroute(this, stuff, excludeEnrouteFor);
        return Mathf.Max(SpaceRemainingFor(stuff) - enroute1, 0);
    }

    public int GetCountCanAccept(Verse.Thing thing) {
        return innerContainer?.GetCountCanAccept(thing) ?? 0;
    }

    public static explicit operator Verse.Thing(InnerStorage storage) {
        return storage.parent;
    }

    public static implicit operator InnerStorage?(Verse.Thing thing) {
        return thing.TryGetComp<InnerStorage>();
    }

    private void AddHaulDestination() {
        if (Map != null && !Map.haulDestinationManager.AllHaulDestinations.Contains(this)) {
            Map.haulDestinationManager.AddHaulDestination(this);
        }
    }
}