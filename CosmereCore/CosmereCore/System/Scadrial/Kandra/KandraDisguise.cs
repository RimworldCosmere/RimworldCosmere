using System.Collections.Generic;
using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Extension;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     Who can tell that a kandra is not what it looks like.
/// </summary>
/// <remarks>
///     A kandra wearing somebody is meant to be convincing, and mostly is. Two things see
///     through it. Bronze reads the Investiture holding the shape together and does not care how
///     good the disguise is, which is why kandra fear Seekers. Everyone else gets a look, and
///     whether they notice depends on how practised the kandra is at wearing a face.
/// </remarks>
public static class KandraDisguise {
    /// <summary>How often an unaided observer gets a chance to notice. Once a game hour.</summary>
    public const int LookInterval = 2500;

    /// <summary>Chance per look at zero skill. Practice takes this to nothing.</summary>
    public const float BaseSuspicion = 0.08f;

    /// <summary>Whether this pawn is currently pretending to be something else.</summary>
    public static bool IsDisguised(Pawn pawn) {
        if (pawn.TryGetComp<CompKandraShapePair>()?.Held != null) return true;

        return pawn.TryGetComp<CompKandraForms>()?.IsWearingSomeoneElse == true;
    }

    /// <summary>The kandra behind the shape, whichever way it is wearing one.</summary>
    public static Pawn? Behind(Pawn pawn) {
        Pawn? inside = pawn.TryGetComp<CompKandraShapePair>()?.Held;
        if (inside != null) return inside;

        return IsDisguised(pawn) ? pawn : null;
    }

    /// <summary>
    ///     Bronze does not guess. Anyone burning it near a kandra knows.
    /// </summary>
    /// <remarks>
    ///     Deliberately not a roll. A Seeker reads the Investiture directly, so a first-generation
    ///     kandra with centuries of practice is exactly as visible as one made last week. That is
    ///     the point of bronze, and the reason a kandra will not stay in a room with one.
    /// </remarks>
    public static bool SeenByBronze(Pawn disguised) {
        if (!disguised.Spawned) return false;
        if (!IsDisguised(disguised)) return false;

        MetalDef bronze = MetalDefOf.Bronze;
        if (bronze == null) return false;

        IReadOnlyList<Pawn> nearby = disguised.Map?.mapPawns?.AllPawnsSpawned ?? [];
        for (int i = 0; i < nearby.Count; i++) {
            Pawn seeker = nearby[i];
            if (seeker == disguised || seeker.Dead) continue;

            // Animals have no gene tracker at all, and IsBurning reaches straight into it.
            if (seeker.genes == null) continue;
            if (!seeker.IsBurning(bronze)) continue;
            if (!seeker.Position.InHorDistOf(disguised.Position, BronzeRange(seeker))) continue;

            return true;
        }

        return false;
    }

    /// <summary>Further with more practice at burning it, the way every other metal works.</summary>
    private static float BronzeRange(Pawn seeker) {
        int skill = seeker.skills?.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower)?.Level ?? 0;

        return 12f + (skill * 1.5f);
    }

    /// <summary>
    ///     Whether an ordinary observer notices something wrong, this look.
    /// </summary>
    /// <remarks>
    ///     Conviction runs 0 to 1 off the Shapeshift skill, so a practised kandra is effectively
    ///     never caught by eye alone and a new one is caught often enough to matter. Bronze is
    ///     handled separately and ignores all of this.
    /// </remarks>
    public static bool Slipped(Pawn disguised) {
        CompKandraForms? forms = Behind(disguised)?.TryGetComp<CompKandraForms>();
        if (forms == null) return false;

        return Rand.Chance(BaseSuspicion * (1f - forms.Conviction));
    }

    /// <summary>
    ///     Whether a disguise stands between these two, so neither should treat the other as a
    ///     threat.
    /// </summary>
    /// <remarks>
    ///     This is what the whole thing is for. A kandra wearing an obligator walks through a
    ///     Steel Ministry raid, and one wearing a wolfhound is just another animal on the map. The
    ///     shape has to be worth wearing before anybody will bother wearing it.
    /// </remarks>
    public static bool Fools(Verse.Thing a, Verse.Thing b) {
        return FoolsOneWay(a, b) || FoolsOneWay(b, a);
    }

    /// <summary>
    ///     Whether <paramref name="observer" /> is taken in by <paramref name="disguised" />.
    /// </summary>
    /// <remarks>
    ///     Once the kandra swings at somebody the shape stops working for good, whatever face is
    ///     on. Anything else would let a player wear an enemy uniform and shoot the whole raid
    ///     while it walked past.
    /// </remarks>
    private static bool FoolsOneWay(Verse.Thing disguised, Verse.Thing observer) {
        if (disguised is not Pawn pawn) return false;

        Pawn? kandra = Behind(pawn);
        CompKandraForms? forms = kandra?.TryGetComp<CompKandraForms>();
        if (forms is not { IsWearingSomeoneElse: true }) return false;
        if (forms.CoverBlown) return false;

        if (forms.FoughtInThisShape(pawn)) {
            forms.BlowCover();
            return false;
        }

        // Wildlife and unfactioned things work out hostility their own way, and a kandra wearing
        // a face has nothing to say to a manhunting boomrat.
        Faction? theirs = observer.Faction;
        if (theirs == null) return false;

        // An animal shape belongs to nobody, which is exactly why it is worth wearing.
        FactionDef? worn = forms.Current?.faction;
        if (worn == null) return true;

        Faction? live = Find.FactionManager?.FirstFactionOfDef(worn);

        return live == null || !live.HostileTo(theirs);
    }
}
