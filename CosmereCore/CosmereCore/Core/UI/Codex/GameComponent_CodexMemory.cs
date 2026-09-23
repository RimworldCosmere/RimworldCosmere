using Cosmere.Core.UI.Model;
using Verse;

namespace Cosmere.Core.UI.Codex;

/// <summary>
///     Remembers which system the codex was last reading. Stored by system id, not by index,
///     because the index means something different on every pawn.
/// </summary>
public sealed class GameComponent_CodexMemory : GameComponent {
    private string? lastSystemId;

    public GameComponent_CodexMemory(Game game) { }

    public static GameComponent_CodexMemory? Get() {
        return Current.Game?.GetComponent<GameComponent_CodexMemory>();
    }

    public static void ApplyTo(CodexState state, IReadOnlyList<IInvestitureProvider> providers) {
        if (providers.Count == 0) return;

        string? remembered = Get()?.lastSystemId;
        if (remembered != null) {
            for (int i = 0; i < providers.Count; i++) {
                if (providers[i].SystemId != remembered) continue;

                state.SelectedSystemIndex = i;
                return;
            }
        }

        if (state.SelectedSystemIndex < 0 || state.SelectedSystemIndex >= providers.Count) {
            state.SelectedSystemIndex = 0;
        }
    }

    /// <summary>
    ///     Called after the switcher has drawn, so this frame's click is what gets stored.
    /// </summary>
    public static void RememberFrom(CodexState state, IReadOnlyList<IInvestitureProvider> providers) {
        GameComponent_CodexMemory? memory = Get();
        if (memory == null) return;
        if (state.SelectedSystemIndex < 0 || state.SelectedSystemIndex >= providers.Count) return;

        memory.lastSystemId = providers[state.SelectedSystemIndex].SystemId;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref lastSystemId, "lastSystemId");
    }
}
