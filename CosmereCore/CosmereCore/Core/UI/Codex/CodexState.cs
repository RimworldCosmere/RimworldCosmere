using UnityEngine;

namespace Cosmere.Core.UI.Codex;

public sealed class CodexState {
    public Vector2 BondDetailScroll = Vector2.zero;
    public Vector2 MemoriesScroll = Vector2.zero;
    public Vector2 ProgressionScroll = Vector2.zero;
    public Vector2 ShardConnectionScroll = Vector2.zero;
    public Vector2 WorldConnectionScroll = Vector2.zero;
    public int SelectedSprenIndex = 0;
    public int SelectedSystemIndex = 0;

    /// <summary>
    ///     Connection sits in the rail beside the investiture systems rather than inside one of
    ///     them, so which of the two is showing is its own piece of state.
    /// </summary>
    public bool ShowingConnection = true;
    public ConnectionCodexPage ConnectionPage = ConnectionCodexPage.Shards;
    public CodexSubtab Subtab = CodexSubtab.Progression;
}
