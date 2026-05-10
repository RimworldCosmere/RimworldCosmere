using System;
using Cosmere.Lightweave.Fonts;
using UnityEngine;

namespace Cosmere.Lightweave.Theme;

public static class ThemeRegistry {
    private static Theme? cachedDefault;
    private static Theme? cachedCosmere;
    private static Theme? cachedScadrial;
    private static Theme? cachedRoshar;

    public static Theme Default {
        get {
            if (cachedDefault != null) {
                return cachedDefault;
            }

            FontSet fonts = RequireFonts();
            cachedDefault = DefaultTheme.Build(fonts.Body, fonts.BodyBold, fonts.Heading, fonts.Display, fonts.Mono);
            return cachedDefault;
        }
    }

    public static Theme Cosmere {
        get {
            if (cachedCosmere != null) {
                return cachedCosmere;
            }

            FontSet fonts = RequireFonts();
            cachedCosmere = CosmereTheme.Build(fonts.Body, fonts.BodyBold, fonts.Heading, fonts.Display, fonts.Mono);
            return cachedCosmere;
        }
    }

    public static Theme Scadrial {
        get {
            if (cachedScadrial != null) {
                return cachedScadrial;
            }

            FontSet fonts = RequireFonts();
            cachedScadrial = ScadrialTheme.Build(fonts.Body, fonts.BodyBold, fonts.Heading, fonts.Display, fonts.Mono);
            return cachedScadrial;
        }
    }

    public static Theme Roshar {
        get {
            if (cachedRoshar != null) {
                return cachedRoshar;
            }

            FontSet fonts = RequireFonts();
            cachedRoshar = RosharTheme.Build(fonts.Body, fonts.BodyBold, fonts.Heading, fonts.Display, fonts.Mono);
            return cachedRoshar;
        }
    }

    private static FontSet RequireFonts() {
        Font? body = LightweaveFonts.ArimoRegular;
        Font? bodyBold = LightweaveFonts.ArimoBold;
        Font? heading = LightweaveFonts.ArimoBold;
        Font? display = LightweaveFonts.Cinzel ?? LightweaveFonts.CarlitoBold;
        Font? mono = LightweaveFonts.JetBrainsMono;
        if (body == null || bodyBold == null || heading == null || display == null || mono == null) {
            throw new InvalidOperationException(
                "ThemeRegistry accessed before LightweaveFonts loaded. Ensure FontLoader.cctor has run."
            );
        }

        return new FontSet(body, bodyBold, heading, display, mono);
    }

    private readonly record struct FontSet(Font Body, Font BodyBold, Font Heading, Font Display, Font Mono);
}
