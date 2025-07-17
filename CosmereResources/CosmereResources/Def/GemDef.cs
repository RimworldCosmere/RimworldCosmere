using UnityEngine;
using Verse;

namespace CosmereResources.Def;

public class GemDef : Verse.Def {
    public Color color;
    public Color? colorTwo;
    public Color? glowColor;

    public string coloredLabel => label.Colorize(ColoredText.DateTimeColor);

    public ThingDef Item => DefDatabase<ThingDef>.GetNamed(defName);
}