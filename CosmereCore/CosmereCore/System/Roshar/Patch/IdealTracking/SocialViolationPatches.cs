using Concord;
using Cosmere.System.Roshar.Surgebinding;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class SocialFightViolationPatch : Pawn_InteractionsTracker {
    [InjectField("pawn")]
    private readonly Pawn initiator = null!;

    protected SocialFightViolationPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, nameof(StartSocialFight))]
    private void AfterStartSocialFight() {
        if (initiator == null) return;

        if (ViolationUtility.IsSurgebinderOfOrder(initiator, RadiantOrderDefOf.Skybreaker)) {
            ViolationUtility.ApplyViolation(initiator, 0.3f, "starting a social fight");
        }

        if (ViolationUtility.IsSurgebinderOfOrder(initiator, RadiantOrderDefOf.Bondsmith)) {
            ViolationUtility.ApplyViolation(initiator, 0.1f, "starting a social fight");
        }
    }
}

[Patch]
public abstract class FactionGoodwillViolationPatch : Faction {
    // The pre-call goodwill is only readable before the change lands.
    [Inject(At.Around, nameof(TryAffectGoodwillWith))]
    private bool AroundTryAffectGoodwillWith(
        Faction other,
        int goodwillChange,
        bool canSendMessage,
        bool canSendHostilityLetter,
        HistoryEventDef reason,
        RimWorld.Planet.GlobalTargetInfo? lookTarget,
        Operation<Faction, int, bool, bool, HistoryEventDef, RimWorld.Planet.GlobalTargetInfo?, bool> original
    ) {
        Faction self = this;
        bool tracked = self.IsPlayer;
        int previousGoodwill = tracked ? self.GoodwillWith(other) : 0;

        bool result = original.Invoke(
            other,
            goodwillChange,
            canSendMessage,
            canSendHostilityLetter,
            reason,
            lookTarget
        );

        if (!result || !tracked) return result;
        if (goodwillChange >= 0) return result;

        int currentGoodwill = self.GoodwillWith(other);
        bool wentHostile = previousGoodwill >= 0 && currentGoodwill < 0;
        if (!wentHostile) return result;

        List<Pawn> colonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            if (ViolationUtility.IsSurgebinderOfOrder(colonist, RadiantOrderDefOf.Bondsmith)) {
                ViolationUtility.ApplyViolation(colonist, 0.3f, "faction turned hostile");
            }
        }

        return result;
    }
}
