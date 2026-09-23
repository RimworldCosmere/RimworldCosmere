using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Sets a faction's goodwill with the player to a fixed value. Natural goodwill only drags
///     a faction back towards its baseline after 3,000,000 ticks outside a +-50 band, and then
///     only ten points at a time, so a value set here holds for any real campaign.
/// </summary>
public class SetGoodwillAction : ProgressionAction {
    public string faction = string.Empty;
    public int goodwill;
    public bool sendMessage = true;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(faction);
        if (def == null) {
            Log.Warn($"ScenarioProgression: FactionDef '{faction}' not found for SetGoodwill");
            return;
        }

        Faction? other = Find.FactionManager.FirstFactionOfDef(def);
        if (other == null) {
            Log.Warn($"ScenarioProgression: no live faction for '{faction}'");
            return;
        }

        Faction? player = Faction.OfPlayer;
        if (player == null || other == player) return;

        int delta = goodwill - player.GoodwillWith(other);
        if (delta == 0) return;

        player.TryAffectGoodwillWith(other, delta, sendMessage, false);
    }

    public override string? Describe() {
        FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(faction);
        if (def == null) return null;

        return "CC_Progression_Effect_Goodwill".Translate(
            def.label.Named("FACTION"),
            goodwill.Named("AMOUNT")
        ).Resolve();
    }
}
