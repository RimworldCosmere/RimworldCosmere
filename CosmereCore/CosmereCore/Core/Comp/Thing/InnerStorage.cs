using System.Runtime.CompilerServices;
using Cosmere.Core.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Comp.Thing;

public class InnerStorageProperties : CompProperties {
    public StorageSettings? defaultStorageSettings;
    public bool dropItemsWhenDeconstructed = true;
    public bool dropItemsWhenDestroyed = false;
    public StorageSettings? fixedStorageSettings;

    /// <summary>How many individual items fit, counted across every stack. -1 for no limit.</summary>
    public int maxItems = -1;

    /// <summary>
    ///     How many separate stacks fit, regardless of how full each one is. -1 for no limit.
    /// </summary>
    /// <remarks>
    ///     Set this when a container is meant to hold "a few stacks of gems" rather than a headcount.
    ///     <see cref="maxItems" /> counts gems, so a charger meant to take 30 stacks of a 200-stacking
    ///     gem would need 6000 there, which nobody guesses right.
    /// </remarks>
    public int maxStacks = -1;

    public bool oneStackOnly = false;

    /// <summary>
    ///     Swaps the vanilla hit points slider for an Investiture band, and honours that band when
    ///     deciding what may be stored.
    /// </summary>
    public bool investitureThreshold = false;

    /// <summary>What to call the band in the tab. Shards name the same thing differently.</summary>
    [MustTranslate]
    public string thresholdLabel = "CC_Storage_InvestitureBand";

    [MustTranslate]
    public string tabName = null!;

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
    private const int EjectCheckInterval = 250;

    /// <summary>
    ///     Which storage each filter belongs to, for the patch that draws the band.
    /// </summary>
    /// <remarks>
    ///     A ThingFilter has no way back to its owner, and the vanilla method we replace is handed
    ///     only the filter. Weak so a destroyed container does not keep its settings alive.
    /// </remarks>
    private static readonly ConditionalWeakTable<ThingFilter, InnerStorage> BandByFilter = new();

    public ThingOwnerWithCapacity<Verse.Thing> innerContainer = null!;

    /// <summary>The share of a thing's Investiture this container will take, 0 to 1.</summary>
    public FloatRange allowedInvestiture = new FloatRange(0f, 1f);

    /// <summary>Whether to put back anything that drifts out of <see cref="allowedInvestiture" />.</summary>
    public bool ejectOutOfRange;

    private StorageSettings? settings;

    public new InnerStorageProperties props => (InnerStorageProperties)base.props;

    public Pawn? ParentPawn => ParentThing as Pawn;

    public ThingWithComps? ParentThing => parent is Apparel apparel ? apparel.Wearer : parent;

    public Verse.Map? Map => parent.MapHeld;

    public bool StorageTabVisible => true;

    /// <summary>
    ///     Whether the colony's haul search may pick this storage as a destination.
    /// </summary>
    /// <remarks>
    ///     Worn storage is its wearer's business. Vanilla's storage search has no idea who a
    ///     container belongs to, so a worn sphere pouch used to be offered to every hauler on the
    ///     map. The pawn would take the job, <c>PawnHaulThingToInnerStorage</c> would refuse it
    ///     because the pouch belongs to somebody else, and the null job left the hauler standing
    ///     there re-picking the same destination and filling the log with "Don't know how to handle
    ///     HaulToStorageJob".
    ///     <para>
    ///         A pouch on the ground has no wearer, so it stays haulable.
    ///     </para>
    /// </remarks>
    public bool HaulDestinationEnabled => StorageTabVisible && ParentPawn == null;

    public StorageSettings GetStoreSettings() {
        return settings!;
    }

    public StorageSettings GetParentStoreSettings() {
        return props.fixedStorageSettings ?? StorageSettings.EverStorableFixedSettings();
    }

    public void Notify_SettingsChanged() { }

    public bool Accepts(Verse.Thing t) {
        if (!innerContainer.CanAcceptAnyOf(t)) return false;
        if (!GetStoreSettings().AllowedToAccept(t)) return false;
        if (props.maxItems >= 0 && innerContainer.TotalStackCount >= props.maxItems) return false;
        if (!WithinInvestitureBand(t)) return false;

        return !AtStackLimit(t);
    }

    /// <summary>
    ///     Finds the storage a filter belongs to, when that storage draws an Investiture band.
    /// </summary>
    /// <param name="filter">The filter vanilla is drawing.</param>
    /// <returns>The owning storage, or null when the filter is not one of ours.</returns>
    public static InnerStorage? BandOwnerOf(ThingFilter filter) {
        return BandByFilter.TryGetValue(filter, out InnerStorage owner) ? owner : null;
    }

