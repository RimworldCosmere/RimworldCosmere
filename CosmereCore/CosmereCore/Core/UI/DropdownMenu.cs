using Verse;

namespace Cosmere.Core.UI;

public class DropdownMenu : FloatMenu {
    public DropdownMenu(List<FloatMenuOption> options) : base(options) {
        vanishIfMouseDistant = false;
    }
}
