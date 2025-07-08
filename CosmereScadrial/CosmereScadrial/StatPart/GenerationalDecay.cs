using System;
using CosmereScadrial.Extension;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.StatPart;

public class GenerationalDecay : RimWorld.StatPart {
    public bool allomancy = false;
    public float decayPerGeneration = 0.25f;
    public bool feruchemy = false;

    public override void TransformValue(StatRequest req, ref float val) {
        if (!req.HasThing || req.Thing is not Pawn pawn)
            return;

        float maternalFactor = GetInheritanceFactor(pawn.GetMother());
        float paternalFactor = GetInheritanceFactor(pawn.GetFather());

        val *= Mathf.Clamp01(maternalFactor + paternalFactor);
    }

    public override string ExplanationPart(StatRequest req) {
        if (!req.HasThing || req.Thing is not Pawn pawn)
            return null;

        float maternal = GetInheritanceFactor(pawn.GetMother());
        float paternal = GetInheritanceFactor(pawn.GetFather());
        float total = Mathf.Clamp01(maternal + paternal);

        return $"Inherited Power: Maternal x{maternal:0.##}, Paternal x{paternal:0.##} → Total x{total:0.##}";
    }

    private float GetInheritanceFactor(Pawn? parent) {
        if (parent == null)
            return 0f;

        int? generation = FindGeneration(parent, IsRelevantMetalborn, 0);
        if (generation == null)
            return 0f;

        return Mathf.Pow(decayPerGeneration, generation.Value);
    }

    private int? FindGeneration(Pawn? pawn, Predicate<Pawn> match, int depth) {
        if (pawn == null || depth > 10)
            return null;

        if (match(pawn))
            return depth;

        int? fromMother = FindGeneration(pawn.GetMother(), match, depth + 1);
        int? fromFather = FindGeneration(pawn.GetFather(), match, depth + 1);

        if (fromMother.HasValue && fromFather.HasValue)
            return Mathf.Min(fromMother.Value, fromFather.Value);
        return fromMother ?? fromFather;
    }

    // TODO: Override this in Allomantic/Feruchemical-specific subclasses if needed
    protected virtual bool IsRelevantMetalborn(Pawn p) {
        return allomancy && p.IsAllomancer() || feruchemy && p.IsFeruchemist();
    }
}