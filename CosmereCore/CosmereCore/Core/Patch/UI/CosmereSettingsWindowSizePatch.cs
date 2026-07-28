using Concord;
using RimWorld;
using UnityEngine;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class CosmereSettingsWindowSizePatch : Dialog_ModSettings {
    [InjectField("mod")]
    private readonly Verse.Mod settingsMod = null!;

    protected CosmereSettingsWindowSizePatch(Verse.Mod mod) : base(mod) { }

    [Inject(At.Return, nameof(InitialSize))]
    private void AfterInitialSize(ControlHandle<Vector2> ch) {
        if (settingsMod is not Mod) return;

        float targetWidth = Mathf.Min(1200f, Verse.UI.screenWidth - 40f);
        float targetHeight = Mathf.Min(1000f, Verse.UI.screenHeight - 40f);
        ch.ReturnValue = new Vector2(
            Mathf.Max(ch.ReturnValue.x, targetWidth),
            Mathf.Max(ch.ReturnValue.y, targetHeight)
        );
    }
}
