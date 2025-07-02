using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CosmereFramework.Quickstart;

internal interface IQuickstart {
    public bool PauseAfterLoad { get; }
    public int MapSize { get; }
    public StorytellerDef Storyteller { get; }
    public DifficultyDef Difficulty { get; }
    public ScenarioDef? Scenario { get; }

    public void PostStart();
    public void PostLoaded();
    public void PostApplyConfiguration();
    public void PrepareColonists(List<Pawn> playerPawns);
}