    /// <summary>
    ///     Whether a thing's charge sits inside the band this container accepts.
    /// </summary>
    /// <param name="t">The thing being offered.</param>
    /// <returns>True when the band allows it, or when no band is configured.</returns>
    private bool WithinInvestitureBand(Verse.Thing t) {
        if (!props.investitureThreshold) return true;
        if (!t.TryGetComp(out InvestitureHolder held)) return true;

        return InvestitureRange.Within(
            held.currentInvestitureSelf,
            held.maxInvestitureSelf,
            allowedInvestiture.min,
            allowedInvestiture.max
        );
    }

    /// <summary>
    ///     Puts back anything that has drifted outside the band since it was stored.
    /// </summary>
    /// <remarks>
    ///     Spheres drain, so a stored one crosses the bottom edge on its own given time. Only runs
    ///     when the player asks for it, because dropping a wearer's spheres mid-walk is a surprise
    ///     nobody wants by default.
    /// </remarks>
    private void EjectDrifted() {
        if (!ejectOutOfRange || !props.investitureThreshold) return;
        if (ParentThing is not { Spawned: true } holder) return;

        for (int i = innerContainer.Count - 1; i >= 0; i--) {
            Verse.Thing held = innerContainer[i];
            if (WithinInvestitureBand(held)) continue;

            innerContainer.TryDrop(held, holder.Position, holder.Map, ThingPlaceMode.Near, held.stackCount, out _);
        }
    }

    /// <summary>
    ///     Whether another stack would not fit.
    /// </summary>
    /// <remarks>
    ///     Topping up a stack that is already in here does not add one, so a full container still
    ///     takes more of what it is already holding.
    /// </remarks>
    /// <param name="t">The thing being offered.</param>
    /// <returns>True when the stack count is spent and nothing here can absorb it.</returns>
    private bool AtStackLimit(Verse.Thing t) {
        if (props.maxStacks < 0) return false;
        if (innerContainer.Count < props.maxStacks) return false;

        for (int i = 0; i < innerContainer.Count; i++) {
            Verse.Thing held = innerContainer[i];
            if (held.CanStackWith(t) && held.stackCount < held.def.stackLimit) return false;
        }

        return true;
    }

    public IntVec3 Position =>
        parent is not Apparel apparel || apparel.Wearer is not { } wearer
            ? parent.SpawnedParentOrMe.Position
            : wearer.SpawnedParentOrMe.Position;

    public int SpaceRemainingFor(ThingDef _) {
        return ItemCapacity - innerContainer.TotalStackCount;
    }

    /// <summary>
    ///     The item budget the container itself is built with.
    /// </summary>
    /// <remarks>
    ///     A def that sets only <c>maxStacks</c> has no headcount to give, and the container needs
    ///     one, so it gets an effectively unbounded budget and lets the stack limit do the work.
    /// </remarks>
    private int ItemCapacity => props.maxItems >= 0 ? props.maxItems : int.MaxValue;

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
        return innerContainer;
    }

    public override void CompTick() {
        innerContainer?.DoTick();

        if (GenTicks.IsTickInterval(EjectCheckInterval)) EjectDrifted();
    }

    public override void PostDestroy(DestroyMode mode, Verse.Map? previousMap) {
        base.PostDestroy(mode, previousMap);
        previousMap?.haulDestinationManager?.RemoveHaulDestination(this);
        if (mode == DestroyMode.Deconstruct && props.dropItemsWhenDeconstructed) {
            foreach (Verse.Thing thing in innerContainer.ToList()) {
                innerContainer.TryDrop(
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
        innerContainer = new ThingOwnerWithCapacity<Verse.Thing>(this, ItemCapacity, props.oneStackOnly);

        settings = new StorageSettings(this);
        if (props.defaultStorageSettings != null) {
            settings.CopyFrom(props.defaultStorageSettings);
        }

        // the band draws in the hit points slider's spot; vanilla skips this method if it isn't configurable.
        if (props.investitureThreshold) BandByFilter.Remove(settings.filter);
        if (props.investitureThreshold) BandByFilter.Add(settings.filter, this);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        AddHaulDestination();
    }

    public override void PostExposeData() {
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this, ItemCapacity, props.oneStackOnly);
        Scribe_Deep.Look(ref settings, "storageSettings", this);
        Scribe_Values.Look(ref allowedInvestiture, "allowedInvestiture", new FloatRange(0f, 1f));
        Scribe_Values.Look(ref ejectOutOfRange, "ejectOutOfRange");
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
