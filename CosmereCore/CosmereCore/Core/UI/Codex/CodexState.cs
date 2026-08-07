using UnityEngine;

namespace Cosmere.Core.UI.Codex;

public sealed class CodexState {
    public Vector2 BondDetailScroll = Vector2.zero;
    public Vector2 ConnectionScroll = Vector2.zero;
    public Vector2 MemoriesScroll = Vector2.zero;
    public Vector2 ProgressionScroll = Vector2.zero;
    public int SelectedSprenIndex = 0;
    public int SelectedSystemIndex = 0;
    public CodexSubtab Subtab = CodexSubtab.Connection;
}
