using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy {
    public class DuraluminAbility : AbilitySelfTarget {
        public DuraluminAbility() { }

        public DuraluminAbility(Pawn pawn) : base(pawn) { }

        public DuraluminAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

        public DuraluminAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
            target = pawn;
        }

        public DuraluminAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
            target = pawn;
        }

        public void QueueCastingJob(LocalTargetInfo target, LocalTargetInfo destination, bool flare) {
            shouldFlare = false;
            base.QueueCastingJob(target, destination);
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
            var result = base.Activate(target, dest);
            shouldFlare = false;

            return result;
        }
    }
}