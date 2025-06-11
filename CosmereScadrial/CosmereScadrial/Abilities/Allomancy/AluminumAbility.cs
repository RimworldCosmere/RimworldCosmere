using System.Linq;
using CosmereScadrial.Comps.Things;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy {
    public class AluminumAbility : AbilitySelfTarget {
        public AluminumAbility() { }

        public AluminumAbility(Pawn pawn) : base(pawn) { }

        public AluminumAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

        public AluminumAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
            target = pawn;
        }

        public AluminumAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
            target = pawn;
        }

        public override void AbilityTick() {
            base.AbilityTick();

            if (status == BurningStatus.Off) return;

            var reserves = pawn.GetComp<MetalReserves>();
            foreach (var metalToWipe in reserves.GetAllAvailableMetals().ToList()
                         .Where(x => !x.Equals(MetallicArtsMetalDefOf.Aluminum))) {
                reserves.RemoveReserve(metalToWipe);
                Messages.Message(
                    $"{pawn.NameFullColored} is burning Aluminum and has wiped his reserves of {metalToWipe.LabelCap}.",
                    MessageTypeDefOf.NeutralEvent,
                    false
                );
            }
        }
    }
}