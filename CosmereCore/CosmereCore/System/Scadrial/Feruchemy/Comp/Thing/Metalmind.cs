using System.Text;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Def;
using Cosmere.Core.DefModExtension;
using Cosmere.System.Scadrial.Feruchemy.Memory;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Feruchemy.Comp.Thing;

public class MetalmindProperties : CompProperties {
    public float maxAmount;

    public MetalmindProperties() {
        compClass = typeof(Metalmind);
    }
}

public class Metalmind : ThingComp, IMetalmindSource {
    private MetalDef? cachedMetal;

    private bool equippedInt = true;

    private float compoundedAmountInt;
    private float storedAmountInt;
    private List<StoredMemory> storedMemoriesInt = [];
    public Pawn? owner { get; private set; }
    private new MetalmindProperties props => (MetalmindProperties)base.props;

    public bool IsCoppermind => Metal?.defName == "Copper";

    public IReadOnlyList<StoredMemory> StoredMemories => storedMemoriesInt;

    public float UsedMemorySpace {
        get {
            float total = 0f;
            for (int i = 0; i < storedMemoriesInt.Count; i++) {
                total += storedMemoriesInt[i].moodMagnitude;
            }

            return total;
        }
    }

    private InvestitureHolder investitureHolder => parent.GetComp<InvestitureHolder>();

    public bool Equipped {
        get => equippedInt;
        set => equippedInt = value;
    }

    public float MaxAmount => props.maxAmount;

    public float TotalStored => storedAmountInt + compoundedAmountInt;
    public float FreeSpace => Mathf.Max(0f, MaxAmount - TotalStored);

    // Both pools draw on the same space, so filling either is bounded by the total.
    public bool CanStore => !IsCoppermind && Equipped && FreeSpace > 0f;
    public bool CanTap => !IsCoppermind && Equipped && StoredAmount > 0f;
    public bool CanTapCompounded => !IsCoppermind && Equipped && CompoundedAmount > 0f;

    // Worn metalminds can hold compounded charge but cannot be compounded into.
    public bool CanStoreCompounded => false;
    public bool IsImplanted => false;

    public float StoredAmount {
        get => storedAmountInt;
        private set {
            storedAmountInt = value;
            SyncInvestitureMirror();
        }
    }

    public float CompoundedAmount {
        get => compoundedAmountInt;
        private set {
            compoundedAmountInt = value;
            SyncInvestitureMirror();
        }
    }

    private void SyncInvestitureMirror() {
        investitureHolder.currentInvestitureSelf =
            TotalStored * ScadrialMetallurgyConstants.BreathEquivalentUnitsPerMetalUnit;
    }

    public MetalDef? Metal {
        get {
            if (cachedMetal != null) return cachedMetal;
            if (parent.def.GetModExtension<MetalsLinked>() is { } metalsLinked) {
                cachedMetal = metalsLinked.Metals.FirstOrDefault();
            }

            if (parent.Stuff is { } stuff) {
                cachedMetal = DefDatabase<MetalDef>.GetNamed(stuff.defName);
            }

            if (cachedMetal != null) return cachedMetal;

            Logger.Error("Metalmind doesn't have a metal");
            return null;
        }
    }

    public void AddStored(float amount) {
        if (!CanStore) return;
        if (!ValidateOwner()) return;

        StoredAmount = Mathf.Clamp(StoredAmount + amount, 0, MaxAmount - CompoundedAmount);
    }

    public void ConsumeStored(float amount) {
        if (!CanTap) return;
        if (!ValidateOwner()) return;

        StoredAmount = Mathf.Clamp(StoredAmount - amount, 0, MaxAmount);
    }

    public void AddCompounded(float amount) {
        if (!CanStore) return;
        if (!ValidateOwner()) return;

        CompoundedAmount = Mathf.Clamp(CompoundedAmount + amount, 0, MaxAmount - StoredAmount);
    }

    public void ConsumeCompounded(float amount) {
        if (!CanTapCompounded) return;
        if (!ValidateOwner()) return;

        CompoundedAmount = Mathf.Clamp(CompoundedAmount - amount, 0, MaxAmount);
    }

    public bool CanFitMemory(float magnitude) {
        return UsedMemorySpace + magnitude <= MaxAmount;
    }

