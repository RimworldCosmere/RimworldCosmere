namespace Cosmere.Core.Quest;

/// <summary>How a quest reaches the player.</summary>
public enum QuestKind {
    /// <summary>Offered by the storyteller on weight, re-offers after a cooldown.</summary>
    Repeatable,

    /// <summary>Fired once from a ScenarioProgressionDef event. Can burn.</summary>
    Capstone,

    /// <summary>Not offered. It arrives, and the player deals with it.</summary>
    Threat,
}
