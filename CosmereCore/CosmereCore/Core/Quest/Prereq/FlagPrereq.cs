namespace Cosmere.Core.Quest.Prereq;

/// <summary>Requires a CosmereQuestFlag to be set. Flags outlive the quest that set them.</summary>
public class FlagPrereq : QuestPrereq {
    public string? flag;

    public override bool IsMet(QuestWorldState state) {
        string? f = flag;
        return f != null && f.Length > 0 && state.flags.Contains(f);
    }

    public override string? ConfigError() {
        return string.IsNullOrEmpty(flag) ? "FlagPrereq has no flag." : null;
    }
}
