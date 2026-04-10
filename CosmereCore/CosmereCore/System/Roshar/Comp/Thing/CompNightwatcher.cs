using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class CompNightwatcherProperties : CompProperties {
    public CompNightwatcherProperties() {
        compClass = typeof(CompNightwatcher);
    }
}

public class CompNightwatcher : ThingComp {
    private bool hasVisited;
    private int visitedTick = -1;
    public float cultivationConnectionBonus;

    public bool HasVisited => hasVisited;

    public void MarkVisited() {
        hasVisited = true;
        visitedTick = GenTicks.TicksGame;
    }

    public void ResetVisit() {
        hasVisited = false;
        visitedTick = -1;
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Values.Look(ref hasVisited, "hasVisited");
        Scribe_Values.Look(ref visitedTick, "visitedTick", -1);
        Scribe_Values.Look(ref cultivationConnectionBonus, "cultivationConnectionBonus");
    }
}
