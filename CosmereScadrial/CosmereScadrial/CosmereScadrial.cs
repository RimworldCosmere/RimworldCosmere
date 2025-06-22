using System;
using System.Linq;
using CosmereFramework;
using UnityEngine;
using Verse;

namespace CosmereScadrial;

public class CosmereScadrial : Mod {
    public static CosmereScadrialSettings settings;

    public CosmereScadrial(ModContentPack content) : base(content) {
        settings = GetSettings<CosmereScadrialSettings>();
    }

    public static CosmereScadrial cosmereScadrialMod => LoadedModManager.GetMod<CosmereScadrial>();

    public override void DoSettingsWindowContents(Rect inRect) {
        Listing_Standard listing = new Listing_Standard();
        listing.Begin(inRect);

        listing.Label("Mists frequency:");

        Rect dropdownRect = listing.GetRect(30f);
        Widgets.Dropdown(
            dropdownRect,
            settings,
            s => s.mistsFrequency,
            s => Enum.GetValues(typeof(MistsFrequency))
                .Cast<MistsFrequency>()
                .Select(freq => new Widgets.DropdownMenuElement<MistsFrequency> {
                    option = new FloatMenuOption(freq.ToString(), () => settings.mistsFrequency = freq),
                    payload = freq,
                }),
            settings.mistsFrequency.ToString()
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