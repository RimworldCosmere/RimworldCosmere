using System;
using System.Linq;
using CosmereCore.Tabs;
using CosmereFramework;
using CosmereScadrial.Inspector;
using UnityEngine;
using Verse;

namespace CosmereScadrial;

public class CosmereScadrial : Mod {
    public static CosmereScadrialSettings Settings;

    public CosmereScadrial(ModContentPack content) : base(content) {
        InvestitureTabRegistry.Register(InvestitureTabScadrial.Draw);
        Settings = GetSettings<CosmereScadrialSettings>();
    }


    public static CosmereScadrial CosmereScadrialMod => LoadedModManager.GetMod<CosmereScadrial>();

    public override void DoSettingsWindowContents(Rect inRect) {
        Listing_Standard listing = new Listing_Standard();
        listing.Begin(inRect);

        listing.Label("Mists frequency:");

        Rect dropdownRect = listing.GetRect(30f);
        Widgets.Dropdown(
            dropdownRect,
            Settings,
            s => s.mistsFrequency,
            s => Enum.GetValues(typeof(MistsFrequency))
                .Cast<MistsFrequency>()
                .Select(freq => new Widgets.DropdownMenuElement<MistsFrequency> {
                    option = new FloatMenuOption(freq.ToString(), () => Settings.mistsFrequency = freq),
                    payload = freq,
                }),
            Settings.mistsFrequency.ToString()
        );

        listing.End();
    }

    public override string SettingsCategory() {
        return "Cosmere: Scadrial";
    }
}

[StaticConstructorOnStartup]
public static class ModStartup {
    static ModStartup() {
        Startup.Initialize("Cryptiklemur.Cosmere.Scadrial");
    }
}