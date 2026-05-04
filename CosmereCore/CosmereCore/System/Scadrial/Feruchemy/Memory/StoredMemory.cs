using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Memory;

public class StoredMemory : IExposable {
    public int age;
    public ThoughtDef? def;
    public float moodOffset;
    public float moodPowerFactor = 1f;
    public Pawn? otherPawn;
    public Pawn? owner;

    public StoredMemory() { }

    public StoredMemory(Thought_Memory thought, Pawn ownerPawn) {
        def = thought.def;
        age = thought.age;
        moodPowerFactor = thought.moodPowerFactor;
        otherPawn = thought.otherPawn;
        owner = ownerPawn;
        moodOffset = thought.MoodOffset();
    }

    public float moodMagnitude => Mathf.Abs(moodOffset);

    public bool isPositive => moodOffset >= 0f;

    public string labelCap {
        get {
            if (def == null) return string.Empty;
            string label = def.LabelCap;
            return otherPawn != null ? $"{label} ({otherPawn.LabelShortCap})" : label;
        }
    }

    public void ExposeData() {
        Scribe_Defs.Look(ref def, "def");
        Scribe_Values.Look(ref age, "age");
        Scribe_Values.Look(ref moodPowerFactor, "moodPowerFactor", 1f);
        Scribe_References.Look(ref otherPawn, "otherPawn", true);
        Scribe_References.Look(ref owner, "owner", true);
        Scribe_Values.Look(ref moodOffset, "moodOffset");
    }
}