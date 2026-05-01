using Cosmere.Lightweave.Runtime;

namespace Cosmere.Lightweave.Doc;

public sealed class DocContext {
    public LightweaveScrollStatus Scroll { get; set; } = new LightweaveScrollStatus();
    public Dictionary<string, float> AnchorOffsets { get; } = new Dictionary<string, float>();

    public void Reset() {
        Scroll.Position = UnityEngine.Vector2.zero;
        AnchorOffsets.Clear();
    }
}
