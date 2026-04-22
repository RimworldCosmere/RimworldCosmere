using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Memory;

public class Thought_Memory_Coppermind : Thought_Memory {
    public Metalmind? sourceCoppermind;
    public StoredMemory? storedMemory;

    public override bool ShouldDiscard {
        get {
            if (sourceCoppermind?.parent == null) return true;
            if (storedMemory == null) return true;
            if (pawn == null) return true;
            if (!sourceCoppermind.storedMemories.Contains(storedMemory)) return true;
            Pawn? holder = sourceCoppermind.parent.holdingOwner?.Owner as Pawn;
            return holder != pawn;
        }
    }

    public bool IsCoppermindInjected => true;

    public override bool Save => false;

    public override void ThoughtInterval() { }
}
