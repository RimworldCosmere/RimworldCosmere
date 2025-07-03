using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CosmereFramework.Quickstart;

public abstract class AbstractQuickstart : IQuickstart {
    public virtual bool PauseAfterLoad => true;
    public abstract int MapSize { get; }
    public abstract StorytellerDef Storyteller { get; }
    public abstract DifficultyDef Difficulty { get; }
    public abstract ScenarioDef? Scenario { get; }
    public virtual void PostStart() { }
    public virtual void PostLoaded() { }
    public virtual void PostApplyConfiguration() { }
    public virtual void PrepareColonists(List<Pawn> pawns) { }
}