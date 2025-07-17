using UnityEngine;
using Verse;

namespace CosmereResources.Def;

public class GemDef : Verse.Def {
    public ThingDef? cachedItem;
    public ThingDef? cachedMineableItem;
    public Color color;
    public Color? colorTwo;
    public Color? glowColor;

    public string coloredLabel => label.Colorize(ColoredText.DateTimeColor);
    public ThingDef Item => cachedItem ??= DefDatabase<ThingDef>.GetNamed(defName);
    public ThingDef MineableItem => cachedMineableItem ??= DefDatabase<ThingDef>.GetNamed("Mineable" + defName);
}