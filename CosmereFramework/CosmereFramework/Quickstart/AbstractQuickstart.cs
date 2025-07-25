using System.Text;
using RimWorld;
using Verse;

namespace Cosmere.Framework.Quickstart;

public abstract class AbstractQuickstart {
    private TaggedString? cachedDescription;

    public abstract TaggedString description { get; }

    public virtual bool pauseAfterLoad => true;

    public virtual int mapSize => 75;

    public virtual float planetCoverage => 0.05f;

    public virtual StorytellerDef storyteller => StorytellerDefOf.Cassandra;

    public virtual DifficultyDef difficulty => DifficultyDefOf.Easy;

    public virtual ScenarioDef scenario => ScenarioDefOf.Crashlanded;

    public virtual void PostStart() { }

    public virtual void PostLoaded() { }

    public virtual void PostApplyConfiguration() { }

    public virtual void PrepareColonists(List<Pawn> pawns) { }

    public TaggedString GetDescription() {
        if (cachedDescription.HasValue) return cachedDescription.Value;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(
            "CF_Quickstart_Name".Translate().Colorize(ColoredText.DateTimeColor) + GetType().Name
        );
        builder.AppendLine();
        builder.AppendLine(
            "CF_Quickstart_MapSize".Translate().Colorize(ColoredText.TipSectionTitleColor) + $"{mapSize}x{mapSize}"
        );
        builder.AppendLine(
            "CF_Quickstart_Difficulty".Translate().Colorize(ColoredText.TipSectionTitleColor) + difficulty.LabelCap.ToString()
        );
        builder.AppendLine(
            "CF_Quickstart_Scenario".Translate().Colorize(ColoredText.TipSectionTitleColor) + (scenario?.LabelCap.ToString() ?? "None")
        );

        builder.AppendLine(
            "CF_Quickstart_PauseAfterLoad".Translate().Colorize(ColoredText.TipSectionTitleColor) + (pauseAfterLoad ? "Yes" : "No")
        );
        builder.AppendLine();
        builder.AppendLine(description.Resolve());

        return (cachedDescription = builder.ToString()).Value;
    }
}