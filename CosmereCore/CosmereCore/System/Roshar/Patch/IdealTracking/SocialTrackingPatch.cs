using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class SocialTrackingPatch : Pawn_InteractionsTracker {
    [InjectField("pawn")]
    private readonly Pawn initiator = null!;

    protected SocialTrackingPatch(Pawn pawn) : base(pawn) { }

    // The opinion delta needs a reading from both sides of the interaction.
    [Inject(At.Around, nameof(TryInteractWith))]
    private bool AroundTryInteractWith(
        Pawn? recipient,
        InteractionDef intDef,
        Operation<Pawn?, InteractionDef, bool> original
    ) {
        Surgebinder? surgebinder = initiator.genes?.GetFirstGeneOfType<Surgebinder>();
        bool tracked = surgebinder != null && recipient?.relations != null;
        int oldOpinion = tracked ? recipient!.relations.OpinionOf(initiator) : 0;

        bool result = original.Invoke(recipient, intDef);

        if (!result || !tracked) return result;

        int newOpinion = recipient!.relations.OpinionOf(initiator);
        if (newOpinion > oldOpinion) {
            initiator.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_SocialHealing, 1);
        }

        if (oldOpinion < 20 && newOpinion >= 20) {
            initiator.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_FriendshipsFormed, 1);
        }

        return result;
    }
}
