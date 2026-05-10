using HarmonyLib;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Settings;

public class LightweaveMod : Verse.Mod {
    private static LightweaveMod? instance;

    public static LightweaveSettings Settings { get; private set; } = null!;

    public LightweaveMod(ModContentPack content) : base(content) {
        instance = this;
        Settings = GetSettings<LightweaveSettings>();
        Harmony harmony = new Harmony("cryptiklemur.lightweave");
        harmony.PatchAll(typeof(LightweaveMod).Assembly);
    }

    public static void Save() {
        instance?.WriteSettings();
    }

    public override string SettingsCategory() {
        return "Lightweave";
    }

    public override void DoSettingsWindowContents(Rect inRect) {
        LightweaveSettingsForm.Render(inRect);
    }
}
