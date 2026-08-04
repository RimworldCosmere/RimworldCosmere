using System.Text;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quickstart;

public abstract class AbstractQuickstart {
    private TaggedString? cachedDescription;

    public abstract TaggedString description { get; }

    public virtual bool pauseAfterLoad => true;

    public virtual int mapSize => 75;

    public virtual float planetCoverage => 0.05f;

    public virtual StorytellerDef storyteller => StorytellerDefOf.Cassandra;

    public virtual DifficultyDef difficulty => DifficultyDefOf.Easy;

    public virtual ScenarioDef scenario => ScenarioDefOf.Crashlanded;

    // Def names rather than ShardDefs: a Scadrial quickstart routinely wants Honor so Radiant
    // grants stop silently no-opping, and the Roshar ShardDefOf lives in a namespace Scadrial
    // code must not import.
    public virtual IReadOnlyList<string> shards => [];

    // Overrides whatever ScenarioEra the scenario declares. Null defers to the scenario, which
    // is what every quickstart that isn't deliberately cross-shard wants.
    public virtual string? era => null;

    public virtual void PostStart() { }

    public virtual void PostLoaded() { }

    public virtual void PostApplyConfiguration() { }

    public virtual void PrepareColonists(List<Pawn> pawns) { }

    public TaggedString GetDescription() {
        if (cachedDescription.HasValue) return cachedDescription.Value;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(
            "CC_Quickstart_Name".Translate().Colorize(ColoredText.DateTimeColor) + GetType().Name
        );
        builder.AppendLine();
        builder.AppendLine(
            "CC_Quickstart_MapSize".Translate().Colorize(ColoredText.TipSectionTitleColor) + $"{mapSize}x{mapSize}"
        );
        builder.AppendLine(
            "CC_Quickstart_Difficulty".Translate().Colorize(ColoredText.TipSectionTitleColor) +
            difficulty.LabelCap.ToString()
        );
        builder.AppendLine(
            "CC_Quickstart_Scenario".Translate().Colorize(ColoredText.TipSectionTitleColor) +
            (scenario?.LabelCap.ToString() ?? "None")
        );

        builder.AppendLine(
            "CC_Quickstart_PauseAfterLoad".Translate().Colorize(ColoredText.TipSectionTitleColor) +
            (pauseAfterLoad ? "Yes" : "No")
        );
        builder.AppendLine(
            "CC_Quickstart_Shards".Translate().Colorize(ColoredText.TipSectionTitleColor) +
            (shards.Count > 0 ? string.Join(", ", shards) : "None")
        );
        builder.AppendLine();
        builder.AppendLine(description.Resolve());

        return (cachedDescription = builder.ToString()).Value;
    }
}
