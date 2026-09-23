using System.Collections.Generic;

namespace Cosmere.Core.Quest;

/// <summary>
///     A quest reduced to only the fields eligibility cares about. CosmereQuestManager
///     projects a CosmereQuestDef into one of these so the filter never touches a Def.
/// </summary>
public class QuestCandidate {
    public int cooldownDays;
    public string defName = string.Empty;
    public List<string>? eras;
    public QuestKind kind = QuestKind.Repeatable;
    public int minColonists;
    public int minDaysElapsed;
    public string? requiredCapstone;
    public List<string>? requiredFlags;
    public List<string>? requiredShards;
    public float selectionWeight = 1f;

    /// <summary>The FactionDef the quest points its site and its enemies at, if any.</summary>
    public string? targetFaction;

    /// <summary>
    ///     The CosmereWorldDef this quest belongs to, or null when it belongs to all of them.
    /// </summary>
    public string? world;
}
