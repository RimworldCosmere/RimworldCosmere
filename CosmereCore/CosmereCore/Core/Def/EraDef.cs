namespace Cosmere.Core.Def;

/// <summary>
///     One age of a shardworld. Quests declare which eras they belong to, and a campaign can
///     advance from one to the next when the story earns it.
/// </summary>
public class EraDef : Verse.Def {
    /// <summary>
    ///     The age this one gives way to. Null means the end of the shardworld's timeline, so
    ///     AdvanceEraAction has nowhere left to go and leaves the campaign where it is.
    /// </summary>
    public EraDef? next;
}
