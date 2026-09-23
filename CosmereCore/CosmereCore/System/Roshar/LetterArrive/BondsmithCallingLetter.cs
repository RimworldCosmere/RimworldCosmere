using Cosmere.Core.Quest;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using Verse;

namespace Cosmere.System.Roshar.LetterArrive;

public class BondsmithCallingLetter : ChoiceLetter {
    private string sprenName = string.Empty;

    public override bool ShouldAutomaticallyOpenLetter => true;

    public override IEnumerable<DiaOption> Choices {
        get {
            yield return new DiaOption("CRO_Bondsmith_Accept".Translate()) {
                action = () => {
                    Pawn pawn = lookTargets.PrimaryTarget.Pawn;
                    Find.LetterStack.RemoveLetter(this);
                    if (TryStartBondQuest(pawn)) return;

                    Find.WindowStack.Add(new Dialog_ChooseRadiantOrder(pawn, sprenName));
                },
                resolveTree = true,
            };

            yield return new DiaOption("CRO_Bondsmith_NotYet".Translate()) {
                action = () => { Find.LetterStack.RemoveLetter(this); },
                resolveTree = true,
            };

            yield return new DiaOption("CRO_Bondsmith_Refuse".Translate()) {
                action = () => {
                    Pawn pawn = lookTargets.PrimaryTarget.Pawn;
                    BondsmithCalling? hediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(
                        DefDatabase<HediffDef>.GetNamedSilentFail($"Cosmere_Roshar_Hediff_BondsmithCalling_{sprenName}")
                    ) as BondsmithCalling;
                    if (hediff != null) {
                        pawn.health!.RemoveHediff(hediff);
                    }

                    BondsmithCallingChecker? checker =
                        Current.Game?.GetComponent<BondsmithCallingChecker>();
                    checker?.RecordRefusal(pawn);

                    Find.LetterStack.RemoveLetter(this);
                },
                resolveTree = true,
            };

            if (lookTargets.IsValid()) {
                yield return Option_JumpToLocationAndPostpone;
            }
        }
    }

    public void Setup(string spren) {
        sprenName = spren;
    }

    /// <summary>
    ///     The Stormfather tests rather than hands the bond over. A godspren with no capstone
    ///     def yet keeps the old dialog, which is still the ordinary spren bond path.
    /// </summary>
    private bool TryStartBondQuest(Pawn pawn) {
        CosmereQuestDef? def =
            DefDatabase<CosmereQuestDef>.GetNamedSilentFail($"Cosmere_Roshar_Quest_Bond{sprenName}");
        Verse.Map? map = pawn.MapHeld;
        if (def == null || map == null) return false;

        CosmereQuestManager? manager = Current.Game?.GetComponent<CosmereQuestManager>();

        return manager?.TryStartCapstone(def, map, pawn) ?? false;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref sprenName, "sprenName", string.Empty);
    }
}
