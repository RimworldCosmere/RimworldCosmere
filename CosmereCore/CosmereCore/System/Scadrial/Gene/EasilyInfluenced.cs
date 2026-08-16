using System.Collections.Generic;
using Cosmere.System.Scadrial.Comp.Game;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

/// <summary>
///     A koloss does what the last voice in its head told it to, and turns on the world otherwise.
/// </summary>
/// <remarks>
///     The gene is the koloss end of the hold. The Allomancer end is the roster; this half watches
///     it, gives the player a way to see and break one bond, and starts the bloodlust when nobody
///     is holding this thing any more.
///     <para>
///         A grace window sits between losing the hold and turning, because the alternative is a
///         koloss that goes murderous in the one tick between a holder running dry and the player
///         noticing. Six hundred ticks is long enough to put another Soother on it.
///     </para>
/// </remarks>
public class EasilyInfluenced : Verse.Gene {
    private const int PollInterval = 250;

    /// <summary>Ticks this koloss has been loose. Negative means it has already turned.</summary>
    private int looseFor;

    public Pawn? Holder => KolossRoster.Current?.HolderOf(pawn);

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref looseFor, "looseFor");
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(PollInterval, delta)) return;
        if (!pawn.Spawned || pawn.Dead || pawn.Downed) return;

        if (Holder != null) {
            looseFor = 0;

            return;
        }

        // Already theirs. Nothing left to count down.
        if (pawn.Faction?.def.defName == KolossControl.KolossFaction) return;

        looseFor += PollInterval;
        if (looseFor < KolossControl.GraceTicks) return;

        KolossControl.Lapse(pawn);
    }

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        // Gene.GetGizmos returns null rather than an empty sequence, so this cannot be foreached
        // without a check first.
        IEnumerable<Verse.Gizmo>? inherited = base.GetGizmos();
        if (inherited != null) {
            foreach (Verse.Gizmo gizmo in inherited) yield return gizmo;
        }

        // Self-guarded on purpose. Pawn.GetGizmos emits gene gizmos OUTSIDE the
        // IsColonistPlayerControlled check, so nothing else stops this appearing on a raider.
        if (!pawn.Spawned || pawn.Faction is not { IsPlayer: true }) yield break;

        Pawn? holder = Holder;

        // Nobody holding it, but still wearing the colony's colours - the grace window. This is
        // the only moment the player can do anything about it, and every other control on the
        // pawn has already gone quiet, so the warning has to carry the countdown itself.
        if (holder == null) {
            int left = Mathf.Max(0, KolossControl.GraceTicks - looseFor);

            yield return new Command_Action {
                defaultLabel = "CS_KolossLoose_Label".Translate(),
                defaultDesc = "CS_KolossLoose_Desc".Translate(
                    left.ToStringSecondsFromTicks().Named("LEFT"),
                    EmotionalResistance.Of(pawn).ToString("F1").Named("NEEDED")
                ),
                icon = TexCommand.Attack,
                action = () => { },
                Disabled = true,
                disabledReason = "CS_KolossLoose_Reason".Translate(),
            };

            yield break;
        }

        if (!holder.IsColonistPlayerControlled) yield break;

        yield return new Command_Action {
            defaultLabel = "CS_KolossRelease_Label".Translate(),
            defaultDesc = "CS_KolossRelease_Desc".Translate(holder.LabelShortCap.Named("HOLDER")),
            icon = TexCommand.Attack,
            action = () => KolossControl.Release(pawn),
        };
    }

    /// <summary>
    ///     One line saying who has this thing, because the whole mechanic is invisible otherwise.
    /// </summary>
    public string InspectLine() {
        Pawn? holder = Holder;

        return holder == null
            ? "CS_KolossInspect_Loose".Translate()
            : "CS_KolossInspect_Held".Translate(holder.LabelShortCap.Named("HOLDER"));
    }
}
