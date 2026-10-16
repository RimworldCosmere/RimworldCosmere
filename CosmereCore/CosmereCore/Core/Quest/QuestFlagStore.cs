using Verse;

namespace Cosmere.Core.Quest;

/// <summary>
///     Indirection so a reward can set a CosmereQuestFlag without holding a reference to the
///     GameComponent that owns them. The flags themselves live on CosmereQuestManager.
/// </summary>
public static class QuestFlagStore {
    public static void SetFlag(string flag) {
        Current.Game?.GetComponent<CosmereQuestManager>()?.SetFlag(flag);
    }

    public static bool HasFlag(string flag) {
        return Current.Game?.GetComponent<CosmereQuestManager>()?.HasFlag(flag) ?? false;
    }
}
