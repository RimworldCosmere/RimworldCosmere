using Concord;
using Cosmere.Core.Window;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class CosmereSettingsWindowSizePatch : Dialog_ModSettings {
    [InjectField("mod")]
    private readonly Verse.Mod settingsMod = null!;

    protected CosmereSettingsWindowSizePatch(Verse.Mod mod) : base(mod) { }

    /// <summary>
    ///     Only Dialog_ModSettings' own declared members are safe injection targets. Anything it
    ///     merely inherits - Margin, PostOpen - resolves to Verse.Window and would patch every window in the game.
    /// </summary>
    [Inject(At.Head, nameof(DoWindowContents))]
    private Control BeforeDoWindowContents(Rect inRect) {
        if (settingsMod is not Mod mod) return Control.Continue;

        Close(false);
        Find.WindowStack.Add(new CosmereSettingsDialog(mod));

        return Control.Cancel;
    }
}
