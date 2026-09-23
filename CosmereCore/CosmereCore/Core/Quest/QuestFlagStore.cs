using System;

namespace Cosmere.Core.Quest;

/// <summary>
///     Indirection so a reward can set a CosmereQuestFlag without depending on the
///     GameComponent that owns them. CosmereQuestManager assigns these on construction.
/// </summary>
public static class QuestFlagStore {
    public static Action<string> SetFlag = _ => { };
    public static Func<string, bool> HasFlag = _ => false;
}
