using UnityEngine;

namespace Cosmere.Core.UI.Codex;

public sealed class CodexState {
    public CodexSubtab Subtab = CodexSubtab.Progression;
    public int SelectedSystemIndex = 0;
    public int SelectedSprenIndex = 0;
    public Vector2 ProgressionScroll = Vector2.zero;
    public Vector2 BondDetailScroll = Vector2.zero;
}
