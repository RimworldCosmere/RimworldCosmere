using Concord;
using Cosmere.System.Scadrial.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Allomancy;

[Patch]
public abstract class HideStoredVialsPatch : Verse.Thing {
    [Inject(At.Head, nameof(Print))]
    private Control BeforePrint() {
        Verse.Thing self = this;
        if (self.def.category != ThingCategory.Item) return Control.Continue;
        if (self.Map == null) return Control.Continue;

        SlotGroup slotGroup = self.Position.GetSlotGroup(self.Map);
        if (slotGroup?.parent is Building_VialCabinet) return Control.Cancel;

        return Control.Continue;
    }
}
