using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

public class SpawnItemAction : ProgressionAction {
    public int count = 1;
    public string? letterText;
    public string? letterTitle;
    public string? stuff;
    public string thing = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        ThingDef? thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(thing);
        if (thingDef == null) {
            Log.Warn($"ScenarioProgression: Thing '{thing}' not found for SpawnItem");
            return;
        }

        ThingDef? stuffDef = null;
        if (stuff != null) {
            stuffDef = DefDatabase<ThingDef>.GetNamedSilentFail(stuff);
        }

        Map? map = Find.CurrentMap;
        if (map == null) return;

        Verse.Thing item = ThingMaker.MakeThing(thingDef, stuffDef);
        item.stackCount = count;

        IntVec3 dropSpot = DropCellFinder.TradeDropSpot(map);
        DropPodUtility.DropThingsNear(dropSpot, map, [item], 110, false, true);

        if (letterTitle != null && letterText != null) {
            Find.LetterStack.ReceiveLetter(
                letterTitle,
                letterText,
                LetterDefOf.PositiveEvent,
                new TargetInfo(dropSpot, map)
            );
        }
    }

    public override string? Describe() {
        ThingDef? thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(thing);
        if (thingDef == null) return null;

        return "CC_Progression_Effect_Item".Translate(
            count.Named("COUNT"),
            thingDef.label.Named("THING")
        ).Resolve();
    }
}
