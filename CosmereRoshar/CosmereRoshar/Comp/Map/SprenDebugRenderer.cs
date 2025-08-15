using Cosmere.Roshar.Debug;
using Verse;

namespace Cosmere.Roshar.Comp.Map;

public class SprenDebugRenderer : MapComponent {
    public SprenDebugRenderer(Verse.Map map) : base(map) { }

    public override void MapComponentTick() {
        SprenDebugOverlay.DrawOverlay();
    }
}