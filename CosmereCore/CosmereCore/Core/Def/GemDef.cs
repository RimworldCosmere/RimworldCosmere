using UnityEngine;
using Verse;

namespace Cosmere.Core.Def;

public class GemDef : Verse.Def {
    private ThingDef? cachedItem;
    private ThingDef? cachedMineableItem;
    public Color color;
    public Color? colorTwo;
    public Color? glowColor;

    public string coloredLabel => label.Colorize(ColoredText.DateTimeColor);
    public ThingDef Item => cachedItem ??= DefDatabase<ThingDef>.GetNamed("Raw" + defName);
    public ThingDef MineableItem => cachedMineableItem ??= DefDatabase<ThingDef>.GetNamed("Mineable" + defName);
}