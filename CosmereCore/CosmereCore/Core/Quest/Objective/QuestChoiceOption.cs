using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>One branch the player may pick at the start of a stage.</summary>
public class QuestChoiceOption : IExposable {
    public int goodwillOnTarget;
    public string? key;
    public string? labelKey;
    public string? tipKey;
    public int silverCost;

    public void ExposeData() {
        Scribe_Values.Look(ref key, "key");
        Scribe_Values.Look(ref labelKey, "labelKey");
        Scribe_Values.Look(ref tipKey, "tipKey");
        Scribe_Values.Look(ref silverCost, "silverCost");
        Scribe_Values.Look(ref goodwillOnTarget, "goodwillOnTarget");
    }
}