    public void StoreMemory(StoredMemory memory) {
        storedMemoriesInt.Add(memory);
        if (IsCoppermind) {
            storedAmountInt = UsedMemorySpace;
        }
    }

    public StoredMemory? RemoveStoredMemoryAt(int index) {
        if (index < 0 || index >= storedMemoriesInt.Count) return null;
        StoredMemory removed = storedMemoriesInt[index];
        storedMemoriesInt.RemoveAt(index);
        if (IsCoppermind) {
            storedAmountInt = UsedMemorySpace;
        }

        return removed;
    }

    public void SyncInjectedThoughts(Pawn holder) {
        if (!IsCoppermind) return;
        if (holder == null) return;
        if (owner == null) owner = holder;
        if (holder != owner) return;

        MemoryThoughtHandler? handler = holder.needs?.mood?.thoughts?.memories;
        if (handler == null) return;

        for (int i = 0; i < storedMemoriesInt.Count; i++) {
            StoredMemory stored = storedMemoriesInt[i];
            if (stored.def == null) continue;
            if (InjectedThoughtExistsFor(handler, stored)) continue;

            Thought_Memory_Coppermind thought = new Thought_Memory_Coppermind {
                def = stored.def,
                age = stored.age,
                moodPowerFactor = stored.moodPowerFactor,
                otherPawn = stored.otherPawn,
                sourceCoppermind = this,
                storedMemory = stored,
            };
            handler.TryGainMemory(thought);
        }
    }

    private static bool InjectedThoughtExistsFor(MemoryThoughtHandler handler, StoredMemory stored) {
        List<Thought_Memory> all = handler.Memories;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Thought_Memory_Coppermind injected && injected.storedMemory == stored) return true;
        }

        return false;
    }

    public override void PostPostMake() {
        investitureHolder.maxInvestitureSelf = MaxAmount * ScadrialMetallurgyConstants.BreathEquivalentUnitsPerMetalUnit;
    }

    public Pawn? GetHoldingPawn() {
        IThingHolder? holder = parent.ParentHolder;
        while (holder != null) {
            if (holder is Pawn_InventoryTracker inv) return inv.pawn;
            if (holder is Pawn_EquipmentTracker eq) return eq.pawn;
            if (holder is Pawn_ApparelTracker app) return app.pawn;
            if (holder is Pawn p) return p;
            holder = holder.ParentHolder;
        }

        return null;
    }

    private bool ValidateOwner() {
        Pawn? currentHolder = GetHoldingPawn();
        if (owner == null) {
            owner = currentHolder;
            return true;
        }

        return currentHolder == null || owner.Equals(currentHolder);
    }

    public override void PostExposeData() {
        base.PostExposeData();

        Scribe_Values.Look(ref storedAmountInt, "storedAmount");
        Scribe_Values.Look(ref compoundedAmountInt, "compoundedAmount", 0f);
        Scribe_Values.Look(ref equippedInt, "equipped");
        Scribe_Collections.Look(ref storedMemoriesInt, "StoredMemories", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            cachedMetal = null;
            if (parent?.Stuff != null) {
                cachedMetal = DefDatabase<MetalDef>.GetNamedSilentFail(parent.Stuff.defName);
            }

            owner = GetHoldingPawn() ?? owner;
            storedMemoriesInt ??= [];

            // A save written when this metalmind held more capacity would load
            // over-full once both pools are counted.
            if (storedAmountInt + compoundedAmountInt > MaxAmount) {
                compoundedAmountInt = Mathf.Max(0f, MaxAmount - storedAmountInt);
            }

            SyncInvestitureMirror();
        }
    }

    public override string CompInspectStringExtra() {
        StringBuilder sb = new StringBuilder();
        TaggedString coloredOwner = owner?.NameFullColored ?? "None".Colorize(ColoredText.DateTimeColor);
        sb.AppendLine("CS_MetalmindOwner".Translate() + ": " + coloredOwner);
        NamedArgument coloredMetal = Metal?.coloredLabel.Named("METAL") ?? "unknown".Named("METAL");
        sb.Append("CS_MetalmindStored".Translate(coloredMetal) + $": {TotalStored:F1} / {MaxAmount}");
        if (compoundedAmountInt > 0f) {
            sb.Append(" " + "CS_MetalmindCompounded".Translate(compoundedAmountInt.ToString("F1").Named("COMPOUNDED")));
        }

        return sb.ToString();
    }
}