using RimWorld;

namespace CosmereFramework.Extension;

public static class Pawn_StoryTrackerExtension {
    public static Trait TryAddTrait(this Pawn_StoryTracker story, TraitDef traitDef, int degree = 0,
        bool forced = false) {
        Trait trait;
        if (story.traits.HasTrait(traitDef)) {
            trait = story.traits.GetTrait(traitDef);
            if (trait.Degree == degree) return trait;
            story.traits.RemoveTrait(trait);
        }

        trait = new Trait(traitDef, degree, forced);
        story.traits.GainTrait(trait);

        return trait;
    }
}