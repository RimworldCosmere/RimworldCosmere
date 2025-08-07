using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Cosmere.Foundation.UI;

/**
 * @todo Implement better dropdown logic
 */
public class DropdownMenu : FloatMenu {
    private readonly Rect dropdownRect;

    public DropdownMenu(Rect dropdownRect, List<FloatMenuOption> options) : base(options) {
        this.dropdownRect = dropdownRect;
        vanishIfMouseDistant = false;
    }

    public new bool vanishIfMouseDistant {
        get => base.vanishIfMouseDistant;
        set => base.vanishIfMouseDistant = value;
    }
}